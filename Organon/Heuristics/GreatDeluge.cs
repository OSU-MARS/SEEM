using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GreatDeluge : Heuristic
    {
        public float FinalWaterLevel { get; set; }
        public float InitialWaterLevel { get; set; }
        public float RainRate { get; set; }
        public int StopAfter { get; set; }

        public GreatDeluge(OrganonStand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, harvestPeriods, planningPeriods, objective)
        {
            this.FinalWaterLevel = 100.0F;
            this.InitialWaterLevel = 0.0F;
            this.RainRate = this.FinalWaterLevel / (10 * 1000);
            this.StopAfter = 1000;

            this.ObjectiveFunctionByIteration = new List<float>(1000 * 1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetColumnName()
        {
            return "Deluge";
        }

        public override TimeSpan Run()
        {
            if (this.FinalWaterLevel <= this.InitialWaterLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FinalWaterLevel));
            }
            if (this.RainRate <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.RainRate));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }
            if (this.StopAfter < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.StopAfter));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float currentObjectiveFunction = this.BestObjectiveFunction;
            //float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            int iterationsSinceBestObjectiveImproved = 0;
            float treeIndexScalingFactor = ((float)this.TreeRecordCount - Constant.RoundToZeroTolerance) / (float)UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            for (double waterLevel = this.InitialWaterLevel; waterLevel < this.FinalWaterLevel; waterLevel += this.RainRate)
            {
                int treeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int currentHarvestPeriod = this.CurrentTrajectory.IndividualTreeSelection[treeIndex];
                int candidateHarvestPeriod = currentHarvestPeriod == 0 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                //int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                //while (candidateHarvestPeriod == currentHarvestPeriod)
                //{
                //    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                //}
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
