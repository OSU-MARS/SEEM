using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GeneticAlgorithm : Heuristic
    {
        private IList<int>? thinningPeriods;

        public GeneticParameters Parameters { get; private init; }
        public PopulationStatistics PopulationStatistics { get; private init; }

        public GeneticAlgorithm(OrganonStand stand, OrganonConfiguration organonConfiguration, Objective objective, GeneticParameters parameters)
            : base(stand, organonConfiguration, objective, parameters)
        {
            this.thinningPeriods = null;

            this.Parameters = parameters;
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
                int uncompactedTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int currentHarvestPeriod = childTrajectory.GetTreeSelection(uncompactedTreeIndex);
                int newHarvestPeriod = this.GetOneOptCandidateRandom(currentHarvestPeriod, this.thinningPeriods);
                childTrajectory.SetTreeSelection(uncompactedTreeIndex, newHarvestPeriod);
            }

            // remaining flip mutation probability
            float mutationProbability = this.GetPseudorandomByteAsProbability();
            if (mutationProbability < flipProbabilityRemaining)
            {
                int uncompactedTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
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
            if (this.GetPseudorandomByteAsProbability() < exchangeProbability)
            {
                int firstTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int secondTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int firstHarvestPeriod = childTrajectory.GetTreeSelection(firstTreeIndex);
                childTrajectory.SetTreeSelection(firstTreeIndex, childTrajectory.GetTreeSelection(secondTreeIndex));
                childTrajectory.SetTreeSelection(secondTreeIndex, firstHarvestPeriod);
            }
            //}
        }

        public override TimeSpan Run()
        {
            // TODO: support initialization from existing population?
            if ((this.Parameters.CrossoverProbabilityEnd < 0.0F) || (this.Parameters.CrossoverProbabilityEnd > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.CrossoverProbabilityEnd));
            }
            if ((this.Parameters.ExchangeProbabilityEnd < 0.0F) || (this.Parameters.ExchangeProbabilityEnd > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.ExchangeProbabilityEnd));
            }
            if ((this.Parameters.ExchangeProbabilityStart < 0.0F) || (this.Parameters.ExchangeProbabilityStart > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.ExchangeProbabilityStart));
            }
            if (this.Parameters.ExponentK > 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.ExponentK));
            }
            if ((this.Parameters.FlipProbabilityEnd < 0.0F) || (this.Parameters.FlipProbabilityEnd > 10.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.FlipProbabilityEnd));
            }
            if ((this.Parameters.FlipProbabilityStart < 0.0F) || (this.Parameters.FlipProbabilityStart > 10.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.FlipProbabilityStart));
            }
            if (this.Parameters.MaximumGenerations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.MaximumGenerations));
            }
            if (this.Parameters.MinimumCoefficientOfVariation < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.MinimumCoefficientOfVariation));
            }
            if (this.Parameters.PopulationSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.PopulationSize));
            }
            if ((this.Parameters.ReservedProportion < 0.0F) || (this.Parameters.ReservedProportion > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.ReservedProportion));
            }

            this.thinningPeriods = this.CurrentTrajectory.Configuration.Treatments.GetValidThinningPeriods();

            Stopwatch stopwatch = new();
            stopwatch.Start();

            int moveCapacity = this.Parameters.MaximumGenerations * this.Parameters.PopulationSize;
            this.AcceptedObjectiveFunctionByMove.Capacity = moveCapacity;
            this.CandidateObjectiveFunctionByMove.Capacity = moveCapacity;

            // begin with population of random harvest schedules
            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            // this.CurrentTrajectory.Configuration.Treatments.Harvests
            Population currentGeneration = new(this.Parameters.PopulationSize, this.Parameters.ReservedProportion, initialTreeRecordCount);
            currentGeneration.RandomizeSchedules(this.CurrentTrajectory, this.Parameters);
            OrganonStandTrajectory individualTrajectory = new(this.CurrentTrajectory);
            this.BestObjectiveFunction = Single.MinValue;
            for (int individualIndex = 0; individualIndex < this.Parameters.PopulationSize; ++individualIndex)
            {
                int[] individualTreeSelection = currentGeneration.IndividualTreeSelections[individualIndex];
                for (int treeIndex = 0; treeIndex < individualTreeSelection.Length; ++treeIndex)
                {
                    individualTrajectory.SetTreeSelection(treeIndex, individualTreeSelection[treeIndex]);
                }
                individualTrajectory.Simulate();

                float individualFitness = this.GetObjectiveFunction(individualTrajectory);
                currentGeneration.AssignFitness(individualIndex, individualFitness);

                if (individualFitness > this.BestObjectiveFunction)
                {
                    this.BestObjectiveFunction = individualFitness;
                    this.BestTrajectory.CopyFrom(individualTrajectory);
                }

                this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                this.CandidateObjectiveFunctionByMove.Add(individualFitness);
            }
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
            for (int generationIndex = 1; generationIndex < this.Parameters.MaximumGenerations; ++generationIndex)
            {
                currentGeneration.RecalculateMatingDistributionFunction();

                float generationFraction = (float)generationIndex / (float)this.Parameters.MaximumGenerations;
                float exponent = MathF.Exp(this.Parameters.ExponentK * generationFraction);
                float crossoverProbability = 0.5F + (this.Parameters.CrossoverProbabilityEnd - 0.5F) * exponent;
                float exchangeProbability = this.Parameters.ExchangeProbabilityEnd - (this.Parameters.ExchangeProbabilityEnd - this.Parameters.ExchangeProbabilityStart) * exponent;
                float flipProbability = this.Parameters.FlipProbabilityEnd - (this.Parameters.FlipProbabilityEnd - this.Parameters.FlipProbabilityStart) * exponent;
                for (int matingIndex = 0; matingIndex < currentGeneration.Size; ++matingIndex)
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
                    // TODO: check for no change breeding and avoid no op stand simulations?
                    firstChildTrajectory.Simulate();
                    float firstChildFitness = this.GetObjectiveFunction(firstChildTrajectory);

                    secondChildTrajectory.Simulate();
                    float secondChildFitness = this.GetObjectiveFunction(secondChildTrajectory);

                    // include offspring in next generation if they're more fit than members of the current population
                    if (nextGeneration.TryReplace(firstChildFitness, firstChildTrajectory, this.Parameters.ReplacementStrategy))
                    {
                        if (firstChildFitness > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = firstChildFitness;
                            this.BestTrajectory.CopyFrom(firstChildTrajectory);
                        }
                    }
                    this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                    this.CandidateObjectiveFunctionByMove.Add(firstChildFitness);

                    if (nextGeneration.TryReplace(secondChildFitness, secondChildTrajectory, this.Parameters.ReplacementStrategy))
                    {
                        if (secondChildFitness > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = secondChildFitness;
                            this.BestTrajectory.CopyFrom(secondChildTrajectory);
                        }
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
                nextGeneration.NewIndividuals = 0;
                
                float coefficientOfVariation = this.PopulationStatistics.AddGeneration(currentGeneration, this.thinningPeriods);
                if (coefficientOfVariation < this.Parameters.MinimumCoefficientOfVariation)
                {
                    break;
                }
            }

            this.CurrentTrajectory.CopyFrom(this.BestTrajectory);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
