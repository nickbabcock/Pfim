using System;
using System.IO;

namespace Pfim
{
    public abstract class Targa
    {
        protected byte[] data;

        public Targa(TargaHeader header)
        {
            Header = header;
            Stride = Util.Stride(header.Width, header.PixelDepth);
            data = new byte[Header.Height * Stride];
        }

        protected int Stride { get; private set; }

        public byte[] Data { get { return data; } }

        public TargaHeader Header { get; private set; }

        protected abstract void BottomLeft(Stream str);

        protected abstract void BottomRight(Stream str);

        protected abstract void TopRight(Stream str);

        protected abstract void TopLeft(Stream str);

        public static Targa Create(Stream str)
        {
            var header = new TargaHeader(str);
            var targa = (header.IsCompressed) ? (Targa)(new CompressedTarga(header))
                : new UncompressedTarga(header);

            switch (header.Orientation)
            {
                case TargaHeader.TargaOrientation.BottomLeft:
                    targa.BottomLeft(str);
                    break;

                case TargaHeader.TargaOrientation.BottomRight:
                    targa.BottomRight(str);
                    break;

                case TargaHeader.TargaOrientation.TopRight:
                    targa.TopRight(str);
                    break;

                case TargaHeader.TargaOrientation.TopLeft:
                    targa.TopLeft(str);
                    break;

                default:
                    throw new ApplicationException("Targa orientation not recognized");
            }

            return targa;
        }
    }
}
