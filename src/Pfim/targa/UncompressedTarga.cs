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
            var stride = Util.Stride(header.Width, header.PixelDepthBits);
            var len = header.Height * stride;
            var data = config.Allocator.Rent(len);
            var trueStride = header.PixelDepthBytes * header.Width;
            InnerBottomLeft(str, config, data, len, stride, trueStride);
            return data;
        }

        private static void InnerBottomLeft(Stream str, PfimConfig config, byte[] data, int dataLen, int stride, int trueStride)
        {
            if (str is MemoryStream s && s.TryGetBuffer(out var arr))
            {
                int dataIndex = dataLen - stride;

                for (int i = 0; dataIndex > 0; i++, dataIndex -= stride)
                {
                    Buffer.BlockCopy(arr.Array, (int) (s.Position + i * trueStride), data, dataIndex, trueStride);
                }
            }
            else
            {
                var buffer = config.Allocator.Rent(config.BufferSize);
                try
                {
                    Util.FillBottomLeft(str, data, dataLen, trueStride, stride, buffer, config.BufferSize);
                }
                finally
                {
                    config.Allocator.Return(buffer);
                }
            }
        }

        public byte[] BottomRight(Stream str, TargaHeader header, PfimConfig config)
        {
            return BottomLeft(str, header, config);
        }

        public byte[] TopRight(Stream str, TargaHeader header, PfimConfig config)
        {
            return BottomLeft(str, header, config);
        }

        /// <summary>Fills data starting from the top left</summary>
        public byte[] TopLeft(Stream str, TargaHeader header, PfimConfig config)
        {
            var stride = Util.Stride(header.Width, header.PixelDepthBits);
            var len = header.Height * stride;
            var data = config.Allocator.Rent(header.Height * stride);

            // If an image stride doesn't need any padding, we can
            // use an optimization where we can just copy the whole stream
            // into pixel data
            if (stride == header.Width * (header.PixelDepthBits / 8))
            {
                Util.Fill(str, data, len, config.BufferSize);
            }
            else
            {
                StrideTopLeft(str, config, header, data);
            }
            return data;
        }

        private void StrideTopLeft(Stream str, PfimConfig config, TargaHeader header, byte[] data)
        {
            var stride = Util.Stride(header.Width, header.PixelDepthBits);
            var width = header.Width * header.PixelDepthBytes;
            var len = header.Height * stride;
            Util.InnerFillUnaligned(str, data, len, width, stride, config.BufferSize);
        }
    }
}
