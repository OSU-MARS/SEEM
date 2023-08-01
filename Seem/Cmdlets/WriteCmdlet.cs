using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Mars.Seem.Cmdlets
{
    public class WriteCmdlet : Cmdlet
    {
        // checking FileStream.Length becomes performance limiting when relatively low numbers of bytes are written per coordinate
        // This is most likely to occur when -HarvestsOnly and many -No switches are set to reduce the size of the output file. As a
        // mitigation, file size is estimated from the number of characters written and Length is called only periodicaly. It's assumed
        // all characters written are in the base UTF8 character set and therefore contribute one byte each to the file size. If this is
        // incorrect it's mostly likely incorrect only for the stand name and the resulting underprediction is assumed to be small enough
        // to be acceptable.
        protected const int StreamLengthSynchronizationInterval = 10 * 1000 * 1000; // 10 MB, as of .NET 5.0 checking every 1.0 MB is undesirably expensive

        private bool openedExistingFile;

        [Parameter]
        public SwitchParameter Append;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? CsvFile;

        [Parameter(HelpMessage = "Approximate upper bound of output file size in gigabytes.  This limit is loosely enforced and maximum file sizes will typically be somewhat larger.")]
        [ValidateRange(0.1F, 100.0F)]
        public float LimitGB { get; set; }

        public WriteCmdlet()
        {
            this.Append = false;
            this.LimitGB = 1.0F; // sanity default, may be set higher in derived classes
            this.openedExistingFile = false;
        }

        protected static string GetCsvHeaderForStandTrajectory(string prefix, WriteStandTrajectoryContext writeContext)
        {
            string header = prefix + ",standAge";
            if (writeContext.NoTreeGrowth == false)
            {
                header += ",TPH,QMD,Htop,BA,SDI,standingCmh,standingMbfh,thinCmh,thinMbfh,BAremoved,BAintensity,TPHdecrease";
            }
            if (writeContext.NoFinancial == false)
            {
                header += ",NPV,LEV";
            }
            if (writeContext.NoCarbon == false)
            {
                header += ",liveTreeBiomass,SPH,snagQmd";
            }
            if (writeContext.NoHarvestCosts == false)
            {
                header += ",thinWheeledHarvesterForwarderCost,thinTaskCost,regenMinCostSystem,regenFellerBuncherGrappleSwingYarderCost,regenFellerBuncherGrappleYoaderCost,regenTrackedHarvesterGrappleSwingYarderCost,regenTrackedHarvesterGrappleYoaderCost,regenWheeledHarvesterGrappleSwingYarderCost,regenWheeledHarvesterGrappleYoaderCost,regenTaskCost";
            }
            if (writeContext.NoTimberSorts == false)
            {
                header += ",thinLogs2S,thinLogs3S,thinLogs4S,thinCmh2S,thinCmh3S,thinCmh4S,thinMbfh2S,thinMbfh3S,thinMbfh4S,thinPond2S,thinPond3S,thinPond4S" +
                          ",standingLogs2S,standingLogs3S,standingLogs4S,standingCmh2S,standingCmh3S,standingCmh4S,standingMbfh2S,standingMbfh3S,standingMbfh4S,regenPond2S,regenPond3S,regenPond4S";
            }
            if (writeContext.NoEquipmentProductivity == false)
            {
                header += ",thinWheeledHarvesterPMh,thinWheeledHarvesterProductivity,thinChainsawCrewWithWheeledHarvester,thinChainsawCrewUtilizationWithWheeledHarvester,thinChainsawCmh,thinChainsawPMhWithWheeledHarvester" +
                          ",thinForwardingMethod,thinForwarderPMh,thinForwarderProductivity,thinForwardedWeight" +
                          ",regenFellerBuncherPMh,regenFellerBuncherProductivity,regenTrackedHarvesterPMh,regenTrackedHarvesterProductivity,regenWheeledHarvesterPMh,regenWheeledHarvesterProductivity" +
                          ",regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder,regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder,regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder,regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder" +
                          ",regenChainsawCrewWithFellerBuncherAndGrappleYoader,regenChainsawUtilizationWithFellerBuncherAndGrappleYoader,regenChainsawCmhWithFellerBuncherAndGrappleYoader,regenChainsawPMhWithFellerBuncherAndGrappleYoader" +
                          ",regenChainsawCrewWithTrackedHarvester,regenChainsawUtilizationWithTrackedHarvester,regenChainsawCmhWithTrackedHarvester,regenChainsawPMhWithTrackedHarvester" +
                          ",regenChainsawCrewWithWheeledHarvester,regenChainsawUtilizationWithWheeledHarvester,regenChainsawCmhWithWheeledHarvester,regenChainsawPMhWithWheeledHarvester" +
                          ",regenGrappleSwingYarderPMhPerHectare,regenGrappleSwingYarderProductivity,regenGrappleSwingYarderOverweightFirstLogsPerHectare,regenGrappleYoaderPMhPerHectare,regenGrappleYoaderProductivity,regenGrappleYoaderOverweightFirstLogsPerHectare" +
                          ",regenProcessorPMhWithGrappleSwingYarder,regenProcessorProductivityWithGrappleSwingYarder,regenProcessorPMhWithGrappleYoader,regenProcessorProductivityWithGrappleYoader,regenLoadedWeight";
            }
            return header;
        }

        protected static void GetMetricConversions(Units inputUnits, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor)
        {
            switch (inputUnits)
            {
                case Units.English:
                    areaConversionFactor = Constant.AcresPerHectare;
                    dbhConversionFactor = Constant.CentimetersPerInch;
                    heightConversionFactor = Constant.MetersPerFoot;
                    break;
                case Units.Metric:
                    areaConversionFactor = 1.0F;
                    dbhConversionFactor = 1.0F;
                    heightConversionFactor = 1.0F;
                    break;
                default:
                    throw new NotSupportedException("Unhandled units " + inputUnits + ".");
            }
        }

        protected long GetMaxFileSizeInBytes()
        {
            return (long)(1E9F * this.LimitGB);
        }

        protected StreamWriter GetWriter()
        {
            FileMode fileMode = this.Append ? FileMode.Append : FileMode.Create;
            if (fileMode == FileMode.Append)
            {
                this.openedExistingFile = File.Exists(this.CsvFile);
            }
            FileStream stream = new(this.CsvFile!, fileMode, FileAccess.Write, FileShare.Read, Constant.Default.FileWriteBufferSizeInBytes, FileOptions.SequentialScan);
            return new StreamWriter(stream, Encoding.UTF8); // callers assume UTF8, see remarks for StreamLengthSynchronizationInterval
        }

        protected bool ShouldWriteHeader()
        {
            return this.openedExistingFile == false;
        }

        /// <summary>
        /// Shared functionality between <see cref="WriteSilviculturalTrajectories"/> and <see cref="WriteStandTrajectories"/>.
        /// </summary>
        /// <remarks>
        /// Since inheritance hierarchy forks between <see cref="WriteSilviculturalTrajectoriesCmdlet"/> family and <see cref="WriteStandTrajectories"/>
        /// either multiple inheritance, which isn't supported by C#, or state objects and base class implementation are needed for code sharing. This
        /// implementation produces one complex function and the alternative requires state object flow across all classes derived from 
        /// <see cref="WriteSilviculturalTrajectoriesCmdlet"/>. Both approaches have similar complexity but this implementation might offer lower
        /// maintenance costs.
        /// </remarks>
        public static int WriteStandTrajectoryToCsv(StreamWriter writer, StandTrajectory trajectory, WriteStandTrajectoryContext writeContext)
        {
            Units trajectoryUnits = trajectory.GetUnits();
            if (trajectoryUnits != Units.English)
            {
                throw new NotSupportedException("Expected stand trajectory with English Units.");
            }
            trajectory.GetMerchantableVolumes(out StandMerchantableVolume standingVolume, out StandMerchantableVolume thinVolume);

            SnagDownLogTable? snagsAndDownLogs = null;
            if (writeContext.NoCarbon == false)
            {
                // computationally costly, so calculate only if needed
                snagsAndDownLogs = new(trajectory, writeContext.MaximumDiameter, writeContext.DiameterClassSize);
            }

            int endOfRotationPeriodIndex = writeContext.EndOfRotationPeriodIndex;
            int estimatedBytesWritten = 0;
            int financialIndex = writeContext.FinancialIndex;
            FinancialScenarios financialScenarios = writeContext.FinancialScenarios;
            StandDensity? previousStandDensity = null;
            float totalThinNetPresentValue = 0.0F;
            for (int periodIndex = 0; periodIndex <= endOfRotationPeriodIndex; ++periodIndex)
            {
                Stand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information missing for period " + periodIndex + ".");

                float basalAreaThinnedPerHa = trajectory.GetBasalAreaThinnedPerHa(periodIndex); // m²/ha
                if (writeContext.HarvestsOnly)
                {
                    if ((basalAreaThinnedPerHa == 0.0F) && (periodIndex != endOfRotationPeriodIndex))
                    {
                        continue; // no harvest in this period so no data to write
                    }
                }

                // financial value
                CutToLengthHarvest thinFinancialValue = periodIndex == 0 ? new() : financialScenarios.GetNetPresentThinningValue(trajectory, financialIndex, periodIndex);

                LongLogHarvest regenFinancialValue = financialScenarios.GetNetPresentRegenerationHarvestValue(trajectory, financialIndex, periodIndex);
                float reforestationNetPresentValue = financialScenarios.GetNetPresentReforestationValue(financialIndex, trajectory.PlantingDensityInTreesPerHectare);
                regenFinancialValue.NetPresentValuePerHa += reforestationNetPresentValue;
                regenFinancialValue.TaskCostPerHa -= reforestationNetPresentValue;

                string linePrefixAndStandAge = writeContext.LinePrefix + "," + trajectory.GetEndOfPeriodAge(periodIndex).ToString(CultureInfo.InvariantCulture);
                writer.Write(linePrefixAndStandAge);
                estimatedBytesWritten += linePrefixAndStandAge.Length;
                if (writeContext.NoTreeGrowth == false)
                {
                    // get densities and volumes
                    float basalAreaIntensity = 0.0F; // fraction
                    if (periodIndex > 0)
                    {
                        previousStandDensity = trajectory.GetStandDensity(periodIndex - 1);
                        basalAreaIntensity = basalAreaThinnedPerHa / previousStandDensity.BasalAreaPerHa;
                    }
                    float thinVolumeScribner = thinVolume.GetScribnerTotal(periodIndex); // MBF/ha
                    Debug.Assert((thinVolumeScribner == 0.0F && basalAreaThinnedPerHa == 0.0F) || (thinVolumeScribner > 0.0F && basalAreaThinnedPerHa > 0.0F));

                    StandDensity currentStandDensity = trajectory.GetStandDensity(periodIndex);
                    float treesPerHectareDecrease = 0.0F;
                    if (periodIndex > 0)
                    {
                        treesPerHectareDecrease = 1.0F - currentStandDensity.TreesPerHa / previousStandDensity!.TreesPerHa;
                    }

                    float quadraticMeanDiameterInCm = stand.GetQuadraticMeanDiameterInCentimeters();
                    float topHeightInM = stand.GetTopHeightInMeters();
                    // 1/(10 in * 2.54 cm/in) = 0.03937008
                    float reinekeStandDensityIndex = currentStandDensity.TreesPerHa * MathF.Pow(0.03937008F * quadraticMeanDiameterInCm, Constant.ReinekeExponent);

                    // write tree growth
                    string treeGrowth = "," +
                        currentStandDensity.TreesPerHa.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                        quadraticMeanDiameterInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + "," +
                        topHeightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture) + "," +
                        currentStandDensity.BasalAreaPerHa.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                        reinekeStandDensityIndex.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                        standingVolume.GetCubicTotal(periodIndex).ToString("0.000", CultureInfo.InvariantCulture) + "," +  // m³/ha
                        standingVolume.GetScribnerTotal(periodIndex).ToString("0.000", CultureInfo.InvariantCulture) + "," +  // MBF/ha
                        thinVolume.GetCubicTotal(periodIndex).ToString("0.000", CultureInfo.InvariantCulture) + "," +  // m³/ha
                        thinVolumeScribner.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        basalAreaThinnedPerHa.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                        basalAreaIntensity.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        treesPerHectareDecrease.ToString("0.000", CultureInfo.InvariantCulture);
                    writer.Write(treeGrowth);
                    estimatedBytesWritten += treeGrowth.Length;

                    previousStandDensity = currentStandDensity;
                }
                if (writeContext.NoFinancial == false)
                {
                    // TODO: remove duplication of code in financialScenarios.GetLandExpectationValue() and GetNetPresentValue()
                    totalThinNetPresentValue += thinFinancialValue.NetPresentValuePerHa;
                    float periodNetPresentValue = totalThinNetPresentValue + regenFinancialValue.NetPresentValuePerHa;
                    float presentToFutureConversionFactor = financialScenarios.GetAppreciationFactor(financialIndex, trajectory.GetEndOfPeriodAge(endOfRotationPeriodIndex));
                    float landExpectationValue = presentToFutureConversionFactor * periodNetPresentValue / (presentToFutureConversionFactor - 1.0F);
                    string financial = "," +
                        periodNetPresentValue.ToString("0", CultureInfo.InvariantCulture) + "," +
                        landExpectationValue.ToString("0", CultureInfo.InvariantCulture);
                    writer.Write(financial);
                    estimatedBytesWritten += financial.Length;
                }
                if (writeContext.NoCarbon == false)
                {
                    float liveBiomass = 0.001F * stand.GetLiveBiomass(); // Mg/ha
                    Debug.Assert(snagsAndDownLogs != null);
                    string carbon = "," +
                        liveBiomass.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        snagsAndDownLogs.SnagsPerHectareByPeriod[periodIndex].ToString("0.0", CultureInfo.InvariantCulture) + "," +
                        snagsAndDownLogs.SnagQmdInCentimetersByPeriod[periodIndex].ToString("0.00", CultureInfo.InvariantCulture);
                    writer.Write(carbon);
                    estimatedBytesWritten += carbon.Length;
                }
                if (writeContext.NoHarvestCosts == false)
                {
                    string harvestCosts = "," +
                        thinFinancialValue.MinimumSystemCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.TaskCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.GetMinimumCostHarvestSystem() + "," +
                        regenFinancialValue.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.TrackedHarvesterGrappleYoaderLoaderCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.WheeledHarvesterGrappleYoaderLoaderCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.TaskCostPerHa.ToString("0.00", CultureInfo.InvariantCulture);
                    writer.Write(harvestCosts);
                    estimatedBytesWritten += harvestCosts.Length;
                }
                if (writeContext.NoTimberSorts == false)
                {
                    string timberSorts = "," +
                        thinVolume.Logs2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinVolume.Logs3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinVolume.Logs4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinVolume.Cubic2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinVolume.Cubic3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinVolume.Cubic4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinVolume.Scribner2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinVolume.Scribner3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinVolume.Scribner4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.PondValue2SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.PondValue3SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.PondValue4SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Logs2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Logs3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Logs4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Cubic2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Cubic3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Cubic4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Scribner2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Scribner3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        standingVolume.Scribner4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.PondValue2SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.PondValue3SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.PondValue4SawPerHa.ToString("0.00", CultureInfo.InvariantCulture);
                    writer.Write(timberSorts);
                    estimatedBytesWritten += timberSorts.Length;
                }
                if (writeContext.NoEquipmentProductivity == false)
                {
                    string equipmentProductivity = "," +
                        thinFinancialValue.WheeledHarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.Productivity.WheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.ChainsawCrewWithWheeledHarvester.ToString() + "," +
                        thinFinancialValue.Productivity.ChainsawUtilizationWithWheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.ChainsawCubicVolumePerHaWithWheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.ChainsawPMhPerHaWithWheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.Productivity.ForwardingMethod.ToString() + "," +
                        thinFinancialValue.ForwarderPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.Productivity.Forwarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        thinFinancialValue.ForwardedWeightPeHa.ToString("0", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.FellerBuncherPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.Productivity.FellerBuncher.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.TrackedHarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.Productivity.TrackedHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.WheeledHarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.Productivity.WheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawCrewWithFellerBuncherAndGrappleSwingYarder.ToString() + "," +
                        regenFinancialValue.Productivity.ChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleSwingYarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawCrewWithFellerBuncherAndGrappleYoader.ToString() + "," +
                        regenFinancialValue.Productivity.ChainsawUtilizationWithFellerBuncherAndGrappleYoader.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleYoader.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawCrewWithTrackedHarvester.ToString() + "," +
                        regenFinancialValue.Productivity.ChainsawUtilizationWithTrackedHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawCubicVolumePerHaWithTrackedHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawPMhPerHaWithTrackedHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawCrewWithWheeledHarvester.ToString() + "," +
                        regenFinancialValue.Productivity.ChainsawUtilizationWithWheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawCubicVolumePerHaWithWheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ChainsawPMhPerHaWithWheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.GrappleSwingYarderPMhPerHectare.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.Productivity.GrappleSwingYarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.GrappleSwingYarderOverweightFirstLogsPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.GrappleYoaderPMhPerHectare.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.Productivity.GrappleYoader.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.GrappleYoaderOverweightFirstLogsPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ProcessorPMhPerHaWithGrappleSwingYarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.Productivity.ProcessorWithGrappleSwingYarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.ProcessorPMhPerHaWithGrappleYoader.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.Productivity.ProcessorWithGrappleYoader.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                        regenFinancialValue.LoadedWeightPerHa.ToString("0", CultureInfo.InvariantCulture);
                    writer.Write(equipmentProductivity);
                    estimatedBytesWritten += equipmentProductivity.Length;
                }

                writer.Write(Environment.NewLine);
                estimatedBytesWritten += Environment.NewLine.Length;
            }

            return estimatedBytesWritten;
        }
    }
}
