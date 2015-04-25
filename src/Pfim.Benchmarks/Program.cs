using ImageMagick;
using SimpleSpeedTester.Core;
using SimpleSpeedTester.Core.OutcomeFilters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pfim.Benchmarks
{
    class Program
    {
        static byte[] data = File.ReadAllBytes(Path.Combine("data", "true-24.tga"));
        static void Main(string[] args)
        {
            var testGroup = new TestGroup("Targa 24");

            var pfim = testGroup.Plan("Pfim", () => Targa.Create(new MemoryStream(data)), 5);

            var targaImage = testGroup.Plan("TargaImage", () =>
                new Paloma.TargaImage(new MemoryStream(data)), 5);

            var magick = testGroup.Plan("magick", () =>
                {
                    var settings = new MagickReadSettings();
                    settings.Format = MagickFormat.Tga;
                    new MagickImage(new MemoryStream(data), settings);
                }, 5);

            DevILSharp.Bootstrap.Init();
            var devil = testGroup.Plan("Devil", () =>
                    DevILSharp.Image.Load(data, DevILSharp.ImageType.Tga), 5);

            Console.WriteLine(pfim.GetResult().GetSummary(new ExcludeMinAndMaxTestOutcomeFilter()));
            Console.WriteLine(targaImage.GetResult().GetSummary(new ExcludeMinAndMaxTestOutcomeFilter()));
            Console.WriteLine(magick.GetResult().GetSummary(new ExcludeMinAndMaxTestOutcomeFilter()));
            Console.WriteLine(devil.GetResult().GetSummary(new ExcludeMinAndMaxTestOutcomeFilter()));
        }

    }
}
