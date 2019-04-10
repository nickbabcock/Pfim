using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

namespace Pfim.Benchmarks
{
    class BaseConfig : ManualConfig
    {
        public BaseConfig()
        {
            Add(new CsvExporter(CsvSeparator.CurrentCulture, new SummaryStyle(false, SizeUnit.B, TimeUnit.Nanosecond, false)));
            Add(MemoryDiagnoser.Default);
            Add(StatisticColumn.Mean);
            Add(StatisticColumn.StdErr);
            Add(StatisticColumn.StdDev);
            Add(StatisticColumn.Median);
            Add(new Job("net-ryu-64bit", EnvironmentMode.RyuJitX64));
        }
    }
}
