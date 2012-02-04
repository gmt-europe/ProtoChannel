using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ProtoClientConnection : ProtoConnection
    {
        private readonly ProtoClient _client;
        private State _state;

        public ProtoClientConnection(ProtoClient client, TcpClient tcpClient)
            : base(tcpClient)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _client = client;

            Connect();
        }

        private void Connect()
        {
            // Perform authentication.

            if (_client.Configuration.Secure)
            {
                _state = State.Authenticating;

                var sslStream = new SslStream(TcpClient.GetStream(), false, _client.Configuration.ValidationCallback ?? DummyValidationCallback);

                sslStream.AuthenticateAsClient(
                    _client.Configuration.TargetHost ?? _client.RemoteEndPoint.Address.ToString(),
                    null,
                    SslProtocols.Tls,
                    false /* checkCertificateRevocation */
                );

                Stream = sslStream;
            }
            else
            {
                Stream = TcpClient.GetStream();
            }

            _state = State.ReceivingHandshake;

            // Push the prolog header.

            SendBuffer.Write(Constants.Header, 0, Constants.Header.Length);

            // Push the version number.

            var buffer = BitConverter.GetBytes((uint)Constants.ProtocolVersion);

            ByteUtil.ConvertNetwork(buffer);

            SendBuffer.Write(buffer, 0, buffer.Length);

            // Send the buffer contents.

            Send();

            while (!IsDisposed && _state == State.ReceivingHandshake)
            {
                Read();
            }
        }

        protected override void ProcessPackage(ProtoConnection.PendingPackage package)
        {
            switch (_state)
            {
                case State.Authenticating:
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
            // Receive the handshake request.

            Messages.HandshakeRequest request;

            using (var stream = new SubStream(ReceiveBuffer, package.Size))
            {
                request = ProtoBuf.Serializer.Deserialize<Messages.HandshakeRequest>(stream);
            }

            // Ask the client which protocol we're going to connect with.

            int protocol = _client.ChooseProtocol((int)request.ProtocolMin, (int)request.ProtocolMax);

            var handshake = new Messages.HandshakeResponse
            {
                Protocol = (uint)protocol
            };

            // Push our response

            long messageStart = BeginSendPackage();

            ProtoBuf.Serializer.Serialize(SendBuffer, handshake);

            EndSendPackage(PackageType.Handshake, messageStart);

            _state = State.Connected;

            // Once we're connected, we can go into async mode.

            IsAsync = true;

            Read();
        }

        private bool DummyValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private enum State
        {
            Authenticating,
            ReceivingHandshake,
            Connected
        }
    }
}
