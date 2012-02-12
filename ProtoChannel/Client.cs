using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal class Client
    {
        public object SyncRoot { get; private set; }

        public object Instance { get; private set; }

        public Service Service { get; private set; }

        public Client(object client, Service service)
        {
            Require.NotNull(client, "client");
            Require.NotNull(service, "service");

            Instance = client;
            Service = service;

            SyncRoot = new object();
        }
    }
}
