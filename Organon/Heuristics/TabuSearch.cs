using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class TabuSearch : SingleTreeHeuristic
    {
        public int Iterations { get; set; }
        //public int Jump { get; set; }
        public int Tenure { get; set; }

        public TabuSearch(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            :  base(stand, organonConfiguration, planningPeriods, objective)
        {
            this.Iterations = stand.GetTreeRecordCount();
            //this.Jump = 1;
            this.Tenure = (int)(0.3 * this.Iterations);
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

        public override TimeSpan Run()
        {
            if (this.Iterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Iterations));
            }
            //if (this.Jump < 1)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(this.Jump));
            //}
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }
            if (this.Tenure < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Tenure));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            this.EvaluateInitialSelection(this.Iterations);

            float acceptedObjectiveFunction = this.BestObjectiveFunction;
            int initialTreeRecordCount = this.GetInitialTreeRecordCount();
            int[,] remainingTabuTenures = new int[initialTreeRecordCount, this.CurrentTrajectory.HarvestPeriods];
            //List<List<int>> treeIndicesBySubset = this.GetDiameterQuantiles();
            //List<List<int>> treeIndicesBySubset = this.GetDiameterSubsets(); // performs poorly compared to diameter quantiles
            //List<List<int>> treeIndicesBySubset = this.GetHeightSubsets(); // performs worse

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            OrganonStandTrajectory bestCandidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            OrganonStandTrajectory bestNonTabuCandidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            //float tenureScalingFactor = ((float)this.Tenure - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            //List<int> allTreeIndices = new List<int>(initialTreeRecordCount);
            //for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
            //{
            //    allTreeIndices.Add(treeIndex);
            //}
            //int jumpBase = 0;
            //List<List<int>> subsets = new List<List<int>>();
            //int treeIndexStep = this.Jump;

            //int subset = 0;
            for (int neighborhoodEvaluation = 0; neighborhoodEvaluation < this.Iterations; ++neighborhoodEvaluation)
            {
                // evaluate potential moves in neighborhood
                float bestCandidateObjectiveFunction = Single.MinValue;
                int bestTreeIndex = -1;
                int bestHarvestPeriod = -1;
                float bestNonTabuCandidateObjectiveFunction = Single.MinValue;
                int bestNonTabuTreeIndex = -1;
                int bestNonTabuHarvestPeriod = -1;
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
                    for (int harvestPeriodIndex = 0; harvestPeriodIndex < this.CurrentTrajectory.HarvestPeriods; harvestPeriodIndex += this.CurrentTrajectory.HarvestPeriods - 1)
                    {
                        if (harvestPeriodIndex == currentHarvestPeriod)
                        {
                            continue;
                        }

                        // find objective function for this tree in this period
                        candidateTrajectory.SetTreeSelection(treeIndex, harvestPeriodIndex);
                        candidateTrajectory.Simulate();
                        float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);

                        if (candidateObjectiveFunction > bestCandidateObjectiveFunction)
                        {
                            bestCandidateObjectiveFunction = candidateObjectiveFunction;
                            bestCandidateTrajectory.CopyFrom(candidateTrajectory);
                            bestTreeIndex = treeIndex;
                            bestHarvestPeriod = harvestPeriodIndex;
                        }

                        int tabuTenure = remainingTabuTenures[treeIndex, harvestPeriodIndex];
                        if ((tabuTenure == 0) && (candidateObjectiveFunction > bestNonTabuCandidateObjectiveFunction))
                        {
                            bestNonTabuCandidateObjectiveFunction = candidateObjectiveFunction;
                            bestNonTabuCandidateTrajectory.CopyFrom(candidateTrajectory);
                            bestNonTabuTreeIndex = treeIndex;
                            bestNonTabuHarvestPeriod = harvestPeriodIndex;
                        }

                        if (tabuTenure > 0)
                        {
                            remainingTabuTenures[treeIndex, harvestPeriodIndex] = tabuTenure - 1;
                        }

                        // revert candidate trajectory to current trajectory as no mmove has yet been accepted
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }
                }

                // make best move and update tabu table
                // other possibilities: 1) make unit tabu, 2) uncomment stochastic tenure
                if (bestCandidateObjectiveFunction > this.BestObjectiveFunction)
                {
                    // always accept best candidate if it improves upon the best solution
                    acceptedObjectiveFunction = bestCandidateObjectiveFunction;
                    this.CurrentTrajectory.CopyFrom(bestCandidateTrajectory);

                    remainingTabuTenures[bestTreeIndex, bestHarvestPeriod] = this.Tenure;
                    // remainingTabuTenures[bestUnitIndex, bestHarvestPeriod] = (int)(tenureScalingFactor * this.GetPseudorandomByteAsFloat()) + 1;

                    this.BestObjectiveFunction = bestCandidateObjectiveFunction;
                    this.BestTrajectory.CopyFrom(this.CurrentTrajectory);

                    this.CandidateObjectiveFunctionByMove.Add(bestCandidateObjectiveFunction);
                    this.TreeIDByMove.Add(bestTreeIndex);
                }
                else if (bestNonTabuTreeIndex != -1)
                {
                    // otherwise, accept the best non-tabu move when one exists
                    // Existence is quite likely since (n trees) * (n periods) > tenure in most configurations.
                    acceptedObjectiveFunction = bestNonTabuCandidateObjectiveFunction;
                    this.CurrentTrajectory.CopyFrom(bestNonTabuCandidateTrajectory);

                    remainingTabuTenures[bestNonTabuTreeIndex, bestNonTabuHarvestPeriod] = this.Tenure;
                    // remainingTabuTenures[bestNonTabuUnitIndex, bestNonTabuHarvestPeriod] = (int)(tenureScalingFactor * this.GetPseudorandomByteAsFloat()) + 1;

                    this.CandidateObjectiveFunctionByMove.Add(bestNonTabuCandidateObjectiveFunction);
                    this.TreeIDByMove.Add(bestNonTabuTreeIndex);
                }

                this.AcceptedObjectiveFunctionByMove.Add(acceptedObjectiveFunction);

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
            return stopwatch.Elapsed;
        }
    }
}
