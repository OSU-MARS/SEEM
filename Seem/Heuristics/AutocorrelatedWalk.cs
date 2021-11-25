using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Silviculture;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class AutocorrelatedWalk : Heuristic<HeuristicParameters>
    {
        public int Iterations { get; set; }

        public AutocorrelatedWalk(OrganonStand stand, HeuristicParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters, evaluatesAcrossRotationsAndFinancialScenarios: false)
        {
            this.Iterations = 4 * stand.GetTreeRecordCount();
        }

        public override string GetName()
        {
            return "AutocorrelatedWalk";
        }

        public override PrescriptionPerformanceCounters Run(StandTrajectoryCoordinate coordinate, HeuristicStandTrajectories trajectories)
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
            PrescriptionPerformanceCounters perfCounters = new();

            this.FinancialValue.SetMoveCapacity(this.Iterations);

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(coordinate, trajectories);
            float acceptedFinancialValue = this.EvaluateInitialSelection(coordinate, this.Iterations, perfCounters);

            for (int iteration = 1; iteration < this.Iterations; ++iteration)
            {
                this.ConstructTreeSelection(this.HeuristicParameters.MinimumConstructionGreediness);
                perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();

                float candidateFinancialValue = this.GetFinancialValue(this.CurrentTrajectory, coordinate.FinancialIndex);
                if (candidateFinancialValue > acceptedFinancialValue)
                {
                    // for now, accept change of tree selection if it increases financial value
                    acceptedFinancialValue = candidateFinancialValue;
                    this.CopyTreeGrowthToBestTrajectory(coordinate, this.CurrentTrajectory);
                    ++perfCounters.MovesAccepted;
                }
                else
                {
                    ++perfCounters.MovesRejected;
                }

                this.FinancialValue.TryAddMove(coordinate, acceptedFinancialValue, candidateFinancialValue);
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}