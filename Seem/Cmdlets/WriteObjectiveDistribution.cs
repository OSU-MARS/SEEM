using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "ObjectiveDistribution")]
    public class WriteObjectiveDistribution : WriteTrajectoriesCmdlet
    {
        protected override void ProcessRecord()
        {
            if (this.Trajectories == null)
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories), "-" + nameof(this.Trajectories) + " must be specified.");
            }
            if (this.Trajectories.CoordinatesEvaluated.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories));
            }

            using StreamWriter writer = this.GetWriter();

            if (this.ShouldWriteHeader())
            {
                writer.WriteLine(WriteTrajectoriesCmdlet.GetHeuristicAndPositionCsvHeader(this.Trajectories) + ",solution,objective,movesAccepted,movesRejected,runtime,timesteps,treesRandomized");
            }

            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int coordinateIndex = 0; coordinateIndex < this.Trajectories.CoordinatesEvaluated.Count; ++coordinateIndex)
            {
                StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[coordinateIndex];
                StandTrajectoryArrayElement element = this.Trajectories[coordinate];
                StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(coordinateIndex, out string linePrefix);

                OptimizationObjectiveDistribution distribution = element.Distribution;
                List<float> highestFinancialValues = distribution.HighestFinancialValueBySolution;
                for (int solutionIndex = 0; solutionIndex < highestFinancialValues.Count; ++solutionIndex)
                {
                    PrescriptionPerformanceCounters perfCounters = distribution.PerfCountersBySolution[solutionIndex];
                    writer.WriteLine(linePrefix + "," +
                                     solutionIndex.ToString(CultureInfo.InvariantCulture) + "," +
                                     highestFinancialValues[solutionIndex].ToString(CultureInfo.InvariantCulture) + "," +
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
