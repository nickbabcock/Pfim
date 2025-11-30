using Pfim.dds.bptc.common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pfim.dds.bptc
{
    /// <summary>
    /// Provides general functions specific to DDS format
    /// </summary>
    public static class DDSGeneral
    {
        /// <summary>
        /// Value at which alpha is included in DXT1 conversions. i.e. pixels lower than this threshold are made 100% transparent, and pixels higher are made 100% opaque.
        /// </summary>
        public static double DXT1AlphaThreshold = 0.2;

        /// <summary>
        /// Determines whether an image size is suitable for DXT compression.
        /// </summary>
        /// <param name="width">Width of image.</param>
        /// <param name="height">Height of image.</param>
        /// <returns>True if size is suitable for DXT compression.</returns>
        public static bool CheckSize_DXT(int width, int height)
        {
            return width % 4 == 0 && height % 4 == 0;
        }

        #region Loading
        private static MipMap ReadUncompressedMipMap(MemoryStream stream, int mipOffset, int mipWidth, int mipHeight, DDS_Header.DDS_PIXELFORMAT ddspf, ImageFormats.ImageEngineFormatDetails formatDetails)
        {
            byte[] data = stream.GetBuffer();
            byte[] mipmap = new byte[mipHeight * mipWidth * 4 * formatDetails.ComponentSize];

            // Smaller sizes breaks things, so just exclude them
            if (mipHeight >= 4 && mipWidth >= 4)
                DDS_Decoders.ReadUncompressed(data, mipOffset, mipmap, mipWidth * mipHeight, ddspf, formatDetails);

            return new MipMap(mipmap, mipWidth, mipHeight, formatDetails);
        }

        private static MipMap ReadCompressedMipMap(MemoryStream compressed, int mipWidth, int mipHeight, int mipOffset, ImageFormats.ImageEngineFormatDetails formatDetails, Action<byte[], int, byte[], int, int, bool> DecompressBlock, PfimConfig config)
        {
            // Gets stream as data. Note that this array isn't necessarily the correct size. Likely to have garbage at the end.
            // Don't want to use ToArray as that creates a new array. Don't want that.
            byte[] CompressedData = compressed.GetBuffer();

            byte[] decompressedData = new byte[4 * mipWidth * mipHeight * formatDetails.ComponentSize];
            int decompressedRowLength = mipWidth * 4;
            int texelRowSkip = decompressedRowLength * 4;

            int texelCount = (mipWidth * mipHeight) / 16;
            int numTexelsInRow = mipWidth / 4;

            if (texelCount != 0)
            {
                var action = new Action<int>(texelIndex =>
                {
                    int compressedPosition = mipOffset + texelIndex * formatDetails.BlockSize;
                    int decompressedStart = (int)(texelIndex / numTexelsInRow) * texelRowSkip + (texelIndex % numTexelsInRow) * 16;

                    // Problem with how I handle dimensions (no virtual padding or anything yet)
                    if (!CheckSize_DXT(mipWidth, mipHeight))  
                        return;

                    try
                    {
                        DecompressBlock(CompressedData, compressedPosition, decompressedData, decompressedStart, decompressedRowLength, formatDetails.IsPremultipliedFormat);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw;
                    }
                });

                // Actually perform decompression using threading, no threading, or GPU.
                if (config.ThreadingEnabled)
                    Parallel.For(0, texelCount, new ParallelOptions { MaxDegreeOfParallelism = config.ThreadingMaxThreads }, (texelIndex, loopstate) =>
                    {
                        // TODO@NickBabcock
                        //if (ImageEngine.IsCancellationRequested)
                          //  loopstate.Stop();

                        action(texelIndex);
                    });
                else
                    for (int texelIndex = 0; texelIndex < texelCount; texelIndex++) // TODO@NickBabcock: may be remove single-threaded decompressing?
                    {
                        // TODO@NickBabcock
                        //if (ImageEngine.IsCancellationRequested)
                          //  break;

                        action(texelIndex);
                    }
            }
            // No else here cos the lack of texels means it's below texel dimensions (4x4). So the resulting block is set to 0. Not ideal, but who sees 2x2 mipmaps?

            //TODO@NickBabcock
            //if (ImageEngine.IsCancellationRequested)
              //  return null;

            return new MipMap(decompressedData, mipWidth, mipHeight, formatDetails);
        }

        

        internal static List<MipMap> LoadDDS(MemoryStream compressed, DDS_Header header, int desiredMaxDimension, ImageFormats.ImageEngineFormatDetails formatDetails, PfimConfig config)
        {
            MipMap[] MipMaps = null;

            int mipWidth = header.Width;
            int mipHeight = header.Height;
            ImageEngineFormat format = header.Format;

            int estimatedMips = header.dwMipMapCount;
            int mipOffset = formatDetails.HeaderSize;
            int originalOffset = mipOffset;

            if (!EnsureMipInImage(compressed.Length, mipWidth, mipHeight, 4, formatDetails, out mipOffset))  // Update number of mips too
                estimatedMips = 1;

            if (estimatedMips == 0)
                estimatedMips = EstimateNumMipMaps(mipWidth, mipHeight);

            mipOffset = originalOffset;  // Needs resetting after checking there's mips in this image.

            // Ensure there's at least 1 mipmap
            if (estimatedMips == 0)
                estimatedMips = 1;

            int orig_estimatedMips = estimatedMips; // Store original count for later (uncompressed only I think)

            // KFreon: Decide which mip to start loading at - going to just load a few mipmaps if asked instead of loading all, then choosing later. That's slow.
            if (desiredMaxDimension != 0 && estimatedMips > 1)
            {
                if (!EnsureMipInImage(compressed.Length, mipWidth, mipHeight, desiredMaxDimension, formatDetails, out mipOffset))  // Update number of mips too
                    throw new InvalidDataException($"Requested mipmap does not exist in this image. Top Image Size: {mipWidth}x{mipHeight}, requested mip max dimension: {desiredMaxDimension}.");

                // Not the first mipmap. 
                if (mipOffset > formatDetails.HeaderSize)
                {

                    double divisor = mipHeight > mipWidth ? mipHeight / desiredMaxDimension : mipWidth / desiredMaxDimension;
                    mipHeight = (int)(mipHeight / divisor);
                    mipWidth = (int)(mipWidth / divisor);

                    if (mipWidth == 0 || mipHeight == 0)  // Reset as a dimension is too small to resize
                    {
                        mipHeight = header.Height;
                        mipWidth = header.Width;
                        mipOffset = formatDetails.HeaderSize;
                    }
                    else
                    {
                        // Update estimated mips due to changing dimensions.
                        estimatedMips = EstimateNumMipMaps(mipWidth, mipHeight);
                    }
                }
                else  // The first mipmap
                    mipOffset = formatDetails.HeaderSize;

            }

            // Move to requested mipmap
            compressed.Position = mipOffset;

            // Block Compressed texture chooser.
            Action<byte[], int, byte[], int, int, bool> DecompressBCBlock = formatDetails.BlockDecoder;

            MipMaps = new MipMap[estimatedMips];
            int blockSize = formatDetails.BlockSize;

            // KFreon: Read mipmaps
            if (formatDetails.IsBlockCompressed)    // Threading done in the decompression, not here.
            {
                for (int m = 0; m < estimatedMips; m++)
                {
                    // TODO@NickBabcock
                    //if (ImageEngine.IsCancellationRequested)
                      //  break;


                    // KFreon: If mip is too small, skip out. This happens most often with non-square textures. I think it's because the last mipmap is square instead of the same aspect.
                    // Don't do the mip size check here (<4) since we still need to have a MipMap object for those lower than this for an accurate count.
                    if (mipWidth <= 0 || mipHeight <= 0)  // Needed cos it doesn't throw when reading past the end for some reason.
                        break;
                    
                    MipMap mipmap = ReadCompressedMipMap(compressed, mipWidth, mipHeight, mipOffset, formatDetails, DecompressBCBlock, config);
                    MipMaps[m] = mipmap;
                    mipOffset += (int)(mipWidth * mipHeight * (blockSize / 16d)); // Also put the division in brackets cos if the mip dimensions are high enough, the multiplications can exceed int.MaxValue)
                    mipWidth /= 2;
                    mipHeight /= 2;
                }
            }
            else
            {
                int startMip = orig_estimatedMips - estimatedMips;

                // UNCOMPRESSED - Can't really do threading in "decompression" so do it for the mipmaps.
                var action = new Action<int>(mipIndex =>
                {
                    // Calculate mipOffset and dimensions
                    int offset, width, height;
                    offset = GetMipOffset(mipIndex, formatDetails, header.Width, header.Height);

                    double divisor = mipIndex == 0 ? 1d : 2 << (mipIndex - 1);   // Divisor represents 2^mipIndex - Math.Pow seems very slow.
                    width = (int)(header.Width / divisor);
                    height = (int)(header.Height / divisor);

                    MipMap mipmap = null;
                    try
                    {
                        mipmap = ReadUncompressedMipMap(compressed, offset, width, height, header.ddspf, formatDetails);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }

                    MipMaps[mipIndex] = mipmap;
                });

                if (config.ThreadingEnabled)
                    Parallel.For(startMip, orig_estimatedMips,  new ParallelOptions { MaxDegreeOfParallelism = config.ThreadingMaxThreads }, (mip, loopState) =>
                    {
                        // TODO@NickBabcock
                        //if (ImageEngine.IsCancellationRequested)
                          //  loopState.Stop();

                        action(mip);
                    });
                else
                    for (int i = startMip; i < orig_estimatedMips; i++) // TODO@NickBabcock: may be remove single-threaded decompressing?
                    {
                        // TODO@NickBabcock
                        //if (ImageEngine.IsCancellationRequested)
                          //  break;

                        action(i);
                    }
            }

            List<MipMap> mips = new List<MipMap>(MipMaps.Where(t => t != null));
            if (mips.Count == 0)
                throw new InvalidOperationException($"No mipmaps loaded. Estimated mips: {estimatedMips}, mip dimensions: {mipWidth}x{mipHeight}");
            return mips;
        }
        #endregion Loading

        #region Mipmap Management

        /// <summary>
        /// Estimates number of MipMaps for a given width and height EXCLUDING the top one.
        /// i.e. If output is 10, there are 11 mipmaps total.
        /// </summary>
        /// <param name="Width">Image Width.</param>
        /// <param name="Height">Image Height.</param>
        /// <returns>Number of mipmaps expected for image.</returns>
        public static int EstimateNumMipMaps(int Width, int Height)
        {
            int limitingDimension = Width > Height ? Height : Width;
            return (int)Math.Log(limitingDimension, 2); // There's 10 mipmaps besides the main top one.
        }


        /// <summary>
        /// Checks image file size to ensure requested mipmap is present in image.
        /// Header mip count can be incorrect or missing. Use this method to validate the mip you're after.
        /// </summary>
        /// <param name="streamLength">Image file stream length.</param>
        /// <param name="mainWidth">Width of image.</param>
        /// <param name="mainHeight">Height of image.</param>
        /// <param name="desiredMipDimension">Max dimension of desired mip.</param>
        /// <param name="destFormatDetails">Destination format details.</param>
        /// <param name="mipOffset">Offset of desired mipmap in image.</param>
        /// <returns>True if mip in image.</returns>
        public static bool EnsureMipInImage(long streamLength, int mainWidth, int mainHeight, int desiredMipDimension, ImageFormats.ImageEngineFormatDetails destFormatDetails, out int mipOffset)
        {
            if (mainWidth <= desiredMipDimension && mainHeight <= desiredMipDimension)
            {
                mipOffset = destFormatDetails.HeaderSize;
                return true; // One mip only
            }

            int dependentDimension = mainWidth > mainHeight ? mainWidth : mainHeight;
            int mipIndex = (int)Math.Log((dependentDimension / desiredMipDimension), 2);
            if (mipIndex < -1)
                throw new InvalidDataException($"Invalid dimensions for mipmapping. Got desired: {desiredMipDimension} and dependent: {dependentDimension}");

            int requiredOffset = GetMipOffset(mipIndex, destFormatDetails, mainHeight, mainWidth);  

            // KFreon: Something wrong with the count here by 1 i.e. the estimate is 1 more than it should be 
            if (destFormatDetails.Format == ImageEngineFormat.DDS_ARGB_8)  // TODO: Might not just be 8 bit, still don't know why it's wrong.
                requiredOffset -= 2;

            mipOffset = requiredOffset;

            // Should only occur when an image has 0 or 1 mipmap.
            //if (streamLength <= (requiredOffset - destFormatDetails.HeaderSize))
            if (streamLength <= requiredOffset)
                return false;

            return true;
        }

        internal static int GetMipOffset(double mipIndex, ImageFormats.ImageEngineFormatDetails destFormatDetails, int baseWidth, int baseHeight)
        {
            // -1 because if we want the offset of the mip, it's the sum of all sizes before it NOT including itself.
            return GetCompressedSizeUpToIndex(mipIndex - 1, destFormatDetails, baseWidth, baseHeight);  
        }

        internal static int GetCompressedSizeUpToIndex(double mipIndex, ImageFormats.ImageEngineFormatDetails destFormatDetails, int baseWidth, int baseHeight)
        {
            /*
                Mipmapping halves both dimensions per mip down. Dimensions are then divided by 4 if block compressed as a texel is 4x4 pixels.
                e.g. 4096 x 4096 block compressed texture with 8 byte blocks e.g. DXT1
                Sizes of mipmaps:
                    4096 / 4 x 4096 / 4 x 8
                    (4096 / 4 / 2) x (4096 / 4 / 2) x 8
                    (4096 / 4 / 2 / 2) x (4096 / 4 / 2 / 2) x 8

                Pattern: Each dimension divided by 2 per mip size decreased.
                Thus, total is divided by 4.
                    Size of any mip = Sum(1/4^n) x divWidth x divHeight x blockSize,  
                        where n is the desired mip (0 based), 
                        divWidth and divHeight are the block compress adjusted dimensions (uncompressed textures lead to just original dimensions, block compressed are divided by 4)

                Turns out the partial sum of the infinite sum: Sum(1/4^n) = 1/3 x (4 - 4^-n). Who knew right?
            */

            // TODO: DDS going down past 4x4
            bool requiresTinyAdjustment = false;
            int selectedMipDimensions = (int)(baseWidth / Math.Pow(2d, mipIndex));
            if (selectedMipDimensions < 4)
                requiresTinyAdjustment = true;

            double divisor = 1;
            if (destFormatDetails.IsBlockCompressed)
                divisor = 4;

            double shift = 1d / (4 << (int)(2 * (mipIndex - 1)));

            if (mipIndex == 0)
                shift = 1d;
            else if (mipIndex == -1)
                shift = 4d;

            double sumPart = mipIndex == -1 ? 0 :
                (1d / 3d) * (4d - shift);   // Shifting represents 4^-mipIndex. Math.Pow seems slow.

            double totalSize = destFormatDetails.HeaderSize + (sumPart * destFormatDetails.BlockSize * (baseWidth / divisor) * (baseHeight / divisor));
            if (requiresTinyAdjustment)
                totalSize += destFormatDetails.BlockSize * 2;

            return (int)totalSize;
        }

        internal static int GetCompressedSizeOfImage(int mipCount, ImageFormats.ImageEngineFormatDetails destFormatDetails, int baseWidth, int baseHeight)
        {
            return GetCompressedSizeUpToIndex(mipCount, destFormatDetails, baseWidth, baseHeight);
        }
        #endregion Mipmap Management
    }
}
