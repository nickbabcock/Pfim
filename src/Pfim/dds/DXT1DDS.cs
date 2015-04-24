using System.IO;

namespace Pfim
{
    class Dxt1Dds : CompressedDds
    {
        private static DdsLoadInfo DXT1LoadInfo = new DdsLoadInfo(true, false, false, 4, 8/*, PixelFormat.Format24bppRgb*/);
        public Dxt1Dds(Stream fsStream, DdsHeader header)
            : base(fsStream, header, DXT1LoadInfo)
        {
            
        }

        protected override int Decompress(byte[] fileBuffer, byte[] rgbarr, int bIndex, uint rgbIndex)
        {
            // Colors are stored in a pair of 16 bits
            ushort color0 = fileBuffer[bIndex++];
            color0 |= (ushort)(fileBuffer[bIndex++] << 8);

            ushort color1 = (fileBuffer[bIndex++]);
            color1 |= (ushort)(fileBuffer[bIndex++] << 8);

            // Extract R5G6B5 (in that order)
            byte r0 = (byte)((color0 & 0x1f));
            byte g0 = (byte)((color0 & 0x7E0) >> 5);
            byte b0 = (byte)((color0 & 0xF800) >> 11);
            r0 = (byte)(r0 << 3 | r0 >> 2);
            g0 = (byte)(g0 << 2 | g0 >> 3);
            b0 = (byte)(b0 << 3 | b0 >> 2);

            byte r1 = (byte)((color1 & 0x1f));
            byte g1 = (byte)((color1 & 0x7E0) >> 5);
            byte b1 = (byte)((color1 & 0xF800) >> 11);
            r1 = (byte)(r1 << 3 | r1 >> 2);
            g1 = (byte)(g1 << 2 | g1 >> 3);
            b1 = (byte)(b1 << 3 | b1 >> 2);

            // Used the two extracted colors to create two new colors that are
            // slightly different.
            byte r2;
            byte g2;
            byte b2;

            byte r3 = 0;
            byte g3 = 0;
            byte b3 = 0;

            if (color0 > color1)
            {
                r2 = (byte)((2 * r0 + r1) / 3);
                g2 = (byte)((2 * g0 + g1) / 3);
                b2 = (byte)((2 * b0 + b1) / 3);

                r3 = (byte)((r0 + 2 * r1) / 3);
                g3 = (byte)((g0 + 2 * g1) / 3);
                b3 = (byte)((b0 + 2 * b1) / 3);
            }
            else
            {
                r2 = (byte)((r0 + r1) / 2);
                g2 = (byte)((g0 + g1) / 2);
                b2 = (byte)((b0 + b1) / 2);
            }


            byte rowVal = 0;
            for (int i = 0; i < 4; i++)
            {
                // Every 2 bit is a code [0-3] and represent what color the
                // current pixel is

                // Read in a byte and thus 4 colors
                rowVal = fileBuffer[bIndex++];
                for (int j = 0; j < 8; j += 2)
                {
                    // Extract code by shifting the row byte so that we can
                    // AND it with 3 and get a value [0-3]
                    switch ((rowVal >> j) & 0x03)
                    {
                        case 0:
                            rgbarr[rgbIndex++] = r0;
                            rgbarr[rgbIndex++] = g0;
                            rgbarr[rgbIndex++] = b0;
                            break;
                        case 1:
                            rgbarr[rgbIndex++] = r1;
                            rgbarr[rgbIndex++] = g1;
                            rgbarr[rgbIndex++] = b1;
                            break;
                        case 2:
                            rgbarr[rgbIndex++] = r2;
                            rgbarr[rgbIndex++] = g2;
                            rgbarr[rgbIndex++] = b2;
                            break;
                        case 3:
                            rgbarr[rgbIndex++] = r3;
                            rgbarr[rgbIndex++] = g3;
                            rgbarr[rgbIndex++] = b3;
                            break;
                    }
                }

                // Jump down a row and start at the beginning of the row
                rgbIndex += Header.Width * 3 - 12;
            }

            // Reset position to start of block
            return bIndex;
        }
        protected override byte PixelDepth
        {
            get { return 3; }
        }
    }
}
