using Osu.Cof.Ferm.Organon;
using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "TreeList")]
    public class WriteTreeList : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStandTrajectory Trajectory { get; set; }

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
            StringBuilder line = new StringBuilder();
            if (this.ShouldWriteHeader())
            {
                line.Append("stand,species,tree,age,DBH,height,crown ratio,expansion factor,dead expansion factor");
                writer.WriteLine(line);
            }

            // rows for trees
            int age = this.Trajectory.PeriodZeroAgeInYears;
            for (int periodIndex = 0; periodIndex < this.Trajectory.PlanningPeriods; ++periodIndex)
            {
                Stand stand = this.Trajectory.StandByPeriod[periodIndex];
                string ageAsString = age.ToString(CultureInfo.InvariantCulture);

                foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                {
                    this.GetDimensionConversions(treesOfSpecies.Units, this.Units, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);

                    string species = treesOfSpecies.Species.ToFourLetterCode();
                    for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                    {
                        float dbh = dbhConversionFactor * treesOfSpecies.Dbh[treeIndex];
                        float height = heightConversionFactor * treesOfSpecies.Height[treeIndex];
                        float liveExpansionFactor = areaConversionFactor * treesOfSpecies.LiveExpansionFactor[treeIndex];
                        float deadExpansionFactor = areaConversionFactor * treesOfSpecies.DeadExpansionFactor[treeIndex];

                        line.Clear();
                        line.Append(stand.Name + "," +
                                    species + "," +
                                    treesOfSpecies.Tag[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                    ageAsString + "," +
                                    dbh.ToString(CultureInfo.InvariantCulture) + "," +
                                    height.ToString(CultureInfo.InvariantCulture) + "," +
                                    treesOfSpecies.CrownRatio[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                    liveExpansionFactor.ToString(CultureInfo.InvariantCulture) + "," +
                                    deadExpansionFactor.ToString(CultureInfo.InvariantCulture));
                        writer.WriteLine(line);
                    }
                }

                age += this.Trajectory.PeriodLengthInYears;
            }
        }
    }
}
