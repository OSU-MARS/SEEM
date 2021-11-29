using Mars.Seem.Heuristics;
using Mars.Seem.Optimization;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class SilviculturalPrescriptionPool : SolutionPool
    {
        private int lowestEliteIndex;
        private float lowestEliteFinancialValue;

        public float[] EliteFinancialValues { get; private init; }
        public IndividualTreeSelectionBySpecies?[] EliteTreeSelections { get; private init; }
        public StandTrajectoryAndObjectives Low { get; private set; }
        public StandTrajectoryAndObjectives High { get; private set; }

        public SilviculturalPrescriptionPool(int capacity)
            : base(capacity)
        {
            this.lowestEliteIndex = 0;
            this.lowestEliteFinancialValue = Single.MinValue;

            this.EliteFinancialValues = new float[capacity];
            this.EliteTreeSelections = new IndividualTreeSelectionBySpecies[capacity];
            this.Low = new(Single.MaxValue);
            this.High = new(Single.MinValue);
        }

        private void Replace(int replacementIndex, StandTrajectory newlyEliteTrajectory, float newFinancialValue, int[] neighborDistances, int nearestNeighborIndex)
        {
            this.EliteFinancialValues[replacementIndex] = newFinancialValue;
            this.EliteTreeSelections[replacementIndex] = newlyEliteTrajectory.TreeSelectionBySpecies;
            this.UpdateNeighborDistances(replacementIndex, neighborDistances, nearestNeighborIndex);

            // update index and objective of lowest financial value solution in pool
            this.lowestEliteFinancialValue = Single.MaxValue;
            for (int treeSelectionIndex = 0; treeSelectionIndex < this.EliteTreeSelections.Length; ++treeSelectionIndex)
            {
                float solutionFinancialValue = this.EliteFinancialValues[treeSelectionIndex];
                if (solutionFinancialValue < this.lowestEliteFinancialValue)
                {
                    this.lowestEliteFinancialValue = solutionFinancialValue;
                    this.lowestEliteIndex = treeSelectionIndex;
                }
            }
        }

        public IndividualTreeSelectionBySpecies GetRandomEliteTreeSelection()
        {
            if (this.SolutionsInPool < 1)
            {
                throw new InvalidOperationException("Solution pool is empty.");
            }

            IndividualTreeSelectionBySpecies? eliteTreeSelection;
            if (this.SolutionsInPool == 1)
            {
                eliteTreeSelection = this.EliteTreeSelections[0];
            }
            else
            {
                // for now, choose an elite solution randomly
                // In cases where heuristics evaluate a trajectory across rotation lengths and financial scenarios it's assumed all
                // solutions in the pool have trajectories available for the same rotation lengths and, therefore, that there isn't
                // reason to prefer one solution over another due to it having a closer match in rotation lengths.
                float solutionIndexScalingFactor = (this.SolutionsInPool - Constant.RoundTowardsZeroTolerance) / byte.MaxValue;
                int solutionIndex = (int)(solutionIndexScalingFactor * this.Pseudorandom.GetPseudorandomByteAsFloat());
                eliteTreeSelection = this.EliteTreeSelections[solutionIndex];
            }
            if (eliteTreeSelection == null)
            {
                throw new InvalidOperationException("Elite solution in pool with " + this.SolutionsInPool + " solutions is unexpectedly null. Is this due to a reader-writer race condition?");
            }

            return eliteTreeSelection;
        }

        public bool TryAddOrReplace(StandTrajectory trajectory, float financialValue)
        {
            return this.TryAddOrReplace(trajectory, financialValue, (Heuristic?)null);
        }

        private bool TryAddOrReplace(StandTrajectory candidateTrajectory, float candidateFinancialValue, Heuristic? heuristic)
        {
            if (this.SolutionsInPool == 0)
            {
                // pool is empty (first time TryAddOrReplace() is called)
                // Neigbor information requires at least a pair of solutions, so no distance matrix or neighbor index updates.
                this.EliteFinancialValues[0] = candidateFinancialValue;
                this.EliteTreeSelections[0] = candidateTrajectory.TreeSelectionBySpecies;
                this.lowestEliteIndex = 0;
                this.lowestEliteFinancialValue = candidateFinancialValue;

                this.Low.FinancialValue = candidateFinancialValue;
                this.Low.Heuristic = heuristic;
                this.Low.Trajectory = candidateTrajectory;
                this.High.FinancialValue = candidateFinancialValue;
                this.High.Heuristic = heuristic;
                this.High.Trajectory = candidateTrajectory;
                this.SolutionsAccepted = 1;
                this.SolutionsInPool = 1; // set last to avoid reader race condition where SolutionsInPool allows access to an EliteSolutions[index] which is null because it hasn't been set yet
                return true;
            }

            bool solutionAccepted;
            if (this.SolutionsInPool < this.PoolCapacity)
            {
                // calculate distances to solutions already in pool
                int[] distancesToSolutionsInPool = new int[this.SolutionsInPool + 1];
                IndividualTreeSelectionBySpecies heuristicTreeSelection = candidateTrajectory.TreeSelectionBySpecies;
                int nearestNeighborDistance = Int32.MaxValue;
                int nearestNeighborIndex = -1;
                for (int solutionIndex = 0; solutionIndex < this.SolutionsInPool; ++solutionIndex)
                {
                    IndividualTreeSelectionBySpecies eliteTreeSelection = this.EliteTreeSelections[solutionIndex]!;
                    int distanceToSolution = SolutionPool.GetHammingDistance(heuristicTreeSelection, eliteTreeSelection);
                    if (distanceToSolution == 0)
                    {
                        // this solution is already in the pool and therefore doesn't need to be added
                        Debug.Assert((candidateFinancialValue >= this.Low.FinancialValue) && (candidateFinancialValue <= this.High.FinancialValue));
                        ++this.SolutionsRejected; // for now, count a duplicate solution as a reject
                        return false;
                    }

                    distancesToSolutionsInPool[solutionIndex] = distanceToSolution;
                    if (distanceToSolution < nearestNeighborDistance)
                    {
                        nearestNeighborDistance = distanceToSolution;
                        nearestNeighborIndex = solutionIndex;
                    }
                }
                // set distance for solution being added
                distancesToSolutionsInPool[^1] = nearestNeighborDistance;

                // add solution since pool is still filling
                this.EliteFinancialValues[this.SolutionsInPool] = candidateFinancialValue;
                this.EliteTreeSelections[this.SolutionsInPool] = candidateTrajectory.TreeSelectionBySpecies;
                if (candidateFinancialValue < this.lowestEliteFinancialValue)
                {
                    this.lowestEliteFinancialValue = candidateFinancialValue;
                    this.lowestEliteIndex = this.SolutionsInPool;
                }

                this.UpdateNeighborDistances(this.SolutionsInPool, distancesToSolutionsInPool, nearestNeighborIndex);
                ++this.SolutionsInPool; // increment last to avoid reader race condition
                solutionAccepted = true;
            }
            else
            {
                solutionAccepted = this.TryReplaceByDiversityOrObjective(candidateTrajectory, candidateFinancialValue);
            }

            if (solutionAccepted)
            {
                if (candidateFinancialValue > this.High.FinancialValue)
                {
                    this.High.FinancialValue = candidateFinancialValue;
                    this.High.Heuristic = heuristic;
                    this.High.Trajectory = candidateTrajectory;
                }
                else if (candidateFinancialValue < this.Low.FinancialValue)
                {
                    this.Low.FinancialValue = candidateFinancialValue;
                    this.Low.Heuristic = heuristic;
                    this.Low.Trajectory = candidateTrajectory;
                }
                ++this.SolutionsAccepted;

                Debug.Assert(candidateFinancialValue >= this.Low.FinancialValue);
            }
            else
            {
                ++this.SolutionsRejected;
            }
            Debug.Assert(candidateFinancialValue <= this.High.FinancialValue);

            return solutionAccepted;
        }

        private bool TryReplaceByDiversityOrObjective(StandTrajectory candidateTrajectory, float candidateFinancialValue)
        {
            int[] distancesToSolutionsInPool = new int[this.SolutionsInPool];
            IndividualTreeSelectionBySpecies heuristicTreeSelection = candidateTrajectory.TreeSelectionBySpecies;
            int nearestLowerNeighborDistance = SolutionPool.UnknownDistance;
            int nearestLowerNeighborIndex = SolutionPool.UnknownNeighbor;
            for (int solutionIndex = 0; solutionIndex < this.SolutionsInPool; ++solutionIndex)
            {
                float eliteFinancialValue = this.EliteFinancialValues[solutionIndex];
                if (eliteFinancialValue > candidateFinancialValue)
                {
                    // for now, treat distances to solutions with more preferable objective functions as irrelevant
                    distancesToSolutionsInPool[solutionIndex] = SolutionPool.UnknownDistance;
                    continue;
                }

                // for now, use Hamming distance as it's interchangeable with Euclidean distance for binary decision variables
                // If needed, Euclidean distance can be used when multiple thinnings are allowed.
                IndividualTreeSelectionBySpecies eliteTreeSelection = this.EliteTreeSelections[solutionIndex]!;
                int distanceToSolution = SolutionPool.GetHammingDistance(heuristicTreeSelection, eliteTreeSelection);
                if (distanceToSolution == 0)
                {
                    // solution is already in the pool
                    return false;
                }

                distancesToSolutionsInPool[solutionIndex] = distanceToSolution;
                if (distanceToSolution < nearestLowerNeighborDistance)
                {
                    nearestLowerNeighborDistance = distanceToSolution;
                    nearestLowerNeighborIndex = solutionIndex;
                }
            }

            // replace on contribution to diversity
            if ((nearestLowerNeighborIndex >= 0) && (nearestLowerNeighborDistance > this.MinimumNeighborDistance))
            {
                this.Replace(nearestLowerNeighborIndex, candidateTrajectory, candidateFinancialValue, distancesToSolutionsInPool, nearestLowerNeighborIndex);
                return true;
            }

            // replace worst if this is a new solution
            if ((candidateFinancialValue > this.lowestEliteFinancialValue) && (nearestLowerNeighborDistance != 0))
            {
                this.Replace(this.lowestEliteIndex, candidateTrajectory, candidateFinancialValue, distancesToSolutionsInPool, nearestLowerNeighborIndex);
                return true;
            }

            return false;
        }
    }
}
