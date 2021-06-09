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
            if (this.Results.Distributions.Count < 1)
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
            for (int resultIndex = 0; resultIndex < this.Results.Distributions.Count; ++resultIndex)
            {
                HeuristicObjectiveDistribution distribution = this.Results.Distributions[resultIndex];
                Heuristic? highHeuristic = this.Results.SolutionIndex[distribution].High;
                if (highHeuristic == null)
                {
                    throw new NotSupportedException("Run " + resultIndex + " is missing a high solution.");
                }
                OrganonStandTrajectory highTrajectory = highHeuristic.BestTrajectory;

                float discountRate = this.Results.DiscountRates[distribution.DiscountRateIndex];
                string linePrefix = highTrajectory.Name + "," + 
                                    highHeuristic.GetName() + "," +
                                    highHeuristic.GetParameters().GetCsvValues() + "," +
                                    WriteCmdlet.GetRateAndAgeCsvValues(highTrajectory, discountRate);

                List<float> bestSolutions = distribution.BestObjectiveFunctionBySolution;
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
            }
        }
    }
}
