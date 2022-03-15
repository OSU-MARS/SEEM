using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class HarvestSystems
    {
        public static HarvestSystems Default { get; private set; }

        public float AddOnWinchCableLengthInM { get; set; } // m
        public float AnchorCostPerSMh { get; set; } // US$/SMh

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

        public float MachineMoveInOrOut { get; set; } // US$ per piece of heavy equipment

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
            this.AddOnWinchCableLengthInM = 380.0F; // Herzog Synchrowinch, 14.5 mm cable
            this.AnchorCostPerSMh = 71.50F; // US$/SMh

            // chainsaw use cases
            this.ChainsawBuckConstant = 51.0F; // seconds
            this.ChainsawBuckCostPerSMh = 80.0F; // US$/SMh
            this.ChainsawBuckLinear = 54.0F; // seconds/m³
            this.ChainsawBuckUtilization = 0.75F; // fraction
            this.ChainsawBuckQuadratic = 30.0F; // seconds/m⁶
            this.ChainsawBuckQuadraticThreshold = 1.0F; // m³
            this.ChainsawByOperatorCostPerSMh = 2.17F; // US$/SMh
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
            this.FellerBuncherCostPerSMh = 192.00F; // US$/SMh
            this.FellerBuncherFellingConstant = 18.0F; // seconds
            this.FellerBuncherFellingLinear = 4.7F; // seconds/m³
            this.FellerBuncherSlopeLinear = 0.0115F; // multiplier
            this.FellerBuncherSlopeThresholdInPercent = 30.0F;
            this.FellerBuncherUtilization = 0.77F; // fraction

            // forwarder default: Ponsse Elephant King
            this.ForwarderCostPerSMh = 200.00F; // US$/SMh
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
            this.GrappleSwingYarderCostPerSMh = 360.00F; // US$/SMh
            this.GrappleSwingYarderUtilization = 0.80F;
            this.GrappleYoaderMaxPayload = 2900.0F; // kg
            this.GrappleYoaderMeanPayload = 1550.0F; // kg
            this.GrappleYoaderCostPerSMh = 248.00F; // US$/SMh
            this.GrappleYoaderUtilization = 0.75F;

            // nominal loader at landing
            this.LoaderCostPerSMh = 172.0F; // US$/SMh
            this.LoaderProductivity = 2.0F * 0.99F * 26275.0F; // kg/PMh₀, two six axle long log truckloads per hour
            this.LoaderUtilization = 0.90F;

            // long log truck
            this.LongLogHaulPayloadInKg = 0.99F * 26275.0F; // kg, 6 axle long log truck assumed
            this.LongLogHaulPerSMh = 83.72F; // US$/m³
            this.LongLogHaulRoundtripSMh = 3.58F; // SMh

            // lowboy roundtrip plus load and unload
            this.MachineMoveInOrOut = 2.0F * 10.0F + 3.0F * 170.0F; // US$/lowboy trip ≈ US$/machine = load + unload + roundtrip travel time * lowboy $/PMh

            // nominal processor at landing
            this.ProcessorBuckConstant = 21.0F; // seconds
            this.ProcessorBuckLinear = 30.0F; // seconds/m³
            this.ProcessorBuckQuadratic1 = 1.5F; // seconds/m⁶
            this.ProcessorBuckQuadratic2 = 4.5F; // seconds/m⁶
            this.ProcessorBuckQuadraticThreshold1 = 2.5F; // m³
            this.ProcessorBuckQuadraticThreshold2 = 6.0F; // m³
            this.ProcessorCostPerSMh = 204.00F; // US$/SMh
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
            this.TrackedHarvesterCostPerSMh = 193.00F; // US$/SMh
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
            this.WheeledHarvesterCostPerSMh = 225.00F; // US$/SMh
            this.WheeledHarvesterFellAndBuckConstant = 28.0F; // seconds
            this.WheeledHarvesterFellAndBuckDiameterLimit = 70.0F; // cm, felling diameter and feed roller limit of H8 head
            this.WheeledHarvesterFellAndBuckLinear = 43.0F; // seconds/m³
            this.WheeledHarvesterFellAndBuckQuadratic = 6.0F; // seconds/m⁶
            this.WheeledHarvesterQuadraticThreshold = 1.9F; // m³
            this.WheeledHarvesterSlopeLinear = 0.0100F; // multiplier
            this.WheeledHarvesterSlopeThresholdInPercent = 45.0F;
            this.WheeledHarvesterUtilization = 0.77F; // fraction
        }

        public void GetCutToLengthHarvestCost(StandTrajectory trajectory, int harvestPeriod, bool isThin, CutToLengthHarvest ctlHarvest)
        {
            if ((ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester != 0.0F) ||
                (ctlHarvest.ForwardedWeightPeHa != 0.0F) ||
                (ctlHarvest.ForwarderPMhPerHa != 0.0F) ||
                (ctlHarvest.MerchantableCubicVolumePerHa != 0.0F) ||
                (ctlHarvest.TaskCostPerHa != 0.0F) ||
                (ctlHarvest.WheeledHarvesterPMhPerHa != 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(ctlHarvest));
            }
            Stand? stand = trajectory.StandByPeriod[harvestPeriod - (isThin ? 1 : 0)];
            if ((stand == null) ||
                (stand.AreaInHa <= 0.0F) ||
                (stand.CorridorLengthInM <= 0.0F) ||
                (stand.CorridorLengthInMTethered < 0.0F) ||
                (stand.CorridorLengthInMTethered > Constant.Maximum.TetheredCorridorLengthInM) ||
                (stand.CorridorLengthInMUntethered < 0.0F) ||
                (stand.ForwardingDistanceOnRoad < 0.0F) ||
                (stand.SlopeInPercent < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(trajectory));
            }

            int anchorMachines = 0;
            float forwarderAndMaybeAnchorCostPerSMh = this.ForwarderCostPerSMh;
            float wheeledHarvesterAndMaybeAnchorCostPerSMh = this.WheeledHarvesterCostPerSMh;
            if ((stand.SlopeInPercent > Constant.Default.SlopeForTetheringInPercent) && (stand.CorridorLengthInMTethered > this.AddOnWinchCableLengthInM))
            {
                forwarderAndMaybeAnchorCostPerSMh += this.AnchorCostPerSMh;
                wheeledHarvesterAndMaybeAnchorCostPerSMh += this.AnchorCostPerSMh;
                anchorMachines = 2;
            }

            ScaledVolume scaledVolume = isThin ? trajectory.TreeVolume.Thinning : trajectory.TreeVolume.RegenerationHarvest;
            // BC Firmwood scaling considers trim to be merchantable cubic volume (Fonseca 2005 §2.2.2.2) so, for now, merchantableFractionOfLogLength = 1
            // float preferredLogLength = scaledVolume.PreferredLogLengthInMeters;
            // float preferredTrimLength = scaledVolume.GetPreferredTrim();
            // float merchantableFractionOfLogLength = preferredLogLength / (preferredLogLength + preferredTrimLength); // assumes cylindrical logs or cylindrical ones with equal amounts of trim at both ends

            bool chainsawFallingWithWheeledHarvester = false;
            bool previousOversizeTreeBehindHarvester = true;
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
                IndividualTreeSelection individualTreeSelection = trajectory.TreeSelectionBySpecies[treesOfSpecies.Species];
                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treesOfSpecies.Species];
                TreeSpeciesVolumeTable volumeTable = scaledVolume.VolumeBySpecies[treesOfSpecies.Species];

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
                    float heightInM = heightToMetersMultiplier * treesOfSpecies.Height[compactedTreeIndex];
                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeMerchantableVolumeInM3 = volumeTable.GetCubicVolumeOfWood(dbhInCm, heightInM, out float unscaledNeiloidVolumeInM3);
                    Debug.Assert(Single.IsNaN(treeMerchantableVolumeInM3) == false);
                    ctlHarvest.MerchantableCubicVolumePerHa += expansionFactorPerHa * treeMerchantableVolumeInM3;

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
                        ctlHarvest.ForwardedWeightPeHa += expansionFactorPerHa * woodAndRemainingBarkVolumePerStem * treeSpeciesProperties.StemDensityAfterHarvester; // / merchantableFractionOfLogLength; // 1 in BC Firmwood
                        ctlHarvest.WheeledHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }
                    else
                    {
                        // a chainsaw is used on this tree, so it applies towards chainsaw utilization
                        // For now, assume all CTL harvests are thins and that the harvester operates in two scenarios occuring with equal
                        // probability.
                        //
                        //   1) A tree requiring chainsaw work is encountered behind the harvester's direction of progress across the stand. In
                        //      this case falling the tree away from the harvester does not impede the harvester in subsequent corridors and it's
                        //      unimportant whether the harvester operator or a separate chainsaw crew bucks the tree.
                        //   2) The tree is ahead of harvester, in which case falling it away from the harvester is unlikely to affect operations
                        //      in the current corridor but does block subsequent corridors. In this case either the operator has to buck the 
                        //      tree and move the logs out of the way with the harvester's processing head or leave the tree standing for falling
                        //      once the harvester has passed, either by the operator from the next corridor or independently by a falling crew.
                        //
                        // Since modelling is currently nonspatial, occurence of these two cases is approximated by a toggle forcing every other
                        // tree too large for the processing head to be manually felled. In a spatial model whether the tree needs to be manually
                        // felled would be determined by its position with respect to the harvester and whether the cutting pattern allows it to
                        // be felled with the processing head. In spatial modelling edge effects, such as needing to fell trees into a unit rather
                        // than across a property line or onto smaller trees in an adjacent stand, can also be considered.
                        ctlHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;

                        float treeChainsawTime; // seconds
                        float treeHarvesterTime; // seconds
                        if ((dbhInCm > this.WheeledHarvesterFellingDiameterLimit) || previousOversizeTreeBehindHarvester)
                        {
                            chainsawFallingWithWheeledHarvester = true;
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

                        float woodAndBarkVolumePerStem = (treeMerchantableVolumeInM3 + unscaledNeiloidVolumeInM3) / (1.0F - treeSpeciesProperties.BarkFraction); // no bark loss from going through feed rollers
                        ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester += expansionFactorPerHa * treeChainsawTime;
                        ctlHarvest.ChainsawCubicVolumePerHaWithWheeledHarvester += expansionFactorPerHa * treeMerchantableVolumeInM3;
                        ctlHarvest.ForwardedWeightPeHa += expansionFactorPerHa * woodAndBarkVolumePerStem * treeSpeciesProperties.StemDensity; // / merchantableFractionOfLogLength; // 1 in BC Firmwood 
                        ctlHarvest.WheeledHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                        previousOversizeTreeBehindHarvester = !previousOversizeTreeBehindHarvester;
                    }
                }
            }

            // for now, assume uniform slope across stand
            if (stand.SlopeInPercent > this.WheeledHarvesterSlopeThresholdInPercent)
            {
                ctlHarvest.WheeledHarvesterPMhPerHa *= 1.0F + this.WheeledHarvesterSlopeLinear * (stand.SlopeInPercent - this.WheeledHarvesterSlopeThresholdInPercent);
            }
            ctlHarvest.WheeledHarvesterPMhPerHa /= Constant.SecondsPerHour;
            ctlHarvest.Productivity.WheeledHarvester = ctlHarvest.MerchantableCubicVolumePerHa / ctlHarvest.WheeledHarvesterPMhPerHa;

            float minimumChainsawCostWithWheeledHarvester = 0.0F;
            if (ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester > 0.0F)
            {
                if (stand.SlopeInPercent > this.ChainsawSlopeThresholdInPercent)
                {
                    ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester *= 1.0F + this.ChainsawSlopeLinear * (stand.SlopeInPercent - this.ChainsawSlopeThresholdInPercent);
                }
                ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester /= Constant.SecondsPerHour;

                float chainsawByOperatorSMh = ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester / this.ChainsawByOperatorUtilization; // SMh
                float chainsawByOperatorCost = (wheeledHarvesterAndMaybeAnchorCostPerSMh + this.ChainsawByOperatorCostPerSMh) * chainsawByOperatorSMh;
                float chainsawCrewCost;
                float chainsawCrewUtilization;
                if (chainsawFallingWithWheeledHarvester)
                {
                    chainsawCrewCost = this.ChainsawFellAndBuckCostPerSMh;
                    chainsawCrewUtilization = this.ChainsawFellAndBuckUtilization;
                }
                else
                {
                    chainsawCrewCost = this.ChainsawBuckCostPerSMh;
                    chainsawCrewUtilization = this.ChainsawBuckUtilization;
                }
                chainsawCrewUtilization *= MathF.Min(ctlHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                chainsawCrewCost *= ctlHarvest.ChainsawPMhPerHaWithWheeledHarvester / chainsawCrewUtilization;

                if (chainsawByOperatorCost < chainsawCrewCost)
                {
                    minimumChainsawCostWithWheeledHarvester = chainsawByOperatorCost;
                    ctlHarvest.ChainsawCrewWithWheeledHarvester = ChainsawCrewType.Operator;
                    ctlHarvest.Productivity.ChainsawUtilizationWithWheeledHarvester = this.ChainsawByOperatorUtilization;
                }
                else
                {
                    minimumChainsawCostWithWheeledHarvester = chainsawCrewCost;
                    ctlHarvest.ChainsawCrewWithWheeledHarvester = chainsawFallingWithWheeledHarvester ? ChainsawCrewType.Fallers : ChainsawCrewType.Bucker;
                    ctlHarvest.Productivity.ChainsawUtilizationWithWheeledHarvester = chainsawCrewUtilization;
                }

                Debug.Assert(ctlHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester > 0.0F);
            }

            float wheeledHarvesterSMhPerHectare = ctlHarvest.WheeledHarvesterPMhPerHa / this.WheeledHarvesterUtilization;
            ctlHarvest.WheeledHarvesterCostPerHa = wheeledHarvesterAndMaybeAnchorCostPerSMh * wheeledHarvesterSMhPerHectare;

            // find payload available for slope from traction
            float forwarderPayloadInKg = MathF.Min(this.ForwarderMaximumPayloadInKg, this.ForwarderTractiveForce / (0.009807F * MathF.Sin(MathF.Atan(0.01F * stand.SlopeInPercent))) - this.ForwarderEmptyWeight);
            if (forwarderPayloadInKg <= 0.0F)
            {
                throw new NotSupportedException("Stand slope of " + stand.SlopeInPercent + "% is too steep for forwarding.");
            }

            // TODO: full bark retention on trees bucked by chainsaw (for now it's assumed all trees are bucked by a harvester)
            // TODO: support cross-species loading
            // TODO: merchantable fraction of actual log length instead of assuming all logs are of preferred length
            SortedList<FiaCode, TreeSpeciesMerchantableVolume> harvestVolumeBySpecies = isThin ? trajectory.ThinningVolumeBySpecies : trajectory.StandingVolumeBySpecies;
            foreach (TreeSpeciesMerchantableVolume harvestVolumeForSpecies in harvestVolumeBySpecies.Values)
            {
                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[harvestVolumeForSpecies.Species];
                float forwarderMaximumMerchantableM3 = forwarderPayloadInKg / (treeSpeciesProperties.StemDensityAfterHarvester * (1.0F + treeSpeciesProperties.BarkFractionAfterHarvester)); // * merchantableFractionOfLogLength; // merchantable m³ = kg / kg/m³ * 1 / merchantable m³/m³ [* merchantable m/m = 1 in BC Firmwood]

                float cubic4Saw = harvestVolumeForSpecies.Cubic4Saw[harvestPeriod];
                float logs4Saw = harvestVolumeForSpecies.Logs4Saw[harvestPeriod];
                float cubic3Saw = harvestVolumeForSpecies.Cubic3Saw[harvestPeriod];
                float logs3Saw = harvestVolumeForSpecies.Logs3Saw[harvestPeriod];
                float cubic2Saw = harvestVolumeForSpecies.Cubic2Saw[harvestPeriod];
                float logs2Saw = harvestVolumeForSpecies.Logs2Saw[harvestPeriod];
                float speciesMerchM3PerHa = cubic2Saw + cubic3Saw + cubic4Saw; // m³/ha
                int sortsPresent = (cubic2Saw > 0.0F ? 1 : 0) + (cubic3Saw > 0.0F ? 1 : 0) + (cubic4Saw > 0.0F ? 1 : 0);

                if (sortsPresent > 0)
                {
                    // logs are forwarded for at least one sort; find default productivity
                    ForwarderTurn turnAllSortsCombined = this.GetForwarderTurn(stand, speciesMerchM3PerHa, logs2Saw + logs3Saw + logs4Saw, forwarderMaximumMerchantableM3, sortsPresent);
                    ctlHarvest.Productivity.Forwarder = Constant.MinutesPerHour * turnAllSortsCombined.Volume / turnAllSortsCombined.Time; // m³/PMh₀
                    ctlHarvest.Productivity.ForwardingMethod = ForwarderLoadingMethod.AllSortsCombined;

                    if (sortsPresent > 1)
                    {
                        // if multiple sorts are present they can be forwarded separately rather than jointly
                        // Four possible combinations: 2S+3S, 2S+4S, 3S+4S, 2S+3S+4S, typically all three or 2S+4S. Turn times are calculated
                        // for each sort and then added to find the total forwarding time per corridor as this approach is robust against sorts
                        // with low volumes. Calculating a volume weighted mean of productivities is not appropriate here as the forwarder must
                        // presumably still travel the full length of the corridor to pick up all logs in low volume sorts.
                        ForwarderTurn turn2S = this.GetForwarderTurn(stand, cubic2Saw, logs2Saw, forwarderMaximumMerchantableM3, 1);
                        ForwarderTurn turn3S = this.GetForwarderTurn(stand, cubic3Saw, logs3Saw, forwarderMaximumMerchantableM3, 1);
                        ForwarderTurn turn4S = this.GetForwarderTurn(stand, cubic4Saw, logs4Saw, forwarderMaximumMerchantableM3, 1);
                        float turnTimeAllSortsSeparate = turn2S.Time + turn3S.Time + turn4S.Time;
                        if (turnTimeAllSortsSeparate < turnAllSortsCombined.Time)
                        {
                            ctlHarvest.Productivity.Forwarder = Constant.MinutesPerHour * turnAllSortsCombined.Volume / turnTimeAllSortsSeparate;
                            ctlHarvest.Productivity.ForwardingMethod = ForwarderLoadingMethod.AllSortsSeparate;
                        }

                        if (sortsPresent == 3)
                        {
                            // combining 2S and 4S and loading them separately from 3S is only meaningful if all three sorts exist
                            // This is an intermediate complexity option and is.
                            ForwarderTurn turn2S4S = this.GetForwarderTurn(stand, cubic2Saw + cubic4Saw, logs2Saw + logs4Saw, forwarderMaximumMerchantableM3, 2);
                            float turnTime2S4SCombined = turn2S4S.Time + turn3S.Time;
                            if ((turnTime2S4SCombined < turnAllSortsCombined.Time) && (turnTime2S4SCombined < turnTimeAllSortsSeparate))
                            {
                                ctlHarvest.Productivity.Forwarder = Constant.MinutesPerHour * turnAllSortsCombined.Volume / turnTime2S4SCombined;
                                ctlHarvest.Productivity.ForwardingMethod = ForwarderLoadingMethod.TwoFourSCombined;
                            }
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
                float forwarderPMhPerSpecies = speciesMerchM3PerHa / ctlHarvest.Productivity.Forwarder;
                // float forwarderCost = treeVolumeClass / forwarderProductivity * forwarderHourlyCost, # $/tree

                ctlHarvest.ForwarderPMhPerHa += forwarderPMhPerSpecies;
            }

            ctlHarvest.ForwarderCostPerHa = forwarderAndMaybeAnchorCostPerSMh * ctlHarvest.ForwarderPMhPerHa / this.ForwarderUtilization;

            if (ctlHarvest.CubicVolumePerHa > 0.0F)
            {
                int machinesToMoveInAndOut = 3 + anchorMachines; // bulldozer + harvester + forwarder + anchors
                float machineMoveInAndOutPerHa = 2.0F * machinesToMoveInAndOut * this.MachineMoveInOrOut / stand.AreaInHa; // 2 = move in + move out
                float haulRoundtripsPerHectare = ctlHarvest.ForwardedWeightPeHa / this.CutToLengthHaulPayloadInKg;
                float haulCostPerHectare = this.CutToLengthRoundtripHaulSMh * haulRoundtripsPerHectare * this.CutToLengthHaulPerSMh;
                ctlHarvest.MinimumSystemCostPerHa = ctlHarvest.WheeledHarvesterCostPerHa + minimumChainsawCostWithWheeledHarvester + ctlHarvest.ForwarderCostPerHa + haulCostPerHectare + machineMoveInAndOutPerHa;
                Debug.Assert((ctlHarvest.WheeledHarvesterCostPerHa >= 0.0F) && (minimumChainsawCostWithWheeledHarvester >= 0.0F) && 
                             (ctlHarvest.ForwarderPMhPerHa > 0.0F) && (ctlHarvest.ForwardedWeightPeHa > 0.0F) && 
                             (haulCostPerHectare > 0.0F) && (machineMoveInAndOutPerHa > 0.0F) &&
                             (Single.IsNaN(ctlHarvest.MinimumSystemCostPerHa) == false) && (ctlHarvest.MinimumSystemCostPerHa > 0.0F));
            }
        }

        private ForwarderTurn GetForwarderTurn(Stand stand, float merchM3PerHa, float logsPerHa, float forwarderMaxMerchM3, int sortsLoaded)
        {
            Debug.Assert((forwarderMaxMerchM3 > 0.0F) && (sortsLoaded > 0) && (sortsLoaded < 4));
            if (merchM3PerHa <= 0.0F)
            {
                Debug.Assert((merchM3PerHa == 0.0F) && (logsPerHa == 0.0F));
                return new();
            }

            float merchM3perM = merchM3PerHa * this.CorridorWidth / Constant.SquareMetersPerHectare; //  merchantable m³ logs/m of corridor = m³/ha * (m²/m corridor) / m²/ha
            float meanLogMerchM3 = merchM3PerHa / logsPerHa; // merchantable m³/log = m³/ha / logs/ha
            float volumePerCorridor = stand.CorridorLengthInM * merchM3perM; // merchantable m³/corridor
            float turnsPerCorridor = volumePerCorridor / forwarderMaxMerchM3;
            float completeLoadsInCorridor = MathF.Floor(turnsPerCorridor);
            float fractionalLoadsInCorridor = turnsPerCorridor - completeLoadsInCorridor;
            float traversalsOfCorridor = turnsPerCorridor > 1.0F ? turnsPerCorridor : 1.0F; // forwarder must descend to the bottom of the corridor at least once
            float forwardingDistanceOnRoad = stand.ForwardingDistanceOnRoad + (sortsLoaded - 1) * Constant.HarvestCost.ForwardingDistanceOnRoadPerSortInM;

            // outbound part of turn: assumed to be descending from road
            float driveEmptyRoad = MathF.Ceiling(turnsPerCorridor) * forwardingDistanceOnRoad / this.ForwarderSpeedOnRoad; // min, driving empty on road
            // nonspatial approximation (level zero): both tethered and untethered distances decrease in turns after the first
            // TODO: assume tethered distance decreases to zero before untethered distance decreases?
            float driveEmptyUntethered = traversalsOfCorridor * stand.CorridorLengthInMUntethered / this.ForwarderSpeedInStandUnloadedUntethered; // min
            // tethering time is treated as a delay
            float driveEmptyTethered = traversalsOfCorridor * stand.CorridorLengthInMTethered / this.ForwarderSpeedInStandUnloadedTethered; // min
            float descent = driveEmptyUntethered + driveEmptyTethered;

            // inbound part of turn: assumed to be ascending towards road
            // Forwarder loading method selection will query for productivity at quite low log densities, resulting in the forwarder loading all the
            // way back to the top of the corridor. Since the form of the regressions doesn't guarantee the combination of loading and driving while
            // loading is greater than the time needed to drive the forwarder back to the top of the corridor, check for this condition and impose
            // a minimum ascent time.
            float loading = completeLoadsInCorridor * MathF.Exp(-1.2460F + this.ForwarderLoadPayload * MathF.Log(forwarderMaxMerchM3) - this.ForwarderLoadMeanLogVolume * MathF.Log(meanLogMerchM3)) +
                            MathF.Exp(-1.2460F + this.ForwarderLoadPayload * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3) - this.ForwarderLoadMeanLogVolume * MathF.Log(meanLogMerchM3)); // min
            float driveWhileLoading = completeLoadsInCorridor * MathF.Exp(-2.5239F + this.ForwarderDriveWhileLoadingLogs * MathF.Log(forwarderMaxMerchM3 / merchM3perM)) +
                                      MathF.Exp(-2.5239F + this.ForwarderDriveWhileLoadingLogs * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3 / merchM3perM)); // min
            float driveLoadedTethered = MathF.Max(turnsPerCorridor - 1.0F, 0.0F) * stand.CorridorLengthInMTethered / this.ForwarderSpeedInStandLoadedTethered; // min
            // untethering time is treated as a delay
            float driveUnloadedTethered = MathF.Max(turnsPerCorridor - 1.0F, 0.0F) * stand.CorridorLengthInMUntethered / this.ForwarderSpeedInStandLoadedUntethered; // min

            float minimumAscentTime = traversalsOfCorridor * (stand.CorridorLengthInMTethered / this.ForwarderSpeedInStandLoadedTethered + stand.CorridorLengthInMUntethered / this.ForwarderSpeedInStandLoadedUntethered);
            float ascent = MathF.Max(loading + driveWhileLoading + driveLoadedTethered + driveUnloadedTethered, minimumAscentTime);

            float driveLoadedRoad = MathF.Ceiling(turnsPerCorridor) * forwardingDistanceOnRoad / this.ForwarderSpeedOnRoad; // min

            // unloading
            // TODO: make unload complexity multiplier a function of the diversity of sorts present rather than simply richness
            float unloadLinear = sortsLoaded switch
            {
                1 => this.ForwarderUnloadLinearOneSort,
                2 => this.ForwarderUnloadLinearTwoSorts,
                3 => this.ForwarderUnloadLinearThreeSorts,
                _ => throw new ArgumentOutOfRangeException(nameof(sortsLoaded))
            };
            float unloading = unloadLinear * (completeLoadsInCorridor * MathF.Exp(this.ForwarderUnloadPayload * MathF.Log(forwarderMaxMerchM3) - this.ForwarderUnloadMeanLogVolume * MathF.Log(meanLogMerchM3)) +
                                              MathF.Exp(this.ForwarderUnloadPayload * MathF.Log(fractionalLoadsInCorridor * forwarderMaxMerchM3) - this.ForwarderUnloadMeanLogVolume * MathF.Log(meanLogMerchM3)));
            float turnTime = driveEmptyRoad + descent + ascent + driveLoadedRoad + unloading; // min
            return new(turnTime, volumePerCorridor);
        }

        public void GetLongLogHarvestCosts(Stand stand, ScaledVolume scaledVolume, LongLogHarvest longLogHarvest)
        {
            if ((stand.AreaInHa <= 0.0F) || (stand.CorridorLengthInM <= 0.0F) || (stand.SlopeInPercent < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(stand));
            }
            if ((longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder != 0.0F) ||
                (longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader != 0.0F) ||
                (longLogHarvest.ChainsawBasalAreaPerHaWithTrackedHarvester != 0.0F) ||
                (longLogHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester != 0.0F) ||
                (longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder != 0.0F) ||
                (longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader != 0.0F) ||
                (longLogHarvest.ChainsawPMhPerHaWithTrackedHarvester != 0.0F) ||
                (longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester != 0.0F) ||
                (longLogHarvest.GrappleSwingYarderPMhPerHectare != 0.0F) ||
                (longLogHarvest.GrappleYoaderPMhPerHectare != 0.0F) ||
                (longLogHarvest.ProcessorPMhPerHaWithGrappleSwingYarder != 0.0F) ||
                (longLogHarvest.ProcessorPMhPerHaWithGrappleYoader != 0.0F) ||
                (longLogHarvest.MerchantableCubicVolumePerHa != 0.0F) ||
                (longLogHarvest.LoadedWeightPerHa != 0.0F) ||
                (longLogHarvest.TrackedHarvesterPMhPerHa != 0.0F) ||
                (longLogHarvest.WheeledHarvesterPMhPerHa != 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(longLogHarvest));
            }

            int fellerBuncherOrTrackedHarvesterAnchorMachine = 0;
            int wheeledHarvesterAnchorMachine = 0;
            float fellerBuncherCostPerSMh = this.FellerBuncherCostPerSMh;
            float trackedHarvesterCostPerSMh = this.TrackedHarvesterCostPerSMh;
            float wheeledHarvesterCostPerSMh = this.WheeledHarvesterCostPerSMh; // add on winch
            if (stand.SlopeInPercent > Constant.Default.SlopeForTetheringInPercent)
            {
                fellerBuncherCostPerSMh += this.AnchorCostPerSMh;
                trackedHarvesterCostPerSMh += this.AnchorCostPerSMh;
                fellerBuncherOrTrackedHarvesterAnchorMachine = 1;

                if (stand.CorridorLengthInM > this.AddOnWinchCableLengthInM)
                {
                    wheeledHarvesterCostPerSMh += this.AnchorCostPerSMh;
                    wheeledHarvesterAnchorMachine = 1;
                }
            }

            bool chainsawFallingWithTrackedHarvester = false;
            bool chainsawFallingWithWheeledHarvester = false;
            // float preferredLogLengthWithTrimInM = scaledVolume.PreferredLogLengthInMeters + scaledVolume.GetPreferredTrim(); // for checking first log weight
            // float merchantableFractionOfLogLength = scaledVolume.PreferredLogLengthInMeters / preferredLogLengthWithTrimInM; // 1 in BC Firmwood
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
                TreeSpeciesVolumeTable volumeTable = scaledVolume.VolumeBySpecies[treesOfSpecies.Species];

                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treesOfSpecies.Species];
                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    float dbhInCm = diameterToCentimetersMultiplier * treesOfSpecies.Dbh[compactedTreeIndex];
                    float heightInM = heightToMetersMultiplier * treesOfSpecies.Height[compactedTreeIndex];
                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeMerchantableVolumeInM3 = volumeTable.GetCubicVolumeOfWood(dbhInCm, heightInM, out float unscaledNeiloidVolume);
                    Debug.Assert(Single.IsNaN(treeMerchantableVolumeInM3) == false);
                    longLogHarvest.MerchantableCubicVolumePerHa += expansionFactorPerHa * treeMerchantableVolumeInM3;

                    // feller-buncher with hot saw or directional felling head
                    // For now, assume directional felling head where needed and no diameter limit to directional felling.
                    float treeFellerBuncherTime = this.FellerBuncherFellingConstant + this.FellerBuncherFellingLinear * treeMerchantableVolumeInM3;
                    longLogHarvest.FellerBuncherPMhPerHa += expansionFactorPerHa * treeFellerBuncherTime;

                    // yarded weight
                    // Since no yarder productivity study appears to consider bark loss it's generally unclear where in the turn reported
                    // yarding weights occur. In lieu of more specific data, yarding weight limits are checked when half the yarding bark
                    // loss has occurred and this same weight is used in finding the total yarded weight for calculating the total number
                    // of yarding turns.
                    // TODO: include weight of remaining crown after branch breaking during felling and swinging
                    float woodAndRemainingBarkVolumePerStemAfterFelling = treeMerchantableVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFractionAtMidspanWithoutHarvester);
                    float yardedWeightPerStemWithFellerBuncher = woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
                    longLogHarvest.YardedWeightPerHaWithFellerBuncher += expansionFactorPerHa * yardedWeightPerStemWithFellerBuncher; // before bark loss since 

                    // chainsaw bucking of trees felled with a feller buncher but too heavy to be yarded as whole stems
                    // For now,
                    //   1) Assume the feller-buncher has a directional felling head if trees are too large for a hot saw and that
                    //      no trees are too large for a directional felling head.
                    //   2) Neglect effects of short log production when trees are large enough full size logs still exceed the
                    //      yoader weight limit.
                    //   3) The tree's first log is calculated from a Smalian estimate diameter outside bark. A more complete correct
                    //      would look up the tree's first log's merchantable volume from the volume table and adjust for the bark
                    //      fraction.
                    float firstLogVolumeInM3 = volumeTable.GetCubicVolumeOfWoodInFirstLog(dbhInCm, heightInM) + unscaledNeiloidVolume;
                    float firstLogWeightWithFellerBuncherAndMidCorridorBarkLoss = firstLogVolumeInM3 * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester; // / merchantableFractionOfLogLength; // 1 in BC Firmwood

                    bool treeBuckedManuallyWithGrappleSwingYarder = false;
                    if (yardedWeightPerStemWithFellerBuncher > this.GrappleSwingYarderMaxPayload)
                    {
                        float treeChainsawBuckTime = this.ChainsawBuckConstant + this.ChainsawBuckLinear * treeMerchantableVolumeInM3;
                        if (treeMerchantableVolumeInM3 > this.ChainsawBuckQuadraticThreshold)
                        {
                            float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ChainsawBuckQuadraticThreshold;
                            treeChainsawBuckTime += this.ChainsawBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                        }
                        longLogHarvest.ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleSwingYarder += expansionFactorPerHa * treeMerchantableVolumeInM3;
                        longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder += expansionFactorPerHa * treeChainsawBuckTime;
                        treeBuckedManuallyWithGrappleSwingYarder = true;

                        longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;
                        if (firstLogWeightWithFellerBuncherAndMidCorridorBarkLoss > this.GrappleSwingYarderMaxPayload)
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
                        longLogHarvest.ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleYoader += expansionFactorPerHa * treeMerchantableVolumeInM3;
                        longLogHarvest.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader += expansionFactorPerHa * treeChainsawBuckTime;
                        treeBuckedManuallyWithGrappleYoader = true;

                        longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;
                        if (firstLogWeightWithFellerBuncherAndMidCorridorBarkLoss > this.GrappleYoaderMaxPayload)
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
                        longLogHarvest.YardedWeightPerHaWithTrackedHarvester += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemWithHarvester * treeSpeciesProperties.StemDensityAtMidspanAfterHarvester;
                        longLogHarvest.TrackedHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }
                    else
                    {
                        // a chainsaw is used on this tree, so it applies towards chainsaw utilization
                        // For now, since long log harvests are assumed to be regeneration harvests, assume the harvester is free to maneuver
                        // across corridors to fall all trees behind its progression across the unit. This implies the harvester is free to assist
                        // chainsaw work by felling as many trees as it can.
                        longLogHarvest.ChainsawBasalAreaPerHaWithTrackedHarvester += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;

                        float treeChainsawTime; // seconds
                        float treeHarvesterTime; // seconds
                        if (dbhInCm > this.TrackedHarvesterFellingDiameterLimit)
                        {
                            // tree felled by chainsaw, no cutting by harvester in this case
                            chainsawFallingWithTrackedHarvester = true;
                            treeChainsawTime = this.ChainsawFellAndBuckConstant + this.ChainsawFellAndBuckLinear * treeMerchantableVolumeInM3;
                            treeHarvesterTime = 0.0F;
                        }
                        else
                        {
                            // tree felled by harvester, bucked by chainsaw
                            // Since there aren't felling only coefficients for the harvester, approximate its felling time as the fell and buck
                            // constant plus linear time and neglect the quadratic terms.
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
                        longLogHarvest.ChainsawCubicVolumePerHaWithTrackedHarvester += expansionFactorPerHa * treeMerchantableVolumeInM3;
                        longLogHarvest.ChainsawPMhPerHaWithTrackedHarvester += expansionFactorPerHa * treeChainsawTime;
                        longLogHarvest.YardedWeightPerHaWithTrackedHarvester += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
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
                        longLogHarvest.YardedWeightPerHaWithWheeledHarvester += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemWithHarvester * treeSpeciesProperties.StemDensityAtMidspanAfterHarvester;
                        longLogHarvest.WheeledHarvesterPMhPerHa += expansionFactorPerHa * treeHarvesterTime;
                    }
                    else
                    {
                        // a chainsaw is used on this tree, so it applies towards chainsaw utilization
                        // As with a the tracked harvester it's assumed the harvester is free to maneuver across corridors to fall trees behind
                        // its progression.
                        longLogHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester += expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;

                        float treeChainsawTime; // seconds
                        float treeHarvesterTime; // seconds
                        if (dbhInCm > this.WheeledHarvesterFellingDiameterLimit)
                        {
                            chainsawFallingWithWheeledHarvester = true;
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
                        longLogHarvest.ChainsawCubicVolumePerHaWithWheeledHarvester += expansionFactorPerHa * treeMerchantableVolumeInM3;
                        longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester += expansionFactorPerHa * treeChainsawTime;
                        longLogHarvest.YardedWeightPerHaWithWheeledHarvester += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
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

                    // haul weight
                    float woodAndRemainingBarkVolumePerStemAfterYardingAndProcessing = (treeMerchantableVolumeInM3 + unscaledNeiloidVolume) / (1.0F - treeSpeciesProperties.BarkFractionAfterYardingAndProcessing); // / merchantableFractionOfLogLength; // 1 in BC Firmwood
                    longLogHarvest.LoadedWeightPerHa += expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterYardingAndProcessing * treeSpeciesProperties.StemDensityAfterYardingAndProcessing;
                }
            }

            // feller-buncher + chainsaw + yarder-processor-loader system costs
            float meanGrappleYardingTurnTime = this.GrappleYardingConstant + this.GrappleYardingLinear * 0.5F * stand.CorridorLengthInM; // parallel yarding
            float loaderSMhPerHectare = longLogHarvest.LoadedWeightPerHa / (this.LoaderUtilization * this.LoaderProductivity);
            // TODO: full corridor and processing bark loss
            {
                if (stand.SlopeInPercent > this.FellerBuncherSlopeThresholdInPercent)
                {
                    longLogHarvest.FellerBuncherPMhPerHa *= 1.0F + this.FellerBuncherSlopeLinear * (stand.SlopeInPercent - this.FellerBuncherSlopeThresholdInPercent);
                }
                longLogHarvest.FellerBuncherPMhPerHa /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.FellerBuncher = longLogHarvest.MerchantableCubicVolumePerHa / longLogHarvest.FellerBuncherPMhPerHa;
                float fellerBuncherCostPerHectare = fellerBuncherCostPerSMh * longLogHarvest.FellerBuncherPMhPerHa / this.FellerBuncherUtilization;

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
                    float chainsawByOperatorCost = chainsawByOperatorSMh * (fellerBuncherCostPerSMh + this.ChainsawByOperatorCostPerSMh);

                    if (chainsawBuckCost < chainsawByOperatorCost)
                    {
                        minimumChainsawCostWithFellerBuncherAndGrappleSwingYarder = chainsawBuckCost;
                        longLogHarvest.ChainsawCrewWithFellerBuncherAndGrappleSwingYarder = ChainsawCrewType.Bucker;
                        longLogHarvest.Productivity.ChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = chainsawBuckUtilization;
                    }
                    else
                    {
                        minimumChainsawCostWithFellerBuncherAndGrappleSwingYarder = chainsawByOperatorCost;
                        longLogHarvest.ChainsawCrewWithFellerBuncherAndGrappleSwingYarder = ChainsawCrewType.Operator;
                        longLogHarvest.Productivity.ChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = this.ChainsawByOperatorUtilization;
                    }
                    Debug.Assert(longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder > 0.0F);
                }

                float grappleSwingYarderTurnsPerHectare = longLogHarvest.YardedWeightPerHaWithFellerBuncher / this.GrappleSwingYarderMeanPayload;
                longLogHarvest.GrappleSwingYarderPMhPerHectare = grappleSwingYarderTurnsPerHectare * meanGrappleYardingTurnTime / Constant.SecondsPerHour;
                longLogHarvest.Productivity.GrappleSwingYarder = longLogHarvest.MerchantableCubicVolumePerHa / longLogHarvest.GrappleSwingYarderPMhPerHectare;
                float grappleSwingYarderSMhPerHectare = longLogHarvest.GrappleSwingYarderPMhPerHectare / this.GrappleSwingYarderUtilization;

                longLogHarvest.ProcessorPMhPerHaWithGrappleSwingYarder /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.ProcessorWithGrappleSwingYarder = longLogHarvest.MerchantableCubicVolumePerHa / longLogHarvest.ProcessorPMhPerHaWithGrappleSwingYarder;
                float processorSMhPerHectareWithGrappleSwingYarder = longLogHarvest.ProcessorPMhPerHaWithGrappleSwingYarder / this.ProcessorUtilization;

                // TODO: chainsaw productivity
                // loader productivity is specified so doesn't need to be output

                float haulRoundtripsPerHectare = longLogHarvest.LoadedWeightPerHa / this.LongLogHaulPayloadInKg;
                float haulCostPerHectare = this.LongLogHaulRoundtripSMh * haulRoundtripsPerHectare * this.LongLogHaulPerSMh;

                float limitingSMhWithFellerBuncherAndGrappleSwingYarder = MathE.Max(grappleSwingYarderSMhPerHectare, processorSMhPerHectareWithGrappleSwingYarder, loaderSMhPerHectare);
                float grappleSwingYarderCostPerHectareWithFellerBuncher = this.GrappleSwingYarderCostPerSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
                float processorCostPerHectareWithGrappleSwingYarder = this.ProcessorCostPerSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
                float loaderCostPerHectareWithFellerBuncherAndGrappleSwingYarder = this.LoaderCostPerSMh * limitingSMhWithFellerBuncherAndGrappleSwingYarder;
                int machinesToMoveInAndOutWithFellerBuncher = 5 + fellerBuncherOrTrackedHarvesterAnchorMachine; // bulldozer + feller-buncher [+ anchor] + yarder + processor + loader
                float machineMoveInAndOutPerHectareWithFellerBuncher = 2.0F * machinesToMoveInAndOutWithFellerBuncher * this.MachineMoveInOrOut / stand.AreaInHa;
                longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa = fellerBuncherCostPerHectare + minimumChainsawCostWithFellerBuncherAndGrappleSwingYarder + 
                    grappleSwingYarderCostPerHectareWithFellerBuncher + processorCostPerHectareWithGrappleSwingYarder + loaderCostPerHectareWithFellerBuncherAndGrappleSwingYarder + 
                    haulCostPerHectare + machineMoveInAndOutPerHectareWithFellerBuncher;

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
                    float chainsawByOperatorCost = chainsawByOperatorSMh * (fellerBuncherCostPerSMh + this.ChainsawByOperatorCostPerSMh);

                    if (chainsawBuckCost < chainsawByOperatorCost)
                    {
                        minimumChainsawCostWithFellerBuncherAndGrappleYoader = chainsawBuckCost;
                        longLogHarvest.ChainsawCrewWithFellerBuncherAndGrappleYoader = ChainsawCrewType.Bucker;
                        longLogHarvest.Productivity.ChainsawUtilizationWithFellerBuncherAndGrappleYoader = chainsawBuckUtilization;
                    }
                    else
                    {
                        minimumChainsawCostWithFellerBuncherAndGrappleYoader = chainsawByOperatorCost;
                        longLogHarvest.ChainsawCrewWithFellerBuncherAndGrappleYoader = ChainsawCrewType.Operator;
                        longLogHarvest.Productivity.ChainsawUtilizationWithFellerBuncherAndGrappleYoader = this.ChainsawByOperatorUtilization;
                    }
                    Debug.Assert(longLogHarvest.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader > 0.0F);
                }

                float grappleYoaderTurnsPerHectare = longLogHarvest.YardedWeightPerHaWithFellerBuncher / this.GrappleYoaderMeanPayload;
                longLogHarvest.GrappleYoaderPMhPerHectare = grappleYoaderTurnsPerHectare * meanGrappleYardingTurnTime / Constant.SecondsPerHour;
                longLogHarvest.Productivity.GrappleYoader = longLogHarvest.MerchantableCubicVolumePerHa / longLogHarvest.GrappleYoaderPMhPerHectare;
                float grappleYoaderSMhPerHectare = longLogHarvest.GrappleYoaderPMhPerHectare / this.GrappleYoaderUtilization;

                longLogHarvest.ProcessorPMhPerHaWithGrappleYoader /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.ProcessorWithGrappleYoader = longLogHarvest.MerchantableCubicVolumePerHa / longLogHarvest.ProcessorPMhPerHaWithGrappleYoader;
                float processorSMhPerHectareWithGrappleYoader = longLogHarvest.ProcessorPMhPerHaWithGrappleYoader / this.ProcessorUtilization;

                float limitingSMhWithFellerBuncherAndGrappleYoader = MathE.Max(grappleYoaderSMhPerHectare, processorSMhPerHectareWithGrappleYoader, loaderSMhPerHectare);
                float grappleYoaderCostPerHectareWithFellerBuncher = this.GrappleYoaderCostPerSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
                float processorCostPerHectareWithGrappleYoader = this.ProcessorCostPerSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
                float loaderCostPerHectareWithFellerBuncherAndGrappleYoader = this.LoaderCostPerSMh * limitingSMhWithFellerBuncherAndGrappleYoader;
                longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa = fellerBuncherCostPerHectare + minimumChainsawCostWithFellerBuncherAndGrappleYoader + 
                    grappleYoaderCostPerHectareWithFellerBuncher + processorCostPerHectareWithGrappleYoader + loaderCostPerHectareWithFellerBuncherAndGrappleYoader +
                    haulCostPerHectare + machineMoveInAndOutPerHectareWithFellerBuncher; 

                // if heuristic selects all trees for thinning then there are no trees at final harvest and SMh and costs are zero
                Debug.Assert((limitingSMhWithFellerBuncherAndGrappleSwingYarder >= 0.0F) && (Single.IsNaN(longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa) == false) && (longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa >= 0.0F) && (longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa < 275.0F * 1000.0F) &&
                             (limitingSMhWithFellerBuncherAndGrappleYoader >= 0.0F) && (Single.IsNaN(longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa) == false) && (longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa >= 0.0F) && (longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa < 260.0F * 1000.0F));

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
                    longLogHarvest.TrackedHarvesterPMhPerHa *= 1.0F + this.TrackedHarvesterSlopeLinear * (stand.SlopeInPercent - this.TrackedHarvesterSlopeThresholdInPercent);
                }
                longLogHarvest.TrackedHarvesterPMhPerHa /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.TrackedHarvester = longLogHarvest.MerchantableCubicVolumePerHa / longLogHarvest.TrackedHarvesterPMhPerHa;

                float trackedHarvesterCostPerHectare = trackedHarvesterCostPerSMh * longLogHarvest.TrackedHarvesterPMhPerHa / this.TrackedHarvesterUtilization;

                float minimumChainsawCostWithTrackedHarvester = 0.0F;
                if (longLogHarvest.ChainsawPMhPerHaWithTrackedHarvester > 0.0F)
                {
                    if (stand.SlopeInPercent > this.ChainsawSlopeThresholdInPercent)
                    {
                        longLogHarvest.ChainsawPMhPerHaWithTrackedHarvester *= 1.0F + this.ChainsawSlopeLinear * (stand.SlopeInPercent - this.ChainsawSlopeThresholdInPercent);
                    }
                    longLogHarvest.ChainsawPMhPerHaWithTrackedHarvester /= Constant.SecondsPerHour;

                    float chainsawByOperatorSMh = longLogHarvest.ChainsawPMhPerHaWithTrackedHarvester / this.ChainsawByOperatorUtilization; // SMh
                    float chainsawByOperatorCost = (trackedHarvesterCostPerSMh + this.ChainsawByOperatorCostPerSMh) * chainsawByOperatorSMh;
                    float chainsawCrewCost;
                    float chainsawCrewUtilization;
                    if (chainsawFallingWithTrackedHarvester)
                    {
                        chainsawCrewCost = this.ChainsawFellAndBuckCostPerSMh;
                        chainsawCrewUtilization = this.ChainsawFellAndBuckUtilization;
                    }
                    else
                    {
                        chainsawCrewCost = this.ChainsawBuckCostPerSMh;
                        chainsawCrewUtilization = this.ChainsawBuckUtilization;
                    }
                    chainsawCrewUtilization *= MathF.Min(longLogHarvest.ChainsawBasalAreaPerHaWithTrackedHarvester / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                    chainsawCrewCost *= longLogHarvest.ChainsawPMhPerHaWithTrackedHarvester / chainsawCrewUtilization;

                    if (chainsawCrewCost < chainsawByOperatorCost)
                    {
                        minimumChainsawCostWithTrackedHarvester = chainsawCrewCost;
                        longLogHarvest.ChainsawCrewWithTrackedHarvester = chainsawFallingWithTrackedHarvester ? ChainsawCrewType.Fallers : ChainsawCrewType.Bucker;
                        longLogHarvest.Productivity.ChainsawUtilizationWithTrackedHarvester = chainsawCrewUtilization;
                    }
                    else
                    {
                        minimumChainsawCostWithTrackedHarvester = chainsawByOperatorCost;
                        longLogHarvest.ChainsawCrewWithTrackedHarvester = ChainsawCrewType.Operator;
                        longLogHarvest.Productivity.ChainsawUtilizationWithTrackedHarvester = this.ChainsawByOperatorUtilization;
                    }
                    Debug.Assert(longLogHarvest.ChainsawBasalAreaPerHaWithTrackedHarvester > 0.0F);
                }

                float grappleSwingYarderTurnsPerHectare = longLogHarvest.YardedWeightPerHaWithFellerBuncher / this.GrappleSwingYarderMeanPayload;
                float grappleSwingYarderSMhPerHectare = grappleSwingYarderTurnsPerHectare * meanGrappleYardingTurnTime / (Constant.SecondsPerHour * this.GrappleSwingYarderUtilization);

                float haulRoundtripsPerHectareWithTrackedHarvester = longLogHarvest.YardedWeightPerHaWithTrackedHarvester / this.LongLogHaulPayloadInKg;
                float haulCostPerHectareWithTrackedHarvester = this.LongLogHaulRoundtripSMh * haulRoundtripsPerHectareWithTrackedHarvester * this.LongLogHaulPerSMh;

                float limitingSMhWithTrackedHarvesterAndGrappleSwingYarder = MathF.Max(grappleSwingYarderSMhPerHectare, loaderSMhPerHectare);
                float swingYarderCostPerHectareWithTrackedHarvester = this.GrappleSwingYarderCostPerSMh * limitingSMhWithTrackedHarvesterAndGrappleSwingYarder;
                float loaderCostPerHectareWithTrackedHarvesterAndGrappleSwingYarder = this.LoaderCostPerSMh * limitingSMhWithTrackedHarvesterAndGrappleSwingYarder;
                int machinesToMoveInAndOutWithTrackedHarvester = 4 + fellerBuncherOrTrackedHarvesterAnchorMachine; // bulldozer + harvester [+ anchor] + yarder + loader
                float machineMoveInAndOutPerHectareWithTrackedHarvester = 2.0F * machinesToMoveInAndOutWithTrackedHarvester * this.MachineMoveInOrOut / stand.AreaInHa;
                longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa = trackedHarvesterCostPerHectare + minimumChainsawCostWithTrackedHarvester + 
                    swingYarderCostPerHectareWithTrackedHarvester + loaderCostPerHectareWithTrackedHarvesterAndGrappleSwingYarder +
                    haulCostPerHectareWithTrackedHarvester + machineMoveInAndOutPerHectareWithTrackedHarvester;

                float grappleYoaderTurnsPerHectare = longLogHarvest.YardedWeightPerHaWithFellerBuncher / this.GrappleYoaderMeanPayload;
                float grappleYoaderSMhPerHectare = grappleYoaderTurnsPerHectare * meanGrappleYardingTurnTime / (Constant.SecondsPerHour * this.GrappleYoaderUtilization);

                float limitingSMhWithTrackedHarvesterAndGrappleYoader = MathF.Max(grappleYoaderSMhPerHectare, loaderSMhPerHectare);
                float grappleYoaderCostPerHectareWithTrackedHarvester = this.GrappleYoaderCostPerSMh * limitingSMhWithTrackedHarvesterAndGrappleYoader;
                float loaderCostPerHectareWithTrackedHarvesterAndGrappleYoader = this.LoaderCostPerSMh * limitingSMhWithTrackedHarvesterAndGrappleYoader;
                longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa = trackedHarvesterCostPerHectare + minimumChainsawCostWithTrackedHarvester + 
                    grappleYoaderCostPerHectareWithTrackedHarvester + loaderCostPerHectareWithTrackedHarvesterAndGrappleYoader +
                    haulCostPerHectareWithTrackedHarvester + machineMoveInAndOutPerHectareWithTrackedHarvester;

                // wheeled harvester
                if (stand.SlopeInPercent > this.WheeledHarvesterSlopeThresholdInPercent)
                {
                    longLogHarvest.WheeledHarvesterPMhPerHa *= 1.0F + this.WheeledHarvesterSlopeLinear * (stand.SlopeInPercent - this.WheeledHarvesterSlopeThresholdInPercent);
                }
                longLogHarvest.WheeledHarvesterPMhPerHa /= Constant.SecondsPerHour;
                longLogHarvest.Productivity.WheeledHarvester = longLogHarvest.MerchantableCubicVolumePerHa / longLogHarvest.WheeledHarvesterPMhPerHa;

                float wheeledHarvesterSMh = longLogHarvest.WheeledHarvesterPMhPerHa / this.WheeledHarvesterUtilization;
                float wheeledHarvesterCostPerHectare = wheeledHarvesterCostPerSMh * wheeledHarvesterSMh;

                float minimumChainsawCostWithWheeledHarvester = 0.0F;
                if (longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester > 0.0F)
                {
                    if (stand.SlopeInPercent > this.ChainsawSlopeThresholdInPercent)
                    {
                        longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester *= 1.0F + this.ChainsawSlopeLinear * (stand.SlopeInPercent - this.ChainsawSlopeThresholdInPercent);
                    }
                    longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester /= Constant.SecondsPerHour;

                    float chainsawByOperatorSMh = longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester / this.ChainsawByOperatorUtilization; // SMh
                    float chainsawByOperatorCost = (wheeledHarvesterCostPerSMh + this.ChainsawByOperatorCostPerSMh) * chainsawByOperatorSMh;
                    float chainsawCrewCost;
                    float chainsawCrewUtilization;
                    if (chainsawFallingWithWheeledHarvester)
                    {
                        chainsawCrewCost = this.ChainsawFellAndBuckCostPerSMh;
                        chainsawCrewUtilization = this.ChainsawFellAndBuckUtilization;
                    }
                    else
                    {
                        chainsawCrewCost = this.ChainsawBuckCostPerSMh;
                        chainsawCrewUtilization = this.ChainsawBuckUtilization;
                    }
                    chainsawCrewUtilization *= MathF.Min(longLogHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                    chainsawCrewCost *= longLogHarvest.ChainsawPMhPerHaWithWheeledHarvester / chainsawCrewUtilization;

                    if (chainsawCrewCost < chainsawByOperatorCost)
                    {
                        minimumChainsawCostWithWheeledHarvester = chainsawCrewCost;
                        longLogHarvest.ChainsawCrewWithWheeledHarvester = chainsawFallingWithWheeledHarvester ? ChainsawCrewType.Fallers : ChainsawCrewType.Bucker;
                        longLogHarvest.Productivity.ChainsawUtilizationWithWheeledHarvester = chainsawCrewUtilization;
                    }
                    else
                    {
                        minimumChainsawCostWithWheeledHarvester = chainsawByOperatorCost;
                        longLogHarvest.ChainsawCrewWithWheeledHarvester = ChainsawCrewType.Operator;
                        longLogHarvest.Productivity.ChainsawUtilizationWithWheeledHarvester = this.ChainsawByOperatorUtilization;
                    }

                    Debug.Assert(longLogHarvest.ChainsawBasalAreaPerHaWithWheeledHarvester > 0.0F);
                }

                float haulRoundtripsPerHectareWithWheeledHarvester = longLogHarvest.YardedWeightPerHaWithWheeledHarvester / this.LongLogHaulPayloadInKg;
                float haulCostPerHectareWithWheeledHarvester = this.LongLogHaulRoundtripSMh * haulRoundtripsPerHectareWithWheeledHarvester * this.LongLogHaulPerSMh;

                float limitingSMhWithWheeledHarvesterAndGrappleSwingYarder = MathF.Max(grappleSwingYarderSMhPerHectare, loaderSMhPerHectare);
                float swingYarderCostPerHectareWithWheeledHarvester = this.GrappleSwingYarderCostPerSMh * limitingSMhWithWheeledHarvesterAndGrappleSwingYarder;
                float loaderCostPerHectareWithWheeledHarvesterAndGrappleSwingYarder = this.LoaderCostPerSMh * limitingSMhWithWheeledHarvesterAndGrappleSwingYarder;
                int machinesToMoveInAndOutWithWheeledHarvester = 4 + wheeledHarvesterAnchorMachine; // bulldozer + harvester [+ anchor] + yarder + loader
                float machineMoveInAndOutPerHectareWithWheeledHarvester = 2.0F * machinesToMoveInAndOutWithWheeledHarvester * this.MachineMoveInOrOut / stand.AreaInHa;
                longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa = wheeledHarvesterCostPerHectare + minimumChainsawCostWithWheeledHarvester + 
                    swingYarderCostPerHectareWithWheeledHarvester + loaderCostPerHectareWithWheeledHarvesterAndGrappleSwingYarder +
                    haulCostPerHectareWithWheeledHarvester + machineMoveInAndOutPerHectareWithWheeledHarvester;

                float limitingSMhWithWheeledHarvesterAndGrappleYoader = MathF.Max(grappleYoaderSMhPerHectare, loaderSMhPerHectare);
                float grappleYoaderCostPerHectareWithWheeledHarvester = this.GrappleYoaderCostPerSMh * limitingSMhWithWheeledHarvesterAndGrappleYoader;
                float loaderCostPerHectareWithWheeledHarvesterAndGrappleYoader = this.LoaderCostPerSMh * limitingSMhWithWheeledHarvesterAndGrappleYoader;
                longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa = wheeledHarvesterCostPerHectare + minimumChainsawCostWithWheeledHarvester + 
                    grappleYoaderCostPerHectareWithWheeledHarvester + loaderCostPerHectareWithWheeledHarvesterAndGrappleYoader +
                    haulCostPerHectareWithWheeledHarvester + machineMoveInAndOutPerHectareWithWheeledHarvester;

                // if all trees are especially large harvester costs per hectare can be zero as only chainsaw felling and bucking is performed
                Debug.Assert((trackedHarvesterCostPerHectare >= 0.0F) && (minimumChainsawCostWithTrackedHarvester >= 0.0F) && (haulCostPerHectareWithTrackedHarvester >= 0.0F) && (machineMoveInAndOutPerHectareWithTrackedHarvester > 0.0F) &&
                             (limitingSMhWithTrackedHarvesterAndGrappleSwingYarder >= 0.0F) && (swingYarderCostPerHectareWithTrackedHarvester >= 0.0F) && (loaderCostPerHectareWithTrackedHarvesterAndGrappleSwingYarder >= 0.0F) &&
                             (Single.IsNaN(longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa) == false) && (longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa >= 0.0F) && (longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa < 200.0F * 1000.0F) &&
                             (limitingSMhWithTrackedHarvesterAndGrappleYoader >= 0.0F) && (grappleYoaderCostPerHectareWithTrackedHarvester >= 0.0F) && (loaderCostPerHectareWithTrackedHarvesterAndGrappleYoader >= 0.0F) &&
                             (Single.IsNaN(longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa) == false) && (longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa >= 0.0F) && (longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa < 200.0F * 1000.0F) &&
                             (wheeledHarvesterCostPerHectare >= 0.0F) && (minimumChainsawCostWithWheeledHarvester >= 0.0F) && (haulCostPerHectareWithWheeledHarvester >= 0.0F) && (machineMoveInAndOutPerHectareWithWheeledHarvester > 0.0F) &&
                             (limitingSMhWithWheeledHarvesterAndGrappleSwingYarder >= 0.0F) && (grappleYoaderCostPerHectareWithWheeledHarvester >= 0.0F) && (loaderCostPerHectareWithWheeledHarvesterAndGrappleYoader >= 0.0F) &&
                             (Single.IsNaN(longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa) == false) && (longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa >= 0.0F) && (longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa < 125.0F * 1000.0F) &&
                             (limitingSMhWithWheeledHarvesterAndGrappleYoader >= 0.0F) && 
                             (Single.IsNaN(longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa) == false) && (longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa > 0.0F) && (longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa < 125.0F * 1000.0F));
            }

            longLogHarvest.MinimumSystemCostPerHa = MathE.Min(longLogHarvest.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa, longLogHarvest.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa,
                                                              longLogHarvest.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa, longLogHarvest.TrackedHarvesterGrappleYoaderLoaderCostPerHa,
                                                              longLogHarvest.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa, longLogHarvest.WheeledHarvesterGrappleYoaderLoaderCostPerHa);
        }

        public void VerifyPropertyValues()
        {
            if ((this.AddOnWinchCableLengthInM < 250.0F) || (this.AddOnWinchCableLengthInM > 350.0F))
            {
                throw new NotSupportedException("Add on winch cable length is not in the range [250.0, 350.0] m.");
            }
            if ((this.AnchorCostPerSMh < 50.0F) || (this.AnchorCostPerSMh > 200.0F))
            {
                throw new NotSupportedException("Cost of tether anchor machine is not in the range US$ [50.0, 200.0]/SMh.");
            }
            if ((this.ChainsawBuckConstant < 0.0F) || (this.ChainsawBuckConstant > 90.0F))
            {
                throw new NotSupportedException("Intercept of chainsaw bucking time is not in the range [0.0, 90.0] seconds.");
            }
            if ((this.ChainsawBuckCostPerSMh < 50.0F) || (this.ChainsawBuckCostPerSMh > 250.0F))
            {
                throw new NotSupportedException("Cost of chainsaw bucking time is not in the range US$ [50.0, 250.0]/PMh.");
            }
            if ((this.ChainsawBuckLinear < 0.0F) || (this.ChainsawBuckLinear > 150.0F))
            {
                throw new NotSupportedException("Linear coefficient of chainsaw bucking time is not in the range [0.0, 150.0] seconds/m³.");
            }
            if ((this.ChainsawBuckUtilization < 0.1F) || (this.ChainsawBuckUtilization > 1.0F))
            {
                throw new NotSupportedException("Utilization of chainsw bucker is not in the range [0.1, 1.0].");
            }
            if ((this.ChainsawBuckQuadratic < 0.0F) || (this.ChainsawBuckQuadratic > 90.0F))
            {
                throw new NotSupportedException("Quadratic coefficient of chainsaw bucking time is not in the range [0.0, 25.0] seconds/m³.");
            }
            if ((this.ChainsawBuckQuadraticThreshold < 0.0F) || (this.ChainsawBuckQuadraticThreshold > 90.0F))
            {
                throw new NotSupportedException("Onset of quadratic chainsaw bucking time is not in the range [0.0, 25.0] m³.");
            }
            if ((this.ChainsawByOperatorCostPerSMh < 0.0F) || (this.ChainsawByOperatorCostPerSMh > 25.0F))
            {
                throw new NotSupportedException("Cost of chainsaw use by heavy equipment operator is not in the range US$ [0.0, 25.0]/PMh.");
            }
            if ((this.ChainsawByOperatorUtilization < 0.1F) || (this.ChainsawByOperatorUtilization > 1.0F))
            {
                throw new NotSupportedException("Utilization of chainsaw use by heavy equipment operator is not in the range [0.1, 1.0].");
            }
            if ((this.ChainsawFellAndBuckConstant < 0.0F) || (this.ChainsawFellAndBuckConstant > 90.0F))
            {
                throw new NotSupportedException("Intercept of chainsaw felling and bucking time is not in the range (0.0, 90.0] seconds.");
            }
            if ((this.ChainsawFellAndBuckCostPerSMh < 50.0F) || (this.ChainsawFellAndBuckCostPerSMh > 400.0F))
            {
                throw new NotSupportedException("Cost of chainsaw felling and bucking is not in the range US$ [50.0, 400.0]/SMh.");
            }
            if ((this.ChainsawFellAndBuckLinear < 0.0F) || (this.ChainsawFellAndBuckLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of chainsaw felling and bucking time is not in the range (0.0, 250.0] seconds/m³.");
            }
            if ((this.ChainsawSlopeLinear < 0.0F) || (this.ChainsawSlopeLinear > 0.2F))
            {
                throw new NotSupportedException("Linear slope coeffcient on chainsaw cycle time is not in the range of [0.0, 0.2].");
            }
            if ((this.ChainsawSlopeThresholdInPercent <= 0.0F) || (this.ChainsawSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on chainsaw crews is not in the range of [0.0, 200.0]%.");
            }
            if ((this.ChainsawFellAndBuckUtilization <= 0.1F) || (this.ChainsawFellAndBuckUtilization > 1.0F))
            {
                throw new NotSupportedException("Utilization of chainsaw felling and bucking crew is not in the range [0.1, 1.0].");
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
            if ((this.FellerBuncherCostPerSMh < 100.0F) || (this.FellerBuncherCostPerSMh > 500.0F))
            {
                throw new NotSupportedException("Feller-buncher operating cost is not in the range US$ [100.0, 500.0]/PMh.");
            }
            if ((this.FellerBuncherSlopeLinear < 0.0F) || (this.FellerBuncherSlopeLinear > 0.1F))
            {
                throw new NotSupportedException("Linear coefficient of slope effect on feller-buncher felling time is not in the range [0.0, 0.1].");
            }
            if ((this.FellerBuncherSlopeThresholdInPercent < 0.0F) || (this.FellerBuncherSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on feller-buncher felling time is not in the range [0.0, 200.0]%.");
            }

            if ((this.ForwarderCostPerSMh < 100.0F) || (this.ForwarderCostPerSMh > 500.0F))
            {
                throw new NotSupportedException("Forwarder operating cost is not in the range US$ [0.0, 500.0]/SMh.");
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
                throw new NotSupportedException("Forwarder maximum payload is not in the range [1000.0, 30000.0] kg.");
            }
            if ((this.ForwarderSpeedInStandLoadedTethered <= 0.0F) || (this.ForwarderSpeedInStandLoadedTethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder loaded travel speed while tethered is not in the range (0.0, 100.0] m/min.");
            }
            if ((this.ForwarderSpeedInStandLoadedUntethered <= 0.0F) || (this.ForwarderSpeedInStandLoadedUntethered > 125.0F))
            {
                throw new NotSupportedException("Forwarder loaded travel speed without a tether is not in the range (0.0, 125.0] m/min.");
            }
            if ((this.ForwarderSpeedInStandUnloadedTethered <= 0.0F) || (this.ForwarderSpeedInStandUnloadedTethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder unloaded travel speed while tethered is not in the range (0.0, 100.0] m/min.");
            }
            if ((this.ForwarderSpeedInStandUnloadedUntethered <= 0.0F) || (this.ForwarderSpeedInStandUnloadedUntethered > 150.0F))
            {
                throw new NotSupportedException("Forwarder unloaded travel speed without a tether is not in the range (0.0, 150.0] m/min.");
            }
            if ((this.ForwarderSpeedOnRoad <= 0.0F) || (this.ForwarderSpeedOnRoad > 150.0F))
            {
                throw new NotSupportedException("Forwarder travel speed on roads is not in the range (0.0, 150.0] m/min.");
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
            if ((this.ForwarderUtilization < 0.1F) || (this.ForwarderUtilization > 1.0F))
            {
                throw new NotSupportedException("Forwarder utilization is not in the range [0.1, 1.0].");
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
            if ((this.GrappleSwingYarderUtilization <= 0.1F) || (this.GrappleSwingYarderUtilization > 1.0F))
            {
                throw new NotSupportedException("Grapple swing yarder utilization is not in the range of [0.1, 1.0].");
            }
            if ((this.GrappleYoaderMaxPayload < 500.0F) || (this.GrappleYoaderMaxPayload > 8000.0F))
            {
                throw new NotSupportedException("Grapple yoader maximum payload is not in the range of [500.0, 8000.0] kg.");
            }
            if ((this.GrappleYoaderMeanPayload < 100.0F) || (this.GrappleYoaderMeanPayload > 4500.0F))
            {
                throw new NotSupportedException("Grapple yoader mean payload is not in the range of [100.0, 4500.0] kg.");
            }
            if ((this.GrappleYoaderCostPerSMh < 100.0F) || (this.GrappleYoaderCostPerSMh > 500.0F))
            {
                throw new NotSupportedException("Grapple yoader operating cost is not in the range of US$ [100.0, 500.0]/SMh.");
            }
            if ((this.GrappleYoaderUtilization < 0.1F) || (this.GrappleYoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Grapple yoader utilization is not in the range of [0.1, 1.0].");
            }

            if ((this.LoaderCostPerSMh < 100.0F) || (this.LoaderCostPerSMh > 500.0F))
            {
                throw new NotSupportedException("Loader operating cost is not in the range US$ [100.0, 500.0]/SMh.");
            }
            if ((this.LoaderProductivity < 20000.0F) || (this.LoaderProductivity > 80000.0F))
            {
                throw new NotSupportedException("Loader productivity is not in the range [20000.0, 80000.0] kg/PMh.");
            }
            if ((this.LoaderUtilization < 0.1F) || (this.LoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Loader utilization is not in the range of [0.1, 1.0].");
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

            if ((this.MachineMoveInOrOut < 100.0F) || (this.MachineMoveInOrOut > 1000.0F))
            {
                throw new NotSupportedException("Cost to move one piece of heavy equipment in or out is not in the range of US$ [100.0, 1000.0].");
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
            if ((this.ProcessorCostPerSMh < 100.0F) || (this.ProcessorCostPerSMh > 500.0F))
            {
                throw new NotSupportedException("Processor operating cost is not in the range US$ [100.0, 500.0]/PMh.");
            }
            if ((this.ProcessorUtilization < 0.1F) || (this.LoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Processor utilization is not in the range of [0.1, 1.0].");
            }

            if ((this.TrackedHarvesterCostPerSMh < 100.0F) || (this.TrackedHarvesterCostPerSMh > 500.0F))
            {
                throw new NotSupportedException("Tracked harvester operating cost is not in the range US$ [100.0, 500.0]/SMh.");
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
            if ((this.TrackedHarvesterUtilization < 0.1F) || (this.TrackedHarvesterUtilization > 1.0F))
            {
                throw new NotSupportedException("Tracked harvester utilization is not in the range [0.1, 1.0].");
            }

            if ((this.WheeledHarvesterCostPerSMh < 100.0F) || (this.WheeledHarvesterCostPerSMh > 500.0F))
            {
                throw new NotSupportedException("Wheeled harvester operating cost is not in the range US$ [100.0, 500.0]/SMh.");
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
            if ((this.WheeledHarvesterUtilization < 0.1F) || (this.WheeledHarvesterUtilization > 1.0F))
            {
                throw new NotSupportedException("Wheeled harvester utilization is not in the range [0.1, 1.0].");
            }
        }

        private struct ForwarderTurn
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
