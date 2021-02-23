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
        public List<HeuristicSolutionDistribution>? Runs { get; set; }

        [Parameter(HelpMessage = "Number of iterations between CSV file lines. Default is 1, which logs every objective function value.")]
        [ValidateRange(1, Int32.MaxValue)]
        public int Step;

        public WriteObjective()
        {
            this.Step = 1;
        }

        protected override void ProcessRecord()
        {
            if (this.Runs!.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Runs));
            }

            using StreamWriter writer = this.GetWriter();

            // for now, perform no reduction when Object.ReferenceEquals(lowestSolution, highestSolution) is true
            StringBuilder line = new StringBuilder();
            if (this.ShouldWriteHeader())
            {
                if ((this.Runs[0].HighestSolution == null) || (this.Runs[0].HighestHeuristicParameters == null) || (this.Runs[0].LowestSolution == null))
                {
                    throw new NotSupportedException("Cannot generate header because first run is missing a highest solution, lowest solution, or highest solution parameters.");
                }

                line.Append("stand,heuristic,discount rate,thin age,rotation," + this.Runs[0].HighestHeuristicParameters!.GetCsvHeader() + ",iteration,count");

                string lowestMoveLogHeader = "lowest move log";
                IHeuristicMoveLog? lowestMoveLog = this.Runs[0].LowestSolution!.GetMoveLog();
                if (lowestMoveLog != null)
                {
                    lowestMoveLogHeader = lowestMoveLog.GetCsvHeader("lowest ");
                }
                line.Append("," + lowestMoveLogHeader);

                line.Append(",lowest,lowest candidate,min,percentile 2.5,percentile 5,lower quartile,median,mean,upper quartile,percentile 95,percentile 97.5,max");

                string highestMoveLogHeader = "highest move log";
                IHeuristicMoveLog? highestMoveLog = this.Runs[0].HighestSolution!.GetMoveLog();
                if (highestMoveLog != null)
                {
                    highestMoveLogHeader = highestMoveLog.GetCsvHeader("highest ");
                }
                line.Append("," + highestMoveLogHeader);

                line.Append(",highest,highest candidate");
                writer.WriteLine(line);
            }

            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                HeuristicSolutionDistribution distribution = this.Runs[runIndex];
                Heuristic? highestHeuristic = distribution.HighestSolution;
                Heuristic? lowestHeuristic = distribution.LowestSolution;
                if ((distribution.HighestHeuristicParameters == null) || (highestHeuristic == null) || (lowestHeuristic == null))
                {
                    throw new NotSupportedException("Solution distribution for run " + runIndex + " is missing a highest or lowest solution or highest solution parameters.");
                }
                IHeuristicMoveLog? highestMoveLog = highestHeuristic.GetMoveLog();
                IHeuristicMoveLog? lowestMoveLog = lowestHeuristic.GetMoveLog();
                // for now, assume highest and lowest solutions used the same parameters
                OrganonStandTrajectory highestTrajectory = highestHeuristic.BestTrajectory;
                string runPrefix = highestTrajectory.Name + "," + highestHeuristic.GetName() + "," + highestTrajectory.TimberValue.DiscountRate + "," + highestTrajectory.GetFirstHarvestAge() + "," + highestTrajectory.GetRotationLength() + "," + distribution.HighestHeuristicParameters.GetCsvValues();

                Debug.Assert(distribution.CountByMove.Count >= lowestHeuristic.AcceptedObjectiveFunctionByMove.Count);
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
                Debug.Assert(distribution.CountByMove.Count >= highestHeuristic.AcceptedObjectiveFunctionByMove.Count);
                for (int moveIndex = 0; moveIndex < distribution.CountByMove.Count; moveIndex += this.Step)
                {
                    line.Clear();

                    string runsWithMoveAtIndex = distribution.CountByMove[moveIndex].ToString(CultureInfo.InvariantCulture);

                    string? lowestMove = null;
                    if ((lowestMoveLog != null) && (lowestMoveLog.Count > moveIndex))
                    {
                        lowestMove = lowestMoveLog.GetCsvValues(moveIndex);
                    }
                    string? lowestObjectiveFunction = null;
                    if (lowestHeuristic.AcceptedObjectiveFunctionByMove.Count > moveIndex)
                    {
                        lowestObjectiveFunction = lowestHeuristic.AcceptedObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? lowestObjectiveFunctionForMove = null;
                    if (lowestHeuristic.CandidateObjectiveFunctionByMove.Count > moveIndex)
                    {
                        lowestObjectiveFunctionForMove = lowestHeuristic.CandidateObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    string minObjectiveFunction = distribution.MinimumObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    string? lowerQuartileObjectiveFunction = null;
                    if (moveIndex < distribution.LowerQuartileByMove.Count)
                    {
                        lowerQuartileObjectiveFunction = distribution.LowerQuartileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? twoPointFivePercentileObjectiveFunction = null;
                    if (moveIndex < distribution.TwoPointFivePercentileByMove.Count)
                    {
                        twoPointFivePercentileObjectiveFunction = distribution.TwoPointFivePercentileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? fifthPercentileObjectiveFunction = null;
                    if (moveIndex < distribution.FifthPercentileByMove.Count)
                    {
                        fifthPercentileObjectiveFunction = distribution.FifthPercentileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? medianObjectiveFunction = null;
                    if (moveIndex < distribution.MedianObjectiveFunctionByMove.Count)
                    {
                        medianObjectiveFunction = distribution.MedianObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string meanObjectiveFunction = distribution.MeanObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    string? upperQuartileObjectiveFunction = null;
                    if (moveIndex < distribution.UpperQuartileByMove.Count)
                    {
                        upperQuartileObjectiveFunction = distribution.UpperQuartileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? ninetyFifthPercentileObjectiveFunction = null;
                    if (moveIndex < distribution.NinetyFifthPercentileByMove.Count)
                    {
                        ninetyFifthPercentileObjectiveFunction = distribution.NinetyFifthPercentileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? ninetySevenPointFivePercentileObjectiveFunction = null;
                    if (moveIndex < distribution.NinetySevenPointFivePercentileByMove.Count)
                    {
                        ninetySevenPointFivePercentileObjectiveFunction = distribution.NinetySevenPointFivePercentileByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string maxObjectiveFunction = distribution.MaximumObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);

                    string? highestMove = null;
                    if ((highestMoveLog != null) && (highestMoveLog.Count > moveIndex))
                    {
                        highestMove = highestMoveLog.GetCsvValues(moveIndex);
                    }
                    string highestObjectiveFunction = String.Empty;
                    if (highestHeuristic.AcceptedObjectiveFunctionByMove.Count > moveIndex)
                    {
                        highestObjectiveFunction = highestHeuristic.AcceptedObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? highestObjectiveFunctionForMove = null;
                    if (highestHeuristic.CandidateObjectiveFunctionByMove.Count > moveIndex)
                    {
                        highestObjectiveFunctionForMove = highestHeuristic.CandidateObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    line.Append(runPrefix + "," + 
                                moveIndex + "," + 
                                runsWithMoveAtIndex + "," + 
                                lowestMove + "," +
                                lowestObjectiveFunction + "," +
                                lowestObjectiveFunctionForMove + "," +
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
                                highestMove + "," +
                                highestObjectiveFunction + "," +
                                highestObjectiveFunctionForMove);
                    writer.WriteLine(line);
                }
            }
        }
    }
}
