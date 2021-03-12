using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Species
{
    internal class RedAlder
    {
        /// <summary>
        /// Estimate red alder site index from conifer site index
        /// </summary>
        /// <param name="SITE_1">Conifer site index from ground.</param>
        /// <returns>Red alder site index from ground?</returns>
        public static float ConiferToRedAlderSiteIndex(float SITE_1)
        {
            return 9.73F + 0.64516F * SITE_1;
        }

        public static float GetGrowthEffectiveAge(float H, float SI)
        {
            // RED ALDER GROWTH EFFECTIVE AGE EQUATION BASED ON H40 EQUATION FROM
            // WORTHINGTON, JOHNSON, STAEBLER AND LLOYD(1960) PNW RESEARCH PAPER 36
            return 19.538F * H / (SI - 0.60924F * H);
        }

        public static float GetSiteIndex(float H, float A)
        {
            // RED ALDER SITE INDEX EQUATION FROM WORTHINGTON, JOHNSON, STAEBLER AND LLOYD(1960) PNW RESEARCH PAPER 36
            return (0.60924F + 19.538F / A) * H;
        }

        public static float GetH50(float A, float SI)
        {
            // RED ALDER H40 EQUATION FROM FROM WORTHINGTON, JOHNSON, STAEBLER AND LLOYD(1960) PNW RESEARCH PAPER 36
            return SI / (0.60924F + 19.538F / A);
        }

        public static void ReduceExpansionFactor(Trees redAlders, float RAAGE, float RAN, float[] PMK)
        {
            if (redAlders.Species != FiaCode.AlnusRubra)
            {
                throw new ArgumentOutOfRangeException(nameof(redAlders));
            }

            float RAQMDN1 = 3.313F + 0.18769F * RAAGE - 0.000198F * RAAGE * RAAGE;
            float RABAN1 = -26.1467F + 5.31482F * RAAGE - 0.037466F * RAAGE * RAAGE;
            float RAQMDN2 = 3.313F + 0.18769F * (RAAGE + 5.0F) - 0.000198F * (RAAGE + Constant.DefaultTimeStepInYears) * (RAAGE + Constant.DefaultTimeStepInYears);
            float RABAN2 = -26.1467F + 5.31482F * (RAAGE + 5.0F) - 0.037466F * (RAAGE + Constant.DefaultTimeStepInYears) * (RAAGE + Constant.DefaultTimeStepInYears);
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
            float RAMORT2 = RAN * (1.0F - RATPAN2 / RATPAN1);

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

        private static void WHHLB_GEA(float H, float SI_UC, out float GEA)
        {
            // RED ALDER GROWTH EFFECTIVE AGE EQUATION BASED ON H40 EQUATION FROM
            // THE WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM DOMINANT HEIGHT GROWTH EQUATION
            float B1 = -4.481266F;
            float B2 = -0.658884F;
            float X = (1.0F / B1) * MathV.Ln(H / SI_UC) + MathV.Exp2(4.3219280949F * B2); // MathV.Exp2(4.3219280949F, B2) = MathF.Pow(20.0F, B2)
            if (X < 0.03F)
            {
                X = 0.03F;
            }
            GEA = MathV.Pow(X, 1.0F / B2);
        }

        public static void WHHLB_H40(float H40M, float TAGEM, float TAGEP, out float PH40P)
        {
            // WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM DOMINANT HEIGHT GROWTH EQUATION FOR RED ALDER
            float B1 = -4.481266F;
            float B2 = -0.658884F;
            PH40P = H40M * MathV.Exp(B1 * (MathV.Pow(TAGEP, B2) - MathV.Pow(TAGEM, B2)));
        }

        public static void WHHLB_HG(float SI_C, float PDEN, float HT, float GP, out float GEA, out float POTHGRO)
        {
            // WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM DOMINANT HEIGHT GROWTH INCREMENT EQUATION FOR RED ALDER
            WHHLB_SI_UC(SI_C, PDEN, out float SI_UC);
            WHHLB_GEA(HT, SI_UC, out GEA);
            float A = GEA + GP;
            WHHLB_H40(HT, GEA, A, out float PHT);
            POTHGRO = PHT - HT;
        }

        private static void WHHLB_SI_UC(float SI_C, float PDEN, out float SI_UC)
        {
            // UNCORRECTS THE DENSITY INPACT UPON THE WEISKITTEL, HANN, HIBBS, LAM, AND BLUHN SITE INDEX FOR RED ALDER
            // SITE INDEX UNCORRECTED FOR DENSITY EFFECT
            SI_UC = SI_C * (1.0F - 0.326480904F * MathV.Exp(-0.000400268678F * MathF.Pow(PDEN, 1.5F)));
        }
    }
}
