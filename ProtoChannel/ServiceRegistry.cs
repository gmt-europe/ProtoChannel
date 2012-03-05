using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using ProtoBuf.Meta;
using ProtoBuf;

namespace ProtoChannel
{
    internal static class ServiceRegistry
    {
        private static readonly object _syncRoot = new object();
        private static readonly Dictionary<Assembly, ServiceAssembly> _assemblies = new Dictionary<Assembly, ServiceAssembly>();

        public static ServiceAssembly GetAssemblyRegistration(Assembly assembly)
        {
            Require.NotNull(assembly, "assembly");

            lock (_syncRoot)
            {
                ServiceAssembly registration;

                if (!_assemblies.TryGetValue(assembly, out registration))
                {
                    registration = CreateAssemblyRegistration(assembly);

                    _assemblies.Add(assembly, registration);
                }

                return registration;
            }
        }

        private static ServiceAssembly CreateAssemblyRegistration(Assembly assembly)
        {
            var messagesById = new ServiceMessageByIdCollection();
            var messagesByType = new ServiceMessageByTypeCollection();
            var typeModel = RuntimeTypeModel.Create();

            foreach (var type in assembly.GetTypes())
            {
                var messageAttributes = type.GetCustomAttributes(typeof(ProtoMessageAttribute), true);

                if (messageAttributes.Length == 0)
                    continue;

                Debug.Assert(messageAttributes.Length == 1);

                var contractAttributes = type.GetCustomAttributes(typeof(ProtoContractAttribute), true);

                if (contractAttributes.Length == 0)
                    throw new ProtoChannelException(String.Format("Type '{0}' specifies the ProtoMessage attribute but not the ProtoContract attribute", type));

                var messageAttribute = (ProtoMessageAttribute)messageAttributes[0];

                var message = new ServiceMessage(messageAttribute, type);

                messagesById.Add(message);
                messagesByType.Add(message);

                typeModel.Add(type, true);
            }

            if (messagesById.Count == 0)
                throw new ProtoChannelException(String.Format("Assembly '{0}' does not contain any messages", assembly));

            return new ServiceAssembly(assembly, typeModel, messagesById, messagesByType);
        }
    }
}
