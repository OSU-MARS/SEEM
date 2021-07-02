using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicSolutionPool : SolutionPool
    {
        private int lowestEliteIndex;
        private float lowestEliteFinancialValue;

        public Heuristic?[] EliteSolutions { get; private init; }
        public Heuristic? Low { get; private set; }
        public Heuristic? High { get; private set; }

        public HeuristicSolutionPool(int capacity)
            : base(capacity)
        {
            this.lowestEliteIndex = 0;
            this.lowestEliteFinancialValue = Single.MinValue;

            this.EliteSolutions = new Heuristic[capacity];
            this.Low = null;
            this.High = null;
        }

        public OrganonStandTrajectory GetEliteSolution(HeuristicResultPosition position)
        {
            if (this.SolutionsInPool < 1)
            {
                throw new InvalidOperationException();
            }
            if (this.SolutionsInPool == 1)
            {
                return this.EliteSolutions[0]!.GetBestTrajectoryWithDefaulting(position);
            }

            float solutionIndexScalingFactor = (this.SolutionsInPool - Constant.RoundTowardsZeroTolerance) / byte.MaxValue;
            int solutionIndex = (int)(solutionIndexScalingFactor * this.Pseudorandom.GetPseudorandomByteAsFloat());
            return this.EliteSolutions[solutionIndex]!.GetBestTrajectoryWithDefaulting(position);
        }

        private void Replace(int replacementIndex, Heuristic heuristic, HeuristicResultPosition position, int[] neighborDistances, int nearestNeighborIndex)
        {
            this.EliteSolutions[replacementIndex] = heuristic;
            this.UpdateNeighborDistances(replacementIndex, neighborDistances, nearestNeighborIndex);

            // update index and objective of lowest solution in pool
            this.lowestEliteFinancialValue = Single.MaxValue;
            for (int solutionIndex = 0; solutionIndex < this.EliteSolutions.Length; ++solutionIndex)
            {
                float solutionOFinancialValue = this.EliteSolutions[solutionIndex]!.FinancialValue.GetHighestValueWithDefaulting(position);
                if (solutionOFinancialValue < this.lowestEliteFinancialValue)
                {
                    this.lowestEliteFinancialValue = solutionOFinancialValue;
                    this.lowestEliteIndex = solutionIndex;
                }
            }
        }

        public bool TryAddOrReplace(Heuristic heuristic, HeuristicResultPosition position)
        {
            if (this.SolutionsInPool == 0)
            {
                // pool is empty (first time TryAddOrReplace() is called)
                // Neigbor information requires at least a pair of solutions, so no distance matrix or neighbor index updates.
                this.EliteSolutions[0] = heuristic;
                this.lowestEliteIndex = 0;
                this.lowestEliteFinancialValue = heuristic.FinancialValue.GetHighestValueWithDefaulting(position);
                this.SolutionsAccepted = 1;
                this.SolutionsInPool = 1;

                this.Low = heuristic;
                this.High = heuristic;
                return true;
            }

            bool solutionAccepted;
            if (this.SolutionsInPool < this.PoolCapacity)
            {
                // calculate distances to solutions already in pool
                int[] distancesToSolutionsInPool = new int[this.SolutionsInPool + 1];
                SortedList<FiaCode, TreeSelection> heuristicTreeSelection = heuristic.GetBestTrajectoryWithDefaulting(position).IndividualTreeSelectionBySpecies;
                int nearestNeighborDistance = Int32.MaxValue;
                int nearestNeighborIndex = -1;
                for (int solutionIndex = 0; solutionIndex < this.SolutionsInPool; ++solutionIndex)
                {
                    SortedList<FiaCode, TreeSelection> eliteTreeSelection = this.EliteSolutions[solutionIndex]!.GetBestTrajectoryWithDefaulting(position).IndividualTreeSelectionBySpecies;
                    int distanceToSolution = SolutionPool.GetHammingDistance(heuristicTreeSelection, eliteTreeSelection);
                    if (distanceToSolution == 0)
                    {
                        // this solution is already in the pool and therefore doesn't need to be added
                        Debug.Assert((heuristic.FinancialValue.GetHighestValueWithDefaulting(position) >= this.Low!.FinancialValue.GetHighestValueWithDefaulting(position)) && 
                                     (heuristic.FinancialValue.GetHighestValueWithDefaulting(position) <= this.High!.FinancialValue.GetHighestValueWithDefaulting(position)));
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
                this.EliteSolutions[this.SolutionsInPool] = heuristic;
                float heuristicFinancialValue = heuristic.FinancialValue.GetHighestValueWithDefaulting(position);
                if (heuristicFinancialValue < this.lowestEliteFinancialValue)
                {
                    this.lowestEliteFinancialValue = heuristicFinancialValue;
                    this.lowestEliteIndex = this.SolutionsInPool;
                }

                this.UpdateNeighborDistances(this.SolutionsInPool, distancesToSolutionsInPool, nearestNeighborIndex);
                ++this.SolutionsInPool;
                solutionAccepted = true;
            }
            else
            {
                solutionAccepted = this.TryReplaceByDiversityOrObjective(heuristic, position);
            }

            if (solutionAccepted)
            {
                float heuristicFinancialValue = heuristic.FinancialValue.GetHighestValueWithDefaulting(position);
                if (heuristicFinancialValue > this.High!.FinancialValue.GetHighestValueWithDefaulting(position))
                {
                    this.High = heuristic;
                }
                else if (heuristicFinancialValue < this.Low!.FinancialValue.GetHighestValueWithDefaulting(position))
                {
                    this.Low = heuristic;
                }
                ++this.SolutionsAccepted;

                Debug.Assert(heuristicFinancialValue >= this.Low!.FinancialValue.GetHighestValueWithDefaulting(position));
            }
            else
            {
                ++this.SolutionsRejected;
            }
            Debug.Assert(heuristic.FinancialValue.GetHighestValueWithDefaulting(position) <= this.High!.FinancialValue.GetHighestValueWithDefaulting(position));

            return solutionAccepted;
        }

        private bool TryReplaceByDiversityOrObjective(Heuristic heuristic, HeuristicResultPosition position)
        {
            int[] distancesToSolutionsInPool = new int[this.SolutionsInPool];
            float heuristicFinancialValue = heuristic.FinancialValue.GetHighestValueWithDefaulting(position);
            SortedList<FiaCode, TreeSelection> heuristicTreeSelection = heuristic.GetBestTrajectoryWithDefaulting(position).IndividualTreeSelectionBySpecies;
            int nearestLowerNeighborDistance = SolutionPool.UnknownDistance;
            int nearestLowerNeighborIndex = SolutionPool.UnknownNeighbor;
            for (int solutionIndex = 0; solutionIndex < this.SolutionsInPool; ++solutionIndex)
            {
                Heuristic eliteSolution = this.EliteSolutions[solutionIndex]!;
                float eliteFinancialValue = eliteSolution.FinancialValue.GetHighestValueWithDefaulting(position);
                if (eliteFinancialValue > heuristicFinancialValue)
                {
                    // for now, treat distances to solutions with more preferable objective functions as irrelevant
                    distancesToSolutionsInPool[solutionIndex] = SolutionPool.UnknownDistance;
                    continue;
                }

                // for now, use Hamming distance as it's interchangeable with Euclidean distance for binary decision variables
                // If needed, Euclidean distance can be used when multiple thinnings are allowed.
                SortedList<FiaCode, TreeSelection> eliteTreeSelection = eliteSolution.GetBestTrajectoryWithDefaulting(position).IndividualTreeSelectionBySpecies;
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
                this.Replace(nearestLowerNeighborIndex, heuristic, position, distancesToSolutionsInPool, nearestLowerNeighborIndex);
                return true;
            }

            // replace worst if this is a new solution
            if ((heuristicFinancialValue > this.lowestEliteFinancialValue) && (nearestLowerNeighborDistance != 0))
            {
                this.Replace(this.lowestEliteIndex, heuristic, position, distancesToSolutionsInPool, nearestLowerNeighborIndex);
                return true;
            }

            return false;
        }
    }
}
