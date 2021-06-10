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
            if (this.Results!.CombinationsEvaluated.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Results));
            }

            using StreamWriter writer = this.GetWriter();

            // for now, perform no reduction when Object.ReferenceEquals(lowSolution, highSolution) is true
            if (this.ShouldWriteHeader())
            {
                HeuristicResultPosition position = this.Results.CombinationsEvaluated[0];
                HeuristicSolutionPool solution = this.Results[position].Pool;
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

            // sort each discount rate's runs by decreasing objective function value  
            List<List<(float, HeuristicResultPosition)>> solutionsByDiscountRateIndexAndObjective = new();
            for (int positionIndex = 0; positionIndex < this.Results.CombinationsEvaluated.Count; ++positionIndex)
            {
                HeuristicResultPosition position = this.Results.CombinationsEvaluated[positionIndex];
                HeuristicResult result = this.Results[position];
                HeuristicObjectiveDistribution distribution = result.Distribution;
                int maxDiscountRateIndex = position.DiscountRateIndex;
                while (maxDiscountRateIndex >= solutionsByDiscountRateIndexAndObjective.Count)
                {
                    solutionsByDiscountRateIndexAndObjective.Add(new());
                }

                Heuristic? highHeuristic = result.Pool.High;
                if (highHeuristic == null)
                {
                    throw new NotSupportedException("Result at position " + positionIndex + " is missing a high solution.");
                }

                List<(float, HeuristicResultPosition)> runsForDiscountRate = solutionsByDiscountRateIndexAndObjective[maxDiscountRateIndex];
                runsForDiscountRate.Add((highHeuristic.HighestFinancialValueByDiscountRate[maxDiscountRateIndex], position));
            }
            for (int discountRateIndex = 0; discountRateIndex < solutionsByDiscountRateIndexAndObjective.Count; ++discountRateIndex)
            {
                List<(float Objective, HeuristicResultPosition)> runsForDiscountRate = solutionsByDiscountRateIndexAndObjective[discountRateIndex];
                runsForDiscountRate.Sort((run1, run2) => run2.Objective.CompareTo(run1.Objective)); // sort descending
            }

            // list runs in declining order of objective function value for logging
            // Runs are listed in groups with each discount rate having the ability to be represented within each group. This controls for reduction in
            // land expectation values with increasing discount rate, enabling preferentiall logging of the move histories for the most desirable runs.
            List<HeuristicResultPosition> prioritizedResults = new();
            int[] resultIndicesByDiscountRate = new int[solutionsByDiscountRateIndexAndObjective.Count];
            for (bool atLeastOneRunAddedByInnerLoop = true; atLeastOneRunAddedByInnerLoop; )
            {
                atLeastOneRunAddedByInnerLoop = false;
                for (int discountRateIndex = 0; discountRateIndex < solutionsByDiscountRateIndexAndObjective.Count; ++discountRateIndex)
                {
                    List<(float Objective, HeuristicResultPosition Position)> runsForDiscountRate = solutionsByDiscountRateIndexAndObjective[discountRateIndex];
                    int resultIndexForDiscountRate = resultIndicesByDiscountRate[discountRateIndex];
                    if (resultIndexForDiscountRate < runsForDiscountRate.Count)
                    {
                        prioritizedResults.Add(runsForDiscountRate[resultIndexForDiscountRate].Position);
                        resultIndicesByDiscountRate[discountRateIndex] = ++resultIndexForDiscountRate;
                        atLeastOneRunAddedByInnerLoop = true;
                    }
                }
            }

            // log runs in declining priority order until either all runs are logged or the file size limit is reached
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int positionIndex = 0; positionIndex < prioritizedResults.Count; ++positionIndex)
            {
                HeuristicResultPosition position = prioritizedResults[positionIndex];
                HeuristicResult result = this.Results[position];
                Heuristic? highHeuristic = result.Pool.High;
                Heuristic? lowHeuristic = result.Pool.Low;
                if ((highHeuristic == null) || (lowHeuristic == null))
                {
                    throw new NotSupportedException("Result at position " + positionIndex + " is missing a high or low solution.");
                }

                int endPeriodIndex = this.Results.PlanningPeriods[position.PlanningPeriodIndex];
                float discountRate = this.Results.DiscountRates[position.DiscountRateIndex];
                HeuristicParameters highParameters = highHeuristic.GetParameters();
                OrganonStandTrajectory highTrajectory = highHeuristic.BestTrajectory;
                string runPrefix = highTrajectory.Name + "," +
                    highHeuristic.GetName() + "," +
                    highParameters.GetCsvValues() + "," +
                    WriteCmdlet.GetRateAndAgeCsvValues(highTrajectory, endPeriodIndex, discountRate);

                // solution distribution is informative and should be logged in this case
                // for now, assume high and low solutions use the same parameters
                List<float> acceptedFinancialValueByMoveLow = lowHeuristic.AcceptedFinancialValueByDiscountRateAndMove[Constant.HeuristicDefault.DiscountRateIndex];
                List<float> candidateFinancialValueByMoveLow = lowHeuristic.CandidateFinancialValueByDiscountRateAndMove[Constant.HeuristicDefault.DiscountRateIndex];
                if (lowHeuristic.AcceptedFinancialValueByDiscountRateAndMove.Count > 1)
                {
                    acceptedFinancialValueByMoveLow = lowHeuristic.AcceptedFinancialValueByDiscountRateAndMove[position.DiscountRateIndex];
                    candidateFinancialValueByMoveLow = lowHeuristic.CandidateFinancialValueByDiscountRateAndMove[position.DiscountRateIndex];
                }
                List<float> acceptedFinancialValueByMoveHigh = highHeuristic.AcceptedFinancialValueByDiscountRateAndMove[Constant.HeuristicDefault.DiscountRateIndex];
                List<float> candidateFinancialValueByMoveHigh = highHeuristic.CandidateFinancialValueByDiscountRateAndMove[Constant.HeuristicDefault.DiscountRateIndex];
                if (highHeuristic.AcceptedFinancialValueByDiscountRateAndMove.Count > 1)
                {
                    acceptedFinancialValueByMoveHigh = highHeuristic.AcceptedFinancialValueByDiscountRateAndMove[position.DiscountRateIndex];
                    candidateFinancialValueByMoveHigh = highHeuristic.CandidateFinancialValueByDiscountRateAndMove[position.DiscountRateIndex];
                }

                IHeuristicMoveLog? highMoveLog = highHeuristic.GetMoveLog();
                IHeuristicMoveLog? lowMoveLog = lowHeuristic.GetMoveLog();
                int maximumMoves = result.Distribution.GetMaximumMoves();
                Debug.Assert((maximumMoves >= acceptedFinancialValueByMoveLow.Count) && (maximumMoves >= candidateFinancialValueByMoveLow.Count));
                Debug.Assert((maximumMoves >= acceptedFinancialValueByMoveHigh.Count) && (maximumMoves >= candidateFinancialValueByMoveHigh.Count));
                for (int moveIndex = 0; moveIndex < maximumMoves; moveIndex += this.Step)
                {
                    string? lowMove = null;
                    if ((lowMoveLog != null) && (lowMoveLog.LengthInMoves > moveIndex))
                    {
                        lowMove = lowMoveLog.GetCsvValues(moveIndex);
                    }
                    string? lowObjectiveFunction = null;
                    if (acceptedFinancialValueByMoveLow.Count > moveIndex)
                    {
                        lowObjectiveFunction = acceptedFinancialValueByMoveLow[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? lowObjectiveFunctionForMove = null;
                    if (candidateFinancialValueByMoveLow.Count > moveIndex)
                    {
                        lowObjectiveFunctionForMove = candidateFinancialValueByMoveLow[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    Statistics moveStatistics = result.Distribution.GetObjectiveFunctionStatisticsForMove(moveIndex);
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
                    if ((highMoveLog != null) && (highMoveLog.LengthInMoves > moveIndex))
                    {
                        highMove = highMoveLog.GetCsvValues(moveIndex);
                    }
                    string highObjectiveFunction = String.Empty;
                    if (acceptedFinancialValueByMoveHigh.Count > moveIndex)
                    {
                        highObjectiveFunction = acceptedFinancialValueByMoveHigh[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? highObjectiveFunctionForMove = null;
                    if (candidateFinancialValueByMoveHigh.Count > moveIndex)
                    {
                        highObjectiveFunctionForMove = candidateFinancialValueByMoveHigh[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    Debug.Assert(moveStatistics.Maximum >= acceptedFinancialValueByMoveLow[moveIndex]);
                    Debug.Assert(moveStatistics.Minimum <= acceptedFinancialValueByMoveHigh[moveIndex]);
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
