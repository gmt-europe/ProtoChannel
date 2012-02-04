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
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ProtoHostConnection<T> : ProtoConnection
        where T : class, new()
    {
        private readonly ProtoHost<T> _host;
        private State _state;
        private SslStream _sslStream;

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

            long messageStart = BeginSendPackage();

            ProtoBuf.Serializer.Serialize(SendBuffer, handshake);

            EndSendPackage(PackageType.Handshake, messageStart);

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

            Messages.HandshakeResponse response;

            using (var stream = new SubStream(ReceiveBuffer, package.Size))
            {
                response = ProtoBuf.Serializer.Deserialize<Messages.HandshakeResponse>(stream);
            }

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

                _host.RaiseClientConnected(this, protocolNumber);
            }
        }

        private enum State
        {
            Authenticating,
            ReceivingProlog,
            ReceivingHandshake,
            Connected
        }
    }
}
