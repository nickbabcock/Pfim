using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Defines a series of functions that can decode a uncompressed targa image
    /// </summary>
    public class UncompressedTarga : IDecodeTarga
    {
        /// <summary>Fills data starting from the bottom left</summary>
        public byte[] BottomLeft(Stream str, TargaHeader header)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Height * stride];
            var rowBits = header.PixelDepth * header.Width;
            Util.FillBottomLeft(str, data, rowBits / 8, stride);
            return data;
        }

        /// <summary>Not implemented</summary>
        public byte[] BottomRight(Stream str, TargaHeader header)
        {
            throw new NotImplementedException();
        }

        /// <summary>Not implemented</summary>
        public byte[] TopRight(Stream str, TargaHeader header)
        {
            throw new NotImplementedException();
        }

        /// <summary>Fills data starting from the top left</summary>
        public byte[] TopLeft(Stream str, TargaHeader header)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Height * stride];
            Util.Fill(str, data);
            return data;
        }
    }
}
