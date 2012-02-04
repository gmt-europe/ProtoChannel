using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ServiceMessageByTypeCollection : KeyedCollection<Type, ServiceMessage>
    {
        protected override Type GetKeyForItem(ServiceMessage item)
        {
            return item.Type;
        }
    }
}
