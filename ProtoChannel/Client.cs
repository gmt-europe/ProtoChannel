using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal class Client
    {
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
    }
}
