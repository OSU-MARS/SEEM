using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon.Heuristics
{
    public class GreatDeluge : Heuristic
    {
        public float FinalWaterLevel { get; set; }
        public float RainRate { get; set; }
        public int StopAfter { get; set; }

        public GreatDeluge(Stand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods)
            : base(stand, organonConfiguration, harvestPeriods, planningPeriods)
        {
            this.FinalWaterLevel = 100.0F;
            this.RainRate = this.FinalWaterLevel / (10 * 1000);
            this.StopAfter = 1000;

            this.ObjectiveFunctionByIteration = new List<float>(1000 * 1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override TimeSpan Run()
        {
            if (this.FinalWaterLevel <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FinalWaterLevel));
            }
            if (this.RainRate <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.RainRate));
            }
            if (this.StopAfter < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.StopAfter));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float currentObjectiveFunction = this.BestObjectiveFunction;
            float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            int iterationsSinceBestObjectiveImproved = 0;
            float treeIndexScalingFactor = ((float)this.TreeRecordCount - Constant.RoundToZeroTolerance) / (float)UInt16.MaxValue;

            StandTrajectory candidateTrajectory = new StandTrajectory(this.CurrentTrajectory);
            for (double waterLevel = 0; waterLevel < this.FinalWaterLevel; waterLevel += this.RainRate)
            {
                int treeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int currentHarvestPeriod = this.CurrentTrajectory.IndividualTreeSelection[treeIndex];
                int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                while (candidateHarvestPeriod == currentHarvestPeriod)
                {
                    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                }
                Debug.Assert(candidateHarvestPeriod >= 0);

                candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                candidateTrajectory.Simulate();
                ++iterationsSinceBestObjectiveImproved;

                float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                if ((candidateObjectiveFunction > waterLevel) || (candidateObjectiveFunction > this.BestObjectiveFunction))
                {
                    currentObjectiveFunction = candidateObjectiveFunction;
                    this.CurrentTrajectory.Copy(candidateTrajectory);
                    if (currentObjectiveFunction > this.BestObjectiveFunction)
                    {
                        this.BestObjectiveFunction = currentObjectiveFunction;
                        this.BestTrajectory.Copy(this.CurrentTrajectory);
                        iterationsSinceBestObjectiveImproved = 0;
                    }
                }
                else
                {
                    candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
                if (iterationsSinceBestObjectiveImproved > this.StopAfter)
                {
                    break;
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
