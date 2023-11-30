using Mars.Seem.Extensions;
using Mars.Seem.Silviculture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mars.Seem.Tree
{
    public abstract class StandTrajectory
    {
        public int EarliestPeriodChangedSinceLastSimulation { get; protected set; }
        public string? Name { get; set; }
        public int PeriodLengthInYears { get; set; }
        public int PeriodZeroAgeInYears { get; set; }

        public int PlanningPeriods { get; private init; } // for now, synonymous with rotation length
        public float PlantingDensityInTreesPerHectare { get; private init; } // trees per hectare

        public SortedList<FiaCode, TreeSpeciesMerchantableVolume> ForwardedThinVolumeBySpecies { get; private init; }
        public SortedList<FiaCode, TreeSpeciesMerchantableVolume> LongLogRegenerationVolumeBySpecies { get; private init; }
        public SortedList<FiaCode, TreeSpeciesMerchantableVolume> LongLogThinVolumeBySpecies { get; private init; }

        // harvest periods by tree, Constant.NoHarvestPeriod indicates no harvest
        public IndividualTreeSelectionBySpecies TreeSelectionBySpecies { get; private init; }
        public TreeScaling TreeScaling { get; set; }

        protected StandTrajectory(TreeScaling treeVolume, int lastPlanningPeriod, float plantingDensityInTreesPerHectare)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(lastPlanningPeriod);
            if ((plantingDensityInTreesPerHectare <= 0.0F) || (plantingDensityInTreesPerHectare > Constant.Maximum.PlantingDensityInTreesPerHectare))
            {
                throw new ArgumentOutOfRangeException(nameof(plantingDensityInTreesPerHectare));
            }

            this.EarliestPeriodChangedSinceLastSimulation = 0;
            this.ForwardedThinVolumeBySpecies = [];
            this.LongLogRegenerationVolumeBySpecies = [];
            this.LongLogThinVolumeBySpecies = [];
            this.Name = null;
            this.PeriodLengthInYears = -1;
            this.PeriodZeroAgeInYears = -1;
            this.PlantingDensityInTreesPerHectare = plantingDensityInTreesPerHectare;
            this.PlanningPeriods = lastPlanningPeriod + 1;
            this.TreeSelectionBySpecies = [];
            this.TreeScaling = treeVolume;
        }

        protected StandTrajectory(StandTrajectory other)
        {
            this.EarliestPeriodChangedSinceLastSimulation = other.EarliestPeriodChangedSinceLastSimulation;
            this.ForwardedThinVolumeBySpecies = [];
            this.LongLogRegenerationVolumeBySpecies = [];
            this.LongLogThinVolumeBySpecies = [];
            this.Name = other.Name;
            this.PeriodLengthInYears = other.PeriodLengthInYears;
            this.PeriodZeroAgeInYears = other.PeriodZeroAgeInYears;
            this.PlantingDensityInTreesPerHectare = other.PlantingDensityInTreesPerHectare;
            this.PlanningPeriods = other.PlanningPeriods;
            this.TreeSelectionBySpecies = [];
            this.TreeScaling = other.TreeScaling; // runtime immutable, assumed thread safe for shallow copy

            foreach (FiaCode treeSpecies in other.TreeSelectionBySpecies.Keys)
            {
                this.ForwardedThinVolumeBySpecies.Add(treeSpecies, new TreeSpeciesMerchantableVolume(other.ForwardedThinVolumeBySpecies[treeSpecies]));
                this.LongLogThinVolumeBySpecies.Add(treeSpecies, new TreeSpeciesMerchantableVolume(other.LongLogThinVolumeBySpecies[treeSpecies]));
                this.LongLogRegenerationVolumeBySpecies.Add(treeSpecies, new TreeSpeciesMerchantableVolume(other.LongLogRegenerationVolumeBySpecies[treeSpecies]));
                this.TreeSelectionBySpecies.Add(treeSpecies, new IndividualTreeSelection(other.TreeSelectionBySpecies[treeSpecies]));
            }
        }

        public abstract Stand?[] StandByPeriod { get; }
        public abstract Treatments Treatments { get; }

        public virtual void ChangeThinningPeriod(int currentPeriod, int newPeriod)
        {
            Debug.Assert(currentPeriod != newPeriod);
            if ((currentPeriod < 0) || (currentPeriod > this.PlanningPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(currentPeriod));
            }
            if ((newPeriod < 0) || (newPeriod > this.PlanningPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(newPeriod));
            }

            bool atLeastOneTreeMoved = false;
            foreach (IndividualTreeSelection treeSelectionForSpecies in this.TreeSelectionBySpecies.Values)
            {
                for (int uncompactedTreeIndex = 0; uncompactedTreeIndex < treeSelectionForSpecies.Count; ++uncompactedTreeIndex)
                {
                    if (treeSelectionForSpecies[uncompactedTreeIndex] == currentPeriod)
                    {
                        treeSelectionForSpecies[uncompactedTreeIndex] = newPeriod;
                        atLeastOneTreeMoved = true;
                    }
                }
            }

            if (atLeastOneTreeMoved)
            {
                this.UpdateEariestPeriodChanged(currentPeriod, newPeriod);
            }
        }

        public abstract StandTrajectory Clone();

        // this function doesn't currently copy
        //   Name, PeriodLengthInYears, PeriodZeroAgeInYears, PlantingDensityInTreesPerHectare, Treatments.Harvests, TreeVolume
        public virtual void CopyTreeGrowthFrom(StandTrajectory other)
        {
            Debug.Assert(Object.ReferenceEquals(this, other) == false);

            if ((this.PeriodLengthInYears != other.PeriodLengthInYears) ||
                (this.PeriodZeroAgeInYears != other.PeriodZeroAgeInYears) ||
                (this.PlantingDensityInTreesPerHectare != other.PlantingDensityInTreesPerHectare) ||
                (this.Treatments.Harvests.Count != other.Treatments.Harvests.Count) ||
                (Object.ReferenceEquals(this.TreeScaling, other.TreeScaling) == false))
            {
                // no apparent need to check heuristics for compatibility
                // number of planning periods may differ: as many periods are copied as possible
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            // update period changed and copy tree selection
            // Since period changed is flowed, low level copy of selections doesn't result in loss of state integrity.
            this.EarliestPeriodChangedSinceLastSimulation = Math.Min(other.EarliestPeriodChangedSinceLastSimulation, this.PlanningPeriods);
            foreach (KeyValuePair<FiaCode, IndividualTreeSelection> otherSelectionForSpecies in other.TreeSelectionBySpecies)
            {
                IndividualTreeSelection thisSelectionForSpecies = this.TreeSelectionBySpecies[otherSelectionForSpecies.Key];
                thisSelectionForSpecies.CopyFrom(otherSelectionForSpecies.Value);
            }

            Debug.Assert(IDictionaryExtensions.KeysIdentical(this.ForwardedThinVolumeBySpecies, other.ForwardedThinVolumeBySpecies) &&
                         IDictionaryExtensions.KeysIdentical(this.LongLogThinVolumeBySpecies, other.LongLogThinVolumeBySpecies) &&
                         IDictionaryExtensions.KeysIdentical(this.LongLogRegenerationVolumeBySpecies, other.LongLogRegenerationVolumeBySpecies));
            for (int treeSpeciesIndex = 0; treeSpeciesIndex < other.ForwardedThinVolumeBySpecies.Count; ++treeSpeciesIndex)
            {
                Debug.Assert((this.ForwardedThinVolumeBySpecies.Keys[treeSpeciesIndex] == other.ForwardedThinVolumeBySpecies.Keys[treeSpeciesIndex]) &&
                             (this.LongLogThinVolumeBySpecies.Keys[treeSpeciesIndex] == other.LongLogThinVolumeBySpecies.Keys[treeSpeciesIndex]) &&
                             (this.LongLogRegenerationVolumeBySpecies.Keys[treeSpeciesIndex] == other.LongLogRegenerationVolumeBySpecies.Keys[treeSpeciesIndex]));

                TreeSpeciesMerchantableVolume otherForwardedThinVolumeForSpecies = other.ForwardedThinVolumeBySpecies.Values[treeSpeciesIndex];
                TreeSpeciesMerchantableVolume thisForwardedThinVolumeForSpecies = this.ForwardedThinVolumeBySpecies.Values[treeSpeciesIndex];
                thisForwardedThinVolumeForSpecies.CopyFrom(otherForwardedThinVolumeForSpecies);

                TreeSpeciesMerchantableVolume otherLongLogThinVolumeForSpecies = other.LongLogThinVolumeBySpecies.Values[treeSpeciesIndex];
                TreeSpeciesMerchantableVolume thisLongLogThinVolumeForSpecies = this.LongLogThinVolumeBySpecies.Values[treeSpeciesIndex];
                thisLongLogThinVolumeForSpecies.CopyFrom(otherLongLogThinVolumeForSpecies);

                TreeSpeciesMerchantableVolume otherLongLogRegenVolumeForSpecies = other.LongLogRegenerationVolumeBySpecies.Values[treeSpeciesIndex];
                TreeSpeciesMerchantableVolume thisLongLogRegenVolumeForSpecies = this.LongLogRegenerationVolumeBySpecies.Values[treeSpeciesIndex];
                thisLongLogRegenVolumeForSpecies.CopyFrom(otherLongLogRegenVolumeForSpecies);
            }
        }

        public void CopyTreeSelectionFrom(IndividualTreeSelectionBySpecies otherTreeSelection)
        {
            if (this.TreeSelectionBySpecies.Count != otherTreeSelection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(otherTreeSelection), "Attempt to copy between mismatched tree selections. This stand trajectory has tree selections for " + this.TreeSelectionBySpecies.Count + " species. The tree selection provided has " + otherTreeSelection.Count + " species.");
            }

            bool atLeastOneTreeMoved = false;
            int earliestHarvestPeriodChanged = Int32.MaxValue;
            for (int speciesIndex = 0; speciesIndex < this.TreeSelectionBySpecies.Count; ++speciesIndex)
            {
                FiaCode otherSpecies = otherTreeSelection.Keys[speciesIndex];
                FiaCode thisSpecies = this.TreeSelectionBySpecies.Keys[speciesIndex];
                if (otherSpecies != thisSpecies)
                {
                    throw new ArgumentOutOfRangeException(nameof(otherTreeSelection), "Attempt to copy between mismatched tree selections. This stand trajectory has " + thisSpecies + " at index " + speciesIndex + " and the tree selection provided has " + otherSpecies + ".");
                }

                IndividualTreeSelection otherSelectionForSpecies = otherTreeSelection.Values[speciesIndex];
                IndividualTreeSelection thisSelectionForSpecies = this.TreeSelectionBySpecies.Values[speciesIndex];
                if (otherSelectionForSpecies.Count != thisSelectionForSpecies.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(otherTreeSelection));
                }

                for (int uncompactedTreeIndex = 0; uncompactedTreeIndex < thisSelectionForSpecies.Count; ++uncompactedTreeIndex)
                {
                    int otherSelection = otherSelectionForSpecies[uncompactedTreeIndex];
                    int thisSelection = thisSelectionForSpecies[uncompactedTreeIndex];
                    if (otherSelection != thisSelection)
                    {
                        thisSelectionForSpecies[uncompactedTreeIndex] = otherSelection;
                        atLeastOneTreeMoved = true;

                        if ((otherSelection == Constant.NoHarvestPeriod) || (otherSelection == Constant.RegenerationHarvestIfEligible))
                        {
                            if (thisSelection != Constant.NoHarvestPeriod)
                            {
                                earliestHarvestPeriodChanged = Math.Min(earliestHarvestPeriodChanged, thisSelection);
                            }
                        }
                        else
                        {
                            earliestHarvestPeriodChanged = Math.Min(earliestHarvestPeriodChanged, otherSelection);
                        }
                    }
                }
            }

            if (atLeastOneTreeMoved)
            {
                this.UpdateEariestPeriodChanged(this.EarliestPeriodChangedSinceLastSimulation, earliestHarvestPeriodChanged);
            }
        }

        public void CopyTreeSelectionTo(IndividualTreeSelectionBySpecies otherTreeSelection)
        {
            Debug.Assert(IDictionaryExtensions.KeysIdentical(this.TreeSelectionBySpecies, otherTreeSelection));
            foreach (KeyValuePair<FiaCode, IndividualTreeSelection> thisSelectionForSpecies in this.TreeSelectionBySpecies)
            {
                IndividualTreeSelection otherSelectionForSpecies = otherTreeSelection[thisSelectionForSpecies.Key];
                thisSelectionForSpecies.Value.CopyTo(otherSelectionForSpecies);
            }
        }

        public void MoveAllEligibleTreesToRegenerationHarvest()
        {
            // see remarks in loop
            Debug.Assert(Constant.RegenerationHarvestIfEligible == 0);

            foreach (IndividualTreeSelection selectionForSpecies in this.TreeSelectionBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < selectionForSpecies.Count; ++treeIndex)
                {
                    int currentHarvestPeriod = selectionForSpecies[treeIndex];
                    if (currentHarvestPeriod == Constant.NoHarvestPeriod)
                    {
                        continue;
                    }

                    if (currentHarvestPeriod != Constant.RegenerationHarvestIfEligible)
                    {
                        selectionForSpecies[treeIndex] = Constant.RegenerationHarvestIfEligible;
                        // if stand has been simulated then the earliest period affected by removing all thinning is the first thin performed
                        this.EarliestPeriodChangedSinceLastSimulation = Math.Min(this.EarliestPeriodChangedSinceLastSimulation, currentHarvestPeriod);
                    }
                }
            }
        }

        public abstract float GetBasalAreaThinnedPerHa(int periodIndex);

        public virtual int GetEndOfPeriodAge(int period)
        {
            Debug.Assert((period >= 0) && (period < this.PlanningPeriods));
            Debug.Assert((this.PeriodZeroAgeInYears >= 0) && (this.PeriodLengthInYears > 0));
            return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * period;
        }

        // this function's existence isn't entirely desirable as it reverses ConstructTreeSelection()'s internal logic
        // Not currently enough of an advantage for refactoring selection construction to return period indices to be worthwhile.
        public int[] GetHarvestPeriodIndices(IList<int> thinningPeriods)
        {
            int treeRecordCount = this.TreeSelectionBySpecies.Values.Sum(treeSelection => treeSelection.Count);

            int[] periodIndices = new int[treeRecordCount];
            int allSpeciesUncompactedTreeIndex = 0;
            foreach (IndividualTreeSelection individualTreeSelection in this.TreeSelectionBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < individualTreeSelection.Count; ++treeIndex)
                {
                    int currentTreeHarvestPeriod = individualTreeSelection[treeIndex];
                    for (int periodIndex = 0; periodIndex < thinningPeriods.Count; ++periodIndex)
                    {
                        if (thinningPeriods[periodIndex] == currentTreeHarvestPeriod)
                        {
                            periodIndices[allSpeciesUncompactedTreeIndex] = periodIndex;
                            break;
                        }
                    }
                    // if needed, check for case where tree's harvest period is not found in thinningPeriods

                    ++allSpeciesUncompactedTreeIndex;
                }
            }

            return periodIndices;
        }

        public int GetFirstThinAge()
        {
            int firstHarvestPeriod = this.GetFirstThinPeriod();
            if (firstHarvestPeriod == Constant.NoHarvestPeriod)
            {
                return -1;
            }
            return this.GetEndOfPeriodAge(firstHarvestPeriod);
        }

        public abstract int GetFirstThinPeriod();

        public (StandMerchantableVolume forwardedThinVolume, StandMerchantableVolume longLogThinVolume, StandMerchantableVolume longLogRegenVolume) GetMerchantableVolumes()
        {
            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                this.RecalculateThinningMerchantableVolumeIfNeeded(periodIndex);
                this.RecalculateRegenerationHarvestMerchantableVolumeIfNeeded(periodIndex);
            }

            StandMerchantableVolume forwardedThinVolume = new(this.ForwardedThinVolumeBySpecies);
            StandMerchantableVolume longLogThinVolume = new(this.LongLogThinVolumeBySpecies);
            StandMerchantableVolume longLogRegenVolume = new(this.LongLogRegenerationVolumeBySpecies);
            return (forwardedThinVolume, longLogThinVolume, longLogRegenVolume);
        }

        public int GetSecondThinAge()
        {
            return this.GetThinAge(1);
        }

        public int GetSecondThinPeriod()
        {
            return this.GetThinPeriod(1);
        }

        public abstract StandDensity GetStandDensity(int periodIndex);

        public virtual int GetStartOfPeriodAge(int period)
        {
            Debug.Assert((period >= 0) && (period < this.PlanningPeriods));
            Debug.Assert((this.PeriodZeroAgeInYears >= 0) && (this.PeriodLengthInYears > 0));
            return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * (period - 1);
        }

        private int GetThinAge(int thinIndex)
        {
            int harvestPeriod = this.GetThinPeriod(thinIndex);
            if (harvestPeriod == Constant.NoHarvestPeriod)
            {
                return Constant.NoHarvestPeriod;
            }
            return this.GetEndOfPeriodAge(harvestPeriod);
        }

        protected abstract int GetThinPeriod(int thinning);

        public int GetThirdThinAge()
        {
            return this.GetThinAge(2);
        }

        public int GetThirdThinPeriod()
        {
            return this.GetThinPeriod(2);
        }

        public float GetTotalCubicVolumeThinned(int periodIndex)
        {
            float totalVolume = 0.0F;
            foreach (TreeSpeciesMerchantableVolume thinVolumeForSpecies in this.ForwardedThinVolumeBySpecies.Values)
            {
                totalVolume += thinVolumeForSpecies.GetCubicTotal(periodIndex);
            }
            return totalVolume;
        }

        public float GetTotalScribnerVolumeThinned(int periodIndex)
        {
            float totalVolume = 0.0F;
            foreach (TreeSpeciesMerchantableVolume thinVolumeForSpecies in this.ForwardedThinVolumeBySpecies.Values)
            {
                totalVolume += thinVolumeForSpecies.GetScribnerTotal(periodIndex);
            }
            return totalVolume;
        }

        public float GetTotalRegenerationHarvestMerchantableCubicVolume(int periodIndex)
        {
            float totalVolume = 0.0F;
            foreach (TreeSpeciesMerchantableVolume standingVolumeForSpecies in this.LongLogRegenerationVolumeBySpecies.Values)
            {
                totalVolume += standingVolumeForSpecies.GetCubicTotal(periodIndex);
            }
            return totalVolume;
        }

        public float GetTotalRegenerationHarvestMerchantableScribnerVolume(int periodIndex)
        {
            float totalVolume = 0.0F;
            foreach (TreeSpeciesMerchantableVolume standingVolumeForSpecies in this.LongLogRegenerationVolumeBySpecies.Values)
            {
                totalVolume += standingVolumeForSpecies.GetScribnerTotal(periodIndex);
            }
            return totalVolume;
        }

        public int GetTreeSelection(int allSpeciesUncompactedTreeIndex)
        {
            int treeIndex = allSpeciesUncompactedTreeIndex;
            foreach (IndividualTreeSelection individualTreeSelection in this.TreeSelectionBySpecies.Values)
            {
                if (treeIndex < individualTreeSelection.Count)
                {
                    return individualTreeSelection[treeIndex];
                }
                treeIndex -= individualTreeSelection.Count;
            }

            throw new ArgumentOutOfRangeException(nameof(allSpeciesUncompactedTreeIndex));
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

        public bool HasThinInPeriod(int periodIndex)
        {
            for (int harvestIndex = 0; harvestIndex < this.Treatments.Harvests.Count; ++harvestIndex)
            {
                Harvest harvest = this.Treatments.Harvests[harvestIndex];
                if (harvest.Period == periodIndex)
                {
                    return true;
                }
            }

            return false;
        }

        public void InvalidateMerchantableVolumes(int periodIndex)
        {
            for (int treeSpeciesIndex = 0; treeSpeciesIndex < this.ForwardedThinVolumeBySpecies.Count; ++treeSpeciesIndex)
            {
                TreeSpeciesMerchantableVolume forwardedThinVolumeForSpecies = this.ForwardedThinVolumeBySpecies.Values[treeSpeciesIndex];
                forwardedThinVolumeForSpecies.MarkUncalculated(periodIndex);
            }
            for (int treeSpeciesIndex = 0; treeSpeciesIndex < this.LongLogThinVolumeBySpecies.Count; ++treeSpeciesIndex)
            {
                TreeSpeciesMerchantableVolume longLogThinVolumeForSpecies = this.LongLogThinVolumeBySpecies.Values[treeSpeciesIndex];
                longLogThinVolumeForSpecies.MarkUncalculated(periodIndex);
            }
            for (int treeSpeciesIndex = 0; treeSpeciesIndex < this.LongLogRegenerationVolumeBySpecies.Count; ++treeSpeciesIndex)
            {
                TreeSpeciesMerchantableVolume longLogRegenVolumeForSpecies = this.LongLogRegenerationVolumeBySpecies.Values[treeSpeciesIndex];
                longLogRegenVolumeForSpecies.MarkUncalculated(periodIndex);
            }
        }

        public abstract void RecalculateRegenerationHarvestMerchantableVolumeIfNeeded(int periodIndex);
        public abstract void RecalculateThinningMerchantableVolumeIfNeeded(int periodIndex);

        public void SetTreeSelection(int allSpeciesUncompactedTreeIndex, int newHarvestPeriod)
        {
            if ((newHarvestPeriod < 0) || (newHarvestPeriod >= this.PlanningPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(newHarvestPeriod));
            }

            int treeIndex = allSpeciesUncompactedTreeIndex;
            foreach (KeyValuePair<FiaCode, IndividualTreeSelection> selectionForSpecies in this.TreeSelectionBySpecies)
            {
                IndividualTreeSelection individualTreeSelection = selectionForSpecies.Value;
                if (treeIndex < individualTreeSelection.Count)
                {
                    int currentHarvestPeriod = individualTreeSelection[treeIndex];
                    Debug.Assert(currentHarvestPeriod != Constant.NoHarvestPeriod); // for now
                    if (currentHarvestPeriod != newHarvestPeriod)
                    {
                        individualTreeSelection[treeIndex] = newHarvestPeriod;
                        this.UpdateEariestPeriodChanged(currentHarvestPeriod, newHarvestPeriod);
                    }
                    return;
                }
                treeIndex -= individualTreeSelection.Count;
            }

            throw new ArgumentOutOfRangeException(nameof(allSpeciesUncompactedTreeIndex));
        }

        public void SetTreeSelection(FiaCode species, int uncompactedTreeIndex, int newHarvestPeriod)
        {
            if ((newHarvestPeriod < Constant.RegenerationHarvestIfEligible) || (newHarvestPeriod >= this.PlanningPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(newHarvestPeriod));
            }

            int currentHarvestPeriod = this.TreeSelectionBySpecies[species][uncompactedTreeIndex];
            if (currentHarvestPeriod != newHarvestPeriod)
            {
                if (currentHarvestPeriod == Constant.NoHarvestPeriod)
                {
                    throw new ArgumentOutOfRangeException(nameof(newHarvestPeriod), "Attempt to mark " + species + " at uncompacted tree index " + uncompactedTreeIndex + " for harvest in period " + newHarvestPeriod + " but tree is marked as excluded from harvest.");
                }
                this.TreeSelectionBySpecies[species][uncompactedTreeIndex] = newHarvestPeriod;
                this.UpdateEariestPeriodChanged(currentHarvestPeriod, newHarvestPeriod);
            }
        }

        public abstract int Simulate();

        private void UpdateEariestPeriodChanged(int currentPeriod, int newPeriod)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(newPeriod, Constant.NoHarvestPeriod);

            Debug.Assert((currentPeriod >= 0) && (currentPeriod <= this.PlanningPeriods) && (newPeriod >= 0) && (newPeriod <= this.PlanningPeriods) && (Constant.RegenerationHarvestIfEligible == 0));

            // four cases
            //   1) tree is not scheduled for thinning and becomes scheduled -> earliest affected period is harvest period
            //   2) tree is scheduled for thinning and becomes unscheduled -> earliest affected period is harvest period
            //   3) tree is reassinged to an earlier harvest period -> earliest affected period is earliest harvest period
            //   4) tree is reassinged to a later harvest period -> earliest affected period is still the earliest harvest period
            int earliestAffectedPeriod;
            if (currentPeriod == Constant.RegenerationHarvestIfEligible)
            {
                earliestAffectedPeriod = newPeriod;
            }
            else if (newPeriod == Constant.RegenerationHarvestIfEligible)
            {
                earliestAffectedPeriod = currentPeriod;
            }
            else
            {
                earliestAffectedPeriod = Math.Min(currentPeriod, newPeriod);
            }

            this.EarliestPeriodChangedSinceLastSimulation = Math.Min(this.EarliestPeriodChangedSinceLastSimulation, earliestAffectedPeriod);
        }
    }

    public abstract class StandTrajectory<TStand, TStandDensity, TTreatments> : StandTrajectory 
        where TStand : Stand
        where TStandDensity : StandDensity
        where TTreatments : Treatments, new()
    {
        private readonly TStand?[] standByPeriod;
        private readonly TTreatments treatments;

        public TStandDensity?[] DensityByPeriod { get; private init; }

        protected StandTrajectory(Stand stand, TreeScaling treeVolume, int lastPlanningPeriod) :
            base(treeVolume, lastPlanningPeriod, stand.PlantingDensityInTreesPerHectare ?? throw new ArgumentOutOfRangeException(nameof(stand), "Stand's planting density is not specified.")) // base does range checks
        {
            this.standByPeriod = new TStand[this.PlanningPeriods];
            this.treatments = new();

            int maximumPlanningPeriodIndex = lastPlanningPeriod + 1;
            this.DensityByPeriod = new TStandDensity[maximumPlanningPeriodIndex];

            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                float reserveDbhInCm = treeVolume.GetMaximumMerchantableDbh(species);
                this.TreeSelectionBySpecies.Add(species, new IndividualTreeSelection(treesOfSpecies, reserveDbhInCm)
                {
                    Count = treesOfSpecies.Count
                });

                this.ForwardedThinVolumeBySpecies.Add(species, new TreeSpeciesMerchantableVolume(species, maximumPlanningPeriodIndex));
                this.LongLogThinVolumeBySpecies.Add(species, new TreeSpeciesMerchantableVolume(species, maximumPlanningPeriodIndex));
                this.LongLogRegenerationVolumeBySpecies.Add(species, new TreeSpeciesMerchantableVolume(species, maximumPlanningPeriodIndex));
            }
        }

        protected StandTrajectory(StandTrajectory<TStand, TStandDensity, TTreatments> other)
            : base(other)
        {
            this.standByPeriod = new TStand[other.PlanningPeriods];
            this.treatments = (TTreatments)other.Treatments.Clone();

            this.DensityByPeriod = new TStandDensity[other.PlanningPeriods];

            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                TStand? otherStand = other.standByPeriod[periodIndex];
                if (otherStand != null)
                {
                    this.standByPeriod[periodIndex] = (TStand)otherStand.Clone();
                }
            }

            // base clones this.StandingVolumeBySpecies and ThinningVolumeBySpecies
        }

        public override TStand?[] StandByPeriod 
        { 
            get { return this.standByPeriod; }
        }

        public override TTreatments Treatments
        { 
            get { return this.treatments; }
        }

        public override int GetFirstThinPeriod()
        {
            int earliestThinningPeriod = Int32.MaxValue;
            for (int harvestIndex = 0; harvestIndex < this.Treatments.Harvests.Count; ++harvestIndex)
            {
                Harvest harvest = this.Treatments.Harvests[harvestIndex];
                if (harvest.Period < earliestThinningPeriod)
                {
                    earliestThinningPeriod = harvest.Period;
                }
            }

            if (earliestThinningPeriod != Int32.MaxValue)
            {
                return earliestThinningPeriod;
            }

            return Constant.NoHarvestPeriod;
        }

        public override TStandDensity GetStandDensity(int periodIndex)
        {
            return this.DensityByPeriod[periodIndex] ?? throw new InvalidOperationException("Stand density is null for period " + periodIndex + ". Has the stand trajectory been simulated?");
        }

        protected override int GetThinPeriod(int thinIndex)
        {
            List<int> thinningPeriods = new(this.Treatments.Harvests.Count);
            for (int harvestIndex = 0; harvestIndex < this.Treatments.Harvests.Count; ++harvestIndex)
            {
                Harvest harvest = this.Treatments.Harvests[harvestIndex];
                Debug.Assert(thinningPeriods.Contains(harvest.Period) == false);
                thinningPeriods.Add(harvest.Period);
            }
            if (thinningPeriods.Count <= thinIndex)
            {
                return Constant.NoHarvestPeriod;
            }

            thinningPeriods.Sort();
            return thinningPeriods[thinIndex];
        }

        // TODO: how to add a merchantable volume object on ingrowth of a new species?
        public override void RecalculateRegenerationHarvestMerchantableVolumeIfNeeded(int periodIndex)
        {
            // standing volume/long log harvest volume
            TStand stand = this.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information is not available for period " + periodIndex + ".");
            for (int merchantableSpeciesIndex = 0; merchantableSpeciesIndex < this.LongLogRegenerationVolumeBySpecies.Count; ++merchantableSpeciesIndex)
            {
                TreeSpeciesMerchantableVolume longLogVolumeForSpecies = this.LongLogRegenerationVolumeBySpecies.Values[merchantableSpeciesIndex];
                if (longLogVolumeForSpecies.IsCalculated(periodIndex) == false)
                {
                    longLogVolumeForSpecies.CalculateMerchantableStandingVolume(stand, periodIndex, this.TreeScaling);
                }
            }
        }

        public override void RecalculateThinningMerchantableVolumeIfNeeded(int periodIndex)
        {
            // forwarded volume if a thin is scheduled in this period
            bool periodHasHarvest = false;
            foreach (Harvest harvest in this.Treatments.Harvests)
            {
                if (harvest.Period == periodIndex)
                {
                    periodHasHarvest = true;
                }
            }

            if (periodHasHarvest)
            {
                // trees' expansion factors are set to zero when harvested so use trees' volume at end of the previous period
                TStand previousStand = this.StandByPeriod[periodIndex - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (periodIndex - 1) + ".");
                for (int treeSpeciesIndex = 0; treeSpeciesIndex < this.ForwardedThinVolumeBySpecies.Count; ++treeSpeciesIndex)
                {
                    TreeSpeciesMerchantableVolume forwardedThinVolumeForSpecies = this.ForwardedThinVolumeBySpecies.Values[treeSpeciesIndex];
                    if (forwardedThinVolumeForSpecies.IsCalculated(periodIndex) == false)
                    {
                        forwardedThinVolumeForSpecies.CalculateMerchantableThinningVolume(previousStand, isCutToLength: true, this.TreeSelectionBySpecies, periodIndex, this.TreeScaling);
                    }
                }
                for (int treeSpeciesIndex = 0; treeSpeciesIndex < this.LongLogThinVolumeBySpecies.Count; ++treeSpeciesIndex)
                {
                    TreeSpeciesMerchantableVolume longLogThinVolumeForSpecies = this.LongLogThinVolumeBySpecies.Values[treeSpeciesIndex];
                    if (longLogThinVolumeForSpecies.IsCalculated(periodIndex) == false)
                    {
                        longLogThinVolumeForSpecies.CalculateMerchantableThinningVolume(previousStand, isCutToLength: false, this.TreeSelectionBySpecies, periodIndex, this.TreeScaling);
                    }
                }

                // check for OrganonTreatments.BasalAreaThinnedByPeriod currently disabled
                // this check can fire spuriously in thinning from below when multiple trees too small to have merchantable volume are removed
                // could make more specific by checking if harvest removes at least one tree.
                //Debug.Assert((this.Treatments.BasalAreaThinnedByPeriod[periodIndex] > Constant.Bucking.MinimumBasalArea4SawEnglish && this.GetTotalScribnerVolumeThinned(periodIndex) > 0.0F) ||
                //              this.GetTotalScribnerVolumeThinned(periodIndex) == 0.0F);
            }
            else
            {
                for (int treeSpeciesIndex = 0; treeSpeciesIndex < this.ForwardedThinVolumeBySpecies.Count; ++treeSpeciesIndex)
                {
                    TreeSpeciesMerchantableVolume forwardedThinVolumeForSpecies = this.ForwardedThinVolumeBySpecies.Values[treeSpeciesIndex];
                    forwardedThinVolumeForSpecies.ClearVolume(periodIndex);
                }
                for (int treeSpeciesIndex = 0; treeSpeciesIndex < this.LongLogThinVolumeBySpecies.Count; ++treeSpeciesIndex)
                {
                    TreeSpeciesMerchantableVolume longLogThinVolumeForSpecies = this.LongLogThinVolumeBySpecies.Values[treeSpeciesIndex];
                    longLogThinVolumeForSpecies.ClearVolume(periodIndex);
                }
            }
        }
    }
}
