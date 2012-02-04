using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ProtoMessageAttribute : Attribute
    {
        public ProtoMessageAttribute(int messageId)
        {
            MessageId = messageId;
        }

        public int MessageId { get; private set; }
    }
}
