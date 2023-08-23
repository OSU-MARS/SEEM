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
    public class WriteObjective : WriteSilviculturalTrajectoriesCmdlet
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
            this.ValidateParameters();

            using StreamWriter writer = this.CreateCsvWriter();

            // for now, perform no reduction when Object.ReferenceEquals(lowSolution, highSolution) is true
            if (this.ShouldWriteCsvHeader())
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[0];
                SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[0];
                SilviculturalPrescriptionPool prescriptions = silviculturalSpace[coordinate].Pool;
                if ((prescriptions.High.Heuristic == null) || (prescriptions.Low.Heuristic == null))
                {
                    throw new NotSupportedException("Cannot generate csv header because first result is missing a high or low heuristic.");
                }
                StringBuilder line = new(this.GetCsvHeaderForSilviculturalCoordinate() + ",move,count");

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

            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[trajectoryIndex];

                // sort each discount rate's runs by decreasing objective function value  
                List<List<(float, SilviculturalCoordinate)>> solutionsByFinancialIndexAndValue = new();
                for (int positionIndex = 0; positionIndex < silviculturalSpace.CoordinatesEvaluated.Count; ++positionIndex)
                {
                    SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[positionIndex];
                    SilviculturalCoordinateExploration exploration = silviculturalSpace[coordinate];
                    OptimizationObjectiveDistribution distribution = exploration.Distribution;
                    int maxFinancialIndex = coordinate.FinancialIndex;
                    while (maxFinancialIndex >= solutionsByFinancialIndexAndValue.Count)
                    {
                        solutionsByFinancialIndexAndValue.Add(new());
                    }

                    List<(float, SilviculturalCoordinate)> runsForDiscountRate = solutionsByFinancialIndexAndValue[maxFinancialIndex];
                    runsForDiscountRate.Add((exploration.Pool.High.FinancialValue, coordinate));
                }
                for (int financialIndex = 0; financialIndex < solutionsByFinancialIndexAndValue.Count; ++financialIndex)
                {
                    List<(float Objective, SilviculturalCoordinate)> runsForDiscountRate = solutionsByFinancialIndexAndValue[financialIndex];
                    runsForDiscountRate.Sort((run1, run2) => run2.Objective.CompareTo(run1.Objective)); // sort descending
                }

                // list runs in declining order of objective function value for logging
                // Runs are listed in groups with each discount rate having the ability to be represented within each group. This controls for reduction in
                // land expectation values with increasing discount rate, enabling preferentiall logging of the move histories for the most desirable runs.
                List<SilviculturalCoordinate> prioritizedCoordinates = new();
                int[] resultIndicesByFinancialScenario = new int[solutionsByFinancialIndexAndValue.Count];
                for (bool atLeastOneRunAddedByInnerLoop = true; atLeastOneRunAddedByInnerLoop;)
                {
                    atLeastOneRunAddedByInnerLoop = false;
                    for (int financialIndex = 0; financialIndex < solutionsByFinancialIndexAndValue.Count; ++financialIndex)
                    {
                        List<(float Objective, SilviculturalCoordinate Coordinate)> runsForScenario = solutionsByFinancialIndexAndValue[financialIndex];
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
                for (int coordinateIndex = 0; coordinateIndex < prioritizedCoordinates.Count; ++coordinateIndex)
                {
                    // since high and low solutions are from the same position, they use the same heuristic parameters
                    SilviculturalCoordinate coordinate = prioritizedCoordinates[coordinateIndex];
                    SilviculturalCoordinateExploration exploration = silviculturalSpace[coordinate];
                    string linePrefix = this.GetCsvPrefixForCoordinate(silviculturalSpace, coordinate);
                    Heuristic highHeuristic = exploration.Pool.High.Heuristic!; // checked for null in GetHeuristicAndPositionCsvValues()
                    Heuristic lowHeuristic = exploration.Pool.Low.Heuristic ?? throw new InvalidOperationException("Evaluated coordinate " + coordinateIndex + " does not have a low heuristic.");

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
                    int maximumMoveInFinancialDistribution = exploration.Distribution.GetMaximumMoveIndex();
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
                            DistributionStatistics financialStatistics = exploration.Distribution.GetFinancialStatisticsForMove(moveIndex);
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

                        string line = linePrefix + "," +
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
                            highFinancialCandidate;
                        writer.WriteLine(line);
                        estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;
                    }

                    if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                    {
                        // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                        knownFileSizeInBytes = writer.BaseStream.Length;
                        estimatedBytesSinceLastFileLength = 0;
                    }
                    if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                    {
                        this.WriteWarning("Write-Objective: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                        break;
                    }
                }
            }
        }
    }
}
