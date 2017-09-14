using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;

namespace Pfim.Benchmarks
{
    class BaseConfig : ManualConfig
    {
        public BaseConfig()
        {
            Add(new CsvExporter(CsvSeparator.CurrentCulture,
                new BenchmarkDotNet.Reports.SummaryStyle
                {
                    PrintUnitsInContent = false,
                    TimeUnit = BenchmarkDotNet.Horology.TimeUnit.Nanosecond
                }));

            Add(StatisticColumn.Mean);
            Add(StatisticColumn.StdErr);
            Add(StatisticColumn.StdDev);
            Add(StatisticColumn.Median);
            Add(new Job("net-ryu-64bit")
            {
                Env = { Runtime = Runtime.Clr, Jit = Jit.RyuJit, Platform = Platform.X64 }
            });
        }
    }
}
