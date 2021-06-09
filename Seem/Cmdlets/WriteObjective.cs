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
    public class WriteObjective : WriteHeuristicResultsCmdlet
    {
        [Parameter(HelpMessage = "Number of iterations between CSV file lines. Default is 1, which logs every objective function value.")]
        [ValidateRange(1, Int32.MaxValue)]
        public int Step;

        public WriteObjective()
        {
            this.LimitGB = 3.0F; // approximate upper bound of what fread() can load in R
            this.Step = 1;
        }

        protected override void ProcessRecord()
        {
            if (this.Results!.Distributions.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Results));
            }

            using StreamWriter writer = this.GetWriter();

            // for now, perform no reduction when Object.ReferenceEquals(lowSolution, highSolution) is true
            if (this.ShouldWriteHeader())
            {
                HeuristicObjectiveDistribution distribution = this.Results.Distributions[0];
                HeuristicSolutionPool solution = this.Results.SolutionIndex[distribution];
                if ((solution.High == null) || (solution.Low == null))
                {
                    throw new NotSupportedException("Cannot generate header because first result is missing a high solution or a low solution.");
                }
                HeuristicParameters highParameters = solution.High.GetParameters();
                StringBuilder line = new("stand,heuristic," + highParameters.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",iteration,count");

                string lowMoveLogHeader = "lowMoveLog";
                IHeuristicMoveLog? lowMoveLog = solution.Low!.GetMoveLog();
                if (lowMoveLog != null)
                {
                    lowMoveLogHeader = lowMoveLog.GetCsvHeader("low");
                }
                line.Append("," + lowMoveLogHeader);

                line.Append(",low,lowCandidate,min,percentile2.5,percentile5,lowerQuartile,median,mean,upperQuartile,percentile95,percentile97.5,max");

                string highMoveLogHeader = "highMoveLog";
                IHeuristicMoveLog? highMoveLog = solution.High!.GetMoveLog();
                if (highMoveLog != null)
                {
                    highMoveLogHeader = highMoveLog.GetCsvHeader("high");
                }
                line.Append("," + highMoveLogHeader);

                line.Append(",high,highCandidate");
                writer.WriteLine(line);
            }

            // sort each iscount rate's runs by decreasing objective function value  
            List<List<(float Objective, int Index)>> solutionsByDiscountRateIndexAndObjective = new();
            for (int resultIndex = 0; resultIndex < this.Results.Distributions.Count; ++resultIndex)
            {
                HeuristicObjectiveDistribution distribution = this.Results.Distributions[resultIndex];
                Heuristic? highHeuristic = this.Results.SolutionIndex[distribution].High;
                if (highHeuristic == null)
                {
                    throw new NotSupportedException("Result " + resultIndex + " is missing a high solution.");
                }

                int discountRateIndex = distribution.DiscountRateIndex;
                while (discountRateIndex >= solutionsByDiscountRateIndexAndObjective.Count)
                {
                    solutionsByDiscountRateIndexAndObjective.Add(new());
                }

                List<(float Objective, int Index)> runsForDiscountRate = solutionsByDiscountRateIndexAndObjective[discountRateIndex];
                runsForDiscountRate.Add((highHeuristic.BestObjectiveFunction, resultIndex));
            }
            for (int discountRateIndex = 0; discountRateIndex < solutionsByDiscountRateIndexAndObjective.Count; ++discountRateIndex)
            {
                List<(float Objective, int Index)> runsForDiscountRate = solutionsByDiscountRateIndexAndObjective[discountRateIndex];
                runsForDiscountRate.Sort((run1, run2) => run2.Objective.CompareTo(run1.Objective)); // descending
            }

            // list runs in declining order of objective function value for logging
            // Runs are listed in groups with each discount rate having the ability to be represented within each group. This controls for reduction in
            // land expectation values with increasing discount rate, enabling preferentiall logging of the move histories for the most desirable runs.
            List<int> prioritizedResultIndices = new();
            int[] resultIndicesByDiscountRate = new int[solutionsByDiscountRateIndexAndObjective.Count];
            for (bool atLeastOneRunAddedByInnerLoop = true; atLeastOneRunAddedByInnerLoop; )
            {
                atLeastOneRunAddedByInnerLoop = false;
                for (int discountRateIndex = 0; discountRateIndex < solutionsByDiscountRateIndexAndObjective.Count; ++discountRateIndex)
                {
                    List<(float Objective, int Index)> runsForDiscountRate = solutionsByDiscountRateIndexAndObjective[discountRateIndex];
                    int resultIndexForDiscountRate = resultIndicesByDiscountRate[discountRateIndex];
                    if (resultIndexForDiscountRate < runsForDiscountRate.Count)
                    {
                        prioritizedResultIndices.Add(runsForDiscountRate[resultIndexForDiscountRate].Index);
                        resultIndicesByDiscountRate[discountRateIndex] = ++resultIndexForDiscountRate;
                        atLeastOneRunAddedByInnerLoop = true;
                    }
                }
            }

            // log runs in declining priority order until either all runs are logged or the file size limit is reached
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            foreach (int resultIndexToLog in prioritizedResultIndices)
            {
                HeuristicObjectiveDistribution distribution = this.Results.Distributions[resultIndexToLog];
                HeuristicSolutionPool solution = this.Results.SolutionIndex[distribution];
                Heuristic? highHeuristic = solution.High;
                Heuristic? lowHeuristic = solution.Low;
                if ((highHeuristic == null) || (lowHeuristic == null))
                {
                    throw new NotSupportedException("Result " + resultIndexToLog + " is missing a high or low solution.");
                }

                float discountRate = this.Results.DiscountRates[distribution.DiscountRateIndex];
                HeuristicParameters highParameters = highHeuristic.GetParameters();
                OrganonStandTrajectory highTrajectory = highHeuristic.BestTrajectory;
                string runPrefix = highTrajectory.Name + "," + 
                    highHeuristic.GetName() + "," +
                    highParameters.GetCsvValues() + "," +
                    WriteCmdlet.GetRateAndAgeCsvValues(highTrajectory, discountRate);

                // for now, assume high and low solutions used the same parameters
                IHeuristicMoveLog? highMoveLog = highHeuristic.GetMoveLog();
                IHeuristicMoveLog? lowMoveLog = lowHeuristic.GetMoveLog();
                int maximumMoves = distribution.GetMaximumMoves();
                Debug.Assert(maximumMoves >= lowHeuristic.AcceptedObjectiveFunctionByMove.Count);
                Debug.Assert(maximumMoves >= highHeuristic.AcceptedObjectiveFunctionByMove.Count);
                for (int moveIndex = 0; moveIndex < maximumMoves; moveIndex += this.Step)
                {
                    string? lowMove = null;
                    if ((lowMoveLog != null) && (lowMoveLog.Count > moveIndex))
                    {
                        lowMove = lowMoveLog.GetCsvValues(moveIndex);
                    }
                    string? lowObjectiveFunction = null;
                    if (lowHeuristic.AcceptedObjectiveFunctionByMove.Count > moveIndex)
                    {
                        lowObjectiveFunction = lowHeuristic.AcceptedObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? lowObjectiveFunctionForMove = null;
                    if (lowHeuristic.CandidateObjectiveFunctionByMove.Count > moveIndex)
                    {
                        lowObjectiveFunctionForMove = lowHeuristic.CandidateObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    Statistics moveStatistics = distribution.GetObjectiveFunctionStatisticsForMove(moveIndex);
                    string runsWithMoveAtIndex = moveStatistics.Count.ToString(CultureInfo.InvariantCulture);
                    string minObjectiveFunction = moveStatistics.Minimum.ToString(CultureInfo.InvariantCulture);
                    string? lowerQuartileObjectiveFunction = null;
                    if (moveStatistics.LowerQuartile != null)
                    {
                        lowerQuartileObjectiveFunction = moveStatistics.LowerQuartile.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    string? twoPointFivePercentileObjectiveFunction = null;
                    if (moveStatistics.TwoPointFivePercentile != null)
                    {
                        twoPointFivePercentileObjectiveFunction = moveStatistics.TwoPointFivePercentile.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    string? fifthPercentileObjectiveFunction = null;
                    if (moveStatistics.FifthPercentile != null)
                    {
                        fifthPercentileObjectiveFunction = moveStatistics.FifthPercentile.Value.ToString(CultureInfo.InvariantCulture);
                    }

                    string medianObjectiveFunction = moveStatistics.Median.ToString(CultureInfo.InvariantCulture);
                    string meanObjectiveFunction = moveStatistics.Mean.ToString(CultureInfo.InvariantCulture);
                    string? upperQuartileObjectiveFunction = null;
                    if (moveStatistics.UpperQuartile != null)
                    {
                        upperQuartileObjectiveFunction = moveStatistics.UpperQuartile.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    string? ninetyFifthPercentileObjectiveFunction = null;
                    if (moveStatistics.NinetyFifthPercentile != null)
                    {
                        ninetyFifthPercentileObjectiveFunction = moveStatistics.NinetyFifthPercentile.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    string? ninetySevenPointFivePercentileObjectiveFunction = null;
                    if (moveStatistics.NinetySevenPointFivePercentile != null)
                    {
                        ninetySevenPointFivePercentileObjectiveFunction = moveStatistics.NinetySevenPointFivePercentile.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    string maxObjectiveFunction = moveStatistics.Maximum.ToString(CultureInfo.InvariantCulture);

                    string? highMove = null;
                    if ((highMoveLog != null) && (highMoveLog.Count > moveIndex))
                    {
                        highMove = highMoveLog.GetCsvValues(moveIndex);
                    }
                    string highObjectiveFunction = String.Empty;
                    if (highHeuristic.AcceptedObjectiveFunctionByMove.Count > moveIndex)
                    {
                        highObjectiveFunction = highHeuristic.AcceptedObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? highObjectiveFunctionForMove = null;
                    if (highHeuristic.CandidateObjectiveFunctionByMove.Count > moveIndex)
                    {
                        highObjectiveFunctionForMove = highHeuristic.CandidateObjectiveFunctionByMove[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    Debug.Assert(moveStatistics.Maximum >= lowHeuristic.AcceptedObjectiveFunctionByMove[moveIndex]);
                    Debug.Assert(moveStatistics.Minimum <= highHeuristic.AcceptedObjectiveFunctionByMove[moveIndex]);
                    writer.WriteLine(runPrefix + "," + 
                                     moveIndex + "," + 
                                     runsWithMoveAtIndex + "," + 
                                     lowMove + "," +
                                     lowObjectiveFunction + "," +
                                     lowObjectiveFunctionForMove + "," +
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
                                     highMove + "," +
                                     highObjectiveFunction + "," +
                                     highObjectiveFunctionForMove);
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-Objective: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
