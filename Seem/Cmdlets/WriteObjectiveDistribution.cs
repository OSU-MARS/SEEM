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
                HeuristicParameters? highestParameters = this.Results.Distributions[0].HeuristicParameters;
                if (highestParameters == null)
                {
                    throw new NotSupportedException("Cannot generate header because first result is missing highest solution parameters.");
                }

                writer.WriteLine("stand,heuristic," + highestParameters.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",solution,objective,accepted,rejected,runtime,timesteps");
            }

            for (int resultIndex = 0; resultIndex < this.Results.Count; ++resultIndex)
            {
                HeuristicDistribution distribution = this.Results.Distributions[resultIndex];
                Heuristic? highestHeuristic = this.Results.Solutions[resultIndex].Highest;
                if ((highestHeuristic == null) || (distribution.HeuristicParameters == null))
                {
                    throw new NotSupportedException("Run " + resultIndex + " is missing a highest solution or highest solution parameters.");
                }
                OrganonStandTrajectory highestTrajectory = highestHeuristic.BestTrajectory;

                float discountRate = this.Results.DiscountRates[distribution.DiscountRateIndex];
                string linePrefix = highestTrajectory.Name + "," + 
                                    highestHeuristic.GetName() + "," + 
                                    distribution.HeuristicParameters.GetCsvValues() + "," +
                                    WriteCmdlet.GetRateAndAgeCsvValues(highestTrajectory, discountRate);

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
