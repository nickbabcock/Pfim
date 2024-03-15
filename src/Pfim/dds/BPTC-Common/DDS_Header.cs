using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pfim.dds.bptc.common
{
    /// <summary>
    /// Contains header information about a DDS File.
    /// </summary>
    public class DDS_Header
    {
        const int MaxHeaderSize = ImageFormats.DDS_DX10_HEADER_LENGTH;

        /// <summary>
        /// Characters beginning a file that indicate file is a DDS image.
        /// </summary>
        public const string Identifier = "DDS ";

        #region Standard Enums and structs
        /// <summary>
        /// Contains information about DDS Pixel Format.
        /// </summary>
        public struct DDS_PIXELFORMAT
        {
            /// <summary>
            /// Sub-header Size in bytes.
            /// </summary>
            public int dwSize { get; set; }

            /// <summary>
            /// Option flags.
            /// </summary>
            public DDS_PFdwFlags dwFlags { get; set; }

            /// <summary>
            /// FourCC of DDS, i.e. DXT1, etc
            /// </summary>
            public FourCC dwFourCC { get; set; }

            /// <summary>
            /// RGB Channel width.
            /// </summary>
            public int dwRGBBitCount { get; set; }

            /// <summary>
            /// Red bit mask. i.e. pixel is FF12AA22, so mask might be FF000000 and we get pure red.
            /// </summary>
            public uint dwRBitMask { get; set; }

            /// <summary>
            /// Green bit mask. i.e. pixel is FF12AA22, so mask might be 00FF0000 and we get 00120000.
            /// </summary>
            public uint dwGBitMask { get; set; }

            /// <summary>
            /// Blue bit mask. i.e. pixel is FF12AA22, so mask might be 0000FF00 and we get 0000AA00.
            /// </summary>
            public uint dwBBitMask { get; set; }

            /// <summary>
            /// Alpha bit mask. i.e. pixel is FF12AA22, so mask might be 000000FF and we get 00000022.
            /// </summary>
            public uint dwABitMask { get; set; }

            /// <summary>
            /// Fill PixelFormat from full DDS header
            /// </summary>
            /// <param name="temp">Full DDS header block.</param>
            public DDS_PIXELFORMAT(byte[] temp)
            {
                dwSize = BitConverter.ToInt32(temp, 76);
                dwFlags = (DDS_PFdwFlags)BitConverter.ToInt32(temp, 80);
                dwFourCC = (FourCC)BitConverter.ToInt32(temp, 84);
                dwRGBBitCount = BitConverter.ToInt32(temp, 88);
                dwRBitMask = BitConverter.ToUInt32(temp, 92);
                dwGBitMask = BitConverter.ToUInt32(temp, 96);
                dwBBitMask = BitConverter.ToUInt32(temp, 100);
                dwABitMask = BitConverter.ToUInt32(temp, 104);

                bool noMasks = dwABitMask == 0 && dwBBitMask == 0 && dwGBitMask == 0 && dwRBitMask == 0;

                // Component Size
                switch (dwFourCC)
                {
                    case FourCC.A16B16G16R16:
                        if (noMasks)
                        {
                            dwABitMask = 4;
                            dwBBitMask = 3;
                            dwGBitMask = 2;
                            dwRBitMask = 1;
                            dwRGBBitCount = 16 * 4;
                        }
                        break;
                    case FourCC.A32B32G32R32F:
                        if (noMasks)
                        {
                            dwABitMask = 4;
                            dwBBitMask = 3;
                            dwGBitMask = 2;
                            dwRBitMask = 1;
                            dwRGBBitCount = 32 * 4;
                        }
                        break;

                    // Others I know, but they aren't supported for now. Too uncommon and too much work to add for fun.
                }
            }
            
            /// <summary>
            /// Build PixelFormat sub-header for a specified surface format.
            /// </summary>
            /// <param name="surfaceFormat">Format to base PixelHeader on.</param>
            public DDS_PIXELFORMAT(ImageEngineFormat surfaceFormat) : this()
            {
                dwSize = 32;
                dwFourCC = ParseFormatToFourCC(surfaceFormat);

                if (dwFourCC != FourCC.Unknown)
                    dwFlags = DDS_PFdwFlags.DDPF_FOURCC;

                switch (surfaceFormat)
                {
                    // Compressed formats don't need anything written here since pitch/linear size is unreliable. Why bother?
                    #region Uncompressed
                    case ImageEngineFormat.DDS_G8_L8:
                        dwFlags |= DDS_PFdwFlags.DDPF_LUMINANCE;
                        dwRGBBitCount = 8;
                        dwRBitMask = 0xFF;
                        break;
                    case ImageEngineFormat.DDS_ARGB_8:
                        dwFlags |= DDS_PFdwFlags.DDPF_ALPHAPIXELS | DDS_PFdwFlags.DDPF_RGB;
                        dwRGBBitCount = 32;
                        dwABitMask = 0xFF000000;
                        dwRBitMask = 0x00FF0000;
                        dwGBitMask = 0x0000FF00;
                        dwBBitMask = 0x000000FF;
                        break;
                    case ImageEngineFormat.DDS_ARGB_4:
                        dwFlags |= DDS_PFdwFlags.DDPF_ALPHAPIXELS | DDS_PFdwFlags.DDPF_RGB;
                        dwRGBBitCount = 24;
                        dwABitMask = 0xF000;
                        dwRBitMask = 0x0F00;
                        dwGBitMask = 0x00F0;
                        dwBBitMask = 0x000F;
                        break;
                    case ImageEngineFormat.DDS_V8U8:
                        dwFlags |= DDS_PFdwFlags.DDPF_SIGNED;
                        dwRGBBitCount = 16;
                        dwRBitMask = 0x00FF;
                        dwGBitMask = 0xFF00;
                        break;
                    case ImageEngineFormat.DDS_A8L8:
                        dwFlags |= DDS_PFdwFlags.DDPF_LUMINANCE | DDS_PFdwFlags.DDPF_ALPHAPIXELS;
                        dwRGBBitCount = 16;
                        dwABitMask = 0xFF00;
                        dwRBitMask = 0x00FF;
                        break;
                    case ImageEngineFormat.DDS_RGB_8:
                        dwFlags |= DDS_PFdwFlags.DDPF_RGB;
                        dwRBitMask = 0xFF0000;
                        dwGBitMask = 0x00FF00;
                        dwBBitMask = 0x0000FF;
                        dwRGBBitCount = 24;
                        break;
                    case ImageEngineFormat.DDS_G16_R16:
                        dwFlags |= DDS_PFdwFlags.DDPF_RGB;
                        dwGBitMask = 0xFFFF0000;
                        dwRBitMask = 0x0000FFFF;
                        dwRGBBitCount = 32;
                        break;
                    case ImageEngineFormat.DDS_ABGR_8:
                        dwFlags |= DDS_PFdwFlags.DDPF_ALPHAPIXELS | DDS_PFdwFlags.DDPF_RGB;
                        dwRGBBitCount = 32;
                        dwABitMask = 0xFF000000;
                        dwBBitMask = 0x00FF0000;
                        dwGBitMask = 0x0000FF00;
                        dwRBitMask = 0x000000FF;
                        break;
                    case ImageEngineFormat.DDS_ARGB_32F:
                        dwFlags |= DDS_PFdwFlags.DDPF_ALPHAPIXELS | DDS_PFdwFlags.DDPF_RGB;
                        dwFourCC = FourCC.DX10;
                        dwRGBBitCount = 128;
                        dwABitMask = 0;
                        dwRBitMask = 0;
                        dwGBitMask = 0;
                        dwBBitMask = 0;
                        break;
                    #endregion Uncompressed
                }
            }


            /// <summary>
            /// String representation of DDS pixel format.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--DDS_PIXELFORMAT--")
                .AppendLine($"dwSize: {dwSize}")
                .AppendLine($"dwFlags: {dwFlags}")
                .AppendLine($"dwFourCC: {dwFourCC}")
                .AppendLine($"dwRGBBitCount: {dwRGBBitCount}")
                .AppendLine($"dwRBitMask: 0x{dwRBitMask.ToString("X")}")  // As Hex
                .AppendLine($"dwGBitMask: 0x{dwGBitMask.ToString("X")}")  // As Hex
                .AppendLine($"dwBBitMask: 0x{dwBBitMask.ToString("X")}")  // As Hex
                .AppendLine($"dwABitMask: 0x{dwABitMask.ToString("X")}")  // As Hex
                .AppendLine("--END DDS_PIXELFORMAT--");
                return Environment.NewLine + sb.ToString() + Environment.NewLine;
            }
        }

        /// <summary>
        /// Old method of identifying Compressed textures.
        /// DX10 indicates new texture, the DX10 Additional header will contain the true format. See <see cref="DXGI_FORMAT"/>.
        /// </summary>
        public enum FourCC
        {
            /// <summary>
            /// Used when FourCC is unknown.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// (BC1) Block Compressed Texture. Compresses 4x4 texels.
            /// Used for Simple Non Alpha.
            /// </summary>
            DXT1 = 0x31545844,  // 1TXD i.e. DXT1 backwards

            /// <summary>
            /// (BC2) Block Compressed Texture. Compresses 4x4 texels.
            /// Used for Sharp Alpha. Premultiplied alpha. 
            /// </summary>
            DXT2 = 0x32545844,

            /// <summary>
            /// (BC2) Block Compressed Texture. Compresses 4x4 texels.
            /// Used for Sharp Alpha. 
            /// </summary>
            DXT3 = 0x33545844,

            /// <summary>
            /// (BC3) Block Compressed Texture. Compresses 4x4 texels.
            /// Used for Gradient Alpha. Premultiplied alpha.
            /// </summary>
            DXT4 = 0x34545844,

            /// <summary>
            /// (BC3) Block Compressed Texture. Compresses 4x4 texels.
            /// Used for Gradient Alpha. 
            /// </summary>
            DXT5 = 0x35545844,

            /// <summary>
            /// Fancy new DirectX 10+ format indicator. DX10 Header will contain true format.
            /// </summary>
            DX10 = 0x30315844,

            /// <summary>
            /// (BC4) Block Compressed Texture. Compresses 4x4 texels.
            /// Used for Normal (bump) Maps. 8 bit single channel with alpha.
            /// </summary>
            ATI1 = 0x31495441,

            /// <summary>
            /// (BC5) Block Compressed Texture. Compresses 4x4 texels.
            /// Used for Normal (bump) Maps. Pair of 8 bit channels.
            /// </summary>
            ATI2N_3Dc = 0x32495441,

            R8G8B8 = 20,
            A8R8G8B8,
            X8R8G8B8,
            R5G6B5,
            X1R5G5B5,
            A1R5G5B5,
            A4R4G4B4,
            R3G3B2,
            A8,
            A8R3G3B2,
            X4R4G4B4,
            A2B10G10R10,
            A8B8G8R8,
            X8B8G8R8,
            G16R16,
            A2R10G10B10,
            A16B16G16R16,

            A8P8 = 40,
            P8,

            L8 = 50,
            A8L8,
            A4L4,

            V8U8 = 60,
            L6V5U5,
            X8L8V8U8,
            Q8W8V8U8,
            V16U16,
            A2W10V10U10,

            UYVY = 0x59565955,
            R8G8_B8G8 = 0x47424752,
            YUY2 = 0x32595559,
            G8R8_G8B8 = 0x42475247,

            D16_LOCKABLE = 70,
            D32,
            D15S1,
            D24S8,
            D24X8,
            D24X4S4,
            D16,

            D32F_LOCKABLE = 82,
            D24FS8,

            L16 = 81,

            Q16Q16V16U16 = 110,
            R16F,
            G16R16F,
            A16B16G16R16F,
            R32F,
            G32R32F,
            A32B32G32R32F,
            CxV8U8,
        }

        /// <summary>
        /// Option flags. Indicate certain properties of DDS, such as mipmapping and dimensions.
        /// </summary>
        [Flags]
        public enum DDSdwFlags
        {
            /// <summary>
            /// Required.
            /// </summary>
            DDSD_CAPS = 0x1,            

            /// <summary>
            /// Required.
            /// </summary>
            DDSD_HEIGHT = 0x2,          

            /// <summary>
            /// Required.
            /// </summary>
            DDSD_WIDTH = 0x4,           

            /// <summary>
            /// Required when Pitch is specified for uncompressed textures.
            /// </summary>
            DDSD_PITCH = 0x8,           

            /// <summary>
            /// Required.
            /// </summary>
            DDSD_PIXELFORMAT = 0x1000,  

            /// <summary>
            /// Required if texture contains mipmaps.
            /// </summary>
            DDSD_MIPMAPCOUNT = 0x20000, 

            /// <summary>
            /// Required when pitch/linear size is specified for compressed textures.
            /// </summary>
            DDSD_LINEARSIZE = 0x80000,  

            /// <summary>
            /// Required for Depth/Volume textures.
            /// </summary>
            DDSD_DEPTH = 0x800000    
        }

        /// <summary>
        /// More option flags, but mostly irrelevant.
        /// </summary>
        [Flags]
        public enum DDSdwCaps
        {
            /// <summary>
            /// Must be specified on image that has more than one surface. (mipmap, cube, volume)
            /// </summary>
            DDSCAPS_COMPLEX = 0x8,

            /// <summary>
            /// Should be set for mipmapped image
            /// </summary> 
            DDSCAPS_MIPMAP = 0x400000, 

            /// <summary>
            /// Required.
            /// </summary>
            DDSCAPS_TEXTURE = 0x1000
        }

        /// <summary>
        /// Denotes PixelFormat flags. Settings that indicate how pixels are shown.
        /// </summary>
        [Flags]
        public enum DDS_PFdwFlags
        {
            /// <summary>
            /// Texture contains alpha. i.e. dwRGBAlphaBitmapMask contains valid data.
            /// </summary>
            DDPF_ALPHAPIXELS = 0x1,     // Texture has alpha - dwRGBAlphaBitMask has a value

            /// <summary>
            /// Used in some older files for alpha channel only uncompressed data. i.e. dwRGBBitCount contains alpha channel bitcount, dwABitMask contains valid data.
            /// </summary>
            DDPF_ALPHA = 0x2,

            /// <summary>
            /// Contains compressed RGB. dwFourCC has a value
            /// </summary>       
            DDPF_FOURCC = 0x4,

            /// <summary>
            /// Contains uncompressed RGB. dwRGBBitCount and RGB bitmasks have a value
            /// </summary>
            DDPF_RGB = 0x40,            

            /// <summary>
            /// Used in some old files for YUV uncompressed data. i.e. dwRGBBitCount contains YUV bitcount, dwRBitMask contains Y mask, dwGBitMask contains U mask, dwBBitMask contains V mask.
            /// YUV is a weird colourspace. Y = intensity, UV = colour. Y = 0-1 (0-255), U,V = -0.5-0.5 (-128-127) or 0-255.
            /// </summary>
            DDPF_YUV = 0x200,           

            /// <summary>
            /// Old flag for single channel colour uncompressed. dwRGBBitCount contains luminescence channel bit count, dwRBitMask contains channel mask. Can combine with DDPF_ALPHAPIXELS for 2 channel DDS file.
            /// </summary>
            DDPF_LUMINANCE = 0x20000,    // Older flag for single channel uncompressed data

            /// <summary>
            /// Undocumented flag that seems to indicate that format is signed. EDIT Seems to be a nVidia thing as specified in the NVTT solution: https://github.com/castano/nvidia-texture-tools
            /// </summary>
            DDPF_SIGNED = 0x80000,
        }
        #endregion Standard Enums and Structs

        #region DXGI/DX10
        /// <summary>
        /// Additional header used by DXGI/DX10 DDS'.
        /// </summary>
        public struct DDS_DXGI_DX10_Additional
        {
            /// <summary>
            /// Surface format.
            /// </summary>
            public DXGI_FORMAT dxgiFormat;

            /// <summary>
            /// Dimension of texture (1D, 2D, 3D)
            /// </summary>
            public D3D10_RESOURCE_DIMENSION resourceDimension;

            public enum D3D10_RESOURCE_MISC_FLAGS
            {
                D3D10_RESOURCE_MISC_GENERATE_MIPS = 0x1,
                D3D10_RESOURCE_MISC_SHARED = 0x2,
                D3D10_RESOURCE_MISC_TEXTURECUBE = 0x4,
                D3D10_RESOURCE_MISC_SHARED_KEYEDMUTEX = 0x10,
                D3D10_RESOURCE_MISC_GDI_COMPATIBLE = 0x20
            }

            /// <summary>
            /// Identifies less common options. e.g. 0x4 = DDS_RESOURCE_MISC_TEXTURECUBE
            /// </summary>
            public D3D10_RESOURCE_MISC_FLAGS miscFlag;

            /// <summary>
            /// Number of elements in array.
            /// For 2D textures that are cube maps, it's the number of cubes. Can also be random images made with texassemble.exe.
            /// For 3D textures, must be 1.
            /// </summary>
            public uint arraySize;

            /// <summary>
            /// Alpha flags.
            /// </summary>
            public DXGI_MiscFlags miscFlags2;

            /// <summary>
            /// Read DX10-DXGI header from full DDS header block.
            /// </summary>
            /// <param name="fullHeaderBlock">Entire DDS header block.</param>
            /// <param name="offset">Offset at which this header starts in full block.</param>
            public DDS_DXGI_DX10_Additional(byte[] fullHeaderBlock, int offset = 128)
            {
                dxgiFormat = (DXGI_FORMAT)BitConverter.ToInt32(fullHeaderBlock, offset);
                resourceDimension = (D3D10_RESOURCE_DIMENSION)BitConverter.ToInt32(fullHeaderBlock, offset + 4);
                miscFlag = (D3D10_RESOURCE_MISC_FLAGS)BitConverter.ToUInt32(fullHeaderBlock, offset + 8);
                arraySize = BitConverter.ToUInt32(fullHeaderBlock, offset + 12);
                miscFlags2 = (DXGI_MiscFlags)BitConverter.ToUInt32(fullHeaderBlock, offset + 16);
            }

            /// <summary>
            /// Shows string description of additional DX10 header.
            /// </summary>
            /// <returns>String header.</returns>
            public override string ToString()
            {
                return Util.StringifyObject(this, true);
            }
        }

        /// <summary>
        /// Option flags to indicate alpha mode.
        /// </summary>
        public enum DXGI_MiscFlags
        {
            /// <summary>
            /// Alpha content unknown. Default for legacy files, assumed to be "Straight".
            /// </summary>
            DDS_ALPHA_MODE_UNKNOWN = 0,

            /// <summary>
            /// Any alpha is "straight". Standard. i.e. RGBA all separate channels. 
            /// </summary>
            DDS_ALPHA_MODE_STRAIGHT = 1,

            /// <summary>
            /// Any alpha channels are premultiplied i.e. RGB (without A) with premultiplied alpha has values that include alpha by way of multiplying the original RGB with the A.
            /// </summary>
            DDS_ALPHA_MODE_PREMULTIPLIED = 2,

            /// <summary>
            /// Alpha is fully opaque.
            /// </summary>
            DDS_ALPHA_MODE_OPAQUE = 3,

            /// <summary>
            /// Alpha channel isn't for transparency.
            /// </summary>
            DDS_ALPHA_MODE_CUSTOM = 4,
        }

        

        /// <summary>
        /// Indicates type of DXGI/DX10 texture.
        /// </summary>
        public enum D3D10_RESOURCE_DIMENSION
        {
            // TODO: Incomplete?

            /// <summary>
            /// 1D Texture specified by dwWidth of DDS_Header = size of texture. Typically, dwHeight = 1 and DDSD_HEIGHT flag is also set in dwFlags.
            /// </summary>
            DDS_DIMENSION_TEXTURE1D = 2,

            /// <summary>
            /// 2D Texture specified by dwWidth and dwHeight of DDS_Header. Can be cube map if miscFlag and arraySize members are set.
            /// </summary>
            DDS_DIMENSION_TEXTURE2D = 3,

            /// <summary>
            /// 3D Texture specified by dwWidth, dwHeight, and dwDepth. Must have DDSD_DEPTH Flag set in dwFlags.
            /// </summary> 
            DDS_DIMENSION_TEXTURE3D = 4, 
        }

        /// <summary>
        /// DXGI/DX10 formats.
        /// </summary>
        public enum DXGI_FORMAT : uint
        {
            DXGI_FORMAT_UNKNOWN = 0,
            DXGI_FORMAT_R32G32B32A32_TYPELESS = 1,
            DXGI_FORMAT_R32G32B32A32_FLOAT = 2,
            DXGI_FORMAT_R32G32B32A32_UINT = 3,
            DXGI_FORMAT_R32G32B32A32_SINT = 4,
            DXGI_FORMAT_R32G32B32_TYPELESS = 5,
            DXGI_FORMAT_R32G32B32_FLOAT = 6,
            DXGI_FORMAT_R32G32B32_UINT = 7,
            DXGI_FORMAT_R32G32B32_SINT = 8,
            DXGI_FORMAT_R16G16B16A16_TYPELESS = 9,
            DXGI_FORMAT_R16G16B16A16_FLOAT = 10,
            DXGI_FORMAT_R16G16B16A16_UNORM = 11,
            DXGI_FORMAT_R16G16B16A16_UINT = 12,
            DXGI_FORMAT_R16G16B16A16_SNORM = 13,
            DXGI_FORMAT_R16G16B16A16_SINT = 14,
            DXGI_FORMAT_R32G32_TYPELESS = 15,
            DXGI_FORMAT_R32G32_FLOAT = 16,
            DXGI_FORMAT_R32G32_UINT = 17,
            DXGI_FORMAT_R32G32_SINT = 18,
            DXGI_FORMAT_R32G8X24_TYPELESS = 19,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22,
            DXGI_FORMAT_R10G10B10A2_TYPELESS = 23,
            DXGI_FORMAT_R10G10B10A2_UNORM = 24,
            DXGI_FORMAT_R10G10B10A2_UINT = 25,
            DXGI_FORMAT_R11G11B10_FLOAT = 26,
            DXGI_FORMAT_R8G8B8A8_TYPELESS = 27,
            DXGI_FORMAT_R8G8B8A8_UNORM = 28,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29,
            DXGI_FORMAT_R8G8B8A8_UINT = 30,
            DXGI_FORMAT_R8G8B8A8_SNORM = 31,
            DXGI_FORMAT_R8G8B8A8_SINT = 32,
            DXGI_FORMAT_R16G16_TYPELESS = 33,
            DXGI_FORMAT_R16G16_FLOAT = 34,
            DXGI_FORMAT_R16G16_UNORM = 35,
            DXGI_FORMAT_R16G16_UINT = 36,
            DXGI_FORMAT_R16G16_SNORM = 37,
            DXGI_FORMAT_R16G16_SINT = 38,
            DXGI_FORMAT_R32_TYPELESS = 39,
            DXGI_FORMAT_D32_FLOAT = 40,
            DXGI_FORMAT_R32_FLOAT = 41,
            DXGI_FORMAT_R32_UINT = 42,
            DXGI_FORMAT_R32_SINT = 43,
            DXGI_FORMAT_R24G8_TYPELESS = 44,
            DXGI_FORMAT_D24_UNORM_S8_UINT = 45,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47,
            DXGI_FORMAT_R8G8_TYPELESS = 48,
            DXGI_FORMAT_R8G8_UNORM = 49,
            DXGI_FORMAT_R8G8_UINT = 50,
            DXGI_FORMAT_R8G8_SNORM = 51,
            DXGI_FORMAT_R8G8_SINT = 52,
            DXGI_FORMAT_R16_TYPELESS = 53,
            DXGI_FORMAT_R16_FLOAT = 54,
            DXGI_FORMAT_D16_UNORM = 55,
            DXGI_FORMAT_R16_UNORM = 56,
            DXGI_FORMAT_R16_UINT = 57,
            DXGI_FORMAT_R16_SNORM = 58,
            DXGI_FORMAT_R16_SINT = 59,
            DXGI_FORMAT_R8_TYPELESS = 60,
            DXGI_FORMAT_R8_UNORM = 61,
            DXGI_FORMAT_R8_UINT = 62,
            DXGI_FORMAT_R8_SNORM = 63,
            DXGI_FORMAT_R8_SINT = 64,
            DXGI_FORMAT_A8_UNORM = 65,
            DXGI_FORMAT_R1_UNORM = 66,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67,
            DXGI_FORMAT_R8G8_B8G8_UNORM = 68,
            DXGI_FORMAT_G8R8_G8B8_UNORM = 69,
            DXGI_FORMAT_BC1_TYPELESS = 70,
            DXGI_FORMAT_BC1_UNORM = 71,
            DXGI_FORMAT_BC1_UNORM_SRGB = 72,
            DXGI_FORMAT_BC2_TYPELESS = 73,
            DXGI_FORMAT_BC2_UNORM = 74,
            DXGI_FORMAT_BC2_UNORM_SRGB = 75,
            DXGI_FORMAT_BC3_TYPELESS = 76,
            DXGI_FORMAT_BC3_UNORM = 77,
            DXGI_FORMAT_BC3_UNORM_SRGB = 78,
            DXGI_FORMAT_BC4_TYPELESS = 79,
            DXGI_FORMAT_BC4_UNORM = 80,
            DXGI_FORMAT_BC4_SNORM = 81,
            DXGI_FORMAT_BC5_TYPELESS = 82,
            DXGI_FORMAT_BC5_UNORM = 83,
            DXGI_FORMAT_BC5_SNORM = 84,
            DXGI_FORMAT_B5G6R5_UNORM = 85,
            DXGI_FORMAT_B5G5R5A1_UNORM = 86,
            DXGI_FORMAT_B8G8R8A8_UNORM = 87,
            DXGI_FORMAT_B8G8R8X8_UNORM = 88,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89,
            DXGI_FORMAT_B8G8R8A8_TYPELESS = 90,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91,
            DXGI_FORMAT_B8G8R8X8_TYPELESS = 92,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93,
            DXGI_FORMAT_BC6H_TYPELESS = 94,
            DXGI_FORMAT_BC6H_UF16 = 95,
            DXGI_FORMAT_BC6H_SF16 = 96,
            DXGI_FORMAT_BC7_TYPELESS = 97,
            DXGI_FORMAT_BC7_UNORM = 98,
            DXGI_FORMAT_BC7_UNORM_SRGB = 99,
            DXGI_FORMAT_AYUV = 100,
            DXGI_FORMAT_Y410 = 101,
            DXGI_FORMAT_Y416 = 102,
            DXGI_FORMAT_NV12 = 103,
            DXGI_FORMAT_P010 = 104,
            DXGI_FORMAT_P016 = 105,
            DXGI_FORMAT_420_OPAQUE = 106,
            DXGI_FORMAT_YUY2 = 107,
            DXGI_FORMAT_Y210 = 108,
            DXGI_FORMAT_Y216 = 109,
            DXGI_FORMAT_NV11 = 110,
            DXGI_FORMAT_AI44 = 111,
            DXGI_FORMAT_IA44 = 112,
            DXGI_FORMAT_P8 = 113,
            DXGI_FORMAT_A8P8 = 114,
            DXGI_FORMAT_B4G4R4A4_UNORM = 115,
            DXGI_FORMAT_P208 = 130,
            DXGI_FORMAT_V208 = 131,
            DXGI_FORMAT_V408 = 132,
            DXGI_FORMAT_FORCE_UINT = 0xffffffff,
        }


        public enum BC7DecodeMode
        {
            /// <summary>
            /// RGB only. 3 subsets per block. RGBP 4.4.4.1 endpoints with a unique P-bit per endpoint. 
            /// 3 bit indicies.
            /// 16 bit partitions.
            /// </summary>
            Mode_0,

            /// <summary>
            /// 
            /// </summary>
            Mode_1,
            Mode_2,
            Mode_3,
            Mode_4,
            Mode_5, 
            Mode_6,
            Mode_7
        }

        #endregion DXGI/DX10

        #region Properties

        /// <summary>
        /// Width of image.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of image.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Size of header in bytes. Must be 124.
        /// </summary>
        public int dwSize { get; set; }

        /// <summary>
        /// Option flags.
        /// </summary>
        public DDSdwFlags dwFlags { get; set; }

        /// <summary>
        /// Pitch or linear size. I think this is stride in Windows lingo?
        /// </summary>
        public int dwPitchOrLinearSize { get; set; }

        /// <summary>
        /// Image depth. Usually not used.
        /// </summary>
        public int dwDepth { get; set; }

        /// <summary>
        /// Number of mipmaps.
        /// </summary>
        public int dwMipMapCount { get; set; }

        /// <summary>
        /// Not used, as per Windows DDS spec.
        /// </summary>
        public int[] dwReserved1 = new int[11];

        /// <summary>
        /// Pixel format of DDS.
        /// </summary>
        public DDS_PIXELFORMAT ddspf { get; set; }

        /// <summary>
        /// More option flags.
        /// </summary>
        public DDSdwCaps dwCaps { get; set; }

        /// <summary>
        /// Don't think it's used.
        /// </summary>
        public int dwCaps2;

        /// <summary>
        /// Don't think it's used.
        /// </summary>
        public int dwCaps3;

        /// <summary>
        /// Don't think it's used.
        /// </summary>
        public int dwCaps4;

        /// <summary>
        /// Not used as per Windows DDS spec.
        /// </summary>
        public int dwReserved2;
        #endregion Properties

        /// <summary>
        /// Additional header for newer DX10 images.
        /// </summary>
        public DDS_DXGI_DX10_Additional DX10_DXGI_AdditionalHeader { get; private set; }

        ImageEngineFormat format = ImageEngineFormat.Unknown;
        /// <summary>
        /// Surface format of DDS.
        /// e.g. DXT1, V8U8, etc
        /// </summary>
        public ImageEngineFormat Format
        {
            get
            {
                if (format == ImageEngineFormat.Unknown)
                    if (ddspf.dwFourCC == FourCC.DX10)
                        format = ImageEngineFormat.DDS_DX10;

                format = DetermineDDSSurfaceFormat(ddspf);
                return format;
            }
        }

        string DDS_FlagStringify(Type enumType)
        {
            string flags = "";

            string[] names = Enum.GetNames(enumType);
            int[] values = (int[])Enum.GetValues(enumType);
            for (int i = 0; i < names.Length; i++)
            {
                if (((int)dwFlags & values[i]) != 0)
                    flags += $"[{names[i]}] ";
            }
            
            return flags;
        }

        /// <summary>
        /// Reads DDS header from stream.
        /// </summary>
        /// <param name="stream">Fully formatted DDS image.</param>
        /// <returns>Header length.</returns>
        protected long Load(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var temp = stream.ReadBytes(MaxHeaderSize);

            if (!CheckIdentifier(temp))
                throw new FormatException("Stream is not a recognised DDS image.");

            // Start header
            dwSize = BitConverter.ToInt32(temp, 4);
            dwFlags = (DDSdwFlags)BitConverter.ToInt32(temp, 8);
            Height = BitConverter.ToInt32(temp, 12);
            Width = BitConverter.ToInt32(temp, 16);
            dwPitchOrLinearSize = BitConverter.ToInt32(temp, 20);
            dwDepth = BitConverter.ToInt32(temp, 24);
            dwMipMapCount = BitConverter.ToInt32(temp, 28);
            for (int i = 0; i < 11; ++i)
                dwReserved1[i] = BitConverter.ToInt32(temp, 32 + (i * 4));

            // DDS PixelFormat
            ddspf = new DDS_PIXELFORMAT(temp);

            dwCaps = (DDSdwCaps)BitConverter.ToInt32(temp, 108);
            dwCaps2 = BitConverter.ToInt32(temp, 112);
            dwCaps3 = BitConverter.ToInt32(temp, 116);
            dwCaps4 = BitConverter.ToInt32(temp, 120);
            dwReserved2 = BitConverter.ToInt32(temp, 124);

            // DX10 Additional header
            if (ddspf.dwFourCC == FourCC.DX10)
                DX10_DXGI_AdditionalHeader = new DDS_DXGI_DX10_Additional(temp);

            return MaxHeaderSize;
        }

        /// <summary>
        /// Read Header from DDS Image.
        /// </summary>
        /// <param name="stream">Fully formatted DDS image.</param>
        public DDS_Header(Stream stream)
        {
            Load(stream);
        }

        /// <summary>
        /// Creates a DDS header from a set of information.
        /// </summary>
        /// <param name="Mips">Number of mipmaps.</param>
        /// <param name="Height">Height of top mipmap.</param>
        /// <param name="Width">Width of top mipmap.</param>
        /// <param name="surfaceformat">Format header represents.</param>
        public DDS_Header(int Mips, int Height, int Width, ImageEngineFormat surfaceformat, DXGI_FORMAT dx10Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN)
        {
            dwSize = 124;
            dwFlags = DDSdwFlags.DDSD_CAPS | DDSdwFlags.DDSD_HEIGHT | DDSdwFlags.DDSD_WIDTH | DDSdwFlags.DDSD_PIXELFORMAT | (Mips != 1 ? DDSdwFlags.DDSD_MIPMAPCOUNT : 0);
            this.Width = Width;
            this.Height = Height;
            dwCaps = DDSdwCaps.DDSCAPS_TEXTURE | (Mips == 1 ? 0 : DDSdwCaps.DDSCAPS_COMPLEX | DDSdwCaps.DDSCAPS_MIPMAP);
            dwMipMapCount = Mips == 1 ? 1 : Mips;
            ddspf = new DDS_PIXELFORMAT(surfaceformat);

            if (surfaceformat == ImageEngineFormat.DDS_DX10 || surfaceformat == ImageEngineFormat.DDS_ARGB_32F)
            {
                if (surfaceformat == ImageEngineFormat.DDS_ARGB_32F)
                    dx10Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT;

                DX10_DXGI_AdditionalHeader = new DDS_DXGI_DX10_Additional
                {
                    dxgiFormat = dx10Format,
                    resourceDimension = D3D10_RESOURCE_DIMENSION.DDS_DIMENSION_TEXTURE2D,
                    miscFlag = DDS_DXGI_DX10_Additional.D3D10_RESOURCE_MISC_FLAGS.D3D10_RESOURCE_MISC_GENERATE_MIPS,
                    miscFlags2 = DXGI_MiscFlags.DDS_ALPHA_MODE_UNKNOWN,
                    arraySize = 1
                };
            }
                
        }
        

        /// <summary>
        /// Determines friendly format from FourCC, with additional DXGI/DX10 format.
        /// </summary>
        /// <param name="fourCC">FourCC of DDS (DXT1-5)</param>
        /// <param name="additionalDX10"></param>
        /// <returns>Friendly format.</returns>
        static ImageEngineFormat ParseFourCC(FourCC fourCC, DXGI_FORMAT additionalDX10 = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN)
        {
            if (fourCC == FourCC.DX10)
                return ImageEngineFormat.DDS_DX10; // TODO: Need to add these at some point.

            if (Enum.IsDefined(typeof(ImageEngineFormat), (int)fourCC))
                return (ImageEngineFormat)fourCC;
            else
                return ImageEngineFormat.Unknown;
        }

        static FourCC ParseFormatToFourCC(ImageEngineFormat format)
        {
            if (Enum.IsDefined(typeof(FourCC), (int)format))
                return (FourCC)format;
            else
                return FourCC.Unknown;
        }

        /// <summary>
        /// Determines DDS Surface Format given the header.
        /// </summary>
        /// <param name="ddspf">DDS PixelFormat structure.</param>
        /// <returns>Friendly format.</returns>
        public static ImageEngineFormat DetermineDDSSurfaceFormat(DDS_Header.DDS_PIXELFORMAT ddspf)
        {
            ImageEngineFormat format = ParseFourCC(ddspf.dwFourCC);

            if (format == ImageEngineFormat.Unknown)
            {
                // Due to some previous settings, need to check these first.
                if (ddspf.dwABitMask <= 4 && ddspf.dwABitMask != 0 &&
                    ddspf.dwBBitMask <= 4 && ddspf.dwBBitMask != 0 &&
                    ddspf.dwGBitMask <= 4 && ddspf.dwGBitMask != 0 &&
                    ddspf.dwRBitMask <= 4 && ddspf.dwRBitMask != 0)
                    format = ImageEngineFormat.DDS_CUSTOM;


                // KFreon: Apparently all these flags mean it's a V8U8 image...
                else if (ddspf.dwRGBBitCount == 16 &&
                           ddspf.dwRBitMask == 0x00FF &&
                           ddspf.dwGBitMask == 0xFF00 &&
                           ddspf.dwBBitMask == 0x00 &&
                           ddspf.dwABitMask == 0x00 &&
                           (ddspf.dwFlags & DDS_PFdwFlags.DDPF_SIGNED) == DDS_PFdwFlags.DDPF_SIGNED)
                    format = ImageEngineFormat.DDS_V8U8;

                // KFreon: Test for L8/G8
                else if (ddspf.dwABitMask == 0 &&
                        ddspf.dwBBitMask == 0 &&
                        ddspf.dwGBitMask == 0 &&
                        ddspf.dwRBitMask == 0xFF &&
                        ddspf.dwFlags == DDS_PFdwFlags.DDPF_LUMINANCE &&
                        ddspf.dwRGBBitCount == 8)
                    format = ImageEngineFormat.DDS_G8_L8;

                // KFreon: A8L8. This can probably be something else as well, but it seems to work for now
                else if (ddspf.dwRGBBitCount == 16 &&
                        ddspf.dwFlags == (DDS_PFdwFlags.DDPF_ALPHAPIXELS | DDS_PFdwFlags.DDPF_LUMINANCE))
                    format = ImageEngineFormat.DDS_A8L8;

                // KFreon: G_R only.
                else if (((ddspf.dwFlags & DDS_PFdwFlags.DDPF_RGB) == DDS_PFdwFlags.DDPF_RGB && !((ddspf.dwFlags & DDS_PFdwFlags.DDPF_ALPHAPIXELS) == DDS_PFdwFlags.DDPF_ALPHAPIXELS)) &&
                        ddspf.dwABitMask == 0 &&
                        ddspf.dwBBitMask == 0 &&
                        ddspf.dwGBitMask != 0 &&
                        ddspf.dwRBitMask != 0)
                    format = ImageEngineFormat.DDS_G16_R16;

                // KFreon: RGB. RGB channels have something in them, but alpha doesn't.
                else if (((ddspf.dwFlags & DDS_PFdwFlags.DDPF_RGB) == DDS_PFdwFlags.DDPF_RGB && !((ddspf.dwFlags & DDS_PFdwFlags.DDPF_ALPHAPIXELS) == DDS_PFdwFlags.DDPF_ALPHAPIXELS)) &&
                        ddspf.dwABitMask == 0 &&
                        ddspf.dwBBitMask != 0 &&
                        ddspf.dwGBitMask != 0 &&
                        ddspf.dwRBitMask != 0)
                {
                    // TODO more formats?
                    if (ddspf.dwBBitMask == 31)
                        format = ImageEngineFormat.DDS_R5G6B5;
                    else
                        format = ImageEngineFormat.DDS_RGB_8;
                }

                // KFreon: RGB and A channels are present.
                else if (((ddspf.dwFlags & (DDS_PFdwFlags.DDPF_RGB | DDS_PFdwFlags.DDPF_ALPHAPIXELS)) == (DDS_PFdwFlags.DDPF_RGB | DDS_PFdwFlags.DDPF_ALPHAPIXELS)) ||
                        ddspf.dwABitMask != 0 &&
                        ddspf.dwBBitMask != 0 &&
                        ddspf.dwGBitMask != 0 &&
                        ddspf.dwRBitMask != 0)
                {
                    // TODO: Some more formats here?
                    format = ImageEngineFormat.DDS_ARGB_8;
                }

                // KFreon: If nothing else fits, but there's data in one of the bitmasks, assume it can be read.
                else if (ddspf.dwABitMask != 0 || ddspf.dwRBitMask != 0 || ddspf.dwGBitMask != 0 || ddspf.dwBBitMask != 0)
                    format = ImageEngineFormat.DDS_CUSTOM;
                else
                    throw new FormatException("DDS Format is unknown.");
            }

            return format;
        }

        internal static bool CheckIdentifier(byte[] IDBlock)
        {
            for (int i = 0; i < Identifier.Length; i++)
                if (IDBlock[i] != Identifier[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Writes header to destination array starting at index.
        /// </summary>
        /// <param name="destination">Array to write header to.</param>
        /// <param name="index">Index in destination to start writing at.</param>
        public void WriteToArray(byte[] destination, int index)
        {
            List<byte> headerData = new List<byte>(150);

            // KFreon: Write magic number ("DDS")
            headerData.AddRange(BitConverter.GetBytes(0x20534444));

            // KFreon: Write all header fields regardless of filled or not
            headerData.AddRange(BitConverter.GetBytes(dwSize));
            headerData.AddRange(BitConverter.GetBytes((int)dwFlags));
            headerData.AddRange(BitConverter.GetBytes(Height));
            headerData.AddRange(BitConverter.GetBytes(Width));
            headerData.AddRange(BitConverter.GetBytes(dwPitchOrLinearSize));
            headerData.AddRange(BitConverter.GetBytes(dwDepth));
            headerData.AddRange(BitConverter.GetBytes(dwMipMapCount));

            // KFreon: Write reserved1
            for (int i = 0; i < 11; i++)
                headerData.AddRange(BitConverter.GetBytes((int)0));

            // KFreon: Write PIXELFORMAT
            headerData.AddRange(BitConverter.GetBytes(ddspf.dwSize));
            headerData.AddRange(BitConverter.GetBytes((int)ddspf.dwFlags));
            headerData.AddRange(BitConverter.GetBytes((int)ddspf.dwFourCC));
            headerData.AddRange(BitConverter.GetBytes(ddspf.dwRGBBitCount));
            headerData.AddRange(BitConverter.GetBytes(ddspf.dwRBitMask));
            headerData.AddRange(BitConverter.GetBytes(ddspf.dwGBitMask));
            headerData.AddRange(BitConverter.GetBytes(ddspf.dwBBitMask));
            headerData.AddRange(BitConverter.GetBytes(ddspf.dwABitMask));


            headerData.AddRange(BitConverter.GetBytes((int)dwCaps));
            headerData.AddRange(BitConverter.GetBytes(dwCaps2));
            headerData.AddRange(BitConverter.GetBytes(dwCaps3));
            headerData.AddRange(BitConverter.GetBytes(dwCaps4));
            headerData.AddRange(BitConverter.GetBytes(dwReserved2));


            if (DX10_DXGI_AdditionalHeader.dxgiFormat != DXGI_FORMAT.DXGI_FORMAT_UNKNOWN)
            {
                headerData.AddRange(BitConverter.GetBytes((int)DX10_DXGI_AdditionalHeader.dxgiFormat));
                headerData.AddRange(BitConverter.GetBytes((int)DX10_DXGI_AdditionalHeader.resourceDimension));
                headerData.AddRange(BitConverter.GetBytes((uint)DX10_DXGI_AdditionalHeader.miscFlag));
                headerData.AddRange(BitConverter.GetBytes(DX10_DXGI_AdditionalHeader.arraySize));
                headerData.AddRange(BitConverter.GetBytes((uint)DX10_DXGI_AdditionalHeader.miscFlags2));
            }

            headerData.CopyTo(destination, index);
        }
    }
}
