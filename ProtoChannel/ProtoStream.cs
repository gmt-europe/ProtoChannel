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

        private Stream _stream;

        public ProtoStream(long length, string streamName, string contentType, Stream stream)
        {
            Length = length;
            StreamName = streamName;
            ContentType = contentType;

            _stream = stream;
        }

        public Stream DetachStream()
        {
            var stream = _stream;

            _stream = null;

            return stream;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

                _disposed = true;
            }
        }
    }
}
