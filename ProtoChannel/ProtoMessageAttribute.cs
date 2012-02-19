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
            Require.That(messageId >= 1, "Message ID must be greater than zero", "messageId");

            MessageId = messageId;
        }

        public int MessageId { get; private set; }
    }
}
