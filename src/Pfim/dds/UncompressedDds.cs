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
        private static DdsLoadInfo loadInfoB8G8R8A8 = new DdsLoadInfo(false, false, false, 1, 4, 32, ImageFormat.Rgba32);
        private static DdsLoadInfo loadInfoB8G8R8 = new DdsLoadInfo(false, false, false, 1, 3, 32, ImageFormat.Rgba32);
        private static DdsLoadInfo loadInfoIndex8 = new DdsLoadInfo(false, false, true, 1, 1, 8, ImageFormat.Rgb8);

        /// <summary>Determines if the image is 32bit rgb</summary>
        public bool IsThirtyTwoBitRgba(DdsHeader Header)
        {
            return Header.PixelFormat.RGBBitCount == 32 &&
                   Header.PixelFormat.PixelFormatFlags.HasFlag(DdsPixelFormatFlags.AlphaPixels) &&
                   Header.PixelFormat.PixelFormatFlags.HasFlag(DdsPixelFormatFlags.Rgb);
        }

        /// <summary>Determines if the image is 24bit rgb</summary>
        public bool IsTwentyFourBitRgb(DdsHeader Header)
        {
            return (Header.PixelFormat.RGBBitCount == 24) &&
                (Header.PixelFormat.RBitMask == 0xff0000) &&
                (Header.PixelFormat.GBitMask == 0xff00) &&
                (Header.PixelFormat.BBitMask == 0xff);
        }

        /// <summary>Determine image info from header</summary>
        public DdsLoadInfo ImageInfo(DdsHeader header)
        {
            if (header.PixelFormat.RGBBitCount == 16)
            {
                ImageFormat format = SixteenBitImageFormat(header);
                return new DdsLoadInfo(false, true, false, 1, 2, 16, format);
            }

            if (IsThirtyTwoBitRgba(header))
                return loadInfoB8G8R8A8;
            else if (IsTwentyFourBitRgb(header))
                return loadInfoB8G8R8;
            else if (header.PixelFormat.RGBBitCount == 8)
                return loadInfoIndex8;
            throw new Exception("Unrecognized format");
        }

        private static ImageFormat SixteenBitImageFormat(DdsHeader header)
        {
            if (header.PixelFormat.PixelFormatFlags.HasFlag(DdsPixelFormatFlags.AlphaPixels))
            {
                return ImageFormat.A1r5g5b5;
            }

            return header.PixelFormat.GBitMask == 0x7e0 ? ImageFormat.R5g6b5 : ImageFormat.R5g5b5;
        }

        /// <summary>Decode data into raw rgb format</summary>
        public byte[] Decode(Stream str, DdsHeader header)
        {
            byte[] buffer = new byte[Dds.CalcSize(ImageInfo(header), header)];
            Util.Fill(str, buffer);
            return buffer;
        }
    }
}
