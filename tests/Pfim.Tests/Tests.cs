using System;
using Xunit;
using System.IO;

namespace Pfim.Tests
{
    public class PfimTests
    {
        [Fact]
        public void TranslateIdentity()
        {
            byte[] buf = {1, 2, 3, 4, 5};
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 0);
            Assert.Equal(5, actual);
        }

        [Fact]
        public void TranslateByteFromStream()
        {
            byte[] buf = {1, 2, 3, 4, 5};
            var mem = new MemoryStream(new byte[] {100});
            var actual = Util.Translate(mem, buf, 1);
            Assert.Equal(5, actual);
            Assert.Equal(new byte[] {2, 3, 4, 5, 100}, buf);
        }

        [Fact]
        public void TranslateByteFromStreamButNoByteToGive()
        {
            byte[] buf = {1, 2, 3, 4, 5};
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 1);
            Assert.Equal(4, actual);
            Assert.Equal(new byte[] {2, 3, 4, 5, 5}, buf);
        }

        [Fact]
        public void TranslateAllButLastByte()
        {
            byte[] buf = {1, 2, 3, 4, 5};
            var mem = new MemoryStream(new byte[] {100, 99, 98, 97});
            var actual = Util.Translate(mem, buf, 4);
            Assert.Equal(5, actual);
            Assert.Equal(new byte[] {5, 100, 99, 98, 97}, buf);
        }

        [Fact]
        public void TranslateAllButLastByteButNoByteToGive()
        {
            byte[] buf = {1, 2, 3, 4, 5};
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 4);
            Assert.Equal(1, actual);
            Assert.Equal(new byte[] {5, 2, 3, 4, 5}, buf);
        }

        [Fact]
        public void FillButtomLeftSinglePixelRows()
        {
            byte[] data = new byte[5];
            var mem = new MemoryStream(new byte[] {1, 2, 3, 4, 5});
            Util.FillBottomLeft(mem, data, 1, 1);
            Assert.Equal(new byte[] {5, 4, 3, 2, 1}, data);
        }

        [Fact]
        public void FillButtomLeftDoublePixelRows()
        {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] {1, 2, 3, 4, 5, 6});
            Util.FillBottomLeft(mem, data, 2, 2);
            Assert.Equal(new byte[] {5, 6, 3, 4, 1, 2}, data);
        }

        [Fact]
        public void FillButtomLeftAndBufferCantHoldAll()
        {
            byte[] data = new byte[5];
            var mem = new MemoryStream(new byte[] {1, 2, 3, 4, 5});
            Util.FillBottomLeft(mem, data, 1, 1);
            Assert.Equal(new byte[] {5, 4, 3, 2, 1}, data);
        }

        [Fact]
        public void FillButtomLeftDoublePixelAndBufferCantHoldAll()
        {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] {1, 2, 3, 4, 5, 6});
            Util.FillBottomLeft(mem, data, 2, 2);
            Assert.Equal(new byte[] {5, 6, 3, 4, 1, 2}, data);
        }

        [Fact]
        public void FillButtomLeftWithPadding()
        {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] {1, 2, 3, 4});
            Util.FillBottomLeft(mem, data, 2, 3);
            Assert.Equal(new byte[] {3, 4, 0, 1, 2, 0}, data);
        }

        [Fact]
        public void FourIsTheMinimumStride()
        {
            Assert.Equal(4, Util.Stride(width: 1, pixelDepth: 32));
        }

        [Fact]
        public void StrideWithPadding()
        {
            Assert.Equal(8, Util.Stride(width: 2, pixelDepth: 24));
        }

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
        public void ParseTargaCya()
        {
            var image = Pfim.FromFile(Path.Combine("data", "CYA.tga"));
            Assert.Equal(209, image.Data[image.Data.Length - 1]);
            Assert.Equal(96, image.Data[image.Data.Length - 2]);
            Assert.Equal(72, image.Data[image.Data.Length - 3]);

            Assert.Equal(209, image.Data[2]);
            Assert.Equal(98, image.Data[1]);
            Assert.Equal(76, image.Data[0]);
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
        public void ParseGrayscaleTarga()
        {
            var image = Pfim.FromFile(Path.Combine("data", "CBW8.tga"));
            Assert.Equal(ImageFormat.Rgb8, image.Format);
            Assert.Equal(76, image.Data[0]);
        }

        [Fact]
        public void Parse8BitColorMapTarga()
        {
            var image = Pfim.FromFile(Path.Combine("data", "CCM8.tga"));
            Assert.Equal(ImageFormat.Rgb8, image.Format);
        }

        [Fact]
        public void Parse16BitTarga()
        {
            var image = Pfim.FromFile(Path.Combine("data", "CTC16.tga"));
            Assert.Equal(ImageFormat.R5g5b5, image.Format);
            Assert.Equal(0, image.Data[0]);
            Assert.Equal(124, image.Data[1]);
        }

        [Fact]
        public void ParseTransparentTarga()
        {
            var image = Pfim.FromFile(Path.Combine("data", "flag_t32.tga"));
            Assert.Equal(ImageFormat.Rgba32, image.Format);
            for (int i = 0; i < image.Data.Length; i += 4)
            {
                Assert.Equal(0, image.Data[i + 3]);
            }
        }

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
            Assert.Equal(ImageFormat.Rgba32, image.Format);
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
            Assert.Equal(ImageFormat.Rgb24, image.Format);
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
            Assert.Equal(ImageFormat.Rgba32, image.Format);
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
            Assert.Equal(ImageFormat.Rgba32, image.Format);
        }

        [Fact]
        public void ParseDdsWhenHeaderStates64Bits()
        {
            var image = Pfim.FromFile(Path.Combine("data", "TestVolume_Noise3D.dds"));
            Assert.Equal(ImageFormat.Rgba32, image.Format);
        }

        [Fact]
        public void ParseDdsA8B8G8R8()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dds_A8B8G8R8.dds"));
            Assert.Equal(ImageFormat.Rgba32, image.Format);
        }

        [Fact]
        public void ParseDdsR5g6b5()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dds_R5G6B5.dds"));
            Assert.Equal(ImageFormat.R5g6b5, image.Format);
        }

        [Fact]
        public void ParseDdsR5g5b5a1()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dds_a1r5g5b5.dds"));
            Assert.Equal(ImageFormat.R5g5b5a1, image.Format);
        }

        [Fact]
        public void ParseDdsRgba16()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dds_A4R4G4B4.dds"));
            Assert.Equal(ImageFormat.Rgba16, image.Format);
        }

        [Fact]
        public void ParseDdsRgb24()
        {
            var image = Pfim.FromFile(Path.Combine("data", "dds_R8G8B8.dds"));
            Assert.Equal(ImageFormat.Rgb24, image.Format);
        }

        [Fact]
        public void ParseWoseBc1Unorm()
        {
            var image = Pfim.FromFile(Path.Combine("data", "wose_BC1_UNORM.DDS"));
            Assert.Equal(ImageFormat.Rgb24, image.Format);
        }

        [Fact]
        public void ParseWoseR8G8B8A8UnormSrgb()
        {
            var image = Pfim.FromFile(Path.Combine("data", "wose_R8G8B8A8_UNORM_SRGB.DDS"));
            Assert.Equal(ImageFormat.Rgba32, image.Format);
        }

        [Theory]
        [InlineData("dds_R8G8B8.dds")]
        [InlineData("32-bit-uncompressed.dds")]
        [InlineData("dxt1-simple.dds")]
        [InlineData("dxt3-simple.dds")]
        [InlineData("dxt5-simple.dds")]
        [InlineData("dds_a1r5g5b5.dds")]
        public void TestDdsMemoryEquivalent(string path)
        {
            var data = File.ReadAllBytes(Path.Combine("data", path));
            var image = Pfim.FromFile(Path.Combine("data", path));
            var image2 = Dds.Create(data, new PfimConfig());
            Assert.Equal(image.Format, image2.Format);
            Assert.Equal(image.Data, image2.Data);
        }

        [Theory]
        [InlineData("true-24.tga")]
        [InlineData("CYA.tga")]
        [InlineData("true-32.tga")]
        [InlineData("true-32-rle.tga")]
        [InlineData("true-32-rle-large.tga")]
        [InlineData("true-24-rle.tga")]
        [InlineData("tiny-rect.tga")]
        [InlineData("true-32-mixed.tga")]
        [InlineData("rgb24_top_left.tga")]
        [InlineData("large-top-left.tga")]
        [InlineData("marbles.tga")]
        [InlineData("marbles2.tga")]
        [InlineData("CBW8.tga")]
        [InlineData("CCM8.tga")]
        [InlineData("CTC16.tga")]
        [InlineData("flag_t32.tga")]
        public void TestTargaMemoryEquivalent(string path)
        {
            var data = File.ReadAllBytes(Path.Combine("data", path));
            var image = Pfim.FromFile(Path.Combine("data", path));
            var image2 = Targa.Create(data, new PfimConfig());
            Assert.Equal(image.Format, image2.Format);
            Assert.Equal(image.Data, image2.Data);
        }
    }
}
