using System;

namespace Osu.Cof.Ferm.Heuristics
{
    public class TimberValue
    {
        public float DiscountRate { get; set; }
        public float DouglasFirPricePerMbf { get; set; }
        public float FixedRegenerationHarvestCostPerAcre { get; set; }
        public float FixedThinningCostPerAcre { get; set; }
        public float TimberAppreciationRate { get; set; }
        public VolumeUnits VolumeUnits { get; set; }

        public TimberValue()
        {
            this.DiscountRate = 0.04F; // per year
            // from recent Oregon Department of Forestry bids on West Oregon District
            this.DouglasFirPricePerMbf = 450.0F; // US$
            // from FOR 469 timber appraisal
            this.FixedRegenerationHarvestCostPerAcre = 570.0F; // US$
            // rough approximation
            this.FixedThinningCostPerAcre = 100.0F; // US$
            this.TimberAppreciationRate = 0.01F; // per year
            this.VolumeUnits = VolumeUnits.CubicMetersPerHectare;
        }

        // returns $/acre
        public float GetPresentValueOfFinalHarvestScribner(float finalHarvestVolumeInBoardFeetPerAcre, int periodsFromPresent, int periodLengthInYears)
        {
            float yearsFromNow = periodLengthInYears * periodsFromPresent + 0.5F;
            float appreciatedPricePerMbf = this.DouglasFirPricePerMbf * MathF.Pow(1.0F + this.TimberAppreciationRate, yearsFromNow);
            float discountFactor = 1.0F / MathF.Pow(1.0F + this.DiscountRate, yearsFromNow);
            return discountFactor * (appreciatedPricePerMbf * 0.001F * finalHarvestVolumeInBoardFeetPerAcre - this.FixedRegenerationHarvestCostPerAcre);
        }

        // returns $/acre
        public float GetPresentValueOfThinScribner(float thinVolumeInBoardFeetPerAcre, int periodsFromPresent, int periodLengthInYears)
        {
            float yearsFromNow = periodLengthInYears * periodsFromPresent + 0.5F;
            float appreciatedPricePerMbf = this.DouglasFirPricePerMbf * MathF.Pow(1.0F + this.TimberAppreciationRate, yearsFromNow);
            float discountFactor = 1.0F / MathF.Pow(1.0F + this.DiscountRate, yearsFromNow);
            return discountFactor * (appreciatedPricePerMbf * 0.001F * thinVolumeInBoardFeetPerAcre - this.FixedThinningCostPerAcre);
        }
    }
}
