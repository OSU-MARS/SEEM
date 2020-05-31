using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    internal class GeneticPopulation : PseudorandomizingTask
    {
        private readonly SortedDictionary<float, List<int>> individualIndexByFitness;
        private readonly float[] matingDistributionFunction;
        private float reservedPopulationProportion;

        public float[][] HarvestVolumesByPeriod { get; private set; }
        public float[] IndividualFitness { get; private set; }
        public int[][] IndividualTreeSelections { get; private set; }

        public GeneticPopulation(int populationSize, int harvestPeriods, float reservedPopulationProportion, int treeCapacity)
        {
            this.individualIndexByFitness = new SortedDictionary<float, List<int>>();
            this.matingDistributionFunction = new float[populationSize];
            this.HarvestVolumesByPeriod = new float[populationSize][];
            this.IndividualFitness = new float[populationSize];
            this.IndividualTreeSelections = new int[populationSize][];
            this.reservedPopulationProportion = reservedPopulationProportion;

            for (int individualIndex = 0; individualIndex < populationSize; ++individualIndex)
            {
                this.HarvestVolumesByPeriod[individualIndex] = new float[harvestPeriods];
                this.IndividualTreeSelections[individualIndex] = new int[treeCapacity];
            }
        }

        public GeneticPopulation(GeneticPopulation other)
            : this(other.Size, other.HarvestPeriods, other.reservedPopulationProportion, other.IndividualTreeSelections[0].Length)
        {
            this.CopyFrom(other);
        }

        private int HarvestPeriods
        {
            get { return this.HarvestVolumesByPeriod[0].Length; }
        }

        public int Size
        {
            get { return this.IndividualFitness.Length; }
        }

        public void CopyFrom(GeneticPopulation other)
        {
            Debug.Assert(Object.ReferenceEquals(this, other) == false);

            if ((this.HarvestPeriods != other.HarvestPeriods) || (this.Size != other.Size))
            {
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            this.individualIndexByFitness.Clear();
            foreach (KeyValuePair<float, List<int>> individualOfFitness in other.individualIndexByFitness)
            {
                this.individualIndexByFitness.Add(individualOfFitness.Key, new List<int>(individualOfFitness.Value));
            }
            this.reservedPopulationProportion = other.reservedPopulationProportion;

            Array.Copy(other.matingDistributionFunction, 0, this.matingDistributionFunction, 0, this.Size);
            Array.Copy(other.IndividualFitness, 0, this.IndividualFitness, 0, this.Size);
            for (int individualIndex = 0; individualIndex < other.Size; ++individualIndex)
            {
                other.HarvestVolumesByPeriod[individualIndex].CopyToExact(this.HarvestVolumesByPeriod[individualIndex]);
                other.IndividualTreeSelections[individualIndex].CopyToExact(this.IndividualTreeSelections[individualIndex]);
            }
        }

        public void FindParents(out int firstParentIndex, out int secondParentIndex)
        {
            // find first parent
            // TODO: check significance of quantization effects from use of two random bytes
            float unityScaling = 1.0F / (float)UInt16.MaxValue;
            float firstParentCumlativeProbability = unityScaling * this.GetTwoPseudorandomBytesAsFloat();
            firstParentIndex = this.Size - 1; // numerical precision may result in the last CDF value being slightly below 1
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                if (firstParentCumlativeProbability <= this.matingDistributionFunction[individualIndex])
                {
                    firstParentIndex = individualIndex;
                    break;
                }
            }

            // find second parent
            // TODO: check significance of allowing selfing
            // TOOD: investigate selection pressure effect of choosing second parent randomly
            float secondParentCumlativeProbability = unityScaling * this.GetTwoPseudorandomBytesAsFloat();
            secondParentIndex = this.Size - 1;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                if (secondParentCumlativeProbability <= this.matingDistributionFunction[individualIndex])
                {
                    secondParentIndex = individualIndex;
                    break;
                }
            }
        }

        public void RandomizeSchedule(HarvestPeriodSelection periodSelection, float proportionalPercentageCenter, float proportionalPercentageWidth)
        {
            if ((proportionalPercentageCenter < 0.0F) || (proportionalPercentageCenter > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(proportionalPercentageCenter));
            }
            if ((proportionalPercentageWidth < 0.0F) || (proportionalPercentageWidth > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(proportionalPercentageWidth));
            }

            float minSelectionPercentage = proportionalPercentageCenter - 0.5F * proportionalPercentageWidth;
            float maxSelectionPercentage = proportionalPercentageCenter + 0.5F * proportionalPercentageWidth;
            if ((minSelectionPercentage < 0.0F) || (maxSelectionPercentage > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(proportionalPercentageWidth));
            }

            float selectionPercentage = minSelectionPercentage;
            float selectionPercentageIncrement = proportionalPercentageWidth / (this.Size - 1);
            float percentageScalingFactor = 100.0F / byte.MaxValue;
            if (periodSelection == HarvestPeriodSelection.All)
            {
                for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
                {
                    float harvestPeriodScalingFactor = (this.HarvestPeriods - Constant.RoundTowardsZeroTolerance) / selectionPercentage;
                    int[] schedule = this.IndividualTreeSelections[individualIndex];
                    for (int treeIndex = 0; treeIndex < schedule.Length; ++treeIndex)
                    {
                        float treePercentage = percentageScalingFactor * this.GetPseudorandomByteAsFloat();
                        if (treePercentage < selectionPercentage)
                        {
                            schedule[treeIndex] = (int)(harvestPeriodScalingFactor * treePercentage);
                        }
                        else
                        {
                            schedule[treeIndex] = 0;
                        }
                    }

                    Debug.Assert(selectionPercentage <= 100.0F);
                    selectionPercentage += selectionPercentageIncrement;
                }
            }
            else if (periodSelection == HarvestPeriodSelection.NoneOrLast)
            {
                for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
                {
                    int[] schedule = this.IndividualTreeSelections[individualIndex];
                    for (int treeIndex = 0; treeIndex < schedule.Length; ++treeIndex)
                    {
                        bool isSelected = (percentageScalingFactor * this.GetPseudorandomByteAsFloat()) < selectionPercentage;
                        schedule[treeIndex] = isSelected ? this.HarvestPeriods - 1: 0;
                    }

                    Debug.Assert(selectionPercentage <= 100.0F);
                    selectionPercentage += selectionPercentageIncrement;
                }
            }
            else
            {
                throw new NotSupportedException(String.Format("Unhandled harvest period selection {0}.", periodSelection));
            }
        }

        public void RecalculateMatingDistributionFunction()
        {
            // find cumulative distribution function (CDF) representing prevalence of individuals in population
            // The reserved proportion is allocated equally across all individuals and guarantees a minimum presence of low fitness individuals.
            float totalFitness = 0.0F;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                // for now, clam negative objective functions to zero
                float individualFitness = Math.Max(this.IndividualFitness[individualIndex], 0.0F);
                totalFitness += individualFitness;
            }

            float guaranteedProportion = this.reservedPopulationProportion / this.Size;
            float fitnessProportion = 1.0F - this.reservedPopulationProportion;
            this.matingDistributionFunction[0] = guaranteedProportion + fitnessProportion * this.IndividualFitness[0] / totalFitness;
            for (int individualIndex = 1; individualIndex < this.Size; ++individualIndex)
            {
                float individualFitness = Math.Max(this.IndividualFitness[individualIndex], 0.0F);
                this.matingDistributionFunction[individualIndex] = matingDistributionFunction[individualIndex - 1];
                this.matingDistributionFunction[individualIndex] += guaranteedProportion + fitnessProportion * individualFitness / totalFitness;

                Debug.Assert(this.matingDistributionFunction[individualIndex] > this.matingDistributionFunction[individualIndex - 1]);
                Debug.Assert(this.matingDistributionFunction[individualIndex] <= 1.00001);
            }
        }

        public void SetFitness(float fitness, int individualIndex)
        {
            this.IndividualFitness[individualIndex] = fitness;
            if (this.individualIndexByFitness.TryGetValue(fitness, out List<int> probablyClones))
            {
                // initial population randomization happened to create two (or more) individuals with identical fitness
                // This is unlikely in large problems, but may not be uncommon in small test problems.
                probablyClones.Add(individualIndex);
            }
            else
            {
                this.individualIndexByFitness.Add(fitness, new List<int>() { individualIndex });
            }
        }

        public bool TryInsert(float fitness, OrganonStandTrajectory trajectory)
        {
            float minimumFitness = this.individualIndexByFitness.Keys.First();
            if (minimumFitness > fitness)
            {
                return false;
            }
            if (this.individualIndexByFitness.ContainsKey(fitness))
            {
                // TODO: tolerance for numerical precision
                // TODO: how to handle cases where differing genetic information produces the same fitness?
                // TODO: check for no change breeding and avoid no op stand simulations?
                return false;
            }

            List<int> replacementIndices = this.individualIndexByFitness[minimumFitness];
            int replacementIndex;
            Debug.Assert(replacementIndices.Count > 0);
            if (replacementIndices.Count == 1)
            {
                // reassign individual index to new fitness
                this.individualIndexByFitness.Remove(minimumFitness);
                this.individualIndexByFitness.Add(fitness, replacementIndices);
                replacementIndex = replacementIndices[0];
            }
            else
            {
                // remove presumed clone from existing list
                replacementIndex = replacementIndices[^1];
                replacementIndices.RemoveAt(replacementIndices.Count - 1);
                this.individualIndexByFitness.Add(fitness, new List<int>() { replacementIndex });
            }

            Array.Copy(trajectory.HarvestVolumesByPeriod, 0, this.HarvestVolumesByPeriod[replacementIndex], 0, trajectory.HarvestVolumesByPeriod.Length);
            this.IndividualFitness[replacementIndex] = fitness;
            trajectory.CopyTreeSelectionTo(this.IndividualTreeSelections[replacementIndex]);
            return true;
        }
    }
}
