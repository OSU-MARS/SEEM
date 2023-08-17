using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Harvest")]
    public class WriteHarvest : WriteSilviculturalTrajectoriesCmdlet
    {
        protected override void ProcessRecord()
        {
            if (this.Trajectories == null)
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories), "-" + nameof(this.Trajectories) + " must be specified.");
            }

            using StreamWriter writer = this.GetCsvWriter();

            StringBuilder line = new();
            if (this.ShouldWriteCsvHeader())
            {
                line.Append("period");
                // harvest volume headers
                for (int coordinateIndex = 0; coordinateIndex < this.Trajectories.CoordinatesEvaluated.Count; ++coordinateIndex)
                {
                    SilviculturalCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[coordinateIndex];
                    StandTrajectory? trajectory = this.Trajectories[coordinate].Pool.High.Trajectory;
                    if (trajectory == null)
                    {
                        throw new NotSupportedException("Cannot write harvest becaue no high trajectory is present at evaluated coordinate index " + coordinateIndex + ".");
                    }

                    line.Append("," + trajectory.Name + "harvest");
                }

                // standing volume headers
                for (int resultIndex = 0; resultIndex < this.Trajectories.CoordinatesEvaluated.Count; ++resultIndex)
                {
                    SilviculturalCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[resultIndex];
                    StandTrajectory? trajectory = this.Trajectories[coordinate].Pool.High.Trajectory!; // checked in previous loop
                    line.Append("," + trajectory.Name + "standing");
                }
                writer.WriteLine(line);
            }

            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            int maxPlanningPeriod = this.Trajectories.RotationLengths.Max();
            for (int periodIndex = 0; periodIndex < maxPlanningPeriod; ++periodIndex)
            {
                line.Clear();
                line.Append(periodIndex);

                foreach (SilviculturalCoordinate coordinate in this.Trajectories.CoordinatesEvaluated)
                {
                    StandTrajectory? trajectory = this.Trajectories[coordinate].Pool.High.Trajectory!; // checked loops above
                    float harvestVolumeScibner = trajectory.GetTotalScribnerVolumeThinned(periodIndex);
                    line.Append("," + harvestVolumeScibner.ToString(CultureInfo.InvariantCulture));
                }

                foreach (SilviculturalCoordinate coordinate in this.Trajectories.CoordinatesEvaluated)
                {
                    StandTrajectory? trajectory = this.Trajectories[coordinate].Pool.High.Trajectory!; // checked loops above
                    float standingVolumeScribner = trajectory.GetTotalStandingScribnerVolume(periodIndex);
                    line.Append("," + standingVolumeScribner.ToString(CultureInfo.InvariantCulture));
                }

                writer.WriteLine(line);
                estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;

                if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                {
                    // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                    knownFileSizeInBytes = writer.BaseStream.Length;
                    estimatedBytesSinceLastFileLength = 0;
                }
                if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-Harvest: Maximum file size of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB reached.");
                    break;
                }
            }
        }
    }
}
