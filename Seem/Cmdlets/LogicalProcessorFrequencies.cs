using System;
using System.Diagnostics;

namespace Mars.Seem.Cmdlets
{
    internal class LogicalProcessorFrequencies : IDisposable
    {
        private bool isDisposed;
        private readonly PerformanceCounter[] frequencyCounters;

        public LogicalProcessorFrequencies()
        {
            int threads = Environment.ProcessorCount;
            this.isDisposed = false;
            this.frequencyCounters = new PerformanceCounter[threads];

            for (int thread = 0; thread < threads; ++thread)
            {
                this.frequencyCounters[thread] = new("Processor Information", "% Processor Performance", "0," + thread);
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    for (int thread = 0; thread < this.frequencyCounters.Length; ++thread)
                    {
                        this.frequencyCounters[thread].Dispose();
                    }
                }

                this.isDisposed = true;
            }
        }

        public float[] NextValue()
        {
            float[] values = new float[this.frequencyCounters.Length];
            for (int thread = 0; thread < this.frequencyCounters.Length; ++thread)
            {
                values[thread] = this.frequencyCounters[thread].NextValue();
            }

            return values;
        }
    }
}
