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
        private int minimumNeighborDistance;
        private int minimumNeighborIndex;
        private readonly int[] nearestNeighborDistance;
        private readonly int[] nearestNeighborIndex;
        private float reservedPopulationProportion;

        public int Count { get; private set; }
        public int HarvestPeriods { get; private set; }
        public float[] IndividualFitness { get; private init; }
        public int[][] IndividualTreeSelections { get; private init; }
        public int NewIndividuals { get; set; }
        public int TreeCount { get; private set; }

        public Population(int populationSize, int harvestPeriods, float reservedPopulationProportion, int treeCount)
        {
            this.individualIndexByFitness = new SortedDictionary<float, List<int>>();
            this.matingDistributionFunction = new float[populationSize];
            this.minimumNeighborDistance = Int32.MaxValue;
            this.minimumNeighborIndex = -1;
            this.nearestNeighborDistance = new int[populationSize];
            this.nearestNeighborIndex = new int[populationSize];
            this.reservedPopulationProportion = reservedPopulationProportion;

            this.HarvestPeriods = harvestPeriods;
            this.IndividualFitness = new float[populationSize];
            this.IndividualTreeSelections = new int[populationSize][];
            this.NewIndividuals = 0;
            this.TreeCount = treeCount;

            int treeCapacity = Trees.GetCapacity(treeCount);
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

        public void Add(float fitness, StandTrajectory trajectory)
        {
            trajectory.CopyTreeSelectionTo(this.IndividualTreeSelections[this.Count]);
            this.AssignFitness(this.Count, fitness);
        }

        private void AssignDistances()
        {
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                int nearestNeighborDistance = Int32.MaxValue;
                int nearestNeighborIndex = -1;
                for (int neighborIndex = 0; neighborIndex < this.Size; ++neighborIndex)
                {
                    if (individualIndex == neighborIndex)
                    {
                        continue;
                    }

                    int neighborDistance = this.GetDistance(this.IndividualTreeSelections[individualIndex], this.IndividualTreeSelections[neighborIndex]);
                    if (neighborDistance < nearestNeighborDistance)
                    {
                        nearestNeighborDistance = neighborDistance;
                        nearestNeighborIndex = neighborIndex;
                    }
                }

                this.nearestNeighborDistance[individualIndex] = nearestNeighborDistance;
                this.nearestNeighborIndex[individualIndex] = nearestNeighborIndex;

                if (nearestNeighborDistance < this.minimumNeighborDistance)
                {
                    this.minimumNeighborDistance = nearestNeighborDistance;
                    this.minimumNeighborIndex = nearestNeighborIndex;
                }
            }
        }

        public void AssignFitness(int individualIndex, float newFitness)
        {
            if (this.Count == this.Size)
            {
                throw new IndexOutOfRangeException();
            }

            this.IndividualFitness[individualIndex] = newFitness;
            if (this.individualIndexByFitness.TryGetValue(newFitness, out List<int>? probablyClones))
            {
                // initial population randomization happened to create two (or more) individuals with identical fitness
                // This is unlikely in large problems, but may not be uncommon in small test problems.
                probablyClones.Add(individualIndex);
            }
            else
            {
                this.individualIndexByFitness.Add(newFitness, new List<int>() { individualIndex });
            }

            ++this.Count;
            ++this.NewIndividuals;

            if (this.Count == this.Size)
            {
                this.AssignDistances();
            }
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

        private int GetDistance(int[] selection1, int[] selection2)
        {
            Debug.Assert(selection1.Length == selection2.Length);

            int distance = 0;
            for (int treeIndex = 0; treeIndex < this.TreeCount; ++treeIndex)
            {
                if (selection1[treeIndex] != selection2[treeIndex])
                {
                    ++distance;
                }
            }
            return distance;
        }

        private static List<int> GetTreeDiameterClasses(Stand standBeforeThinning, int diameterClasses)
        {
            if (diameterClasses < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(diameterClasses));
            }

            float maximumDbh = Single.MinValue;
            float minimumDbh = Single.MaxValue;
            int totalCapacity = 0;
            foreach (Trees treesOfSpecies in standBeforeThinning.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    maximumDbh = MathF.Max(maximumDbh, treesOfSpecies.Dbh[treeIndex]);
                    minimumDbh = MathF.Min(maximumDbh, treesOfSpecies.Dbh[treeIndex]);
                }
                totalCapacity += treesOfSpecies.Capacity;
            }

            List<int> diameterClassByTree = new List<int>(totalCapacity);
            float diameterClassWidth = (maximumDbh - minimumDbh) / diameterClasses;
            foreach (Trees treesOfSpecies in standBeforeThinning.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    int diameterClass = (int)((treesOfSpecies.Dbh[treeIndex] - minimumDbh) / diameterClassWidth);
                    if (diameterClass < 0)
                    {
                        diameterClass = 0; // limit numerical error
                    }
                    else if (diameterClass >= diameterClasses)
                    {
                        diameterClass = diameterClasses - 1; // limit numerical error
                    }
                    diameterClassByTree.Add(diameterClass);
                }
                for (int treeIndex = treesOfSpecies.Count; treeIndex < treesOfSpecies.Capacity; ++treeIndex)
                {
                    diameterClassByTree.Add(-1);
                }
            }

            return diameterClassByTree;
        }

        private static List<int> GetTreeDiameterQuantiles(Stand standBeforeThinning, int diameterQuantiles)
        {
            if (standBeforeThinning.TreesBySpecies.Count != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(standBeforeThinning));
            }

            Trees treesOfSpecies = standBeforeThinning.TreesBySpecies.Values.First();
            return Population.GetTreeQuantiles(treesOfSpecies, treesOfSpecies.GetDbhSortOrder, diameterQuantiles);
        }

        private static List<int> GetTreeHeightQuantiles(Stand standBeforeThinning, int diameterQuantiles)
        {
            if (standBeforeThinning.TreesBySpecies.Count != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(standBeforeThinning));
            }

            Trees treesOfSpecies = standBeforeThinning.TreesBySpecies.Values.First();
            return Population.GetTreeQuantiles(treesOfSpecies, treesOfSpecies.GetHeightSortOrder, diameterQuantiles);
        }

        private static List<int> GetTreeQuantiles(Trees treesOfSpecies, Func<int[]> getSortOrder, int quantiles)
        {
            if (quantiles < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantiles));
            }

            int[] treeSortOrder = getSortOrder.Invoke();
            List<int> quantileByTree = new List<int>(treesOfSpecies.Capacity);
            float quantileScalingFactor = (quantiles - Constant.RoundTowardsZeroTolerance) / treesOfSpecies.Count;
            for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
            {
                int quantile = (int)MathF.Floor(quantileScalingFactor * treeSortOrder[treeIndex]);
                Debug.Assert(quantile < quantiles);
                quantileByTree.Add(quantile);
            }
            for (int treeIndex = treesOfSpecies.Count; treeIndex < treesOfSpecies.Capacity; ++treeIndex)
            {
                quantileByTree.Add(-1);
            }
            return quantileByTree;
        }

        public void RandomizeSchedules(Stand? standBeforeThinning, PopulationParameters parameters, HarvestPeriodSelection periodSelection)
        {
            if (parameters.ProportionalPercentage != Constant.HeuristicDefault.ProportionalPercentage)
            {
                throw new NotImplementedException(nameof(parameters) + ".ProportionalPercentage is not currently supported.");
            }

            if (standBeforeThinning == null)
            {
                throw new ArgumentNullException(nameof(standBeforeThinning));
            }

            // set up selection probabilities
            // With multiple diameter classes:
            //   Since each diameter class runs from a selection probability of zero to one the total probability range to traverse when intializing individuals
            //   is (1 - 0) * diameterClasses. Individuals in a population of size p are therefore spaced diameterClasses / p apart in probability. A loop
            //   initializing p individuals takes p - 1 steps, traversing a probability distance of (p - 1) / p * diameterClasses, and leaving a separation of
            //   diameterClasses / p between the last individual initialized and the individual the loop started on.
            // With a single diameter class;
            //   A special case of multiple diameter classes. Therefore, calculation remain the same.
            List<int> selectionProbabilityIndexByTree = parameters.InitializationMethod switch
            {
                PopulationInitializationMethod.DiameterClass => Population.GetTreeDiameterClasses(standBeforeThinning, parameters.InitializationClasses),
                PopulationInitializationMethod.DiameterQuantile => Population.GetTreeDiameterQuantiles(standBeforeThinning, parameters.InitializationClasses),
                PopulationInitializationMethod.HeightQuantile => Population.GetTreeHeightQuantiles(standBeforeThinning, parameters.InitializationClasses),
                _ => throw new NotSupportedException("Unhandled population initialization method " + parameters.InitializationMethod + ".")
            };
            List<float> selectionProbabilityByIndex = new List<float>(parameters.InitializationClasses);
            float selectionProbabilityIncrement = (float)parameters.InitializationClasses / this.Size; 
            float initialSelectionProbability = 0.5F * selectionProbabilityIncrement;
            for (int selectionClass = 0; selectionClass < parameters.InitializationClasses; ++selectionClass)
            {
                selectionProbabilityByIndex.Add(initialSelectionProbability);
            }

            // select trees
            float probabilityScalingFactor = 1.0F / byte.MaxValue;
            if (periodSelection == HarvestPeriodSelection.All)
            {
                throw new NotImplementedException();
            }
            else if (periodSelection == HarvestPeriodSelection.ThinPeriodOrRetain)
            {
                for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
                {
                    // randomly select this individual's trees based on current diameter class selection probabilities
                    int[] schedule = this.IndividualTreeSelections[individualIndex];
                    Debug.Assert(selectionProbabilityIndexByTree.Count == schedule.Length);
                    for (int treeIndex = 0; treeIndex < schedule.Length; ++treeIndex)
                    {
                        int selectionIndex = selectionProbabilityIndexByTree[treeIndex];
                        if (selectionIndex >= 0) // diameter class of unused capacity is set to -1
                        {
                            float selectionProbability = selectionProbabilityByIndex[selectionIndex];
                            bool isSelected = (probabilityScalingFactor * this.GetPseudorandomByteAsFloat()) < selectionProbability;
                            schedule[treeIndex] = isSelected ? this.HarvestPeriods - 1 : 0;
                        }
                    }

                    // increment selection class probabilities
                    for (int selectionClass = 0; selectionClass < parameters.InitializationClasses; ++selectionClass)
                    {
                        float nextSelectionProbability = selectionProbabilityByIndex[selectionClass] + selectionProbabilityIncrement;
                        if (nextSelectionProbability < 1.0F)
                        {
                            // no rollover to next diameter class, so stop incrementing
                            selectionProbabilityByIndex[selectionClass] = nextSelectionProbability;
                            break;
                        }

                        // roll over this diameter class and continue on to increment the next largest diameter class
                        // Use roll over, rather than reset, to avoid creation of individuals with duplicate diameter class selection probabilities.
                        selectionProbabilityByIndex[selectionClass] = nextSelectionProbability - 1.0F;
                    }
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
                // for now, clamp negative objective functions to small positive ones to avoid total fitness of zero and NaNs
                float individualFitness = Math.Max(this.IndividualFitness[individualIndex], 0.0001F);
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

        private void Replace(float currentFitness, List<int> currentIndices, float newFitness, StandTrajectory trajectory, int nearestNeighborDistance, int nearestNeighborIndex)
        {
            Debug.Assert(currentIndices.Count > 0);
            int replacementIndex;
            if (currentIndices.Count == 1)
            {
                // reassign individual index to new fitness
                this.individualIndexByFitness.Remove(currentFitness);
                replacementIndex = currentIndices[0];

                if (this.individualIndexByFitness.TryGetValue(newFitness, out List<int>? existingIndices))
                {
                    // prepend individual to list to maximize population change since removal below operates from end of list
                    existingIndices.Insert(0, replacementIndex);
                }
                else
                {
                    // OK to reuse existing list due to ownership release
                    this.individualIndexByFitness.Add(newFitness, currentIndices);
                }
            }
            else
            {
                // remove presumed clone from existing list
                replacementIndex = currentIndices[^1];
                if (currentFitness != newFitness)
                {
                    currentIndices.RemoveAt(currentIndices.Count - 1);
                    if (this.individualIndexByFitness.TryGetValue(newFitness, out List<int>? existingList))
                    {
                        // could potentially no-op by undoing the previous RemoveAt(), but this is likely a rare case
                        existingList.Add(replacementIndex);
                    }
                    else
                    {
                        // mainline case
                        this.individualIndexByFitness.Add(newFitness, new List<int>() { replacementIndex });
                    }
                }
            }

            this.IndividualFitness[replacementIndex] = newFitness;
            trajectory.CopyTreeSelectionTo(this.IndividualTreeSelections[replacementIndex]);

            this.nearestNeighborDistance[replacementIndex] = nearestNeighborDistance;
            this.nearestNeighborIndex[replacementIndex] = nearestNeighborIndex;

            this.minimumNeighborDistance = Int32.MaxValue;
            this.minimumNeighborIndex = -1;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                int neighborDistance = this.nearestNeighborDistance[individualIndex];
                if (neighborDistance < this.minimumNeighborDistance)
                {
                    this.minimumNeighborDistance = neighborDistance;
                    this.minimumNeighborIndex = individualIndex;
                }
            }

            ++this.NewIndividuals;
        }

        public bool TryReplace(float newFitness, StandTrajectory trajectory, PopulationReplacementStrategy replacementStrategy)
        {
            return replacementStrategy switch
            {
                PopulationReplacementStrategy.ContributionOfDiversityReplaceWorst => this.TryReplaceByDiversityOrFitness(newFitness, trajectory),
                PopulationReplacementStrategy.ReplaceWorst => this.TryReplaceWorst(newFitness, trajectory),
                _ => throw new NotSupportedException(String.Format("Unhandled replacement strategy {0}.", replacementStrategy))
            };
        }

        public bool TryReplaceByDiversityOrFitness(float newFitness, StandTrajectory trajectory)
        {
            int[] candidateSelection = new int[Trees.GetCapacity(this.TreeCount)];
            trajectory.CopyTreeSelectionTo(candidateSelection);

            int nearestNeighborDistance = Int32.MaxValue;
            int nearestNeighborIndex = -1;
            KeyValuePair<float, List<int>> nearestNeighbor = default;
            foreach (KeyValuePair<float, List<int>> individual in this.individualIndexByFitness)
            {
                float individualFitness = individual.Key;
                if (individualFitness > newFitness)
                {
                    break;
                }
                for (int index = 0; index < individual.Value.Count; ++index)
                {
                    int individualIndex = individual.Value[index];

                    // for now, use Hamming distance as it's interchangeable with Euclidean distance for binary decision variables
                    // If needed, Euclidean distance can be used when multiple thinnings are allowed.
                    int neighborDistance = this.GetDistance(candidateSelection, this.IndividualTreeSelections[individualIndex]);
                    if (neighborDistance < nearestNeighborDistance)
                    {
                        nearestNeighbor = individual;
                        nearestNeighborDistance = neighborDistance;
                        nearestNeighborIndex = individualIndex;
                    }
                }
            }

            if ((nearestNeighborIndex >= 0) && (nearestNeighborDistance > this.minimumNeighborDistance))
            {
                // the check against minimum neighbor distance prevents introduction of more than a pair of clones through this path
                // The remove worst path could potentially create higher numbers of clones, though this is unlikely.
                this.Replace(nearestNeighbor.Key, nearestNeighbor.Value, newFitness, trajectory, nearestNeighborDistance, nearestNeighborIndex);
                return true;
            }

            return this.TryReplaceWorst(newFitness, trajectory);
        }

        public bool TryReplaceWorst(float newFitness, StandTrajectory trajectory)
        {
            float minimumFitness = this.individualIndexByFitness.Keys.First();
            if (minimumFitness > newFitness)
            {
                return false;
            }

            List<int> replacementIndices = this.individualIndexByFitness[minimumFitness];
            this.Replace(minimumFitness, replacementIndices, newFitness, trajectory, -1, -1);
            return true;
        }
    }
}
