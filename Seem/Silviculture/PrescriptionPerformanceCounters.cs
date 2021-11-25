using System;

namespace Osu.Cof.Ferm.Silviculture
{
    public class PrescriptionPerformanceCounters
    {
        public static readonly PrescriptionPerformanceCounters Zero = new();

        public TimeSpan Duration { get; set; }
        // an i7-3770 can easily exceed 2^31 steps per a day with Organon
        // While single heuristic runs are not expected to approach 2^31 timesteps, top level performance counter accumulation across all runs
        // therefore would occasionally result in integer rollover with an Int32 or UInt32.
        public long GrowthModelTimesteps { get; set; }
        public int MovesAccepted { get; set; }
        public int MovesRejected { get; set; }
        public int TreesRandomizedInConstruction { get; set; }

        public PrescriptionPerformanceCounters()
        {
            this.Duration = TimeSpan.Zero;
            this.GrowthModelTimesteps = 0;
            this.MovesAccepted = 0;
            this.MovesRejected = 0;
            this.TreesRandomizedInConstruction = 0;
        }

        public static PrescriptionPerformanceCounters operator +(PrescriptionPerformanceCounters counters1, PrescriptionPerformanceCounters counters2)
        {
            return new PrescriptionPerformanceCounters()
            {
                Duration = counters1.Duration + counters2.Duration,
                GrowthModelTimesteps = counters1.GrowthModelTimesteps + counters2.GrowthModelTimesteps,
                MovesAccepted = counters1.MovesAccepted + counters2.MovesAccepted,
                MovesRejected = counters1.MovesRejected + counters2.MovesRejected,
                TreesRandomizedInConstruction = counters1.TreesRandomizedInConstruction + counters2.TreesRandomizedInConstruction
            };
        }
    }
}
