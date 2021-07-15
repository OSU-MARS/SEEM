using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
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
            using StreamWriter writer = this.GetWriter();

            // header
            if (this.ShouldWriteHeader())
            {
                writer.WriteLine("stand,plot,tag,species,age,DBH,height,crownRatio,EF,deadEF");
            }

            // rows for trees
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            int age = this.Trajectory!.PeriodZeroAgeInYears;
            for (int periodIndex = 0; periodIndex < this.Trajectory.PlanningPeriods; ++periodIndex)
            {
                Stand stand = this.Trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information not available for period " + periodIndex + ".");
                string ageAsString = age.ToString(CultureInfo.InvariantCulture);

                foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                {
                    WriteTreeList.GetDimensionConversions(treesOfSpecies.Units, this.Units, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);

                    string species = treesOfSpecies.Species.ToFourLetterCode();
                    for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                    {
                        float dbh = dbhConversionFactor * treesOfSpecies.Dbh[treeIndex];
                        float height = heightConversionFactor * treesOfSpecies.Height[treeIndex];
                        float liveExpansionFactor = areaConversionFactor * treesOfSpecies.LiveExpansionFactor[treeIndex];
                        float deadExpansionFactor = areaConversionFactor * treesOfSpecies.DeadExpansionFactor[treeIndex];

                        writer.WriteLine(stand.Name + "," +
                                         treesOfSpecies.Plot[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                         treesOfSpecies.Tag[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                         species + "," +
                                         ageAsString + "," +
                                         dbh.ToString(CultureInfo.InvariantCulture) + "," +
                                         height.ToString(CultureInfo.InvariantCulture) + "," +
                                         treesOfSpecies.CrownRatio[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                         liveExpansionFactor.ToString(CultureInfo.InvariantCulture) + "," +
                                         deadExpansionFactor.ToString(CultureInfo.InvariantCulture));
                    }
                }

                age += this.Trajectory.PeriodLengthInYears;

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-TreeList: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
