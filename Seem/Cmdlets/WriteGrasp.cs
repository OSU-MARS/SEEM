using Mars.Seem.Heuristics;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Grasp")]
    public class WriteGrasp : WriteCmdlet
    {
        [Parameter]
        [ValidateNotNull]
        public HeuristicStandTrajectories? Results { get; set; }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Results != null);

            using StreamWriter writer = this.GetWriter();

            // header
            if (this.ShouldWriteHeader())
            {
                writer.WriteLine("stand,alpha,selected,rejected");
            }

            // rows for bin counts
            StandTrajectoryCoordinate coordinate = this.Results.CoordinatesEvaluated[0];
            SilviculturalPrescriptionPool prescriptions = this.Results[coordinate].Pool;

            StandTrajectory? highTrajectory = prescriptions.High.Trajectory;
            if (highTrajectory == null)
            {
                throw new NotSupportedException("First coordinate evaluated is missing a high heuristic or trajectory.");
            }

            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0; 
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int binIndex = 0; binIndex < this.Results.GraspReactivity.SelectionHistogram.Length; ++binIndex)
            {
                string binCenter = (this.Results.GraspReactivity.GreedinessBinWidth * (binIndex + 0.5F)).ToString(CultureInfo.InvariantCulture);
                string selected = this.Results.GraspReactivity.SelectionHistogram[binIndex].ToString(CultureInfo.InvariantCulture);
                string rejected = this.Results.GraspReactivity.RejectionHistogram[binIndex].ToString(CultureInfo.InvariantCulture);

                string line = highTrajectory.Name + "," +
                    binCenter + "," +
                    selected + "," +
                    rejected;
                writer.WriteLine(line);
                estimatedBytesSinceLastFileLength += line.Length;
            }

            if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
            {
                // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                knownFileSizeInBytes = writer.BaseStream.Length;
                estimatedBytesSinceLastFileLength = 0;
            }
            if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
            {
                this.WriteWarning("Write-SnagsAndLogs: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
            }
        }
    }
}
