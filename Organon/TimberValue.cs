using System;

namespace Osu.Cof.Ferm
{
    public class TimberValue
    {
        public float DiscountRate { get; set; }
        public float DouglasFirPondValuePerMbf { get; set; }
        public float FixedRegenerationHarvestCostPerHectare { get; set; }
        public float FixedThinningCostPerHectare { get; set; }
        public float ReforestationCostPerHectare { get; set; }
        public float RegenerationHarvestCostPerMbf { get; set; }
        public float TaxesAndManagementPerHectare { get; set; }
        public float ThinningCostPerMbf { get; set; }
        public float TimberAppreciationRate { get; set; }

        public TimberValue()
        {
            // all defaults in US$, nominal values from FOR 469 appraisal of land under Oregon's standard forestland program
            // Nominal Douglas-fir pond value from 
            //   1) pre-coronavirus bids on the Oregon Department of Forestry's West Oregon District in early 2020
            //   2) mean values in Washington Department of Natural Resources log price reports
            // Somewhat different calculations apply for lands enrolled in the small tract forestland program.
            this.DiscountRate = 0.04F; // per year
            this.DouglasFirPondValuePerMbf = 525.0F;
            this.FixedRegenerationHarvestCostPerHectare = Constant.AcresPerHectare * 100.0F;
            this.FixedThinningCostPerHectare = Constant.AcresPerHectare * 60.0F;
            this.ReforestationCostPerHectare = Constant.AcresPerHectare * 560.0F;
            this.RegenerationHarvestCostPerMbf = 250.0F; // includes forest products havest tax
            this.TaxesAndManagementPerHectare = Constant.AcresPerHectare * 7.5F; // per year, mean western Oregon forest land tax of $3.40/acre in 2006 plus nominal management expense
            this.ThinningCostPerMbf = 275.0F; // includes forest products harvest tax
            this.TimberAppreciationRate = 0.01F; // per year
        }

        public float GetNetPresentValueRegenerationHarvestScribner(float volumeInMbfPerHectare, int rotationLengthInYears)
        {
            float appreciatedPricePerMbf = this.DouglasFirPondValuePerMbf * MathF.Pow(1.0F + this.TimberAppreciationRate, rotationLengthInYears);
            float netFutureValue = volumeInMbfPerHectare * (appreciatedPricePerMbf - this.RegenerationHarvestCostPerMbf) - this.FixedRegenerationHarvestCostPerHectare;
            float discountFactor = 1.0F / MathF.Pow(1.0F + this.DiscountRate, rotationLengthInYears);
            return discountFactor * netFutureValue;
        }

        public float GetNetPresentValueThiningScribner(float volumeInMbfPerHectare, int thinningAgeInYears)
        {
            float appreciatedPricePerMbf = this.DouglasFirPondValuePerMbf * MathF.Pow(1.0F + this.TimberAppreciationRate, thinningAgeInYears);
            float netFutureValue = volumeInMbfPerHectare * (appreciatedPricePerMbf - this.ThinningCostPerMbf) - this.FixedThinningCostPerHectare;
            float discountFactor = 1.0F / MathF.Pow(1.0F + this.DiscountRate, thinningAgeInYears);
            return discountFactor * netFutureValue;
        }

        public float ToLandExpectationValue(float firstRotationNetPresentValue, int rotationLengthInYears)
        {
            float appreciationFactor = MathF.Pow(1.0F + this.DiscountRate, rotationLengthInYears);
            float firstRotationFutureValue = appreciationFactor * firstRotationNetPresentValue;
            float landExpectationValue = firstRotationFutureValue / (appreciationFactor - 1.0F) - this.TaxesAndManagementPerHectare / this.DiscountRate;
            return landExpectationValue;
        }
    }
}
