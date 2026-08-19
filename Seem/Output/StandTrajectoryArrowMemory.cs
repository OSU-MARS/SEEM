using Apache.Arrow;
using Apache.Arrow.Types;
using Mars.Seem.Extensions;
using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Output
{
    public class StandTrajectoryArrowMemory : ArrowMemory
    {
        private StandTrajectoryBatch? currentBatch;

        public StandTrajectoryArrowMemory(WriteStandTrajectoryContext writeContext, int totalNumberOfRecords)
            : base(StandTrajectoryArrowMemory.CreateSchema(writeContext)) // 525 bytes/record * 4M records = 1.96 GB
        {
            this.currentBatch = null;

            this.TotalNumberOfRecords = totalNumberOfRecords; // base..ctor(Schema, int) takes batch length, not total number of records
        }

        private void Add(StandTrajectory trajectory, int startPeriod, WriteStandTrajectoryContext writeContext, int startIndexInRecordBatch, int periodsToCopy)
        {
            Debug.Assert(this.currentBatch != null);
            if (UInt32.TryParse(trajectory.Name, out UInt32 standID) == false)
            {
                throw new NotSupportedException($"Stand trajectory name '{trajectory.Name}' could not be converted to an unsigned 32 bit integer. For the moment, trajectory names are required to be stand IDs.");
            }

            (StandMerchantableVolume forwardedThinVolume, StandMerchantableVolume longLogThinVolume, StandMerchantableVolume longLogRegenVolume) = trajectory.GetMerchantableVolumes();

            //SnagDownLogTable? snagsAndDownLogs = null;
            //if (writeContext.NoCarbon == false)
            //{
            //    snagsAndDownLogs = new(trajectory, writeContext.MaximumDiameter, writeContext.DiameterClassSize);
            //}

            (int firstThinAgeInt32, int secondThinAgeInt32, int thirdThinAgeInt32, int rotationAgeInt32) = writeContext.GetHarvestAges();
            Int16 firstThinAge = (Int16)firstThinAgeInt32;
            Int16 secondThinAge = (Int16)secondThinAgeInt32;
            Int16 thirdThinAge = (Int16)thirdThinAgeInt32;
            Int16 rotationAge = (Int16)rotationAgeInt32;

            int lastPeriodToCopy = startPeriod + periodsToCopy - 1;
            int financialIndex = writeContext.FinancialIndex;
            FinancialScenarios financialScenarios = writeContext.FinancialScenarios;
            StandDensity? previousStandDensity = null;
            float totalThinNetPresentValue = 0.0F;
            Int16 year = writeContext.StartYear != null ? (Int16)writeContext.StartYear : Constant.NoDataInt16;
            for (int periodIndex = startPeriod, recordIndex = startIndexInRecordBatch; periodIndex <= lastPeriodToCopy; ++periodIndex, ++recordIndex)
            {
                Stand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException($"Stand information missing for period {periodIndex}.");

                float basalAreaThinnedPerHa = trajectory.GetBasalAreaThinnedPerHa(periodIndex); // m²/ha
                if (writeContext.HarvestsOnly)
                {
                    if ((basalAreaThinnedPerHa == 0.0F) && (periodIndex != writeContext.EndOfRotationPeriod))
                    {
                        continue; // no trees cut in this before end of rotation period so no data to write
                    }
                }

                // financial value
                StandMerchantableVolume thinVolume = longLogThinVolume;
                if (financialScenarios.TryGetNetPresentThinValue(trajectory, financialIndex, periodIndex, out HarvestFinancialValue? thinFinancialValue))
                {
                    bool isCutToLengthThin = (thinFinancialValue.MinimumCostHarvestSystem == HarvestSystemEquipment.TrackedHarvesterForwarder) ||
                                             (thinFinancialValue.MinimumCostHarvestSystem == HarvestSystemEquipment.TrackedHarvesterForwarder);
                    if (isCutToLengthThin)
                    {
                        thinVolume = forwardedThinVolume;
                    }
                }
                LongLogHarvest longLogRegenHarvest = financialScenarios.GetNetPresentRegenerationHarvestValue(trajectory, financialIndex, periodIndex);

                this.currentBatch.Stand[recordIndex] = standID;
                this.currentBatch.Thin1[recordIndex] = firstThinAge;
                this.currentBatch.Thin2[recordIndex] = secondThinAge;
                this.currentBatch.Thin3[recordIndex] = thirdThinAge;
                this.currentBatch.Rotation[recordIndex] = rotationAge;
                this.currentBatch.FinancialScenario[recordIndex] = (UInt32)financialIndex;
                this.currentBatch.Year[recordIndex] = year;
                this.currentBatch.StandAge[recordIndex] = (Int16)trajectory.GetEndOfPeriodAge(periodIndex);
                
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
                    this.currentBatch.Tph[recordIndex] = currentStandDensity.TreesPerHa;
                    this.currentBatch.Qmd[recordIndex] = quadraticMeanDiameterInCm;
                    this.currentBatch.HTop[recordIndex] = topHeightInM;
                    this.currentBatch.BasalArea[recordIndex] = currentStandDensity.BasalAreaPerHa;
                    this.currentBatch.ReinekeSdi[recordIndex] = reinekeStandDensityIndex;
                    this.currentBatch.StandingCmh[recordIndex] = longLogRegenVolume.GetCubicTotal(periodIndex);
                    this.currentBatch.StandingMbfh[recordIndex] = longLogRegenVolume.GetScribnerTotal(periodIndex);
                    this.currentBatch.ThinCmh[recordIndex] = thinVolume.GetCubicTotal(periodIndex); // TODO: support long log thins
                    this.currentBatch.ThinMbfh[recordIndex] = thinVolumeScribner;
                    this.currentBatch.BAremoved[recordIndex] = basalAreaThinnedPerHa;
                    this.currentBatch.BAintensity[recordIndex] = basalAreaIntensity;
                    this.currentBatch.TphDecrease[recordIndex] = treesPerHectareDecrease;

                    previousStandDensity = currentStandDensity;
                }

                if (writeContext.NoFinancial == false)
                {
                    if (thinFinancialValue != null)
                    {
                        totalThinNetPresentValue += thinFinancialValue.NetPresentValuePerHa;
                    }

                    float periodNetPresentValue = financialScenarios.GetNetPresentValue(trajectory, financialIndex, writeContext.EndOfRotationPeriod, totalThinNetPresentValue, longLogRegenHarvest);
                    float landExpectationValue = financialScenarios.GetLandExpectationValue(trajectory, financialIndex, writeContext.EndOfRotationPeriod, totalThinNetPresentValue, longLogRegenHarvest);

                    this.currentBatch.Npv[recordIndex] = periodNetPresentValue;
                    this.currentBatch.Lev[recordIndex] = landExpectationValue;
                }

                //if (writeContext.NoCarbon == false)
                //{
                //    float liveBiomass = 0.001F * stand.GetLiveBiomass(); // Mg/ha
                //    this.currentBatchLiveBimass[recordIndex] = liveBiomass;
                //    Debug.Assert(snagsAndDownLogs != null);

                //    this.currentBatchSph[recordIndex] = snagsAndDownLogs.SnagsPerHectareByPeriod[periodIndex];
                //    matchSnagQmd[recordIndex] = snagsAndDownLogs.SnagQmdInCentimetersByPeriod[periodIndex]
                //}

                if (writeContext.NoHarvestCosts == false)
                {
                    if (thinFinancialValue != null)
                    {
                        if (thinFinancialValue is CutToLengthHarvest cutToLengthThin)
                        {
                            this.currentBatch.ThinMinCostSystem[recordIndex] = cutToLengthThin.MinimumCostHarvestSystem;
                            this.currentBatch.ThinFallerGrappleSwingYarderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinFallerGrappleYoaderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinFellerBuncherGrappleSwingYarderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinFellerBuncherGrappleYoaderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinTrackedHarvesterForwarderCost[recordIndex] = cutToLengthThin.TrackedHarvester.SystemCostPerHaWithForwarder;
                            this.currentBatch.ThinTrackedHarvesterGrappleSwingYarderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinTrackedHarvesterGrappleYoaderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinWheeledHarvesterForwarderCost[recordIndex] = cutToLengthThin.WheeledHarvester.SystemCostPerHaWithForwarder;
                            this.currentBatch.ThinWheeledHarvesterGrappleSwingYarderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinWheeledHarvesterGrappleYoaderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinTaskCost[recordIndex] = cutToLengthThin.HarvestRelatedTaskCostPerHa;
                        }
                        else if (thinFinancialValue is LongLogHarvest longLogThin)
                        {
                            this.currentBatch.ThinMinCostSystem[recordIndex] = longLogThin.MinimumCostHarvestSystem;
                            this.currentBatch.ThinFallerGrappleSwingYarderCost[recordIndex] = longLogThin.Fallers.SystemCostPerHaWithYarder;
                            this.currentBatch.ThinFallerGrappleYoaderCost[recordIndex] = longLogThin.Fallers.SystemCostPerHaWithYoader;
                            this.currentBatch.ThinFellerBuncherGrappleSwingYarderCost[recordIndex] = longLogThin.FellerBuncher.Yarder.SystemCostPerHa;
                            this.currentBatch.ThinFellerBuncherGrappleYoaderCost[recordIndex] = longLogThin.FellerBuncher.Yoader.SystemCostPerHa;
                            this.currentBatch.ThinTrackedHarvesterForwarderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinTrackedHarvesterGrappleSwingYarderCost[recordIndex] = longLogThin.TrackedHarvester.SystemCostPerHaWithYarder;
                            this.currentBatch.ThinTrackedHarvesterGrappleYoaderCost[recordIndex] = longLogThin.TrackedHarvester.SystemCostPerHaWithYoader;
                            this.currentBatch.ThinWheeledHarvesterForwarderCost[recordIndex] = Single.NaN;
                            this.currentBatch.ThinWheeledHarvesterGrappleSwingYarderCost[recordIndex] = longLogThin.WheeledHarvester.SystemCostPerHaWithYarder;
                            this.currentBatch.ThinWheeledHarvesterGrappleYoaderCost[recordIndex] = longLogThin.WheeledHarvester.SystemCostPerHaWithYoader;
                            this.currentBatch.ThinTaskCost[recordIndex] = longLogThin.HarvestRelatedTaskCostPerHa;
                        }
                        else
                        {
                            throw new NotSupportedException($"Unhandled thinning of type {thinFinancialValue.GetType().Name}.");
                        }
                    }

                    this.currentBatch.RegenMinCostSystem[recordIndex] = longLogRegenHarvest.MinimumCostHarvestSystem;
                    this.currentBatch.RegenFallerGrappleSwingYarderCost[recordIndex] = longLogRegenHarvest.Fallers.SystemCostPerHaWithYarder;
                    this.currentBatch.RegenFallerGrappleYoaderCost[recordIndex] = longLogRegenHarvest.Fallers.SystemCostPerHaWithYoader;
                    this.currentBatch.RegenFellerBuncherGrappleSwingYarderCost[recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.SystemCostPerHa;
                    this.currentBatch.RegenFellerBuncherGrappleYoaderCost[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.SystemCostPerHa;
                    // cutToLengthRegenHarvest.TrackedHarvester.SystemCostPerHaWithForwarder
                    this.currentBatch.RegenTrackedHarvesterGrappleSwingYarderCost[recordIndex] = longLogRegenHarvest.TrackedHarvester.SystemCostPerHaWithYarder;
                    this.currentBatch.RegenTrackedHarvesterGrappleYoaderCost[recordIndex] = longLogRegenHarvest.TrackedHarvester.SystemCostPerHaWithYoader;
                    // cutToLengthRegenHarvest.WheeledHarvester.SystemCostPerHaWithForwarder not applicable
                    this.currentBatch.RegenWheeledHarvesterGrappleSwingYarderCost[recordIndex] = longLogRegenHarvest.WheeledHarvester.SystemCostPerHaWithYarder;
                    this.currentBatch.RegenWheeledHarvesterGrappleYoaderCost[recordIndex] = longLogRegenHarvest.WheeledHarvester.SystemCostPerHaWithYoader;
                    this.currentBatch.RegenTaskCost[recordIndex] = longLogRegenHarvest.HarvestRelatedTaskCostPerHa;
                    this.currentBatch.ReforestationNpv[recordIndex] = longLogRegenHarvest.ReforestationNpv;
                }
                if (writeContext.NoTimberSorts == false)
                {
                    if (thinFinancialValue != null)
                    {
                        this.currentBatch.ThinLogs2S [recordIndex] = thinVolume.Logs2Saw[periodIndex];
                        this.currentBatch.ThinLogs3S[recordIndex] = thinVolume.Logs3Saw[periodIndex];
                        this.currentBatch.ThinLogs4S[recordIndex] = thinVolume.Logs4Saw[periodIndex];
                        this.currentBatch.ThinCmh2S[recordIndex] = thinVolume.Cubic2Saw[periodIndex];
                        this.currentBatch.ThinCmh3S[recordIndex] = thinVolume.Cubic3Saw[periodIndex];
                        this.currentBatch.ThinCmh4S[recordIndex] = thinVolume.Cubic4Saw[periodIndex];
                        this.currentBatch.ThinMbfh2S[recordIndex] = thinVolume.Scribner2Saw[periodIndex];
                        this.currentBatch.ThinMbfh3S[recordIndex] = thinVolume.Scribner3Saw[periodIndex];
                        this.currentBatch.ThinMbfh4S[recordIndex] = thinVolume.Scribner4Saw[periodIndex];
                        this.currentBatch.ThinPond2S[recordIndex] = thinFinancialValue.PondValue2SawPerHa;
                        this.currentBatch.ThinPond3S[recordIndex] = thinFinancialValue.PondValue3SawPerHa;
                        this.currentBatch.ThinPond4S[recordIndex] = thinFinancialValue.PondValue4SawPerHa;
                    }
                    else
                    {
                        this.currentBatch.ThinLogs2S[recordIndex] = Single.NaN; // no thin in this timestep so no data
                        this.currentBatch.ThinLogs3S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinLogs4S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinCmh2S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinCmh3S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinCmh4S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinMbfh2S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinMbfh3S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinMbfh4S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinPond2S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinPond3S[recordIndex] = Single.NaN;
                        this.currentBatch.ThinPond4S[recordIndex] = Single.NaN;
                    }
                    this.currentBatch.StandingLogs2S[recordIndex] = longLogRegenVolume.Logs2Saw[periodIndex];
                    this.currentBatch.StandingLogs3S[recordIndex] = longLogRegenVolume.Logs3Saw[periodIndex];
                    this.currentBatch.StandingLogs4S[recordIndex] = longLogRegenVolume.Logs4Saw[periodIndex];
                    this.currentBatch.StandingCmh2S[recordIndex] = longLogRegenVolume.Cubic2Saw[periodIndex];
                    this.currentBatch.StandingCmh3S[recordIndex] = longLogRegenVolume.Cubic3Saw[periodIndex];
                    this.currentBatch.StandingCmh4S[recordIndex] = longLogRegenVolume.Cubic4Saw[periodIndex];
                    this.currentBatch.StandingMbfh2S[recordIndex] = longLogRegenVolume.Scribner2Saw[periodIndex];
                    this.currentBatch.StandingMbfh3S[recordIndex] = longLogRegenVolume.Scribner3Saw[periodIndex];
                    this.currentBatch.StandingMbfh4S[recordIndex] = longLogRegenVolume.Scribner4Saw[periodIndex];
                    this.currentBatch.RegenPond2S[recordIndex] = longLogRegenHarvest.PondValue2SawPerHa;
                    this.currentBatch.RegenPond3S[recordIndex] = longLogRegenHarvest.PondValue3SawPerHa;
                    this.currentBatch.RegenPond4S[recordIndex] = longLogRegenHarvest.PondValue4SawPerHa;
                }

                if (writeContext.NoEquipmentProductivity == false)
                {
                    if (thinFinancialValue != null)
                    {
                        if (thinFinancialValue is CutToLengthHarvest cutToLengthThin)
                        {
                            this.currentBatch.ThinFallerPMh[recordIndex] = Single.NaN;
                            this.currentBatch.ThinFallerProductivity[recordIndex] = Single.NaN;
                            this.currentBatch.ThinFellerBuncherPMh[recordIndex] = Single.NaN;
                            this.currentBatch.ThinFellerBuncherProductivity[recordIndex] = Single.NaN;
                            this.currentBatch.ThinTrackedHarvesterPMh[recordIndex] = cutToLengthThin.TrackedHarvester.HarvesterPMhPerHa;
                            this.currentBatch.ThinTrackedHarvesterProductivity[recordIndex] = cutToLengthThin.TrackedHarvester.HarvesterProductivity;
                            this.currentBatch.ThinWheeledHarvesterPMh[recordIndex] = cutToLengthThin.WheeledHarvester.HarvesterPMhPerHa;
                            this.currentBatch.ThinWheeledHarvesterProductivity[recordIndex] = cutToLengthThin.WheeledHarvester.HarvesterProductivity;
                            this.currentBatch.ThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder[recordIndex] = ChainsawCrewType.None;
                            this.currentBatch.ThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                            this.currentBatch.ThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                            this.currentBatch.ThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                            this.currentBatch.ThinChainsawCrewWithFellerBuncherAndGrappleYoader[recordIndex] = ChainsawCrewType.None;
                            this.currentBatch.ThinChainsawUtilizationWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                            this.currentBatch.ThinChainsawCmhWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                            this.currentBatch.ThinChainsawPMhWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                            this.currentBatch.ThinChainsawCrewWithTrackedHarvester[recordIndex] = cutToLengthThin.TrackedHarvester.ChainsawCrew;
                            this.currentBatch.ThinChainsawUtilizationWithTrackedHarvester[recordIndex] = cutToLengthThin.TrackedHarvester.ChainsawUtilization;
                            this.currentBatch.ThinChainsawCmhWithTrackedHarvester[recordIndex] = cutToLengthThin.TrackedHarvester.ChainsawCubicVolumePerHa;
                            this.currentBatch.ThinChainsawPMhWithTrackedHarvester[recordIndex] = cutToLengthThin.TrackedHarvester.ChainsawPMhPerHa;
                            this.currentBatch.ThinChainsawCrewWithWheeledHarvester[recordIndex] = cutToLengthThin.WheeledHarvester.ChainsawCrew;
                            this.currentBatch.ThinChainsawUtilizationWithWheeledHarvester[recordIndex] = cutToLengthThin.WheeledHarvester.ChainsawUtilization;
                            this.currentBatch.ThinChainsawCmhWithWheeledHarvester[recordIndex] = cutToLengthThin.WheeledHarvester.ChainsawCubicVolumePerHa;
                            this.currentBatch.ThinChainsawPMhWithWheeledHarvester[recordIndex] = cutToLengthThin.WheeledHarvester.ChainsawPMhPerHa;
                            this.currentBatch.ThinForwardingMethod[recordIndex] = cutToLengthThin.Forwarder.LoadingMethod;
                            this.currentBatch.ThinForwarderPMh[recordIndex] = cutToLengthThin.Forwarder.ForwarderPMhPerHa;
                            this.currentBatch.ThinForwarderProductivity[recordIndex] = cutToLengthThin.Forwarder.ForwarderProductivity;
                            this.currentBatch.ThinForwardedWeight[recordIndex] = cutToLengthThin.Forwarder.ForwardedWeightPerHa;
                            this.currentBatch.ThinGrappleSwingYarderPMhPerHectare[recordIndex] = Single.NaN;
                            this.currentBatch.ThinGrappleSwingYarderProductivity[recordIndex] = Single.NaN;
                            this.currentBatch.ThinGrappleSwingYarderOverweightFirstLogsPerHectare[recordIndex] = Single.NaN;
                            this.currentBatch.ThinGrappleYoaderPMhPerHectare[recordIndex] = Single.NaN;
                            this.currentBatch.ThinGrappleYoaderProductivity[recordIndex] = Single.NaN;
                            this.currentBatch.ThinGrappleYoaderOverweightFirstLogsPerHectare[recordIndex] = Single.NaN;
                            this.currentBatch.ThinProcessorPMhWithGrappleSwingYarder[recordIndex] = Single.NaN;
                            this.currentBatch.ThinProcessorProductivityWithGrappleSwingYarder[recordIndex] = Single.NaN;
                            this.currentBatch.ThinProcessorPMhWithGrappleYoader[recordIndex] = Single.NaN;
                            this.currentBatch.ThinProcessorProductivityWithGrappleYoader[recordIndex] = Single.NaN;
                            this.currentBatch.ThinLoadedWeight[recordIndex] =cutToLengthThin.Forwarder.ForwardedWeightPerHa;
                        }
                        else if (thinFinancialValue is LongLogHarvest longLogThin)
                        {
                            this.currentBatch.ThinFallerPMh[recordIndex] = longLogThin.Fallers.ChainsawPMhPerHa;
                            this.currentBatch.ThinFallerProductivity[recordIndex] = longLogThin.Fallers.ChainsawProductivity;
                            this.currentBatch.ThinFellerBuncherPMh[recordIndex] = longLogThin.FellerBuncher.FellerBuncherPMhPerHa;
                            this.currentBatch.ThinFellerBuncherProductivity[recordIndex] = longLogThin.FellerBuncher.FellerBuncherProductivity;
                            this.currentBatch.ThinTrackedHarvesterPMh[recordIndex] = longLogThin.TrackedHarvester.HarvesterPMhPerHa;
                            this.currentBatch.ThinTrackedHarvesterProductivity[recordIndex] = longLogThin.TrackedHarvester.HarvesterProductivity;
                            this.currentBatch.ThinWheeledHarvesterPMh[recordIndex] = longLogThin.WheeledHarvester.HarvesterPMhPerHa;
                            this.currentBatch.ThinWheeledHarvesterProductivity[recordIndex] = longLogThin.WheeledHarvester.HarvesterProductivity;
                            this.currentBatch.ThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogThin.FellerBuncher.Yarder.ChainsawCrew;
                            this.currentBatch.ThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogThin.FellerBuncher.Yarder.ChainsawUtilization;
                            this.currentBatch.ThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogThin.FellerBuncher.Yarder.ChainsawCubicVolumePerHa;
                            this.currentBatch.ThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogThin.FellerBuncher.Yarder.ChainsawPMhPerHa;
                            this.currentBatch.ThinChainsawCrewWithFellerBuncherAndGrappleYoader[recordIndex] = longLogThin.FellerBuncher.Yoader.ChainsawCrew;
                            this.currentBatch.ThinChainsawUtilizationWithFellerBuncherAndGrappleYoader[recordIndex] = longLogThin.FellerBuncher.Yoader.ChainsawUtilization;
                            this.currentBatch.ThinChainsawCmhWithFellerBuncherAndGrappleYoader[recordIndex] = longLogThin.FellerBuncher.Yoader.ChainsawCubicVolumePerHa;
                            this.currentBatch.ThinChainsawPMhWithFellerBuncherAndGrappleYoader[recordIndex] = longLogThin.FellerBuncher.Yoader.ChainsawPMhPerHa;
                            this.currentBatch.ThinChainsawCrewWithTrackedHarvester[recordIndex] = longLogThin.TrackedHarvester.ChainsawCrew;
                            this.currentBatch.ThinChainsawUtilizationWithTrackedHarvester[recordIndex] = longLogThin.TrackedHarvester.ChainsawUtilization;
                            this.currentBatch.ThinChainsawCmhWithTrackedHarvester[recordIndex] = longLogThin.TrackedHarvester.ChainsawCubicVolumePerHa;
                            this.currentBatch.ThinChainsawPMhWithTrackedHarvester[recordIndex] = longLogThin.TrackedHarvester.ChainsawPMhPerHa;
                            this.currentBatch.ThinChainsawCrewWithWheeledHarvester[recordIndex] = longLogThin.WheeledHarvester.ChainsawCrew;
                            this.currentBatch.ThinChainsawUtilizationWithWheeledHarvester[recordIndex] = longLogThin.WheeledHarvester.ChainsawUtilization;
                            this.currentBatch.ThinChainsawCmhWithWheeledHarvester[recordIndex] = longLogThin.WheeledHarvester.ChainsawCubicVolumePerHa;
                            this.currentBatch.ThinChainsawPMhWithWheeledHarvester[recordIndex] = longLogThin.WheeledHarvester.ChainsawPMhPerHa;
                            this.currentBatch.ThinForwardingMethod[recordIndex] = ForwarderLoadingMethod.None;
                            this.currentBatch.ThinForwarderPMh[recordIndex] = Single.NaN;
                            this.currentBatch.ThinForwarderProductivity[recordIndex] = Single.NaN;
                            this.currentBatch.ThinForwardedWeight[recordIndex] = Single.NaN;
                            this.currentBatch.ThinGrappleSwingYarderPMhPerHectare[recordIndex] = longLogThin.Yarder.YarderPMhPerHectare;
                            this.currentBatch.ThinGrappleSwingYarderProductivity[recordIndex] = longLogThin.Yarder.YarderProductivity;
                            this.currentBatch.ThinGrappleSwingYarderOverweightFirstLogsPerHectare[recordIndex] = longLogThin.Yarder.OverweightFirstLogsPerHa;
                            this.currentBatch.ThinGrappleYoaderPMhPerHectare[recordIndex] = longLogThin.Yoader.YarderPMhPerHectare;
                            this.currentBatch.ThinGrappleYoaderProductivity[recordIndex] = longLogThin.Yoader.YarderProductivity;
                            this.currentBatch.ThinGrappleYoaderOverweightFirstLogsPerHectare[recordIndex] = longLogThin.Yoader.OverweightFirstLogsPerHa;
                            this.currentBatch.ThinProcessorPMhWithGrappleSwingYarder[recordIndex] = longLogThin.Yarder.ProcessorPMhPerHa;
                            this.currentBatch.ThinProcessorProductivityWithGrappleSwingYarder[recordIndex] = longLogThin.Yarder.ProcessorProductivity;
                            this.currentBatch.ThinProcessorPMhWithGrappleYoader[recordIndex] = longLogThin.Yoader.ProcessorPMhPerHa;
                            this.currentBatch.ThinProcessorProductivityWithGrappleYoader[recordIndex] = longLogThin.Yoader.ProcessorProductivity;
                            this.currentBatch.ThinLoadedWeight[recordIndex] = longLogThin.FellerBuncher.LoadedWeightPerHa;
                        }
                        else
                        {
                            throw new NotSupportedException($"Unhandled thinning of type {thinFinancialValue.GetType().Name}.");
                        }
                    }
                    else
                    {
                        this.currentBatch.ThinFallerPMh[recordIndex] = Single.NaN; // no thin in this timestep so no data
                        this.currentBatch.ThinFallerProductivity[recordIndex] = Single.NaN;
                        this.currentBatch.ThinFellerBuncherPMh[recordIndex] = Single.NaN;
                        this.currentBatch.ThinFellerBuncherProductivity[recordIndex] = Single.NaN;
                        this.currentBatch.ThinTrackedHarvesterPMh[recordIndex] = Single.NaN;
                        this.currentBatch.ThinTrackedHarvesterProductivity[recordIndex] = Single.NaN;
                        this.currentBatch.ThinWheeledHarvesterPMh[recordIndex] = Single.NaN;
                        this.currentBatch.ThinWheeledHarvesterProductivity[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder[recordIndex] = ChainsawCrewType.None;
                        this.currentBatch.ThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawCrewWithFellerBuncherAndGrappleYoader[recordIndex] = ChainsawCrewType.None;
                        this.currentBatch.ThinChainsawUtilizationWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawCmhWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawPMhWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawCrewWithTrackedHarvester[recordIndex] = ChainsawCrewType.None;
                        this.currentBatch.ThinChainsawUtilizationWithTrackedHarvester[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawCmhWithTrackedHarvester[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawPMhWithTrackedHarvester[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawCrewWithWheeledHarvester[recordIndex] = ChainsawCrewType.None;
                        this.currentBatch.ThinChainsawUtilizationWithWheeledHarvester[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawCmhWithWheeledHarvester[recordIndex] = Single.NaN;
                        this.currentBatch.ThinChainsawPMhWithWheeledHarvester[recordIndex] = Single.NaN;
                        this.currentBatch.ThinForwardingMethod[recordIndex] = ForwarderLoadingMethod.None;
                        this.currentBatch.ThinForwarderPMh[recordIndex] = Single.NaN;
                        this.currentBatch.ThinForwarderProductivity[recordIndex] = Single.NaN;
                        this.currentBatch.ThinForwardedWeight[recordIndex] = Single.NaN;
                        this.currentBatch.ThinGrappleSwingYarderPMhPerHectare[recordIndex] = Single.NaN;
                        this.currentBatch.ThinGrappleSwingYarderProductivity[recordIndex] = Single.NaN;
                        this.currentBatch.ThinGrappleSwingYarderOverweightFirstLogsPerHectare[recordIndex] = Single.NaN;
                        this.currentBatch.ThinGrappleYoaderPMhPerHectare[recordIndex] = Single.NaN;
                        this.currentBatch.ThinGrappleYoaderProductivity[recordIndex] = Single.NaN;
                        this.currentBatch.ThinGrappleYoaderOverweightFirstLogsPerHectare[recordIndex] = Single.NaN;
                        this.currentBatch.ThinProcessorPMhWithGrappleSwingYarder[recordIndex] = Single.NaN;
                        this.currentBatch.ThinProcessorProductivityWithGrappleSwingYarder[recordIndex] = Single.NaN;
                        this.currentBatch.ThinProcessorPMhWithGrappleYoader[recordIndex] = Single.NaN;
                        this.currentBatch.ThinProcessorProductivityWithGrappleYoader[recordIndex] = Single.NaN;
                        this.currentBatch.ThinLoadedWeight[recordIndex] = Single.NaN;
                    }

                    this.currentBatch.RegenFallerPMh[recordIndex] = longLogRegenHarvest.Fallers.ChainsawPMhPerHa;
                    this.currentBatch.RegenFallerProductivity[recordIndex] = longLogRegenHarvest.Fallers.ChainsawProductivity;
                    this.currentBatch.RegenFellerBuncherPMh[recordIndex] = longLogRegenHarvest.FellerBuncher.FellerBuncherPMhPerHa;
                    this.currentBatch.RegenFellerBuncherProductivity[recordIndex] = longLogRegenHarvest.FellerBuncher.FellerBuncherProductivity;
                    this.currentBatch.RegenTrackedHarvesterPMh[recordIndex] = longLogRegenHarvest.TrackedHarvester.HarvesterPMhPerHa;
                    this.currentBatch.RegenTrackedHarvesterProductivity[recordIndex] = longLogRegenHarvest.TrackedHarvester.HarvesterProductivity;
                    this.currentBatch.RegenWheeledHarvesterPMh[recordIndex] = longLogRegenHarvest.WheeledHarvester.HarvesterPMhPerHa;
                    this.currentBatch.RegenWheeledHarvesterProductivity[recordIndex] = longLogRegenHarvest.WheeledHarvester.HarvesterProductivity;
                    this.currentBatch.RegenChainsawCrewWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.ChainsawCrew;
                    this.currentBatch.RegenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder [recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.ChainsawUtilization;
                    this.currentBatch.RegenChainsawCmhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.ChainsawCubicVolumePerHa;
                    this.currentBatch.RegenChainsawPMhWithFellerBuncherAndGrappleSwingYarder [recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.ChainsawPMhPerHa;
                    this.currentBatch.RegenChainsawCrewWithFellerBuncherAndGrappleYoader[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.ChainsawCrew;
                    this.currentBatch.RegenChainsawUtilizationWithFellerBuncherAndGrappleYoader[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.ChainsawUtilization;
                    this.currentBatch.RegenChainsawCmhWithFellerBuncherAndGrappleYoader[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.ChainsawCubicVolumePerHa;
                    this.currentBatch.RegenChainsawPMhWithFellerBuncherAndGrappleYoader[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.ChainsawPMhPerHa;
                    this.currentBatch.RegenChainsawCrewWithTrackedHarvester[recordIndex] = longLogRegenHarvest.TrackedHarvester.ChainsawCrew;
                    this.currentBatch.RegenChainsawUtilizationWithTrackedHarvester[recordIndex] = longLogRegenHarvest.TrackedHarvester.ChainsawUtilization;
                    this.currentBatch.RegenChainsawCmhWithTrackedHarvester[recordIndex] = longLogRegenHarvest.TrackedHarvester.ChainsawCubicVolumePerHa;
                    this.currentBatch.RegenChainsawPMhWithTrackedHarvester[recordIndex] = longLogRegenHarvest.TrackedHarvester.ChainsawPMhPerHa;
                    this.currentBatch.RegenChainsawCrewWithWheeledHarvester[recordIndex] = longLogRegenHarvest.WheeledHarvester.ChainsawCrew;
                    this.currentBatch.RegenChainsawUtilizationWithWheeledHarvester[recordIndex] = longLogRegenHarvest.WheeledHarvester.ChainsawUtilization;
                    this.currentBatch.RegenChainsawCmhWithWheeledHarvester[recordIndex] = longLogRegenHarvest.WheeledHarvester.ChainsawCubicVolumePerHa;
                    this.currentBatch.RegenChainsawPMhWithWheeledHarvester[recordIndex] = longLogRegenHarvest.WheeledHarvester.ChainsawPMhPerHa;
                    this.currentBatch.RegenGrappleSwingYarderPMhPerHectare[recordIndex] = longLogRegenHarvest.Yarder.YarderPMhPerHectare;
                    this.currentBatch.RegenGrappleSwingYarderProductivity[recordIndex] = longLogRegenHarvest.Yarder.YarderProductivity;
                    this.currentBatch.RegenGrappleSwingYarderOverweightFirstLogsPerHectare[recordIndex] = longLogRegenHarvest.Yarder.OverweightFirstLogsPerHa;
                    this.currentBatch.RegenGrappleYoaderPMhPerHectare[recordIndex] = longLogRegenHarvest.Yoader.YarderPMhPerHectare;
                    this.currentBatch.RegenGrappleYoaderProductivity[recordIndex] = longLogRegenHarvest.Yoader.YarderProductivity;
                    this.currentBatch.RegenGrappleYoaderOverweightFirstLogsPerHectare[recordIndex] = longLogRegenHarvest.Yoader.OverweightFirstLogsPerHa;
                    this.currentBatch.RegenProcessorPMhWithGrappleSwingYarder[recordIndex] = longLogRegenHarvest.Yarder.ProcessorPMhPerHa;
                    this.currentBatch.RegenProcessorProductivityWithGrappleSwingYarder[recordIndex] = longLogRegenHarvest.Yarder.ProcessorProductivity;
                    this.currentBatch.RegenProcessorPMhWithGrappleYoader[recordIndex] = longLogRegenHarvest.Yoader.ProcessorPMhPerHa;
                    this.currentBatch.RegenProcessorProductivityWithGrappleYoader[recordIndex] = longLogRegenHarvest.Yoader.ProcessorProductivity;
                    this.currentBatch.RegenLoadedWeight[recordIndex] = longLogRegenHarvest.FellerBuncher.LoadedWeightPerHa;
                }

                if (year != Constant.NoDataInt16)
                {
                    year += (Int16)trajectory.PeriodLengthInYears;
                }
            }

            this.RecordCount += periodsToCopy;
        }

        public void Add(StandTrajectory trajectory, WriteStandTrajectoryContext writeContext)
        {
            Debug.Assert((writeContext.EndOfRotationPeriod >= 0) && (writeContext.FinancialIndex >= 0));

            // get position and remaining space in current batch, assuming batch length has single record granularity.
            int periodsToCopy = writeContext.GetPeriodsToWrite(trajectory);
            int startIndexInRecordBatch = this.RecordCount % this.MaximumBatchLength;
            int capacityRemainingInCurrentBatch = this.MaximumBatchLength - startIndexInRecordBatch;
            int periodsToCopyToCurrentBatch = Int32.Min(periodsToCopy, capacityRemainingInCurrentBatch);

            if (startIndexInRecordBatch == 0)
            {
                this.AppendNewBatch(writeContext);
            }
            this.Add(trajectory, startPeriod: 0, writeContext, startIndexInRecordBatch, periodsToCopyToCurrentBatch);

            int periodsRemainingToCopy = periodsToCopy - periodsToCopyToCurrentBatch;
            if (periodsRemainingToCopy > 0)
            {
                this.AppendNewBatch(writeContext);
                this.Add(trajectory, startPeriod: periodsToCopyToCurrentBatch, writeContext, startIndexInRecordBatch: 0, periodsRemainingToCopy);
            }
        }

        private void AppendNewBatch(WriteStandTrajectoryContext writeContext)
        {
            int capacityInRecords = Int32.Min(this.TotalNumberOfRecords - this.RecordCount, this.MaximumBatchLength);
            this.currentBatch = new(writeContext, capacityInRecords);

            // repackage arrays into Arrow record batch
            // Order must match schema. Mismatches result in column data swaps or corruption, depending on sizes of data elements.
            this.RecordBatches.Add(new(this.Schema, this.currentBatch.AsArrowArrays(this.Schema), capacityInRecords));
        }

        private static Schema CreateSchema(WriteStandTrajectoryContext writeContext)
        {
            // identifiers
            List<Field> fields =
            [
                new("stand", UInt32Type.Default, false),
                new("thin1", Int16Type.Default, false),
                new("thin2", Int16Type.Default, false),
                new("thin3", Int16Type.Default, false),
                new("rotation", Int16Type.Default, false),
                new("financialScenario", UInt32Type.Default, false),
                new("year", Int16Type.Default, false),
                new("standAge", Int16Type.Default, false)
            ];

            Dictionary<string, string> metadata = new()
            {
                // stand and stand trajectory coordinate always on
                { "stand", "stand ID" },
                { "thin1", "age of first thin in years or -1 for no thin" },
                { "thin2", "age of second thin in years or -1 for no thin" },
                { "thin3", "age of third thin in years or -1 for no thin" },
                { "rotation", "rotation age in years" },
                { "financialScenario", "index of financial scenario used in cost calculations" },
                { "year", "calendar year, CE, if specified" },
                { "standAge", "nominal age of dominant and codominant trees in stand, years" },
            };

            // tree growth
            if (writeContext.NoTreeGrowth == false)
            {
                fields.AddRange([ new("TPH", FloatType.Default, false),
                                  new("QMD", FloatType.Default, false),
                                  new("Htop", FloatType.Default, false),
                                  new("BA", FloatType.Default, false),
                                  new("SDI", FloatType.Default, false),
                                  new("standingCmh", FloatType.Default, false),
                                  new("standingMbfh", FloatType.Default, false),
                                  new("thinCmh", FloatType.Default, false),
                                  new("thinMbfh", FloatType.Default, false),
                                  new("BAremoved", FloatType.Default, false),
                                  new("BAintensity", FloatType.Default, false),
                                  new("TPHdecrease", FloatType.Default, false) ]);
                metadata.Add("TPH", "trees per hectare");
                metadata.Add("QMD", "quadratic mean diameter, cm");
                metadata.Add("Htop", "H100, m");
                metadata.Add("BA", "basal area, m² ha⁻¹");
                metadata.Add("SDI", "Reineke SDI");
                metadata.Add("standingCmh", "standing merchantable cubic volume, BC Firmwood m³ ha⁻¹");
                metadata.Add("standingMbfh", "standing merchantable board foot volume in 12.2 m (40 foot) logs to a 12.7 cm (5 inch) top, Scribner.C MBF ha⁻¹");
                metadata.Add("thinCmh", "merchantable volume removed in thinning, BC Firmwood m³ ha⁻¹");
                metadata.Add("thinMbfh", "merchantable volume removed in thinning, Scribner.C MBF ha⁻¹");
                metadata.Add("BAremoved", "basal area removed in thinning, m² ha⁻¹");
                metadata.Add("BAintensity", "fraction of previous timestep’s basal area removed in thinning");
                metadata.Add("TPHdecrease", "total decrease in trees per hectare from thinning and mortality");
            }

            // financial
            if (writeContext.NoFinancial == false)
            {
                fields.AddRange([ new("NPV", FloatType.Default, false),
                                  new("LEV", FloatType.Default, false) ]);
                metadata.Add("NPV", "the net present value of a thin at this time step, if one occurs otherwise, the net present value of a harvest rotation at the stand age, US$ ha⁻¹");
                metadata.Add("LEV", "land expectation value of harvest rotation at stand age, US$ ha⁻¹");
            }

            // carbon - not supported for now
            if (writeContext.NoCarbon == false)
            {
                //fields.AddRange([ new("liveTreeBiomass", FloatType.Default, false),
                //                  new("SPH", FloatType.Default, false),
                //                  new("snagQMD", FloatType.Default, false) ]);
                //metadata.Add("liveTreeBiomass", "biomass of live trees, kg ha⁻¹");
                //metadata.Add("SPH", "snags per hectare");
                //metadata.Add("snagQMD", "quadratic mean diameter of snags, cm");
            }

            // harvest cost
            if (writeContext.NoHarvestCosts == false)
            {
                fields.AddRange([ new("thinMinCostSystem", UInt8Type.Default, false),
                                  new("thinFallerGrappleSwingYarderCost", FloatType.Default, false),
                                  new("thinFallerGrappleYoaderCost", FloatType.Default, false),
                                  new("thinFellerBuncherGrappleSwingYarderCost", FloatType.Default, false),
                                  new("thinFellerBuncherGrappleYoaderCost", FloatType.Default, false),
                                  new("thinTrackedHarvesterForwarderCost", FloatType.Default, false),
                                  new("thinTrackedHarvesterGrappleSwingYarderCost", FloatType.Default, false),
                                  new("thinTrackedHarvesterGrappleYoaderCost", FloatType.Default, false),
                                  new("thinWheeledHarvesterForwarderCost", FloatType.Default, false),
                                  new("thinWheeledHarvesterGrappleSwingYarderCost", FloatType.Default, false),
                                  new("thinWheeledHarvesterGrappleYoaderCost", FloatType.Default, false),
                                  new("thinTaskCost", FloatType.Default, false),
                                  new("regenMinCostSystem", UInt8Type.Default, false),
                                  new("regenFallerGrappleSwingYarderCost", FloatType.Default, false),
                                  new("regenFallerGrappleYoaderCost", FloatType.Default, false),
                                  new("regenFellerBuncherGrappleSwingYarderCost", FloatType.Default, false),
                                  new("regenFellerBuncherGrappleYoaderCost", FloatType.Default, false),
                                  new("regenTrackedHarvesterGrappleSwingYarderCost", FloatType.Default, false),
                                  new("regenTrackedHarvesterGrappleYoaderCost", FloatType.Default, false),
                                  new("regenWheeledHarvesterGrappleSwingYarderCost", FloatType.Default, false),
                                  new("regenWheeledHarvesterGrappleYoaderCost", FloatType.Default, false),
                                  new("regenTaskCost", FloatType.Default, false),
                                  new("reforestationNpv", FloatType.Default, false) ]);
                metadata.Add("thinMinCostSystem", "enum indicating lowest cost system for thinning");
                metadata.Add("thinFallerGrappleSwingYarderCost", "total felling and stump to mill extraction cost for thin if using hand falling, a swing yarder with whole tree grapple yarding, processor, and loader, US$ ha⁻¹");
                metadata.Add("thinFallerGrappleYoaderCost", "total felling and stump to mill extraction cost for thin if using hand falling, a yoader with whole tree grapple yarding, processor, and loader, US$ ha⁻¹");
                metadata.Add("thinFellerBuncherGrappleSwingYarderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) feller-buncher, swing yarder with whole tree grapple yarding, processor, and loader, US$ ha⁻¹");
                metadata.Add("thinFellerBuncherGrappleYoaderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) feller-buncher, yoader with whole tree grapple yarding, processor, and loader, US$ ha⁻¹");
                metadata.Add("thinTrackedHarvesterForwarderCost", "total felling and stump to mill extraction cost for thin if using (tethered) tracked harvester and forwarder, US$ ha⁻¹");
                metadata.Add("thinTrackedHarvesterGrappleSwingYarderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) tracked harvester, swing yarder with long log grapple yarding, and loader, US$ ha⁻¹");
                metadata.Add("thinTrackedHarvesterGrappleYoaderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) tracked harvester, yoader with long log grapple yarding, and loader, US$ ha⁻¹");
                metadata.Add("thinWheeledHarvesterForwarderCost", "total felling and stump to mill extraction cost for thin if using (tethered) wheeled harvester and forwarder, US$ ha⁻¹");
                metadata.Add("thinWheeledHarvesterGrappleSwingYarderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) eight-wheel harvester, swing yarder with long log grapple yarding, and loader, US$ ha⁻¹");
                metadata.Add("thinWheeledHarvesterGrappleYoaderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) eight-wheel harvester, yoader with long log grapple yarding, and loader, US$ ha⁻¹");
                metadata.Add("thinTaskCost", "cost of slash disposal, road maintenance, and other harvest related tasks for thinning, US$ ha⁻¹");
                metadata.Add("regenMinCostSystem", "enum indicating lowest cost system for regeneration harvest");
                metadata.Add("regenFallerGrappleSwingYarderCost", "total felling and stump to mill extraction cost for regeneration harvest if using hand falling, a swing yarder with whole tree grapple yarding, processor, and loader, US$ ha⁻¹");
                metadata.Add("regenFallerGrappleYoaderCost", "total felling and stump to mill extraction cost for regeneration harvest if using hand falling, a yoader with whole tree grapple yarding, processor, and loader, US$ ha⁻¹");
                metadata.Add("regenFellerBuncherGrappleSwingYarderCost", "total felling and stump to mill extraction cost using a (tethered) feller-buncher, swing yarder with whole tree grapple yarding, processor, and loader, US$ ha⁻¹");
                metadata.Add("regenFellerBuncherGrappleYoaderCost", "total felling and stump to mill extraction cost using a (tethered) feller-buncher, yoader with whole tree grapple yarding, processor, and loader, US$ ha⁻¹");
                metadata.Add("regenTrackedHarvesterGrappleSwingYarderCost", "total felling and stump to mill extraction cost using a (tethered) tracked harvester, swing yarder with long log grapple yarding, and loader, US$ ha⁻¹");
                metadata.Add("regenTrackedHarvesterGrappleYoaderCost", "total felling and stump to mill extraction cost using a (tethered) tracked harvester, yoader with long log grapple yarding, and loader, US$ ha⁻¹");
                metadata.Add("regenWheeledHarvesterGrappleSwingYarderCost", "total felling and stump to mill extraction cost using a (tethered) wheeled harvester, swing yarder with long log grapple yarding, and loader, US$ ha⁻¹");
                metadata.Add("regenWheeledHarvesterGrappleYoaderCost", "total felling and stump to mill extraction cost using a (tethered) wheeled harvester, yoader with long log grapple yarding, and loader, US$ ha⁻¹");
                metadata.Add("regenTaskCost", "cost of slash disposal, road maintenance, reforestation, and other harvest related tasks for thinning, US$ ha⁻¹");
                metadata.Add("reforestationNpv", "net present value of reforestation if a regeneration harvest is performed at this timestep, US$ ha⁻¹");
            }

            // timber sorts
            if (writeContext.NoTimberSorts == false)
            {
                fields.AddRange([ new("thinLogs2S", FloatType.Default, false),
                                  new("thinLogs3S", FloatType.Default, false),
                                  new("thinLogs4S", FloatType.Default, false),
                                  new("thinCmh2S", FloatType.Default, false),
                                  new("thinCmh3S", FloatType.Default, false),
                                  new("thinCmh4S", FloatType.Default, false),
                                  new("thinMbfh2S", FloatType.Default, false),
                                  new("thinMbfh3S", FloatType.Default, false),
                                  new("thinMbfh4S", FloatType.Default, false),
                                  new("thinPond2S", FloatType.Default, false),
                                  new("thinPond3S", FloatType.Default, false),
                                  new("thinPond4S", FloatType.Default, false),
                                  new("standingLogs2S", FloatType.Default, false),
                                  new("standingLogs3S", FloatType.Default, false),
                                  new("standingLogs4S", FloatType.Default, false),
                                  new("standingCmh2S", FloatType.Default, false),
                                  new("standingCmh3S", FloatType.Default, false),
                                  new("standingCmh4S", FloatType.Default, false),
                                  new("standingMbfh2S", FloatType.Default, false),
                                  new("standingMbfh3S", FloatType.Default, false),
                                  new("standingMbfh4S", FloatType.Default, false),
                                  new("regenPond2S", FloatType.Default, false),
                                  new("regenPond3S", FloatType.Default, false),
                                  new("regenPond4S", FloatType.Default, false) ]);
                metadata.Add("thinLogs2S", "2 saw logs cut during thinning by sort (2, 3, or 4 saw), logs ha⁻¹");
                metadata.Add("thinLogs3S", "3 saw logs cut during thinning by sort (2, 3, or 4 saw), logs ha⁻¹");
                metadata.Add("thinLogs4S", "4 saw logs cut during thinning by sort (2, 3, or 4 saw), logs ha⁻¹");
                metadata.Add("thinCmh2S", "merchantable 2 saw volume thinned by sort, BC Firmwood m³ ha⁻¹");
                metadata.Add("thinCmh3S", "merchantable 3 saw volume thinned by sort, BC Firmwood m³ ha⁻¹");
                metadata.Add("thinCmh4S", "merchantable 4 saw volume thinned by sort, BC Firmwood m³ ha⁻¹");
                metadata.Add("thinMbfh2S", "merchantable 2 saw volume thinned by sort, Scribner.C MBF ha-1");
                metadata.Add("thinMbfh3S", "merchantable 3 saw volume thinned by sort, Scribner.C MBF ha-1");
                metadata.Add("thinMbfh4S", "merchantable 4 saw volume thinned by sort, Scribner.C MBF ha-1");
                metadata.Add("thinPond2S", "pond value of 2 saw logs cut during thinning by sort, US$ ha⁻¹");
                metadata.Add("thinPond3S", "pond value of 3 saw logs cut during thinning by sort, US$ ha⁻¹");
                metadata.Add("thinPond4S", "pond value of 4 saw logs cut during thinning by sort, US$ ha⁻¹");
                metadata.Add("standingLogs2S", "12.2 m 2 saw logs buckable from standing trees by sort, logs ha⁻¹");
                metadata.Add("standingLogs3S", "12.2 m 3 saw logs buckable from standing trees by sort, logs ha⁻¹");
                metadata.Add("standingLogs4S", "12.2 m 4 saw logs buckable from standing trees by sort, logs ha⁻¹");
                metadata.Add("standingCmh2S", "merchantable volume of 12.2 m 2 saw logs in standing trees by sort, BC Firmwood m³ ha⁻¹");
                metadata.Add("standingCmh3S", "merchantable volume of 12.2 m 3 saw logs in standing trees by sort, BC Firmwood m³ ha⁻¹");
                metadata.Add("standingCmh4S", "merchantable volume of 12.2 m 4 saw logs in standing trees by sort, BC Firmwood m³ ha⁻¹");
                metadata.Add("standingMbfh2S", "merchantable volume of 12.2 m 2 saw logs in standing trees by sort, Scribner.C MBF ha⁻¹");
                metadata.Add("standingMbfh3S", "merchantable volume of 12.2 m 3 saw logs in standing trees by sort, Scribner.C MBF ha⁻¹");
                metadata.Add("standingMbfh4S", "merchantable volume of 12.2 m 4 saw logs in standing trees by sort, Scribner.C MBF ha⁻¹");
                metadata.Add("regenPond2S", "pond value of standing 12.2 m 2 saw logs, US$ ha⁻¹");
                metadata.Add("regenPond3S", "pond value of standing 12.2 m 3 saw logs, US$ ha⁻¹");
                metadata.Add("regenPond4S", "pond value of standing 12.2 m 4 saw logs, US$ ha⁻¹");
            }

            // equipment productivity
            if (writeContext.NoEquipmentProductivity == false)
            {
                fields.AddRange([ new("thinFallerPMh", FloatType.Default, false),
                                  new("thinFallerProductivity", FloatType.Default, false),
                                  new("thinFellerBuncherPMh", FloatType.Default, false),
                                  new("thinFellerBuncherProductivity", FloatType.Default, false),
                                  new("thinTrackedHarvesterPMh", FloatType.Default, false),
                                  new("thinTrackedHarvesterProductivity", FloatType.Default, false),
                                  new("thinWheeledHarvesterPMh", FloatType.Default, false),
                                  new("thinWheeledHarvesterProductivity", FloatType.Default, false),
                                  new("thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder", UInt8Type.Default, false),
                                  new("thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                                  new("thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                                  new("thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                                  new("thinChainsawCrewWithFellerBuncherAndGrappleYoader", UInt8Type.Default, false),
                                  new("thinChainsawUtilizationWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                                  new("thinChainsawCmhWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                                  new("thinChainsawPMhWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                                  new("thinChainsawCrewWithTrackedHarvester", UInt8Type.Default, false),
                                  new("thinChainsawUtilizationWithTrackedHarvester", FloatType.Default, false),
                                  new("thinChainsawCmhWithTrackedHarvester", FloatType.Default, false),
                                  new("thinChainsawPMhWithTrackedHarvester", FloatType.Default, false),
                                  new("thinChainsawCrewWithWheeledHarvester", UInt8Type.Default, false),
                                  new("thinChainsawUtilizationWithWheeledHarvester", FloatType.Default, false),
                                  new("thinChainsawCmhWithWheeledHarvester", FloatType.Default, false),
                                  new("thinChainsawPMhWithWheeledHarvester", FloatType.Default, false),
                                  new("thinForwardingMethod", UInt8Type.Default, false),
                                  new("thinForwarderPMh", FloatType.Default, false),
                                  new("thinForwarderProductivity", FloatType.Default, false),
                                  new("thinForwardedWeight", FloatType.Default, false),
                                  new("thinGrappleSwingYarderPMhPerHectare", FloatType.Default, false),
                                  new("thinGrappleSwingYarderProductivity", FloatType.Default, false),
                                  new("thinGrappleSwingYarderOverweightFirstLogsPerHectare", FloatType.Default, false),
                                  new("thinGrappleYoaderPMhPerHectare", FloatType.Default, false),
                                  new("thinGrappleYoaderProductivity", FloatType.Default, false),
                                  new("thinGrappleYoaderOverweightFirstLogsPerHectare", FloatType.Default, false),
                                  new("thinProcessorPMhWithGrappleSwingYarder", FloatType.Default, false),
                                  new("thinProcessorProductivityWithGrappleSwingYarder", FloatType.Default, false),
                                  new("thinProcessorPMhWithGrappleYoader", FloatType.Default, false),
                                  new("thinProcessorProductivityWithGrappleYoader", FloatType.Default, false),
                                  new("thinLoadedWeight", FloatType.Default, false),
                                  new("regenFallerPMh", FloatType.Default, false),
                                  new("regenFallerProductivity", FloatType.Default, false),
                                  new("regenFellerBuncherPMh", FloatType.Default, false),
                                  new("regenFellerBuncherProductivity", FloatType.Default, false),
                                  new("regenTrackedHarvesterPMh", FloatType.Default, false),
                                  new("regenTrackedHarvesterProductivity", FloatType.Default, false),
                                  new("regenWheeledHarvesterPMh", FloatType.Default, false),
                                  new("regenWheeledHarvesterProductivity", FloatType.Default, false),
                                  new("regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder", UInt8Type.Default, false),
                                  new("regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                                  new("regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                                  new("regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                                  new("regenChainsawCrewWithFellerBuncherAndGrappleYoader", UInt8Type.Default, false),
                                  new("regenChainsawUtilizationWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                                  new("regenChainsawCmhWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                                  new("regenChainsawPMhWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                                  new("regenChainsawCrewWithTrackedHarvester", UInt8Type.Default, false),
                                  new("regenChainsawUtilizationWithTrackedHarvester", FloatType.Default, false),
                                  new("regenChainsawCmhWithTrackedHarvester", FloatType.Default, false),
                                  new("regenChainsawPMhWithTrackedHarvester", FloatType.Default, false),
                                  new("regenChainsawCrewWithWheeledHarvester", UInt8Type.Default, false),
                                  new("regenChainsawUtilizationWithWheeledHarvester", FloatType.Default, false),
                                  new("regenChainsawCmhWithWheeledHarvester", FloatType.Default, false),
                                  new("regenChainsawPMhWithWheeledHarvester", FloatType.Default, false),
                                  new("regenGrappleSwingYarderPMhPerHectare", FloatType.Default, false),
                                  new("regenGrappleSwingYarderProductivity", FloatType.Default, false),
                                  new("regenGrappleSwingYarderOverweightFirstLogsPerHectare", FloatType.Default, false),
                                  new("regenGrappleYoaderPMhPerHectare", FloatType.Default, false),
                                  new("regenGrappleYoaderProductivity", FloatType.Default, false),
                                  new("regenGrappleYoaderOverweightFirstLogsPerHectare", FloatType.Default, false),
                                  new("regenProcessorPMhWithGrappleSwingYarder", FloatType.Default, false),
                                  new("regenProcessorProductivityWithGrappleSwingYarder", FloatType.Default, false),
                                  new("regenProcessorPMhWithGrappleYoader", FloatType.Default, false),
                                  new("regenProcessorProductivityWithGrappleYoader", FloatType.Default, false),
                                  new("regenLoadedWeight", FloatType.Default, false) ]);
                metadata.Add("thinFallerPMh", "productive “machine” hours for hand fallers to perform felling and any needed bucking at the stump during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinFallerProductivity", "productivity of fallers during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinFellerBuncherPMh", "productive machine hours for a (tethered) feller-buncher to perform felling during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinFellerBuncherProductivity", "productivity of feller-buncher during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinTrackedHarvesterPMh", "productive machine hours for a (tethered) tracked harvester to perform felling and bucking during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinTrackedHarvesterProductivity", "productivity of tracked harvester during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinWheeledHarvesterPMh", "productive machine hours for a (tethered) eight-wheel harvester to perform felling and bucking during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinWheeledHarvesterProductivity", "productivity of eight-wheel harvester during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder", "enum indicating lowest cost chainsaw crew type to use with a feller-buncher, swing yarder, processor, and loader system");
                metadata.Add("thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder", "fraction of time chainsaw crew spends felling and/or bucking with short moves to the next tree versus making longer moves across the unit with a feller-buncher, swing yarder, processor, and loader system");
                metadata.Add("thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder", "merchantable volume felled and/or bucked by chainsaw crew following a feller-buncher and bucking for a swing yarder, m³ ha⁻¹");
                metadata.Add("thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder", "productive “machine” hours put in by chainsaw crew following a feller-buncher and bucking for a swing yarder, PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinChainsawCrewWithFellerBuncherAndGrappleYoader", "enum indicating lowest cost chainsaw crew type to use with a feller-buncher, yoader, processor, and loader system");
                metadata.Add("thinChainsawUtilizationWithFellerBuncherAndGrappleYoader", "fraction of time chainsaw crew spends felling and/or bucking with short moves to the next tree versus making longer moves across the unit with a feller-buncher, yoader, processor, and loader system");
                metadata.Add("thinChainsawCmhWithFellerBuncherAndGrappleYoader", "merchantable volume felled and/or bucked by chainsaw crew following a feller-buncher and bucking for a yoader, m³ ha⁻¹");
                metadata.Add("thinChainsawPMhWithFellerBuncherAndGrappleYoader", "productive “machine” hours put in by chainsaw crew following a feller-buncher and bucking for a yoader, PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinChainsawCrewWithTrackedHarvester", "enum indicating lowest cost chainsaw crew type to use with a tracked harvester");
                metadata.Add("thinChainsawUtilizationWithTrackedHarvester", "fraction of time chainsaw crew spends felling and/or bucking with short moves to the next tree versus making longer moves across the unit when supporting a tracked harvester");
                metadata.Add("thinChainsawCmhWithTrackedHarvester", "merchantable volume felled and/or bucked by chainsaw crew supporting a tracked harvester, m³ ha⁻¹");
                metadata.Add("thinChainsawPMhWithTrackedHarvester", "productive “machine” hours put in by chainsaw crew supporting an eight-wheel harvester, PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinChainsawCrewWithWheeledHarvester", "enum indicating lowest cost chainsaw crew type to use with an eight-wheel harvester");
                metadata.Add("thinChainsawCrewUtilizationWithWheeledHarvester", "fraction of time chainsaw crew spends felling and/or bucking with short moves to the next tree versus making longer moves across the unit when supporting an eight-wheel harvester");
                metadata.Add("thinChainsawCmhWithWheeledHarvester", "merchantable volume felled and/or bucked by chainsaw crew supporting an eight-wheel harvester, m³ ha⁻¹");
                metadata.Add("thinChainsawPMhWithWheeledHarvester", "productive “machine” hours put in by chainsaw crew supporting an eight-wheel harvester, PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinForwardingMethod", "enum indicating optimal loading of 2S, 3S, and 4S sorts on forwarder, currently assumes only a single species is harvested");
                metadata.Add("thinForwarderPMh", "productive machine hours for (tethered) wheeled forwarder to move logs from stump to road during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinForwarderProductivity", "productivity of wheeled forwarder during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinForwardedWeight", "total green weight of logs and retained bark forwarded to road, kg ha⁻¹");
                metadata.Add("thinGrappleSwingYarderPMhPerHectare", "productive machine hours for a swing yarder to grapple logs from stump to chute during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinGrappleSwingYarderProductivity", "productivity of grapple swing yarder during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinGrappleSwingYarderOverweightFirstLogsPerHectare", "number of first logs which must be bucked off to avoid a yarding whole tree weighting more than a swing yarder’s payload limit, logs ha⁻¹ (or, equivalently, trees per hectare)");
                metadata.Add("thinGrappleYoaderPMhPerHectare", "productive machine hours for a yoader to grapple logs from stump to chute during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinGrappleYoaderProductivity", "productivity of yoader during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinGrappleYoaderOverweightFirstLogsPerHectare", "number of first logs which must be bucked off to avoid a yarding whole tree weighting more than a yoader’s payload limit, logs ha⁻¹ (or, equivalently, trees per hectare)");
                metadata.Add("thinProcessorPMhWithGrappleSwingYarder", "productive machine hours for a processor following a swing yarder during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinProcessorProductivityWithGrappleSwingYarder", "productivity of processor paired with a swing yarder during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinProcessorPMhWithGrappleYoader", "productive machine hours for a processor following a yoader during thinning, PMh₀ ha⁻¹");
                metadata.Add("thinProcessorProductivityWithGrappleYoader", "productivity of processor paired with a yoader during thinning, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("thinLoadedWeight", "total green weight of cut to length logs loaded onto a mule train (harvester-forwarder thins) or long logs loaded onto a truck (yarded thins), kg ha⁻¹");
                metadata.Add("regenFallerPMh", "productive “machine” hours for hand fallers to perform felling and any needed bucking at the stump during regeneration harvest, PMh₀ ha⁻¹");
                metadata.Add("regenFallerProductivity", "productivity of fallers during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("regenFellerBuncherPMh", "productive machine hours for (tethered) feller-buncher to perform felling during regeneration harvest, PMh₀ ha⁻¹");
                metadata.Add("regenFellerBuncherProductivity", "productivity of feller-buncher during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("regenTrackedHarvesterPMh", "productive machine hours for (tethered) tracked harvester to perform felling and bucking during regeneration harvest, PMh₀ ha⁻¹");
                metadata.Add("regenTrackedHarvesterProductivity", "productivity of tracked harvester during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("regenWheeledHarvesterPMh", "productive machine hours for (tethered) eight-wheel wheel harvester to perform felling and bucking during regeneration harvest, PMh₀ ha⁻¹");
                metadata.Add("regenWheeledHarvesterProductivity", "productivity of eight-wheel harvester during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder", "most cost of effective type of chainsaw crew to support a feller-buncher in bucking logs to meet a grapple swing yarder’s payload capability");
                metadata.Add("regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder", "fraction of time chainsaw crew spends in wood production, as opposed to hiking to the next tree, when using a feller-buncher and swing yarder system, SMh SMh⁻¹");
                metadata.Add("regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder", "total merchantable wood volume of trees processed, at least partially, by chainsaw during regeneration harvest when using a feller-buncher and swing yarder system, m³ ha⁻¹");
                metadata.Add("regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder", "productive machine hours put in by chainsaw crew when using a feller-buncher and swing yarder system, PMh₀ ha⁻¹");
                metadata.Add("regenChainsawCrewWithFellerBuncherAndGrappleYoader", "most cost of effective type of chainsaw crew to support a feller-buncher in bucking logs to meet a grapple yoader’s payload capability");
                metadata.Add("regenChainsawUtilizationWithFellerBuncherAndGrappleYoader", "fraction of time chainsaw crew spends in wood production, as opposed to hiking to the next tree, when using a feller-buncher and yoader system, SMh SMh⁻¹");
                metadata.Add("regenChainsawCmhWithFellerBuncherAndGrappleYoader", "total merchantable wood volume of trees processed, at least partially, by chainsaw during regeneration harvest when using a feller-buncher and yoader system, m³ ha⁻¹");
                metadata.Add("regenChainsawPMhWithFellerBuncherAndGrappleYoader", "productive machine hours put in by chainsaw crew when using a feller-buncher and yoader system, PMh₀ ha⁻¹");
                metadata.Add("regenChainsawCrewWithTrackedHarvester", "most cost of effective type of chainsaw crew to support a tracked harvester in felling trees to meet a grapple yoader’s payload capability");
                metadata.Add("regenChainsawUtilizationWithTrackedHarvester", "fraction of time chainsaw crew spends in wood production, as opposed to hiking to the next tree, when using a tracked harvester, SMh SMh⁻¹");
                metadata.Add("regenChainsawCmhWithTrackedHarvester", "total merchantable wood volume of trees processed, at least partially, by chainsaw during regeneration harvest when using a tracked harvester, m³ ha⁻¹");
                metadata.Add("regenChainsawPMhWithTrackedHarvester", "productive machine hours put in by chainsaw crew when using a tracked harvester, PMh₀ ha⁻¹");
                metadata.Add("regenChainsawCrewWithWheeledHarvester", "most cost of effective type of chainsaw crew to support an eight-wheel harvester in felling trees to meet a grapple yoader’s payload capability");
                metadata.Add("regenChainsawUtilizationWithWheeledHarvester", "fraction of time chainsaw crew spends in wood production, as opposed to hiking to the next tree, when using an eight-wheel harvester, SMh SMh⁻¹");
                metadata.Add("regenChainsawCmhWithWheeledHarvester", "total merchantable wood volume of trees processed, at least partially, by chainsaw during regeneration harvest when using an eight-wheel harvester, m³ ha⁻¹");
                metadata.Add("regenChainsawPMhWithWheeledHarvester", "productive machine hours put in by chainsaw crew when using an eight wheeled harvester, PMh₀ ha⁻¹");
                metadata.Add("regenGrappleSwingYarderPMhPerHectare", "productive machine hours for a swing yarder grappling whole trees or bucked logs regeneration harvest, PMh₀ ha⁻¹");
                metadata.Add("regenGrappleSwingYarderProductivity", "productivity of swing yarder during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("regenGrappleSwingYarderOverweightFirstLogsPerHectare", "number of first logs which must be bucked off to avoid a yarding whole tree weighting more than a swing yarder’s payload limit, logs ha⁻¹ (or, equivalently, trees per hectare)");
                metadata.Add("regenGrappleYoaderPMhPerHectare", "productive machine hours for a yoader grappling whole trees or bucked logs during regeneration harvest, PMh₀ ha⁻¹");
                metadata.Add("regenGrappleYoaderProductivity", "productivity of yoader during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("regenGrappleYoaderOverweightFirstLogsPerHectare", "number of first logs which must be bucked off to avoid a yarding whole tree weighting more than a yoader’s payload limit, logs ha⁻¹ (or, equivalently, trees per hectare)");
                metadata.Add("regenProcessorPMhWithGrappleSwingYarder", "productive machine hours for a processor following a swing yarder during regeneration harvest, PMh₀ ha⁻¹");
                metadata.Add("regenProcessorProductivityWithGrappleSwingYarder", "productivity of processor paired with a swing yarder during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("regenProcessorPMhWithGrappleYoader", "productive machine hours for a processor following a yoader during regeneration harvest, PMh₀ ha⁻¹");
                metadata.Add("regenProcessorProductivityWithGrappleYoader", "productivity of processor paired with a yoader during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹");
                metadata.Add("regenLoadedWeight", "total weight of logs and retained bark loaded on log trucks during regeneration harvest, kg ha⁻¹");
            }

            return new Schema(fields, metadata);
        }

        public class StandTrajectoryBatch
        {
            // identifiers
            public UInt32[] Stand { get; private init; }
            public Int16[] Thin1 { get; private init; }
            public Int16[] Thin2 { get; private init; }
            public Int16[] Thin3 { get; private init; }
            public Int16[] Rotation { get; private init; }
            public UInt32[] FinancialScenario { get; private init; }
            public Int16[] Year { get; private init; }
            public Int16[] StandAge { get; private init; }
            // tree growth
            public float[] Tph { get; private init; }
            public float[] Qmd { get; private init; }
            public float[] HTop { get; private init; }
            public float[] BasalArea { get; private init; }
            public float[] ReinekeSdi { get; private init; }
            public float[] StandingCmh { get; private init; }
            public float[] StandingMbfh { get; private init; }
            public float[] ThinCmh { get; private init; }
            public float[] ThinMbfh { get; private init; }
            public float[] BAremoved { get; private init; }
            public float[] BAintensity { get; private init; }
            public float[] TphDecrease { get; private init; }
            // financial
            public float[] Npv { get; private init; }
            public float[] Lev { get; private init; }
            // carbon - not supported for now
            //public float[] LiveTreeBiomass { get; private init; }
            //public float[] SPH { get; private init; }
            //public float[] SnagQMD { get; private init; }
            // harvest cost
            public HarvestSystemEquipment[] ThinMinCostSystem { get; private init; }
            public float[] ThinFallerGrappleSwingYarderCost { get; private init; }
            public float[] ThinFallerGrappleYoaderCost { get; private init; }
            public float[] ThinFellerBuncherGrappleSwingYarderCost { get; private init; }
            public float[] ThinFellerBuncherGrappleYoaderCost { get; private init; }
            public float[] ThinTrackedHarvesterForwarderCost { get; private init; }
            public float[] ThinTrackedHarvesterGrappleSwingYarderCost { get; private init; }
            public float[] ThinTrackedHarvesterGrappleYoaderCost { get; private init; }
            public float[] ThinWheeledHarvesterForwarderCost { get; private init; }
            public float[] ThinWheeledHarvesterGrappleSwingYarderCost { get; private init; }
            public float[] ThinWheeledHarvesterGrappleYoaderCost { get; private init; }
            public float[] ThinTaskCost { get; private init; }
            public HarvestSystemEquipment[] RegenMinCostSystem { get; private init; }
            public float[] RegenFallerGrappleSwingYarderCost { get; private init; }
            public float[] RegenFallerGrappleYoaderCost { get; private init; }
            public float[] RegenFellerBuncherGrappleSwingYarderCost { get; private init; }
            public float[] RegenFellerBuncherGrappleYoaderCost { get; private init; }
            public float[] RegenTrackedHarvesterGrappleSwingYarderCost { get; private init; }
            public float[] RegenTrackedHarvesterGrappleYoaderCost { get; private init; }
            public float[] RegenWheeledHarvesterGrappleSwingYarderCost { get; private init; }
            public float[] RegenWheeledHarvesterGrappleYoaderCost { get; private init; }
            public float[] RegenTaskCost { get; private init; }
            public float[] ReforestationNpv { get; private init; }
            // timber sorts
            public float[] ThinLogs2S { get; private init; }
            public float[] ThinLogs3S { get; private init; }
            public float[] ThinLogs4S { get; private init; }
            public float[] ThinCmh2S { get; private init; }
            public float[] ThinCmh3S { get; private init; }
            public float[] ThinCmh4S { get; private init; }
            public float[] ThinMbfh2S { get; private init; }
            public float[] ThinMbfh3S { get; private init; }
            public float[] ThinMbfh4S { get; private init; }
            public float[] ThinPond2S { get; private init; }
            public float[] ThinPond3S { get; private init; }
            public float[] ThinPond4S { get; private init; }
            public float[] StandingLogs2S { get; private init; }
            public float[] StandingLogs3S { get; private init; }
            public float[] StandingLogs4S { get; private init; }
            public float[] StandingCmh2S { get; private init; }
            public float[] StandingCmh3S { get; private init; }
            public float[] StandingCmh4S { get; private init; }
            public float[] StandingMbfh2S { get; private init; }
            public float[] StandingMbfh3S { get; private init; }
            public float[] StandingMbfh4S { get; private init; }
            public float[] RegenPond2S { get; private init; }
            public float[] RegenPond3S { get; private init; }
            public float[] RegenPond4S { get; private init; }
            // equipment productivity
            public float[] ThinFallerPMh { get; private init; }
            public float[] ThinFallerProductivity { get; private init; }
            public float[] ThinFellerBuncherPMh { get; private init; }
            public float[] ThinFellerBuncherProductivity { get; private init; }
            public float[] ThinTrackedHarvesterPMh { get; private init; }
            public float[] ThinTrackedHarvesterProductivity { get; private init; }
            public float[] ThinWheeledHarvesterPMh { get; private init; }
            public float[] ThinWheeledHarvesterProductivity { get; private init; }
            public ChainsawCrewType[] ThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder { get; private init; }
            public float[] ThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder { get; private init; }
            public float[] ThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder { get; private init; }
            public float[] ThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder { get; private init; }
            public ChainsawCrewType[] ThinChainsawCrewWithFellerBuncherAndGrappleYoader { get; private init; }
            public float[] ThinChainsawUtilizationWithFellerBuncherAndGrappleYoader { get; private init; }
            public float[] ThinChainsawCmhWithFellerBuncherAndGrappleYoader { get; private init; }
            public float[] ThinChainsawPMhWithFellerBuncherAndGrappleYoader { get; private init; }
            public ChainsawCrewType[] ThinChainsawCrewWithTrackedHarvester { get; private init; }
            public float[] ThinChainsawUtilizationWithTrackedHarvester { get; private init; }
            public float[] ThinChainsawCmhWithTrackedHarvester { get; private init; }
            public float[] ThinChainsawPMhWithTrackedHarvester { get; private init; }
            public ChainsawCrewType[] ThinChainsawCrewWithWheeledHarvester { get; private init; }
            public float[] ThinChainsawUtilizationWithWheeledHarvester { get; private init; }
            public float[] ThinChainsawCmhWithWheeledHarvester { get; private init; }
            public float[] ThinChainsawPMhWithWheeledHarvester { get; private init; }
            public ForwarderLoadingMethod[] ThinForwardingMethod { get; private init; }
            public float[] ThinForwarderPMh { get; private init; }
            public float[] ThinForwarderProductivity { get; private init; }
            public float[] ThinForwardedWeight { get; private init; }
            public float[] ThinGrappleSwingYarderPMhPerHectare { get; private init; }
            public float[] ThinGrappleSwingYarderProductivity { get; private init; }
            public float[] ThinGrappleSwingYarderOverweightFirstLogsPerHectare { get; private init; }
            public float[] ThinGrappleYoaderPMhPerHectare { get; private init; }
            public float[] ThinGrappleYoaderProductivity { get; private init; }
            public float[] ThinGrappleYoaderOverweightFirstLogsPerHectare { get; private init; }
            public float[] ThinProcessorPMhWithGrappleSwingYarder { get; private init; }
            public float[] ThinProcessorProductivityWithGrappleSwingYarder { get; private init; }
            public float[] ThinProcessorPMhWithGrappleYoader { get; private init; }
            public float[] ThinProcessorProductivityWithGrappleYoader { get; private init; }
            public float[] ThinLoadedWeight { get; private init; }
            public float[] RegenFallerPMh { get; private init; }
            public float[] RegenFallerProductivity { get; private init; }
            public float[] RegenFellerBuncherPMh { get; private init; }
            public float[] RegenFellerBuncherProductivity { get; private init; }
            public float[] RegenTrackedHarvesterPMh { get; private init; }
            public float[] RegenTrackedHarvesterProductivity { get; private init; }
            public float[] RegenWheeledHarvesterPMh { get; private init; }
            public float[] RegenWheeledHarvesterProductivity { get; private init; }
            public ChainsawCrewType[] RegenChainsawCrewWithFellerBuncherAndGrappleSwingYarder { get; private init; }
            public float[] RegenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder { get; private init; }
            public float[] RegenChainsawCmhWithFellerBuncherAndGrappleSwingYarder { get; private init; }
            public float[] RegenChainsawPMhWithFellerBuncherAndGrappleSwingYarder { get; private init; }
            public ChainsawCrewType[] RegenChainsawCrewWithFellerBuncherAndGrappleYoader { get; private init; }
            public float[] RegenChainsawUtilizationWithFellerBuncherAndGrappleYoader { get; private init; }
            public float[] RegenChainsawCmhWithFellerBuncherAndGrappleYoader { get; private init; }
            public float[] RegenChainsawPMhWithFellerBuncherAndGrappleYoader { get; private init; }
            public ChainsawCrewType[] RegenChainsawCrewWithTrackedHarvester { get; private init; }
            public float[] RegenChainsawUtilizationWithTrackedHarvester { get; private init; }
            public float[] RegenChainsawCmhWithTrackedHarvester { get; private init; }
            public float[] RegenChainsawPMhWithTrackedHarvester { get; private init; }
            public ChainsawCrewType[] RegenChainsawCrewWithWheeledHarvester { get; private init; }
            public float[] RegenChainsawUtilizationWithWheeledHarvester { get; private init; }
            public float[] RegenChainsawCmhWithWheeledHarvester { get; private init; }
            public float[] RegenChainsawPMhWithWheeledHarvester { get; private init; }
            public float[] RegenGrappleSwingYarderPMhPerHectare { get; private init; }
            public float[] RegenGrappleSwingYarderProductivity { get; private init; }
            public float[] RegenGrappleSwingYarderOverweightFirstLogsPerHectare { get; private init; }
            public float[] RegenGrappleYoaderPMhPerHectare { get; private init; }
            public float[] RegenGrappleYoaderProductivity { get; private init; }
            public float[] RegenGrappleYoaderOverweightFirstLogsPerHectare { get; private init; }
            public float[] RegenProcessorPMhWithGrappleSwingYarder { get; private init; }
            public float[] RegenProcessorProductivityWithGrappleSwingYarder { get; private init; }
            public float[] RegenProcessorPMhWithGrappleYoader { get; private init; }
            public float[] RegenProcessorProductivityWithGrappleYoader { get; private init; }
            public float[] RegenLoadedWeight { get; private init; }

            public StandTrajectoryBatch(WriteStandTrajectoryContext writeContext, int capacityInRecords)
            {
                if (capacityInRecords < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(capacityInRecords), $"Batch must contain at least one record. Requested size was {capacityInRecords} records.");
                }

                // identifiers
                this.Stand = new UInt32[capacityInRecords];
                this.Thin1 = new Int16[capacityInRecords];
                this.Thin2 = new Int16[capacityInRecords];
                this.Thin3 = new Int16[capacityInRecords];
                this.Rotation = new Int16[capacityInRecords];
                this.FinancialScenario = new UInt32[capacityInRecords];
                this.Year = new Int16[capacityInRecords];
                this.StandAge = new Int16[capacityInRecords];

                if (writeContext.NoTreeGrowth == false)
                {
                    this.Tph = new float[capacityInRecords];
                    this.Qmd = new float[capacityInRecords];
                    this.HTop = new float[capacityInRecords];
                    this.BasalArea = new float[capacityInRecords];
                    this.ReinekeSdi = new float[capacityInRecords];
                    this.StandingCmh = new float[capacityInRecords];
                    this.StandingMbfh = new float[capacityInRecords];
                    this.ThinCmh = new float[capacityInRecords];
                    this.ThinMbfh = new float[capacityInRecords];
                    this.BAremoved = new float[capacityInRecords];
                    this.BAintensity = new float[capacityInRecords];
                    this.TphDecrease = new float[capacityInRecords];
                }
                else
                {
                    this.Tph = [];
                    this.Qmd = [];
                    this.HTop = [];
                    this.BasalArea = [];
                    this.ReinekeSdi = [];
                    this.StandingCmh = [];
                    this.StandingMbfh = [];
                    this.ThinCmh = [];
                    this.ThinMbfh = [];
                    this.BAremoved = [];
                    this.BAintensity = [];
                    this.TphDecrease = [];
                }

                if (writeContext.NoFinancial == false)
                {
                    this.Npv = new float[capacityInRecords];
                    this.Lev = new float[capacityInRecords];
                }
                else
                {
                    this.Npv = [];
                    this.Lev = [];
                }

                //if (writeContext.NoCarbon == false)
                //{
                //    this.liveTreeBiomass = new float[capacityInRecords];
                //    this.SPH = new float[capacityInRecords];
                //    this.snagQMD = new float[capacityInRecords];
                //}

                if (writeContext.NoHarvestCosts == false)
                {
                    this.ThinMinCostSystem = new HarvestSystemEquipment[capacityInRecords];
                    this.ThinFallerGrappleSwingYarderCost = new float[capacityInRecords];
                    this.ThinFallerGrappleYoaderCost = new float[capacityInRecords];
                    this.ThinFellerBuncherGrappleSwingYarderCost = new float[capacityInRecords];
                    this.ThinFellerBuncherGrappleYoaderCost = new float[capacityInRecords];
                    this.ThinTrackedHarvesterForwarderCost = new float[capacityInRecords];
                    this.ThinTrackedHarvesterGrappleSwingYarderCost = new float[capacityInRecords];
                    this.ThinTrackedHarvesterGrappleYoaderCost = new float[capacityInRecords];
                    this.ThinWheeledHarvesterForwarderCost = new float[capacityInRecords];
                    this.ThinWheeledHarvesterGrappleSwingYarderCost = new float[capacityInRecords];
                    this.ThinWheeledHarvesterGrappleYoaderCost = new float[capacityInRecords];
                    this.ThinTaskCost = new float[capacityInRecords];
                    this.RegenMinCostSystem = new HarvestSystemEquipment[capacityInRecords];
                    this.RegenFallerGrappleSwingYarderCost = new float[capacityInRecords];
                    this.RegenFallerGrappleYoaderCost = new float[capacityInRecords];
                    this.RegenFellerBuncherGrappleSwingYarderCost = new float[capacityInRecords];
                    this.RegenFellerBuncherGrappleYoaderCost = new float[capacityInRecords];
                    this.RegenTrackedHarvesterGrappleSwingYarderCost = new float[capacityInRecords];
                    this.RegenTrackedHarvesterGrappleYoaderCost = new float[capacityInRecords];
                    this.RegenWheeledHarvesterGrappleSwingYarderCost = new float[capacityInRecords];
                    this.RegenWheeledHarvesterGrappleYoaderCost = new float[capacityInRecords];
                    this.RegenTaskCost = new float[capacityInRecords];
                    this.ReforestationNpv = new float[capacityInRecords];
                }
                else
                {
                    this.ThinMinCostSystem = [];
                    this.ThinFallerGrappleSwingYarderCost = [];
                    this.ThinFallerGrappleYoaderCost = [];
                    this.ThinFellerBuncherGrappleSwingYarderCost = [];
                    this.ThinFellerBuncherGrappleYoaderCost = [];
                    this.ThinTrackedHarvesterForwarderCost = [];
                    this.ThinTrackedHarvesterGrappleSwingYarderCost = [];
                    this.ThinTrackedHarvesterGrappleYoaderCost = [];
                    this.ThinWheeledHarvesterForwarderCost = [];
                    this.ThinWheeledHarvesterGrappleSwingYarderCost = [];
                    this.ThinWheeledHarvesterGrappleYoaderCost = [];
                    this.ThinTaskCost = [];
                    this.RegenMinCostSystem = [];
                    this.RegenFallerGrappleSwingYarderCost = [];
                    this.RegenFallerGrappleYoaderCost = [];
                    this.RegenFellerBuncherGrappleSwingYarderCost = [];
                    this.RegenFellerBuncherGrappleYoaderCost = [];
                    this.RegenTrackedHarvesterGrappleSwingYarderCost = [];
                    this.RegenTrackedHarvesterGrappleYoaderCost = [];
                    this.RegenWheeledHarvesterGrappleSwingYarderCost = [];
                    this.RegenWheeledHarvesterGrappleYoaderCost = [];
                    this.RegenTaskCost = [];
                    this.ReforestationNpv = [];
                }

                if (writeContext.NoTimberSorts == false)
                {
                    this.ThinLogs2S = new float[capacityInRecords];
                    this.ThinLogs3S = new float[capacityInRecords];
                    this.ThinLogs4S = new float[capacityInRecords];
                    this.ThinCmh2S = new float[capacityInRecords];
                    this.ThinCmh3S = new float[capacityInRecords];
                    this.ThinCmh4S = new float[capacityInRecords];
                    this.ThinMbfh2S = new float[capacityInRecords];
                    this.ThinMbfh3S = new float[capacityInRecords];
                    this.ThinMbfh4S = new float[capacityInRecords];
                    this.ThinPond2S = new float[capacityInRecords];
                    this.ThinPond3S = new float[capacityInRecords];
                    this.ThinPond4S = new float[capacityInRecords];
                    this.StandingLogs2S = new float[capacityInRecords];
                    this.StandingLogs3S = new float[capacityInRecords];
                    this.StandingLogs4S = new float[capacityInRecords];
                    this.StandingCmh2S = new float[capacityInRecords];
                    this.StandingCmh3S = new float[capacityInRecords];
                    this.StandingCmh4S = new float[capacityInRecords];
                    this.StandingMbfh2S = new float[capacityInRecords];
                    this.StandingMbfh3S = new float[capacityInRecords];
                    this.StandingMbfh4S = new float[capacityInRecords];
                    this.RegenPond2S = new float[capacityInRecords];
                    this.RegenPond3S = new float[capacityInRecords];
                    this.RegenPond4S = new float[capacityInRecords];
                }
                else
                {
                    this.ThinLogs2S = [];
                    this.ThinLogs3S = [];
                    this.ThinLogs4S = [];
                    this.ThinCmh2S = [];
                    this.ThinCmh3S = [];
                    this.ThinCmh4S = [];
                    this.ThinMbfh2S = [];
                    this.ThinMbfh3S = [];
                    this.ThinMbfh4S = [];
                    this.ThinPond2S = [];
                    this.ThinPond3S = [];
                    this.ThinPond4S = [];
                    this.StandingLogs2S = [];
                    this.StandingLogs3S = [];
                    this.StandingLogs4S = [];
                    this.StandingCmh2S = [];
                    this.StandingCmh3S = [];
                    this.StandingCmh4S = [];
                    this.StandingMbfh2S = [];
                    this.StandingMbfh3S = [];
                    this.StandingMbfh4S = [];
                    this.RegenPond2S = [];
                    this.RegenPond3S = [];
                    this.RegenPond4S = [];
                }

                if (writeContext.NoEquipmentProductivity == false)
                {
                    this.ThinFallerPMh = new float[capacityInRecords];
                    this.ThinFallerProductivity = new float[capacityInRecords];
                    this.ThinFellerBuncherPMh = new float[capacityInRecords];
                    this.ThinFellerBuncherProductivity = new float[capacityInRecords];
                    this.ThinTrackedHarvesterPMh = new float[capacityInRecords];
                    this.ThinTrackedHarvesterProductivity = new float[capacityInRecords];
                    this.ThinWheeledHarvesterPMh = new float[capacityInRecords];
                    this.ThinWheeledHarvesterProductivity = new float[capacityInRecords];
                    this.ThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder = new ChainsawCrewType[capacityInRecords];
                    this.ThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = new float[capacityInRecords];
                    this.ThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder = new float[capacityInRecords];
                    this.ThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder = new float[capacityInRecords];
                    this.ThinChainsawCrewWithFellerBuncherAndGrappleYoader = new ChainsawCrewType[capacityInRecords];
                    this.ThinChainsawUtilizationWithFellerBuncherAndGrappleYoader = new float[capacityInRecords];
                    this.ThinChainsawCmhWithFellerBuncherAndGrappleYoader = new float[capacityInRecords];
                    this.ThinChainsawPMhWithFellerBuncherAndGrappleYoader = new float[capacityInRecords];
                    this.ThinChainsawCrewWithTrackedHarvester = new ChainsawCrewType[capacityInRecords];
                    this.ThinChainsawUtilizationWithTrackedHarvester = new float[capacityInRecords];
                    this.ThinChainsawCmhWithTrackedHarvester = new float[capacityInRecords];
                    this.ThinChainsawPMhWithTrackedHarvester = new float[capacityInRecords];
                    this.ThinChainsawCrewWithWheeledHarvester = new ChainsawCrewType[capacityInRecords];
                    this.ThinChainsawUtilizationWithWheeledHarvester = new float[capacityInRecords];
                    this.ThinChainsawCmhWithWheeledHarvester = new float[capacityInRecords];
                    this.ThinChainsawPMhWithWheeledHarvester = new float[capacityInRecords];
                    this.ThinForwardingMethod = new ForwarderLoadingMethod[capacityInRecords];
                    this.ThinForwarderPMh = new float[capacityInRecords];
                    this.ThinForwarderProductivity = new float[capacityInRecords];
                    this.ThinForwardedWeight = new float[capacityInRecords];
                    this.ThinGrappleSwingYarderPMhPerHectare = new float[capacityInRecords];
                    this.ThinGrappleSwingYarderProductivity = new float[capacityInRecords];
                    this.ThinGrappleSwingYarderOverweightFirstLogsPerHectare = new float[capacityInRecords];
                    this.ThinGrappleYoaderPMhPerHectare = new float[capacityInRecords];
                    this.ThinGrappleYoaderProductivity = new float[capacityInRecords];
                    this.ThinGrappleYoaderOverweightFirstLogsPerHectare = new float[capacityInRecords];
                    this.ThinProcessorPMhWithGrappleSwingYarder = new float[capacityInRecords];
                    this.ThinProcessorProductivityWithGrappleSwingYarder = new float[capacityInRecords];
                    this.ThinProcessorPMhWithGrappleYoader = new float[capacityInRecords];
                    this.ThinProcessorProductivityWithGrappleYoader = new float[capacityInRecords];
                    this.ThinLoadedWeight = new float[capacityInRecords];

                    this.RegenFallerPMh = new float[capacityInRecords];
                    this.RegenFallerProductivity = new float[capacityInRecords];
                    this.RegenFellerBuncherPMh = new float[capacityInRecords];
                    this.RegenFellerBuncherProductivity = new float[capacityInRecords];
                    this.RegenTrackedHarvesterPMh = new float[capacityInRecords];
                    this.RegenTrackedHarvesterProductivity = new float[capacityInRecords];
                    this.RegenWheeledHarvesterPMh = new float[capacityInRecords];
                    this.RegenWheeledHarvesterProductivity = new float[capacityInRecords];
                    this.RegenChainsawCrewWithFellerBuncherAndGrappleSwingYarder = new ChainsawCrewType[capacityInRecords];
                    this.RegenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = new float[capacityInRecords];
                    this.RegenChainsawCmhWithFellerBuncherAndGrappleSwingYarder = new float[capacityInRecords];
                    this.RegenChainsawPMhWithFellerBuncherAndGrappleSwingYarder = new float[capacityInRecords];
                    this.RegenChainsawCrewWithFellerBuncherAndGrappleYoader = new ChainsawCrewType[capacityInRecords];
                    this.RegenChainsawUtilizationWithFellerBuncherAndGrappleYoader = new float[capacityInRecords];
                    this.RegenChainsawCmhWithFellerBuncherAndGrappleYoader = new float[capacityInRecords];
                    this.RegenChainsawPMhWithFellerBuncherAndGrappleYoader = new float[capacityInRecords];
                    this.RegenChainsawCrewWithTrackedHarvester = new ChainsawCrewType[capacityInRecords];
                    this.RegenChainsawUtilizationWithTrackedHarvester = new float[capacityInRecords];
                    this.RegenChainsawCmhWithTrackedHarvester = new float[capacityInRecords];
                    this.RegenChainsawPMhWithTrackedHarvester = new float[capacityInRecords];
                    this.RegenChainsawCrewWithWheeledHarvester = new ChainsawCrewType[capacityInRecords];
                    this.RegenChainsawUtilizationWithWheeledHarvester = new float[capacityInRecords];
                    this.RegenChainsawCmhWithWheeledHarvester = new float[capacityInRecords];
                    this.RegenChainsawPMhWithWheeledHarvester = new float[capacityInRecords];
                    this.RegenGrappleSwingYarderPMhPerHectare = new float[capacityInRecords];
                    this.RegenGrappleSwingYarderProductivity = new float[capacityInRecords];
                    this.RegenGrappleSwingYarderOverweightFirstLogsPerHectare = new float[capacityInRecords];
                    this.RegenGrappleYoaderPMhPerHectare = new float[capacityInRecords];
                    this.RegenGrappleYoaderProductivity = new float[capacityInRecords];
                    this.RegenGrappleYoaderOverweightFirstLogsPerHectare = new float[capacityInRecords];
                    this.RegenProcessorPMhWithGrappleSwingYarder = new float[capacityInRecords];
                    this.RegenProcessorProductivityWithGrappleSwingYarder = new float[capacityInRecords];
                    this.RegenProcessorPMhWithGrappleYoader = new float[capacityInRecords];
                    this.RegenProcessorProductivityWithGrappleYoader = new float[capacityInRecords];
                    this.RegenLoadedWeight = new float[capacityInRecords];
                }
                else
                {
                    this.ThinFallerPMh = [];
                    this.ThinFallerProductivity = [];
                    this.ThinFellerBuncherPMh = [];
                    this.ThinFellerBuncherProductivity = [];
                    this.ThinTrackedHarvesterPMh = [];
                    this.ThinTrackedHarvesterProductivity = [];
                    this.ThinWheeledHarvesterPMh = [];
                    this.ThinWheeledHarvesterProductivity = [];
                    this.ThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder = [];
                    this.ThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = [];
                    this.ThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder = [];
                    this.ThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder = [];
                    this.ThinChainsawCrewWithFellerBuncherAndGrappleYoader = [];
                    this.ThinChainsawUtilizationWithFellerBuncherAndGrappleYoader = [];
                    this.ThinChainsawCmhWithFellerBuncherAndGrappleYoader = [];
                    this.ThinChainsawPMhWithFellerBuncherAndGrappleYoader = [];
                    this.ThinChainsawCrewWithTrackedHarvester = [];
                    this.ThinChainsawUtilizationWithTrackedHarvester = [];
                    this.ThinChainsawCmhWithTrackedHarvester = [];
                    this.ThinChainsawPMhWithTrackedHarvester = [];
                    this.ThinChainsawCrewWithWheeledHarvester = [];
                    this.ThinChainsawUtilizationWithWheeledHarvester = [];
                    this.ThinChainsawCmhWithWheeledHarvester = [];
                    this.ThinChainsawPMhWithWheeledHarvester = [];
                    this.ThinForwardingMethod = [];
                    this.ThinForwarderPMh = [];
                    this.ThinForwarderProductivity = [];
                    this.ThinForwardedWeight = [];
                    this.ThinGrappleSwingYarderPMhPerHectare = [];
                    this.ThinGrappleSwingYarderProductivity = [];
                    this.ThinGrappleSwingYarderOverweightFirstLogsPerHectare = [];
                    this.ThinGrappleYoaderPMhPerHectare = [];
                    this.ThinGrappleYoaderProductivity = [];
                    this.ThinGrappleYoaderOverweightFirstLogsPerHectare = [];
                    this.ThinProcessorPMhWithGrappleSwingYarder = [];
                    this.ThinProcessorProductivityWithGrappleSwingYarder = [];
                    this.ThinProcessorPMhWithGrappleYoader = [];
                    this.ThinProcessorProductivityWithGrappleYoader = [];
                    this.ThinLoadedWeight = [];

                    this.RegenFallerPMh = [];
                    this.RegenFallerProductivity = [];
                    this.RegenFellerBuncherPMh = [];
                    this.RegenFellerBuncherProductivity = [];
                    this.RegenTrackedHarvesterPMh = [];
                    this.RegenTrackedHarvesterProductivity = [];
                    this.RegenWheeledHarvesterPMh = [];
                    this.RegenWheeledHarvesterProductivity = [];
                    this.RegenChainsawCrewWithFellerBuncherAndGrappleSwingYarder = [];
                    this.RegenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = [];
                    this.RegenChainsawCmhWithFellerBuncherAndGrappleSwingYarder = [];
                    this.RegenChainsawPMhWithFellerBuncherAndGrappleSwingYarder = [];
                    this.RegenChainsawCrewWithFellerBuncherAndGrappleYoader = [];
                    this.RegenChainsawUtilizationWithFellerBuncherAndGrappleYoader = [];
                    this.RegenChainsawCmhWithFellerBuncherAndGrappleYoader = [];
                    this.RegenChainsawPMhWithFellerBuncherAndGrappleYoader = [];
                    this.RegenChainsawCrewWithTrackedHarvester = [];
                    this.RegenChainsawUtilizationWithTrackedHarvester = [];
                    this.RegenChainsawCmhWithTrackedHarvester = [];
                    this.RegenChainsawPMhWithTrackedHarvester = [];
                    this.RegenChainsawCrewWithWheeledHarvester = [];
                    this.RegenChainsawUtilizationWithWheeledHarvester = [];
                    this.RegenChainsawCmhWithWheeledHarvester = [];
                    this.RegenChainsawPMhWithWheeledHarvester = [];
                    this.RegenGrappleSwingYarderPMhPerHectare = [];
                    this.RegenGrappleSwingYarderProductivity = [];
                    this.RegenGrappleSwingYarderOverweightFirstLogsPerHectare = [];
                    this.RegenGrappleYoaderPMhPerHectare = [];
                    this.RegenGrappleYoaderProductivity = [];
                    this.RegenGrappleYoaderOverweightFirstLogsPerHectare = [];
                    this.RegenProcessorPMhWithGrappleSwingYarder = [];
                    this.RegenProcessorProductivityWithGrappleSwingYarder = [];
                    this.RegenProcessorPMhWithGrappleYoader = [];
                    this.RegenProcessorProductivityWithGrappleYoader = [];
                    this.RegenLoadedWeight = [];
                }
            }

            public IArrowArray[] AsArrowArrays(Schema schema)
            {
                IArrowArray[] arrowArrays = new IArrowArray[schema.FieldsList.Count];

                // identifiers
                arrowArrays[0] = this.Stand.AsArrowArray();
                arrowArrays[1] = this.Thin1.AsArrowArray();
                arrowArrays[2] = this.Thin2.AsArrowArray();
                arrowArrays[3] = this.Thin3.AsArrowArray();
                arrowArrays[4] = this.Rotation.AsArrowArray();
                arrowArrays[5] = this.FinancialScenario.AsArrowArray();
                arrowArrays[6] = this.Year.AsArrowArray();
                arrowArrays[7] = this.StandAge.AsArrowArray();

                int index = 7;
                if (this.Tph.Length > 0) // tree growth
                {
                    arrowArrays[++index] = this.Tph.AsArrowArray();
                    arrowArrays[++index] = this.Qmd.AsArrowArray();
                    arrowArrays[++index] = this.HTop.AsArrowArray();
                    arrowArrays[++index] = this.BasalArea.AsArrowArray();
                    arrowArrays[++index] = this.ReinekeSdi.AsArrowArray();
                    arrowArrays[++index] = this.StandingCmh.AsArrowArray();
                    arrowArrays[++index] = this.StandingMbfh.AsArrowArray();
                    arrowArrays[++index] = this.ThinCmh.AsArrowArray();
                    arrowArrays[++index] = this.ThinMbfh.AsArrowArray();
                    arrowArrays[++index] = this.BAremoved.AsArrowArray();
                    arrowArrays[++index] = this.BAintensity.AsArrowArray();
                    arrowArrays[++index] = this.TphDecrease.AsArrowArray();
                }

                if (this.Npv.Length > 0) // financial
                {
                    arrowArrays[++index] = this.Npv.AsArrowArray();
                    arrowArrays[++index] = this.Lev.AsArrowArray();
                }

                // carbon - not supported for now
                //this.liveTreeBiomass.AsArrowArray();
                //this.SPH.AsArrowArray();
                //this.snagQMD.AsArrowArray();

                if (this.ThinMinCostSystem.Length > 0) // harvest cost
                {
                    arrowArrays[++index] = this.ThinMinCostSystem.AsArrowArray();
                    arrowArrays[++index] = this.ThinFallerGrappleSwingYarderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinFallerGrappleYoaderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinFellerBuncherGrappleSwingYarderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinFellerBuncherGrappleYoaderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinTrackedHarvesterForwarderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinTrackedHarvesterGrappleSwingYarderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinTrackedHarvesterGrappleYoaderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinWheeledHarvesterForwarderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinWheeledHarvesterGrappleSwingYarderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinWheeledHarvesterGrappleYoaderCost.AsArrowArray();
                    arrowArrays[++index] = this.ThinTaskCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenMinCostSystem.AsArrowArray();
                    arrowArrays[++index] = this.RegenFallerGrappleSwingYarderCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenFallerGrappleYoaderCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenFellerBuncherGrappleSwingYarderCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenFellerBuncherGrappleYoaderCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenTrackedHarvesterGrappleSwingYarderCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenTrackedHarvesterGrappleYoaderCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenWheeledHarvesterGrappleSwingYarderCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenWheeledHarvesterGrappleYoaderCost.AsArrowArray();
                    arrowArrays[++index] = this.RegenTaskCost.AsArrowArray();
                    arrowArrays[++index] = this.ReforestationNpv.AsArrowArray();
                }

                if (this.ThinLogs2S.Length > 0) // timber sorts
                {
                    arrowArrays[++index] = this.ThinLogs2S.AsArrowArray();
                    arrowArrays[++index] = this.ThinLogs3S.AsArrowArray();
                    arrowArrays[++index] = this.ThinLogs4S.AsArrowArray();
                    arrowArrays[++index] = this.ThinCmh2S.AsArrowArray();
                    arrowArrays[++index] = this.ThinCmh3S.AsArrowArray();
                    arrowArrays[++index] = this.ThinCmh4S.AsArrowArray();
                    arrowArrays[++index] = this.ThinMbfh2S.AsArrowArray();
                    arrowArrays[++index] = this.ThinMbfh3S.AsArrowArray();
                    arrowArrays[++index] = this.ThinMbfh4S.AsArrowArray();
                    arrowArrays[++index] = this.ThinPond2S.AsArrowArray();
                    arrowArrays[++index] = this.ThinPond3S.AsArrowArray();
                    arrowArrays[++index] = this.ThinPond4S.AsArrowArray();
                    arrowArrays[++index] = this.StandingLogs2S.AsArrowArray();
                    arrowArrays[++index] = this.StandingLogs3S.AsArrowArray();
                    arrowArrays[++index] = this.StandingLogs4S.AsArrowArray();
                    arrowArrays[++index] = this.StandingCmh2S.AsArrowArray();
                    arrowArrays[++index] = this.StandingCmh3S.AsArrowArray();
                    arrowArrays[++index] = this.StandingCmh4S.AsArrowArray();
                    arrowArrays[++index] = this.StandingMbfh2S.AsArrowArray();
                    arrowArrays[++index] = this.StandingMbfh3S.AsArrowArray();
                    arrowArrays[++index] = this.StandingMbfh4S.AsArrowArray();
                    arrowArrays[++index] = this.RegenPond2S.AsArrowArray();
                    arrowArrays[++index] = this.RegenPond3S.AsArrowArray();
                    arrowArrays[++index] = this.RegenPond4S.AsArrowArray();
                }

                if (this.ThinFallerPMh.Length > 0) // equipment productivity
                {
                    arrowArrays[++index] = this.ThinFallerPMh.AsArrowArray();
                    arrowArrays[++index] = this.ThinFallerProductivity.AsArrowArray();
                    arrowArrays[++index] = this.ThinFellerBuncherPMh.AsArrowArray();
                    arrowArrays[++index] = this.ThinFellerBuncherProductivity.AsArrowArray();
                    arrowArrays[++index] = this.ThinTrackedHarvesterPMh.AsArrowArray();
                    arrowArrays[++index] = this.ThinTrackedHarvesterProductivity.AsArrowArray();
                    arrowArrays[++index] = this.ThinWheeledHarvesterPMh.AsArrowArray();
                    arrowArrays[++index] = this.ThinWheeledHarvesterProductivity.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawCrewWithFellerBuncherAndGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawUtilizationWithFellerBuncherAndGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawCmhWithFellerBuncherAndGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawPMhWithFellerBuncherAndGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawCrewWithTrackedHarvester.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawUtilizationWithTrackedHarvester.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawCmhWithTrackedHarvester.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawPMhWithTrackedHarvester.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawCrewWithWheeledHarvester.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawUtilizationWithWheeledHarvester.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawCmhWithWheeledHarvester.AsArrowArray();
                    arrowArrays[++index] = this.ThinChainsawPMhWithWheeledHarvester.AsArrowArray();
                    arrowArrays[++index] = this.ThinForwardingMethod.AsArrowArray();
                    arrowArrays[++index] = this.ThinForwarderPMh.AsArrowArray();
                    arrowArrays[++index] = this.ThinForwarderProductivity.AsArrowArray();
                    arrowArrays[++index] = this.ThinForwardedWeight.AsArrowArray();
                    arrowArrays[++index] = this.ThinGrappleSwingYarderPMhPerHectare.AsArrowArray();
                    arrowArrays[++index] = this.ThinGrappleSwingYarderProductivity.AsArrowArray();
                    arrowArrays[++index] = this.ThinGrappleSwingYarderOverweightFirstLogsPerHectare.AsArrowArray();
                    arrowArrays[++index] = this.ThinGrappleYoaderPMhPerHectare.AsArrowArray();
                    arrowArrays[++index] = this.ThinGrappleYoaderProductivity.AsArrowArray();
                    arrowArrays[++index] = this.ThinGrappleYoaderOverweightFirstLogsPerHectare.AsArrowArray();
                    arrowArrays[++index] = this.ThinProcessorPMhWithGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.ThinProcessorProductivityWithGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.ThinProcessorPMhWithGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.ThinProcessorProductivityWithGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.ThinLoadedWeight.AsArrowArray();

                    arrowArrays[++index] = this.RegenFallerPMh.AsArrowArray();
                    arrowArrays[++index] = this.RegenFallerProductivity.AsArrowArray();
                    arrowArrays[++index] = this.RegenFellerBuncherPMh.AsArrowArray();
                    arrowArrays[++index] = this.RegenFellerBuncherProductivity.AsArrowArray();
                    arrowArrays[++index] = this.RegenTrackedHarvesterPMh.AsArrowArray();
                    arrowArrays[++index] = this.RegenTrackedHarvesterProductivity.AsArrowArray();
                    arrowArrays[++index] = this.RegenWheeledHarvesterPMh.AsArrowArray();
                    arrowArrays[++index] = this.RegenWheeledHarvesterProductivity.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawCrewWithFellerBuncherAndGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawCmhWithFellerBuncherAndGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawPMhWithFellerBuncherAndGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawCrewWithFellerBuncherAndGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawUtilizationWithFellerBuncherAndGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawCmhWithFellerBuncherAndGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawPMhWithFellerBuncherAndGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawCrewWithTrackedHarvester.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawUtilizationWithTrackedHarvester.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawCmhWithTrackedHarvester.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawPMhWithTrackedHarvester.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawCrewWithWheeledHarvester.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawUtilizationWithWheeledHarvester.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawCmhWithWheeledHarvester.AsArrowArray();
                    arrowArrays[++index] = this.RegenChainsawPMhWithWheeledHarvester.AsArrowArray();
                    arrowArrays[++index] = this.RegenGrappleSwingYarderPMhPerHectare.AsArrowArray();
                    arrowArrays[++index] = this.RegenGrappleSwingYarderProductivity.AsArrowArray();
                    arrowArrays[++index] = this.RegenGrappleSwingYarderOverweightFirstLogsPerHectare.AsArrowArray();
                    arrowArrays[++index] = this.RegenGrappleYoaderPMhPerHectare.AsArrowArray();
                    arrowArrays[++index] = this.RegenGrappleYoaderProductivity.AsArrowArray();
                    arrowArrays[++index] = this.RegenGrappleYoaderOverweightFirstLogsPerHectare.AsArrowArray();
                    arrowArrays[++index] = this.RegenProcessorPMhWithGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.RegenProcessorProductivityWithGrappleSwingYarder.AsArrowArray();
                    arrowArrays[++index] = this.RegenProcessorPMhWithGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.RegenProcessorProductivityWithGrappleYoader.AsArrowArray();
                    arrowArrays[++index] = this.RegenLoadedWeight.AsArrowArray();
                }

                return arrowArrays;
            }
        }
    }
}
