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

        public float DiscountRate { get; set; }
        public float DouglasFir2SawPondValuePerMbf { get; set; }
        public float DouglasFir3SawPondValuePerMbf { get; set; }
        public float DouglasFir4SawPondValuePerMbf { get; set; }
        public float FixedRegenerationHarvestCostPerHectare { get; set; }
        public float FixedReforestationCostPerHectare { get; set; }
        public float FixedThinningCostPerHectare { get; set; }
        public float RegenerationHarvestCostPerMbf { get; set; }
        public float SeedlingCost { get; set; }
        public float TaxesAndManagementPerHectareYear { get; set; }
        public float ThinningCostPerMbf { get; set; }
        public float TimberAppreciationRate { get; set; }

        public ScaledVolume ScaledVolumeRegenerationHarvest { get; private init; }
        public ScaledVolume ScaledVolumeThinning { get; private init; }

        static TimberValue()
        {
            TimberValue.Default = new TimberValue(false);
        }

        public TimberValue(bool scribnerFromLumberRecovery)
        {
            // all defaults in US$, nominal values from FOR 469 appraisal of land under Oregon's standard forestland program
            // Nominal Douglas-fir pond value from 
            //   1) pre-coronavirus bids on the Oregon Department of Forestry's West Oregon District in early 2020
            //   2) mean values in Washington Department of Natural Resources log price reports
            // Somewhat different calculations apply for lands enrolled in the small tract forestland program.
            this.DiscountRate = 0.04F; // per year
            //this.DouglasFirSpecialMillPondValuePerMbf = 675.50; // US$/MBF special mill and better, WA DNR coast region median monthly mean delivered price October 2011-January 2021
            this.DouglasFir2SawPondValuePerMbf = 605.50F; // US$/MBF 2S
            this.DouglasFir3SawPondValuePerMbf = 591.00F; // US$/MBF 3S
            this.DouglasFir4SawPondValuePerMbf = 505.00F; // US$/MBF 4S/CNS
            this.FixedReforestationCostPerHectare = Constant.AcresPerHectare * (136.0F + 154.0F + 39.0F); // US$/ha: site prep + planting labor + one release spray, cost of seedlings not included
            this.FixedRegenerationHarvestCostPerHectare = Constant.AcresPerHectare * 100.0F; // US$/ha
            this.FixedThinningCostPerHectare = Constant.AcresPerHectare * 60.0F; // US$/ha
            this.RegenerationHarvestCostPerMbf = 250.0F; // US$/MBF, includes forest products havest tax
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

            this.ScaledVolumeRegenerationHarvest = new ScaledVolume(Constant.Bucking.LogLengthRegenerationHarvest, scribnerFromLumberRecovery);
            this.ScaledVolumeThinning = new ScaledVolume(Constant.Bucking.LogLengthThinning, scribnerFromLumberRecovery);
        }

        public float GetDiscountFactor(int years)
        {
            return 1.0F / MathF.Pow(1.0F + this.DiscountRate, years);
        }

        public float GetNetPresentRegenerationHarvestValue(float volumeInMbfPerHectare, int rotationLengthInYears)
        {
            if (volumeInMbfPerHectare < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeInMbfPerHectare));
            }

            float unappreciatedPricePerMbf;
            if ((rotationLengthInYears >= 20) && (rotationLengthInYears < 50))
            {
                unappreciatedPricePerMbf = this.douglasFirFinal2045PondValueIntercept + this.douglasFirFinal2045PondValueSlope * rotationLengthInYears;
            }
            else if (rotationLengthInYears <= 75)
            {
                unappreciatedPricePerMbf = this.douglasFirFinal5075PondValueIntercept + this.douglasFirFinal5075PondValueSlope * rotationLengthInYears;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(rotationLengthInYears));
            }

            float appreciatedPricePerMbf = unappreciatedPricePerMbf * MathF.Pow(1.0F + this.TimberAppreciationRate, rotationLengthInYears);
            float netFutureValue = volumeInMbfPerHectare * (appreciatedPricePerMbf - this.RegenerationHarvestCostPerMbf) - this.FixedRegenerationHarvestCostPerHectare;
            float discountFactor = 1.0F / MathF.Pow(1.0F + this.DiscountRate, rotationLengthInYears);
            return discountFactor * netFutureValue;
        }

        public float GetNetPresentRegenerationHarvestValue(float net2sawMbfPerHectare, float net3sawMbfPerHectare, float net4sawMbfPerHectare, int harvestAgeInYears)
        {
            float appreciationFactor = this.GetTimberAppreciationFactor(harvestAgeInYears);
            float netValuePerHectareAtHarvest = (appreciationFactor * this.DouglasFir2SawPondValuePerMbf - this.RegenerationHarvestCostPerMbf) * net2sawMbfPerHectare +
                                                (appreciationFactor * this.DouglasFir3SawPondValuePerMbf - this.RegenerationHarvestCostPerMbf) * net3sawMbfPerHectare +
                                                (appreciationFactor * this.DouglasFir4SawPondValuePerMbf - this.RegenerationHarvestCostPerMbf) * net4sawMbfPerHectare -
                                                this.FixedRegenerationHarvestCostPerHectare;
            float discountFactor = this.GetDiscountFactor(harvestAgeInYears);
            return discountFactor * netValuePerHectareAtHarvest;
        }

        public float GetNetPresentThinningValue(float volumeInMbfPerHectare, int thinningAgeInYears)
        {
            if (volumeInMbfPerHectare < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeInMbfPerHectare));
            }
            if ((thinningAgeInYears < 30) || (thinningAgeInYears > 45))
            {
                throw new ArgumentOutOfRangeException(nameof(thinningAgeInYears));
            }

            float unappreciatedPricePerMbf = this.douglasFirThinPondValueIntercept + this.douglasFirThinPondValueSlope * thinningAgeInYears;
            float appreciatedPricePerMbf = unappreciatedPricePerMbf * MathF.Pow(1.0F + this.TimberAppreciationRate, thinningAgeInYears);
            float netFutureValue = volumeInMbfPerHectare * (appreciatedPricePerMbf - this.ThinningCostPerMbf) - this.FixedThinningCostPerHectare;
            float discountFactor = 1.0F / MathF.Pow(1.0F + this.DiscountRate, thinningAgeInYears);
            return discountFactor * netFutureValue;
        }

        public float GetNetPresentThinningValue(float net2sawMbfPerHectare, float net3sawMbfPerHectare, float net4sawMbfPerHectare, int thinningAgeInYears)
        {
            float appreciationFactor = this.GetTimberAppreciationFactor(thinningAgeInYears);
            float netValuePerHectareAtHarvest = (appreciationFactor * this.DouglasFir2SawPondValuePerMbf - this.ThinningCostPerMbf) * net2sawMbfPerHectare +
                                                (appreciationFactor * this.DouglasFir3SawPondValuePerMbf - this.ThinningCostPerMbf) * net3sawMbfPerHectare +
                                                (appreciationFactor * this.DouglasFir4SawPondValuePerMbf - this.ThinningCostPerMbf) * net4sawMbfPerHectare -
                                                this.FixedThinningCostPerHectare;
            float discountFactor = this.GetDiscountFactor(thinningAgeInYears);
            return discountFactor * netValuePerHectareAtHarvest;
        }

        public float GetTimberAppreciationFactor(int years)
        {
            return MathF.Pow(1.0F + this.TimberAppreciationRate, years);
        }

        public float ToLandExpectationValue(float firstRotationNetPresentValue, int rotationLengthInYears)
        {
            float appreciationFactor = MathF.Pow(1.0F + this.DiscountRate, rotationLengthInYears);
            float firstRotationFutureValue = appreciationFactor * firstRotationNetPresentValue;
            float landExpectationValue = firstRotationFutureValue / (appreciationFactor - 1.0F) - this.TaxesAndManagementPerHectareYear / this.DiscountRate;
            return landExpectationValue;
        }
    }
}
