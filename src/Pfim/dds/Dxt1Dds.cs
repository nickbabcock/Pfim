namespace Pfim
{
    public class Dxt1Dds : CompressedDds
    {
        private const int PIXEL_DEPTH = 4;
        private const int DIV_SIZE = 4;

        public Dxt1Dds(DdsHeader header, PfimConfig config) : base(header, config)
        {
        }

        protected override byte PixelDepthBytes => PIXEL_DEPTH;
        protected override byte DivSize => DIV_SIZE;
        protected override byte CompressedBytesPerBlock => 8;
        public override ImageFormat Format => ImageFormat.Rgba32;
        public override int BitsPerPixel => 8 * PIXEL_DEPTH;

        protected override int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint stride)
        {
            // Colors are stored in a pair of 16 bits
            ushort color0 = stream[streamIndex++];
            color0 |= (ushort)(stream[streamIndex++] << 8);

            ushort color1 = (stream[streamIndex++]);
            color1 |= (ushort)(stream[streamIndex++] << 8);

            // Extract R5G6B5
            var c0 = ColorFloatRgb.FromRgb565(color0);
            var c1 = ColorFloatRgb.FromRgb565(color1);

            // Used the two extracted colors to create two new colors that are
            // slightly different.
            (var i0, var i1) = (c0.As8BitA(), c1.As8BitA());
            Color8888[] colors = color0 > color1 ?
                new[] { i0, i1, c0.Lerp(c1, 1f / 3).As8BitA(), c0.Lerp(c1, 2f / 3).As8BitA() } :
                new[] { i0, i1, c0.Lerp(c1, 0.5f).As8BitA(), default };

            for (int i = 0; i < 4; i++)
            {
                // Every 2 bit is a code [0-3] and represent what color the
                // current pixel is

                // Read in a byte and thus 4 colors
                byte rowVal = stream[streamIndex++];
                for (int j = 0; j < 8; j += 2)
                {
                    // Extract code by shifting the row byte so that we can
                    // AND it with 3 and get a value [0-3]
                    var col = colors[(rowVal >> j) & 0x03];
                    data[dataIndex++] = col.r;
                    data[dataIndex++] = col.g;
                    data[dataIndex++] = col.b;
                    data[dataIndex++] = col.a;
                }

                // Jump down a row and start at the beginning of the row
                dataIndex += PIXEL_DEPTH * (stride - DIV_SIZE);
            }

            // Reset position to start of block
            return streamIndex;
        }
    }
}
