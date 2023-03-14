using System.Buffers;

namespace Joker.Buffers
{
    public static class PinnedBlockMemoryPoolFactory
    {
        public static MemoryPool<byte> Create()
        {
            return new PinnedBlockMemoryPool();
        }
    }
}