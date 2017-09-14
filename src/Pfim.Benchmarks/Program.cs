using System;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;

namespace Pfim.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var runPfimOnly = args.Contains("--pfim");
            var config = ManualConfig.Union(DefaultConfig.Instance, new BaseConfig());
            if (runPfimOnly)
            {
                config.Add(new NameFilter(x => x.Contains("Pfim")));
            }

            BenchmarkRunner.Run<TargaBenchmark>(config);
            BenchmarkRunner.Run<DdsBenchmark>(config);
        }
    }
}
