using Mars.Seem.Extensions;
using System;

namespace Mars.Seem.Tree
{
    internal class RedAlder
    {
        public static TreeSpeciesProperties Properties { get; private set; }

        static RedAlder()
        {
            // Miles PD, Smith BW. 2009. Specific gravity and other properties of wood and bark for 156 tree species found in North
            //   America (No. NRS-RN-38). Northern Research Station, US Forest Service. https://doi.org/10.2737/NRS-RN-38
            RedAlder.Properties = new TreeSpeciesProperties(greenWoodDensity: 737.0F, // kg/m³
                barkFraction: 0.12F,
                barkDensity: 977.0F, // kg/m³
                processingBarkLoss: 0.20F, // loss with spiked feed rollers
                yardingBarkLoss: 0.10F); // dragging abrasion loss over full corridor (if needed, this could be reparameterized to a function of corridor length)
        }

        /// <summary>
        /// Estimate red alder site index from conifer site index
        /// </summary>
        /// <param name="coniferSiteIndex">Conifer site index from ground.</param>
        /// <returns>Red alder site index from ground?</returns>
        public static float ConiferToRedAlderSiteIndex(float coniferSiteIndex)
        {
            return 9.73F + 0.64516F * coniferSiteIndex;
        }

        /// <summary>
        /// Hibbs D, Bluhm A, Garber S. 2007. Stem Taper and Volume of Managed Red Alder. Western Journal of Applied Forestry 22(1): 61–66. https://doi.org/10.1093/wjaf/22.1.61
        /// </summary>
        public static float GetDiameterInsideBark(float dbhInCm, float heightInM, float evaluationHeightInM)
        {
            float dbhInInches = Constant.InchesPerCentimeter * dbhInCm;
            float heightInFeet = Constant.FeetPerMeter * heightInM;
            float relativeHeight = evaluationHeightInM / heightInM; // Z
            float X = (1.0F - MathF.Sqrt(relativeHeight)) / (1.0F - MathF.Sqrt(4.5F / heightInFeet));
            float diameterInsideBarkInInches = 0.8995F * MathV.Pow(dbhInInches, 1.0205F) * MathV.Pow(X, 0.2631F * (1.364409F * MathV.Pow(dbhInInches, 1.0F / 3.0F) * MathV.Exp(-18.8990F * relativeHeight) + MathV.Exp(4.2549F * MathV.Pow(dbhInInches / heightInFeet, 0.6221F) * relativeHeight)));
            float diameterInsideBarkInCm = Constant.CentimetersPerInch * diameterInsideBarkInInches;
            return diameterInsideBarkInCm;
        }

        internal static float GetNeiloidHeight(float dbhInCm, float heightInM)
        {
            // approximation from plotting families of Hibbs et al. 2007 dib curves in R and fitting the neiloid inflection point
            // from linear regressions in RedAlder.R
            float heightDiameterRatio = heightInM / (0.01F * dbhInCm);
            float neiloidHeightInM = -0.22F + 1.0F / (0.025F * heightDiameterRatio) + 0.01F * (0.1F + 0.084F * heightDiameterRatio) * dbhInCm;
            return MathF.Max(neiloidHeightInM, Constant.Bucking.DefaultStumpHeightInM);
        }

        private static float GetGrowthEffectiveAgeWeiskittel(float height, float siteIndexUncorrected)
        {
            // red alder growth effective age equation based on H40 from Weiskittel et al. 2009's dominant height growth equation
            // Weiskittel AR, Hann DW, Hibbs DE, Lam TY, Bluhm A. 2009. Modeling top height growth of red alder plantations. Forest Ecology and
            //   Managment 258(3):323-331. https://doi.org/10.1016/j.foreco.2009.04.029
            float B1 = -4.481266F;
            float B2 = -0.658884F;
            float X = (1.0F / B1) * MathV.Ln(height / siteIndexUncorrected) + MathV.Exp2(4.3219280949F * B2); // MathV.Exp2(4.3219280949F, B2) = MathF.Pow(20.0F, B2)
            if (X < 0.03F)
            {
                X = 0.03F;
            }
            float growthEffectiveAge = MathV.Pow(X, 1.0F / B2);
            return growthEffectiveAge;
        }

        public static float GetGrowthEffectiveAgeWorthington(float H, float SI)
        {
            // Red alder growth effective age based on H40 equation from
            // Worthington NP, Johnson FA, Staebler GR, and Lloyd WJ. 1960. Normal Yield Tables for Red Alder. PNW Research Paper 36, Pacific Northwest Forest and Range Experiment Station.
            return 19.538F * H / (SI - 0.60924F * H);
        }

        public static float GetH40Weiskittel(float H40M, float TAGEM, float TAGEP)
        {
            // Weiskittel et al. 2009 dominant height growth equation for red alder
            float B1 = -4.481266F;
            float B2 = -0.658884F;
            float PH40P = H40M * MathV.Exp(B1 * (MathV.Pow(TAGEP, B2) - MathV.Pow(TAGEM, B2)));
            return PH40P;
        }

        public static float GetH50Worthington(float growthEffectiveAge, float siteIndex)
        {
            // Worthington et al. 1960, inverse of GetSiteIndex()
            return siteIndex / (0.60924F + 19.538F / growthEffectiveAge);
        }

        public static void GetPotentialHeightGrowthWeiskittel(float siteIndexCorrected, float PDEN, float currentHeight, float GP, out float growthEffectiveAge, out float potentialHeightGrowth)
        {
            // removes density impact on Weiskittel et al. 2009's site index
            float siteIndexUncorrected = siteIndexCorrected * (1.0F - 0.326480904F * MathV.Exp(-0.000400268678F * MathF.Pow(PDEN, 1.5F)));

            // Weiskittel et al. 2009 dominant height growth equation for red alder
            growthEffectiveAge = RedAlder.GetGrowthEffectiveAgeWeiskittel(currentHeight, siteIndexUncorrected);
            float A = growthEffectiveAge + GP;
            float potentialHeight = GetH40Weiskittel(currentHeight, growthEffectiveAge, A);
            potentialHeightGrowth = potentialHeight - currentHeight;
        }

        public static float GetSiteIndexWorthington(float tallestHeight, float growthEffectiveAge)
        {
            // Worthington et al. 1960
            return (0.60924F + 19.538F / growthEffectiveAge) * tallestHeight;
        }

        public static void ReduceExpansionFactor(Trees redAlders, float growthEffectiveAge, float treesPerAcre, float[] PMK)
        {
            if (redAlders.Species != FiaCode.AlnusRubra)
            {
                throw new ArgumentOutOfRangeException(nameof(redAlders));
            }

            float RAQMDN1 = 3.313F + 0.18769F * growthEffectiveAge - 0.000198F * growthEffectiveAge * growthEffectiveAge;
            float RABAN1 = -26.1467F + 5.31482F * growthEffectiveAge - 0.037466F * growthEffectiveAge * growthEffectiveAge;
            float RAQMDN2 = 3.313F + 0.18769F * (growthEffectiveAge + 5.0F) - 0.000198F * (growthEffectiveAge + 5.0F) * (growthEffectiveAge + 5.0F);
            float RABAN2 = -26.1467F + 5.31482F * (growthEffectiveAge + 5.0F) - 0.037466F * (growthEffectiveAge + 5.0F) * (growthEffectiveAge + 5.0F);
            float RATPAN1 = RABAN1 / (Constant.ForestersEnglish * RAQMDN1 * RAQMDN1);
            float RATPAN2 = RABAN2 / (Constant.ForestersEnglish * RAQMDN2 * RAQMDN2);
            if ((RATPAN1 < 0.0F) || (RATPAN2 < 0.0F))
            {
                for (int alderIndex = 0; alderIndex < redAlders.Count; ++alderIndex)
                {
                    // TODO: why a 100% stand mortality case?
                    redAlders.DeadExpansionFactor[alderIndex] += redAlders.LiveExpansionFactor[alderIndex];
                    redAlders.LiveExpansionFactor[alderIndex] = 0.0F;
                }
                return;
            }

            float RAMORT1 = 0.0F;
            for (int alderIndex = 0; alderIndex < redAlders.Count; ++alderIndex)
            {
                // TODO: avoid exp() for large values of PMK
                float PM = 1.0F / (1.0F + MathV.Exp(-PMK[alderIndex]));
                RAMORT1 += PM * redAlders.LiveExpansionFactor[alderIndex];
            }
            float RAMORT2 = treesPerAcre * (1.0F - RATPAN2 / RATPAN1);

            if (RAMORT1 < RAMORT2)
            {
                float KR1 = 0.0F;
                for (int KK = 0; KK < 7; ++KK)
                {
                    // TODO: avoid exp() by calculating iteratively
                    float NK = 10.0F / MathV.Exp10(KK);
                kr1: KR1 += NK;
                    RAMORT1 = 0.0F;
                    for (int alderIndex = 0; alderIndex < redAlders.Count; ++alderIndex)
                    {
                        // TODO: avoid exp() for large values of PMK
                        float PM = 1.0F / (1.0F + MathV.Exp(-(KR1 + PMK[alderIndex])));
                        RAMORT1 += PM * redAlders.LiveExpansionFactor[alderIndex];
                    }
                    if (RAMORT1 > RAMORT2)
                    {
                        KR1 -= NK;
                    }
                    else
                    {
                        goto kr1;
                    }
                }

                for (int alderIndex = 0; alderIndex < redAlders.Count; ++alderIndex)
                {
                    // TODO: pass previously applied mortality instead of performing fragile reconstruction from PMK
                    float previouslyAppliedSurvivalProbability = 1.0F - 1.0F / (1.0F + MathV.Exp(-PMK[alderIndex]));
                    float adjustedSurvivalProbability = 1.0F - 1.0F / (1.0F + MathV.Exp(-(KR1 + PMK[alderIndex])));
                    float revisedLiveExpansionFactor = adjustedSurvivalProbability / previouslyAppliedSurvivalProbability * redAlders.LiveExpansionFactor[alderIndex];
                    float newlyDeadExpansionFactor = redAlders.LiveExpansionFactor[alderIndex] - revisedLiveExpansionFactor;

                    redAlders.DeadExpansionFactor[alderIndex] += newlyDeadExpansionFactor;
                    redAlders.LiveExpansionFactor[alderIndex] = revisedLiveExpansionFactor;
                }
            }
        }
    }
}
