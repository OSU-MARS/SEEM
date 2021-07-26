using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Tree;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "SnagsAndLogs")]
    public class WriteSnagsAndLogs : WriteStandTrajectory
    {
        [Parameter(HelpMessage = "Omits diameter classes without any snags or logs. In most cases this substantially reduces output file size.")]
        public SwitchParameter SkipZero { get; set; }

        protected override void ProcessRecord()
        {
            this.ValidateParameters();

            using StreamWriter writer = this.GetWriter();

            // header
            bool resultsSpecified = this.Results != null;
            if (this.ShouldWriteHeader())
            {
                HeuristicParameters? heuristicParameters = null;
                if (resultsSpecified)
                {
                    heuristicParameters = WriteCmdlet.GetFirstHeuristicParameters(this.Results);
                }
                else if (this.Trajectories![0].Heuristic != null)
                {
                    heuristicParameters = this.Trajectories[0].Heuristic!.GetParameters();
                }

                writer.WriteLine(WriteCmdlet.GetHeuristicAndPositionCsvHeader(heuristicParameters) + ",standAge,species,diameter class,snags,logs");
            }

            // rows for periods
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            int maxIndex = resultsSpecified ? this.Results!.PositionsEvaluated.Count : this.Trajectories!.Count;
            for (int positionOrTrajectoryIndex = 0; positionOrTrajectoryIndex < maxIndex; ++positionOrTrajectoryIndex)
            {
                OrganonStandTrajectory highTrajectory = this.GetHighestTrajectoryAndLinePrefix(positionOrTrajectoryIndex, out StringBuilder linePrefix, out int _, out int _);

                SnagLogTable snagsAndLogs = new(highTrajectory, this.MaximumDiameter, this.DiameterClassSize);
                for (int periodIndex = 0; periodIndex < highTrajectory.PlanningPeriods; ++periodIndex)
                {
                    OrganonStand? stand = highTrajectory.StandByPeriod[periodIndex];
                    Debug.Assert(stand != null);
                    string standAge = stand.AgeInYears.ToString(CultureInfo.InvariantCulture);

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
