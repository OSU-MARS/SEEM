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
        public List<HeuristicSolutionDistribution>? Runs { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Runs!.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Runs));
            }

            using StreamWriter writer = this.GetWriter();

            StringBuilder line = new();
            if (this.ShouldWriteHeader())
            {
                HeuristicParameters? highestParameters = this.Runs[0].HighestHeuristicParameters;
                if (highestParameters == null)
                {
                    throw new NotSupportedException("Cannot generate header because first run is missing highest solution parameters.");
                }

                line.Append("stand,heuristic," + highestParameters.GetCsvHeader() + ",discount rate,first thin,second thin,rotation,solution,objective,runtime");
                writer.WriteLine(line);
            }

            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                HeuristicSolutionDistribution distribution = this.Runs[runIndex];
                Heuristic? highestHeuristic = distribution.HighestSolution;
                if ((highestHeuristic == null) || (distribution.HighestHeuristicParameters == null))
                {
                    throw new NotSupportedException("Run " + runIndex + " is missing a highest solution or highest solution parameters.");
                }
                OrganonStandTrajectory highestTrajectory = highestHeuristic.BestTrajectory;

                int firstThinAge = highestTrajectory.GetFirstHarvestAge();
                string? firstThinAgeString = firstThinAge != -1 ? firstThinAge.ToString(CultureInfo.InvariantCulture) : null;
                int secondThinAge = highestTrajectory.GetSecondHarvestAge();
                string? secondThinAgeString = secondThinAge != -1 ? secondThinAge.ToString(CultureInfo.InvariantCulture) : null;
                string linePrefix = highestTrajectory.Name + "," + highestHeuristic.GetName() + "," + 
                    distribution.HighestHeuristicParameters.GetCsvValues() + "," + 
                    highestTrajectory.TimberValue.DiscountRate.ToString(CultureInfo.InvariantCulture) + "," +
                    firstThinAgeString + "," +
                    secondThinAgeString + "," +
                    highestTrajectory.GetRotationLength().ToString(CultureInfo.InvariantCulture);

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
