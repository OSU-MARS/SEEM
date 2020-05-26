using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RecordTravel : Heuristic
    {
        public float FixedDeviation { get; set; }
        public float RelativeDeviation { get; set; }
        public int StopAfter { get; set; }

        public RecordTravel(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            int treeRecordCount = stand.GetTreeRecordCount();
            this.FixedDeviation = 0.0F;
            this.RelativeDeviation = 1.5F / treeRecordCount;
            this.StopAfter = 1000;

            this.ObjectiveFunctionByMove = new List<float>(5 * treeRecordCount)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetName()
        {
            return "RecordTravel";
        }

        public override TimeSpan Run()
        {
            if ((this.RelativeDeviation < 0.0) || (this.RelativeDeviation > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.RelativeDeviation));
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
            double minimumAcceptableObjectiveFunction = this.BestObjectiveFunction - this.RelativeDeviation * MathF.Abs(this.BestObjectiveFunction) - this.FixedDeviation;
            float treeIndexScalingFactor = ((float)this.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / (float)UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            while (iterationsSinceBestObjectiveImproved < this.StopAfter)
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
                ++iterationsSinceBestObjectiveImproved;

                float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                if (candidateObjectiveFunction > minimumAcceptableObjectiveFunction)
                {
                    currentObjectiveFunction = candidateObjectiveFunction;
                    this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                    if (currentObjectiveFunction > this.BestObjectiveFunction)
                    {
                        this.BestObjectiveFunction = currentObjectiveFunction;
                        this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
                        iterationsSinceBestObjectiveImproved = 0;
                        minimumAcceptableObjectiveFunction = this.BestObjectiveFunction - this.RelativeDeviation * MathF.Abs(this.BestObjectiveFunction) - this.FixedDeviation;
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

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
