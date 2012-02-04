using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ProtoMethodAttribute : Attribute
    {
        public bool IsOneWay { get; set; }
    }
}
