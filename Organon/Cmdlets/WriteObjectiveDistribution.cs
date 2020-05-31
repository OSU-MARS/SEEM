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
    public class WriteObjectiveDistribution : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution> Runs { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Runs.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Runs));
            }

            using StreamWriter writer = this.GetWriter();

            StringBuilder line = new StringBuilder();
            if (this.ShouldWriteHeader())
            {
                line.Append("stand,heuristic," + this.Runs[0].HeuristicParameters.GetCsvHeader() + ",thin age,rotation,solution,objective,runtime");
                writer.WriteLine(line);
            }

            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                HeuristicSolutionDistribution distribution = this.Runs[runIndex];
                Heuristic bestHeuristic = distribution.BestSolution;
                OrganonStandTrajectory bestTrajectory = bestHeuristic.BestTrajectory;
                string linePrefix = bestTrajectory.Name + "," + bestHeuristic.GetName() + "," + distribution.HeuristicParameters.GetCsvValues() + "," + bestTrajectory.GetFirstHarvestAge() + "," + bestTrajectory.GetRotationLength();

                List<float> bestSolutions = distribution.BestObjectiveFunctionBySolution;
                for (int solutionIndex = 0; solutionIndex < bestSolutions.Count; ++solutionIndex)
                {
                    line.Clear();

                    string objectiveFunction = bestSolutions[solutionIndex].ToString(CultureInfo.InvariantCulture);
                    string runtime = distribution.RuntimeBySolution[solutionIndex].TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                    line.Append(linePrefix + "," + solutionIndex + "," + objectiveFunction + "," + runtime);
                    writer.WriteLine(line);
                }
            }
        }
    }
}
