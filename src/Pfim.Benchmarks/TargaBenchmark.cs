using System.IO;
using BenchmarkDotNet.Attributes;
using FreeImageAPI;
using ImageMagick;

namespace Pfim.Benchmarks
{
    public class TargaBenchmark
    {
        [Params("true-32-rle-large.tga", "true-24-large.tga", "true-24.tga", "true-32-rle.tga")]
        public string Payload { get; set; }

        private byte[] data;

        [GlobalSetup]
        public void SetupData()
        {
            data = File.ReadAllBytes(Payload);
            DevILSharp.Bootstrap.Init();
        }

        [Benchmark]
        public IImage PfimTarga() => Targa.Create(new MemoryStream(data));

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
        public int DevILSharpTarga()
        {
            using (var image = DevILSharp.Image.Load(data, DevILSharp.ImageType.Tga))
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
    }
}