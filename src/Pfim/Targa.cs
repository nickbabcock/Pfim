using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pfim
{
    public abstract class Targa
    {
        protected byte[] data;

        public Targa(Stream stream, TargaHeader header)
        {
            Header = header;
            Stride = Util.Stride(header.Width, header.PixelDepth);
            data = new byte[Header.Height * Stride];

            switch (Header.Orientation)
            {
                case TargaHeader.TargaOrientation.BottomLeft:
                    BottomLeft(stream);
                    break;
                case TargaHeader.TargaOrientation.BottomRight:
                    BottomRight(stream);
                    break;
                case TargaHeader.TargaOrientation.TopRight:
                    TopRight(stream);
                    break;
                case TargaHeader.TargaOrientation.TopLeft:
                    TopLeft(stream);
                    break;
                default:
                    throw new ApplicationException("Targa orientation not recognized");
            }
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
            return (header.IsCompressed) ? (Targa)(new CompressedTarga(str, header)) : new UncompressedTarga(str, header);
        }
    }
}
