using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ServiceMessageByIdCollection : Dictionary<int, ServiceMessage>
    {
        public void Add(ServiceMessage item)
        {
            Add(item.Id, item);
        }
    }
}
