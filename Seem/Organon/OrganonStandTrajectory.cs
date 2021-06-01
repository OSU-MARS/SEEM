using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonStandTrajectory : StandTrajectory
    {
        private Dictionary<FiaCode, SpeciesCalibration> organonCalibration;
        private OrganonGrowth organonGrowth;

        public OrganonConfiguration Configuration { get; private init; }
        public OrganonStandDensity[] DensityByPeriod { get; private init; }

        public Heuristic? Heuristic { get; set; }
        public OrganonStand?[] StandByPeriod { get; private init; }
        public OrganonTreatments Treatments { get; private init; }

        public OrganonStandTrajectory(OrganonStand stand, OrganonConfiguration organonConfiguration, TimberValue timberValue, int lastPlanningPeriod)
            : base(timberValue, lastPlanningPeriod, 
                  stand.PlantingDensityInTreesPerHectare ?? throw new ArgumentOutOfRangeException(nameof(stand), "Stand's planting density is not specified.")) // base does range checks
        {
            if (timberValue == null)
            {
                throw new ArgumentNullException(nameof(timberValue));
            }

            this.organonCalibration = organonConfiguration.CreateSpeciesCalibration();
            this.organonGrowth = new OrganonGrowth();

            int maximumPlanningPeriodIndex = lastPlanningPeriod + 1;
            this.Configuration = new OrganonConfiguration(organonConfiguration);
            this.DensityByPeriod = new OrganonStandDensity[maximumPlanningPeriodIndex];

            this.Heuristic = null;
            this.Name = stand.Name;
            this.PeriodLengthInYears = organonConfiguration.Variant.TimeStepInYears;
            this.PeriodZeroAgeInYears = stand.AgeInYears;
            this.StandByPeriod = new OrganonStand[maximumPlanningPeriodIndex];
            this.Treatments = new OrganonTreatments();

            this.DensityByPeriod[0] = new OrganonStandDensity(organonConfiguration.Variant, stand);
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                this.IndividualTreeSelectionBySpecies.Add(treesOfSpecies.Species, new int[treesOfSpecies.Capacity]);
            }
            this.StandByPeriod[0] = new OrganonStand(stand); // subsequent periods initialized lazily in Simulate()
            this.StandByPeriod[0]!.Name += 0;

            this.GetVolume(0);
        }

        // shallow copy FIA and Organon for now
        // deep copy of tree growth data
        public OrganonStandTrajectory(OrganonStandTrajectory other)
            : base(other)
        {
            this.organonCalibration = other.organonCalibration;
            this.organonGrowth = other.organonGrowth;

            this.Configuration = new OrganonConfiguration(other.Configuration);
            this.DensityByPeriod = new OrganonStandDensity[other.PlanningPeriods];
            this.Heuristic = other.Heuristic;
            this.StandByPeriod = new OrganonStand[other.PlanningPeriods];
            this.Treatments = new();

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

            this.Treatments.CopyFrom(other.Treatments);
        }

        // this function doesn't currently copy
        //   Heuristic, Name, PeriodLengthInYears, PeriodZeroAgeInYears, PlantingDensityInTreesPerHectare, TimberValue, Treatments
        public void CopyTreeGrowthFrom(OrganonStandTrajectory other)
        {
            Debug.Assert(Object.ReferenceEquals(this, other) == false);

            if ((this.PeriodLengthInYears != other.PeriodLengthInYears) ||
                (this.PeriodZeroAgeInYears != other.PeriodZeroAgeInYears) ||
                (this.PlantingDensityInTreesPerHectare != other.PlantingDensityInTreesPerHectare) ||
                (Object.ReferenceEquals(this.TimberValue, other.TimberValue) == false) ||
                (this.Treatments.Harvests.Count != other.Treatments.Harvests.Count))
            {
                // no apparent need to check heuristics for compatibility
                // number of planning periods may differ: as many periods are copied as possible
                throw new ArgumentOutOfRangeException(nameof(other));
            }
            for (int harvestIndex = 0; harvestIndex < this.Treatments.Harvests.Count; ++harvestIndex)
            {
                IHarvest thisHarvest = this.Treatments.Harvests[harvestIndex];
                IHarvest otherHarvest = other.Treatments.Harvests[harvestIndex];
                if (thisHarvest.Period != otherHarvest.Period)
                {
                    throw new NotSupportedException(nameof(other));
                }
            }

            // for now, shallow copies where feasible
            this.organonCalibration = other.organonCalibration;
            this.organonGrowth = other.organonGrowth; // BUGBUG: has no state, should have run state which can be copied

            // deep copies of mutable state changed by modified tree selection and resimulation
            // May need deep copy of treatment because 
            // 1) thinning prescriptions are being evaluated and therefore the best prescription needs to be reported
            // 2) BUGBUG: no Organon run state object has been implemented
            int copyablePeriods = Math.Min(this.PlanningPeriods, other.PlanningPeriods);
            Debug.Assert((this.BasalAreaRemoved.Length == this.PlanningPeriods) &&
                         (this.DensityByPeriod.Length == this.PlanningPeriods) &&
                         (this.StandByPeriod.Length == this.PlanningPeriods));

            this.Configuration.CopyFrom(other.Configuration);

            foreach (KeyValuePair<FiaCode, int[]> otherSelectionForSpecies in other.IndividualTreeSelectionBySpecies)
            {
                int[] thisSelectionForSpecies = this.IndividualTreeSelectionBySpecies[otherSelectionForSpecies.Key];
                if (otherSelectionForSpecies.Value.Length != thisSelectionForSpecies.Length)
                {
                    throw new NotSupportedException("Individual tree selections are of different lengths.");
                }
                Array.Copy(otherSelectionForSpecies.Value, 0, thisSelectionForSpecies, 0, thisSelectionForSpecies.Length);
            }
            this.EarliestPeriodChangedSinceLastSimulation = Math.Min(other.EarliestPeriodChangedSinceLastSimulation, this.PlanningPeriods + 1);

            for (int periodIndex = 0; periodIndex < copyablePeriods; ++periodIndex)
            {
                this.BasalAreaRemoved[periodIndex] = other.BasalAreaRemoved[periodIndex];

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
            for (int periodIndex = copyablePeriods; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                this.BasalAreaRemoved[periodIndex] = 0;
                // this.DensityByPeriod and StandByPeriod do not require clearing so long as EarliestPeriodChangedSinceLastSimulation is set correctly
                // However, this is a potentially confusing state while debugging.
                // this.DensityByPeriod[periodIndex] = ?
                // this.StandByPeriod[periodIndex] = null;
            }

            this.StandingVolume.CopyFrom(other.StandingVolume);
            this.ThinningVolume.CopyFrom(other.ThinningVolume);
        }

        public int GetInitialTreeRecordCount()
        {
            Stand initialStand = this.StandByPeriod[0] ?? throw new NotSupportedException("Initial stand infomation is missing.");
            return initialStand.GetTreeRecordCount();
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

        private void GetVolume(int periodIndex)
        {
            // harvest volumes, if applicable
            foreach (IHarvest harvest in this.Treatments.Harvests)
            {
                if (harvest.Period == periodIndex)
                {
                    // tree's expansion factor is set to zero when it's marked for harvest
                    // Use tree's volume at end of the the previous period.
                    // TODO: track per species volumes
                    OrganonStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                    this.ThinningVolume.SetScribnerHarvest(previousStand, this.IndividualTreeSelectionBySpecies, periodIndex, this.TimberValue);
                    // could make more specific by checking if harvest removes at least one tree
                    Debug.Assert((this.BasalAreaRemoved[periodIndex] > Constant.Bucking.MinimumBasalArea4SawEnglish && this.ThinningVolume.GetScribnerTotal(periodIndex) > 0.0F) ||
                                  this.ThinningVolume.GetScribnerTotal(periodIndex) == 0.0F);
                }
            }

            // standing volume
            OrganonStand stand = this.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information is not available for period " + periodIndex + ".");
            this.StandingVolume.SetStandingScribnerVolume(stand, periodIndex, this.TimberValue);
        }

        public void GetGradedVolumes(out StandCubicAndScribnerVolume gradedVolumeStanding, out StandCubicAndScribnerVolume gradedVolumeHarvested)
        {
            gradedVolumeHarvested = new StandCubicAndScribnerVolume(this.ThinningVolume);
            gradedVolumeStanding = new StandCubicAndScribnerVolume(this.StandingVolume);
            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                // harvest volumes, if applicable
                foreach (IHarvest harvest in this.Treatments.Harvests)
                {
                    if (harvest.Period == periodIndex)
                    {
                        // tree's expansion factor is set to zero when it's marked for harvest
                        // Use tree's volume at end of the the previous period.
                        // TODO: track per species volumes
                        OrganonStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                        gradedVolumeHarvested.SetCubicVolumeHarvested(previousStand, this.IndividualTreeSelectionBySpecies, periodIndex, this.TimberValue);
                    }
                }

                // standing volume
                OrganonStand stand = this.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information is not available for period " + periodIndex + ".");
                gradedVolumeStanding.SetStandingCubicVolume(stand, periodIndex, this.TimberValue);
            }
        }

        public int Simulate()
        {
            if (this.StandByPeriod.Length < 2)
            {
                throw new NotSupportedException("Stand has no periods to simulate.");
            }
            if (this.StandByPeriod[0] == null)
            {
                throw new NotSupportedException("Pre-simulation information required for stand.");
            }

            // period 0 is the initial condition and therefore never needs to be simulated
            // Since simulation is computationally expensive, the current implementation is lazy and relies on triggers to simulate only on demand. In 
            // particular, in single entry cases no stand modification occurs before the target harvest period and, therefore, periods 1...entry - 1 need
            // to be simulated only once.
            // TODO: make tree selection logic smart enough to track the earliest period needing resimulation
            bool periodNeedsSimulation = this.StandByPeriod[1] == null; // stands not yet simulated have nulls in StandByPeriod

            float[]? crownCompetitionByHeight = null;
            OrganonStand? simulationStand = periodNeedsSimulation ? new OrganonStand(this.StandByPeriod[0]!) : null;
            int growthModelEvaulations = 0;
            for (int periodIndex = 1; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity standDensity = this.DensityByPeriod[periodIndex - 1];

                // if treatments apply to this period, they may or may not have changed the tree selection
                // In individual tree selection cases, tree selection will have been modified by a heuristic (in GRASP solution construction, by 
                // local search, by evolutionary operators, or some other mechanism) before this function is called. In thinning by prescription
                // tree selection occurs when the treatment is applied, likely resulting in a change.
                if (this.Treatments.ApplyToPeriod(periodIndex, this))
                {
                    periodNeedsSimulation |= this.EarliestPeriodChangedSinceLastSimulation == periodIndex;
                }

                if (periodNeedsSimulation)
                {
                    if (simulationStand == null)
                    {
                        OrganonStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                        simulationStand = new OrganonStand(previousStand);
                    }
                    foreach (KeyValuePair<FiaCode, int[]> individualTreeSelection in this.IndividualTreeSelectionBySpecies)
                    {
                        Trees treesOfSpecies = simulationStand.TreesBySpecies[individualTreeSelection.Key];
                        bool atLeastOneTreeRemoved = false;
                        // loop assumes trailing capacity is set to zero and of insignificant length
                        // if needed, loop can be changed to use either the simulation stand's tree count or a reference tree count rather than capacity
                        for (int compactedTreeIndex = 0, uncompactedTreeIndex = 0; uncompactedTreeIndex < individualTreeSelection.Value.Length; ++uncompactedTreeIndex)
                        {
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

                    this.BasalAreaRemoved[periodIndex] = this.Treatments.BasalAreaRemovedByPeriod[periodIndex];
                    crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.Configuration.Variant, simulationStand);
                    standDensity = new OrganonStandDensity(this.Configuration.Variant, simulationStand);
                }

                if (periodNeedsSimulation)
                {
                    // simulate this period
                    Debug.Assert(simulationStand != null);
                    if (crownCompetitionByHeight == null)
                    {
                        crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(this.Configuration.Variant, simulationStand);
                    }
                    OrganonGrowth.Grow(this.Configuration, this.Treatments, simulationStand, standDensity, this.organonCalibration, 
                                       ref crownCompetitionByHeight, out OrganonStandDensity standDensityAfterGrowth, out int _);

                    this.DensityByPeriod[periodIndex] = standDensityAfterGrowth;
                    if (this.StandByPeriod[periodIndex] == null)
                    {
                        // lazy initialization
                        OrganonStand standForPeriod = new(simulationStand);
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
                    this.GetVolume(periodIndex);

                    Debug.Assert((this.BasalAreaRemoved[periodIndex] > Constant.Bucking.MinimumBasalArea4SawEnglish && this.ThinningVolume.GetScribnerTotal(periodIndex) > 0.0F) ||
                                  this.BasalAreaRemoved[periodIndex] == 0.0F);
                    ++growthModelEvaulations;
                }
            }

            this.EarliestPeriodChangedSinceLastSimulation = this.PlanningPeriods;
            return growthModelEvaulations;
        }
    }
}
