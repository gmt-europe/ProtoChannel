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
        public ServiceTypeByIdCollection TypesById { get; private set; }

        public ServiceTypeByTypeCollection TypesByType { get; private set; }

        public ServiceAssembly(Assembly assembly)
        {
            Require.NotNull(assembly, "assembly");

            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(ProtoContractAttribute), true);

                if (attributes.Length == 0)
                    continue;

                var serviceType = new ServiceType(type);

                TypesByType.Add(serviceType);

                if (serviceType.Message != null)
                    TypesById.Add(serviceType);
            }
        }
    }
}
