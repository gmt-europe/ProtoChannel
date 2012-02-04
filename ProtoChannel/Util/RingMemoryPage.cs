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
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");

            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        public byte[] Buffer { get; private set; }
        public int Offset { get; private set; }
        public int Count { get; private set; }
    }
}
