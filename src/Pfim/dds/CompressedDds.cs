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

        /// <summary>Decompress a given block</summary>
        protected abstract int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint stride);

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
        private int BlocksPerStride => Util.Stride((int)Header.Width, PixelDepth) / DivSize;

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

            var strideBytes = BlocksPerStride * DivSize * PixelDepth;
            var stride = Util.Stride((int) Header.Width, PixelDepth);
            var heightBlocks = Util.Stride((int) Header.Height, PixelDepth) / DivSize;
            var len = BlocksPerStride * heightBlocks * DivSize * DivSize * PixelDepth;
            byte[] data = new byte[len];
            int pixelsLeft = len;
            int dataIndex = 0;

            int bufferSize;
            int divSize = DivSize;

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

                    var origDataIndex = dataIndex;

                    // Now that we have enough pixels to fill a stride (and
                    // this includes the normally 4 pixels below the stride)
                    for (uint i = 0; i < blocksPerStride; i++)
                    {
                        bIndex = Decode(streamBuffer, data, bIndex, (uint)dataIndex, (uint)stride);

                        // Advance to the next block, which is (pixel depth *
                        // divSize) bytes away
                        dataIndex += divSize * PixelDepth;
                    }

                    // Each decoded block is divSize by divSize so pixels left
                    // is Width * multiplied by block height
                    //pixelsLeft -= Header.Width * divSize;
                    workingSize -= bytesPerStride;

                    var filled = strideBytes * divSize;
                    pixelsLeft -= filled;

                    // Jump down to the block that is exactly (divSize - 1)
                    // below the current row we are on
                    dataIndex = origDataIndex + filled;
                }
            } while (bufferSize != 0 && pixelsLeft > 0);

            return data;
        }

        private byte[] InMemoryDecode(byte[] memBuffer, int bIndex)
        {
            var stride = Util.Stride((int)Header.Width, PixelDepth);
            var strideBytes = BlocksPerStride * DivSize * PixelDepth;
            var heightBlocks = Util.Stride((int)Header.Height, PixelDepth) / DivSize;
            var len = BlocksPerStride * heightBlocks * DivSize * DivSize * PixelDepth;
            byte[] data = new byte[len];
            var pixelsLeft = len;
            int dataIndex = 0;
            int divSize = DivSize;
            int blocksPerStride = BlocksPerStride;

            // Same implementation as the stream based decoding, just a little bit
            // more straightforward.
            while (pixelsLeft > 0)
            {
                var origDataIndex = dataIndex;

                for (uint i = 0; i < blocksPerStride; i++)
                {
                    bIndex = Decode(memBuffer, data, bIndex, (uint)dataIndex, (uint)stride);
                    dataIndex += divSize * PixelDepth;
                }

                var filled = strideBytes * divSize;
                pixelsLeft -= filled;

                // Jump down to the block that is exactly (divSize - 1)
                // below the current row we are on
                dataIndex = origDataIndex + filled;
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
                long totalSize = BlocksPerStride * CompressedBytesPerBlock * (Header.Height / DivSize);

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
