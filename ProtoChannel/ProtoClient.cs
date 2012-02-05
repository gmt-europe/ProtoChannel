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

        public ProtoClientConfiguration Configuration { get; private set; }

        internal ServiceAssembly ServiceAssembly { get; private set; }

        private ProtoClient(ProtoClientConfiguration configuration, TcpClient tcpClient, string hostname)
        {
            if (hostname == null)
                throw new ArgumentNullException("hostname");

            Configuration = configuration ?? new ProtoClientConfiguration();
            Configuration.Freeze();

            ServiceAssembly = ServiceRegistry.GetAssemblyRegistration(
                Configuration.ServiceAssembly ?? GetType().Assembly
            );

            var streamManager = Configuration.StreamManager ?? new MemoryStreamManager();

            _connection = new ClientConnection(this, tcpClient, hostname, streamManager);
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
            if (remoteEndPoint == null)
                throw new ArgumentNullException("remoteEndPoint");

            var client = new TcpClient();

            client.Connect(remoteEndPoint);

            return client;
        }

        private static TcpClient CreateClient(IPAddress address, int port)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var client = new TcpClient();

            client.Connect(address, port);

            return client;
        }

        private static TcpClient CreateClient(string hostname, int port)
        {
            if (hostname == null)
                throw new ArgumentNullException("hostname");

            var client = new TcpClient();

            client.Connect(hostname, port);

            return client;
        }

        internal protected virtual int ChooseProtocol(int minProtocol, int maxProtocol)
        {
            return maxProtocol;
        }

        protected T SendMessage<T>(object message)
        {
            return (T)ClientConnection.EndSendMessage(_connection.BeginSendMessage(message, typeof(T), null, null));
        }

        protected void SendMessage(object message)
        {
            ClientConnection.EndSendMessage(_connection.BeginSendMessage(message, null, null, null));
        }

        protected T EndSendMessage<T>(IAsyncResult asyncResult)
        {
            return (T)ClientConnection.EndSendMessage(asyncResult);
        }

        protected IAsyncResult BeginSendMessage(object message, Type responseType, AsyncCallback callback, object asyncState)
        {
            return _connection.BeginSendMessage(message, responseType, callback, asyncState);
        }

        protected void EndSendMessage(IAsyncResult asyncResult)
        {
            ClientConnection.EndSendMessage(asyncResult);
        }

        protected void PostMessage(object message)
        {
            _connection.PostMessage(message);
        }

        public uint SendStream(Stream stream, string streamName, string contentType)
        {
            return _connection.SendStream(stream, streamName, contentType);
        }

        public ProtoStream GetStream(uint streamId)
        {
            return _connection.GetStream(streamId);
        }

        public IAsyncResult BeginGetStream(uint streamId, AsyncCallback callback, object asyncState)
        {
            return _connection.BeginGetStream(streamId, callback, asyncState);
        }

        public ProtoStream EndGetStream(IAsyncResult asyncResult)
        {
            return _connection.EndGetStream(asyncResult);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }

                _disposed = true;
            }
        }
    }
}
