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
        /// <summary>
        /// Constructs a targa image from a targa image and raw data
        /// </summary>
        /// <param name="header">The targa header</param>
        /// <param name="data">The decoded targa data</param>
        private Targa(TargaHeader header, byte[] data)
        {
            Header = header;
            Data = data;
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
            var header = new TargaHeader(str);
            var targa = (header.IsCompressed) ? (IDecodeTarga)(new CompressedTarga())
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

            return new Targa(header, data);
        }

        public void ApplyColorMap()
        {
            // Check targa header field 2 and 3 as "it is best to check Field 3, Image Type, 
            // to make sure you have a file which can use the data stored in the Color Map Field.
            // Otherwise ignore theinformation"
            if (!Header.HasColorMap || 
                (Header.ImageType != TargaHeader.TargaImageType.RunLengthColorMap &&
                Header.ImageType != TargaHeader.TargaImageType.UncompressedColorMap)) {
                return;
            }

            var currentDepthBytes = BitsPerPixel / 8;
            var colorMapDepthBytes = Header.ColorMapDepth / 8;
            var newData = new byte[colorMapDepthBytes * Data.Length];
            switch (Header.ColorMapDepth)
            {
                case 16:
                case 24:
                case 32:
                    for (int i = 0; i < Data.Length; i += currentDepthBytes)
                    {
                        var colorMapIndex = Data[i] * colorMapDepthBytes;
                        for (int j = 0; j < colorMapDepthBytes; j++)
                        {
                            newData[i * colorMapDepthBytes + j] = Header.ColorMap[colorMapIndex + j];
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unrecognized color map depth {Header.ColorMapDepth}");
            }

            Data = newData;
            Header.PixelDepth = (byte)Header.ColorMapDepth;
            Header.ColorMap = new byte[] { };
            Header.ColorMapLength = 0;
            Header.HasColorMap = false;
            Header.ColorMapDepth = 0;
        }

        /// <summary>The raw image data</summary>
        public byte[] Data { get; private set; }

        public TargaHeader Header { get; private set; }

        /// <summary>Width of the image in pixels</summary>
        public int Width => Header.Width;

        /// <summary>Height of the image in pixels</summary>
        public int Height => Header.Height;

        /// <summary>The number of bytes that compose one line</summary>
        public int Stride => Util.Stride(Header.Width, Header.PixelDepth);

        public int BitsPerPixel => Header.PixelDepth;

        /// <summary>The format of the raw data</summary>
        public ImageFormat Format
        {
            get
            {
                switch (Header.PixelDepth)
                {
                    case 8: return ImageFormat.Rgb8;
                    case 16: return ImageFormat.R5g5b5;
                    case 24: return ImageFormat.Rgb24;
                    case 32: return ImageFormat.Rgba32;
                    default: throw new Exception($"Unrecognized pixel depth: {Header.PixelDepth}");
                }
            }
        }
    }
}
