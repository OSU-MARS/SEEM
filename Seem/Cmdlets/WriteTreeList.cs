using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "TreeList")]
    public class WriteTreeList : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStandTrajectory? Trajectory { get; set; }

        [Parameter]
        public Units Units { get; set; }

        public WriteTreeList()
        {
            this.Units = Units.Metric;
        }

        protected override void ProcessRecord()
        {
            using StreamWriter writer = this.CreateCsvWriter();

            // header
            if (this.ShouldWriteCsvHeader())
            {
                writer.WriteLine("stand,plot,tag,species,age,DBH,height,crownRatio,EF,deadEF");
            }

            // rows for trees
            int age = this.Trajectory!.PeriodZeroAgeInYears;
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int periodIndex = 0; periodIndex < this.Trajectory.PlanningPeriods; ++periodIndex)
            {
                Stand stand = this.Trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information not available for period " + periodIndex + ".");
                string ageAsString = age.ToString(CultureInfo.InvariantCulture);

                foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                {
                    WriteTreeList.GetMetricConversions(treesOfSpecies.Units, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);

                    string species = treesOfSpecies.Species.ToFourLetterCode();
                    for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                    {
                        float dbh = dbhConversionFactor * treesOfSpecies.Dbh[treeIndex];
                        float height = heightConversionFactor * treesOfSpecies.Height[treeIndex];
                        float liveExpansionFactor = areaConversionFactor * treesOfSpecies.LiveExpansionFactor[treeIndex];
                        float deadExpansionFactor = areaConversionFactor * treesOfSpecies.DeadExpansionFactor[treeIndex];

                        string line = stand.Name + "," +
                            treesOfSpecies.Plot[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                            treesOfSpecies.Tag[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                            species + "," +
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

                age += this.Trajectory.PeriodLengthInYears;

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
}
