using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm
{
    public class StandTrajectory
    {
        public float[] BasalAreaRemoved { get; private set; }
        public float[] HarvestVolumesByPeriod { get; private set; }

        // harvest periods by tree, 0 indicates no harvest
        public SortedDictionary<FiaCode, int[]> IndividualTreeSelectionBySpecies { get; private set; }

        public string Name { get; set; }
        public int PeriodLengthInYears { get; set; }
        public int PeriodZeroAgeInYears { get; set; }

        public float[] StandingVolumeByPeriod { get; private set; }
        public bool TreeSelectionChangedSinceLastSimulation { get; protected set; }
        public VolumeUnits VolumeUnits { get; private set; }

        public StandTrajectory(int lastPlanningPeriod, int thinningPeriod, VolumeUnits volumeUnits)
        {
            if (lastPlanningPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lastPlanningPeriod));
            }
            if (lastPlanningPeriod < thinningPeriod)
            {
                throw new ArgumentOutOfRangeException();
            }

            int maximumPlanningPeriodIndex = lastPlanningPeriod + 1;
            this.BasalAreaRemoved = new float[thinningPeriod + 1];
            this.HarvestVolumesByPeriod = new float[thinningPeriod + 1];
            this.IndividualTreeSelectionBySpecies = new SortedDictionary<FiaCode, int[]>();
            this.Name = null;
            this.PeriodLengthInYears = -1;
            this.PeriodZeroAgeInYears = -1;
            this.StandingVolumeByPeriod = new float[maximumPlanningPeriodIndex];
            this.TreeSelectionChangedSinceLastSimulation = false;
            this.VolumeUnits = volumeUnits;
        }

        public StandTrajectory(StandTrajectory other)
        {
            this.BasalAreaRemoved = new float[other.HarvestPeriods];
            this.HarvestVolumesByPeriod = new float[other.HarvestPeriods];
            this.IndividualTreeSelectionBySpecies = new SortedDictionary<FiaCode, int[]>();
            this.Name = other.Name;
            this.PeriodLengthInYears = other.PeriodLengthInYears;
            this.PeriodZeroAgeInYears = other.PeriodZeroAgeInYears;
            this.StandingVolumeByPeriod = new float[other.PlanningPeriods];
            this.TreeSelectionChangedSinceLastSimulation = other.TreeSelectionChangedSinceLastSimulation;
            this.VolumeUnits = other.VolumeUnits;

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
        }

        public int HarvestPeriods
        {
            get { return this.HarvestVolumesByPeriod.Length; }
        }

        public int PlanningPeriods
        {
            get { return this.StandingVolumeByPeriod.Length; }
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

        public int GetFirstHarvestAge()
        {
            if ((this.PeriodZeroAgeInYears < 0) || (this.PeriodLengthInYears < 0))
            {
                throw new NotSupportedException();
            }

            for (int periodIndex = 1; periodIndex < this.HarvestVolumesByPeriod.Length; ++periodIndex)
            {
                if (this.HarvestVolumesByPeriod[periodIndex] > 0.0F)
                {
                    return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * (periodIndex - 1);
                }
            }
            return -1;
        }

        public int GetFirstHarvestPeriod()
        {
            for (int periodIndex = 1; periodIndex < this.HarvestVolumesByPeriod.Length; ++periodIndex)
            {
                if (this.HarvestVolumesByPeriod[periodIndex] > 0.0F)
                {
                    return periodIndex;
                }
            }
            return -1;
        }

        public int GetRotationLength()
        {
            if ((this.PeriodZeroAgeInYears < 0) || (this.PeriodLengthInYears < 0))
            {
                throw new NotSupportedException();
            }
            return this.PeriodZeroAgeInYears + this.PeriodLengthInYears * (this.PlanningPeriods - 1);
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

            throw new ArgumentOutOfRangeException(nameof(allSpeciesTreeIndex));
        }

        public void SetTreeSelection(FiaCode species, int treeIndex, int harvestPeriod)
        {
            if ((harvestPeriod < 0) || (harvestPeriod >= this.HarvestPeriods))
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }

            int currentPeriod = this.IndividualTreeSelectionBySpecies[species][treeIndex];
            this.IndividualTreeSelectionBySpecies[species][treeIndex] = harvestPeriod;
            if (currentPeriod != harvestPeriod)
            {
                this.TreeSelectionChangedSinceLastSimulation = true;
            }
        }
    }
}
