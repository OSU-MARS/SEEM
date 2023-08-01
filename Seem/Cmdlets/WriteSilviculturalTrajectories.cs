using Mars.Seem.Tree;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    /// <summary>
    /// Same parameters as <see cref="WriteStandTrajectories"/>.
    /// </summary>
    [Cmdlet(VerbsCommunications.Write, "SilviculuralTrajectories")]
    public class WriteSilviculturalTrajectories : WriteSilviculturalTrajectoriesCmdlet
    {
        [Parameter]
        [ValidateRange(0.1F, 100.0F)]
        public float DiameterClassSize { get; set; } // cm

        [Parameter(HelpMessage = "Write only simulation timesteps where a harvest (thinning or regeneration) occurs.")]
        public SwitchParameter HarvestsOnly { get; set; }

        [Parameter]
        [ValidateRange(1.0F, 1000.0F)]
        public float MaximumDiameter { get; set; } // cm

        [Parameter(HelpMessage = "Exclude biomass and snag columns from output. A substantial computational savings results from switching off snag decay calculations.")]
        public SwitchParameter NoCarbon { get; set; }

        [Parameter(HelpMessage = "Exclude equipment produtivity columns (PMh₀ and merchantable m³/PMh₀) from output.")]
        public SwitchParameter NoEquipmentProductivity { get; set; }

        [Parameter(HelpMessage = "Exclude NPV and LEV columns from output.")]
        public SwitchParameter NoFinancial { get; set; }

        [Parameter(HelpMessage = "Exclude harvest cost columns from output.")]
        public SwitchParameter NoHarvestCosts { get; set; }

        [Parameter(HelpMessage = "Exclude columns for 2S, 3S, and 4S logs, merchantable m³, Scribner MBF, and point value from output.")]
        public SwitchParameter NoTimberSorts { get; set; }

        [Parameter(HelpMessage = "Exclude columns for TPH, QMD, top height, basal area, SDI, and merchantable wood volume from output.")]
        public SwitchParameter NoTreeGrowth { get; set; }

        public WriteSilviculturalTrajectories()
        {
            this.DiameterClassSize = Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters;
            this.HarvestsOnly = false;
            this.MaximumDiameter = Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters;
            this.NoCarbon = false;
            this.NoEquipmentProductivity = false;
            this.NoFinancial = false;
            this.NoHarvestCosts = false;
            this.NoTimberSorts = false;
            this.NoTreeGrowth = false;
        }

        protected override void ProcessRecord()
        {
            // this.DiameterClassSize and MaximumDiameter are checked by PowerShell
            this.ValidateParameters();
            Debug.Assert(this.Trajectories != null);

            WriteStandTrajectoryContext writeContext = new(this.Trajectories.FinancialScenarios, this.HarvestsOnly, this.NoTreeGrowth, this.NoFinancial, this.NoCarbon, this.NoHarvestCosts, this.NoTimberSorts, this.NoEquipmentProductivity, this.DiameterClassSize, this.MaximumDiameter);
            using StreamWriter writer = this.GetWriter();
            if (this.ShouldWriteHeader())
            {
                string header = WriteCmdlet.GetCsvHeaderForStandTrajectory(this.GetCsvHeaderForSilviculturalCoordinate(), writeContext);
                writer.WriteLine(header);
            }

            // write stand trajectory with highest objective function value at each silvicultural coordinate
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            int maxCoordinateIndex = this.GetMaxCoordinateIndex();
            for (int coordinateIndex = 0; coordinateIndex < maxCoordinateIndex; ++coordinateIndex)
            {
                StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(coordinateIndex, out string linePrefix, out int endOfRotationPeriod, out int financialIndex);
                writeContext.EndOfRotationPeriodIndex = endOfRotationPeriod;
                writeContext.FinancialIndex = financialIndex;
                writeContext.LinePrefix = linePrefix;
                estimatedBytesSinceLastFileLength += WriteCmdlet.WriteStandTrajectoryToCsv(writer, highTrajectory, writeContext);

                if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                {
                    // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                    knownFileSizeInBytes = writer.BaseStream.Length;
                    estimatedBytesSinceLastFileLength = 0;
                }
                if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-StandTrajectory: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
