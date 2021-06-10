using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class AutocorrelatedWalk : Heuristic<HeuristicParameters>
    {
        public int Iterations { get; set; }

        public AutocorrelatedWalk(OrganonStand stand, HeuristicParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters)
        {
            this.Iterations = 4 * stand.GetTreeRecordCount();
        }

        public override string GetName()
        {
            return "AutocorrelatedWalk";
        }

        public override HeuristicPerformanceCounters Run(HeuristicResultPosition position, HeuristicResults solutionIndex)
        {
            if (this.Iterations < 1)
            {
                throw new InvalidOperationException(nameof(this.Iterations));
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            this.AcceptedFinancialValueByDiscountRateAndMove.Capacity = this.Iterations;
            this.CandidateFinancialValueByDiscountRateAndMove.Capacity = this.Iterations;

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(position, solutionIndex);
            this.EvaluateInitialSelection(Constant.HeuristicDefault.DiscountRateIndex, this.Iterations, perfCounters);

            for (int iteration = 1; iteration < this.Iterations; ++iteration)
            {
                this.ConstructTreeSelection(this.HeuristicParameters.ConstructionGreediness);
                perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();

                float candidateFinancialValue = this.GetFinancialValue(this.CurrentTrajectory, position.DiscountRateIndex);
                if (candidateFinancialValue > this.HighestFinancialValueByDiscountRate[position.DiscountRateIndex])
                {
                    // accept change of tree selection if it improves upon the best solution
                    this.HighestFinancialValueByDiscountRate[position.DiscountRateIndex] = candidateFinancialValue;
                    this.BestTrajectory.CopyTreeGrowthFrom(this.CurrentTrajectory);
                    ++perfCounters.MovesAccepted;
                }
                else
                {
                    ++perfCounters.MovesRejected;
                }

                this.AcceptedFinancialValueByDiscountRateAndMove[Constant.HeuristicDefault.DiscountRateIndex].Add(this.HighestFinancialValueByDiscountRate[position.DiscountRateIndex]);
                this.CandidateFinancialValueByDiscountRateAndMove[Constant.HeuristicDefault.DiscountRateIndex].Add(candidateFinancialValue);
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}