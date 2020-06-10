using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RandomGuessing : Heuristic
    {
        public float CentralSelectionPercentage { get; set; }
        public int Iterations { get; set; }
        public float SelectionPercentageWidth { get; set; }

        public RandomGuessing(OrganonStand stand, OrganonConfiguration configuration, int planningPeriods, Objective objective, float centralSelectionPercentage)
            : base(stand, configuration, planningPeriods, objective)
        {
            this.CentralSelectionPercentage = centralSelectionPercentage;
            this.Iterations = 4 * stand.GetTreeRecordCount();
            this.SelectionPercentageWidth = 20.0F;

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
            if ((this.CentralSelectionPercentage < 0.0F) || (this.CentralSelectionPercentage > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.CentralSelectionPercentage));
            }
            if (this.ChainFrom != Constant.HeuristicDefault.ChainFrom)
            {
                throw new NotSupportedException(nameof(this.ChainFrom));
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
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float selectionPercentageScaling = this.SelectionPercentageWidth / (float)UInt16.MaxValue;
            for (int iteration = 0; iteration < this.Iterations; ++iteration)
            {
                float selectionPercentage = minSelectionPercentage + selectionPercentageScaling * this.GetTwoPseudorandomBytesAsFloat();
                this.RandomizeSelections(selectionPercentage);
                this.CurrentTrajectory.Simulate();

                float candidateObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
                if (candidateObjectiveFunction > this.BestObjectiveFunction)
                {
                    // accept change of tree selection if it improves upon the best solution
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