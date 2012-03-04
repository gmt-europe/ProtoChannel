using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class Service
    {
        public Type Type { get; private set; }

        public ServiceAssembly ServiceAssembly { get; private set; }

        public ServiceMethodCollection Methods { get; private set; }

        public ServiceMessageByIdCollection Messages { get; private set; }

        public Type CallbackContractType { get; private set; }

        public Service(Type serviceType, ServiceMethodCollection methods, ServiceMessageByIdCollection messagesById, ServiceAssembly serviceAssembly)
        {
            Require.NotNull(serviceType, "serviceType");
            Require.NotNull(methods, "methods");
            Require.NotNull(messagesById, "messagesById");
            Require.NotNull(serviceAssembly, "serviceAssembly");

            Type = serviceType;
            ServiceAssembly = serviceAssembly;
            Methods = methods;
            Messages = messagesById;

            var callbackAttributes = serviceType.GetCustomAttributes(typeof(ProtoCallbackContractAttribute), true);

            if (callbackAttributes.Length > 0)
            {
                Debug.Assert(callbackAttributes.Length == 1);

                CallbackContractType = ((ProtoCallbackContractAttribute)callbackAttributes[0]).Type;
            }
        }
    }
}
