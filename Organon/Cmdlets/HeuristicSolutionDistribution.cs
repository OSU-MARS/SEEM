using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class HeuristicSolutionDistribution
    {
        public List<float> BestObjectiveFunctionByRun { get; private set; }
        public Heuristic BestSolution { get; private set; }

        public List<int> CountByMove { get; private set; }
        public List<float> MaximumObjectiveFunctionByMove { get; private set; }
        public List<float> MeanObjectiveFunctionByMove { get; private set; }
        public List<float> MinimumObjectiveFunctionByMove { get; private set; }
        public List<float> VarianceByMove { get; private set; }

        public TimeSpan TotalCoreSeconds { get; private set; }
        public int TotalMoves { get; private set; }
        public int TotalRuns { get; private set; }

        public HeuristicSolutionDistribution()
        {
            this.BestObjectiveFunctionByRun = new List<float>();
            this.BestSolution = null;
            this.CountByMove = new List<int>();
            this.MaximumObjectiveFunctionByMove = new List<float>();
            this.MeanObjectiveFunctionByMove = new List<float>();
            this.MinimumObjectiveFunctionByMove = new List<float>();
            this.TotalCoreSeconds = TimeSpan.Zero;
            this.TotalMoves = 0;
            this.TotalRuns = 0;
            this.VarianceByMove = new List<float>();
        }

        public void AddRun(Heuristic heuristic, TimeSpan coreSeconds)
        {
            this.BestObjectiveFunctionByRun.Add(heuristic.BestObjectiveFunction);

            for (int moveIndex = 0; moveIndex < heuristic.ObjectiveFunctionByMove.Count; ++moveIndex)
            {
                float objectiveFunction = heuristic.ObjectiveFunctionByMove[moveIndex];
                if (moveIndex >= this.CountByMove.Count)
                {
                    this.CountByMove.Add(1);
                    this.MaximumObjectiveFunctionByMove.Add(objectiveFunction);
                    this.MeanObjectiveFunctionByMove.Add(objectiveFunction);
                    this.MinimumObjectiveFunctionByMove.Add(objectiveFunction);
                    this.VarianceByMove.Add(objectiveFunction * objectiveFunction);
                }
                else
                {
                    ++this.CountByMove[moveIndex];

                    float maxObjectiveFunction = this.MaximumObjectiveFunctionByMove[moveIndex];
                    if (objectiveFunction > maxObjectiveFunction)
                    {
                        this.MaximumObjectiveFunctionByMove[moveIndex] = objectiveFunction;
                    }

                    // division and convergence to variance are done in OnRunsComplete()
                    this.MeanObjectiveFunctionByMove[moveIndex] += objectiveFunction;
                    this.VarianceByMove[moveIndex] += objectiveFunction * objectiveFunction;

                    float minObjectiveFunction = this.MinimumObjectiveFunctionByMove[moveIndex];
                    if (objectiveFunction < minObjectiveFunction)
                    {
                        this.MinimumObjectiveFunctionByMove[moveIndex] = objectiveFunction;
                    }
                }
            }

            this.TotalCoreSeconds += coreSeconds;
            this.TotalMoves += heuristic.ObjectiveFunctionByMove.Count;
            ++this.TotalRuns;

            if ((this.BestSolution == null) || (heuristic.BestObjectiveFunction > this.BestSolution.BestObjectiveFunction))
            {
                this.BestSolution = heuristic;
            }
        }

        public void OnRunsComplete()
        {
            Debug.Assert(this.MeanObjectiveFunctionByMove.Count == this.VarianceByMove.Count);

            // find mean objective function
            for (int moveIndex = 0; moveIndex < this.MeanObjectiveFunctionByMove.Count; ++moveIndex)
            {
                float runsAsFloat = this.CountByMove[moveIndex];
                this.MeanObjectiveFunctionByMove[moveIndex] /= runsAsFloat;
                this.VarianceByMove[moveIndex] = this.VarianceByMove[moveIndex] / runsAsFloat - this.MeanObjectiveFunctionByMove[moveIndex] * this.MeanObjectiveFunctionByMove[moveIndex];
            }
        }
    }
}
