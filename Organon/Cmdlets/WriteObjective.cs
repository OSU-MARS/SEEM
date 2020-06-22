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
            if (this.Runs.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Runs));
            }

            using StreamWriter writer = this.GetWriter();

            StringBuilder line = new StringBuilder();
            if (this.ShouldWriteHeader())
            {
                line.Append("stand,heuristic," + this.Runs[0].HighestHeuristicParameters.GetCsvHeader() + ",thin age,rotation,iteration,count,lowest,min,percentile 2.5,percentile 5,lower quartile,median,mean,upper quartile,percentile 95,percentile 97.5,max,highest");
                writer.WriteLine(line);
            }

            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                HeuristicSolutionDistribution distribution = this.Runs[runIndex];
                Heuristic highestHeuristic = distribution.HighestSolution;
                Heuristic lowestHeuristic = distribution.LowestSolution;
                OrganonStandTrajectory highestTrajectory = highestHeuristic.BestTrajectory;
                string linePrefix = highestTrajectory.Name + "," + highestHeuristic.GetName() + "," + distribution.HighestHeuristicParameters.GetCsvValues() + "," + highestTrajectory.GetFirstHarvestAge() + "," + highestTrajectory.GetRotationLength();

                Debug.Assert(distribution.CountByMove.Count >= lowestHeuristic.ObjectiveFunctionByMove.Count);
                Debug.Assert(distribution.CountByMove.Count == distribution.MinimumObjectiveFunctionByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= distribution.TwoPointFivePercentileByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= distribution.FifthPercentileByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= distribution.LowerQuartileByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= distribution.MedianObjectiveFunctionByMove.Count);
                Debug.Assert(distribution.CountByMove.Count == distribution.MeanObjectiveFunctionByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= distribution.NinetyFifthPercentileByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= distribution.NinetySevenPointFivePercentileByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= distribution.UpperQuartileByMove.Count);
                Debug.Assert(distribution.CountByMove.Count == distribution.MaximumObjectiveFunctionByMove.Count);
                Debug.Assert(distribution.CountByMove.Count >= highestHeuristic.ObjectiveFunctionByMove.Count);
                for (int moveIndex = 0; moveIndex < distribution.CountByMove.Count; moveIndex += this.Step)
                {
                    line.Clear();

                    string moves = distribution.CountByMove[moveIndex].ToString(CultureInfo.InvariantCulture);

                    string lowestObjectiveFunction = String.Empty;
                    if (lowestHeuristic.ObjectiveFunctionByMove.Count > moveIndex)
                    {
                        lowestObjectiveFunction = lowestHeuristic.ObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    string minObjectiveFunction = distribution.MinimumObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    string lowerQuartileObjectiveFunction = null;
                    if (moveIndex < distribution.LowerQuartileByMove.Count)
                    {
                        lowerQuartileObjectiveFunction = distribution.LowerQuartileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string twoPointFivePercentileObjectiveFunction = null;
                    if (moveIndex < distribution.TwoPointFivePercentileByMove.Count)
                    {
                        twoPointFivePercentileObjectiveFunction = distribution.TwoPointFivePercentileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string fifthPercentileObjectiveFunction = null;
                    if (moveIndex < distribution.FifthPercentileByMove.Count)
                    {
                        fifthPercentileObjectiveFunction = distribution.FifthPercentileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string medianObjectiveFunction = null;
                    if (moveIndex < distribution.MedianObjectiveFunctionByMove.Count)
                    {
                        medianObjectiveFunction = distribution.MedianObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string meanObjectiveFunction = distribution.MeanObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    string upperQuartileObjectiveFunction = null;
                    if (moveIndex < distribution.UpperQuartileByMove.Count)
                    {
                        upperQuartileObjectiveFunction = distribution.UpperQuartileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string ninetyFifthPercentileObjectiveFunction = null;
                    if (moveIndex < distribution.NinetyFifthPercentileByMove.Count)
                    {
                        ninetyFifthPercentileObjectiveFunction = distribution.NinetyFifthPercentileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string ninetySevenPointFivePercentileObjectiveFunction = null;
                    if (moveIndex < distribution.NinetySevenPointFivePercentileByMove.Count)
                    {
                        ninetySevenPointFivePercentileObjectiveFunction = distribution.NinetySevenPointFivePercentileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string maxObjectiveFunction = distribution.MaximumObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    
                    string highestObjectiveFunction = String.Empty;
                    if (highestHeuristic.ObjectiveFunctionByMove.Count > moveIndex)
                    {
                        highestObjectiveFunction = highestHeuristic.ObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    line.Append(linePrefix + "," + 
                                moveIndex + "," + 
                                moves + "," + 
                                lowestObjectiveFunction + "," +
                                minObjectiveFunction + "," + 
                                twoPointFivePercentileObjectiveFunction + "," +
                                fifthPercentileObjectiveFunction + "," +
                                lowerQuartileObjectiveFunction + "," + 
                                medianObjectiveFunction + "," + 
                                meanObjectiveFunction + "," + 
                                upperQuartileObjectiveFunction + "," +
                                ninetyFifthPercentileObjectiveFunction + "," +
                                ninetySevenPointFivePercentileObjectiveFunction + "," +
                                maxObjectiveFunction + "," + 
                                highestObjectiveFunction);
                    writer.WriteLine(line);
                }
            }
        }
    }
}
