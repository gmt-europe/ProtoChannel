using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoChannel
{
    public class DiskStreamManager : IStreamManager
    {
        public string Path { get; private set; }

        public int MaxStreamSize { get; private set; }

        public DiskStreamManager(string path)
            : this(path, int.MaxValue)
        {
        }

        public DiskStreamManager(string path, int maxStreamSize)
        {
            Require.NotNull(path, "path");

            if (!Directory.Exists(path))
                throw new ArgumentException("Path does not exist", "path");

            Path = path;
            MaxStreamSize = maxStreamSize;
        }

        public Stream GetStream(ProtoStream stream)
        {
            Require.NotNull(stream, "stream");

            if (stream.Length > MaxStreamSize)
                return null;

            while (true)
            {
                string fileName = System.IO.Path.Combine(Path, Guid.NewGuid().ToString());

                if (!File.Exists(fileName))
                    return File.Create(fileName, 0x4000, FileOptions.DeleteOnClose);
            }
        }
    }
}
