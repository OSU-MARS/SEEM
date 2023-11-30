using Mars.Seem.Heuristics;
using Mars.Seem.Silviculture;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Optimization
{
    /// <summary>
    /// Distribution of heuristic objective function values at a given tuple and their associated performance counters.
    /// </summary>
    /// <remarks>
    /// Because the size of heuristic solution pools is limited, in long runs storage of objective function values dominates the run's 
    /// memory footprint. Since this can lead to DRAM exhaustion, disk thrashing, and poor CPU utilization <see cref="OptimizationObjectiveDistribution"/>
    /// seeks to minimize its memory footprint. The primary mechanism for this is keeping shallow copies of the heuristic objective
    /// functions included in the distribution and then computing statistics on demand. This allows <see cref="OptimizationObjectiveDistribution"/>
    /// and <see cref="SilviculturalPrescriptionPool"> instances to share objective function memory and prevents distribution tracking from incurring
    /// additional memory until the solution pool size is reached and additional heuristic runs begin to push solutions out of pools. Past this
    /// point, memory consumption is minimized by retaining only the additional heuristics' objective function lists.
    /// </remarks>
    // In cases where all heuristic runs in a distribution are known to be complete it can be possible to reduce the distribution's memory
    // footprint by calculating its statistics and discarding the objective function lists which are no longer needed. Since objective function
    // values and statistics are all stored as 32 bit values, this requires a heuristic be run more than at least <solution pool size> + 5 + 2 =
    // <pool size> + 7 times. This occurs because 
    //   1) Memory is only released when discarding solutions not also kept in solution pools.
    //   2) Conversion to statistics creates need to store count, minimum, mean, median, and maximum values. Memory breakeven on these statistics
    //      therefore requires at five lists of objective function values be released. If the heuristics ran to varying number of moves then
    //      more lists will need to be released, with the number of lists required increasing with the degree of imbalance in the number of moves.
    //   3) Since at least five runs must have occurred, lower and upper quartiles will also be calculated. Two more runs, for a total of seven,
    //      are therefore required. Since quantiles list need not be populated beyond the number of moves required to generate them they are less
    //      sensitive to move imbalances.
    // Generation of 10th and 90th percentiles at 10 runs requires two more runs be released. Two runs must also be released for the 2.5 and 97.5
    // percentiles at 40 runs total. It's more likely calculating these percentiles will release memory but this will not be the case when
    // solution pool sizes are large enough.
    public class OptimizationObjectiveDistribution
    {
        private readonly List<IList<float>> acceptedFinancialValueBySolution;

        public List<float> HighestFinancialValueBySolution { get; private init; }
        public List<PrescriptionPerformanceCounters> PerfCountersBySolution { get; private init; }

        public OptimizationObjectiveDistribution()
        {
            this.acceptedFinancialValueBySolution = [];

            this.HighestFinancialValueBySolution = [];
            this.PerfCountersBySolution = [];
        }

        public int SolutionsInFinancialDistribution
        {
            get { return this.acceptedFinancialValueBySolution.Count; }
        }

        public int TotalSolutions 
        { 
            get 
            {
                Debug.Assert(this.HighestFinancialValueBySolution.Count == this.PerfCountersBySolution.Count);
                return this.HighestFinancialValueBySolution.Count; 
            }
        }

        public void Add(float financialValue, PrescriptionPerformanceCounters perfCounters)
        {
            this.HighestFinancialValueBySolution.Add(financialValue);
            this.PerfCountersBySolution.Add(perfCounters);
        }

        public void Add(Heuristic heuristic, SilviculturalCoordinate coordinate, PrescriptionPerformanceCounters perfCounters)
        {
            this.Add(heuristic.FinancialValue.GetHighestValueWithDefaulting(coordinate), perfCounters);

            if (heuristic.RunParameters.LogOnlyImprovingMoves == false)
            {
                // for now, include only dense acceptance trajectories starting with move 0 in distributions
                // If needed, thinning prescription enumerations with sparse acceptance trajectories can be included but doing so is likely
                // to be problematic to memory consumption due to the trajectories' length.
                this.acceptedFinancialValueBySolution.Add(heuristic.FinancialValue.GetAcceptedValuesWithDefaulting(coordinate));
            }
        }

        public int GetMaximumMoveIndex()
        {
            int maximumMoves = 0;
            foreach (List<float> acceptedObjectives in this.acceptedFinancialValueBySolution)
            {
                if (acceptedObjectives.Count > maximumMoves)
                {
                    maximumMoves = acceptedObjectives.Count;
                }
            }
            return maximumMoves;
        }

        public DistributionStatistics GetFinancialStatisticsForMove(int moveIndex)
        {
            // assemble objective functions for this move
            List<float> financialValues = [];
            foreach (List<float> acceptedObjectives in this.acceptedFinancialValueBySolution)
            {
                if (moveIndex < acceptedObjectives.Count)
                {
                    financialValues.Add(acceptedObjectives[moveIndex]);
                }
            }

            return new(financialValues);
        }
    }
}
