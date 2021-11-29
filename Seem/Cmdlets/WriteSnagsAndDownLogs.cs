using Mars.Seem.Extensions;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "SnagsAndDownLogs")]
    public class WriteSnagsAndDownLogs : WriteStandTrajectory
    {
        [Parameter(HelpMessage = "Omits diameter classes without any snags or dead logs. In most cases this substantially reduces output file size.")]
        public SwitchParameter SkipZero { get; set; }

        protected override void ProcessRecord()
        {
            this.ValidateParameters();

            using StreamWriter writer = this.GetWriter();

            // header
            bool resultsSpecified = this.Trajectories != null;
            if (this.ShouldWriteHeader())
            {
                writer.WriteLine(this.GetCsvHeaderForCoordinate(this.Trajectories!) + ",standAge,species,diameter class,snags,logs");
            }

            // rows for periods
            int maxCoordinateIndex = this.GetMaxCoordinateIndex();
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int coordinateIndex = 0; coordinateIndex < maxCoordinateIndex; ++coordinateIndex)
            {
                StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(coordinateIndex, out string linePrefix);

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

                            writer.WriteLine(linePrefixForPeriodAndSpecies + "," +
                                             diameterClass.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                             snagsPerHectare.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                             logsPerHectare.ToString("0.00", CultureInfo.InvariantCulture));
                        }
                    }
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-SnagsAndLogs: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
