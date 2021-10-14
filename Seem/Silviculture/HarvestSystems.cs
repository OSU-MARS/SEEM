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

        public float ChainsawPMh { get; set; } // $/PMh
        public float ChainsawProductivity { get; set; } // m³/PMh

        public float CorridorWidth { get; set; } // m

        public float FellerBuncherFellingConstant { get; set; } // seconds
        public float FellerBuncherFellingLinear { get; set; } // seconds/m³
        public float FellerBuncherPMh { get; set; } // $/PMh
        public float FellerBuncherSlopeThresholdInPercent { get; set; }

        public float ForwarderPayloadInKg { get; set; }
        public float ForwarderPMh { get; set; } // $/PMh
        public float ForwarderSpeedInStandLoadedTethered { get; set; } // m/min
        public float ForwarderSpeedInStandLoadedUntethered { get; set; } // m/min
        public float ForwarderSpeedInStandUnloadedTethered { get; set; } // m/min
        public float ForwarderSpeedInStandUnloadedUntethered { get; set; } // m/min
        public float ForwarderSpeedOnRoad { get; set; } // m/min

        public float GrappleYardingConstant { get; set; } // seconds
        public float GrappleYardingLinear { get; set; } // seconds/m of yarding distance
        public float GrappleSwingYarderMaxPayload { get; set; } // kg
        public float GrappleSwingYarderMeanPayload { get; set; } // kg
        public float GrappleSwingYarderSMh { get; set; } // $/PMh
        public float GrappleSwingYarderUtilization { get; set; } // fraction
        public float GrappleYoaderMaxPayload { get; set; } // kg
        public float GrappleYoaderMeanPayload { get; set; } // kg
        public float GrappleYoaderSMh { get; set; } // $/PMh
        public float GrappleYoaderUtilization { get; set; } // fraction

        public float LoaderProductivity { get; set; } // m³/PMh
        public float LoaderSMh { get; set; } // $/PMh
        public float LoaderUtilization { get; set; } // fraction

        public float ProcessorConstant { get; set; } // seconds
        // TODO: public float ProcessorDiameterLimit { get; set; } // m
        public float ProcessorLinear { get; set; } // seconds/m³
        public float ProcessorQuadratic1 { get; set; } // seconds/m⁶
        public float ProcessorQuadratic2 { get; set; } // seconds/m⁶
        public float ProcessorQuadraticThreshold1 { get; set; } // m³
        public float ProcessorQuadraticThreshold2 { get; set; } // m³
        public float ProcessorSMh { get; set; } // $/PMh
        public float ProcessorUtilization { get; set; } // fraction

        public float TrackedHarvesterDiameterLimit { get; set; } // cm
        public float TrackedHarvesterFellingConstant { get; set; } // seconds
        public float TrackedHarvesterFellingLinear { get; set; } // seconds/m³
        public float TrackedHarvesterFellingQuadratic1 { get; set; } // seconds/m⁶
        public float TrackedHarvesterFellingQuadratic2 { get; set; } // seconds/m⁶
        public float TrackedHarvesterPMh { get; set; } // $/PMh
        public float TrackedHarvesterQuadraticThreshold1 { get; set; } // m³
        public float TrackedHarvesterQuadraticThreshold2 { get; set; } // m³
        public float TrackedHarvesterSlopeThresholdInPercent { get; set; }

        public float WheeledHarvesterDiameterLimit { get; set; } // cm
        public float WheeledHarvesterFellingConstant { get; set; } // seconds
        public float WheeledHarvesterFellingLinear { get; set; } // seconds/m³
        public float WheeledHarvesterFellingQuadratic { get; set; } // seconds/m⁶
        public float WheeledHarvesterPMh { get; set; } // $/PMh
        public float WheeledHarvesterQuadraticThreshold { get; set; } // m³
        public float WheeledHarvesterSlopeThresholdInPercent { get; set; }

        static HarvestSystems()
        {
            HarvestSystems.Default = new();
        }

        // if needed, forwarder weight and engine power can be modeled to check for slope reducing loaded speeds
        public HarvestSystems()
        {
            this.ChainsawPMh = 339.00F; // $/PMh
            this.ChainsawProductivity = 16.2F; // m³/PMh
            this.CorridorWidth = 15.0F; // m

            // nominal feller-buncher, either hot saw or directional felling head
            this.FellerBuncherFellingConstant = 18.0F; // seconds
            this.FellerBuncherFellingLinear = 4.7F; // seconds/m³
            this.FellerBuncherPMh = 274.0F; // $/PMh
            this.FellerBuncherSlopeThresholdInPercent = 30.0F;

            // Ponsse Elephant King
            this.ForwarderPayloadInKg = 20000.0F;
            this.ForwarderPMh = 258.00F; // $/PMh
            this.ForwarderSpeedInStandLoadedTethered = 33.0F; // m/min
            this.ForwarderSpeedInStandLoadedUntethered = 45.0F; // m/min
            this.ForwarderSpeedInStandUnloadedTethered = 50.0F; // m/min
            this.ForwarderSpeedInStandUnloadedUntethered = 60.0F; // m/min
            this.ForwarderSpeedOnRoad = 66.0F; // m/min

            // nominal grapple yarding
            this.GrappleYardingConstant = 45.0F; // seconds
            this.GrappleYardingLinear = 0.72F; // seconds/m of yarding distance
            this.GrappleSwingYarderMaxPayload = 4000.0F; // kg
            this.GrappleSwingYarderMeanPayload = 2000.0F; // kg
            this.GrappleSwingYarderSMh = 350.00F; // $/SMh
            this.GrappleSwingYarderUtilization = 0.80F;
            this.GrappleYoaderMaxPayload = 2900.0F; // kg
            this.GrappleYoaderMeanPayload = 1550.0F; // kg
            this.GrappleYoaderSMh = 239.00F; // $/SMh
            this.GrappleYoaderUtilization = 0.75F;

            // nominal loader at landing
            this.LoaderProductivity = 72.5F; // m³/PMh
            this.LoaderSMh = 151.0F; // $/SMh
            this.LoaderUtilization = 0.85F;

            // nominal processor at landing
            this.ProcessorConstant = 21.0F; // seconds
            this.ProcessorLinear = 30.0F; // seconds/m³
            this.ProcessorQuadratic1 = 1.5F; // seconds/m⁶
            this.ProcessorQuadratic2 = 4.5F; // seconds/m⁶
            this.ProcessorQuadraticThreshold1 = 2.5F; // m³
            this.ProcessorQuadraticThreshold2 = 6.0F; // m³
            this.ProcessorSMh = 179.00F; // $/SMh
            this.ProcessorUtilization = 0.80F;

            // nominal tracked harvester
            this.TrackedHarvesterDiameterLimit = 80.0F; // cm, felling diameter and feed roller limit of H8 head
            this.TrackedHarvesterFellingConstant = 28.0F; // seconds
            this.TrackedHarvesterFellingLinear = 40.0F; // seconds/m³
            this.TrackedHarvesterFellingQuadratic1 = 3.0F; // seconds/m⁶
            this.TrackedHarvesterFellingQuadratic2 = 3.0F; // seconds/m⁶
            this.TrackedHarvesterPMh = 298.00F; // $/PMh
            this.TrackedHarvesterQuadraticThreshold1 = 2.2F; // m³
            this.TrackedHarvesterQuadraticThreshold2 = 6.0F; // m³
            this.TrackedHarvesterSlopeThresholdInPercent = 30.0F;

            // eight wheel harvesters
            // Ponsse Scorpion King
            //this.WheeledHarvesterDiameterLimit = 65.0F; // cm, feed roller limit of H7 head
            //this.WheeledHarvesterFellingConstant = 28.0F; // seconds
            //this.WheeledHarvesterFellingLinear = 43.0F; // seconds/m³
            //this.WheeledHarvesterFellingQuadratic = 8.0F; // seconds/m⁶
            //this.WheeledHarvesterOperatingCost = 301.00F; // $/PMh
            //this.WheeledHarvesterQuadraticThreshold = 1.6F; // m³
            //this.WheeledHarvesterSlopeThresholdInPercent = 45.0F;
            // Ponsse Bear
            this.WheeledHarvesterDiameterLimit = 70.0F; // cm, felling diameter and feed roller limit of H8 head
            this.WheeledHarvesterFellingConstant = 28.0F; // seconds
            this.WheeledHarvesterFellingLinear = 43.0F; // seconds/m³
            this.WheeledHarvesterFellingQuadratic = 6.0F; // seconds/m⁶
            this.WheeledHarvesterPMh = 308.00F; // $/PMh
            this.WheeledHarvesterQuadraticThreshold = 1.9F; // m³
            this.WheeledHarvesterSlopeThresholdInPercent = 45.0F;
        }

        public float GetForwardingCostForSort(Stand stand, FiaCode treeSpecies, float cubicVolumePerHa, float logsPerHa)
        {
            if (cubicVolumePerHa == 0.0F)
            {
                Debug.Assert(logsPerHa == 0.0F);
                return 0.0F;
            }
            if (cubicVolumePerHa < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(cubicVolumePerHa));
            }
            if (logsPerHa < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(logsPerHa));
            }

            TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treeSpecies];
            float treeDensityAfterProcessing = treeSpeciesProperties.GetStemDensityAfterProcessing();

            float forwarderPayloadCubic = this.ForwarderPayloadInKg / treeDensityAfterProcessing; // m³

            float logDensity = cubicVolumePerHa * this.CorridorWidth / Constant.SquareMetersPerHectare; //  m³ logs/m of corridor = m³/ha * (m²/m corridor) / m²/ha
            float loadingDistance = forwarderPayloadCubic / logDensity; // m = m³ load / m³ logs/m
            float corridorsPerLoad = loadingDistance / (stand.ForwardingDistanceInStandUntethered + stand.ForwardingDistanceInStandTethered);
            float meanLogVolume = cubicVolumePerHa / logsPerHa; // m³/log = m³/ha / logs/ha

            float outboundOnRoad = stand.ForwardingDistanceOnRoad / this.ForwarderSpeedOnRoad; // min, driving empty on road
            float outboundUntethered = stand.ForwardingDistanceInStandUntethered / this.ForwarderSpeedInStandUnloadedUntethered; // min
            float outboundTethered = stand.ForwardingDistanceInStandTethered / this.ForwarderSpeedInStandUnloadedTethered; // min
            float loading = MathF.Exp(-1.2460F + 0.9726F * MathF.Log(forwarderPayloadCubic) - 0.5955F * MathF.Log(meanLogVolume)); // min
            float drivingWhileLoading = MathF.Exp(-2.5239F + 0.7698F * MathF.Log(forwarderPayloadCubic / logDensity)); // min
            float returningTethered = MathF.Max(stand.ForwardingDistanceInStandTethered - loadingDistance, 0.0F) / this.ForwarderSpeedInStandLoadedTethered; // min
            float returningUntethered = MathF.Max(stand.ForwardingDistanceInStandUntethered + stand.ForwardingDistanceInStandTethered - loadingDistance, 0.0F) / this.ForwarderSpeedInStandLoadedUntethered; // min
            float returningOnRoad = MathF.Max(stand.ForwardingDistanceOnRoad - this.CorridorWidth * Math.Min(corridorsPerLoad - 1, 0), 0.0F) / this.ForwarderSpeedOnRoad; // min
            float unloading = MathF.Exp(-0.7620F + 0.6240F * MathF.Log(forwarderPayloadCubic) - 0.4860F * MathF.Log(meanLogVolume));
            float turnTime = outboundOnRoad + corridorsPerLoad * (outboundUntethered + outboundTethered) + loading + drivingWhileLoading + corridorsPerLoad * (returningTethered + returningUntethered) + returningOnRoad + unloading; // min, tether attachment and removal are treated as delays
            float forwarderProductivity = 60.0F / turnTime * (1.0F - treeSpeciesProperties.BarkFractionRemainingAfterProcessing) * forwarderPayloadCubic; // merchantable m³/PMh = 60 minutes/hour / minutes * m³ net of bark volume remaining after processing
            float forwarderCostPerHectare = cubicVolumePerHa / forwarderProductivity * this.ForwarderPMh; // $/ha = m³/ha / m³/PMh * $/PMh
            Debug.Assert((forwarderCostPerHectare > 0.0F) && (forwarderProductivity > 0.0F) && (turnTime > 0.0F) && (outboundOnRoad >= 0.0F) && (outboundUntethered >= 0.0F) && (outboundTethered >= 0.0F) && (loading > 0.0F) && (drivingWhileLoading > 0.0F) && (returningTethered >= 0.0F) && (returningUntethered >= 0.0F) && (returningOnRoad >= 0.0F) && (unloading > 0.0F));
            return forwarderCostPerHectare;
        }

        // TODO: check tracked harvester
        public float GetHarvesterFellingAndProcessingCost(Stand stand, SortedList<FiaCode, TreeSelection> individualTreeSelectionBySpecies, int harvestPeriod)
        {
            float chainsawBuckingVolume = 0.0F; // total m³/ha of trees too large for harvester
            float wheeledHarvesterFellingTime = 0.0F; // seconds/ha
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

                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                    if (individualTreeSelection[uncompactedTreeIndex] != harvestPeriod)
                    {
                        // tree was either removed previously or was retained rather than thinned
                        continue;
                    }

                    // mechanized felling time
                    // Could factor constant and linear terms out of loop.
                    float treeMerchantableCubicVolume = treesOfSpecies.MerchantableCubicVolumePerStem[compactedTreeIndex];
                    Debug.Assert(Single.IsNaN(treeMerchantableCubicVolume) == false);
                    float treeFellingTime = this.WheeledHarvesterFellingConstant + this.WheeledHarvesterFellingLinear * treeMerchantableCubicVolume;
                    if (treeMerchantableCubicVolume > this.WheeledHarvesterQuadraticThreshold)
                    {
                        float volumeBeyondThreshold = treeMerchantableCubicVolume - this.WheeledHarvesterQuadraticThreshold;
                        treeFellingTime += this.WheeledHarvesterFellingQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                    }

                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    wheeledHarvesterFellingTime += expansionFactorPerHa * treeFellingTime;

                    // additional manual felling effort if tree is too large for processing head or beyond machine limits
                    // For now, assume manual time is in addition to mechanical time to account for harvester delays dealing with tree
                    // or chainsaw crew time to access tree. This reasonably plausible when small numbers of trees are too large for the
                    // harvester but is likely an overestimate if many trees are oversize.
                    float dbhInCm = diameterToCentimetersMultiplier * treesOfSpecies.Dbh[compactedTreeIndex];
                    if (dbhInCm > this.WheeledHarvesterDiameterLimit)
                    {
                        chainsawBuckingVolume += expansionFactorPerHa * treeMerchantableCubicVolume;
                    }
                }
            }

            // for now, assume uniform slope across stand
            if (stand.SlopeInPercent > this.WheeledHarvesterSlopeThresholdInPercent)
            {
                float harvesterSlopeMultiplier = 1.0F + 0.01F * (stand.SlopeInPercent - this.WheeledHarvesterSlopeThresholdInPercent);
                wheeledHarvesterFellingTime *= harvesterSlopeMultiplier;
            }

            float chainsawPMhPerHectare = chainsawBuckingVolume / this.ChainsawProductivity;
            float chainsawCostPerHectare = this.ChainsawPMh * chainsawPMhPerHectare;

            float wheeledHarvesterPMhPerHectare = wheeledHarvesterFellingTime / 3600.0F;
            float wheeledHarvesterCostPerHectare = this.WheeledHarvesterPMh * wheeledHarvesterPMhPerHectare;

            float harvestCost = chainsawCostPerHectare + wheeledHarvesterCostPerHectare;
            return harvestCost;
        }

        public LongLogHarvest GetLongLogHarvestCosts(Stand stand)
        {
            // whole tree grapple yarding
            float chainsawBuckingVolumeWithGrappleSwingYarder = 0.0F; // m³
            float chainsawBuckingVolumeWithGrappleYoader = 0.0F; // m³
            float chainsawBuckingVolumeWithTrackedHarvester = 0.0F; // m³
            float chainsawBuckingVolumeWithWheeledHarvester = 0.0F; // m³
            float fellerBuncherFellingTime = 0.0F; // seconds/ha
            LongLogHarvest longLogHarvest = new();
            float processingTimeWithGrappleSwingYarder = 0.0F; // seconds/ha
            float processingTimeWithGrappleYoader = 0.0F; // seconds/ha
            float totalMerchantableCubicVolume = 0.0F; // m³
            float totalYardedWeight = 0.0F; // kg
            float trackedHarvesterFellingAndProcessingTime = 0.0F; // seconds/ha
            float wheeledHarvesterFellingAndProcessingTime = 0.0F; // seconds/ha
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
                float treeSpeciesStemDensity = treeSpeciesProperties.GetStemDensity();
                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeMerchantableCubicVolumeInM3 = treesOfSpecies.MerchantableCubicVolumePerStem[compactedTreeIndex];
                    Debug.Assert(Single.IsNaN(treeMerchantableCubicVolumeInM3) == false);

                    // feller-buncher: hot saw or directional felling head
                    float treeFellingTime = this.FellerBuncherFellingConstant + this.FellerBuncherFellingLinear * treeMerchantableCubicVolumeInM3;
                    fellerBuncherFellingTime += expansionFactorPerHa * treeFellingTime;

                    // harvester
                    float trackedHarvesterTreeFellingAndProcessingTime = this.TrackedHarvesterFellingConstant + this.TrackedHarvesterFellingLinear * treeMerchantableCubicVolumeInM3;
                    trackedHarvesterFellingAndProcessingTime += expansionFactorPerHa * trackedHarvesterTreeFellingAndProcessingTime;
                    if (treeMerchantableCubicVolumeInM3 > this.TrackedHarvesterQuadraticThreshold1)
                    {
                        float volumeBeyondThreshold = treeMerchantableCubicVolumeInM3 - this.TrackedHarvesterQuadraticThreshold1;
                        treeFellingTime += this.TrackedHarvesterFellingQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                    }
                    if (treeMerchantableCubicVolumeInM3 > this.TrackedHarvesterQuadraticThreshold2)
                    {
                        float volumeBeyondThreshold = treeMerchantableCubicVolumeInM3 - this.TrackedHarvesterQuadraticThreshold2;
                        treeFellingTime += this.TrackedHarvesterFellingQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                    }

                    float wheeledHarvesterTreeFellingAndProcessingTime = this.WheeledHarvesterFellingConstant + this.WheeledHarvesterFellingLinear * treeMerchantableCubicVolumeInM3;
                    wheeledHarvesterFellingAndProcessingTime += expansionFactorPerHa * wheeledHarvesterTreeFellingAndProcessingTime;
                    if (treeMerchantableCubicVolumeInM3 > this.WheeledHarvesterQuadraticThreshold)
                    {
                        float volumeBeyondThreshold = treeMerchantableCubicVolumeInM3 - this.WheeledHarvesterQuadraticThreshold;
                        treeFellingTime += this.WheeledHarvesterFellingQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                    }

                    // yarder
                    float woodAndBarkVolumePerStem = treeMerchantableCubicVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFraction);
                    float yardedWeightPerStem = woodAndBarkVolumePerStem * treeSpeciesStemDensity;
                    totalYardedWeight += expansionFactorPerHa * yardedWeightPerStem;

                    // chainsaw bucking of trees too large for harvester or to fit trees in yarder weight limit
                    // For now,
                    //   1) Assume he feller-buncher has a directional felling head if trees are too large for a hot saw and that
                    //      no trees are too large for a directional felling head.
                    //   2) Neglect effects of short log production when trees are large enough full size logs still exceed the
                    //      yoader weight limit.
                    float dbhInCm = diameterToCentimetersMultiplier * treesOfSpecies.Dbh[compactedTreeIndex];
                    float heightInM = heightToMetersMultiplier * treesOfSpecies.Height[compactedTreeIndex];
                    if (dbhInCm > this.TrackedHarvesterDiameterLimit)
                    {
                        chainsawBuckingVolumeWithTrackedHarvester += expansionFactorPerHa * treeMerchantableCubicVolumeInM3;
                    }
                    if (dbhInCm > this.WheeledHarvesterDiameterLimit)
                    {
                        chainsawBuckingVolumeWithWheeledHarvester += expansionFactorPerHa * treeMerchantableCubicVolumeInM3;
                    }
                    // crude Smalian estimate of first log weight for checking against yarder payload limits
                    // For now, the tree's first log is treated as a truncated cone. A more complete implementation would look up the
                    // tree's first log's merchantable volume from the volume table and adjust for bark.
                    float conicalTaper = dbhInCm / (heightInM - Constant.DbhHeightInM); // cm/m
                    float firstLogTopDiameter = dbhInCm - conicalTaper * (Constant.Bucking.DefaultLongLogLength - Constant.DbhHeightInM + Constant.Bucking.DefaultStumpHeight);
                    float firstLogBottomDiameter = dbhInCm + conicalTaper * (Constant.DbhHeightInM - Constant.Bucking.DefaultStumpHeight);
                    float approximateFirstLogWeight = 0.25F * MathF.PI * 0.0001F * 0.5F * (firstLogTopDiameter * firstLogTopDiameter + firstLogBottomDiameter * firstLogBottomDiameter) * Constant.Bucking.DefaultLongLogLength * treeSpeciesStemDensity;

                    bool treeBuckedManuallyWithGrappleSwingYarder = false;
                    if (yardedWeightPerStem > this.GrappleSwingYarderMaxPayload)
                    {
                        chainsawBuckingVolumeWithGrappleSwingYarder += expansionFactorPerHa * treeMerchantableCubicVolumeInM3;
                        treeBuckedManuallyWithGrappleSwingYarder = true;

                        if (approximateFirstLogWeight > this.GrappleSwingYarderMaxPayload)
                        {
                            longLogHarvest.GrappleSwingYarderOverweightFirstLogs += expansionFactorPerHa;
                        }
                    }

                    bool treeBuckedManuallyWithGrappleYoader = false;
                    if (yardedWeightPerStem > this.GrappleYoaderMaxPayload)
                    {
                        treeBuckedManuallyWithGrappleYoader = true;
                        chainsawBuckingVolumeWithGrappleYoader += expansionFactorPerHa * treeMerchantableCubicVolumeInM3;

                        if (approximateFirstLogWeight > this.GrappleYoaderMaxPayload)
                        {
                            longLogHarvest.GrappleYoaderOverweightFirstLogs += expansionFactorPerHa;
                        }
                    }

                    // processor
                    float treeProcessingTimeWithGrappleSwingYarder = this.ProcessorConstant + this.ProcessorLinear * treeMerchantableCubicVolumeInM3;
                    if (treeBuckedManuallyWithGrappleSwingYarder == false)
                    {
                        if (treeMerchantableCubicVolumeInM3 > this.ProcessorQuadraticThreshold1)
                        {
                            float volumeBeyondThreshold = treeMerchantableCubicVolumeInM3 - this.ProcessorQuadraticThreshold1;
                            treeProcessingTimeWithGrappleSwingYarder += this.ProcessorQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        if (treeMerchantableCubicVolumeInM3 > this.ProcessorQuadraticThreshold2)
                        {
                            float volumeBeyondThreshold = treeMerchantableCubicVolumeInM3 - this.ProcessorQuadraticThreshold2;
                            treeProcessingTimeWithGrappleSwingYarder += this.ProcessorQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                    }
                    processingTimeWithGrappleSwingYarder += expansionFactorPerHa * treeProcessingTimeWithGrappleSwingYarder;

                    float treeProcessingTimeWithGrappleYoader = this.ProcessorConstant + this.ProcessorLinear * treeMerchantableCubicVolumeInM3;
                    if (treeBuckedManuallyWithGrappleYoader == false)
                    {
                        if (treeMerchantableCubicVolumeInM3 > this.ProcessorQuadraticThreshold1)
                        {
                            float volumeBeyondThreshold = treeMerchantableCubicVolumeInM3 - this.ProcessorQuadraticThreshold1;
                            treeProcessingTimeWithGrappleYoader += this.ProcessorQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        if (treeMerchantableCubicVolumeInM3 > this.ProcessorQuadraticThreshold2)
                        {
                            float volumeBeyondThreshold = treeMerchantableCubicVolumeInM3 - this.ProcessorQuadraticThreshold2;
                            treeProcessingTimeWithGrappleYoader += this.ProcessorQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                    }
                    processingTimeWithGrappleYoader += expansionFactorPerHa * treeProcessingTimeWithGrappleYoader;

                    // loader cost and machine productivity calculations
                    totalMerchantableCubicVolume += expansionFactorPerHa * treeMerchantableCubicVolumeInM3;
                }
            }

            // decoupled feller-buncher, harvester, and chainsaw bucking costs
            // For now, assume uniform slope across stand.
            if (stand.SlopeInPercent > this.FellerBuncherSlopeThresholdInPercent)
            {
                float fellerBuncherSlopeMultiplier = 1.0F + 0.01F * (stand.SlopeInPercent - this.FellerBuncherSlopeThresholdInPercent);
                fellerBuncherFellingTime *= fellerBuncherSlopeMultiplier;
            }
            float fellerBuncherPMhPerHectare = fellerBuncherFellingTime / 3600.0F;
            float fellerBuncherCostPerHectare = this.FellerBuncherPMh * fellerBuncherPMhPerHectare;

            if (stand.SlopeInPercent > this.TrackedHarvesterSlopeThresholdInPercent)
            {
                float harvesterSlopeMultiplier = 1.0F + 0.01F * (stand.SlopeInPercent - this.TrackedHarvesterSlopeThresholdInPercent);
                trackedHarvesterFellingAndProcessingTime *= harvesterSlopeMultiplier;
            }
            float trackedHarvesterPMhPerHectare = trackedHarvesterFellingAndProcessingTime / 3600.0F;
            float trackedHarvesterCostPerHectare = this.TrackedHarvesterPMh * trackedHarvesterPMhPerHectare;

            if (stand.SlopeInPercent > this.WheeledHarvesterSlopeThresholdInPercent)
            {
                float harvesterSlopeMultiplier = 1.0F + 0.01F * (stand.SlopeInPercent - this.WheeledHarvesterSlopeThresholdInPercent);
                wheeledHarvesterFellingAndProcessingTime *= harvesterSlopeMultiplier;
            }
            float wheeledHarvesterPMhPerHectare = wheeledHarvesterFellingAndProcessingTime / 3600.0F;
            float wheeledHarvesterCostPerHectare = this.WheeledHarvesterPMh * wheeledHarvesterPMhPerHectare;

            float chainsawPMhPerHectareWithTrackedHarvester = chainsawBuckingVolumeWithTrackedHarvester / this.ChainsawProductivity;
            float chainsawCostWithTrackedHarvester = this.ChainsawPMh * chainsawPMhPerHectareWithTrackedHarvester;

            float chainsawPMhPerHectareWithWheeledHarvester = chainsawBuckingVolumeWithWheeledHarvester / this.ChainsawProductivity;
            float chainsawCostWithWheeledHarvester = this.ChainsawPMh * chainsawPMhPerHectareWithWheeledHarvester;

            float chainsawPMhPerHectareWithGrappleSwingYarder = chainsawBuckingVolumeWithGrappleSwingYarder / this.ChainsawProductivity;
            float chainsawCostWithGrappleSwingYarder = this.ChainsawPMh * chainsawPMhPerHectareWithGrappleSwingYarder;

            float chainsawPMhPerHectareWithGrappleYoader = chainsawBuckingVolumeWithGrappleYoader / this.ChainsawProductivity;
            float chainsawCostWithGrappleYoader = this.ChainsawPMh * chainsawPMhPerHectareWithGrappleYoader;

            // coupled yarder-processor-loader costs
            float meanGrappleYardingTurnTime = this.GrappleYardingConstant + this.GrappleYardingLinear * 0.5F * stand.SkylineLength; // parallel yarding
            float grappleSwingYarderTurnsPerHectare = totalYardedWeight / this.GrappleSwingYarderMeanPayload;
            float grappleSwingYarderPMhPerHectare = grappleSwingYarderTurnsPerHectare * meanGrappleYardingTurnTime / 3600.0F;
            float grappleSwingYarderSMhPerHectare = grappleSwingYarderPMhPerHectare / this.GrappleSwingYarderUtilization;
            float processorPMhPerHectareWithGrappleSwingYarder = processingTimeWithGrappleSwingYarder / 3600.0F;
            float processorSMhPerHectareWithGrappleSwingYarder = processorPMhPerHectareWithGrappleSwingYarder / this.ProcessorUtilization;

            float grappleYoaderTurnsPerHectare = totalYardedWeight / this.GrappleYoaderMeanPayload;
            float grappleYoaderPMhPerHectare = grappleYoaderTurnsPerHectare * meanGrappleYardingTurnTime / 3600.0F;
            float grappleYoaderSMhPerHectare = grappleYoaderPMhPerHectare / this.GrappleYoaderUtilization;
            float processorPMhPerHectareWithGrappleYoader = processingTimeWithGrappleYoader / 3600.0F;
            float processorSMhPerHectareWithGrappleYoader = processorPMhPerHectareWithGrappleYoader / this.ProcessorUtilization;

            float loaderPMhPerHectare = totalMerchantableCubicVolume / this.LoaderProductivity;
            float loaderSMhPerHectare = loaderPMhPerHectare / this.LoaderUtilization;

            float limitingSMhWithFellerBuncherAndGrappleSwingYarder = MathE.Max(grappleSwingYarderSMhPerHectare, processorSMhPerHectareWithGrappleSwingYarder, loaderSMhPerHectare);
            float grappleSwingYarderCostPerHectareWithFellerBuncher = this.GrappleSwingYarderSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
            float processorCostPerHectareWithGrappleSwingYarder = this.ProcessorSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
            float loaderCostPerHectareWithFellerBuncherAndGrappleSwingYarder = this.LoaderSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
            longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCost = chainsawCostWithGrappleSwingYarder + fellerBuncherCostPerHectare + grappleSwingYarderCostPerHectareWithFellerBuncher + processorCostPerHectareWithGrappleSwingYarder + loaderCostPerHectareWithFellerBuncherAndGrappleSwingYarder;

            float limitingSMhWithFellerBuncherAndGrappleYoader = MathE.Max(grappleYoaderSMhPerHectare, processorSMhPerHectareWithGrappleYoader, loaderSMhPerHectare);
            float grappleYoaderCostPerHectareWithFellerBuncher = this.GrappleYoaderSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
            float processorCostPerHectareWithGrappleYoader = this.ProcessorSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
            float loaderCostPerHectareWithFellerBuncherAndGrappleYoader = this.LoaderSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
            longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCost = chainsawCostWithGrappleYoader + fellerBuncherCostPerHectare + grappleYoaderCostPerHectareWithFellerBuncher + processorCostPerHectareWithGrappleYoader + loaderCostPerHectareWithFellerBuncherAndGrappleYoader;

            float limitingSMhWithTrackedHarvesterAndGrappleSwingYarder = MathF.Max(grappleSwingYarderSMhPerHectare, loaderSMhPerHectare);
            float swingYarderCostPerHectareWithTrackedHarvester = this.GrappleSwingYarderSMh * limitingSMhWithTrackedHarvesterAndGrappleSwingYarder;
            float loaderCostPerHectareWithTrackedHarvesterAndGrappleSwingYarder = this.LoaderSMh * limitingSMhWithTrackedHarvesterAndGrappleSwingYarder;
            longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCost = chainsawCostWithTrackedHarvester + trackedHarvesterCostPerHectare + swingYarderCostPerHectareWithTrackedHarvester + loaderCostPerHectareWithTrackedHarvesterAndGrappleSwingYarder;

            float limitingSMhWithTrackedHarvesterAndGrappleYoader = MathF.Max(grappleYoaderSMhPerHectare, loaderSMhPerHectare);
            float grappleYoaderCostPerHectareWithTrackedHarvester = this.GrappleYoaderSMh * limitingSMhWithTrackedHarvesterAndGrappleYoader;
            float loaderCostPerHectareWithTrackedHarvesterAndGrappleYoader = this.LoaderSMh * limitingSMhWithTrackedHarvesterAndGrappleYoader;
            longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCost = chainsawCostWithTrackedHarvester + trackedHarvesterCostPerHectare + grappleYoaderCostPerHectareWithTrackedHarvester + loaderCostPerHectareWithTrackedHarvesterAndGrappleYoader;

            float limitingSMhWithWheeledHarvesterAndGrappleSwingYarder = MathF.Max(grappleSwingYarderSMhPerHectare, loaderSMhPerHectare);
            float swingYarderCostPerHectareWithWheeledHarvester = this.GrappleSwingYarderSMh * limitingSMhWithWheeledHarvesterAndGrappleSwingYarder;
            float loaderCostPerHectareWithWheeledHarvesterAndGrappleSwingYarder = this.LoaderSMh * limitingSMhWithWheeledHarvesterAndGrappleSwingYarder;
            longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCost = chainsawCostWithWheeledHarvester + wheeledHarvesterCostPerHectare + swingYarderCostPerHectareWithWheeledHarvester + loaderCostPerHectareWithWheeledHarvesterAndGrappleSwingYarder;

            float limitingSMhWithWheeledHarvesterAndGrappleYoader = MathF.Max(grappleYoaderSMhPerHectare, loaderSMhPerHectare);
            float grappleYoaderCostPerHectareWithWheeledHarvester = this.GrappleYoaderSMh * limitingSMhWithWheeledHarvesterAndGrappleYoader;
            float loaderCostPerHectareWithWheeledHarvesterAndGrappleYoader = this.LoaderSMh * limitingSMhWithWheeledHarvesterAndGrappleYoader;
            longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCost = chainsawCostWithWheeledHarvester + wheeledHarvesterCostPerHectare + grappleYoaderCostPerHectareWithWheeledHarvester + loaderCostPerHectareWithWheeledHarvesterAndGrappleYoader;

            // equipment productivity
            // Chainsaw and loader productivity are specified, so don't need to be output.
            longLogHarvest.Productivity.FellerBuncher = totalMerchantableCubicVolume / fellerBuncherPMhPerHectare;
            longLogHarvest.Productivity.GrappleSwingYarder = totalMerchantableCubicVolume / grappleSwingYarderPMhPerHectare;
            longLogHarvest.Productivity.GrappleYoader = totalMerchantableCubicVolume / grappleYoaderPMhPerHectare;
            longLogHarvest.Productivity.ProcessorWithGrappleSwingYarder = totalMerchantableCubicVolume / processorPMhPerHectareWithGrappleSwingYarder;
            longLogHarvest.Productivity.ProcessorWithGrappleYoader = totalMerchantableCubicVolume / processorPMhPerHectareWithGrappleYoader;
            longLogHarvest.Productivity.TrackedHarvester = totalMerchantableCubicVolume / trackedHarvesterPMhPerHectare;
            longLogHarvest.Productivity.WheeledHarvester = totalMerchantableCubicVolume / wheeledHarvesterPMhPerHectare;

            // utilization not currently calculated but may be of debugging interest
            //float swingYarderCoupledUtilization = this.GrappleSwingYarderUtilization * yoaderSMhPerHectare / limitingSMh;
            //float yoaderCoupledUtilization = this.GrappleYoaderUtilization * yoaderSMhPerHectare / limitingSMh;
            //float processorCoupledUtilization = this.ProcessorUtilization * processorSMhPerHectare / limitingSMh;
            //float loaderCoupledUtilization = this.LoaderUtilization * loaderSMhPerHectare / limitingSMh;

            Debug.Assert((limitingSMhWithFellerBuncherAndGrappleSwingYarder > 0.0F) && (Single.IsNaN(longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCost) == false) && (longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCost > 0.0F) && (longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCost < 50000.0F) &&
                         (limitingSMhWithFellerBuncherAndGrappleYoader > 0.0F) && (Single.IsNaN(longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCost) == false) && (longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCost > 0.0F) && (longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCost < 50000.0F) &&
                         (limitingSMhWithTrackedHarvesterAndGrappleSwingYarder > 0.0F) && (Single.IsNaN(longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCost) == false) && (longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCost > 0.0F) && (longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCost < 50000.0F) &&
                         (limitingSMhWithTrackedHarvesterAndGrappleYoader > 0.0F) && (Single.IsNaN(longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCost) == false) && (longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCost > 0.0F) && (longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCost < 50000.0F) &&
                         (limitingSMhWithWheeledHarvesterAndGrappleSwingYarder > 0.0F) && (Single.IsNaN(longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCost) == false) && (longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCost > 0.0F) && (longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCost < 50000.0F) &&
                         (limitingSMhWithWheeledHarvesterAndGrappleYoader > 0.0F) && (Single.IsNaN(longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCost) == false) && (longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCost > 0.0F) && (longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCost < 50000.0F));
            longLogHarvest.MinimumCost = MathE.Min(longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCost, longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCost,
                                                   longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCost, longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCost,
                                                   longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCost, longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCost);
            return longLogHarvest;
        }
    }
}
