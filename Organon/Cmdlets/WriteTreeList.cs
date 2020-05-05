using Osu.Cof.Ferm.Organon;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "TreeList")]
    public class WriteTreeList : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStandTrajectory Trajectory { get; set; }

        protected override void ProcessRecord()
        {
            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            // header
            StringBuilder line = new StringBuilder("tree,DBH in period 0");
            for (int periodIndex = 1; periodIndex < this.Trajectory.PlanningPeriods; ++periodIndex)
            {
                line.Append("," + periodIndex.ToString(CultureInfo.InvariantCulture));
            }
            writer.WriteLine(line);

            // rows for trees
            int previousSpeciesCount = 0;
            foreach (KeyValuePair<FiaCode, int[]> treeSelectionForSpecies in this.Trajectory.IndividualTreeSelectionBySpecies)
            {
                for (int treeIndex = 0; treeIndex < treeSelectionForSpecies.Value.Length; ++treeIndex)
                {
                    Trees treesOfSpecies = this.Trajectory.StandByPeriod[0].TreesBySpecies[treeSelectionForSpecies.Key];
                    line.Clear();

                    // for now, best guess of using tree tag or index as unique identifier
                    line.Append(treesOfSpecies.Tag[treeIndex] == 0 ? previousSpeciesCount + treeIndex : treesOfSpecies.Tag[treeIndex]);
                    line.Append("," + treesOfSpecies.Dbh[treeIndex]);
                    for (int periodIndex = 1; periodIndex < this.Trajectory.PlanningPeriods; ++periodIndex)
                    {
                        treesOfSpecies = this.Trajectory.StandByPeriod[periodIndex].TreesBySpecies[treeSelectionForSpecies.Key];
                        line.Append("," + treesOfSpecies.Dbh[treeIndex]);
                    }
                    writer.WriteLine(line);
                }
            }
        }
    }
}
