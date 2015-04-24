using System;
using System.IO;

namespace Pfim
{
    public class UncompressedTarga : IDecodeTarga
    {
        public byte[] BottomLeft(Stream str, TargaHeader header)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Width * stride];
            var pixelWidth = header.PixelDepth * header.Width;
            var padding = stride * 8 - pixelWidth;
            Util.FillBottomLeft(str, data, pixelWidth / 8, padding: padding);
            return data;
        }

        public byte[] BottomRight(Stream str, TargaHeader header)
        {
            throw new NotImplementedException();
        }

        public byte[] TopRight(Stream str, TargaHeader header)
        {
            throw new NotImplementedException();
        }

        public byte[] TopLeft(Stream str, TargaHeader header)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Width * stride];
            Util.Fill(str, data);
            return data;
        }
    }
}
