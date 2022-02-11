using Mars.Seem.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Cmdlets
{
    public class GrowthModelBenchmark
    {
        public List<float[]> ProcessorFrequenciesInPercent { get; private init; }
        public float[] ProcessorReferenceFrequenciesInGHz { get; private init; }
        public DateTime[] End { get; private init; }
        public TimeSpan PollingInterval { get; init; }
        public Simd Simd { get; init; }
        public DateTime Start { get; set; }
        public int[] Timesteps { get; private init; }
        public DateTime[] WarmupEnd { get; private init; }
        public int[] WarmupTimesteps { get; private init; }

        public GrowthModelBenchmark(int threads)
        {
            ProcessorPowerInformation powerInfo = NativeMethods.CallNtPowerInformation();

            this.PollingInterval = TimeSpan.FromSeconds(1.0F);
            this.ProcessorFrequenciesInPercent = new();
            this.ProcessorReferenceFrequenciesInGHz = new float[powerInfo.MaxMHz.Length];
            this.End = new DateTime[threads];
            this.PollingInterval = TimeSpan.Zero;
            this.Simd = Simd.Width128;
            this.Timesteps = new int[threads];
            this.WarmupEnd = new DateTime[threads];
            this.WarmupTimesteps = new int[threads];

            for (int thread = 0; thread < powerInfo.MaxMHz.Length; ++thread)
            {
                this.ProcessorReferenceFrequenciesInGHz[thread] = 0.001F * powerInfo.MaxMHz[thread];
            }
        }

        public int Threads
        {
            get { return this.Timesteps.Length; }
        }

        public float[] GetAverageFrequenciesInGHz()
        {
            float[] averageFrequencies = new float[this.ProcessorFrequenciesInPercent[0].Length];
            int startIndex = this.GetFirstCoreSampleIndexInBenchmark();
            for (int index = startIndex; index < this.ProcessorFrequenciesInPercent.Count; ++index)
            {
                float[] processorFrequencyMultipliers = this.ProcessorFrequenciesInPercent[index];
                for (int thread = 0; thread < averageFrequencies.Length; ++thread)
                {
                    averageFrequencies[thread] += processorFrequencyMultipliers[thread];
                }
            }

            float samplesAveraged = this.ProcessorFrequenciesInPercent.Count - startIndex;
            for (int thread = 0; thread < averageFrequencies.Length; ++thread)
            {
                averageFrequencies[thread] *= 0.01F * this.ProcessorReferenceFrequenciesInGHz[thread] / samplesAveraged;
            }

            return averageFrequencies;
        }

        private int GetFirstCoreSampleIndexInBenchmark()
        {
            // find average end of warmup period
            // Rather than summing and then dividing as usual, avoid integer rollover by predividing by the number of threads 
            // and then summing. This approach increases rounding error but, as it remains at the microsecond level, it is
            // acceptable in this case. Other options are double (less precise than long due to the mantissa bits) and
            // System.Numerics.BigInteger. Use of unsigned long only prevents integer rollover up to 5 threads (UInt64.MaxValue /
            // DateTime.MaxValue.Ticks = 5.85).
            long meanWarmupEndTicks = 0;
            for (int thread = 0; thread < this.Threads; ++thread)
            {
                meanWarmupEndTicks += this.WarmupEnd[thread].Ticks / this.Threads;
            }

            long firstSampleIndex = (meanWarmupEndTicks - this.Start.Ticks) / this.PollingInterval.Ticks;
            Debug.Assert(firstSampleIndex >= 0);
            return (int)firstSampleIndex;
        }
    }
}
