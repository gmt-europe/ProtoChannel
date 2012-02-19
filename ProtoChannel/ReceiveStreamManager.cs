using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal class ReceiveStreamManager
    {
        private readonly IStreamManager _streamManager;
        private readonly Dictionary<int, PendingReceiveStream> _streams = new Dictionary<int, PendingReceiveStream>();

        public ReceiveStreamManager(IStreamManager streamManager)
        {
            Require.NotNull(streamManager, "streamManager");

            _streamManager = streamManager;
        }

        public bool RegisterStream(int associationId, Messages.StartStream message)
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

        public ProtocolError? TryGetStream(int associationId, out PendingReceiveStream stream)
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

        public ProtocolError? EndStream(int associationId)
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
            if (stream.IsRequested && stream.IsCompleted)
            {
                _streams.Remove(stream.AssociationId);

                stream.Dispose();
            }
        }

        public IAsyncResult BeginGetStream(int streamId, AsyncCallback callback, object asyncState)
        {
            PendingReceiveStream stream;

            if (!_streams.TryGetValue(streamId, out stream))
                throw new ProtoChannelException("Stream is not available");

            stream.IsRequested = true;

            RemoveStream(stream);

            return stream.GetAsyncResult(callback, asyncState);
        }
    }
}
