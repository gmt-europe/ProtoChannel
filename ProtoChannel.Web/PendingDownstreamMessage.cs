using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class PendingDownstreamMessage
    {
        public MessageKind Kind { get; private set; }
        public uint AssociationId { get; private set; }
        public object Message { get; private set; }

        public PendingDownstreamMessage(MessageKind kind, uint associationId, object message)
        {
            Require.NotNull(message, "message");

            Kind = kind;
            AssociationId = associationId;
            Message = message;
        }
    }
}
