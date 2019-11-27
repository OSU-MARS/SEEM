using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class Mortality
    {
        // ROUTINE FOR SETTING TREE MORTALITY
        public static void MORTAL(OrganonConfiguration configuration, int simulationStep, Stand stand, 
                                  StandDensity densityGrowth, float SI_1,
                                  float SI_2, float[] PN, float[] YF, ref float RAAGE)
        {
            float[] POW = new float[stand.TreeRecordCount];
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                POW[treeIndex] = 1.0F;
                FiaCode species = stand.Species[treeIndex];
                if ((configuration.Variant.Variant == Variant.Rap) && (species != FiaCode.AlnusRubra))
                {
                    POW[treeIndex] = 0.2F;
                }
            }

            float A3;
            if (configuration.Variant.Variant != Variant.Rap)
            {
                A3 = 14.39533971F;
            }
            else
            {
                A3 = 3.88F;
            }
            float RDCC;
            if (configuration.Variant.Variant != Variant.Rap)
            {
                RDCC = 0.60F;
            }
            else
            {
                RDCC = 0.5211F;
            }

            float KB = 0.005454154F;
            float standBasalArea = 0.0F;
            float standTreesPerAcre = 0.0F;
            float RAN = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                standBasalArea += expansionFactor * KB * stand.Dbh[treeIndex] * stand.Dbh[treeIndex];
                standTreesPerAcre += expansionFactor;

                FiaCode species = stand.Species[treeIndex];
                if ((species == FiaCode.AlnusRubra) && (configuration.Variant.Variant != Variant.Rap))
                {
                    RAN += expansionFactor;
                }
            }

            if (RAN <= 0.0001)
            {
                RAAGE = 0.0F;
            }
            float SQMDA = (float)Math.Sqrt(standBasalArea / (KB * standTreesPerAcre));
            float RD = standTreesPerAcre / (float)Math.Exp(stand.A1 / stand.A2 - Math.Log(SQMDA) / stand.A2);
            if (simulationStep == 0)
            {
                stand.RD0 = RD;
                stand.NO = 0.0F;
                stand.A1MAX = stand.A1;
            }
            float BAA = 0.0F;
            float NA = 0.0F;
            OldGro(stand, 0.0F, out float OG1);

            // INDIVIDUAL TREE MORTALITY EQUATIONS
            float[] PMK = new float[stand.TreeRecordCount];
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    continue;
                }
                int speciesGroup = stand.SpeciesGroup[treeIndex];
                PM_FERT(speciesGroup, configuration.Variant.Variant, simulationStep, PN, YF, out float FERTADJ);
                float DBH = stand.Dbh[treeIndex];
                float SBAL1 = densityGrowth.GetBasalAreaLarger(DBH);
                float CR = stand.CrownRatio[treeIndex];

                switch (configuration.Variant.Variant)
                {
                    case Variant.Swo:
                        PM_SWO(speciesGroup, DBH, CR, SI_1, SBAL1, OG1, out POW[treeIndex], out PMK[treeIndex]);
                        break;
                    case Variant.Nwo:
                        PM_NWO(speciesGroup, DBH, CR, SI_1, SI_2, SBAL1, out POW[treeIndex], out PMK[treeIndex]);
                        break;
                    case Variant.Smc:
                        PM_SMC(speciesGroup, DBH, CR, SI_1, SI_2, SBAL1, out POW[treeIndex], out PMK[treeIndex]);
                        break;
                    case Variant.Rap:
                        PM_RAP(speciesGroup, DBH, CR, SI_1, SI_2, SBAL1, out POW[treeIndex], out PMK[treeIndex]);
                        break;
                    default:
                        throw OrganonVariant.CreateUnhandledVariantException(configuration.Variant.Variant);
                }
                PMK[treeIndex] = PMK[treeIndex] + FERTADJ;
            }

            if (configuration.Variant.Variant != Variant.Rap)
            {
                if (RAAGE >= 55.0)
                {
                    RedAlder.RAMORT(stand, RAAGE, RAN, PMK);
                }
                RAAGE += Constant.DefaultTimeStepInYears;
            }

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float CR = stand.CrownRatio[treeIndex];
                float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
                float XPM = 1.0F / (1.0F + (float)Math.Exp(-PMK[treeIndex]));
                float PS = (float)Math.Pow(1.0F - XPM, POW[treeIndex]);
                float PM = 1.0F - PS * CRADJ;

                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                NA += expansionFactor * (1.0F - PM);
                BAA += KB * (float)Math.Pow(stand.Dbh[treeIndex] + stand.DbhGrowth[treeIndex], 2) * expansionFactor * (1.0F - PM);
            }

            // DETERMINE IF ADDITIONAL MORTALITY MUST BE TAKEN
            if (configuration.AdditionalMortality)
            {
                float QMDA = (float)Math.Sqrt(BAA / (KB * NA));
                float RDA = NA / (float)Math.Exp(stand.A1 / stand.A2 - Math.Log(QMDA) / stand.A2);
                if (simulationStep == 0)
                {
                    // INITALIZATIONS FOR FIRST GROWTH CYCLE
                    if (RD >= 1.0)
                    {
                        if (RDA > RD)
                        {
                            stand.A1MAX = (float)(Math.Log(SQMDA) + stand.A2 * Math.Log(standTreesPerAcre));
                        }
                        else
                        {
                            stand.A1MAX = (float)(Math.Log(QMDA) + stand.A2 * Math.Log(NA));
                        }
                        if (stand.A1MAX < stand.A1)
                        {
                            stand.A1MAX = stand.A1;
                        }
                    }
                    else
                    {
                        if (configuration.Variant.Variant != Variant.Rap)
                        {
                            if (RD > RDCC)
                            {
                                float XA3 = -1.0F / A3;
                                stand.NO = standTreesPerAcre * (float)Math.Pow(Math.Log(RD) / Math.Log(RDCC), XA3);
                            }
                            else
                            {
                                stand.NO = configuration.PDEN;
                            }
                        }
                        // INITIALIZATIONS FOR SUBSEQUENT GROWTH CYCLES
                        else
                        {
                            if (stand.RD0 >= 1.0F)
                            {
                                stand.A1MAX = (float)(Math.Log(QMDA) + stand.A2 * Math.Log(NA));
                                if (stand.A1MAX < stand.A1)
                                {
                                    stand.A1MAX = stand.A1;
                                }
                            }
                            else
                            {
                                int IND;
                                if ((RD >= 1.0F) && (stand.NO <= 0.0F))
                                {
                                    if (RDA > RD)
                                    {
                                        stand.A1MAX = (float)(Math.Log(SQMDA) + stand.A2 * Math.Log(standTreesPerAcre));
                                    }
                                    else
                                    {
                                        stand.A1MAX = (float)(Math.Log(QMDA) + stand.A2 * Math.Log(NA));
                                    }
                                    IND = 1;
                                    if (stand.A1MAX < stand.A1)
                                    {
                                        stand.A1MAX = stand.A1;
                                    }
                                }
                                else
                                {
                                    IND = 0;
                                    if (configuration.Variant.Variant != Variant.Rap)
                                    {
                                        if ((RD > RDCC) && (stand.NO <= 0.0F))
                                        {
                                            float XA3 = -1.0F / A3;
                                            stand.NO = standTreesPerAcre * (float)Math.Pow(Math.Log(RD) / Math.Log(RDCC), XA3);
                                        }
                                        else
                                        {
                                            stand.NO = configuration.PDEN;
                                        }
                                    }
                                }

                                // COMPUTATION OF ADDITIONAL MORTALITY IF NECESSARY
                                float QMDP;
                                if ((IND == 0) && (stand.NO > 0.0F))
                                {
                                    if (configuration.Variant.Variant != Variant.Rap)
                                    {
                                        QMDP = QUAD1(NA, stand.NO, RDCC, stand.A1);
                                    }
                                    else
                                    {
                                        QMDP = QUAD2(NA, stand.NO, stand.A1);
                                    }
                                }
                                else
                                {
                                    QMDP = (float)Math.Exp(stand.A1MAX - stand.A2 * Math.Log(NA));
                                }

                                if ((RD <= RDCC) || (QMDP > QMDA))
                                {
                                    // NO ADDITIONAL MORTALITY NECESSARY
                                    for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                                    {
                                        float CR = stand.CrownRatio[treeIndex];
                                        float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
                                        float XPM = 1.0F / (1.0F + (float)Math.Exp(-PMK[treeIndex]));
                                        float PS = (float)Math.Pow(1.0 - XPM, POW[treeIndex]);
                                        float PM = 1.0F - PS * CRADJ;
                                        Debug.Assert(PM >= 0.0F);
                                        Debug.Assert(PM <= 1.0F);

                                        stand.DeadExpansionFactor[treeIndex] = PM * stand.LiveExpansionFactor[treeIndex];
                                        stand.LiveExpansionFactor[treeIndex] -= stand.DeadExpansionFactor[treeIndex];
                                    }
                                }
                                else
                                {
                                    // ADJUSTMENT TO MORTALITY NECESSARY
                                    float KR1 = 0.0F;
                                    for (int KK = 0; KK < 7; ++KK)
                                    {
                                        float NK = 10.0F / (float)Math.Pow(10.0, KK);
                                    kr1: KR1 += NK;
                                        float NAA = 0.0F;
                                        float BAAA = 0.0F;
                                        for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                                        {
                                            if (stand.LiveExpansionFactor[treeIndex] < 0.001F)
                                            {
                                                continue;
                                            }

                                            float CR = stand.CrownRatio[treeIndex];
                                            float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
                                            float XPM = 1.0F / (1.0F + (float)Math.Exp(-(KR1 + PMK[treeIndex])));
                                            float PS = (float)Math.Pow(1.0 - XPM, POW[treeIndex]);
                                            float PM = 1.0F - PS * CRADJ;

                                            float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                                            NAA += expansionFactor * (1.0F - PM);
                                            BAAA += KB * (float)Math.Pow(stand.Dbh[treeIndex] + stand.DbhGrowth[treeIndex], 2.0) * expansionFactor * (1.0F - PM);
                                        }
                                        QMDA = (float)Math.Sqrt(BAAA / (KB * NAA));
                                        if (IND == 0)
                                        {
                                            if (configuration.Variant.Variant != Variant.Rap)
                                            {
                                                QMDP = QUAD1(NAA, stand.NO, RDCC, stand.A1);
                                            }
                                            else
                                            {
                                                QMDP = QUAD2(NAA, stand.NO, stand.A1);
                                            }
                                        }
                                        else
                                        {
                                            QMDP = (float)Math.Exp(stand.A1MAX - stand.A2 * Math.Log(NAA));
                                        }
                                        if (QMDP >= QMDA)
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
                                        if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                                        {
                                            stand.DeadExpansionFactor[treeIndex] = 0.0F;
                                            stand.LiveExpansionFactor[treeIndex] = 0.0F;
                                        }
                                        else
                                        {
                                            float CR = stand.CrownRatio[treeIndex];
                                            float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
                                            float XPM = 1.0F / (1.0F + (float)Math.Exp(-(KR1 + PMK[treeIndex])));
                                            float PS = (float)Math.Pow(1.0F - XPM, POW[treeIndex]);
                                            float PM = 1.0F - PS * CRADJ;
                                            Debug.Assert(PM >= 0.0F);
                                            Debug.Assert(PM <= 1.0F);

                                            stand.DeadExpansionFactor[treeIndex] = PM * stand.LiveExpansionFactor[treeIndex];
                                            stand.LiveExpansionFactor[treeIndex] -= stand.DeadExpansionFactor[treeIndex];
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
                for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                {
                    float CR = stand.CrownRatio[treeIndex];
                    float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
                    float XPM = 1.0F / (1.0F + (float)Math.Exp(-PMK[treeIndex]));
                    float PS = (float)Math.Pow(1.0 - XPM, POW[treeIndex]);
                    float PM = 1.0F - PS * CRADJ;
                    Debug.Assert(PM >= 0.0F);
                    Debug.Assert(PM <= 1.0F);

                    stand.DeadExpansionFactor[treeIndex] = PM * stand.LiveExpansionFactor[treeIndex];
                    stand.LiveExpansionFactor[treeIndex] -= stand.DeadExpansionFactor[treeIndex];
                }
            }

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.LiveExpansionFactor[treeIndex] < 0.00001F)
                {
                    stand.LiveExpansionFactor[treeIndex] = 0.0F;
                }
            }
        }

        private static float QUAD1(float ti, float t1, float RDCC, float A1)
        {
            // Hann 2003 Research Contribution 40, Table 38: Parameters for predicting the maximum size density line for Douglas-fir
            // maximum size density line for measurement i: ln(max QMDi for TPA) = MLQi = g1 + g2 * LTi
            // stand approach to maximum density: LQi = MLQi - (g1 + g2 * LT0 - LQ0) exp(g3 * (LT0 - LTi))
            // A1 = g1 + sum_j(g1_j) + g2 * log(ti)
            float g2 = 0.62305F;
            float g3 = 14.39533971F;
            float A4 = -((float)Math.Log(RDCC) * g2 / A1);
            float X = A1 - g2 * (float)Math.Log(ti) - (A1 * A4) * (float)Math.Exp(-g3 * (Math.Log(t1) - Math.Log(ti)));
            return (float)Math.Exp(X);
        }

        private static float QUAD2(float NI, float NO, float A1)
        {
            float A2 = 0.64F;
            float A3 = 3.88F;
            float A4 = 0.07F;
            float X = A1 - A2 * (float)Math.Log(NI) - (A1 * A4) * (float)Math.Exp(-A3 * (Math.Log(NO) - Math.Log(NI)));
            return (float)Math.Exp(X);
        }

        private static void PM_FERT(int ISPGRP, Variant variant, int simulationStep, float[] PN, float[] yearsSinceFertilization, out float FERTADJ)
        {
            float c5;
            float PF2;
            float PF3;
            if (variant != Variant.Rap)
            {
                // Hann 2003 Research Contribution 40, Table 37: Parameters for predicting fertlization response of 5-year mortality
                if (ISPGRP == 0)
                {
                    // Douglas-fir
                    c5 = 0.0000552859F;
                    PF2 = 1.5F;
                    PF3 = -0.5F;
                }
                else
                {
                    // western hemlock
                    c5 = 0.0F;
                    PF2 = 1.0F;
                    PF3 = 0.0F;
                }
            }
            else
            {
                c5 = 0.0F;
                PF2 = 1.0F;
                PF3 = 0.0F;
            }

            float XTIME = Constant.DefaultTimeStepInYears * (float)simulationStep;
            float FERTX1 = 0.0F;
            for (int II = 1; II < 5; ++II)
            {
                // BUGBUG: summation range doesn't match 13 or 18 year periods given in Hann 2003 Table 3
                FERTX1 += PN[II] * (float)Math.Exp((PF3 / PF2) * (yearsSinceFertilization[0] - yearsSinceFertilization[II]));
            }
            FERTADJ = c5 * (float)Math.Pow(PN[0] + FERTX1, PF2) * (float)Math.Exp(PF3 * (XTIME - yearsSinceFertilization[0]));
        }

        private static void PM_NWO(int ISPGRP, float DBH, float CR, float SI_1, float SI_2, float BAL, out float POW, out float PM)
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
                { // B0
                    -4.13142F, -7.60159F, -0.761609F, -0.761609F, // DF,GF,WH,RC
                    -4.072781265F, -6.089598985F, -2.976822456F, -6.00031085F, // PY,MD,BL,WO
                    -2.0F, -3.020345211F, -1.386294361F,               // RA,PD,WI
                },
                { // B1
                    -1.13736F, -0.200523F, -0.529366F, -0.529366F, // DF,GF,WH,RC
                    -0.176433475F, -0.245615070F, 0.0F, -0.10490823F, // PY,MD,BL,WO
                    -0.5F, 0.0F, 0.0F,               // RA,PD,WI
                },
                { // B2
                    0.0F, 0.0F, 0.0F, 0.0F, // DF,GF,WH,RC
                    0.0F, 0.0F, 0.0F, 0.0F, // PY,MD,BL,WO
                    0.015F, 0.0F, 0.0F,               // RA,PD,WI
                },
                { // B3
                    -0.823305F, 0.0F, -4.74019F, -4.74019F, // DF,GF,WH,RC
                    -1.729453975F, -3.208265570F, -6.223250962F, -0.99541909F, // PY,MD,BL,WO
                    -3.0F, -8.467882343F, 0.0F,               // RA,PD,WI
                },
                { // B4
                    0.0307749F, 0.0441333F, 0.0119587F, 0.0119587F, // DF,GF,WH,RC
                    0.0F, 0.033348079F, 0.0F, 0.00912739F, // PY,MD,BL,WO
                    0.015F, 0.013966388F, 0.0F,               // RA,PD,WI
                },
                { // B5
                    0.00991005F, 0.00063849F, 0.00756365F, 0.00756365F, // DF,GF,WH,RC
                    0.012525642F, 0.013571319F, 0.0F, 0.87115652F, // PY,MD,BL,WO
                    0.01F, 0.009461545F, 0.0F,               // RA,PD,WI
                },
                { // POW
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

        private static void PM_SMC(int ISPGRP, float DBH, float CR, float SI_1, float SI_2, float BAL, out float POW, out float PM)
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

        private static void PM_RAP(int ISPGRP, float DBH, float CR, float SI_1, float SI_2, float BAL, out float POW, out float PM)
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

        private static void PM_SWO(int ISPGRP, float DBH, float CR, float SI_1, float BAL, float OG, out float POW, out float PM)
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
                { // B0
                    -4.648483270F, -2.215777201F, -1.050000682F, -1.531051304F, // DF,GW,PP,SP
                    -1.922689902F, -1.166211991F, -0.761609F, -4.072781265F, // IC,WH,RC,PY
                    -6.089598985F, -4.317549852F, -2.410756914F, -2.990451960F, // MD,GC,TA,CL
                    -2.976822456F, -6.00031085F, -3.108619921F, -2.0F, // BL,WO,BO,RA
                    -3.020345211F, -1.386294361F,                             // PD,WI
                },
                { // B1
                    -0.266558690F, -0.162895666F, -0.194363402F, 0.0F, // DF,GW,PP,SP
                    -0.136081990F, 0.0F, -0.529366F, -0.176433475F, // IC,WH,RC,PY
                    -0.245615070F, -0.057696253F, 0.0F, 0.0F, // MD,GC,TA,CL
                    0.0F, -0.10490823F, -0.570366764F, -0.5F, // BL,WO,BO,RA
                    0.0F, 0.0F,                             // PD,WI
                },
                { // B2
                    0.003699110F, 0.003317290F, 0.003803100F, 0.0F, // DF,GW,PP,SP
                    0.002479863F, 0.0F, 0.0F, 0.0F, // IC,WH,RC,PY
                    0.0F, 0.0F, 0.0F, 0.0F, // MD,GC,TA,CL
                    0.0F, 0.0F, 0.018205398F, 0.015F, // BL,WO,BO,RA
                    0.0F, 0.0F,                             // PD,WI
                },
                { // B3
                    -2.118026640F, -3.561438261F, -3.557300286F, 0.0F, // DF,GW,PP,SP
                    -3.178123293F, -4.602668157F, -4.74019F, -1.729453975F, // IC,WH,RC,PY
                    -3.208265570F, 0.0F, -1.049353753F, 0.0F, // MD,GC,TA,CL
                    -6.223250962F, -0.99541909F, -4.584655216F, -3.0F, // BL,WO,BO,RA
                    -8.467882343F, 0.0F,                             // PD,WI
                },
                { // B4
                    0.025499430F, 0.014644689F, 0.003971638F, 0.0F, // DF,GW,PP,SP
                    0.0F, 0.0F, 0.0119587F, 0.0F, // IC,WH,RC,PY
                    0.033348079F, 0.004861355F, 0.008845583F, 0.0F, // MD,GC,TA,CL
                    0.0F, 0.00912739F, 0.014926170F, 0.015F, // BL,WO,BO,RA
                    0.013966388F, 0.0F,                             // PD,WI
                },
                { // B5
                    0.003361340F, 0.0F, 0.005573601F, 0.0F, // DF,GW,PP,SP
                    0.004684133F, 0.0F, 0.00756365F, 0.012525642F, // IC,WH,RC,PY
                    0.013571319F, 0.00998129F, 0.0F, 0.002884840F, // MD,GC,TA,CL
                    0.0F, 0.87115652F, 0.012419026F, 0.01F, // BL,WO,BO,RA
                    0.009461545F, 0.0F,                             // PD,WI
                },
                { // B6
                    0.013553950F, 0.0F, 0.0F, 0.0F, // DF,GW,PP,SP
                    0.0F, 0.0F, 0.0F, 0.0F, // IC,WH,RC,PY
                    0.0F, 0.0F, 0.0F, 0.0F, // MD,GC,TA,CL
                    0.0F, 0.0F, 0.0F, 0.0F, // BL,WO,BO,RA
                    0.0F, 0.0F,                             // PD,WI
                },
                { // B7
                    -2.723470950F, 0.0F, 0.0F, 0.0F, // DF,GW,PP,SP
                    0.0F, 0.0F, 0.0F, 0.0F, // IC,WH,RC,PY
                    0.0F, 0.0F, 0.0F, 0.0F, // MD,GC,TA,CL
                    0.0F, 0.0F, 0.0F, 0.0F, // BL,WO,BO,RA
                    0.0F, 0.0F,                             // PD,WI
                },
                { // POW
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

        /// <summary>
        /// Computes old growth index?
        /// </summary>
        /// <param name="stand">Stand data.</param>
        /// <param name="GROWTH">Tree data.</param>
        /// <param name="XIND">Zero or minus one, usually zero.</param>
        /// <param name="OG">Old growth indicator.</param>
        /// <remarks>
        /// XIND
        ///    0.0: NOT ADD GROWTH VALUES OR MORTALITY VALUES
        ///   -1.0: SUBTRACT GROWTH VALUES AND ADD MORTALITY VALUES
        ///    1.0: ADD GROWTH VALUES AND SUBTRACT MORTALITY VALUES
        /// </remarks>
        // BUGBUG: supports only to 98 DBH
        public static void OldGro(Stand stand, float XIND, out float OG)
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

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.IsBigSixSpecies(treeIndex))
                {
                    float heightInFeet = stand.Height[treeIndex] + XIND * stand.HeightGrowth[treeIndex];
                    float dbhInInches = stand.Dbh[treeIndex] + XIND * stand.DbhGrowth[treeIndex];
                    float expansionFactor = stand.LiveExpansionFactor[treeIndex] - XIND * stand.DeadExpansionFactor[treeIndex];
                    Debug.Assert(expansionFactor >= 0.0F);
                    Debug.Assert(expansionFactor <= 1000.0F);

                    int ID = (int)dbhInInches + 1;
                    if (ID > 99)
                    {
                        ID = 99;
                    }
                    HTCL[ID] = HTCL[ID] + heightInFeet * expansionFactor;
                    DCL[ID] = DCL[ID] + dbhInInches * expansionFactor;
                    TRCL[ID] = TRCL[ID] + expansionFactor;
                }
            }

            float TOTHT = 0.0F;
            float TOTD = 0.0F;
            float TOTTR = 0.0F;
            for (int I = 99; I > 0; --I)
            {
                TOTHT += HTCL[I];
                TOTD += DCL[I];
                TOTTR += TRCL[I];
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
