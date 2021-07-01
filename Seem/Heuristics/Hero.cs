using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class Hero : SingleTreeHeuristic<HeuristicParameters>
    {
        public bool IsStochastic { get; set; }
        public int MaximumIterations { get; set; }

        public Hero(OrganonStand stand, HeuristicParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters)
        {
            this.IsStochastic = false;
            this.MaximumIterations = Constant.HeuristicDefault.HeroMaximumIterations;
        }

        public override string GetName()
        {
            if (this.IsStochastic)
            {
                return "HeroStochastic";
            }
            return "Hero";
        }

        public override HeuristicPerformanceCounters Run(HeuristicResultPosition position, HeuristicResults results)
        {
            if (this.MaximumIterations < 1)
            {
                throw new InvalidOperationException(nameof(this.MaximumIterations));
            }

            IList<int> harvestPeriods = this.CurrentTrajectory.Treatments.GetHarvestPeriods();

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(position, results);
            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            float acceptedFinancialValue = this.EvaluateInitialSelection(this.MaximumIterations * initialTreeRecordCount, perfCounters);
            float previousBestObjectiveFunction = acceptedFinancialValue;
            OrganonStandTrajectory candidateTrajectory = new(this.CurrentTrajectory);
            bool decrementPeriodIndex = false;
            int[] uncompactedPeriodIndices = this.CurrentTrajectory.GetHarvestPeriodIndices(harvestPeriods);
            int[] uncompactedTreeIndices = ArrayExtensions.CreateSequentialIndices(initialTreeRecordCount);
            for (int iteration = 0; iteration < this.MaximumIterations; ++iteration)
            {
                // randomize on every iteration since a single randomization against the order of the data has little effect
                if (this.IsStochastic)
                {
                    this.Pseudorandom.Shuffle(uncompactedTreeIndices);
                }

                for (int sourceTreeIndex = 0; sourceTreeIndex < initialTreeRecordCount; ++sourceTreeIndex)
                {
                    if (this.IsStochastic)
                    {
                        decrementPeriodIndex = this.Pseudorandom.GetPseudorandomByteAsProbability() < 0.5F;
                    }

                    // evaluate other cut option
                    int treeIndex = uncompactedTreeIndices[sourceTreeIndex];
                    int currentPeriodIndex = uncompactedPeriodIndices[treeIndex];
                    int candidatePeriodIndex = decrementPeriodIndex ? currentPeriodIndex - 1 : currentPeriodIndex + 1;

                    for (int periodEvaluationForTree = 0; periodEvaluationForTree < harvestPeriods.Count - 1; ++periodEvaluationForTree)
                    {
                        if (candidatePeriodIndex >= harvestPeriods.Count)
                        {
                            candidatePeriodIndex = 0;
                        }
                        else if (candidatePeriodIndex < 0)
                        {
                            candidatePeriodIndex = harvestPeriods.Count - 1;
                        }
                        int candidateHarvestPeriod = harvestPeriods[candidatePeriodIndex];
                        int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex); // capture for revert
                        Debug.Assert(currentHarvestPeriod != candidateHarvestPeriod);
                        candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                        perfCounters.GrowthModelTimesteps += candidateTrajectory.Simulate();

                        float candidateFinancialValue = this.GetFinancialValue(candidateTrajectory, position.DiscountRateIndex);
                        if (candidateFinancialValue > acceptedFinancialValue)
                        {
                            // accept change of no cut-cut decision if it improves upon the best solution
                            acceptedFinancialValue = candidateFinancialValue;
                            uncompactedPeriodIndices[treeIndex] = candidatePeriodIndex;
                            this.CurrentTrajectory.CopyTreeGrowthFrom(candidateTrajectory);
                            ++perfCounters.MovesAccepted;
                        }
                        else
                        {
                            // otherwise, revert changes candidate trajectory for considering next tree's move
                            candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                            ++perfCounters.MovesRejected;
                        }

                        this.FinancialValue.AddMove(acceptedFinancialValue, candidateFinancialValue);
                        this.MoveLog.TreeIDByMove.Add(treeIndex);

                        if (decrementPeriodIndex)
                        {
                            --candidatePeriodIndex;
                        }
                        else
                        {
                            ++candidatePeriodIndex;
                        }
                    }
                }

                if (acceptedFinancialValue <= previousBestObjectiveFunction)
                {
                    // convergence: stop if no improvement
                    break;
                }
                previousBestObjectiveFunction = acceptedFinancialValue;
            }

            this.CopyTreeGrowthToBestTrajectory(this.CurrentTrajectory);

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}
