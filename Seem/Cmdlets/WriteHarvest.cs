using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Harvest")]
    public class WriteHarvest : WriteHeuristicResultsCmdlet
    {
        protected override void ProcessRecord()
        {
            Debug.Assert(this.Results != null);

            using StreamWriter writer = this.GetWriter();

            StringBuilder line = new();
            if (this.ShouldWriteHeader())
            {
                line.Append("period");
                // harvest volume headers
                for (int resultIndex = 0; resultIndex < this.Results.CombinationsEvaluated.Count; ++resultIndex)
                {
                    HeuristicResultPosition position = this.Results.CombinationsEvaluated[resultIndex];
                    Heuristic? highHeuristic = this.Results[position].Pool.High;
                    if (highHeuristic == null)
                    {
                        throw new NotSupportedException("Cannot write harvest becaue no heuristic solution was provided for run " + resultIndex + ".");
                    }

                    
                    OrganonStandTrajectory bestTrajectory = highHeuristic.GetBestTrajectoryWithDefaulting(position);
                    line.Append("," + bestTrajectory.Name + "harvest");
                }
                // standing volume headers
                for (int resultIndex = 0; resultIndex < this.Results.CombinationsEvaluated.Count; ++resultIndex)
                {
                    HeuristicResultPosition position = this.Results.CombinationsEvaluated[resultIndex];
                    OrganonStandTrajectory bestTrajectory = this.Results[position].Pool.High!.GetBestTrajectoryWithDefaulting(position);
                    line.Append("," + bestTrajectory.Name + "standing");
                }
                writer.WriteLine(line);
            }

            int maxPlanningPeriod = this.Results.RotationLengths.Max();
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int periodIndex = 0; periodIndex < maxPlanningPeriod; ++periodIndex)
            {
                line.Clear();
                line.Append(periodIndex);

                foreach (HeuristicResultPosition position in this.Results.CombinationsEvaluated)
                {
                    Heuristic highHeuristic = this.Results[position].Pool.High!;
                    float harvestVolumeScibner = highHeuristic.GetBestTrajectoryWithDefaulting(position).ThinningVolume.GetScribnerTotal(periodIndex);
                    line.Append("," + harvestVolumeScibner.ToString(CultureInfo.InvariantCulture));
                }

                foreach (HeuristicResultPosition position in this.Results.CombinationsEvaluated)
                {
                    Heuristic highHeuristic = this.Results[position].Pool.High!;
                    float standingVolumeScribner = highHeuristic.GetBestTrajectoryWithDefaulting(position).StandingVolume.GetScribnerTotal(periodIndex);
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
