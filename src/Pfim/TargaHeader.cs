using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pfim
{
    public class TargaHeader
    {
        public enum TargaImageType
        {
            NoData = 0,
            UncompressedColorMap = 1,
            UncompressedTrueColor = 2,
            UncompressedBW = 3,
            RunLengthColorMap = 9,
            RunLengthTrueColor = 10,
            RunLengthBW = 11,
        }

        public enum TargaOrientation
        {
            BottomLeft = 0,
            BottomRight = 1,
            TopLeft = 2,
            TopRight = 3
        }

        public TargaHeader(Stream str)
        {
            byte[] buf = new byte[18];
            if (str.Read(buf, 0, 18) != 18)
                throw new ArgumentException("str", "Stream doesn't have enough data for a .tga");

            IDLength = buf[0];
            HasColorMap = buf[1] == 1;
            ImageType = (TargaImageType)buf[2];
            ColorMapOrigin = BitConverter.ToInt16(buf, 3);
            ColorMapLength = BitConverter.ToInt16(buf, 5);
            ColorMapDepth = buf[7];
            XOrigin = BitConverter.ToInt16(buf, 8);
            YOrigin = BitConverter.ToInt16(buf, 10);
            Width = BitConverter.ToInt16(buf, 12);
            Height = BitConverter.ToInt16(buf, 14);
            PixelDepth = buf[16];

            // Extract the bits in place 4 and 5 for orientation
            Orientation = (TargaOrientation)((buf[17] >> 4) & 3);

            if (IDLength != 0)
            {
                buf = new byte[IDLength];
                str.Read(buf, 0, IDLength);
                ImageId = new string(buf.Select(x => (char)x).ToArray());
            }

            //if (HasColorMap)
            //{
            //    buf = new byte[PixelDepth / 8];
            //    str.Read(buf, 0, buf.Length);
            //    if (PixelDepth / 8 == 3)
            //    {
            //        for (int i = 0; i < buf.Length; i += 3)
            //            ColorMap.Add(Color.FromArgb(buf[i], buf[i + 1], buf[i + 2]));
            //    }
            //    else if (PixelDepth / 8 == 4)
            //    {
            //        for (int i = 0; i < buf.Length; i += 4)
            //            ColorMap.Add(Color.FromArgb(buf[i], buf[i + 1], buf[i + 2], buf[i + 3]));
            //    }
            //}
        }

        /// <summary>
        /// This field identifies the number of bytes contained in Field 6, the Image ID Field. The maximum number
        /// of characters is 255. A value of zero indicates that no Image ID field is included with the image.
        /// </summary>
        public byte IDLength { get; private set; }

        /// <summary>
        /// This field indicates the type of color map (if any) included with the image.
        /// </summary>
        public bool HasColorMap { get; private set; }

        public TargaImageType ImageType { get; private set; }

        /// <summary>
        /// Index of the first color map entry.
        /// </summary>
        public short ColorMapOrigin { get; private set; }

        /// <summary>
        /// Total number of color map entries included
        /// </summary>
        public short ColorMapLength { get; private set; }

        /// <summary>
        /// Establishes the number of bits per entry. Typically 15, 16, 24 or 32-bit values are used.
        /// </summary>
        public short ColorMapDepth { get; private set; }

        /// <summary>
        /// These bytes specify the absolute horizontal coordinate for the lower left corner of the image.
        /// </summary>
        public short XOrigin { get; private set; }

        /// <summary>
        /// These bytes specify the absolute vertical coordinate for the lower left corner of the image.
        /// </summary>
        public short YOrigin { get; private set; }

        /// <summary>
        /// Width of the image in pixels.
        /// </summary>
        public short Width { get; private set; }

        /// <summary>
        ///  Height of the image in pixels
        /// </summary>
        public short Height { get; private set; }

        /// <summary>
        /// Number of bits per pixel. This number includes the Attribute or Alpha channel bits
        /// </summary>
        public byte PixelDepth { get; private set; }

        /// <summary>
        /// Order in which pixel data is transferred from the file to the screen
        /// </summary>
        public TargaOrientation Orientation { get; private set; }

        public string ImageId { get; private set; }

        //public List<Color> ColorMap { get; private set; }

        public bool IsCompressed
        {
            get
            {
                return ImageType == TargaImageType.RunLengthTrueColor ||
                    ImageType == TargaImageType.RunLengthColorMap ||
                    ImageType == TargaImageType.RunLengthBW;
            }
        }
    }
}
