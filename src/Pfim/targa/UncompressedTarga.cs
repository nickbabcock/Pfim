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
            InnerBottomLeft(str, config, data, stride, rowBits);
            return data;
        }

#if NETSTANDARD1_3
        private static void InnerBottomLeft(Stream str, PfimConfig config, byte[] data, int stride, int rowBits)
        {
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
        }
#else
       private static void InnerBottomLeft(Stream str, PfimConfig config, byte[] data, int stride, int rowBits)
        {
            Util.FillBottomLeft(str, data, rowBits / 8, stride, config.BufferSize);
        }
#endif

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

            // If an image stride doesn't need any padding, we can
            // use an optimization where we can just copy the whole stream
            // into pixel data
            if (stride == header.Width * (header.PixelDepth / 8))
            {
                InnerTopLeft(str, config, data);
            }
            else
            {
                StrideTopLeft(str, config, header, data);
            }
            return data;
        }

        private void StrideTopLeft(Stream str, PfimConfig config, TargaHeader header, byte[] data)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            Util.InnerFillUnaligned(str, data, header.Width, stride, config.BufferSize);
        }

        private static void InnerTopLeft(Stream str, PfimConfig config, byte[] data)
        {
            Util.Fill(str, data, config.BufferSize);
        }
    }
}
