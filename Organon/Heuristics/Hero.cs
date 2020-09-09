using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class Hero : SingleTreeHeuristic
    {
        public bool IsStochastic { get; set; }
        public int MaximumIterations { get; set; }

        public Hero(OrganonStand stand, OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
            : base(stand, organonConfiguration, objective, parameters)
        {
            this.IsStochastic = false;
            this.MaximumIterations = 50;
        }

        public override string GetName()
        {
            if (this.IsStochastic)
            {
                return "HeroStochastic";
            }
            return "Hero";
        }

        public override TimeSpan Run()
        {
            if (this.MaximumIterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaximumIterations));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int initialTreeRecordCount = this.GetInitialTreeRecordCount();
            this.EvaluateInitialSelection(this.MaximumIterations * initialTreeRecordCount);

            float acceptedObjectiveFunction = this.BestObjectiveFunction;
            float previousBestObjectiveFunction = this.BestObjectiveFunction;
            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            int[] treeIndices = this.CreateSequentialArray(initialTreeRecordCount);
            for (int iteration = 0; iteration < this.MaximumIterations; ++iteration)
            {
                if (this.IsStochastic)
                {
                    this.Pseudorandom.Shuffle(treeIndices);
                }

                for (int iterationMoveIndex = 0; iterationMoveIndex < initialTreeRecordCount; ++iterationMoveIndex)
                {
                    // evaluate other cut option
                    int treeIndex = treeIndices[iterationMoveIndex];
                    int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex);
                    int candidateHarvestPeriod = currentHarvestPeriod == 0 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                    candidateTrajectory.Simulate();

                    float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                    if (candidateObjectiveFunction > acceptedObjectiveFunction)
                    {
                        // accept change of no cut-cut decision if it improves upon the best solution
                        acceptedObjectiveFunction = candidateObjectiveFunction;
                        this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                    }
                    else
                    {
                        // otherwise, revert changes candidate trajectory for considering next tree's move
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }

                    this.AcceptedObjectiveFunctionByMove.Add(acceptedObjectiveFunction);
                    this.CandidateObjectiveFunctionByMove.Add(candidateObjectiveFunction);
                    this.MoveLog.TreeIDByMove.Add(treeIndex);
                }

                if (acceptedObjectiveFunction <= previousBestObjectiveFunction)
                {
                    // convergence: stop if no improvement
                    break;
                }
                previousBestObjectiveFunction = acceptedObjectiveFunction;
            }

            this.BestObjectiveFunction = acceptedObjectiveFunction;
            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
