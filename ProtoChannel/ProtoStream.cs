using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoChannel
{
    public sealed class ProtoStream : IDisposable
    {
        private bool _disposed;

        public long Length { get; private set; }

        public string StreamName { get; private set; }

        public string ContentType { get; private set; }

        public Stream Stream { get; private set; }

        public ProtoStream(long length, string streamName, string contentType, Stream stream)
        {
            Length = length;
            StreamName = streamName;
            ContentType = contentType;
            Stream = stream;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (Stream != null)
                {
                    Stream.Dispose();
                    Stream = null;
                }

                _disposed = true;
            }
        }
    }
}
