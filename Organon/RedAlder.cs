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

        public static float RA_MH(float DBH, float HT, float CR, float TD)
        {
            if (TD <= 0.0F)
            {
                return HT;
            }

            float D0 = TAPER_RA(DBH, HT, CR, 0.0F);
            if (D0 <= TD)
            {
                return 0.0F;
            }

            int IHT = (int)(10.0F * HT) - 1;
            for (int I = 0; I < IHT; ++I)
            {
                float HI = HT - 0.1F * (float)I;
                float DI = TAPER_RA(DBH, HT, CR, HI);
                if (DI >= TD)
                {
                    return HI;
                }
            }
            return 0.0F;
        }

        public static float RA_SCRIB(int SVOL, int LOGLL, float LOGTD, float LOGSH, float LOGTA, float LOGML, float DOB, float HT, float CR)
        {
            // COMPUTE MERCHANTABLE HEIGHT
            float MH;
            if (LOGTD <= 0.0F)
            {
                MH = HT;
            }
            else
            {
                MH = RA_MH(DOB, HT, CR, LOGTD);
            }
            if (MH == 0.0F || MH <= LOGSH)
            {
                return 0.0F;
            }

            // CALCULATE LOG VOLUMES
            int NW = (int)MathF.Round((MH - LOGSH) / ((float)LOGLL + LOGTA));
            if (NW < 0)
            {
                NW = 0;
            }
            float TLL = MH - LOGSH - (float)NW * ((float)LOGLL + LOGTA);
            float D = 1.0F;
            float EX = 1.0F;
            float H = LOGSH;
            float VALU = 0;
            float[,] NL = new float[40, 4];
            float[,] LVOL = new float[40, 4];
            float[,] TOTS = new float[2, 4];
            for (int II = 0; II < NW; ++II)
            {
                H = H + (float)LOGLL + LOGTA;
                RA_LOGVOL(3, DOB, HT, CR, SVOL, LOGLL, H, D, TLL, EX, out float _, out float VOLG, NL, LVOL, TOTS);
                VALU += VOLG;
            }

            // COMPUTE VOLUME OF TOP LOG
            if (TLL >= (LOGML + LOGTA))
            {
                int J = (int)(TLL - LOGTA);
                TLL = (float)J + LOGTA;
                D = TLL / (float)LOGLL;
                H += TLL;
                RA_LOGVOL(3, DOB, HT, CR, SVOL, LOGLL, H, D, TLL, EX, out float _, out float VOLG, NL, LVOL, TOTS);
                VALU += VOLG;
            }
            if (VALU < 0.0F)
            {
                VALU = 0.0F;
            }
            return VALU;
        }

        public static void RA_LOGVOL(int N, float DBH, float HT, float CR, int SVOL, int LOGLL, float HI, float D, float TLL, float EX, out float DI, out float V, float[,] NL, float[,] LVOL, float[,] TOTS)
        {
            // ROUTINE TO CALCULATE LOG VOLUME AND ADD TO APPROPRIATE CELL
            //
            // N = TYPE OF CALCULATION
            //       = 1  MERCHANTABLE HEIGHT
            //       = 2  LOG VOLUME
            //       = 3  TREE VOLUME
            //       = 4  TOP DIAMETER CHECK
            //
            // SVOL = SPECIES GROUP FOR LOG REPORT
            //     EX = TREE RESIDUAL OR CUT EXPANSION FACTOR
            //     D = RATIO OF LOG LENGTH TO SPECIFIED LOG LENGTH

            // USE TAPER EQUATION TO DETERMINE DIAMETER AT TOP OF LOG
            V = 0.0F;
            DI = TAPER_RA(DBH, HT, CR, HI);
            if (N == 1 || N == 4)
            {
                return;
            }

            // EXTRACT VOLUME FROM VOLUME TABLES
            int LEN;
            if (D < 1.0F || D > 1.0F)
            {
                LEN = (int)TLL;
            }
            else
            {
                LEN = LOGLL;
            }

            float[] SVTBL = new float[] { 0.0F, 0.143F, 0.39F, 0.676F, 1.07F, 1.160F, 1.4F, 1.501F, 2.084F,
                                          3.126F, 3.749F, 4.9F, 6.043F, 7.14F, 8.88F, 10.0F, 11.528F,
                                          13.29F, 14.99F, 17.499F, 18.99F, 20.88F, 23.51F, 25.218F,
                                          28.677F, 31.249F, 34.22F, 36.376F, 38.04F, 41.06F, 44.376F,
                                          45.975F, 48.99F, 50.0F, 54.688F, 57.66F, 64.319F, 66.73F, 70.0F,
                                          75.24F, 79.48F, 83.91F, 87.19F, 92.501F, 94.99F, 99.075F,
                                          103.501F, 107.97F, 112.292F, 116.99F, 121.65F, 126.525F,
                                          131.51F, 136.51F, 141.61F, 146.912F, 152.21F, 157.71F,
                                          163.288F, 168.99F, 174.85F, 180.749F, 186.623F, 193.17F,
                                          199.12F, 205.685F, 211.81F, 218.501F, 225.685F, 232.499F,
                                          239.317F, 246.615F, 254.04F, 261.525F, 269.04F, 276.63F,
                                          284.26F, 292.501F, 300.655F, 308.97F };
            float[] SVTBL16 = new float[] { 1.249F, 1.608F, 1.854F, 2.410F, 3.542F, 4.167F };
            float[] SVTBL32 = new float[] { 1.57F, 1.8F, 2.2F, 2.9F, 3.815F, 4.499F };
            int DII = (int)DI - 1;
            if (DII >= 5 && DII <= 10)
            {
                if (LEN >= 16 && LEN <= 31)
                {
                    V = SVTBL16[DII - 5] * (float)LEN * EX;
                }
                else if (LEN >= 32 && LEN <= 40)
                {
                    V = SVTBL32[DII - 5] * (float)LEN * EX;
                }
                else
                {
                    V = SVTBL[DII] * (float)LEN * EX;
                }
            }
            else
            {
                V = SVTBL[DII] * (float)LEN * EX;
            }
            if (N == 3)
            {
                return;
            }
            DII /= 2;
            NL[DII, SVOL] = NL[DII, SVOL] + EX;
            LVOL[DII, SVOL] = LVOL[DII, SVOL] + V;
            TOTS[1, SVOL] = TOTS[1, SVOL] + V;
        }

        public static void RAMORT(Stand stand, float RAAGE, float RAN, float[] PMK)
        {
            float RAMORT1 = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = stand.Species[treeIndex];
                if (species == FiaCode.AlnusRubra)
                {
                    float PM = 1.0F / (1.0F + MathV.Exp(-PMK[treeIndex]));
                    RAMORT1 += PM * stand.LiveExpansionFactor[treeIndex];
                }
            }

            float RAQMDN1 = 3.313F + 0.18769F * RAAGE - 0.000198F * RAAGE * RAAGE;
            float RABAN1 = -26.1467F + 5.31482F * RAAGE - 0.037466F * RAAGE * RAAGE;
            float RAQMDN2 = 3.313F + 0.18769F * (RAAGE + 5.0F) - 0.000198F * (RAAGE + Constant.DefaultTimeStepInYears) * (RAAGE + Constant.DefaultTimeStepInYears);
            float RABAN2 = -26.1467F + 5.31482F * (RAAGE + 5.0F) - 0.037466F * (RAAGE + Constant.DefaultTimeStepInYears) * (RAAGE + Constant.DefaultTimeStepInYears);
            float RATPAN1 = RABAN1 / (Constant.ForestersEnglish * RAQMDN1 * RAQMDN1);
            float RATPAN2 = RABAN2 / (Constant.ForestersEnglish * RAQMDN2 * RAQMDN2);
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
                    float NK = 10.0F / MathV.Exp10(KK);
                kr1: KR1 += NK;
                    RAMORT1 = 0.0F;
                    for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                    {
                        FiaCode species = stand.Species[treeIndex];
                        if (species == FiaCode.AlnusRubra)
                        {
                            float PM = 1.0F / (1.0F + MathV.Exp(-(KR1 + PMK[treeIndex])));
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

        private static float TAPER_RA(float DBH, float HT, float CR, float HI)
        {
            float A1 = 0.9113F;
            float A2 = 1.0160F;
            float A3 = 0.2623F;
            float A4 = -18.7695F;
            float A5 = 3.1931F;
            float A6 = 0.1631F;
            float A7 = 0.4180F;
            float D140 = 0.000585F + 0.997212F * DBH;
            float Z = HI / HT;
            float P = 4.5F / HT;
            float X = (1.0F - MathF.Sqrt(Z)) / (1.0F - MathF.Sqrt(P));
            float C = A3 * (1.364409F * MathV.Pow(D140, 0.3333333F) * MathV.Exp(A4 * Z) + MathV.Exp(A5 * MathV.Pow(CR, A6) * MathV.Pow(D140 / HT, A7) * Z));
            float DI = A1 * MathV.Pow(D140, A2) * MathF.Pow(X, C);
            return DI;
        }

        public static void WHHLB_GEA(float H, float SI_UC, out float GEA)
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

        public static void WHHLB_SI_UC(float SI_C, float PDEN, out float SI_UC)
        {
            // UNCORRECTS THE DENSITY INPACT UPON THE WEISKITTEL, HANN, HIBBS, LAM, AND BLUHN SITE INDEX FOR RED ALDER
            // SITE INDEX UNCORRECTED FOR DENSITY EFFECT
            SI_UC = SI_C * (1.0F - 0.326480904F * MathV.Exp(-0.000400268678F * MathF.Pow(PDEN, 1.5F)));
        }
    }
}
