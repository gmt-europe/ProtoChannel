using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ReceiveStreamManager
    {
        private readonly IStreamManager _streamManager;
        private readonly Dictionary<uint, PendingReceiveStream> _streams = new Dictionary<uint, PendingReceiveStream>();

        public ReceiveStreamManager(IStreamManager streamManager)
        {
            if (streamManager == null)
                throw new ArgumentNullException("streamManager");

            _streamManager = streamManager;
        }

        public bool RegisterStream(uint associationId, Messages.StartStream message)
        {
            var protoStream = new ProtoStream(message.Length, message.StreamName, message.ContentType, null);

            var stream = _streamManager.GetStream(protoStream);

            if (stream == null)
                return false;

            var pendingStream = new PendingReceiveStream(
                message.Length, message.StreamName, message.ContentType, associationId, stream
            );

            _streams.Add(pendingStream.AssociationId, pendingStream);

            return true;
        }

        public ProtocolError? TryGetStream(uint associationId, out PendingReceiveStream stream)
        {
            if (!_streams.TryGetValue(associationId, out stream))
                return ProtocolError.InvalidStreamAssociationId;

            if (stream.IsDisposed)
            {
                RemoveStream(stream);

                stream = null;

                return ProtocolError.InvalidStreamPackageType;
            }

            return null;
        }

        public ProtocolError? EndStream(uint associationId)
        {
            PendingReceiveStream stream;

            if (!_streams.TryGetValue(associationId, out stream))
                return ProtocolError.InvalidStreamAssociationId;

            stream.SetAsCompleted();

            bool wasDisposed = stream.IsDisposed;

            RemoveStream(stream);

            if (wasDisposed)
                return ProtocolError.InvalidStreamPackageType;
            else
                return null;
        }

        private void RemoveStream(PendingReceiveStream stream)
        {
            _streams.Remove(stream.AssociationId);

            stream.Dispose();
        }

        public IAsyncResult BeginGetStream(uint streamId, AsyncCallback callback, object asyncState)
        {
            PendingReceiveStream stream;

            if (!_streams.TryGetValue(streamId, out stream))
                throw new ProtoChannelException("Stream is not available");

            return stream.GetAsyncResult(callback, asyncState);
        }
    }
}
