using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class ThresholdAccepting : Heuristic
    {
        public int IterationsPerThreshold { get; set; }
        public List<float> Thresholds { get; private set; }

        public ThresholdAccepting(OrganonStand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, harvestPeriods, planningPeriods, objective)
        {
            this.IterationsPerThreshold = 5 * stand.GetTreeRecordCount();
            this.Thresholds = new List<float>() { 0.90F, 0.92F, 0.95F, 0.97F, 0.99F, 1.0F };

            this.ObjectiveFunctionByIteration = new List<float>(this.Thresholds.Count * this.IterationsPerThreshold)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetColumnName()
        {
            return "ThresholdAccepting";
        }

        // similar to SimulatedAnnealing.Run(), differences are in move acceptance
        public override TimeSpan Run()
        {
            if (this.IterationsPerThreshold < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.IterationsPerThreshold));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }
            if (this.Thresholds.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Thresholds));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float currentObjectiveFunction = this.BestObjectiveFunction;
            //float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            float treeIndexScalingFactor = ((float)this.GetInitialTreeRecordCount() - Constant.RoundToZeroTolerance) / (float)UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            foreach (double threshold in this.Thresholds)
            {
                for (int iteration = 0; iteration < this.IterationsPerThreshold; ++iteration)
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
                        this.CurrentTrajectory.Copy(candidateTrajectory);
                        if (currentObjectiveFunction > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = currentObjectiveFunction;
                            this.BestTrajectory.Copy(this.CurrentTrajectory);
                        }
                    }
                    else
                    {
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }

                    this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
