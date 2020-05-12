using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    internal class GeneticPopulation : RandomNumberConsumer
    {
        private readonly float[] matingDistributionFunction;
        private readonly float reservedPopulationProportion;

        public float[][] HarvestVolumesByPeriod { get; private set; }
        public float[] IndividualFitness { get; private set; }
        public int[][] IndividualTreeSelections { get; private set; }

        public GeneticPopulation(int populationSize, int harvestPeriods, float reservedPopulationProportion, int treeCapacity)
        {
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
            Array.Copy(other.matingDistributionFunction, 0, this.matingDistributionFunction, 0, this.Size);
            Array.Copy(other.IndividualFitness, 0, this.IndividualFitness, 0, this.Size);
            for (int individualIndex = 0; individualIndex < other.Size; ++individualIndex)
            {
                other.HarvestVolumesByPeriod[individualIndex].CopyToExact(this.HarvestVolumesByPeriod[individualIndex]);
                other.IndividualTreeSelections[individualIndex].CopyToExact(this.IndividualTreeSelections[individualIndex]);
            }
        }

        private int HarvestPeriods
        {
            get { return this.HarvestVolumesByPeriod[0].Length; }
        }

        public int Size
        {
            get { return this.IndividualFitness.Length; }
        }

        public void FindParents(out int firstParentIndex, out int secondParentIndex)
        {
            // find first parent
            // TODO: check significance of quantization effects from use of two random bytes
            float unityScaling = 1.0F / (float)UInt16.MaxValue;
            float firstParentCumlativeProbability = unityScaling * this.GetTwoPseudorandomBytesAsFloat();
            firstParentIndex = -1;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                if (firstParentCumlativeProbability < this.matingDistributionFunction[individualIndex])
                {
                    firstParentIndex = individualIndex;
                    break;
                }
            }

            // find second parent
            // TODO: check significance of allowing selfing
            // TOOD: investigate selection pressure effect of choosing second parent randomly
            float secondParentCumlativeProbability = unityScaling * this.GetTwoPseudorandomBytesAsFloat();
            secondParentIndex = -1;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                if (secondParentCumlativeProbability < this.matingDistributionFunction[individualIndex])
                {
                    secondParentIndex = individualIndex;
                    break;
                }
            }

            Debug.Assert(firstParentIndex != -1);
            Debug.Assert(secondParentIndex != -1);
        }

        public void RandomizeSchedule(HarvestPeriodSelection periodSelection, float centralSelectionProbability, float selectionProbabilityWidth)
        {
            if ((centralSelectionProbability < 0.0F) || (centralSelectionProbability > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(centralSelectionProbability));
            }
            if ((selectionProbabilityWidth < 0.0F) || (selectionProbabilityWidth > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(selectionProbabilityWidth));
            }

            float minSelectionProbability = centralSelectionProbability - 0.5F * selectionProbabilityWidth;
            float maxSelectionProbability = centralSelectionProbability + 0.5F * selectionProbabilityWidth;
            if ((minSelectionProbability < 0.0F) || (maxSelectionProbability > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(selectionProbabilityWidth));
            }

            float selectionProbability = minSelectionProbability;
            float selectionProbabilityIncrement = selectionProbabilityWidth / (this.Size - 1);
            float unityScalingFactor = 1.0F / byte.MaxValue;
            if (periodSelection == HarvestPeriodSelection.All)
            {
                for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
                {
                    float harvestPeriodScalingFactor = (this.HarvestPeriods - Constant.RoundTowardsZeroTolerance) / selectionProbability;
                    int[] schedule = this.IndividualTreeSelections[individualIndex];
                    for (int treeIndex = 0; treeIndex < schedule.Length; ++treeIndex)
                    {
                        float treeProbability = unityScalingFactor * this.GetPseudorandomByteAsFloat();
                        if (treeProbability < selectionProbability)
                        {
                            schedule[treeIndex] = (int)(harvestPeriodScalingFactor * treeProbability);
                        }
                        else
                        {
                            schedule[treeIndex] = 0;
                        }
                    }

                    Debug.Assert(selectionProbability <= 1.0F);
                    selectionProbability += selectionProbabilityIncrement;
                }
            }
            else if (periodSelection == HarvestPeriodSelection.NoneOrLast)
            {
                for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
                {
                    int[] schedule = this.IndividualTreeSelections[individualIndex];
                    for (int treeIndex = 0; treeIndex < schedule.Length; ++treeIndex)
                    {
                        bool isSelected = (unityScalingFactor * this.GetPseudorandomByteAsFloat()) < selectionProbability;
                        schedule[treeIndex] = isSelected ? this.HarvestPeriods - 1: 0;
                    }

                    Debug.Assert(selectionProbability <= 1.0F);
                    selectionProbability += selectionProbabilityIncrement;
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
    }
}
