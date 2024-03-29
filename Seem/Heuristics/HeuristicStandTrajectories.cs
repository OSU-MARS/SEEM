﻿using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Heuristics
{
    public abstract class HeuristicStandTrajectories : SilviculturalSpace
    {
        public GraspReactivity GraspReactivity { get; private init; }

        protected HeuristicStandTrajectories(int parameterCombinations, IList<int> firstThinPeriods, IList<int> secondThinPeriods, IList<int> thirdThinPeriods, IList<int> rotationLengths, FinancialScenarios financialScenarios, int individualSolutionPoolSize)
            : base(parameterCombinations, firstThinPeriods, secondThinPeriods, thirdThinPeriods, rotationLengths, financialScenarios, individualSolutionPoolSize)
        {
            this.GraspReactivity = new();
        }

        public abstract HeuristicParameters GetParameters(int parameterIndex);
    }

    public class HeuristicStandTrajectories<TParameters> : HeuristicStandTrajectories where TParameters : HeuristicParameters
    {
        // TODO: refactor StandTrajectories' parameter combination dimension to here?
        public new IList<TParameters> ParameterCombinations { get; private init; }

        public HeuristicStandTrajectories(IList<TParameters> parameterCombinations, IList<int> firstThinPeriods, IList<int> secondThinPeriods, IList<int> thirdThinPeriods, IList<int> rotationLengths, FinancialScenarios financialScenarios, int individualSolutionPoolSize)
            : base(parameterCombinations.Count, firstThinPeriods, secondThinPeriods, thirdThinPeriods, rotationLengths, financialScenarios, individualSolutionPoolSize)
        {
            this.ParameterCombinations = parameterCombinations;
        }

        public void AssimilateIntoCoordinate(Heuristic heuristic, SilviculturalCoordinate coordinate, PrescriptionPerformanceCounters perfCounters)
        {
            // doesn't call base.AssimilateIntoCoordinate() due to 1) flow of heuristic to objective distribution and 2) checks for pool selection pressure
            StandTrajectory trajectory = heuristic.GetBestTrajectory(coordinate);
            this.VerifyStandEntries(trajectory, coordinate);

            SilviculturalCoordinateExploration element = this[coordinate];
            element.Distribution.Add(heuristic, coordinate, perfCounters);

            // additions to pools which haven't filled yet aren't informative to GRASP's reactive choice of α as there's no selection
            // pressure
            // One interpretation of this is the pool size sets the minimum amount of information needed to make decisions about the
            // value of solution diversity and objective function improvements.
            float financialValue = heuristic.FinancialValue.GetHighestValueWithDefaulting(coordinate);
            bool poolAlreadyFull = element.Pool.IsFull;
            bool acceptedAsEliteSolution = element.Pool.TryAddOrReplace(trajectory, financialValue, heuristic);
            this.GraspReactivity.Add(heuristic.ConstructionGreediness, acceptedAsEliteSolution && poolAlreadyFull);

            Debug.Assert((element.Pool.SolutionsInPool > 0) && 
                         (element.Pool.High.Heuristic != null) && (element.Pool.High.Trajectory != null) &&
                         (element.Pool.Low.Heuristic != null) && (element.Pool.Low.Trajectory != null));
        }

        public override HeuristicParameters GetParameters(int parameterIndex)
        {
            return this.ParameterCombinations[parameterIndex];
        }
    }
}
