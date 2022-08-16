using System;
using System.IO;

namespace Pfim.Tests
{
    public class ChunkedStream : Stream
    {
        private readonly byte[] data;
        public ChunkedStream(byte[] data) => this.data = data;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => data.Length;

        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int toCopy = Math.Min(Math.Min(count, 100), (int)(data.Length - Position));
            Buffer.BlockCopy(data, (int)Position, buffer, offset, toCopy);
            Position += toCopy;
            return toCopy;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
