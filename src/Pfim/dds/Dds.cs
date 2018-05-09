using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Class that represents direct draw surfaces
    /// </summary>
    public abstract class Dds : IImage
    {
        /// <summary>
        /// Instantiates a direct draw surface image from a header, the data,
        /// and additional info.
        /// </summary>
        protected Dds(DdsHeader header)
        {
            Header = header;
        }

        public DdsHeader Header { get; }
        public abstract int BitsPerPixel { get; }
        public int BytesPerPixel => BitsPerPixel / 8;
        public int Stride => (int)(4 * ((Header.Width * BytesPerPixel + 3) / 4));
        public virtual byte[] Data { get; protected set; }
        public int Width => (int)Header.Width;
        public int Height => (int)Header.Height;
        public abstract ImageFormat Format { get; }
        public abstract bool Compressed { get; }
        public abstract void Decompress();

        public static Dds Create(byte[] data, PfimConfig config)
        {
            return Create(Util.CreateExposed(data), config);
        }

        /// <summary>Create a direct draw image from a stream</summary>
        public static Dds Create(Stream stream, PfimConfig config)
        {
            DdsHeader header = new DdsHeader(stream);
            Dds dds;
            switch (header.PixelFormat.FourCC)
            {
                case CompressionAlgorithm.D3DFMT_DXT1:
                    dds = new Dxt1Dds(header);
                    break;

                case CompressionAlgorithm.D3DFMT_DXT2:
                case CompressionAlgorithm.D3DFMT_DXT4:
                    throw new ArgumentException("Cannot support DXT2 or DXT4");
                case CompressionAlgorithm.D3DFMT_DXT3:
                    dds = new Dxt3Dds(header);
                    break;

                case CompressionAlgorithm.D3DFMT_DXT5:
                    dds = new Dxt5Dds(header);
                    break;

                case CompressionAlgorithm.None:
                    dds = new UncompressedDds(header);
                    break;

                case CompressionAlgorithm.DX10:
                    var header10 = new DdsHeaderDxt10(stream);
                    dds = header10.NewDecoder(header);
                    break;

                default:
                    throw new ArgumentException($"FourCC: {header.PixelFormat.FourCC} not supported.");
            }

            dds.Decode(stream, config);
            return dds;
        }

        protected abstract void Decode(Stream stream, PfimConfig config);
    }
}
