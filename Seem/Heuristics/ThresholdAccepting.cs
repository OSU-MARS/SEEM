using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public class ThresholdAccepting : SingleTreeHeuristic<HeuristicParameters>
    {
        public List<int> IterationsPerThreshold { get; private init; }
        public List<float> Thresholds { get; private init; }

        public ThresholdAccepting(OrganonStand stand, HeuristicParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters)
        {
            int treeRecords = stand.GetTreeRecordCount();
            this.IterationsPerThreshold = new List<int>() { (int)(11.5F * treeRecords), 25, (int)(7.5F * treeRecords) };
            this.Thresholds = new List<float>() { 1.0F, 0.999F, 1.0F };
        }

        public override string GetName()
        {
            return "ThresholdAccepting";
        }

        // similar to SimulatedAnnealing.Run(), differences are in move acceptance
        public override HeuristicPerformanceCounters Run(HeuristicResultPosition position, HeuristicResults results)
        {
            if (this.IterationsPerThreshold.Count < 1)
            {
                throw new InvalidOperationException(nameof(this.IterationsPerThreshold));
            }
            if (this.Thresholds.Count != this.IterationsPerThreshold.Count)
            {
                throw new InvalidOperationException(nameof(this.Thresholds));
            }
            foreach (float threshold in this.Thresholds)
            {
                if ((threshold < 0.0F) || (threshold > 1.0F))
                {
                    throw new InvalidOperationException(nameof(this.Thresholds));
                }
            }

            IList<int> thinningPeriods = this.CurrentTrajectory.Treatments.GetHarvestPeriods();
            if ((thinningPeriods.Count < 2) || (thinningPeriods.Count > 3))
            {
                throw new NotSupportedException("Currently, only one or two thins are supported.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(position, results);
            this.EvaluateInitialSelection(this.IterationsPerThreshold.Sum(), perfCounters);

            float acceptedFinancialValue = this.FinancialValue.GetHighestValue();
            float treeIndexScalingFactor = (this.CurrentTrajectory.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new(this.CurrentTrajectory);
            for (int thresholdIndex = 0; thresholdIndex < this.Thresholds.Count; ++thresholdIndex)
            {
                float iterations = this.IterationsPerThreshold[thresholdIndex];
                float threshold = this.Thresholds[thresholdIndex];
                for (int iterationInThreshold = 0; iterationInThreshold < iterations; ++iterationInThreshold)
                {
                    // if needed, support two opt moves
                    int treeIndex = (int)(treeIndexScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                    int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex);
                    int candidateHarvestPeriod = this.GetOneOptCandidateRandom(currentHarvestPeriod, thinningPeriods);
                    Debug.Assert(candidateHarvestPeriod >= 0);

                    candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                    perfCounters.GrowthModelTimesteps += candidateTrajectory.Simulate();

                    float candidateFinancialValue = this.GetFinancialValue(candidateTrajectory, position.DiscountRateIndex);
                    bool acceptMove = candidateFinancialValue > threshold * acceptedFinancialValue;
                    if (acceptMove)
                    {
                        acceptedFinancialValue = candidateFinancialValue; 
                        this.CurrentTrajectory.CopyTreeGrowthFrom(candidateTrajectory);
                        ++perfCounters.MovesAccepted;

                        if (acceptedFinancialValue > this.FinancialValue.GetHighestValue())
                        {
                            this.CopyTreeGrowthToBestTrajectory(this.CurrentTrajectory);
                        }
                    }
                    else
                    {
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                        ++perfCounters.MovesRejected;
                    }

                    this.FinancialValue.AddMove(acceptedFinancialValue, candidateFinancialValue);
                    this.MoveLog.TreeIDByMove.Add(treeIndex);
                }
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}
