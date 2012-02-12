using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ProtoCallbackContractAttribute : Attribute
    {
        public Type Type { get; private set; }

        public ProtoCallbackContractAttribute(Type type)
        {
            Require.NotNull(type, "type");

            if (!typeof(ProtoCallbackChannel).IsAssignableFrom(type))
                throw new ProtoChannelException("Callback contract type is not of type ProtoCallbackChannel");

            Type = type;
        }
    }
}
