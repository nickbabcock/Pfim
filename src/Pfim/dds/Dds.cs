using System;
using System.Collections.Generic;
using System.IO;
using Pfim.dds;
using Pfim.dds.bptc;
using Pfim.dds.bptc.common;

namespace Pfim
{
    /// <summary>
    /// Class that represents direct draw surfaces
    /// </summary>
    public abstract class Dds : IImage
    {
        private readonly PfimConfig _config;

        /// <summary>
        /// Instantiates a direct draw surface image from a header, the data,
        /// and additional info.
        /// </summary>
        protected Dds(DdsHeader header, PfimConfig config)
        {
            Header = header;
            _config = config;
        }

        protected PfimConfig Config => _config;
        public DdsHeader Header { get; }
        public abstract int BitsPerPixel { get; }
        public int BytesPerPixel => BitsPerPixel / 8;
        public virtual int Stride => Util.Stride((int)Header.Width, BitsPerPixel);
        public virtual byte[] Data { get; protected set; }
        public int DataLen { get; protected set; }
        public int Width => (int)Header.Width;
        public int Height => (int)Header.Height;
        public abstract ImageFormat Format { get; }
        public abstract bool Compressed { get; }
        public abstract void Decompress();
        public DdsHeaderDxt10 Header10 { get; private set; }

        public static Dds Create(byte[] data, PfimConfig config)
        {
            return Create(Util.CreateExposed(data), config);
        }

        /// <summary>Create a direct draw image from a stream</summary>
        public static Dds Create(Stream stream, PfimConfig config)
        {
            DdsHeader header = new DdsHeader(stream);
            return DecodeDds(stream, config, header);
        }

        /// <summary>
        /// Same as a regular create except assumes that the magic number has already been consumed
        /// </summary>
        internal static IImage CreateSkipMagic(Stream stream, PfimConfig config)
        {
            DdsHeader header = new DdsHeader(stream, true);
            return DecodeDds(stream, config, header);
        }

        private static Dds DecodeDds(Stream stream, PfimConfig config, DdsHeader header)
        {
            Dds dds;
            switch (header.PixelFormat.FourCC)
            {
                case CompressionAlgorithm.D3DFMT_DXT1:
                    dds = new Dxt1Dds(header, config);
                    break;

                case CompressionAlgorithm.D3DFMT_DXT2:
                case CompressionAlgorithm.D3DFMT_DXT4:
                    throw new ArgumentException("Cannot support DXT2 or DXT4");
                case CompressionAlgorithm.D3DFMT_DXT3:
                    dds = new Dxt3Dds(header, config);
                    break;

                case CompressionAlgorithm.D3DFMT_DXT5:
                    dds = new Dxt5Dds(header, config);
                    break;

                case CompressionAlgorithm.None:
                    dds = new UncompressedDds(header, config);
                    break;

                case CompressionAlgorithm.DX10:
                    var header10 = new DdsHeaderDxt10(stream);

                    // TODO@NickBabcock: dirty approach to make it just work. Obviously, this should be in DdsHeaderDxt10.cs & Bc6/7Dds.cs
                    switch (header10.DxgiFormat)
                    {
                        case DxgiFormat.BC6H_TYPELESS:
                        case DxgiFormat.BC6H_UF16:
                        case DxgiFormat.BC6H_SF16:
                        case DxgiFormat.BC7_TYPELESS:
                        case DxgiFormat.BC7_UNORM:

                            var CompressedSize = (int)stream.Length;
                            DDS_Header Header = new DDS_Header(stream);
                            var DX10Format = Header.DX10_DXGI_AdditionalHeader.dxgiFormat;
                            ImageEngineFormat tempFormat = Header.Format;
                            if (DX10Format == DDS_Header.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT)   // Trickses to get around the DX10 float header deal - Apparently float formats should be specified with the DX10 header...
                            {
                                tempFormat = ImageEngineFormat.DDS_ARGB_32F;
                                var tempPF = ((DDS_Header)Header).ddspf;
                                tempPF.dwRBitMask = 1;
                                tempPF.dwGBitMask = 2;
                                tempPF.dwBBitMask = 3;
                                tempPF.dwABitMask = 4;
                                Header.ddspf = tempPF;
                            }
                            var FormatDetails = new ImageFormats.ImageEngineFormatDetails(tempFormat, DX10Format);
                            stream.Seek(0, SeekOrigin.Begin);

                            int maxDimension = 0;
                            int decodeWidth = header.Width > header.Height ? maxDimension : 0;
                            int decodeHeight = header.Width < header.Height ? maxDimension : 0;

                            var memStream = new MemoryStream();
                            stream.CopyTo(memStream);
                            List<MipMap> mipmaps = DDSGeneral.LoadDDS(memStream, Header, maxDimension, FormatDetails, config);
                            var OriginalData = new byte[CompressedSize];
                            stream.Position = 0;
                            stream.Read(OriginalData, 0, CompressedSize);
                            dds = new Dxt5Dds(header, config); // ~Krakean~: just to init the dds var.
                            dds.Data = mipmaps[0].Pixels; // Krakean: return the first(original or biggest one) mipmap by default.
                            return dds;
                    }

                    dds = header10.NewDecoder(header, config);
                    dds.Header10 = header10;
                    break;

                case CompressionAlgorithm.ATI2:
                    dds = new Bc5Dds(header, config);
                    break;

                default:
                    throw new ArgumentException($"FourCC: {header.PixelFormat.FourCC} not supported.");
            }

            dds.Decode(stream, config);
            return dds;
        }

        protected abstract void Decode(Stream stream, PfimConfig config);

        public void ApplyColorMap()
        {
        }

        public void Dispose()
        {
            _config.Allocator.Return(Data);
        }
    }
}
