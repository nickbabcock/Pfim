using Pfim.dds.bptc.common;
using System.Collections.Generic;

namespace Pfim.dds.bptc
{
    internal static class DDS_Decoders
    {
        // TODO: Virtual/physical size. Less than 4x4 texels

        #region Compressed Readers

        // BC6
        internal static void DecompressBC6Block(byte[] source, int sourceStart, byte[] destination, int decompressedStart, int decompressedLineLength, bool unused)
        {
            var colours = BC6.DecompressBC6(source, sourceStart, false);
            SetColoursFromDX10(colours, destination, decompressedStart, decompressedLineLength);
        }


        // BC7
        internal static void DecompressBC7Block(byte[] source, int sourceStart, byte[] destination, int decompressedStart, int decompressedLineLength, bool unused)
        {
            var colours = BC7.DecompressBC7(source, sourceStart);
            SetColoursFromDX10(colours, destination, decompressedStart, decompressedLineLength);
        }

        static void SetColoursFromDX10(DX10_Helpers.LDRColour[] block, byte[] destination, int decompressedStart, int decompressedLineLength)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    // BGRA
                    int BPos = decompressedStart + (i * decompressedLineLength) + (j * 4);
                    int GPos = decompressedStart + (i * decompressedLineLength) + (j * 4) + 1;
                    int RPos = decompressedStart + (i * decompressedLineLength) + (j * 4) + 2;
                    int APos = decompressedStart + (i * decompressedLineLength) + (j * 4) + 3;
                    var colour = block[(i * 4) + j];

                    destination[RPos] = (byte)colour.R;
                    destination[GPos] = (byte)colour.G;
                    destination[BPos] = (byte)colour.B;
                    destination[APos] = (byte)colour.A;
                }
            }
        }


        static byte ExpandTo255(double v)
        {
            if (double.IsNaN(v) || v == 0)
                return 128;
            else
                return (byte)(((v + 1d) / 2d) * 255d);
        }

        internal static int GetDecompressedOffset(int start, int lineLength, int pixelIndex)
        {
            return start + (lineLength * (pixelIndex / 4)) + (pixelIndex % 4) * 4;
        }
        #endregion Compressed Readers

        #region Uncompressed Readers
        internal static void ReadUncompressed(byte[] source, int sourceStart, byte[] destination, int pixelCount, DDS_Header.DDS_PIXELFORMAT ddspf, ImageFormats.ImageEngineFormatDetails formatDetails)
        {
            bool requiresSignedAdjustment = ((ddspf.dwFlags & DDS_Header.DDS_PFdwFlags.DDPF_SIGNED) == DDS_Header.DDS_PFdwFlags.DDPF_SIGNED);
            int sourceIncrement = ddspf.dwRGBBitCount / 8;  // /8 for bits to bytes conversion
            bool oneChannel = (ddspf.dwFlags & DDS_Header.DDS_PFdwFlags.DDPF_LUMINANCE) == DDS_Header.DDS_PFdwFlags.DDPF_LUMINANCE;
            bool twoChannel = (ddspf.dwFlags & DDS_Header.DDS_PFdwFlags.DDPF_ALPHAPIXELS) == DDS_Header.DDS_PFdwFlags.DDPF_ALPHAPIXELS && oneChannel;

            uint AMask = ddspf.dwABitMask;
            uint RMask = ddspf.dwRBitMask;
            uint GMask = ddspf.dwGBitMask;
            uint BMask = ddspf.dwBBitMask;


            ///// Figure out channel existance and ordering.
            // Setup array that indicates channel offset from pixel start.
            // e.g. Alpha is usually first, and is given offset 0.
            // NOTE: Ordering array is in ARGB order, and the stored indices change depending on detected channel order.
            // A negative index indicates channel doesn't exist in data and sets channel to 0xFF.
            List<uint> maskOrder = new List<uint>(4) { AMask, RMask, GMask, BMask };
            maskOrder.Sort();
            maskOrder.RemoveAll(t => t == 0);  // Required, otherwise indicies get all messed up when there's only two channels, but it's not indicated as such.

            // TODO: Cubemaps and hardcoded format readers for performance
            int AIndex = 0;
            int RIndex = 0;
            int GIndex = 0;
            int BIndex = 0;

            if (twoChannel)  // Note: V8U8 does not come under this one.
            {
                // Intensity is first byte, then the alpha. Set all RGB to intensity for grayscale.
                // Second mask is always RMask as determined by the DDS Spec.
                AIndex = AMask > RMask ? 1 : 0;
                RIndex = AMask > RMask ? 0 : 1;
                GIndex = AMask > RMask ? 0 : 1;
                BIndex = AMask > RMask ? 0 : 1;
            }
            else if (oneChannel)
            {
                // Decide whether it's alpha or not.
                AIndex = AMask == 0 ? -1 : 0; 
                RIndex = AMask == 0 ? 0 : -1; 
                GIndex = AMask == 0 ? 0 : -1;
                BIndex = AMask == 0 ? 0 : -1; 
            }
            else
            {
                // Set default ordering
                AIndex = AMask == 0 ? -1 : maskOrder.IndexOf(AMask) * formatDetails.ComponentSize;
                RIndex = RMask == 0 ? -1 : maskOrder.IndexOf(RMask) * formatDetails.ComponentSize;
                GIndex = GMask == 0 ? -1 : maskOrder.IndexOf(GMask) * formatDetails.ComponentSize;
                BIndex = BMask == 0 ? -1 : maskOrder.IndexOf(BMask) * formatDetails.ComponentSize;
            }

            // Determine order of things
            int destAInd = 3 * formatDetails.ComponentSize;
            int destRInd = 2 * formatDetails.ComponentSize;
            int destGInd = 1 * formatDetails.ComponentSize;
            int destBInd = 0;

            switch (formatDetails.ComponentSize)
            {
                case 1:
                    // Check masks fit properly
                    if (maskOrder.Count != sourceIncrement)
                    {
                        // Determine mask size
                        var lengths = new int[4];
                        lengths[0] = CountSetBits(BMask);
                        lengths[1] = CountSetBits(GMask);
                        lengths[2] = CountSetBits(RMask);
                        lengths[3] = CountSetBits(AMask);

                        ReadBytesLegacy(source, sourceStart, sourceIncrement, lengths, destination, new int[] { destBInd, destGInd, destRInd, destAInd });
                    }
                    else
                        ReadBytes(source, sourceStart, sourceIncrement, new int[] { BIndex, GIndex, RIndex, AIndex }, destination, new int[] { destBInd, destGInd, destRInd, destAInd });
                    break;
                case 2:
                    ReadUShorts(source, sourceStart, sourceIncrement, new int[] { BIndex, GIndex, RIndex, AIndex }, destination, new int[] { destBInd, destGInd, destRInd, destAInd });
                    break;
                case 4:
                    ReadFloats(source, sourceStart, sourceIncrement, new int[] { BIndex, GIndex, RIndex, AIndex }, destination, new int[] { destBInd, destGInd, destRInd, destAInd });
                    break;
            }
            
            if (requiresSignedAdjustment)
            {
                for (int i = 0; i < destination.Length; i += 4)
                {
                    //destination[i] -= 128;  // Don't adjust blue
                    destination[i + 1] -= 128;
                    destination[i + 2] -= 128;

                    // Alpha not adjusted
                }
            }
        }

        static int CountSetBits(uint i)
        {
            i = i - ((i >> 1) & 0x5555_5555);
            i = (i & 0x3333_3333) + ((i >> 2) & 0x3333_3333);
            return (int)((((i + (i >> 4)) & 0x0F0F_0F0F) * 0x0101_0101) >> 24);
        }

        static void ReadBytes(byte[] source, int sourceStart, int sourceIncrement, int[] sourceInds, byte[] destination, int[] destInds)
        {
            for (int i = 0, j = sourceStart; i < destination.Length; i += 4, j += sourceIncrement)
            {
                for (int k = 0; k < 4; k++)
                {
                    int sourceInd = sourceInds[k];
                    destination[i + destInds[k]] = sourceInd == -1 ? byte.MaxValue : source[j + sourceInd];
                }
            }
        }

        static void ReadBytesLegacy(byte[] source, int sourceStart, int sourceIncrement, int[] lengths, byte[] destination, int[] destInds)
        {
            int count = 0;
            for (int i = 0, j = sourceStart; i < destination.Length; i += 4, j += sourceIncrement)
            {
                for (int k = 0; k < 4; k++)
                {
                    int channelLength = lengths[k];
                    byte temp = byte.MaxValue;
                    if (channelLength != 0)
                    {
                        temp = (byte)DX10_Helpers.GetBits(source, sourceStart, ref count, lengths[k]);

                        // Put on 0-255 range.
                        temp = (byte)(temp << (8 - channelLength));
                    }

                    destination[i + destInds[k]] = temp;
                }
            }
        }

        static void ReadUShorts(byte[] source, int sourceStart, int sourceIncrement, int[] sourceInds, byte[] destination, int[] destInds)
        {
            for (int i = 0, j = sourceStart; i < destination.Length; i += 8, j += sourceIncrement)
            {
                for (int k = 0; k < 4; k++)
                {
                    int sourceInd = j + sourceInds[k];
                    int destInd = i + destInds[k];
                    if (sourceInds[k] == -1)
                    {
                        destination[destInd] = byte.MaxValue;
                        destination[destInd + 1] = byte.MaxValue;
                    }
                    else
                    {
                        destination[destInd] = source[sourceInd];
                        destination[destInd + 1] = source[sourceInd + 1];
                    }
                }
            }
        }

        static void ReadFloats(byte[] source, int sourceStart, int sourceIncrement, int[] sourceInds, byte[] destination, int[] destInds)
        {
            for (int i = 0, j = sourceStart; i < destination.Length; i += 16, j += sourceIncrement)
            {
                for (int k = 0; k < 4; k++)
                {
                    int sourceInd = j + sourceInds[k];
                    int destInd = i + destInds[k];
                    if (sourceInds[k] == -1)
                    {
                        destination[destInd] = 0;
                        destination[destInd + 1] = 0;
                        destination[destInd + 2] = 63;
                        destination[destInd + 3] = 127;
                    }
                    else
                    {
                        destination[destInd] = source[sourceInd];
                        destination[destInd + 1] = source[sourceInd + 1];
                        destination[destInd + 2] = source[sourceInd + 2];
                        destination[destInd + 3] = source[sourceInd + 3];
                    }
                }
            }
        }
        #endregion Uncompressed Readers
    }
}
