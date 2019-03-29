using System.IO;
using Xunit;

namespace Pfim.Tests
{
    public class UtilTests
    {
        [Fact]
        public void TranslateIdentity()
        {
            byte[] buf = { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 0);
            Assert.Equal(5, actual);
        }

        [Fact]
        public void TranslateByteFromStream()
        {
            byte[] buf = { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream(new byte[] { 100 });
            var actual = Util.Translate(mem, buf, 1);
            Assert.Equal(5, actual);
            Assert.Equal(new byte[] { 2, 3, 4, 5, 100 }, buf);
        }

        [Fact]
        public void TranslateByteFromStreamButNoByteToGive()
        {
            byte[] buf = { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 1);
            Assert.Equal(4, actual);
            Assert.Equal(new byte[] { 2, 3, 4, 5, 5 }, buf);
        }

        [Fact]
        public void TranslateAllButLastByte()
        {
            byte[] buf = { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream(new byte[] { 100, 99, 98, 97 });
            var actual = Util.Translate(mem, buf, 4);
            Assert.Equal(5, actual);
            Assert.Equal(new byte[] { 5, 100, 99, 98, 97 }, buf);
        }

        [Fact]
        public void TranslateAllButLastByteButNoByteToGive()
        {
            byte[] buf = { 1, 2, 3, 4, 5 };
            var mem = new MemoryStream();
            var actual = Util.Translate(mem, buf, 4);
            Assert.Equal(1, actual);
            Assert.Equal(new byte[] { 5, 2, 3, 4, 5 }, buf);
        }

        [Fact]
        public void FillButtomLeftSinglePixelRows()
        {
            byte[] data = new byte[5];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            Util.FillBottomLeft(mem, data, 1, 1);
            Assert.Equal(new byte[] { 5, 4, 3, 2, 1 }, data);
        }

        [Fact]
        public void FillButtomLeftDoublePixelRows()
        {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            Util.FillBottomLeft(mem, data, 2, 2);
            Assert.Equal(new byte[] { 5, 6, 3, 4, 1, 2 }, data);
        }

        [Fact]
        public void FillBottomLeftAndBufferCantHoldAll()
        {
            byte[] data = new byte[5];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            Util.FillBottomLeft(mem, data, 1, 1);
            Assert.Equal(new byte[] { 5, 4, 3, 2, 1 }, data);
        }

        [Fact]
        public void FillBottomLeftDoublePixelAndBufferCantHoldAll()
        {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            Util.FillBottomLeft(mem, data, 2, 2);
            Assert.Equal(new byte[] { 5, 6, 3, 4, 1, 2 }, data);
        }

        [Fact]
        public void FillBottomLeftWithPadding()
        {
            byte[] data = new byte[6];
            var mem = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            Util.FillBottomLeft(mem, data, 2, 3);
            Assert.Equal(new byte[] { 3, 4, 0, 1, 2, 0 }, data);
        }

        [Theory]
        [InlineData(4, 1, 32)]
        [InlineData(8, 2, 24)]
        public void TestStride(int expected, int widthBytes, int pixelDepthBits)
        {
            Assert.Equal(expected, Util.Stride(width: widthBytes, pixelDepth: pixelDepthBits));
        }

    }
}
