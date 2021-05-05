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
                for (int resultIndex = 0; resultIndex < this.Results.Count; ++resultIndex)
                {
                    Heuristic? highestSolution = this.Results.Solutions[resultIndex].Highest;
                    if (highestSolution == null)
                    {
                        throw new NotSupportedException("Cannot write harvest becaue no heuristic solution was provided for run " + resultIndex + ".");
                    }

                    OrganonStandTrajectory bestTrajectory = highestSolution.BestTrajectory;
                    line.Append("," + bestTrajectory.Name + "harvest");
                }
                // standing volume headers
                for (int runIndex = 0; runIndex < this.Results.Count; ++runIndex)
                {
                    OrganonStandTrajectory bestTrajectory = this.Results.Solutions[runIndex].Highest!.BestTrajectory;
                    line.Append("," + bestTrajectory.Name + "standing");
                }
                writer.WriteLine(line);
            }

            int maxPlanningPeriod = 0;
            for (int runIndex = 0; runIndex < this.Results!.Count; ++runIndex)
            {
                Heuristic? highestSolution = this.Results.Solutions[runIndex].Highest;
                if (highestSolution == null)
                {
                    throw new NotSupportedException("Cannot write harvest becaue no heuristic solution was provided for run " + runIndex + ".");
                }

                OrganonStandTrajectory bestTrajectory = highestSolution.BestTrajectory;
                maxPlanningPeriod = Math.Max(maxPlanningPeriod, bestTrajectory.PlanningPeriods);
            }
            for (int periodIndex = 0; periodIndex < maxPlanningPeriod; ++periodIndex)
            {
                line.Clear();
                line.Append(periodIndex);

                for (int runIndex = 0; runIndex < this.Results.Count; ++runIndex)
                {
                    Heuristic heuristic = this.Results.Solutions[runIndex].Highest!;
                    float harvestVolumeScibner = heuristic.BestTrajectory.ThinningVolume.ScribnerTotal[periodIndex];
                    line.Append("," + harvestVolumeScibner.ToString(CultureInfo.InvariantCulture));
                }

                for (int runIndex = 0; runIndex < this.Results.Count; ++runIndex)
                {
                    Heuristic heuristic = this.Results.Solutions[runIndex].Highest!;
                    float standingVolumeScribner = heuristic.BestTrajectory.StandingVolume.ScribnerTotal[periodIndex];
                    line.Append("," + standingVolumeScribner.ToString(CultureInfo.InvariantCulture));
                }

                writer.WriteLine(line);
            }
        }
    }
}
