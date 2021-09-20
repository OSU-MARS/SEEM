using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Silviculture
{
    public class HarvestSystems
    {
        public static HarvestSystems Default { get; private set; }

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
        public float GrappleYoaderMeanPayload { get; set; } // kg
        public float GrappleYoaderSMh { get; set; } // $/PMh
        public float GrappleYoaderUtilization { get; set; } // fraction

        public float HarvesterFellingConstant{ get; set; } // seconds
        public float HarvesterFellingLinear { get; set; } // seconds/m³
        public float HarvesterFellingQuadratic { get; set; } // seconds/m⁶
        public float HarvesterPMh { get; set; } // $/PMh
        public float HarvesterQuadraticThreshold { get; set; } // m³
        public float HarvesterSlopeThresholdInPercent { get; set; }

        public float LoaderProductivity { get; set; } // m³/PMh
        public float LoaderSMh { get; set; } // $/PMh
        public float LoaderUtilization { get; set; } // fraction

        public float ProcessorConstant { get; set; } // seconds
        public float ProcessorLinear { get; set; } // seconds/m³
        public float ProcessorQuadratic1 { get; set; } // seconds/m⁶
        public float ProcessorQuadratic2 { get; set; } // seconds/m⁶
        public float ProcessorQuadraticThreshold1 { get; set; } // m³
        public float ProcessorQuadraticThreshold2 { get; set; } // m³
        public float ProcessorSMh { get; set; } // $/PMh
        public float ProcessorUtilization { get; set; } // fraction

        static HarvestSystems()
        {
            HarvestSystems.Default = new();
        }

        // if needed, forwarder weight and engine power can be modeled to check for slope reducing loaded speeds
        public HarvestSystems()
        {
            this.CorridorWidth = 15.0F; // m

            // nominal feller-buncher, either hot saw or directional felling head
            this.FellerBuncherFellingConstant = 14.0F; // seconds
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
            this.GrappleYoaderMeanPayload = 1550.0F; // kg
            this.GrappleYoaderSMh = 239.00F; // $/SMh
            this.GrappleYoaderUtilization = 0.75F;

            // Ponsse Scorpion King
            //this.HarvesterFellingConstant = 28.0F; // seconds
            //this.HarvesterFellingLinear = 43.0F; // seconds/m³
            //this.HarvesterFellingQuadratic = 8.0F; // seconds/m⁶
            //this.HarvesterOperatingCost = 301.00F; // $/PMh
            //this.HarvesterQuadraticThreshold = 1.6F; // m³
            //this.HarvesterSlopeThresholdInPercent = 45.0F;
            // Ponsse Bear
            this.HarvesterFellingConstant = 28.0F; // seconds
            this.HarvesterFellingLinear = 43.0F; // seconds/m³
            this.HarvesterFellingQuadratic = 6.0F; // seconds/m⁶
            this.HarvesterPMh = 308.00F; // $/PMh
            this.HarvesterQuadraticThreshold = 1.9F; // m³
            this.HarvesterSlopeThresholdInPercent = 45.0F;

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
        }

        public float GetFellerBuncherYarderProcessorLoaderCost(Stand stand)
        {
            // whole tree grapple yarding
            float fellerBuncherFellingTime = 0.0F; // seconds/ha
            float processingTime = 0.0F; // seconds/ha
            float totalMerchantableCubicVolume = 0.0F; // m³
            float totalYardedWeight = 0.0F; // kg
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                float hectareExpansionFactorMultiplier = 1.0F;
                if (treesOfSpecies.Units == Units.English)
                {
                    hectareExpansionFactorMultiplier = Constant.AcresPerHectare;
                }

                float totalMerchantableVolumeForSpecies = 0.0F; // m³
                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    float expansionFactor = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeMerchantableCubicVolume = treesOfSpecies.MerchantableCubicVolumePerStem[compactedTreeIndex];
                    Debug.Assert(Single.IsNaN(treeMerchantableCubicVolume) == false);

                    // feller buncher
                    float treeFellingTime = this.FellerBuncherFellingConstant + this.FellerBuncherFellingLinear * treeMerchantableCubicVolume;
                    fellerBuncherFellingTime += expansionFactor * treeFellingTime;

                    // accumulate volume for yarding and loader calculations below
                    totalMerchantableVolumeForSpecies += treeMerchantableCubicVolume;

                    // processor
                    float treeProcessingTime = this.ProcessorConstant + this.ProcessorLinear * treeMerchantableCubicVolume;
                    if (treeMerchantableCubicVolume > this.ProcessorQuadraticThreshold1)
                    {
                        float volumeBeyondThreshold = treeMerchantableCubicVolume - this.ProcessorQuadraticThreshold1;
                        treeProcessingTime += this.ProcessorQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                    }
                    if (treeMerchantableCubicVolume > this.ProcessorQuadraticThreshold2)
                    {
                        float volumeBeyondThreshold = treeMerchantableCubicVolume - this.ProcessorQuadraticThreshold2;
                        treeProcessingTime += this.ProcessorQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                    }

                    processingTime += expansionFactor * treeProcessingTime;
                }

                // yarding
                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treesOfSpecies.Species];
                float totalYardedVolumeForSpecies = totalMerchantableVolumeForSpecies / (1.0F - treeSpeciesProperties.BarkFraction);
                float barkVolumeForSpecies = totalYardedVolumeForSpecies - totalMerchantableVolumeForSpecies;
                float yardedWeightForSpecies = totalMerchantableVolumeForSpecies * treeSpeciesProperties.WoodDensity + barkVolumeForSpecies * treeSpeciesProperties.BarkDensity;
                totalMerchantableCubicVolume += totalMerchantableVolumeForSpecies;
                totalYardedWeight += yardedWeightForSpecies;
            }

            // decoupled feller buncher cost
            // For now, assume uniform slope across stand.
            if (stand.SlopeInPercent > this.FellerBuncherSlopeThresholdInPercent)
            {
                float fellerBuncherSlopeMultiplier = 1.0F + 0.01F * (stand.SlopeInPercent - this.FellerBuncherSlopeThresholdInPercent);
                fellerBuncherFellingTime *= fellerBuncherSlopeMultiplier;
            }
            float fellerBuncherPMhPerHectare = fellerBuncherFellingTime / 3600.0F;
            float fellerBuncherCostPerHectare = this.FellerBuncherPMh * fellerBuncherPMhPerHectare;

            // coupled yarder-processor-loader productivity
            float meanYoaderTurnTime = this.GrappleYardingConstant + this.GrappleYardingLinear * 0.5F * stand.SkylineLength; // parallel yarding
            float yoaderTurnsPerHectare = totalYardedWeight / this.GrappleYoaderMeanPayload;

            float yoaderPMhPerHectare = yoaderTurnsPerHectare * meanYoaderTurnTime / 3600.0F;
            float yoaderSMhPerHectare = yoaderPMhPerHectare / this.GrappleYoaderUtilization;

            float processorPMhPerHectare = processingTime / 3600.0F;
            float processorSMhPerHectare = processorPMhPerHectare / this.ProcessorUtilization;

            float loaderPMhPerHectare = totalMerchantableCubicVolume / this.LoaderProductivity;
            float loaderSMhPerHectare = loaderPMhPerHectare / this.LoaderUtilization;

            float limitingSMh = MathF.Max(MathF.Max(yoaderSMhPerHectare, processorSMhPerHectare), loaderSMhPerHectare);
            // not needed but potentially of debugging interest
            //float yoaderCoupledUtilization = this.GrappleYoaderUtilization * yoaderSMhPerHectare / limitingSMh;
            //float processorCoupledUtilization = this.ProcessorUtilization * processorSMhPerHectare / limitingSMh;
            //float loaderCoupledUtilization = this.LoaderUtilization * loaderSMhPerHectare / limitingSMh;

            float yoaderCostPerHectare = this.GrappleYoaderSMh * limitingSMh;
            float processorCostPerHectare = this.ProcessorSMh * limitingSMh;
            float loaderCostPerHectare = this.LoaderSMh * limitingSMh;

            float harvestCost = fellerBuncherCostPerHectare + yoaderCostPerHectare + processorCostPerHectare + loaderCostPerHectare;
            Debug.Assert((limitingSMh > 0.0F) && (Single.IsNaN(harvestCost) == false) && (harvestCost > 0.0F));
            return harvestCost;
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
            float unloading = MathF.Exp(-0.762F + 0.6240F * MathF.Log(forwarderPayloadCubic) - 0.486F * MathF.Log(meanLogVolume));
            float turnTime = outboundOnRoad + corridorsPerLoad * (outboundUntethered + outboundTethered) + loading + drivingWhileLoading + corridorsPerLoad * (returningTethered + returningUntethered) + returningOnRoad + unloading; // min, tether attachment and removal are treated as delays
            float forwarderProductivity = 60.0F / turnTime * (1.0F - treeSpeciesProperties.BarkFractionRemainingAfterProcessing) * forwarderPayloadCubic; // merchantable m³/PMh = 60 minutes/hour / minutes * m³ net of bark volume remaining after processing
            float forwarderCostPerHectare = cubicVolumePerHa / forwarderProductivity * this.ForwarderPMh; // $/ha = m³/ha / m³/PMh * $/PMh
            Debug.Assert((forwarderCostPerHectare > 0.0F) && (forwarderProductivity > 0.0F) && (turnTime > 0.0F) && (outboundOnRoad >= 0.0F) && (outboundUntethered >= 0.0F) && (outboundTethered >= 0.0F) && (loading > 0.0F) && (drivingWhileLoading > 0.0F) && (returningTethered >= 0.0F) && (returningUntethered >= 0.0F) && (returningOnRoad >= 0.0F) && (unloading > 0.0F));
            return forwarderCostPerHectare;
        }

        public float GetHarvesterFellingAndProcessingCost(Stand stand, SortedList<FiaCode, TreeSelection> individualTreeSelectionBySpecies, int harvestPeriod)
        {
            float harvesterFellingTime = 0.0F; // seconds/ha
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                float hectareExpansionFactorMultiplier = 1.0F;
                if (treesOfSpecies.Units == Units.English)
                {
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

                    float treeMerchantableCubicVolume = treesOfSpecies.MerchantableCubicVolumePerStem[compactedTreeIndex];
                    Debug.Assert(Single.IsNaN(treeMerchantableCubicVolume) == false);
                    float treeFellingTime = this.HarvesterFellingConstant + this.HarvesterFellingLinear * treeMerchantableCubicVolume;
                    if (treeMerchantableCubicVolume > this.HarvesterQuadraticThreshold)
                    {
                        float volumeBeyondThreshold = treeMerchantableCubicVolume - this.HarvesterQuadraticThreshold;
                        treeFellingTime += this.HarvesterFellingQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                    }

                    float expansionFactor = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    harvesterFellingTime += expansionFactor * treeFellingTime;
                }
            }

            // for now, assume uniform slope across stand
            if (stand.SlopeInPercent > this.HarvesterSlopeThresholdInPercent)
            {
                float harvesterSlopeMultiplier = 1.0F + 0.01F * (stand.SlopeInPercent - this.HarvesterSlopeThresholdInPercent);
                harvesterFellingTime *= harvesterSlopeMultiplier;
            }

            float harvesterPMhPerHectare = harvesterFellingTime / 3600.0F;
            float harvesterCostPerHectare = this.HarvesterPMh * harvesterPMhPerHectare;
            return harvesterCostPerHectare;
        }
    }
}
