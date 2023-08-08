using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class FellerBuncherSystems
    {
        public bool AnchorMachine { get; private set; } // number of anchor machines used in tethered operation, zero if not applicable
        public float FellerBuncherCostPerHa { get; private set; } // US$/ha
        public float FellerBuncherCostPerSMh { get; private set; } // US$/SMh
        public float FellerBuncherPMhPerHa { get; private set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float FellerBuncherProductivity { get; private set; } // m³/PMh₀
        public float LoadedWeightPerHa { get; private set; } // m³/ha
        // yarded weight with feller buncher (plus chainsaw) should generally be higher than with harvesters as bark loss is lowest
        // However, this depends on the number of trees which require chainsaw felling or bucking.
        public float YardedWeightPerHa { get; private set; } // kg/ha

        public FellerBuncherSystem Yarder { get; private init; }
        public FellerBuncherSystem Yoader { get; private init; }

        public FellerBuncherSystems()
        {
            this.Yarder = new();
            this.Yoader = new();

            this.Clear();
        }

        public void AddTree(float fellerBuncherPMsPerHa, float yardedWeightInKgPerHa, float loadedWeightInKgPerHa)
        {
            this.FellerBuncherPMhPerHa += fellerBuncherPMsPerHa;
            this.LoadedWeightPerHa += loadedWeightInKgPerHa;
            this.YardedWeightPerHa += yardedWeightInKgPerHa;
        }

        public void AddTree(float fellerBuncherPMsPerHa, float chainsawPMsPerHaWithYarder, float chainsawPMsPerHaWithYoader, float treeBasalAreaInM2PerHa, float treeMerchantableVolumeInM3PerHa, float yardedWeightInKgPerHa, float loadedWeightInKgPerHa)
        {
            this.AddTree(fellerBuncherPMsPerHa, yardedWeightInKgPerHa, loadedWeightInKgPerHa);

            if (chainsawPMsPerHaWithYarder > 0.0F)
            {
                this.Yarder.AddChainsawTimeOnTree(chainsawPMsPerHaWithYarder, treeBasalAreaInM2PerHa, treeMerchantableVolumeInM3PerHa);
            }
            if (chainsawPMsPerHaWithYoader > 0.0F)
            {
                this.Yoader.AddChainsawTimeOnTree(chainsawPMsPerHaWithYoader, treeBasalAreaInM2PerHa, treeMerchantableVolumeInM3PerHa);
            }
        }

        public void CalculatePMhAndProductivity(Stand stand, HarvestSystems harvestSystems, float merchantableCubicVolumePerHa)
        {
            float slopeInPercent = stand.SlopeInPercent;

            // include anchor cost if falling machine is operating tethered
            this.FellerBuncherCostPerSMh = harvestSystems.FellerBuncherCostPerSMh;
            if (stand.SlopeInPercent > Constant.Default.SlopeForTetheringInPercent)
            {
                this.AnchorMachine = true;
                this.FellerBuncherCostPerSMh += harvestSystems.AnchorCostPerSMh;
            }

            if (this.FellerBuncherPMhPerHa > 0.0F)
            {
                if (slopeInPercent > harvestSystems.FellerBuncherSlopeThresholdInPercent)
                {
                    this.FellerBuncherPMhPerHa *= 1.0F + harvestSystems.FellerBuncherSlopeLinear * (slopeInPercent - harvestSystems.FellerBuncherSlopeThresholdInPercent);
                }
                this.FellerBuncherPMhPerHa /= Constant.SecondsPerHour;
                this.FellerBuncherProductivity = merchantableCubicVolumePerHa / this.FellerBuncherPMhPerHa;
                Debug.Assert((this.FellerBuncherProductivity >= 0.0F) && (this.FellerBuncherProductivity < 5000.0F));
            }
            else
            {
                Debug.Assert(Single.IsNaN(this.FellerBuncherProductivity));
            }

            this.Yarder.AdjustChainsawForSlopeAndSetProductivity(stand, harvestSystems, this.FellerBuncherCostPerSMh);
            this.Yoader.AdjustChainsawForSlopeAndSetProductivity(stand, harvestSystems, this.FellerBuncherCostPerSMh);

            this.FellerBuncherCostPerHa = this.FellerBuncherCostPerSMh * this.FellerBuncherPMhPerHa / harvestSystems.FellerBuncherUtilization;
        }

        public void CalculateSystemCost(Stand stand, HarvestSystems harvestSystems, YardingSystem yarder, YardingSystem yoader, float loaderSMhPerHa)
        {
            // feller-buncher + chainsaw + yarder-processor-loader system costs
            float processorSMhPerHectareWithYarder = yarder.ProcessorPMhPerHa / harvestSystems.ProcessorUtilization;

            float haulRoundtripsPerHectare = this.LoadedWeightPerHa / harvestSystems.LongLogHaulPayloadInKg;
            float haulCostPerHectare = harvestSystems.LongLogHaulRoundtripSMh * haulRoundtripsPerHectare * harvestSystems.LongLogHaulPerSMh;

            float limitingSMhWithYarder = MathE.Max(yarder.YarderSMhPerHectare, processorSMhPerHectareWithYarder, loaderSMhPerHa);
            float yarderCostPerHectare = harvestSystems.GrappleSwingYarderCostPerSMh * limitingSMhWithYarder;
            float processorCostPerHectareWithYarder = harvestSystems.ProcessorCostPerSMh * limitingSMhWithYarder;
            float loaderCostPerHectareWithYarder = harvestSystems.LoaderCostPerSMh * limitingSMhWithYarder;
            // bulldozer + feller-buncher [+ anchor] + yarder (larger yarders would count as two machines as two lowboys are required) + processor + loader
            int machinesToMoveInAndOutWithFellerBuncher = 5 + (this.AnchorMachine ? 1 : 0);
            float machineMoveInAndOutPerHectare = 2.0F * machinesToMoveInAndOutWithFellerBuncher * harvestSystems.MachineMoveInOrOut / stand.AreaInHa;
            this.Yarder.SystemCostPerHa = this.FellerBuncherCostPerHa + this.Yarder.ChainsawMinimumCost +
                yarderCostPerHectare + processorCostPerHectareWithYarder + loaderCostPerHectareWithYarder +
                haulCostPerHectare + machineMoveInAndOutPerHectare;

            // grapple yoader
            float processorSMhPerHectareWithYoader = yoader.ProcessorPMhPerHa / harvestSystems.ProcessorUtilization;

            float limitingSMhWithYoader = MathE.Max(yoader.YarderSMhPerHectare, processorSMhPerHectareWithYoader, loaderSMhPerHa);
            float yoaderCostPerHectare = harvestSystems.GrappleYoaderCostPerSMh * limitingSMhWithYoader;
            float processorCostPerHectareWithYoader = harvestSystems.ProcessorCostPerSMh * limitingSMhWithYoader;
            float loaderCostPerHectareWithYoader = harvestSystems.LoaderCostPerSMh * limitingSMhWithYoader;
            this.Yoader.SystemCostPerHa = this.FellerBuncherCostPerHa + this.Yoader.ChainsawMinimumCost +
                yoaderCostPerHectare + processorCostPerHectareWithYoader + loaderCostPerHectareWithYoader +
                haulCostPerHectare + machineMoveInAndOutPerHectare;

            // if heuristic selects all trees for thinning then there are no trees at final harvest and SMh and costs are zero
            Debug.Assert((limitingSMhWithYarder >= 0.0F) && (Single.IsNaN(this.Yarder.SystemCostPerHa) == false) && (this.Yarder.SystemCostPerHa >= 0.0F) && (this.Yarder.SystemCostPerHa < 275.0F * 1000.0F));
            Debug.Assert((limitingSMhWithYoader >= 0.0F) && (Single.IsNaN(this.Yoader.SystemCostPerHa) == false) && (this.Yoader.SystemCostPerHa >= 0.0F) && (this.Yoader.SystemCostPerHa < 260.0F * 1000.0F));

            // coupled utilization not currently calculated but may be of debugging interest
            //float swingYarderCoupledUtilization = grappleSwingYarderPMhPerHectare / limitingSMh;
            //float yoaderCoupledUtilization = grappleYoaderPMhPerHectare / limitingSMh;
            //float processorCoupledUtilization = processorPMhPerHectareWithGrappleSwingYarder / limitingSMh;
            //float processorCoupledUtilization = processorPMhPerHectareWithGrappleYoader / limitingSMh;
            //float loaderCoupledUtilization = loaderPMhPerHectare / limitingSMh;
        }

        public void Clear()
        {
            this.AnchorMachine = false;
            this.FellerBuncherCostPerHa = Single.NaN;
            this.FellerBuncherCostPerSMh = Single.NaN;
            this.FellerBuncherPMhPerHa = 0.0F;
            this.FellerBuncherProductivity = Single.NaN;
            this.LoadedWeightPerHa = 0.0F;
            this.YardedWeightPerHa = 0.0F;
            this.Yarder.Clear();
            this.Yoader.Clear();
        }
    }
}
