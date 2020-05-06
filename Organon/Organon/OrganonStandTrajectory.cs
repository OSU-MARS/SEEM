using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonStandTrajectory
    {
        private FiaVolume fiaVolume;
        private Dictionary<FiaCode, float[]> organonCalibration;
        private OrganonConfiguration organonConfiguration;
        private OrganonGrowth organonGrowth;

        public float[] BasalAreaRemoved { get; protected set; }
        public OrganonStandDensity[] DensityByPeriod { get; private set; }
        public float[] HarvestVolumesByPeriod { get; protected set; }
        public float IndividualTreeExpansionFactor { get; protected set; }
        // harvest periods by tree, 0 indicates no harvest
        public SortedDictionary<FiaCode, int[]> IndividualTreeSelectionBySpecies { get; private set; }

        public string Name { get; set; }
        public int PeriodLengthInYears { get; set; }

        public OrganonStand[] StandByPeriod { get; private set; }
        public float[] StandingVolumeByPeriod { get; set; }
        public VolumeUnits VolumeUnits { get; set; }

        public OrganonStandTrajectory(OrganonStand stand, OrganonConfiguration organonConfiguration, int lastPlanningPeriod, VolumeUnits volumeUnits)
        {
            if (lastPlanningPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lastPlanningPeriod));
            }
            if (organonConfiguration.Treatments.Harvests.Count > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(organonConfiguration));
            }

            int thinningPeriod = 0;
            if (organonConfiguration.Treatments.Harvests.Count == 1)
            {
                thinningPeriod = organonConfiguration.Treatments.Harvests[0].Period;
            }
            if (lastPlanningPeriod < thinningPeriod)
            {
                throw new ArgumentOutOfRangeException(nameof(organonConfiguration));
            }

            int maximumPlanningPeriodIndex = lastPlanningPeriod + 1;
            this.BasalAreaRemoved = new float[thinningPeriod + 1];
            this.DensityByPeriod = new OrganonStandDensity[maximumPlanningPeriodIndex];
            this.fiaVolume = new FiaVolume();
            this.organonCalibration = organonConfiguration.CreateSpeciesCalibration();
            this.organonConfiguration = new OrganonConfiguration(organonConfiguration);
            this.organonGrowth = new OrganonGrowth();
            this.HarvestVolumesByPeriod = new float[thinningPeriod + 1];
            // TODO: check all trees in stand have same expansion factor
            this.IndividualTreeExpansionFactor = stand.TreesBySpecies.First().Value.LiveExpansionFactor[0];
            this.IndividualTreeSelectionBySpecies = new SortedDictionary<FiaCode, int[]>();
            this.Name = stand.Name;
            this.PeriodLengthInYears = organonConfiguration.Variant.TimeStepInYears;
            this.StandByPeriod = new OrganonStand[maximumPlanningPeriodIndex];
            this.StandingVolumeByPeriod = new float[maximumPlanningPeriodIndex];
            this.VolumeUnits = volumeUnits;

            this.DensityByPeriod[0] = new OrganonStandDensity(stand, organonConfiguration.Variant);
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                this.IndividualTreeSelectionBySpecies.Add(treesOfSpecies.Species, new int[treesOfSpecies.Capacity]);
            }
            this.StandByPeriod[0] = new OrganonStand(stand); // subsequent periods initialized lazily in Simulate()
            this.StandByPeriod[0].Name += 0;

            this.GetStandingAndHarvestedVolume(0);
        }

        // shallow copy FIA and Organon for now
        // deep copy of tree growth data
        public OrganonStandTrajectory(OrganonStandTrajectory other)
        {
            this.fiaVolume = other.fiaVolume;
            this.organonCalibration = other.organonCalibration;
            this.organonConfiguration = other.organonConfiguration;
            this.organonGrowth = other.organonGrowth;

            this.BasalAreaRemoved = new float[other.HarvestPeriods];
            this.DensityByPeriod = new OrganonStandDensity[other.PlanningPeriods];
            this.HarvestVolumesByPeriod = new float[other.HarvestPeriods];
            this.IndividualTreeExpansionFactor = other.IndividualTreeExpansionFactor;
            this.IndividualTreeSelectionBySpecies = new SortedDictionary<FiaCode, int[]>();
            this.Name = other.Name;
            this.PeriodLengthInYears = other.PeriodLengthInYears;
            this.StandingVolumeByPeriod = new float[other.PlanningPeriods];
            this.StandByPeriod = new OrganonStand[other.PlanningPeriods];

            Array.Copy(other.BasalAreaRemoved, 0, this.BasalAreaRemoved, 0, this.HarvestPeriods);
            Array.Copy(other.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod, 0, this.HarvestPeriods);

            foreach (KeyValuePair<FiaCode, int[]> otherSelectionForSpecies in other.IndividualTreeSelectionBySpecies)
            {
                int[] thisSelectionForSpecies = this.IndividualTreeSelectionBySpecies.GetOrAdd(otherSelectionForSpecies.Key, otherSelectionForSpecies.Value.Length);
                if (otherSelectionForSpecies.Value.Length != thisSelectionForSpecies.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(other.IndividualTreeSelectionBySpecies));
                }
                Array.Copy(otherSelectionForSpecies.Value, 0, thisSelectionForSpecies, 0, thisSelectionForSpecies.Length);
            }

            Array.Copy(other.StandingVolumeByPeriod, 0, this.StandingVolumeByPeriod, 0, this.PlanningPeriods);
            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity otherDensity = other.DensityByPeriod[periodIndex];
                if (otherDensity != null)
                {
                    this.DensityByPeriod[periodIndex] = new OrganonStandDensity(otherDensity);
                }

                OrganonStand otherStand = other.StandByPeriod[periodIndex];
                if (otherStand != null)
                {
                    this.StandByPeriod[periodIndex] = new OrganonStand(otherStand);
                }
            }

            this.VolumeUnits = other.VolumeUnits;
        }

        public int HarvestPeriods
        {
            get { return this.HarvestVolumesByPeriod.Length; }
        }

        public int PlanningPeriods
        {
            get { return this.StandingVolumeByPeriod.Length; }
        }

        public void Copy(OrganonStandTrajectory other)
        {
            if ((this.HarvestPeriods != other.HarvestPeriods) || (this.PlanningPeriods != other.PlanningPeriods))
            {
                // TODO: check rest of stand properties
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            // for now, shallow copies
            this.fiaVolume = other.fiaVolume;
            this.organonCalibration = other.organonCalibration;
            this.organonConfiguration = other.organonConfiguration;
            this.organonGrowth = other.organonGrowth;

            Array.Copy(other.BasalAreaRemoved, 0, this.BasalAreaRemoved, 0, this.BasalAreaRemoved.Length);
            Array.Copy(other.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod.Length);
            for (int periodIndex = 0; periodIndex < this.StandByPeriod.Length; ++periodIndex)
            {
                OrganonStandDensity otherDensity = other.DensityByPeriod[periodIndex];
                if (otherDensity != null)
                {
                    if (this.DensityByPeriod[periodIndex] == null)
                    {
                        this.DensityByPeriod[periodIndex] = new OrganonStandDensity(otherDensity);
                    }
                    else
                    {
                        this.DensityByPeriod[periodIndex].Copy(otherDensity);
                    }
                }

                OrganonStand otherStand = other.StandByPeriod[periodIndex];
                if (otherStand != null)
                {
                    if (this.StandByPeriod[periodIndex] == null)
                    {
                        this.StandByPeriod[periodIndex] = new OrganonStand(otherStand);
                    }
                    else
                    {
                        this.StandByPeriod[periodIndex].CopyTreeGrowth(otherStand);
                    }
                }
                else
                {
                    this.StandByPeriod[periodIndex] = null;
                }
            }
            Array.Copy(other.StandingVolumeByPeriod, 0, this.StandingVolumeByPeriod, 0, this.StandingVolumeByPeriod.Length);

            foreach (KeyValuePair<FiaCode, int[]> otherSelectionForSpecies in other.IndividualTreeSelectionBySpecies)
            {
                int[] thisSelectionForSpecies = this.IndividualTreeSelectionBySpecies[otherSelectionForSpecies.Key];
                if (otherSelectionForSpecies.Value.Length != thisSelectionForSpecies.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(other.IndividualTreeSelectionBySpecies));
                }
                Array.Copy(otherSelectionForSpecies.Value, 0, thisSelectionForSpecies, 0, thisSelectionForSpecies.Length);
            }

            this.VolumeUnits = other.VolumeUnits;
        }

        public void CopyTreeSelectionTo(int[] allTreeSelection)
        {
            int destinationIndex = 0;
            foreach (int[] individualTreeSelection in this.IndividualTreeSelectionBySpecies.Values)
            {
                // BUGBUG: assumes either a single species or that all species but the last have tree counts matching the species capacity
                // TODO: make this copy species count aware to avoid packing gaps when capacity > count
                Array.Copy(individualTreeSelection, 0, allTreeSelection, destinationIndex, individualTreeSelection.Length);
                destinationIndex += individualTreeSelection.Length;
            }
        }

        private void GetHarvestedCubicMetersPerHectare(int periodIndex)
        {
            // tree's expansion factor is set to zero when it's marked for harvest
            // Use tree's volume from the previous period.
            OrganonStand previousStand = this.StandByPeriod[periodIndex - 1];
            double cvts4perAcre = 0.0F;
            foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
            {
                int[] individualTreeSelection = this.IndividualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                Debug.Assert(individualTreeSelection.Length == previousTreesOfSpecies.Capacity);
                for (int treeIndex = 0; treeIndex < previousTreesOfSpecies.Count; ++treeIndex)
                {
                    if (individualTreeSelection[treeIndex] == periodIndex)
                    {
                        double treesPerAcre = previousTreesOfSpecies.LiveExpansionFactor[treeIndex];
                        Debug.Assert(treesPerAcre > 0.0F);
                        cvts4perAcre += treesPerAcre * this.fiaVolume.GetMerchantableCubicFeet(previousTreesOfSpecies, treeIndex);
                    }
                }
            }
            this.HarvestVolumesByPeriod[periodIndex] = (float)(Constant.AcresPerHectare * Constant.CubicMetersPerCubicFoot * cvts4perAcre);
        }

        private void GetHarvestedScribnerBoardFeetPerAcre(int periodIndex)
        {
            // tree's expansion factor is set to zero when it's marked for harvest
            // Use tree's volume from the previous period.
            // TODO: track per species volumes
            OrganonStand previousStand = this.StandByPeriod[periodIndex - 1];
            float scribner6x32footLogPerAcre = 0.0F;
            foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
            {
                int[] individualTreeSelection = this.IndividualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                Debug.Assert(individualTreeSelection.Length == previousTreesOfSpecies.Capacity);
                for (int treeIndex = 0; treeIndex < previousTreesOfSpecies.Count; ++treeIndex)
                {
                    if (individualTreeSelection[treeIndex] == periodIndex)
                    {
                        float treesPerAcre = previousTreesOfSpecies.LiveExpansionFactor[treeIndex];
                        Debug.Assert(treesPerAcre > 0.0F);
                        scribner6x32footLogPerAcre += treesPerAcre * this.fiaVolume.GetScribnerBoardFeet(previousTreesOfSpecies, treeIndex);
                    }
                }
            }
            this.HarvestVolumesByPeriod[periodIndex] = scribner6x32footLogPerAcre;
        }

        private void GetStandingAndHarvestedVolume(int periodIndex)
        {
            bool isHarvestPeriod = false;
            foreach (IHarvest harvest in this.organonConfiguration.Treatments.Harvests)
            {
                if (harvest.Period == periodIndex)
                {
                    isHarvestPeriod = true;
                    break;
                }
            }

            switch (this.VolumeUnits)
            {
                case VolumeUnits.CubicMetersPerHectare:
                    if (isHarvestPeriod)
                    {
                        this.GetHarvestedCubicMetersPerHectare(periodIndex);
                    }
                    this.GetStandingCubicMetersPerHectare(periodIndex);
                    break;
                case VolumeUnits.ScribnerBoardFeetPerAcre:
                    if (isHarvestPeriod)
                    {
                        this.GetHarvestedScribnerBoardFeetPerAcre(periodIndex);
                    }
                    this.GetStandingScribnerBoardFeetPerAcre(periodIndex);
                    break;
                default:
                    throw new NotSupportedException(String.Format("Unhandled volume units {0}.", this.VolumeUnits));
            }
        }

        private void GetStandingCubicMetersPerHectare(int periodIndex)
        {
            OrganonStand stand = this.StandByPeriod[periodIndex];
            float cvts4perAcre = 0.0F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                int[] individualTreeSelection = this.IndividualTreeSelectionBySpecies[treesOfSpecies.Species];
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    if ((individualTreeSelection[treeIndex] == 0) || (periodIndex < individualTreeSelection[treeIndex]))
                    {
                        float treesPerAcre = treesOfSpecies.LiveExpansionFactor[treeIndex];
                        cvts4perAcre += treesPerAcre * this.fiaVolume.GetMerchantableCubicFeet(treesOfSpecies, treeIndex);
                    }
                }
            }
            this.StandingVolumeByPeriod[periodIndex] = Constant.AcresPerHectare * Constant.CubicMetersPerCubicFoot * cvts4perAcre;
        }

        private void GetStandingScribnerBoardFeetPerAcre(int periodIndex)
        {
            OrganonStand stand = this.StandByPeriod[periodIndex];
            double scribner6x32footLogPerAcre = 0.0F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                scribner6x32footLogPerAcre += this.fiaVolume.GetScribnerBoardFeetPerAcre(treesOfSpecies);
                //int[] individualTreeSelection = this.IndividualTreeSelectionBySpecies[treesOfSpecies.Species];
                //for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                //{
                //    if ((individualTreeSelection[treeIndex] == 0) || (periodIndex < individualTreeSelection[treeIndex]))
                //    {
                //        double treesPerAcre = treesOfSpecies.LiveExpansionFactor[treeIndex];
                //        scribner6x32footLogPerAcre += treesPerAcre * this.fiaVolume.GetScribnerBoardFeet(treesOfSpecies, treeIndex);
                //    }
                //}
            }
            this.StandingVolumeByPeriod[periodIndex] = (float)scribner6x32footLogPerAcre;
        }

        public int GetTreeSelection(int allSpeciesTreeIndex)
        {
            int treeIndex = allSpeciesTreeIndex;
            foreach (KeyValuePair<FiaCode, int[]> individualTreeSelection in this.IndividualTreeSelectionBySpecies)
            {
                if (treeIndex < individualTreeSelection.Value.Length)
                {
                    return individualTreeSelection.Value[treeIndex];
                }
                treeIndex -= individualTreeSelection.Value.Length;
            }

            throw new ArgumentOutOfRangeException(nameof(allSpeciesTreeIndex));
        }

        public void SetTreeSelection(int allSpeciesTreeIndex, int harvestPeriod)
        {
            if ((harvestPeriod < 0) || (harvestPeriod >= this.HarvestPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }

            int treeIndex = allSpeciesTreeIndex;
            foreach (KeyValuePair<FiaCode, int[]> individualTreeSelection in this.IndividualTreeSelectionBySpecies)
            {
                if (treeIndex < individualTreeSelection.Value.Length)
                {
                    individualTreeSelection.Value[treeIndex] = harvestPeriod;
                    return;
                }
                treeIndex -= individualTreeSelection.Value.Length;
            }

            throw new ArgumentOutOfRangeException(nameof(allSpeciesTreeIndex));
        }

        public void SetTreeSelection(FiaCode species, int treeIndex, int harvestPeriod)
        {
            if ((harvestPeriod < 0) || (harvestPeriod >= this.HarvestPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }

            this.IndividualTreeSelectionBySpecies[species][treeIndex] = harvestPeriod;
        }

        public void Simulate()
        {
            // TODO: clear volumes and/or basal area?
            this.organonConfiguration.Treatments.Reset();

            // period 0 is the initial condition and therefore never needs to be simulated
            // Since simulation is computationally expensive, the current implementation is lazy and relies on triggers to simulate only on demand. In 
            // particular, in single entry cases no stand modification occurs before the target harvest period and, therefore, periods 1...entry - 1 need
            // to be simulated only once.
            Debug.Assert(this.StandByPeriod.Length > 1, "At least one simulation period expected.");
            bool standEnteredOrNotSimulated = this.StandByPeriod[1] == null; // not simulated case, entry checked in loop below
            float[] crownCompetitionByHeight = null;
            OrganonStand simulationStand = standEnteredOrNotSimulated ? new OrganonStand(this.StandByPeriod[0]) : null;
            for (int periodIndex = 1; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity standDensity = this.DensityByPeriod[periodIndex - 1];

                // trigger stand resimulation due to harvest
                float basalAreaRemoved = this.organonConfiguration.Treatments.EvaluateTriggers(periodIndex, this);
                bool recalculateStandDensity = basalAreaRemoved > 0.0F;
                if (recalculateStandDensity)
                {
                    foreach (KeyValuePair<FiaCode, int[]> individualTreeSelection in this.IndividualTreeSelectionBySpecies)
                    {
                        for (int treeIndex = 0; treeIndex < individualTreeSelection.Value.Length; ++treeIndex) // assumes trailing capacity is set to zero and of insignificant length
                        {
                            // if needed, this loop can be changed to use either the simulation stand's tree count or a reference tree count rather than capacity
                            if (individualTreeSelection.Value[treeIndex] == periodIndex)
                            {
                                if (simulationStand == null)
                                {
                                    simulationStand = new OrganonStand(this.StandByPeriod[periodIndex - 1]);
                                }
                                simulationStand.TreesBySpecies[individualTreeSelection.Key].LiveExpansionFactor[treeIndex] = 0.0F;
                            }
                        }
                    }

                    this.BasalAreaRemoved[periodIndex] = basalAreaRemoved;
                    crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.organonConfiguration.Variant, simulationStand);
                    standDensity = new OrganonStandDensity(simulationStand, this.organonConfiguration.Variant);
                    standEnteredOrNotSimulated = true;
                }

                if (standEnteredOrNotSimulated)
                {
                    // simulate this period
                    if (crownCompetitionByHeight == null)
                    {
                        crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.organonConfiguration.Variant, simulationStand);
                    }
                    this.organonGrowth.Grow(periodIndex, this.organonConfiguration, simulationStand, standDensity, this.organonCalibration, 
                                            ref crownCompetitionByHeight, out OrganonStandDensity standDensityAfterGrowth, out int _);

                    this.DensityByPeriod[periodIndex] = standDensityAfterGrowth;
                    if (this.StandByPeriod[periodIndex] == null)
                    {
                        // lazy initialization
                        OrganonStand standForPeriod = new OrganonStand(simulationStand);
                        standForPeriod.Name = standForPeriod.Name[0..^1] + periodIndex;
                        this.StandByPeriod[periodIndex] = standForPeriod;
                    }
                    else
                    {
                        // update on resimulation
                        this.StandByPeriod[periodIndex].CopyTreeGrowth(simulationStand);
                    }

                    // recalculate volume for this period
                    this.GetStandingAndHarvestedVolume(periodIndex);
                }
            }
        }
    }
}
