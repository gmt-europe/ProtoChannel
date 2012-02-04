using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ProtoChannel.Util
{
    internal class RingMemoryStream : Stream
    {
        private const long InitialPagesBuffer = 16;

        private RingMemoryBufferManager _manager;
        private byte[][] _pages;
        private long _pagesOffset;
        private long _length;
        private long _position;
        private bool _disposed;
        private long _head;
        private long _capacity;

        public RingMemoryStream(int blockSize)
        {
            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException("blockSize");

            BlockSize = blockSize;

            _manager = RingMemoryBufferManager.GetManager(blockSize);
            _pages = new byte[InitialPagesBuffer][];
        }

        public long Head
        {
            get { return _head; }
            set
            {
                VerifyNotDisposed();

                if (value < _head)
                    throw new ArgumentOutOfRangeException("value", "Cannot decrease header");
                if (value > _length)
                    throw new ArgumentOutOfRangeException("value", "Head is greater than the length");
                if (value > _position)
                    throw new ArgumentOutOfRangeException("value", "Head is greater than the position");

                long startPage = GetPage(_head);
                long endPage = GetPage(value);

                Debug.Assert(startPage == _pagesOffset);

                long toRemove = endPage - startPage;

                for (long i = 0; i < toRemove; i++)
                {
                    _manager.ReleaseBlock(_pages[i]);
                }

                PagesAllocated -= toRemove;
                _pagesOffset = endPage;

                if (PagesAllocated > 0)
                    Array.Copy(_pages, toRemove, _pages, 0, PagesAllocated);

                if (PagesAllocated < _pages.Length)
                    Array.Clear(_pages, (int)PagesAllocated, (int)(_pages.Length - PagesAllocated));

                _head = value;
            }
        }

        public int BlockSize { get; private set; }

        internal long PagesAllocated { get; private set; }

        public long Capacity
        {
            get { return _capacity; }
            set
            {
                if (value < _capacity)
                    throw new ArgumentOutOfRangeException("value");

                long startPage = GetPage(_capacity);
                long endPage = GetPage(value - 1);

                long requiredPages = endPage - _pagesOffset + 1;

                EnsurePagesBuffer(requiredPages);

                for (long i = startPage - _pagesOffset; i < requiredPages; i++)
                {
                    _pages[i] = _manager.GetBlock();
                }

                PagesAllocated = requiredPages;

                _capacity = GetPageOffset(endPage + 1);
            }
        }

        private void EnsurePagesBuffer(long count)
        {
            if (_pages.Length < count)
            {
                count = (count & ~(InitialPagesBuffer - 1)) + InitialPagesBuffer;

                var newPages = new byte[count][];

                Array.Copy(_pages, newPages, PagesAllocated);

                _pages = newPages;
            }
        }

        private long GetPage(long offset)
        {
            return offset / BlockSize;
        }

        private long GetPageOffset(long page)
        {
            return page * BlockSize;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            VerifyNotDisposed();

            // No-op.
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return _position; }
            set
            {
                VerifyNotDisposed();

                VerifyValidRange(value, value);

                _position = value;
            }
        }

        private void VerifyValidRange(long start, long end)
        {
            if (start < _head || end > _length)
                throw new ArgumentException("Parameters are outside of valid range");
        }

        private void VerifyCapacityRange(long start, long end)
        {
            if (start < _head || end > _capacity)
                throw new ArgumentException("Parameters are outside of valid range");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            VerifyNotDisposed();

            switch (origin)
            {
                case SeekOrigin.Current: offset += _position; break;
                case SeekOrigin.End: offset += _length; break;
            }

            VerifyValidRange(offset, offset);

            _position = offset;

            return _position;
        }

        public override void SetLength(long value)
        {
            VerifyNotDisposed();

            if (value < _length)
                throw new ArgumentOutOfRangeException("value", "Length cannot be decreased");

            _length = value;

            if (_length > _capacity)
                Capacity = _length;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            VerifyNotDisposed();

            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");

            if (count == 0)
                return 0;

            long endPosition = _position + count;

            VerifyValidRange(_position, endPosition);

            long startPage = GetPage(_position);
            long endPage = GetPage(endPosition - 1);

            for (long i = startPage; i <= endPage; i++)
            {
                long pageOffset =
                    i == startPage
                    ? _position - GetPageOffset(i)
                    : 0;

                long pageCount =
                    i == endPage
                    ? (endPosition - GetPageOffset(i)) - pageOffset
                    : BlockSize - pageOffset;

                Array.Copy(_pages[i - _pagesOffset], pageOffset, buffer, offset, pageCount);

                offset += (int)pageCount;
            }

            _position = endPosition;

            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            VerifyNotDisposed();

            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");

            if (count == 0)
                return;

            long endPosition = _position + count;

            if (endPosition > _length)
                SetLength(endPosition);

            VerifyValidRange(_position, endPosition);

            long startPage = GetPage(_position);
            long endPage = GetPage(endPosition - 1);

            for (long i = startPage; i <= endPage; i++)
            {
                long pageOffset =
                    i == startPage
                    ? _position - GetPageOffset(i)
                    : 0;

                long pageCount =
                    i == endPage
                    ? (endPosition - GetPageOffset(i)) - pageOffset
                    : BlockSize - pageOffset;

                Array.Copy(buffer, offset, _pages[i - _pagesOffset], pageOffset, pageCount);

                offset += (int)pageCount;
            }

            _position = endPosition;
        }

        public override int ReadByte()
        {
            VerifyNotDisposed();

            long endPosition = _position + 1;

            VerifyValidRange(_position, endPosition);

            long startPage = GetPage(_position);

            long pageOffset = _position - GetPageOffset(startPage);

            int result = (int)_pages[startPage - _pagesOffset][pageOffset];

            _position++;

            return result;
        }

        public override void WriteByte(byte value)
        {
            VerifyNotDisposed();

            long endPosition = _position + 1;

            SetLength(endPosition);

            VerifyValidRange(_position, endPosition);

            long startPage = GetPage(_position);

            long pageOffset = _position - GetPageOffset(startPage);

            _pages[startPage - _pagesOffset][pageOffset] = value;

            _position++;
        }

        public RingMemoryPage[] GetPages(long offset, long count)
        {
            VerifyNotDisposed();

            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            VerifyCapacityRange(offset, offset + count);

            long startPage = GetPage(offset);
            long endPage = GetPage(offset + count - 1);

            var result = new RingMemoryPage[endPage - startPage + 1];

            for (long i = startPage; i <= endPage; i++)
            {
                long pageOffset =
                    i == startPage
                    ? offset - GetPageOffset(i)
                    : 0;

                long pageCount =
                    i == endPage
                    ? (offset + count) - GetPageOffset(i)
                    : BlockSize - pageOffset;

                result[i] = new RingMemoryPage(_pages[i - _pagesOffset], (int)pageOffset, (int)pageCount);
            }

            return result;
        }

        public RingMemoryPage GetPage(long offset, long count)
        {
            VerifyNotDisposed();

            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            VerifyCapacityRange(offset, offset + count);

            long startPage = GetPage(offset);
            long endPage = GetPage(offset + count - 1);

            if (startPage != endPage)
                throw new ArgumentOutOfRangeException("count", "GetPage cannot span pages");

            long pageOffset = offset - GetPageOffset(startPage);

            return new RingMemoryPage(_pages[startPage - _pagesOffset], (int)pageOffset, (int)count);
        }

        public RingMemoryPage GetWriteBuffer()
        {
            VerifyNotDisposed();

            // Ensure that we have space available.

            if (Capacity == Length)
                Capacity++;

            long page = GetPage(Length);
            long pageOffset = _length - GetPageOffset(page);
            long pageCount = BlockSize - pageOffset;

            Debug.Assert(pageCount > 0);

            return new RingMemoryPage(
                _pages[page - _pagesOffset],
                (int)pageOffset,
                (int)pageCount
            );
        }

        private void VerifyNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                for (int i = 0; i < PagesAllocated; i++)
                {
                    _manager.ReleaseBlock(_pages[i]);
                }

                _pages = null;

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
