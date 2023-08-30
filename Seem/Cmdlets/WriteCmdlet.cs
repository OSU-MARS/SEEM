using Apache.Arrow.Ipc;
using Apache.Arrow;
using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;
using Mars.Seem.Output;

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

        private bool openedExistingCsvFile;

        [Parameter(HelpMessage = "Whether to append to an existing .csv file or overwrite it (default). Has no effect if the file does not exist or if -FilePath indicates a file type other than .csv.")]
        public SwitchParameter AppendToCsv;

        [Parameter(Mandatory = true, HelpMessage = "Relative or absolute path to write to. File type is inferred from the extension given.")]
        [ValidateNotNullOrEmpty]
        public string? FilePath;

        [Parameter(HelpMessage = "Approximate upper bound of output file size in gigabytes.  This limit is loosely enforced and maximum file sizes will typically be somewhat larger.")]
        [ValidateRange(0.1F, 100.0F)]
        public float LimitGB { get; set; }

        public WriteCmdlet()
        {
            this.openedExistingCsvFile = false;

            this.AppendToCsv = false;
            this.LimitGB = 1.0F; // sanity default, may be set higher in derived classes
        }

        protected StreamWriter CreateCsvWriter()
        {
            Debug.Assert(this.FilePath != null);

            FileMode fileMode = this.AppendToCsv ? FileMode.Append : FileMode.Create;
            if (fileMode == FileMode.Append)
            {
                this.openedExistingCsvFile = File.Exists(this.FilePath);
            }
            FileStream stream = new(this.FilePath, fileMode, FileAccess.Write, FileShare.Read, Constant.Default.FileWriteBufferSizeInBytes, FileOptions.SequentialScan);
            return new StreamWriter(stream, Encoding.UTF8); // callers assume UTF8, see remarks for StreamLengthSynchronizationInterval
        }

        protected StandTrajectoryArrowMemory CreateStandTrajectoryArrowMemory(int periodsToWrite)
        {
            StandTrajectoryArrowMemory arrowMemory = new(periodsToWrite);

            // estimate output file size
            float uncompressedBytesPerRow = arrowMemory.GetUncompressedBytesPerRow();
            float uncompressedFileSizeInGB = uncompressedBytesPerRow * periodsToWrite / (1024.0F * 1024.0F * 1024.0F);
            Debug.Assert(uncompressedBytesPerRow * arrowMemory.BatchLength < 2.0F * 1024.0F * 1024.0F * 1024.0F); // https://github.com/apache/arrow/issues/37069
            if (uncompressedFileSizeInGB > this.LimitGB)
            {
                throw new NotSupportedException("Expected file size of " + uncompressedFileSizeInGB.ToString("0.00") + " GB exceeds size limit of " + this.LimitGB.ToString("0.00") + " GB.");
            }

            return arrowMemory;
        }

        protected static string GetCsvHeaderForStandTrajectory(string prefix, WriteStandTrajectoryContext writeContext)
        {
            string header = prefix + ",year,standAge";
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
                header += ",thinMinCostSystem,thinFallerGrappleSwingYarderCost,thinFallerGrappleYoaderCost,thinFellerBuncherGrappleSwingYarderCost,thinFellerBuncherGrappleYoaderCost" +
                          ",thinTrackedHarvesterForwarderCost,thinTrackedHarvesterGrappleSwingYarderCost,thinTrackedHarvesterGrappleYoaderCost,thinWheeledHarvesterForwarderCost,thinWheeledHarvesterGrappleSwingYarderCost,thinWheeledHarvesterGrappleYoaderCost" +
                          ",thinTaskCost" +
                          ",regenMinCostSystem,regenFallerGrappleSwingYarderCost,regenFallerGrappleYoaderCost,regenFellerBuncherGrappleSwingYarderCost,regenFellerBuncherGrappleYoaderCost" +
                          ",regenTrackedHarvesterGrappleSwingYarderCost,regenTrackedHarvesterGrappleYoaderCost,regenWheeledHarvesterGrappleSwingYarderCost,regenWheeledHarvesterGrappleYoaderCost" + 
                          ",regenTaskCost,reforestationNpv";
            }
            if (writeContext.NoTimberSorts == false)
            {
                header += ",thinLogs2S,thinLogs3S,thinLogs4S,thinCmh2S,thinCmh3S,thinCmh4S,thinMbfh2S,thinMbfh3S,thinMbfh4S,thinPond2S,thinPond3S,thinPond4S" +
                          ",standingLogs2S,standingLogs3S,standingLogs4S,standingCmh2S,standingCmh3S,standingCmh4S,standingMbfh2S,standingMbfh3S,standingMbfh4S,regenPond2S,regenPond3S,regenPond4S";
            }
            if (writeContext.NoEquipmentProductivity == false)
            {
                header += ",thinFallerPMh,thinFallerProductivity,thinFellerBuncherPMh,thinFellerBuncherProductivity,thinTrackedHarvesterPMh,thinTrackedHarvesterProductivity,thinWheeledHarvesterPMh,thinWheeledHarvesterProductivity" +
                          ",thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder,thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder,thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder,thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder" +
                          ",thinChainsawCrewWithFellerBuncherAndGrappleYoader,thinChainsawUtilizationWithFellerBuncherAndGrappleYoader,thinChainsawCmhWithFellerBuncherAndGrappleYoader,thinChainsawPMhWithFellerBuncherAndGrappleYoader" +
                          ",thinChainsawCrewWithTrackedHarvester,thinChainsawUtilizationWithTrackedHarvester,thinChainsawCmhWithTrackedHarvester,thinChainsawPMhWithTrackedHarvester" +
                          ",thinChainsawCrewWithWheeledHarvester,thinChainsawUtilizationWithWheeledHarvester,thinChainsawCmhWithWheeledHarvester,thinChainsawPMhWithWheeledHarvester" +
                          ",thinForwardingMethod,thinForwarderPMh,thinForwarderProductivity,thinForwardedWeight" +
                          ",thinGrappleSwingYarderPMhPerHectare,thinGrappleSwingYarderProductivity,thinGrappleSwingYarderOverweightFirstLogsPerHectare,thinGrappleYoaderPMhPerHectare,thinGrappleYoaderProductivity,thinGrappleYoaderOverweightFirstLogsPerHectare" +
                          ",thinProcessorPMhWithGrappleSwingYarder,thinProcessorProductivityWithGrappleSwingYarder,thinProcessorPMhWithGrappleYoader,thinProcessorProductivityWithGrappleYoader,thinLoadedWeight" +
                          ",regenFallerPMh,regenFallerProductivity,regenFellerBuncherPMh,regenFellerBuncherProductivity,regenTrackedHarvesterPMh,regenTrackedHarvesterProductivity,regenWheeledHarvesterPMh,regenWheeledHarvesterProductivity" +
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

        protected bool ShouldWriteCsvHeader()
        {
            return this.openedExistingCsvFile == false;
        }

        protected void WriteFeather(StandTrajectoryArrowMemory standTrajectories)
        {
            Debug.Assert(this.FilePath != null);
            if (standTrajectories.RecordBatches.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(standTrajectories));
            }

            using FileStream stream = new(this.FilePath, FileMode.Create, FileAccess.Write, FileShare.None, Constant.Default.FileWriteBufferSizeInBytes, FileOptions.SequentialScan);
            using ArrowFileWriter writer = new(stream, standTrajectories.Schema);
            writer.WriteStart();

            for (int batchIndex = 0; batchIndex < standTrajectories.RecordBatches.Count; ++batchIndex)
            {
                writer.WriteRecordBatch(standTrajectories.RecordBatches[batchIndex]);
            }
            
            writer.WriteEnd();
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
        protected static int WriteStandTrajectoryToCsv(StreamWriter writer, StandTrajectory trajectory, WriteStandTrajectoryContext writeContext)
        {
            trajectory.GetMerchantableVolumes(out StandMerchantableVolume longLogVolume, out StandMerchantableVolume forwardedVolume);

            SnagDownLogTable? snagsAndDownLogs = null;
            if (writeContext.NoCarbon == false)
            {
                // computationally costly, so calculate only if needed
                snagsAndDownLogs = new(trajectory, writeContext.MaximumDiameter, writeContext.DiameterClassSize);
            }

            int endOfRotationPeriod = writeContext.EndOfRotationPeriod;
            int estimatedBytesWritten = 0;
            int financialIndex = writeContext.FinancialIndex;
            FinancialScenarios financialScenarios = writeContext.FinancialScenarios;
            string linePrefix = writeContext.GetCsvPrefixForSilviculturalCoordinate();
            StandDensity? previousStandDensity = null;
            float totalThinNetPresentValue = 0.0F;
            int? year = writeContext.StartYear;
            for (int periodIndex = 0; periodIndex <= endOfRotationPeriod; ++periodIndex)
            {
                Stand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information missing for period " + periodIndex + ".");

                float basalAreaThinnedPerHa = trajectory.GetBasalAreaThinnedPerHa(periodIndex); // m²/ha
                if (writeContext.HarvestsOnly)
                {
                    if ((basalAreaThinnedPerHa == 0.0F) && (periodIndex != endOfRotationPeriod))
                    {
                        continue; // no trees cut in this before end of rotation period so no data to write
                    }
                }

                // financial value
                financialScenarios.TryGetNetPresentThinValue(trajectory, financialIndex, periodIndex, out HarvestFinancialValue? thinFinancialValue);
                LongLogHarvest longLogRegenHarvest = financialScenarios.GetNetPresentRegenerationHarvestValue(trajectory, financialIndex, periodIndex);

                string linePrefixAndStandAge = linePrefix + "," + year.ToString() + "," + trajectory.GetEndOfPeriodAge(periodIndex).ToString(CultureInfo.InvariantCulture);
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

                    // TODO: support long log thins
                    float thinVolumeScribner = forwardedVolume.GetScribnerTotal(periodIndex); // MBF/ha
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
                    string treeGrowth = "," + currentStandDensity.TreesPerHa.ToString("0.0", CultureInfo.InvariantCulture) + 
                        "," + quadraticMeanDiameterInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + 
                        "," + topHeightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture) + 
                        "," + currentStandDensity.BasalAreaPerHa.ToString("0.0", CultureInfo.InvariantCulture) + 
                        "," + reinekeStandDensityIndex.ToString("0.0", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.GetCubicTotal(periodIndex).ToString("0.000", CultureInfo.InvariantCulture) + // m³/ha
                        "," + longLogVolume.GetScribnerTotal(periodIndex).ToString("0.000", CultureInfo.InvariantCulture) + // MBF/ha
                        "," + forwardedVolume.GetCubicTotal(periodIndex).ToString("0.000", CultureInfo.InvariantCulture) + // m³/ha, TODO: support long log thins
                        "," + thinVolumeScribner.ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + basalAreaThinnedPerHa.ToString("0.0", CultureInfo.InvariantCulture) + 
                        "," + basalAreaIntensity.ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + treesPerHectareDecrease.ToString("0.000", CultureInfo.InvariantCulture);
                    writer.Write(treeGrowth);
                    estimatedBytesWritten += treeGrowth.Length;

                    previousStandDensity = currentStandDensity;
                }
                if (writeContext.NoFinancial == false)
                {
                    if (thinFinancialValue != null)
                    {
                        totalThinNetPresentValue += thinFinancialValue.NetPresentValuePerHa;
                    }
                    
                    float periodNetPresentValue = financialScenarios.GetNetPresentValue(trajectory, financialIndex, endOfRotationPeriod, totalThinNetPresentValue, longLogRegenHarvest);
                    float landExpectationValue = financialScenarios.GetLandExpectationValue(trajectory, financialIndex, endOfRotationPeriod, totalThinNetPresentValue, longLogRegenHarvest);
                    string financial = "," + periodNetPresentValue.ToString("0", CultureInfo.InvariantCulture) + 
                                       "," + landExpectationValue.ToString("0", CultureInfo.InvariantCulture);
                    writer.Write(financial);
                    estimatedBytesWritten += financial.Length;
                }
                if (writeContext.NoCarbon == false)
                {
                    float liveBiomass = 0.001F * stand.GetLiveBiomass(); // Mg/ha
                    Debug.Assert(snagsAndDownLogs != null);
                    string carbon = "," + liveBiomass.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + snagsAndDownLogs.SnagsPerHectareByPeriod[periodIndex].ToString("0.0", CultureInfo.InvariantCulture) + 
                        "," + snagsAndDownLogs.SnagQmdInCentimetersByPeriod[periodIndex].ToString("0.00", CultureInfo.InvariantCulture);
                    writer.Write(carbon);
                    estimatedBytesWritten += carbon.Length;
                }
                if (writeContext.NoHarvestCosts == false)
                {
                    string harvestCosts;
                    if (thinFinancialValue != null)
                    {
                        if (thinFinancialValue is CutToLengthHarvest cutToLengthThin)
                        {
                            harvestCosts =
                                "," + cutToLengthThin.MinimumCostHarvestSystem + 
                                "," + // longLogThin.Fallers.SystemCostPerHaWithYarder
                                "," + // longLogThin.Fallers.SystemCostPerHaWithYoader
                                "," + // longLogThin.FellerBuncher.Yarder.SystemCostPerHa
                                "," + // longLogThin.FellerBuncher.Yoader.SystemCostPerHa
                                "," + cutToLengthThin.TrackedHarvester.SystemCostPerHaWithForwarder.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + // longLogThin.TrackedHarvester.SystemCostPerHaWithYarder
                                "," + // longLogThin.TrackedHarvester.SystemCostPerHaWithYoader
                                "," + cutToLengthThin.WheeledHarvester.SystemCostPerHaWithForwarder.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + // longLogThin.WheeledHarvester.SystemCostPerHaWithYarder
                                "," + // longLogThin.WheeledHarvester.SystemCostPerHaWithYoader
                                "," + cutToLengthThin.HarvestRelatedTaskCostPerHa.ToString("0.00", CultureInfo.InvariantCulture);
                        }
                        else if (thinFinancialValue is LongLogHarvest longLogThin)
                        {
                            harvestCosts =
                                "," + longLogThin.MinimumCostHarvestSystem +
                                "," + longLogThin.Fallers.SystemCostPerHaWithYarder.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + longLogThin.Fallers.SystemCostPerHaWithYoader.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + longLogThin.FellerBuncher.Yarder.SystemCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + longLogThin.FellerBuncher.Yoader.SystemCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + // longLogThin.TrackedHarvester.SystemCostPerHaWithForwarder
                                "," + longLogThin.TrackedHarvester.SystemCostPerHaWithYarder.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + longLogThin.TrackedHarvester.SystemCostPerHaWithYoader.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + // longLogThin.WheeledHarvester.SystemCostPerHaWithForwarder
                                "," + longLogThin.WheeledHarvester.SystemCostPerHaWithYarder.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + longLogThin.WheeledHarvester.SystemCostPerHaWithYoader.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + longLogThin.HarvestRelatedTaskCostPerHa.ToString("0.00", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            throw new NotSupportedException("Unhandled thinning of type " + thinFinancialValue.GetType().Name + ".");
                        }
                    }
                    else
                    {
                        harvestCosts =
                                "," + // thin.MinimumCostHarvestSystem
                                "," + // longLogThin.Fallers.SystemCostPerHaWithYarder
                                "," + // longLogThin.Fallers.SystemCostPerHaWithYoader
                                "," + // longLogThin.FellerBuncher.Yarder.SystemCostPerHa
                                "," + // longLogThin.FellerBuncher.Yoader.SystemCostPerHa
                                "," + // cutToLengthThin.TrackedHarvester.SystemCostPerHaWithForwarder
                                "," + // longLogThin.TrackedHarvester.SystemCostPerHaWithYarder
                                "," + // longLogThin.TrackedHarvester.SystemCostPerHaWithYoader
                                "," + // cutToLengthThin.WheeledHarvester.SystemCostPerHaWithForwarder
                                "," + // longLogThin.WheeledHarvester.SystemCostPerHaWithYarder
                                "," + // longLogThin.WheeledHarvester.SystemCostPerHaWithYoader
                                ","; // thin.HarvestRelatedTaskCostPerHa
                    }
                    harvestCosts += 
                        "," + longLogRegenHarvest.MinimumCostHarvestSystem + 
                        "," + longLogRegenHarvest.Fallers.SystemCostPerHaWithYarder.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Fallers.SystemCostPerHaWithYoader.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.Yarder.SystemCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.Yoader.SystemCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        // cutToLengthRegenHarvest.TrackedHarvester.SystemCostPerHaWithForwarder
                        "," + longLogRegenHarvest.TrackedHarvester.SystemCostPerHaWithYarder.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.TrackedHarvester.SystemCostPerHaWithYoader.ToString("0.00", CultureInfo.InvariantCulture) +
                        // cutToLengthRegenHarvest.WheeledHarvester.SystemCostPerHaWithForwarder not applicable
                        "," + longLogRegenHarvest.WheeledHarvester.SystemCostPerHaWithYarder.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.WheeledHarvester.SystemCostPerHaWithYoader.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.HarvestRelatedTaskCostPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.ReforestationNpv.ToString("0.00", CultureInfo.InvariantCulture);
                    writer.Write(harvestCosts);
                    estimatedBytesWritten += harvestCosts.Length;
                }
                if (writeContext.NoTimberSorts == false)
                {
                    string timberSorts;
                    if (thinFinancialValue != null)
                    {
                        timberSorts = "," + forwardedVolume.Logs2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + forwardedVolume.Logs3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + forwardedVolume.Logs4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + forwardedVolume.Cubic2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + forwardedVolume.Cubic3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + forwardedVolume.Cubic4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + forwardedVolume.Scribner2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + forwardedVolume.Scribner3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + forwardedVolume.Scribner4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) +
                            "," + thinFinancialValue.PondValue2SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) +
                            "," + thinFinancialValue.PondValue3SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) +
                            "," + thinFinancialValue.PondValue4SawPerHa.ToString("0.00", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        timberSorts = "," + // forwardedVolume.Logs2Saw[periodIndex]
                            "," + // forwardedVolume.Logs3Saw[periodIndex]
                            "," + // forwardedVolume.Logs4Saw[periodIndex]
                            "," + // forwardedVolume.Cubic2Saw[periodIndex]
                            "," + // forwardedVolume.Cubic3Saw[periodIndex]
                            "," + // forwardedVolume.Cubic4Saw[periodIndex]
                            "," + // forwardedVolume.Scribner2Saw[periodIndex]
                            "," + // forwardedVolume.Scribner3Saw[periodIndex]
                            "," + // forwardedVolume.Scribner4Saw[periodIndex]
                            "," + // thinFinancialValue.PondValue2SawPerHa
                            "," + // thinFinancialValue.PondValue3SawPerHa
                            ","; // thinFinancialValue.PondValue4SawPerHa
                    }
                    timberSorts +=
                        "," + longLogVolume.Logs2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.Logs3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.Logs4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.Cubic2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.Cubic3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.Cubic4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.Scribner2Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.Scribner3Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogVolume.Scribner4Saw[periodIndex].ToString("0.000", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.PondValue2SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.PondValue3SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.PondValue4SawPerHa.ToString("0.00", CultureInfo.InvariantCulture);
                    writer.Write(timberSorts);
                    estimatedBytesWritten += timberSorts.Length;
                }
                if (writeContext.NoEquipmentProductivity == false)
                {
                    string equipmentProductivity;
                    if (thinFinancialValue != null)
                    {
                        if (thinFinancialValue is CutToLengthHarvest cutToLengthThin)
                        {
                            equipmentProductivity =
                                "," + // longLogThin.Fallers.ChainsawPMhPerHa
                                "," + // longLogThin.Fallers.ChainsawProductivity
                                "," + // longLogThin.FellerBuncher.FellerBuncherPMhPerHa
                                "," + // longLogThin.FellerBuncher.FellerBuncherProductivity
                                "," + cutToLengthThin.TrackedHarvester.HarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.TrackedHarvester.HarvesterProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.WheeledHarvester.HarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.WheeledHarvester.HarvesterProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + // longLogThin.FellerBuncher.Yarder.ChainsawCrew
                                "," + // longLogThin.FellerBuncher.Yarder.ChainsawUtilization
                                "," + // longLogThin.FellerBuncher.Yarder.ChainsawCubicVolumePerHa
                                "," + // longLogThin.FellerBuncher.Yarder.ChainsawPMhPerHa
                                "," + // longLogThin.FellerBuncher.Yoader.ChainsawCrew
                                "," + // longLogThin.FellerBuncher.Yoader.ChainsawUtilization
                                "," + // longLogThin.FellerBuncher.Yoader.ChainsawCubicVolumePerHa
                                "," + // longLogThin.FellerBuncher.Yoader.ChainsawPMhPerHa
                                "," + cutToLengthThin.TrackedHarvester.ChainsawCrew.ToString() + 
                                "," + cutToLengthThin.TrackedHarvester.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.TrackedHarvester.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.TrackedHarvester.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.WheeledHarvester.ChainsawCrew.ToString() + 
                                "," + cutToLengthThin.WheeledHarvester.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.WheeledHarvester.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.WheeledHarvester.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.Forwarder.LoadingMethod.ToString() + 
                                "," + cutToLengthThin.Forwarder.ForwarderPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.Forwarder.ForwarderProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + cutToLengthThin.Forwarder.ForwardedWeightPerHa.ToString("0", CultureInfo.InvariantCulture) +
                                "," + // longLogThin.Yarder.YarderPMhPerHectare
                                "," + // longLogThin.Yarder.YarderProductivity
                                "," + // longLogThin.Yarder.OverweightFirstLogsPerHa
                                "," + // longLogThin.Yoader.YarderPMhPerHectare
                                "," + // longLogThin.Yoader.YarderProductivity
                                "," + // longLogThin.Yoader.OverweightFirstLogsPerHa
                                "," + // longLogThin.Yarder.ProcessorPMhPerHa
                                "," + // longLogThin.Yarder.ProcessorProductivity
                                "," + // longLogThin.Yoader.ProcessorPMhPerHa
                                "," + // longLogThin.Yoader.ProcessorProductivity
                                cutToLengthThin.Forwarder.ForwardedWeightPerHa.ToString("0", CultureInfo.InvariantCulture);
                        }
                        else if (thinFinancialValue is LongLogHarvest longLogThin)
                        {
                            equipmentProductivity =
                                "," + longLogThin.Fallers.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Fallers.ChainsawProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.FellerBuncherPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.FellerBuncherProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.TrackedHarvester.HarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.TrackedHarvester.HarvesterProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.WheeledHarvester.HarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.WheeledHarvester.HarvesterProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.Yarder.ChainsawCrew.ToString() + 
                                "," + longLogThin.FellerBuncher.Yarder.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.Yarder.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.Yarder.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.Yoader.ChainsawCrew.ToString() + 
                                "," + longLogThin.FellerBuncher.Yoader.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.Yoader.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.Yoader.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.TrackedHarvester.ChainsawCrew.ToString() + 
                                "," + longLogThin.TrackedHarvester.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.TrackedHarvester.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.TrackedHarvester.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.WheeledHarvester.ChainsawCrew.ToString() + 
                                "," + longLogThin.WheeledHarvester.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.WheeledHarvester.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.WheeledHarvester.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) +
                                "," + // cutToLengthThin.Forwarder.LoadingMethod
                                "," + // cutToLengthThin.Forwarder.ForwarderPMhPerHa
                                "," + // cutToLengthThin.Forwarder.ForwarderProductivity
                                "," + // cutToLengthThin.Forwarder.ForwardedWeightPerHa
                                "," + longLogThin.Yarder.YarderPMhPerHectare.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yarder.YarderProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yarder.OverweightFirstLogsPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yoader.YarderPMhPerHectare.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yoader.YarderProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yoader.OverweightFirstLogsPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yarder.ProcessorPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yarder.ProcessorProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yoader.ProcessorPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.Yoader.ProcessorProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                                "," + longLogThin.FellerBuncher.LoadedWeightPerHa.ToString("0", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            throw new NotSupportedException("Unhandled thinning of type " + thinFinancialValue.GetType().Name + ".");
                        }
                    }
                    else
                    {
                        equipmentProductivity =
                            "," + // longLogThin.Fallers.ChainsawPMhPerHa
                            "," + // longLogThin.Fallers.ChainsawProductivity
                            "," + // longLogThin.FellerBuncher.FellerBuncherPMhPerHa
                            "," + // longLogThin.FellerBuncher.FellerBuncherProductivity
                            "," + // cutToLengthThin.TrackedHarvester.HarvesterPMhPerHa
                            "," + // cutToLengthThin.TrackedHarvester.HarvesterProductivity
                            "," + // cutToLengthThin.WheeledHarvester.HarvesterPMhPerHa
                            "," + // cutToLengthThin.WheeledHarvester.HarvesterProductivity
                            "," + // longLogThin.FellerBuncher.Yarder.ChainsawCrew
                            "," + // longLogThin.FellerBuncher.Yarder.ChainsawUtilization
                            "," + // longLogThin.FellerBuncher.Yarder.ChainsawCubicVolumePerHa
                            "," + // longLogThin.FellerBuncher.Yarder.ChainsawPMhPerHa
                            "," + // longLogThin.FellerBuncher.Yoader.ChainsawCrew
                            "," + // longLogThin.FellerBuncher.Yoader.ChainsawUtilization
                            "," + // longLogThin.FellerBuncher.Yoader.ChainsawCubicVolumePerHa
                            "," + // longLogThin.FellerBuncher.Yoader.ChainsawPMhPerHa
                            "," + // cutToLengthThin.TrackedHarvester.ChainsawCrew
                            "," + // cutToLengthThin.TrackedHarvester.ChainsawUtilization
                            "," + // cutToLengthThin.TrackedHarvester.ChainsawCubicVolumePerHa
                            "," + // cutToLengthThin.TrackedHarvester.ChainsawPMhPerHa
                            "," + // cutToLengthThin.WheeledHarvester.ChainsawCrew
                            "," + // cutToLengthThin.WheeledHarvester.ChainsawUtilization
                            "," + // cutToLengthThin.WheeledHarvester.ChainsawCubicVolumePerHa
                            "," + // cutToLengthThin.WheeledHarvester.ChainsawPMhPerHa
                            "," + // cutToLengthThin.Forwarder.LoadingMethod
                            "," + // cutToLengthThin.Forwarder.ForwarderPMhPerHa
                            "," + // cutToLengthThin.Forwarder.ForwarderProductivity
                            "," + // cutToLengthThin.Forwarder.ForwardedWeightPerHa.ToString("0", CultureInfo.InvariantCulture) + "," +
                            "," + // longLogThin.Yarder.YarderPMhPerHectare
                            "," + // longLogThin.Yarder.YarderProductivity
                            "," + // longLogThin.Yarder.OverweightFirstLogsPerHa
                            "," + // longLogThin.Yoader.YarderPMhPerHectare
                            "," + // longLogThin.Yoader.YarderProductivity
                            "," + // longLogThin.Yoader.OverweightFirstLogsPerHa
                            "," + // longLogThin.Yarder.ProcessorPMhPerHa
                            "," + // longLogThin.Yarder.ProcessorProductivity
                            "," + // longLogThin.Yoader.ProcessorPMhPerHa
                            "," + // longLogThin.Yoader.ProcessorProductivity
                            ","; // cutToLengthThin.Forwarder.ForwardedWeightPerHa.ToString("0", CultureInfo.InvariantCulture);
                    }
                    equipmentProductivity +=
                        "," + longLogRegenHarvest.Fallers.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Fallers.ChainsawProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.FellerBuncherPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.FellerBuncherProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.TrackedHarvester.HarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.TrackedHarvester.HarvesterProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.WheeledHarvester.HarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.WheeledHarvester.HarvesterProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.Yarder.ChainsawCrew.ToString() + 
                        "," + longLogRegenHarvest.FellerBuncher.Yarder.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.Yarder.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.Yarder.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.Yoader.ChainsawCrew.ToString() + 
                        "," + longLogRegenHarvest.FellerBuncher.Yoader.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.Yoader.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.Yoader.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.TrackedHarvester.ChainsawCrew.ToString() + 
                        "," + longLogRegenHarvest.TrackedHarvester.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.TrackedHarvester.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.TrackedHarvester.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.WheeledHarvester.ChainsawCrew.ToString() + 
                        "," + longLogRegenHarvest.WheeledHarvester.ChainsawUtilization.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.WheeledHarvester.ChainsawCubicVolumePerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.WheeledHarvester.ChainsawPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yarder.YarderPMhPerHectare.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yarder.YarderProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yarder.OverweightFirstLogsPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yoader.YarderPMhPerHectare.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yoader.YarderProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yoader.OverweightFirstLogsPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yarder.ProcessorPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yarder.ProcessorProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yoader.ProcessorPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.Yoader.ProcessorProductivity.ToString("0.00", CultureInfo.InvariantCulture) + 
                        "," + longLogRegenHarvest.FellerBuncher.LoadedWeightPerHa.ToString("0", CultureInfo.InvariantCulture);
                    writer.Write(equipmentProductivity);
                    estimatedBytesWritten += equipmentProductivity.Length;
                }

                writer.Write(Environment.NewLine);
                estimatedBytesWritten += Environment.NewLine.Length;

                if (year != null)
                {
                    year += trajectory.PeriodLengthInYears;
                }
            }

            return estimatedBytesWritten;
        }
    }
}
