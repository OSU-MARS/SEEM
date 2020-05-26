using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public class ThresholdAccepting : Heuristic
    {
        public List<int> IterationsPerThreshold { get; private set; }
        public List<float> Thresholds { get; private set; }

        public ThresholdAccepting(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            int treeRecords = stand.GetTreeRecordCount();
            float oneTreeChange = 1.0F / treeRecords;
            this.IterationsPerThreshold = new List<int>() { 2 * treeRecords, 50, 200, 50, 250 };
            this.Thresholds = new List<float>() { 1.0F, 1.0F - 2.0F * oneTreeChange, 1.0F, 1.0F - oneTreeChange, 1.0F };

            this.ObjectiveFunctionByMove = new List<float>(this.IterationsPerThreshold.Sum())
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetName()
        {
            return "ThresholdAccepting";
        }

        // similar to SimulatedAnnealing.Run(), differences are in move acceptance
        public override TimeSpan Run()
        {
            if (this.IterationsPerThreshold.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.IterationsPerThreshold));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }
            if (this.Thresholds.Count != this.IterationsPerThreshold.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Thresholds));
            }
            foreach (float threshold in this.Thresholds)
            {
                if ((threshold < 0.0F) || (threshold > 1.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Thresholds));
                }
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float currentObjectiveFunction = this.BestObjectiveFunction;
            //float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            float treeIndexScalingFactor = ((float)this.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / (float)UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            for (int thresholdIndex = 0; thresholdIndex < this.Thresholds.Count; ++thresholdIndex)
            {
                float iterations = this.IterationsPerThreshold[thresholdIndex];
                float threshold = this.Thresholds[thresholdIndex];
                for (int iteration = 0; iteration < iterations; ++iteration)
                {
                    int treeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                    int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex);
                    int candidateHarvestPeriod = currentHarvestPeriod == 0 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    //int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                    //while (candidateHarvestPeriod == currentHarvestPeriod)
                    //{
                    //    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                    //}
                    Debug.Assert(candidateHarvestPeriod >= 0);

                    candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                    candidateTrajectory.Simulate();

                    float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                    bool acceptMove = candidateObjectiveFunction > threshold * currentObjectiveFunction;
                    if (acceptMove)
                    {
                        currentObjectiveFunction = candidateObjectiveFunction; 
                        this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                        if (currentObjectiveFunction > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = currentObjectiveFunction;
                            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
                        }
                    }
                    else
                    {
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }

                    this.ObjectiveFunctionByMove.Add(currentObjectiveFunction);

                    if (this.ObjectiveFunctionByMove.Count == this.ChainFrom)
                    {
                        this.BestTrajectoryByMove.Add(this.ChainFrom, new StandTrajectory(this.BestTrajectory));
                    }
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
