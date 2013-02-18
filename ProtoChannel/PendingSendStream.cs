using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.Logging;

namespace ProtoChannel
{
    internal class PendingSendStream : PendingStream
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PendingSendStream));

        private bool _disposed;

        public Stream Stream { get; private set; }
        public bool IsAccepted { get; set; }
        public override long Position { get; set; }

        public PendingSendStream(long length, string streamName, string contentType, StreamDisposition disposition, int associationId, Stream stream)
            : base(length, streamName, contentType, disposition, associationId)
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
                    try
                    {
                        Stream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Disposing send stream failed", ex);
                    }

                    Stream = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
