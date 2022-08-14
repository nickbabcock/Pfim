namespace Pfim
{
    struct Colors888
    {
        public byte r;
        public byte g;
        public byte b;
    }

    struct Color8888
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
    }

    struct ColorFloatRgb
    {
        public float r;
        public float g;
        public float b;

        public static ColorFloatRgb FromRgb565(ushort rgb565)
        {
            const float F5 = 255f / 31f;
            const float F6 = 255f / 63f;

            return new ColorFloatRgb()
            {
                r = ((rgb565 & 0x1f)) * F5,
                g = ((rgb565 & 0x7E0) >> 5) * F6,
                b = ((rgb565 & 0xF800) >> 11) * F5,
            };
        }

        public ColorFloatRgb Lerp(ColorFloatRgb other, float blend)
        {
            return new ColorFloatRgb()
            {
                r = r + blend * (other.r - r),
                g = g + blend * (other.g - g),
                b = b + blend * (other.b - b),
            };
        }

        public void As8Bit(out Colors888 result)
        {
            result.r = (byte)(r + 0.5f);
            result.g = (byte)(g + 0.5f);
            result.b = (byte)(b + 0.5f);
        }
        public void As8Bit(out Color8888 result)
        {
            result.r = (byte)(r + 0.5f);
            result.g = (byte)(g + 0.5f);
            result.b = (byte)(b + 0.5f);
            result.a = 255;
        }
    }
}
