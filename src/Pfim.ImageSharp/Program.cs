using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using IS = SixLabors.ImageSharp;

namespace Pfim.ImageSharp
{
    // Reads in a tga / dds and saves it as a png
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                ConvertFile(arg);
            }
        }

        private static void ConvertFile(string file)
        {
            using (var image = Pfim.FromFile(file))
            {
                byte[] newData;

                // Since image sharp can't handle data with line padding in a stride
                // we create an stripped down array if any padding is detected
                var tightStride = image.Width * image.BitsPerPixel / 8;
                if (image.Stride != tightStride)
                {
                    newData = new byte[image.Height * tightStride];
                    for (int i = 0; i < image.Height; i++)
                    {
                        Buffer.BlockCopy(image.Data, i * image.Stride, newData, i * tightStride, tightStride);
                    }
                }
                else
                {
                    newData = image.Data;
                }

                SaveAsPng(file, image, newData);
            }
        }

        private static void SaveAsPng(string file, IImage image, byte[] newData)
        {
            var newFile = Path.ChangeExtension(file, ".png");
            var encoder = new PngEncoder();
            using (var fs = File.Create(newFile))
            {
                switch (image.Format)
                {
                    case ImageFormat.Rgba32:
                        IS.Image.LoadPixelData<Bgra32>(newData, image.Width, image.Height).Save(fs, encoder);
                        break;
                    case ImageFormat.Rgb24:
                        IS.Image.LoadPixelData<Bgr24>(newData, image.Width, image.Height).Save(fs, encoder);
                        break;
                    case ImageFormat.Rgba16:
                        IS.Image.LoadPixelData<Bgra4444>(newData, image.Width, image.Height).Save(fs, encoder);
                        break;
                    case ImageFormat.R5g5b5:
                        // Turn the alpha channel on for image sharp.
                        for (int i = 1; i < newData.Length; i += 2)
                        {
                            newData[i] |= 128;
                        }
                        IS.Image.LoadPixelData<Bgra5551>(newData, image.Width, image.Height).Save(fs, encoder);
                        break;
                    case ImageFormat.R5g5b5a1:
                        IS.Image.LoadPixelData<Bgra5551>(newData, image.Width, image.Height).Save(fs, encoder);
                        break;
                    case ImageFormat.R5g6b5:
                        IS.Image.LoadPixelData<Bgr565>(newData, image.Width, image.Height).Save(fs, encoder);
                        break;
                    case ImageFormat.Rgb8:
                        IS.Image.LoadPixelData<L8>(newData, image.Width, image.Height).Save(fs, encoder);
                        break;
                    default:
                        throw new Exception($"ImageSharp does not recognize image format: {image.Format}");
                }
            }
        }
    }
}
