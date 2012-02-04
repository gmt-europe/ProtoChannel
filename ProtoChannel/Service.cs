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

        public Service(Type serviceType, ServiceMethodCollection methods, ServiceMessageByIdCollection messagesById)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            if (methods == null)
                throw new ArgumentNullException("methods");
            if (messagesById == null)
                throw new ArgumentNullException("messagesById");

            Type = serviceType;
            Methods = new ReadOnlyKeyedCollection<ServiceMessage, ServiceMethod>(methods);
            Messages = new ReadOnlyKeyedCollection<int, ServiceMessage>(messagesById);
        }
    }
}
