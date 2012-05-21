using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel.Util
{
    /// <summary>
    /// Reference to a section of memory from a <see cref="RingMemoryStream"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="RingMemoryPage"/> contains a reference to a specific memory
    /// page including an offset and length. This information is used to access
    /// the memory of a <see cref="RingMemoryStream"/>.
    /// </remarks>
    internal struct RingMemoryPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RingMemoryPage"/> with
        /// the specified buffer, offset and count.
        /// </summary>
        /// <param name="buffer">The buffer of the memory page</param>
        /// <param name="offset">The offset into the buffer which has been
        /// made available through this memory page
        /// </param>
        /// <param name="count">The number of bytes of the buffer that
        /// has been made available through this memory page</param>
        public RingMemoryPage(byte[] buffer, int offset, int count)
            : this()
        {
            Require.NotNull(buffer, "buffer");
            Require.That(offset >= 0, "Offset must be positive", "offset");
            Require.That(offset + count <= buffer.Length, "Count cannot be beyond the buffer length", "count");

            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        /// <summary>
        /// Gets the buffer to which this <see cref="RingMemoryPage"/> refers.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Gets the offset into the buffer which is available through this
        /// <see cref="RingMemoryPage"/>.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the number of bytes that is available through this
        /// <see cref="RingMemoryPage"/>.
        /// </summary>
        public int Count { get; private set; }
    }
}
