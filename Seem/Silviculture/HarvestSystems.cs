using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Silviculture
{
    public class HarvestSystems
    {
        public static HarvestSystems Default { get; private set; }

        public float ChainsawBuckConstant { get; set; } // seconds
        public float ChainsawBuckCostPerSMh { get; set; } // US$/SMh
        public float ChainsawBuckLinear { get; set; } // seconds/m³
        public float ChainsawBuckUtilization { get; set; } // fraction
        public float ChainsawBuckQuadratic { get; set; } // seconds/m⁶
        public float ChainsawBuckQuadraticThreshold { get; set; } // m³
        public float ChainsawByOperatorCostPerSMh { get; set; } // US$/SMh
        public float ChainsawByOperatorUtilization { get; set; } // fraction
        public float ChainsawFellAndBuckConstant { get; set; } // seconds
        public float ChainsawFellAndBuckCostPerSMh { get; set; } // US$/SMh
        public float ChainsawFellAndBuckLinear { get; set; } // seconds/m³
        public float ChainsawFellAndBuckUtilization { get; set; } // fraction
        public float ChainsawSlopeLinear { get; set; } // multiplier
        public float ChainsawSlopeThresholdInPercent { get; set; }

        public float CorridorWidth { get; set; } // m

        public float CutToLengthHaulPayloadInKg { get; set; } // US$/SMh
        public float CutToLengthRoundtripHaulSMh { get; set; } // SMh
        public float CutToLengthHaulPerSMh { get; set; } // US$/SMh

        public float FellerBuncherCostPerSMh { get; set; } // US$/SMh
        public float FellerBuncherFellingConstant { get; set; } // seconds
        public float FellerBuncherFellingLinear { get; set; } // seconds/m³
        public float FellerBuncherSlopeLinear { get; set; } // multiplier
        public float FellerBuncherSlopeThresholdInPercent { get; set; }
        public float FellerBuncherUtilization { get; set; } // US$/SMh

        public float ForwarderCostPerSMh { get; set; } // US$/SMh
        public float ForwarderDriveWhileLoadingLogs { get; set; } // driving while loading time coefficient for number of logs in payload
        public float ForwarderEmptyWeight { get; set; } // kg
        public float ForwarderLoadMeanLogVolume { get; set; } // load time coefficient for mean log volume in payload
        public float ForwarderLoadPayload { get; set; } // load time coefficient for payload size
        public float ForwarderMaximumPayloadInKg { get; set; }
        public float ForwarderSpeedInStandLoadedTethered { get; set; } // m/min
        public float ForwarderSpeedInStandLoadedUntethered { get; set; } // m/min
        public float ForwarderSpeedInStandUnloadedTethered { get; set; } // m/min
        public float ForwarderSpeedInStandUnloadedUntethered { get; set; } // m/min
        public float ForwarderSpeedOnRoad { get; set; } // m/min
        public float ForwarderTractiveForce { get; set; } // kN
        public float ForwarderUnloadLinearOneSort { get; set; } // linear unload time coefficient
        public float ForwarderUnloadLinearTwoSorts { get; set; } // linear unload time coefficient
        public float ForwarderUnloadLinearThreeSorts { get; set; } // linear unload time coefficient
        public float ForwarderUnloadMeanLogVolume { get; set; } // unload time coefficient for mean log volume in payload
        public float ForwarderUnloadPayload { get; set; } // unload time coefficient for payload size
        public float ForwarderUtilization { get; set; } // fraction

        public float GrappleYardingConstant { get; set; } // seconds
        public float GrappleYardingLinear { get; set; } // seconds/m of highlead or skyline yarding distance
        public float GrappleSwingYarderCostPerSMh { get; set; } // US$/SMh
        public float GrappleSwingYarderMaxPayload { get; set; } // kg
        public float GrappleSwingYarderMeanPayload { get; set; } // kg
        public float GrappleSwingYarderUtilization { get; set; } // fraction
        public float GrappleYoaderCostPerSMh { get; set; } // US$/SMh
        public float GrappleYoaderMaxPayload { get; set; } // kg
        public float GrappleYoaderMeanPayload { get; set; } // kg
        public float GrappleYoaderUtilization { get; set; } // fraction

        public float LoaderCostPerSMh { get; set; } // US$/SMh
        public float LoaderProductivity { get; set; } // kg/PMh₀
        public float LoaderUtilization { get; set; } // fraction

        public float LongLogHaulPayloadInKg { get; set; } // kg
        public float LongLogHaulPerSMh { get; set; } // US$/SMh
        public float LongLogHaulRoundtripSMh { get; set; } // SMh

        public float ProcessorBuckConstant { get; set; } // seconds
        // TODO: public float ProcessorDiameterLimit { get; set; } // m
        public float ProcessorBuckLinear { get; set; } // seconds/m³
        public float ProcessorBuckQuadratic1 { get; set; } // seconds/m⁶
        public float ProcessorBuckQuadratic2 { get; set; } // seconds/m⁶
        public float ProcessorBuckQuadraticThreshold1 { get; set; } // m³
        public float ProcessorBuckQuadraticThreshold2 { get; set; } // m³
        public float ProcessorCostPerSMh { get; set; } // US$/SMh
        public float ProcessorUtilization { get; set; } // fraction

        public float TrackedHarvesterCostPerSMh { get; set; } // US$/SMh
        public float TrackedHarvesterFellAndBuckConstant { get; set; } // seconds
        public float TrackedHarvesterFellAndBuckDiameterLimit { get; set; } // cm
        public float TrackedHarvesterFellAndBuckLinear { get; set; } // seconds/m³
        public float TrackedHarvesterFellAndBuckQuadratic1 { get; set; } // seconds/m⁶
        public float TrackedHarvesterFellAndBuckQuadratic2 { get; set; } // seconds/m⁶
        public float TrackedHarvesterFellingDiameterLimit { get; set; } // cm
        public float TrackedHarvesterQuadraticThreshold1 { get; set; } // m³
        public float TrackedHarvesterQuadraticThreshold2 { get; set; } // m³
        public float TrackedHarvesterSlopeLinear { get; set; } // multiplier
        public float TrackedHarvesterSlopeThresholdInPercent { get; set; }
        public float TrackedHarvesterUtilization { get; set; } // fraction

        public float WheeledHarvesterCostPerSMh { get; set; } // US$/SMh
        public float WheeledHarvesterFellAndBuckConstant { get; set; } // seconds
        public float WheeledHarvesterFellAndBuckDiameterLimit { get; set; } // cm
        public float WheeledHarvesterFellAndBuckLinear { get; set; } // seconds/m³
        public float WheeledHarvesterFellAndBuckQuadratic { get; set; } // seconds/m⁶
        public float WheeledHarvesterFellingDiameterLimit { get; set; } // cm
        public float WheeledHarvesterQuadraticThreshold { get; set; } // m³
        public float WheeledHarvesterSlopeLinear { get; set; } // multiplier
        public float WheeledHarvesterSlopeThresholdInPercent { get; set; }
        public float WheeledHarvesterUtilization { get; set; } // US$/fraction

        static HarvestSystems()
        {
            HarvestSystems.Default = new();
        }

        // if needed, forwarder weight and engine power can be modeled to check for slope reducing loaded speeds
        public HarvestSystems()
        {
            // chainsaw use cases
            this.ChainsawBuckConstant = 51.0F; // seconds
            this.ChainsawBuckCostPerSMh = 80.0F; // US$/SMh
            this.ChainsawBuckLinear = 54.0F; // seconds/m³
            this.ChainsawBuckUtilization = 0.75F; // fraction
            this.ChainsawBuckQuadratic = 30.0F; // seconds/m⁶
            this.ChainsawBuckQuadraticThreshold = 1.0F; // m³
            this.ChainsawByOperatorCostPerSMh = 3.77F; // US$/SMh
            this.ChainsawByOperatorUtilization = 0.25F; // fraction
            this.ChainsawFellAndBuckConstant = 65.0F; // seconds
            this.ChainsawFellAndBuckCostPerSMh = 149.50F; // US$/SMh
            this.ChainsawFellAndBuckLinear = 105.0F; // seconds/m³
            this.ChainsawFellAndBuckUtilization = 0.5F; // fraction
            this.ChainsawSlopeLinear = 0.0125F; // multiplier
            this.ChainsawSlopeThresholdInPercent = 50.0F;

            this.CorridorWidth = 15.0F; // m

            // mule train
            this.CutToLengthHaulPayloadInKg = 28700.0F; // kg, 7 axle mule train assumed
            this.CutToLengthHaulPerSMh = 95.74F; // US$/SMh
            this.CutToLengthRoundtripHaulSMh = 3.92F; // SMh

            // tracked feller-buncher, either hot saw or directional felling head
            this.FellerBuncherCostPerSMh = 289.50F; // US$/SMh
            this.FellerBuncherFellingConstant = 18.0F; // seconds
            this.FellerBuncherFellingLinear = 4.7F; // seconds/m³
            this.FellerBuncherSlopeLinear = 0.0115F; // multiplier
            this.FellerBuncherSlopeThresholdInPercent = 30.0F;
            this.FellerBuncherUtilization = 0.77F; // fraction

            // forwarder default: Ponsse Elephant King
            this.ForwarderCostPerSMh = 211.50F; // US$/SMh
            this.ForwarderDriveWhileLoadingLogs = 0.7698F; // regression coefficient
            this.ForwarderEmptyWeight = 23700.0F + 1900.0F + 0.5F * 2.0F * 2600.0F; // kg, typical configured weight + Synchrowinch + half weight of tracks
            this.ForwarderLoadMeanLogVolume = 0.5955F; // regression coefficient
            this.ForwarderLoadPayload = 0.9726F; // regression coefficient
            this.ForwarderMaximumPayloadInKg = 20000.0F;
            this.ForwarderSpeedInStandLoadedTethered = 33.0F; // m/min
            this.ForwarderSpeedInStandLoadedUntethered = 45.0F; // m/min
            this.ForwarderSpeedInStandUnloadedTethered = 50.0F; // m/min
            this.ForwarderSpeedInStandUnloadedUntethered = 60.0F; // m/min
            this.ForwarderSpeedOnRoad = 66.0F; // m/min
            this.ForwarderTractiveForce = 200.0F; // kN, derated from Ponsse spec of 240 kN for representative soil conditions
            this.ForwarderUnloadLinearOneSort = 0.4667F; // regression coefficient
            this.ForwarderUnloadLinearTwoSorts = 1.25F * this.ForwarderUnloadLinearOneSort; // regression coefficient
            this.ForwarderUnloadLinearThreeSorts = 1.75F * this.ForwarderUnloadLinearOneSort; // regression coefficient
            this.ForwarderUnloadMeanLogVolume = 0.486F; // regression coefficient
            this.ForwarderUnloadPayload = 0.6240F; // regression coefficient
            this.ForwarderUtilization = 0.79F; // fraction

            // nominal grapple yarders
            this.GrappleYardingConstant = 45.0F; // seconds
            this.GrappleYardingLinear = 0.72F; // seconds/m of yarding distance
            this.GrappleSwingYarderMaxPayload = 4000.0F; // kg
            this.GrappleSwingYarderMeanPayload = 2000.0F; // kg
            this.GrappleSwingYarderCostPerSMh = 367.00F; // US$/SMh
            this.GrappleSwingYarderUtilization = 0.80F;
            this.GrappleYoaderMaxPayload = 2900.0F; // kg
            this.GrappleYoaderMeanPayload = 1550.0F; // kg
            this.GrappleYoaderCostPerSMh = 263.50F; // US$/SMh
            this.GrappleYoaderUtilization = 0.75F;

            // nominal loader at landing
            this.LoaderCostPerSMh = 177.0F; // US$/SMh
            this.LoaderProductivity = 2.0F * 0.99F * 26275.0F; // kg/PMh₀, two six axle long log truckloads per hour
            this.LoaderUtilization = 0.90F;

            // long log truck
            this.LongLogHaulPayloadInKg = 0.99F * 26275.0F; // kg, 6 axle long log truck assumed
            this.LongLogHaulPerSMh = 83.72F; // US$/m³
            this.LongLogHaulRoundtripSMh = 3.58F; // SMh

            // nominal processor at landing
            this.ProcessorBuckConstant = 21.0F; // seconds
            this.ProcessorBuckLinear = 30.0F; // seconds/m³
            this.ProcessorBuckQuadratic1 = 1.5F; // seconds/m⁶
            this.ProcessorBuckQuadratic2 = 4.5F; // seconds/m⁶
            this.ProcessorBuckQuadraticThreshold1 = 2.5F; // m³
            this.ProcessorBuckQuadraticThreshold2 = 6.0F; // m³
            this.ProcessorCostPerSMh = 212.00F; // US$/SMh
            this.ProcessorUtilization = 0.89F;

            // nominal tracked harvester
            this.TrackedHarvesterFellAndBuckConstant = 28.0F; // seconds
            this.TrackedHarvesterFellAndBuckDiameterLimit = 80.0F; // cm, somewhat under felling diameter and feed roller limit of H9 head due to estimated boom lift and swing torque limits
            this.TrackedHarvesterFellAndBuckLinear = 40.0F; // seconds/m³
            this.TrackedHarvesterFellAndBuckQuadratic1 = 3.0F; // seconds/m⁶
            this.TrackedHarvesterFellAndBuckQuadratic2 = 3.0F; // seconds/m⁶
            this.TrackedHarvesterFellingDiameterLimit = 105.0F; // cm, face and back cuts with H9 head
            this.TrackedHarvesterQuadraticThreshold1 = 2.2F; // m³
            this.TrackedHarvesterQuadraticThreshold2 = 6.0F; // m³
            this.TrackedHarvesterSlopeLinear = 0.0115F; // multiplier
            this.TrackedHarvesterSlopeThresholdInPercent = 30.0F;
            this.TrackedHarvesterCostPerSMh = 290.50F; // US$/SMh
            this.TrackedHarvesterUtilization = 0.77F; // fraction

            // eight wheel harvesters
            // Ponsse Scorpion King with H7, Synchrowinch, and tracks
            //this.WheeledHarvesterDiameterLimit = 65.0F; // cm, feed roller limit of H7 head
            //this.WheeledHarvesterFellingConstant = 28.0F; // seconds
            //this.WheeledHarvesterFellingLinear = 43.0F; // seconds/m³
            //this.WheeledHarvesterFellingQuadratic = 8.0F; // seconds/m⁶
            //this.WheeledHarvesterOperatingCost = 301.00F; // US$/PMh₀
            //this.WheeledHarvesterQuadraticThreshold = 1.6F; // m³
            //this.WheeledHarvesterSlopeThresholdInPercent = 45.0F;
            // Ponsse Bear with H8, Synchrowinch, and tracks
            this.WheeledHarvesterCostPerSMh = 239.50F; // US$/SMh
            this.WheeledHarvesterFellAndBuckConstant = 28.0F; // seconds
            this.WheeledHarvesterFellAndBuckDiameterLimit = 70.0F; // cm, felling diameter and feed roller limit of H8 head
            this.WheeledHarvesterFellAndBuckLinear = 43.0F; // seconds/m³
            this.WheeledHarvesterFellAndBuckQuadratic = 6.0F; // seconds/m⁶
            this.WheeledHarvesterQuadraticThreshold = 1.9F; // m³
            this.WheeledHarvesterSlopeLinear = 0.0100F; // multiplier
            this.WheeledHarvesterSlopeThresholdInPercent = 45.0F;
            this.WheeledHarvesterUtilization = 0.77F; // fraction
        }

        private float GetForwardingProductivity(Stand stand, float merchM3PerHa, float logsPerHa, float forwarderMaxMerchM3, int sortsLoaded)
        {
            Debug.Assert((forwarderMaxMerchM3 > 0.0F) && (sortsLoaded > 0) && (sortsLoaded < 4));
            if (merchM3PerHa <= 0.0F)
            {
                Debug.Assert((merchM3PerHa == 0.0F) && (logsPerHa == 0.0F));
                return 0.0F;
            }

            float merchM3perM = merchM3PerHa * this.CorridorWidth / Constant.SquareMetersPerHectare; //  merchantable m³ logs/m of corridor = m³/ha * (m²/m corridor) / m²/ha
            float meanLogMerchM3 = merchM3PerHa / logsPerHa; // merchantable m³/log = m³/ha / logs/ha
            float volumePerCorridor = stand.CorridorLength * merchM3perM; // merchantable m³/corridor
            float turnsPerCorridor = volumePerCorridor / forwarderMaxMerchM3;
            float completeLoadsInCorridor = MathF.Floor(turnsPerCorridor);
            float fractionalLoadsInCorridor = turnsPerCorridor - completeLoadsInCorridor;
            float forwardingDistanceOnRoad = stand.ForwardingDistanceOnRoad + (sortsLoaded - 1) * Constant.HarvestCost.ForwardingDistanceOnRoadPerSortInM;

            float driveEmptyRoad = MathF.Ceiling(turnsPerCorridor) * forwardingDistanceOnRoad / this.ForwarderSpeedOnRoad; // min, driving empty on road
            // nonspatial approximation (level zero): both tethered and untethered distances decrease in turns after the first
            // TODO: assume tethered distance decreases to zero before untethered distance decreases?
            float driveEmptyUntethered = turnsPerCorridor * stand.ForwardingDistanceInStandUntethered / this.ForwarderSpeedInStandUnloadedUntethered; // min
            // tethering time is treated as a delay
            float driveEmptyTethered = turnsPerCorridor * stand.ForwardingDistanceInStandTethered / this.ForwarderSpeedInStandUnloadedTethered; // min
            float loading = completeLoadsInCorridor * MathF.Exp(-1.2460F + this.ForwarderLoadPayload * MathF.Log(forwarderMaxMerchM3) - this.ForwarderLoadMeanLogVolume * MathF.Log(meanLogMerchM3)) +
                            MathF.Exp(-1.2460F + this.ForwarderLoadPayload * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3) - this.ForwarderLoadMeanLogVolume * MathF.Log(meanLogMerchM3)); // min
            float driveWhileLoading = completeLoadsInCorridor * MathF.Exp(-2.5239F + this.ForwarderDriveWhileLoadingLogs * MathF.Log(forwarderMaxMerchM3 / merchM3perM)) +
                                      MathF.Exp(-2.5239F + this.ForwarderDriveWhileLoadingLogs * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3 / merchM3perM)); // min
            float driveLoadedTethered = MathF.Max(turnsPerCorridor - 1.0F, 0.0F) * stand.ForwardingDistanceInStandTethered / this.ForwarderSpeedInStandLoadedTethered; // min
            // untethering time is treated as a delay
            float driveUnloadedTethered = MathF.Max(turnsPerCorridor - 1.0F, 0.0F) * stand.ForwardingDistanceInStandUntethered / this.ForwarderSpeedInStandLoadedUntethered; // min
            float driveLoadedRoad = MathF.Ceiling(turnsPerCorridor) * forwardingDistanceOnRoad / this.ForwarderSpeedOnRoad; // min
            float unloadLinear = sortsLoaded switch
            {
                1 => this.ForwarderUnloadLinearOneSort,
                2 => this.ForwarderUnloadLinearTwoSorts,
                3 => this.ForwarderUnloadLinearThreeSorts,
                _ => throw new ArgumentOutOfRangeException(nameof(sortsLoaded))
            };
            float unloading = unloadLinear * (completeLoadsInCorridor * MathF.Exp(this.ForwarderUnloadPayload * MathF.Log(forwarderMaxMerchM3) - this.ForwarderUnloadMeanLogVolume * MathF.Log(meanLogMerchM3)) +
                                              MathF.Exp(this.ForwarderUnloadPayload * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3) - this.ForwarderUnloadMeanLogVolume * MathF.Log(meanLogMerchM3)));
            float turnTime = driveEmptyRoad + driveEmptyUntethered + driveEmptyTethered + loading + driveWhileLoading + driveLoadedTethered + driveUnloadedTethered + driveLoadedRoad + unloading; // min
            float productivity = Constant.MinutesPerHour * volumePerCorridor / turnTime; // m³/PMh₀
            return productivity;
        }

        public CutToLengthHarvest GetCutToLengthHarvestCost(Stand stand, SortedList<FiaCode, TreeSelection> individualTreeSelectionBySpecies, SortedList<FiaCode, TreeSpeciesMerchantableVolume> harvestVolumeBySpecies, int harvestPeriod)
        {
            CutToLengthHarvest ctlHarvest = new();
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                float diameterToCentimetersMultiplier = 1.0F;
                float hectareExpansionFactorMultiplier = 1.0F;
                if (treesOfSpecies.Units == Units.English)
                {
                    diameterToCentimetersMultiplier = Constant.CentimetersPerInch;
                    hectareExpansionFactorMultiplier = Constant.AcresPerHectare;
                }
                TreeSelection individualTreeSelection = individualTreeSelectionBySpecies[treesOfSpecies.Species];
                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treesOfSpecies.Species];

                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                    if (individualTreeSelection[uncompactedTreeIndex] != harvestPeriod)
                    {
                        // tree was either removed previously or was retained rather than thinned
                        continue;
                    }

                    // Could factor constant and linear terms out of loop (true for all machines, both CTL and long log) but it doesn't
                    // appear important to do so.
                    float dbhInCm = diameterToCentimetersMultiplier * treesOfSpecies.Dbh[compactedTreeIndex];
                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeMerchantableVolumeInM3 = treesOfSpecies.MerchantableCubicVolumePerStem[compactedTreeIndex];
                    Debug.Assert(Single.IsNaN(treeMerchantableVolumeInM3) == false);
                    ctlHarvest.TotalMerchantableCubicVolume += expansionFactorPerHa * treeMerchantableVolumeInM3;

                    // TODO: check tracked harvester

                    // wheeled harvster
                    if (dbhInCm <= this.WheeledHarvesterFellAndBuckDiameterLimit)
                    {
                        // tree felled and bucked by harvester, no chainsaw use
                        float treeHarvesterTime = this.WheeledHarvesterFellAndBuckConstant + this.WheeledHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                        if (treeMerchantableVolumeInM3 > this.WheeledHarvesterQuadraticThreshold)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.WheeledHarvesterQuadraticThreshold;
                            treeHarvesterTime += this.WheeledHarvesterFellAndBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                        }

                        float woodAndRemainingBarkVolumePerStem = treeMerchantableVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFractionAfterHarvester);
                        ctlHarvest.TotalForwardedWeightPeHa += expansionFactorPerHa * woodAndRemainingBarkVolumePerStem * treeSpeciesProperties.StemDensityAfterHarvester;
                        ctlHarvest.WheeledHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }
                    else
                    {
                        // a chainsaw is used on this tree, so it applies towards chainsaw utilization
                        ctlHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;

                        float treeChainsawTime; // seconds
                        float treeHarvesterTime; // seconds
                        if (dbhInCm > this.WheeledHarvesterFellingDiameterLimit)
                        {
                            treeChainsawTime = this.ChainsawFellAndBuckConstant + this.ChainsawFellAndBuckLinear * treeMerchantableVolumeInM3;
                            treeHarvesterTime = 0.0F;
                        }
                        else
                        {
                            treeChainsawTime = this.ChainsawBuckConstant + this.ChainsawBuckLinear * treeMerchantableVolumeInM3;
                            treeHarvesterTime = this.WheeledHarvesterFellAndBuckConstant + this.WheeledHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                        }
                        if (treeMerchantableVolumeInM3 > this.ChainsawBuckQuadraticThreshold)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ChainsawBuckQuadraticThreshold;
                            treeChainsawTime += this.ChainsawBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                        }

                        float woodAndBarkVolumePerStem = treeMerchantableVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFraction); // no bark loss from going through feed rollers
                        ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester += expansionFactorPerHa * treeChainsawTime;
                        ctlHarvest.TotalForwardedWeightPeHa += expansionFactorPerHa * woodAndBarkVolumePerStem * treeSpeciesProperties.StemDensity;
                        ctlHarvest.WheeledHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }
                }
            }

            // for now, assume uniform slope across stand
            if (stand.SlopeInPercent > this.WheeledHarvesterSlopeThresholdInPercent)
            {
                ctlHarvest.WheeledHarvesterPMhPerHa *= 1.0F + this.WheeledHarvesterSlopeLinear * (stand.SlopeInPercent - this.WheeledHarvesterSlopeThresholdInPercent);
            }
            ctlHarvest.WheeledHarvesterPMhPerHa /= Constant.SecondsPerHour;
            ctlHarvest.Productivity.WheeledHarvester = ctlHarvest.TotalMerchantableCubicVolume / ctlHarvest.WheeledHarvesterPMhPerHa;

            float minimumChainsawCostWithWheeledHarvester = 0.0F;
            if (ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester > 0.0F)
            {
                if (stand.SlopeInPercent > this.ChainsawSlopeThresholdInPercent)
                {
                    ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester *= 1.0F + this.ChainsawSlopeLinear * (stand.SlopeInPercent - this.ChainsawSlopeThresholdInPercent);
                }
                ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester /= Constant.SecondsPerHour;

                float chainsawBuckUtilization = this.ChainsawBuckUtilization * MathF.Min(ctlHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                float chainsawBuckCost = this.ChainsawBuckCostPerSMh * ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester / chainsawBuckUtilization;

                float chainsawByOperatorSMh = ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester / this.ChainsawByOperatorUtilization;
                float chainsawByOperatorCost = chainsawByOperatorSMh * (this.FellerBuncherCostPerSMh + this.ChainsawByOperatorCostPerSMh);

                minimumChainsawCostWithWheeledHarvester = MathF.Min(chainsawBuckCost, chainsawByOperatorCost);
                Debug.Assert(ctlHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester > 0.0F);
            }

            float wheeledHarvesterSMhPerHectare = ctlHarvest.WheeledHarvesterPMhPerHa / this.WheeledHarvesterUtilization;
            ctlHarvest.WheeledHarvesterCostPerHa = this.WheeledHarvesterCostPerSMh * wheeledHarvesterSMhPerHectare;

            // find payload available for slope from traction
            float forwarderPayloadInKg = MathF.Max(this.ForwarderMaximumPayloadInKg, this.ForwarderTractiveForce / (0.009807F * MathF.Sin(MathF.Atan(0.01F * stand.SlopeInPercent))) - this.ForwarderEmptyWeight);
            if (forwarderPayloadInKg <= 0.0F)
            {
                throw new NotSupportedException("Stand slope (" + stand.SlopeInPercent + "%) is too steep for forwarding.");
            }

            // TODO: full bark retention on trees bucked by chainsaw (for now it's assumed all trees are bucked by a harvester)
            // TODO: support cross-species loading
            foreach (TreeSpeciesMerchantableVolume harvestVolumeForSpecies in harvestVolumeBySpecies.Values)
            {
                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[harvestVolumeForSpecies.Species];
                float forwarderMaximumMerchantableM3 = forwarderPayloadInKg / (treeSpeciesProperties.StemDensityAfterHarvester * (1.0F + treeSpeciesProperties.BarkFractionAfterHarvester)); // merchantable m³ = kg / kg/m³ * 1 / merchantable m³/m³

                float cubic4Saw = harvestVolumeForSpecies.Cubic4Saw[harvestPeriod];
                float logs4Saw = harvestVolumeForSpecies.Logs4Saw[harvestPeriod];
                float cubic3Saw = harvestVolumeForSpecies.Cubic3Saw[harvestPeriod];
                float logs3Saw = harvestVolumeForSpecies.Logs3Saw[harvestPeriod];
                float cubic2Saw = harvestVolumeForSpecies.Cubic2Saw[harvestPeriod];
                float logs2Saw = harvestVolumeForSpecies.Logs2Saw[harvestPeriod];
                float speciesMerchM3PerHa = cubic2Saw + cubic3Saw + cubic4Saw; // m³/ha
                int sortsPresent = (cubic2Saw > 0.0F ? 1 : 0) + (cubic3Saw > 0.0F ? 1 : 0) + (cubic4Saw > 0.0F ? 1 : 0);

                float productivityAllSortsCombined = this.GetForwardingProductivity(stand, speciesMerchM3PerHa, logs2Saw + logs3Saw + logs4Saw, forwarderMaximumMerchantableM3, sortsPresent);
                ctlHarvest.Productivity.Forwarder = productivityAllSortsCombined;

                float productivity2S = this.GetForwardingProductivity(stand, cubic2Saw, logs2Saw, forwarderMaximumMerchantableM3, 1);
                float productivity3S = this.GetForwardingProductivity(stand, cubic3Saw, logs3Saw, forwarderMaximumMerchantableM3, 1);
                float productivity4S = this.GetForwardingProductivity(stand, cubic4Saw, logs4Saw, forwarderMaximumMerchantableM3, 1);
                float productivityAllSortsSeparate = (cubic2Saw * productivity2S + cubic3Saw * productivity3S + cubic4Saw * productivity4S) / speciesMerchM3PerHa;
                if (productivityAllSortsSeparate > productivityAllSortsCombined)
                {
                    ctlHarvest.Productivity.Forwarder = productivityAllSortsSeparate;
                }

                if ((cubic2Saw > 0.0F) && (cubic4Saw > 0.0F))
                {
                    float productivity2S4S = this.GetForwardingProductivity(stand, cubic2Saw + cubic4Saw, logs2Saw + logs4Saw, forwarderMaximumMerchantableM3, 2);
                    float productivity2S4SCombined = ((cubic2Saw + cubic3Saw) * productivity2S4S + cubic4Saw * productivity4S) / speciesMerchM3PerHa;
                    if (productivity2S4SCombined > productivityAllSortsSeparate)
                    {
                        ctlHarvest.Productivity.Forwarder = ctlHarvest.Productivity.Forwarder;
                    }
                }

                //float forwarderProductivity = pmax((volumePerCorridor2S * forwarderProductivity2S + volumePerCorridor3S * forwarderProductivity3S + volumePerCorridor4S * forwarderProductivity4S) / (volumePerCorridor2S + volumePerCorridor3S + volumePerCorridor4S),
                //                             (volumePerCorridor2S4S * forwarderProductivity2S4S + volumePerCorridor3S * forwarderProductivity3S) / (volumePerCorridor2S4S + volumePerCorridor3S),
                //                             forwarderProductivity2S3S4S),
                //float forwarderLoadingMethod = if_else(forwarderProductivity == forwarderProductivity2S3S4S,
                //                                 "all sorts combined",
                //                                 if_else(forwarderProductivity == forwarderProductivity2S4S,
                //                                         "2S+4S combined, 3S separate",
                //                                         "all sorts separate"));
                float forwarderPMhPerSpecies = speciesMerchM3PerHa / ctlHarvest.Productivity.Forwarder;
                // float forwarderCost = treeVolumeClass / forwarderProductivity * forwarderHourlyCost, # $/tree

                ctlHarvest.ForwarderPMhPerHa += forwarderPMhPerSpecies;
            }
            ctlHarvest.ForwarderCostPerHa += this.ForwarderCostPerSMh * ctlHarvest.ForwarderPMhPerHa / this.ForwarderUtilization;

            float haulRoundtripsPerHectare = ctlHarvest.TotalForwardedWeightPeHa / this.CutToLengthHaulPayloadInKg;
            float haulCostPerHectare = this.CutToLengthRoundtripHaulSMh * haulRoundtripsPerHectare * this.CutToLengthHaulPerSMh;

            ctlHarvest.MinimumCostPerHa = ctlHarvest.WheeledHarvesterCostPerHa + minimumChainsawCostWithWheeledHarvester + ctlHarvest.ForwarderCostPerHa + haulCostPerHectare;
            return ctlHarvest;
        }

        public LongLogHarvest GetLongLogHarvestCosts(Stand stand, ScaledVolume scaledVolume)
        {
            LongLogHarvest longLogHarvest = new();
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                float diameterToCentimetersMultiplier = 1.0F;
                float heightToMetersMultiplier = 1.0F;
                float hectareExpansionFactorMultiplier = 1.0F;
                if (treesOfSpecies.Units == Units.English)
                {
                    diameterToCentimetersMultiplier = Constant.CentimetersPerInch;
                    heightToMetersMultiplier = Constant.MetersPerFoot;
                    hectareExpansionFactorMultiplier = Constant.AcresPerHectare;
                }

                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treesOfSpecies.Species];
                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeMerchantableVolumeInM3 = treesOfSpecies.MerchantableCubicVolumePerStem[compactedTreeIndex];
                    Debug.Assert(Single.IsNaN(treeMerchantableVolumeInM3) == false);
                    longLogHarvest.TotalMerchantableCubicVolumePerHa += expansionFactorPerHa * treeMerchantableVolumeInM3;

                    // feller-buncher with hot saw or directional felling head
                    // For now, assume directional felling head where needed and no diameter limit to directional felling.
                    float treeFellerBuncherTime = this.FellerBuncherFellingConstant + this.FellerBuncherFellingLinear * treeMerchantableVolumeInM3;
                    longLogHarvest.FellerBuncherPMhPerHa += expansionFactorPerHa * treeFellerBuncherTime;

                    // yarded weight
                    // Since no yarder productivity study appears to consider bark loss it's generally unclear where in the turn reported
                    // yarding weights occur. In lieu of more specific data, yarding weight limits are checked when half the yarding bark
                    // loss has occurred and this same weight is used in finding the total yarded weight for calculating the total number
                    // of yarding turns.
                    // TODO: include weight of crown not broken off during felling (and swinging)
                    float woodAndRemainingBarkVolumePerStemAfterFelling = treeMerchantableVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFractionAtMidspanWithoutHarvester);
                    float yardedWeightPerStemWithFellerBuncher = woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
                    longLogHarvest.TotalYardedWeightPerHaWithFellerBuncher += expansionFactorPerHa * yardedWeightPerStemWithFellerBuncher; // before bark loss since 

                    // chainsaw bucking of trees felled with a feller buncher but too heavy to be yarded as whole stems
                    // For now,
                    //   1) Assume the feller-buncher has a directional felling head if trees are too large for a hot saw and that
                    //      no trees are too large for a directional felling head.
                    //   2) Neglect effects of short log production when trees are large enough full size logs still exceed the
                    //      yoader weight limit.
                    //   3) The tree's first log is calculated from a Smalian estimate diameter outside bark. A more complete correct
                    //      would look up the tree's first log's merchantable volume from the volume table and adjust for the bark
                    //      fraction.
                    float dbhInCm = diameterToCentimetersMultiplier * treesOfSpecies.Dbh[compactedTreeIndex];
                    float heightInM = heightToMetersMultiplier * treesOfSpecies.Height[compactedTreeIndex];
                    float conicalTaper = dbhInCm / (heightInM - Constant.DbhHeightInM); // cm/m
                    float firstLogTopDiameterOutsideBarkInCm = dbhInCm - conicalTaper * (Constant.Bucking.DefaultLongLogLengthInM - Constant.DbhHeightInM + Constant.Bucking.DefaultStumpHeightInM);
                    float firstLogBottomDiameterOutsideBarkInCm = dbhInCm + conicalTaper * (Constant.DbhHeightInM - Constant.Bucking.DefaultStumpHeightInM);
                    float approximateFirstLogWeightWithMidCorridorBarkLoss = 0.25F * MathF.PI * 0.0001F * 0.5F * (firstLogTopDiameterOutsideBarkInCm * firstLogTopDiameterOutsideBarkInCm + firstLogBottomDiameterOutsideBarkInCm * firstLogBottomDiameterOutsideBarkInCm) * scaledVolume.PreferredLogLengthInMeters * treeSpeciesProperties.StemDensity;

                    bool treeBuckedManuallyWithGrappleSwingYarder = false;
                    if (yardedWeightPerStemWithFellerBuncher > this.GrappleSwingYarderMaxPayload)
                    {
                        float treeChainsawBuckTime = this.ChainsawBuckConstant + this.ChainsawBuckLinear * treeMerchantableVolumeInM3;
                        if (treeMerchantableVolumeInM3 > this.ChainsawBuckQuadraticThreshold)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ChainsawBuckQuadraticThreshold;
                            treeChainsawBuckTime += this.ChainsawBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder += expansionFactorPerHa * treeChainsawBuckTime;
                        treeBuckedManuallyWithGrappleSwingYarder = true;

                        longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;
                        if (approximateFirstLogWeightWithMidCorridorBarkLoss > this.GrappleSwingYarderMaxPayload)
                        {
                            // for now, logs produced by feller-buncher plus chainsaw aren't distinguished from their somewhat lighter
                            // equivalent with less bark after being bucked by a harvester
                            longLogHarvest.GrappleSwingYarderOverweightFirstLogsPerHa += expansionFactorPerHa;
                        }
                    }

                    bool treeBuckedManuallyWithGrappleYoader = false;
                    if (yardedWeightPerStemWithFellerBuncher > this.GrappleYoaderMaxPayload)
                    {
                        float treeChainsawBuckTime = this.ChainsawBuckConstant + this.ChainsawBuckLinear * treeMerchantableVolumeInM3;
                        if (treeMerchantableVolumeInM3 > this.ChainsawBuckQuadraticThreshold)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ChainsawBuckQuadraticThreshold;
                            treeChainsawBuckTime += this.ChainsawBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader += expansionFactorPerHa * treeChainsawBuckTime;
                        treeBuckedManuallyWithGrappleYoader = true;

                        longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;
                        if (approximateFirstLogWeightWithMidCorridorBarkLoss > this.GrappleYoaderMaxPayload)
                        {
                            // for now, logs produced by feller-buncher plus chainsaw aren't distinguished from their somewhat lighter
                            // equivalent with less bark after being bucked by a harvester
                            longLogHarvest.GrappleYoaderOverweightFirstLogsPerHa += expansionFactorPerHa;
                        }
                    }

                    // tracked harvester
                    float woodAndRemainingBarkVolumePerStemWithHarvester = treeMerchantableVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFractionAtMidspanAfterHarvester);
                    if (dbhInCm <= this.TrackedHarvesterFellAndBuckDiameterLimit)
                    {
                        // tree felled and bucked by harvester, no chainsaw use
                        float treeHarvesterTime = this.TrackedHarvesterFellAndBuckConstant + this.TrackedHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                        if (treeMerchantableVolumeInM3 > this.TrackedHarvesterQuadraticThreshold1)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.TrackedHarvesterQuadraticThreshold1;
                            treeHarvesterTime += this.TrackedHarvesterFellAndBuckQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        if (treeMerchantableVolumeInM3 > this.TrackedHarvesterQuadraticThreshold2)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.TrackedHarvesterQuadraticThreshold2;
                            treeHarvesterTime += this.TrackedHarvesterFellAndBuckQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        longLogHarvest.TotalYardedWeightPerHaWithTrackedHarvester += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemWithHarvester * treeSpeciesProperties.StemDensityAtMidspanAfterHarvester;
                        longLogHarvest.TrackedHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }
                    else
                    {
                        // a chainsaw is used on this tree, so it applies towards chainsaw utilization
                        longLogHarvest.ChainsawBasalAreaPerHaWithTrackedHarvester += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;

                        float treeChainsawTime; // seconds
                        float treeHarvesterTime; // seconds
                        if (dbhInCm > this.TrackedHarvesterFellingDiameterLimit)
                        {
                            // tree felled by chainsaw, no cutting by harvester in this case
                            treeChainsawTime = this.ChainsawFellAndBuckConstant + this.ChainsawFellAndBuckLinear * treeMerchantableVolumeInM3;
                            treeHarvesterTime = 0.0F;
                        }
                        else
                        {
                            // tree felled by harvester, bucked by chainsaw
                            treeChainsawTime = this.ChainsawBuckConstant + this.ChainsawBuckLinear * treeMerchantableVolumeInM3;
                            treeHarvesterTime = this.TrackedHarvesterFellAndBuckConstant + this.TrackedHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                        }
                        if (treeMerchantableVolumeInM3 > this.ChainsawBuckQuadraticThreshold)
                        {
                            // tree is always bucked by chainsaw, so quadratic component always applies above threshold
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ChainsawBuckQuadraticThreshold;
                            treeChainsawTime += this.ChainsawBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                        }

                        // tree is not bucked by harvester, so bark loss is only from yarding
                        longLogHarvest.ChainsawPMhPerHaTrackedHarvester += expansionFactorPerHa * treeChainsawTime;
                        longLogHarvest.TotalYardedWeightPerHaWithTrackedHarvester += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
                        longLogHarvest.TrackedHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }

                    // wheeled harvester
                    if (dbhInCm <= this.WheeledHarvesterFellAndBuckDiameterLimit)
                    {
                        // tree felled and bucked by harvester, no chainsaw use
                        float treeHarvesterTime = this.WheeledHarvesterFellAndBuckConstant + this.WheeledHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                        if (treeMerchantableVolumeInM3 > this.WheeledHarvesterQuadraticThreshold)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.WheeledHarvesterQuadraticThreshold;
                            treeHarvesterTime += this.WheeledHarvesterFellAndBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        longLogHarvest.TotalYardedWeightPerHaWithWheeledHarvester += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemWithHarvester * treeSpeciesProperties.StemDensityAtMidspanAfterHarvester;
                        longLogHarvest.WheeledHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }
                    else
                    {
                        // a chainsaw is used on this tree, so it applies towards chainsaw utilization
                        longLogHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;

                        float treeChainsawTime; // seconds
                        float treeHarvesterTime; // seconds
                        if (dbhInCm > this.WheeledHarvesterFellingDiameterLimit)
                        {
                            treeChainsawTime = this.ChainsawFellAndBuckConstant + this.ChainsawFellAndBuckLinear * treeMerchantableVolumeInM3;
                            treeHarvesterTime = 0.0F;
                        }
                        else
                        {
                            treeChainsawTime = this.ChainsawBuckConstant + this.ChainsawBuckLinear * treeMerchantableVolumeInM3;
                            treeHarvesterTime = this.WheeledHarvesterFellAndBuckConstant + this.WheeledHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                        }
                        if (treeMerchantableVolumeInM3 > this.ChainsawBuckQuadraticThreshold)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ChainsawBuckQuadraticThreshold;
                            treeChainsawTime += this.ChainsawBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                        }

                        // tree is not bucked by harvester, so bark loss is only from yarding
                        longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester += expansionFactorPerHa * treeChainsawTime;
                        longLogHarvest.TotalYardedWeightPerHaWithWheeledHarvester += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
                        longLogHarvest.WheeledHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }

                    // processor
                    // Include constant and linear time components regardless of whether processor needs to buck the tree as, if the tree
                    // was bucked by chainsaw before yarding, the processor still needs to move the logs from the yarder to the loader.
                    float treeProcessingTimeWithGrappleSwingYarder = this.ProcessorBuckConstant + this.ProcessorBuckLinear * treeMerchantableVolumeInM3;
                    if (treeBuckedManuallyWithGrappleSwingYarder == false)
                    {
                        if (treeMerchantableVolumeInM3 > this.ProcessorBuckQuadraticThreshold1)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ProcessorBuckQuadraticThreshold1;
                            treeProcessingTimeWithGrappleSwingYarder += this.ProcessorBuckQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        if (treeMerchantableVolumeInM3 > this.ProcessorBuckQuadraticThreshold2)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ProcessorBuckQuadraticThreshold2;
                            treeProcessingTimeWithGrappleSwingYarder += this.ProcessorBuckQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                    }
                    longLogHarvest.ProcessorPMhPerHaWithGrappleSwingYarder += expansionFactorPerHa * treeProcessingTimeWithGrappleSwingYarder;

                    float treeProcessingTimeWithGrappleYoader = this.ProcessorBuckConstant + this.ProcessorBuckLinear * treeMerchantableVolumeInM3;
                    if (treeBuckedManuallyWithGrappleYoader == false)
                    {
                        if (treeMerchantableVolumeInM3 > this.ProcessorBuckQuadraticThreshold1)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ProcessorBuckQuadraticThreshold1;
                            treeProcessingTimeWithGrappleYoader += this.ProcessorBuckQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        if (treeMerchantableVolumeInM3 > this.ProcessorBuckQuadraticThreshold2)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ProcessorBuckQuadraticThreshold2;
                            treeProcessingTimeWithGrappleYoader += this.ProcessorBuckQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                    }
                    longLogHarvest.ProcessorPMhPerHaWithGrappleYoader += expansionFactorPerHa * treeProcessingTimeWithGrappleYoader;

                    // loader productivity is specified so no loader calculations

                    // haul hweight
                    float woodAndRemainingBarkVolumePerStemAfterYardingAndProcessing = treeMerchantableVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFractionAfterYardingAndProcessing);
                    longLogHarvest.TotalLoadedWeightPerHa += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterYardingAndProcessing * treeSpeciesProperties.StemDensityAfterYardingAndProcessing;
                }
            }

            // feller-buncher + chainsaw + yarder-processor-loader system costs
            float meanGrappleYardingTurnTime = this.GrappleYardingConstant + this.GrappleYardingLinear * 0.5F * stand.CorridorLength; // parallel yarding
            float loaderSMhPerHectare = longLogHarvest.TotalLoadedWeightPerHa / (this.LoaderUtilization * this.LoaderProductivity);
            // TODO: full corridor and processing bark loss
            {
                if (stand.SlopeInPercent > this.FellerBuncherSlopeThresholdInPercent)
                {
                    longLogHarvest.FellerBuncherPMhPerHa *= 1.0F + this.FellerBuncherSlopeLinear * (stand.SlopeInPercent - this.FellerBuncherSlopeThresholdInPercent);
                }
                longLogHarvest.FellerBuncherPMhPerHa /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.FellerBuncher = longLogHarvest.TotalMerchantableCubicVolumePerHa / longLogHarvest.FellerBuncherPMhPerHa;
                float fellerBuncherCostPerHectare = this.FellerBuncherCostPerSMh * longLogHarvest.FellerBuncherPMhPerHa / this.FellerBuncherUtilization;

                float minimumChainsawCostWithFellerBuncherAndGrappleSwingYarder = 0.0F;
                if (longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder > 0.0F)
                {
                    if (stand.SlopeInPercent > this.ChainsawSlopeThresholdInPercent)
                    {
                        longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder *= 1.0F + this.ChainsawSlopeLinear * (stand.SlopeInPercent - this.ChainsawSlopeThresholdInPercent);
                    }
                    longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder /= Constant.SecondsPerHour;

                    float chainsawBuckUtilization = this.ChainsawBuckUtilization * MathF.Min(longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                    float chainsawBuckCost = this.ChainsawBuckCostPerSMh * longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder / chainsawBuckUtilization;

                    float chainsawByOperatorSMh = longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder / this.ChainsawByOperatorUtilization;
                    float chainsawByOperatorCost = chainsawByOperatorSMh * (this.FellerBuncherCostPerSMh + this.ChainsawByOperatorCostPerSMh);
                    
                    minimumChainsawCostWithFellerBuncherAndGrappleSwingYarder = MathF.Min(chainsawBuckCost, chainsawByOperatorCost);
                    Debug.Assert(longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder > 0.0F);
                }

                float grappleSwingYarderTurnsPerHectare = longLogHarvest.TotalYardedWeightPerHaWithFellerBuncher / this.GrappleSwingYarderMeanPayload;
                float grappleSwingYarderPMhPerHectare = grappleSwingYarderTurnsPerHectare * meanGrappleYardingTurnTime / Constant.SecondsPerHour;
                longLogHarvest.Productivity.GrappleSwingYarder = longLogHarvest.TotalMerchantableCubicVolumePerHa / grappleSwingYarderPMhPerHectare;
                float grappleSwingYarderSMhPerHectare = grappleSwingYarderPMhPerHectare / this.GrappleSwingYarderUtilization;

                longLogHarvest.ProcessorPMhPerHaWithGrappleSwingYarder /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.ProcessorWithGrappleSwingYarder = longLogHarvest.TotalMerchantableCubicVolumePerHa / longLogHarvest.ProcessorPMhPerHaWithGrappleSwingYarder;
                float processorSMhPerHectareWithGrappleSwingYarder = longLogHarvest.ProcessorPMhPerHaWithGrappleSwingYarder / this.ProcessorUtilization;

                // TODO: chainsaw productivity
                // loader productivity is specified so doesn't need to be output

                float haulRoundtripsPerHectare = longLogHarvest.TotalLoadedWeightPerHa / this.LongLogHaulPayloadInKg;
                float haulCostPerHectare = this.LongLogHaulRoundtripSMh * haulRoundtripsPerHectare * this.LongLogHaulPerSMh;

                float limitingSMhWithFellerBuncherAndGrappleSwingYarder = MathE.Max(grappleSwingYarderSMhPerHectare, processorSMhPerHectareWithGrappleSwingYarder, loaderSMhPerHectare);
                float grappleSwingYarderCostPerHectareWithFellerBuncher = this.GrappleSwingYarderCostPerSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
                float processorCostPerHectareWithGrappleSwingYarder = this.ProcessorCostPerSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
                float loaderCostPerHectareWithFellerBuncherAndGrappleSwingYarder = this.LoaderCostPerSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
                longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa = fellerBuncherCostPerHectare + minimumChainsawCostWithFellerBuncherAndGrappleSwingYarder + 
                    grappleSwingYarderCostPerHectareWithFellerBuncher + processorCostPerHectareWithGrappleSwingYarder + loaderCostPerHectareWithFellerBuncherAndGrappleSwingYarder + 
                    haulCostPerHectare;

                // grapple yoader
                float minimumChainsawCostWithFellerBuncherAndGrappleYoader = 0.0F;
                if (longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader > 0.0F)
                {
                    if (stand.SlopeInPercent > this.ChainsawSlopeThresholdInPercent)
                    {
                        longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader *= 1.0F + this.ChainsawSlopeLinear * (stand.SlopeInPercent - this.ChainsawSlopeThresholdInPercent);
                    }
                    longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader /= Constant.SecondsPerHour;

                    float chainsawBuckUtilization = this.ChainsawBuckUtilization * MathF.Min(longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                    float chainsawBuckSMh = longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader / chainsawBuckUtilization;
                    float chainsawBuckCost = this.ChainsawBuckCostPerSMh * chainsawBuckSMh;

                    float chainsawByOperatorSMh = longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader / this.ChainsawByOperatorUtilization;
                    float chainsawByOperatorCost = chainsawByOperatorSMh * (this.FellerBuncherCostPerSMh + this.ChainsawByOperatorCostPerSMh);
                    
                    minimumChainsawCostWithFellerBuncherAndGrappleYoader = MathF.Min(chainsawBuckCost, chainsawByOperatorCost);
                    Debug.Assert(longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader > 0.0F);
                }

                float grappleYoaderTurnsPerHectare = longLogHarvest.TotalYardedWeightPerHaWithFellerBuncher / this.GrappleYoaderMeanPayload;
                float grappleYoaderPMhPerHectare = grappleYoaderTurnsPerHectare * meanGrappleYardingTurnTime / Constant.SecondsPerHour;
                longLogHarvest.Productivity.GrappleYoader = longLogHarvest.TotalMerchantableCubicVolumePerHa / grappleYoaderPMhPerHectare;
                float grappleYoaderSMhPerHectare = grappleYoaderPMhPerHectare / this.GrappleYoaderUtilization;

                float processorPMhPerHectareWithGrappleYoader = longLogHarvest.ProcessorPMhPerHaWithGrappleYoader / Constant.SecondsPerHour;
                longLogHarvest.Productivity.ProcessorWithGrappleYoader = longLogHarvest.TotalMerchantableCubicVolumePerHa / processorPMhPerHectareWithGrappleYoader;
                float processorSMhPerHectareWithGrappleYoader = processorPMhPerHectareWithGrappleYoader / this.ProcessorUtilization;

                float limitingSMhWithFellerBuncherAndGrappleYoader = MathE.Max(grappleYoaderSMhPerHectare, processorSMhPerHectareWithGrappleYoader, loaderSMhPerHectare);
                float grappleYoaderCostPerHectareWithFellerBuncher = this.GrappleYoaderCostPerSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
                float processorCostPerHectareWithGrappleYoader = this.ProcessorCostPerSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
                float loaderCostPerHectareWithFellerBuncherAndGrappleYoader = this.LoaderCostPerSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
                longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa = fellerBuncherCostPerHectare + minimumChainsawCostWithFellerBuncherAndGrappleYoader + 
                    grappleYoaderCostPerHectareWithFellerBuncher + processorCostPerHectareWithGrappleYoader + loaderCostPerHectareWithFellerBuncherAndGrappleYoader +
                    haulCostPerHectare;

                Debug.Assert((limitingSMhWithFellerBuncherAndGrappleSwingYarder > 0.0F) && (Single.IsNaN(longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa) == false) && (longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa > 0.0F) && (longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa < 50000.0F) &&
                             (limitingSMhWithFellerBuncherAndGrappleYoader > 0.0F) && (Single.IsNaN(longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa) == false) && (longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa > 0.0F) && (longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa < 50000.0F));

                // coupled utilization not currently calculated but may be of debugging interest
                //float swingYarderCoupledUtilization = grappleSwingYarderPMhPerHectare / limitingSMh;
                //float yoaderCoupledUtilization = grappleYoaderPMhPerHectare / limitingSMh;
                //float processorCoupledUtilization = processorPMhPerHectareWithGrappleSwingYarder / limitingSMh;
                //float processorCoupledUtilization = processorPMhPerHectareWithGrappleYoader / limitingSMh;
                //float loaderCoupledUtilization = loaderPMhPerHectare / limitingSMh;
            }

            // harvester + chainsaw + yarder-processor-loader system costs
            {
                // tracked harvester
                // For now, assume uniform slope across stand.
                if (stand.SlopeInPercent > this.TrackedHarvesterSlopeThresholdInPercent)
                {
                    float harvesterSlopeMultiplier = 1.0F + this.TrackedHarvesterSlopeLinear * (stand.SlopeInPercent - this.TrackedHarvesterSlopeThresholdInPercent);
                    longLogHarvest.TrackedHarvesterPMhPerHa *= harvesterSlopeMultiplier;
                }
                longLogHarvest.TrackedHarvesterPMhPerHa /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.TrackedHarvester = longLogHarvest.TotalMerchantableCubicVolumePerHa / longLogHarvest.TrackedHarvesterPMhPerHa;

                float trackedHarvesterCostPerHectare = this.TrackedHarvesterCostPerSMh * longLogHarvest.TrackedHarvesterPMhPerHa / this.TrackedHarvesterUtilization;

                float minimumChainsawCostWithTrackedHarvester = 0.0F;
                if (longLogHarvest.ChainsawPMhPerHaTrackedHarvester > 0.0F)
                {
                    if (stand.SlopeInPercent > this.ChainsawSlopeThresholdInPercent)
                    {
                        longLogHarvest.ChainsawPMhPerHaTrackedHarvester *= 1.0F + this.ChainsawSlopeLinear * (stand.SlopeInPercent - this.ChainsawSlopeThresholdInPercent);
                    }
                    float chainsawFellAndBuckPMhPerHectare = longLogHarvest.ChainsawPMhPerHaTrackedHarvester / Constant.SecondsPerHour;
                    float chainsawFellAndBuckUtilization = this.ChainsawFellAndBuckUtilization * MathF.Min(longLogHarvest.ChainsawBasalAreaPerHaWithTrackedHarvester / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                    float chainsawFellAndBuckCost = this.ChainsawFellAndBuckCostPerSMh * chainsawFellAndBuckPMhPerHectare / chainsawFellAndBuckUtilization;
                    float chainsawByOperatorSMh = chainsawFellAndBuckPMhPerHectare / this.ChainsawByOperatorUtilization; // SMh
                    float chainsawByOperatorCost = (this.TrackedHarvesterCostPerSMh + this.ChainsawByOperatorCostPerSMh) * chainsawByOperatorSMh;
                    minimumChainsawCostWithTrackedHarvester = MathF.Min(chainsawFellAndBuckCost, chainsawByOperatorCost);
                    Debug.Assert(longLogHarvest.ChainsawBasalAreaPerHaWithTrackedHarvester > 0.0F);
                }

                float grappleSwingYarderTurnsPerHectare = longLogHarvest.TotalYardedWeightPerHaWithFellerBuncher / this.GrappleSwingYarderMeanPayload;
                float grappleSwingYarderSMhPerHectare = grappleSwingYarderTurnsPerHectare * meanGrappleYardingTurnTime / (Constant.SecondsPerHour * this.GrappleSwingYarderUtilization);

                float haulRoundtripsPerHectareWithTrackedHarvester = longLogHarvest.TotalYardedWeightPerHaWithTrackedHarvester / this.LongLogHaulPayloadInKg;
                float haulCostPerHectareWithTrackedHarvester = this.LongLogHaulRoundtripSMh * haulRoundtripsPerHectareWithTrackedHarvester * this.LongLogHaulPerSMh;

                float limitingSMhWithTrackedHarvesterAndGrappleSwingYarder = MathF.Max(grappleSwingYarderSMhPerHectare, loaderSMhPerHectare);
                float swingYarderCostPerHectareWithTrackedHarvester = this.GrappleSwingYarderCostPerSMh * limitingSMhWithTrackedHarvesterAndGrappleSwingYarder;
                float loaderCostPerHectareWithTrackedHarvesterAndGrappleSwingYarder = this.LoaderCostPerSMh * limitingSMhWithTrackedHarvesterAndGrappleSwingYarder;
                longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa = trackedHarvesterCostPerHectare + minimumChainsawCostWithTrackedHarvester + 
                    swingYarderCostPerHectareWithTrackedHarvester + loaderCostPerHectareWithTrackedHarvesterAndGrappleSwingYarder +
                    haulCostPerHectareWithTrackedHarvester;

                float grappleYoaderTurnsPerHectare = longLogHarvest.TotalYardedWeightPerHaWithFellerBuncher / this.GrappleYoaderMeanPayload;
                float grappleYoaderSMhPerHectare = grappleYoaderTurnsPerHectare * meanGrappleYardingTurnTime / (Constant.SecondsPerHour * this.GrappleYoaderUtilization);

                float limitingSMhWithTrackedHarvesterAndGrappleYoader = MathF.Max(grappleYoaderSMhPerHectare, loaderSMhPerHectare);
                float grappleYoaderCostPerHectareWithTrackedHarvester = this.GrappleYoaderCostPerSMh * limitingSMhWithTrackedHarvesterAndGrappleYoader;
                float loaderCostPerHectareWithTrackedHarvesterAndGrappleYoader = this.LoaderCostPerSMh * limitingSMhWithTrackedHarvesterAndGrappleYoader;
                longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa = trackedHarvesterCostPerHectare + minimumChainsawCostWithTrackedHarvester + 
                    grappleYoaderCostPerHectareWithTrackedHarvester + loaderCostPerHectareWithTrackedHarvesterAndGrappleYoader +
                    haulCostPerHectareWithTrackedHarvester;

                // wheeled harvester
                if (stand.SlopeInPercent > this.WheeledHarvesterSlopeThresholdInPercent)
                {
                    longLogHarvest.WheeledHarvesterPMhPerHa *= 1.0F + this.WheeledHarvesterSlopeLinear * (stand.SlopeInPercent - this.WheeledHarvesterSlopeThresholdInPercent);
                }
                longLogHarvest.WheeledHarvesterPMhPerHa /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.WheeledHarvester = longLogHarvest.TotalMerchantableCubicVolumePerHa / longLogHarvest.WheeledHarvesterPMhPerHa;

                float wheeledHarvesterSMh = longLogHarvest.WheeledHarvesterPMhPerHa / this.WheeledHarvesterUtilization;
                float wheeledHarvesterCostPerHectare = this.WheeledHarvesterCostPerSMh * wheeledHarvesterSMh;

                float minimumChainsawCostWithWheeledHarvester = 0.0F;
                if (longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester > 0.0F)
                {
                    if (stand.SlopeInPercent > this.ChainsawSlopeThresholdInPercent)
                    {
                        longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester *= 1.0F + this.ChainsawSlopeLinear * (stand.SlopeInPercent - this.ChainsawSlopeThresholdInPercent);
                    }
                    float chainsawFellAndBuckPMhPerHectare = longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester / Constant.SecondsPerHour;
                    float chainsawFellAndBuckUtilization = this.ChainsawFellAndBuckUtilization * MathF.Min(longLogHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                    float chainsawFellAndBuckCost = this.ChainsawFellAndBuckCostPerSMh * chainsawFellAndBuckPMhPerHectare / chainsawFellAndBuckUtilization;
                    float chainsawByOperatorSMh = longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester / (Constant.SecondsPerHour * this.ChainsawByOperatorUtilization); // SMh
                    float chainsawByOperatorCost = chainsawByOperatorSMh * (this.WheeledHarvesterCostPerSMh + this.ChainsawByOperatorCostPerSMh);
                    minimumChainsawCostWithWheeledHarvester = MathF.Min(chainsawFellAndBuckCost, chainsawByOperatorCost);
                    Debug.Assert(longLogHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester > 0.0F);
                }

                float haulRoundtripsPerHectareWithWheeledHarvester = longLogHarvest.TotalYardedWeightPerHaWithWheeledHarvester / this.LongLogHaulPayloadInKg;
                float haulCostPerHectareWithWheeledHarvester = this.LongLogHaulRoundtripSMh * haulRoundtripsPerHectareWithWheeledHarvester * this.LongLogHaulPerSMh;

                float limitingSMhWithWheeledHarvesterAndGrappleSwingYarder = MathF.Max(grappleSwingYarderSMhPerHectare, loaderSMhPerHectare);
                float swingYarderCostPerHectareWithWheeledHarvester = this.GrappleSwingYarderCostPerSMh * limitingSMhWithWheeledHarvesterAndGrappleSwingYarder;
                float loaderCostPerHectareWithWheeledHarvesterAndGrappleSwingYarder = this.LoaderCostPerSMh * limitingSMhWithWheeledHarvesterAndGrappleSwingYarder;
                longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa = wheeledHarvesterCostPerHectare + minimumChainsawCostWithWheeledHarvester + 
                    swingYarderCostPerHectareWithWheeledHarvester + loaderCostPerHectareWithWheeledHarvesterAndGrappleSwingYarder +
                    haulCostPerHectareWithWheeledHarvester;

                float limitingSMhWithWheeledHarvesterAndGrappleYoader = MathF.Max(grappleYoaderSMhPerHectare, loaderSMhPerHectare);
                float grappleYoaderCostPerHectareWithWheeledHarvester = this.GrappleYoaderCostPerSMh * limitingSMhWithWheeledHarvesterAndGrappleYoader;
                float loaderCostPerHectareWithWheeledHarvesterAndGrappleYoader = this.LoaderCostPerSMh * limitingSMhWithWheeledHarvesterAndGrappleYoader;
                longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa = wheeledHarvesterCostPerHectare + minimumChainsawCostWithWheeledHarvester + 
                    grappleYoaderCostPerHectareWithWheeledHarvester + loaderCostPerHectareWithWheeledHarvesterAndGrappleYoader +
                    haulCostPerHectareWithWheeledHarvester;

                Debug.Assert((limitingSMhWithTrackedHarvesterAndGrappleSwingYarder > 0.0F) && (Single.IsNaN(longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa) == false) && (longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa > 0.0F) && (longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa < 50000.0F) &&
                             (limitingSMhWithTrackedHarvesterAndGrappleYoader > 0.0F) && (Single.IsNaN(longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa) == false) && (longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa > 0.0F) && (longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa < 50000.0F) &&
                             (limitingSMhWithWheeledHarvesterAndGrappleSwingYarder > 0.0F) && (Single.IsNaN(longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa) == false) && (longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa > 0.0F) && (longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa < 50000.0F) &&
                             (limitingSMhWithWheeledHarvesterAndGrappleYoader > 0.0F) && (Single.IsNaN(longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa) == false) && (longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa > 0.0F) && (longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa < 50000.0F));
            }

            longLogHarvest.MinimumCostPerHa = MathE.Min(longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa, longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa,
                                                        longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa, longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa,
                                                        longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa, longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa);
            return longLogHarvest;
        }

        public void VerifyPropertyValues()
        {
            if ((this.ChainsawBuckConstant < 0.0F) || (this.ChainsawBuckConstant > 90.0F))
            {
                throw new NotSupportedException("Intercept of chainsaw bucking time is not in the range [0.0, 90.0] seconds.");
            }
            if ((this.ChainsawBuckCostPerSMh < 0.0F) || (this.ChainsawBuckCostPerSMh > 250.0F))
            {
                throw new NotSupportedException("Cost of chainsaw bucking time is not in the range US$ [0.0, 250.0]/PMh.");
            }
            if ((this.ChainsawBuckLinear < 0.0F) || (this.ChainsawBuckLinear > 150.0F))
            {
                throw new NotSupportedException("Linear coefficient of chainsaw bucking time is not in the range [0.0, 150.0] seconds/m³.");
            }
            if ((this.ChainsawBuckUtilization <= 0.0F) || (this.ChainsawBuckUtilization > 1.0F))
            {
                throw new NotSupportedException("Utilization of chainsw bucker is not in the range (0.0, 1.0].");
            }
            if ((this.ChainsawBuckQuadratic < 0.0F) || (this.ChainsawBuckQuadratic > 90.0F))
            {
                throw new NotSupportedException("Quadratic coefficient of chainsaw bucking time is not in the range [0.0, 25.0] seconds/m³.");
            }
            if ((this.ChainsawBuckQuadraticThreshold < 0.0F) || (this.ChainsawBuckQuadraticThreshold > 90.0F))
            {
                throw new NotSupportedException("Onset of quadratic chainsaw bucking time is not in the range [0.0, 25.0] m³.");
            }
            if ((this.ChainsawByOperatorCostPerSMh < 0.0F) || (this.ChainsawByOperatorCostPerSMh > 250.0F))
            {
                throw new NotSupportedException("Cost of chainsaw use by heavy equipment operator is not in the range US$ [0.0, 250.0]/PMh.");
            }
            if ((this.ChainsawByOperatorUtilization < 0.0F) || (this.ChainsawByOperatorUtilization > 250.0F))
            {
                throw new NotSupportedException("Utilization of chainsaw use by heavy equipment operator is not in the range (0.0, 1.0].");
            }
            if ((this.ChainsawFellAndBuckConstant < 0.0F) || (this.ChainsawFellAndBuckConstant > 90.0F))
            {
                throw new NotSupportedException("Intercept of chainsaw felling and bucking time is not in the range (0.0, 90.0] seconds.");
            }
            if ((this.ChainsawFellAndBuckCostPerSMh < 0.0F) || (this.ChainsawFellAndBuckCostPerSMh > 500.0F))
            {
                throw new NotSupportedException("Cost of chainsaw felling and bucking is not in the range US$ [0.0, 500.0]/SMh.");
            }
            if ((this.ChainsawFellAndBuckLinear < 0.0F) || (this.ChainsawFellAndBuckLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of chainsaw felling and bucking time is not in the range (0.0, 250.0] seconds/m³.");
            }
            if ((this.ChainsawSlopeLinear < 0.0F) || (this.ChainsawSlopeLinear > 0.1F))
            {
                throw new NotSupportedException("Linear slope coeffcient on chainsaw cycle time is not in the range of [0.0, 0.1].");
            }
            if ((this.ChainsawSlopeThresholdInPercent <= 0.0F) || (this.ChainsawSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on chainsaw crews is not in the range of [0.0, 200.0]%.");
            }
            if ((this.ChainsawFellAndBuckUtilization <= 0.0F) || (this.ChainsawFellAndBuckUtilization > 1.0F))
            {
                throw new NotSupportedException("Utilization of chainsaw felling and bucking crew is not in the range (0.0, 1.0].");
            }

            if ((this.CorridorWidth < 1.0F) || (this.CorridorWidth > 25.0F))
            {
                throw new NotSupportedException("Equipment corridor width is not in the range [1.0, 25.0].");
            }

            if ((this.CutToLengthHaulPayloadInKg < 22000.0F) || (this.CutToLengthHaulPayloadInKg > 34000.0F))
            {
                throw new NotSupportedException("Regeneration harvest haul mean load weight is not in the range [22,000, 34,000] kg.");
            }
            if ((this.CutToLengthHaulPerSMh < 0.0F) || (this.CutToLengthHaulPerSMh > 250.0F))
            {
                throw new NotSupportedException("Thinning haul cost is not in the range US$ [0.0, 250.0]/SMh.");
            }
            if ((this.CutToLengthRoundtripHaulSMh < 0.0F) || (this.CutToLengthRoundtripHaulSMh > 24.0F))
            {
                throw new NotSupportedException("Thinning haul time is not in the range [0.0, 24.0] hours.");
            }

            if ((this.FellerBuncherFellingConstant < 0.0F) || (this.FellerBuncherFellingConstant > 500.0F))
            {
                throw new NotSupportedException("Intercept of feller-buncher per tree felling time is not in the range [0.0, 500.0] seconds.");
            }
            if ((this.FellerBuncherFellingLinear < 0.0F) || (this.FellerBuncherFellingLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of feller-buncher per tree felling time is not in the range [0.0, 250.0] seconds/m³.");
            }
            if ((this.FellerBuncherCostPerSMh < 0.0F) || (this.FellerBuncherCostPerSMh > 1000.0F))
            {
                throw new NotSupportedException("Feller-buncher operating cost is not in the range US$ [0.0, 1000.0]/PMh.");
            }
            if ((this.FellerBuncherSlopeLinear < 0.0F) || (this.FellerBuncherSlopeLinear > 0.1F))
            {
                throw new NotSupportedException("Linear coefficient of slope effect on feller-buncher felling time is not in the range [0.0, 0.1].");
            }
            if ((this.FellerBuncherSlopeThresholdInPercent < 0.0F) || (this.FellerBuncherSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on feller-buncher felling time is not in the range [0.0, 200.0]%.");
            }

            if ((this.ForwarderCostPerSMh < 0.0F) || (this.ForwarderCostPerSMh > 750.0F))
            {
                throw new NotSupportedException("Forwarder operating cost is not in the range US$ [0.0, 750.0]/SMh.");
            }
            if ((this.ForwarderDriveWhileLoadingLogs < 0.65F) || (this.ForwarderDriveWhileLoadingLogs > 0.9F))
            {
                throw new NotSupportedException("Forwarder drive while loading coefficient for mean logs in payload is not in the range [0.65, 0.9].");
            }
            if ((this.ForwarderEmptyWeight < 1000.0F) || (this.ForwarderEmptyWeight > 35000.0F))
            {
                throw new NotSupportedException("Forwarder empty weight is not in the range [1000.0, 35000.0] kg.");
            }
            if ((this.ForwarderLoadMeanLogVolume < 0.5F) || (this.ForwarderLoadMeanLogVolume > 0.7F))
            {
                throw new NotSupportedException("Forwarder load coefficient for mean log volume is not in the range [0.5, 0.7].");
            }
            if ((this.ForwarderLoadPayload < 0.8F) || (this.ForwarderLoadPayload > 1.2F))
            {
                throw new NotSupportedException("Forwarder load coefficient for payload size is not in the range [0.8, 1.2].");
            }
            if ((this.ForwarderMaximumPayloadInKg < 1000.0F) || (this.ForwarderMaximumPayloadInKg > 30000.0F))
            {
                throw new NotSupportedException("Forwarder payload is not in the range [1000.0, 30000.0] kg.");
            }
            if ((this.ForwarderSpeedInStandLoadedTethered <= 0.0F) || (this.ForwarderSpeedInStandLoadedTethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder loaded travel speed while tethered is not in the range (0.0, 100.0] m/min.");
            }
            if ((this.ForwarderSpeedInStandLoadedUntethered <= 0.0F) || (this.ForwarderSpeedInStandLoadedUntethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder loaded travel speed without a tether is not in the range (0.0, 100.0] m/min.");
            }
            if ((this.ForwarderSpeedInStandUnloadedTethered <= 0.0F) || (this.ForwarderSpeedInStandUnloadedTethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder unloaded travel speed while tethered is not in the range (0.0, 100.0] m/min.");
            }
            if ((this.ForwarderSpeedInStandUnloadedUntethered <= 0.0F) || (this.ForwarderSpeedInStandUnloadedUntethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder unloaded travel speed without a tether is not in the range (0.0, 100.0] m/min.");
            }
            if ((this.ForwarderSpeedOnRoad <= 0.0F) || (this.ForwarderSpeedOnRoad > 100.0F))
            {
                throw new NotSupportedException("Forwarder travel speed on roads is not in the range (0.0, 100.0] m/min.");
            }
            if ((this.ForwarderTractiveForce < 50.0F) || (this.ForwarderTractiveForce > 500.0F))
            {
                throw new NotSupportedException("Forwarder tractive force is not in the range [50.0, 500.0] kN.");
            }
            if ((this.ForwarderUnloadLinearOneSort < 0.3F) || (this.ForwarderUnloadLinearOneSort > 1.0F))
            {
                throw new NotSupportedException("Forwarder linear unload coefficient for one sort is not in the range [0.3, 1.0].");
            }
            if ((this.ForwarderUnloadLinearTwoSorts < 0.4F) || (this.ForwarderUnloadLinearTwoSorts > 1.0F))
            {
                throw new NotSupportedException("Forwarder linear unload coefficient for two sorts is not in the range [0.4, 1.0].");
            }
            if ((this.ForwarderUnloadLinearThreeSorts < 0.5F) || (this.ForwarderUnloadLinearThreeSorts > 1.0F))
            {
                throw new NotSupportedException("Forwarder linear unload coefficient for three sorts is not in the range [0.5, 1.0].");
            }
            if ((this.ForwarderUnloadMeanLogVolume < 0.4F) || (this.ForwarderUnloadMeanLogVolume > 0.6F))
            {
                throw new NotSupportedException("Forwarder unload coefficient for mean log volume is not in the range [0.4, 0.6].");
            }
            if ((this.ForwarderUnloadPayload < 0.5F) || (this.ForwarderUnloadPayload > 0.7F))
            {
                throw new NotSupportedException("Forwarder unload coefficient for payload size is not in the range [0.5, 0.7].");
            }
            if ((this.ForwarderUtilization <= 0.0F) || (this.ForwarderUtilization > 1.0F))
            {
                throw new NotSupportedException("Forwarder utilization is not in the range (0.0, 1.0].");
            }

            if ((this.GrappleYardingConstant <= 0.0F) || (this.GrappleYardingConstant > 500.0F))
            {
                throw new NotSupportedException("Grapple yarding turn time constant is not in the range (0.0, 500.0] seconds.");
            }
            if ((this.GrappleYardingLinear <= 0.0F) || (this.GrappleYardingLinear > 5.0F))
            {
                throw new NotSupportedException("Grapple yarding turn time per meter of skyline length is not in the range (0.0, 5.0] seconds.");
            }
            if ((this.GrappleSwingYarderCostPerSMh < 100.0F) || (this.GrappleSwingYarderCostPerSMh > 750.0F))
            {
                throw new NotSupportedException("Grapple swing yarder operating cost is not in the range of US$ [100.0, 750.0]/SMh.");
            }
            if ((this.GrappleSwingYarderMaxPayload < 500.0F) || (this.GrappleSwingYarderMaxPayload > 8000.0F))
            {
                throw new NotSupportedException("Grapple swing yarder maximum payload is not in the range of [500.0, 8000.0] kg.");
            }
            if ((this.GrappleSwingYarderMeanPayload < 100.0F) || (this.GrappleSwingYarderMeanPayload > 4500.0F))
            {
                throw new NotSupportedException("Grapple swing yarder mean payload is not in the range of [100.0, 4500.0] kg.");
            }
            if ((this.GrappleSwingYarderUtilization <= 0.0F) || (this.GrappleSwingYarderUtilization > 1.0F))
            {
                throw new NotSupportedException("Grapple swing yarder utilization is not in the range of (0.0, 1.0].");
            }
            if ((this.GrappleYoaderMaxPayload < 500.0F) || (this.GrappleYoaderMaxPayload > 8000.0F))
            {
                throw new NotSupportedException("Grapple yoader maximum payload is not in the range of [500.0, 8000.0] kg.");
            }
            if ((this.GrappleYoaderMeanPayload < 100.0F) || (this.GrappleYoaderMeanPayload > 4500.0F))
            {
                throw new NotSupportedException("Grapple yoader mean payload is not in the range of [100.0, 4500.0] kg.");
            }
            if ((this.GrappleYoaderCostPerSMh < 100.0F) || (this.GrappleYoaderCostPerSMh > 750.0F))
            {
                throw new NotSupportedException("Grapple yoader operating cost is not in the range of US$ [100.0, 750.0]/PMh.");
            }
            if ((this.GrappleYoaderUtilization <= 0.0F) || (this.GrappleYoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Grapple yoader utilization is not in the range of (0.0, 1.0].");
            }

            if ((this.LoaderCostPerSMh < 0.0F) || (this.LoaderCostPerSMh > 750.0F))
            {
                throw new NotSupportedException("Loader operating cost is not in the range US$ [0.0, 750.0]/SMh.");
            }
            if ((this.LoaderProductivity < 20000.0F) || (this.LoaderProductivity > 80000.0F))
            {
                throw new NotSupportedException("Loader productivity is not in the range [20000.0, 80000.0] kg/PMh.");
            }
            if ((this.LoaderUtilization <= 0.0F) || (this.LoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Loader utilization is not in the range of (0.0, 1.0].");
            }

            if ((this.LongLogHaulPayloadInKg < 20000.0F) || (this.LongLogHaulPayloadInKg > 32000.0F))
            {
                throw new NotSupportedException("Regeneration harvest haul mean load weight is not in the range [20,000, 32,000] kg.");
            }
            if ((this.LongLogHaulPerSMh < 0.0F) || (this.LongLogHaulPerSMh > 250.0F))
            {
                throw new NotSupportedException("Regeneration harvest haul cost is not in the range US$ [0.0, 250.0]/SMh.");
            }
            if ((this.LongLogHaulRoundtripSMh < 0.0F) || (this.LongLogHaulRoundtripSMh > 24.0F))
            {
                throw new NotSupportedException("Regeneration harvest haul time is not in the range [0.0, 24.0] hours.");
            }

            if ((this.ProcessorBuckConstant < 0.0F) || (this.ProcessorBuckConstant > 500.0F))
            {
                throw new NotSupportedException("Intercept of processing time is not in the range [0.0, 500.0] seconds.");
            }
            if ((this.ProcessorBuckLinear < 0.0F) || (this.ProcessorBuckLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of processing time is not in the range [0.0, 250.0] seconds/m³.");
            }
            if ((this.ProcessorBuckQuadratic1 < 0.0F) || (this.ProcessorBuckQuadratic1 > 30.0F))
            {
                throw new NotSupportedException("First quadratic coefficient of processing time is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            if ((this.ProcessorBuckQuadratic2 < 0.0F) || (this.ProcessorBuckQuadratic2 > 30.0F))
            {
                throw new NotSupportedException("Second quadratic coefficient of processing is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            if ((this.ProcessorBuckQuadraticThreshold1 <= 0.0F) || (this.ProcessorBuckQuadraticThreshold1 > 20.0F))
            {
                throw new NotSupportedException("Onset of first quadratic increase in processing time is not in the range (0.0, 20.0] m³.");
            }
            if ((this.ProcessorBuckQuadraticThreshold2 <= 0.0F) || (this.ProcessorBuckQuadraticThreshold2 > 20.0F))
            {
                throw new NotSupportedException("Onset of second quadratic increase in processing time is not in the range (0.0, 20.0] m³.");
            }
            if ((this.ProcessorCostPerSMh < 0.0F) || (this.ProcessorCostPerSMh > 750.0F))
            {
                throw new NotSupportedException("Processor operating cost is not in the range US$ [0.0, 750.0]/PMh.");
            }
            if ((this.ProcessorUtilization <= 0.0F) || (this.LoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Processor utilization is not in the range of (0.0, 1.0].");
            }

            if ((this.TrackedHarvesterCostPerSMh < 0.0F) || (this.TrackedHarvesterCostPerSMh > 750.0F))
            {
                throw new NotSupportedException("Tracked harvester operating cost is not in the range US$ [0.0, 750.0]/SMh.");
            }
            if ((this.TrackedHarvesterFellAndBuckDiameterLimit < 30.0F) || (this.TrackedHarvesterFellAndBuckDiameterLimit > 100.0F))
            {
                throw new NotSupportedException("Intercept of tracked harvester's diameter limit for felling and bucking is not in the range [30.0, 100.0] cm.");
            }
            if ((this.TrackedHarvesterFellAndBuckConstant < 0.0F) || (this.TrackedHarvesterFellAndBuckConstant > 500.0F))
            {
                throw new NotSupportedException("Intercept of tracked harvester felling and processing time is not in the range [0.0, 500.0] seconds.");
            }
            if ((this.TrackedHarvesterFellAndBuckLinear < 0.0F) || (this.TrackedHarvesterFellAndBuckLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of tracked harvester felling and processing time is not in the range [0.0, 250.0] seconds/m³.");
            }
            if ((this.TrackedHarvesterFellAndBuckQuadratic1 < 0.0F) || (this.TrackedHarvesterFellAndBuckQuadratic1 > 30.0F))
            {
                throw new NotSupportedException("First quadratic coefficient of tracked harvester felling and processing time is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            if ((this.TrackedHarvesterFellAndBuckQuadratic2 < 0.0F) || (this.TrackedHarvesterFellAndBuckQuadratic2 > 30.0F))
            {
                throw new NotSupportedException("Second quadratic coefficient of tracked harvester felling and processing time is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            if ((this.TrackedHarvesterQuadraticThreshold1 <= 0.0F) || (this.TrackedHarvesterQuadraticThreshold1 > 20.0F))
            {
                throw new NotSupportedException("Onset of first quadratic increase in tracked harvester felling and processing time is not in the range (0.0, 20.0] m³.");
            }
            if ((this.TrackedHarvesterQuadraticThreshold2 <= 0.0F) || (this.TrackedHarvesterQuadraticThreshold2 > 20.0F))
            {
                throw new NotSupportedException("Onset of second quadratic increase in tracked harvester felling and processing time is not in the range (0.0, 20.0] m³.");
            }
            if ((this.TrackedHarvesterSlopeLinear < 0.0F) || (this.TrackedHarvesterSlopeLinear > 0.1F))
            {
                throw new NotSupportedException("Linear coefficient of slope effects on tracked harvester felling and processing time is not in the range [0.0, 0.1].");
            }
            if ((this.TrackedHarvesterSlopeThresholdInPercent < 0.0F) || (this.TrackedHarvesterSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on tracked harvester felling and processing time is not in the range [0.0, 200.0] %.");
            }
            if ((this.TrackedHarvesterUtilization <= 0.0F) || (this.TrackedHarvesterUtilization > 1.0F))
            {
                throw new NotSupportedException("Tracked harvester utilization is not in the range (0.0, 1.0].");
            }

            if ((this.WheeledHarvesterCostPerSMh < 0.0F) || (this.WheeledHarvesterCostPerSMh > 750.0F))
            {
                throw new NotSupportedException("Wheeled harvester operating cost is not in the range US$ [0.0, 750.0]/SMh.");
            }
            if ((this.WheeledHarvesterFellAndBuckDiameterLimit < 30.0F) || (this.WheeledHarvesterFellAndBuckDiameterLimit > 100.0F))
            {
                throw new NotSupportedException("Intercept of wheeled harvester's diameter limit for felling and bucking is not in the range [30.0, 100.0] cm.");
            }
            if ((this.WheeledHarvesterFellAndBuckConstant < 0.0F) || (this.WheeledHarvesterFellAndBuckConstant > 500.0F))
            {
                throw new NotSupportedException("Intercept of wheeled harvester felling and processing time is not in the range [0.0, 500.0] seconds.");
            }
            if ((this.WheeledHarvesterFellAndBuckLinear < 0.0F) || (this.WheeledHarvesterFellAndBuckLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of wheeled harvester felling and processing time is not in the range [0.0, 250.0] seconds/m³.");
            }
            if ((this.WheeledHarvesterFellAndBuckQuadratic < 0.0F) || (this.WheeledHarvesterFellAndBuckQuadratic > 30.0F))
            {
                throw new NotSupportedException("Quadratic coefficient of wheeled harvester felling and processing time is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            if ((this.WheeledHarvesterQuadraticThreshold <= 0.0F) || (this.WheeledHarvesterQuadraticThreshold > 20.0F))
            {
                throw new NotSupportedException("Onset of quadratic increase in wheeled harvester felling and processing time is not in the range (0.0, 20.0] m³.");
            }
            if ((this.WheeledHarvesterSlopeLinear < 0.0F) || (this.WheeledHarvesterSlopeLinear > 0.1F))
            {
                throw new NotSupportedException("Linear coefficient of slope effects on wheeled harvester felling and processing time is not in the range [0.0, 0.1].");
            }
            if ((this.WheeledHarvesterSlopeThresholdInPercent < 0.0F) || (this.WheeledHarvesterSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on wheeled harvester felling and processing time is not in the range [0.0, 200.0] %.");
            }
            if ((this.WheeledHarvesterUtilization <= 0.0F) || (this.WheeledHarvesterUtilization > 1.0F))
            {
                throw new NotSupportedException("Wheeled harvester utilization is not in the range (0.0, 1.0].");
            }
        }
    }
}
