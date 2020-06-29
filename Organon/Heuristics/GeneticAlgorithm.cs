using Osu.Cof.Ferm.Cmdlets;
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
        public PopulationStatistics PopulationStatistics { get; private set; }
        public float ProportionalPercentageCenter { get; set; }
        public float ProportionalPercentageWidth { get; set; }
        public PopulationReplacementStrategy ReplacementStrategy { get; set; }
        public float ReservedPopulationProportion { get; set; }

        public GeneticAlgorithm(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            int treeRecords = stand.GetTreeRecordCount();
            this.CrossoverProbabilityEnd = Constant.GeneticDefault.CrossoverProbabilityEnd;
            this.ExchangeProbabilityEnd = Constant.GeneticDefault.ExchangeProbabilityEnd;
            this.ExchangeProbabilityStart = Constant.GeneticDefault.ExchangeProbabilityStart;
            this.ExponentK = Constant.GeneticDefault.ExponentK;
            this.FlipProbabilityEnd = Constant.GeneticDefault.FlipProbabilityEnd;
            this.FlipProbabilityStart = Constant.GeneticDefault.FlipProbabilityStart;
            this.MaximumGenerations = (int)(Constant.GeneticDefault.MaximumGenerationCoefficient * treeRecords + 0.5F);
            this.MinimumCoefficientOfVariation = Constant.GeneticDefault.MinimumCoefficientOfVariation;
            this.PopulationSize = Constant.GeneticDefault.PopulationSize;
            this.PopulationStatistics = new PopulationStatistics();
            this.ProportionalPercentageCenter = Constant.GeneticDefault.ProportionalPercentageCenter;
            this.ProportionalPercentageWidth = Constant.GeneticDefault.ProportionalPercentageWidth;
            this.ReplacementStrategy = Constant.GeneticDefault.ReplacementStrategy;
            this.ReservedPopulationProportion = Constant.GeneticDefault.ReservedPopulationProportion;
        }

        public override string GetName()
        {
            return "Genetic";
        }

        public override HeuristicParameters GetParameters()
        {
            return base.GetParameters();
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
            if ((this.FlipProbabilityEnd < 0.0F) || (this.FlipProbabilityEnd > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.FlipProbabilityEnd));
            }
            if ((this.FlipProbabilityStart < 0.0F) || (this.FlipProbabilityStart > 1.0F))
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
            if ((this.ProportionalPercentageCenter < 0.0F) || (this.ProportionalPercentageCenter > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.ProportionalPercentageCenter));
            }
            if ((this.ProportionalPercentageWidth < 0.0F) || (this.ProportionalPercentageWidth > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.ProportionalPercentageWidth));
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
            currentGeneration.RandomizeSchedule(this.Objective.HarvestPeriodSelection, this.ProportionalPercentageCenter, this.ProportionalPercentageWidth);
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
                    if (this.GetPseudorandomByteAsProbability() < exchangeProbability)
                    {
                        // 2-opt exchange
                        int firstTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int secondTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int firstHarvestPeriod = firstChildTrajectory.GetTreeSelection(firstTreeIndex);
                        firstChildTrajectory.SetTreeSelection(firstTreeIndex, firstChildTrajectory.GetTreeSelection(secondTreeIndex));
                        firstChildTrajectory.SetTreeSelection(secondTreeIndex, firstHarvestPeriod);
                    }

                    if (this.GetPseudorandomByteAsProbability() < flipProbability)
                    {
                        // 1-opt for single thin
                        int treeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int harvestPeriod = firstChildTrajectory.GetTreeSelection(treeIndex);
                        int newHarvestPeriod = harvestPeriod == 0 ? firstChildTrajectory.HarvestPeriods - 1 : 0;
                        firstChildTrajectory.SetTreeSelection(treeIndex, newHarvestPeriod);
                    }

                    if (this.GetPseudorandomByteAsProbability() < exchangeProbability)
                    {
                        // 2-opt exchange
                        int firstTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int secondTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int firstHarvestPeriod = secondChildTrajectory.GetTreeSelection(firstTreeIndex);
                        secondChildTrajectory.SetTreeSelection(firstTreeIndex, secondChildTrajectory.GetTreeSelection(secondTreeIndex));
                        secondChildTrajectory.SetTreeSelection(secondTreeIndex, firstHarvestPeriod);
                    }

                    if (this.GetPseudorandomByteAsProbability() < flipProbability)
                    {
                        // 1-opt for single thin
                        int treeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int harvestPeriod = secondChildTrajectory.GetTreeSelection(treeIndex);
                        int newHarvestPeriod = harvestPeriod == 0 ? secondChildTrajectory.HarvestPeriods - 1 : 0;
                        secondChildTrajectory.SetTreeSelection(treeIndex, newHarvestPeriod);
                    }

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
