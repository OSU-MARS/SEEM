using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public class TabuSearch : SingleTreeHeuristic<TabuParameters>
    {
        public int EscapeAfter { get; set; }
        public int EscapeDistance { get; set; }
        public int Iterations { get; set; }
        public int MaximumTenure { get; set; }
        public TabuTenure Tenure { get; set; }

        public TabuSearch(OrganonStand stand, TabuParameters parameters, RunParameters runParameters)
            :  base(stand, parameters, runParameters)
        {
            this.EscapeAfter = parameters.EscapeAfter;
            this.EscapeDistance = parameters.EscapeDistance;
            this.Iterations = parameters.Iterations;
            //this.Jump = parameters.Jump;
            this.MaximumTenure = parameters.MaximumTenure;
            this.Tenure = parameters.Tenure;
        }

        //private List<List<int>> GetDiameterQuantiles()
        //{
        //    List<List<int>> treeIndicesByDiameterQuantile = new List<List<int>>();
        //    int initialTreeRecordCount = this.GetInitialTreeRecordCount();
        //    List<int> allTreeIndices = new List<int>(initialTreeRecordCount);
        //    for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
        //    {
        //        allTreeIndices.Add(treeIndex);
        //    }
        //    treeIndicesByDiameterQuantile.Add(allTreeIndices);

        //    if (this.Jump == 1)
        //    {
        //        return treeIndicesByDiameterQuantile;
        //    }

        //    int firstHarvestPeriod = this.CurrentTrajectory.GetFirstHarvestPeriod();
        //    Stand standAtFirstHarvest = this.CurrentTrajectory.StandByPeriod[firstHarvestPeriod];
        //    if (standAtFirstHarvest.TreesBySpecies.Count != 1)
        //    {
        //        throw new NotSupportedException();
        //    }
        //    Trees treesOfSpecies = standAtFirstHarvest.TreesBySpecies.Values.First();
        //    int[] dbhSortOrder = treesOfSpecies.GetDbhSortOrder();
        //    float quantileSize = 1.0F / this.Jump;
        //    float treeIncrement = 1.0F / treesOfSpecies.Count;
        //    float treeCumulativeDistribution = 0.0F;
        //    for (int dbhSortIndex = 0; dbhSortIndex < treesOfSpecies.Count; treeCumulativeDistribution += treeIncrement, ++dbhSortIndex)
        //    {
        //        // numerical rounding may result in CDF positions slightly greater than 1
        //        // Plus one to account for inclusion of all trees as the first element of the list.
        //        int quantileIndex = Math.Min((int)(treeCumulativeDistribution / quantileSize), this.Jump - 1) + 1;
        //        Debug.Assert(treeCumulativeDistribution < 1.001F);

        //        if (treeIndicesByDiameterQuantile.Count <= quantileIndex)
        //        {
        //            treeIndicesByDiameterQuantile.Add(new List<int>());
        //        }

        //        List<int> treeIndicesForQuantile = treeIndicesByDiameterQuantile[quantileIndex];
        //        treeIndicesForQuantile.Add(dbhSortOrder[dbhSortIndex]);
        //    }

        //    return treeIndicesByDiameterQuantile;
        //}

        //private List<List<int>> GetDiameterSubsets()
        //{
        //    List<List<int>> treeIndicesBySubset = new List<List<int>>();
        //    int initialTreeRecordCount = this.GetInitialTreeRecordCount();
        //    List<int> allTreeIndices = new List<int>(initialTreeRecordCount);
        //    for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
        //    {
        //        allTreeIndices.Add(treeIndex);
        //    }
        //    treeIndicesBySubset.Add(allTreeIndices);

        //    if (this.Jump == 1)
        //    {
        //        return treeIndicesBySubset;
        //    }

        //    int firstHarvestPeriod = this.CurrentTrajectory.GetFirstHarvestPeriod();
        //    Stand standAtFirstHarvest = this.CurrentTrajectory.StandByPeriod[firstHarvestPeriod];
        //    if (standAtFirstHarvest.TreesBySpecies.Count != 1)
        //    {
        //        throw new NotSupportedException();
        //    }
        //    Trees treesOfSpecies = standAtFirstHarvest.TreesBySpecies.Values.First();
        //    int[] dbhSortOrder = treesOfSpecies.GetDbhSortOrder();
        //    for (int subset = 0; subset < this.Jump; ++subset)
        //    {
        //        List<int> treeIndicesForSubset = new List<int>();
        //        treeIndicesBySubset.Add(treeIndicesForSubset);
        //        for (int dbhSortIndex = subset; dbhSortIndex < treesOfSpecies.Count; dbhSortIndex += this.Jump)
        //        {
        //            treeIndicesForSubset.Add(dbhSortOrder[dbhSortIndex]);
        //        }
        //    }

        //    return treeIndicesBySubset;
        //}

        //private List<List<int>> GetHeightSubsets()
        //{
        //    List<List<int>> treeIndicesBySubset = new List<List<int>>();
        //    int initialTreeRecordCount = this.GetInitialTreeRecordCount();
        //    List<int> allTreeIndices = new List<int>(initialTreeRecordCount);
        //    for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
        //    {
        //        allTreeIndices.Add(treeIndex);
        //    }
        //    treeIndicesBySubset.Add(allTreeIndices);

        //    if (this.Jump == 1)
        //    {
        //        return treeIndicesBySubset;
        //    }

        //    int firstHarvestPeriod = this.CurrentTrajectory.GetFirstHarvestPeriod();
        //    Stand standAtFirstHarvest = this.CurrentTrajectory.StandByPeriod[firstHarvestPeriod];
        //    if (standAtFirstHarvest.TreesBySpecies.Count != 1)
        //    {
        //        throw new NotSupportedException();
        //    }
        //    Trees treesOfSpecies = standAtFirstHarvest.TreesBySpecies.Values.First();
        //    int[] heightSortOrder = treesOfSpecies.GetHeightSortOrder();
        //    for (int subset = 0; subset < this.Jump; ++subset)
        //    {
        //        List<int> treeIndicesForSubset = new List<int>();
        //        treeIndicesBySubset.Add(treeIndicesForSubset);
        //        for (int dbhSortIndex = subset; dbhSortIndex < treesOfSpecies.Count; dbhSortIndex += this.Jump)
        //        {
        //            treeIndicesForSubset.Add(heightSortOrder[dbhSortIndex]);
        //        }
        //    }

        //    return treeIndicesBySubset;
        //}

        public override string GetName()
        {
            return "Tabu";
        }

        //private List<int> GetNextRandomSubset(int totalTrees /*, List<List<int>> subsets*/)
        //{
        //    List<int> subsetIndices = new List<int>();
        //    for (int subsetIndex = 0; subsetIndex < totalTrees / this.Jump; ++subsetIndex)
        //    {
        //        subsetIndices.Add(this.Pseudorandom.Next(totalTrees));
        //    }
        //    return subsetIndices;

        //    // guaranteeing all trees are considered results in slower convergance than entirely random subsets
        //    //if (subsets.Count == 0)
        //    //{
        //    //    int[] allTreeIndices = new int[totalTrees];
        //    //    for (int treeIndex = 0; treeIndex < allTreeIndices.Length; ++treeIndex)
        //    //    {
        //    //        allTreeIndices[treeIndex] = treeIndex;
        //    //    }
        //    //    this.Pseudorandom.Shuffle(allTreeIndices);

        //    //    for (int subset = 0; subset < this.Jump; ++subset)
        //    //    {
        //    //        subsets.Add(new List<int>());
        //    //    }
        //    //    for (int treeIndex = 0; treeIndex < allTreeIndices.Length; treeIndex += this.Jump)
        //    //    {
        //    //        for (int subset = 0; subset < this.Jump; ++subset)
        //    //        {
        //    //            if (treeIndex + subset >= allTreeIndices.Length)
        //    //            {
        //    //                break;
        //    //            }
        //    //            subsets[subset].Add(allTreeIndices[treeIndex + subset]);
        //    //        }
        //    //    }
        //    //}
        //    //List<int> subsetIndices = subsets[^1];
        //    //subsets.RemoveAt(subsets.Count - 1);
        //    //return subsetIndices;
        //}

        private int GetTenure(float tenureScalingFactor)
        {
            return this.Tenure switch
            {
                TabuTenure.Fixed => this.MaximumTenure,
                TabuTenure.Stochastic => (int)(tenureScalingFactor * this.Pseudorandom.GetPseudorandomByteAsFloat()) + 2,
                _ => throw new NotSupportedException(String.Format("Unhandled tenure mode {0}.", this.Tenure))
            };
        }

        public override HeuristicPerformanceCounters Run(HeuristicResultPosition position, HeuristicResults results)
        {
            if (this.EscapeAfter < 1)
            {
                throw new InvalidOperationException(nameof(this.EscapeAfter));
            }
            if (this.EscapeDistance < 2)
            {
                throw new InvalidOperationException(nameof(this.EscapeDistance));
            }
            if (this.Iterations < 1)
            {
                throw new InvalidOperationException(nameof(this.Iterations));
            }
            //if (this.Jump < 1)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(this.Jump));
            //}
            if (this.MaximumTenure < 2)
            {
                throw new InvalidOperationException(nameof(this.MaximumTenure));
            }
            if (this.RunParameters.LogOnlyImprovingMoves)
            {
                throw new NotSupportedException("Logging of only improving moves isn't currently supported.");
            }

            IList<int> thinningPeriods = this.CurrentTrajectory.Treatments.GetHarvestPeriods();
            if (thinningPeriods.Count < 2)
            {
                throw new NotSupportedException("A decision between at least two thinning periods is expected.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(position, results);
            float highestFinancialValue = this.EvaluateInitialSelection(position, this.Iterations, perfCounters);

            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            int[,] remainingTabuTenures = new int[initialTreeRecordCount, thinningPeriods.Max() + 1];
            //List<List<int>> treeIndicesBySubset = this.GetDiameterQuantiles();
            //List<List<int>> treeIndicesBySubset = this.GetDiameterSubsets(); // performs poorly compared to diameter quantiles
            //List<List<int>> treeIndicesBySubset = this.GetHeightSubsets(); // performs worse

            OrganonStandTrajectory candidateTrajectory = new(this.CurrentTrajectory);
            OrganonStandTrajectory highestNonTabuTrajectory = new(this.CurrentTrajectory);
            OrganonStandTrajectory highTrajectoryInIteration = new(this.CurrentTrajectory);
            float tenureScalingFactor = (this.MaximumTenure - 2 - Constant.RoundTowardsZeroTolerance) / byte.MaxValue;
            //List<int> allTreeIndices = new List<int>(initialTreeRecordCount);
            //for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
            //{
            //    allTreeIndices.Add(treeIndex);
            //}
            //int jumpBase = 0;
            //List<List<int>> subsets = new List<List<int>>();
            //int treeIndexStep = this.Jump;

            float acceptedFinancialValue = Single.MinValue;
            SortedList<float, OneOptMove> highestNonTabuMovesByFinancialValue = new();
            float highestFinancialValueSinceLastEscape = highestFinancialValue;
            int iterationsSinceFinancialValueIncreasedOrEscape = 0;
            //int subset = 0;
            for (int iteration = 1; iteration < this.Iterations; ++iteration)
            {
                // attempt escape if needed
                if (iterationsSinceFinancialValueIncreasedOrEscape > this.EscapeAfter)
                {
                    foreach (OneOptMove move in highestNonTabuMovesByFinancialValue.Values)
                    {
                        // tenure all harvest periods for tree to block immediate undos
                        int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(move.TreeIndex);
                        int newOrExtendedTenure = Math.Max(this.GetTenure(tenureScalingFactor), 
                                                           Math.Max(remainingTabuTenures[move.TreeIndex, currentHarvestPeriod],
                                                                    remainingTabuTenures[move.TreeIndex, move.ThinPeriod]));
                        remainingTabuTenures[move.TreeIndex, currentHarvestPeriod] = newOrExtendedTenure;
                        remainingTabuTenures[move.TreeIndex, move.ThinPeriod] = newOrExtendedTenure;

                        // TODO: how to log tree IDs?
                        // last set is redundant since performed at end of previous iteration
                        // Last move can also potentially immediately undone since its tenure is only set one way here. The move has
                        // already been applied so the tree's current and move harvest periods are the same.
                        candidateTrajectory.SetTreeSelection(move.TreeIndex, move.ThinPeriod);
                        this.CurrentTrajectory.SetTreeSelection(move.TreeIndex, move.ThinPeriod);
                    }
                    iterationsSinceFinancialValueIncreasedOrEscape = -1; // incremented below
                    highestFinancialValueSinceLastEscape = Single.MinValue;
                }

                // evaluate potential moves in neighborhood
                float highestUnrestrictedFinancialValue = Single.MinValue;
                int bestTreeIndex = -1;
                int bestHarvestPeriod = -1;

                highestNonTabuMovesByFinancialValue.Clear();
                float highestNonTabuFinancialValue = Single.MinValue;
                float minimumNonTabuObjectiveFunction = Single.MaxValue;
                //List<int> treesInNeighborhood = allTreeIndices;
                //if ((this.Jump > 1) && (treeIndexStep == this.Jump))
                //{
                //    treesInNeighborhood = this.GetNextRandomSubset(initialTreeRecordCount /*, subsets*/);
                //}
                //List<int> treesInNeighborhood = treeIndicesBySubset[subset];
                //foreach (int treeIndex in treesInNeighborhood)
                //for (int treeIndex = jumpBase; treeIndex < initialTreeRecordCount; treeIndex += treeIndexStep)
                for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
                {
                    int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex);
                    // for (int harvestPeriodIndex = 0; harvestPeriodIndex < this.CurrentTrajectory.HarvestPeriods; ++harvestPeriodIndex)
                    foreach (int thinningPeriod in thinningPeriods)
                    {
                        if (thinningPeriod == currentHarvestPeriod)
                        {
                            continue;
                        }

                        // find objective function for this tree in this period
                        candidateTrajectory.SetTreeSelection(treeIndex, thinningPeriod);
                        perfCounters.GrowthModelTimesteps += candidateTrajectory.Simulate();
                        float candidateFinancialValue = this.GetFinancialValue(candidateTrajectory, position.FinancialIndex);

                        if (candidateFinancialValue > highestUnrestrictedFinancialValue)
                        {
                            highestUnrestrictedFinancialValue = candidateFinancialValue;
                            highTrajectoryInIteration.CopyTreeGrowthFrom(candidateTrajectory);
                            bestTreeIndex = treeIndex;
                            bestHarvestPeriod = thinningPeriod;
                        }

                        int tabuTenure = remainingTabuTenures[treeIndex, thinningPeriod];
                        if (tabuTenure == 0)
                        {
                            if (highestNonTabuMovesByFinancialValue.ContainsKey(candidateFinancialValue) == false)
                            {
                                // TODO: random resolution for colliding objective functions rather than ignore?
                                // Assume best non-tabu move gets accepted below, so fill move list to escape distance + 1 entries.
                                if (highestNonTabuMovesByFinancialValue.Count <= this.EscapeDistance)
                                {
                                    highestNonTabuMovesByFinancialValue.Add(candidateFinancialValue, new OneOptMove(treeIndex, thinningPeriod));
                                    minimumNonTabuObjectiveFunction = Math.Min(minimumNonTabuObjectiveFunction, candidateFinancialValue);
                                }
                                else if (candidateFinancialValue > minimumNonTabuObjectiveFunction)
                                {
                                    highestNonTabuMovesByFinancialValue.Remove(minimumNonTabuObjectiveFunction);
                                    highestNonTabuMovesByFinancialValue.Add(candidateFinancialValue, new OneOptMove(treeIndex, thinningPeriod));
                                    minimumNonTabuObjectiveFunction = highestNonTabuMovesByFinancialValue.Keys.First();
                                }
                            }

                            if (candidateFinancialValue > highestNonTabuFinancialValue)
                            {
                                highestNonTabuFinancialValue = candidateFinancialValue;
                                highestNonTabuTrajectory.CopyTreeGrowthFrom(candidateTrajectory);
                            }
                        }

                        if (tabuTenure > 0)
                        {
                            remainingTabuTenures[treeIndex, thinningPeriod] = tabuTenure - 1;
                        }

                        // revert candidate trajectory to current trajectory as no move has yet been accepted
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }
                }

                ++iterationsSinceFinancialValueIncreasedOrEscape;

                // make best move and update tabu table
                if (highestUnrestrictedFinancialValue > highestFinancialValue)
                {
                    // always accept best candidate if it improves upon the best solution
                    acceptedFinancialValue = highestUnrestrictedFinancialValue;
                    highestFinancialValue = highestUnrestrictedFinancialValue;
                    highestNonTabuFinancialValue = highestUnrestrictedFinancialValue;
                    this.CurrentTrajectory.CopyTreeGrowthFrom(highTrajectoryInIteration);

                    remainingTabuTenures[bestTreeIndex, bestHarvestPeriod] = this.GetTenure(tenureScalingFactor);
                    this.CopyTreeGrowthToBestTrajectory(this.CurrentTrajectory);

                    highestFinancialValueSinceLastEscape = highestUnrestrictedFinancialValue;
                    iterationsSinceFinancialValueIncreasedOrEscape = 0;
                    this.MoveLog.TryAddMove(bestTreeIndex);
                    ++perfCounters.MovesAccepted;
                }
                else if (highestNonTabuMovesByFinancialValue.Count > 0)
                {
                    // otherwise, accept the best non-tabu move when one exists
                    // This is either a disimproving move or an improving move which does not exceed the best objective function observed.
                    // Existence is quite likely since (n trees) * (n periods) > tenure in most configurations.
                    //if (bestNonTabuCandidateObjectiveFunction < acceptedObjectiveFunction)
                    //{
                    //    stochasticTenure = true;
                    //}
                    acceptedFinancialValue = highestNonTabuFinancialValue;
                    this.CurrentTrajectory.CopyTreeGrowthFrom(highestNonTabuTrajectory);

                    KeyValuePair<float, OneOptMove> highestValueNonTabuMove = highestNonTabuMovesByFinancialValue.Last();
                    remainingTabuTenures[highestValueNonTabuMove.Value.TreeIndex, highestValueNonTabuMove.Value.ThinPeriod] = this.GetTenure(tenureScalingFactor);
                    this.MoveLog.TryAddMove(highestValueNonTabuMove.Value.TreeIndex);
                    ++perfCounters.MovesRejected;

                    if (acceptedFinancialValue > highestFinancialValueSinceLastEscape)
                    {
                        highestFinancialValueSinceLastEscape = acceptedFinancialValue;
                        iterationsSinceFinancialValueIncreasedOrEscape = 0;
                    }
                }

                this.FinancialValue.TryAddMove(acceptedFinancialValue, highestNonTabuFinancialValue);

                //if (++jumpBase >= this.Jump)
                //{
                //    jumpBase = 0;
                //    treeIndexStep = 1;
                //}
                //else if (treeIndexStep != this.Jump)
                //{
                //    jumpBase = 0;
                //    treeIndexStep = this.Jump;
                //}
                //if (++subset >= this.Jump)
                //{
                //    subset = 0;
                //}
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}
