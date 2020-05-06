using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class Hero : Heuristic
    {
        public int Iterations { get; set; }

        public Hero(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            this.Iterations = 100;

            this.ObjectiveFunctionByIteration = new List<float>(1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetName()
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
            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            int initialTreeRecordCount = this.GetInitialTreeRecordCount();
            for (int iteration = 0; iteration < this.Iterations; ++iteration)
            {
                for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
                {
                    // evaluate other cut option
                    int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex);
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
