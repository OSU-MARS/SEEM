using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public class Population : PseudorandomizingTask
    {
        private readonly SortedDictionary<float, List<int>> individualIndexByFitness;
        private readonly float[] matingDistributionFunction;
        private float reservedPopulationProportion;

        public int HarvestPeriods { get; private set; }
        public float[] IndividualFitness { get; private set; }
        public int[][] IndividualTreeSelections { get; private set; }
        public int NewIndividuals { get; set; }
        public int TreeCount { get; private set; }

        public Population(int populationSize, int harvestPeriods, float reservedPopulationProportion, int treeCount)
        {
            this.individualIndexByFitness = new SortedDictionary<float, List<int>>();
            this.matingDistributionFunction = new float[populationSize];
            this.reservedPopulationProportion = reservedPopulationProportion;

            this.HarvestPeriods = harvestPeriods;
            this.IndividualFitness = new float[populationSize];
            this.IndividualTreeSelections = new int[populationSize][];
            this.NewIndividuals = 0;
            this.TreeCount = treeCount;

            int treeCapacity = Constant.Simd128x4.Width * (treeCount / Constant.Simd128x4.Width + 1);
            for (int individualIndex = 0; individualIndex < populationSize; ++individualIndex)
            {
                this.IndividualTreeSelections[individualIndex] = new int[treeCapacity];
            }
        }

        public Population(Population other)
            : this(other.Size, other.HarvestPeriods, other.reservedPopulationProportion, other.TreeCount)
        {
            this.CopyFrom(other);
        }

        public int Size
        {
            get { return this.IndividualFitness.Length; }
        }

        public void CopyFrom(Population other)
        {
            Debug.Assert(Object.ReferenceEquals(this, other) == false);
            if (this.Size != other.Size)
            {
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            this.HarvestPeriods = other.HarvestPeriods;
            this.individualIndexByFitness.Clear();
            foreach (KeyValuePair<float, List<int>> individualOfFitness in other.individualIndexByFitness)
            {
                this.individualIndexByFitness.Add(individualOfFitness.Key, new List<int>(individualOfFitness.Value));
            }
            this.NewIndividuals = other.NewIndividuals;
            this.reservedPopulationProportion = other.reservedPopulationProportion;
            this.TreeCount = other.TreeCount;

            Array.Copy(other.matingDistributionFunction, 0, this.matingDistributionFunction, 0, this.Size);
            Array.Copy(other.IndividualFitness, 0, this.IndividualFitness, 0, this.Size);
            for (int individualIndex = 0; individualIndex < other.Size; ++individualIndex)
            {
                other.IndividualTreeSelections[individualIndex].CopyToExact(this.IndividualTreeSelections[individualIndex]);
            }
        }

        public void CrossoverKPoint(int points, int firstParentIndex, int secondParentIndex, int[] sortOrder, StandTrajectory firstChildTrajectory, StandTrajectory secondChildTrajectory)
        {
            // get parents' schedules
            int[] firstParentHarvestSchedule = this.IndividualTreeSelections[firstParentIndex];
            int[] secondParentHarvestSchedule = this.IndividualTreeSelections[secondParentIndex];
            int treeRecordCount = sortOrder.Length; // sortOrder is of tree record count, parent schedules are of capacity length
            Debug.Assert(firstParentHarvestSchedule.Length == secondParentHarvestSchedule.Length);

            // find length and position of crossover
            float treeScalingFactor = ((float)treeRecordCount - Constant.RoundTowardsZeroTolerance) / (float)UInt16.MaxValue;
            int[] crossoverPoints = new int[points];
            for (int pointIndex = 0; pointIndex < points; ++pointIndex)
            {
                crossoverPoints[pointIndex] = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
            }
            Array.Sort(crossoverPoints);

            bool isCrossed = false; // initial value is unimportant as designation of first vs second child is arbitrary
            int crossoverIndex = 0;
            int crossoverPosition = crossoverPoints[crossoverIndex];
            for (int sortIndex = 0; sortIndex < treeRecordCount; ++sortIndex)
            {
                if (crossoverPosition == sortIndex)
                {
                    isCrossed = !isCrossed;
                    crossoverPosition = crossoverPoints[crossoverIndex];
                    ++crossoverIndex;
                }

                int treeIndex = sortOrder[sortIndex];
                if (isCrossed)
                {
                    firstChildTrajectory.SetTreeSelection(treeIndex, secondParentHarvestSchedule[treeIndex]);
                    secondChildTrajectory.SetTreeSelection(treeIndex, firstParentHarvestSchedule[treeIndex]);
                }
                else
                {
                    firstChildTrajectory.SetTreeSelection(treeIndex, firstParentHarvestSchedule[treeIndex]);
                    secondChildTrajectory.SetTreeSelection(treeIndex, secondParentHarvestSchedule[treeIndex]);
                }
            }
        }

        public void CrossoverUniform(int firstParentIndex, int secondParentIndex, float changeProbability, StandTrajectory firstChildTrajectory, StandTrajectory secondChildTrajectory)
        {
            // get parents' schedules
            int[] firstParentHarvestSchedule = this.IndividualTreeSelections[firstParentIndex];
            int[] secondParentHarvestSchedule = this.IndividualTreeSelections[secondParentIndex];
            Debug.Assert(firstParentHarvestSchedule.Length == secondParentHarvestSchedule.Length);

            for (int treeIndex = 0; treeIndex < firstParentHarvestSchedule.Length; ++treeIndex)
            {
                int firstHarvestPeriod = firstParentHarvestSchedule[treeIndex];
                int secondHarvestPeriod = secondParentHarvestSchedule[treeIndex];
                if (firstHarvestPeriod != secondHarvestPeriod)
                {
                    int harvestPeriodBuffer = firstHarvestPeriod;
                    if (this.GetPseudorandomByteAsProbability() < changeProbability)
                    {
                        firstHarvestPeriod = secondHarvestPeriod;
                    }
                    if (this.GetPseudorandomByteAsProbability() < changeProbability)
                    {
                        secondHarvestPeriod = harvestPeriodBuffer;
                    }
                }
                firstChildTrajectory.SetTreeSelection(treeIndex, firstHarvestPeriod);
                secondChildTrajectory.SetTreeSelection(treeIndex, secondHarvestPeriod);
            }
        }

        public void FindParents(out int firstParentIndex, out int secondParentIndex)
        {
            // find first parent
            // TODO: check significance of quantization effects from use of two random bytes
            float probabilityScaling = 1.0F / (float)UInt16.MaxValue;
            float firstParentCumlativeProbability = probabilityScaling * this.GetTwoPseudorandomBytesAsFloat();
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
            float secondParentCumlativeProbability = probabilityScaling * this.GetTwoPseudorandomBytesAsFloat();
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
            ++this.NewIndividuals;
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

            this.IndividualFitness[replacementIndex] = fitness;
            trajectory.CopyTreeSelectionTo(this.IndividualTreeSelections[replacementIndex]);
            ++this.NewIndividuals;
            return true;
        }
    }
}
