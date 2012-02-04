using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal class PendingMessageManager
    {
        private const uint MaxAssociationId = ushort.MaxValue;

        private uint _nextAssociationId;
        private readonly Dictionary<uint, PendingMessage> _pendingMessages = new Dictionary<uint, PendingMessage>();

        internal PendingMessage GetPendingMessage(ServiceMessage messageType, AsyncCallback callback, object asyncState)
        {
            uint associationId = GetAssociationId();

            var pendingMessage = new PendingMessage(messageType, associationId, callback, asyncState);

            _pendingMessages.Add(associationId, pendingMessage);

            return pendingMessage;
        }

        private uint GetAssociationId()
        {
            if (_pendingMessages.Count == MaxAssociationId)
                throw new ProtoChannelException("Cannot allocate association ID because there are too many pending messages");

            while (_pendingMessages.ContainsKey(_nextAssociationId))
            {
                _nextAssociationId++;
            }

            return _nextAssociationId++;
        }

        public PendingMessage RemovePendingMessage(uint associationId)
        {
            PendingMessage result;

            if (!_pendingMessages.TryGetValue(associationId, out result))
                throw new ProtoChannelException("No pending message found for the provided association ID");

            return result;
        }
    }
}
