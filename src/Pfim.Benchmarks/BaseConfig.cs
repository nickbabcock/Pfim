using System;
using System.Collections.Generic;
using System.Linq;
using Perfolizer.Horology;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

namespace Pfim.Benchmarks
{
    class BaseConfig : ManualConfig
    {
        public BaseConfig()
        {
            AddExporter(new CsvExporter(CsvSeparator.CurrentCulture, new SummaryStyle(null, false, SizeUnit.B, TimeUnit.Nanosecond, false)));
            AddDiagnoser(MemoryDiagnoser.Default);
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdErr);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Median);
        }
    }
}
