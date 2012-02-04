using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ServiceMethodCollection : KeyedCollection<ServiceMessage, ServiceMethod>
    {
        protected override ServiceMessage GetKeyForItem(ServiceMethod item)
        {
            return item.Request;
        }
    }
}
