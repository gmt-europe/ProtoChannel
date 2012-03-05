using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ProtoChannel
{
    internal class SendStreamManager
    {
        private const int MaxAssociationId = 0x1fffff; // 21 bits

        private int _nextAssociationId;
        private readonly Queue<PendingSendStream> _sendQueue = new Queue<PendingSendStream>();
        private readonly Dictionary<int, PendingSendStream> _streams = new Dictionary<int, PendingSendStream>();

        public int RegisterStream(Stream stream, string streamName, string contentType, int? associationId)
        {
            Require.NotNull(stream, "stream");

            if (_streams.Count == MaxAssociationId)
                throw new ProtoChannelException("No stream association ID's are available");

            stream.Position = 0;

            var protoStream = new PendingSendStream(
                stream.Length, streamName, contentType, GetNextAssociationId(associationId), stream
            );

            _streams.Add(protoStream.AssociationId, protoStream);

            return protoStream.AssociationId;
        }

        private int GetNextAssociationId(int? associationId)
        {
            Require.That(!associationId.HasValue || associationId.Value >= 0, "Association ID may not be negative", "associationId");

            if (associationId.HasValue)
            {
                if (_streams.ContainsKey(associationId.Value))
                    throw new ProtoChannelException("Requested association ID is already in use");

                return associationId.Value;
            }

            while (_streams.ContainsKey(_nextAssociationId))
            {
                _nextAssociationId = (_nextAssociationId + 1) & MaxAssociationId;
            }

            int result = _nextAssociationId;

            _nextAssociationId = (_nextAssociationId + 1) & MaxAssociationId;

            return result;
        }

        public ProtocolError? AcceptStream(int associationId)
        {
            PendingSendStream stream;

            if (!_streams.TryGetValue(associationId, out stream))
                return ProtocolError.InvalidStreamAssociationId;

            if (stream.IsAccepted)
            {
                RemoveStream(stream);

                return ProtocolError.InvalidStreamPackageType;
            }

            stream.IsAccepted = true;

            _sendQueue.Enqueue(stream);

            return null;
        }

        public ProtocolError? RejectStream(int associationId)
        {
            PendingSendStream stream;

            if (!_streams.TryGetValue(associationId, out stream))
                return ProtocolError.InvalidStreamAssociationId;

            if (stream == null)
                return ProtocolError.InvalidStreamAssociationId;

            RemoveStream(stream);

            if (stream.IsAccepted)
                return ProtocolError.InvalidStreamPackageType;
            else
                return null;
        }

        private void RemoveStream(PendingSendStream stream)
        {
            // The stream is not removed from the queue. Processing the queue
            // knows that it should skip disposed streams.

            _streams.Remove(stream.AssociationId);

            stream.Dispose();
        }

        public StreamSendRequest? GetSendRequest()
        {
            PendingSendStream stream = null;

            while (_sendQueue.Count > 0)
            {
                stream = _sendQueue.Dequeue();

                if (!stream.IsDisposed)
                    break;
            }

            if (stream == null)
                return null;

            Debug.Assert(stream.IsAccepted);

            long length = Math.Min(
                stream.Stream.Length - stream.Stream.Position,
                Constants.StreamDataSize
            );

            // If we're not at the end yet, put the stream pack into the end
            // of the queue for the next run.

            if (length > 0)
                _sendQueue.Enqueue(stream);
            else
                RemoveStream(stream);

            return new StreamSendRequest(stream, length);
        }
    }
}
