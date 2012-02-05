using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal class HostClient
    {
        public object SyncRoot { get; private set; }

        public object Client { get; private set; }

        public HostClient(object client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            Client = client;

            SyncRoot = new object();
        }
    }
}
