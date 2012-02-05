using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ProtoChannel.Util;
using System.Threading.Tasks;

namespace ProtoChannel
{
    internal class HostConnection : ProtoConnection
    {
        private readonly ProtoHost _host;
        private State _state;
        private SslStream _sslStream;
        private HostClient _client;
        private bool _disposed;
        private readonly Queue<PendingMessage> _pendingMessages = new Queue<PendingMessage>();

        public HostConnection(ProtoHost host, TcpClient tcpClient, IStreamManager streamManager)
            : base(tcpClient, streamManager)
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

                    BeginAuthenticateAsServer(
                        _host.Configuration.Certificate,
                        _host.Configuration.ValidationCallback,
                        AuthenticateAsServerCallback,
                        null
                    );
                }
                else
                {
                    _state = State.ReceivingProlog;

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
                EndAuthenticateAsServer(asyncResult);

                _state = State.ReceivingProlog;

                Read();
            }
            catch (Exception ex)
            {
                _host.RaiseUnhandledException(this, ex);
            }
        }

        protected override bool ProcessInput()
        {
            // Locked by TcpConnection.

            if (_state == State.ReceivingProlog)
            {
                // Is there enough data in the buffer?

                if (ReadAvailable >= 8)
                {
                    byte[] header = new byte[8];

                    Read(header, 0, header.Length);

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
            long packageStart = BeginSendPackage();

            WriteMessage(TypeModel, new Messages.HandshakeRequest
            {
                ProtocolMin = (uint)_host.Configuration.MinimumProtocolNumber,
                ProtocolMax = (uint)_host.Configuration.MaximumProtocolNumber
            });

            EndSendPackage(PackageType.Handshake, packageStart);

            _state = State.Connected;
        }

        protected override void ProcessPackage(PendingPackage package)
        {
            // Locked by TcpConnection.

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

            var response = (Messages.HandshakeResponse)ReadMessage(
                TypeModel, typeof(Messages.HandshakeResponse), (int)package.Length
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

                // When creating the client failed, we shut down because there's
                // nothing more to do.

                if (_client == null)
                {
                    Dispose();
                }
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

            object message = ReadMessage(
                _host.ServiceAssembly.TypeModel, messageType.Type, (int)length
            );

            // Start processing the message.

            lock (SyncRoot)
            {
                _pendingMessages.Enqueue(new PendingMessage(message, isOneWay, associationId, method));

                if (_pendingMessages.Count == 1)
                {
#if _NET_2
                    ThreadPool.QueueUserWorkItem(ExecuteMessages, null);
#else
                    Task.Factory.StartNew(ExecuteMessages, null);
#endif
                }
            }
        }

        private void ExecuteMessages(object unused)
        {
            try
            {
                while (true)
                {
                    PendingMessage pendingMessage;

                    lock (SyncRoot)
                    {
                        if (_disposed || _pendingMessages.Count == 0)
                            return;

                        // Leave the message in the queue to not trigger a new
                        // ExecuteMessages.

                        pendingMessage = _pendingMessages.Peek();
                    }

                    object result;

                    lock (_client.SyncRoot)
                    {
                        using (OperationContext.SetScope(new OperationContext(this)))
                        {
                            result = pendingMessage.Method.Method.Invoke(
                                _client.Client, new[] { pendingMessage.Message }
                            );
                        }
                    }

                    lock (SyncRoot)
                    {
                        if (_disposed)
                            return;

                        _pendingMessages.Dequeue();

                        if (!pendingMessage.IsOneWay)
                            SendResponse(pendingMessage, result);
                    }
                }
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

            Write(buffer, 1, buffer.Length - 1);

            // Write the association ID.

            buffer = BitConverter.GetBytes((ushort)message.AssociationId);

            ByteUtil.ConvertNetwork(buffer);

            Write(buffer, 0, buffer.Length);

            // Write the message.

            WriteMessage(_host.ServiceAssembly.TypeModel, result);

            EndSendPackage(PackageType.Message, packageStart);
        }

        private void ProcessResponseMessage(uint type, uint length, uint associationId)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            lock (SyncRoot)
            {
                if (!_disposed)
                {
                    if (_sslStream != null)
                    {
                        _sslStream.Dispose();
                        _sslStream = null;
                    }

                    _host.RemoveConnection(this);

                    _disposed = true;
                }
            }

            base.Dispose(disposing);
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
