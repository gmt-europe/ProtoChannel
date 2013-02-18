using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoChannel.Test.Streaming
{
    internal class FailingStream : Stream
    {
        private MemoryStream _inner;
        private readonly StreamFailureType _type;

        public FailingStream(long length, StreamFailureType type)
        {
            _inner = new MemoryStream(new byte[length]);
            _type = type;
        }

        public override void Close()
        {
            ThrowWhenType(StreamFailureType.Dispose);

            _inner.Close();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override bool CanTimeout
        {
            get { return _inner.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return _inner.ReadTimeout; }
            set { _inner.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _inner.WriteTimeout; }
            set { _inner.WriteTimeout = value; }
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long page = _inner.Position / Constants.StreamDataSize;

            if (page == 0)
                ThrowWhenType(StreamFailureType.InitialRead);
            else if (page == 1)
                ThrowWhenType(StreamFailureType.ReadSecondBlock);

            return _inner.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return _inner.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            return _inner.Seek(offset, loc);
        }

        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return _inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _inner.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _inner.CanWrite; }
        }

        public override long Length
        {
            get { return _inner.Length; }
        }

        public override long Position
        {
            get
            {
                ThrowWhenType(StreamFailureType.ReadPosition);
                
                return _inner.Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        private void ThrowWhenType(StreamFailureType type)
        {
            if ((_type & type) != 0)
                throw new Exception("Failure because of failing stream");
        }

        protected override void Dispose(bool disposing)
        {
            ThrowWhenType(StreamFailureType.Dispose);

            if (_inner != null)
            {
                _inner.Dispose();
                _inner = null;
            }

            base.Dispose(disposing);
        }
    }
}
