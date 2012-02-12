using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel.Util
{
    internal struct RingMemoryPage
    {
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

        public byte[] Buffer { get; private set; }
        public int Offset { get; private set; }
        public int Count { get; private set; }
    }
}
