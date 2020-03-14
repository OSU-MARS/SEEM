using Osu.Cof.Organon.Heuristics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Organon.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Objective")]
    public class WriteObjective : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<Heuristic> Heuristics { get; set; }

        [Parameter(HelpMessage = "Number of iterations between CSV file lines. Default is 100, which prints every 100th objective function value.")]
        [ValidateRange(1, Int32.MaxValue)]
        public int Step;

        public WriteObjective()
        {
            this.Step = 100;
        }

        protected override void ProcessRecord()
        {
            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            StringBuilder line = new StringBuilder("iteration");
            int maxIteration = 0;
            for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                Heuristic heuristic = this.Heuristics[heuristicIndex];
                line.Append("," + heuristic.GetColumnName());
                maxIteration = Math.Max(maxIteration, heuristic.ObjectiveFunctionByIteration.Count);
            }
            writer.WriteLine(line);

            for (int iteration = 0; iteration < maxIteration; iteration += this.Step)
            {
                line.Clear();
                line.Append(iteration);

                for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
                {
                    line.Append(",");

                    Heuristic heuristic = this.Heuristics[heuristicIndex];
                    if (heuristic.ObjectiveFunctionByIteration.Count > iteration)
                    {
                        double objectiveFunction = heuristic.ObjectiveFunctionByIteration[iteration];
                        line.Append(objectiveFunction.ToString(CultureInfo.InvariantCulture));
                    }
                }

                writer.WriteLine(line);
            }
        }
    }
}
