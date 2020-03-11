using System;
using System.Collections.Generic;

namespace Osu.Cof.Organon.Heuristics
{
    public abstract class Heuristic : RandomNumberConsumer
    {
        public float BestObjectiveFunction { get; protected set; }
        public StandTrajectory BestTrajectory { get; protected set; }
        public StandTrajectory CurrentTrajectory { get; protected set; }
        public List<float> ObjectiveFunctionByIteration { get; protected set; }

        protected Heuristic(Stand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods, VolumeUnits volumeUnits)
        {
            this.BestTrajectory = new StandTrajectory(stand, organonConfiguration, harvestPeriods, planningPeriods, volumeUnits);
            this.CurrentTrajectory = new StandTrajectory(stand, organonConfiguration, harvestPeriods, planningPeriods, volumeUnits);

            this.BestTrajectory.Simulate();
            this.BestObjectiveFunction = this.GetObjectiveFunction(this.BestTrajectory);
            this.CurrentTrajectory.Copy(this.BestTrajectory);
        }
        
        protected int TreeRecordCount
        {
            get { return this.CurrentTrajectory.StandByPeriod[0].TreeRecordCount; }
        }

        public float GetObjectiveFunction(StandTrajectory trajectory)
        {
            // find objective function value
            // Volume objective functions are in m³/ha or MBF/ac
            float objectiveFunction = 0.0F;
            for (int periodIndex = 1; periodIndex < trajectory.HarvestVolumesByPeriod.Length; ++periodIndex)
            {
                objectiveFunction += trajectory.HarvestVolumesByPeriod[periodIndex];
            }
            objectiveFunction += trajectory.StandingVolumeByPeriod[trajectory.StandingVolumeByPeriod.Length - 1];
            if (trajectory.VolumeUnits == VolumeUnits.ScribnerBoardFeetPerAcre)
            {
                objectiveFunction *= 0.001F;
            }
            return objectiveFunction;
        }

        public void RandomizeSchedule()
        {
            float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                int harvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                this.CurrentTrajectory.SetTreeSelection(treeIndex, harvestPeriod);
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
