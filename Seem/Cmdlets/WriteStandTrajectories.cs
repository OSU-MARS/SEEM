using Mars.Seem.Optimization;
using Mars.Seem.Tree;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    /// <summary>
    /// Same parameters as <see cref="WriteSilviculturalTrajectories"/>.
    /// </summary>
    [Cmdlet(VerbsCommunications.Write, "StandTrajectories")]
    public class WriteStandTrajectories : WriteCmdlet
    {
        [Parameter]
        [ValidateRange(0.1F, 100.0F)]
        public float DiameterClassSize { get; set; } // cm

        [Parameter(HelpMessage = "Write only simulation timesteps where a harvest (thinning or regeneration) occurs.")]
        public SwitchParameter HarvestsOnly { get; set; }

        [Parameter]
        [ValidateNotNull]
        public FinancialScenarios FinancialScenarios { get; set; }

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

        [Parameter(Mandatory = true, HelpMessage = "List of stand trajectories to write.")]
        [ValidateNotNullOrEmpty]
        public List<StandTrajectory>? Trajectories { get; set; }

        public WriteStandTrajectories()
        {
            this.DiameterClassSize = Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters;
            this.FinancialScenarios = FinancialScenarios.Default;
            this.HarvestsOnly = false;
            this.MaximumDiameter = Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters;
            this.NoCarbon = false;
            this.NoEquipmentProductivity = false;
            this.NoFinancial = false;
            this.NoHarvestCosts = false;
            this.NoTimberSorts = false;
            this.NoTreeGrowth = false;
            this.Trajectories = null;
        }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Trajectories != null);

            WriteStandTrajectoryContext writeContext = new(this.FinancialScenarios, this.HarvestsOnly, this.NoTreeGrowth, this.NoFinancial, this.NoCarbon, this.NoHarvestCosts, this.NoTimberSorts, this.NoEquipmentProductivity, this.DiameterClassSize, this.MaximumDiameter);
            using StreamWriter writer = this.GetWriter();
            if (this.ShouldWriteHeader())
            {
                string header = WriteCmdlet.GetCsvHeaderForStandTrajectory("stand,financialScenario", writeContext);
                writer.WriteLine(header);
            }

            // write stand trajectories
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                StandTrajectory trajectory = this.Trajectories[trajectoryIndex];
                writeContext.EndOfRotationPeriodIndex = trajectory.PlanningPeriods - 1;
                for (int financialIndex = 0; financialIndex < this.FinancialScenarios.Count; ++financialIndex)
                {
                    writeContext.FinancialIndex = financialIndex;
                    string financialScenario = this.FinancialScenarios.Name[financialIndex];
                    writeContext.LinePrefix = trajectory.Name + "," + financialScenario;
                    estimatedBytesSinceLastFileLength += WriteCmdlet.WriteStandTrajectoryToCsv(writer, trajectory, writeContext);
                }

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
