using DocumentFormat.OpenXml.Bibliography;
using Mars.Seem.Extensions;
using Mars.Seem.Output;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "TreeList")]
    public class WriteTreeList : WriteCmdlet
    {
        [Parameter(HelpMessage = "Calendar year at which stand trajectories start.")]
        public int? StartYear { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<StandTrajectory>? Trajectories { get; set; }

        [Parameter]
        public Units Units { get; set; }

        public WriteTreeList()
        {
            this.StartYear = null;
            this.Trajectories = null;
            this.Units = Units.Metric;
        }

        protected override void ProcessRecord()
        {
            string? fileExtension = Path.GetExtension(this.FilePath);
            switch (fileExtension)
            {
                case Constant.FileExtension.Csv:
                    this.WriteCsv();
                    break;
                case Constant.FileExtension.Feather:
                    this.WriteFeather();
                    break;
                default:
                    throw new NotSupportedException("Unknown file type '" + fileExtension + "' in " + nameof(this.FilePath) + "'" + this.FilePath + "'.");
            }
        }

        private void WriteCsv()
        {
            Debug.Assert(this.Trajectories != null);

            using StreamWriter writer = this.CreateCsvWriter();

            // header
            if (this.ShouldWriteCsvHeader())
            {
                writer.WriteLine("stand,plot,tag,species,year,standAge,DBH,height,crownRatio,liveExpansionFactor,deadExpansionFactor");
            }

            // write tree lists
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                StandTrajectory trajectory = this.Trajectories[trajectoryIndex];
                // rows for trees
                int age = trajectory.PeriodZeroAgeInYears;
                int? year = this.StartYear;
                for (int periodIndex = 0; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
                {
                    Stand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information not available for period " + periodIndex + ".");
                    string ageAsString = age.ToString(CultureInfo.InvariantCulture); // currently no support for individual tree ages

                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        (float diameterToCmMultiplier, float heightToMetersMultiplier, float hectareExpansionFactorMultiplier) = treesOfSpecies.Units.GetConversionToMetric();
                        
                        string species = treesOfSpecies.Species.ToFourLetterCode();
                        for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                        {
                            float dbh = diameterToCmMultiplier * treesOfSpecies.Dbh[treeIndex];
                            float height = heightToMetersMultiplier * treesOfSpecies.Height[treeIndex];
                            float liveExpansionFactor = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[treeIndex];
                            float deadExpansionFactor = hectareExpansionFactorMultiplier * treesOfSpecies.DeadExpansionFactor[treeIndex];

                            string line = stand.Name + "," +
                                treesOfSpecies.Plot[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                treesOfSpecies.Tag[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                species + "," +
                                year.ToString() + "," +
                                ageAsString + "," +
                                dbh.ToString(CultureInfo.InvariantCulture) + "," +
                                height.ToString(CultureInfo.InvariantCulture) + "," +
                                treesOfSpecies.CrownRatio[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                liveExpansionFactor.ToString(CultureInfo.InvariantCulture) + "," +
                                deadExpansionFactor.ToString(CultureInfo.InvariantCulture);
                            writer.WriteLine(line);
                            estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;
                        }
                    }

                    age += trajectory.PeriodLengthInYears;
                    if (year != null)
                    {
                        year += trajectory.PeriodLengthInYears;
                    }

                    if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                    {
                        // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                        knownFileSizeInBytes = writer.BaseStream.Length;
                        estimatedBytesSinceLastFileLength = 0;
                    }
                    if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                    {
                        this.WriteWarning("Write-TreeList: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                        break;
                    }
                }
            }
        }

        private void WriteFeather()
        {
            Debug.Assert(this.Trajectories != null);

            TreeListArrowMemory arrowMemory = new(this.Trajectories, this.StartYear);
            this.CheckOutputFileSize(arrowMemory);
            this.WriteFeather(arrowMemory);
        }
    }
}