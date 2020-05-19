using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class Hero : Heuristic
    {
        private readonly bool isStochastic;

        public int Iterations { get; set; }

        public Hero(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, bool isStochastic)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            this.isStochastic = isStochastic;
            this.Iterations = 100;

            this.ObjectiveFunctionByMove = new List<float>(1000)
            {
                this.BestObjectiveFunction
            };
        }

        private int[] CreateSequentialArray(int length)
        {
            Debug.Assert(length > 0);

            int[] array = new int[length];
            for (int index = 0; index < length; ++index)
            {
                array[index] = index;
            }
            return array;
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
            int[] randomizedTreeIndices = this.isStochastic ? this.CreateSequentialArray(initialTreeRecordCount) : null;
            for (int iteration = 0; iteration < this.Iterations; ++iteration)
            {
                if (this.isStochastic)
                {
                    this.ShuffleRandom(randomizedTreeIndices);
                }

                for (int iterationMove = 0; iterationMove < initialTreeRecordCount; ++iterationMove)
                {
                    // evaluate other cut option
                    int treeIndex = this.isStochastic ? randomizedTreeIndices[iterationMove] : iterationMove;
                    int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex);
                    int candidateHarvestPeriod = currentHarvestPeriod == 0 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                    candidateTrajectory.Simulate();

                    float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                    if (candidateObjectiveFunction > currentObjectiveFunction)
                    {
                        // accept change of no cut-cut decision if it improves upon the best solution
                        currentObjectiveFunction = candidateObjectiveFunction;
                        this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                    }
                    else
                    {
                        // otherwise, revert changes candidate trajectory for considering next tree's move
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }

                    this.ObjectiveFunctionByMove.Add(currentObjectiveFunction);
                }

                if (currentObjectiveFunction <= previousObjectiveFunction)
                {
                    // convergence: stop if no improvement
                    break;
                }
                previousObjectiveFunction = currentObjectiveFunction;
            }

            this.BestObjectiveFunction = currentObjectiveFunction;
            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
