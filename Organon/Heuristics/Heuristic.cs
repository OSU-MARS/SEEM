using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class Heuristic : RandomNumberConsumer
    {
        public float BestObjectiveFunction { get; protected set; }
        public OrganonStandTrajectory BestTrajectory { get; protected set; }
        public OrganonStandTrajectory CurrentTrajectory { get; protected set; }
        public Objective Objective { get; protected set; }
        public List<float> ObjectiveFunctionByIteration { get; protected set; }

        protected Heuristic(OrganonStand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods, Objective objective)
        {
            this.BestTrajectory = new OrganonStandTrajectory(stand, organonConfiguration, harvestPeriods, planningPeriods, objective.VolumeUnits);
            this.Objective = objective;

            this.BestTrajectory.Simulate();
            this.BestObjectiveFunction = this.GetObjectiveFunction(this.BestTrajectory);
            this.CurrentTrajectory = new OrganonStandTrajectory(this.BestTrajectory);

            string name = this.GetName();
            this.BestTrajectory.Name = name + this.BestTrajectory.Name + "Best";
            this.CurrentTrajectory.Name = name + this.CurrentTrajectory.Name + "Current";
        }

        public abstract string GetName();

        public float GetObjectiveFunction(OrganonStandTrajectory trajectory)
        {
            Debug.Assert(trajectory.PeriodLengthInYears > 0);

            // find objective function value
            // Volume objective functions are in m³/ha or MBF/ac.
            float objectiveFunction = 0.0F;
            if (this.Objective.IsNetPresentValue)
            {
                if (trajectory.VolumeUnits == VolumeUnits.CubicMetersPerHectare)
                {
                    // TODO: also, check tree model is using a five year time step
                    throw new NotSupportedException();
                }

                // net present value
                // Harvest and standing volumes are in board feet and prices are in MBF, hence multiplications by 0.001.
                // TODO: support per species pricing
                for (int periodIndex = 1; periodIndex < trajectory.HarvestVolumesByPeriod.Length; ++periodIndex)
                {
                    float thinVolumeInBF = trajectory.HarvestVolumesByPeriod[periodIndex];
                    if (thinVolumeInBF > 0.0)
                    {
                        objectiveFunction += this.Objective.GetPresentValueOfThinScribner(thinVolumeInBF, periodIndex - 1, trajectory.PeriodLengthInYears);
                    }
                }

                objectiveFunction += this.Objective.GetPresentValueOfFinalHarvestScribner(trajectory.StandingVolumeByPeriod[^1], trajectory.StandingVolumeByPeriod.Length - 1, trajectory.PeriodLengthInYears);

                // convert from US$/ac to k$/ac
                objectiveFunction *= 0.001F;
            }
            else
            {
                // direct volume addition
                for (int periodIndex = 1; periodIndex < trajectory.HarvestVolumesByPeriod.Length; ++periodIndex)
                {
                    objectiveFunction += trajectory.HarvestVolumesByPeriod[periodIndex];
                }
                objectiveFunction += trajectory.StandingVolumeByPeriod[^1];
                if (trajectory.VolumeUnits == VolumeUnits.ScribnerBoardFeetPerAcre)
                {
                    objectiveFunction *= 0.001F;
                }
            }

            return objectiveFunction;
        }

        protected int GetInitialTreeRecordCount()
        {
            return this.CurrentTrajectory.StandByPeriod[0].GetTreeRecordCount();
        }

        public void RandomizeSchedule()
        {
            int initialTreeRecordCount = this.GetInitialTreeRecordCount();
            if (this.Objective.HarvestPeriodSelection == HarvestPeriodSelection.All)
            {
                float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
                for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
                {
                    int harvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                    this.CurrentTrajectory.SetTreeSelection(treeIndex, harvestPeriod);
                }
            }
            else if (this.Objective.HarvestPeriodSelection == HarvestPeriodSelection.NoneOrLast)
            {
                float unityScalingFactor = 1.0F / (float)byte.MaxValue;
                for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
                {
                    int harvestPeriod = unityScalingFactor * this.GetPseudorandomByteAsFloat() > 0.5 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    this.CurrentTrajectory.SetTreeSelection(treeIndex, harvestPeriod);
                }
            }
            else
            {
                throw new NotSupportedException(String.Format("Unhandled harvest period selection {0}.", this.Objective.HarvestPeriodSelection));
            }

            this.CurrentTrajectory.Simulate();
            this.BestTrajectory.Copy(this.CurrentTrajectory);

            this.BestObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
            if (this.ObjectiveFunctionByIteration.Count > 0)
            {
                this.ObjectiveFunctionByIteration[0] = this.BestObjectiveFunction;
            }
        }

        public abstract TimeSpan Run();
    }
}
