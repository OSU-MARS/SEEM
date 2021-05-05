using Osu.Cof.Ferm.Heuristics;
using System;
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
        public HeuristicResultSet? Results { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Results!.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Results));
            }

            using StreamWriter writer = this.GetWriter();

            StringBuilder line = new();
            if (this.ShouldWriteHeader())
            {
                HeuristicDistribution distribution = this.Results.Distributions[0];
                if (distribution.HeuristicParameters == null)
                {
                    throw new NotSupportedException("Cannot generate header because first run is missing highest solution parameters");
                }

                line.Append("stand,heuristic," + distribution.HeuristicParameters!.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",generation,highest min,highest mean,highest max,highest cov,highest alleles,highest heterozygosity,highest individuals,highest polymorphism,lowest min,lowest mean,lowest max,lowest cov,lowest alleles,lowest heterozygosity,lowest individuals,lowest polymorphism");
                writer.WriteLine(line);
            }

            for (int resultIndex = 0; resultIndex < this.Results.Count; ++resultIndex)
            {
                HeuristicDistribution distribution = this.Results.Distributions[resultIndex];
                HeuristicSolutionPool solution = this.Results.Solutions[resultIndex];
                if ((solution.Highest == null) || (solution.Lowest == null) || (distribution.HeuristicParameters == null))
                {
                    throw new NotSupportedException("Result " + resultIndex + " is missing a highest solution, lowest solution, or highest solution parameters");
                }
                GeneticAlgorithm highestHeuristic = (GeneticAlgorithm)solution.Highest;
                GeneticAlgorithm lowestHeuristic = (GeneticAlgorithm)solution.Lowest;
                StandTrajectory highestTrajectory = highestHeuristic.BestTrajectory;
                string linePrefix = highestTrajectory.Name + "," + 
                    highestHeuristic.GetName() + "," + 
                    distribution.HeuristicParameters.GetCsvValues() + "," +
                    WriteCmdlet.GetRateAndAgeCsvValues(highestTrajectory);

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
