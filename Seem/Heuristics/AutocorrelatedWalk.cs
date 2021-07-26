using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class AutocorrelatedWalk : Heuristic<HeuristicParameters>
    {
        public int Iterations { get; set; }

        public AutocorrelatedWalk(OrganonStand stand, HeuristicParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters, false)
        {
            this.Iterations = 4 * stand.GetTreeRecordCount();
        }

        public override string GetName()
        {
            return "AutocorrelatedWalk";
        }

        public override HeuristicPerformanceCounters Run(HeuristicResultPosition position, HeuristicResults results)
        {
            if (this.Iterations < 1)
            {
                throw new InvalidOperationException(nameof(this.Iterations));
            }
            if (this.RunParameters.LogOnlyImprovingMoves)
            {
                throw new NotSupportedException("Logging of only improving moves isn't currently supported.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            this.FinancialValue.SetMoveCapacity(this.Iterations);

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(position, results);
            float acceptedFinancialValue = this.EvaluateInitialSelection(position, this.Iterations, perfCounters);

            for (int iteration = 1; iteration < this.Iterations; ++iteration)
            {
                this.ConstructTreeSelection(this.HeuristicParameters.MinimumConstructionGreediness);
                perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();

                float candidateFinancialValue = this.GetFinancialValue(this.CurrentTrajectory, position.FinancialIndex);
                if (candidateFinancialValue > acceptedFinancialValue)
                {
                    // for now, accept change of tree selection if it increases financial value
                    acceptedFinancialValue = candidateFinancialValue;
                    this.CopyTreeGrowthToBestTrajectory(this.CurrentTrajectory);
                    ++perfCounters.MovesAccepted;
                }
                else
                {
                    ++perfCounters.MovesRejected;
                }

                this.FinancialValue.TryAddMove(acceptedFinancialValue, candidateFinancialValue);
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}