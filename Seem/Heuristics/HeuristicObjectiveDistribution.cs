using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    /// <summary>
    /// Distribution of heuristic objective function values at a given tuple and their associated performance counters.
    /// </summary>
    /// <remarks>
    /// Because the size of heuristic solution pools is limited, in long runs storage of objective function values dominates the run's 
    /// memory footprint. Since this can lead to DRAM exhaustion, disk thrashing, and poor CPU utilization <see cref="HeuristicObjectiveDistribution"/>
    /// seeks to minimize its memory footprint. The primary mechanism for this is keeping shallow copies of the heuristic objective
    /// functions included in the distribution and then computing statistics on demand. This allows <see cref="HeuristicObjectiveDistribution"/>
    /// and <see cref="HeuristicSolutionPool"> instances to share objective function memory and prevents distribution tracking from incurring
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
    // Generation of 5th and 95th percentiles at 20 runs requires two more runs be released. Two runs must also be released for the 2.5 and 97.5
    // percentiles at 40 runs total. It's more likely calculating these percentiles will release memory but this will not be the case when
    // solution pool sizes are large enough.
    public class HeuristicObjectiveDistribution
    {
        private readonly List<List<float>> acceptedFinancialValueBySolution;

        public List<float> HighestFinancialValueBySolution { get; private init; }
        public List<HeuristicPerformanceCounters> PerfCountersBySolution { get; private init; }

        public HeuristicObjectiveDistribution()
        {
            this.acceptedFinancialValueBySolution = new();

            this.HighestFinancialValueBySolution = new();
            this.PerfCountersBySolution = new();
        }

        public int TotalSolutions 
        { 
            get 
            {
                Debug.Assert(this.HighestFinancialValueBySolution.Count == this.PerfCountersBySolution.Count);
                return this.HighestFinancialValueBySolution.Count; 
            }
        }

        public void AddSolution(Heuristic heuristic, int discountRateIndex, HeuristicPerformanceCounters perfCounters)
        {
            int moveDiscountRateIndex = discountRateIndex;
            if (heuristic.AcceptedFinancialValueByDiscountRateAndMove.Count == 1)
            {
                moveDiscountRateIndex = 0; // heuristic does not report objective functions across discount rates
            }
            this.acceptedFinancialValueBySolution.Add(heuristic.AcceptedFinancialValueByDiscountRateAndMove[moveDiscountRateIndex]);

            this.HighestFinancialValueBySolution.Add(heuristic.HighestFinancialValueByDiscountRate[discountRateIndex]);
            this.PerfCountersBySolution.Add(perfCounters);
        }

        public int GetMaximumMoves()
        {
            int maximumMoves = Int32.MinValue;
            foreach (List<float> acceptedObjectives in this.acceptedFinancialValueBySolution)
            {
                if (acceptedObjectives.Count > maximumMoves)
                {
                    maximumMoves = acceptedObjectives.Count;
                }
            }
            return maximumMoves;
        }

        public Statistics GetObjectiveFunctionStatisticsForMove(int moveIndex)
        {
            // assemble objective functions for this move
            List<float> objectiveFunctions = new();
            foreach (List<float> acceptedObjectives in this.acceptedFinancialValueBySolution)
            {
                if (moveIndex < acceptedObjectives.Count)
                {
                    objectiveFunctions.Add(acceptedObjectives[moveIndex]);
                }
            }

            return new(objectiveFunctions);
        }
    }
}
