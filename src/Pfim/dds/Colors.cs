namespace Pfim
{
    struct Color888
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

        public ColorFloatRgb Lerp(ColorFloatRgb other, float blend) => new ColorFloatRgb()
        {
            r = r + blend * (other.r - r),
            g = g + blend * (other.g - g),
            b = b + blend * (other.b - b),
        };

        public Color888 As8Bit() => new Color888
        {
            r = (byte)(r + 0.5f),
            g = (byte)(g + 0.5f),
            b = (byte)(b + 0.5f),
        };

        public Color8888 As8BitA() => new Color8888
        {
            r = (byte)(r + 0.5f),
            g = (byte)(g + 0.5f),
            b = (byte)(b + 0.5f),
            a = 255,
        };
    }
}
