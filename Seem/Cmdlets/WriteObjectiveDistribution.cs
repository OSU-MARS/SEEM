using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "ObjectiveDistribution")]
    public class WriteObjectiveDistribution : WriteHeuristicResultsCmdlet
    {
        protected override void ProcessRecord()
        {
            Debug.Assert(this.Results != null);
            if (this.Results.CombinationsEvaluated.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Results));
            }

            using StreamWriter writer = this.GetWriter();

            if (this.ShouldWriteHeader())
            {
                HeuristicParameters heuristicParameters = WriteCmdlet.GetFirstHeuristicParameters(this.Results);
                writer.WriteLine("stand,heuristic," + heuristicParameters.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",solution,objective,moveAccepted,movesRejected,runtime,timesteps,treesRandomized");
            }

            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int positionIndex = 0; positionIndex < this.Results.CombinationsEvaluated.Count; ++positionIndex)
            {
                HeuristicResultPosition position = this.Results.CombinationsEvaluated[positionIndex];
                HeuristicResult result = this.Results[position];
                Heuristic? highHeuristic = result.Pool.High;
                if (highHeuristic == null)
                {
                    throw new NotSupportedException("Result at position " + positionIndex + " is missing a high solution.");
                }
                OrganonStandTrajectory highTrajectory = highHeuristic.BestTrajectory;

                int endPeriodIndex = this.Results.PlanningPeriods[position.PlanningPeriodIndex];
                float discountRate = this.Results.DiscountRates[position.DiscountRateIndex];
                string linePrefix = highTrajectory.Name + "," +
                                    highHeuristic.GetName() + "," +
                                    highHeuristic.GetParameters().GetCsvValues() + "," +
                                    WriteCmdlet.GetRateAndAgeCsvValues(highTrajectory, endPeriodIndex, discountRate);

                HeuristicObjectiveDistribution distribution = result.Distribution;
                List<float> bestSolutions = distribution.HighestFinancialValueBySolution;
                for (int solutionIndex = 0; solutionIndex < bestSolutions.Count; ++solutionIndex)
                {
                    HeuristicPerformanceCounters perfCounters = distribution.PerfCountersBySolution[solutionIndex];
                    writer.WriteLine(linePrefix + "," +
                                     solutionIndex.ToString(CultureInfo.InvariantCulture) + "," +
                                     bestSolutions[solutionIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                     perfCounters.MovesAccepted.ToString(CultureInfo.InvariantCulture) + "," +
                                     perfCounters.MovesRejected.ToString(CultureInfo.InvariantCulture) + "," +
                                     perfCounters.Duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     perfCounters.GrowthModelTimesteps.ToString(CultureInfo.InvariantCulture) + "," +
                                     perfCounters.TreesRandomizedInConstruction.ToString(CultureInfo.InvariantCulture));
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-ObjectiveDistribution: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
