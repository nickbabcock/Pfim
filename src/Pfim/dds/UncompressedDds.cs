using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// A DirectDraw Surface that is not compressed.  
    /// Thus what is in the input stream gets directly translated to the image buffer.
    /// </summary>
    internal class UncompressedDds : IDecodeDds
    {
        /// <summary>Determine image info from header</summary>
        public DdsLoadInfo ImageInfo(DdsHeader header)
        {
            bool rgbSwapped = header.PixelFormat.RBitMask < header.PixelFormat.GBitMask;

            switch (header.PixelFormat.RGBBitCount)
            {
                case 8:
                    return new DdsLoadInfo(false, rgbSwapped, true, 1, 1, 8, ImageFormat.Rgb8);
                case 16:
                    ImageFormat format = SixteenBitImageFormat(header);
                    return new DdsLoadInfo(false, rgbSwapped, false, 1, 2, 16, format);
                case 24:
                    return new DdsLoadInfo(false, rgbSwapped, false, 1, 3, 24, ImageFormat.Rgb24);
                case 32:
                    return new DdsLoadInfo(false, rgbSwapped, false, 1, 4, 32, ImageFormat.Rgba32);
                default:
                    throw new Exception($"Unrecognized rgb bit count: {header.PixelFormat.RGBBitCount}");
            }
        }

        private static ImageFormat SixteenBitImageFormat(DdsHeader header)
        {
            var pf = header.PixelFormat;

            if (pf.ABitMask == 0xF000 && pf.RBitMask == 0xF00 && pf.GBitMask == 0xF0 && pf.BBitMask == 0xF)
            {
                return ImageFormat.Rgba16;
            }

            if (pf.PixelFormatFlags.HasFlag(DdsPixelFormatFlags.AlphaPixels))
            {
                return ImageFormat.R5g5b5a1;
            }

            return pf.GBitMask == 0x7e0 ? ImageFormat.R5g6b5 : ImageFormat.R5g5b5;
        }

        /// <summary>Decode data into raw rgb format</summary>
        public byte[] Decode(Stream str, DdsHeader header, DdsLoadInfo imageInfo)
        {
            byte[] buffer = new byte[Dds.CalcSize(ImageInfo(header), header)];
            Util.Fill(str, buffer);

            // Swap the R and B channels
            if (imageInfo.Swap)
            {
                switch (imageInfo.Format)
                {
                    case ImageFormat.Rgba32:
                        for (int i = 0; i < buffer.Length; i += 4)
                        {
                            byte temp = buffer[i];
                            buffer[i] = buffer[i + 2];
                            buffer[i + 2] = temp;
                        }
                        break;
                    case ImageFormat.Rgba16:
                        for (int i = 0; i < buffer.Length; i += 2)
                        {
                            byte temp = (byte) (buffer[i] & 0xF);
                            buffer[i] = (byte) ((buffer[i] & 0xF0) + (buffer[i + 1] & 0XF));
                            buffer[i + 1] = (byte) ((buffer[i + 1] & 0xF0) + temp);
                        }
                        break;
                    default:
                        throw new Exception($"Do not know how to swap {imageInfo.Format}");
                }

            }

            return buffer;
        }
    }
}
