using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ProtoMessageAttribute : Attribute
    {
        public ProtoMessageAttribute(int messageId)
        {
            Require.That(messageId >= 1, "Message ID must be greater than zero", "messageId");

            MessageId = messageId;
        }

        public int MessageId { get; private set; }
    }
}
