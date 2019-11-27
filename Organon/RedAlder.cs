using System;

namespace Osu.Cof.Organon
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

        public static void RAMORT(Stand stand, float RAAGE, float RAN, float[] PMK)
        {
            float KB = 0.005454154F;
            float RAMORT1 = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = stand.Species[treeIndex];
                if (species == FiaCode.AlnusRubra)
                {
                    float PM = 1.0F / (1.0F + (float)Math.Exp(-PMK[treeIndex]));
                    RAMORT1 += PM * stand.LiveExpansionFactor[treeIndex];
                }
            }

            float RAQMDN1 = 3.313F + 0.18769F * RAAGE - 0.000198F * RAAGE * RAAGE;
            float RABAN1 = -26.1467F + 5.31482F * RAAGE - 0.037466F * RAAGE * RAAGE;
            float RAQMDN2 = 3.313F + 0.18769F * (RAAGE + 5.0F) - 0.000198F * (float)Math.Pow(RAAGE + Constant.DefaultTimeStepInYears, 2.0);
            float RABAN2 = -26.1467F + 5.31482F * (RAAGE + 5.0F) - 0.037466F * (float)Math.Pow(RAAGE + Constant.DefaultTimeStepInYears, 2.0);
            float RATPAN1 = RABAN1 / (KB * RAQMDN1 * RAQMDN1);
            float RATPAN2 = RABAN2 / (KB * RAQMDN2 * RAQMDN2);
            float RAMORT2;
            if ((RATPAN1 > 0.0F) && (RATPAN2 > 0.0F))
            {
                RAMORT2 = RAN * (1.0F - RATPAN2 / RATPAN1);
            }
            else
            {
                for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                {
                    FiaCode species = stand.Species[treeIndex];
                    if (species == FiaCode.AlnusRubra)
                    {
                        PMK[treeIndex] = 1000.0F;
                    }
                }
                return;
            }

            if (RAMORT1 < RAMORT2)
            {
                float KR1 = 0.0F;
                for (int KK = 0; KK < 7; ++KK)
                {
                    float NK = 10.0F / (float)Math.Pow(10.0, KK);
                kr1: KR1 += NK;
                    RAMORT1 = 0.0F;
                    for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                    {
                        FiaCode species = stand.Species[treeIndex];
                        if (species == FiaCode.AlnusRubra)
                        {
                            float PM = 1.0F / (1.0F + (float)Math.Exp(-(KR1 + PMK[treeIndex])));
                            RAMORT1 += PM * stand.LiveExpansionFactor[treeIndex];
                        }
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
                for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                {
                    FiaCode species = stand.Species[treeIndex];
                    if (species == FiaCode.AlnusRubra)
                    {
                        PMK[treeIndex] = KR1 + PMK[treeIndex];
                    }
                }
            }
        }

        public static void WHHLB_GEA(float H, float SI_UC, out float GEA)
        {
            // RED ALDER GROWTH EFFECTIVE AGE EQUATION BASED ON H40 EQUATION FROM
            // THE WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM DOMINANT HEIGHT GROWTH EQUATION
            float B1 = -4.481266F;
            float B2 = -0.658884F;
            float X = (1.0F / B1) * (float)(Math.Log(H / SI_UC) + Math.Pow(20.0, B2));
            if (X < 0.03F)
            {
                X = 0.03F;
            }
            GEA = (float)Math.Pow(X, 1.0 / B2);
        }

        public static void WHHLB_H40(float H40M, float TAGEM, float TAGEP, out float PH40P)
        {
            // WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM DOMINANT HEIGHT GROWTH EQUATION FOR RED ALDER
            float B1 = -4.481266F;
            float B2 = -0.658884F;
            PH40P = H40M * (float)Math.Exp(B1 * (Math.Pow(TAGEP, B2) - Math.Pow(TAGEM, B2)));
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

        public static void WHHLB_SI_UC(float SI_C, float PDEN, out float SI_UC)
        {
            // UNCORRECTS THE DENSITY INPACT UPON THE WEISKITTEL, HANN, HIBBS, LAM, AND BLUHN SITE INDEX FOR RED ALDER
            // SITE INDEX UNCORRECTED FOR DENSITY EFFECT
            SI_UC = SI_C * (1.0F - 0.326480904F * (float)Math.Exp(-0.000400268678 * Math.Pow(PDEN, 1.5)));
        }
    }
}
