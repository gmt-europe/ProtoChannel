using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoBuf;
using ProtoChannel.Util;

namespace ProtoChannel.Web
{
    internal class ServiceAssembly
    {
        private readonly Dictionary<Type, ServiceType> _typeCache = new Dictionary<Type, ServiceType>();

        public ServiceTypeByIdCollection TypesById { get; private set; }

        public ServiceTypeByTypeCollection TypesByType { get; private set; }

        public ServiceAssembly(Assembly assembly)
        {
            Require.NotNull(assembly, "assembly");

            TypesByType = new ServiceTypeByTypeCollection();
            TypesById = new ServiceTypeByIdCollection();

            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(ProtoContractAttribute), true);

                if (attributes.Length == 0)
                    continue;

                var serviceType = GetServiceType(type);

                TypesByType.Add(serviceType);

                if (serviceType.Message != null)
                    TypesById.Add(serviceType);
            }
        }

        public ServiceType GetServiceType(Type type)
        {
            ServiceType serviceType;

            if (_typeCache.TryGetValue(type, out serviceType))
                return serviceType;

            serviceType = new ServiceType(type);

            _typeCache[type] = serviceType;

            serviceType.Build(this);

            return serviceType;
        }
    }
}
