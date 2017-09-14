using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using FreeImageAPI;
using ImageMagick;

namespace Pfim.Benchmarks
{
    public class DdsBenchmark
    {
        [Params("dxt1-simple.dds", "dxt3-simple.dds", "dxt5-simple.dds", "32-bit-uncompressed.dds")]
        public string Payload { get; set; }

        private byte[] data;

        [GlobalSetup]
        public void SetupData()
        {
            data = File.ReadAllBytes(Payload);
            DevILSharp.Bootstrap.Init();
        }

        [Benchmark]
        public IImage PfimTarga() => Dds.Create(new MemoryStream(data));

        [Benchmark]
        public FreeImageBitmap FreeImage() => FreeImageAPI.FreeImageBitmap.FromStream(new MemoryStream(data));

        [Benchmark]
        public int ImageMagick()
        {
            var settings = new MagickReadSettings { Format = MagickFormat.Dds };
            using (var image = new MagickImage(new MemoryStream(data), settings))
            {
                return image.Width;
            }
        }

        [Benchmark]
        public int DevILSharpTarga()
        {
            using (var image = DevILSharp.Image.Load(data, DevILSharp.ImageType.Dds))
            {
                return image.Width;
            }
        }
    }
}
