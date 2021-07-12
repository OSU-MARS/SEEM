using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm
{
    public class StandTrajectory
    {
        public float[] BasalAreaRemoved { get; private init; }

        // harvest periods by tree, 0 indicates no harvest
        public SortedList<FiaCode, TreeSelection> IndividualTreeSelectionBySpecies { get; private init; }

        public int EarliestPeriodChangedSinceLastSimulation { get; protected set; }
        public string? Name { get; set; }
        public int PeriodLengthInYears { get; set; }
        public int PeriodZeroAgeInYears { get; set; }
        public float PlantingDensityInTreesPerHectare { get; private init; } // trees per hectare

        public SortedList<FiaCode, TreeSpeciesMerchantableVolume> StandingVolumeBySpecies { get; private init; }
        public SortedList<FiaCode, TreeSpeciesMerchantableVolume> ThinningVolumeBySpecies { get; private init; }
        public TreeVolume TreeVolume { get; set; }

        public StandTrajectory(TreeVolume treeVolume, int lastPlanningPeriod, float plantingDensityInTreesPerHectare)
        {
            if (lastPlanningPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lastPlanningPeriod));
            }
            if ((plantingDensityInTreesPerHectare <= 0.0F) || (plantingDensityInTreesPerHectare > Constant.Maximum.PlantingDensityInTreesPerHectare))
            {
                throw new ArgumentOutOfRangeException(nameof(plantingDensityInTreesPerHectare));
            }

            int maximumPlanningPeriodIndex = lastPlanningPeriod + 1;
            this.BasalAreaRemoved = new float[maximumPlanningPeriodIndex];
            this.EarliestPeriodChangedSinceLastSimulation = 0;
            this.IndividualTreeSelectionBySpecies = new();
            this.Name = null;
            this.PeriodLengthInYears = -1;
            this.PeriodZeroAgeInYears = -1;
            this.PlantingDensityInTreesPerHectare = plantingDensityInTreesPerHectare;
            this.StandingVolumeBySpecies = new();
            this.ThinningVolumeBySpecies = new();
            this.TreeVolume = treeVolume;
        }

        public StandTrajectory(StandTrajectory other)
        {
            this.BasalAreaRemoved = new float[other.BasalAreaRemoved.Length];
            this.EarliestPeriodChangedSinceLastSimulation = other.EarliestPeriodChangedSinceLastSimulation;
            this.TreeVolume = other.TreeVolume; // runtime immutable, assumed thread safe for shallow copy
            this.IndividualTreeSelectionBySpecies = new();
            this.Name = other.Name;
            this.PeriodLengthInYears = other.PeriodLengthInYears;
            this.PeriodZeroAgeInYears = other.PeriodZeroAgeInYears;
            this.PlantingDensityInTreesPerHectare = other.PlantingDensityInTreesPerHectare;
            this.StandingVolumeBySpecies = new();
            this.ThinningVolumeBySpecies = new();

            Array.Copy(other.BasalAreaRemoved, 0, this.BasalAreaRemoved, 0, other.BasalAreaRemoved.Length);

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

        public int PlanningPeriods
        {
            get { return this.BasalAreaRemoved.Length; }
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

        public int GetFirstThinAge()
        {
            int firstHarvestPeriod = this.GetFirstThinPeriod();
            if (firstHarvestPeriod == Constant.NoThinPeriod)
            {
                return -1;
            }
            return this.GetStartOfPeriodAge(firstHarvestPeriod);
        }

        public int GetFirstThinPeriod()
        {
            int earliestThinningPeriod = this.PlanningPeriods;
            foreach (TreeSpeciesMerchantableVolume thinningVolumeForSpecies in this.ThinningVolumeBySpecies.Values)
            {
                for (int periodIndex = 1; periodIndex < earliestThinningPeriod; ++periodIndex)
                {
                    if (thinningVolumeForSpecies.GetScribnerTotal(periodIndex) > 0.0F)
                    {
                        earliestThinningPeriod = periodIndex;
                        break;
                    }
                }
            }

            if (earliestThinningPeriod != this.PlanningPeriods)
            {
                return earliestThinningPeriod;
            }

            return Constant.NoThinPeriod;
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

        public int GetSecondThinAge()
        {
            return this.GetThinAge(2);
        }

        public int GetSecondThinPeriod()
        {
            return this.GetThinPeriod(2);
        }

        public int GetStartOfPeriodAge(int periodIndex)
        {
            Debug.Assert((periodIndex >= 0) && (periodIndex < this.PlanningPeriods));
            Debug.Assert((this.PeriodZeroAgeInYears >= 0) && (this.PeriodLengthInYears > 0));
            return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * (periodIndex - 1);
        }

        private int GetThinAge(int thinning)
        {
            int harvestPeriod = this.GetThinPeriod(thinning);
            if (harvestPeriod == Constant.NoThinPeriod)
            {
                return Constant.NoThinPeriod;
            }
            return this.GetStartOfPeriodAge(harvestPeriod);
        }

        private int GetThinPeriod(int thinning)
        {
            int earliestPeriodForThin = this.PlanningPeriods;
            foreach (TreeSpeciesMerchantableVolume thinningVolumeForSpecies in this.ThinningVolumeBySpecies.Values)
            {
                int thinsFound = 0;
                for (int periodIndex = 1; periodIndex < earliestPeriodForThin; ++periodIndex)
                {
                    if (thinningVolumeForSpecies.GetScribnerTotal(periodIndex) > 0.0F)
                    {
                        ++thinsFound;
                        if (thinsFound == thinning)
                        {
                            earliestPeriodForThin = periodIndex;
                            break;
                        }
                    }
                }
            }

            if (earliestPeriodForThin != this.PlanningPeriods)
            {
                return earliestPeriodForThin;
            }

            return Constant.NoThinPeriod;
        }

        public int GetThirdThinAge()
        {
            return this.GetThinAge(3);
        }

        public int GetThirdThinPeriod()
        {
            return this.GetThinPeriod(3);
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
            foreach (KeyValuePair<FiaCode, TreeSelection> selectionForSpecies in this.IndividualTreeSelectionBySpecies)
            {
                TreeSelection individualTreeSelection = selectionForSpecies.Value;
                if (treeIndex < individualTreeSelection.Count)
                {
                    return individualTreeSelection[treeIndex];
                }
                treeIndex -= individualTreeSelection.Count;
            }

            throw new ArgumentOutOfRangeException(nameof(allSpeciesUncompactedTreeIndex));
        }

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
            // see remarks in loop
            Debug.Assert(Constant.NoHarvestPeriod == 0);

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
}
