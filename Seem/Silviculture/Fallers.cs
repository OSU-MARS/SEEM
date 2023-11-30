using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class Fallers : FallingSystem
    {
        public float ChainsawProductivity { get; set; } // m³/PMh₀
        public float LoadedWeightPerHa { get; set; } // m³/ha
        public float SystemCostPerHaWithYarder { get; private set; } // US$/ha
        public float SystemCostPerHaWithYoader { get; private set; } // US$/ha
        public float YardedWeightPerHa { get; set; } // kg/ha

        public Fallers()
        {
            this.ChainsawCrew = ChainsawCrewType.Fallers;
            this.ChainsawFalling = true;
            this.ChainsawProductivity = Single.NaN;

            this.Clear();
        }

        public void AddTree(float chainsawPMsPerHa, float treeBasalAreaPerHa, float treeMerchantableVolumeInM3perHa, float yardedWeightInKgPerHa, float loadedWeightInKgPerHa)
        {
            this.ChainsawBasalAreaPerHa += treeBasalAreaPerHa;
            this.ChainsawCubicVolumePerHa += treeMerchantableVolumeInM3perHa;
            this.ChainsawPMhPerHa += chainsawPMsPerHa;
            this.LoadedWeightPerHa += loadedWeightInKgPerHa;
            this.YardedWeightPerHa += yardedWeightInKgPerHa;
        }

        public void CalculatePMhAndProductivity(Stand stand, HarvestSystems harvestSystems, float merchantableCubicVolumePerHa)
        {
            // passthrough to protected API as no felling machine is present
            this.AdjustChainsawForSlope(stand, harvestSystems);

            this.ChainsawProductivity = merchantableCubicVolumePerHa / this.ChainsawPMhPerHa;
        }

        public void CalculateSystemCost(Stand stand, HarvestSystems harvestSystems, YardingSystem yarder, YardingSystem yoader, float loaderSMhPerHa)
        {
            // bulldozer + yarder (larger yarders would count as two machines as two lowboys are required) + processor + loader
            float machineMoveInAndOutPerHa = harvestSystems.GetMoveInAndOutCost(4, stand);
            float haulCostPerHa = harvestSystems.GetLongLogHaulCost(this.LoadedWeightPerHa);
            float systemCostOffLanding = this.ChainsawMinimumCost + haulCostPerHa + machineMoveInAndOutPerHa;

            // fallers + chainsaw + yarder-processor-loader system costs
            float yarderLandingCostPerHa = harvestSystems.GetYarderProcessorAndLoaderCost(yarder, loaderSMhPerHa);
            this.SystemCostPerHaWithYarder = systemCostOffLanding + yarderLandingCostPerHa;

            // yoader
            float yoaderLandingCostPerHa = harvestSystems.GetYarderProcessorAndLoaderCost(yoader, loaderSMhPerHa);
            this.SystemCostPerHaWithYoader = systemCostOffLanding + yoaderLandingCostPerHa;

            Debug.Assert((this.ChainsawMinimumCost >= 0.0F) && (haulCostPerHa >= 0.0F) && (machineMoveInAndOutPerHa > 0.0F));
            Debug.Assert((systemCostOffLanding >= 0.0F) && (yarderLandingCostPerHa >= 0.0F) && (yoaderLandingCostPerHa >= 0.0F));
            Debug.Assert((Single.IsNaN(this.SystemCostPerHaWithYarder) == false) && (this.SystemCostPerHaWithYarder >= 0.0F) && (this.SystemCostPerHaWithYarder < 210.0F * 1000.0F));
            Debug.Assert((Single.IsNaN(this.SystemCostPerHaWithYoader) == false) && (this.SystemCostPerHaWithYoader >= 0.0F) && (this.SystemCostPerHaWithYoader < 220.0F * 1000.0F));
        }

        public new void Clear()
        {
            // avoid reset of crew type and falling flag
            base.ClearVolumeAndPMh();

            this.ChainsawProductivity = Single.NaN;
            this.LoadedWeightPerHa = 0.0F;
            this.SystemCostPerHaWithYarder = Single.NaN;
            this.SystemCostPerHaWithYoader = Single.NaN;
            this.YardedWeightPerHa = 0.0F;
        }
    }
}
