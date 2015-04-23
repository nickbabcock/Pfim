using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pfim
{
    public class UncompressedTarga : Targa
    {
        public UncompressedTarga(Stream str, TargaHeader header) : base(str, header) { }

        protected override void BottomLeft(Stream str)
        {
            var pixelWidth = Header.PixelDepth * Header.Width;
            var padding = Util.Stride(Header.Width, Header.PixelDepth) * 8 - pixelWidth;
            Util.FillBottomLeft(str, data, pixelWidth / 8, padding: padding);
        }

        protected override void BottomRight(Stream str)
        {
            throw new NotImplementedException();
        }

        protected override void TopRight(Stream str)
        {
            throw new NotImplementedException();
        }

        protected override void TopLeft(Stream str)
        {
            Util.Fill(str, data);
        }
    }
}
