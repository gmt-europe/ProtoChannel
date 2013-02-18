using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoChannel
{
    internal class PendingSendStream : PendingStream
    {
        private bool _disposed;

        public Stream Stream { get; private set; }

        public bool IsAccepted { get; set; }

        public long Position { get; set; }

        public PendingSendStream(long length, string streamName, string contentType, int associationId, Stream stream)
            : base(length, streamName, contentType, associationId)
        {
            Require.NotNull(stream, "stream");

            Stream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (Stream != null)
                {
                    Stream.Dispose();
                    Stream = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
