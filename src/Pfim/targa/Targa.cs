using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Provides a mechanism for decoding and storing the decoded information
    /// about a targa image
    /// </summary>
    public class Targa : IImage
    {
        private readonly PfimConfig _config;

        /// <summary>
        /// Constructs a targa image from a targa image and raw data
        /// </summary>
        private Targa(TargaHeader header, PfimConfig config, byte[] data, int dataLen)
        {
            _config = config;
            Header = header;
            Data = data;
            DataLen = dataLen;
        }

        public static Targa Create(byte[] data, PfimConfig config)
        {
            return Create(Util.CreateExposed(data), config);
        }

        public bool Compressed => false;
        public void Decompress()
        {
            // Never compressed
        }

        /// <summary>
        /// Creates a targa image from a given stream. The type of targa is determined from the
        /// targa header, which is assumed to be a part of the stream
        /// </summary>
        /// <param name="str">Stream to read the targa image from</param>
        /// <returns>A targa image</returns>
        public static Targa Create(Stream str, PfimConfig config)
        {
            var header = new TargaHeader(str, config);
            return DecodeTarga(str, config, header);
        }

        internal static IImage CreateWithPartialHeader(Stream str, PfimConfig config, byte[] magic)
        {
            var header = new TargaHeader(str, magic, 4, config);
            return DecodeTarga(str, config, header);
        }

        private static Targa DecodeTarga(Stream str, PfimConfig config, TargaHeader header)
        {
            var targa = (header.IsCompressed)
                ? (IDecodeTarga) (new CompressedTarga())
                : new UncompressedTarga();

            byte[] data;
            switch (header.Orientation)
            {
                case TargaHeader.TargaOrientation.BottomLeft:
                    data = targa.BottomLeft(str, header, config);
                    break;

                case TargaHeader.TargaOrientation.BottomRight:
                    data = targa.BottomRight(str, header, config);
                    break;

                case TargaHeader.TargaOrientation.TopRight:
                    data = targa.TopRight(str, header, config);
                    break;

                case TargaHeader.TargaOrientation.TopLeft:
                    data = targa.TopLeft(str, header, config);
                    break;

                default:
                    throw new Exception("Targa orientation not recognized");
            }

            var stride = Util.Stride(header.Width, header.PixelDepthBits);
            var len = header.Height * stride;
            return new Targa(header, config, data, len);
        }

        public void ApplyColorMap()
        {
            // Check targa header field 2 and 3 as "it is best to check Field 3, Image Type, 
            // to make sure you have a file which can use the data stored in the Color Map Field.
            // Otherwise ignore the information"
            if (!Header.HasColorMap || 
                (Header.ImageType != TargaHeader.TargaImageType.RunLengthColorMap &&
                Header.ImageType != TargaHeader.TargaImageType.UncompressedColorMap)) {
                return;
            }

            var currentDepthBytes = Header.PixelDepthBytes;
            var colorMapDepthBytes = Header.ColorMapDepthBytes;
            var newLen = colorMapDepthBytes * DataLen;
            var newData = _config.Allocator.Rent(newLen);
            switch (Header.ColorMapDepthBits)
            {
                case 16:
                case 24:
                case 32:
                    for (int i = 0; i < DataLen; i += currentDepthBytes)
                    {
                        var colorMapIndex = Data[i] * colorMapDepthBytes;
                        for (int j = 0; j < colorMapDepthBytes; j++)
                        {
                            newData[i * colorMapDepthBytes + j] = Header.ColorMap[colorMapIndex + j];
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unrecognized color map depth {Header.ColorMapDepthBits}");
            }

            _config.Allocator.Return(Data);
            Data = newData;
            DataLen = newLen;
            Header.PixelDepthBits = (byte)Header.ColorMapDepthBits;
            Header.ColorMap = new byte[] { };
            Header.ColorMapLength = 0;
            Header.HasColorMap = false;
            Header.ColorMapDepthBits = 0;
        }

        /// <summary>The raw image data</summary>
        public byte[] Data { get; private set; }

        public int DataLen { get; private set; }

        public TargaHeader Header { get; private set; }

        /// <summary>Width of the image in pixels</summary>
        public int Width => Header.Width;

        /// <summary>Height of the image in pixels</summary>
        public int Height => Header.Height;

        /// <summary>The number of bytes that compose one line</summary>
        public int Stride => Util.Stride(Header.Width, Header.PixelDepthBits);

        public int BitsPerPixel => Header.PixelDepthBits;

        /// <summary>The format of the raw data</summary>
        public ImageFormat Format
        {
            get
            {
                switch (Header.PixelDepthBits)
                {
                    case 8: return ImageFormat.Rgb8;
                    case 16: return ImageFormat.R5g5b5;
                    case 24: return ImageFormat.Rgb24;
                    case 32: return ImageFormat.Rgba32;
                    default: throw new Exception($"Unrecognized pixel depth: {Header.PixelDepthBits}");
                }
            }
        }

        public void Dispose()
        {
            _config.Allocator.Return(Data);
        }
    }
}
