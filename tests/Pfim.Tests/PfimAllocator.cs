using System;
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
            var result = _shared.Rent(size);
            Array.Clear(result, 0, result.Length);
            return result;
        }

        public void Return(byte[] data)
        {
            Interlocked.Decrement(ref _rented);
            _shared.Return(data);
        }

        public int Rented => _rented;
    }
}
