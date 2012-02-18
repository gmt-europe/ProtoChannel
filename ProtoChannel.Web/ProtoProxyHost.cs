using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoChannel.Web.Util;

namespace ProtoChannel.Web
{
    internal class ProtoProxyHost : IDisposable
    {
        private bool _disposed;
        private readonly string _hostname;
        private readonly int _hostPort;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, ProtoProxyClient> _clients = new Dictionary<string, ProtoProxyClient>();
        private readonly Assembly _serviceAssembly;

        public ProtoProxyHost(string hostname, int hostPort, Assembly serviceAssembly)
        {
            if (hostname == null)
                throw new ArgumentNullException("hostname");
            if (serviceAssembly == null)
                throw new ArgumentNullException("serviceAssembly");

            _hostname = hostname;
            _hostPort = hostPort;
            _serviceAssembly = serviceAssembly;
        }

        public string CreateClient(int protocolVersion)
        {
            string key = RandomKeyGenerator.GetRandomKey(12);

            var configuration = new ProtoClientConfiguration
            {
                ServiceAssembly = _serviceAssembly,
                CallbackObject = new CallbackChannel(this, key)
            };

            var client = new ProtoClient(_hostname, _hostPort, configuration, protocolVersion);

            lock (_syncRoot)
            {
                _clients.Add(key, new ProtoProxyClient(key, client));
            }

            return key;
        }

        public ProtoProxyClient FindClient(string channelId)
        {
            if (channelId == null)
                throw new ArgumentNullException("channelId");

            lock (_syncRoot)
            {
                ProtoProxyClient client;

                _clients.TryGetValue(channelId, out client);

                return client;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        public void BeginSendMessage(ProtoProxyClient client, ServiceType serviceType, object message, uint associationId)
        {
            var pendingMessage = new PendingMessage(client, associationId);

            client.Client.BeginSendMessage(message, null, pendingMessage.EndSendMessage, null);
        }

        public void PostMessage(ProtoProxyClient client, object message)
        {
            client.Client.PostMessage(message);
        }

        private class PendingMessage
        {
            private readonly ProtoProxyClient _client;
            private readonly uint _associationId;

            public PendingMessage(ProtoProxyClient client, uint associationId)
            {
                if (client == null)
                    throw new ArgumentNullException("client");

                _client = client;
                _associationId = associationId;
            }

            public void EndSendMessage(IAsyncResult asyncResult)
            {
                var message = _client.Client.EndSendMessage(asyncResult);

                _client.EndSendMessage(_associationId, message);
            }
        }
    }
}
