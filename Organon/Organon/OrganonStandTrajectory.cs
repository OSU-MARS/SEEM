using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonStandTrajectory : StandTrajectory
    {
        private FiaVolume fiaVolume;
        private Dictionary<FiaCode, float[]> organonCalibration;
        private OrganonGrowth organonGrowth;

        public OrganonConfiguration Configuration { get; private set; }
        public OrganonStandDensity[] DensityByPeriod { get; private set; }

        public Heuristic Heuristic { get; set; }
        public OrganonStand[] StandByPeriod { get; private set; }

        public OrganonStandTrajectory(OrganonStand stand, OrganonConfiguration organonConfiguration, int lastPlanningPeriod, VolumeUnits volumeUnits)
            : base(lastPlanningPeriod, organonConfiguration.Treatments.Harvests.Count == 1 ? organonConfiguration.Treatments.Harvests[0].Period : 0, volumeUnits)
        {
            if (organonConfiguration.Treatments.Harvests.Count > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(organonConfiguration));
            }

            int maximumPlanningPeriodIndex = lastPlanningPeriod + 1;
            this.Configuration = new OrganonConfiguration(organonConfiguration);
            this.DensityByPeriod = new OrganonStandDensity[maximumPlanningPeriodIndex];
            this.fiaVolume = new FiaVolume();
            this.organonCalibration = organonConfiguration.CreateSpeciesCalibration();
            this.organonGrowth = new OrganonGrowth();
            this.Heuristic = null;
            this.Name = stand.Name;
            this.PeriodLengthInYears = organonConfiguration.Variant.TimeStepInYears;
            this.StandByPeriod = new OrganonStand[maximumPlanningPeriodIndex];

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
            : base(other)
        {
            this.fiaVolume = other.fiaVolume;
            this.organonCalibration = other.organonCalibration;
            this.Configuration = new OrganonConfiguration(other.Configuration);
            this.organonGrowth = other.organonGrowth;

            this.DensityByPeriod = new OrganonStandDensity[other.PlanningPeriods];
            this.Heuristic = other.Heuristic;
            this.StandByPeriod = new OrganonStand[other.PlanningPeriods];

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
        }

        public void CopyFrom(OrganonStandTrajectory other)
        {
            this.CopySelectionsFrom(other);

            // for now, shallow copies where feasible
            this.fiaVolume = other.fiaVolume; // has no state
            this.Heuristic = other.Heuristic; // assumed invariant within OptimizeCmdlet.Run() tasks
            this.organonCalibration = other.organonCalibration; // unused
            this.organonGrowth = other.organonGrowth; // BUGBUG: has no state, should have run state which can be copied

            // deep copies of mutable state changed by modified tree selection and resimulation
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
                        this.DensityByPeriod[periodIndex].CopyFrom(otherDensity);
                    }
                }

                // may need deep copy of treatment because 
                // 1) thinning prescriptions are being evaluated and therefore the best prescription needs to be reported
                // 2) BUGBUG: no Organon run state object has been implemented
                this.Configuration.CopyFrom(other.Configuration);

                OrganonStand otherStand = other.StandByPeriod[periodIndex];
                if (otherStand != null)
                {
                    if (this.StandByPeriod[periodIndex] == null)
                    {
                        this.StandByPeriod[periodIndex] = new OrganonStand(otherStand);
                    }
                    else
                    {
                        this.StandByPeriod[periodIndex].CopyTreeGrowthFrom(otherStand);
                    }
                }
                else
                {
                    this.StandByPeriod[periodIndex] = null;
                }
            }
        }

        public void CopySelectionsFrom(StandTrajectory other)
        {
            Debug.Assert(Object.ReferenceEquals(this, other) == false);

            if ((this.HarvestPeriods != other.HarvestPeriods) || (this.PlanningPeriods != other.PlanningPeriods))
            {
                // TODO: check rest of stand properties
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            Array.Copy(other.BasalAreaRemoved, 0, this.BasalAreaRemoved, 0, this.BasalAreaRemoved.Length);
            Array.Copy(other.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod.Length);
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
            this.TreeSelectionChangedSinceLastSimulation = other.TreeSelectionChangedSinceLastSimulation;
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

        public void DeselectAllTrees()
        {
            foreach (int[] selectionForSpecies in this.IndividualTreeSelectionBySpecies.Values)
            {
                Array.Clear(selectionForSpecies, 0, selectionForSpecies.Length);
            }
            this.TreeSelectionChangedSinceLastSimulation = true;
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

            Debug.Assert((this.BasalAreaRemoved[periodIndex] > 0.0F && this.HarvestVolumesByPeriod[periodIndex] > 0.0F) ||
                         (this.BasalAreaRemoved[periodIndex] == 0.0F && this.HarvestVolumesByPeriod[periodIndex] == 0.0F));
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

            Debug.Assert((this.BasalAreaRemoved[periodIndex] > 0.0F && this.HarvestVolumesByPeriod[periodIndex] > 0.0F) ||
                         (this.BasalAreaRemoved[periodIndex] == 0.0F && this.HarvestVolumesByPeriod[periodIndex] == 0.0F));
        }

        public int GetHarvestAge()
        {
            for (int periodIndex = 1; periodIndex < this.HarvestVolumesByPeriod.Length; ++periodIndex)
            {
                if (this.HarvestVolumesByPeriod[periodIndex] > 0.0F)
                {
                    return this.GetInitialStandAge() + this.PeriodLengthInYears * (periodIndex - 1);
                }
            }
            return -1;
        }

        public int GetInitialStandAge()
        {
            return this.StandByPeriod[0].AgeInYears;
        }

        public int GetRotationLength()
        {
            return this.GetInitialStandAge() + this.PeriodLengthInYears * (this.PlanningPeriods - 1);
        }

        private void GetStandingAndHarvestedVolume(int periodIndex)
        {
            bool isHarvestPeriod = false;
            foreach (IHarvest harvest in this.Configuration.Treatments.Harvests)
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

        public void Simulate()
        {
            // TODO: clear volumes and/or basal area?
            this.Configuration.Treatments.ClearHarvestState();

            // period 0 is the initial condition and therefore never needs to be simulated
            // Since simulation is computationally expensive, the current implementation is lazy and relies on triggers to simulate only on demand. In 
            // particular, in single entry cases no stand modification occurs before the target harvest period and, therefore, periods 1...entry - 1 need
            // to be simulated only once.
            Debug.Assert(this.StandByPeriod.Length > 1, "At least one simulation period expected.");
            bool standEnteredOrNotSimulated = this.StandByPeriod[1] == null; // not yet simulated case, entry checked in loop below
            float[] crownCompetitionByHeight = null;
            OrganonStand simulationStand = standEnteredOrNotSimulated ? new OrganonStand(this.StandByPeriod[0]) : null;
            for (int periodIndex = 1; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity standDensity = this.DensityByPeriod[periodIndex - 1];

                // trigger stand resimulation due to change in tree selection
                if (this.Configuration.Treatments.IsTriggerInPeriod(periodIndex))
                {
                    float basalAreaRemoved = this.Configuration.Treatments.EvaluateTriggers(periodIndex, this);
                    if (simulationStand == null)
                    {
                        simulationStand = new OrganonStand(this.StandByPeriod[periodIndex - 1]);
                    }
                    foreach (KeyValuePair<FiaCode, int[]> individualTreeSelection in this.IndividualTreeSelectionBySpecies)
                    {
                        for (int treeIndex = 0; treeIndex < individualTreeSelection.Value.Length; ++treeIndex) // assumes trailing capacity is set to zero and of insignificant length
                        {
                            // if needed, this loop can be changed to use either the simulation stand's tree count or a reference tree count rather than capacity
                            if (individualTreeSelection.Value[treeIndex] == periodIndex)
                            {
                                simulationStand.TreesBySpecies[individualTreeSelection.Key].LiveExpansionFactor[treeIndex] = 0.0F;
                            }
                        }
                    }

                    this.BasalAreaRemoved[periodIndex] = basalAreaRemoved;
                    if (this.TreeSelectionChangedSinceLastSimulation)
                    {
                        crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.Configuration.Variant, simulationStand);
                        standDensity = new OrganonStandDensity(simulationStand, this.Configuration.Variant);
                    }
                    standEnteredOrNotSimulated = true;
                }

                if (standEnteredOrNotSimulated)
                {
                    // simulate this period
                    if (crownCompetitionByHeight == null)
                    {
                        crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.Configuration.Variant, simulationStand);
                    }
                    this.organonGrowth.Grow(periodIndex, this.Configuration, simulationStand, standDensity, this.organonCalibration, 
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
                        this.StandByPeriod[periodIndex].CopyTreeGrowthFrom(simulationStand);
                    }

                    // recalculate volume for this period
                    this.GetStandingAndHarvestedVolume(periodIndex);

                    #if DEBUG
                    if (periodIndex < this.HarvestVolumesByPeriod.Length)
                    {
                        Debug.Assert((this.BasalAreaRemoved[periodIndex] == 0.0F && this.HarvestVolumesByPeriod[periodIndex] == 0.0F) || (this.BasalAreaRemoved[periodIndex] > 0.0F && this.HarvestVolumesByPeriod[periodIndex] > 0.0F));

                    }
                    #endif
                }
            }

            this.TreeSelectionChangedSinceLastSimulation = false;
        }
    }
}
