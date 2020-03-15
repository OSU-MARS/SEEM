using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon.Heuristics
{
    public class RecordToRecordTravel : Heuristic
    {
        public float Deviation { get; set; }
        public int StopAfter { get; set; }

        public RecordToRecordTravel(Stand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, harvestPeriods, planningPeriods, objective)
        {
            this.Deviation = 100.0F;
            this.StopAfter = 1000;

            this.ObjectiveFunctionByIteration = new List<float>(1000 * 1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetColumnName()
        {
            return "RecordTravel";
        }

        public override TimeSpan Run()
        {
            if (this.Deviation <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Deviation));
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
            double minimumAcceptableObjectiveFunction = this.BestObjectiveFunction - this.Deviation;
            float treeIndexScalingFactor = ((float)this.TreeRecordCount - Constant.RoundToZeroTolerance) / (float)UInt16.MaxValue;

            StandTrajectory candidateTrajectory = new StandTrajectory(this.CurrentTrajectory);
            while (iterationsSinceBestObjectiveImproved < this.StopAfter)
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

                candidateTrajectory.IndividualTreeSelection[treeIndex] = candidateHarvestPeriod;
                candidateTrajectory.Simulate();
                ++iterationsSinceBestObjectiveImproved;

                float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                if (candidateObjectiveFunction > minimumAcceptableObjectiveFunction)
                {
                    currentObjectiveFunction = candidateObjectiveFunction;
                    this.CurrentTrajectory.Copy(candidateTrajectory);
                    if (currentObjectiveFunction > this.BestObjectiveFunction)
                    {
                        this.BestObjectiveFunction = currentObjectiveFunction;
                        this.BestTrajectory.Copy(this.CurrentTrajectory);
                        iterationsSinceBestObjectiveImproved = 0;
                        minimumAcceptableObjectiveFunction = this.BestObjectiveFunction - this.Deviation;
                    }
                }
                else
                {
                    candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
