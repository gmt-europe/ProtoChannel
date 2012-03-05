using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
#if _NET_4
using System.Collections.Concurrent;
#endif

namespace ProtoChannel.Util
{
#if _NET_4
    internal class RingMemoryBufferManager
    {
        private static readonly ConcurrentDictionary<int, RingMemoryBufferManager> _managers = new ConcurrentDictionary<int, RingMemoryBufferManager>();

        private readonly ConcurrentBag<byte[]> _cache = new ConcurrentBag<byte[]>();

        public int BlockSize { get; private set; }

        private RingMemoryBufferManager(int blockSize)
        {
            BlockSize = blockSize;
        }

        public static RingMemoryBufferManager GetManager(int blockSize)
        {
            return _managers.GetOrAdd(blockSize, p => new RingMemoryBufferManager(p));
        }

        public byte[] GetBlock()
        {
            byte[] result;

            if (!_cache.TryTake(out result))
                result = new byte[BlockSize];

            return result;
        }

        public void ReleaseBlock(byte[] block)
        {
            Require.NotNull(block, "block");

            Debug.Assert(block.Length == BlockSize);

            _cache.Add(block);
        }
    }
#else
    internal class RingMemoryBufferManager
    {
        private static readonly object _staticSyncLock = new object();
        private static readonly Dictionary<int, RingMemoryBufferManager> _managers = new Dictionary<int, RingMemoryBufferManager>();

        private readonly object _syncRoot = new object();
        private readonly Queue<byte[]> _cache = new Queue<byte[]>();

        public int BlockSize { get; private set; }

        private RingMemoryBufferManager(int blockSize)
        {
            BlockSize = blockSize;
        }

        public static RingMemoryBufferManager GetManager(int blockSize)
        {
            lock (_staticSyncLock)
            {
                RingMemoryBufferManager manager;

                if (!_managers.TryGetValue(blockSize, out manager))
                {
                    manager = new RingMemoryBufferManager(blockSize);

                    _managers.Add(blockSize, manager);
                }

                return manager;
            }
        }

        public byte[] GetBlock()
        {
            lock (_syncRoot)
            {
                if (_cache.Count > 0)
                    return _cache.Dequeue();
            }

            return new byte[BlockSize];
        }

        public void ReleaseBlock(byte[] block)
        {
            Require.NotNull(block, "block");

            Debug.Assert(block.Length == BlockSize);

            lock (_syncRoot)
            {
                _cache.Enqueue(block);
            }
        }
    }
#endif
}
