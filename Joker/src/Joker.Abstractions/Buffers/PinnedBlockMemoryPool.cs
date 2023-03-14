using System.Buffers;
using System.Collections.Concurrent;

namespace Joker.Buffers
{
    public sealed class PinnedBlockMemoryPool : MemoryPool<byte>
    {
        private const int blockSize = 4096;

        public override int MaxBufferSize => blockSize;

        public static int BlockSize => blockSize;

        private readonly ConcurrentQueue<MemoryPoolBlock> blocks = new();
        private bool isDisposed; // To detect redundant calls
        private readonly object disposeSync = new();

        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            if (minBufferSize > blockSize)
            {
                throw new ArgumentOutOfRangeException(nameof(minBufferSize), $"Cannot allocate more than {blockSize} bytes in a single buffer");
            }

            ObjectDisposedException.ThrowIf(isDisposed, nameof(isDisposed));

            if (blocks.TryDequeue(out var block))
            {
                // block successfully taken from the stack - return it
                return block;
            }
            return new MemoryPoolBlock(this, BlockSize);
        }

        internal void Return(MemoryPoolBlock block)
        {
            if (!isDisposed)
            {
                blocks.Enqueue(block);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            lock (disposeSync)
            {
                isDisposed = true;

                if (disposing)
                {
                    // Discard blocks in pool
                    while (blocks.TryDequeue(out _))
                    {
                    }
                }
            }
        }
    }
}