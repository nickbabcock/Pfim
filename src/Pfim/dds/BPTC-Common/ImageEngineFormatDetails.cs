using System;
using System.Diagnostics;

namespace Pfim.dds.bptc.common
{
    public partial class ImageFormats
    {
        /// <summary>
        /// Length of header in bytes when Additional DX10 Header is present.
        /// </summary>
        public const int DDS_DX10_HEADER_LENGTH = 148;


        /// <summary>
        /// Length of header when pre-DX10 format.
        /// </summary>
        public const int DDS_NO_DX10_HEADER_LENGTH = 128;



        /// <summary>
        /// Detailed representation of an image format.
        /// </summary>
        [DebuggerDisplay("Format: {Format}, ComponentSize: {ComponentSize}")]
        public class ImageEngineFormatDetails
        {
            /// <summary>
            /// Length of header (DDS only)
            /// </summary>
            public int HeaderSize => (Format == ImageEngineFormat.DDS_DX10 || (Format == ImageEngineFormat.DDS_ARGB_32F && DX10Format == DDS_Header.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT)) ? DDS_DX10_HEADER_LENGTH : DDS_NO_DX10_HEADER_LENGTH;


            /// <summary>
            /// Format of details.
            /// </summary>
            public ImageEngineFormat Format { get; }


            /// <summary>
            /// DX10Format when Format is set to DX10.
            /// </summary>
            public DDS_Header.DXGI_FORMAT DX10Format { get; }

            /// <summary>
            /// Indicates whether format contains premultiplied alpha.
            /// </summary>
            public bool IsPremultipliedFormat => false;

            /// <summary>
            /// Number of bytes in colour.
            /// </summary>
            public int ComponentSize
            {
                get
                {
                    var aveChannelWidth = BitCount / 8;
                    var remainder = aveChannelWidth % MaxNumberOfChannels;
                    if (remainder != 0)
                        return 1;  // TODO: More component sizes?

                    return aveChannelWidth / MaxNumberOfChannels;
                }
            }

            /// <summary>
            /// Number of bits in colour.
            /// </summary>
            public int BitCount { get; }

            /// <summary>
            /// Indicates whether supported format is Block Compressed.
            /// </summary>
            public bool IsBlockCompressed => IsBlockCompressed(Format);

            /// <summary>
            /// Indicates whether format supports mipmaps.
            /// </summary>
            public bool IsMippable => IsFormatMippable(Format);

            /// <summary>
            /// Size of a discrete block in bytes. (e.g. 2 channel 8 bit colour = 2, DXT1 = 16). Block can mean texel (DXTn) or pixel (uncompressed)
            /// </summary>
            public int BlockSize => GetBlockSize(Format, ComponentSize);

            /// <summary>
            /// String representation of formats' file extension. No '.'.
            /// </summary>
            public string Extension => Supported_Extension.ToString();

            /// <summary>
            /// Enum version of formats' file extension.
            /// </summary>
            public SupportedExtensions Supported_Extension => IsDDS ? SupportedExtensions.DDS : ParseExtension(Format.ToString());

            /// <summary>
            /// Indicates whether format is a DDS format.
            /// </summary>
            public bool IsDDS => Format.ToString().Contains("DDS") || Format == ImageEngineFormat.DDS_DX10;

            /// <summary>
            /// Max number of supported channels. Usually 4, but some formats are 1 (G8), 2 (V8U8), or 3 (RGB) channels.
            /// </summary>
            public int MaxNumberOfChannels => MaxNumberOfChannels(Format);

            /// <summary>
            /// Writes the max value to array using the correct bit styles.
            /// e.g. Will write int.Max when component size is int.Length (4 bytes).
            /// </summary>
            public Action<byte[], int> SetMaxValue = null;

            /// <summary>
            /// Reads a byte from a source array using the correct bit styles.
            /// </summary>
            public Func<byte[], int, byte> ReadByte = null;

            /// <summary>
            /// Reads a ushort (int16) from a source array using the correct bit styles.
            /// </summary>
            public Func<byte[], int, ushort> ReadUShort = null;

            /// <summary>
            /// Reads a float from a source array using the correct bit styles.
            /// </summary>
            public Func<byte[], int, float> ReadFloat = null;

            Func<byte[], int, byte[]> ReadUShortAsArray = null;
            Func<byte[], int, byte[]> ReadFloatAsArray = null;

            /// <summary>
            /// Holds the encoder to be used when compressing/writing image.
            /// </summary>
            public Action<byte[], int, int, byte[], int, AlphaSettings, ImageEngineFormatDetails> BlockEncoder = null;

            /// <summary>
            /// Holds the decoder to be used when decompressing/reading image.
            /// </summary>
            public Action<byte[], int, byte[], int, int, bool> BlockDecoder = null;

            /// <summary>
            /// Writes a colour from source to destination performing correct bit style conversions if requried.
            /// </summary>
            public Action<byte[], int, ImageEngineFormatDetails, byte[], int> WriteColour = null;

            /// <summary>
            /// Details the given format.
            /// </summary>
            /// <param name="dxgiFormat">Optional DX10 format. Default = Unknown.</param>
            /// <param name="inFormat">Image Format.</param>
            public ImageEngineFormatDetails(ImageEngineFormat inFormat, DDS_Header.DXGI_FORMAT dxgiFormat = new DDS_Header.DXGI_FORMAT())
            {
                Format = inFormat;

                DX10Format = dxgiFormat;

                BitCount = 8;
                {
                    switch (inFormat)
                    {
                        case ImageEngineFormat.DDS_G8_L8:
                        case ImageEngineFormat.DDS_A8:
                            BitCount = 8;
                            break;
                        case ImageEngineFormat.DDS_A8L8:
                        case ImageEngineFormat.DDS_V8U8:
                            BitCount = 16;
                            break;
                        case ImageEngineFormat.DDS_ABGR_8:
                        case ImageEngineFormat.DDS_ARGB_8:
                        case ImageEngineFormat.DDS_G16_R16:
                            BitCount = 32;
                            break;
                        case ImageEngineFormat.DDS_RGB_8:
                            BitCount = 24;
                            break;
                        case ImageEngineFormat.DDS_ARGB_32F:
                            DX10Format = DDS_Header.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT;
                            BitCount = 128;
                            break;
                        case ImageEngineFormat.DDS_R5G6B5:
                            BitCount = 16;
                            break;
                        case ImageEngineFormat.DDS_ARGB_4:
                        case ImageEngineFormat.DDS_CUSTOM:
                        case ImageEngineFormat.DDS_DX10:
                            BitCount = GetDX10BitCount(DX10Format);
                            break;
                    }
                }

                // Functions
                ReadByte = ReadByteFromByte;
                ReadUShort = ReadUShortFromByte;
                ReadFloat = ReadFloatFromByte;
                SetMaxValue = WriteByteMax;
                WriteColour = WriteByte;
                ReadUShortAsArray = ReadUShortFromByteAsArray;
                ReadFloatAsArray = ReadFloatFromByteOrUShortAsArray;

                if (ComponentSize == 2)
                {
                    ReadByte = ReadByteFromUShort;
                    ReadUShort = ReadUShortFromUShort;
                    ReadFloat = ReadFloatFromUShort;
                    SetMaxValue = WriteUShortMax;
                    WriteColour = WriteUShort;
                    ReadUShortAsArray = ReadUShortFromUShortAsArray;
                    // Don't need ReadFloatAsArray set here, as it's shared between byte and ushort reading.
                }
                else if (ComponentSize == 4)
                {
                    ReadByte = ReadByteFromFloat;
                    ReadUShort = ReadUShortFromFloat;
                    ReadFloat = ReadFloatFromFloat;
                    SetMaxValue = WriteFloatMax;
                    WriteColour = WriteFloat;
                    ReadUShortAsArray = ReadUShortFromFloatAsArray;
                    ReadFloatAsArray = ReadFloatFromFloatAsArray;
                }

                BlockEncoder = null;

                switch (inFormat)
                {
                    case ImageEngineFormat.DDS_DX10:
                        if (DX10Format.ToString().Contains("BC7"))
                            BlockDecoder = DDS_Decoders.DecompressBC7Block;
                        else
                            BlockDecoder = DDS_Decoders.DecompressBC6Block;
                        break;
                }
            }

            int GetDX10BitCount(DDS_Header.DXGI_FORMAT DX10Format)
            {
                int dx10Format = 32;
                switch (DX10Format)
                {
                    // For now, 32 works.
                }

                return dx10Format;
            }


            #region Bit Conversions
            byte ReadByteFromByte(byte[] source, int sourceStart)
            {
                return source[sourceStart];
            }

            byte ReadByteFromUShort(byte[] source, int sourceStart)
            {
                return (byte)(((source[sourceStart + 1] << 8) | source[sourceStart]) * (255d / ushort.MaxValue));
            }

            byte ReadByteFromFloat(byte[] source, int sourceStart)
            {
                return (byte)(BitConverter.ToSingle(source, sourceStart) * 255f);
            }

            /************************************/

            ushort ReadUShortFromByte(byte[] source, int sourceStart)
            {
                return source[sourceStart];
            }

            ushort ReadUShortFromUShort(byte[] source, int sourceStart)
            {
                return BitConverter.ToUInt16(source, sourceStart);
            }

            ushort ReadUShortFromFloat(byte[] source, int sourceStart)
            {
                return (ushort)(BitConverter.ToSingle(source, sourceStart) * ushort.MaxValue);
            }

            /************************************/

            float ReadFloatFromByte(byte[] source, int sourceStart)
            {
                return source[sourceStart] / 255f;
            }

            float ReadFloatFromUShort(byte[] source, int sourceStart)
            {
                return BitConverter.ToUInt16(source, sourceStart) / (ushort.MaxValue * 1f);
            }

            float ReadFloatFromFloat(byte[] source, int sourceStart)
            {
                return BitConverter.ToSingle(source, sourceStart);
            }
            #endregion Bit Conversions

            #region Max Value Writers
            void WriteByteMax(byte[] source, int sourceStart)
            {
                source[sourceStart] = 255;
            }

            void WriteUShortMax(byte[] source, int sourceStart)
            {
                source[sourceStart] = 255;
                source[sourceStart + 1] = 255;
            }

            void WriteFloatMax(byte[] source, int sourceStart)
            {
                source[sourceStart] = 0;
                source[sourceStart + 1] = 0;
                source[sourceStart + 2] = 63;
                source[sourceStart + 3] = 127;
            }
            #endregion Max Value Writers

            #region Readers to arrays
            byte[] ReadUShortFromByteAsArray(byte[] source, int sourceStart)
            {
                return new byte[] { 0, source[sourceStart] };
            }

            byte[] ReadUShortFromUShortAsArray(byte[] source, int sourceStart)
            {
                return new byte[] { source[sourceStart], source[sourceStart + 1] };
            }

            byte[] ReadUShortFromFloatAsArray(byte[] source, int sourceStart)
            {
                return BitConverter.GetBytes(ReadUShortFromFloat(source, sourceStart));
            }

            /**/

            byte[] ReadFloatFromByteOrUShortAsArray(byte[] source, int sourceStart)
            {
                return BitConverter.GetBytes(ReadFloat(source, sourceStart));
            }
            byte[] ReadFloatFromFloatAsArray(byte[] source, int sourceStart)
            {
                return new byte[] { source[sourceStart], source[sourceStart + 1], source[sourceStart + 2], source[sourceStart + 3] };
            }
            #endregion Readers to arrays

            #region Writers
            void WriteByte(byte[] source, int sourceStart, ImageEngineFormatDetails sourceFormatDetails, byte[] destination, int destStart)
            {
                destination[destStart] = sourceFormatDetails.ReadByte(source, sourceStart);
            }

            void WriteUShort(byte[] source, int sourceStart, ImageEngineFormatDetails sourceFormatDetails, byte[] destination, int destStart)
            {
                byte[] bytes = sourceFormatDetails.ReadUShortAsArray(source, sourceStart);
                destination[destStart] = bytes[0];
                destination[destStart + 1] = bytes[1];
            }

            void WriteFloat(byte[] source, int sourceStart, ImageEngineFormatDetails sourceFormatDetails, byte[] destination, int destStart)
            {
                byte[] bytes = sourceFormatDetails.ReadFloatAsArray(source, sourceStart);
                destination[destStart] = bytes[0];
                destination[destStart + 1] = bytes[1];
                destination[destStart + 2] = bytes[2];
                destination[destStart + 3] = bytes[3];
            }
            #endregion Writers
        }
    }
}
