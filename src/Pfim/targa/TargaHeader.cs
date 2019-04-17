using System;
using System.IO;
using System.Text;

namespace Pfim
{
    /// <summary>Class for parsing and storing information from a targa header</summary>
    public class TargaHeader
    {
        /// <summary>The targa image's type</summary>
        public enum TargaImageType
        {
            /// <summary>No image data included</summary>
            NoData = 0,

            /// <summary>Uncompressed, color-mapped image</summary>
            UncompressedColorMap = 1,

            /// <summary>Uncompressed, true-color image</summary>
            UncompressedTrueColor = 2,

            /// <summary>Uncompressed, black-and-white image</summary>
            UncompressedBW = 3,

            /// <summary>Run-length encoded, color-mapped image</summary>
            RunLengthColorMap = 9,

            /// <summary>Run-length encoded, true-color image</summary>
            RunLengthTrueColor = 10,

            /// <summary>Run-length encoded, black-and-white image</summary>
            RunLengthBW = 11,
        }

        /// <summary>
        /// Targa images may be laid out in such a way that the bytes in the
        /// file corresponds to different x and y origins. This enum
        /// enumerates all possible origins of the first pixel
        /// </summary>
        public enum TargaOrientation
        {
            /// <summary>
            /// First byte read corresponds to the pixel in the bottom left
            /// </summary>
            BottomLeft = 0,

            /// <summary>
            /// First byte read corresponds to the pixel in the bottom right
            /// </summary>
            BottomRight = 1,

            /// <summary>
            /// First byte read corresponds to the pixel in the top left
            /// </summary>
            TopLeft = 2,

            /// <summary>
            /// First byte read corresponds to the pixel in the top right
            /// </summary>
            TopRight = 3
        }

        private const int MINIMUM_SIZE = 18;

        internal TargaHeader(Stream str, byte[] partial, int partialLen, PfimConfig config)
        {
            DecodeTargaHeader(str, partial, partialLen, config);
        }

        /// <summary>
        /// Instantiate a targa header from a given stream. The stream will be parsed
        /// </summary>
        public TargaHeader(Stream str, PfimConfig config)
        {
            byte[] buf = new byte[MINIMUM_SIZE];
            if (str.Read(buf, 0, MINIMUM_SIZE) != MINIMUM_SIZE)
            {
                throw new ArgumentException("Stream doesn't have enough data for a .tga", nameof(str));
            }

            DecodeTargaHeader(str, buf, MINIMUM_SIZE, config);
        }

        private void DecodeTargaHeader(Stream str, byte[] partial, int partialLen, PfimConfig config)
        {
            byte[] buf;

            if (partialLen == MINIMUM_SIZE)
            {
                buf = partial;
            }
            else if (partialLen > MINIMUM_SIZE)
            {
                throw new ArgumentException($"Partial header can't exceed {MINIMUM_SIZE} bytes");
            }
            else
            {
                buf = new byte[MINIMUM_SIZE];
                var left = MINIMUM_SIZE - partialLen;
                for (int i = 0; i < partialLen; i++)
                {
                    buf[i] = partial[i];
                }

                if (str.Read(buf, partialLen, left) != left)
                {
                    throw new ArgumentException("Stream doesn't have enough data for a .tga", nameof(str));
                }
            }

            IDLength = buf[0];
            HasColorMap = buf[1] == 1;
            ImageType = (TargaImageType)buf[2];
            if (!Enum.IsDefined(typeof(TargaImageType), ImageType))
            {
                throw new ArgumentException("Detected invalid targa image");
            }

            ColorMapOrigin = BitConverter.ToInt16(buf, 3);
            ColorMapLength = BitConverter.ToInt16(buf, 5);
            ColorMapDepthBits = buf[7];
            XOrigin = BitConverter.ToInt16(buf, 8);
            YOrigin = BitConverter.ToInt16(buf, 10);
            Width = BitConverter.ToInt16(buf, 12);
            Height = BitConverter.ToInt16(buf, 14);
            PixelDepthBits = buf[16];

            // Extract the bits in place 4 and 5 for orientation
            Orientation = (TargaOrientation)((buf[17] >> 4) & 3);

            if (IDLength != 0)
            {
                var idBuf = new byte[IDLength];
                var amount = str.Read(idBuf, 0, IDLength);
                ImageId = Encoding.Unicode.GetString(idBuf, 0, amount);
            }

            if (HasColorMap)
            {
                if (ColorMapOrigin > ColorMapLength)
                {
                    throw new ArgumentException("Color map origin was detected to exceed color map length");
                }

                var mapBytes = ColorMapDepthBytes;
                ColorMap = new byte[ColorMapLength * mapBytes];
                str.Read(ColorMap, ColorMapOrigin * mapBytes, (ColorMapLength - ColorMapOrigin) * mapBytes);
            }
        }

        /// <summary>
        /// This field identifies the number of bytes contained in Field 6, the Image ID Field. The maximum number
        /// of characters is 255. A value of zero indicates that no Image ID field is included with the image.
        /// </summary>
        public byte IDLength { get; private set; }

        /// <summary>
        /// This field indicates the type of color map (if any) included with the image.
        /// </summary>
        public bool HasColorMap { get; internal set; }

        /// <summary>
        /// Type of the targa image
        /// </summary>
        public TargaImageType ImageType { get; internal set; }

        /// <summary>
        /// Index of the first color map entry.
        /// </summary>
        public short ColorMapOrigin { get; internal set; }

        /// <summary>
        /// Total number of color map entries included
        /// </summary>
        public short ColorMapLength { get; internal set; }

        /// <summary>
        /// Establishes the number of bits per entry. Typically 15, 16, 24 or 32-bit values are used.
        /// </summary>
        public short ColorMapDepthBits { get; internal set; }

        /// <summary>
        /// Number of whole bytes per pixel.
        /// </summary>
        public byte ColorMapDepthBytes => (byte)(ColorMapDepthBits / 8);

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
        public byte PixelDepthBits { get; internal set; }

        /// <summary>
        /// Number of whole bytes per pixel. This number includes Alpha channel bits
        /// </summary>
        public byte PixelDepthBytes => (byte)(PixelDepthBits / 8);

        /// <summary>
        /// Order in which pixel data is transferred from the file to the screen
        /// </summary>
        public TargaOrientation Orientation { get; private set; }

        /// <summary>
        /// This optional field contains identifying information about the
        /// image. The maximum length for this field is 255 bytes. Refer to
        /// Field 1 for the length of this field. If field 1 is set to Zero
        /// indicating that no Image ID exists then these bytes are not
        /// written to the file
        /// </summary>
        public string ImageId { get; private set; }

        public byte[] ColorMap { get; internal set; }

        //public List<Color> ColorMap { get; private set; }

        /// <summary>
        /// Returns whether the data is run length encoded, which means the
        /// image is compressed
        /// </summary>
        public bool IsCompressed => ImageType == TargaImageType.RunLengthTrueColor ||
                                    ImageType == TargaImageType.RunLengthColorMap ||
                                    ImageType == TargaImageType.RunLengthBW;
    }
}
