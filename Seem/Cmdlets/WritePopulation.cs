using Mars.Seem.Heuristics;
using Mars.Seem.Output;
using Mars.Seem.Silviculture;
using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Population")]
    public class WritePopulation : WriteSilviculturalTrajectoriesCmdlet
    {
        protected override void ProcessRecord()
        {
            this.ValidateParameters();

            using StreamWriter writer = this.CreateCsvWriter();

            if (this.ShouldWriteCsvHeader())
            {
                writer.WriteLine(this.GetCsvHeaderForSilviculturalCoordinate() + ",generation,highMin,highMean,highMax,highCov,highAlleles,highHeterozygosity,highIndividuals,highPolymorphism,lowMin,lowMean,lowMax,lowCov,lowAlleles,lowHeterozygosity,lowIndividuals,lowPolymorphism");
            }

            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            WriteSilviculturalCoordinateContext writeContext = new(this.HeuristicParameters);
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[trajectoryIndex];
                writeContext.SetSilviculturalSpace(silviculturalSpace);

                for (int positionIndex = 0; positionIndex < silviculturalSpace.CoordinatesEvaluated.Count; ++positionIndex)
                {
                    SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[positionIndex];
                    writeContext.SetSilviculturalCoordinate(coordinate);
                    string linePrefix = writeContext.GetCsvPrefixForSilviculturalCoordinate();

                    SilviculturalCoordinateExploration exploration = silviculturalSpace[coordinate];
                    PopulationStatistics highStatistics = ((GeneticAlgorithm)exploration.Pool.High.Heuristic!).PopulationStatistics;
                    PopulationStatistics lowStatistics = ((GeneticAlgorithm)exploration.Pool.Low.Heuristic!).PopulationStatistics;
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

                        string line = linePrefix + "," +
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
                            lowPolymorphism;
                        writer.WriteLine(line);
                        estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;
                    }

                    if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                    {
                        // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                        knownFileSizeInBytes = writer.BaseStream.Length;
                        estimatedBytesSinceLastFileLength = 0;
                    }
                    if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                    {
                        this.WriteWarning("Write-Population: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                        break;
                    }
                }
            }
        }
    }
}
