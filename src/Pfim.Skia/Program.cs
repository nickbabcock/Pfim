using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Pfim.Skia
{
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
            SKColorType colorType;
            using (var image = Pfim.FromFile(file))
            {
                var newData = image.Data;
                var newDataLen = image.DataLen;
                var stride = image.Stride;
                switch (image.Format)
                {
                    case ImageFormat.Rgb8:
                        colorType = SKColorType.Gray8;
                        break;
                    case ImageFormat.R5g6b5:
                        // color channels still need to be swapped
                        colorType = SKColorType.Rgb565;
                        break;
                    case ImageFormat.Rgba16:
                        // color channels still need to be swapped
                        colorType = SKColorType.Argb4444;
                        break;
                    case ImageFormat.Rgb24:
                        // Skia has no 24bit pixels, so we upscale to 32bit
                        var pixels = image.DataLen / 3;
                        newDataLen = pixels * 4;
                        newData = new byte[newDataLen];
                        for (int i = 0; i < pixels; i++)
                        {
                            newData[i * 4] = image.Data[i * 3];
                            newData[i * 4 + 1] = image.Data[i * 3 + 1];
                            newData[i * 4 + 2] = image.Data[i * 3 + 2];
                            newData[i * 4 + 3] = 255;
                        }

                        stride = image.Width * 4;
                        colorType = SKColorType.Bgra8888;
                        break;
                    case ImageFormat.Rgba32:
                        colorType = SKColorType.Bgra8888;
                        break;
                    default:
                        throw new ArgumentException($"Skia unable to interpret pfim format: {image.Format}");
                }

                var imageInfo = new SKImageInfo(image.Width, image.Height, colorType);
                var handle = GCHandle.Alloc(newData, GCHandleType.Pinned);
                var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(newData, 0);
                using (var data = SKData.Create(ptr, newDataLen, (address, context) => handle.Free()))
                using (var skImage = SKImage.FromPixelData(imageInfo, data, stride))
                using (var bitmap = SKBitmap.FromImage(skImage))
                using (var fs = File.Create(Path.ChangeExtension(file, ".png")))
                using (var wstream = new SKManagedWStream(fs))
                {
                    var success = SKPixmap.Encode(wstream, bitmap, SKEncodedImageFormat.Png, 95);
                    Console.WriteLine(success ? "Image converted successfully" : "Image unsuccessful");
                }
            }
        }
    }
}
