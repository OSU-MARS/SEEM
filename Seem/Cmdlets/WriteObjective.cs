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
        [Parameter(HelpMessage = "Maximum size of output file in gigabytes.")]
        [ValidateRange(0.1F, 100.0F)]
        public float MaxFileSize { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public HeuristicResultSet? Results { get; set; }

        [Parameter(HelpMessage = "Number of iterations between CSV file lines. Default is 1, which logs every objective function value.")]
        [ValidateRange(1, Int32.MaxValue)]
        public int Step;

        public WriteObjective()
        {
            this.MaxFileSize = 5.0F; // 5 GB, approximate upper bound of what fread() can load in R
            this.Step = 1;
        }

        protected override void ProcessRecord()
        {
            if (this.Results!.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Results));
            }

            using StreamWriter writer = this.GetWriter();

            // for now, perform no reduction when Object.ReferenceEquals(lowestSolution, highestSolution) is true
            if (this.ShouldWriteHeader())
            {
                HeuristicDistribution distribution = this.Results.Distributions[0];
                HeuristicSolutionPool solution = this.Results.Solutions[0];
                if ((solution.Highest == null) || (distribution.HeuristicParameters == null) || (solution.Lowest == null))
                {
                    throw new NotSupportedException("Cannot generate header because first result is missing a highest solution, lowest solution, or heuristic parameters.");
                }

                StringBuilder line = new("stand,heuristic," + distribution.HeuristicParameters!.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",iteration,count");

                string lowestMoveLogHeader = "lowest move log";
                IHeuristicMoveLog? lowestMoveLog = solution.Lowest!.GetMoveLog();
                if (lowestMoveLog != null)
                {
                    lowestMoveLogHeader = lowestMoveLog.GetCsvHeader("lowest ");
                }
                line.Append("," + lowestMoveLogHeader);

                line.Append(",lowest,lowest candidate,min,percentile 2.5,percentile 5,lower quartile,median,mean,upper quartile,percentile 95,percentile 97.5,max");

                string highestMoveLogHeader = "highest move log";
                IHeuristicMoveLog? highestMoveLog = solution.Highest!.GetMoveLog();
                if (highestMoveLog != null)
                {
                    highestMoveLogHeader = highestMoveLog.GetCsvHeader("highest ");
                }
                line.Append("," + highestMoveLogHeader);

                line.Append(",highest,highest candidate");
                writer.WriteLine(line);
            }

            // sort each iscount rate's runs by decreasing objective function value  
            List<List<(float Objective, int Index)>> solutionsByDiscountRateIndexAndObjective = new();
            for (int resultIndex = 0; resultIndex < this.Results.Count; ++resultIndex)
            {
                Heuristic? highestHeuristic = this.Results.Solutions[resultIndex].Highest;
                if (highestHeuristic == null)
                {
                    throw new NotSupportedException("Result " + resultIndex + " is missing a highest solution.");
                }

                HeuristicDistribution distribution = this.Results.Distributions[resultIndex];
                int discountRateIndex = distribution.DiscountRateIndex;
                while (discountRateIndex >= solutionsByDiscountRateIndexAndObjective.Count)
                {
                    solutionsByDiscountRateIndexAndObjective.Add(new());
                }

                List<(float Objective, int Index)> runsForDiscountRate = solutionsByDiscountRateIndexAndObjective[discountRateIndex];
                runsForDiscountRate.Add((highestHeuristic.BestObjectiveFunction, resultIndex));
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
            long maxFileSizeInBytes = (long)(1E9F * this.MaxFileSize);
            foreach (int resultIndexToLog in prioritizedResultIndices)
            {
                HeuristicDistribution distribution = this.Results.Distributions[resultIndexToLog];
                HeuristicSolutionPool solution = this.Results.Solutions[resultIndexToLog];
                Heuristic? highestHeuristic = solution.Highest;
                Heuristic? lowestHeuristic = solution.Lowest;
                if ((distribution.HeuristicParameters == null) || (highestHeuristic == null) || (lowestHeuristic == null))
                {
                    throw new NotSupportedException("Result " + resultIndexToLog + " is missing a highest or lowest solution or highest solution parameters.");
                }
                IHeuristicMoveLog? highestMoveLog = highestHeuristic.GetMoveLog();
                IHeuristicMoveLog? lowestMoveLog = lowestHeuristic.GetMoveLog();
                // for now, assume highest and lowest solutions used the same parameters
                OrganonStandTrajectory highestTrajectory = highestHeuristic.BestTrajectory;

                string runPrefix = highestTrajectory.Name + "," + 
                    highestHeuristic.GetName() + "," +
                    distribution.HeuristicParameters.GetCsvValues() + "," +
                    WriteCmdlet.GetRateAndAgeCsvValues(highestTrajectory);

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

                    writer.WriteLine(runPrefix + "," + 
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
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Maximum file size of " + this.MaxFileSize.ToString("0.00") + " GB reached.");
                    break;
                }
            }
        }
    }
}
