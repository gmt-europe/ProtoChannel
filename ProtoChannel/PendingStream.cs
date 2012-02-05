using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal abstract class PendingStream : IDisposable
    {
        public long Length { get; private set; }

        public string StreamName { get; private set; }

        public string ContentType { get; private set; }

        public uint AssociationId { get; private set; }

        public bool IsDisposed { get; private set; }

        protected PendingStream(long length, string streamName, string contentType, uint associationId)
        {
            Length = length;
            StreamName = streamName;
            ContentType = contentType;
            AssociationId = associationId;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }
    }
}
