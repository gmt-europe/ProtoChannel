using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProtoChannel
{
    public class ProtoClient : IDisposable, IProtoConnection
    {
        private bool _disposed;
        private ClientConnection _connection;
        private Exception _unhandledException;

        public const int ProtocolVersion = Constants.ProtocolVersion;

        public ProtoClientConfiguration Configuration { get; private set; }

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            var ev = Disposed;

            if (ev != null)
                ev(this, e);
        }

        internal ServiceAssembly ServiceAssembly { get; private set; }

        private ProtoClient(ProtoClientConfiguration configuration, TcpClient tcpClient, string hostname)
        {
            Require.NotNull(hostname, "hostname");

            Configuration = configuration ?? new ProtoClientConfiguration();
            Configuration.Freeze();

            ServiceAssembly = ServiceRegistry.GetAssemblyRegistration(
                Configuration.ServiceAssembly ?? GetType().Assembly
            );

            var streamManager = Configuration.StreamManager ?? new MemoryStreamManager();

            _connection = new ClientConnection(this, tcpClient, hostname, streamManager);

            if (Configuration.CallbackObject != null)
            {
                Service service = null;

                if (!(Configuration.CallbackObject is IProtoMessageDispatcher))
                    service = ServiceAssembly.GetServiceRegistration(Configuration.CallbackObject.GetType());

                _connection.Client = new Client(configuration.CallbackObject, ServiceAssembly, service);
            }
        }

        public ProtoClient(IPEndPoint remoteEndPoint)
            : this(remoteEndPoint, null)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ProtoClient(IPEndPoint remoteEndPoint, ProtoClientConfiguration configuration)
            : this(configuration, CreateClient(remoteEndPoint), remoteEndPoint.Address.ToString())
        {
        }

        public ProtoClient(IPAddress address, int port)
            : this(address, port, null)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ProtoClient(IPAddress address, int port, ProtoClientConfiguration configuration)
            : this(configuration, CreateClient(address, port), address.ToString())
        {
        }

        public ProtoClient(string hostname, int port)
            : this(hostname, port, null)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ProtoClient(string hostname, int port, ProtoClientConfiguration configuration)
            : this(configuration, CreateClient(hostname, port), hostname)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static TcpClient CreateClient(IPEndPoint remoteEndPoint)
        {
            Require.NotNull(remoteEndPoint, "remoteEndPoint");

            var client = new TcpClient();

            client.Connect(remoteEndPoint);

            return client;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static TcpClient CreateClient(IPAddress address, int port)
        {
            Require.NotNull(address, "address");

            var client = new TcpClient();

            client.Connect(address, port);

            return client;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static TcpClient CreateClient(string hostname, int port)
        {
            Require.NotNull(hostname, "hostname");

            var client = new TcpClient();

            client.Connect(hostname, port);

            return client;
        }

        internal protected virtual int ChooseProtocol(int minProtocol, int maxProtocol)
        {
            return maxProtocol;
        }

        public IAsyncResult BeginSendMessage(object message, Type responseType, AsyncCallback callback, object asyncState)
        {
            VerifyState();

            return _connection.BeginSendMessage(message, responseType, callback, asyncState);
        }

        public object EndSendMessage(IAsyncResult asyncResult)
        {
            VerifyState();

            return _connection.EndSendMessage(asyncResult);
        }

        public void PostMessage(object message)
        {
            VerifyState();

            _connection.PostMessage(message);
        }

        public int SendStream(Stream stream, string streamName, string contentType)
        {
            VerifyState();

            return SendStream(stream, streamName, contentType, null);
        }

        internal int SendStream(Stream stream, string streamName, string contentType, int? associationId)
        {
            VerifyState();

            return _connection.SendStream(stream, streamName, contentType, associationId);
        }

        public IAsyncResult BeginGetStream(int streamId, AsyncCallback callback, object asyncState)
        {
            VerifyState();

            return _connection.BeginGetStream(streamId, callback, asyncState);
        }

        public ProtoStream EndGetStream(IAsyncResult asyncResult)
        {
            VerifyState();

            return _connection.EndGetStream(asyncResult);
        }

        private void VerifyState()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (_unhandledException != null)
                throw new ProtoChannelException("Client is in a faulted state", _unhandledException);
        }

        internal void RaiseUnhandledException(Exception exception)
        {
            _unhandledException = exception;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                OnDisposed(EventArgs.Empty);

                var connection = _connection;

                _connection = null;

                if (connection != null)
                    connection.Dispose();

                _disposed = true;
            }
        }
    }
}
