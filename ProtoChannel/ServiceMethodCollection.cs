using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ServiceMethodCollection : Dictionary<ServiceMessage, ServiceMethod>
    {
        public void Add(ServiceMethod item)
        {
            Add(item.Request, item);
        }
    }
}
