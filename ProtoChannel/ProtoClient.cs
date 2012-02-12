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
                _connection.Client = new Client(
                    configuration.CallbackObject,
                    ServiceAssembly.GetServiceRegistration(Configuration.CallbackObject.GetType())
                );
            }
        }

        public ProtoClient(IPEndPoint remoteEndPoint)
            : this(remoteEndPoint, null)
        {
        }

        public ProtoClient(IPEndPoint remoteEndPoint, ProtoClientConfiguration configuration)
            : this(configuration, CreateClient(remoteEndPoint), remoteEndPoint.Address.ToString())
        {
        }

        public ProtoClient(IPAddress address, int port)
            : this(address, port, null)
        {
        }

        public ProtoClient(IPAddress address, int port, ProtoClientConfiguration configuration)
            : this(configuration, CreateClient(address, port), address.ToString())
        {
        }

        public ProtoClient(string hostname, int port)
            : this(hostname, port, null)
        {
        }

        public ProtoClient(string hostname, int port, ProtoClientConfiguration configuration)
            : this(configuration, CreateClient(hostname, port), hostname)
        {
        }

        private static TcpClient CreateClient(IPEndPoint remoteEndPoint)
        {
            Require.NotNull(remoteEndPoint, "remoteEndPoint");

            var client = new TcpClient();

            client.Connect(remoteEndPoint);

            return client;
        }

        private static TcpClient CreateClient(IPAddress address, int port)
        {
            Require.NotNull(address, "address");

            var client = new TcpClient();

            client.Connect(address, port);

            return client;
        }

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
            return _connection.BeginSendMessage(message, responseType, callback, asyncState);
        }

        public object EndSendMessage(IAsyncResult asyncResult)
        {
            return _connection.EndSendMessage(asyncResult);
        }

        public void PostMessage(object message)
        {
            _connection.PostMessage(message);
        }

        public uint SendStream(Stream stream, string streamName, string contentType)
        {
            return _connection.SendStream(stream, streamName, contentType);
        }

        public IAsyncResult BeginGetStream(uint streamId, AsyncCallback callback, object asyncState)
        {
            return _connection.BeginGetStream(streamId, callback, asyncState);
        }

        public ProtoStream EndGetStream(IAsyncResult asyncResult)
        {
            return _connection.EndGetStream(asyncResult);
        }

        internal void RaiseUnhandledException(Exception exception)
        {
            _unhandledException = exception;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                OnDisposed(EventArgs.Empty);

                var connection = _connection;

                _connection = null;

                if (connection != null)
                    connection.Dispose();

                _disposed = true;
            }

            var unhandledException = _unhandledException;

            _unhandledException = null;

            if (unhandledException != null)
                throw unhandledException;
        }
    }
}
