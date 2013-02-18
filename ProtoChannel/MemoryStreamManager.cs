using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoChannel
{
    public class MemoryStreamManager : IStreamManager
    {
        public int MaxStreamSize { get; private set; }

        public MemoryStreamManager()
            : this(int.MaxValue)
        {
        }

        public MemoryStreamManager(int maxStreamSize)
        {
            MaxStreamSize = maxStreamSize;
        }

        public Stream GetStream(long length)
        {
            if (length > MaxStreamSize)
                return null;
            else
                return new MemoryStream();
        }
    }
}
