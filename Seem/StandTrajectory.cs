using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class StandTrajectory
    {
        public float[] BasalAreaRemoved { get; private init; }
        public int HarvestPeriods { get; private init; }

        // harvest periods by tree, 0 indicates no harvest
        public SortedDictionary<FiaCode, int[]> IndividualTreeSelectionBySpecies { get; private init; }

        public string? Name { get; set; }
        public int PeriodLengthInYears { get; set; }
        public int PeriodZeroAgeInYears { get; set; }
        public float PlantingDensityInTreesPerHectare { get; private init; } // trees per hectare

        public StandVolume StandingVolume { get; private init; }
        public StandVolume ThinningVolume { get; private init; }
        public TimberValue TimberValue { get; set; }
        public bool TreeSelectionChangedSinceLastSimulation { get; protected set; }

        public StandTrajectory(TimberValue timberValue, int planningPeriods, int latestHarvestPeriod, float plantingDensityInTreesPerHectare)
        {
            if (planningPeriods < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(planningPeriods));
            }
            if (planningPeriods < latestHarvestPeriod)
            {
                throw new ArgumentOutOfRangeException(nameof(latestHarvestPeriod));
            }
            if ((plantingDensityInTreesPerHectare <= 0.0F) || (plantingDensityInTreesPerHectare > Constant.Maximum.PlantingDensityInTreesPerHectare))
            {
                throw new ArgumentOutOfRangeException(nameof(plantingDensityInTreesPerHectare));
            }

            int maximumPlanningPeriodIndex = planningPeriods + 1;
            this.BasalAreaRemoved = new float[planningPeriods + 1];
            this.HarvestPeriods = latestHarvestPeriod + 1;
            this.IndividualTreeSelectionBySpecies = new SortedDictionary<FiaCode, int[]>();
            this.Name = null;
            this.PeriodLengthInYears = -1;
            this.PeriodZeroAgeInYears = -1;
            this.PlantingDensityInTreesPerHectare = plantingDensityInTreesPerHectare;
            this.StandingVolume = new StandVolume(maximumPlanningPeriodIndex);
            this.ThinningVolume = new StandVolume(maximumPlanningPeriodIndex);
            this.TimberValue = timberValue;
            this.TreeSelectionChangedSinceLastSimulation = false;
        }

        public StandTrajectory(StandTrajectory other)
        {
            this.BasalAreaRemoved = new float[other.BasalAreaRemoved.Length];
            this.HarvestPeriods = other.HarvestPeriods;
            this.IndividualTreeSelectionBySpecies = new SortedDictionary<FiaCode, int[]>();
            this.Name = other.Name;
            this.PeriodLengthInYears = other.PeriodLengthInYears;
            this.PeriodZeroAgeInYears = other.PeriodZeroAgeInYears;
            this.PlantingDensityInTreesPerHectare = other.PlantingDensityInTreesPerHectare;
            this.StandingVolume = new StandVolume(other.StandingVolume);
            this.ThinningVolume = new StandVolume(other.ThinningVolume);
            this.TimberValue = other.TimberValue; // stateless, thread safe
            this.TreeSelectionChangedSinceLastSimulation = other.TreeSelectionChangedSinceLastSimulation;

            Array.Copy(other.BasalAreaRemoved, 0, this.BasalAreaRemoved, 0, this.HarvestPeriods);

            foreach (KeyValuePair<FiaCode, int[]> otherSelectionForSpecies in other.IndividualTreeSelectionBySpecies)
            {
                int[] thisSelectionForSpecies = this.IndividualTreeSelectionBySpecies.GetOrAdd(otherSelectionForSpecies.Key, otherSelectionForSpecies.Value.Length);
                if (otherSelectionForSpecies.Value.Length != thisSelectionForSpecies.Length)
                {
                    throw new NotSupportedException("Lengths of individual tree selections do not match.");
                }
                Array.Copy(otherSelectionForSpecies.Value, 0, thisSelectionForSpecies, 0, thisSelectionForSpecies.Length);
            }
        }

        public int PlanningPeriods
        {
            get { return this.StandingVolume.NetPresentValue.Length; }
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

        protected int GetEndOfPeriodAge(int periodIndex)
        {
            Debug.Assert((periodIndex >= 0) && (periodIndex < this.PlanningPeriods));
            Debug.Assert((this.PeriodZeroAgeInYears >= 0) && (this.PeriodLengthInYears > 0));
            return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * periodIndex;
        }

        public int GetFirstHarvestAge()
        {
            int firstHarvestPeriod = this.GetFirstHarvestPeriod();
            if (firstHarvestPeriod == -1)
            {
                return -1;
            }
            return this.GetStartOfPeriodAge(firstHarvestPeriod);
        }

        public int GetFirstHarvestPeriod()
        {
            for (int periodIndex = 1; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                if (this.ThinningVolume.ScribnerTotal[periodIndex] > 0.0F)
                {
                    return periodIndex;
                }
            }
            return -1;
        }

        public int GetRotationLength()
        {
            return this.GetEndOfPeriodAge(this.PlanningPeriods - 1);
        }

        public int GetSecondHarvestAge()
        {
            int secondHarvestPeriod = this.GetSecondHarvestPeriod();
            if (secondHarvestPeriod == -1)
            {
                return -1;
            }
            return this.GetStartOfPeriodAge(secondHarvestPeriod);
        }


        public int GetSecondHarvestPeriod()
        {
            int harvestsFound = 0;
            for (int periodIndex = 1; periodIndex < this.PlanningPeriods; ++periodIndex)
            {
                if (this.ThinningVolume.ScribnerTotal[periodIndex] > 0.0F)
                {
                    ++harvestsFound;
                    if (harvestsFound == 2)
                    {
                        return periodIndex;
                    }
                }
            }
            return -1;
        }

        protected int GetStartOfPeriodAge(int periodIndex)
        {
            Debug.Assert(periodIndex > 0);
            Debug.Assert((this.PeriodZeroAgeInYears >= 0) && (this.PeriodLengthInYears > 0));
            return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * (periodIndex - 1);
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

        public void SetTreeSelection(int allSpeciesUncompactedTreeIndex, int harvestPeriod)
        {
            if ((harvestPeriod < 0) || (harvestPeriod >= this.HarvestPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }

            int treeIndex = allSpeciesUncompactedTreeIndex;
            foreach (KeyValuePair<FiaCode, int[]> individualTreeSelection in this.IndividualTreeSelectionBySpecies)
            {
                if (treeIndex < individualTreeSelection.Value.Length)
                {
                    int currentPeriod = individualTreeSelection.Value[treeIndex];
                    individualTreeSelection.Value[treeIndex] = harvestPeriod;
                    if (currentPeriod != harvestPeriod)
                    {
                        this.TreeSelectionChangedSinceLastSimulation = true;
                    }
                    return;
                }
                treeIndex -= individualTreeSelection.Value.Length;
            }

            throw new ArgumentOutOfRangeException(nameof(allSpeciesUncompactedTreeIndex));
        }

        public void SetTreeSelection(FiaCode species, int uncompactedTreeIndex, int harvestPeriod)
        {
            if ((harvestPeriod < 0) || (harvestPeriod >= this.HarvestPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }

            int currentPeriod = this.IndividualTreeSelectionBySpecies[species][uncompactedTreeIndex];
            this.IndividualTreeSelectionBySpecies[species][uncompactedTreeIndex] = harvestPeriod;
            if (currentPeriod != harvestPeriod)
            {
                this.TreeSelectionChangedSinceLastSimulation = true;
            }
        }
    }
}
