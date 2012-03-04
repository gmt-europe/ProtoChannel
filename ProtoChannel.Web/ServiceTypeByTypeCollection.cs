using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class ServiceTypeByTypeCollection : Dictionary<Type, ServiceType>
    {
        public void Add(ServiceType item)
        {
            Add(item.Type, item);
        }
    }
}
