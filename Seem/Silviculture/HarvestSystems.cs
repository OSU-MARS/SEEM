using Mars.Seem.Tree;
using System;

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

        private float GetChainsawBuckTime(float treeMerchantableVolumeInM3)
        {
            float treeChainsawTime = this.ChainsawBuckConstant + this.ChainsawBuckLinear * treeMerchantableVolumeInM3;
            if (treeMerchantableVolumeInM3 > this.ChainsawBuckQuadraticThreshold)
            {
                // tree is always bucked by chainsaw, so quadratic component always applies above threshold
                float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ChainsawBuckQuadraticThreshold;
                treeChainsawTime += this.ChainsawBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
            }
            return treeChainsawTime;
        }

        public float GetChainsawFellAndBuckTime(float treeMerchantableVolumeInM3)
        {
            float treeChainsawPMs = this.ChainsawFellAndBuckConstant + this.ChainsawFellAndBuckLinear * treeMerchantableVolumeInM3;
            if (treeMerchantableVolumeInM3 > this.ChainsawBuckQuadraticThreshold)
            {
                float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ChainsawBuckQuadraticThreshold;
                treeChainsawPMs += this.ChainsawBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
            }
            return treeChainsawPMs;
        }

        public (float treeFellerBuncherPMs, float treeChainsawPMsWithYarder, float treeChainsawPMsWithYoader) GetFellerBuncherTime(float treeMerchantableVolumeInM3, float treeYardedWeightInKg)
        {
            float treeFellerBuncherPMs = this.FellerBuncherFellingConstant + this.FellerBuncherFellingLinear * treeMerchantableVolumeInM3;

            float treeChainsawPMsWithYarder = 0.0F;  // productive machine seconds
            if (treeYardedWeightInKg > this.GrappleSwingYarderMaxPayload)
            {
                treeChainsawPMsWithYarder = this.GetChainsawBuckTime(treeMerchantableVolumeInM3);
            }

            float treeChainsawPMsWithYoader = 0.0F;  // productive machine seconds
            if (treeYardedWeightInKg > this.GrappleYoaderMaxPayload)
            {
                if (treeChainsawPMsWithYarder > 0.0F)
                {
                    treeChainsawPMsWithYoader = treeChainsawPMsWithYarder; // reuse already computed value
                }
                else
                {
                    treeChainsawPMsWithYoader = this.GetChainsawBuckTime(treeMerchantableVolumeInM3);
                }
            }

            return (treeFellerBuncherPMs, treeChainsawPMsWithYarder, treeChainsawPMsWithYoader);
        }

        public float GetProcessorTime(float treeMerchantableVolumeInM3, bool includeQuadratic)
        {
            float treeProcessingPMs = this.ProcessorBuckConstant + this.ProcessorBuckLinear * treeMerchantableVolumeInM3;
            if (includeQuadratic)
            {
                if (treeMerchantableVolumeInM3 > this.ProcessorBuckQuadraticThreshold1)
                {
                    float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ProcessorBuckQuadraticThreshold1;
                    treeProcessingPMs += this.ProcessorBuckQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                }
                if (treeMerchantableVolumeInM3 > this.ProcessorBuckQuadraticThreshold2)
                {
                    float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.ProcessorBuckQuadraticThreshold2;
                    treeProcessingPMs += this.ProcessorBuckQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                }
            }

            return treeProcessingPMs;
        }

        public (float treeHarvesterPMs, float treeChainsawPMs) GetTrackedHarvesterTime(float dbhInCm, float treeMerchantableVolumeInM3)
        {
            float treeHarvesterPMs; // productive machine seconds
            float treeChainsawPMs = 0.0F; // productive machine seconds
            if (dbhInCm <= this.TrackedHarvesterFellAndBuckDiameterLimit)
            {
                treeHarvesterPMs = this.TrackedHarvesterFellAndBuckConstant + this.TrackedHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                if (treeMerchantableVolumeInM3 > this.TrackedHarvesterQuadraticThreshold1)
                {
                    float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.TrackedHarvesterQuadraticThreshold1;
                    treeHarvesterPMs += this.TrackedHarvesterFellAndBuckQuadratic1 * volumeBeyondThreshold * volumeBeyondThreshold;
                }
                if (treeMerchantableVolumeInM3 > this.TrackedHarvesterQuadraticThreshold2)
                {
                    float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.TrackedHarvesterQuadraticThreshold2;
                    treeHarvesterPMs += this.TrackedHarvesterFellAndBuckQuadratic2 * volumeBeyondThreshold * volumeBeyondThreshold;
                }
            }
            else
            {
                if (dbhInCm > this.TrackedHarvesterFellingDiameterLimit)
                {
                    // tree felled and bucked by chainsaw, no cutting by harvester in this case
                    treeHarvesterPMs = 0.0F;
                    treeChainsawPMs = this.GetChainsawFellAndBuckTime(treeMerchantableVolumeInM3);
                }
                else
                {
                    // tree felled by harvester, bucked by chainsaw
                    // Since there aren't felling only coefficients for the harvester, approximate the tree's felling time as the fell and
                    // buck constant plus linear time and neglect the quadratic terms.
                    treeHarvesterPMs = this.TrackedHarvesterFellAndBuckConstant + this.TrackedHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                    treeChainsawPMs = this.GetChainsawBuckTime(treeMerchantableVolumeInM3);
                }
            }

            return (treeHarvesterPMs, treeChainsawPMs);
        }

        public (float treeHarvesterPMs, float treeChainsawPMs) GetWheeledHarvesterTime(float dbhInCm, float treeMerchantableVolumeInM3, bool previousOversizeTreeBehindHarvester)
        {
            float treeChainsawPMs = 0.0F; // productive machine seconds
            float treeHarvesterPMs; // productive machine seconds
            if (dbhInCm <= this.WheeledHarvesterFellAndBuckDiameterLimit)
            {
                // tree felled and bucked by harvester, no chainsaw use
                treeHarvesterPMs = this.WheeledHarvesterFellAndBuckConstant + this.WheeledHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                if (treeMerchantableVolumeInM3 > this.WheeledHarvesterQuadraticThreshold)
                {
                    float volumeBeyondThreshold = treeMerchantableVolumeInM3 - this.WheeledHarvesterQuadraticThreshold;
                    treeHarvesterPMs += this.WheeledHarvesterFellAndBuckQuadratic * volumeBeyondThreshold * volumeBeyondThreshold;
                }
            }
            else
            {
                if ((dbhInCm > this.WheeledHarvesterFellingDiameterLimit) || previousOversizeTreeBehindHarvester)
                {
                    // tree felled and bucked by chainsaw, no cutting by harvester in this case
                    treeHarvesterPMs = 0.0F;
                    treeChainsawPMs = this.GetChainsawFellAndBuckTime(treeMerchantableVolumeInM3);
                }
                else
                {
                    // tree felled by harvester, bucked by chainsaw
                    // Since there aren't felling only coefficients for the harvester, approximate its felling time as the fell and
                    // buck constant plus linear time and neglect the quadratic terms.
                    treeHarvesterPMs = this.WheeledHarvesterFellAndBuckConstant + this.WheeledHarvesterFellAndBuckLinear * treeMerchantableVolumeInM3;
                    treeChainsawPMs = this.GetChainsawBuckTime(treeMerchantableVolumeInM3);
                }
            }

            return (treeHarvesterPMs, treeChainsawPMs);
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
    }
}