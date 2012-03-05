using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoChannel
{
    public class HybridStreamManager : IStreamManager
    {
        private readonly DiskStreamManager _diskStreamManager;
        private readonly MemoryStreamManager _memoryStreamManager;

        public int MaxStreamSize
        {
            get { return _diskStreamManager.MaxStreamSize; }
        }

        public int MaxMemoryStreamSize
        {
            get { return _memoryStreamManager.MaxStreamSize; }
        }

        public string Path
        {
            get { return _diskStreamManager.Path; }
        }

        public HybridStreamManager(string path, int maxMemoryStreamSize)
            : this(path, maxMemoryStreamSize, int.MaxValue)
        {
        }

        public HybridStreamManager(string path, int maxMemoryStreamSize, int maxStreamSize)
        {
            if (maxMemoryStreamSize >= maxStreamSize)
                throw new ArgumentException("Max memory stream size must be less than the max stream size");

            _diskStreamManager = new DiskStreamManager(path, maxStreamSize);
            _memoryStreamManager = new MemoryStreamManager(maxMemoryStreamSize);
        }

        public Stream GetStream(ProtoStream stream)
        {
            Require.NotNull(stream, "stream");

            if (stream.Length > MaxStreamSize)
                return null;

            if (stream.Length > MaxMemoryStreamSize)
                return _diskStreamManager.GetStream(stream);
            else
                return _memoryStreamManager.GetStream(stream);
        }
    }
}
