using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicSolutionPool : SolutionPool
    {
        private readonly Heuristic?[] eliteSolutions;
        private int lowestEliteIndex;
        private float lowestEliteObjectiveFunction;

        public Heuristic? Low { get; private set; }
        public Heuristic? High { get; private set; }

        public HeuristicSolutionPool(int capacity)
            : base(capacity)
        {
            this.eliteSolutions = new Heuristic[capacity];
            this.lowestEliteIndex = 0;
            this.lowestEliteObjectiveFunction = Single.MinValue;

            this.Low = null;
            this.High = null;
        }

        public OrganonStandTrajectory GetEliteSolution()
        {
            if (this.SolutionsInPool < 1)
            {
                throw new InvalidOperationException();
            }
            if (this.SolutionsInPool == 1)
            {
                return this.eliteSolutions[0]!.BestTrajectory;
            }

            float solutionIndexScalingFactor = (this.SolutionsInPool - Constant.RoundTowardsZeroTolerance) / byte.MaxValue;
            int solutionIndex = (int)(solutionIndexScalingFactor * this.Pseudorandom.GetPseudorandomByteAsFloat());
            return this.eliteSolutions[solutionIndex]!.BestTrajectory;
        }

        private void Replace(int replacementIndex, Heuristic heuristic, int discountRateIndex, int[] neighborDistances, int nearestNeighborIndex)
        {
            this.eliteSolutions[replacementIndex] = heuristic;
            this.UpdateNearestNeighborDistances(replacementIndex, neighborDistances, nearestNeighborIndex);

            // update index and objective of lowest solution in pool
            this.lowestEliteObjectiveFunction = Single.MaxValue;
            for (int index = 0; index < this.eliteSolutions.Length; ++index)
            {
                float solutionObjective = this.eliteSolutions[index]!.HighestFinancialValueByDiscountRate[discountRateIndex];
                if (solutionObjective < this.lowestEliteObjectiveFunction)
                {
                    this.lowestEliteObjectiveFunction = solutionObjective;
                    this.lowestEliteIndex = index;
                }
            }
        }

        public bool TryAddOrReplace(Heuristic heuristic, int discountRateIndex)
        {
            // pool is empty (first time TryAddOrReplace() is called)
            if (this.SolutionsInPool == 0)
            {
                this.eliteSolutions[0] = heuristic;
                this.lowestEliteIndex = 0;
                this.lowestEliteObjectiveFunction = heuristic.HighestFinancialValueByDiscountRate[discountRateIndex];
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
                int[] distancesToSolutionsInPool = new int[this.SolutionsInPool];
                int nearestNeighborDistance = Int32.MaxValue;
                int nearestNeighborIndex = -1;
                for (int solutionIndex = 0; solutionIndex < this.SolutionsInPool; ++solutionIndex)
                {
                    int distanceToSolution = SolutionPool.GetHammingDistance(heuristic.BestTrajectory.IndividualTreeSelectionBySpecies, this.eliteSolutions[solutionIndex]!.BestTrajectory.IndividualTreeSelectionBySpecies);
                    if (distanceToSolution == 0)
                    {
                        // this solution is already in the pool and therefore doesn't need to be added
                        Debug.Assert((heuristic.HighestFinancialValueByDiscountRate[discountRateIndex] >= this.Low!.HighestFinancialValueByDiscountRate[discountRateIndex]) && (heuristic.HighestFinancialValueByDiscountRate[discountRateIndex] <= this.High!.HighestFinancialValueByDiscountRate[discountRateIndex]));
                        return false;
                    }

                    distancesToSolutionsInPool[solutionIndex] = distanceToSolution;
                    if (distanceToSolution < nearestNeighborDistance)
                    {
                        nearestNeighborDistance = distanceToSolution;
                        nearestNeighborIndex = solutionIndex;
                    }
                }

                // add solution since pool is still filling
                this.eliteSolutions[this.SolutionsInPool] = heuristic;
                if (heuristic.HighestFinancialValueByDiscountRate[discountRateIndex] < this.lowestEliteObjectiveFunction)
                {
                    this.lowestEliteObjectiveFunction = heuristic.HighestFinancialValueByDiscountRate[discountRateIndex];
                    this.lowestEliteIndex = this.SolutionsInPool;
                }

                this.UpdateNearestNeighborDistances(this.SolutionsInPool, distancesToSolutionsInPool, nearestNeighborIndex);

                ++this.SolutionsInPool;
                solutionAccepted = true;
            }
            else
            {
                solutionAccepted = this.TryReplaceByDiversityOrObjective(heuristic, discountRateIndex);
            }

            if (solutionAccepted)
            {
                if (heuristic.HighestFinancialValueByDiscountRate[discountRateIndex] > this.High!.HighestFinancialValueByDiscountRate[discountRateIndex])
                {
                    this.High = heuristic;
                }
                else if (heuristic.HighestFinancialValueByDiscountRate[discountRateIndex] < this.Low!.HighestFinancialValueByDiscountRate[discountRateIndex])
                {
                    this.Low = heuristic;
                }
                ++this.SolutionsAccepted;

                Debug.Assert(heuristic.HighestFinancialValueByDiscountRate[discountRateIndex] >= this.Low!.HighestFinancialValueByDiscountRate[discountRateIndex]);
            }
            else
            {
                ++this.SolutionsRejected;
            }
            Debug.Assert(heuristic.HighestFinancialValueByDiscountRate[discountRateIndex] <= this.High!.HighestFinancialValueByDiscountRate[discountRateIndex]);

            return solutionAccepted;
        }

        private bool TryReplaceByDiversityOrObjective(Heuristic heuristic, int discountRateIndex)
        {
            int[] distancesToSolutionsInPool = new int[this.SolutionsInPool];
            int nearestLowerNeighborDistance = SolutionPool.UnknownDistance;
            int nearestLowerNeighborIndex = SolutionPool.UnknownNeighbor;
            for (int solutionIndex = 0; solutionIndex < this.SolutionsInPool; ++solutionIndex)
            {
                Heuristic eliteSolution = this.eliteSolutions[solutionIndex]!;
                float eliteObjective = eliteSolution.HighestFinancialValueByDiscountRate[discountRateIndex];
                if (eliteObjective > heuristic.HighestFinancialValueByDiscountRate[discountRateIndex])
                {
                    // for now, treat distances to solutions with more preferable objective functions as irrelevant
                    distancesToSolutionsInPool[solutionIndex] = SolutionPool.UnknownDistance;
                    continue;
                }

                // for now, use Hamming distance as it's interchangeable with Euclidean distance for binary decision variables
                // If needed, Euclidean distance can be used when multiple thinnings are allowed.
                int distanceToSolution = SolutionPool.GetHammingDistance(heuristic.BestTrajectory.IndividualTreeSelectionBySpecies, eliteSolution.BestTrajectory.IndividualTreeSelectionBySpecies);
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
                this.Replace(nearestLowerNeighborIndex, heuristic, discountRateIndex, distancesToSolutionsInPool, nearestLowerNeighborIndex);
                return true;
            }

            // replace worst if this is a new solution
            if ((heuristic.HighestFinancialValueByDiscountRate[discountRateIndex] > this.lowestEliteObjectiveFunction) && (nearestLowerNeighborDistance != 0))
            {
                this.Replace(this.lowestEliteIndex, heuristic, discountRateIndex, distancesToSolutionsInPool, nearestLowerNeighborIndex);
                return true;
            }

            return false;
        }
    }
}
