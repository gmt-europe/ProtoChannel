using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal class SendStreamManager
    {
        private const uint MaxAssociationId = 0x1fffff; // 21 bits

        private uint _nextAssociationId;
        private readonly Queue<PendingSendStream> _sendQueue = new Queue<PendingSendStream>();
        private readonly Dictionary<uint, PendingSendStream> _streams = new Dictionary<uint, PendingSendStream>();

        public uint RegisterStream(Stream stream, string streamName, string contentType)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (_streams.Count == MaxAssociationId)
                throw new ProtoChannelException("No stream association ID's are available");

            stream.Position = 0;

            var protoStream = new PendingSendStream(
                stream.Length, streamName, contentType, GetNextAssociationId(), stream
            );

            _streams.Add(protoStream.AssociationId, protoStream);

            return protoStream.AssociationId;
        }

        private uint GetNextAssociationId()
        {
            while (_streams.ContainsKey(_nextAssociationId))
            {
                _nextAssociationId = (_nextAssociationId + 1) & MaxAssociationId;
            }

            uint result = _nextAssociationId;

            _nextAssociationId = (_nextAssociationId + 1) & MaxAssociationId;

            return result;
        }

        public ProtocolError? AcceptStream(uint associationId)
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

        public ProtocolError? RejectStream(uint associationId)
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

        public ProtocolError? EndStream(uint associationId)
        {
            PendingSendStream stream;

            if (!_streams.TryGetValue(associationId, out stream))
                return ProtocolError.InvalidStreamAssociationId;

            RemoveStream(stream);

            if (!stream.IsAccepted)
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

            return new StreamSendRequest(stream, length);
        }
    }
}
