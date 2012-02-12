using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoBuf.Meta;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ServiceAssembly
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<Type, Service> _registrations = new Dictionary<Type, Service>();

        public Assembly Assembly { get; private set; }

        public RuntimeTypeModel TypeModel { get; private set; }

        public IKeyedCollection<int, ServiceMessage> MessagesById { get; private set; }

        public IKeyedCollection<Type, ServiceMessage> MessagesByType { get; private set; }

        public ServiceAssembly(Assembly assembly, RuntimeTypeModel typeModel, ServiceMessageByIdCollection messagesById, ServiceMessageByTypeCollection messagesByType)
        {
            Require.NotNull(assembly, "assembly");
            Require.NotNull(typeModel, "typeModel");
            Require.NotNull(messagesById, "messagesById");
            Require.NotNull(messagesByType, "messagesByType");

            Assembly = assembly;
            TypeModel = typeModel;
            MessagesById = new ReadOnlyKeyedCollection<int, ServiceMessage>(messagesById);
            MessagesByType = new ReadOnlyKeyedCollection<Type, ServiceMessage>(messagesByType);
        }

        public Service GetServiceRegistration(Type serviceType)
        {
            Require.NotNull(serviceType, "serviceType");

            lock (_syncRoot)
            {
                Service service;

                if (!_registrations.TryGetValue(serviceType, out service))
                {
                    service = CreateServiceRegistration(serviceType);

                    _registrations.Add(serviceType, service);
                }

                return service;
            }
        }

        private Service CreateServiceRegistration(Type serviceType)
        {
            var methods = new ServiceMethodCollection();
            var messages = new ServiceMessageByIdCollection();

            foreach (var method in serviceType.GetMethods())
            {
                var methodAttributes = method.GetCustomAttributes(typeof(ProtoMethodAttribute), true);

                if (methodAttributes.Length == 0)
                    continue;

                Debug.Assert(methodAttributes.Length == 1);

                var methodAttribute = (ProtoMethodAttribute)methodAttributes[0];

                var serviceMethod = new ServiceMethod(method, methodAttribute, this);

                if (methods.Contains(serviceMethod.Request))
                    throw new ProtoChannelException(String.Format("Invalid service contract '{0}'; multiple ProtoMethod's found for message type '{1}'", serviceType, serviceMethod.Request.Type));

                if (!messages.Contains(serviceMethod.Request.Id))
                    messages.Add(serviceMethod.Request);

                if (serviceMethod.Response != null && !messages.Contains(serviceMethod.Response.Id))
                    messages.Add(serviceMethod.Response);

                methods.Add(serviceMethod);
            }

            if (methods.Count == 0)
                throw new ProtoChannelException(String.Format("Invalid service contract '{0}'; contract does not specify any handlers", serviceType));

            return new Service(serviceType, methods, messages, this);
        }
    }
}
