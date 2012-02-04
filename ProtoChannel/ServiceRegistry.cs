using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal static class ServiceRegistry
    {
        private static readonly object _syncRoot = new object();
        private static readonly Dictionary<Type, Service> _registrations = new Dictionary<Type, Service>();
        private static readonly Dictionary<Type, ServiceMessage> _messages = new Dictionary<Type, ServiceMessage>();

        public static Service GetServiceRegistration(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

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

        private static Service CreateServiceRegistration(Type serviceType)
        {
            var methods = new ServiceMethodCollection();
            var messages = new ServiceMessageCollection();

            foreach (var method in serviceType.GetMethods())
            {
                var methodAttributes = method.GetCustomAttributes(typeof(ProtoMethodAttribute), true);

                if (methodAttributes.Length == 0)
                    continue;

                Debug.Assert(methodAttributes.Length == 1);

                var methodAttribute = (ProtoMethodAttribute)methodAttributes[0];

                var serviceMethod = new ServiceMethod(method, methodAttribute);

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

            return new Service(serviceType, methods, messages);
        }

        public static ServiceMessage GetMessageRegistration(Type messageType)
        {
            if (messageType == null)
                throw new ArgumentNullException("messageType");

            lock (_syncRoot)
            {
                ServiceMessage message;

                if (!_messages.TryGetValue(messageType, out message))
                {
                    message = CreateMessageRegistration(messageType);

                    _messages.Add(messageType, message);
                }

                return message;
            }
        }

        private static ServiceMessage CreateMessageRegistration(Type messageType)
        {
            var messageAttributes = messageType.GetCustomAttributes(typeof(ProtoMessageAttribute), true);

            if (messageAttributes.Length == 0)
                throw new ProtoChannelException(String.Format("Type '{0}' does not specify the ProtoMessage attribute", messageType));

            Debug.Assert(messageAttributes.Length == 1);

            var messageAttribute = (ProtoMessageAttribute)messageAttributes[0];

            return new ServiceMessage(messageAttribute, messageType);
        }
    }
}
