namespace Pfim.dds.bptc.common
{
    /// <summary>
    /// Represents a mipmap of an image.
    /// </summary>
    public class MipMap
    {
        /// <summary>
        /// Pixels in bitmap image.
        /// </summary>
        public byte[] Pixels
        {
            get; set;
        }

        /// <summary>
        /// Size of mipmap in memory.
        /// </summary>
        public int UncompressedSize { get; private set; }

        /// <summary>
        /// Details of the format that this mipmap was created from.
        /// </summary>
        public ImageFormats.ImageEngineFormatDetails LoadedFormatDetails { get; private set; }

        /// <summary>
        /// Mipmap width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Mipmap height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Indicates if there is any alpha in image.
        /// </summary>
        public bool IsAlphaPresent
        {
            get
            {
                if (Pixels?.Length != 0)
                {
                    for (int i = 3; i < Pixels.Length; i += 4)   // TODO: ComponentSize
                    {
                        if (Pixels[i] != 0)
                            return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Creates a Mipmap object from a WPF image.
        /// </summary>
        public MipMap(byte[] pixels, int width, int height, ImageFormats.ImageEngineFormatDetails details)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
            LoadedFormatDetails = details;

            UncompressedSize = ImageFormats.GetUncompressedSize(width, height, details.MaxNumberOfChannels, false);
        }
    }
}
