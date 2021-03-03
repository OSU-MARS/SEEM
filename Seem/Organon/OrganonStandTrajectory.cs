using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonStandTrajectory : StandTrajectory
    {
        private FiaVolume fiaVolume;
        private Dictionary<FiaCode, SpeciesCalibration> organonCalibration;
        private OrganonGrowth organonGrowth;

        public OrganonConfiguration Configuration { get; private init; }
        public OrganonStandDensity[] DensityByPeriod { get; private init; }

        public Heuristic? Heuristic { get; set; }
        public OrganonStand?[] StandByPeriod { get; private init; }
        public bool UseFiaVolume { get; private set; }

        public OrganonStandTrajectory(OrganonStand stand, OrganonConfiguration organonConfiguration, TimberValue timberValue, int lastPlanningPeriod, bool useFiaVolume)
            : base(timberValue, lastPlanningPeriod, 
                  stand.PlantingDensityInTreesPerHectare ?? throw new ArgumentOutOfRangeException(nameof(stand))) // base does range checks
        {
            if (timberValue == null)
            {
                throw new ArgumentNullException(nameof(timberValue));
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
            this.PeriodZeroAgeInYears = stand.AgeInYears;
            this.StandByPeriod = new OrganonStand[maximumPlanningPeriodIndex];
            this.UseFiaVolume = useFiaVolume;

            this.DensityByPeriod[0] = new OrganonStandDensity(stand, organonConfiguration.Variant);
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                this.IndividualTreeSelectionBySpecies.Add(treesOfSpecies.Species, new int[treesOfSpecies.Capacity]);
            }
            this.StandByPeriod[0] = new OrganonStand(stand); // subsequent periods initialized lazily in Simulate()
            this.StandByPeriod[0]!.Name += 0;

            this.GetVolumeAndValue(0);
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
            this.UseFiaVolume = other.UseFiaVolume;

            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity otherDensity = other.DensityByPeriod[periodIndex];
                if (otherDensity != null)
                {
                    this.DensityByPeriod[periodIndex] = new OrganonStandDensity(otherDensity);
                }

                OrganonStand? otherStand = other.StandByPeriod[periodIndex];
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

                OrganonStand? otherStand = other.StandByPeriod[periodIndex];
                if (otherStand != null)
                {
                    if (this.StandByPeriod[periodIndex] == null)
                    {
                        this.StandByPeriod[periodIndex] = new OrganonStand(otherStand);
                    }
                    else
                    {
                        this.StandByPeriod[periodIndex]!.CopyTreeGrowthFrom(otherStand);
                    }
                }
                else
                {
                    this.StandByPeriod[periodIndex] = null;
                }
            }

            this.UseFiaVolume = other.UseFiaVolume;
        }

        public void CopySelectionsFrom(StandTrajectory other)
        {
            Debug.Assert(Object.ReferenceEquals(this, other) == false);

            if ((this.BasalAreaRemoved.Length != other.BasalAreaRemoved.Length) || (this.PlanningPeriods != other.PlanningPeriods))
            {
                // TODO: check rest of stand properties
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            Array.Copy(other.BasalAreaRemoved, 0, this.BasalAreaRemoved, 0, this.BasalAreaRemoved.Length);
            this.ThinningVolume.CopyFrom(other.ThinningVolume);
            this.StandingVolume.CopyFrom(other.StandingVolume);

            foreach (KeyValuePair<FiaCode, int[]> otherSelectionForSpecies in other.IndividualTreeSelectionBySpecies)
            {
                int[] thisSelectionForSpecies = this.IndividualTreeSelectionBySpecies[otherSelectionForSpecies.Key];
                if (otherSelectionForSpecies.Value.Length != thisSelectionForSpecies.Length)
                {
                    throw new NotSupportedException("Individual tree selections are of different lengths.");
                }
                Array.Copy(otherSelectionForSpecies.Value, 0, thisSelectionForSpecies, 0, thisSelectionForSpecies.Length);
            }
            this.TreeSelectionChangedSinceLastSimulation = other.TreeSelectionChangedSinceLastSimulation;
        }

        public Units GetUnits()
        {
            Debug.Assert(this.StandByPeriod[0] != null);
            Units units = this.StandByPeriod[0]!.GetUnits();
            for (int periodIndex = 1; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                Debug.Assert(this.StandByPeriod[periodIndex] != null);
                if (this.StandByPeriod[periodIndex]!.GetUnits() != units)
                {
                    throw new NotSupportedException();
                }
            }
            return units;
        }

        private void GetVolumeAndValue(int periodIndex)
        {
            if (this.UseFiaVolume)
            {
                this.GetFiaVolumeAndValue(periodIndex);
            }
            else
            {
                this.GetScaledVolumeAndValue(periodIndex);
            }
        }

        private void GetFiaVolumeAndValue(int periodIndex)
        {
            // harvest volumes, if applicable
            foreach (IHarvest harvest in this.Configuration.Treatments.Harvests)
            {
                if (harvest.Period == periodIndex)
                {
                    // tree's expansion factor is set to zero when it's marked for harvest
                    // Use tree's volume at end of the the previous period.
                    // TODO: track per species volumes
                    OrganonStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                    double harvestedScribner6x32footLogPerAcre = 0.0F;
                    foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
                    {
                        if (previousTreesOfSpecies.Units != Units.English)
                        {
                            throw new NotSupportedException();
                        }

                        int[] individualTreeSelection = this.IndividualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                        Debug.Assert(individualTreeSelection.Length == previousTreesOfSpecies.Capacity); // tree selection and tree capacities are expected to match
                        Debug.Assert(previousTreesOfSpecies.Capacity - previousTreesOfSpecies.Count < Constant.Simd128x4.Width); // also expected that trees haven't previously been compacted
                        for (int compactedTreeIndex = 0; compactedTreeIndex < previousTreesOfSpecies.Count; ++compactedTreeIndex)
                        {
                            int uncompactedTreeIndex = previousTreesOfSpecies.UncompactedIndex[compactedTreeIndex];
                            if (individualTreeSelection[uncompactedTreeIndex] == periodIndex)
                            {
                                float treesPerAcre = previousTreesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                                Debug.Assert(treesPerAcre > 0.0F);
                                harvestedScribner6x32footLogPerAcre += treesPerAcre * FiaVolume.GetScribnerBoardFeet(previousTreesOfSpecies, compactedTreeIndex);
                            }
                        }
                    }

                    this.ThinningVolume.ScribnerTotal[periodIndex] = 0.001F * Constant.AcresPerHectare * (float)harvestedScribner6x32footLogPerAcre;
                    if (harvestedScribner6x32footLogPerAcre <= 0.0001F)
                    {
                        Debug.Assert(harvestedScribner6x32footLogPerAcre == 0.0F);
                        this.ThinningVolume.NetPresentValue[periodIndex] = 0.0F;
                    }
                    else
                    {
                        int thinAge = this.GetStartOfPeriodAge(periodIndex);
                        this.ThinningVolume.NetPresentValue[periodIndex] = this.TimberValue.GetNetPresentThinningValue(this.ThinningVolume.ScribnerTotal[periodIndex], thinAge);
                    }

                    // could make more specific by checking if harvest removes at least one tree with merchantable volume
                    // basal area threshold will need to be updated when metric units are supportd
                    Debug.Assert((this.BasalAreaRemoved[periodIndex] > Constant.Bucking.MinimumBasalArea4SawEnglish && this.ThinningVolume.ScribnerTotal[periodIndex] > 0.0F) ||
                                  this.ThinningVolume.ScribnerTotal[periodIndex] == 0.0F);
                }
            }

            // standing volume
            OrganonStand stand = this.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information is not available for period " + periodIndex + ".");
            double standingCvts4perAcre = 0.0F;
            double standingScribner6x32footLogPerAcre = 0.0F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                if (treesOfSpecies.Units != Units.English)
                {
                    throw new NotSupportedException();
                }

                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    if (expansionFactor > 0.0F)
                    {
                        standingCvts4perAcre += expansionFactor * FiaVolume.GetMerchantableCubicFeet(treesOfSpecies, compactedTreeIndex);
                    }
                }

                standingScribner6x32footLogPerAcre += FiaVolume.GetScribnerBoardFeetPerAcre(treesOfSpecies);
            }

            this.StandingVolume.ScribnerTotal[periodIndex] = 0.001F * Constant.AcresPerHectare * (float)standingScribner6x32footLogPerAcre;
            int harvestAge = this.GetEndOfPeriodAge(periodIndex);
            this.StandingVolume.NetPresentValue[periodIndex] = this.TimberValue.GetNetPresentRegenerationHarvestValue(this.StandingVolume.ScribnerTotal[periodIndex], harvestAge);
        }

        public void GetGradedVolumes(out StandGradedVolume gradedVolumeStanding, out StandGradedVolume gradedVolumeHarvested)
        {
            gradedVolumeHarvested = new StandGradedVolume(this.PlanningPeriods);
            gradedVolumeStanding = new StandGradedVolume(this.PlanningPeriods);
            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                // harvest volumes, if applicable
                foreach (IHarvest harvest in this.Configuration.Treatments.Harvests)
                {
                    if (harvest.Period == periodIndex)
                    {
                        // tree's expansion factor is set to zero when it's marked for harvest
                        // Use tree's volume at end of the the previous period.
                        // TODO: track per species volumes
                        OrganonStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                        gradedVolumeHarvested.FromStand(previousStand, this.IndividualTreeSelectionBySpecies, periodIndex, this.TimberValue, this.GetStartOfPeriodAge(periodIndex));
                    }
                }

                // standing volume
                OrganonStand stand = this.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information is not available for period " + periodIndex + ".");
                gradedVolumeStanding.FromStand(stand, periodIndex, this.TimberValue, this.GetEndOfPeriodAge(periodIndex));
            }
        }

        private void GetScaledVolumeAndValue(int periodIndex)
        {
            // harvest volumes, if applicable
            foreach (IHarvest harvest in this.Configuration.Treatments.Harvests)
            {
                if (harvest.Period == periodIndex)
                {
                    // tree's expansion factor is set to zero when it's marked for harvest
                    // Use tree's volume at end of the the previous period.
                    // TODO: track per species volumes
                    OrganonStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                    this.ThinningVolume.FromStand(previousStand, this.IndividualTreeSelectionBySpecies, periodIndex, this.TimberValue, this.GetStartOfPeriodAge(periodIndex));
                    // could make more specific by checking if harvest removes at least one tree
                    Debug.Assert((this.BasalAreaRemoved[periodIndex] > Constant.Bucking.MinimumBasalArea4SawEnglish && this.ThinningVolume.ScribnerTotal[periodIndex] > 0.0F) ||
                                  this.ThinningVolume.ScribnerTotal[periodIndex] == 0.0F);
                }
            }

            // standing volume
            OrganonStand stand = this.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information is not available for period " + periodIndex + "."); 
            this.StandingVolume.FromStand(stand, periodIndex, this.TimberValue, this.GetEndOfPeriodAge(periodIndex));
        }

        public int GetInitialTreeRecordCount()
        {
            Stand initialStand = this.StandByPeriod[0] ?? throw new NotSupportedException("Initial stand infomation is missing.");
            return initialStand.GetTreeRecordCount();
        }

        public void Simulate()
        {
            // TODO: clear volumes and/or basal area?
            this.Configuration.Treatments.ClearHarvestState();

            // period 0 is the initial condition and therefore never needs to be simulated
            // Since simulation is computationally expensive, the current implementation is lazy and relies on triggers to simulate only on demand. In 
            // particular, in single entry cases no stand modification occurs before the target harvest period and, therefore, periods 1...entry - 1 need
            // to be simulated only once.
            Debug.Assert(this.StandByPeriod.Length > 1 && this.StandByPeriod[0] != null, "Pre-simulation information expected for stand.");
            bool standEnteredOrNotSimulated = this.StandByPeriod[1] == null; // not yet simulated case, entry checked in loop below
            float[]? crownCompetitionByHeight = null;
            OrganonStand? simulationStand = standEnteredOrNotSimulated ? new OrganonStand(this.StandByPeriod[0]!) : null;
            for (int periodIndex = 1; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity standDensity = this.DensityByPeriod[periodIndex - 1];

                // trigger stand resimulation due to change in tree selection
                if (this.Configuration.Treatments.IsTriggerInPeriod(periodIndex))
                {
                    float basalAreaRemoved = this.Configuration.Treatments.EvaluateTriggers(periodIndex, this);
                    if (simulationStand == null)
                    {
                        OrganonStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                        simulationStand = new OrganonStand(previousStand);
                    }
                    foreach (KeyValuePair<FiaCode, int[]> individualTreeSelection in this.IndividualTreeSelectionBySpecies)
                    {
                        Trees treesOfSpecies = simulationStand.TreesBySpecies[individualTreeSelection.Key];
                        bool atLeastOneTreeRemoved = false;
                        for (int compactedTreeIndex = 0, uncompactedTreeIndex = 0; uncompactedTreeIndex < individualTreeSelection.Value.Length; ++uncompactedTreeIndex) // assumes trailing capacity is set to zero and of insignificant length
                        {
                            // if needed, this loop can be changed to use either the simulation stand's tree count or a reference tree count rather than capacity
                            int treeSelection = individualTreeSelection.Value[uncompactedTreeIndex];
                            if (treeSelection == periodIndex)
                            {
                                // tree is harvested in this period, so set its expansion factor to zero
                                Debug.Assert(this.StandByPeriod[0]!.TreesBySpecies[treesOfSpecies.Species].Tag[uncompactedTreeIndex] == treesOfSpecies.Tag[compactedTreeIndex]);
                                Debug.Assert(treesOfSpecies.LiveExpansionFactor[compactedTreeIndex] > 0.0F);
                                treesOfSpecies.LiveExpansionFactor[compactedTreeIndex] = 0.0F;
                                atLeastOneTreeRemoved = true;
                            }
                            if ((treeSelection == Constant.NoHarvestPeriod) || (treeSelection >= periodIndex))
                            {
                                // if tree is retained up to this period it's present in the current, compacted tree list and a compacted index increment 
                                // is needed
                                // Conversely, if tree was harvested in a previous period then it will have been removed from the live tree list by 
                                // compaction and no increment is needed.
                                ++compactedTreeIndex;
                            }
                        }
                        if (atLeastOneTreeRemoved)
                        {
                            treesOfSpecies.RemoveZeroExpansionFactorTrees();
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
                    Debug.Assert(simulationStand != null);
                    if (crownCompetitionByHeight == null)
                    {
                        crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.Configuration.Variant, simulationStand);
                    }
                    OrganonGrowth.Grow(periodIndex, this.Configuration, simulationStand, standDensity, this.organonCalibration, 
                                       ref crownCompetitionByHeight, out OrganonStandDensity standDensityAfterGrowth, out int _);

                    this.DensityByPeriod[periodIndex] = standDensityAfterGrowth;
                    if (this.StandByPeriod[periodIndex] == null)
                    {
                        // lazy initialization
                        OrganonStand standForPeriod = new OrganonStand(simulationStand);
                        Debug.Assert(standForPeriod.Name != null);
                        standForPeriod.Name = standForPeriod.Name[0..^1] + periodIndex;
                        this.StandByPeriod[periodIndex] = standForPeriod;
                    }
                    else
                    {
                        // update on resimulation
                        this.StandByPeriod[periodIndex]!.CopyTreeGrowthFrom(simulationStand);
                    }

                    // recalculate volume for this period
                    this.GetVolumeAndValue(periodIndex);

                    Debug.Assert((this.BasalAreaRemoved[periodIndex] > Constant.Bucking.MinimumBasalArea4SawEnglish && this.ThinningVolume.ScribnerTotal[periodIndex] > 0.0F) ||
                                  this.BasalAreaRemoved[periodIndex] == 0.0F);
                }
            }

            this.TreeSelectionChangedSinceLastSimulation = false;
        }
    }
}
