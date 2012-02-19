using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProtoChannel.Web
{
    internal static class ServiceRegistry
    {
        private static readonly ConcurrentDictionary<Assembly, ServiceAssembly> _assemblies = new ConcurrentDictionary<Assembly, ServiceAssembly>();

        public static ServiceAssembly GetAssembly(Assembly assembly)
        {
            Require.NotNull(assembly, "assembly");

            return _assemblies.GetOrAdd(assembly, ServiceAssemblyFactory);
        }

        private static ServiceAssembly ServiceAssemblyFactory(Assembly assembly)
        {
            return new ServiceAssembly(assembly);
        }
    }
}
