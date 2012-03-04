using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class ServiceTypeByIdCollection : Dictionary<int, ServiceType>
    {
        public void Add(ServiceType item)
        {
            Debug.Assert(item.Message != null);

            Add(item.Message.Id, item);
        }
    }
}
