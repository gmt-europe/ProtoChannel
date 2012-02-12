using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal class ServiceMessage
    {
        private readonly ProtoMessageAttribute _attribute;

        public Type Type { get; private set; }

        public int Id
        {
            get { return _attribute.MessageId; }
        }

        public ServiceMessage(ProtoMessageAttribute attribute, Type type)
        {
            Type = type;
            Require.NotNull(attribute, "attribute");
            Require.NotNull(type, "type");

            _attribute = attribute;
        }
    }
}
