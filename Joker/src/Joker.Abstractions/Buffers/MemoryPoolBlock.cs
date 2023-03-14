using System.Buffers;
using System.Runtime.InteropServices;

namespace Joker.Buffers
{
    public sealed class MemoryPoolBlock : IMemoryOwner<byte>
    {
        public MemoryPoolBlock(PinnedBlockMemoryPool pool, int length)
        {
            Pool = pool;

            var pinnedArray = GC.AllocateUninitializedArray<byte>(length, pinned: true);

            Memory = MemoryMarshal.CreateFromPinnedArray(pinnedArray, 0, pinnedArray.Length);
        }

        public PinnedBlockMemoryPool Pool { get; }

        public Memory<byte> Memory { get; }

        public void Dispose()
        {
            Pool.Return(this);
        }
    }
}