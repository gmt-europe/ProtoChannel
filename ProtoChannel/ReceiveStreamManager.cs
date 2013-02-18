using System;
using System.Collections.Generic;
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
            var stream = _streamManager.GetStream(message.Length);

            if (stream == null)
                return false;

            var pendingStream = new PendingReceiveStream(
                message.Length,
                message.StreamName,
                message.ContentType,
                message.Attachment ? StreamDisposition.Attachment : StreamDisposition.Inline,
                associationId,
                stream
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

        public ProtocolError? EndStream(int associationId, bool success)
        {
            PendingReceiveStream stream;

            if (!_streams.TryGetValue(associationId, out stream))
                return ProtocolError.InvalidStreamAssociationId;

            if (success)
                stream.SetAsCompleted();
            else
                stream.SetAsFailed(new ProtoChannelException("Stream transfer failed"));

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

        public void SetError(ProtoChannelException exception)
        {
            foreach (var stream in _streams.Values)
            {
                if (!stream.IsCompleted)
                    stream.SetAsFailed(exception);
            }
        }
    }
}
