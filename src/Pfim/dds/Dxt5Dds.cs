namespace Pfim
{
    internal class Dxt5Dds : CompressedDds
    {
        private static DdsLoadInfo loadInfoDXT5 = new DdsLoadInfo(true, false, false, 4, 16, 32);
        const byte PIXEL_DEPTH = 4;
        const byte DIV_SIZE = 4;

        private byte[] alpha = new byte[8];

        private int extractAlpha(byte[] workingFilePtr, int bIndex)
        {
            byte alpha0;
            byte alpha1;
            alpha[0] = alpha0 = workingFilePtr[bIndex++];
            alpha[1] = alpha1 = workingFilePtr[bIndex++];

            if (alpha0 > alpha1)
            {
                for (int i = 1; i < 7; i++)
                    alpha[1 + i] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
            }
            else
            {
                for (int i = 1; i < 5; ++i)
                    alpha[1 + i] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);
                alpha[6] = 0;
                alpha[7] = 255;
            }
            return bIndex;
        }

        protected override byte PixelDepth => PIXEL_DEPTH;

        protected override int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint width)
        {
            streamIndex = extractAlpha(stream, streamIndex);

            ulong alphaCodes = stream[streamIndex++];
            alphaCodes |= ((ulong)stream[streamIndex++] << 8);
            alphaCodes |= ((ulong)stream[streamIndex++] << 16);
            alphaCodes |= ((ulong)stream[streamIndex++] << 24);
            alphaCodes |= ((ulong)stream[streamIndex++] << 32);
            alphaCodes |= ((ulong)stream[streamIndex++] << 40);

            // Colors are stored in a pair of 16 bits
            ushort color0 = stream[streamIndex++];
            color0 |= (ushort)(stream[streamIndex++] << 8);

            ushort color1 = (stream[streamIndex++]);
            color1 |= (ushort)(stream[streamIndex++] << 8);

            var colors = new Color888[4];

            // Extract R5G6B5 (in that order)
            colors[0].r = (byte)((color0 & 0x1f));
            colors[0].g = (byte)((color0 & 0x7E0) >> 5);
            colors[0].b = (byte)((color0 & 0xF800) >> 11);
            colors[0].r = (byte)(colors[0].r << 3 | colors[0].r >> 2);
            colors[0].g = (byte)(colors[0].g << 2 | colors[0].g >> 3);
            colors[0].b = (byte)(colors[0].b << 3 | colors[0].b >> 2);

            colors[1].r = (byte)((color1 & 0x1f));
            colors[1].g = (byte)((color1 & 0x7E0) >> 5);
            colors[1].b = (byte)((color1 & 0xF800) >> 11);
            colors[1].r = (byte)(colors[1].r << 3 | colors[1].r >> 2);
            colors[1].g = (byte)(colors[1].g << 2 | colors[1].g >> 3);
            colors[1].b = (byte)(colors[1].b << 3 | colors[1].b >> 2);

            colors[2].r = (byte)((2 * colors[0].r + colors[1].r) / 3);
            colors[2].g = (byte)((2 * colors[0].g + colors[1].g) / 3);
            colors[2].b = (byte)((2 * colors[0].b + colors[1].b) / 3);

            colors[3].r = (byte)((colors[0].r + 2 * colors[1].r) / 3);
            colors[3].g = (byte)((colors[0].g + 2 * colors[1].g) / 3);
            colors[3].b = (byte)((colors[0].b + 2 * colors[1].b) / 3);

            byte rowVal = 0;
            for (int alphaShift = 0; alphaShift < 48; alphaShift += 12)
            {
                rowVal = stream[streamIndex++];

                for (int j = 0; j < 4; j++)
                {
                    // 3 bits determine alpha index to use
                    byte alphaIndex = (byte)((alphaCodes >> (alphaShift + 3 * j)) & 0x07);
                    var col = colors[((rowVal >> (j * 2)) & 0x03)];
                    data[dataIndex++] = col.r;
                    data[dataIndex++] = col.g;
                    data[dataIndex++] = col.b;
                    data[dataIndex++] = alpha[alphaIndex];
                }
                dataIndex += PIXEL_DEPTH * (width - DIV_SIZE);
            }
            return streamIndex;
        }

        public override DdsLoadInfo ImageInfo(DdsHeader header)
        {
            return loadInfoDXT5;
        }
    }
}
