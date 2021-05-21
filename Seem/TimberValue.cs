using System;

namespace Osu.Cof.Ferm
{
    public class TimberValue
    {
        private readonly float douglasFirFinal2045PondValueIntercept;
        private readonly float douglasFirFinal2045PondValueSlope;
        private readonly float douglasFirFinal5075PondValueIntercept;
        private readonly float douglasFirFinal5075PondValueSlope;
        private readonly float douglasFirThinPondValueIntercept;
        private readonly float douglasFirThinPondValueSlope;

        public static TimberValue Default { get; private set; }

        public float DouglasFir2SawPondValuePerMbf { get; set; }
        public float DouglasFir3SawPondValuePerMbf { get; set; }
        public float DouglasFir4SawPondValuePerMbf { get; set; }
        public float FixedRegenerationHarvestCostPerHectare { get; set; }
        public float FixedSitePrepAndReplantingCostPerHectare { get; set; }
        public float FixedThinningCostPerHectare { get; set; }
        public float ReleaseSprayCostPerHectare { get; set; }
        public float RegenerationHarvestCostPerMbf { get; set; }
        public float SeedlingCost { get; set; }
        public float TaxesAndManagementPerHectareYear { get; set; }
        public float ThinningCostPerMbf { get; set; }
        public float TimberAppreciationRate { get; set; }

        public ScaledVolume ScaledVolumeRegenerationHarvest { get; private init; }
        public ScaledVolume ScaledVolumeThinning { get; private init; }

        static TimberValue()
        {
            TimberValue.Default = new TimberValue(Constant.Bucking.DefaultMaximumDiameterInCentimeters, Constant.Bucking.DefaultMaximumHeightInMeters, false);
        }

        public TimberValue(float maximumDiameterInCentimeters, float maximumHeightInMeters, bool scribnerFromLumberRecovery)
        {
            // all defaults in US$, nominal values from FOR 469 appraisal of land under Oregon's standard forestland program
            // Nominal Douglas-fir pond value from 
            //   1) pre-coronavirus bids on the Oregon Department of Forestry's West Oregon District in early 2020
            //   2) mean values in Washington Department of Natural Resources log price reports
            // Somewhat different calculations apply for lands enrolled in the small tract forestland program.
            //this.DouglasFirSpecialMillPondValuePerMbf = 675.50; // US$/MBF special mill and better, WA DNR coast region median monthly mean delivered price October 2011-January 2021
            this.DouglasFir2SawPondValuePerMbf = 605.50F; // US$/MBF 2S
            this.DouglasFir3SawPondValuePerMbf = 591.00F; // US$/MBF 3S
            this.DouglasFir4SawPondValuePerMbf = 505.00F; // US$/MBF 4S/CNS
            this.FixedRegenerationHarvestCostPerHectare = Constant.AcresPerHectare * 100.0F; // US$/ha
            this.FixedSitePrepAndReplantingCostPerHectare = Constant.AcresPerHectare * (136.0F + 154.0F); // US$/ha: site prep + planting labor, cost of seedlings not included
            this.FixedThinningCostPerHectare = Constant.AcresPerHectare * 60.0F; // US$/ha
            this.RegenerationHarvestCostPerMbf = 250.0F; // US$/MBF, includes forest products havest tax
            this.ReleaseSprayCostPerHectare = Constant.AcresPerHectare * 39.0F; // US$/ha, one release spray
            this.SeedlingCost = 0.50F; // US$ per seedling
            this.TaxesAndManagementPerHectareYear = Constant.AcresPerHectare * 7.5F; // US$/ha-year, mean western Oregon forest land tax of $3.40/acre in 2006 plus nominal management expense
            this.ThinningCostPerMbf = 275.0F; // US$/MBF, includes forest products harvest tax
            this.TimberAppreciationRate = 0.01F; // per year
            // first order model from Malcolm Knapp volume regression in R: WA DNR coast region median monthly mean delivered price October 2011-November 2020
            this.douglasFirFinal2045PondValueIntercept = 513.2525F;
            this.douglasFirFinal2045PondValueSlope = 1.443416F;
            this.douglasFirFinal5075PondValueIntercept = 567.5667F;
            this.douglasFirFinal5075PondValueSlope = 0.3645541F;
            this.douglasFirThinPondValueIntercept = 453.6676F;
            this.douglasFirThinPondValueSlope = 2.775607F;

            this.ScaledVolumeRegenerationHarvest = new ScaledVolume(maximumDiameterInCentimeters, maximumHeightInMeters, Constant.Bucking.LogLengthRegenerationHarvest, scribnerFromLumberRecovery);
            this.ScaledVolumeThinning = new ScaledVolume(maximumDiameterInCentimeters, maximumHeightInMeters, Constant.Bucking.LogLengthThinning, scribnerFromLumberRecovery);
        }

        public TimberValue(TimberValue other)
        {
            this.douglasFirFinal2045PondValueIntercept = other.douglasFirFinal2045PondValueIntercept;
            this.douglasFirFinal2045PondValueSlope = other.douglasFirFinal2045PondValueSlope;
            this.douglasFirFinal5075PondValueIntercept = other.douglasFirFinal5075PondValueIntercept;
            this.douglasFirFinal5075PondValueSlope = other.douglasFirFinal5075PondValueSlope;
            this.douglasFirThinPondValueIntercept = other.douglasFirThinPondValueIntercept;
            this.douglasFirThinPondValueSlope = other.douglasFirThinPondValueSlope;

            this.DouglasFir2SawPondValuePerMbf = other.DouglasFir2SawPondValuePerMbf;
            this.DouglasFir3SawPondValuePerMbf = other.DouglasFir3SawPondValuePerMbf;
            this.DouglasFir4SawPondValuePerMbf = other.DouglasFir4SawPondValuePerMbf;
            this.FixedRegenerationHarvestCostPerHectare = other.FixedRegenerationHarvestCostPerHectare;
            this.FixedSitePrepAndReplantingCostPerHectare = other.FixedSitePrepAndReplantingCostPerHectare;
            this.FixedThinningCostPerHectare = other.FixedThinningCostPerHectare;
            this.RegenerationHarvestCostPerMbf = other.RegenerationHarvestCostPerMbf;
            this.ReleaseSprayCostPerHectare = other.ReleaseSprayCostPerHectare;
            this.SeedlingCost = other.SeedlingCost;
            this.TaxesAndManagementPerHectareYear = other.TaxesAndManagementPerHectareYear;
            this.ThinningCostPerMbf = other.ThinningCostPerMbf;
            this.TimberAppreciationRate = other.TimberAppreciationRate;

            // for now, assume shallow copy of volume tables is acceptable
            this.ScaledVolumeRegenerationHarvest = other.ScaledVolumeRegenerationHarvest;
            this.ScaledVolumeThinning = other.ScaledVolumeThinning;
        }

        public static float GetAppreciationFactor(float discountRate, int years)
        {
            return MathF.Pow(1.0F + discountRate, years);
        }

        public static float GetDiscountFactor(float discountRate, int years)
        {
            return 1.0F / MathF.Pow(1.0F + discountRate, years);
        }

        public float GetNetPresentReforestationValue(float discountRate, float plantingDensityInTreesPerHectare)
        {
            // amoritzed reforestation expenses under 26 USC § 194 https://www.law.cornell.edu/uscode/text/26/194
            // NPV = C0 * (-1 + 1/7 * (0.5 / (1 + r) + 1 / (1 + r)^2 + ... + 1 / (1 + r)^7 + 0.5 / (1 + r)^8))
            //     = C0 * (-1 + 1/7 * (1 / (1 + r) * (0.5 + 1 / (1 + r)^1 + ... + 1 / (1 + r)^6 + 0.5 / (1 + r)^8)))
            //     = C0 * (-1 + 1/7 * (1 / (1 + r) * (0.5 + 1 / (1 + r) * (1 + ... + 1 / (1 + r)^6 + 0.5 / (1 + r)^8)))
            //     = C0 * (-1 + 1/7 * (1 / (1 + r) * (0.5 + 1 / (1 + r) * (1 + 1 / (1 + r) * (1 + ... ))))
            float annualDiscountFactor = 1.0F / (1.0F + discountRate);
            float amortizationFactor = -1.0F + 1.0F / 7.0F * (annualDiscountFactor * (0.5F + // amoritzation year 1
                                                              annualDiscountFactor * (1.0F + // year 2
                                                              annualDiscountFactor * (1.0F + // year 3
                                                              annualDiscountFactor * (1.0F + // year 4
                                                              annualDiscountFactor * (1.0F + // year 5
                                                              annualDiscountFactor * (1.0F + // year 6
                                                              annualDiscountFactor * (1.0F + // year 7
                                                              annualDiscountFactor * 0.5F)))))))); // year 8
            // float amortizationFactor = -1.0F; // disable amoritzation
            float replantingCost = this.FixedSitePrepAndReplantingCostPerHectare + this.SeedlingCost * plantingDensityInTreesPerHectare;
            float reforestationNpv = replantingCost * amortizationFactor;

            // 26 USC § 194(c)(3) defines reforestation expenses as site prep and planting, release sprays are therefore excluded
            float releaseSprayCost = this.ReleaseSprayCostPerHectare * annualDiscountFactor * annualDiscountFactor;
            // releaseSprayCost = this.ReleaseSprayCostPerHectare; // disable discounting
            reforestationNpv -= releaseSprayCost;

            return reforestationNpv;
        }

        public float GetNetPresentRegenerationHarvestValue(StandScribnerVolume standingVolume, float discountRate, int periodIndex, int harvestAgeInYears, out float npv2Saw, out float npv3Saw, out float npv4Saw)
        {
            // TODO: support multiple species
            float appreciationFactor = this.GetTimberAppreciationFactor(harvestAgeInYears);
            npv2Saw = (appreciationFactor * this.DouglasFir2SawPondValuePerMbf - this.RegenerationHarvestCostPerMbf) * standingVolume.Scribner2Saw[periodIndex];
            npv3Saw = (appreciationFactor * this.DouglasFir3SawPondValuePerMbf - this.RegenerationHarvestCostPerMbf) * standingVolume.Scribner3Saw[periodIndex];
            npv4Saw = (appreciationFactor * this.DouglasFir4SawPondValuePerMbf - this.RegenerationHarvestCostPerMbf) * standingVolume.Scribner4Saw[periodIndex];

            float netValuePerHectareAtHarvest = npv2Saw + npv3Saw + npv4Saw - this.FixedRegenerationHarvestCostPerHectare;
            float discountFactor = TimberValue.GetDiscountFactor(discountRate, harvestAgeInYears);
            return discountFactor * netValuePerHectareAtHarvest - this.TaxesAndManagementPerHectareYear * (1.0F - discountFactor) / discountRate;
        }

        public float GetNetPresentThinningValue(StandScribnerVolume harvestVolume, float discountRate, int periodIndex, int thinningAgeInYears, out float npv2Saw, out float npv3Saw, out float npv4Saw)
        {
            float mbf2Saw = harvestVolume.Scribner2Saw[periodIndex];
            float mbf3Saw = harvestVolume.Scribner3Saw[periodIndex];
            float mbf4Saw = harvestVolume.Scribner4Saw[periodIndex];
            if (mbf2Saw + mbf3Saw + mbf4Saw == 0.0F)
            {
                // no thinning performed
                npv2Saw = 0.0F;
                npv3Saw = 0.0F;
                npv4Saw = 0.0F;
                return 0.0F;
            }

            float appreciationFactor = this.GetTimberAppreciationFactor(thinningAgeInYears);
            npv2Saw = (appreciationFactor * this.DouglasFir2SawPondValuePerMbf - this.ThinningCostPerMbf) * mbf2Saw;
            npv3Saw = (appreciationFactor * this.DouglasFir3SawPondValuePerMbf - this.ThinningCostPerMbf) * mbf3Saw;
            npv4Saw = (appreciationFactor * this.DouglasFir4SawPondValuePerMbf - this.ThinningCostPerMbf) * mbf4Saw;
            float netValuePerHectareAtHarvest = npv2Saw + npv3Saw + npv4Saw - this.FixedThinningCostPerHectare;
            float discountFactor = TimberValue.GetDiscountFactor(discountRate, thinningAgeInYears);
            return discountFactor * netValuePerHectareAtHarvest;
        }

        public float GetTimberAppreciationFactor(int years)
        {
            return MathF.Pow(1.0F + this.TimberAppreciationRate, years);
        }
    }
}
