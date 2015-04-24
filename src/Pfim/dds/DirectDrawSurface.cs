namespace Pfim
{
    struct DDSLoadInfo
    {
        internal bool compressed;
        internal bool swap;
        internal bool palette;

        /// <summary>
        /// The length of a block is in pixels.
        /// This mainly affects compressed images as they are
        /// encoded in blocks that are divSize by divSize.
        /// Uncompressed DDS do not need this value.
        /// </summary>
        internal uint divSize;

        /// <summary>
        /// Number of bytes needed to decode block of pixels
        /// that is divSize by divSize.  This takes into account
        /// how many bytes it takes to extract color and alpha information.
        /// Uncompressed DDS do not need this value.
        /// </summary>
        internal uint blockBytes;
        //internal PixelFormat pixelFormat;

        public DDSLoadInfo(bool isCompresed, bool isSwap, bool isPalette, uint aDivSize, uint aBlockBytes/*, PixelFormat PixelFormat*/)
        {
            compressed = isCompresed;
            swap = isSwap;
            palette = isPalette;
            divSize = aDivSize;
            blockBytes = aBlockBytes;
            //pixelFormat = PixelFormat;
        }
    }
}
