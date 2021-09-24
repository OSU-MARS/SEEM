﻿using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Silviculture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Tree
{
    public abstract class StandTrajectory
    {
        // harvest periods by tree, Constant.NoHarvestPeriod indicates no harvest
        public SortedList<FiaCode, TreeSelection> IndividualTreeSelectionBySpecies { get; private init; }

        public int EarliestPeriodChangedSinceLastSimulation { get; protected set; }
        public string? Name { get; set; }
        public int PeriodLengthInYears { get; set; }
        public int PeriodZeroAgeInYears { get; set; }

        public int PlanningPeriods { get; private init; }
        public float PlantingDensityInTreesPerHectare { get; private init; } // trees per hectare

        public SortedList<FiaCode, TreeSpeciesMerchantableVolume> StandingVolumeBySpecies { get; private init; }
        public SortedList<FiaCode, TreeSpeciesMerchantableVolume> ThinningVolumeBySpecies { get; private init; }

        public TreeVolume TreeVolume { get; set; }

        protected StandTrajectory(TreeVolume treeVolume, int lastPlanningPeriod, float plantingDensityInTreesPerHectare)
        {
            if (lastPlanningPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lastPlanningPeriod));
            }
            if ((plantingDensityInTreesPerHectare <= 0.0F) || (plantingDensityInTreesPerHectare > Constant.Maximum.PlantingDensityInTreesPerHectare))
            {
                throw new ArgumentOutOfRangeException(nameof(plantingDensityInTreesPerHectare));
            }

            this.EarliestPeriodChangedSinceLastSimulation = 0;
            this.IndividualTreeSelectionBySpecies = new();
            this.Name = null;
            this.PeriodLengthInYears = -1;
            this.PeriodZeroAgeInYears = -1;
            this.PlantingDensityInTreesPerHectare = plantingDensityInTreesPerHectare;
            this.PlanningPeriods = lastPlanningPeriod + 1;
            this.StandingVolumeBySpecies = new();
            this.ThinningVolumeBySpecies = new();
            this.TreeVolume = treeVolume;
        }

        protected StandTrajectory(StandTrajectory other)
        {
            this.EarliestPeriodChangedSinceLastSimulation = other.EarliestPeriodChangedSinceLastSimulation;
            this.IndividualTreeSelectionBySpecies = new();
            this.Name = other.Name;
            this.PeriodLengthInYears = other.PeriodLengthInYears;
            this.PeriodZeroAgeInYears = other.PeriodZeroAgeInYears;
            this.PlantingDensityInTreesPerHectare = other.PlantingDensityInTreesPerHectare;
            this.PlanningPeriods = other.PlanningPeriods;
            this.StandingVolumeBySpecies = new();
            this.ThinningVolumeBySpecies = new();
            this.TreeVolume = other.TreeVolume; // runtime immutable, assumed thread safe for shallow copy

            foreach (KeyValuePair<FiaCode, TreeSelection> otherSelectionForSpecies in other.IndividualTreeSelectionBySpecies)
            {
                FiaCode treeSpecies = otherSelectionForSpecies.Key;
                TreeSelection thisSelectionForSpecies = new(otherSelectionForSpecies.Value);
                TreeSpeciesMerchantableVolume thisStandingVolume = new(other.StandingVolumeBySpecies[treeSpecies]);
                TreeSpeciesMerchantableVolume thisThinningVolume = new(other.ThinningVolumeBySpecies[treeSpecies]);

                this.IndividualTreeSelectionBySpecies.Add(treeSpecies, thisSelectionForSpecies);
                this.StandingVolumeBySpecies.Add(treeSpecies, thisStandingVolume);
                this.ThinningVolumeBySpecies.Add(treeSpecies, thisThinningVolume);
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
            foreach (TreeSelection treeSelectionForSpecies in this.IndividualTreeSelectionBySpecies.Values)
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

        public void CopyTreeSelectionFrom(SortedList<FiaCode, TreeSelection> otherTreeSelection)
        {
            if (this.IndividualTreeSelectionBySpecies.Count != otherTreeSelection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(otherTreeSelection), "Attempt to copy between mismatched tree selections. This stand trajectory has tree selections for " + this.IndividualTreeSelectionBySpecies.Count + " species. The tree selection provided has " + otherTreeSelection.Count + " species.");
            }

            bool atLeastOneTreeMoved = false;
            int earliestThinningPeriod = Int32.MaxValue;
            for (int speciesIndex = 0; speciesIndex < this.IndividualTreeSelectionBySpecies.Count; ++speciesIndex)
            {
                FiaCode otherSpecies = otherTreeSelection.Keys[speciesIndex];
                FiaCode thisSpecies = this.IndividualTreeSelectionBySpecies.Keys[speciesIndex];
                if (otherSpecies != thisSpecies)
                {
                    throw new ArgumentOutOfRangeException(nameof(otherTreeSelection), "Attempt to copy between mismatched tree selections. This stand trajectory has " + thisSpecies + " at index " + speciesIndex + " and the tree selection provided has " + otherSpecies + ".");
                }

                TreeSelection otherSelectionForSpecies = otherTreeSelection.Values[speciesIndex];
                TreeSelection thisSelectionForSpecies = this.IndividualTreeSelectionBySpecies.Values[speciesIndex];
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

                        if (otherSelection == Constant.NoHarvestPeriod)
                        {
                            earliestThinningPeriod = Math.Min(earliestThinningPeriod, thisSelection);
                        }
                        else
                        {
                            earliestThinningPeriod = Math.Min(earliestThinningPeriod, otherSelection);
                        }
                    }
                }
            }

            if (atLeastOneTreeMoved)
            {
                this.UpdateEariestPeriodChanged(this.EarliestPeriodChangedSinceLastSimulation, earliestThinningPeriod);
            }
        }

        public void CopyTreeSelectionTo(SortedList<FiaCode, TreeSelection> otherTreeSelection)
        {
            Debug.Assert(IDictionaryExtensions.KeysIdentical(this.IndividualTreeSelectionBySpecies, otherTreeSelection));
            foreach (KeyValuePair<FiaCode, TreeSelection> thisSelectionForSpecies in this.IndividualTreeSelectionBySpecies)
            {
                TreeSelection otherSelectionForSpecies = otherTreeSelection[thisSelectionForSpecies.Key];
                thisSelectionForSpecies.Value.CopyTo(otherSelectionForSpecies);
            }
        }

        public void DeselectAllTrees()
        {
            // see remarks in loop
            Debug.Assert(Constant.NoHarvestPeriod == 0);

            foreach (TreeSelection selectionForSpecies in this.IndividualTreeSelectionBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < selectionForSpecies.Count; ++treeIndex)
                {
                    int currentHarvestPeriod = selectionForSpecies[treeIndex];
                    if (currentHarvestPeriod != Constant.NoHarvestPeriod)
                    {
                        selectionForSpecies[treeIndex] = Constant.NoHarvestPeriod;
                        // if stand has been simulated then the earliest period affected by removing all thinning is the first thin performed
                        this.EarliestPeriodChangedSinceLastSimulation = Math.Min(this.EarliestPeriodChangedSinceLastSimulation, currentHarvestPeriod);
                    }
                }
            }
        }

        public int GetEndOfPeriodAge(int periodIndex)
        {
            Debug.Assert((periodIndex >= 0) && (periodIndex < this.PlanningPeriods));
            Debug.Assert((this.PeriodZeroAgeInYears >= 0) && (this.PeriodLengthInYears > 0));
            return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * periodIndex;
        }

        // this function's existence isn't entirely desirable as it reverses ConstructTreeSelection()'s internal logic
        // Not currently enough of an advantage for refactoring selection construction to return period indices to be worthwhile.
        public int[] GetHarvestPeriodIndices(IList<int> thinningPeriods)
        {
            int treeRecordCount = this.IndividualTreeSelectionBySpecies.Values.Sum(treeSelection => treeSelection.Count);

            int[] periodIndices = new int[treeRecordCount];
            int allSpeciesUncompactedTreeIndex = 0;
            foreach (TreeSelection individualTreeSelection in this.IndividualTreeSelectionBySpecies.Values)
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
            if (firstHarvestPeriod == Constant.NoThinPeriod)
            {
                return -1;
            }
            return this.GetStartOfPeriodAge(firstHarvestPeriod);
        }

        public abstract int GetFirstThinPeriod();

        public void GetMerchantableVolumes(out StandMerchantableVolume standingVolume, out StandMerchantableVolume thinnedVolume)
        {
            for (int periodIndex = 0; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                this.RecalculateStandingVolumeIfNeeded(periodIndex);
                this.RecalculateThinningVolumeIfNeeded(periodIndex);
            }

            thinnedVolume = new StandMerchantableVolume(this.ThinningVolumeBySpecies);
            standingVolume = new StandMerchantableVolume(this.StandingVolumeBySpecies);
        }

        public int GetSecondThinAge()
        {
            return this.GetThinAge(1);
        }

        public int GetSecondThinPeriod()
        {
            return this.GetThinPeriod(1);
        }

        public int GetStartOfPeriodAge(int periodIndex)
        {
            Debug.Assert((periodIndex >= 0) && (periodIndex < this.PlanningPeriods));
            Debug.Assert((this.PeriodZeroAgeInYears >= 0) && (this.PeriodLengthInYears > 0));
            return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * (periodIndex - 1);
        }

        private int GetThinAge(int thinIndex)
        {
            int harvestPeriod = this.GetThinPeriod(thinIndex);
            if (harvestPeriod == Constant.NoThinPeriod)
            {
                return Constant.NoThinPeriod;
            }
            return this.GetStartOfPeriodAge(harvestPeriod);
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
            foreach (TreeSpeciesMerchantableVolume thinVolumeForSpecies in this.ThinningVolumeBySpecies.Values)
            {
                totalVolume += thinVolumeForSpecies.GetCubicTotal(periodIndex);
            }
            return totalVolume;
        }

        public float GetTotalScribnerVolumeThinned(int periodIndex)
        {
            float totalVolume = 0.0F;
            foreach (TreeSpeciesMerchantableVolume thinVolumeForSpecies in this.ThinningVolumeBySpecies.Values)
            {
                totalVolume += thinVolumeForSpecies.GetScribnerTotal(periodIndex);
            }
            return totalVolume;
        }

        public float GetTotalStandingCubicVolume(int periodIndex)
        {
            float totalVolume = 0.0F;
            foreach (TreeSpeciesMerchantableVolume standingVolumeForSpecies in this.StandingVolumeBySpecies.Values)
            {
                totalVolume += standingVolumeForSpecies.GetCubicTotal(periodIndex);
            }
            return totalVolume;
        }

        public float GetTotalStandingScribnerVolume(int periodIndex)
        {
            float totalVolume = 0.0F;
            foreach (TreeSpeciesMerchantableVolume standingVolumeForSpecies in this.StandingVolumeBySpecies.Values)
            {
                totalVolume += standingVolumeForSpecies.GetScribnerTotal(periodIndex);
            }
            return totalVolume;
        }

        public int GetTreeSelection(int allSpeciesUncompactedTreeIndex)
        {
            int treeIndex = allSpeciesUncompactedTreeIndex;
            foreach (TreeSelection individualTreeSelection in this.IndividualTreeSelectionBySpecies.Values)
            {
                if (treeIndex < individualTreeSelection.Count)
                {
                    return individualTreeSelection[treeIndex];
                }
                treeIndex -= individualTreeSelection.Count;
            }

            throw new ArgumentOutOfRangeException(nameof(allSpeciesUncompactedTreeIndex));
        }

        public void InvalidateMerchantableVolumes(int periodIndex)
        {
            foreach (TreeSpeciesMerchantableVolume thinningVolumeForSpecies in this.ThinningVolumeBySpecies.Values)
            {
                thinningVolumeForSpecies.MarkUncalculated(periodIndex);
            }
            foreach (TreeSpeciesMerchantableVolume standingVolumeForSpecies in this.StandingVolumeBySpecies.Values)
            {
                standingVolumeForSpecies.MarkUncalculated(periodIndex);
            }
        }

        public abstract void RecalculateStandingVolumeIfNeeded(int periodIndex);
        public abstract void RecalculateThinningVolumeIfNeeded(int periodIndex);

        public void SetTreeSelection(int allSpeciesUncompactedTreeIndex, int newHarvestPeriod)
        {
            if ((newHarvestPeriod < 0) || (newHarvestPeriod >= this.PlanningPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(newHarvestPeriod));
            }

            int treeIndex = allSpeciesUncompactedTreeIndex;
            foreach (KeyValuePair<FiaCode, TreeSelection> selectionForSpecies in this.IndividualTreeSelectionBySpecies)
            {
                TreeSelection individualTreeSelection = selectionForSpecies.Value;
                if (treeIndex < individualTreeSelection.Count)
                {
                    int currentHarvestPeriod = individualTreeSelection[treeIndex];
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
            if ((newHarvestPeriod < Constant.NoHarvestPeriod) || (newHarvestPeriod >= this.PlanningPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(newHarvestPeriod));
            }

            int currentHarvestPeriod = this.IndividualTreeSelectionBySpecies[species][uncompactedTreeIndex];
            if (currentHarvestPeriod != newHarvestPeriod)
            {
                this.IndividualTreeSelectionBySpecies[species][uncompactedTreeIndex] = newHarvestPeriod;
                this.UpdateEariestPeriodChanged(currentHarvestPeriod, newHarvestPeriod);
            }
        }

        private void UpdateEariestPeriodChanged(int currentPeriod, int newPeriod)
        {
            Debug.Assert((currentPeriod >= 0) && (currentPeriod <= this.PlanningPeriods) && (newPeriod >= 0) && (newPeriod <= this.PlanningPeriods) && (Constant.NoHarvestPeriod == 0));

            // four cases
            //   1) tree is not scheduled for thinning and becomes scheduled -> earliest affected period is harvest period
            //   2) tree is scheduled for thinning and becomes unscheduled -> earliest affected period is harvest period
            //   3) tree is reassinged to an earlier harvest period -> earliest affected period is earliest harvest period
            //   4) tree is reassinged to a later harvest period -> earliest affected period is still the earliest harvest period
            int earliestAffectedPeriod;
            if (currentPeriod == Constant.NoHarvestPeriod)
            {
                earliestAffectedPeriod = newPeriod;
            }
            else if (newPeriod == Constant.NoHarvestPeriod)
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

    public abstract class StandTrajectory<TStand, TTreatments> : StandTrajectory 
        where TStand : Stand 
        where TTreatments : Treatments
    {
        private readonly TStand?[] standByPeriod;
        private readonly TTreatments treatments;

        protected StandTrajectory(TreeVolume treeVolume, int lastPlanningPeriod, float plantingDensityInTreesPerHectare, TTreatments treatments) :
            base(treeVolume, lastPlanningPeriod, plantingDensityInTreesPerHectare)
        {
            this.standByPeriod = new TStand[this.PlanningPeriods];
            this.treatments = treatments;
        }

        protected StandTrajectory(StandTrajectory<TStand, TTreatments> other)
            : base(other)
        {
            this.standByPeriod = new TStand[other.PlanningPeriods];
            this.treatments = (TTreatments)other.Treatments.Clone();
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
            foreach (Harvest harvest in this.Treatments.Harvests)
            {
                if (harvest.Period < earliestThinningPeriod)
                {
                    earliestThinningPeriod = harvest.Period;
                }
            }

            if (earliestThinningPeriod != Int32.MaxValue)
            {
                return earliestThinningPeriod;
            }

            return Constant.NoThinPeriod;
        }

        protected override int GetThinPeriod(int thinIndex)
        {
            List<int> thinningPeriods = new(this.Treatments.Harvests.Count);
            foreach (Harvest harvest in this.Treatments.Harvests)
            {
                Debug.Assert(thinningPeriods.Contains(harvest.Period) == false);
                thinningPeriods.Add(harvest.Period);
            }
            if (thinningPeriods.Count <= thinIndex)
            {
                return Constant.NoThinPeriod;
            }

            thinningPeriods.Sort();
            return thinningPeriods[thinIndex];
        }
    }
}