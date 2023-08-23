using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class Forwarder
    {
        public bool AnchorMachine { get; set; }
        public float ForwardedWeightPerHa { get; set; } // kg/ha
        public float ForwarderCostPerHa { get; set; } // US$/ha
        public float ForwarderCostPerSMh { get; set; } // US$/SMh
        public float ForwarderPMhPerHa { get; set; } // accumulated in delay free minutes/ha and then converted to PMh₀/ha
        public float ForwarderProductivity { get; set; } // m³/PMh₀
        public ForwarderLoadingMethod LoadingMethod { get; set; }

        public Forwarder() 
        {
            this.Clear();
        }

        public void AddTree(float treeWeightInKgPerHa)
        {
            this.ForwardedWeightPerHa += treeWeightInKgPerHa;
        }

        public void CalculatePMhAndProductivity(Stand stand, StandTrajectory trajectory, int harvestPeriod, HarvestSystems harvestSystems)
        {
            this.ForwarderCostPerSMh = harvestSystems.ForwarderCostPerSMh;
            if ((stand.SlopeInPercent > Constant.Default.SlopeForTetheringInPercent) && (stand.CorridorLengthInMTethered > harvestSystems.AddOnWinchCableLengthInM))
            {
                this.AnchorMachine = true;
                this.ForwarderCostPerSMh += harvestSystems.AnchorCostPerSMh;
            }

            // find payload available for slope from traction
            float forwarderPayloadInKg = Forwarder.GetForwarderPayloadInKg(stand, harvestSystems);
            if (forwarderPayloadInKg <= 0.0F)
            {
                throw new NotSupportedException("Either stand slope of " + stand.SlopeInPercent + "% or access slope of " + stand.AccessSlopeInPercent + "% is too steep for forwarding.");
            }

            // TODO: full bark retention on trees bucked by chainsaw (for now it's assumed all trees are bucked by a harvester)
            // TODO: support cross-species loading
            // TODO: merchantable fraction of actual log length instead of assuming all logs are of preferred length
            foreach (TreeSpeciesMerchantableVolume harvestVolumeForSpecies in trajectory.ForwardedVolumeBySpecies.Values)
            {
                float cubic4Saw = harvestVolumeForSpecies.Cubic4Saw[harvestPeriod];
                float logs4Saw = harvestVolumeForSpecies.Logs4Saw[harvestPeriod];
                float cubic3Saw = harvestVolumeForSpecies.Cubic3Saw[harvestPeriod];
                float logs3Saw = harvestVolumeForSpecies.Logs3Saw[harvestPeriod];
                float cubic2Saw = harvestVolumeForSpecies.Cubic2Saw[harvestPeriod];
                float logs2Saw = harvestVolumeForSpecies.Logs2Saw[harvestPeriod];
                float speciesMerchM3PerHa = cubic2Saw + cubic3Saw + cubic4Saw; // m³/ha
                
                int sortsPresent = (cubic2Saw > 0.0F ? 1 : 0) + (cubic3Saw > 0.0F ? 1 : 0) + (cubic4Saw > 0.0F ? 1 : 0);
                if (sortsPresent == 0)
                {
                    continue; // no harvest of species, so no forwarding of species
                }

                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[harvestVolumeForSpecies.Species];
                float forwarderMaximumMerchantableM3 = forwarderPayloadInKg / (treeSpeciesProperties.StemDensityAfterHarvester * (1.0F + treeSpeciesProperties.BarkFractionAfterHarvester)); // * merchantableFractionOfLogLength; // merchantable m³ = kg / kg/m³ * 1 / merchantable m³/m³ [* merchantable m/m = 1 in BC Firmwood]

                // logs are forwarded for at least one sort; find default productivity
                ForwarderTurn turnAllSortsCombined = Forwarder.GetForwarderTurn(stand, harvestSystems, speciesMerchM3PerHa, logs2Saw + logs3Saw + logs4Saw, forwarderMaximumMerchantableM3, sortsPresent);
                this.ForwarderProductivity = Constant.MinutesPerHour * turnAllSortsCombined.Volume / turnAllSortsCombined.Time; // m³/PMh₀
                this.LoadingMethod = ForwarderLoadingMethod.AllSortsCombined;

                if (sortsPresent > 1)
                {
                    // if multiple sorts are present they can be forwarded separately rather than jointly
                    // Four possible combinations: 2S+3S, 2S+4S, 3S+4S, 2S+3S+4S, typically all three or 2S+4S. Turn times are calculated
                    // for each sort and then added to find the total forwarding time per corridor as this approach is robust against sorts
                    // with low volumes. Calculating a volume weighted mean of productivities is not appropriate here as the forwarder must
                    // presumably still travel the full length of the corridor to pick up all logs in low volume sorts.
                    ForwarderTurn turn2S = Forwarder.GetForwarderTurn(stand, harvestSystems, cubic2Saw, logs2Saw, forwarderMaximumMerchantableM3, 1);
                    ForwarderTurn turn3S = Forwarder.GetForwarderTurn(stand, harvestSystems, cubic3Saw, logs3Saw, forwarderMaximumMerchantableM3, 1);
                    ForwarderTurn turn4S = Forwarder.GetForwarderTurn(stand, harvestSystems, cubic4Saw, logs4Saw, forwarderMaximumMerchantableM3, 1);
                    float turnTimeAllSortsSeparate = turn2S.Time + turn3S.Time + turn4S.Time;
                    if (turnTimeAllSortsSeparate < turnAllSortsCombined.Time)
                    {
                        this.ForwarderProductivity = Constant.MinutesPerHour * turnAllSortsCombined.Volume / turnTimeAllSortsSeparate;
                        this.LoadingMethod = ForwarderLoadingMethod.AllSortsSeparate;
                    }

                    if (sortsPresent == 3)
                    {
                        // combining 2S and 4S and loading them separately from 3S is only meaningful if all three sorts exist
                        // This is an intermediate complexity option and is.
                        ForwarderTurn turn2S4S = Forwarder.GetForwarderTurn(stand, harvestSystems, cubic2Saw + cubic4Saw, logs2Saw + logs4Saw, forwarderMaximumMerchantableM3, 2);
                        float turnTime2S4SCombined = turn2S4S.Time + turn3S.Time;
                        if ((turnTime2S4SCombined < turnAllSortsCombined.Time) && (turnTime2S4SCombined < turnTimeAllSortsSeparate))
                        {
                            this.ForwarderProductivity = Constant.MinutesPerHour * turnAllSortsCombined.Volume / turnTime2S4SCombined;
                            this.LoadingMethod = ForwarderLoadingMethod.TwoFourSCombined;
                        }
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
                float forwarderPMhPerSpecies = speciesMerchM3PerHa / this.ForwarderProductivity;
                // float forwarderCost = treeVolumeClass / forwarderProductivity * forwarderHourlyCost, # $/tree

                this.ForwarderPMhPerHa += forwarderPMhPerSpecies;
            }

            this.ForwarderCostPerHa = this.ForwarderCostPerSMh * this.ForwarderPMhPerHa / harvestSystems.ForwarderUtilization;
            Debug.Assert((this.ForwarderCostPerHa > 0.0F) && (this.ForwarderPMhPerHa > 0.0F) && (this.ForwardedWeightPerHa > 0.0F));
        }

        public void Clear() 
        {
            this.AnchorMachine = false;
            this.ForwardedWeightPerHa = 0.0F;
            this.ForwarderCostPerHa = 0.0F;
            this.ForwarderCostPerSMh = 0.0F;
            this.ForwarderPMhPerHa = 0.0F;
            this.LoadingMethod = ForwarderLoadingMethod.None;
            this.ForwarderProductivity = Single.NaN;
        }

        private static float GetForwarderPayloadInKg(float slopeInPercent, HarvestSystems harvestSystems)
        {
            return MathF.Min(harvestSystems.ForwarderMaximumPayloadInKg, harvestSystems.ForwarderTractiveForce / (0.009807F * MathF.Sin(MathF.Atan(0.01F * slopeInPercent))) - harvestSystems.ForwarderEmptyWeight);
        }

        public static float GetForwarderPayloadInKg(Stand stand, HarvestSystems harvestSystems)
        {
            float forwarderPayloadInKg = MathF.Min(harvestSystems.ForwarderMaximumPayloadInKg, harvestSystems.ForwarderTractiveForce / (0.009807F * MathF.Sin(MathF.Atan(0.01F * stand.SlopeInPercent))) - harvestSystems.ForwarderEmptyWeight);
            if (stand.AccessDistanceInM > 0.0F)
            {
                float accessPayloadInKg = Forwarder.GetForwarderPayloadInKg(stand.AccessSlopeInPercent, harvestSystems);
                forwarderPayloadInKg = MathF.Min(forwarderPayloadInKg, accessPayloadInKg);
            }

            return forwarderPayloadInKg;
        }

        private static ForwarderTurn GetForwarderTurn(Stand stand, HarvestSystems harvestSystems, float merchM3PerHa, float logsPerHa, float forwarderMaxMerchM3, int sortsLoaded)
        {
            Debug.Assert((forwarderMaxMerchM3 > 0.0F) && (sortsLoaded > 0) && (sortsLoaded < 4));
            if (merchM3PerHa <= 0.0F)
            {
                Debug.Assert((merchM3PerHa == 0.0F) && (logsPerHa == 0.0F));
                return new();
            }

            bool isTetheredAccess = stand.AccessSlopeInPercent > Constant.Default.SlopeForTetheringInPercent;
            float accessSpeedLoaded = isTetheredAccess ? harvestSystems.ForwarderSpeedInStandLoadedTethered : harvestSystems.ForwarderSpeedInStandLoadedUntethered;
            float accessSpeedUnloaded = isTetheredAccess ? harvestSystems.ForwarderSpeedInStandUnloadedTethered : harvestSystems.ForwarderSpeedInStandUnloadedUntethered;
            float merchM3perM = merchM3PerHa * harvestSystems.CorridorWidth / Constant.SquareMetersPerHectare; //  merchantable m³ logs/m of corridor = m³/ha * (m²/m corridor) / m²/ha
            float meanLogMerchM3 = merchM3PerHa / logsPerHa; // merchantable m³/log = m³/ha / logs/ha
            float volumePerCorridor = stand.CorridorLengthInM * merchM3perM; // merchantable m³/corridor
            float turnsPerCorridor = volumePerCorridor / forwarderMaxMerchM3;
            float completeLoadsInCorridor = MathF.Floor(turnsPerCorridor);
            float fractionalLoadsInCorridor = turnsPerCorridor - completeLoadsInCorridor;
            float traversalsOfCorridor = turnsPerCorridor > 1.0F ? turnsPerCorridor : 1.0F; // forwarder must descend to the bottom of the corridor at least once
            float forwardingDistanceOnRoad = stand.ForwardingDistanceOnRoad + (sortsLoaded - 1) * Constant.HarvestCost.ForwardingDistanceOnRoadPerSortInM;

            // outbound part of turn: assumed to be descending from road
            float driveEmptyRoad = completeLoadsInCorridor * forwardingDistanceOnRoad / harvestSystems.ForwarderSpeedOnRoad; // min, driving empty on road
            float driveEmptyAccess = completeLoadsInCorridor * stand.AccessDistanceInM / accessSpeedUnloaded; // min, driving empty overland to get to stand
            // nonspatial approximation (level zero): both tethered and untethered distances decrease in turns after the first
            // TODO: assume tethered distance decreases to zero before untethered distance decreases?
            float driveEmptyUntethered = traversalsOfCorridor * stand.CorridorLengthInMUntethered / harvestSystems.ForwarderSpeedInStandUnloadedUntethered; // min
            // tethering time is treated as a delay
            float driveEmptyTethered = traversalsOfCorridor * stand.CorridorLengthInMTethered / harvestSystems.ForwarderSpeedInStandUnloadedTethered; // min
            float descent = driveEmptyAccess + driveEmptyUntethered + driveEmptyTethered;

            // inbound part of turn: assumed to be ascending towards road
            // Forwarder loading method selection will query for productivity at quite low log densities, resulting in the forwarder loading all the
            // way back to the top of the corridor. Since the form of the regressions doesn't guarantee the combination of loading and driving while
            // loading is greater than the time needed to drive the forwarder back to the top of the corridor, check for harvestSystems condition and impose
            // a minimum ascent time.
            float loading = completeLoadsInCorridor * MathF.Exp(-1.2460F + harvestSystems.ForwarderLoadPayload * MathF.Log(forwarderMaxMerchM3) - harvestSystems.ForwarderLoadMeanLogVolume * MathF.Log(meanLogMerchM3)) +
                            MathF.Exp(-1.2460F + harvestSystems.ForwarderLoadPayload * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3) - harvestSystems.ForwarderLoadMeanLogVolume * MathF.Log(meanLogMerchM3)); // min
            float driveWhileLoading = completeLoadsInCorridor * MathF.Exp(-2.5239F + harvestSystems.ForwarderDriveWhileLoadingLogs * MathF.Log(forwarderMaxMerchM3 / merchM3perM)) +
                                      MathF.Exp(-2.5239F + harvestSystems.ForwarderDriveWhileLoadingLogs * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3 / merchM3perM)); // min
            float driveLoadedTethered = MathF.Max(turnsPerCorridor - 1.0F, 0.0F) * stand.CorridorLengthInMTethered / harvestSystems.ForwarderSpeedInStandLoadedTethered; // min
            // untethering time is treated as a delay
            float driveLoadedUnethered = MathF.Max(turnsPerCorridor - 1.0F, 0.0F) * stand.CorridorLengthInMUntethered / harvestSystems.ForwarderSpeedInStandLoadedUntethered; // min
            float driveLoadedAccess = stand.AccessDistanceInM / accessSpeedLoaded;

            float minimumAscentTime = traversalsOfCorridor * (stand.CorridorLengthInMTethered / harvestSystems.ForwarderSpeedInStandLoadedTethered + stand.CorridorLengthInMUntethered / harvestSystems.ForwarderSpeedInStandLoadedUntethered);
            float ascent = MathF.Max(loading + driveWhileLoading + driveLoadedTethered + driveLoadedUnethered, minimumAscentTime) + driveLoadedAccess;

            float driveLoadedRoad = MathF.Ceiling(turnsPerCorridor) * forwardingDistanceOnRoad / harvestSystems.ForwarderSpeedOnRoad; // min

            // unloading
            // TODO: make unload complexity multiplier a function of the diversity of sorts present rather than simply richness
            float unloadLinear = sortsLoaded switch
            {
                1 => harvestSystems.ForwarderUnloadLinearOneSort,
                2 => harvestSystems.ForwarderUnloadLinearTwoSorts,
                3 => harvestSystems.ForwarderUnloadLinearThreeSorts,
                _ => throw new ArgumentOutOfRangeException(nameof(sortsLoaded))
            };
            float unloading = unloadLinear * (completeLoadsInCorridor * MathF.Exp(harvestSystems.ForwarderUnloadPayload * MathF.Log(forwarderMaxMerchM3) - harvestSystems.ForwarderUnloadMeanLogVolume * MathF.Log(meanLogMerchM3)) +
                                              MathF.Exp(harvestSystems.ForwarderUnloadPayload * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3) - harvestSystems.ForwarderUnloadMeanLogVolume * MathF.Log(meanLogMerchM3)));
            float turnTime = driveEmptyRoad + descent + ascent + driveLoadedRoad + unloading; // min
            return new(turnTime, volumePerCorridor);
        }

        private readonly struct ForwarderTurn
        {
            public float Time { get; init; } // minutes
            public float Volume { get; init; } // merchantable m³

            public ForwarderTurn(float timeInMinutes, float volumeInM3)
            {
                this.Time = timeInMinutes;
                this.Volume = volumeInM3;
            }
        }
    }
}
