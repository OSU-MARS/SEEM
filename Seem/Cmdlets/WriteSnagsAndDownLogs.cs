using Mars.Seem.Extensions;
using Mars.Seem.Output;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "SnagsAndDownLogs")]
    public class WriteSnagsAndDownLogs : WriteSilviculturalTrajectories
    {
        [Parameter(HelpMessage = "Omits diameter classes without any snags or dead logs. In most cases this substantially reduces output file size.")]
        public SwitchParameter SkipZero { get; set; }

        protected override void ProcessRecord()
        {
            this.ValidateParameters();

            using StreamWriter writer = this.CreateCsvWriter();

            // header
            if (this.ShouldWriteCsvHeader())
            {
                writer.WriteLine(this.GetCsvHeaderForSilviculturalCoordinate() + ",standAge,species,diameter class,snags,logs");
            }

            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            WriteSilviculturalCoordinateContext writeContext = new(this.HeuristicParameters);
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[trajectoryIndex];
                writeContext.SetSilviculturalSpace(silviculturalSpace);

                // rows for periods
                int maxCoordinateIndex = WriteSilviculturalTrajectoriesCmdlet.GetMaxCoordinateIndex(silviculturalSpace);
                for (int coordinateIndex = 0; coordinateIndex < maxCoordinateIndex; ++coordinateIndex)
                {
                    writeContext.SetSilviculturalCoordinate(coordinateIndex);
                    string linePrefix = writeContext.GetCsvPrefixForSilviculturalCoordinate();

                    StandTrajectory highTrajectory = writeContext.HighTrajectory;
                    SnagDownLogTable snagsAndLogs = new(highTrajectory, this.MaximumDiameter, this.DiameterClassSize);
                    for (int periodIndex = 0; periodIndex < highTrajectory.PlanningPeriods; ++periodIndex)
                    {
                        Stand? stand = highTrajectory.StandByPeriod[periodIndex];
                        Debug.Assert(stand != null);
                        string standAge = highTrajectory.GetEndOfPeriodAge(periodIndex).ToString(CultureInfo.InvariantCulture);

                        foreach (FiaCode species in snagsAndLogs.SnagsPerHectareBySpeciesAndDiameterClass.Keys)
                        {
                            float[,] logsByPeriodAndDiameterClass = snagsAndLogs.LogsPerHectareBySpeciesAndDiameterClass[species];
                            float[,] snagsByPeriodAndDiameterClass = snagsAndLogs.SnagsPerHectareBySpeciesAndDiameterClass[species];
                            string standAgeAndSpeciesCode = standAge + "," + species.ToFourLetterCode();
                            string linePrefixForPeriodAndSpecies = linePrefix + "," + standAgeAndSpeciesCode;

                            for (int diameterClassIndex = 0; diameterClassIndex < snagsAndLogs.DiameterClasses; ++diameterClassIndex)
                            {
                                float snagsPerHectare = snagsByPeriodAndDiameterClass[periodIndex, diameterClassIndex];
                                float logsPerHectare = logsByPeriodAndDiameterClass[periodIndex, diameterClassIndex];
                                if (this.SkipZero && (snagsPerHectare == 0.0F) && (logsPerHectare == 0.0F))
                                {
                                    continue;
                                }
                                float diameterClass = snagsAndLogs.GetDiameter(diameterClassIndex);

                                string line = linePrefixForPeriodAndSpecies + "," +
                                    diameterClass.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + "," +
                                    snagsPerHectare.ToString(Constant.Default.ExpansionFactorFormat, CultureInfo.InvariantCulture) + "," +
                                    logsPerHectare.ToString(Constant.Default.ExpansionFactorFormat, CultureInfo.InvariantCulture);
                                writer.WriteLine(line);
                                estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;
                            }
                        }
                    }

                    if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                    {
                        // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                        knownFileSizeInBytes = writer.BaseStream.Length;
                        estimatedBytesSinceLastFileLength = 0;
                    }
                    if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                    {
                        this.WriteWarning("Write-SnagsAndLogs: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                        break;
                    }
                }
            }
        }
    }
}
