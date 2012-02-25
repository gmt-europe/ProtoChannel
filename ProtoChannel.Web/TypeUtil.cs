using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal static class TypeUtil
    {
        public static object GetDefaultValue(Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
                return Activator.CreateInstance(type);
            else
                return null;
        }

        public static Type GetCollectionType(Type type)
        {
            if (type == typeof(string) || type == typeof(byte[]))
                return null;

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return type.GetGenericArguments()[0];
            }
            else
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        return @interface.GetGenericArguments()[0];
                }
            }

            return null;
        }
    }
}
