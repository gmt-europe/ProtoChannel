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
    /// <summary>
    /// Manages memory pages of a specific block size.
    /// </summary>
    /// <remarks>
    /// An instance of the <see cref="RingMemoryBufferManager"/> class manages
    /// memory pages of a specific block size. Instances for a specific block
    /// size are shared between multiple streams.
    /// </remarks>
    internal class RingMemoryBufferManager
    {
        private static readonly ConcurrentDictionary<int, RingMemoryBufferManager> _managers = new ConcurrentDictionary<int, RingMemoryBufferManager>();

        private readonly ConcurrentBag<byte[]> _cache = new ConcurrentBag<byte[]>();

        /// <summary>
        /// Gets the block size managed by the <see cref="RingMemoryBufferManager"/>.
        /// </summary>
        public int BlockSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RingMemoryBufferManager"/>
        /// with the specified block size.
        /// </summary>
        /// <param name="blockSize"></param>
        private RingMemoryBufferManager(int blockSize)
        {
            BlockSize = blockSize;
        }

        /// <summary>
        /// Gets the singleton manager for the specified block size.
        /// </summary>
        /// <param name="blockSize">The block size of the <see cref="RingMemoryBufferManager"/></param>
        /// <returns>The <see cref="RingMemoryBufferManager"/> for the specified block size</returns>
        public static RingMemoryBufferManager GetManager(int blockSize)
        {
            return _managers.GetOrAdd(blockSize, p => new RingMemoryBufferManager(p));
        }

        /// <summary>
        /// Gets a single memory block from the cache or, when not available,
        /// allocates a new block.
        /// </summary>
        /// <returns>A memory block of <see cref="BlockSize"/> size</returns>
        public byte[] GetBlock()
        {
            byte[] result;

            if (!_cache.TryTake(out result))
                result = new byte[BlockSize];

            return result;
        }

        /// <summary>
        /// Releases a memory block and puts it back into the cache.
        /// </summary>
        /// <param name="block">The memory block to release</param>
        public void ReleaseBlock(byte[] block)
        {
            Require.NotNull(block, "block");

            Debug.Assert(block.Length == BlockSize);

            _cache.Add(block);
        }
    }
#else
    /// <summary>
    /// Manages memory pages of a specific block size.
    /// </summary>
    /// <remarks>
    /// An instance of the <see cref="RingMemoryBufferManager"/> class manages
    /// memory pages of a specific block size. Instances for a specific block
    /// size are shared between multiple streams.
    /// </remarks>
    internal class RingMemoryBufferManager
    {
        private static readonly object _staticSyncLock = new object();
        private static readonly Dictionary<int, RingMemoryBufferManager> _managers = new Dictionary<int, RingMemoryBufferManager>();

        private readonly object _syncRoot = new object();
        private readonly Queue<byte[]> _cache = new Queue<byte[]>();

        /// <summary>
        /// Gets the block size managed by the <see cref="RingMemoryBufferManager"/>.
        /// </summary>
        public int BlockSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RingMemoryBufferManager"/>
        /// with the specified block size.
        /// </summary>
        /// <param name="blockSize"></param>
        private RingMemoryBufferManager(int blockSize)
        {
            BlockSize = blockSize;
        }

        /// <summary>
        /// Gets the singleton manager for the specified block size.
        /// </summary>
        /// <param name="blockSize">The block size of the <see cref="RingMemoryBufferManager"/></param>
        /// <returns>The <see cref="RingMemoryBufferManager"/> for the specified block size</returns>
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

        /// <summary>
        /// Gets a single memory block from the cache or, when not available,
        /// allocates a new block.
        /// </summary>
        /// <returns>A memory block of <see cref="BlockSize"/> size</returns>
        public byte[] GetBlock()
        {
            lock (_syncRoot)
            {
                if (_cache.Count > 0)
                    return _cache.Dequeue();
            }

            return new byte[BlockSize];
        }

        /// <summary>
        /// Releases a memory block and puts it back into the cache.
        /// </summary>
        /// <param name="block">The memory block to release</param>
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
