using Osu.Cof.Organon.Heuristics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Organon.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "ObjectiveDistribution")]
    public class WriteObjectiveDistribution : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<double[]> Distribution { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<Heuristic> Heuristics { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Distribution.Count != this.Heuristics.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Distribution), "Number of distributions and number of heuristics must match.");
            }

            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            StringBuilder line = new StringBuilder("run");
            int maxRun = 0;
            for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                Heuristic heuristic = this.Heuristics[heuristicIndex];
                line.Append("," + heuristic.GetColumnName());

                double[] distribution = this.Distribution[heuristicIndex];
                maxRun = Math.Max(maxRun, distribution.Length);
            }
            writer.WriteLine(line);

            for (int run = 0; run < maxRun; ++run)
            {
                line.Clear();
                line.Append(run);

                for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
                {
                    line.Append(",");

                    double[] distribution = this.Distribution[heuristicIndex];
                    if (distribution.Length > run)
                    {
                        double objectiveFunction = distribution[run];
                        line.Append(objectiveFunction.ToString(CultureInfo.InvariantCulture));
                    }
                }

                writer.WriteLine(line);
            }
        }
    }
}
