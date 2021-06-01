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
        public HeuristicResultSet? Results { get; set; }

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
                HeuristicDistribution distribution = this.Results.Distributions[0];
                HeuristicSolutionPool solution = this.Results.SolutionIndex[distribution];
                if ((solution.High == null) || (distribution.HeuristicParameters == null) || (solution.Low == null))
                {
                    throw new NotSupportedException("Cannot generate header because first result is missing a high solution, low solution, or heuristic parameters.");
                }

                StringBuilder line = new("stand,heuristic," + distribution.HeuristicParameters!.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",iteration,count");

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
                HeuristicDistribution distribution = this.Results.Distributions[resultIndex];
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
                HeuristicDistribution distribution = this.Results.Distributions[resultIndexToLog];
                HeuristicSolutionPool solution = this.Results.SolutionIndex[distribution];
                Heuristic? highHeuristic = solution.High;
                Heuristic? lowHeuristic = solution.Low;
                if ((distribution.HeuristicParameters == null) || (highHeuristic == null) || (lowHeuristic == null))
                {
                    throw new NotSupportedException("Result " + resultIndexToLog + " is missing a high or low solution or heuristic parameters.");
                }
                IHeuristicMoveLog? highMoveLog = highHeuristic.GetMoveLog();
                IHeuristicMoveLog? lowMoveLog = lowHeuristic.GetMoveLog();
                // for now, assume high and low solutions used the same parameters
                OrganonStandTrajectory highTrajectory = highHeuristic.BestTrajectory;

                float discountRate = this.Results.DiscountRates[distribution.DiscountRateIndex];
                string runPrefix = highTrajectory.Name + "," + 
                    highHeuristic.GetName() + "," +
                    distribution.HeuristicParameters.GetCsvValues() + "," +
                    WriteCmdlet.GetRateAndAgeCsvValues(highTrajectory, discountRate);

                Debug.Assert(distribution.CountByMove.Count >= lowHeuristic.AcceptedObjectiveFunctionByMove.Count);
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
                Debug.Assert(distribution.CountByMove.Count >= highHeuristic.AcceptedObjectiveFunctionByMove.Count);
                for (int moveIndex = 0; moveIndex < distribution.CountByMove.Count; moveIndex += this.Step)
                {
                    string runsWithMoveAtIndex = distribution.CountByMove[moveIndex].ToString(CultureInfo.InvariantCulture);

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

                    Debug.Assert(distribution.MaximumObjectiveFunctionByMove[moveIndex] >= lowHeuristic.AcceptedObjectiveFunctionByMove[moveIndex]);
                    Debug.Assert(distribution.MinimumObjectiveFunctionByMove[moveIndex] <= highHeuristic.AcceptedObjectiveFunctionByMove[moveIndex]);
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
