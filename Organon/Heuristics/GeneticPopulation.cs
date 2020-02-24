using System;
using System.Diagnostics;

namespace Osu.Cof.Organon.Heuristics
{
    internal class GeneticPopulation : RandomNumberConsumer
    {
        private readonly float[] matingDistributionFunction;
        private readonly float reservedPopulationProportion;

        public float[][] HarvestVolumesByPeriod { get; private set; }
        public float[] IndividualFitness { get; private set; }
        public int[][] IndividualTreeSelections { get; private set; }

        public GeneticPopulation(int populationSize, int harvestUnits, int harvestPeriods, float reservedPopulationProportion)
        {
            this.matingDistributionFunction = new float[populationSize];
            this.HarvestVolumesByPeriod = new float[populationSize][];
            this.IndividualFitness = new float[populationSize];
            this.IndividualTreeSelections = new int[populationSize][];
            this.reservedPopulationProportion = reservedPopulationProportion;

            double harvestPeriodScalingFactor = ((float)harvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            for (int individualIndex = 0; individualIndex < populationSize; ++individualIndex)
            {
                float[] harvestByPeriod = new float[harvestPeriods];
                this.HarvestVolumesByPeriod[individualIndex] = harvestByPeriod;

                int[] schedule = new int[harvestUnits];
                for (int unitIndex = 0; unitIndex < harvestUnits; ++unitIndex)
                {
                    schedule[unitIndex] = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                }
                this.IndividualTreeSelections[individualIndex] = schedule;
            }
        }

        public GeneticPopulation(GeneticPopulation other)
            : this(other.Size, other.HarvestUnits, other.HarvestPeriods, other.reservedPopulationProportion)
        {
            Array.Copy(other.matingDistributionFunction, 0, this.matingDistributionFunction, 0, this.Size);
            Array.Copy(other.IndividualFitness, 0, this.IndividualFitness, 0, this.Size);
            for (int individualIndex = 0; individualIndex < other.Size; ++individualIndex)
            {
                Array.Copy(other.HarvestVolumesByPeriod[individualIndex], 0, this.HarvestVolumesByPeriod[individualIndex], 0, this.HarvestPeriods);
                Array.Copy(other.IndividualTreeSelections[individualIndex], 0, this.IndividualTreeSelections[individualIndex], 0, this.HarvestUnits);
            }
        }

        public int HarvestPeriods
        {
            get { return this.HarvestVolumesByPeriod[0].Length; }
        }

        public int HarvestUnits
        {
            get { return this.IndividualTreeSelections[0].Length; }
        }

        public int Size
        {
            get { return this.IndividualFitness.Length; }
        }

        public void FindParents(out int firstParentIndex, out int secondParentIndex)
        {
            // find first parent
            // TODO: check significance of quantization effects from use of two random bytes
            double parentScalingFactor = 1.0 / (double)UInt16.MaxValue;
            double firstParentCumlativeProbability = parentScalingFactor * this.GetTwoPseudorandomBytesAsFloat();
            firstParentIndex = this.Size - 1;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                if (firstParentCumlativeProbability < matingDistributionFunction[individualIndex])
                {
                    firstParentIndex = individualIndex;
                    break;
                }
            }

            // find second parent
            // TODO: check significance of allowing selfing
            // TOOD: investigate selection pressure effect of choosing second parent randomly
            double secondParentCumlativeProbability = parentScalingFactor * this.GetTwoPseudorandomBytesAsFloat();
            secondParentIndex = this.Size - 1;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                if (secondParentCumlativeProbability < matingDistributionFunction[individualIndex])
                {
                    secondParentIndex = individualIndex;
                    break;
                }
            }
        }

        public void RecalculateMatingDistributionFunction()
        {
            // find cumulative distribution function (CDF) representing prevalence of individuals in population
            // The reserved proportion is allocated equally across all individuals and guarantees a minimum presence of low fitness individuals.
            float totalFitness = 0.0F;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                float individualFitness = this.IndividualFitness[individualIndex];
                totalFitness += individualFitness;
            }

            float guaranteedProportion = this.reservedPopulationProportion / this.Size;
            float fitnessProportion = 1.0F - this.reservedPopulationProportion;
            this.matingDistributionFunction[0] = guaranteedProportion + fitnessProportion * this.IndividualFitness[0] / totalFitness;
            for (int individualIndex = 1; individualIndex < this.Size; ++individualIndex)
            {
                this.matingDistributionFunction[individualIndex] = matingDistributionFunction[individualIndex - 1];
                this.matingDistributionFunction[individualIndex] += guaranteedProportion + fitnessProportion * this.IndividualFitness[individualIndex] / totalFitness;

                Debug.Assert(this.matingDistributionFunction[individualIndex] > this.matingDistributionFunction[individualIndex - 1]);
                Debug.Assert(this.matingDistributionFunction[individualIndex] <= 1.00001);
            }
        }
    }
}
