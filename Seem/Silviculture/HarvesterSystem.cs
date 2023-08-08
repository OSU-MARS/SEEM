using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class HarvesterSystem : FallingSystem
    {
        public bool AnchorMachine { get; private set; } // number of anchor machines used in tethered operation, zero if not applicable
        public float HarvesterCostPerHa { get; private set; } // US$/ha
        public float HarvesterCostPerSMh { get; private set; } // US$/SMh
        public float HarvesterPMhPerHa { get; private set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float HarvesterProductivity { get; private set; } // m³/PMh₀
        public bool IsTracked { get; init; }
        public float SystemCostPerHaWithYarder { get; private set; } // US$/ha
        public float SystemCostPerHaWithYoader { get; private set; } // US$/ha
        // yarded weight with harvesters depends on bark loss from processing by the harvester versus chainsaw bucking
        public float YardedWeightPerHa { get; private set; } // kg/ha

        public HarvesterSystem()
        {
            this.IsTracked = false;

            this.Clear();
        }

        public void AddTree(float harvesterPMsPerHa)
        {
            this.HarvesterPMhPerHa += harvesterPMsPerHa;
            // no yarding case: implies harvester-forwarder system
        }

        public void AddTree(float treeHarvesterPMsPerHa, float yardedWeightInKgPerHa)
        {
            this.HarvesterPMhPerHa += treeHarvesterPMsPerHa;
            this.YardedWeightPerHa += yardedWeightInKgPerHa;
        }

        public void AddTree(float harvesterPMsPerHa, float chainsawPMsPerHa, float treeBasalAreaPerHa, float treeMerchantableVolumeInM3perHa)
        {
            if (harvesterPMsPerHa == 0.0F)
            {
                this.ChainsawFalling = true;
            }
            else
            {
                this.HarvesterPMhPerHa += harvesterPMsPerHa;
            }

            this.ChainsawBasalAreaPerHa += treeBasalAreaPerHa;
            this.ChainsawCubicVolumePerHa += treeMerchantableVolumeInM3perHa;
            this.ChainsawPMhPerHa += chainsawPMsPerHa;
        }

        public void AddTree(float harvesterPMsPerHa, float chainsawPMsPerHa, float treeBasalAreaPerHa, float treeMerchantableVolumeInM3perHa, float yardedWeightInKgPerHa)
        {
            this.AddTree(harvesterPMsPerHa, chainsawPMsPerHa, treeBasalAreaPerHa, treeMerchantableVolumeInM3perHa);
            this.YardedWeightPerHa += yardedWeightInKgPerHa;
        }

        public void CalculatePMhAndProductivity(Stand stand, HarvestSystems harvestSystems, float merchantableCubicVolumePerHa)
        {
            // include anchor cost if falling machine is operating tethered
            this.HarvesterCostPerSMh = this.IsTracked ? harvestSystems.TrackedHarvesterCostPerSMh: harvestSystems.WheeledHarvesterCostPerSMh;
            if (stand.SlopeInPercent > Constant.Default.SlopeForTetheringInPercent)
            {
                if (this.IsTracked)
                {
                    this.AnchorMachine = true;
                    this.HarvesterCostPerSMh += harvestSystems.AnchorCostPerSMh;
                }
                else if (stand.CorridorLengthInM > harvestSystems.AddOnWinchCableLengthInM)
                {
                    // wheeled harvester needs add on winch when onboard cable length is exceeded
                    this.AnchorMachine = true;
                    this.HarvesterCostPerSMh += harvestSystems.AnchorCostPerSMh;
                }
            }

            if (this.HarvesterPMhPerHa > 0.0F)
            {
                float slopeInPercent = stand.SlopeInPercent;
                float slopeThreshold = this.IsTracked ? harvestSystems.TrackedHarvesterSlopeThresholdInPercent : harvestSystems.WheeledHarvesterSlopeThresholdInPercent;
                if (slopeInPercent > slopeThreshold)
                {
                    float pmhIncreaseForSlope;
                    if (this.IsTracked)
                    {
                        pmhIncreaseForSlope = harvestSystems.TrackedHarvesterSlopeLinear * (slopeInPercent - harvestSystems.TrackedHarvesterSlopeThresholdInPercent);
                    }
                    else
                    {
                        pmhIncreaseForSlope = harvestSystems.WheeledHarvesterSlopeLinear * (slopeInPercent - harvestSystems.WheeledHarvesterSlopeThresholdInPercent);
                    }
                    this.HarvesterPMhPerHa *= 1.0F + pmhIncreaseForSlope;
                }

                this.HarvesterPMhPerHa /= Constant.SecondsPerHour;
                this.HarvesterProductivity = merchantableCubicVolumePerHa / this.HarvesterPMhPerHa;
                Debug.Assert((this.HarvesterProductivity >= 0.0F) && (this.HarvesterProductivity < 1000.0F));
            }
            else
            {
                Debug.Assert(Single.IsNaN(this.HarvesterProductivity));
            }

            this.AdjustChainsawForSlopeAndSetProductivity(stand, harvestSystems, this.HarvesterCostPerSMh);

            float wheeledHarvesterSMhPerHectare = this.HarvesterPMhPerHa / harvestSystems.WheeledHarvesterUtilization;
            this.HarvesterCostPerHa = this.HarvesterCostPerSMh * wheeledHarvesterSMhPerHectare;
            Debug.Assert(this.HarvesterCostPerHa >= 0.0F);
        }

        public void CalculateSystemCost(Stand stand, HarvestSystems harvestSystems, YardingSystem yarder, YardingSystem yoader, float loaderSMhPerHa)
        {
            // feller-buncher + chainsaw + yarder-loader system costs
            float haulRoundtripsPerHectare = this.YardedWeightPerHa / harvestSystems.LongLogHaulPayloadInKg;
            float haulCostPerHa = harvestSystems.LongLogHaulRoundtripSMh * haulRoundtripsPerHectare * harvestSystems.LongLogHaulPerSMh;
            // bulldozer + harvester [+ anchor] + yarder (larger yarders would count as two machines as two lowboys are required) + loader
            int machinesToMoveInAndOut = 4 + (this.AnchorMachine ? 1 : 0);
            float machineMoveInAndOutPerHectare = 2.0F * machinesToMoveInAndOut * harvestSystems.MachineMoveInOrOut / stand.AreaInHa;
            float systemCostBeyondLanding = this.HarvesterCostPerHa + this.ChainsawMinimumCost + haulCostPerHa + machineMoveInAndOutPerHectare;

            // yarder
            float limitingSMhWithYarder = MathF.Max(yarder.YarderSMhPerHectare, loaderSMhPerHa);
            float yarderCostPerHectare = harvestSystems.GrappleSwingYarderCostPerSMh * limitingSMhWithYarder;
            float loaderCostPerHectareWithYarder = harvestSystems.LoaderCostPerSMh * limitingSMhWithYarder;
            this.SystemCostPerHaWithYarder = systemCostBeyondLanding + yarderCostPerHectare + loaderCostPerHectareWithYarder;

            // yoader
            float limitingSMhWithYoader = MathF.Max(yoader.YarderSMhPerHectare, loaderSMhPerHa);
            float yoaderCostPerHectare = harvestSystems.GrappleYoaderCostPerSMh * limitingSMhWithYoader;
            float loaderCostPerHectareWithYoader = harvestSystems.LoaderCostPerSMh * limitingSMhWithYoader;
            this.SystemCostPerHaWithYoader = systemCostBeyondLanding + yoaderCostPerHectare + loaderCostPerHectareWithYoader;

            Debug.Assert((this.HarvesterCostPerHa >= 0.0F) && (this.ChainsawMinimumCost >= 0.0F) && (haulCostPerHa >= 0.0F) && (machineMoveInAndOutPerHectare > 0.0F));
            Debug.Assert((limitingSMhWithYarder >= 0.0F) && (yarderCostPerHectare >= 0.0F) && (loaderCostPerHectareWithYarder >= 0.0F));
            Debug.Assert((Single.IsNaN(this.SystemCostPerHaWithYarder) == false) && (this.SystemCostPerHaWithYarder >= 0.0F) && (this.SystemCostPerHaWithYarder < 200.0F * 1000.0F));
            Debug.Assert((limitingSMhWithYoader >= 0.0F) && (yoaderCostPerHectare >= 0.0F) && (loaderCostPerHectareWithYoader >= 0.0F));
            Debug.Assert((Single.IsNaN(this.SystemCostPerHaWithYoader) == false) && (this.SystemCostPerHaWithYoader >= 0.0F) && (this.SystemCostPerHaWithYoader < 200.0F * 1000.0F));
        }

        public new void Clear()
        {
            base.Clear();

            this.AnchorMachine = false;
            this.HarvesterCostPerHa = Single.NaN;
            this.HarvesterCostPerSMh = Single.NaN;
            this.HarvesterPMhPerHa = 0.0F;
            this.HarvesterProductivity = Single.NaN;
            this.SystemCostPerHaWithYarder = Single.NaN;
            this.SystemCostPerHaWithYoader = Single.NaN;
            this.YardedWeightPerHa = 0.0F;
        }
    }
}
