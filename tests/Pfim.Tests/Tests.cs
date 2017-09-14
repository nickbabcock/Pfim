using System;
using System.Text;
using Xunit;
using Pfim;
using System.IO;
using System.Linq;

namespace Pfim.Tests
{
    public class HashTests
    {
        [Fact]
        public void translateIdentity() {
            byte[] buf = new byte[] { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 0);
            Assert.Equal(5, actual);
        }

        [Fact]
        public void translateByteFromStream() {
            byte[] buf = new byte[] { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream(new byte[]{ 100 });
            var actual = Util.Translate(mem, buf, 1);
            Assert.Equal(5, actual);
            Assert.Equal(new byte[] { 2, 3, 4, 5, 100 }, buf);
        }

        [Fact]
        public void translateByteFromStreamButNoByteToGive() {
            byte[] buf = new byte[] { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 1);
            Assert.Equal(4, actual);
            Assert.Equal(new byte[] { 2, 3, 4, 5, 5 }, buf);
        }

        [Fact]
        public void translateAllButLastByte() {
            byte[] buf = new byte[] { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream(new byte[] {100, 99, 98, 97});
            var actual = Util.Translate(mem, buf, 4);
            Assert.Equal(5, actual);
            Assert.Equal(new byte[] { 5, 100, 99, 98, 97 }, buf);
        }

        [Fact]
        public void translateAllButLastByteButNoByteToGive() {
            byte[] buf = new byte[] { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 4);
            Assert.Equal(1, actual);
            Assert.Equal(new byte[] { 5, 2, 3, 4, 5 }, buf);
        }

        [Fact]
        public void fillButtomLeftSinglePixelRows() {
            byte[] data = new byte[5];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4, 5});
            Util.FillBottomLeft(mem, data, 1);
            Assert.Equal(new byte[] { 5, 4, 3, 2, 1 }, data);
        }

        [Fact]
        public void fillButtomLeftDoublePixelRows() {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6});
            Util.FillBottomLeft(mem, data, 2);
            Assert.Equal(new byte[] { 5, 6, 3, 4, 1, 2 }, data);
        }

        [Fact]
        public void fillButtomLeftAndBufferCantHoldAll() {
            byte[] data = new byte[5];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4, 5});
            Util.FillBottomLeft(mem, data, 1, 2);
            Assert.Equal(new byte[] { 5, 4, 3, 2, 1 }, data);
        }

        [Fact]
        public void fillButtomLeftDoublePixelAndBufferCantHoldAll() {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6});
            Util.FillBottomLeft(mem, data, 2, 2);
            Assert.Equal(new byte[] { 5, 6, 3, 4, 1, 2 }, data);
        }

        [Fact]
        public void fillButtomLeftWithPadding() {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4});
            Util.FillBottomLeft(mem, data, 2, padding: 1);
            Assert.Equal(new byte[] { 3, 4, 0, 1, 2, 0 }, data);
        }

        [Fact]
        public void fourIsTheMinimumStride() {
            Assert.Equal(4, Util.Stride(width: 1, pixelDepth: 32));
        }

        [Fact]
        public void strideWithPadding() {
            Assert.Equal(8, Util.Stride(width: 2, pixelDepth: 24));
        }

        [Fact]
        public void parseTargaTrue24SingleColor() {
            byte[] expected = new byte[64*64*3];
            for (int i = 0; i < expected.Length; i += 3) {
                expected[i] = 255;
                expected[i + 1] = 176;
                expected[i + 2] = 0;
            }

            var image = Pfim.FromFile(Path.Combine("data", "true-24.tga"));
            Assert.Equal(expected, image.Data);
        }

        [Fact]
        public void parseTargaCya()
        {
            var image = Pfim.FromFile(Path.Combine("data", "CYA.tga"));
            Assert.Equal(209, image.Data[image.Data.Length - 1]);
            Assert.Equal(96, image.Data[image.Data.Length - 2]);
            Assert.Equal(72, image.Data[image.Data.Length - 3]);
        }

        [Fact]
        public void parseTargaTrue32SingleColor () {
            byte[] expected = new byte[64 * 64 * 4];  
            for (int i = 0; i < expected.Length; i += 4) {
                expected[i] = 0;
                expected[i + 1] = 0;
                expected[i + 2] = 127;
                expected[i + 3] = 255;
            }

            var image = Pfim.FromFile(Path.Combine("data", "true-32.tga"));
            Assert.Equal(expected, image.Data);
        }

        [Fact]
        public void parseTarga32SingleSmallRunLength () {
            byte[] data = new byte[8];
            byte[] stream = new byte[] {129, 2, 4, 6, 8};
            CompressedTarga.RunLength(data, stream, 0, 0, 4);
            Assert.Equal(new byte[] { 2, 4, 6, 8, 2, 4, 6, 8 }, data);
        }
        
        [Fact]
        public void parseTarga24SingleSmallRunLength () {
            byte[] data = new byte[6];
            byte[] stream = new byte[] {129, 2, 4, 6};
            CompressedTarga.RunLength(data, stream, 0, 0, 3);
            Assert.Equal(new byte[] { 2, 4, 6, 2, 4, 6 }, data);
        }

        [Fact]
        public void parseTarga24RunLength () {
            byte[] data = new byte[18];
            byte[] stream = new byte[] {132, 2, 4, 6, 128, 8, 10, 12};
            CompressedTarga.RunLength(data, stream, 0, 0, 3);
            Assert.Equal(new byte[] { 2, 4, 6, 2, 4, 6, 2, 4, 6, 2, 4, 6, 2, 4, 6, 0, 0, 0 }, data);
        }

        [Fact]
        public void parseTarga32RunLength () {
            byte[] data = new byte[64 * 4];
            int i = 0;
            for (; i < 32 * 4; i += 4) {
                data[i] = 0;
                data[i+1] = 216;
                data[i+2] = 255;
                data[i+3] = 255;
            }
            for (; i < 32 * 4 + 16 * 4; i += 4) {
                data[i] = 255;
                data[i+1] = 148;
                data[i+2] = 0;
                data[i+3] = 255;
            }
            for (; i < 32 * 4 + 16 * 4 + 8*4; i += 4) {
                data[i] = 0;
                data[i+1] = 255;
                data[i+2] = 76;
                data[i+3] = 255;
            }
            for (; i < 32 * 4 + 16 * 4 + 8*4 + 8*4; i += 4) {
                data[i] = 0;
                data[i+1] = 0;
                data[i+2] = 255;
                data[i+3] = 255;
            }

            var image = Pfim.FromFile(Path.Combine("data", "true-32-rle.tga"));

            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void parseTarga24TrueRunLength () {
            byte[] data = new byte[64 * 3];
            int i = 0;
            for (; i < 32 * 3; i += 3) {
                data[i] = 0;
                data[i+1] = 216;
                data[i+2] = 255;
            }
            for (; i < 32 * 3 + 16 * 3; i += 3) {
                data[i] = 255;
                data[i+1] = 148;
                data[i+2] = 0;
            }
            for (; i < 32 * 3 + 16 * 3 + 8*3; i += 3) {
                data[i] = 0;
                data[i+1] = 255;
                data[i+2] = 76;
            }
            for (; i < 32 * 3 + 16 * 3 + 8*3 + 8*3; i += 3) {
                data[i] = 0;
                data[i+1] = 0;
                data[i+2] = 255;
            }

            var image = Pfim.FromFile(Path.Combine("data", "true-24-rle.tga"));

            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void parseTrueTarga32MixedEncoding() {
            var image = Pfim.FromFile(Path.Combine("data", "true-32-mixed.tga"));
            byte[] data = new byte[256];
            for (int i = 0; i < 16 * 4; i+=4) {
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
            for (int i = 128; i < 192; i+=4) {
                data[i] = 255;
                data[i + 1] = 148;
                data[i + 2] = 0;
                data[i + 3] = 255;
            }
            for (int i = 192; i < 224; i+=4) {
                data[i] = 0;
                data[i + 1] = 255;
                data[i + 2] = 76;
                data[i + 3] = 255;
            }
            for (int i = 224; i < 256; i+=4) {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 255;
                data[i + 3] = 255;
            }
            Assert.Equal(data, image.Data);
        }
        
        [Fact]
        public void parseLarge32TargetImage() {
            var image = Pfim.FromFile(Path.Combine("data", "true-32-rle-large.tga"));
            byte[] data = new byte[1200 * 1200 * 4];
            for (int i = 0; i < data.Length; i += 4) {
                data[i] = 0;
                data[i + 1] = 51;
                data[i + 2] = 127;
                data[i + 3] = 255;
            }
            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void parseTargaTopLeft() {
            var image = Pfim.FromFile(Path.Combine("data", "rgb24_top_left.tga"));
            for (int i = 0; i < image.Data.Length; i += 3) {
                if (!((image.Data[i] == 0 && image.Data[i + 1] == 255 && image.Data[i + 2] == 0) ||
                    (image.Data[i] == 12 && image.Data[i + 1] == 0 && image.Data[i + 2] == 255) ||
                    (image.Data[i] == 255 && image.Data[i + 1] == 255 && image.Data[i + 2] == 255) ||
                    (image.Data[i] == 255 && image.Data[i + 1] == image.Data[i + 2]))) {
                    Assert.True(false, $"Did not expect pixel {image.Data[i]} {image.Data[i+1]} {image.Data[i+2]}");
                }
            }
        }

        [Fact]
        public void parse32BitUncompressedDds() {
            var image = Pfim.FromFile(Path.Combine("data", "32-bit-uncompressed.dds"));
            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4) {
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
        public void parseSimpleDxt1() {
            var image = Pfim.FromFile(Path.Combine("data", "dxt1-simple.dds"));
            byte[] data = new byte[64 * 64 * 3];
            for (int i = 0; i < data.Length; i += 3) {
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
        public void parseSimpleDxt3() {
            var image = Pfim.FromFile(Path.Combine("data", "dxt3-simple.dds"));
            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4) {
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
        public void parseSimpleDxt5() {
            var image = Pfim.FromFile(Path.Combine("data", "dxt5-simple.dds"));
            byte[] data = new byte[64 * 64 * 4];
            for (int i = 0; i < data.Length; i += 4) {
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
    }
}
