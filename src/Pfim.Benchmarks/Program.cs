using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using CommandLine;

namespace Pfim.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            result.WithParsed(options =>
            {
                var config = ManualConfig.Union(DefaultConfig.Instance, new BaseConfig());
                if (options.Filter != null)
                {
                    var filter = options.Filter.ToUpperInvariant();
                    config.Add(new NameFilter(x => x.ToUpperInvariant().Contains(filter)));
                }

                BenchmarkRunner.Run<TargaBenchmark>(config);
                BenchmarkRunner.Run<DdsBenchmark>(config);
            })
            .WithNotParsed(errors => Console.WriteLine("Errored"));
        }
    }
}
