using Mars.Seem.Tree;
using System;

namespace Mars.Seem.Silviculture
{
    public class Fallers : FallingSystem
    {
        public float LoadedWeightPerHa { get; set; } // m³/ha
        public float SystemCostPerHa { get; private set; }
        public float YardedWeightPerHa { get; set; } // kg/ha

        public Fallers()
        {
            this.ChainsawCrew = ChainsawCrewType.Fallers;
            this.ChainsawFalling = true;

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

        public void CalculatePMhAndProductivity(Stand stand, HarvestSystems harvestSystems)
        {
            // passthrough to protected API as no felling machine is present
            this.AdjustChainsawForSlopeAndSetProductivity(stand, harvestSystems);
        }

        // TODO
        //public void CalculateSystemCost()
        //{
        //}

        public new void Clear()
        {
            // avoid reset of crew type and falling flag
            base.ClearVolumeAndPMh();

            this.LoadedWeightPerHa = 0.0F;
            this.SystemCostPerHa = Single.NaN;
            this.YardedWeightPerHa = 0.0F;
        }
    }
}
