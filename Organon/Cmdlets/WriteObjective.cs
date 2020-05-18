using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Objective")]
    public class WriteObjective : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution> Runs { get; set; }

        [Parameter(HelpMessage = "Number of iterations between CSV file lines. Default is 1, which logs every objective function value.")]
        [ValidateRange(1, Int32.MaxValue)]
        public int Step;

        public WriteObjective()
        {
            this.Step = 1;
        }

        protected override void ProcessRecord()
        {
            using StreamWriter writer = this.GetWriter();

            StringBuilder line = new StringBuilder();
            if (this.ShouldWriteHeader())
            {
                line.Append("stand,heuristic,default selection probability,thin age,rotation,iteration,count,min,mean,max,best");
                writer.WriteLine(line);
            }

            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                HeuristicSolutionDistribution distribution = this.Runs[runIndex];
                Heuristic bestHeuristic = distribution.BestSolution;
                OrganonStandTrajectory bestTrajectory = bestHeuristic.BestTrajectory;
                string linePrefix = bestTrajectory.Name + "," + bestHeuristic.GetName() + "," + distribution.DefaultSelectionProbability.ToString(Constant.DefaultSelectionFormat, CultureInfo.InvariantCulture) + "," + bestTrajectory.GetHarvestAge() + "," + bestTrajectory.GetRotationLength();

                Debug.Assert(distribution.CountByMove.Count == distribution.MinimumObjectiveFunctionByMove.Count);
                Debug.Assert(distribution.CountByMove.Count == distribution.MeanObjectiveFunctionByMove.Count);
                Debug.Assert(distribution.CountByMove.Count == distribution.MaximumObjectiveFunctionByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= bestHeuristic.ObjectiveFunctionByMove.Count);
                for (int moveIndex = 0; moveIndex < distribution.CountByMove.Count; moveIndex += this.Step)
                {
                    line.Clear();

                    string moves = distribution.CountByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    string minObjectiveFunction = distribution.MinimumObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    string meanObjectiveFunction = distribution.MeanObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    string maxObjectiveFunction = distribution.MaximumObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    string bestObjectiveFunction = String.Empty;
                    if (bestHeuristic.ObjectiveFunctionByMove.Count > moveIndex)
                    {
                        bestObjectiveFunction = bestHeuristic.ObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    line.Append(linePrefix + "," + moveIndex + "," + moves + "," + minObjectiveFunction + "," + meanObjectiveFunction + "," + maxObjectiveFunction + "," + bestObjectiveFunction);
                    writer.WriteLine(line);
                }
            }
        }
    }
}
