using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class SolutionPool : PseudorandomizingTask
    {
        public const int UnknownDistance = Int32.MaxValue;
        public const int UnknownNeighbor = -1;

        protected int MinimumNeighborDistance { get; set; }
        protected int MinimumNeighborIndex { get; set; }

        public int[,] DistanceMatrix { get; private init; }
        public int[] NearestNeighborIndex { get; private init; }
        public int SolutionsAccepted { get; set; } // public setters to allow reset between generations
        public int SolutionsRejected { get; set; }
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

                this.NearestNeighborIndex[index1] = SolutionPool.UnknownNeighbor;
            }
        }

        public bool IsFull
        {
            get { return this.SolutionsInPool == this.PoolCapacity; }
        }

        public int PoolCapacity
        {
            get { return this.NearestNeighborIndex.Length; }
        }

        protected static int GetHammingDistance(SortedList<FiaCode, TreeSelection> selectionBySpecies1, SortedList<FiaCode, TreeSelection> selectionBySpecies2)
        {
            if (selectionBySpecies1.Count != selectionBySpecies2.Count)
            {
                throw new ArgumentException("Tree selections have different numbers of species.");
            }

            int hammingDistance = 0;
            foreach (KeyValuePair<FiaCode, TreeSelection> selection1forSpecies in selectionBySpecies1)
            {
                TreeSelection selection1 = selection1forSpecies.Value;
                TreeSelection selection2 = selectionBySpecies2[selection1forSpecies.Key];
                if (selection1.Count != selection2.Count)
                {
                    throw new ArgumentException("Tree selections for " + selection1forSpecies.Key + " have different tree counts.");
                }
                for (int treeIndex = 0; treeIndex < selection1.Count; ++treeIndex)
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

        private int GetNearestNeighborIndex(int solutionIndex)
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

            Debug.Assert(solutionIndex != nearestLowerNeighborIndex);
            return nearestLowerNeighborIndex;
        }

        protected void UpdateNeighborDistances(int newSolutionIndex, int[] neighborDistances, int nearestNeighborIndex)
        {
            Debug.Assert((nearestNeighborIndex >= SolutionPool.UnknownNeighbor) && 
                         (nearestNeighborIndex < this.SolutionsInPool) &&
                         ((nearestNeighborIndex == SolutionPool.UnknownNeighbor) || (neighborDistances[nearestNeighborIndex] != SolutionPool.UnknownDistance)) &&
                         ((neighborDistances.Length == this.SolutionsInPool) || (neighborDistances.Length == this.SolutionsInPool + 1))); // insert or append new solution

            // update distance matrix and neighbors based on information in neighborDistances
            int minimumNeighborDistance = SolutionPool.UnknownDistance;
            int minimumNeighborIndex = SolutionPool.UnknownNeighbor;
            for (int solutionIndex = 0; solutionIndex < neighborDistances.Length; ++solutionIndex)
            {
                int distanceToNeighbor = neighborDistances[solutionIndex];
                Debug.Assert(distanceToNeighbor > 0);

                if (solutionIndex == newSolutionIndex)
                {
                    if (newSolutionIndex == nearestNeighborIndex)
                    {
                        // if solution is replacing its nearest neighbor, try to find the next nearest neighbor
                        int minimumDistance = SolutionPool.UnknownDistance;
                        int minimumIndex = SolutionPool.UnknownNeighbor;
                        for (int neighborIndex = 0; neighborIndex < this.SolutionsInPool; ++neighborIndex)
                        {
                            if (newSolutionIndex == neighborIndex)
                            {
                                continue;
                            }

                            int neighborDistance = neighborDistances[neighborIndex];
                            if (neighborDistance < minimumDistance)
                            {
                                minimumIndex = neighborIndex;
                            }
                        }

                        this.NearestNeighborIndex[newSolutionIndex] = minimumIndex; // may be unknown
                    }
                    else
                    {
                        // on diagonal of distance matrix so no distance matrix or minimum distance updates needed
                        this.NearestNeighborIndex[newSolutionIndex] = nearestNeighborIndex;
                    }

                    // no check on minimumNeighborDistance as distanceToNeighbor is set to the distance between the new solution and
                    // the solution it's replacing
                }
                else
                {
                    // update the distance matrix and set the new solution as the nearest neighbor where needed
                    // There are three cases here:
                    //   1) The new solution is being added and, therefore, newSolutionIndex != nearestNeighborIndex.
                    //   2) The new solution is being inserted but not replacing the least diverse solution in the pool.
                    //      newSolutionIndex != nearestNeighborIndex also holds.
                    //   3) The new solution is replacing the least diverse solution, so newSolutionIndex == nearestNeighborIndex.
                    Debug.Assert((distanceToNeighbor > 0) || (distanceToNeighbor == SolutionPool.UnknownDistance));
                    this.DistanceMatrix[solutionIndex, newSolutionIndex] = distanceToNeighbor; // may be unknown
                    this.DistanceMatrix[newSolutionIndex, solutionIndex] = distanceToNeighbor;

                    int nearestNeighborOfIndividual = this.NearestNeighborIndex[solutionIndex];
                    if (nearestNeighborOfIndividual == SolutionPool.UnknownNeighbor)
                    {
                        if (distanceToNeighbor != SolutionPool.UnknownDistance)
                        {
                            // neighbor doesn't have a known nearest neighbor and the distance between it and the new solution is known, so set its
                            // neighbor to the new solution
                            this.NearestNeighborIndex[solutionIndex] = newSolutionIndex;
                        }
                        // fall through: no action because distance isn't known
                    }
                    else
                    {
                        // update if neighbor's existing neighbor is more distant than the new solution
                        int nearestNeighborDistanceOfIndividual = this.DistanceMatrix[solutionIndex, nearestNeighborOfIndividual];
                        if (distanceToNeighbor < nearestNeighborDistanceOfIndividual)
                        {
                            this.NearestNeighborIndex[solutionIndex] = newSolutionIndex;
                        }
                    }

                    // update minimum distance if this neighbor had the replaced individual as its nearest neighbor
                    if (nearestNeighborOfIndividual == newSolutionIndex)
                    {
                        int newNearestNeighborIndex = this.GetNearestNeighborIndex(solutionIndex);
                        this.NearestNeighborIndex[solutionIndex] = newNearestNeighborIndex;

                        if (newNearestNeighborIndex != SolutionPool.UnknownNeighbor)
                        {
                            int newNearestNeighborDistance = this.DistanceMatrix[solutionIndex, newNearestNeighborIndex];
                            if (minimumNeighborDistance < newNearestNeighborDistance)
                            {
                                Debug.Assert(newNearestNeighborIndex != SolutionPool.UnknownNeighbor);

                                minimumNeighborDistance = newNearestNeighborDistance;
                                minimumNeighborIndex = newNearestNeighborIndex;
                            }
                        }
                    }

                    if (distanceToNeighbor < minimumNeighborDistance)
                    {
                        Debug.Assert(solutionIndex != SolutionPool.UnknownNeighbor);

                        minimumNeighborDistance = distanceToNeighbor;
                        minimumNeighborIndex = solutionIndex;
                    }
                }
            }

            // check reciprocal pairing
            Debug.Assert(((neighborDistances.Length == 1) && (this.NearestNeighborIndex[0] == SolutionPool.UnknownNeighbor)) ||
                         ((neighborDistances.Length == 2) && (((this.NearestNeighborIndex[0] == 0) && (this.NearestNeighborIndex[1] == 1)) ||
                                                              ((this.NearestNeighborIndex[0] == 1) && (this.NearestNeighborIndex[1] == 0)) ||
                                                              (this.NearestNeighborIndex[0] == this.NearestNeighborIndex[1]))) ||
                         (neighborDistances.Length > 2));

            // if needed, update least diverse solution
            // This happens when
            //   1) A solution being upserted is closer to its nearest neighbor than any of the solutions currently in the pool. This is
            //      common when pools are filling always happens when the second solution is added and a neighbor distance becomes possible.
            //   2) The solution being replaced is the least diverse solution in the pool. In this case the existing minimum distance
            //      information is no longer relevant and must be replaced.
            if ((minimumNeighborDistance < this.MinimumNeighborDistance) || (newSolutionIndex == this.MinimumNeighborIndex))
            {
                this.MinimumNeighborDistance = minimumNeighborDistance;
                // could also be set to newSolutionIndex since both the new solution and its neighbor are equidistant
                // For now, default to pointing to the solution which has been in the pool for longer. Objective function information could
                // potentially be used to make a decision here.
                this.MinimumNeighborIndex = minimumNeighborIndex;
            }
        }
    }
}
