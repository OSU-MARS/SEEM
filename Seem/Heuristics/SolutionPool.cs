using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class SolutionPool : PseudorandomizingTask
    {
        protected const int UnknownDistance = Int32.MaxValue;
        protected const int UnknownNeighbor = -1;

        protected int[,] DistanceMatrix { get; private init; }
        protected int MinimumNeighborDistance { get; set; }
        protected int MinimumNeighborIndex { get; set; }
        protected int[] NearestNeighborIndex { get; private init; }

        public int SolutionsAccepted { get; set; } // public setter to allow reset between generations
        public int SolutionsInPool { get; protected set; }

        protected SolutionPool(int poolCapacity)
        {
            // can make distance matrix triangular since it's symmetiric but, for now, it's stored as a full matrix
            this.SolutionsInPool = 0;
            this.DistanceMatrix = new int[poolCapacity, poolCapacity];
            this.MinimumNeighborDistance = Int32.MaxValue;
            this.MinimumNeighborIndex = SolutionPool.UnknownNeighbor;
            this.NearestNeighborIndex = new int[poolCapacity];

            for (int index1 = 0; index1 < poolCapacity; ++index1)
            {
                this.DistanceMatrix[index1, index1] = 0;
                for (int index2 = index1 + 1; index2 < poolCapacity; ++index2)
                {
                    this.DistanceMatrix[index1, index2] = SolutionPool.UnknownDistance;
                    this.DistanceMatrix[index2, index1] = SolutionPool.UnknownDistance;
                }
            }
        }

        protected int PoolCapacity
        {
            get { return this.NearestNeighborIndex.Length; }
        }

        protected static int GetHammingDistance(SortedDictionary<FiaCode, int[]> selectionBySpecies1, SortedDictionary<FiaCode, int[]> selectionBySpecies2)
        {
            if (selectionBySpecies1.Count != selectionBySpecies2.Count)
            {
                throw new ArgumentException("Tree selections have different numbers of species.");
            }

            int hammingDistance = 0;
            foreach (KeyValuePair<FiaCode, int[]> selection1forSpecies in selectionBySpecies1)
            {
                int[] selection1 = selection1forSpecies.Value;
                int[] selection2 = selectionBySpecies2[selection1forSpecies.Key];
                if (selection1.Length != selection2.Length)
                {
                    throw new ArgumentException("Tree selections for " + selection1forSpecies.Key + " have different lengths.");
                }
                for (int treeIndex = 0; treeIndex < selection1.Length; ++treeIndex)
                {
                    if (selection1[treeIndex] != selection2[treeIndex])
                    {
                        ++hammingDistance;
                    }
                }
            }

            return hammingDistance;
        }

        protected static int GetHammingDistance(int[] selection1, int[] selection2)
        {
            if (selection1.Length != selection2.Length)
            {
                throw new ArgumentException("Tree selections are not of equal length.");
            }

            int hammingDistance = 0;
            for (int treeIndex = 0; treeIndex < selection1.Length; ++treeIndex)
            {
                if (selection1[treeIndex] != selection2[treeIndex])
                {
                    ++hammingDistance;
                }
            }
            return hammingDistance;
        }

        protected void UpdateNearestNeighborDistances(int newSolutionIndex, int[] neighborDistances, int nearestNeighborIndex)
        {
            Debug.Assert((nearestNeighborIndex >= SolutionPool.UnknownNeighbor) && (nearestNeighborIndex < this.PoolCapacity));

            int minimumNeighborDistance = SolutionPool.UnknownDistance;
            int minimumNeighborIndex = SolutionPool.UnknownNeighbor;
            for (int neighborIndex = 0; neighborIndex < this.SolutionsInPool; ++neighborIndex)
            {
                if (neighborIndex == newSolutionIndex)
                {
                    // equivalent of call to this.UpdateNearestNeighborIndex() below
                    // Could be hoisted but placed here for readablity.
                    this.NearestNeighborIndex[newSolutionIndex] = nearestNeighborIndex;
                    continue;
                }

                // update the distance matrix and the new solution's nearest neighbor
                if (nearestNeighborIndex != SolutionPool.UnknownNeighbor)
                {
                    int distanceToNeighbor = neighborDistances[neighborIndex];
                    Debug.Assert(distanceToNeighbor > 0);
                    this.DistanceMatrix[newSolutionIndex, nearestNeighborIndex] = distanceToNeighbor;
                    this.DistanceMatrix[nearestNeighborIndex, newSolutionIndex] = distanceToNeighbor;
                }

                // update minimum distance if this neighbor had the replaced individual as its nearest neighbor
                int nearestNeighborOfIndividual = this.NearestNeighborIndex[neighborIndex];
                if (nearestNeighborOfIndividual == newSolutionIndex)
                {
                    this.UpdateNearestNeighborIndex(neighborIndex);
                }
            }

            // if needed, update least diverse solution
            if (minimumNeighborDistance < this.MinimumNeighborDistance)
            {
                Debug.Assert(minimumNeighborIndex != SolutionPool.UnknownNeighbor);

                this.MinimumNeighborDistance = minimumNeighborDistance;
                // could also be set to newSolutionIndex since both the new solution and its neighbor are equidistant
                // For now, default to pointing to the solution which has been in the pool for longer. Objective function information could
                // potentially be used to make a decision here.
                this.MinimumNeighborIndex = minimumNeighborIndex;
            }
        }

        private void UpdateNearestNeighborIndex(int solutionIndex)
        {
            int nearestLowerNeighborDistance = SolutionPool.UnknownDistance;
            int nearestLowerNeighborIndex = SolutionPool.UnknownNeighbor;
            for (int neighborIndex = 0; neighborIndex < this.PoolCapacity; ++neighborIndex)
            {
                if (neighborIndex == solutionIndex)
                {
                    continue; // prevent self (at distance 0) from becoming nearest neighbor
                }

                int neighborDistance = this.DistanceMatrix[solutionIndex, neighborIndex];
                if ((neighborDistance != SolutionPool.UnknownDistance) && (neighborDistance < nearestLowerNeighborDistance))
                {
                    nearestLowerNeighborDistance = neighborDistance;
                    nearestLowerNeighborIndex = neighborIndex;
                }
            }

            this.NearestNeighborIndex[solutionIndex] = nearestLowerNeighborIndex;
        }
    }
}
