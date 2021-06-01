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

        public override HeuristicPerformanceCounters Run(HeuristicSolutionPosition position, HeuristicSolutionIndex solutionIndex)
        {
            if (this.Iterations < 1)
            {
                throw new InvalidOperationException(nameof(this.Iterations));
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            this.AcceptedObjectiveFunctionByMove.Capacity = this.Iterations;
            this.CandidateObjectiveFunctionByMove.Capacity = this.Iterations;

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(position, solutionIndex);
            this.EvaluateInitialSelection(this.Iterations, perfCounters);

            for (int iteration = 1; iteration < this.Iterations; ++iteration)
            {
                this.ConstructTreeSelection(this.HeuristicParameters.ConstructionGreediness);
                perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();

                float candidateObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
                if (candidateObjectiveFunction > this.BestObjectiveFunction)
                {
                    // accept change of tree selection if it improves upon the best solution
                    this.BestObjectiveFunction = candidateObjectiveFunction;
                    this.BestTrajectory.CopyTreeGrowthFrom(this.CurrentTrajectory);
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