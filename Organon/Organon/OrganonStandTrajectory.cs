using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonStandTrajectory
    {
        private readonly FiaVolume fiaVolume;
        private readonly Dictionary<FiaCode, float[]> organonCalibration;
        private readonly OrganonConfiguration organonConfiguration;
        private readonly OrganonGrowth organonGrowth;

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

        public OrganonStandTrajectory(OrganonStand stand, OrganonConfiguration organonConfiguration, int lastHarvestPeriod, int lastPlanningPeriod, VolumeUnits volumeUnits)
        {
            if (lastPlanningPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lastPlanningPeriod));
            }
            if (lastPlanningPeriod < lastHarvestPeriod)
            {
                throw new ArgumentOutOfRangeException(nameof(lastHarvestPeriod));
            }

            int maximumPlanningPeriodIndex = lastPlanningPeriod + 1;
            this.DensityByPeriod = new OrganonStandDensity[maximumPlanningPeriodIndex];
            this.fiaVolume = new FiaVolume();
            this.organonCalibration = organonConfiguration.CreateSpeciesCalibration();
            this.organonConfiguration = organonConfiguration;
            this.organonGrowth = new OrganonGrowth();
            this.HarvestVolumesByPeriod = new float[lastHarvestPeriod + 1];
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
            this.StandByPeriod[0] = stand.Clone(); // subsequent periods initialized lazily in Simulate()
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

            this.HarvestVolumesByPeriod = new float[other.HarvestPeriods];
            this.IndividualTreeExpansionFactor = other.IndividualTreeExpansionFactor;
            this.IndividualTreeSelectionBySpecies = new SortedDictionary<FiaCode, int[]>();
            this.DensityByPeriod = new OrganonStandDensity[other.PlanningPeriods];
            this.Name = other.Name;
            this.StandingVolumeByPeriod = new float[other.PlanningPeriods];
            this.StandByPeriod = new OrganonStand[other.PlanningPeriods];

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

            Array.Copy(other.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod.Length);
            for (int periodIndex = 0; periodIndex < this.StandByPeriod.Length; ++periodIndex)
            {
                OrganonStand otherStand = other.StandByPeriod[periodIndex];
                if (otherStand != null)
                {
                    if (this.StandByPeriod[periodIndex] == null)
                    {
                        this.StandByPeriod[periodIndex] = otherStand.Clone();
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
            bool isHarvestPeriod = (periodIndex > 0) && ((this.HarvestPeriods - 1) == periodIndex);
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

        public void Simulate()
        {
            OrganonStandDensity standDensity = this.DensityByPeriod[0];
            OrganonTreatments treatments = new OrganonTreatments();

            int simulationStep = 0;
            // period 0 is the initial condition and therefore never needs to be simulated
            // Since simulation is computationally expensive, the current implementation is lazy and relies on triggers to simulate only on demand. In 
            // particular, in single entry cases no stand modification occurs before the target harvest period and, therefore, periods 1...entry - 1 need
            // to be simulated only once.
            Debug.Assert(this.StandByPeriod.Length > 1, "At least one simulation period expected.");
            bool standEnteredOrNotSimulated = this.StandByPeriod[1] == null;
            float[] crownCompetitionByHeight = null;
            OrganonStand simulationStand = standEnteredOrNotSimulated ? this.StandByPeriod[0].Clone() : null;
            for (int simulationPeriod = 1; simulationPeriod < this.PlanningPeriods; ++simulationPeriod)
            {
                // mainline case: trigger stand resimulation due to harvest
                bool recalculateStandDensity = false;
                foreach (KeyValuePair<FiaCode, int[]> individualTreeSelection in this.IndividualTreeSelectionBySpecies)
                {
                    for (int treeIndex = 0; treeIndex < individualTreeSelection.Value.Length; ++treeIndex) // assumes trailing capacity is set to zero and of insignificant length
                    {
                        // if needed, this loop can be changed to use either the simulation stand's tree count or a reference tree count rather than capacity
                        if (individualTreeSelection.Value[treeIndex] == simulationPeriod)
                        {
                            if (simulationStand == null)
                            {
                                simulationStand = this.StandByPeriod[simulationPeriod - 1].Clone();
                            }
                            simulationStand.TreesBySpecies[individualTreeSelection.Key].LiveExpansionFactor[treeIndex] = 0.0F;
                            recalculateStandDensity = true;
                        }
                    }
                }
                if (recalculateStandDensity)
                {
                    standDensity = new OrganonStandDensity(simulationStand, this.organonConfiguration.Variant);
                    crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.organonConfiguration.Variant, simulationStand);
                    standEnteredOrNotSimulated = true;
                }

                if (standEnteredOrNotSimulated)
                {
                    // simulate this period
                    if (crownCompetitionByHeight == null)
                    {
                        crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.organonConfiguration.Variant, simulationStand);
                    }
                    this.organonGrowth.Grow(ref simulationStep, this.organonConfiguration, simulationStand, standDensity, this.organonCalibration, treatments,
                                            ref crownCompetitionByHeight, out OrganonStandDensity standDensityAfterGrowth, out int _);

                    this.DensityByPeriod[simulationPeriod] = standDensityAfterGrowth;
                    if (this.StandByPeriod[simulationPeriod] == null)
                    {
                        // lazy initialization
                        OrganonStand standForPeriod = simulationStand.Clone();
                        standForPeriod.Name = standForPeriod.Name.Substring(0, standForPeriod.Name.Length - 1) + simulationPeriod;
                        this.StandByPeriod[simulationPeriod] = standForPeriod;
                    }
                    else
                    {
                        // update on resimulation
                        this.StandByPeriod[simulationPeriod].CopyTreeGrowth(simulationStand);
                    }
                    standDensity = standDensityAfterGrowth;

                    // recalculate volume for this period
                    this.GetStandingAndHarvestedVolume(simulationPeriod);
                }
            }
        }
    }
}
