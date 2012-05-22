using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal sealed class Client : IDisposable
    {
        private bool _disposed;

        public object SyncRoot { get; private set; }

        public object Instance { get; private set; }

        public ServiceAssembly ServiceAssembly { get; private set; }

        public Service Service { get; private set; }

        public Client(object client, ServiceAssembly serviceAssembly, Service service)
        {
            Require.NotNull(client, "client");
            Require.NotNull(serviceAssembly, "serviceAssembly");

            Instance = client;
            ServiceAssembly = serviceAssembly;
            Service = service;

            SyncRoot = new object();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (Instance != null)
                {
                    var disposable = Instance as IDisposable;

                    if (disposable != null)
                        disposable.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
