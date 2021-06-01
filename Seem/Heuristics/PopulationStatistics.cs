using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    // numerous references for genetic diversity
    // one such: https://cropgenebank.sgrp.cgiar.org/index.php/learning-space-mainmenu-454/training-modules-mainmenu-455/molecular-markers-mainmenu-545
    //   - measures of genetic diversity
    public class PopulationStatistics
    {
        public List<float> CoefficientOfVarianceByGeneration { get; private init; }
        public List<float> MaximumFitnessByGeneration { get; private init; }
        public List<float> MeanAllelesPerLocusByGeneration { get; private init; }
        public List<float> MeanFitnessByGeneration { get; private init; }
        public List<float> MeanHeterozygosityByGeneration { get; private init; }
        public List<float> MinimumFitnessByGeneration { get; private init; }
        public List<int> NewIndividualsByGeneration { get; private init; }
        public List<float> PolymorphismByGeneration { get; private init; }

        public PopulationStatistics()
        {
            this.CoefficientOfVarianceByGeneration = new List<float>();
            this.MaximumFitnessByGeneration = new List<float>();
            this.MeanAllelesPerLocusByGeneration = new List<float>();
            this.MeanFitnessByGeneration = new List<float>();
            this.MeanHeterozygosityByGeneration = new List<float>();
            this.MinimumFitnessByGeneration = new List<float>();
            this.NewIndividualsByGeneration = new List<int>();
            this.PolymorphismByGeneration = new List<float>();
        }

        public int Generations
        {
            get { return this.CoefficientOfVarianceByGeneration.Count; }
        }

        public float AddGeneration(Population population, IList<int> thinningPeriods)
        {
            if (population.SolutionsInPool < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(population));
            }

            // find count polymorphic alles (trees) and accumulate variance terms
            // Numerically preferred recursive variance from Hoemmen, M. 2007. Computing the standard deviation efficiently. http://suave_skola.varak.net/proj2/stddev.pdf
            //  - with conversion from ones based to zeros based indexing
            int[,] alleleCounts = new int[population.TreeCount, thinningPeriods.Max() + 1];
            float maximumFitness = Single.MinValue;
            double meanFitness = population.IndividualFitness[0];
            float minimumFitness = Single.MaxValue;
            double sumOfSquares = 0.0F;
            for (int individualIndex = 0; individualIndex < population.SolutionsInPool; ++individualIndex)
            {
                float individualFitness = population.IndividualFitness[individualIndex];
                if (individualFitness > maximumFitness)
                {
                    maximumFitness = individualFitness;
                }
                double individualDifference = individualFitness - meanFitness;
                sumOfSquares += (double)individualIndex / (double)(individualIndex + 1) * individualDifference * individualDifference;
                meanFitness += ((double)individualFitness - meanFitness) / (double)(individualIndex + 1);
                if (individualFitness < minimumFitness)
                {
                    minimumFitness = individualFitness;
                }

                int[] individualSchedule = population.IndividualTreeSelections[individualIndex];
                for (int treeIndex = 0; treeIndex < population.TreeCount; ++treeIndex)
                {
                    int treeSchedule = individualSchedule[treeIndex];
                    ++alleleCounts[treeIndex, treeSchedule];
                }
            }

            float meanHeterozygosity = population.TreeCount;
            int polymorphicLoci = 0;
            int uniqueAllelesPresent = 0;
            for (int treeIndex = 0; treeIndex < alleleCounts.GetLength(0); ++treeIndex)
            {
                bool isPolymorphicLocus = false;
                for (int periodIndex = 0; periodIndex < alleleCounts.GetLength(1); ++periodIndex)
                {
                    int alleleCount = alleleCounts[treeIndex, periodIndex];
                    if (alleleCount > 0)
                    {
                        float frequency = (float)alleleCount / (float)population.SolutionsInPool;
                        if (frequency < Constant.PolymorphicLocusThreshold)
                        {
                            isPolymorphicLocus = true;
                        }
                        meanHeterozygosity -= frequency * frequency;
                        ++uniqueAllelesPresent;
                    }
                }

                if (isPolymorphicLocus)
                {
                    ++polymorphicLoci;
                }
            }

            // find and add summary statistics
            float coeffcientOfVariation = 0.0F;
            if (sumOfSquares != 0.0)
            {
                double variance = sumOfSquares / population.SolutionsInPool;
                double fitnessStandardDeviation = Math.Sqrt(variance);
                Debug.Assert(Double.IsNaN(fitnessStandardDeviation) == false);
                coeffcientOfVariation = (float)(fitnessStandardDeviation / Math.Abs(meanFitness));
            }
            this.CoefficientOfVarianceByGeneration.Add(coeffcientOfVariation);

            this.NewIndividualsByGeneration.Add(population.SolutionsAccepted);
            population.SolutionsAccepted = 0;

            this.MaximumFitnessByGeneration.Add(maximumFitness);
            float meanAllelesPerLocus = (float)uniqueAllelesPresent / (float)population.TreeCount;
            this.MeanAllelesPerLocusByGeneration.Add(meanAllelesPerLocus);
            meanHeterozygosity /= (float)population.TreeCount;
            this.MeanFitnessByGeneration.Add((float)meanFitness);
            this.MeanHeterozygosityByGeneration.Add(meanHeterozygosity);
            this.MinimumFitnessByGeneration.Add(minimumFitness);

            float polymorphicFraction = (float)polymorphicLoci / (float)population.TreeCount;
            this.PolymorphismByGeneration.Add(polymorphicFraction);

            return coeffcientOfVariation;
        }
    }
}
