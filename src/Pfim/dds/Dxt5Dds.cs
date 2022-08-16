using Pfim.dds;

namespace Pfim
{
    public class Dxt5Dds : CompressedDds
    {
        private const byte PIXEL_DEPTH = 4;
        private const byte DIV_SIZE = 4;

        private readonly byte[] alpha = new byte[8];

        public override int BitsPerPixel => 8 * PIXEL_DEPTH;
        public override ImageFormat Format => ImageFormat.Rgba32;
        protected override byte DivSize => DIV_SIZE;
        protected override byte CompressedBytesPerBlock => 16;

        public Dxt5Dds(DdsHeader header, PfimConfig config) : base(header, config)
        {
        }

        protected override byte PixelDepthBytes => PIXEL_DEPTH;

        protected override int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint stride)
        {
            streamIndex = Bc5Dds.ExtractGradient(alpha, stream, streamIndex);

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

            // Extract R5G6B5
            var c0 = ColorFloatRgb.FromRgb565(color0);
            var c1 = ColorFloatRgb.FromRgb565(color1);

            (var i0, var i1) = (c0.As8Bit(), c1.As8Bit());
            Color888[] colors = new[] { i0, i1, c0.Lerp(c1, 1f / 3).As8Bit(), c0.Lerp(c1, 2f / 3).As8Bit() };

            for (int alphaShift = 0; alphaShift < 48; alphaShift += 12)
            {
                byte rowVal = stream[streamIndex++];
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
                dataIndex += PIXEL_DEPTH * (stride - DIV_SIZE);
            }
            return streamIndex;
        }
    }
}
