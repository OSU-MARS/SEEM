using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class Mortality
    {
        public static void MORTAL(Variant VERSION, int CYCLG, int NTREES, int IB, int[,] TDATAI, bool POST, bool MORT,
                                  float[,] TDATAR, float[,] SCR, float[,] GROWTH, float[] MGEXP, float[] DEADEXP, float[] BALL1, float[] BAL1, float SI_1,
                                  float SI_2, float[] PN, float[] YF, float A1, float A2, float A1MAX, float PA1MAX, float NO, float RD0, float RAAGE, float PDEN)
        {
            // ROUTINE FOR SETTING TREE MORTALITY
            float[] POW = new float[2000];
            for (int I = 0; I < NTREES; ++I)
            {
                POW[I] = 1.0F;
                if (VERSION == Variant.Rap && TDATAI[I, 0] != 351)
                {
                    POW[I] = 0.2F;
                }
            }

            float A3;
            if (VERSION <= Variant.Smc)
            {
                A3 = 14.39533971F;
            }
            else
            {
                A3 = 3.88F;
            }
            float RDCC;
            if (VERSION <= Variant.Smc)
            {
                RDCC = 0.60F;
            }
            else
            {
                RDCC = 0.5211F;
            }

            float KB = 0.005454154F;
            float KR1 = 1.0F;
            float STBA = 0.0F;
            float STN = 0.0F;
            float RAN = 0.0F;
            float[] PMK = new float[2000];
            for (int I = 0; I < NTREES; ++I)
            {
                STBA = STBA + TDATAR[I, 0] * TDATAR[I, 0] * KB * TDATAR[I, 3];
                STN = STN + TDATAR[I, 3];
                if (TDATAI[I, 0] == 351 && VERSION <= Variant.Smc)
                {
                    RAN = RAN + TDATAR[I, 3];
                }
                if (CYCLG == 0 && POST)
                {
                    STBA = STBA + TDATAR[I, 0] * TDATAR[I, 0] * KB * MGEXP[I];
                    STN = STN + MGEXP[I];
                }
                PMK[I] = 0.0F;
                DEADEXP[I] = 0.0F;
            }
            if (RAN <= 0.0001)
            {
                RAAGE = 0.0F;
            }
            float SQMDM = (float)Math.Exp(A1 - A2 * Math.Log(STN));
            float SQMDA = (float)Math.Sqrt(STBA / (KB * STN));
            float RD = STN / (float)Math.Exp(A1 / A2 - Math.Log(SQMDA) / A2);
            if (CYCLG == 0)
            {
                RD0 = RD;
                NO = 0.0F;
                A1MAX = A1;
            }
            float BAA = 0.0F;
            float NA = 0.0F;
            OldGro(NTREES, IB, TDATAI, TDATAR, GROWTH, DEADEXP, 0.0F, out float OG1);

            // INDIVIDUAL TREE MORTALITY EQUATIONS
            for (int I = 0; I < NTREES; ++I)
            {
                if (TDATAR[I, 3] <= 0.0F)
                {
                    continue;
                }
                int ISPGRP = TDATAI[I, 1];
                float DBH = TDATAR[I, 0];
                float HT = TDATAR[I, 1];
                PM_FERT(ISPGRP, VERSION, CYCLG, PN, YF, out float FERTADJ);
                DiameterGrowth.GET_BAL(DBH, BALL1, BAL1, out float SBAL1);
                float CR;
                if (SCR[I, 0] > TDATAR[I, 2])
                {
                    CR = SCR[I, 0];
                }
                else
                {
                    CR = TDATAR[I, 2];
                }

                switch (VERSION)
                {
                    case Variant.Swo:
                        PM_SWO(ISPGRP, DBH, CR, SI_1, SBAL1, OG1, POW[I], out PMK[I]);
                        break;
                    case Variant.Nwo:
                        PM_NWO(ISPGRP, DBH, CR, SI_1, SI_2, SBAL1, POW[I], out PMK[I]);
                        break;
                    case Variant.Smc:
                        PM_SMC(ISPGRP, DBH, CR, SI_1, SI_2, SBAL1, POW[I], out PMK[I]);
                        break;
                    case Variant.Rap:
                        PM_RAP(ISPGRP, DBH, CR, SI_1, SI_2, SBAL1, POW[I], out PMK[I]);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                PMK[I] = PMK[I] + FERTADJ;
            }

            if (VERSION <= Variant.Smc)
            {
                if (RAAGE >= 55.0)
                {
                    RAMORT(NTREES, TDATAI, RAAGE, TDATAR, RAN, PMK);
                }
                RAAGE = RAAGE + 5.0F;
            }

            for (int I = 0; I < NTREES; ++I)
            {
                float CR;
                if (SCR[I, 0] > TDATAR[I, 2])
                {
                    CR = SCR[I, 0];
                }
                else
                {
                    CR = TDATAR[I, 2];
                }
                float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
                float XPM = 1.0F / (1.0F + (float)Math.Exp(-PMK[I]));
                float PS = (float)Math.Pow(1.0F - XPM, POW[I]);
                float PM = 1.0F - PS * CRADJ;
                NA = NA + TDATAR[I, 3] * (1.0F - PM);
                BAA = BAA + KB * (float)Math.Pow(TDATAR[I, 0] + GROWTH[I, 1], 2) * TDATAR[I, 3] * (1.0F - PM);
            }

            // DETERMINE IF ADDITIONAL MORTALITY MUST BE TAKEN
            if (MORT)
            {
                float QMDA = (float)Math.Sqrt(BAA / (KB * NA));
                float RDA = NA / (float)Math.Exp(A1 / A2 - Math.Log(QMDA) / A2);
                if (CYCLG == 0)
                {
                    // INITALIZATIONS FOR FIRST GROWTH CYCLE
                    int IND;
                    if (RD >= 1.0)
                    {
                        if (RDA > RD)
                        {
                            A1MAX = (float)(Math.Log(SQMDA) + A2 * Math.Log(STN));
                        }
                        else
                        {
                            A1MAX = (float)(Math.Log(QMDA) + A2 * Math.Log(NA));
                        }
                        IND = 1;
                        if (A1MAX < A1)
                        {
                            A1MAX = A1;
                        }
                        PA1MAX = A1MAX;
                    }
                    else
                    {
                        IND = 0;
                        if (VERSION <= Variant.Smc)
                        {
                            if (RD > RDCC)
                            {
                                float XA3 = -1.0F / A3;
                                NO = STN * (float)Math.Pow(Math.Log(RD) / Math.Log(RDCC), XA3);
                            }
                            else
                            {
                                NO = PDEN;
                            }
                        }
                        // INITIALIZATIONS FOR SUBSEQUENT GROWTH CYCLES
                        else
                        {
                            if (RD0 >= 1.0F)
                            {
                                IND = 0;
                                A1MAX = (float)(Math.Log(QMDA) + A2 * Math.Log(NA));
                                if (A1MAX > PA1MAX)
                                {
                                    A1MAX = PA1MAX;
                                }
                                if (A1MAX < A1)
                                {
                                    A1MAX = A1;
                                }
                                PA1MAX = A1MAX;
                            }
                            else
                            {
                                if (RD >= 1.0F && NO <= 0.0F)
                                {
                                    if (RDA > RD)
                                    {
                                        A1MAX = (float)(Math.Log(SQMDA) + A2 * Math.Log(STN));
                                    }
                                    else
                                    {
                                        A1MAX = (float)(Math.Log(QMDA) + A2 * Math.Log(NA));
                                    }
                                    IND = 1;
                                    if (A1MAX < A1)
                                    {
                                        A1MAX = A1;
                                    }
                                    PA1MAX = A1MAX;
                                }
                                else
                                {
                                    IND = 0;
                                    if (VERSION <= Variant.Smc)
                                    {
                                        if (RD > RDCC && NO <= 0.0F)
                                        {
                                            float XA3 = -1.0F / A3;
                                            NO = STN * (float)Math.Pow(Math.Log(RD) / Math.Log(RDCC), XA3);
                                        }
                                        else
                                        {
                                            NO = PDEN;
                                        }
                                    }
                                }

                                // COMPUTATION OF ADDITIONAL MORTALITY IF NECESSARY
                                float QMDP;
                                if (IND == 0 && NO > 0.0F)
                                {
                                    if (VERSION <= Variant.Smc)
                                    {
                                        QMDP = QUAD1(NA, NO, RDCC, A1);
                                    }
                                    else
                                    {
                                        QMDP = QUAD2(NA, NO, RDCC, A1);
                                    }
                                }
                                else
                                {
                                    QMDP = (float)Math.Exp(A1MAX - A2 * Math.Log(NA));
                                }

                                if (RD <= RDCC || QMDP > QMDA)
                                {
                                    // NO ADDITIONAL MORTALITY NECESSARY
                                    for (int I = 0; I < NTREES; ++I)
                                    {
                                        float CR;
                                        if (SCR[I, 0] > TDATAR[I, 2])
                                        {
                                            CR = SCR[I, 0];
                                        }
                                        else
                                        {
                                            CR = TDATAR[I, 2];
                                        }

                                        float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
                                        float XPM = 1.0F / (1.0F + (float)Math.Exp(-PMK[I]));
                                        float PS = (float)Math.Pow(1.0 - XPM, POW[I]);
                                        float PM = 1.0F - PS * CRADJ;
                                        Debug.Assert(PM >= 0.0F);
                                        Debug.Assert(PM <= 1000.0F);

                                        DEADEXP[I] = TDATAR[I, 3] * PM;
                                        TDATAR[I, 3] = TDATAR[I, 3] * (1.0F - PM);
                                    }
                                }
                                else
                                {
                                    // ADJUSTMENT TO MORTALITY NECESSARY
                                    KR1 = 0.0F;
                                    for (int KK = 0; KK < 7; ++KK)
                                    {
                                        float NK = 10.0F / (float)Math.Pow(10.0, KK);
                                    kr1: KR1 = KR1 + NK;
                                        float NAA = 0.0F;
                                        float BAAA = 0.0F;
                                        for (int I = 0; I < NTREES; ++I)
                                        {
                                            if (TDATAR[I, 3] < 0.001F)
                                            {
                                                continue;
                                            }

                                            float CR;
                                            if (SCR[I, 0] > TDATAR[I, 2])
                                            {
                                                CR = SCR[I, 0];
                                            }
                                            else
                                            {
                                                CR = TDATAR[I, 2];
                                            }

                                            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
                                            float XPM = 1.0F / (1.0F + (float)Math.Exp(-(KR1 + PMK[I])));
                                            float PS = (float)Math.Pow(1.0 - XPM, POW[I]);
                                            float PM = 1.0F - PS * CRADJ;
                                            NAA = NAA + TDATAR[I, 3] * (1.0F - PM);
                                            BAAA = BAAA + KB * (float)Math.Pow(TDATAR[I, 0] + GROWTH[I, 1], 2.0) * TDATAR[I, 3] * (1.0F - PM);
                                        }
                                        QMDA = (float)Math.Sqrt(BAAA / (KB * NAA));
                                        if (IND == 0)
                                        {
                                            if (VERSION <= Variant.Smc)
                                            {
                                                QMDP = QUAD1(NAA, NO, RDCC, A1);
                                            }
                                            else
                                            {
                                                QMDP = QUAD2(NAA, NO, RDCC, A1);
                                            }
                                        }
                                        else
                                        {
                                            QMDP = (float)Math.Exp(A1MAX - A2 * Math.Log(NAA));
                                        }
                                        if (QMDP >= QMDA)
                                        {
                                            KR1 = KR1 - NK;
                                        }
                                        else
                                        {
                                            goto kr1;
                                        }
                                    }
                                    for (int I = 0; I < NTREES; ++I)
                                    {
                                        if (TDATAR[I, 3] <= 0.0F)
                                        {
                                            DEADEXP[I] = 0.0F;
                                            TDATAR[I, 3] = 0.0F;
                                        }
                                        else
                                        {
                                            float CR;
                                            if (SCR[I, 0] > TDATAR[I, 2])
                                            {
                                                CR = SCR[I, 0];
                                            }
                                            else
                                            {
                                                CR = TDATAR[I, 2];
                                            }
                                            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
                                            float XPM = 1.0F / (1.0F + (float)Math.Exp(-(KR1 + PMK[I])));
                                            float PS = (float)Math.Pow(1.0F - XPM, POW[I]);
                                            float PM = 1.0F - PS * CRADJ;
                                            Debug.Assert(PM >= 0.0F);
                                            Debug.Assert(PM <= 1000.0F);

                                            DEADEXP[I] = TDATAR[I, 3] * PM;
                                            TDATAR[I, 3] = TDATAR[I, 3] * (1.0F - PM);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int I = 0; I < NTREES; ++I)
                {
                    float CR;
                    if (SCR[I, 0] > TDATAR[I, 2])
                    {
                        CR = SCR[I, 0];
                    }
                    else
                    {
                        CR = TDATAR[I, 2];
                    }

                    float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
                    float XPM = 1.0F / (1.0F + (float)Math.Exp(-PMK[I]));
                    float PS = (float)Math.Pow(1.0 - XPM, POW[I]);
                    float PM = 1.0F - PS * CRADJ;
                    Debug.Assert(PM >= 0.0F);
                    Debug.Assert(PM <= 1000.0F);

                    DEADEXP[I] = TDATAR[I, 3] * PM;
                    TDATAR[I, 3] = TDATAR[I, 3] * (1.0F - PM);
                }
            }

            for (int I = 0; I < NTREES; ++I)
            {
                Debug.Assert(DEADEXP[I] >= 0.0F);
                Debug.Assert(DEADEXP[I] <= 1000.0F);

                if (TDATAR[I, 3] < 0.00001F)
                {
                    TDATAR[I, 3] = 0.0F;
                }
            }
        }

        private static float QUAD1(float NI, float NO, float RDCC, float A1)
        {
            float A2 = 0.62305F;
            float A3 = 14.39533971F;
            float A4 = -((float)Math.Log(RDCC) * A2 / A1);
            float X = A1 - A2 * (float)Math.Log(NI) - (A1 * A4) * (float)Math.Exp(-A3 * (Math.Log(NO) - Math.Log(NI)));
            return (float)Math.Exp(X);
        }

        private static float QUAD2(float NI, float NO, float RDCC, float A1)
        {
            float A2 = 0.64F;
            float A3 = 3.88F;
            float A4 = 0.07F;
            float X = A1 - A2 * (float)Math.Log(NI) - (A1 * A4) * (float)Math.Exp(-A3 * (Math.Log(NO) - Math.Log(NI)));
            return (float)Math.Exp(X);
        }

        private static void RAMORT(int NTREES, int[,] TDATAI, float RAAGE, float[,] TDATAR, float RAN, float[] PMK)
        {
            float KB = 0.005454154F;
            float RAMORT1 = 0.0F;
            for (int I = 0; I < NTREES; ++I)
            {
                if (TDATAI[I, 0] == 351)
                {
                    float PM = 1.0F / (1.0F + (float)Math.Exp(-PMK[I]));
                    RAMORT1 = RAMORT1 + TDATAR[I, 3] * PM;
                }
            }

            float RAQMDN1 = 3.313F + 0.18769F * RAAGE - 0.000198F * RAAGE * RAAGE;
            float RABAN1 = -26.1467F + 5.31482F * RAAGE - 0.037466F * RAAGE * RAAGE;
            float RAQMDN2 = 3.313F + 0.18769F * (RAAGE + 5.0F) - 0.000198F * (float)Math.Pow(RAAGE + 5.0, 2.0);
            float RABAN2 = -26.1467F + 5.31482F * (RAAGE + 5.0F) - 0.037466F * (float)Math.Pow(RAAGE + 5.0F, 2.0);
            float RATPAN1 = RABAN1 / (KB * RAQMDN1 * RAQMDN1);
            float RATPAN2 = RABAN2 / (KB * RAQMDN2 * RAQMDN2);
            float RAMORT2;
            if (RATPAN1 > 0.0F && RATPAN2 > 0.0F)
            {
                RAMORT2 = RAN * (1.0F - RATPAN2 / RATPAN1);
            }
            else
            {
                for (int I = 0; I < NTREES; ++I)
                {
                    if (TDATAI[I, 0] == 351)
                    {
                        PMK[I] = 1000.0F;
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
                kr1: KR1 = KR1 + NK;
                    RAMORT1 = 0.0F;
                    for (int I = 0; I < NTREES; ++I)
                    {
                        if (TDATAI[I, 0] == 351)
                        {
                            float PM = 1.0F / (1.0F + (float)Math.Exp(-(KR1 + PMK[I])));
                            RAMORT1 = RAMORT1 + TDATAR[I, 3] * PM;
                        }
                    }
                    if (RAMORT1 > RAMORT2)
                    {
                        KR1 = KR1 - NK;
                    }
                    else
                    {
                        goto kr1;
                    }
                }
                for (int I = 0; I < NTREES; ++I)
                {
                    if (TDATAI[I, 0] == 351)
                    {
                        PMK[I] = KR1 + PMK[I];
                    }
                }
            }
        }

        private static void PM_FERT(int ISPGRP, Variant VERSION, int CYCLG, float[] PN, float[] YF, out float FERTADJ)
        {
            float PF1;
            float PF2;
            float PF3;
            if (VERSION <= Variant.Smc)
            {
                if (ISPGRP == 0)
                {
                    PF1 = 0.0000552859F;
                    PF2 = 1.5F;
                    PF3 = -0.5F;
                }
                else
                {
                    PF1 = 0.0F;
                    PF2 = 1.0F;
                    PF3 = 0.0F;
                }
            }
            else
            {
                PF1 = 0.0F;
                PF2 = 1.0F;
                PF3 = 0.0F;
            }

            float XTIME = (float)CYCLG * 5.0F;
            float FERTX1 = 0.0F;
            for (int II = 1; II < 5; ++II)
            {
                FERTX1 = FERTX1 + PN[II] * (float)Math.Exp((PF3 / PF2) * (YF[0] - YF[II]));
            }
            float FERTX2 = (float)Math.Exp(PF3 * (XTIME - YF[0]));
            FERTADJ = PF1 * (float)Math.Pow(PN[0]+ FERTX1, PF2) * FERTX2;
        }

        private static void PM_SWO(int ISPGRP, float DBH, float CR, float SI_1, float BAL, float OG, float POW, out float PM)
        {
            // NEW SWO MORTALITY WITH REVISED CLO PARAMETERS(8 parameters - all species)
            // DF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // GW Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // SP Coefficients from Unpublished Equation on File at OSU Dept.Forest Resources
            // IC Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RC Coefficients from WH of Hann, Marshall, Hanus(2003) FRL Research Contribution 40
            // PY Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // GC Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // TA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // CL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // BO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RA Coefficients from Best Guess
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Best Guess
            float[,] MPAR = {
                {
                    -4.648483270F, -2.215777201F, -1.050000682F, -1.531051304F, // DF,GW,PP,SP
                    -1.922689902F, -1.166211991F, -0.761609F, -4.072781265F, // IC,WH,RC,PY
                    -6.089598985F, -4.317549852F, -2.410756914F, -2.990451960F, // MD,GC,TA,CL
                    -2.976822456F, -6.00031085F, -3.108619921F, -2.0F, // BL,WO,BO,RA
                    -3.020345211F, -1.386294361F,                             // PD,WI
                },
                {
                    -0.266558690F, -0.162895666F, -0.194363402F, 0.0F, // DF,GW,PP,SP
                    -0.136081990F, 0.0F, -0.529366F, -0.176433475F, // IC,WH,RC,PY
                    -0.245615070F, -0.057696253F, 0.0F, 0.0F, // MD,GC,TA,CL
                    0.0F, -0.10490823F, -0.570366764F, -0.5F, // BL,WO,BO,RA
                    0.0F, 0.0F,                             // PD,WI
                },
                {
                    0.003699110F, 0.003317290F, 0.003803100F, 0.0F, // DF,GW,PP,SP
                    0.002479863F, 0.0F, 0.0F, 0.0F, // IC,WH,RC,PY
                    0.0F, 0.0F, 0.0F, 0.0F, // MD,GC,TA,CL
                    0.0F, 0.0F, 0.018205398F, 0.015F, // BL,WO,BO,RA
                    0.0F, 0.0F,                             // PD,WI
                },
                {
                    -2.118026640F, -3.561438261F, -3.557300286F, 0.0F, // DF,GW,PP,SP
                    -3.178123293F, -4.602668157F, -4.74019F, -1.729453975F, // IC,WH,RC,PY
                    -3.208265570F, 0.0F, -1.049353753F, 0.0F, // MD,GC,TA,CL
                    -6.223250962F, -0.99541909F, -4.584655216F, -3.0F, // BL,WO,BO,RA
                    -8.467882343F, 0.0F,                             // PD,WI
                },
                {
                    0.025499430F, 0.014644689F, 0.003971638F, 0.0F, // DF,GW,PP,SP
                    0.0F, 0.0F, 0.0119587F, 0.0F, // IC,WH,RC,PY
                    0.033348079F, 0.004861355F, 0.008845583F, 0.0F, // MD,GC,TA,CL
                    0.0F, 0.00912739F, 0.014926170F, 0.015F, // BL,WO,BO,RA
                    0.013966388F, 0.0F,                             // PD,WI
                },
                {
                    0.003361340F, 0.0F, 0.005573601F, 0.0F, // DF,GW,PP,SP
                    0.004684133F, 0.0F, 0.00756365F, 0.012525642F, // IC,WH,RC,PY
                    0.013571319F, 0.00998129F, 0.0F, 0.002884840F, // MD,GC,TA,CL
                    0.0F, 0.87115652F, 0.012419026F, 0.01F, // BL,WO,BO,RA
                    0.009461545F, 0.0F,                             // PD,WI
                },
                {
                    0.013553950F, 0.0F, 0.0F, 0.0F, // DF,GW,PP,SP
                    0.0F, 0.0F, 0.0F, 0.0F, // IC,WH,RC,PY
                    0.0F, 0.0F, 0.0F, 0.0F, // MD,GC,TA,CL
                    0.0F, 0.0F, 0.0F, 0.0F, // BL,WO,BO,RA
                    0.0F, 0.0F,                             // PD,WI
                },
                {
                    -2.723470950F, 0.0F, 0.0F, 0.0F, // DF,GW,PP,SP
                    0.0F, 0.0F, 0.0F, 0.0F, // IC,WH,RC,PY
                    0.0F, 0.0F, 0.0F, 0.0F, // MD,GC,TA,CL
                    0.0F, 0.0F, 0.0F, 0.0F, // BL,WO,BO,RA
                    0.0F, 0.0F,                             // PD,WI
                },
                {
                    1.0F, 1.0F, 1.0F, 1.0F, // DF,GW,PP,SP
                    1.0F, 1.0F, 1.0F, 1.0F, // IC,WH,RC,PY
                    1.0F, 1.0F, 1.0F, 1.0F, // MD,GC,TA,CL
                    1.0F, 1.0F, 1.0F, 1.0F, // BL,WO,BO,RA
                    1.0F, 1.0F } // PD,WI
            };

            float B0 = MPAR[0, ISPGRP];
            float B1 = MPAR[1, ISPGRP];
            float B2 = MPAR[2, ISPGRP];
            float B3 = MPAR[3, ISPGRP];
            float B4 = MPAR[4, ISPGRP];
            float B5 = MPAR[5, ISPGRP];
            float B6 = MPAR[6, ISPGRP];
            float B7 = MPAR[7, ISPGRP];
            POW = MPAR[8, ISPGRP];
            if (ISPGRP == 13)
            {
                // Oregon White Oak
                PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_1 + 4.5F) + B5 * (float)Math.Log(BAL + 5.0);
            }
            else
            {
                PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_1 + 4.5F) + B5 * BAL + B6 * BAL * (float)Math.Exp(B7 * OG);
            }
        }

        private static void PM_NWO(int ISPGRP, float DBH, float CR, float SI_1, float SI_2, float BAL, float POW, out float PM)
        {
            // NWO MORTALITY(6 parameters - all species)
            //
            // DF Coefficients from Unpublished Equation on File at OSU Dept.Forest Resources
            // GF Coefficients from Unpublished Equation on File at OSU Dept. Forest Resources
            // WH Coefficients from Hann, Marshall, Hanus (2003) FRL Research Contribution 40
            // RC Coefficients from WH of Hann, Marshall, Hanus(2003) FRL Research Contribution 40
            // PY Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // RA Coefficients from Best Guess
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Best Guess
            float[,] MPAR = {
                {
                    -4.13142F, -7.60159F, -0.761609F, -0.761609F, // DF,GF,WH,RC
                    -4.072781265F, -6.089598985F, -2.976822456F, -6.00031085F, // PY,MD,BL,WO
                    -2.0F, -3.020345211F, -1.386294361F,               // RA,PD,WI
                },
                {
                    -1.13736F, -0.200523F, -0.529366F, -0.529366F, // DF,GF,WH,RC
                    -0.176433475F, -0.245615070F, 0.0F, -0.10490823F, // PY,MD,BL,WO
                    -0.5F, 0.0F, 0.0F,               // RA,PD,WI
                },
                {
                    0.0F, 0.0F, 0.0F, 0.0F, // DF,GF,WH,RC
                    0.0F, 0.0F, 0.0F, 0.0F, // PY,MD,BL,WO
                    0.015F, 0.0F, 0.0F,               // RA,PD,WI
                },
                {
                    -0.823305F, 0.0F, -4.74019F, -4.74019F, // DF,GF,WH,RC
                    -1.729453975F, -3.208265570F, -6.223250962F, -0.99541909F, // PY,MD,BL,WO
                    -3.0F, -8.467882343F, 0.0F,               // RA,PD,WI
                },
                {
                    0.0307749F, 0.0441333F, 0.0119587F, 0.0119587F, // DF,GF,WH,RC
                    0.0F, 0.033348079F, 0.0F, 0.00912739F, // PY,MD,BL,WO
                    0.015F, 0.013966388F, 0.0F,               // RA,PD,WI
                },
                {
                    0.00991005F, 0.00063849F, 0.00756365F, 0.00756365F, // DF,GF,WH,RC
                    0.012525642F, 0.013571319F, 0.0F, 0.87115652F, // PY,MD,BL,WO
                    0.01F, 0.009461545F, 0.0F,               // RA,PD,WI
                },
                {
                    1.0F, 1.0F, 1.0F, 1.0F, // DF,GF,WH,RC
                    1.0F, 1.0F, 1.0F, 1.0F, // PY,MD,BL,WO
                    1.0F, 1.0F, 1.0F  // RA,PD,WI
                }
            };

            float B0 = MPAR[0, ISPGRP];
            float B1 = MPAR[1, ISPGRP];
            float B2 = MPAR[2, ISPGRP];
            float B3 = MPAR[3, ISPGRP];
            float B4 = MPAR[4, ISPGRP];
            float B5 = MPAR[5, ISPGRP];
            POW = MPAR[6, ISPGRP];
            float SQDBH = (float)Math.Sqrt(DBH);
            float CR25 = (float)Math.Pow(CR, 0.25);
            if (ISPGRP == 0)
            {
                // Douglas fir
                PM = B0 + B1 * SQDBH + B3 * CR25 + B4 * (SI_1 + 4.5F) + B5 * BAL;
            }
            else
            {
                if (ISPGRP == 1)
                {
                    // Grand Fir
                    PM = B0 + B1 * DBH + B4 * (SI_1 + 4.5F) + B5 * (BAL / DBH);
                }
                else if (ISPGRP == 2 || ISPGRP == 3)
                {
                    // Western Hemlock and Western Red Cedar
                    PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_2 + 4.5F) + B5 * BAL;
                }
                else if (ISPGRP == 7)
                {
                    // Oregon White Oak
                    PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_1 + 4.5F) + B5 * (float)Math.Log(BAL + 5.0);
                }
                else
                {
                    PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_1 + 4.5F) + B5 * BAL;
                }
            }
        }

        private static void PM_SMC(int ISPGRP, float DBH, float CR, float SI_1, float SI_2, float BAL, float POW, out float PM)
        {
            // SMC MORTALITY(6 parameters - all species)
            //
            // DF Coefficients from Hann, Marshall, and Hanus(2006) FRL Research Contribution 49
            // GF Coefficients from Unpublished Equation on File at OSU Dept.Forest Resources
            // WH Coefficients from Hann, Marshall, Hanus(2003) FRL Research Contribution 40
            // RC Coefficients from WH of Hann, Marshall, Hanus(2003) FRL Research Contribution 40
            // PY Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // RA Coefficients from Best Guess
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Best Guess
            float[,] MPAR = {
                {
                    -3.12161659F, -7.60159F, -0.761609F, -0.761609F, // DF,GF,WH,RC
                    -4.072781265F, -6.089598985F, -2.976822456F, -6.00031085F, // PY,MD,BL,WO
                    -2.0F, -3.020345211F, -1.386294361F,               // RA,PD,WI
                },
                {
                    -0.44724396F, -0.200523F, -0.529366F, -0.529366F, // DF,GF,WH,RC
                    -0.176433475F, -0.245615070F, 0.0F, -0.10490823F, // PY,MD,BL,WO
                    -0.5F, 0.0F, 0.0F,               // RA,PD,WI
                },
                {
                    0.0F, 0.0F, 0.0F, 0.0F, // DF,GF,WH,RC
                    0.0F, 0.0F, 0.0F, 0.0F, // PY,MD,BL,WO
                    0.015F, 0.0F, 0.0F,               // RA,PD,WI
                },
                {
                    -2.48387172F, 0.0F, -4.74019F, -4.74019F, // DF,GF,WH,RC
                    -1.729453975F, -3.208265570F, -6.223250962F, -0.99541909F, // PY,MD,BL,WO
                    -3.0F, -8.467882343F, 0.0F,               // RA,PD,WI
                },
                {
                    0.01843137F, 0.0441333F, 0.0119587F, 0.0119587F, // DF,GF,WH,RC
                    0.0F, 0.033348079F, 0.0F, 0.00912739F, // PY,MD,BL,WO
                    0.015F, 0.013966388F, 0.0F,               // RA,PD,WI
                },
                {
                    0.01353918F, 0.00063849F, 0.00756365F, 0.00756365F, // DF,GF,WH,RC
                    0.012525642F, 0.013571319F, 0.0F, 0.87115652F, // PY,MD,BL,WO
                    0.01F, 0.009461545F, 0.0F,               // RA,PD,WI
                },
                {
                    1.0F, 1.0F, 1.0F, 1.0F, // DF,GF,WH,RC
                    1.0F, 1.0F, 1.0F, 1.0F, // PY,MD,BL,WO
                    1.0F, 1.0F, 1.0F  // RA,PD,WI
                }
            };

            float B0 = MPAR[0, ISPGRP];
            float B1 = MPAR[1, ISPGRP];
            float B2 = MPAR[2, ISPGRP];
            float B3 = MPAR[3, ISPGRP];
            float B4 = MPAR[4, ISPGRP];
            float B5 = MPAR[5, ISPGRP];
            POW = MPAR[6, ISPGRP];
            if (ISPGRP == 1)
            {
                // Grand Fir
                PM = B0 + B1 * DBH + B4 * (SI_1 + 4.5F) + B5 * (BAL / DBH);
            }
            else if (ISPGRP == 2 || ISPGRP == 3)
            {
                // Western Hemlock and Western Red Cedar
                PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_2 + 4.5F) + B5 * BAL;
            }
            else if (ISPGRP == 7)
            {
                // Oregon White Oak
                PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_1 + 4.5F) + B5 * (float)Math.Log(BAL + 5.0);
            }
            else
            {
                PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_1 + 4.5F) + B5 * BAL;
            }
        }

        private static void PM_RAP(int ISPGRP, float DBH, float CR, float SI_1, float SI_2, float BAL, float POW, out float PM)
        {
            // RAP MORTALITY(6 parameters - all species)
            //
            // RA Coefficients from Hann, Bluhm, and Hibbs New Red Alder Equation
            // DF Coefficients from Hann, Marshall, and Hanus(2006) FRL Research Contribution 49
            // WH Coefficients from Hann, Marshall, Hanus(2003) FRL Research Contribution 40
            // RC Coefficients from WH of Hann, Marshall, Hanus(2003) FRL Research Contribution 40
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Best Guess
            float[,] MPAR = {
                {
                    -4.333150734F, -3.12161659F, -0.761609F, -0.761609F, // RA,DF,WH,RC
                    -2.976822456F, -3.020345211F, -1.386294361F,               // BL,PD,WI
                },
                {
                    -0.9856713799F, -0.44724396F, -0.529366F, -0.529366F, // RA,DF,WH,RC
                    0.0F, 0.0F, 0.0F,               // BL,PD,WI,WO
                },
                {
                    0.0F, 0.0F, 0.0F, 0.0F, // RA,DF,WH,RC
                    0.0F, 0.0F, 0.0F,               // BL,PD,WI
                },
                {
                    -2.583317081F, -2.48387172F, -4.74019F, -4.74019F, // RA,DF,WH,RC
                    -6.223250962F, -8.467882343F, 0.0F,               // BL,PD,WI
                },
                {
                    0.0369852164F, 0.01843137F, 0.0119587F, 0.0119587F, // RA,DF,WH,RC
                    0.0F, 0.013966388F, 0.0F,               // BL,PD,WI
                },
                {
                    0.0394546978F, 0.01353918F, 0.00756365F, 0.00756365F, // RA,DF,WH,RC
                    0.0F, 0.009461545F, 0.0F,               // BL,PD,WI
                },
                {
                    1.0F, 0.2F, 0.2F, 0.2F, // RA,DF,WH,RC
                    0.2F, 0.2F, 0.2F // BL,PD,WI
                }
            };

            float B0 = MPAR[0, ISPGRP];
            float B1 = MPAR[1, ISPGRP];
            float B2 = MPAR[2, ISPGRP];
            float B3 = MPAR[3, ISPGRP];
            float B4 = MPAR[4, ISPGRP];
            float B5 = MPAR[5, ISPGRP];
            POW = MPAR[6, ISPGRP];
            float SITE;
            if (ISPGRP == 0)
            {
                SITE = SI_1 + 4.5F;
            }
            else if (ISPGRP == 1 || ISPGRP > 3)
            {
                SITE = SI_2 + 4.5F;
            }
            else if (ISPGRP == 2 || ISPGRP == 3)
            {
                SITE = -0.432F + 0.899F * (SI_2 + 4.5F);
            }
            else
            {
                // BUGBUG: not handled in Fortran
                throw new NotSupportedException();
            }
            PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * SITE + B5 * BAL;
        }

        /// <summary>
        /// Computes old growth index?
        /// </summary>
        /// <param name="NTREES">Number of trees in tree data.</param>
        /// <param name="IB">Big six species index? (DOUG?)</param>
        /// <param name="TDATAI">Tree data.</param>
        /// <param name="TDATAR">Tree data.</param>
        /// <param name="GROWTH">Tree data.</param>
        /// <param name="DEADEXP">Tree data.</param>
        /// <param name="XIND">Zero or minus one, usually zero.</param>
        /// <param name="OG"></param>
        /// <remarks>
        /// XIND
        ///    0.0: NOT ADD GROWTH VALUES OR MORTALITY VALUES
        ///   -1.0: SUBTRACT GROWTH VALUES AND ADD MORTALITY VALUES
        ///    1.0: ADD GROWTH VALUES AND SUBTRACT MORTALITY VALUES
        /// </remarks>
        // BUGBUG: supports only to 98 DBH
        public static void OldGro(int NTREES, int IB, int[,] TDATAI, float[,] TDATAR, float[,] GROWTH, float[] DEADEXP, float XIND, out float OG)
        {
            float[] HTCL = new float[100];
            float[] DCL = new float[100];
            float[] TRCL = new float[100];
            for (int I = 0; I < 100; ++I)
            {
                HTCL[I] = 0.0F;
                DCL[I] = 0.0F;
                TRCL[I] = 0.0F;
            }
            for (int I = 0; I < NTREES; ++I)
            {
                if (TDATAI[I, 1] <= IB)
                {
                    float HT = TDATAR[I, 1] + XIND * GROWTH[I, 0];
                    float DBH = TDATAR[I, 0] + XIND * GROWTH[I, 1];
                    float EXPAN = TDATAR[I, 3] - XIND * DEADEXP[I];
                    Debug.Assert(EXPAN >= 0.0);
                    Debug.Assert(EXPAN <= 1000.0);

                    int ID = (int)DBH + 1;
                    if (ID > 99)
                    {
                        ID = 99;
                    }
                    HTCL[ID] = HTCL[ID] + HT * EXPAN;
                    DCL[ID] = DCL[ID] + DBH * EXPAN;
                    TRCL[ID] = TRCL[ID] + EXPAN;
                }
            }

            float TOTHT = 0.0F;
            float TOTD = 0.0F;
            float TOTTR = 0.0F;
            for (int I = 99; I > 0; --I)
            {
                TOTHT = TOTHT + HTCL[I];
                TOTD = TOTD + DCL[I];
                TOTTR = TOTTR + TRCL[I];
                if (TOTTR > 5.0F)
                {
                    float TRDIFF = TRCL[I] - (TOTTR - 5.0F);
                    TOTHT = TOTHT - HTCL[I] + ((HTCL[I] / TRCL[I]) * TRDIFF);
                    TOTD = TOTD - DCL[I] + ((DCL[I] / TRCL[I]) * TRDIFF);
                    TOTTR = 5.0F;
                    break;
                }
            }

            // DETERMINE THE OLD GROWTH INDICATOR "OG"
            if (TOTTR > 0.0F)
            {
                float HT5 = TOTHT / TOTTR;
                float DBH5 = TOTD / TOTTR;
                OG = DBH5 * HT5 / 10000.0F;
                Debug.Assert(OG >= 0.0F);
                Debug.Assert(OG <= 1000.0F);
            }
            else
            {
                OG = 0.0F;
            }
        }
    }
}
