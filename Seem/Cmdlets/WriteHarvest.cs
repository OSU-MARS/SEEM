using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Harvest")]
    public class WriteHarvest : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public HeuristicResultSet? Results { get; set; }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Results != null);

            using StreamWriter writer = this.GetWriter();

            StringBuilder line = new();
            if (this.ShouldWriteHeader())
            {
                line.Append("period");
                // harvest volume headers
                for (int resultIndex = 0; resultIndex < this.Results.Distributions.Count; ++resultIndex)
                {
                    Heuristic? highHeuristic = this.Results.SolutionIndex[this.Results.Distributions[resultIndex]].High;
                    if (highHeuristic == null)
                    {
                        throw new NotSupportedException("Cannot write harvest becaue no heuristic solution was provided for run " + resultIndex + ".");
                    }

                    OrganonStandTrajectory bestTrajectory = highHeuristic.BestTrajectory;
                    line.Append("," + bestTrajectory.Name + "harvest");
                }
                // standing volume headers
                for (int runIndex = 0; runIndex < this.Results.Distributions.Count; ++runIndex)
                {
                    OrganonStandTrajectory bestTrajectory = this.Results.SolutionIndex[this.Results.Distributions[runIndex]].High!.BestTrajectory;
                    line.Append("," + bestTrajectory.Name + "standing");
                }
                writer.WriteLine(line);
            }

            int maxPlanningPeriod = 0;
            for (int runIndex = 0; runIndex < this.Results!.Distributions.Count; ++runIndex)
            {
                Heuristic? highHeuristic = this.Results.SolutionIndex[this.Results.Distributions[runIndex]].High;
                if (highHeuristic == null)
                {
                    throw new NotSupportedException("Cannot write harvest becaue no heuristic solution was provided for run " + runIndex + ".");
                }

                OrganonStandTrajectory bestTrajectory = highHeuristic.BestTrajectory;
                maxPlanningPeriod = Math.Max(maxPlanningPeriod, bestTrajectory.PlanningPeriods);
            }

            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int periodIndex = 0; periodIndex < maxPlanningPeriod; ++periodIndex)
            {
                line.Clear();
                line.Append(periodIndex);

                for (int runIndex = 0; runIndex < this.Results.Distributions.Count; ++runIndex)
                {
                    Heuristic highHeuristic = this.Results.SolutionIndex[this.Results.Distributions[runIndex]].High!;
                    float harvestVolumeScibner = highHeuristic.BestTrajectory.ThinningVolume.GetScribnerTotal(periodIndex);
                    line.Append("," + harvestVolumeScibner.ToString(CultureInfo.InvariantCulture));
                }

                for (int runIndex = 0; runIndex < this.Results.Distributions.Count; ++runIndex)
                {
                    Heuristic highHeuristic = this.Results.SolutionIndex[this.Results.Distributions[runIndex]].High!;
                    float standingVolumeScribner = highHeuristic.BestTrajectory.StandingVolume.GetScribnerTotal(periodIndex);
                    line.Append("," + standingVolumeScribner.ToString(CultureInfo.InvariantCulture));
                }

                writer.WriteLine(line);

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-Harvest: Maximum file size of " + this.LimitGB.ToString("0.00") + " GB reached.");
                    break;
                }
            }
        }
    }
}
