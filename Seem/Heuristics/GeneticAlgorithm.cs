using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GeneticAlgorithm : Heuristic<GeneticParameters>
    {
        private IList<int>? thinningPeriods;

        public PopulationStatistics PopulationStatistics { get; private init; }

        public GeneticAlgorithm(OrganonStand stand, GeneticParameters heuristicPparameters, RunParameters runParameters)
            : base(stand, heuristicPparameters, runParameters)
        {
            this.thinningPeriods = null;

            this.PopulationStatistics = new PopulationStatistics();
        }

        public override string GetName()
        {
            return "Genetic";
        }

        private void MutateChild(OrganonStandTrajectory childTrajectory, float flipProbability, float exchangeProbability, float treeScalingFactor)
        {
            Debug.Assert((this.thinningPeriods != null) && (this.thinningPeriods.Count == 2));

            // perform guaranteed flip mutations until remaining flip probability is less than 1.0
            float flipProbabilityRemaining = flipProbability;
            for (; flipProbabilityRemaining >= 1.0F; --flipProbabilityRemaining)
            {
                int uncompactedTreeIndex = (int)(treeScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                int currentHarvestPeriod = childTrajectory.GetTreeSelection(uncompactedTreeIndex);
                int newHarvestPeriod = this.GetOneOptCandidateRandom(currentHarvestPeriod, this.thinningPeriods);
                childTrajectory.SetTreeSelection(uncompactedTreeIndex, newHarvestPeriod);
            }

            // remaining flip mutation probability
            float mutationProbability = this.Pseudorandom.GetPseudorandomByteAsProbability();
            if (mutationProbability < flipProbabilityRemaining)
            {
                int uncompactedTreeIndex = (int)(treeScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                int currentHarvestPeriod = childTrajectory.GetTreeSelection(uncompactedTreeIndex);
                int newHarvestPeriod = this.GetOneOptCandidateRandom(currentHarvestPeriod, this.thinningPeriods);
                childTrajectory.SetTreeSelection(uncompactedTreeIndex, newHarvestPeriod);
            }

            // exchange mutations
            //if (flipProbabilityRemaining + exchangeProbability < 1.000001F)
            //{
            //    // flip and exchange mutations exclusive: disadvantageous relative to independent mutations
            //    mutationProbability -= flipProbabilityRemaining;
            //    if (mutationProbability < exchangeProbability)
            //    {
            //        int firstTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
            //        int secondTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
            //        int firstHarvestPeriod = childTrajectory.GetTreeSelection(firstTreeIndex);
            //        childTrajectory.SetTreeSelection(firstTreeIndex, childTrajectory.GetTreeSelection(secondTreeIndex));
            //        childTrajectory.SetTreeSelection(secondTreeIndex, firstHarvestPeriod);
            //    }
            //}
            //else
            //{
            // flip and exchange mutations independent
            if (this.Pseudorandom.GetPseudorandomByteAsProbability() < exchangeProbability)
            {
                int firstTreeIndex = (int)(treeScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                int secondTreeIndex = (int)(treeScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                int firstHarvestPeriod = childTrajectory.GetTreeSelection(firstTreeIndex);
                childTrajectory.SetTreeSelection(firstTreeIndex, childTrajectory.GetTreeSelection(secondTreeIndex));
                childTrajectory.SetTreeSelection(secondTreeIndex, firstHarvestPeriod);
            }
            //}
        }

        public override HeuristicPerformanceCounters Run(HeuristicSolutionPosition position, HeuristicSolutionIndex solutionIndex)
        {
            // TODO: support initialization from existing population?
            if ((this.HeuristicParameters.CrossoverProbabilityEnd < 0.0F) || (this.HeuristicParameters.CrossoverProbabilityEnd > 1.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.CrossoverProbabilityEnd));
            }
            if ((this.HeuristicParameters.ExchangeProbabilityEnd < 0.0F) || (this.HeuristicParameters.ExchangeProbabilityEnd > 1.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.ExchangeProbabilityEnd));
            }
            if ((this.HeuristicParameters.ExchangeProbabilityStart < 0.0F) || (this.HeuristicParameters.ExchangeProbabilityStart > 1.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.ExchangeProbabilityStart));
            }
            if (this.HeuristicParameters.ExponentK > 0.0F)
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.ExponentK));
            }
            if ((this.HeuristicParameters.FlipProbabilityEnd < 0.0F) || (this.HeuristicParameters.FlipProbabilityEnd > 10.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.FlipProbabilityEnd));
            }
            if ((this.HeuristicParameters.FlipProbabilityStart < 0.0F) || (this.HeuristicParameters.FlipProbabilityStart > 10.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.FlipProbabilityStart));
            }
            if (this.HeuristicParameters.MaximumGenerations < 1)
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.MaximumGenerations));
            }
            if (this.HeuristicParameters.MinimumCoefficientOfVariation < 0.0)
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.MinimumCoefficientOfVariation));
            }
            if (this.HeuristicParameters.PopulationSize < 1)
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.PopulationSize));
            }
            if ((this.HeuristicParameters.ReservedProportion < 0.0F) || (this.HeuristicParameters.ReservedProportion > 1.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.ReservedProportion));
            }

            this.thinningPeriods = this.CurrentTrajectory.Treatments.GetValidThinningPeriods();

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            int moveCapacity = this.HeuristicParameters.MaximumGenerations * this.HeuristicParameters.PopulationSize;
            this.AcceptedObjectiveFunctionByMove.Capacity = moveCapacity;
            this.CandidateObjectiveFunctionByMove.Capacity = moveCapacity;

            // begin with population of tree selections randomized across a proportional harvest intensity gradient
            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            Population currentGeneration = new(this.HeuristicParameters.PopulationSize, this.HeuristicParameters.ReservedProportion, initialTreeRecordCount);
            perfCounters.TreesRandomizedInConstruction += currentGeneration.ConstructTreeSelections(this.CurrentTrajectory, this.HeuristicParameters);
            OrganonStandTrajectory individualTrajectory = new(this.CurrentTrajectory);
            this.BestObjectiveFunction = Single.MinValue;
            for (int individualIndex = 0; individualIndex < this.HeuristicParameters.PopulationSize; ++individualIndex)
            {
                int[] individualTreeSelection = currentGeneration.IndividualTreeSelections[individualIndex];
                for (int treeIndex = 0; treeIndex < individualTreeSelection.Length; ++treeIndex)
                {
                    individualTrajectory.SetTreeSelection(treeIndex, individualTreeSelection[treeIndex]);
                }
                perfCounters.GrowthModelTimesteps += individualTrajectory.Simulate();

                float individualFitness = this.GetObjectiveFunction(individualTrajectory);
                currentGeneration.InsertFitness(individualIndex, individualFitness);
                ++perfCounters.MovesAccepted;

                if (individualFitness > this.BestObjectiveFunction)
                {
                    this.BestObjectiveFunction = individualFitness;
                    this.BestTrajectory.CopyTreeGrowthFrom(individualTrajectory);
                }

                this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                this.CandidateObjectiveFunctionByMove.Add(individualFitness);
            }
            Debug.Assert(currentGeneration.SolutionsAccepted == currentGeneration.SolutionsInPool);
            this.PopulationStatistics.AddGeneration(currentGeneration, this.thinningPeriods);

            // get sort order for K-point crossover
            //Stand standBeforeThin = this.BestTrajectory.StandByPeriod[this.BestTrajectory.HarvestPeriods];
            //if ((standBeforeThin.TreesBySpecies.Count != 1) || (standBeforeThin.TreesBySpecies.ContainsKey(FiaCode.PseudotsugaMenziesii) == false))
            //{
            //    throw new NotImplementedException();
            //}
            //int[] dbhSortOrder = standBeforeThin.TreesBySpecies[FiaCode.PseudotsugaMenziesii].GetDbhSortOrder();

            // for each generation of size n, perform n fertile matings
            float treeScalingFactor = (initialTreeRecordCount - Constant.RoundTowardsZeroTolerance) / UInt16.MaxValue;
            Population nextGeneration = new(currentGeneration);
            OrganonStandTrajectory firstChildTrajectory = individualTrajectory;
            OrganonStandTrajectory secondChildTrajectory = new(this.CurrentTrajectory);
            for (int generationIndex = 1; generationIndex < this.HeuristicParameters.MaximumGenerations; ++generationIndex)
            {
                currentGeneration.RecalculateMatingDistributionFunction();

                float generationFraction = (float)generationIndex / (float)this.HeuristicParameters.MaximumGenerations;
                float exponent = MathF.Exp(this.HeuristicParameters.ExponentK * generationFraction);
                float crossoverProbability = 0.5F + (this.HeuristicParameters.CrossoverProbabilityEnd - 0.5F) * exponent;
                float exchangeProbability = this.HeuristicParameters.ExchangeProbabilityEnd - (this.HeuristicParameters.ExchangeProbabilityEnd - this.HeuristicParameters.ExchangeProbabilityStart) * exponent;
                float flipProbability = this.HeuristicParameters.FlipProbabilityEnd - (this.HeuristicParameters.FlipProbabilityEnd - this.HeuristicParameters.FlipProbabilityStart) * exponent;
                for (int matingIndex = 0; matingIndex < currentGeneration.SolutionsInPool; ++matingIndex)
                {
                    // crossover parents' genetic material to create offsprings' genetic material
                    currentGeneration.FindParents(out int firstParentIndex, out int secondParentIndex);
                    //currentGeneration.CrossoverKPoint(1, firstParentIndex, secondParentIndex, dbhSortOrder, firstChildTrajectory, secondChildTrajectory);
                    currentGeneration.CrossoverUniform(firstParentIndex, secondParentIndex, crossoverProbability, firstChildTrajectory, secondChildTrajectory);

                    // maybe perform mutations
                    this.MutateChild(firstChildTrajectory, flipProbability, exchangeProbability, treeScalingFactor);
                    this.MutateChild(secondChildTrajectory, flipProbability, exchangeProbability, treeScalingFactor);
                    // open question: in cases where multiple thinning periods are available should path relinking occur?
                    // This applies in cases of two or more thins. In the case of two thins, if parent 1 retains and parent 2 removes a tree in 
                    // the second thinning then interior relinking would remove the tree in the first thin.

                    // evaluate fitness of offspring
                    // If crossover and mutation induced no changes simulation will be a no op. While this is handled by timestep caching additional checks
                    // could be made here.
                    perfCounters.GrowthModelTimesteps += firstChildTrajectory.Simulate();
                    float firstChildFitness = this.GetObjectiveFunction(firstChildTrajectory);

                    perfCounters.GrowthModelTimesteps += secondChildTrajectory.Simulate();
                    float secondChildFitness = this.GetObjectiveFunction(secondChildTrajectory);

                    // include offspring in next generation if they're more fit than members of the current population
                    if (nextGeneration.TryReplace(firstChildFitness, firstChildTrajectory, this.HeuristicParameters.ReplacementStrategy))
                    {
                        ++perfCounters.MovesAccepted;
                        if (firstChildFitness > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = firstChildFitness;
                            this.BestTrajectory.CopyTreeGrowthFrom(firstChildTrajectory);
                        }
                    }
                    else
                    {
                        ++perfCounters.MovesRejected;
                    }
                    this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                    this.CandidateObjectiveFunctionByMove.Add(firstChildFitness);

                    if (nextGeneration.TryReplace(secondChildFitness, secondChildTrajectory, this.HeuristicParameters.ReplacementStrategy))
                    {
                        ++perfCounters.MovesAccepted;
                        if (secondChildFitness > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = secondChildFitness;
                            this.BestTrajectory.CopyTreeGrowthFrom(secondChildTrajectory);
                        }
                    }
                    else
                    {
                        ++perfCounters.MovesRejected;
                    }
                    this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                    this.CandidateObjectiveFunctionByMove.Add(secondChildFitness);

                    // identify the fittest individual among the two parents and the two offspring and place it in the next generation
                    //float firstParentFitness = currentGeneration.IndividualFitness[firstParentIndex];
                    //float secondParentFitness = currentGeneration.IndividualFitness[secondParentIndex];

                    //bool firstChildFittest = firstChildFitness > secondChildFitness;
                    //float fittestChildFitness = firstChildFittest ? firstChildFitness : secondChildFitness;
                    //bool firstParentFittest = firstParentFitness > secondParentFitness;
                    //float fittestParentFitness = firstParentFittest ? firstParentFitness : secondParentFitness;

                    //if (fittestChildFitness > fittestParentFitness)
                    //{
                    //    // fittest individual is a child
                    //    nextGeneration.IndividualFitness[matingIndex] = fittestChildFitness;
                    //    if (firstChildFittest)
                    //    {
                    //        firstChildTrajectory.CopyTreeSelectionTo(nextGeneration.IndividualTreeSelections[matingIndex]);
                    //        Array.Copy(firstChildTrajectory.HarvestVolumesByPeriod, 0, nextGeneration.HarvestVolumesByPeriod[matingIndex], 0, firstChildTrajectory.HarvestVolumesByPeriod.Length);
                    //        if (firstChildFitness > this.BestObjectiveFunction)
                    //        {
                    //            this.BestObjectiveFunction = firstChildFitness;
                    //            this.BestTrajectory.CopyFrom(firstChildTrajectory);
                    //            bestIndividualIndex = matingIndex;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        secondChildTrajectory.CopyTreeSelectionTo(nextGeneration.IndividualTreeSelections[matingIndex]);
                    //        Array.Copy(secondChildTrajectory.HarvestVolumesByPeriod, 0, nextGeneration.HarvestVolumesByPeriod[matingIndex], 0, secondChildTrajectory.HarvestVolumesByPeriod.Length);
                    //        if (secondChildFitness > this.BestObjectiveFunction)
                    //        {
                    //            this.BestObjectiveFunction = secondChildFitness;
                    //            this.BestTrajectory.CopyFrom(secondChildTrajectory);
                    //            bestIndividualIndex = matingIndex;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    // fittest individual is a parent
                    //    nextGeneration.IndividualFitness[matingIndex] = fittestParentFitness;
                    //    if (firstParentFittest)
                    //    {
                    //        nextGeneration.IndividualTreeSelections[matingIndex] = firstParentHarvestSchedule;
                    //        nextGeneration.HarvestVolumesByPeriod[matingIndex] = currentGeneration.HarvestVolumesByPeriod[firstParentIndex];
                    //    }
                    //    else
                    //    {
                    //        nextGeneration.IndividualTreeSelections[matingIndex] = secondParentHarvestSchedule;
                    //        nextGeneration.HarvestVolumesByPeriod[matingIndex] = currentGeneration.HarvestVolumesByPeriod[secondParentIndex];
                    //    }
                    //}
                }

                currentGeneration.CopyFrom(nextGeneration);
                nextGeneration.SolutionsAccepted = 0;
                
                float coefficientOfVariation = this.PopulationStatistics.AddGeneration(currentGeneration, this.thinningPeriods);
                if (coefficientOfVariation < this.HeuristicParameters.MinimumCoefficientOfVariation)
                {
                    break;
                }
            }

            this.CurrentTrajectory.CopyTreeGrowthFrom(this.BestTrajectory);

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}
