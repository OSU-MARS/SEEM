using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GeneticAlgorithm : Heuristic
    {
        public float CentralSelectionProbability { get; set; }
        public float EndStandardDeviation { get; set; }
        public float ExchangeProbability { get; set; }
        public float FlipProbability { get; set; }
        public int MaximumGenerations { get; set; }
        public int PopulationSize { get; set; }
        public float ReservedPopulationProportion { get; set; }
        public float SelectionProbabilityWidth { get; set; }
        public List<float> VarianceByGeneration { get; private set; }

        public GeneticAlgorithm(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            this.CentralSelectionProbability = 0.5F;
            this.EndStandardDeviation = 0.001F;
            this.ExchangeProbability = 0.7F;
            this.FlipProbability = 0.9F;
            this.MaximumGenerations = 100;
            this.PopulationSize = 40;
            this.ReservedPopulationProportion = 0.7F;
            this.SelectionProbabilityWidth = 1.0F;
            this.VarianceByGeneration = new List<float>();

            this.ObjectiveFunctionByMove = new List<float>(this.MaximumGenerations);
        }

        public override string GetName()
        {
            return "Genetic";
        }

        private float GetMaximumFitnessAndVariance(GeneticPopulation generation)
        {
            // use double precision for intermediate variance calculations
            // When calculation is done with floats the sum of squares and squared sum accumulations and their eventual subtractions is sensitive to 
            // numerical precision, resulting in an understimate of variance, truncation to zero, a negative value. Using double precision avoids these
            // difficulties for when the end variance criteria is on the order of 1E-6.

            float highestFitness = Single.MinValue;
            double sum = 0.0F;
            double sumOfSquares = 0.0F;
            for (int individualIndex = 0; individualIndex < generation.Size; ++individualIndex)
            {
                float individualFitness = generation.IndividualFitness[individualIndex];
                sum += individualFitness;
                sumOfSquares += individualFitness * individualFitness;
                if (individualFitness > highestFitness)
                {
                    highestFitness = individualFitness;
                }
            }
            Debug.Assert(highestFitness >= this.BestObjectiveFunction);

            double n = generation.Size;
            //double meanHarvest = sum / n;
            //double variance = sumOfSquares / n - meanHarvest * meanHarvest;
            float variance = (float)((sumOfSquares - sum * sum / n) / n); // somewhat less likely to truncate to zero compared to direct calculation of mean
            return variance;
        }

        public override TimeSpan Run()
        {
            if (this.EndStandardDeviation <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.EndStandardDeviation));
            }
            if (this.MaximumGenerations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaximumGenerations));
            }
            if (this.PopulationSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.PopulationSize));
            }
            if ((this.ReservedPopulationProportion < 0.0) || (this.ReservedPopulationProportion > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.ReservedPopulationProportion));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // begin with population of random harvest schedules
            // TODO: CopyTreeSelectionFrom() for initializing tree selection?
            // TODO: should incoming schedule on this.CurrentSolution be one of the individuals in the population?
            int initialTreeRecordCount = this.GetInitialTreeRecordCount();
            int treeSelectionCapacity = Constant.Simd128x4.Width * (initialTreeRecordCount / Constant.Simd128x4.Width + 1);
            GeneticPopulation currentGeneration = new GeneticPopulation(this.PopulationSize, this.CurrentTrajectory.HarvestPeriods, this.ReservedPopulationProportion, treeSelectionCapacity);
            currentGeneration.RandomizeSchedule(this.Objective.HarvestPeriodSelection, this.CentralSelectionProbability, this.SelectionProbabilityWidth);
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
                currentGeneration.SetFitness(individualFitness, individualIndex);
                if (individualFitness > this.BestObjectiveFunction)
                {
                    this.BestObjectiveFunction = individualFitness;
                    this.BestTrajectory.CopyFrom(individualTrajectory);
                }
                this.ObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
            }

            // for each generation of size n, perform n fertile matings
            double endVariance = this.EndStandardDeviation * this.EndStandardDeviation;
            float treeScalingFactor = ((float)initialTreeRecordCount - Constant.RoundTowardsZeroTolerance) / (float)UInt16.MaxValue;
            float mutationScalingFactor = 1.0F / (float)UInt16.MaxValue;
            float variance = this.GetMaximumFitnessAndVariance(currentGeneration);
            GeneticPopulation nextGeneration = new GeneticPopulation(currentGeneration);
            OrganonStandTrajectory firstChildTrajectory = individualTrajectory;
            OrganonStandTrajectory secondChildTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            for (int generationIndex = 1; (generationIndex < this.MaximumGenerations) && (variance > endVariance); ++generationIndex)
            {
                currentGeneration.RecalculateMatingDistributionFunction();
                for (int matingIndex = 0; matingIndex < currentGeneration.Size; ++matingIndex)
                {
                    // crossover parents' genetic material to create offsprings' genetic material
                    currentGeneration.FindParents(out int firstParentIndex, out int secondParentIndex);
                    int crossoverPosition = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                    int[] firstParentHarvestSchedule = currentGeneration.IndividualTreeSelections[firstParentIndex];
                    int[] secondParentHarvestSchedule = currentGeneration.IndividualTreeSelections[secondParentIndex];
                    for (int treeIndex = 0; treeIndex < crossoverPosition; ++treeIndex)
                    {
                        firstChildTrajectory.SetTreeSelection(treeIndex, firstParentHarvestSchedule[treeIndex]);
                        secondChildTrajectory.SetTreeSelection(treeIndex, secondParentHarvestSchedule[treeIndex]);
                    }
                    for (int treeIndex = crossoverPosition; treeIndex < initialTreeRecordCount; ++treeIndex)
                    {
                        firstChildTrajectory.SetTreeSelection(treeIndex, secondParentHarvestSchedule[treeIndex]);
                        secondChildTrajectory.SetTreeSelection(treeIndex, firstParentHarvestSchedule[treeIndex]);
                    }

                    // maybe perform mutations
                    float firstExchangeProbability = mutationScalingFactor * this.GetPseudorandomByteAsFloat();
                    if (firstExchangeProbability < this.ExchangeProbability)
                    {
                        // 2-opt exchange
                        int firstTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int secondTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int harvestPeriod = firstChildTrajectory.GetTreeSelection(firstTreeIndex);
                        firstChildTrajectory.SetTreeSelection(firstTreeIndex, firstChildTrajectory.GetTreeSelection(secondTreeIndex));
                        firstChildTrajectory.SetTreeSelection(secondTreeIndex, harvestPeriod);
                    }

                    float firstFlipProbability = mutationScalingFactor * this.GetPseudorandomByteAsFloat();
                    if (firstFlipProbability < this.FlipProbability)
                    {
                        // 1-opt for single thin
                        int treeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int harvestPeriod = firstChildTrajectory.GetTreeSelection(treeIndex);
                        int newHarvestPeriod = harvestPeriod == 0 ? firstChildTrajectory.HarvestPeriods - 1 : 0;
                        firstChildTrajectory.SetTreeSelection(treeIndex, newHarvestPeriod);
                    }

                    float secondExchangeProbability = mutationScalingFactor * this.GetPseudorandomByteAsFloat();
                    if (secondExchangeProbability < this.ExchangeProbability)
                    {
                        // 2-opt exchange
                        int firstTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int secondTreeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int harvestPeriod = secondParentHarvestSchedule[firstTreeIndex];
                        secondChildTrajectory.SetTreeSelection(firstTreeIndex, firstChildTrajectory.GetTreeSelection(secondTreeIndex));
                        secondChildTrajectory.SetTreeSelection(secondTreeIndex, harvestPeriod);
                    }

                    float secondFlipProbability = mutationScalingFactor * this.GetPseudorandomByteAsFloat();
                    if (secondFlipProbability < this.FlipProbability)
                    {
                        // 1-opt for single thin
                        int treeIndex = (int)(treeScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        int harvestPeriod = secondChildTrajectory.GetTreeSelection(treeIndex);
                        int newHarvestPeriod = harvestPeriod == 0 ? secondChildTrajectory.HarvestPeriods - 1 : 0;
                        secondChildTrajectory.SetTreeSelection(treeIndex, newHarvestPeriod);
                    }

                    // evaluate fitness of offspring
                    firstChildTrajectory.Simulate();
                    float firstChildFitness = this.GetObjectiveFunction(firstChildTrajectory);

                    secondChildTrajectory.Simulate();
                    float secondChildFitness = this.GetObjectiveFunction(secondChildTrajectory);

                    // include offspring in next generation if they're more fit than members of the current population
                    if (nextGeneration.TryInsert(firstChildFitness, firstChildTrajectory))
                    {
                        if (firstChildFitness > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = firstChildFitness;
                            this.BestTrajectory.CopyFrom(firstChildTrajectory);
                        }
                    }
                    if (nextGeneration.TryInsert(secondChildFitness, secondChildTrajectory))
                    {
                        if (secondChildFitness > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = secondChildFitness;
                            this.BestTrajectory.CopyFrom(secondChildTrajectory);
                        }
                    }

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

                    this.ObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                }

                //GeneticPopulation generationSwapPointer = currentGeneration;
                //currentGeneration = nextGeneration;
                //nextGeneration = generationSwapPointer;
                currentGeneration.CopyFrom(nextGeneration);
                variance = this.GetMaximumFitnessAndVariance(currentGeneration);
                this.VarianceByGeneration.Add(variance);
            }

            this.CurrentTrajectory.CopyFrom(this.BestTrajectory);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
