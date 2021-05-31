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
    public class WriteObjectiveDistribution : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public HeuristicResultSet? Results { get; set; }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Results != null);
            if (this.Results.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Results));
            }

            using StreamWriter writer = this.GetWriter();

            if (this.ShouldWriteHeader())
            {
                HeuristicParameters? heuristicParameters = this.Results.Distributions[0].HeuristicParameters;
                if (heuristicParameters == null)
                {
                    throw new NotSupportedException("Cannot generate header because first result is missing heuristic parameters.");
                }

                writer.WriteLine("stand,heuristic," + heuristicParameters.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",solution,objective,moveAccepted,movesRejected,runtime,timesteps");
            }

            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int resultIndex = 0; resultIndex < this.Results.Count; ++resultIndex)
            {
                HeuristicDistribution distribution = this.Results.Distributions[resultIndex];
                Heuristic? highHeuristic = this.Results.Solutions[resultIndex].High;
                if ((highHeuristic == null) || (distribution.HeuristicParameters == null))
                {
                    throw new NotSupportedException("Run " + resultIndex + " is missing a high solution or high heuristic parameters.");
                }
                OrganonStandTrajectory highTrajectory = highHeuristic.BestTrajectory;

                float discountRate = this.Results.DiscountRates[distribution.DiscountRateIndex];
                string linePrefix = highTrajectory.Name + "," + 
                                    highHeuristic.GetName() + "," + 
                                    distribution.HeuristicParameters.GetCsvValues() + "," +
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
                                     perfCounters.GrowthModelTimesteps.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }
}
