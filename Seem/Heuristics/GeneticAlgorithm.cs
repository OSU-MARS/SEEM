using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GeneticAlgorithm : Heuristic
    {
        public float CrossoverProbabilityEnd { get; set; }
        public float ExchangeProbabilityEnd { get; set; }
        public float ExchangeProbabilityStart { get; set; }
        public float ExponentK { get; set; }
        public float FlipProbabilityEnd { get; set; }
        public float FlipProbabilityStart { get; set; }
        public int MaximumGenerations { get; set; }
        public float MinimumCoefficientOfVariation { get; set; }
        public int PopulationSize { get; set; }
        public PopulationStatistics PopulationStatistics { get; private init; }
        public PopulationReplacementStrategy ReplacementStrategy { get; set; }
        public float ReservedPopulationProportion { get; set; }

        public GeneticAlgorithm(OrganonStand stand, OrganonConfiguration organonConfiguration, Objective objective, GeneticParameters parameters)
            : base(stand, organonConfiguration, objective, parameters)
        {
            this.CrossoverProbabilityEnd = parameters.CrossoverProbabilityEnd;
            this.ExchangeProbabilityEnd = parameters.ExchangeProbabilityEnd;
            this.ExchangeProbabilityStart = parameters.ExchangeProbabilityStart;
            this.ExponentK = parameters.ExponentK;
            this.FlipProbabilityEnd = parameters.FlipProbabilityEnd;
            this.FlipProbabilityStart = parameters.FlipProbabilityStart;
            this.MaximumGenerations = parameters.MaximumGenerations;
            this.MinimumCoefficientOfVariation = parameters.MinimumCoefficientOfVariation;
            this.PopulationSize = parameters.PopulationSize;
            this.ReplacementStrategy = parameters.ReplacementStrategy;
            this.ReservedPopulationProportion = parameters.ReservedProportion;

            this.PopulationStatistics = new PopulationStatistics();
        }

        public override string GetName()
        {
            return "Genetic";
        }

        private void MutateChild(StandTrajectory childTrajectory, float flipProbability, float exchangeProbability, float treeScalingFactor)
        {
            // perform guaranteed flip mutations until remaining flip probability is less than 1.0
            float flipProbabilityRemaining = flipProbability;
            for (; flipProbabilityRemaining >= 1.0F; --flipProbabilityRemaining)
            {
                int treeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int harvestPeriod = childTrajectory.GetTreeSelection(treeIndex);
                int newHarvestPeriod = harvestPeriod == 0 ? childTrajectory.HarvestPeriods - 1 : 0;
                childTrajectory.SetTreeSelection(treeIndex, newHarvestPeriod);
            }

            // remaining flip mutation probability
            float mutationProbability = this.GetPseudorandomByteAsProbability();
            if (mutationProbability < flipProbabilityRemaining)
            {
                int treeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int harvestPeriod = childTrajectory.GetTreeSelection(treeIndex);
                int newHarvestPeriod = harvestPeriod == 0 ? childTrajectory.HarvestPeriods - 1 : 0;
                childTrajectory.SetTreeSelection(treeIndex, newHarvestPeriod);
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
            if ((this.CrossoverProbabilityEnd < 0.0F) || (this.CrossoverProbabilityEnd > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.CrossoverProbabilityEnd));
            }
            if ((this.ExchangeProbabilityEnd < 0.0F) || (this.ExchangeProbabilityEnd > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.ExchangeProbabilityEnd));
            }
            if ((this.ExchangeProbabilityStart < 0.0F) || (this.ExchangeProbabilityStart > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.ExchangeProbabilityStart));
            }
            if (this.ExponentK > 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ExponentK));
            }
            if ((this.FlipProbabilityEnd < 0.0F) || (this.FlipProbabilityEnd > 10.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.FlipProbabilityEnd));
            }
            if ((this.FlipProbabilityStart < 0.0F) || (this.FlipProbabilityStart > 10.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.FlipProbabilityStart));
            }
            if (this.MaximumGenerations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaximumGenerations));
            }
            if (this.MinimumCoefficientOfVariation < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MinimumCoefficientOfVariation));
            }
            if (this.PopulationSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.PopulationSize));
            }
            if ((this.ReservedPopulationProportion < 0.0F) || (this.ReservedPopulationProportion > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.ReservedPopulationProportion));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int moveCapacity = this.MaximumGenerations * this.PopulationSize;
            this.AcceptedObjectiveFunctionByMove.Capacity = moveCapacity;
            this.CandidateObjectiveFunctionByMove.Capacity = moveCapacity;

            // begin with population of random harvest schedules
            int initialTreeRecordCount = this.GetInitialTreeRecordCount();
            Population currentGeneration = new Population(this.PopulationSize, this.CurrentTrajectory.HarvestPeriods, this.ReservedPopulationProportion, initialTreeRecordCount);
            currentGeneration.RandomizeSchedules(this.Objective.HarvestPeriodSelection, this.CurrentTrajectory.StandByPeriod[0]); // TODO: use thinning period
            OrganonStandTrajectory individualTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            this.BestObjectiveFunction = Single.MinValue;
            for (int individualIndex = 0; individualIndex < this.PopulationSize; ++individualIndex)
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
            this.PopulationStatistics.AddGeneration(currentGeneration);

            // get sort order for K-point crossover
            //Stand standBeforeThin = this.BestTrajectory.StandByPeriod[this.BestTrajectory.HarvestPeriods];
            //if ((standBeforeThin.TreesBySpecies.Count != 1) || (standBeforeThin.TreesBySpecies.ContainsKey(FiaCode.PseudotsugaMenziesii) == false))
            //{
            //    throw new NotImplementedException();
            //}
            //int[] dbhSortOrder = standBeforeThin.TreesBySpecies[FiaCode.PseudotsugaMenziesii].GetDbhSortOrder();

            // for each generation of size n, perform n fertile matings
            float treeScalingFactor = ((float)initialTreeRecordCount - Constant.RoundTowardsZeroTolerance) / (float)UInt16.MaxValue;
            Population nextGeneration = new Population(currentGeneration);
            OrganonStandTrajectory firstChildTrajectory = individualTrajectory;
            OrganonStandTrajectory secondChildTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            for (int generationIndex = 1; generationIndex < this.MaximumGenerations; ++generationIndex)
            {
                currentGeneration.RecalculateMatingDistributionFunction();

                float generationFraction = (float)generationIndex / (float)this.MaximumGenerations;
                float exponent = MathF.Exp(this.ExponentK * generationFraction);
                float crossoverProbability = 0.5F + (this.CrossoverProbabilityEnd - 0.5F) * exponent;
                float exchangeProbability = this.ExchangeProbabilityEnd - (this.ExchangeProbabilityEnd - this.ExchangeProbabilityStart) * exponent;
                float flipProbability = this.FlipProbabilityEnd - (this.FlipProbabilityEnd - this.FlipProbabilityStart) * exponent;
                for (int matingIndex = 0; matingIndex < currentGeneration.Size; ++matingIndex)
                {
                    // crossover parents' genetic material to create offsprings' genetic material
                    currentGeneration.FindParents(out int firstParentIndex, out int secondParentIndex);
                    //currentGeneration.CrossoverKPoint(1, firstParentIndex, secondParentIndex, dbhSortOrder, firstChildTrajectory, secondChildTrajectory);
                    currentGeneration.CrossoverUniform(firstParentIndex, secondParentIndex, crossoverProbability, firstChildTrajectory, secondChildTrajectory);

                    // maybe perform mutations
                    this.MutateChild(firstChildTrajectory, flipProbability, exchangeProbability, treeScalingFactor);
                    this.MutateChild(secondChildTrajectory, flipProbability, exchangeProbability, treeScalingFactor);

                    // evaluate fitness of offspring
                    // TODO: check for no change breeding and avoid no op stand simulations?
                    firstChildTrajectory.Simulate();
                    float firstChildFitness = this.GetObjectiveFunction(firstChildTrajectory);

                    secondChildTrajectory.Simulate();
                    float secondChildFitness = this.GetObjectiveFunction(secondChildTrajectory);

                    // include offspring in next generation if they're more fit than members of the current population
                    if (nextGeneration.TryReplace(firstChildFitness, firstChildTrajectory, this.ReplacementStrategy))
                    {
                        if (firstChildFitness > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = firstChildFitness;
                            this.BestTrajectory.CopyFrom(firstChildTrajectory);
                        }
                    }
                    this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                    this.CandidateObjectiveFunctionByMove.Add(firstChildFitness);

                    if (nextGeneration.TryReplace(secondChildFitness, secondChildTrajectory, this.ReplacementStrategy))
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
                
                float coefficientOfVariation = this.PopulationStatistics.AddGeneration(currentGeneration);
                if (coefficientOfVariation < this.MinimumCoefficientOfVariation)
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
