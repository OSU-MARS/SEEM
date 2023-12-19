using Mars.Seem.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "GrowthModelBenchmark")]
    public class WriteGrowthModelBenchmark : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<GrowthModelBenchmark>? Benchmarks { get; set; }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Benchmarks != null);
            using StreamWriter writer = this.CreateCsvWriter();

            if (this.ShouldWriteCsvHeader())
            {
                writer.WriteLine("simd,threads,GHz,thread,timesteps,duration");
            }

            foreach (GrowthModelBenchmark benchmark in this.Benchmarks)
            {
                // for now, assume benchmark threads ran on the highest frequency cores
                // TODO: affinitize threads to cores or track thread affinity
                List<float> averageProcessorFrequenciesInGHz = new(benchmark.GetAverageFrequenciesInGHz());
                averageProcessorFrequenciesInGHz.Sort((float a, float b) => b.CompareTo(a)); // descending

                string linePrefix = benchmark.Simd.ToString() + "," + benchmark.Threads.ToString(CultureInfo.InvariantCulture);
                for (int thread = 0; thread < benchmark.Threads; ++thread)
                {
                    float averageGHz = averageProcessorFrequenciesInGHz[thread];
                    int timesteps = benchmark.Timesteps[thread];
                    double durationInSeconds = (benchmark.End[thread] - benchmark.WarmupEnd[thread]).TotalSeconds;
                    writer.WriteLine(linePrefix + "," +
                                     averageGHz.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                     thread.ToString(CultureInfo.InvariantCulture) + "," +
                                     timesteps.ToString(CultureInfo.InvariantCulture) + "," +
                                     durationInSeconds.ToString("0.000", CultureInfo.InvariantCulture));
                }
            }
        }
    }
}
