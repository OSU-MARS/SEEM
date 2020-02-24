using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    public class StandTrajectory
    {
        private readonly FiaVolume fiaVolume;
        private readonly Dictionary<FiaCode, float[]> organonCalibration;
        private readonly OrganonConfiguration organonConfiguration;
        private readonly OrganonGrowth organonGrowth;

        public float[] HarvestVolumesByPeriod { get; protected set; }
        public float IndividualTreeExpansionFactor { get; protected set; }
        // harvest periods by tree, 0 indicates no harvest
        public int[] IndividualTreeSelection { get; set; }
        public StandDensity InitialDensity { get; protected set; }

        public Stand[] StandByPeriod { get; private set; }
        public float[] StandingVolumeByPeriod { get; set; }

        public StandTrajectory(Stand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods)
        {
            if (planningPeriods < harvestPeriods)
            {
                throw new ArgumentOutOfRangeException(nameof(planningPeriods));
            }

            this.fiaVolume = new FiaVolume();
            this.organonCalibration = organonConfiguration.CreateSpeciesCalibration();
            this.organonConfiguration = organonConfiguration;
            this.organonGrowth = new OrganonGrowth();
            this.HarvestVolumesByPeriod = new float[harvestPeriods + 1];
            this.InitialDensity = new StandDensity(stand, organonConfiguration.Variant);
            // TODO: check all trees in stand have same expansion factor
            this.IndividualTreeExpansionFactor = stand.LiveExpansionFactor[0];
            this.IndividualTreeSelection = new int[stand.TreeRecordCount];

            int maximumPlanningPeriodIndex = planningPeriods + 1;
            this.StandByPeriod = new Stand[maximumPlanningPeriodIndex];
            for (int periodIndex = 0; periodIndex < maximumPlanningPeriodIndex; ++periodIndex)
            {
                this.StandByPeriod[periodIndex] = stand.Clone();
            }

            this.StandingVolumeByPeriod = new float[maximumPlanningPeriodIndex];
        }

        // shallow copy FIA and Organon for now
        // deep copy of tree growth data
        public StandTrajectory(StandTrajectory other)
        {
            this.fiaVolume = other.fiaVolume;
            this.organonCalibration = other.organonCalibration;
            this.organonConfiguration = other.organonConfiguration;
            this.organonGrowth = other.organonGrowth;

            this.HarvestVolumesByPeriod = new float[other.HarvestPeriods];
            this.IndividualTreeExpansionFactor = other.IndividualTreeExpansionFactor;
            this.IndividualTreeSelection = new int[other.TreeRecordCount];
            this.InitialDensity = new StandDensity(other.InitialDensity);
            this.StandingVolumeByPeriod = new float[other.PlanningPeriods];
            this.StandByPeriod = new Stand[other.PlanningPeriods];

            Array.Copy(other.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod, 0, this.HarvestPeriods);
            Array.Copy(other.IndividualTreeSelection, 0, this.IndividualTreeSelection, 0, this.TreeRecordCount);
            Array.Copy(other.StandingVolumeByPeriod, 0, this.StandingVolumeByPeriod, 0, this.PlanningPeriods);
            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                this.StandByPeriod[periodIndex] = new Stand(other.StandByPeriod[periodIndex]);
            }
        }

        public int HarvestPeriods
        {
            get { return this.HarvestVolumesByPeriod.Length; }
        }

        public int PlanningPeriods
        {
            get { return this.StandingVolumeByPeriod.Length; }
        }

        public int TreeRecordCount
        {
            get { return this.IndividualTreeSelection.Length; }
        }

        public void Copy(StandTrajectory other)
        {
            if ((this.HarvestPeriods != other.HarvestPeriods) || (this.PlanningPeriods != other.PlanningPeriods))
            {
                // TODO: check rest of stand properties
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            Array.Copy(other.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod.Length);
            for (int periodIndex = 0; periodIndex < this.StandByPeriod.Length; ++periodIndex)
            {
                this.StandByPeriod[periodIndex].CopyTreeGrowth(other.StandByPeriod[periodIndex]);
            }
            Array.Copy(other.StandingVolumeByPeriod, 0, this.StandingVolumeByPeriod, 0, this.StandingVolumeByPeriod.Length);
            Array.Copy(other.IndividualTreeSelection, 0, this.IndividualTreeSelection, 0, this.IndividualTreeSelection.Length);
        }

        private void GetHarvestVolume(float[] harvestVolume)
        {
            for (int periodIndex = 1; periodIndex < this.HarvestPeriods; ++periodIndex)
            {
                // tree's expansion factor is set to zero when it's marked for harvest
                // Use tree's volume from the previous period.
                Stand previousStand = this.StandByPeriod[periodIndex - 1];
                float cvts4 = 0.0F;
                for (int treeIndex = 0; treeIndex < previousStand.TreeRecordCount; ++treeIndex)
                {
                    if (this.IndividualTreeSelection[treeIndex] == periodIndex)
                    {
                        Debug.Assert(previousStand.LiveExpansionFactor[treeIndex] > 0.0F);
                        cvts4 += this.fiaVolume.GetMerchantableCubicVolumePerHectare(previousStand, treeIndex);
                    }
                }
                harvestVolume[periodIndex] = cvts4;
            }
        }

        private void GetStandingVolume(float[] standingVolumeByPeriod)
        {
            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                Stand stand = this.StandByPeriod[periodIndex];
                float cvts4 = 0.0F;
                for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                {
                    if ((this.IndividualTreeSelection[treeIndex] == 0) || (this.IndividualTreeSelection[treeIndex] < periodIndex))
                    {
                        cvts4 += this.fiaVolume.GetMerchantableCubicVolumePerHectare(stand, treeIndex);
                    }
                }
                standingVolumeByPeriod[periodIndex] = cvts4;
            }
        }

        public void SetTreeSelection(int treeIndex, int harvestPeriod)
        {
            if ((harvestPeriod < 0) || (harvestPeriod >= this.HarvestPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }
            this.IndividualTreeSelection[treeIndex] = harvestPeriod;
        }

        public void Simulate()
        {
            Stand simulationStand = this.StandByPeriod[0].Clone();
            StandDensity standDensity = this.InitialDensity;

            float BABT = 0.0F;
            float[] BART = new float[5];
            float[] CCH = new float[41];
            int fertilizationCycle = 0;
            float OLD = 0.0F;
            float[] PN = new float[5];
            int thinningCycle = 0;
            float[] YF = new float[5];
            float[] YT = new float[5];

            int simulationStep = 0;
            for (int simulationPeriod = 1; simulationPeriod < this.PlanningPeriods; ++simulationPeriod)
            {
                bool recalculateStandDensity = false;
                for (int treeIndex = 0; treeIndex < simulationStand.TreeRecordCount; ++treeIndex)
                {
                    if (this.IndividualTreeSelection[treeIndex] == simulationPeriod)
                    {
                        simulationStand.LiveExpansionFactor[treeIndex] = 0.0F;
                        recalculateStandDensity = true;
                    }
                }
                if (recalculateStandDensity)
                {
                    standDensity = new StandDensity(simulationStand, this.organonConfiguration.Variant);
                }

                this.organonGrowth.Grow(ref simulationStep, this.organonConfiguration, simulationStand, ref thinningCycle, ref fertilizationCycle, 
                                        standDensity, this.organonCalibration, PN, YF, BABT, BART, YT, ref CCH, ref OLD, 0.0F, 
                                        out StandDensity standDensityAfterGrowth);
                this.StandByPeriod[simulationPeriod].CopyTreeGrowth(simulationStand);
                standDensity = standDensityAfterGrowth;
            }

            // recalculate volumes
            Array.Clear(this.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod.Length);
            Array.Clear(this.StandingVolumeByPeriod, 0, this.StandingVolumeByPeriod.Length);
            this.GetHarvestVolume(this.HarvestVolumesByPeriod);
            this.GetStandingVolume(this.StandingVolumeByPeriod);
        }

        public void Simulate(int[] treeSelection)
        {
            if (treeSelection.Length != this.IndividualTreeSelection.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(treeSelection));
            }

            for (int treeIndex = 0; treeIndex < this.StandByPeriod[0].TreeRecordCount; ++treeIndex)
            {
                this.SetTreeSelection(treeIndex, treeSelection[treeIndex]);
            }

            this.Simulate();
        }
    }
}
