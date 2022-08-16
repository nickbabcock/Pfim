namespace Pfim
{
    public class Dxt3Dds : CompressedDds
    {
        private const byte PIXEL_DEPTH = 4;
        private const byte DIV_SIZE = 4;

        protected override byte DivSize => DIV_SIZE;
        protected override byte CompressedBytesPerBlock => 16;
        protected override byte PixelDepthBytes => PIXEL_DEPTH;
        public override int BitsPerPixel => PIXEL_DEPTH * 8;
        public override ImageFormat Format => ImageFormat.Rgba32;

        public Dxt3Dds(DdsHeader header, PfimConfig config) : base(header, config)
        {
        }

        protected override int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint stride)
        {
            /*
             * Strategy for decompression:
             * -We're going to decode both alpha and color at the same time
             * to save on space and time as we don't have to allocate an array
             * to store values for later use.
             */

            // Remember where the alpha data is stored so we can decode simultaneously
            int alphaPtr = streamIndex;

            // Jump ahead to the color data
            streamIndex += 8;

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

            for (int i = 0; i < 4; i++)
            {
                byte rowVal = stream[streamIndex++];

                // Each row of rgb values have 4 alpha values that  are
                // encoded in 4 bits
                ushort rowAlpha = stream[alphaPtr++];
                rowAlpha |= (ushort)(stream[alphaPtr++] << 8);

                for (int j = 0; j < 8; j += 2)
                {
                    byte currentAlpha = (byte)((rowAlpha >> (j * 2)) & 0x0f);
                    currentAlpha |= (byte)(currentAlpha << 4);
                    var col = colors[((rowVal >> j) & 0x03)];
                    data[dataIndex++] = col.r;
                    data[dataIndex++] = col.g;
                    data[dataIndex++] = col.b;
                    data[dataIndex++] = currentAlpha;
                }
                dataIndex += PIXEL_DEPTH * (stride - DIV_SIZE);
            }
            return streamIndex;
        }
    }
}
