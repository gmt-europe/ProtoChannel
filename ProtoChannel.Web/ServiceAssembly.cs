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
        private ServiceTypeByIdCollection _typesById = new ServiceTypeByIdCollection();
        private ServiceTypeByTypeCollection _typesByType = new ServiceTypeByTypeCollection();

        public Assembly Assembly { get; set; }

        public IKeyedCollection<int, ServiceType> TypesById { get; private set; }

        public IKeyedCollection<Type, ServiceType> TypesByType { get; private set; }

        public ServiceAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            TypesById = new ReadOnlyKeyedCollection<int, ServiceType>(_typesById);
            TypesByType = new ReadOnlyKeyedCollection<Type, ServiceType>(_typesByType);

            Assembly = assembly;

            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(ProtoContractAttribute), true);

                if (attributes.Length == 0)
                    continue;

                var serviceType = new ServiceType(type);

                _typesByType.Add(serviceType);

                if (serviceType.Message != null)
                    _typesById.Add(serviceType);
            }
        }

        private class ServiceTypeByTypeCollection : KeyedCollection<Type, ServiceType>
        {
            protected override Type GetKeyForItem(ServiceType item)
            {
                return item.Type;
            }
        }

        private class ServiceTypeByIdCollection : KeyedCollection<int, ServiceType>
        {
            protected override int GetKeyForItem(ServiceType item)
            {
                Debug.Assert(item.Message != null);

                return item.Message.Id;
            }
        }
    }
}
