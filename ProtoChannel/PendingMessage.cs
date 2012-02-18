using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class PendingMessage : AsyncResultImpl<object>
    {
        public ServiceMessage MessageType { get; private set; }

        public uint AssociationId { get; private set; }

        public PendingMessage(ServiceMessage messageType, uint associationId, AsyncCallback callback, object asyncState)
            : base(callback, asyncState)
        {
            MessageType = messageType;
            AssociationId = associationId;
        }
    }
}
