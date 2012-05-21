using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ProtoChannel.Util
{
    /// <summary>
    /// Memory based <see cref="Stream"/> implementation which supports an
    /// endless stream.
    /// </summary>
    /// <remarks>
    /// A <see cref="RingMemoryStream"/> is a stream which is exposed as an
    /// endless stream. Internally it makes use of a shared cache of memory
    /// pages to limit the required number of allocations.
    /// </remarks>
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

        /// <summary>
        /// Initialize a new instance of the <see cref="RingMemoryStream"/> class
        /// with the specified block size.
        /// </summary>
        /// <remarks>
        /// The block size of a <see cref="RingMemoryStream"/> defines how large
        /// the memory pages are which are used to provide the memory of the stream.
        /// Advised is to choose a relatively large block size. When providing
        /// more than 85,000 bytes, it has the advantages that the large object
        /// heap is used and the allocated memory isn't checked by the garbage
        /// collector that much (see http://msdn.microsoft.com/en-us/magazine/cc534993.aspx 
        /// for more information).
        /// </remarks>
        /// <param name="blockSize">The block size of the memory pages of
        /// the <see cref="RingMemoryStream"/></param>
        public RingMemoryStream(int blockSize)
        {
            Require.That(blockSize > 0, "Block size must be greater than zero", "blockSize");

            BlockSize = blockSize;

            _manager = RingMemoryBufferManager.GetManager(blockSize);
            _pages = new byte[InitialPagesBuffer][];
        }

        /// <summary>
        /// Gets or sets the current head of the stream.
        /// </summary>
        /// <remarks>
        /// The head of the stream is the position until which point all data
        /// has been processed and the backing pages have been released.
        /// </remarks>
        public long Head
        {
            get { return _head; }
            set
            {
                VerifyNotDisposed();

                Require.That(value >= _head, "Cannot decrease header", "value");
                Require.That(value <= _length, "Head is greater than the length", "value");
                Require.That(value <= _position, "Head is greater than the position", "value");

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

        /// <summary>
        /// Gets the block size of the stream.
        /// </summary>
        public int BlockSize { get; private set; }

        /// <summary>
        /// Gets the number of pages that are currently in use by the stream.
        /// </summary>
        internal long PagesAllocated { get; private set; }

        /// <summary>
        /// Gets or sets the capacity of the stream.
        /// </summary>
        /// <remarks>
        /// The capacity of the stream is the range (excluding up until the head)
        /// of memory that is allocated for the stream.
        /// </remarks>
        public long Capacity
        {
            get { return _capacity; }
            set
            {
                Require.That(value >= _capacity, "Capacity cannot be decreased", "value");

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

        /// <summary>
        /// Ensures that the <see cref="_pages"/> array has at least <see cref="count"/>
        /// positions available.
        /// </summary>
        /// <remarks>
        /// <see cref="count"/> is rounded up according to <see cref="InitialPagesBuffer"/>.
        /// </remarks>
        /// <param name="count">The minimal number of positions that must be available</param>
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

        /// <summary>
        /// Gets the page for the specified offset.
        /// </summary>
        /// <param name="offset">The offset of which to get the page</param>
        /// <returns>The page of the offset</returns>
        private long GetPage(long offset)
        {
            return offset / BlockSize;
        }

        /// <summary>
        /// Gets the offset of the first byte of the specified page.
        /// </summary>
        /// <param name="page">The page of which to get the offset</param>
        /// <returns>The offset of the first byte of the specified page</returns>
        private long GetPageOffset(long page)
        {
            return page * BlockSize;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>2</filterpriority>
        public override void Flush()
        {
            VerifyNotDisposed();

            // No-op.
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Length
        {
            get { return _length; }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
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

        /// <summary>
        /// Validates whether the specified range is valid taking <see cref="Head"/>
        /// and <see cref="Length"/> into account.
        /// </summary>
        /// <param name="start">The start of the range</param>
        /// <param name="end">The end of the range</param>
        private void VerifyValidRange(long start, long end)
        {
            if (start < _head || end > _length)
                throw new ArgumentException("Parameters are outside of valid range");
        }

        /// <summary>
        /// Validates whether the specified range is valid taking <see cref="Head"/>
        /// and <see cref="Capacity"/> into account.
        /// </summary>
        /// <param name="start">The start of the range</param>
        /// <param name="end">The end of the range</param>
        private void VerifyCapacityRange(long start, long end)
        {
            if (start < _head || end > _capacity)
                throw new ArgumentException("Parameters are outside of valid range");
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter. </param><param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
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

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            VerifyNotDisposed();

            Require.That(value >= _length, "Length cannot be decreased", "value");

            _length = value;

            // Capacity manages the actual allocation of new pages. Ensure that
            // we have enough capacity to facilitate the specified length.

            if (_length > _capacity)
                Capacity = _length;
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. </param><param name="count">The maximum number of bytes to be read from the current stream. </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support reading. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            VerifyNotDisposed();

            Require.NotNull(buffer, "buffer");
            Require.That(offset >= 0 && offset <= buffer.Length, "Offset must fall within the buffer", "offset");
            Require.That(count >= 0 && offset + count <= buffer.Length, "Count must fall within the buffer", "count");

            if (count == 0)
                return 0;

            long endPosition = _position + count;

            VerifyValidRange(_position, endPosition);

            // Calculate the complete range of pages based on the data that
            // is being read.

            long startPage = GetPage(_position);
            long endPage = GetPage(endPosition - 1);

            // Iterate over all pages that fall within the range that is being
            // read and copy the requested bytes from these pages into the target
            // array.

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

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. </param><param name="count">The number of bytes to be written to the current stream. </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support writing. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            VerifyNotDisposed();

            Require.NotNull(buffer, "buffer");
            Require.That(offset >= 0 && offset <= buffer.Length, "Offset must fall within the buffer", "offset");
            Require.That(count >= 0 && offset + count <= buffer.Length, "Count must fall within the buffer", "count");

            if (count == 0)
                return;

            long endPosition = _position + count;

            // Ensure that we have enough memory to write the new data to.

            if (endPosition > _length)
                SetLength(endPosition);

            VerifyValidRange(_position, endPosition);

            // Calculate the complete range of pages based on the data that
            // is being read.

            long startPage = GetPage(_position);
            long endPage = GetPage(endPosition - 1);

            // Iterate over all pages that fall within the range that is being
            // read and copy the new data into these pages.

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

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
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

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
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

        /// <summary>
        /// Gets a reference to the backing memory pages based on the provided
        /// offset and count.
        /// </summary>
        /// <remarks>
        /// <see cref="GetPages"/> provides access to the backing pages of the
        /// stream. Reading from or writing to the stream, this method is preferred
        /// over <see cref="Read"/> because working with the backing pages limits
        /// the number of extra memory allocations that need to be made.
        /// </remarks>
        /// <param name="offset">The start offset for which to get the backing pages</param>
        /// <param name="count">The number of bytes the returned pages must span</param>
        /// <returns>The memory pages describing the requested range</returns>
        public RingMemoryPage[] GetPages(long offset, long count)
        {
            VerifyNotDisposed();

            Require.That(count > 0, "Count must be greater than zero", "count");

            VerifyCapacityRange(offset, offset + count);

            // Calculate the complete range of pages based on the data that
            // is being read.

            long startPage = GetPage(offset);
            long endPage = GetPage(offset + count - 1);

            var result = new RingMemoryPage[endPage - startPage + 1];

            // For all returned memory pages, calculate the offset and count
            // for each separate page and return this including a reference to
            // the backing memory for that page.

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

        /// <summary>
        /// Gets a specific backing memory page.
        /// </summary>
        /// <remarks>
        /// <see cref="GetPage"/> returns a single backing memory page based
        /// on the specified offset and count. Note that the <see cref="count"/>
        /// may not span multiple pages.
        /// </remarks>
        /// <param name="offset">The start offset for which to get the backing page</param>
        /// <param name="count">The number of bytes the returned page must span</param>
        /// <returns>The memory page describing the requested range</returns>
        public RingMemoryPage GetPage(long offset, long count)
        {
            VerifyNotDisposed();

            Require.That(count > 0, "Count must be greater than zero", "count");

            VerifyCapacityRange(offset, offset + count);

            long startPage = GetPage(offset);
            long endPage = GetPage(offset + count - 1);

            Require.That(startPage == endPage, "GetPage cannot span pages", "count");

            long pageOffset = offset - GetPageOffset(startPage);

            return new RingMemoryPage(_pages[startPage - _pagesOffset], (int)pageOffset, (int)count);
        }

        /// <summary>
        /// Gets a specific backing memory page to write to.
        /// </summary>
        /// <remarks>
        /// <see cref="GetWriteBuffer"/> gets a single backing memory page to write
        /// to. The buffer that is returned from this method can be used as a target
        /// buffer for other operations, e.g. for a different <see cref="Stream.Read"/>
        /// operation.
        /// </remarks>
        /// <returns></returns>
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

        /// <summary>
        /// Verifies that the <see cref="RingMemoryStream"/> has not yet been
        /// disposed.
        /// </summary>
        private void VerifyNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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
