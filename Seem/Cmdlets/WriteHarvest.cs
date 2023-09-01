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
            this.ValidateParameters();

            using StreamWriter writer = this.CreateCsvWriter();

            StringBuilder line = new();
            if (this.ShouldWriteCsvHeader())
            {
                line.Append("period");
                SilviculturalSpace defaultSilviculturalSpace = this.Trajectories[0];

                // harvest volume headers
                for (int coordinateIndex = 0; coordinateIndex < defaultSilviculturalSpace.CoordinatesEvaluated.Count; ++coordinateIndex)
                {
                    SilviculturalCoordinate coordinate = defaultSilviculturalSpace.CoordinatesEvaluated[coordinateIndex];
                    StandTrajectory highTrajectory = defaultSilviculturalSpace.GetHighTrajectory(coordinate);
                    line.Append("," + highTrajectory.Name + "harvest");
                }

                // standing volume headers
                for (int resultIndex = 0; resultIndex < defaultSilviculturalSpace.CoordinatesEvaluated.Count; ++resultIndex)
                {
                    SilviculturalCoordinate coordinate = defaultSilviculturalSpace.CoordinatesEvaluated[resultIndex];
                    StandTrajectory highTrajectory = defaultSilviculturalSpace.GetHighTrajectory(coordinate);
                    line.Append("," + highTrajectory.Name + "standing");
                }
                writer.WriteLine(line);
            }

            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[trajectoryIndex];

                int maxPlanningPeriod = silviculturalSpace.RotationLengths.Max();
                for (int periodIndex = 0; periodIndex < maxPlanningPeriod; ++periodIndex)
                {
                    line.Clear();
                    line.Append(periodIndex);

                    foreach (SilviculturalCoordinate coordinate in silviculturalSpace.CoordinatesEvaluated)
                    {
                        StandTrajectory highTrajectory = silviculturalSpace.GetHighTrajectory(coordinate);
                        float thinVolumeScribner = highTrajectory.GetTotalScribnerVolumeThinned(periodIndex);
                        line.Append("," + thinVolumeScribner.ToString(CultureInfo.InvariantCulture));
                    }

                    foreach (SilviculturalCoordinate coordinate in silviculturalSpace.CoordinatesEvaluated)
                    {
                        StandTrajectory highTrajectory = silviculturalSpace.GetHighTrajectory(coordinate);
                        float regenVolumeScribner = highTrajectory.GetTotalRegenerationHarvestMerchantableScribnerVolume(periodIndex);
                        line.Append("," + regenVolumeScribner.ToString(CultureInfo.InvariantCulture));
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
}
