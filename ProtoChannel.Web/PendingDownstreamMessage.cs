using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class PendingDownstreamMessage
    {
        public uint AssociationId { get; private set; }

        public object Message { get; private set; }

        public PendingDownstreamMessage(uint associationId, object message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            AssociationId = associationId;
            Message = message;
        }
    }
}
