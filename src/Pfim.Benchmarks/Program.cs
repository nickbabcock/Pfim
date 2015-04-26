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
        private static ExcludeMinAndMaxTestOutcomeFilter filter = new ExcludeMinAndMaxTestOutcomeFilter();
        static void Main(string[] args)
        {
            Console.WriteLine("{0,-20} {1,-13} {2,-13} {3,-13} {4,-13}", "Benchmark", "Pfim", "TargaImage", "Image Magick", "DevIL");
            Console.WriteLine(new string('-', 79));
            runTest("uncmp-btm-left tga", File.ReadAllBytes(Path.Combine("data", "true-24.tga")), MagickFormat.Tga, DevILSharp.ImageType.Tga);
            runTest("cmp-btm-left tga", File.ReadAllBytes(Path.Combine("data", "true-32-rle.tga")), MagickFormat.Tga, DevILSharp.ImageType.Tga);
            runTest("large tga", File.ReadAllBytes(Path.Combine("data", "true-32-rle-large.tga")), MagickFormat.Tga, DevILSharp.ImageType.Tga);
        }

        private static void runTest(string groupName, byte[] data, MagickFormat format, DevILSharp.ImageType imageType)
        {
            var testGroup = new TestGroup(groupName);
            var pfim = testGroup.Plan("Pfim", () => Targa.Create(new MemoryStream(data)), 5)
                .GetResult().GetSummary(filter).AverageExecutionTime;

            var targaImage = testGroup.Plan("TargaImage", () =>
                new Paloma.TargaImage(new MemoryStream(data)), 5)
                .GetResult().GetSummary(filter).AverageExecutionTime;

            var magick = testGroup.Plan("magick", () =>
            {
                var settings = new MagickReadSettings();
                settings.Format = MagickFormat.Tga;
                new MagickImage(new MemoryStream(data), settings);
            }, 5).GetResult().GetSummary(filter).AverageExecutionTime;

            DevILSharp.Bootstrap.Init();
            var devil = testGroup.Plan("Devil", () =>
                    DevILSharp.Image.Load(data, DevILSharp.ImageType.Tga), 5)
                    .GetResult().GetSummary(filter).AverageExecutionTime;

            Console.WriteLine("{0,-20} {1,-13:N5} {2,-13:N5} {3,-13:N5} {4,-13:N5}", groupName, pfim, targaImage, magick, devil);
        }
    }
}
