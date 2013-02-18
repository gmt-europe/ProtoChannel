using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if _NET_MD
#pragma warning disable 0168
#else
using Common.Logging;
#endif

namespace ProtoChannel
{
    internal class PendingSendStream : PendingStream
    {
#if !_NET_MD
        private static readonly ILog Log = LogManager.GetLogger(typeof(PendingSendStream));
#endif

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
                    try
                    {
                        Stream.Dispose();
                    }
                    catch (Exception ex)
                    {
#if !_NET_MD
                        Log.Warn("Disposing send stream failed", ex);
#endif
                    }

                    Stream = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
