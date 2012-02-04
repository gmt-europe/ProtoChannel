using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProtoChannel
{
    public class ProtoClient : IDisposable
    {
        private bool _disposed;
        private ProtoClientConnection _connection;

        public IPEndPoint RemoteEndPoint { get; private set; }

        public ProtoClientConfiguration Configuration { get; private set; }

        internal ServiceAssembly ServiceAssembly { get; private set; }

        public ProtoClient(IPEndPoint remoteEndPoint)
            : this(remoteEndPoint, null)
        {
        }

        public ProtoClient(IPEndPoint remoteEndPoint, ProtoClientConfiguration configuration)
        {
            RemoteEndPoint = remoteEndPoint;

            Configuration = configuration ?? new ProtoClientConfiguration();
            Configuration.Freeze();

            ServiceAssembly = ServiceRegistry.GetAssemblyRegistration(
                Configuration.ServiceAssembly ?? GetType().Assembly
            );

            var client = new TcpClient();

            client.Connect(remoteEndPoint);

            _connection = new ProtoClientConnection(this, client);
        }

        internal protected virtual int ChooseProtocol(int minProtocol, int maxProtocol)
        {
            return maxProtocol;
        }

        protected T SendMessage<T>(object message)
        {
            return (T)_connection.EndSendMessage(_connection.BeginSendMessage(message, typeof(T), null, null));
        }

        protected void SendMessage(object message)
        {
            _connection.EndSendMessage(_connection.BeginSendMessage(message, null, null, null));
        }

        protected T EndSendMessage<T>(IAsyncResult asyncResult)
        {
            return (T)_connection.EndSendMessage(asyncResult);
        }

        protected void BeginSendMessage(object message, Type responseType, AsyncCallback callback, object asyncState)
        {
            _connection.BeginSendMessage(message, responseType, callback, asyncState);
        }

        protected void EndSendMessage(IAsyncResult asyncResult)
        {
            _connection.EndSendMessage(asyncResult);
        }

        protected void PostMessage(object message)
        {
            _connection.PostMessage(message);
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
