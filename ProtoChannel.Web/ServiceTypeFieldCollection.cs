using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class ServiceTypeFieldCollection : Dictionary<int, ServiceTypeField>
    {
        public void Add(ServiceTypeField item)
        {
            Add(item.Tag, item);
        }
    }
}
