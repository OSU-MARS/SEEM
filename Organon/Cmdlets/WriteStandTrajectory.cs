using Osu.Cof.Ferm.Organon;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Units")]
    public class WriteStandTrajectory : Cmdlet
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
            StringBuilder line = new StringBuilder("tree,harvest period");
            for (int harvestPeriod = 0; harvestPeriod < this.Trajectory.HarvestPeriods; ++harvestPeriod)
            {
                line.Append(", ");
            }
            writer.WriteLine(line);

            // rows for trees
            int treeRecordCount = this.Trajectory.StandByPeriod[0].TreeRecordCount;
            for (int treeIndex = 0; treeIndex < treeRecordCount; ++treeIndex)
            {
                line.Clear();
                line.Append(treeIndex);
                line.Append(",");
                line.Append(this.Trajectory.IndividualTreeSelection[treeIndex]);
                writer.WriteLine(line);
            }
        }
    }
}
