using Osu.Cof.Ferm.Heuristics;
using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Population")]
    public class WritePopulation : WriteHeuristicResultsCmdlet
    {
        protected override void ProcessRecord()
        {
            if (this.Results!.Distributions.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Results));
            }

            using StreamWriter writer = this.GetWriter();

            if (this.ShouldWriteHeader())
            {
                HeuristicParameters heuristicParameters = WriteCmdlet.GetFirstHeuristicParameters(this.Results);
                writer.WriteLine("stand,heuristic," + heuristicParameters!.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",generation,highMin,highMean,highMax,highCov,highAlleles,highHeterozygosity,highIndividuals,highPolymorphism,lowMin,lowMean,lowMax,lowCov,lowAlleles,lowHeterozygosity,lowIndividuals,lowPolymorphism");
            }

            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int resultIndex = 0; resultIndex < this.Results.Distributions.Count; ++resultIndex)
            {
                HeuristicObjectiveDistribution distribution = this.Results.Distributions[resultIndex];
                HeuristicSolutionPool solution = this.Results.SolutionIndex[distribution];
                if ((solution.High == null) || (solution.Low == null))
                {
                    throw new NotSupportedException("Result " + resultIndex + " is missing a high solution or a low solution.");
                }
                GeneticAlgorithm highHeuristic = (GeneticAlgorithm)solution.High;
                HeuristicParameters highParameters = highHeuristic.GetParameters();
                StandTrajectory highTrajectory = highHeuristic.BestTrajectory;
                GeneticAlgorithm lowHeuristic = (GeneticAlgorithm)solution.Low;

                float discountRate = this.Results.DiscountRates[distribution.DiscountRateIndex];
                string linePrefix = highTrajectory.Name + "," + 
                    highHeuristic.GetName() + "," +
                    highParameters.GetCsvValues() + "," +
                    WriteCmdlet.GetRateAndAgeCsvValues(highTrajectory, discountRate);

                PopulationStatistics highStatistics = highHeuristic.PopulationStatistics;
                PopulationStatistics lowStatistics = lowHeuristic.PopulationStatistics;
                int maxGenerations = Math.Max(highStatistics.Generations, lowStatistics.Generations);
                for (int generationIndex = 0; generationIndex < maxGenerations; ++generationIndex)
                {
                    string? highMinimumFitness = null;
                    string? highMeanFitness = null;
                    string? highMaximumFitness = null;
                    string? highCoefficientOfVariance = null;
                    string? highMeanAlleles = null;
                    string? highMeanHeterozygosity = null;
                    string? highNewIndividuals = null;
                    string? highPolymorphism = null;
                    if (highStatistics.Generations > generationIndex)
                    {
                        highMinimumFitness = highStatistics.MinimumFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highMeanFitness = highStatistics.MeanFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highMaximumFitness = highStatistics.MaximumFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highCoefficientOfVariance = highStatistics.CoefficientOfVarianceByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highMeanAlleles = highStatistics.MeanAllelesPerLocusByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highMeanHeterozygosity = highStatistics.MeanHeterozygosityByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highNewIndividuals = highStatistics.NewIndividualsByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        highPolymorphism = highStatistics.PolymorphismByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    string? lowMinimumFitness = null;
                    string? lowMeanFitness = null;
                    string? lowMaximumFitness = null;
                    string? lowCoefficientOfVariance = null;
                    string? lowMeanAlleles = null;
                    string? lowMeanHeterozygosity = null;
                    string? lowNewIndividuals = null;
                    string? lowPolymorphism = null;
                    if (lowStatistics.Generations > generationIndex)
                    {
                        lowMinimumFitness = lowStatistics.MinimumFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowMeanFitness = lowStatistics.MeanFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowMaximumFitness = lowStatistics.MaximumFitnessByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowCoefficientOfVariance = lowStatistics.CoefficientOfVarianceByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowMeanAlleles = lowStatistics.MeanAllelesPerLocusByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowMeanHeterozygosity = lowStatistics.MeanHeterozygosityByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowNewIndividuals = lowStatistics.NewIndividualsByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                        lowPolymorphism = lowStatistics.PolymorphismByGeneration[generationIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    writer.WriteLine(linePrefix + "," + 
                                     generationIndex + "," +
                                     highMinimumFitness + "," +
                                     highMeanFitness + "," +
                                     highMaximumFitness + "," +
                                     highCoefficientOfVariance + "," +
                                     highMeanAlleles + "," +
                                     highMeanHeterozygosity + "," +
                                     highNewIndividuals + "," +
                                     highPolymorphism + "," +
                                     lowMinimumFitness + "," +
                                     lowMeanFitness + "," +
                                     lowMaximumFitness + "," +
                                     lowCoefficientOfVariance + "," +
                                     lowMeanAlleles + "," +
                                     lowMeanHeterozygosity + "," +
                                     lowNewIndividuals + "," +
                                     lowPolymorphism);
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-Population: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
