using System.Buffers;
using System.Threading;

namespace Pfim.Tests
{
    class PfimAllocator : IImageAllocator
    {
        private int _rented;
        private readonly ArrayPool<byte> _shared = ArrayPool<byte>.Shared;

        public byte[] Rent(int size)
        {
            Interlocked.Increment(ref _rented);
            return _shared.Rent(size);
        }

        public void Return(byte[] data)
        {
            Interlocked.Decrement(ref _rented);
            _shared.Return(data);
        }

        public int Rented => _rented;
    }
}
