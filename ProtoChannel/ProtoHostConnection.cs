using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using ProtoChannel.Util;
using ProtoBuf.Meta;

namespace ProtoChannel
{
    internal class ProtoHostConnection<T> : ProtoConnection
        where T : class, new()
    {
        private readonly ProtoHost<T> _host;
        private readonly object _clientSyncRoot = new object();
        private State _state;
        private SslStream _sslStream;
        private T _client;

        public ProtoHostConnection(ProtoHost<T> host, TcpClient tcpClient)
            : base(tcpClient)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            _host = host;

            IsAsync = true;

            Connect();
        }

        private void Connect()
        {
            try
            {
                if (_host.Configuration.Secure)
                {
                    _state = State.Authenticating;

                    _sslStream = new SslStream(TcpClient.GetStream(), false, _host.Configuration.ValidationCallback ?? DummyValidationCallback);

                    _sslStream.BeginAuthenticateAsServer(
                        _host.Configuration.Certificate,
                        false /* clientCertificateRequired */,
                        SslProtocols.Tls,
                        false /* checkCertificateRevocation */,
                        AuthenticateAsServerCallback,
                        null
                    );
                }
                else
                {
                    _state = State.ReceivingProlog;

                    Stream = TcpClient.GetStream();

                    Read();
                }
            }
            catch (Exception ex)
            {
                _host.RaiseUnhandledException(this, ex);
            }
        }

        private void AuthenticateAsServerCallback(IAsyncResult asyncResult)
        {
            try
            {
                _sslStream.EndAuthenticateAsServer(asyncResult);

                Stream = _sslStream;

                _state = State.ReceivingProlog;

                Read();
            }
            catch (Exception ex)
            {
                _host.RaiseUnhandledException(this, ex);
            }
        }

        private bool DummyValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        protected override bool ProcessInput()
        {
            if (_state == State.ReceivingProlog)
            {
                // Is there enough data in the buffer?

                if (ReceiveBuffer.Length - ReceiveBuffer.Position >= 8)
                {
                    byte[] header = new byte[8];

                    ReceiveBuffer.Read(header, 0, header.Length);

                    // Verify the magic.

                    if (!ByteUtil.Equals(header, Constants.Header, 4))
                    {
                        SendError(ProtocolError.InvalidProtocolHeader);
                        return false;
                    }

                    // Verify the protocol number.

                    ByteUtil.ConvertNetwork(header, 4, 4);

                    uint protocolVersion = BitConverter.ToUInt32(header, 4);

                    if (protocolVersion != Constants.ProtocolVersion)
                    {
                        SendError(ProtocolError.InvalidProtocol);
                        return false;
                    }

                    _state = State.ReceivingHandshake;

                    SendHandshake();

                    return true;
                }
                else
                {
                    // Wait for more data to come in.

                    return false;
                }
            }
            else
            {
                return base.ProcessInput();
            }
        }

        private void SendHandshake()
        {
            Debug.Assert(SendBuffer.Length == SendBuffer.Position);

            var handshake = new Messages.HandshakeRequest
            {
                ProtocolMin = (uint)_host.Configuration.MinimumProtocolNumber,
                ProtocolMax = (uint)_host.Configuration.MaximumProtocolNumber
            };

            long packageStart = BeginSendPackage();

            ProtoBuf.Serializer.Serialize(SendBuffer, handshake);

            EndSendPackage(PackageType.Handshake, packageStart);

            _state = State.Connected;
        }

        protected override void ProcessPackage(PendingPackage package)
        {
            switch (_state)
            {
                case State.Authenticating:
                case State.ReceivingProlog:
                    SendError(ProtocolError.InvalidPackageType);
                    return;

                case State.ReceivingHandshake:
                    if (package.Type != PackageType.Handshake)
                    {
                        SendError(ProtocolError.InvalidPackageType);
                        return;
                    }
                    break;

                case State.Connected:
                    break;

                default:
                    throw new NotImplementedException();
            }

            switch (package.Type)
            {
                case PackageType.Handshake:
                    ProcessHandshake(package);
                    break;

                default:
                    base.ProcessPackage(package);
                    break;
            }
        }

        private void ProcessHandshake(PendingPackage package)
        {
            // Receive the handshake response.

            var response = (Messages.HandshakeResponse)TypeModel.Deserialize(
                ReceiveBuffer, null, typeof(Messages.HandshakeResponse), (int)package.Length
            );

            // Validate the protocol number.

            int protocolNumber = (int)response.Protocol;

            if (
                protocolNumber < _host.Configuration.MinimumProtocolNumber ||
                protocolNumber > _host.Configuration.MaximumProtocolNumber
            ) {
                SendError(ProtocolError.InvalidProtocol);
            }
            else
            {
                // Else, we've got a valid connection and can proceed with
                // creating the service client.

                _client = _host.RaiseClientConnected(this, protocolNumber);
            }
        }

        protected override void ProcessMessage(MessageKind kind, uint type, uint length, uint associationId)
        {
            if (kind == MessageKind.Response)
                ProcessResponseMessage(type, length, associationId);
            else
                ProcessRequestMessage(type, length, associationId, kind == MessageKind.OneWay);
        }

        private void ProcessRequestMessage(uint type, uint length, uint associationId, bool isOneWay)
        {
            // Validate the request and find the method.

            ServiceMessage messageType;

            if (!_host.Service.Messages.TryGetValue((int)type, out messageType))
            {
                SendError(ProtocolError.InvalidMessageType);
                return;
            }

            ServiceMethod method;

            if (!_host.Service.Methods.TryGetValue(messageType, out method))
            {
                SendError(ProtocolError.InvalidMessageType);
                return;
            }

            if (method.IsOneWay != isOneWay)
            {
                SendError(method.IsOneWay ? ProtocolError.ExpectedIsOneWay : ProtocolError.ExpectedRequest);
                return;
            }

            // Parse the message.

            var message = _host.ServiceAssembly.TypeModel.Deserialize(
                ReceiveBuffer, null, messageType.Type, (int)length
            );

            // Start processing the message.

            ThreadPool.QueueUserWorkItem(
                p => ExecuteMessage((PendingMessage)p),
                new PendingMessage(message, isOneWay, associationId, method)
            );
        }

        private void ExecuteMessage(PendingMessage message)
        {
            try
            {
                object result;

                lock (_clientSyncRoot)
                {
                    result = message.Method.Method.Invoke(_client, new[] { message.Message });
                }

                if (!message.IsOneWay)
                    SendResponse(message, result);
            }
            catch (Exception ex)
            {
                _host.RaiseUnhandledException(this, ex);
            }
        }

        private void SendResponse(PendingMessage message, object result)
        {
            long packageStart = BeginSendPackage();

            // Write the header.

            uint header = (uint)MessageKind.Response | (uint)message.Method.Response.Id << 2;

            byte[] buffer = BitConverter.GetBytes(header);

            ByteUtil.ConvertNetwork(buffer);

            SendBuffer.Write(buffer, 1, buffer.Length - 1);

            // Write the association ID.

            buffer = BitConverter.GetBytes((ushort)message.AssociationId);

            ByteUtil.ConvertNetwork(buffer);

            SendBuffer.Write(buffer, 0, buffer.Length);

            // Write the message.

            ProtoBuf.Serializer.NonGeneric.Serialize(SendBuffer, result);

            EndSendPackage(PackageType.Message, packageStart);
        }

        private void ProcessResponseMessage(uint type, uint length, uint associationId)
        {
            throw new NotImplementedException();
        }

        private enum State
        {
            Authenticating,
            ReceivingProlog,
            ReceivingHandshake,
            Connected
        }

        private class PendingMessage
        {
            private readonly object _message;
            private readonly bool _isOneWay;
            private readonly uint _associationId;
            private readonly ServiceMethod _method;

            public PendingMessage(object message, bool isOneWay, uint associationId, ServiceMethod method)
            {
                if (message == null)
                    throw new ArgumentNullException("message");
                if (method == null)
                    throw new ArgumentNullException("method");

                _message = message;
                _isOneWay = isOneWay;
                _associationId = associationId;
                _method = method;
            }

            public object Message
            {
                get { return _message; }
            }

            public bool IsOneWay
            {
                get { return _isOneWay; }
            }

            public uint AssociationId
            {
                get { return _associationId; }
            }

            public ServiceMethod Method
            {
                get { return _method; }
            }
        }
    }
}
