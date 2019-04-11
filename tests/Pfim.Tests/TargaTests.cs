using System;
using Xunit;
using System.IO;
using System.Text;

namespace Pfim.Tests
{
    public class TargaTests
    {
        [Fact]
        public void ParseTargaTrue24SingleColor()
        {
            byte[] expected = new byte[64 * 64 * 3];
            for (int i = 0; i < expected.Length; i += 3)
            {
                expected[i] = 255;
                expected[i + 1] = 176;
                expected[i + 2] = 0;
            }

            var image = Pfim.FromFile(Path.Combine("data", "true-24.tga"));
            Assert.Equal(expected, image.Data);
        }

        [Fact]
        public void ParseTargaTrue32SingleColor()
        {
            byte[] expected = new byte[64 * 64 * 4];
            for (int i = 0; i < expected.Length; i += 4)
            {
                expected[i] = 0;
                expected[i + 1] = 0;
                expected[i + 2] = 127;
                expected[i + 3] = 255;
            }

            var image = Pfim.FromFile(Path.Combine("data", "true-32.tga"));
            Assert.Equal(expected, image.Data);
        }

        [Fact]
        public void ParseTarga32SingleSmallRunLength()
        {
            byte[] data = new byte[8];
            byte[] stream = {129, 2, 4, 6, 8};
            CompressedTarga.RunLength(data, stream, 0, 0, 4);
            Assert.Equal(new byte[] {2, 4, 6, 8, 2, 4, 6, 8}, data);
        }

        [Fact]
        public void ParseTarga24SingleSmallRunLength()
        {
            byte[] data = new byte[6];
            byte[] stream = {129, 2, 4, 6};
            CompressedTarga.RunLength(data, stream, 0, 0, 3);
            Assert.Equal(new byte[] {2, 4, 6, 2, 4, 6}, data);
        }

        [Fact]
        public void ParseTarga24RunLength()
        {
            byte[] data = new byte[18];
            byte[] stream = {132, 2, 4, 6, 128, 8, 10, 12};
            CompressedTarga.RunLength(data, stream, 0, 0, 3);
            Assert.Equal(new byte[] {2, 4, 6, 2, 4, 6, 2, 4, 6, 2, 4, 6, 2, 4, 6, 0, 0, 0}, data);
        }

        [Fact]
        public void ParseTarga32RunLength()
        {
            byte[] data = new byte[64 * 4];
            int i = 0;
            for (; i < 32 * 4; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 216;
                data[i + 2] = 255;
                data[i + 3] = 255;
            }
            for (; i < 32 * 4 + 16 * 4; i += 4)
            {
                data[i] = 255;
                data[i + 1] = 148;
                data[i + 2] = 0;
                data[i + 3] = 255;
            }
            for (; i < 32 * 4 + 16 * 4 + 8 * 4; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 255;
                data[i + 2] = 76;
                data[i + 3] = 255;
            }
            for (; i < 32 * 4 + 16 * 4 + 8 * 4 + 8 * 4; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 255;
                data[i + 3] = 255;
            }

            var image = Pfim.FromFile(Path.Combine("data", "true-32-rle.tga"));

            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void ParseTarga24TrueRunLength()
        {
            byte[] data = new byte[64 * 3];
            int i = 0;
            for (; i < 32 * 3; i += 3)
            {
                data[i] = 0;
                data[i + 1] = 216;
                data[i + 2] = 255;
            }
            for (; i < 32 * 3 + 16 * 3; i += 3)
            {
                data[i] = 255;
                data[i + 1] = 148;
                data[i + 2] = 0;
            }
            for (; i < 32 * 3 + 16 * 3 + 8 * 3; i += 3)
            {
                data[i] = 0;
                data[i + 1] = 255;
                data[i + 2] = 76;
            }
            for (; i < 32 * 3 + 16 * 3 + 8 * 3 + 8 * 3; i += 3)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 255;
            }

            var image = Pfim.FromFile(Path.Combine("data", "true-24-rle.tga"));

            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void ParseUncompressedNonSquareTga()
        {
            var image = Pfim.FromFile(Path.Combine("data", "tiny-rect.tga"));
            byte[] data = new byte[12 * 20 * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 216;
                data[i + 2] = 255;
                data[i + 3] = 255;
            }
            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void ParseTrueTarga32MixedEncoding()
        {
            var image = Pfim.FromFile(Path.Combine("data", "true-32-mixed.tga"));
            byte[] data = new byte[256];
            for (int i = 0; i < 16 * 4; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 216;
                data[i + 2] = 255;
                data[i + 3] = 255;
            }
            Array.Copy(new byte[] {0, 0, 0, 255}, 0, data, 64, 4);
            Array.Copy(new byte[] {64, 64, 64, 255}, 0, data, 68, 4);
            Array.Copy(new byte[] {0, 0, 255, 255}, 0, data, 72, 4);
            Array.Copy(new byte[] {0, 106, 255, 255}, 0, data, 76, 4);
            Array.Copy(new byte[] {0, 216, 255, 255}, 0, data, 80, 4);
            Array.Copy(new byte[] {0, 255, 182, 255}, 0, data, 84, 4);
            Array.Copy(new byte[] {0, 255, 76, 255}, 0, data, 88, 4);
            Array.Copy(new byte[] {33, 255, 0, 255}, 0, data, 92, 4);
            Array.Copy(new byte[] {144, 255, 0, 255}, 0, data, 96, 4);
            Array.Copy(new byte[] {255, 255, 0, 255}, 0, data, 100, 4);
            Array.Copy(new byte[] {255, 148, 0, 255}, 0, data, 104, 4);
            Array.Copy(new byte[] {255, 38, 0, 255}, 0, data, 108, 4);
            Array.Copy(new byte[] {255, 0, 72, 255}, 0, data, 112, 4);
            Array.Copy(new byte[] {255, 0, 178, 255}, 0, data, 116, 4);
            Array.Copy(new byte[] {220, 0, 255, 255}, 0, data, 120, 4);
            Array.Copy(new byte[] {110, 0, 255, 255}, 0, data, 124, 4);
            for (int i = 128; i < 192; i += 4)
            {
                data[i] = 255;
                data[i + 1] = 148;
                data[i + 2] = 0;
                data[i + 3] = 255;
            }
            for (int i = 192; i < 224; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 255;
                data[i + 2] = 76;
                data[i + 3] = 255;
            }
            for (int i = 224; i < 256; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 255;
                data[i + 3] = 255;
            }
            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void ParseLarge32TargetImage()
        {
            var image = Pfim.FromFile(Path.Combine("data", "true-32-rle-large.tga"));
            byte[] data = new byte[1200 * 1200 * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 51;
                data[i + 2] = 127;
                data[i + 3] = 255;
            }
            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void ParseTargaTopLeft()
        {
            bool seenBlue = false;
            var image = Pfim.FromFile(Path.Combine("data", "rgb24_top_left.tga"));
            for (int i = 0; i < image.Data.Length; i += 3)
            {
                seenBlue |= image.Data[i] == 12 && image.Data[i + 1] == 0 && image.Data[i + 2] == 255;
                if (image.Data[i] == 255 && image.Data[i + 1] == 4 && image.Data[i + 2] == 4 && !seenBlue)
                {
                    Assert.True(false, "Expected to see blue before red (this could mean that the color channels are swapped)");
                }

                if (!((image.Data[i] == 0 && image.Data[i + 1] == 255 && image.Data[i + 2] == 0) ||
                      (image.Data[i] == 255 && image.Data[i + 1] == 0 && image.Data[i + 2] == 12) ||
                      (image.Data[i] == 255 && image.Data[i + 1] == 255 && image.Data[i + 2] == 255) ||
                      (image.Data[i + 2] == 255 && image.Data[i + 1] == image.Data[i])))
                {
                    Assert.True(false, $"Did not expect pixel {image.Data[i]} {image.Data[i + 1]} {image.Data[i + 2]}");
                }
            }
        }

        [Fact]
        public void ParseTargaTopLeftColorMap()
        {
            var image = Pfim.FromFile(Path.Combine("data", "rgb24_top_left_colormap.tga"));
            Assert.Equal(8, image.BitsPerPixel);
            Assert.Equal(4096, image.Data.Length);
            Assert.NotEqual(ImageFormat.Rgb24, image.Format);
            image.ApplyColorMap();
            Assert.Equal(ImageFormat.Rgb24, image.Format);
            Assert.Equal(255, image.Data[0]);
            Assert.Equal(255, image.Data[1]);
            Assert.Equal(255, image.Data[2]);
            Assert.Equal(255, image.Data[3]);
            Assert.Equal(255, image.Data[4]);
            Assert.Equal(255, image.Data[5]);
        }

        [Fact]
        public void ParseTargaTopLeftRleStride()
        {
            var data = File.ReadAllBytes(Path.Combine("data", "DSCN1910_24bpp_uncompressed_10_2.tga"));
            var image = Targa.Create(data, new PfimConfig());
            Assert.Equal(461, image.Width);
        }

        [Fact]
        public void ParseTargaTopLeftStride()
        {
            var data = File.ReadAllBytes(Path.Combine("data", "DSCN1910_24bpp_uncompressed_10_3.tga"));
            var image = Targa.Create(data, new PfimConfig());
            Assert.Equal(461, image.Width);
            Assert.True(image.Data[460 * 3] != 0 && image.Data[461 * 3 + 1] != 0 && image.Data[461 * 3 + 2] != 0);
        }

        [Fact]
        public void ParseLargeTargaTopLeft()
        {
            var image = Pfim.FromFile(Path.Combine("data", "large-top-left.tga"));
            foreach (byte bt in image.Data)
            {
                Assert.Equal(0, bt);
            }
        }

        [Fact]
        public void ParseLargeTargaBottomLeft()
        {
            var image = Pfim.FromFile(Path.Combine("data", "marbles.tga"));
            Assert.Equal(4264260, image.Data.Length);
            Assert.Equal(0, image.Data[0]);
            Assert.Equal(0, image.Data[1]);
            Assert.Equal(0, image.Data[2]);
        }

        [Fact]
        public void ParseMarblesTarga()
        {
            var image = Pfim.FromFile(Path.Combine("data", "marbles2.tga"));
            Assert.Equal(100 * 71 * 3, image.Data.Length);
            Assert.Equal(2, image.Data[0]);
            Assert.Equal(3, image.Data[1]);
            Assert.Equal(3, image.Data[2]);
        }

        [Fact]
        public void ParseTransparentTarga()
        {
            var image = Pfim.FromFile(Path.Combine("data", "flag_t32.tga"));
            for (int i = 0; i < image.Data.Length; i += 4)
            {
                Assert.Equal(0, image.Data[i + 3]);
            }
        }

        [Fact]
        public void InvalidTargaException()
        {
            var data = Encoding.ASCII.GetBytes("Hello world! A wonderful evening");
            var ex = Assert.ThrowsAny<Exception>(() => Pfim.FromStream(new MemoryStream(data)));
            Assert.Equal("Detected invalid targa image", ex.Message);
        }
    }
}
