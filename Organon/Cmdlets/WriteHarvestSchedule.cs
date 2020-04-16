using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "HarvestSchedule")]
    public class WriteHarvestSchedule : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<Heuristic> Heuristics { get; set; }

        protected override void ProcessRecord()
        {
            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            StringBuilder line = new StringBuilder("tree");
            int maxTreeIndex = 0;
            for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                Heuristic heuristic = this.Heuristics[heuristicIndex];
                line.Append("," + heuristic.GetColumnName());
                maxTreeIndex = Math.Max(maxTreeIndex, heuristic.BestTrajectory.IndividualTreeSelection.Length);
            }
            writer.WriteLine(line);

            for (int treeIndex = 0; treeIndex < maxTreeIndex; ++treeIndex)
            {
                line.Clear();
                line.Append(treeIndex);

                for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
                {
                    line.Append(",");

                    Heuristic heuristic = this.Heuristics[heuristicIndex];
                    if (heuristic.BestTrajectory.TreeRecordCount > treeIndex)
                    {
                        int harvestPeriod = heuristic.BestTrajectory.IndividualTreeSelection[treeIndex];
                        line.Append(harvestPeriod.ToString(CultureInfo.InvariantCulture));
                    }
                }

                writer.WriteLine(line);
            }
        }
    }
}
