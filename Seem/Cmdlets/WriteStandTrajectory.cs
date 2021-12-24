using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "StandTrajectory")]
    public class WriteStandTrajectory : WriteTrajectoriesCmdlet
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

        public WriteStandTrajectory()
        {
            this.DiameterClassSize = Constant.Bucking.DiameterClassSizeInCentimeters;
            this.HarvestsOnly = false;
            this.MaximumDiameter = Constant.Bucking.DefaultMaximumFinalHarvestDiameterInCentimeters;
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

            using StreamWriter writer = this.GetWriter();

            // header
            if (this.ShouldWriteHeader())
            {
                string header = this.GetCsvHeaderForCoordinate() + ",standAge";
                if (this.NoTreeGrowth == false)
                {
                    header += ",TPH,QMD,Htop,BA,SDI,standingCmh,standingMbfh,thinCmh,thinMbfh,BAremoved,BAintensity,TPHdecrease";
                }
                if (this.NoFinancial == false)
                {
                    header += ",NPV,LEV";
                }
                if (this.NoCarbon == false)
                {
                    header += ",liveTreeBiomass,SPH,snagQmd";
                }
                if (this.NoHarvestCosts == false)
                {
                    header += ",thinWheeledHarvesterForwarderCost,thinTaskCost,regenMinCostSystem,regenFellerBuncherGrappleSwingYarderCost,regenFellerBuncherGrappleYoaderCost,regenTrackedHarvesterGrappleSwingYarderCost,regenTrackedHarvesterGrappleYoaderCost,regenWheeledHarvesterGrappleSwingYarderCost,regenWheeledHarvesterGrappleYoaderCost,regenTaskCost";
                }
                if (this.NoTimberSorts == false)
                {
                    header += ",thinLogs2S,thinLogs3S,thinLogs4S,thinCmh2S,thinCmh3S,thinCmh4S,thinMbfh2S,thinMbfh3S,thinMbfh4S,thinPond2S,thinPond3S,thinPond4S" +
                              ",standingLogs2S,standingLogs3S,standingLogs4S,standingCmh2S,standingCmh3S,standingCmh4S,standingMbfh2S,standingMbfh3S,standingMbfh4S,regenPond2S,regenPond3S,regenPond4S";
                }
                if (this.NoEquipmentProductivity == false)
                {
                    header += ",thinWheeledHarvesterPMh,thinWheeledHarvesterProductivity,thinChainsawCrewWithWheeledHarvester,thinChainsawCmh,thinChainsawPMhWithWheeledHarvester" +
                              ",thinForwardingMethod,thinForwarderPMh,thinForwarderProductivity,thinForwardedWeight" +
                              ",regenFellerBuncherPMh,regenFellerBuncherProductivity,regenTrackedHarvesterPMh,regenTrackedHarvesterProductivity,regenWheeledHarvesterPMh,regenWheeledHarvesterProductivity" +
                              ",regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder,regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder,regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder,regenChainsawCrewWithFellerBuncherAndGrappleYoader,regenChainsawCmhWithFellerBuncherAndGrappleYoader,regenChainsawPMhWithFellerBuncherAndGrappleYoader,regenChainsawCrewWithTrackedHarvester,regenChainsawCmhWithTrackedHarvester,regenChainsawPMhWithTrackedHarvester,regenChainsawCrewWithWheeledHarvester,regenChainsawCmhWithWheeledHarvester,regenChainsawPMhWithWheeledHarvester" +
                              ",regenGrappleSwingYarderPMhPerHectare,regenGrappleSwingYarderProductivity,regenGrappleSwingYarderOverweightFirstLogsPerHectare,regenGrappleYoaderPMhPerHectare,regenGrappleYoaderProductivity,regenGrappleYoaderOverweightFirstLogsPerHectare" +
                              ",regenProcessorPMhWithGrappleSwingYarder,regenProcessorProductivityWithGrappleSwingYarder,regenProcessorPMhWithGrappleYoader,regenProcessorProductivityWithGrappleYoader,regenLoadedWeight";
                }
                writer.WriteLine(header);
            }

            // rows for periods
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            int maxCoordinateIndex = this.GetMaxCoordinateIndex();
            for (int positionIndex = 0; positionIndex < maxCoordinateIndex; ++positionIndex)
            {
                StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(positionIndex, out string linePrefix, out int endOfRotationPeriod, out int financialIndex);
                Units trajectoryUnits = highTrajectory.GetUnits();
                if (trajectoryUnits != Units.English)
                {
                    throw new NotSupportedException("Expected stand trajectory with English Units.");
                }
                highTrajectory.GetMerchantableVolumes(out StandMerchantableVolume standingVolume, out StandMerchantableVolume thinVolume);

                SnagDownLogTable? snagsAndDownLogs = null;
                if (this.NoCarbon == false)
                {
                    // computationally costly, so calculate only if needed
                    snagsAndDownLogs = new(highTrajectory, this.MaximumDiameter, this.DiameterClassSize);
                }

                float totalThinNetPresentValue = 0.0F;
                for (int period = 0; period <= endOfRotationPeriod; ++period)
                {
                    Stand stand = highTrajectory.StandByPeriod[period] ?? throw new NotSupportedException("Stand information missing for period " + period + ".");
                    FinancialScenarios financialScenarios = this.Trajectories.FinancialScenarios;

                    float basalAreaThinnedPerHa = highTrajectory.GetBasalAreaThinnedPerHa(period); // m²/ha
                    if (this.HarvestsOnly)
                    {
                        if ((basalAreaThinnedPerHa == 0.0F) && (period != endOfRotationPeriod))
                        {
                            continue; // no harvest in this period so no data to write
                        }
                    }

                    // financial value
                    CutToLengthHarvest thinFinancialValue = period == 0 ? new() : financialScenarios.GetNetPresentThinningValue(highTrajectory, financialIndex, period);

                    LongLogHarvest regenFinancialValue = financialScenarios.GetNetPresentRegenerationHarvestValue(highTrajectory, financialIndex, period);
                    float reforestationNetPresentValue = financialScenarios.GetNetPresentReforestationValue(financialIndex, highTrajectory.PlantingDensityInTreesPerHectare);
                    regenFinancialValue.NetPresentValuePerHa += reforestationNetPresentValue;
                    regenFinancialValue.TaskCostPerHa -= reforestationNetPresentValue;

                    string linePrefixAndStandAge = linePrefix + "," + highTrajectory.GetEndOfPeriodAge(period).ToString(CultureInfo.InvariantCulture);
                    writer.Write(linePrefixAndStandAge);
                    estimatedBytesSinceLastFileLength += linePrefixAndStandAge.Length;
                    if (this.NoTreeGrowth == false)
                    {
                        // get densities and volumes
                        StandDensity? previousStandDensity = null;
                        float basalAreaIntensity = 0.0F; // fraction
                        if (period > 0)
                        {
                            previousStandDensity = highTrajectory.GetStandDensity(period - 1);
                            basalAreaIntensity = basalAreaThinnedPerHa / previousStandDensity.BasalAreaPerHa;
                        }
                        float thinVolumeScribner = thinVolume.GetScribnerTotal(period); // MBF/ha
                        Debug.Assert((thinVolumeScribner == 0.0F && basalAreaThinnedPerHa == 0.0F) || (thinVolumeScribner > 0.0F && basalAreaThinnedPerHa > 0.0F));

                        StandDensity currentStandDensity = highTrajectory.GetStandDensity(period);
                        float treesPerHectareDecrease = 0.0F;
                        if (period > 0)
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
                            standingVolume.GetCubicTotal(period).ToString("0.000", CultureInfo.InvariantCulture) + "," +  // m³/ha
                            standingVolume.GetScribnerTotal(period).ToString("0.000", CultureInfo.InvariantCulture) + "," +  // MBF/ha
                            thinVolume.GetCubicTotal(period).ToString("0.000", CultureInfo.InvariantCulture) + "," +  // m³/ha
                            thinVolumeScribner.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            basalAreaThinnedPerHa.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                            basalAreaIntensity.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            treesPerHectareDecrease.ToString("0.000", CultureInfo.InvariantCulture);
                        writer.Write(treeGrowth);
                        estimatedBytesSinceLastFileLength += treeGrowth.Length;

                        previousStandDensity = currentStandDensity;
                    }
                    if (this.NoFinancial == false)
                    {
                        // TODO: remove duplication of code in financialScenarios.GetLandExpectationValue() and GetNetPresentValue()
                        totalThinNetPresentValue += thinFinancialValue.NetPresentValuePerHa;
                        float periodNetPresentValue = totalThinNetPresentValue + regenFinancialValue.NetPresentValuePerHa;
                        float presentToFutureConversionFactor = financialScenarios.GetAppreciationFactor(financialIndex, highTrajectory.GetEndOfPeriodAge(endOfRotationPeriod));
                        float landExpectationValue = presentToFutureConversionFactor * periodNetPresentValue / (presentToFutureConversionFactor - 1.0F);
                        string financial = "," + 
                            periodNetPresentValue.ToString("0", CultureInfo.InvariantCulture) + "," +
                            landExpectationValue.ToString("0", CultureInfo.InvariantCulture);
                        writer.Write(financial);
                        estimatedBytesSinceLastFileLength += financial.Length;
                    }
                    if (this.NoCarbon == false)
                    {
                        float liveBiomass = 0.001F * stand.GetLiveBiomass(); // Mg/ha
                        Debug.Assert(snagsAndDownLogs != null);
                        string carbon = "," + 
                            liveBiomass.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            snagsAndDownLogs.SnagsPerHectareByPeriod[period].ToString("0.0", CultureInfo.InvariantCulture) + "," +
                            snagsAndDownLogs.SnagQmdInCentimetersByPeriod[period].ToString("0.00", CultureInfo.InvariantCulture);
                        writer.Write(carbon);
                        estimatedBytesSinceLastFileLength += carbon.Length;
                    }
                    if (this.NoHarvestCosts == false)
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
                        estimatedBytesSinceLastFileLength += harvestCosts.Length;
                    }
                    if (this.NoTimberSorts == false)
                    {
                        string timberSorts = "," + 
                            thinVolume.Logs2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinVolume.Logs3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinVolume.Logs4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinVolume.Cubic2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinVolume.Cubic3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinVolume.Cubic4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinVolume.Scribner2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinVolume.Scribner3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinVolume.Scribner4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            thinFinancialValue.PondValue2SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            thinFinancialValue.PondValue3SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            thinFinancialValue.PondValue4SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Logs2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Logs3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Logs4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Cubic2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Cubic3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Cubic4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Scribner2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Scribner3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            standingVolume.Scribner4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.PondValue2SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.PondValue3SawPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.PondValue4SawPerHa.ToString("0.00", CultureInfo.InvariantCulture);
                        writer.Write(timberSorts);
                        estimatedBytesSinceLastFileLength += timberSorts.Length;
                    }
                    if (this.NoEquipmentProductivity == false)
                    {
                        string equipmentProductivity = "," +
                            thinFinancialValue.WheeledHarvesterPMhPerHa.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            thinFinancialValue.Productivity.WheeledHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            thinFinancialValue.ChainsawCrewWithWheeledHarvester.ToString() + "," +
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
                            regenFinancialValue.ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleSwingYarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.ChainsawCrewWithFellerBuncherAndGrappleYoader.ToString() + "," +
                            regenFinancialValue.ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleYoader.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.ChainsawCrewWithTrackedHarvester.ToString() + "," +
                            regenFinancialValue.ChainsawCubicVolumePerHaWithTrackedHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.ChainsawPMhPerHaWithTrackedHarvester.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                            regenFinancialValue.ChainsawCrewWithWheeledHarvester.ToString() + "," +
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
                        estimatedBytesSinceLastFileLength += equipmentProductivity.Length;
                    }

                    writer.Write(Environment.NewLine);
                    estimatedBytesSinceLastFileLength += Environment.NewLine.Length;
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
