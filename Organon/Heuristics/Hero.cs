using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon.Heuristics
{
    public class Hero : Heuristic
    {
        public int Iterations { get; set; }

        public Hero(Stand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, harvestPeriods, planningPeriods, objective)
        {
            this.Iterations = 100;

            this.ObjectiveFunctionByIteration = new List<float>(1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetColumnName()
        {
            return "Hero";
        }

        public override TimeSpan Run()
        {
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

            float currentObjectiveFunction = this.BestObjectiveFunction;
            float previousObjectiveFunction = this.BestObjectiveFunction;
            StandTrajectory candidateTrajectory = new StandTrajectory(this.CurrentTrajectory);
            for (int neighborhoodEvaluation = 0; neighborhoodEvaluation < this.Iterations; ++neighborhoodEvaluation)
            {
                for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
                {
                    // evaluate other cut option
                    int currentHarvestPeriod = this.CurrentTrajectory.IndividualTreeSelection[treeIndex];
                    int candidateHarvestPeriod = currentHarvestPeriod == 0 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                    candidateTrajectory.Simulate();

                    float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                    if (candidateObjectiveFunction > currentObjectiveFunction)
                    {
                        // accept change of no cut-cut decision if it improves upon the best solution
                        currentObjectiveFunction = candidateObjectiveFunction;
                        this.CurrentTrajectory.Copy(candidateTrajectory);
                    }
                    else
                    {
                        // otherwise, revert changes candidate trajectory for considering next tree's move
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
                if (currentObjectiveFunction <= previousObjectiveFunction)
                {
                    // convergence: stop if no improvement
                    break;
                }
                previousObjectiveFunction = currentObjectiveFunction;
            }

            this.BestObjectiveFunction = currentObjectiveFunction;
            this.BestTrajectory.Copy(this.CurrentTrajectory);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
