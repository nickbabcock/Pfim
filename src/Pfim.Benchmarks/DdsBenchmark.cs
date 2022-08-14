﻿using System.IO;
using BenchmarkDotNet.Attributes;
using FreeImageAPI;
using ImageMagick;
using Pfim.Tests;
using DS = DevILSharp;

namespace Pfim.Benchmarks
{
    [Config(typeof(BaseConfig))]
    public class DdsBenchmark
    {
        [Params("dxt1.dds", "dxt3.dds", "dxt5.dds", "32bit.dds")]
        public string Payload { get; set; }

        private byte[] data;

        private readonly PfimConfig _pfimConfig = new PfimConfig(allocator: new PfimAllocator());

        [GlobalSetup]
        public void SetupData()
        {
            data = File.ReadAllBytes(Path.Combine("bench", Payload));

            Aardvark.Base.Aardvark.Init();
            DS.Bootstrap.Init();
        }

        [Benchmark]
        public int Pfim()
        {
            using (var image = Dds.Create(data, _pfimConfig))
            {
                return image.BytesPerPixel;
            }
        }

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
        public int DevILSharp()
        {
            using (var image = DS.Image.Load(data, DS.ImageType.Dds))
            {
                return image.Width;
            }
        }
    }
}
