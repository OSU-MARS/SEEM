using Mars.Seem.Extensions;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Organon
{
    public class OrganonStandTrajectory : StandTrajectory<OrganonStand, OrganonStandDensity, OrganonTreatments>
    {
        private SortedList<FiaCode, SpeciesCalibration> organonCalibration;
        private OrganonGrowth organonGrowth;

        public OrganonConfiguration Configuration { get; private init; }

        public OrganonStandTrajectory(OrganonStand stand, OrganonConfiguration organonConfiguration, TreeScaling treeVolume, int lastPlanningPeriod)
            : base(stand, treeVolume, lastPlanningPeriod)
        {
            this.organonCalibration = organonConfiguration.CreateSpeciesCalibration();
            this.organonGrowth = new OrganonGrowth();

            this.Configuration = new OrganonConfiguration(organonConfiguration);

            this.Name = stand.Name;
            this.PeriodLengthInYears = organonConfiguration.Variant.TimeStepInYears;
            this.PeriodZeroAgeInYears = stand.AgeInYears;

            this.DensityByPeriod[0] = new OrganonStandDensity(organonConfiguration.Variant, stand);
            Debug.Assert(Constant.RegenerationHarvestIfEligible == 0, "Tree selection initialization assumes .");

            this.StandByPeriod[0] = new OrganonStand(stand) // subsequent periods initialized lazily in Simulate()
            {
                Name = this.Name + "p" + 0.ToString()
            };
        }

        // shallow copy FIA and Organon for now
        // deep copy of tree growth data
        public OrganonStandTrajectory(OrganonStandTrajectory other)
            : base(other)
        {
            this.organonCalibration = other.organonCalibration;
            this.organonGrowth = other.organonGrowth;

            this.Configuration = new OrganonConfiguration(other.Configuration);

            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity? otherDensity = other.DensityByPeriod[periodIndex];
                if (otherDensity != null)
                {
                    this.DensityByPeriod[periodIndex] = new OrganonStandDensity(otherDensity);
                }
            }
        }

        public override void ChangeThinningPeriod(int currentPeriod, int newPeriod)
        {
            Debug.Assert(currentPeriod != newPeriod);

            // check for corresponding harvest prescription
            // Harvests should already be shifted to the new period due to the call to CopyFrom().
            bool matchingHarvestFound = false;
            foreach (Harvest harvest in this.Treatments.Harvests)
            {
                if (harvest.Period == newPeriod)
                {
                    matchingHarvestFound = true;
                    break;
                }
            }
            if (matchingHarvestFound == false)
            {
                throw new NotSupportedException("Expected a harvest to have already been moved to period " + newPeriod + ".");
            }

            // clear no longer applicable basal area and fertilization
            // StandByPeriod and DensityByPeriod are updated at next simulation. Corresponding clear on base.BasalAreaRemovedByPeriod is
            // done in base.ChangeThinningPeriod().
            this.Treatments.BasalAreaThinnedByPeriod[currentPeriod] = 0.0F;
            this.Treatments.PoundsOfNitrogenPerAcreByPeriod[currentPeriod] = 0.0F;

            // move tree selection
            base.ChangeThinningPeriod(currentPeriod, newPeriod);
        }

        public override OrganonStandTrajectory Clone()
        {
            return new OrganonStandTrajectory(this);
        }

        public void CopyTreeGrowthFrom(OrganonStandTrajectory other)
        {
            base.CopyTreeGrowthFrom(other);

            // for now, shallow copies where feasible
            this.organonCalibration = other.organonCalibration;
            this.organonGrowth = other.organonGrowth; // BUGBUG: has no state, should have run state which can be copied

            // deep copies of mutable state changed by modified tree selection and resimulation
            int copyablePeriods = Math.Min(this.PlanningPeriods, other.PlanningPeriods);
            Debug.Assert((this.DensityByPeriod.Length == this.PlanningPeriods) &&
                         (this.StandByPeriod.Length == this.PlanningPeriods));

            this.Configuration.CopyFrom(other.Configuration);

            for (int periodIndex = 0; periodIndex < copyablePeriods; ++periodIndex)
            {
                OrganonStandDensity? otherDensity = other.DensityByPeriod[periodIndex];
                if (otherDensity != null)
                {
                    OrganonStandDensity? thisDensity = this.DensityByPeriod[periodIndex];
                    if (thisDensity == null)
                    {
                        this.DensityByPeriod[periodIndex] = new OrganonStandDensity(otherDensity);
                    }
                    else
                    {
                        thisDensity.CopyFrom(otherDensity);
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
            //for (int periodIndex = copyablePeriods; periodIndex < this.PlanningPeriods; ++periodIndex)
            //{
                // this.DensityByPeriod and StandByPeriod do not require clearing so long as EarliestPeriodChangedSinceLastSimulation is set correctly
                // However, this is a potentially confusing state while debugging.
            //    this.DensityByPeriod[periodIndex] = ?
            //    this.StandByPeriod[periodIndex] = null;
            //}

            Debug.Assert(IDictionaryExtensions.KeysIdentical(this.LongLogVolumeBySpecies, other.LongLogVolumeBySpecies) &&
                         IDictionaryExtensions.KeysIdentical(this.ForwardedVolumeBySpecies, other.ForwardedVolumeBySpecies));
            foreach (KeyValuePair<FiaCode, TreeSpeciesMerchantableVolume> otherStandingVolumeForSpecies in other.LongLogVolumeBySpecies)
            {
                FiaCode treeSpecies = otherStandingVolumeForSpecies.Key;
                TreeSpeciesMerchantableVolume thisStandingVolumeForSpecies = this.LongLogVolumeBySpecies[treeSpecies];
                TreeSpeciesMerchantableVolume otherThinningVolumeForSpecies = other.ForwardedVolumeBySpecies[treeSpecies];
                TreeSpeciesMerchantableVolume thisThinningVolumeForSpecies = this.ForwardedVolumeBySpecies[treeSpecies];

                thisStandingVolumeForSpecies.CopyFrom(otherStandingVolumeForSpecies.Value);
                thisThinningVolumeForSpecies.CopyFrom(otherThinningVolumeForSpecies);
            }

            this.Treatments.CopyTreeGrowthFrom(other.Treatments);
        }

        public override void CopyTreeGrowthFrom(StandTrajectory other)
        {
            if (other is OrganonStandTrajectory organonTrajectory)
            {
                this.CopyTreeGrowthFrom(organonTrajectory);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(other));
            }
        }

        public override float GetBasalAreaThinnedPerHa(int periodIndex)
        {
            float squareFeetPerAcre = this.Treatments.BasalAreaThinnedByPeriod[periodIndex]; // m²/acre
            return Constant.AcresPerHectare * Constant.MetersPerFoot * Constant.MetersPerFoot * squareFeetPerAcre; // m²/ha
        }

        public int GetInitialTreeRecordCount()
        {
            Stand initialStand = this.StandByPeriod[0] ?? throw new NotSupportedException("Initial stand infomation is missing.");
            return initialStand.GetTreeRecordCount();
        }

        public float GetTreeDiameter(int allSpeciesUncompactedTreeIndex, int periodIndex)
        {
            OrganonStand? standForPeriod = this.StandByPeriod[periodIndex];
            if (standForPeriod == null)
            {
                throw new InvalidOperationException("Stand for period " + periodIndex + " has not been simulated.");
            }

            foreach (Trees treesOfSpecies in standForPeriod.TreesBySpecies.Values)
            {
                int maximumUncompactedIndex = treesOfSpecies.UncompactedIndex[^1];
                if (allSpeciesUncompactedTreeIndex > maximumUncompactedIndex)
                {
                    continue;
                }

                int treeIndex = Array.BinarySearch<int>(treesOfSpecies.UncompactedIndex, allSpeciesUncompactedTreeIndex);
                if (treeIndex < 0)
                {
                    throw new InvalidOperationException("All species uncompacted index " + allSpeciesUncompactedTreeIndex + " expected to fall within tree species " + treesOfSpecies.Species + " but was not found.");
                }
                return treesOfSpecies.Dbh[treeIndex];
            }

            throw new ArgumentOutOfRangeException(nameof(allSpeciesUncompactedTreeIndex));
        }

        public override int Simulate()
        {
            if ((this.StandByPeriod.Length < 1) || (this.StandByPeriod[0] == null))
            {
                throw new InvalidOperationException("Initialization required for stand.");
            }
            if (this.StandByPeriod.Length < 2)
            {
                // no periods to simulate, so nothing to do
                // This is potentially an error condition in that Simulate() is being called as no op though, given Simulate()'s lazy
                // behavior and caching, it seems unlikely this will be a problem. And allowing this case permits consideration of rotation
                // lengths equal to the stand measurement age, which is sometimes useful for completeness.
                return 0;
            }

            // period 0 is the initial condition and therefore never needs to be simulated
            // Since simulation is computationally expensive, the current implementation is lazy and relies on triggers to simulate only on demand. In 
            // particular, in single entry cases no stand modification occurs before the target harvest period and, therefore, periods 1...entry - 1 need
            // to be simulated only once.
            bool periodNeedsSimulation = this.StandByPeriod[1] == null; // stands not yet simulated have nulls in StandByPeriod

            float[]? crownCompetitionByHeight = null;
            OrganonStand? simulationStand = periodNeedsSimulation ? new OrganonStand(this.StandByPeriod[0]!) : null;
            int growthModelEvaulations = 0;
            for (int periodIndex = 1; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity? standDensity = this.DensityByPeriod[periodIndex - 1];
                Debug.Assert(standDensity != null);

                // if treatments apply to this period, they may have already changed the tree selection or may change it in ApplyToPeriod()
                // In individual tree selection cases, tree selection will have been modified by a heuristic (in GRASP solution construction, by 
                // local search, by evolutionary operators, or some other mechanism) before this function is called. In thinning by prescription
                // tree selection occurs when the treatment is applied, likely resulting in a change. In either case, manipulation of tree
                // selection will the earliest period changed to this period.
                // It is also possible the tree selection has not unchanged, either because the treatments choose not to manipulate trees or
                // because there are no treatments, but the period needs to be simulated because this stand trajectory was copied from a shorter
                // trajectory and hasn't yet been extended to its last planning period.
                this.Treatments.ApplyToPeriod(periodIndex, this);
                periodNeedsSimulation |= this.EarliestPeriodChangedSinceLastSimulation == periodIndex;

                if (periodNeedsSimulation)
                {
                    if (simulationStand == null)
                    {
                        OrganonStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                        simulationStand = new OrganonStand(previousStand);
                    }
                    foreach (KeyValuePair<FiaCode, IndividualTreeSelection> individualTreeSelection in this.TreeSelectionBySpecies)
                    {
                        Trees treesOfSpecies = simulationStand.TreesBySpecies[individualTreeSelection.Key];
                        bool atLeastOneTreeRemoved = false;
                        for (int compactedTreeIndex = 0, uncompactedTreeIndex = 0; uncompactedTreeIndex < individualTreeSelection.Value.Count; ++uncompactedTreeIndex)
                        {
                            int treeSelection = individualTreeSelection.Value[uncompactedTreeIndex];
                            if (treeSelection == periodIndex)
                            {
                                // tree is harvested in this period, so set its expansion factor to zero
                                Debug.Assert(this.StandByPeriod[0]!.TreesBySpecies[treesOfSpecies.Species].Tag[uncompactedTreeIndex] == treesOfSpecies.Tag[compactedTreeIndex]);
                                Debug.Assert((periodIndex > 0) && (treesOfSpecies.LiveExpansionFactor[compactedTreeIndex] > 0.0F));
                                treesOfSpecies.LiveExpansionFactor[compactedTreeIndex] = 0.0F;
                                atLeastOneTreeRemoved = true;
                            }
                            if ((treeSelection == Constant.NoHarvestPeriod) || (treeSelection == Constant.RegenerationHarvestIfEligible) || (treeSelection >= periodIndex))
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
                        Debug.Assert(simulationStand.Name != null);
                        OrganonStand standForPeriod = new(simulationStand)
                        {
                            Name = this.Name + "p" + periodIndex
                        };
                        this.StandByPeriod[periodIndex] = standForPeriod;
                    }
                    else
                    {
                        // update on resimulation
                        this.StandByPeriod[periodIndex]!.CopyTreeGrowthFrom(simulationStand);
                    }

                    // mark volume for this period as needing recalculation
                    this.InvalidateMerchantableVolumes(periodIndex);
                    ++growthModelEvaulations;
                }
            }

            Debug.Assert((this.StandByPeriod[this.PlanningPeriods - 1] != null) && (this.Treatments.BasalAreaThinnedByPeriod.Count == this.PlanningPeriods)); // check for complete simulation
            this.EarliestPeriodChangedSinceLastSimulation = this.PlanningPeriods;
            return growthModelEvaulations;
        }
    }
}
