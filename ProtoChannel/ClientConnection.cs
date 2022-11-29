using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ClientConnection : ProtoConnection
    {
        private ProtoClient _client;
        private readonly string _hostname;
        private bool _connected;
        private bool _disposed;
        private Timer _timer;

        public ClientConnection(ProtoClient client, TcpClient tcpClient, string hostname, IStreamManager streamManager)
            : base(tcpClient, streamManager, client.ServiceAssembly, client)
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
                    _hostname,
                    _client.Configuration.SecurityProtocol
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

            // And start the keep alive timer.

            StartKeepAliveTimer();
        }

        private void StartKeepAliveTimer()
        {
            if (!_client.Configuration.KeepAlive.HasValue)
                return;

            int keepAlive = (int)_client.Configuration.KeepAlive.Value.TotalMilliseconds;

            _timer = new Timer(SendKeepAlive, null, keepAlive, keepAlive);
        }

        private void SendKeepAlive(object state)
        {
            lock (SyncRoot)
            {
                if (IsDisposed)
                    return;

                EndSendPackage(PackageType.Ping, BeginSendPackage());
            }
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

                var timer = _timer;
                _timer = null;

                if (timer != null)
                    timer.Dispose();

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
