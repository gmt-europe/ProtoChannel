using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    public class StreamTransferEventArgs : EventArgs
    {
        public long Length { get; private set; }
        public string StreamName { get; private set; }
        public string ContentType { get; private set; }
        public StreamDisposition Disposition { get; private set; }
        public int StreamId { get; private set; }
        public long Transferred { get; private set; }
        public StreamTransferEventType EventType { get; private set; }

        internal StreamTransferEventArgs(PendingStream stream, StreamTransferEventType eventType)
        {
            Require.NotNull(stream, "stream");

            Length = stream.Length;
            StreamName = stream.StreamName;
            ContentType = stream.ContentType;
            Disposition = stream.Disposition;
            StreamId = stream.AssociationId;
            Transferred = stream.Position;
            EventType = eventType;
        }
    }

    public delegate void StreamTransferEventHandler(object sender, StreamTransferEventArgs e);
}
