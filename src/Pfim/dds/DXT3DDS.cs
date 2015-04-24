using System.IO;

namespace Pfim
{
    class DXT3DDS : CompressedDDS
    {
        const byte PIXEL_DEPTH = 4;
        const byte DIV_SIZE = 4;
        private static DDSLoadInfo loadInfoDXT3 = new DDSLoadInfo(true, false, false, 4, 16/*, PixelFormat.Format32bppArgb*/);
        public DXT3DDS(FileStream fsStream, DDSHeader header)
            : base(fsStream, header, loadInfoDXT3)
        {
        }

        protected override int Decompress(byte[] fileBuffer, byte[] rgbarr, int bIndex, uint rgbIndex)
        {
            /* 
             * Strategy for decompression:
             * -We're going to decode both alpha and color at the same time 
             * to save on space and time as we don't have to allocate an array 
             * to store values for later use.
             */

            //Remember where the alpha data is stored so we can decode simultaneously
            int alphaPtr = bIndex;

            //Jump ahead to the color data
            bIndex += 8;

            /* Colors are stored in a pair of 16 bits */
            ushort color0 = fileBuffer[bIndex++];
            color0 |= (ushort)(fileBuffer[bIndex++] << 8);

            ushort color1 = (fileBuffer[bIndex++]);
            color1 |= (ushort)(fileBuffer[bIndex++] << 8);

            //Extract R5G6B5 (in that order)
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

            //Used the two extracted colors to create two new colors
            //that are slightly different.
            byte r2 = (byte)((2 * r0 + r1) / 3);
            byte g2 = (byte)((2 * g0 + g1) / 3);
            byte b2 = (byte)((2 * b0 + b1) / 3);

            byte r3 = (byte)((r0 + 2 * r1) / 3);
            byte g3 = (byte)((g0 + 2 * g1) / 3);
            byte b3 = (byte)((b0 + 2 * b1) / 3);

            byte rowVal = 0;
            ushort rowAlpha;
            for (int i = 0; i < 4; i++)
            {
                rowVal = fileBuffer[bIndex++];

                //Each row of rgb values have 4 alpha values that 
                //are encoded in 4 bits
                rowAlpha = fileBuffer[alphaPtr++];
                rowAlpha |= (ushort)(fileBuffer[alphaPtr++] << 8);

                for (int j = 0; j < 8; j += 2)
                {
                    byte currentAlpha = (byte)((rowAlpha >> (j * 2)) & 0x0f);
                    currentAlpha |= (byte)(currentAlpha << 4);

                    //index code
                    switch (((rowVal >> j) & 0x03))
                    {
                        case 0:
                            rgbarr[rgbIndex++] = r0;
                            rgbarr[rgbIndex++] = g0;
                            rgbarr[rgbIndex++] = b0;
                            rgbarr[rgbIndex++] = currentAlpha;
                            break;
                        case 1:
                            rgbarr[rgbIndex++] = r1;
                            rgbarr[rgbIndex++] = g1;
                            rgbarr[rgbIndex++] = b1;
                            rgbarr[rgbIndex++] = currentAlpha;
                            break;
                        case 2:
                            rgbarr[rgbIndex++] = r2;
                            rgbarr[rgbIndex++] = g2;
                            rgbarr[rgbIndex++] = b2;
                            rgbarr[rgbIndex++] = currentAlpha;
                            break;
                        case 3:
                            rgbarr[rgbIndex++] = r3;
                            rgbarr[rgbIndex++] = g3;
                            rgbarr[rgbIndex++] = b3;
                            rgbarr[rgbIndex++] = currentAlpha;
                            break;
                    }
                }
                rgbIndex += PIXEL_DEPTH * (Header.Width - DIV_SIZE);
            }
            return bIndex;
        }
        protected override byte PixelDepth
        {
            get { return PIXEL_DEPTH; }
        }
    }
}
