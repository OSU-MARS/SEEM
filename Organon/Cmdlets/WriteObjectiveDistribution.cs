using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "ObjectiveDistribution")]
    public class WriteObjectiveDistribution : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution> Runs { get; set; }

        protected override void ProcessRecord()
        {
            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            StringBuilder line = new StringBuilder("stand,heuristic,thin age,rotation,solution,objective");
            writer.WriteLine(line);

            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                Heuristic bestHeuristic = this.Runs[runIndex].BestSolution;
                OrganonStandTrajectory bestTrajectory = bestHeuristic.BestTrajectory;
                string linePrefix = bestTrajectory.Name + "," + bestHeuristic.GetName() + "," + bestTrajectory.GetHarvestYear() + "," + bestTrajectory.GetRotationLength();

                List<float> bestSolutions = this.Runs[runIndex].BestObjectiveFunctionByRun;
                for (int solutionIndex = 0; solutionIndex < bestSolutions.Count; ++solutionIndex)
                {
                    line.Clear();

                    string objectiveFunction = bestSolutions[solutionIndex].ToString(CultureInfo.InvariantCulture);
                    line.Append(linePrefix + "," + solutionIndex + "," + objectiveFunction);
                    writer.WriteLine(line);
                }
            }
        }
    }
}
