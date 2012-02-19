using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Configuration;
using ProtoChannel.Web.Util;

namespace ProtoChannel.Web
{
    internal class ProtoProxyHost : IDisposable
    {
        private static readonly TimeSpan MaintenanceInterval = TimeSpan.FromSeconds(30);

        private bool _disposed;
        private readonly string _hostname;
        private readonly int _hostPort;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, ProtoProxyClient> _clients = new Dictionary<string, ProtoProxyClient>();
        private readonly Assembly _serviceAssembly;
        private Timer _maintenancetimer;

        public ProtoProxyHost(string hostname, int hostPort, Assembly serviceAssembly)
        {
            Require.NotNull(hostname, "hostname");
            Require.NotNull(serviceAssembly, "serviceAssembly");

            _hostname = hostname;
            _hostPort = hostPort;
            _serviceAssembly = serviceAssembly;

            _maintenancetimer = new Timer(Maintenance, null, MaintenanceInterval, MaintenanceInterval);
        }

        public string CreateClient(int protocolVersion)
        {
            string key = RandomKeyGenerator.GetRandomKey(12);

            var configuration = BuildConfiguration();

            configuration.CallbackObject = new CallbackChannel(this, key);

            var client = new ProtoClient(_hostname, _hostPort, configuration, protocolVersion);

            lock (_syncRoot)
            {
                _clients.Add(key, new ProtoProxyClient(this, key, client));
            }

            return key;
        }

        private ProtoClientConfiguration BuildConfiguration()
        {
            var config = (ProtoConfigurationSection)WebConfigurationManager.GetSection("protoChannel");

            var configuration = new ProtoClientConfiguration
            {
                ServiceAssembly = _serviceAssembly,
                Secure = config.Secure
            };

            if (config.MaxMessageSize > 0)
                configuration.MaxMessageSize = config.MaxMessageSize;

            if (config.MaxStreamSize > 0)
                configuration.MaxStreamSize = config.MaxStreamSize;

            if (config.SkipCertificateValidations)
                configuration.ValidationCallback = (p1, p2, p3, p4) => true;

            int streamManagers =
                (config.DiskStreamManager.ElementInformation.IsPresent ? 1 : 0) +
                (config.MemoryStreamManager.ElementInformation.IsPresent ? 1 : 0) +
                (config.HybridStreamManager.ElementInformation.IsPresent ? 1 : 0);

            if (streamManagers > 1)
                throw new InvalidOperationException("Specify ether a diskStreamManager, memoryStreamManager or hybridStreamManager");

            if (config.DiskStreamManager.ElementInformation.IsPresent)
            {
                if (config.DiskStreamManager.MaxStreamSize > 0)
                    configuration.StreamManager = new DiskStreamManager(config.DiskStreamManager.Path, config.MaxStreamSize);
                else
                    configuration.StreamManager = new DiskStreamManager(config.DiskStreamManager.Path);
            }

            if (config.MemoryStreamManager.ElementInformation.IsPresent)
            {
                if (config.MemoryStreamManager.MaxStreamSize > 0)
                    configuration.StreamManager = new MemoryStreamManager(config.MemoryStreamManager.MaxStreamSize);
                else
                    configuration.StreamManager = new MemoryStreamManager();
            }

            if (config.HybridStreamManager.ElementInformation.IsPresent)
            {
                if (config.HybridStreamManager.MaxStreamSize > 0)
                    configuration.StreamManager = new HybridStreamManager(config.HybridStreamManager.Path, config.HybridStreamManager.MaxMemoryStreamSize, config.HybridStreamManager.MaxStreamSize);
                else
                    configuration.StreamManager = new HybridStreamManager(config.HybridStreamManager.Path, config.HybridStreamManager.MaxMemoryStreamSize);
            }

            return configuration;
        }

        public ProtoProxyClient FindClient(string channelId)
        {
            Require.NotNull(channelId, "channelId");

            lock (_syncRoot)
            {
                ProtoProxyClient client;

                _clients.TryGetValue(channelId, out client);

                return client;
            }
        }

        public void RemoveClient(ProtoProxyClient client)
        {
            Require.NotNull(client, "client");

            lock(_syncRoot)
            {
                _clients.Remove(client.Key);
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

        private void Maintenance(object unused)
        {
            ProtoProxyClient[] clients;

            // Create a copy of the list of clients to not have nested locking.

            lock (_syncRoot)
            {
                clients = _clients.Values.ToArray();
            }

            foreach (var client in clients)
            {
                client.Maintenance();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_maintenancetimer != null)
                {
                    _maintenancetimer.Dispose();
                    _maintenancetimer = null;
                }

                _disposed = true;
            }
        }

        private class PendingMessage
        {
            private readonly ProtoProxyClient _client;
            private readonly uint _associationId;

            public PendingMessage(ProtoProxyClient client, uint associationId)
            {
                Require.NotNull(client, "client");

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
