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

        public ProtoClient(IPEndPoint remoteEndPoint)
            : this(remoteEndPoint, null)
        {
        }

        public ProtoClient(IPEndPoint remoteEndPoint, ProtoClientConfiguration configuration)
        {
            RemoteEndPoint = remoteEndPoint;

            Configuration = configuration ?? new ProtoClientConfiguration();
            Configuration.Freeze();

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
            throw new NotImplementedException();
        }

        protected void SendMessage(object message)
        {
        }

        protected IAsyncResult BeginSendMessage(object message)
        {
            throw new NotImplementedException();
        }

        protected T EndSendMessage<T>(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        protected void EndSendMessage(IAsyncResult asyncResult)
        {
        }

        protected void Post(object message)
        {
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
