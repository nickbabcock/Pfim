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

        /// <summary>Determine image info from header</summary>
        public abstract DdsLoadInfo ImageInfo(DdsHeader header);

        /// <summary>Uncompress a given block</summary>
        protected abstract int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint width);

        /// <summary>Number of bytes for a pixel in the decoded data</summary>
        protected abstract byte PixelDepth { get; }
        protected abstract byte CompressedBytesPerBlock { get; }
        public override bool Compressed => _compressed;

        /// <summary>Decode data into raw rgb format</summary>
        public byte[] DataDecode(Stream stream, PfimConfig config)
        {
            byte[] data = new byte[Header.Width * Header.Height * PixelDepth];
            DdsLoadInfo loadInfo = ImageInfo(Header);
            uint dataIndex = 0;

            int bufferSize;
            int workingSize;
            uint pixelsLeft = Header.Width * Header.Height;

            // The number of bytes that represent a stride in the image
            int bytesPerStride = (int)((Header.Width / loadInfo.DivSize) * loadInfo.BlockBytes);
            int blocksPerStride = (int)(Header.Width / loadInfo.DivSize);

            if (stream is MemoryStream s && s.TryGetBuffer(out var arr))
            {
                var memBuffer = arr.Array;
                int bIndex = (int) s.Position;
                while (pixelsLeft > 0)
                {
                    // Now that we have enough pixels to fill a stride (and
                    // this includes the normally 4 pixels below the stride)
                    for (uint i = 0; i < blocksPerStride; i++)
                    {
                        bIndex = Decode(memBuffer, data, bIndex, dataIndex, Header.Width);

                        // Advance to the next block, which is (pixel depth *
                        // divSize) bytes away
                        dataIndex += loadInfo.DivSize * PixelDepth;
                    }

                    // Each decoded block is divSize by divSize so pixels left
                    // is Width * multiplied by block height
                    pixelsLeft -= Header.Width * loadInfo.DivSize;

                    // Jump down to the block that is exactly (divSize - 1)
                    // below the current row we are on
                    dataIndex += (PixelDepth * (loadInfo.DivSize - 1) * Header.Width);
                }

                return data;
            }

            byte[] streamBuffer = new byte[config.BufferSize];

            do
            {
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
                        dataIndex += loadInfo.DivSize * PixelDepth;
                    }

                    // Each decoded block is divSize by divSize so pixels left
                    // is Width * multiplied by block height
                    pixelsLeft -= Header.Width * loadInfo.DivSize;
                    workingSize -= bytesPerStride;

                    // Jump down to the block that is exactly (divSize - 1)
                    // below the current row we are on
                    dataIndex += (PixelDepth * (loadInfo.DivSize - 1) * Header.Width);
                }
            } while (bufferSize != 0 && pixelsLeft != 0);

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
                var loadInfo = ImageInfo(Header);
                int blocksPerStride = (int)(Header.Width / loadInfo.DivSize);
                Data = new byte[blocksPerStride * CompressedBytesPerBlock * (Header.Height / loadInfo.DivSize)];
                _compressed = true;
                if (stream is MemoryStream s && s.TryGetBuffer(out var arr))
                {
                    Buffer.BlockCopy(arr.Array, (int) s.Position, Data, 0, Data.Length);
                }
                else
                {
                    Util.Fill(stream, Data, config.BufferSize);
                }
            }

        }

        public override void Decompress()
        {
            if (!_compressed)
            {
                return;
            }

            var mem = new MemoryStream(Data, 0, Data.Length, false, true);
            Data = DataDecode(mem, _config);
            _compressed = false;
        }
    }
}
