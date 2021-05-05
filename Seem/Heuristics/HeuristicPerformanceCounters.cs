using System;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicPerformanceCounters
    {
        public TimeSpan Duration { get; set; }
        public int GrowthModelTimesteps { get; set; }
        public int MovesAccepted { get; set; }
        public int MovesRejected { get; set; }

        public HeuristicPerformanceCounters()
        {
            this.Duration = TimeSpan.Zero;
            this.GrowthModelTimesteps = 0;
            this.MovesAccepted = 0;
            this.MovesRejected = 0;
        }

        public static HeuristicPerformanceCounters operator +(HeuristicPerformanceCounters counters1, HeuristicPerformanceCounters counters2)
        {
            return new HeuristicPerformanceCounters()
            {
                Duration = counters1.Duration + counters2.Duration,
                GrowthModelTimesteps = counters1.GrowthModelTimesteps + counters2.GrowthModelTimesteps,
                MovesAccepted = counters1.MovesAccepted + counters2.MovesAccepted,
                MovesRejected = counters1.MovesRejected + counters2.MovesRejected
            };
        }
    }
}
