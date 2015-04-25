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

            Console.WriteLine(pfim.GetResult().GetSummary(new ExcludeMinAndMaxTestOutcomeFilter()));
            Console.WriteLine(targaImage.GetResult().GetSummary(new ExcludeMinAndMaxTestOutcomeFilter()));
        }

    }
}
