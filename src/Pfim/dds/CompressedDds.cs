using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Class representing decoding compressed direct draw surfaces
    /// </summary>
    public abstract class CompressedDds : Dds
    {
        private bool _compressed;
        private PfimConfig _config;

        protected CompressedDds(DdsHeader header) : base(header)
        {
        }

        /// <summary>Uncompress a given block</summary>
        protected abstract int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint width);

        /// <summary>Number of bytes for a pixel in the decoded data</summary>
        protected abstract byte PixelDepth { get; }

        /// <summary>
        /// The length of a block is in pixels. This mainly affects compressed
        /// images as they are encoded in blocks that are divSize by divSize.
        /// Uncompressed DDS do not need this value.
        /// </summary>
        protected abstract byte DivSize { get; }

        /// <summary>
        /// Number of bytes needed to decode block of pixels that is divSize
        /// by divSize.  This takes into account how many bytes it takes to
        /// extract color and alpha information. Uncompressed DDS do not need
        /// this value.
        /// </summary>
        protected abstract byte CompressedBytesPerBlock { get; }
        public override bool Compressed => _compressed;

        private int BytesPerStride => BlocksPerStride * CompressedBytesPerBlock;
        private int BlocksPerStride => (int) (Header.Width / DivSize);

        /// <summary>Decode data into raw rgb format</summary>
        public byte[] DataDecode(Stream stream, PfimConfig config)
        {
#if NETSTANDARD1_3
            // If we are decoding in memory data, decode stream from that instead of
            // an intermediate buffer
            if (stream is MemoryStream s && s.TryGetBuffer(out var arr))
            {
                return InMemoryDecode(arr.Array, (int)s.Position);
            }
#endif

            byte[] data = new byte[Header.Width * Header.Height * PixelDepth];
            uint dataIndex = 0;

            int bufferSize;
            uint pixelsLeft = Header.Width * Header.Height;
            uint divSize = DivSize;

            byte[] streamBuffer = new byte[config.BufferSize];
            int bytesPerStride = BytesPerStride;
            int blocksPerStride = BlocksPerStride;

            do
            {
                int workingSize;
                bufferSize = workingSize = stream.Read(streamBuffer, 0, config.BufferSize);
                int bIndex = 0;
                while (workingSize > 0 && pixelsLeft > 0)
                {
                    // If there is not enough of the buffer to fill the next
                    // set of 16 square pixels Get the next buffer
                    if (workingSize < bytesPerStride)
                    {
                        bufferSize = workingSize = Util.Translate(stream, streamBuffer, bIndex);
                        bIndex = 0;
                    }

                    // Now that we have enough pixels to fill a stride (and
                    // this includes the normally 4 pixels below the stride)
                    for (uint i = 0; i < blocksPerStride; i++)
                    {
                        bIndex = Decode(streamBuffer, data, bIndex, dataIndex, Header.Width);

                        // Advance to the next block, which is (pixel depth *
                        // divSize) bytes away
                        dataIndex += divSize * PixelDepth;
                    }

                    // Each decoded block is divSize by divSize so pixels left
                    // is Width * multiplied by block height
                    pixelsLeft -= Header.Width * divSize;
                    workingSize -= bytesPerStride;

                    // Jump down to the block that is exactly (divSize - 1)
                    // below the current row we are on
                    dataIndex += (PixelDepth * (divSize - 1) * Header.Width);
                }
            } while (bufferSize != 0 && pixelsLeft != 0);

            return data;
        }

        private byte[] InMemoryDecode(byte[] memBuffer, int bIndex)
        {
            byte[] data = new byte[Header.Width * Header.Height * PixelDepth];
            uint dataIndex = 0;
            uint divSize = DivSize;
            int blocksPerStride = BlocksPerStride;
            uint pixelsLeft = Header.Width * Header.Height;

            // Same implementation as the stream based decoding, just a little bit
            // more straightforward.
            while (pixelsLeft > 0)
            {
                for (uint i = 0; i < blocksPerStride; i++)
                {
                    bIndex = Decode(memBuffer, data, bIndex, dataIndex, Header.Width);
                    dataIndex += divSize * PixelDepth;
                }

                pixelsLeft -= Header.Width * divSize;
                dataIndex += (PixelDepth * (divSize - 1) * Header.Width);
            }

            return data;
        }

        protected override void Decode(Stream stream, PfimConfig config)
        {
            _config = config;
            if (config.Decompress)
            {
                Data = DataDecode(stream, config);
            }
            else
            {
                int blocksPerStride = (int)(Header.Width / DivSize);
                long totalSize = blocksPerStride * CompressedBytesPerBlock * (Header.Height / DivSize);

                var width = (int) Header.Width;
                var height = (int) Header.Height;
                for (int i = 1; i < Header.MipMapCout; i++)
                {
                    width = (int)Math.Pow(2, Math.Floor(Math.Log(width - 1, 2)));
                    height = (int)Math.Pow(2, Math.Floor(Math.Log(height - 1, 2)));
                    var widthBlocks = Math.Max(DivSize, width) / DivSize;
                    var heightBlocks = Math.Max(DivSize, height) / DivSize;
                    totalSize += widthBlocks * heightBlocks * CompressedBytesPerBlock;
                }

                Data = new byte[totalSize];
                _compressed = true;
                Util.Fill(stream, Data, config.BufferSize);
            }
        }

        public override void Decompress()
        {
            if (!_compressed)
            {
                return;
            }

            Data = InMemoryDecode(Data, 0);
            _compressed = false;
        }
    }
}
