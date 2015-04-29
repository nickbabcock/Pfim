using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// A DirectDraw Surface that is not compressed.  
    /// Thus what is in the input stream gets directly translated to the image buffer.
    /// </summary>
    public class UncompressedDds : IDecodeDds
    {
        private static DdsLoadInfo loadInfoB8G8R8A8 = new DdsLoadInfo(false, false, false, 1, 4 /*PixelFormat.Format32bppArgb*/);
        private static DdsLoadInfo loadInfoB8G8R8 = new DdsLoadInfo(false, false, false, 1, 3 /*PixelFormat.Format24bppRgb*/);
        private static DdsLoadInfo loadInfoB5G5R5A1 = new DdsLoadInfo(false, true, false, 1, 2 /*PixelFormat.Format16bppArgb1555*/);
        private static DdsLoadInfo loadInfoB5G6R5 = new DdsLoadInfo(false, true, false, 1, 2 /*PixelFormat.Format16bppRgb565*/);
        private static DdsLoadInfo loadInfoIndex8 = new DdsLoadInfo(false, false, true, 1, 1 /*PixelFormat.Format8bppIndexed*/);

        public bool IsSixteenBitAlphaZero(DdsHeader Header)
        {
            return (Header.PixelFormat.RGBBitCount == 16) &&
                (Header.PixelFormat.RBitMask == 0x0000f800) &&
                (Header.PixelFormat.GBitMask == 0x000007e0) &&
                (Header.PixelFormat.BBitMask == 0x0000001f);
        }

        public bool IsSixteenBitAlphaOne(DdsHeader Header)
        {
            return (Header.PixelFormat.RGBBitCount == 16) &&
                (Header.PixelFormat.RBitMask == 0x00007c00) &&
                (Header.PixelFormat.GBitMask == 0x000003e0) &&
                (Header.PixelFormat.BBitMask == 0x0000001f) &&
                (Header.PixelFormat.ABitMask == 0x00008000);
        }

        public bool IsThirtyTwoBitRgba(DdsHeader Header)
        {
                return (Header.PixelFormat.RGBBitCount == 32) &&
                    (Header.PixelFormat.RBitMask == 0xff0000) &&
                    (Header.PixelFormat.GBitMask == 0xff00) &&
                    (Header.PixelFormat.BBitMask == 0xff) &&
                    (Header.PixelFormat.ABitMask == 0xff000000U);
        }

        public bool IsTwentyFourBitRgb(DdsHeader Header)
        {
            return (Header.PixelFormat.RGBBitCount == 24) &&
                (Header.PixelFormat.RBitMask == 0xff0000) &&
                (Header.PixelFormat.GBitMask == 0xff00) &&
                (Header.PixelFormat.BBitMask == 0xff);
        }

        public DdsLoadInfo ImageInfo(DdsHeader header)
        {
            if (IsThirtyTwoBitRgba(header))
                return loadInfoB8G8R8A8;
            else if (IsTwentyFourBitRgb(header))
                return loadInfoB8G8R8;
            else if (IsSixteenBitAlphaOne(header))
                return loadInfoB5G5R5A1;
            else if (IsSixteenBitAlphaOne(header))
                return loadInfoB5G6R5;
            else if (header.PixelFormat.RGBBitCount == 8)
                return loadInfoIndex8;
            throw new ApplicationException("Unrecognized format");
        }

        public byte[] Decode(Stream str, DdsHeader header)
        {
            byte[] buffer = new byte[Dds.CalcSize(ImageInfo(header), header)];
            Util.Fill(str, buffer);
            return buffer;
        }
    }
}
