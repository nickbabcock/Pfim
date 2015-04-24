using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Provides a mechanism for decoding and storing the decoded information
    /// about a targa image
    /// </summary>
    public class Targa
    {
        /// <summary>Raw data of the image</summary>
        public readonly byte[] data;

        /// <summary>Header of the image</summary>
        public readonly TargaHeader header;

        /// <summary>
        /// Constructs a targa image from a targa image and raw data
        /// </summary>
        /// <param name="header">The targa header</param>
        /// <param name="data">The decoded targa data</param>
        internal Targa(TargaHeader header, byte[] data)
        {
            this.header = header;
            this.data = data;
        }

        /// <summary>
        /// Creates a targa image from a given stream. The type of targa is determined from the
        /// targa header, which is assumed to be a part of the stream
        /// </summary>
        /// <param name="str">Stream to read the targa image from</param>
        /// <returns>A targa image</returns>
        public static Targa Create(Stream str)
        {
            var header = new TargaHeader(str);
            var targa = (header.IsCompressed) ? (IDecodeTarga)(new CompressedTarga())
                : new UncompressedTarga();

            byte[] data;
            switch (header.Orientation)
            {
                case TargaHeader.TargaOrientation.BottomLeft:
                    data = targa.BottomLeft(str, header);
                    break;

                case TargaHeader.TargaOrientation.BottomRight:
                    data = targa.BottomRight(str, header);
                    break;

                case TargaHeader.TargaOrientation.TopRight:
                    data = targa.TopRight(str, header);
                    break;

                case TargaHeader.TargaOrientation.TopLeft:
                    data = targa.TopLeft(str, header);
                    break;

                default:
                    throw new ApplicationException("Targa orientation not recognized");
            }

            return new Targa(header, data);
        }
    }
}
