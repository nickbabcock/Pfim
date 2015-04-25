using SimpleSpeedTester.Core;
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
        static MemoryStream mem = new MemoryStream(File.ReadAllBytes(Path.Combine("data", "true-24.tga")));
        static void Main(string[] args)
        {
            // initialize a new test group
            var testGroup = new TestGroup("Example2");

            // PlanAndExecute actually executes the Action delegate 5 times and returns the result summary
            var testResultSummary = testGroup.PlanAndExecute("Test1", () =>
            { 
                Targa.Create(mem);
                mem.Seek(0, SeekOrigin.Begin);
            }, 5);

            Console.WriteLine(testResultSummary);
        }

    }
}
