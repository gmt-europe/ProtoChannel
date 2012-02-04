using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class Service
    {
        public Type Type { get; private set; }

        public IKeyedCollection<ServiceMessage, ServiceMethod> Methods { get; private set; }

        public IKeyedCollection<int, ServiceMessage> Messages { get; private set; }

        public Service(Type serviceType, ServiceMethodCollection methods, ServiceMessageCollection messages)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            if (methods == null)
                throw new ArgumentNullException("methods");
            if (messages == null)
                throw new ArgumentNullException("messages");

            Type = serviceType;
            Methods = new ReadOnlyKeyedCollection<ServiceMessage, ServiceMethod>(methods);
            Messages = new ReadOnlyKeyedCollection<int, ServiceMessage>(messages);
        }
    }
}
