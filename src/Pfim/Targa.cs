using System;
using System.IO;

namespace Pfim
{
    public class Targa
    {
        public readonly byte[] data;
        public readonly TargaHeader header;

        public Targa(TargaHeader header, byte[] data)
        {
            this.header = header;
            this.data = data;
        }

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
