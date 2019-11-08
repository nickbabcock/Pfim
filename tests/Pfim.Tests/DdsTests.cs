using Pfim.dds;
using System.IO;
using Xunit;

namespace Pfim.Tests
{
    public class DdsTests
    {
        [Fact]
        public void Parse32BitUncompressedDds()
        {
            var image = Pfim.FromFile(Path.Combine("data", "32-bit-uncompressed.dds"));
            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 127;
                data[i + 3] = 255;
            }

            Assert.Equal(data, image.Data);
            Assert.Equal(64, image.Height);
            Assert.Equal(64, image.Width);
        }

        [Fact]
        public void ParseSimpleDxt1()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dxt1-simple.dds"));
            byte[] data = new byte[64 * 64 * 3];
            for (int i = 0; i < data.Length; i += 3)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 127;
            }

            Assert.Equal(data, image.Data);
            Assert.Equal(64, image.Height);
            Assert.Equal(64, image.Width);
        }

        [Fact]
        public void ParseSimpleDxt3()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dxt3-simple.dds"));
            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 128;
                data[i + 3] = 255;
            }

            Assert.Equal(data, image.Data);
            Assert.Equal(64, image.Height);
            Assert.Equal(64, image.Width);
        }

        [Fact]
        public void ParseSimpleDxt5()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dxt5-simple.dds"));
            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 128;
                data[i + 3] = 255;
            }

            Assert.Equal(data, image.Data);
            Assert.Equal(64, image.Height);
            Assert.Equal(64, image.Width);
        }

        [Fact]
        public void ParseSimpleDxt5Odd()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dxt5-simple-odd.dds"));
            Assert.Equal(32, image.Stride);
            Assert.Equal(0, image.Data[8 * 5 * 4]);
            Assert.Equal(0, image.Data[8 * 5 * 4 + 1]);
            Assert.Equal(128, image.Data[8 * 5 * 4 + 2]);
            Assert.Equal(255, image.Data[8 * 5 * 4 + 3]);
            Assert.Equal(image.Data.Length, 8 * 12 * 4);
            Assert.Equal(9, image.Height);
            Assert.Equal(5, image.Width);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Assert.Equal(0, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8)]);
                    Assert.Equal(0, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 1]);
                    Assert.Equal(128, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 2]);
                    Assert.Equal(255, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 3]);
                }
            }
        }

        public void ParseSimpleBc2()
        {
            var image = Pfim.FromFile(Path.Combine("data", "bc2-simple-srgb.dds"));
            Assert.True(image is Dxt3Dds);
            Assert.Equal(DxgiFormat.BC2_UNORM_SRGB, ((Dxt3Dds)image).Header10?.DxgiFormat);

            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 255;
                data[i + 1] = 189;
                data[i + 2] = 189;
                data[i + 3] = 255;
            }

            Assert.Equal(data, image.Data);
            Assert.Equal(64, image.Height);
            Assert.Equal(64, image.Width);
        }

        [Fact]
        public void ParseSimpleBc3()
        {
            var image = Pfim.FromFile(Path.Combine("data", "bc3-simple-srgb.dds"));
            Assert.True(image is Dxt5Dds);
            Assert.Equal(DxgiFormat.BC3_UNORM_SRGB, ((Dxt5Dds)image).Header10?.DxgiFormat);

            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 255;
                data[i + 1] = 189;
                data[i + 2] = 189;
                data[i + 3] = 255;
            }

            Assert.Equal(data, image.Data);
            Assert.Equal(64, image.Height);
            Assert.Equal(64, image.Width);
        }

        [Fact]
        public void ParseSimpleBc5()
        {
            var image = Pfim.FromFile(Path.Combine("data", "bc5-simple.dds"));
            Assert.True(image is Bc5Dds);
            Assert.Equal(CompressionAlgorithm.BC5U, ((Bc5Dds)image).Header?.PixelFormat.FourCC);

            byte[] data = new byte[64 * 64 * 3];
            for (int i = 0; i < data.Length; i += 3)
            {
                data[i] = 0;
                data[i + 1] = 128;
                data[i + 2] = 128;
        }

            Assert.Equal(data, image.Data);
            Assert.Equal(64, image.Height);
            Assert.Equal(64, image.Width);
        }

        [Fact]
        public void ParseSimpleBc7()
        {
            var image = Pfim.FromFile(Path.Combine("data", "bc7-simple.dds"));
            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4)
            { 
                // Format says Rgba32 (r at least significant)
                // but it is Bgra32 (b at least significant)
                // this mistake seems to be the case with all
                // decoders, so I followed suit
                data[i] = 255; // b
                data[i + 1] = 128; // g
                data[i + 2] = 129; // r
                data[i + 3] = 255; // a
            }

            Assert.Equal(data, image.Data);
            Assert.Equal(64, image.Height);
            Assert.Equal(64, image.Width);
        }

        [Fact]
        public void ParseSimpleUncompressedOdd()
        {
            var image = Pfim.FromFile(Path.Combine("data", "32-bit-uncompressed-odd.dds"));
            Assert.Equal(20, image.Stride);
            Assert.Equal(image.Data.Length, 5 * 9 * 4);
            Assert.Equal(9, image.Height);
            Assert.Equal(5, image.Width);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Assert.Equal(0, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8)]);
                    Assert.Equal(0, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 1]);
                    Assert.Equal(128, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 2]);
                    Assert.Equal(255, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 3]);
                }
            }
        }

        [Fact]
        public void Parse24bitUncompressedOdd()
        {
            var image = Pfim.FromFile(Path.Combine("data", "24-bit-uncompressed-odd.dds"));
            Assert.Equal(4, image.Stride);
            Assert.Equal(12, image.Data.Length);
            Assert.Equal(3, image.Height);
            Assert.Equal(1, image.Width);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    Assert.Equal(0, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 0]);
                    Assert.Equal(0, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 1]);
                    Assert.Equal(128, image.Data[i * image.Stride + (j * image.BitsPerPixel / 8) + 2]);
                }
            }
        }

        [Fact]
        public void ParseSimpleDxt51x1()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dxt5-simple-1x1.dds"));
            Assert.Equal(16, image.Stride);
            Assert.Equal(0, image.Data[0]);
            Assert.Equal(0, image.Data[1]);
            Assert.Equal(128, image.Data[2]);
            Assert.Equal(255, image.Data[3]);
            Assert.Equal(64, image.Data.Length);
            Assert.Equal(1, image.Height);
            Assert.Equal(1, image.Width);
        }

        [Fact]
        public void ParseWoseBc1Snorm()
        {
            var image = Pfim.FromFile(Path.Combine("data", "wose_BC1_UNORM_SRGB.DDS"));
            Assert.IsAssignableFrom<Dds>(image);
            var dds = (Dds)image;
            Assert.Equal(DxgiFormat.BC1_UNORM_SRGB, dds.Header10?.DxgiFormat);
        }

        [Theory]
        [InlineData("dxt1-simple.dds")]
        [InlineData("dxt3-simple.dds")]
        [InlineData("dxt5-simple.dds")]
        [InlineData("dxt5-simple-odd.dds")]
        [InlineData("dxt5-simple-1x1.dds")]
        [InlineData("Antenna_Metal_0_Normal.dds")]
        [InlineData("wose_BC1_UNORM.DDS")]
        public void TestDdsCompression(string path)
        {
            var data = File.ReadAllBytes(Path.Combine("data", path));
            var image = Dds.Create(data, new PfimConfig());
            var image2 = Dds.Create(data, new PfimConfig(decompress: false));

            Assert.False(image.Compressed);
            Assert.True(image2.Compressed);
            Assert.NotEqual(image.Data, image2.Data);
            Assert.Equal(image.Format, image2.Format);
            image2.Decompress();
            Assert.Equal(image.Data, image2.Data);
        }

        [Fact]
        public void TestDdsMipMap1()
        {
            var image = Pfim.FromFile(Path.Combine("data", "wose_BC1_UNORM.DDS"));
            var expectedMips = new[]
            {
                new MipMapOffset(36, 36, 108, 15552, 3888),
                new MipMapOffset(18, 18, 60, 19440, 1200),
                new MipMapOffset(9, 9, 36, 20640, 432),
                new MipMapOffset(4, 4, 12, 21072, 48),
                new MipMapOffset(2, 2, 12, 21120, 48),
                new MipMapOffset(1, 1, 12, 21168, 48)
            };
            Assert.Equal(expectedMips, image.MipMaps);
            Assert.Equal(21168 + 48, image.Data.Length);

            image = Dds.Create(File.ReadAllBytes(Path.Combine("data", "wose_BC1_UNORM.DDS")), new PfimConfig());
            Assert.Equal(expectedMips, image.MipMaps);
            Assert.Equal(21168 + 48, image.Data.Length);
        }
    }
}
