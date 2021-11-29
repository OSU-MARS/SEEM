using Mars.Seem.Heuristics;
using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Objective")]
    public class WriteObjective : WriteTrajectoriesCmdlet
    {
        [Parameter(HelpMessage = "Number of iterations between CSV file lines. Default is 1, which logs objective function values for every move.")]
        [ValidateRange(1, Int32.MaxValue)]
        public int Step;

        public WriteObjective()
        {
            this.LimitGB = 1.5F; // approximate upper bound of what fread() can load in R is 3 GB
            this.Step = 1;
        }

        protected override void ProcessRecord()
        {
            if (this.Trajectories == null)
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories), "-" + nameof(this.Trajectories) + " must be specified.");
            }
            if (this.Trajectories.CoordinatesEvaluated.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories));
            }

            using StreamWriter writer = this.GetWriter();

            // for now, perform no reduction when Object.ReferenceEquals(lowSolution, highSolution) is true
            if (this.ShouldWriteHeader())
            {
                StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[0];
                SilviculturalPrescriptionPool prescriptions = this.Trajectories[coordinate].Pool;
                if ((prescriptions.High.Heuristic == null) || (prescriptions.Low.Heuristic == null))
                {
                    throw new NotSupportedException("Cannot generate header because first result is missing a high or low heuristic.");
                }
                StringBuilder line = new(WriteTrajectoriesCmdlet.GetHeuristicAndPositionCsvHeader(this.Trajectories) + ",move,count");

                string lowMoveLogHeader = "lowMoveLog";
                HeuristicMoveLog? lowMoveLog = prescriptions.Low.Heuristic.GetMoveLog();
                if (lowMoveLog != null)
                {
                    lowMoveLogHeader = lowMoveLog.GetCsvHeader("low");
                }
                line.Append("," + lowMoveLogHeader);

                line.Append(",low,lowCandidate,min,percentile2.5,percentile10,lowerQuartile,median,mean,upperQuartile,percentile90,percentile97.5,max");

                string highMoveLogHeader = "highMoveLog";
                HeuristicMoveLog? highMoveLog = prescriptions.High.Heuristic.GetMoveLog();
                if (highMoveLog != null)
                {
                    highMoveLogHeader = highMoveLog.GetCsvHeader("high");
                }
                line.Append("," + highMoveLogHeader);

                line.Append(",high,highCandidate");
                writer.WriteLine(line);
            }

            // sort each discount rate's runs by decreasing objective function value  
            List<List<(float, StandTrajectoryCoordinate)>> solutionsByFinancialIndexAndValue = new();
            for (int positionIndex = 0; positionIndex < this.Trajectories.CoordinatesEvaluated.Count; ++positionIndex)
            {
                StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[positionIndex];
                StandTrajectoryArrayElement element = this.Trajectories[coordinate];
                OptimizationObjectiveDistribution distribution = element.Distribution;
                int maxFinancialIndex = coordinate.FinancialIndex;
                while (maxFinancialIndex >= solutionsByFinancialIndexAndValue.Count)
                {
                    solutionsByFinancialIndexAndValue.Add(new());
                }

                List<(float, StandTrajectoryCoordinate)> runsForDiscountRate = solutionsByFinancialIndexAndValue[maxFinancialIndex];
                runsForDiscountRate.Add((element.Pool.High.FinancialValue, coordinate));
            }
            for (int financialIndex = 0; financialIndex < solutionsByFinancialIndexAndValue.Count; ++financialIndex)
            {
                List<(float Objective, StandTrajectoryCoordinate)> runsForDiscountRate = solutionsByFinancialIndexAndValue[financialIndex];
                runsForDiscountRate.Sort((run1, run2) => run2.Objective.CompareTo(run1.Objective)); // sort descending
            }

            // list runs in declining order of objective function value for logging
            // Runs are listed in groups with each discount rate having the ability to be represented within each group. This controls for reduction in
            // land expectation values with increasing discount rate, enabling preferentiall logging of the move histories for the most desirable runs.
            List<StandTrajectoryCoordinate> prioritizedCoordinates = new();
            int[] resultIndicesByFinancialScenario = new int[solutionsByFinancialIndexAndValue.Count];
            for (bool atLeastOneRunAddedByInnerLoop = true; atLeastOneRunAddedByInnerLoop; )
            {
                atLeastOneRunAddedByInnerLoop = false;
                for (int financialIndex = 0; financialIndex < solutionsByFinancialIndexAndValue.Count; ++financialIndex)
                {
                    List<(float Objective, StandTrajectoryCoordinate Coordinate)> runsForScenario = solutionsByFinancialIndexAndValue[financialIndex];
                    int resultIndexForScenario = resultIndicesByFinancialScenario[financialIndex];
                    if (resultIndexForScenario < runsForScenario.Count)
                    {
                        prioritizedCoordinates.Add(runsForScenario[resultIndexForScenario].Coordinate);
                        resultIndicesByFinancialScenario[financialIndex] = ++resultIndexForScenario;
                        atLeastOneRunAddedByInnerLoop = true;
                    }
                }
            }

            // log runs in declining priority order until either all runs are logged or the file size limit is reached
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int coordinateIndex = 0; coordinateIndex < prioritizedCoordinates.Count; ++coordinateIndex)
            {
                // since high and low solutions are from the same position, they use the same heuristic parameters
                StandTrajectoryCoordinate coordinate = prioritizedCoordinates[coordinateIndex];
                StandTrajectoryArrayElement element = this.Trajectories[coordinate];
                string linePrefix = this.GetPositionPrefix(coordinate);
                Heuristic highHeuristic = element.Pool.High.Heuristic!; // checked for null in GetHeuristicAndPositionCsvValues()
                Heuristic lowHeuristic = element.Pool.Low.Heuristic ?? throw new InvalidOperationException("Evaluated coordinate " + coordinateIndex + " does not have a low heuristic.");

                if (highHeuristic.RunParameters.LogOnlyImprovingMoves != lowHeuristic.RunParameters.LogOnlyImprovingMoves)
                {
                    throw new NotSupportedException("High and low heuristic move logs are mismatched. They must either both contain all moves or both contain only improving moves.");
                }

                IList<float> acceptedFinancialValueByMoveHigh = highHeuristic.FinancialValue.GetAcceptedValuesWithDefaulting(coordinate);
                IList<float> acceptedFinancialValueByMoveLow = lowHeuristic.FinancialValue.GetAcceptedValuesWithDefaulting(coordinate);
                IList<float> candidateFinancialValueByMoveHigh = highHeuristic.FinancialValue.GetCandidateValuesWithDefaulting(coordinate);
                IList<float> candidateFinancialValueByMoveLow = lowHeuristic.FinancialValue.GetCandidateValuesWithDefaulting(coordinate);
                if ((acceptedFinancialValueByMoveLow.Count != candidateFinancialValueByMoveLow.Count) ||
                    (acceptedFinancialValueByMoveHigh.Count != candidateFinancialValueByMoveHigh.Count))
                {
                    throw new NotSupportedException("Mismatch between accepted and candidate move lengths of high or low heuristic.");
                }
                int maximumMoveInFinancialDistribution = element.Distribution.GetMaximumMoveIndex();
                int maximumMoveIndex = Math.Max(Math.Max(acceptedFinancialValueByMoveLow.Count, acceptedFinancialValueByMoveHigh.Count), maximumMoveInFinancialDistribution);

                HeuristicMoveLog? highMoveLog = highHeuristic.GetMoveLog();
                HeuristicMoveLog? lowMoveLog = lowHeuristic.GetMoveLog();
                for (int moveIndex = 0; moveIndex < maximumMoveIndex; moveIndex += this.Step)
                {
                    string? lowMove = null;
                    int lowMoveNumber = moveIndex;
                    if (lowMoveLog != null)
                    {
                        lowMoveNumber = lowMoveLog.GetMoveNumberWithDefaulting(coordinate, moveIndex);
                        lowMove = lowMoveLog.GetCsvValues(coordinate, lowMoveNumber);
                    }
                    string? lowObjectiveFunction = null;
                    if (acceptedFinancialValueByMoveLow.Count > moveIndex)
                    {
                        lowObjectiveFunction = acceptedFinancialValueByMoveLow[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? lowObjectiveFunctionCandidate = null;
                    if (candidateFinancialValueByMoveLow.Count > moveIndex)
                    {
                        lowObjectiveFunctionCandidate = candidateFinancialValueByMoveLow[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    string? runsWithMoveAtIndex = null;
                    string? minFinancialValue = null;
                    string? twoPointFivePercentileFinancialValue = null;
                    string? fifthPercentileFinancialValue = null;
                    string? lowerQuartileFinancialValue = null;
                    string? medianFinancialValue = null;
                    string? meanFinancialValue = null;
                    string? upperQuartileFinancialValue = null;
                    string? ninetyFifthPercentileFinancialValue = null;
                    string? ninetySevenPointFivePercentileFinancialValue = null;
                    string? maxFinancialValue = null;
                    if (maximumMoveInFinancialDistribution > moveIndex)
                    {
                        DistributionStatistics financialStatistics = element.Distribution.GetFinancialStatisticsForMove(moveIndex);
                        runsWithMoveAtIndex = financialStatistics.Count.ToString(CultureInfo.InvariantCulture);
                        minFinancialValue = financialStatistics.Minimum.ToString(CultureInfo.InvariantCulture);
                        if (financialStatistics.TwoPointFivePercentile.HasValue)
                        {
                            twoPointFivePercentileFinancialValue = financialStatistics.TwoPointFivePercentile.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        if (financialStatistics.TenthPercentile.HasValue)
                        {
                            fifthPercentileFinancialValue = financialStatistics.TenthPercentile.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        if (financialStatistics.LowerQuartile.HasValue)
                        {
                            lowerQuartileFinancialValue = financialStatistics.LowerQuartile.Value.ToString(CultureInfo.InvariantCulture);
                        }

                        medianFinancialValue = financialStatistics.Median.ToString(CultureInfo.InvariantCulture);
                        meanFinancialValue = financialStatistics.Mean.ToString(CultureInfo.InvariantCulture);
                        if (financialStatistics.UpperQuartile.HasValue)
                        {
                            upperQuartileFinancialValue = financialStatistics.UpperQuartile.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        if (financialStatistics.NinetiethPercentile.HasValue)
                        {
                            ninetyFifthPercentileFinancialValue = financialStatistics.NinetiethPercentile.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        if (financialStatistics.NinetySevenPointFivePercentile.HasValue)
                        {
                            ninetySevenPointFivePercentileFinancialValue = financialStatistics.NinetySevenPointFivePercentile.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        maxFinancialValue = financialStatistics.Maximum.ToString(CultureInfo.InvariantCulture);

                        Debug.Assert((acceptedFinancialValueByMoveLow.Count <= moveIndex) || (financialStatistics.Maximum >= acceptedFinancialValueByMoveLow[moveIndex]));
                        Debug.Assert((acceptedFinancialValueByMoveHigh.Count <= moveIndex) || (financialStatistics.Minimum <= acceptedFinancialValueByMoveHigh[moveIndex]));
                    }

                    string? highMove = null;
                    int highMoveNumber = moveIndex;
                    if (highMoveLog != null)
                    {
                        highMoveNumber = highMoveLog.GetMoveNumberWithDefaulting(coordinate, moveIndex);
                        highMove = highMoveLog!.GetCsvValues(coordinate, highMoveNumber);
                    }
                    string? highFinancialValue = null;
                    if (acceptedFinancialValueByMoveHigh.Count > moveIndex)
                    {
                        highFinancialValue = acceptedFinancialValueByMoveHigh[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }
                    string? highFinancialCandidate = null;
                    if (candidateFinancialValueByMoveHigh.Count > moveIndex)
                    {
                        highFinancialCandidate = candidateFinancialValueByMoveHigh[moveIndex].ToString(CultureInfo.InvariantCulture);
                    }

                    if (lowMoveNumber != highMoveNumber)
                    {
                        throw new NotSupportedException("Low move number " + lowMoveNumber + " does not match high move number " + highMoveNumber + ".");
                    }
                    string moveNumber;
                    if (highMoveNumber != moveIndex)
                    {
                        moveNumber = highMoveNumber.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        moveNumber = moveIndex.ToString(CultureInfo.InvariantCulture);
                    }

                    writer.WriteLine(linePrefix + "," +
                                     moveNumber + "," +
                                     runsWithMoveAtIndex + "," +
                                     lowMove + "," +
                                     lowObjectiveFunction + "," +
                                     lowObjectiveFunctionCandidate + "," +
                                     minFinancialValue + "," +
                                     twoPointFivePercentileFinancialValue + "," +
                                     fifthPercentileFinancialValue + "," +
                                     lowerQuartileFinancialValue + "," +
                                     medianFinancialValue + "," +
                                     meanFinancialValue + "," +
                                     upperQuartileFinancialValue + "," +
                                     ninetyFifthPercentileFinancialValue + "," +
                                     ninetySevenPointFivePercentileFinancialValue + "," +
                                     maxFinancialValue + "," +
                                     highMove + "," +
                                     highFinancialValue + "," +
                                     highFinancialCandidate);
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
