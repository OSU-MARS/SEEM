using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Population")]
    public class WritePopulation : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution>? Runs { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Runs!.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Runs));
            }

            using StreamWriter writer = this.GetWriter();

            StringBuilder line = new StringBuilder();
            if (this.ShouldWriteHeader())
            {
                if (this.Runs[0].HighestHeuristicParameters == null)
                {
                    throw new NotSupportedException("Cannot generate header because first run is missing highest solution parameters");
                }

                line.Append("stand,heuristic," + this.Runs[0].HighestHeuristicParameters!.GetCsvHeader() + ",discount rate,thin age,rotation,generation,highest min,highest mean,highest max,highest cov,highest alleles,highest heterozygosity,highest individuals,highest polymorphism,lowest min,lowest mean,lowest max,lowest cov,lowest alleles,lowest heterozygosity,lowest individuals,lowest polymorphism");
                writer.WriteLine(line);
            }

            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                HeuristicSolutionDistribution distribution = this.Runs[runIndex];
                if ((distribution.HighestSolution == null) || (distribution.LowestSolution == null) || (distribution.HighestHeuristicParameters == null))
                {
                    throw new NotSupportedException("Run " + runIndex + " is missing a highest solution, lowest solution, or highest solution parameters");
                }
                GeneticAlgorithm highestHeuristic = (GeneticAlgorithm)distribution.HighestSolution;
                GeneticAlgorithm lowestHeuristic = (GeneticAlgorithm)distribution.LowestSolution;
                StandTrajectory highestTrajectory = highestHeuristic.BestTrajectory;
                string linePrefix = highestTrajectory.Name + "," + highestHeuristic.GetName() + "," + 
                    distribution.HighestHeuristicParameters.GetCsvValues() + "," + highestTrajectory.TimberValue.DiscountRate.ToString(CultureInfo.InvariantCulture) + "," + 
                    highestTrajectory.GetFirstHarvestAge().ToString(CultureInfo.InvariantCulture) + "," + highestTrajectory.GetRotationLength().ToString(CultureInfo.InvariantCulture);

                PopulationStatistics highestStatistics = highestHeuristic.PopulationStatistics;
                PopulationStatistics lowestStatistics = lowestHeuristic.PopulationStatistics;
                int maxGenerations = Math.Max(highestStatistics.Generations, lowestStatistics.Generations);
                for (int generationIndex = 0; generationIndex < maxGenerations; ++generationIndex)
                {
                    line.Clear();

                    string? highestMinimumFitness = null;
                    string? highestMeanFitness = null;
                    string? highestMaximumFitness = null;
                    string? highestCoefficientOfVariance = null;
                    string? highestMeanAlleles = null;
                    string? highestMeanHeterozygosity = null;
                    string? highestNewIndividuals = null;
                    string? highestPolymorphism = null;
                    if (highestStatistics.Generations > generationIndex)
                    {
                        highestMinimumFitness = highestStatistics.MinimumFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highestMeanFitness = highestStatistics.MeanFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highestMaximumFitness = highestStatistics.MaximumFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highestCoefficientOfVariance = highestStatistics.CoefficientOfVarianceByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highestMeanAlleles = highestStatistics.MeanAllelesPerLocusByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highestMeanHeterozygosity = highestStatistics.MeanHeterozygosityByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highestNewIndividuals = highestStatistics.NewIndividualsByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highestPolymorphism = highestStatistics.PolymorphismByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    string? lowestMinimumFitness = null;
                    string? lowestMeanFitness = null;
                    string? lowestMaximumFitness = null;
                    string? lowestCoefficientOfVariance = null;
                    string? lowestMeanAlleles = null;
                    string? lowestMeanHeterozygosity = null;
                    string? lowestNewIndividuals = null;
                    string? lowestPolymorphism = null;
                    if (lowestStatistics.Generations > generationIndex)
                    {
                        lowestMinimumFitness = lowestStatistics.MinimumFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowestMeanFitness = lowestStatistics.MeanFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowestMaximumFitness = lowestStatistics.MaximumFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowestCoefficientOfVariance = lowestStatistics.CoefficientOfVarianceByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowestMeanAlleles = lowestStatistics.MeanAllelesPerLocusByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowestMeanHeterozygosity = lowestStatistics.MeanHeterozygosityByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowestNewIndividuals = lowestStatistics.NewIndividualsByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowestPolymorphism = lowestStatistics.PolymorphismByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    line.Append(linePrefix + "," + 
                                generationIndex + "," +
                                highestMinimumFitness + "," +
                                highestMeanFitness + "," +
                                highestMaximumFitness + "," +
                                highestCoefficientOfVariance + "," +
                                highestMeanAlleles + "," +
                                highestMeanHeterozygosity + "," +
                                highestNewIndividuals + "," +
                                highestPolymorphism + "," +
                                lowestMinimumFitness + "," +
                                lowestMeanFitness + "," +
                                lowestMaximumFitness + "," +
                                lowestCoefficientOfVariance + "," +
                                lowestMeanAlleles + "," +
                                lowestMeanHeterozygosity + "," +
                                lowestNewIndividuals + "," +
                                lowestPolymorphism);
                    writer.WriteLine(line);
                }
            }
        }
    }
}
