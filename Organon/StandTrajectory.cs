using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class StandTrajectory
    {
        public float[] BasalAreaRemoved { get; private set; }
        public int HarvestPeriods { get; private set; }

        // harvest periods by tree, 0 indicates no harvest
        public SortedDictionary<FiaCode, int[]> IndividualTreeSelectionBySpecies { get; private set; }

        public string Name { get; set; }
        public int PeriodLengthInYears { get; set; }
        public int PeriodZeroAgeInYears { get; set; }

        public StandVolume StandingVolume { get; private set; }
        public StandVolume ThinningVolume { get; private set; }
        public TimberValue TimberValue { get; set; }
        public bool TreeSelectionChangedSinceLastSimulation { get; protected set; }

        public StandTrajectory(TimberValue timberValue, int planningPeriods, int thinningPeriod)
        {
            if (planningPeriods < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(planningPeriods));
            }
            if (planningPeriods < thinningPeriod)
            {
                throw new ArgumentOutOfRangeException();
            }

            int maximumPlanningPeriodIndex = planningPeriods + 1;
            this.BasalAreaRemoved = new float[planningPeriods + 1];
            this.HarvestPeriods = thinningPeriod + 1;
            this.IndividualTreeSelectionBySpecies = new SortedDictionary<FiaCode, int[]>();
            this.Name = null;
            this.PeriodLengthInYears = -1;
            this.PeriodZeroAgeInYears = -1;
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
                    throw new ArgumentOutOfRangeException(nameof(other.IndividualTreeSelectionBySpecies));
                }
                Array.Copy(otherSelectionForSpecies.Value, 0, thisSelectionForSpecies, 0, thisSelectionForSpecies.Length);
            }
        }

        public int PlanningPeriods
        {
            get { return this.StandingVolume.Cubic.Length; }
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
            for (int periodIndex = 1; periodIndex < this.ThinningVolume.Scribner.Length; ++periodIndex)
            {
                if (this.ThinningVolume.Scribner[periodIndex] > 0.0F)
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
