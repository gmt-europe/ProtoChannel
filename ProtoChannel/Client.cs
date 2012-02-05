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
            if (client == null)
                throw new ArgumentNullException("client");
            if (service == null)
                throw new ArgumentNullException("service");

            Instance = client;
            Service = service;

            SyncRoot = new object();
        }
    }
}
