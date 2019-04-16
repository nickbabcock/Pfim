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

        protected CompressedDds(DdsHeader header, PfimConfig config) : base(header, config)
        {
        }

        /// <summary>Decompress a given block</summary>
        protected abstract int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint stride);

        /// <summary>Number of bytes for a pixel in the decoded data</summary>
        protected abstract byte PixelDepthBytes { get; }

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

        public override int Stride => DeflatedStrideBytes;
        private int BytesPerStride => WidthBlocks * CompressedBytesPerBlock;
        private int WidthBlocks => CalcBlocks((int) Header.Width);
        private int HeightBlocks => CalcBlocks((int) Header.Height);
        private int StridePixels => WidthBlocks * DivSize;
        private int DeflatedStrideBytes => StridePixels * PixelDepthBytes;
        private int CalcBlocks(int pixels) => Math.Max(1, (pixels + 3) / 4);

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

            var stride = DeflatedStrideBytes;
            var stridePixels = StridePixels;
            var heightBlocks = HeightBlocks;
            var len = heightBlocks * DivSize * stride;
            DataLen = len;
            byte[] data = config.Allocator.Rent(len);
            int pixelsLeft = len;
            int dataIndex = 0;

            int bufferSize;
            int divSize = DivSize;

            int bytesPerStride = BytesPerStride;
            int blocksPerStride = WidthBlocks;

            byte[] streamBuffer = config.Allocator.Rent(config.BufferSize);
            try
            {
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
                            bufferSize = workingSize = Util.Translate(stream, streamBuffer, config.BufferSize, bIndex);
                            bIndex = 0;
                        }

                        var origDataIndex = dataIndex;

                        // Now that we have enough pixels to fill a stride (and
                        // this includes the normally 4 pixels below the stride)
                        for (uint i = 0; i < blocksPerStride; i++)
                        {
                            bIndex = Decode(streamBuffer, data, bIndex, (uint)dataIndex, (uint)stridePixels);

                            // Advance to the next block, which is (pixel depth *
                            // divSize) bytes away
                            dataIndex += divSize * PixelDepthBytes;
                        }

                        // Each decoded block is divSize by divSize so pixels left
                        // is Width * multiplied by block height
                        workingSize -= bytesPerStride;

                        var filled = stride * divSize;
                        pixelsLeft -= filled;

                        // Jump down to the block that is exactly (divSize - 1)
                        // below the current row we are on
                        dataIndex = origDataIndex + filled;
                    }
                } while (bufferSize != 0 && pixelsLeft > 0);

                return data;
            }
            finally
            {
                config.Allocator.Return(streamBuffer);
            }
        }

        private byte[] InMemoryDecode(byte[] memBuffer, int bIndex)
        {
            var stride = DeflatedStrideBytes;
            var stridePixels = StridePixels;
            var heightBlocks = HeightBlocks;
            var len = heightBlocks * DivSize * stride;
            DataLen = len;
            byte[] data = Config.Allocator.Rent(len);
            var pixelsLeft = len;
            int dataIndex = 0;
            int divSize = DivSize;
            int blocksPerStride = WidthBlocks;

            // Same implementation as the stream based decoding, just a little bit
            // more straightforward.
            while (pixelsLeft > 0)
            {
                var origDataIndex = dataIndex;

                for (uint i = 0; i < blocksPerStride; i++)
                {
                    bIndex = Decode(memBuffer, data, bIndex, (uint)dataIndex, (uint)stridePixels);
                    dataIndex += divSize * PixelDepthBytes;
                }

                var filled = stride * divSize;
                pixelsLeft -= filled;

                // Jump down to the block that is exactly (divSize - 1)
                // below the current row we are on
                dataIndex = origDataIndex + filled;
            }

            return data;
        }

        protected override void Decode(Stream stream, PfimConfig config)
        {
            if (config.Decompress)
            {
                Data = DataDecode(stream, config);
            }
            else
            {
                var heightBlockAligned = HeightBlocks;
                long totalSize = WidthBlocks * CompressedBytesPerBlock * heightBlockAligned;

                var width = (int) Header.Width;
                var height = (int) Header.Height;
                for (int i = 1; i < Header.MipMapCount; i++)
                {
                    width = (int)Math.Pow(2, Math.Floor(Math.Log(width - 1, 2)));
                    height = (int)Math.Pow(2, Math.Floor(Math.Log(height - 1, 2)));
                    var widthBlocks = Math.Max(DivSize, width) / DivSize;
                    var heightBlocks = Math.Max(DivSize, height) / DivSize;
                    totalSize += widthBlocks * heightBlocks * CompressedBytesPerBlock;
                }

                DataLen = (int)totalSize;
                Data = config.Allocator.Rent((int)totalSize);
                _compressed = true;
                Util.Fill(stream, Data, DataLen, config.BufferSize);
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
