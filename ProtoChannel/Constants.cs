using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal static class Constants
    {
        // The ring buffer block size is set to 128k which is the first
        // multiple of 2 after the limit for the large object heap. The limit
        // for the large object heap is 85,000 bytes (see
        // http://msdn.microsoft.com/en-us/magazine/cc534993.aspx).
        // The reason we want to force the large object heap is because the
        // blocks we allocate with the ring buffer are never released and
        // thus we'd like them to be checked as little as possible.
        public const int RingBufferBlockSize = 128 << 10;

        public const int ProtocolVersion = 1;

        public static readonly byte[] Header = { 0x50, 0x43, 0x48, 0x58 };

        public const int StreamDataSize = 0x4000;
    }
}
