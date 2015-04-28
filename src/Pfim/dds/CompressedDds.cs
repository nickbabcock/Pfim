using System.IO;

namespace Pfim
{
    public abstract class CompressedDds : DdsBase
    {
        public CompressedDds(Stream stream, DdsHeader header, DdsLoadInfo loadinfo)
            : base(header, loadinfo)
        {
            ReadImage(stream);
        }

        public override byte[] Data { get { return rgbarr; } }
        private byte[] rgbarr;

        private void ReadImage(Stream stream)
        {
            rgbarr = new byte[Header.Width * Header.Height * PixelDepth];
            uint rgbIndex = 0;

            int bufferSize;
            int workingSize;
            uint pixelsLeft = Header.Width * Header.Height;

            // The number of bytes that represent a stride in the image
            int bytesPerStride = (int)((Header.Width / LoadInfo.divSize) * LoadInfo.blockBytes);
            int blocksPerStride = (int)(Header.Width / LoadInfo.divSize);

            byte[] fileBuffer = new byte[Util.BUFFER_SIZE];

            do
            {
                bufferSize = workingSize = stream.Read(fileBuffer, 0, Util.BUFFER_SIZE);
                int bIndex = 0;
                while (workingSize > 0 && pixelsLeft > 0)
                {
                    // If there is not enough of the buffer to fill the next
                    // set of 16 square pixels Get the next buffer
                    if (workingSize < bytesPerStride)
                    {
                        bufferSize = workingSize = Util.Translate(stream, fileBuffer, workingSize);
                        bIndex = 0;
                    }

                    // Now that we have enough pixels to fill a stride (and
                    // this includes the normally 4 pixels below the stride)
                    for (uint i = 0; i < blocksPerStride; i++)
                    {
                        bIndex = Decompress(fileBuffer, rgbarr, bIndex, rgbIndex);

                        // Advance to the next block, which is (pixel depth *
                        // divSize) bytes away
                        rgbIndex += LoadInfo.divSize * PixelDepth;
                    }

                    // Each decoded block is divSize by divSize so pixels left
                    // is Width * multiplied by block height
                    pixelsLeft -= Header.Width * LoadInfo.divSize;
                    workingSize -= bytesPerStride;

                    // Jump down to the block that is exactly (divSize - 1)
                    // below the current row we are on
                    rgbIndex += (PixelDepth * (LoadInfo.divSize - 1) * Header.Width);
                }
            } while (bufferSize != 0 && pixelsLeft != 0);
        }

        protected abstract int Decompress(byte[] fileBuffer, byte[] rgbarr, int bIndex, uint rgbIndex);
        protected abstract byte PixelDepth { get; }
    }
}
