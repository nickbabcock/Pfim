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
        public byte[] BottomLeft(Stream str, TargaHeader header, PfimConfig config)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Height * stride];
            var rowBits = header.PixelDepth * header.Width;

            if (str is MemoryStream s && s.TryGetBuffer(out var arr))
            {
                int dataIndex = data.Length - stride;
                int rowBytes = rowBits / 8;
                int totalRows = data.Length / rowBytes;
                for (int i = 0; i < totalRows; i++, dataIndex -= stride)
                {
                    Buffer.BlockCopy(arr.Array, (int) (s.Position + i * rowBytes), data, dataIndex, rowBytes);
                }
            }
            else
            {
                Util.FillBottomLeft(str, data, rowBits / 8, stride, config.BufferSize);
            }
            return data;
        }

        /// <summary>Not implemented</summary>
        public byte[] BottomRight(Stream str, TargaHeader header, PfimConfig config)
        {
            throw new NotImplementedException();
        }

        /// <summary>Not implemented</summary>
        public byte[] TopRight(Stream str, TargaHeader header, PfimConfig config)
        {
            throw new NotImplementedException();
        }

        /// <summary>Fills data starting from the top left</summary>
        public byte[] TopLeft(Stream str, TargaHeader header, PfimConfig config)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Height * stride];
            if (str is MemoryStream s && s.TryGetBuffer(out var arr))
            {
                Buffer.BlockCopy(arr.Array, (int) s.Position, data, 0, data.Length);
            }
            else
            {
                Util.Fill(str, data, config.BufferSize);
            }
            return data;
        }
    }
}
