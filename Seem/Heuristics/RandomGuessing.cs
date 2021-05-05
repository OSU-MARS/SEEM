using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RandomGuessing : Heuristic
    {
        public float CentralSelectionPercentage { get; set; }
        public int Iterations { get; set; }
        public float SelectionPercentageWidth { get; set; }

        public RandomGuessing(OrganonStand stand, OrganonConfiguration configuration, Objective objective, HeuristicParameters parameters)
            : base(stand, configuration, objective, parameters)
        {
            if (parameters.ConstructionRandomness != Constant.GraspDefault.FullyRandomConstruction)
            {
                throw new NotSupportedException(nameof(parameters));
            }

            this.CentralSelectionPercentage = parameters.ProportionalPercentage;
            this.Iterations = 4 * stand.GetTreeRecordCount();
            this.SelectionPercentageWidth = 20.0F;
        }

        public override string GetName()
        {
            return "Random";
        }

        public override HeuristicPerformanceCounters Run()
        {
            if ((this.CentralSelectionPercentage < 0.0F) || (this.CentralSelectionPercentage > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.CentralSelectionPercentage));
            }
            if ((this.SelectionPercentageWidth < 0.0F) || (this.SelectionPercentageWidth > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.SelectionPercentageWidth));
            }

            float minSelectionPercentage = this.CentralSelectionPercentage - 0.5F * this.SelectionPercentageWidth;
            float maxSelectionPercentage = this.CentralSelectionPercentage + 0.5F * this.SelectionPercentageWidth;
            if ((minSelectionPercentage < 0.0F) || (maxSelectionPercentage > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.SelectionPercentageWidth));
            }

            if (this.Iterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Iterations));
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            this.AcceptedObjectiveFunctionByMove.Capacity = this.Iterations;
            this.CandidateObjectiveFunctionByMove.Capacity = this.Iterations;

            float selectionPercentageScaling = this.SelectionPercentageWidth / (float)UInt16.MaxValue;
            for (int iteration = 0; iteration < this.Iterations; ++iteration)
            {
                float proportionalPercentage = minSelectionPercentage + selectionPercentageScaling * this.GetTwoPseudorandomBytesAsFloat();
                this.ConstructTreeSelection(1.0F, proportionalPercentage);
                perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();

                float candidateObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
                if (candidateObjectiveFunction > this.BestObjectiveFunction)
                {
                    // accept change of tree selection if it improves upon the best solution
                    this.BestObjectiveFunction = candidateObjectiveFunction;
                    this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
                    ++perfCounters.MovesAccepted;
                }
                else
                {
                    ++perfCounters.MovesRejected;
                }

                this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                this.CandidateObjectiveFunctionByMove.Add(candidateObjectiveFunction);
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}