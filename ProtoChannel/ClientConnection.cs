using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ClientConnection : ProtoConnection
    {
        private ProtoClient _client;
        private readonly string _hostname;
        private bool _connected;
        private bool _disposed;

        public ClientConnection(ProtoClient client, TcpClient tcpClient, string hostname, IStreamManager streamManager)
            : base(tcpClient, streamManager, client.ServiceAssembly)
        {
            Require.NotNull(client, "client");
            Require.NotNull(hostname, "hostname");

            _client = client;
            _hostname = hostname;

            Connect();
        }

        private void Connect()
        {
            if (_client.Configuration.Secure)
            {
                AuthenticateAsClient(
                    _client.Configuration.ValidationCallback,
                    _hostname
                );
            }

            // Push the prolog header.

            Write(Constants.Header, 0, Constants.Header.Length);

            // Push the version number.

            var buffer = BitConverterEx.GetNetworkBytes((uint)Constants.ProtocolVersion);

            Write(buffer, 0, buffer.Length);

            // Send the buffer contents.

            Send();

            while (!IsDisposed && !_connected)
            {
                Read();
            }
        }

        protected override void ProcessPackage(PendingPackage package)
        {
            // Locked by TcpConnection.

            if (!_connected && package.Type != PackageType.Handshake)
            {
                SendError(ProtocolError.InvalidPackageType);
                return;
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

            var request = (Messages.HandshakeRequest)ReadMessage(
                TypeModel, typeof(Messages.HandshakeRequest), (int)package.Length
            );

            // Ask the client which protocol we're going to connect with.

            int protocol = _client.ChooseProtocol((int)request.ProtocolMin, (int)request.ProtocolMax);

            // Push our response

            long packageStart = BeginSendPackage();

            WriteMessage(TypeModel, new Messages.HandshakeResponse
            {
                Protocol = (uint)protocol
            });

            EndSendPackage(PackageType.Handshake, packageStart);

            _connected = true;

            // Once we're connected, we can go into async mode.

            IsAsync = true;

            Read();
        }

        protected override void RaiseUnhandledException(Exception exception)
        {
            _client.RaiseUnhandledException(exception);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                var client = _client;

                _client = null;

                if (client != null)
                    client.Dispose();

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
