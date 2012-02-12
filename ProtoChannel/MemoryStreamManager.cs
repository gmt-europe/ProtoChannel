using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public Stream GetStream(ProtoStream stream)
        {
            Require.NotNull(stream, "stream");

            if (stream.Length > MaxStreamSize)
                return null;
            else
                return new MemoryStream();
        }
    }
}
