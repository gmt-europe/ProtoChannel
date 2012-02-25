using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.CodeGenerator
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
    }
}
