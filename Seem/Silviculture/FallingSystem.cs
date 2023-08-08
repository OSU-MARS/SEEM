using Mars.Seem.Tree;
using System.Diagnostics;
using System;

namespace Mars.Seem.Silviculture
{
    public class FallingSystem
    {
        public float ChainsawBasalAreaPerHa { get; protected set; } // m²/ha
        public ChainsawCrewType ChainsawCrew { get; protected set; }
        public float ChainsawCubicVolumePerHa { get; protected set; } // m³/ha
        public bool ChainsawFalling { get; protected set; } // versus only bucking after a machine
        public float ChainsawPMhPerHa { get; protected set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ChainsawUtilization { get; protected set; } // fraction between 1 and 0
        public float ChainsawMinimumCost { get; private set; } // US$/ha

        public FallingSystem()
        {
            this.Clear();
        }

        public void AddChainsawTimeOnTree(float chainsawPMsPerHa, float treeBasalAreaInM2PerHa, float treeMerchantableVolumeInM3PerHa)
        {
            this.ChainsawBasalAreaPerHa += treeBasalAreaInM2PerHa;
            this.ChainsawCubicVolumePerHa += treeMerchantableVolumeInM3PerHa;
            this.ChainsawPMhPerHa += chainsawPMsPerHa;
        }

        // slope and productivity for faller crew or a bucker
        protected void AdjustChainsawForSlopeAndSetProductivity(Stand stand, HarvestSystems harvestSystems)
        {
            this.ChainsawMinimumCost = 0.0F;
            if (this.ChainsawPMhPerHa > 0.0F)
            {
                float slopeInPercent = stand.SlopeInPercent;
                if (slopeInPercent > harvestSystems.ChainsawSlopeThresholdInPercent)
                {
                    this.ChainsawPMhPerHa *= 1.0F + harvestSystems.ChainsawSlopeLinear * (slopeInPercent - harvestSystems.ChainsawSlopeThresholdInPercent);
                }
                this.ChainsawPMhPerHa /= Constant.SecondsPerHour;

                float chainsawCrewCost;
                float chainsawCrewUtilization;
                if (this.ChainsawFalling)
                {
                    chainsawCrewCost = harvestSystems.ChainsawFellAndBuckCostPerSMh;
                    chainsawCrewUtilization = harvestSystems.ChainsawFellAndBuckUtilization;
                }
                else
                {
                    chainsawCrewCost = harvestSystems.ChainsawBuckCostPerSMh;
                    chainsawCrewUtilization = harvestSystems.ChainsawBuckUtilization;
                }
                chainsawCrewUtilization *= MathF.Min(this.ChainsawBasalAreaPerHa / Constant.HarvestCost.ChainsawBasalAreaPerHaForFullUtilization, 1.0F);
                chainsawCrewCost *= this.ChainsawPMhPerHa / chainsawCrewUtilization;

                this.ChainsawMinimumCost = chainsawCrewCost;
                this.ChainsawCrew = this.ChainsawFalling ? ChainsawCrewType.Fallers : ChainsawCrewType.Bucker;
                this.ChainsawUtilization = chainsawCrewUtilization;

                Debug.Assert((this.ChainsawBasalAreaPerHa > 0.0F) && (this.ChainsawBasalAreaPerHa < 10000.0F) &&
                             (this.ChainsawUtilization >= 0.0F) && (this.ChainsawUtilization <= 1.0F) &&
                             (this.ChainsawMinimumCost >= 0.0F));
            }
        }

        // slope and productivity for faller crew, bucker, or felling machine operator
        public void AdjustChainsawForSlopeAndSetProductivity(Stand stand, HarvestSystems harvestSystems, float fellingMachineCostPerSMh)
        {
            this.ChainsawMinimumCost = 0.0F;
            if (this.ChainsawPMhPerHa > 0.0F)
            {
                // faller crew or bucker
                this.AdjustChainsawForSlopeAndSetProductivity(stand, harvestSystems);

                // operator periodically halts machine for a small amount of chainsaw work
                float chainsawByOperatorSMh = this.ChainsawPMhPerHa / harvestSystems.ChainsawByOperatorUtilization; // SMh
                float chainsawByOperatorCost = (fellingMachineCostPerSMh + harvestSystems.ChainsawByOperatorCostPerSMh) * chainsawByOperatorSMh;
                if (chainsawByOperatorCost < this.ChainsawMinimumCost)
                {
                    this.ChainsawMinimumCost = chainsawByOperatorCost;
                    this.ChainsawCrew = ChainsawCrewType.Operator;
                    this.ChainsawUtilization = harvestSystems.ChainsawByOperatorUtilization;
                    Debug.Assert(this.ChainsawMinimumCost >= 0.0F);
                }
            }
        }

        public void Clear()
        {
            this.ChainsawCrew = ChainsawCrewType.None;
            this.ChainsawFalling = false;
            // split clear API so that crew type and falling flag aren't reset for harvest systems relying on hand falling
            this.ClearVolumeAndPMh();
        }

        protected void ClearVolumeAndPMh()
        {
            this.ChainsawBasalAreaPerHa = 0.0F;
            this.ChainsawCubicVolumePerHa = 0.0F;
            this.ChainsawPMhPerHa = 0.0F;
            this.ChainsawUtilization = 0.0F;
            this.ChainsawMinimumCost = Single.NaN;
        }
    }
}
