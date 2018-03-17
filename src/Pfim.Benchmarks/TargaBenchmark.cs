using System.Drawing;
using System.IO;
using BenchmarkDotNet.Attributes;
using DmitryBrant.ImageFormats;
using FreeImageAPI;
using ImageMagick;
using StbSharp;
using TGASharpLib;
using DS = DevILSharp;

namespace Pfim.Benchmarks
{
    public class TargaBenchmark
    {
        [Params("true-32-rle-large.tga", "true-24-large.tga", "true-24.tga", "true-32-rle.tga", "rgb24_top_left.tga")]
        public string Payload { get; set; }

        private byte[] data;

        [GlobalSetup]
        public void SetupData()
        {
            data = File.ReadAllBytes(Payload);
            DS.Bootstrap.Init();
        }

        [Benchmark]
        public IImage Pfim() => Targa.Create(new MemoryStream(data));

        [Benchmark]
        public FreeImageBitmap FreeImage() => FreeImageAPI.FreeImageBitmap.FromStream(new MemoryStream(data));

        [Benchmark]
        public int ImageMagick()
        {
            var settings = new MagickReadSettings {Format = MagickFormat.Tga};
            using (var image = new MagickImage(new MemoryStream(data), settings))
            {
                return image.Width;
            }
        }

        [Benchmark]
        public int DevILSharp()
        {
            using (var image = DS.Image.Load(data, DS.ImageType.Tga))
            {
                return image.Width;
            }
        }

        [Benchmark]
        public int TargaImage()
        {
            using (var image = new Paloma.TargaImage(new MemoryStream(data)))
            {
                return image.Stride;
            }
        }

        [Benchmark]
        public int ImageFormats()
        {
            using (var img = TgaReader.Load(new MemoryStream(data)))
            {
                return img.Width;
            }
        }

        [Benchmark]
        public int StbSharp() => StbImage.LoadFromMemory(data, StbImage.STBI_rgb_alpha).Width;

        // TgaSharpLib does not neutrally orient targa images until a conversion to bitmap,
        // so to make the comparison apples to apples, we flip the image in the benchmark
        [Benchmark]
        public Bitmap TgaSharpLib() => TGA.FromBytes(data).ToBitmap();
    }
}