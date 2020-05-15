using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RandomGuessing : Heuristic
    {
        public float CentralSelectionProbability { get; set; }
        public int Iterations { get; set; }
        public float SelectionProbabilityWidth { get; set; }

        public RandomGuessing(OrganonStand stand, OrganonConfiguration configuration, int planningPeriods, Objective objective, float centralSelectionProbability)
            : base(stand, configuration, planningPeriods, objective)
        {
            this.CentralSelectionProbability = centralSelectionProbability;
            this.Iterations = 4 * stand.GetTreeRecordCount();
            this.SelectionProbabilityWidth = 0.2F;

            this.ObjectiveFunctionByMove = new List<float>(1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetName()
        {
            return "Random";
        }

        public override TimeSpan Run()
        {
            if ((this.CentralSelectionProbability < 0.0F) || (this.CentralSelectionProbability > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.CentralSelectionProbability));
            }
            if ((this.SelectionProbabilityWidth < 0.0F) || (this.SelectionProbabilityWidth > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.SelectionProbabilityWidth));
            }

            float minSelectionProbability = this.CentralSelectionProbability - 0.5F * this.SelectionProbabilityWidth;
            float maxSelectionProbability = this.CentralSelectionProbability + 0.5F * this.SelectionProbabilityWidth;
            if ((minSelectionProbability < 0.0F) || (maxSelectionProbability > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.SelectionProbabilityWidth));
            }

            if (this.Iterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Iterations));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float selectionProbabilityScaling = this.SelectionProbabilityWidth / (float)UInt16.MaxValue;
            for (int iteration = 0; iteration < this.Iterations; ++iteration)
            {
                float selectionProbability = minSelectionProbability + selectionProbabilityScaling * this.GetTwoPseudorandomBytesAsFloat();
                this.RandomizeSelections(selectionProbability);
                this.CurrentTrajectory.Simulate();

                float candidateObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
                if (candidateObjectiveFunction > this.BestObjectiveFunction)
                {
                    // accept change of no cut-cut decision if it improves upon the best solution
                    this.BestObjectiveFunction = candidateObjectiveFunction;
                    this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
                }

                this.ObjectiveFunctionByMove.Add(candidateObjectiveFunction);
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
