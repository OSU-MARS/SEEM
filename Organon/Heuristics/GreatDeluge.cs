using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GreatDeluge : Heuristic
    {
        public float FinalMultiplier { get; set; }
        public float IntitialMultiplier { get; set; }
        public int Iterations { get; set; }
        public int LowerWaterAfter { get; set; }
        public float LowerWaterBy { get; set; }
        public float RainRate { get; set; }
        public int StopAfter { get; set; }

        public GreatDeluge(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            int treeRecords = stand.GetTreeRecordCount();
            this.FinalMultiplier = 2.0F;
            this.IntitialMultiplier = 1.5F;
            this.Iterations = 10 * treeRecords;
            this.LowerWaterAfter = treeRecords;
            this.LowerWaterBy = 0.01F;
            this.RainRate = (this.FinalMultiplier - this.IntitialMultiplier) * this.BestObjectiveFunction / this.Iterations;
            this.StopAfter = (int)(0.25F * this.Iterations);

            this.ObjectiveFunctionByMove = new List<float>(this.Iterations)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetName()
        {
            return "Deluge";
        }

        public override void CopySelectionsFrom(StandTrajectory trajectory)
        {
            base.CopySelectionsFrom(trajectory);
            this.RainRate = (this.FinalMultiplier - this.IntitialMultiplier) * this.BestObjectiveFunction / this.Iterations;
        }

        public override TimeSpan Run()
        {
            if (this.FinalMultiplier < this.IntitialMultiplier)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FinalMultiplier));
            }
            if (this.RainRate <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.RainRate));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }
            if (this.LowerWaterAfter < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.LowerWaterAfter));
            }
            if (this.StopAfter < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.StopAfter));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float currentObjectiveFunction = this.BestObjectiveFunction;
            //float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            int iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;
            int iterationsSinceBestObjectiveImproved = 0;
            float treeIndexScalingFactor = ((float)this.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / (float)UInt16.MaxValue;

            // initial solution in constructor is considered iteration 0, so loop starts with iteration 1
            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            float hillClimbingThreshold = this.BestObjectiveFunction;
            float waterLevel = this.IntitialMultiplier * this.BestObjectiveFunction;
            for (int iteration = 1; iteration < this.Iterations; ++iteration, waterLevel += this.RainRate)
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
                ++iterationsSinceObjectiveImprovedOrWaterLevelLowered;
                ++iterationsSinceBestObjectiveImproved;

                float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                if ((candidateObjectiveFunction > waterLevel) || (candidateObjectiveFunction > hillClimbingThreshold))
                {
                    // accept move
                    currentObjectiveFunction = candidateObjectiveFunction;
                    this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                    hillClimbingThreshold = currentObjectiveFunction;
                    iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;

                    if (currentObjectiveFunction > this.BestObjectiveFunction)
                    {
                        this.BestObjectiveFunction = currentObjectiveFunction;
                        this.BestTrajectory.CopyFrom(this.CurrentTrajectory);

                        iterationsSinceBestObjectiveImproved = 0;
                    }
                }
                else
                {
                    // undo move
                    candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                }

                this.ObjectiveFunctionByMove.Add(currentObjectiveFunction);
                if (iterationsSinceBestObjectiveImproved > this.StopAfter)
                {
                    break;
                }
                else if (iterationsSinceObjectiveImprovedOrWaterLevelLowered > this.LowerWaterAfter)
                {
                    // could also adjust rain rate but there but does not seem to be a clear need to do so
                    waterLevel = (1.0F - this.LowerWaterBy) * this.BestObjectiveFunction;
                    hillClimbingThreshold = waterLevel;
                    iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;
                }

                if (iteration == this.ChainFrom)
                {
                    this.BestTrajectoryByMove.Add(iteration, new StandTrajectory(this.BestTrajectory));
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
