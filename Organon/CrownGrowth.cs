using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class CrownGrowth
    {
        public static void CALC_CC(OrganonVariant variant, int speciesGroup, float HLCW, float LCW, float HT, float DBH, float HCB, float EXPAN, float[] CCH)
        {
            float XHLCW;
            float XLCW;
            if (HCB > HLCW)
            {
                XHLCW = HCB;
                switch (variant.Variant)
                {
                    case Variant.Swo:
                        CW_SWO(speciesGroup, HLCW, LCW, HT, DBH, XHLCW, out XLCW);
                        break;
                    case Variant.Nwo:
                        CW_NWO(speciesGroup, HLCW, LCW, HT, DBH, XHLCW, out XLCW);
                        break;
                    case Variant.Smc:
                        CW_SMC(speciesGroup, HLCW, LCW, HT, DBH, XHLCW, out XLCW);
                        break;
                    case Variant.Rap:
                        CW_RAP(speciesGroup, HLCW, LCW, HT, DBH, XHLCW, out XLCW);
                        break;
                    default:
                        throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
                }
            }
            else
            {
                XHLCW = HLCW;
                XLCW = LCW;
            }
            for (int II = 39; II >= 0; --II)
            {
                int L = II - 1;
                float XL = (float)(L) * (CCH[40] / 40.0F);
                float CW;
                if (XL <= XHLCW)
                {
                    CW = XLCW;
                }
                else if (XL > XHLCW && XL < HT)
                {
                    switch (variant.Variant)
                    {
                        case Variant.Swo:
                            CW_SWO(speciesGroup, HLCW, LCW, HT, DBH, XL, out CW);
                            break;
                        case Variant.Nwo:
                            CW_NWO(speciesGroup, HLCW, LCW, HT, DBH, XL, out CW);
                            break;
                        case Variant.Smc:
                            CW_SMC(speciesGroup, HLCW, LCW, HT, DBH, XL, out CW);
                            break;
                        case Variant.Rap:
                            CW_RAP(speciesGroup, HLCW, LCW, HT, DBH, XL, out CW);
                            break;
                        default:
                            throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
                    }
                }
                else
                {
                    CW = 0.0F;
                }
                float CA = (CW * CW) * (.001803F * EXPAN);
                CCH[II] = CCH[II] + CA;
            }
        }

        /// <summary>
        /// Find crown closure. (DOUG? can this be removed as dead code since the value of CC is never consumed?)
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="CCH"></param>
        /// <param name="CC">Crown closure</param>
        public static void CRNCLO(OrganonVariant variant, Stand stand, float[] CCH, out float CC)
        {
            for (int L = 0; L < 40; ++L)
            {
                CCH[L] = 0.0F;
            }
            CCH[40] = stand.Height[0];
            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.Height[treeIndex] > CCH[40])
                {
                    CCH[40] = stand.Height[treeIndex];
                }
            }

            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float crownRatio = stand.CrownRatio[treeIndex];
                float crownLengthInFeet = crownRatio * heightInFeet;
                float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                int speciesGroup = stand.SpeciesGroup[treeIndex];
                switch (variant.Variant)
                {
                    case Variant.Swo:
                        MCW_SWO(speciesGroup, dbhInInches, heightInFeet, out float MCW);
                        LCW_SWO(speciesGroup, MCW, crownRatio, dbhInInches, heightInFeet, out float LCW);
                        HLCW_SWO(speciesGroup, heightInFeet, crownRatio, out float HLCW);
                        CALC_CC(variant, speciesGroup, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        break;
                    case Variant.Nwo:
                        MCW_NWO(speciesGroup, dbhInInches, heightInFeet, out MCW);
                        LCW_NWO(speciesGroup, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        HLCW_NWO(speciesGroup, heightInFeet, crownRatio, out HLCW);
                        CALC_CC(variant, speciesGroup, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        break;
                    case Variant.Smc:
                        MCW_SMC(speciesGroup, dbhInInches, heightInFeet, out MCW);
                        LCW_SMC(speciesGroup, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        HLCW_SMC(speciesGroup, heightInFeet, crownRatio, out HLCW);
                        CALC_CC(variant, speciesGroup, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        break;
                    case Variant.Rap:
                        MCW_RAP(speciesGroup, dbhInInches, heightInFeet, out MCW);
                        LCW_RAP(speciesGroup, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        HLCW_RAP(speciesGroup, heightInFeet, crownRatio, out HLCW);
                        CALC_CC(variant, speciesGroup, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        break;
                }
            }
            CC = CCH[0];
        }

        public static void CrowGro(OrganonVariant variant, Stand stand, StandDensity densityBeforeGrowth, StandDensity densityAfterGrowth,
                                   float SI_1, float SI_2, float[,] CALIB, float[] CCH)
        {
            // DETERMINE 5-YR CROWN RECESSION
            Mortality.OldGro(stand, -1.0F, out float OG1);
            Mortality.OldGro(stand, 0.0F, out float OG2);
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                // CALCULATE HCB START OF GROWTH
                // CALCULATE STARTING HEIGHT
                float PHT = stand.Height[treeIndex] - stand.HeightGrowth[treeIndex];
                // CALCULATE STARTING DBH
                float PDBH = stand.Dbh[treeIndex] - stand.DbhGrowth[treeIndex];
                int speciesGroup = stand.SpeciesGroup[treeIndex];
                float SCCFL1 = densityBeforeGrowth.GetCrownCompetitionFactorLarger(PDBH);
                float PCR1;
                switch (variant.Variant)
                {
                    case Variant.Swo:
                        HCB_SWO(speciesGroup, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1, out float HCB1);
                        PCR1 = 1.0F - HCB1 / PHT;
                        break;
                    case Variant.Nwo:
                        HCB_NWO(speciesGroup, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1, out HCB1);
                        PCR1 = CALIB[speciesGroup, 1] * (1.0F - HCB1 / PHT);
                        break;
                    case Variant.Smc:
                        HCB_SMC(speciesGroup, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1, out HCB1);
                        PCR1 = 1.0F - HCB1 / PHT;
                        break;
                    case Variant.Rap:
                        HCB_RAP(speciesGroup, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1, out HCB1);
                        PCR1 = 1.0F - HCB1 / PHT;
                        break;
                    default:
                        throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
                }

                float PHCB1 = (1.0F - PCR1) * PHT;

                // CALCULATE HCB END OF GROWTH
                float HT = stand.Height[treeIndex];
                float DBH = stand.Dbh[treeIndex];
                float SCCFL2 = densityAfterGrowth.GetCrownCompetitionFactorLarger(DBH);
                float MAXHCB;
                float PCR2;
                switch (variant.Variant)
                {
                    case Variant.Swo:
                        HCB_SWO(speciesGroup, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2, out float HCB2);
                        MAXHCB_SWO(speciesGroup, HT, SCCFL2, out MAXHCB);
                        PCR2 = 1.0F - HCB2 / HT;
                        break;
                    case Variant.Nwo:
                        HCB_NWO(speciesGroup, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2, out HCB2);
                        MAXHCB_NWO(speciesGroup, HT, SCCFL2, out MAXHCB);
                        PCR2 = CALIB[speciesGroup, 1] * (1.0F - HCB2 / HT);
                        break;
                    case Variant.Smc:
                        HCB_SMC(speciesGroup, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2, out HCB2);
                        MAXHCB_SMC(speciesGroup, HT, SCCFL2, out MAXHCB);
                        PCR2 = 1.0F - HCB2 / HT;
                        break;
                    case Variant.Rap:
                        HCB_RAP(speciesGroup, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2, out HCB2);
                        MAXHCB_RAP(speciesGroup, HT, SCCFL2, out MAXHCB);
                        PCR2 = 1.0F - HCB2 / HT;
                        break;
                    default:
                        throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
                }

                float PHCB2 = (1.0F - PCR2) * HT;

                // DETERMINE CROWN GROWTH
                float HCBG = PHCB2 - PHCB1;
                if (HCBG < 0.0F)
                {
                    HCBG = 0.0F;
                }
                Debug.Assert(HCBG >= 0.0F); // catch NaNs

                float AHCB1 = (1.0F - stand.CrownRatio[treeIndex]) * PHT;
                float AHCB2 = AHCB1 + HCBG;
                if (AHCB1 >= MAXHCB)
                {
                    stand.CrownRatio[treeIndex] = 1.0F - AHCB1 / HT;
                }
                else if (AHCB2 >= MAXHCB)
                {
                    stand.CrownRatio[treeIndex] = 1.0F - MAXHCB / HT;
                }
                else
                {
                    stand.CrownRatio[treeIndex] = 1.0F - AHCB2 / HT;
                }
                Debug.Assert((stand.CrownRatio[treeIndex] >= 0.0F) && (stand.CrownRatio[treeIndex] <= 1.0F));
            }

            CRNCLO(variant, stand, CCH, out float _);
        }

        private static void CW_NWO(int ISPGRP, float HLCW, float LCW, float HT, float DBH, float XL, out float CW)
        {
            // CROWN WIDTH ABOVE LARGEST CROWN WIDTH(3 parameters - all species)
            //
            // DF Coefficients from Hann(1999) FS 45: 217-225
            // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WH Coefficients from Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
            // RC Coefficients from WH Hann and Hanus(2001) FRL Research Contribution 34
            // PY Coefficients from WH Hann and Hanus(2001) FRL Research Contribution 34
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            float[,] CWAPAR = {
                {
                   0.929973F ,  0.999291F ,  0.461782F ,   0.629785F,     // DF,GF,WH,RC
                   0.629785F,   0.5F      ,  0.5F      ,   0.5F     ,     // PY,MD,BL,WO
                   0.5F      ,  0.5F      ,  0.5F                         // RA,PD,WI
                },
                {
                  -0.135212F ,  0.0F      ,  0.552011F ,   0.0F     ,     // DF,GF,WH,RC
                   0.0F      ,  0.0F      ,  0.0F      ,   0.0F     ,     // PY,MD,BL,WO
                   0.0F      ,  0.0F      ,  0.0F                         // RA,PD,WI
                },
                {
                  -0.0157579F, -0.0314603F,  0.0F      ,   0.0F     ,     // DF,GF,WH,RC
                   0.0F      ,  0.0F      ,  0.0F      ,   0.0F     ,     // PY,MD,BL,WO
                   0.0F      ,  0.0F      ,  0.0F                         // RA,PD,WI
                }
            };

            float B1 = CWAPAR[0, ISPGRP];
            float B2 = CWAPAR[1, ISPGRP];
            float B3 = CWAPAR[2, ISPGRP];
            float RP = (HT - XL) / (HT - HLCW);
            float RATIO = HT / DBH;
            if (ISPGRP == 0)
            {
                if (RATIO > 50.0F)
                {
                    RATIO = 50.0F;
                }
            }
            else if (ISPGRP == 1)
            {
                if (RATIO > 31.0F)
                {
                    RATIO = 31.0F;
                }
            }
            CW = (float)(LCW * Math.Pow(RP, B1 + B2 * Math.Sqrt(RP) + B3 * (RATIO)));
        }

        private static void CW_RAP(int ISPGRP, float HLCW, float LCW, float HT, float DBH, float XL, out float CW)
        {
            // CROWN WIDTH ABOVE LARGEST CROWN WIDTH(3 parameters - all species)
            //
            // RA Coefficients from Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
            // DF Coefficients from Hann(1999) FS 45: 217-225
            // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RC Coefficients from WH
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            float[,] CWAPAR = {
                {
                  0.63420194F,  0.929973F ,  0.629785F ,   0.629785F,     // RA,DF,WH,RC
                  0.5F       ,   0.5F     ,  0.5F      ,                 // BL,PD,WI
                },
                {
                  0.17649614F, -0.135212F ,  0.0F      ,   0.0F     ,     // RA,DF,WH,RC
                  0.0F       ,  0.0F      ,  0.0F      ,                 // BL,PD,WI
                },
                {
                 -0.02315018F, -0.0157579F,  0.0F      ,   0.0F     ,     // RA,DF,WH,RC
                  0.0F       ,  0.0F      ,  0.0F                       // BL,PD,WI
                }
            };

            float B1 = CWAPAR[0, ISPGRP];
            float B2 = CWAPAR[1, ISPGRP];
            float B3 = CWAPAR[2, ISPGRP];
            float RP = (HT - XL) / (HT - HLCW);
            float RATIO = HT / DBH;
            if (ISPGRP == 1)
            {
                if (RATIO > 50.0F)
                {
                    RATIO = 50.0F;
                }
            }
            CW = (float)(LCW * Math.Pow(RP, B1 + B2 * Math.Sqrt(RP) + B3 * (RATIO)));
        }

        private static void CW_SMC(int ISPGRP, float HLCW, float LCW, float HT, float DBH, float XL, out float CW)
        {
            // CROWN WIDTH ABOVE LARGEST CROWN WIDTH(3 parameters - all species)
            //
            // DF Coefficients from Hann(1999) FS 45: 217-225
            // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RC Coefficients from WH
            // PY Coefficients from WH
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            float[,] CWAPAR = {
                {
                   0.929973F ,  0.999291F ,  0.629785F ,   0.629785F,     // DF,GF,WH,RC
                   0.629785F,   0.5F      ,  0.5F      ,   0.5F     ,     // PY,MD,BL,WO
                   0.5F      ,  0.5F      ,  0.5F      ,                  // RA,PD,WI
                },
                {
                  -0.135212F ,  0.0F      ,  0.0F      ,   0.0F     ,     // DF,GF,WH,RC
                   0.0F      ,  0.0F      ,  0.0F      ,   0.0F     ,     // PY,MD,BL,WO
                   0.0F      ,  0.0F      ,  0.0F      ,                  // RA,PD,WI
                },
                {
                  -0.0157579F, -0.0314603F,  0.0F      ,   0.0F     ,     // DF,GF,WH,RC
                   0.0F      ,  0.0F      ,  0.0F      ,   0.0F     ,     // PY,MD,BL,WO
                   0.0F      ,  0.0F      ,  0.0F                         // RA,PD,WI
                }
            };

            float B1 = CWAPAR[0, ISPGRP];
            float B2 = CWAPAR[1, ISPGRP];
            float B3 = CWAPAR[2, ISPGRP];
            float RP = (HT - XL) / (HT - HLCW);
            float RATIO = HT / DBH;
            if (ISPGRP == 0)
            {
                if (RATIO > 50.0F)
                {
                    RATIO = 50.0F;
                }
            }
            else if (ISPGRP == 1)
            {
                if (RATIO > 31.0)
                {
                    RATIO = 31.0F;
                }
            }
            CW = (float)(LCW * Math.Pow(RP, B1 + B2 * Math.Sqrt(RP) + B3 * (RATIO)));
        }

        private static void CW_SWO(int ISPGRP, float HLCW, float LCW, float HT, float DBH, float XL, out float CW)
        {
            // CROWN WIDTH ABOVE LARGEST CROWN WIDTH(3 parameters - all species)
            //
            // DF Coefficients from Hann(1999) FS 45: 217-225
            // GW Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // SP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // IC Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RC Coefficients from IC
            // PY Coefficients from WH
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // GC Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // TA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // CL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            float[,] CWAPAR = {
                {
                     0.929973F,   0.999291F,   0.755583F,   0.755583F,  0.629785F, // DF,GW,PP,SP,IC,
                     0.629785F,   0.629785F,   0.629785F,   0.5F     ,  0.5F     , // WH,RC,PY,MD,GC,
                     0.5F     ,   0.5F     ,   0.5F     ,   0.5F     ,  0.5F     , // TA,CL,BL,WO,BO,
                     0.5F     ,   0.5F     ,   0.5F     ,                        // RA,PD,WI
                },
                {
                    -0.135212F,   0.0F     ,   0.0F     ,   0.0F     ,   0.0F     ,// DF,GW,PP,SP,IC,
                     0.0F     ,   0.0F     ,   0.0F     ,   0.0F     ,   0.0F     ,// WH,RC,PY,MD,GC,
                     0.0F     ,   0.0F     ,   0.0F     ,   0.0F     ,   0.0F     ,// TA,CL,BL,WO,BO,
                     0.0F     ,   0.0F     ,   0.0F     ,                        // RA,PD,WI
                },
                {
                    -0.0157579F, -0.0314603F,   0.0F     ,   0.0F     ,   0.0F     ,// DF,GW,PP,SP,IC,
                     0.0F      ,  0.0F      ,   0.0F     ,   0.0F     ,   0.0F     ,// WH,RC,PY,MD,GC,
                     0.0F      ,  0.0F      ,   0.0F     ,   0.0F     ,   0.0F     ,// TA,CL,BL,WO,BO,
                     0.0F      ,  0.0F      ,   0.0F                            // RA,PD,WI
                }
            };

            float B1 = CWAPAR[0, ISPGRP];
            float B2 = CWAPAR[1, ISPGRP];
            float B3 = CWAPAR[2, ISPGRP];
            float RP = (HT - XL) / (HT - HLCW);
            float RATIO = HT / DBH;
            if (ISPGRP == 0)
            {
                if (RATIO > 50.0F)
                {
                    RATIO = 50.0F;
                }
            }
            else if (ISPGRP == 1)
            {
                if (RATIO > 31.0F)
                {
                    RATIO = 31.0F;
                }
            }
            CW = (float)(LCW * Math.Pow(RP, (B1 + B2 * Math.Sqrt(RP) + B3 * (RATIO))));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ISPGRP">Tree's species group.</param>
        /// <param name="HT">Tree height (feet).</param>
        /// <param name="DBH">Tree's diameter at breast height (inches)</param>
        /// <param name="CCFL"></param>
        /// <param name="BA">Stand basal area.</param>
        /// <param name="SI_1">Stand site index.</param>
        /// <param name="SI_2">Stand site index.</param>
        /// <param name="OG"></param>
        /// <param name="HCB">Height to crown base (feet).</param>
        public static void HCB_NWO(int ISPGRP, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG, out float HCB)
        {
            // HEIGHT TO CROWN BASE(7 parameters - all species)
            // 
            // DF Coefficients from Zumrawi and Hann (1989) FRL Research Paper 52
            // GF Coefficients from Zumrawi and Hann (1989) FRL Research Paper 52
            // WH Coefficients from Johnson (2002) Willamette Industries Report
            // RC Coefficients from Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // MD Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // BL Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // WO Coefficients from Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26-33
            // RA Coefficients from Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // WI Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // 
            float[,] HCBPAR = {
                {
                 1.94093F    ,  1.04746F    ,  1.92682F    ,  4.49102006F ,   // DF,GF,WH,RC
                 0.0F        ,  2.955339267F,  0.9411395642F, 1.05786632F ,   // PY,MD,BL,WO
                 0.56713781F ,  0.0F        ,  0.0F                           // RA,PD,WI
                },
                {
                -0.0065029F  , -0.0066643F  , -0.00280478F ,  0.0F        ,   // DF,GF,WH,RC
                 0.0F        ,  0.0F        , -0.00768402F ,  0.0F        ,   // PY,MD,BL,WO
                -0.010377976F,  0.0F        ,  0.0F                           // RA,PD,WI
                },
                {
                -0.0048737F  , -0.0067129F  , -0.0011939F  , -0.00132412F ,   // DF,GF,WH,RC
                 0.0F        ,  0.0F        , -0.005476131F, -0.00183283F ,   // PY,MD,BL,WO
                -0.002066036F, -0.005666559F, -0.005666559F                   // RA,PD,WI
                },
                {
                -0.261573F   ,  0.0F        , -0.513134F   , -1.01460531F ,   // DF,GF,WH,RC
                 0.0F        , -0.798610738F,  0.0F        , -0.28644547F ,   // PY,MD,BL,WO
                 0.0F        , -0.745540494F, -0.745540494F                   // RA,PD,WI
                },
                {
                 1.08785F    ,  0.0F        ,  3.68901F    ,  0.0F        ,   // DF,GF,WH,RC
                 2.030940382F,  3.095269471F,  0.0F        ,  0.0F        ,   // PY,MD,BL,WO
                 1.39796223F ,  0.0F        ,  0.0F                           // RA,PD,WI
                },
                {
                 0.0F        ,  0.0F        ,  0.00742219F ,  0.01340624F ,   // DF,GF,WH,RC
                 0.0F        ,  0.0F        ,  0.0F        ,  0.0F        ,   // PY,MD,BL,WO
                 0.0F        ,  0.038476613F,  0.038476613F,                  // RA,PD,WI
                },
                {
                 0.0F        ,  0.0F        ,  0.0F        ,  0.0F        ,   // DF,GF,WH,RC
                 0.0F        ,  0.700465646F,  0.0F        ,  0.0F        ,   // PY,MD,BL,WO
                 0.0F        ,  0.0F        ,  0.0F                           // RA,PD,WI
                }
            };

            float B0 = HCBPAR[0, ISPGRP];
            float B1 = HCBPAR[1, ISPGRP];
            float B2 = HCBPAR[2, ISPGRP];
            float B3 = HCBPAR[3, ISPGRP];
            float B4 = HCBPAR[4, ISPGRP];
            float B5 = HCBPAR[5, ISPGRP];
            float B6 = HCBPAR[6, ISPGRP];
            if (ISPGRP == 2)
            {
                HCB = (float)(HT / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SI_2 + B6 * OG * OG)));
            }
            else
            {
                HCB = (float)(HT / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SI_1 + B6 * OG * OG)));
            }
            Debug.Assert(HCB >= 0.0F);
            Debug.Assert(HCB <= HT);
        }

        public static void HCB_RAP(int ISPGRP, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG, out float HCB)
        {
            // HEIGHT TO CROWN BASE(7 parameters - all species)
            // 
            // RA Coefficients from Hann, Bluhm, and Hibbs (2011) Development and Evaluation of the Tree-Level Equations and Their Combined 
            //    Stand-Level Behavior in the Red Alder Plantation Version of Organon
            // DF Coefficients from Hann and Hanus (2004) FS 34: 1193-2003
            // WH Coefficients from Johnson (2002) Willamette Industries Report
            // RC Coefficients from Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
            // BL Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // PD Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // WI Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // 
            float[,] HCBPAR = {
                {
                    3.73113020F  ,  6.18464679F,  1.92682F   ,  4.49102006F,   // RA,DF,WH,RC
                    0.9411395642F,  0.0F       ,  0.0F       ,                 // BL,PD,WI
                },
                {
                   -0.021546486F , -0.00328764F , -0.00280478F ,  0.0F       ,   // RA,DF,WH,RC
                   -0.00768402F  ,  0.0F        ,  0.0F       ,                 // BM,PD,WI
                },
                {
                   -0.0016572840F, -0.00136555F , -0.0011939F  , -0.00132412F ,   // RA,DF,WH,RC
                   -0.005476131F , -0.005666559F, -0.005666559F,                 // BL,PD,WI
                },
                {
                    -1.0649544F   , -1.19702220F , -0.513134F   , -1.01460531F,   // RA,DF.WH,RC
                    0.0F          , -0.745540494F, -0.745540494F,                 // BL,PD,WI
                },
                {
                    7.47699601F ,  3.17028263F,  3.68901F   ,  0.0F       ,   // RA,DF,WH,RC
                    0.0F        ,  0.0F       ,  0.0F       ,                 // BL,PD,WI
                },
                {
                    0.0252953320F,   0.0F        ,  0.00742219F ,  0.01340624F ,   // RA,DF,WH,RC
                    0.0F         ,   0.038476613F,  0.038476613F,                 // BL,PD,WI
                },
                {
                    0.0F       ,   0.0F       ,  0.0F       ,  0.0F       ,   // RA,DF,WH,RC
                    0.0F       ,   0.0F       ,  0.0F       ,                 // BL,PD,WI
                },
                {
                    1.6F       ,   0.0F       ,  0.0F       ,  0.0F       ,   //RA,DF,WH,RC
                    0.0F       ,   0.0F       ,  0.0F                         //BL,PD,WI
                }
            };

            float B0 = HCBPAR[0, ISPGRP];
            float B1 = HCBPAR[1, ISPGRP];
            float B2 = HCBPAR[2, ISPGRP];
            float B3 = HCBPAR[3, ISPGRP];
            float B4 = HCBPAR[4, ISPGRP];
            float B5 = HCBPAR[5, ISPGRP];
            float B6 = HCBPAR[6, ISPGRP];
            float K = HCBPAR[7, ISPGRP];
            if (ISPGRP == 0)
            {
                HCB = (float)((HT - K) / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SI_1 + B6 * OG * OG)) + K);
            }
            else
            {
                float SITE = SI_2;
                if (ISPGRP == 2)
                {
                    SITE = 0.480F + (1.110F * (SI_2 + 4.5F));
                }
                HCB = (float)(HT / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SITE + B6 * OG * OG)));
            }
            Debug.Assert(HCB >= 0.0F);
            Debug.Assert(HCB <= HT);
        }

        public static void HCB_SMC(int ISPGRP, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG, out float HCB)
        {
            // 
            // HEIGHT TO CROWN BASE(7 parameters - all species)
            // 
            // DF Coefficients from Hann and Hanus (2004) FS 34: 1193-2003
            // GF Coefficients from Zumrawi and Hann (1989) FRL Research Paper 52
            // WH Coefficients from Johnson (2002) Willamette Industries Report
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // MD Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // BL Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // WO Coefficients from Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26-33
            // RA Coefficients from Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // WI Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // 
            float[,] HCBPAR = {
                {
                     6.18464679F ,  1.04746F    ,  1.92682F     ,  4.49102006F,   // DF,GF,WH,RC
                     0.0F        ,  2.955339267F,  0.9411395642F,  1.05786632F,   // PY,MD,BL,WO
                     0.56713781F ,  0.0F        ,  0.0F                        // RA,PD,WI
                },
                {
                    -0.00328764F , -0.0066643F , -0.00280478F,  0.0F       ,   // DF,GF,WH,RC
                     0.0F        ,  0.0F       , -0.00768402F,  0.0F       ,   // PY,MD,BL,WO
                    -0.010377976F,  0.0F       ,  0.0F                        // RA,PD,WI
                },
                {
                    -0.00136555F , -0.0067129F  , -0.0011939F  , -0.00132412F,   // DF,GF,WH,RC
                     0.0F        ,  0.0F        , -0.005476131F, -0.00183283F,   // PY,MD,BL,WO
                    -0.002066036F, -0.005666559F, -0.005666559F,                 // RA,PD,WI
                },
                {
                    -1.19702220F,  0.0F        , -0.513134F   , -1.01460531F,   // DF,GF,WH,RC
                     0.0F       , -0.798610738F,  0.0F       , -0.28644547F,   // PY,MD,BL,WO
                     0.0F       , -0.745540494F, -0.745540494F                 // RA,PD,WI
                },
                {
                     3.17028263F ,  0.0F        ,  3.68901F    ,  0.0F       ,   // DF,GF,WH,RC
                     2.030940382F,  3.095269471F,  0.0F        ,  0.0F       ,   // PY,MD,BL,WO
                     1.39796223F ,  0.0F        ,  0.0F                        // RA,PD,WI
                },
                {
                     0.0F       ,  0.0F       ,   0.00742219F ,  0.01340624F ,   // DF,GF,WH,RC
                     0.0F       ,  0.0F       ,   0.0F        ,  0.0F       ,   // PY,MD,BL,WO
                     0.0F       ,  0.038476613F,  0.038476613F                 // RA,PD,WI
                },
                {
                     0.0F       ,  0.0F        ,  0.0F       ,  0.0F       ,   // DF,GF,WH,RC
                     0.0F       ,  0.700465646F,  0.0F       ,  0.0F       ,   // PY,MD,BL,WO
                     0.0F       ,  0.0F        ,  0.0F  // RA,PD,WI
                }
            };

            float B0 = HCBPAR[0, ISPGRP];
            float B1 = HCBPAR[1, ISPGRP];
            float B2 = HCBPAR[2, ISPGRP];
            float B3 = HCBPAR[3, ISPGRP];
            float B4 = HCBPAR[4, ISPGRP];
            float B5 = HCBPAR[5, ISPGRP];
            float B6 = HCBPAR[6, ISPGRP];
            if (ISPGRP == 2)
            {
                HCB = (float)(HT / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SI_2 + B6 * OG * OG)));
            }
            else
            {
                HCB = (float)(HT / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SI_1 + B6 * OG * OG)));
            }
            Debug.Assert(HCB >= 0.0F);
            Debug.Assert(HCB <= HT);
        }

        public static void HCB_SWO(int ISPGRP, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG, out float HCB)
        {
            // REAL * 4 HT,DBH,CCFL,BA,SI_1,SI_2,OG,HCB,HCBPAR[18, 7),B0,B1,B2,B3,
            //1       B4,B5,B6
            // 
            // NEW HEIGHT TO CROWN BASE FOR UNDAMAGED TREES ONLY
            // (7 parameters - all species)
            // 
            // DF Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // GW Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // PP Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // SP Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // IC Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // WH Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // RC Coefficients from Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // MD Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // GC Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // TA Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // CL Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // BL Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // WO Coefficients from Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26 - 33
            // BO Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // RA Coefficients from Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            // WI Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
            float[,] HCBPAR = {
                {
                     1.797136911F,  3.451045887F,  1.656364063F,  3.785155749F,  // DF,GW,PP,SP
                     2.428285297F,  0.0F        ,  4.49102006F ,  0.0F        ,  // IC,WH,RC,PY
                     2.955339267F,  0.544237656F,  0.833006499F,  0.5376600543F, // MD,GC,TA,CL
                     0.9411395642F, 1.05786632F ,  2.60140655F ,  0.56713781F  , // BL,WO,BO,RA
                     0.0F         , 0.0F                                         // PD,WI
                },
                {
                    -0.010188791F, -0.005985239F, -0.002755463F, -0.009012547F, // DF,GW,PP,SP
                    -0.006882851F,  0.0F        ,  0.0F        ,  0.0F        , // IC,WH,RC,PY
                     0.0F        , -0.020571754F, -0.012984204F, -0.018632397F, // MD,GC,TA,CL
                    -0.00768402F ,  0.0F        ,  0.0F        , -0.010377976F, // BL,WO,BO,RA
                     0.0F        ,  0.0F                                        // PD,WI
                },
                {
                    -0.003346230F, -0.003211194F,  0.0F        , -0.003318574F, // DF,GW,PP,SP
                    -0.002612590F,  0.0F        , -0.00132412F ,  0.0F        , // IC,WH,RC,PY
                     0.0F        , -0.004317523F, -0.002704717F,  0.0F        , // MD,GC,TA,CL
                    -0.005476131F, -0.00183283F , -0.002273616F, -0.002066036F, // BL,WO,BO,RA
                    -0.005666559F, -0.005666559F,                             // PD,WI
                },
                {
                    -0.412217810F, -0.671479750F, -0.568302547F, -0.670270058F, // DF,GW,PP,SP
                    -0.572782216F,  0.0F        , -1.01460531F ,  0.0F        , // IC,WH,RC,PY
                    -0.798610738F,  0.0F        ,  0.0F        ,  0.0F        , // MD,GC,TA,CL
                     0.0F        , -0.28644547F , -0.554980629F,  0.0F        , // BL,WO,BO,RA
                    -0.745540494F, -0.745540494F,                               // PD,WI
                },
                {
                     3.958656001F,  3.931095518F,  6.730693919F,  2.758645081F, // DF,GW,PP,SP
                     2.113378338F,  4.801329946F,  0.0F        ,  2.030940382F, // IC,WH,RC,PY
                     3.095269471F,  3.132713612F,  0.0F        ,  0.0F        , // MD,GC,TA,CL
                     0.0F        ,  0.0F        ,  0.0F        ,  1.39796223F , // BL,WO,BO,RA
                     0.0F        ,  0.0F                                        // PD,WI
                },
                {
                     0.008526562F,  0.003115567F,  0.001852526F,  0.0F        , // DF,GW,PP,SP
                     0.008480754F,  0.0F        ,  0.01340624F ,  0.0F        , // IC,WH,RC,PY
                     0.0F        ,  0.0F        ,  0.0F        ,  0.0F        , // MD,GC,TA,CL
                     0.0F        ,  0.0F        ,  0.0F        ,  0.0F        , // BL,WO,BO,RA
                     0.038476613F,  0.038476613F,                               // PD,WI
                },
                {
                     0.448909636F,  0.516180892F,  0.0F        ,  0.841525071F, // DF,GW,PP,SP
                     0.506226895F,  0.0F        ,  0.0F        ,  0.0F        , // IC,WH,RC,PY
                     0.700465646F,  0.483748898F,  0.2491242765F, 0.0F        , // MD,GC,TA,CL
                     0.0F        ,  0.0F        ,  0.0F        ,  0.0F        , // BL,WO,BO,RA
                     0.0F        ,  0.0F                                        // PD,WI
                }
            };

            float B0 = HCBPAR[0, ISPGRP];
            float B1 = HCBPAR[1, ISPGRP];
            float B2 = HCBPAR[2, ISPGRP];
            float B3 = HCBPAR[3, ISPGRP];
            float B4 = HCBPAR[4, ISPGRP];
            float B5 = HCBPAR[5, ISPGRP];
            float B6 = HCBPAR[6, ISPGRP];
            if (ISPGRP == 2)
            {
                HCB = (float)(HT / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SI_2 + B6 * OG * OG)));
            }
            else
            {
                HCB = (float)(HT / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SI_1 + B6 * OG * OG)));
            }
            Debug.Assert(HCB >= 0.0F);
            Debug.Assert(HCB <= HT);
        }

        /// <summary>
        /// Estimate height to largest crown width.
        /// </summary>
        /// <param name="ISPGRP">Tree's species group.</param>
        /// <param name="HT">Tree's height (feet).</param>
        /// <param name="CR">Tree's crown ratio.</param>
        /// <param name="SCR"></param>
        /// <param name="HLCW">Heigh to largest crown width (feet)</param>
        public static void HLCW_NWO(int ISPGRP, float HT, float CR, out float HLCW)
        {
            // DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH(1 parameter - all species)
            //
            // DF Coefficients from Hann(1999) FS 45: 217-225
            // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WH Coefficients from Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
            // RC Coefficients from WH of Hann and Hanus(2001) FRL Research Contribution 34
            // PY Coefficients from WH of Hann and Hanus(2001) FRL Research Contribution 34
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            float[] DACBPAR = {
                   0.062000F, 0.028454F, 0.355270F, 0.209806F, 0.209806F,  // DF,GF,WH,RC,PY
                   0.0F     , 0.0F     , 0.0F     , 0.0F     , 0.0F     ,  // MD,BL,WO,RA,PD
                   0.0F                                                    // WI
            };

            float B1 = DACBPAR[ISPGRP];
            float CL = CR * HT;
            HLCW = HT - (1.0F - B1) * CL;
        }

        public static void HLCW_RAP(int ISPGRP, float HT, float CR, out float HLCW)
        {
            // DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH(1 parameter - all species)
            //
            // RA Coefficients from Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
            // DF Coefficients from Hann(1999) FS 45: 217-225
            // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RC Coefficients from WH
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            float[,] DACBPAR = {
                {
                    0.63619616F, 0.062000F, 0.209806F, 0.209806F, 0.0F,  // RA,DF,WH,RC,BL
                    0.0F, 0.0F,                                // PD,WI
                },
                {
                    -1.2180562F, 0.0F, 0.0F, 0.0F, 0.0F,  // RA,DF,WH,RC,BL
                    0.0F, 0.0F                                     // PD,WI
                }
            };

            float B1 = DACBPAR[0, ISPGRP];
            float B2 = DACBPAR[1, ISPGRP];
            float CL = CR * HT;
            HLCW = (float)(HT - (1.0F - B1 * Math.Exp(Math.Pow(B2 * (1.0F - HT / 140.0F), 3))) * CL);
        }

        public static void HLCW_SMC(int ISPGRP, float HT, float CR, out float HLCW)
        {
            //C DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH(1 parameter - all species)
            //
            // DF Coefficients from Hann(1999) FS 45: 217-225
            // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RC Coefficients from WH
            // PY Coefficients from WH
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            float[] DACBPAR = {
                0.062000F, 0.028454F, 0.209806F, 0.209806F, 0.209806F,  // DF,GF,WH,RC,PY
                0.0F, 0.0F, 0.0F, 0.0F, 0.0F,  // MD,BL,WO,RA,PD
                0.0F                                               // WI
            };

            float B1 = DACBPAR[ISPGRP];
            float CL = CR * HT;
            HLCW = HT - (1.0F - B1) * CL;
        }

        public static void HLCW_SWO(int ISPGRP, float HT, float CR, out float HLCW)
        {
            // DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH(1 parameter - all species)
            //
            // DF Coefficients from Hann(1999) FS 45: 217-225
            // GW Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // SP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // IC Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RC Coefficients from IC
            // PY Coefficients from WH
            // MD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // GC Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // TA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // CL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BL Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // BO Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // RA Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // PD Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            // WI Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
            float[] DACBPAR = {
                   0.062000F, 0.028454F, 0.05F    , 0.05F    , 0.20F    ,  // DF,GW,PP,SP,IC,
                   0.209806F, 0.20F    , 0.209806F, 0.0F     , 0.0F     ,  // WH,RC,PY,MD,GC,
                   0.0F     , 0.0F     , 0.0F     , 0.0F     , 0.0F     ,  // TA,CL,BL,WO,BO,
                   0.0F     , 0.0F     , 0.0F                             // RA,PD,WI
            };

            float B1 = DACBPAR[ISPGRP];
            float CL = CR * HT;
            HLCW = HT - (1.0F - B1) * CL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ISPGRP">Tree's species group.</param>
        /// <param name="MCW">Tree's maximum crown width (feet).</param>
        /// <param name="CR">Tree's crown ratio.</param>
        /// <param name="DBH">Tree's diameter at breast height (inches).</param>
        /// <param name="HT">Tree's height (feet).</param>
        /// <param name="LCW">Tree's largest crown width (feet).</param>
        public static void LCW_NWO(int ISPGRP, float MCW, float CR, float DBH, float HT, out float LCW)
        {
            // LARGEST CROWN WIDTH(3 parameters - all species)
            //
            // DF Coefficients from Hann(1997) FRL Research Contribution 17
            // GF Coefficients from Hann(1997) FRL Research Contribution 17
            // WH Coefficients from Johnson(2002) Willamette Industries Report
            // RC Coefficients from IC of Hann(1997) FRL Research Contribution 17
            // PY Coefficients from WH of Hann(1997) FRL Research Contribution 17
            // MD Coefficients from Hann(1997) FRL Research Contribution 17
            // BL Coefficients from Hann(1997) FRL Research Contribution 17
            // WO Coefficients from Hann(1997) FRL Research Contribution 17
            // RA Coefficients from Hann(1997) FRL Research Contribution 17
            // PD Coefficients from GC of Hann(1997) FRL Research Contribution 17
            // WI Coefficients from GC of Hann(1997) FRL Research Contribution 17
            //
            float[,] LCWPAR = {
                {
                  0.0F       ,  0.0F       ,  0.105590F  , -0.2513890F ,  // DF,GF,WH,RC
                  0.0F       ,  0.118621F  ,  0.0F       ,  0.3648110F ,  // PY,MD,BL,WO
                  0.3227140F ,  0.0F       ,  0.0F       ,               // RA,PD,WI
                },
                {
                  0.00436324F,  0.00308402F,  0.0035662F ,  0.006925120F, // DF,GF,WH,RC
                  0.0F       ,  0.00384872F,  0.0F       ,  0.0F       ,  // PY,MD,BL,WO
                  0.0F       ,  0.0F       ,  0.0F       ,               // RA,PD,WI
                },
                {
                  0.6020020F ,  0.0F       ,  0.0F       ,  0.985922F   , // DF,GF,WH,RC
                  0.0F       ,  0.0F       ,  1.470180F  ,  0.0F        , // PY,MD,BL,WO
                  0.0F       ,  1.61440F   ,  1.61440F                  // RA,PD,WI
                }
            };

            float B1 = LCWPAR[0, ISPGRP];
            float B2 = LCWPAR[1, ISPGRP];
            float B3 = LCWPAR[2, ISPGRP];
            float CL = CR * HT;
            LCW = (float)(MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
        }

        public static void LCW_RAP(int ISPGRP, float MCW, float CR, float DBH, float HT, out float LCW)
        {
            // LARGEST CROWN WIDTH(3 parameters - all species)
            //
            // RA Coefficients from Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
            // DF Coefficients from Hann(1997) FRL Research Contribution 17
            // WH Coefficients from Hann(1997) FRL Research Contribution 17
            // RC Coefficients from IC of Hann(1997) FRL Research Contribution 17
            // BL Coefficients from Hann(1997) FRL Research Contribution 17
            // PD Coefficients from GC of Hann(1997) FRL Research Contribution 17
            // WI Coefficients from GC of Hann(1997) FRL Research Contribution 17
            float[,] LCWPAR = {
                {
                  0.78160725F,  1.0F       ,  1.0F       ,  1.0F       , // RA,DF,WH,RC
                  1.0F       ,  1.0F       ,  1.0F                       // BL,PD,WI
                },
                {
                  0.44092737F,  0.0F       ,  0.0F      , -0.2513890F,  // RA,DF,WH,RC
                  0.0F       ,  0.0F       ,  0.0F                      // BM,PD,WI
                },
                {
                  0.0F       ,  0.00436324F,  0.0F      ,  0.006925120F, // RA,DF,WH,RC
                  0.0F       ,  0.0F       ,  0.0F                       // BL,PD,WI
                },
                {
                  0.0F       ,  0.6020020F ,  0.0F       ,  0.985922F  , // RA,DF,WH,RC
                  1.470180F  ,  1.61440F   ,  1.61440F                   // BL,PD,WI
                }
            };

            float B0 = LCWPAR[0, ISPGRP];
            float B1 = LCWPAR[1, ISPGRP];
            float B2 = LCWPAR[2, ISPGRP];
            float B3 = LCWPAR[3, ISPGRP];
            float CL = CR * HT;
            LCW = (float)(B0 * MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
        }

        public static void LCW_SMC(int ISPGRP, float MCW, float CR, float DBH, float HT, out float LCW)
        {
            // LARGEST CROWN WIDTH(3 parameters - all species)
            //
            // DF Coefficients from Hann(1997) FRL Research Contribution 17
            // GF Coefficients from Hann(1997) FRL Research Contribution 17
            // WH Coefficients from Hann(1997) FRL Research Contribution 17
            // RC Coefficients from IC of Hann(1997) FRL Research Contribution 17
            // PY Coefficients from WH of Hann(1997) FRL Research Contribution 17
            // MD Coefficients from Hann(1997) FRL Research Contribution 17
            // BL Coefficients from Hann(1997) FRL Research Contribution 17
            // WO Coefficients from Hann(1997) FRL Research Contribution 17
            // RA Coefficients from Hann(1997) FRL Research Contribution 17
            // PD Coefficients from GC of Hann(1997) FRL Research Contribution 17
            // WI Coefficients from GC of Hann(1997) FRL Research Contribution 17
            //
            float[,] LCWPAR = {
                {
                    0.0F       ,  0.0F       ,  0.0F       , -0.2513890F,  // DF,GF,WH,RC
                    0.0F       ,  0.118621F  ,  0.0F       ,  0.3648110F,  // PY,MD,BL,WO
                    0.3227140F ,  0.0F       ,  0.0F                       // RA,PD,WI
                },
                {
                    0.00436324F,  0.00308402F,  0.0F       ,  0.006925120F, // DF,GF,WH,RC
                    0.0F       ,  0.00384872F,  0.0F       ,  0.0F        , // PY,MD,BL,WO
                    0.0F       ,  0.0F       ,  0.0F                        // RA,PD,WI
                },
                {
                    0.6020020F ,  0.0F       ,  0.0F       ,  0.985922F   , // DF,GF,WH,RC
                    0.0F       ,  0.0F       ,  1.470180F  ,  0.0F        , // PY,MD,BL,WO
                    0.0F       ,  1.61440F   ,  1.61440F                    // RA,PD,WI
                }
            };

            float B1 = LCWPAR[0, ISPGRP];
            float B2 = LCWPAR[1, ISPGRP];
            float B3 = LCWPAR[2, ISPGRP];
            float CL = CR * HT;
            LCW = (float)(MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
        }

        public static void LCW_SWO(int ISPGRP, float MCW, float CR, float DBH, float HT, out float LCW)
        {
            // LARGEST CROWN WIDTH(3 parameters - all species)
            //
            // DF Coefficients from Hann(1997) FRL Research Contribution 17
            // GW Coefficients from Hann(1997) FRL Research Contribution 17
            // PP Coefficients from Hann(1997) FRL Research Contribution 17
            // SP Coefficients from Hann(1997) FRL Research Contribution 17
            // IC Coefficients from Hann(1997) FRL Research Contribution 17
            // WH Coefficients from Hann(1997) FRL Research Contribution 17
            // RC Coefficients from IC
            // PY Coefficients from WH
            // MD Coefficients from Hann(1997) FRL Research Contribution 17
            // GC Coefficients from Hann(1997) FRL Research Contribution 17
            // TA Coefficients from Hann(1997) FRL Research Contribution 17
            // CL Coefficients from Hann(1997) FRL Research Contribution 17
            // BL Coefficients from Hann(1997) FRL Research Contribution 17
            // WO Coefficients from Hann(1997) FRL Research Contribution 17
            // BO Coefficients from Hann(1997) FRL Research Contribution 17
            // RA Coefficients from Hann(1997) FRL Research Contribution 17
            // PD Coefficients from GC
            // WI Coefficients from GC
            //
            float[,] LCWPAR = {
                {
                    0.0F, 0.0F, 0.355532F, 0.0F,  // DF,GW,PP,SP
                    -0.251389F, 0.0F, -0.251389F, 0.0F,  // IC,WH,RC,PY
                    0.118621F, 0.0F, 0.0F, 0.0F,  // MD,GC,TA,CL
                    0.0F, 0.364811F, 0.0F, 0.3227140F,  // BL,WO,BO,RA
                    0.0F, 0.0F                            // PD,WI
                },
                {
                    0.00371834F, 0.00308402F, 0.0F, 0.00339675F,  // DF,GW,PP,SP
                    0.00692512F, 0.0F, 0.00692512F, 0.0F,  // IC,WH,RC,PY
                    0.00384872F, 0.0F, 0.0111972F, 0.0207676F,  // MD,GC,TA,CL
                    0.0F, 0.0F, 0.0F, 0.0F,  // BL,WO,BO,RA
                    0.0F, 0.0F                             // PD,WI
                },
                {
                    0.808121F, 0.0F, 0.0F, 0.532418F,  // DF,GW,PP,SP
                    0.985922F, 0.0F, 0.985922F, 0.0F,  // IC,WH,RC,PY
                    0.0F, 1.161440F, 0.0F, 0.0F,  // MD,GC,TA,CL
                    1.47018F, 0.0F, 1.27196F, 0.0F,  // BL,WO,BO,RA
                    1.161440F, 1.161440F                                // PD,WI
                }
            };

            float B1 = LCWPAR[0, ISPGRP];
            float B2 = LCWPAR[1, ISPGRP];
            float B3 = LCWPAR[2, ISPGRP];
            float CL = CR * HT;
            LCW = (float)(MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
        }

        private static void MAXHCB_NWO(int ISPGRP, float HT, float CCFL, out float MAXHCB)
        {
            // MAXIMUM HEIGHT TO CROWN BASE
            //(5 parameters - all species)
            //
            float[,] MAXPAR = {
                {
                    0.96F       ,  0.96F       ,  1.01F       ,  0.96F       , // DF,GF,WH,RC
                    0.85F       ,  0.981F      ,  1.0F        ,  1.0F        , // PY,MD,BL,WO
                    0.93F       ,  1.0F        ,  0.985F      ,                // RA,PD,WI
                },
                {
                    0.26F       ,  0.31F       ,  0.36F       ,  0.31F       , // DF,GF,WH,RC
                    0.35F       ,  0.161F      ,  0.45F       ,  0.3F        , // PY,MD,BL,WO
                    0.18F       ,  0.45F       ,  0.285F      ,                // RA,PD,WI
                },
                {
                    -0.900721383F, -2.450718394F, -0.944528054F, -1.059636222F, // DF,GW,WH,RC
                    -0.922868139F, -1.73666044F , -1.020016685F, -0.95634399F , // PY,MD,BL,WO
                    -0.928243505F, -1.020016685F, -0.969750805F,                // RA,PD,WI
                },
                {
                    1.0F        ,  1.0F        ,  0.6F        ,  1.0F        , // DF,GW,WH,RC
                    0.8F        ,  1.0F        ,  1.0F        ,  1.1F        , // PY,MD,BL,WO
                    1.0F        ,  1.0F        ,  0.9F        ,                // RA,PD,WI
                },
                {
                    0.95F       ,  0.95F       ,  0.96F       ,  0.95F       , // DF,GW,WH,RC
                    0.80F       ,  0.98F       ,  0.95F       ,  0.98F       , // PY,MD,BL,WO
                    0.92F       ,  0.95F       ,  0.98F                        // RA,PD,WI
                }
            };

            float B0 = MAXPAR[0, ISPGRP];
            float B1 = MAXPAR[1, ISPGRP];
            float B2 = MAXPAR[2, ISPGRP];
            float B3 = MAXPAR[3, ISPGRP];
            float LIMIT = MAXPAR[4, ISPGRP];
            float MAXBR = (float)(B0 - B1 * Math.Exp(B2 * Math.Pow(CCFL / 100.0, B3)));
            if (MAXBR > LIMIT)
            {
                MAXBR = LIMIT;
            }
            MAXHCB = MAXBR * HT;
            Debug.Assert(MAXHCB >= 0.0F);
            Debug.Assert(MAXHCB <= 500.0F);
        }

        private static void MAXHCB_RAP(int ISPGRP, float HT, float CCFL, out float MAXHCB)
        {
            // MAXIMUM HEIGHT TO CROWN BASE
            //(5 parameters - all species)
            float[,] MAXPAR = {
                {
                    0.93F, 0.96F, 1.01F, 0.96F, // RA,DF,WH,RC
                    1.0F, 1.0F, 0.985F,               // BL,PD,WI
                },
                {
                    0.18F, 0.26F, 0.36F, 0.31F, // RA,DF,WH,RC
                    0.45F, 0.45F, 0.285F,               // BL,PD,WI
                },
                {
                    -0.928243505F, -0.34758F, -0.944528054F, -1.059636222F, // RA,DF,WH,RC
                    -1.020016685F, -1.020016685F, -0.969750805F,               // BL,PD,WI
                },
                {
                    1.0F, 1.5F, 0.6F, 1.0F, // RA,DF,WH,RC
                    1.0F, 1.0F, 0.9F,               // BL,PD,WI
                },
                {
                    0.92F, 0.95F, 0.96F, 0.95F, // RA,DF,WH,RC
                    0.95F, 0.95F, 0.98F                       // BL,PD,WI
                }
            };

            float B0 = MAXPAR[0, ISPGRP];
            float B1 = MAXPAR[1, ISPGRP];
            float B2 = MAXPAR[2, ISPGRP];
            float B3 = MAXPAR[3, ISPGRP];
            float LIMIT = MAXPAR[4, ISPGRP];
            float MAXBR = (float)(B0 - B1 * Math.Exp(B2 * Math.Pow(CCFL / 100.0, B3)));
            if (MAXBR > LIMIT)
            {
                MAXBR = LIMIT;
            }
            MAXHCB = MAXBR * HT;
        }

        private static void MAXHCB_SMC(int ISPGRP, float HT, float CCFL, out float MAXHCB)
        {
            // MAXIMUM HEIGHT TO CROWN BASE
            //(5 parameters - all species)
            //
            float[,] MAXPAR = {
                {
                    0.96F       ,  0.96F       ,  1.01F       ,  0.96F       , // DF,GF,WH,RC
                    0.85F       ,  0.981F      ,  1.0F        ,  1.0F        , // PY,MD,BL,WO
                    0.93F       ,  1.0F        ,  0.985F                      // RA,PD,WI
                },
                {
                    0.26F       ,  0.31F       ,  0.36F       ,  0.31F       , // DF,GF,WH,RC
                    0.35F       ,  0.161F      ,  0.45F       ,  0.3F        , // PY,MD,BL,WO
                    0.18F       ,  0.45F       ,  0.285F      ,                // RA,PD,WI
                },
                {
                    -0.34758F    , -2.450718394F, -0.944528054F, -1.059636222F, // DF,GW,WH,RC
                    -0.922868139F, -1.73666044F , -1.020016685F, -0.95634399F , // PY,MD,BL,WO
                    -0.928243505F, -1.020016685F, -0.969750805F,                // RA,PD,WI
                },
                {
                    1.5F        ,  1.0F        ,  0.6F        ,  1.0F        , // DF,GW,WH,RC
                    0.8F        ,  1.0F        ,  1.0F        ,  1.1F        , // PY,MD,BL,WO
                    1.0F        ,  1.0F        ,  0.9F                         // RA,PD,WI
                },
                {
                    0.95F       ,  0.95F       ,  0.96F       ,  0.95F       , // DF,GW,WH,RC
                    0.80F       ,  0.98F       ,  0.95F       ,  0.98F       , // PY,MD,BL,WO
                    0.92F       ,  0.95F       ,  0.98F                        // RA,PD,WI
                }
            };

            float B0 = MAXPAR[0, ISPGRP];
            float B1 = MAXPAR[1, ISPGRP];
            float B2 = MAXPAR[2, ISPGRP];
            float B3 = MAXPAR[3, ISPGRP];
            float LIMIT = MAXPAR[4, ISPGRP];
            float MAXBR = (float)(B0 - B1 * Math.Exp(B2 * Math.Pow(CCFL / 100.0, B3)));
            if (MAXBR > LIMIT)
            {
                MAXBR = LIMIT;
            }
            MAXHCB = MAXBR * HT;
        }

        private static void MAXHCB_SWO(int ISPGRP, float HT, float CCFL, out float MAXHCB)
        {
            // MAXIMUM HEIGHT TO CROWN BASE (5 parameters - all species)
            float[,] MAXPAR = {
                {
                   0.96F       ,  0.96F       ,  1.01F       ,  1.02F       , // DF,GW,PP,SP
                   0.97F       ,  1.01F       ,  0.96F       ,  0.85F       , // IC,WH,RC,PY
                   0.981F      ,  1.0F        ,  0.98F       ,  1.0F        , // MD,GC,TA,CL
                   1.0F         , 1.0F        ,  1.0F        ,  0.93F       , // BL,WO,BO,RA
                   1.0F         , 0.985F      ,                               // PD,WI
                },
                {
                   0.26F       ,  0.31F       ,  0.36F       ,  0.27F       , // DF,GW,PP,SP
                   0.22F       ,  0.36F       ,  0.31F       ,  0.35F       , // IC,WH,RC,PY
                   0.161F      ,  0.45F       ,  0.33F       ,  0.45F       , // MD,GC,TA,CL
                   0.45F       ,  0.3F        ,  0.2F        ,  0.18F       , // BL,WO,BO,RA
                   0.45F       ,  0.285F      ,                               // PD,WI
                },
                {
                  -0.987864873F, -2.450718394F, -1.041915784F, -0.922718593F, // DF,GW,PP,SP
                  -0.002612590F, -0.944528054F, -1.059636222F, -0.922868139F, // IC,WH,RC,PY
                  -1.73666044F , -1.219919284F, -0.911341687F, -0.922025464F, // MD,GC,TA,CL
                  -1.020016685F, -0.95634399F , -1.053892465F, -0.928243505F, // BL,WO,BO,RA
                  -1.020016685F, -0.969750805F,                               // PD,WI
                },
                {
                   1.0F        ,  1.0F        ,  0.6F        ,  0.4F        , // DF,GW,PP,SP
                   1.0F        ,  0.6F        ,  1.0F        ,  0.8F        , // IC,WH,RC,PY
                   1.0F        ,  1.2F        ,  1.0F        ,  1.0F        , // MD,GC,TA,CL
                   1.0F        ,  1.1F        ,  1.0F        ,  1.0F        , // BL,WO,BO,RA
                   1.0F        ,  0.9F        ,                               // PD,WI
                },
                {
                   0.95F       ,  0.95F       ,  0.95F       ,  0.96F       , // DF,GW,PP,SP
                   0.95F       ,  0.96F       ,  0.95F       ,  0.80F       , // IC,WH,RC,PY
                   0.98F       ,  0.98F       ,  0.97F       ,  0.98F       , // MD,GC,TA,CL
                   0.95F       ,  0.98F       ,  0.98F       ,  0.92F       , // BL,WO,BO,RA
                   0.95F       ,  0.98F                                       // PD,WI
                }
            };

            float B0 = MAXPAR[0, ISPGRP];
            float B1 = MAXPAR[1, ISPGRP];
            float B2 = MAXPAR[2, ISPGRP];
            float B3 = MAXPAR[3, ISPGRP];
            float LIMIT = MAXPAR[4, ISPGRP];
            float MAXBR = (float)(B0 - B1 * Math.Exp(B2 * Math.Pow(CCFL / 100.0, B3)));
            if (MAXBR > LIMIT)
            {
                MAXBR = LIMIT;
            }
            MAXHCB = MAXBR * HT;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ISPGRP">Tree's species group.</param>
        /// <param name="D">Tree's diameter at breast height (inches).</param>
        /// <param name="H">Tree's height (feet).</param>
        /// <param name="MCW">Estimated maximum crown width.</param>
        public static void MCW_NWO(int ISPGRP, float D, float H, out float MCW)
        {
            // DETERMINE MCW FOR EACH TREE
            // MAXIMUM CROWN WIDTH(4 parameters - all species)
            //
            // DF Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // GF Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // WH Coefficients from Johnson(2002) Willamette Industries Report
            // RC Coefficients from Smith(1966) Proc. 6th World Forestry Conference
            // PY Coefficients from WH of Paine and Hann(1982) FRL Research Paper 46
            // MD Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // BL Coefficients from Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
            // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
            // RA Coefficients from Smith(1966) Proc. 6th World Forestry Conference
            // PD Coefficients from GC of Paine and Hann(1982) FRL Research Paper 46
            // WI Coefficients from GC of Paine and Hann(1982) FRL Research Paper 46
            float[,] MCWPAR = {
                {
                     4.6198F    ,   6.1880F    ,   4.3586F    ,   4.0F       ,// DF,GF,WH,RC
                     4.5652F    ,   3.4298629F ,   4.0953F    ,   3.0785639F ,// PY,MD,BL,WO
                     8.0F       ,   2.9793895F ,   2.9793895F                 // RA,PD,WI
                },
                {
                     1.8426F    ,   1.0069F    ,   1.57458F   ,   1.65F      ,// DF,GF,WH,RC
                     1.4147F    ,   1.3532302F ,   2.3849F    ,   1.9242211F ,// PY,MD,BL,WO
                     1.53F      ,   1.5512443F ,   1.5512443F                 // RA,PD,WI
                },
                {
                    -0.011311F  ,   0.0F       ,   0.0F       ,   0.0F       ,// DF,GF,WH,RC
                     0.0F       ,   0.0F       ,  -0.0102651F ,   0.0F       ,// PY,MD,BL,WO
                     0.0F       ,  -0.01416129F,  -0.01416129F                // RA,PD,WI
                },
                {
                    81.45F      , 999.99F      ,  76.70F      , 999.99F      ,// DF,GF,WH,RC
                   999.99F      , 999.99F      , 102.53F      , 999.99F      ,// PY,MD,BL,WO
                   999.99F      ,  54.77F      ,  54.77F                      // RA,PD,WI
                }
            };

            float DBH = D;
            float HT = H;
            float B0 = MCWPAR[0, ISPGRP];
            float B1 = MCWPAR[1, ISPGRP];
            float B2 = MCWPAR[2, ISPGRP];
            float PKDBH = MCWPAR[3, ISPGRP];
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * DBH + B2 * DBH * DBH;
            }
        }

        public static void MCW_RAP(int ISPGRP, float D, float H, out float MCW)
        {
            // DETERMINE MCW FOR EACH TREE
            // MAXIMUM CROWN WIDTH(4 parameters - all species)
            //
            // RA Coefficients from Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
            // DF Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // WH Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // RC Coefficients from Smith(1966) Proc. 6th World Forestry Conference
            // BL Coefficients from Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
            // PD Coefficients from GC of Paine and Hann (1982) FRL Research Paper 46
            // WI Coefficients from GC of Paine and Hann(1982) FRL Research Paper 46
            float[,] MCWPAR = {
                {
                    2.320746348F, 4.6198F, 4.5652F, 4.0F,// RA,DF,WH,RC
                    4.0953F, 2.9793895F, 2.9793895F,              // BL,PD,WI
                },
                {
                    6.661401926F, 1.8426F, 1.4147F, 1.65F,// RA,DF,WH,RC
                    2.3849F, 1.5512443F, 1.5512443F,              // BL,PD,WI
                },
                {
                    0.0F, -0.011311F, 0.0F, 0.0F,// RA,DF,WH,RC
                    -0.011630F, -0.01416129F, -0.01416129F,              // BL,PD,WI
                },
                {
                    0.6F, 1.0F, 1.0F, 1.0F,// RA,DF,WH,RC
                    1.0F, 1.0F, 1.0F,              // BL,PD,WI
                },
                {
                    999.99F, 81.45F, 999.99F, 999.99F,// RA,DF,WH,RC
                    102.53F, 54.77F, 54.77F                    // BL,PD,WI
                }
            };

            float DBH = D;
            float HT = H;
            float B0 = MCWPAR[0, ISPGRP];
            float B1 = MCWPAR[1, ISPGRP];
            float B2 = MCWPAR[2, ISPGRP];
            float K = MCWPAR[3, ISPGRP];
            float PKDBH = MCWPAR[4, ISPGRP];
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * (float)Math.Pow(DBH, K) + B2 * DBH * DBH;
            }
        }

        public static void MCW_SMC(int ISPGRP, float D, float H, out float MCW)
        {
            // DETERMINE MCW FOR EACH TREE
            // MAXIMUM CROWN WIDTH(4 parameters - all species)
            //
            // DF Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // GF Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // WH Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // RC Coefficients from Smith(1966) Proc. 6th World Forestry Conference
            // PY Coefficients from WH of Paine and Hann(1982) FRL Research Paper 46
            // MD Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // BL Coefficients from Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
            // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
            // RA Coefficients from Smith(1966) Proc. 6th World Forestry Conference
            // PD Coefficients from GC of Paine and Hann(1982) FRL Research Paper 46
            // WI Coefficients from GC of Paine and Hann(1982) FRL Research Paper 46
            float[,] MCWPAR = {
                {
                    4.6198F, 6.1880F, 4.5652F, 4.0F,// DF,GF,WH,RC
                    4.5652F, 3.4298629F, 4.0953F, 3.0785639F,// PY,MD,BL,WO
                    8.0F, 2.9793895F, 2.9793895F,              // RA,PD,WI
                },
                {
                    1.8426F, 1.0069F, 1.4147F, 1.65F,// DF,GF,WH,RC
                    1.4147F, 1.3532302F, 2.3849F, 1.9242211F,// PY,MD,BL,WO
                    1.53F, 1.5512443F, 1.5512443F,              // RA,PD,WI
                },
                {
                    -0.011311F, 0.0F, 0.0F, 0.0F,// DF,GF,WH,RC
                    0.0F, 0.0F, -0.011630F, 0.0F,// PY,MD,BL,WO
                    0.0F, -0.01416129F, -0.01416129F,              // RA,PD,WI
                },
                {
                    81.45F, 999.99F, 999.99F, 999.99F,// DF,GF,WH,RC
                    999.99F, 999.99F, 102.53F, 999.99F,// PY,MD,BL,WO
                    999.99F, 54.77F, 54.77F                     // RA,PD,WI
                }
            };

            float DBH = D;
            float HT = H;
            float B0 = MCWPAR[0, ISPGRP];
            float B1 = MCWPAR[1, ISPGRP];
            float B2 = MCWPAR[2, ISPGRP];
            float PKDBH = MCWPAR[3, ISPGRP];
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * DBH + B2 * DBH * DBH;
            }
        }

        public static void MCW_SWO(int ISPGRP, float D, float H, out float MCW)
        {
            // DETERMINE MCW FOR EACH TREE
            //**********************************************************************
            //
            // MCW = MCW VALUE
            //
            //
            // MAXIMUM CROWN WIDTH(4 parameters - all species)
            //
            // DF Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // GW Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // PP Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // SP Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // IC Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // WH Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // RC Coefficients from Smith(1966) Proc. 6th World Forestry Conference
            // PY Coefficients from WH
            // MD Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // GC Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // TA Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // CL Coefficients from TA
            // BL Coefficients from Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
            // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
            // BO Coefficients from Paine and Hann(1982) FRL Research Paper 46
            // RA Coefficients from Smith(1966) Proc. 6th World Forestry Conference
            // PD Coefficients from GC
            // WI Coefficients from GC
            //
            float[,] MCWPAR = {
                {
                    4.6366F    ,   6.1880F    ,   3.4835F    ,   4.6600546F , // DF,GW,PP,SP
                    3.2837F    ,   4.5652F    ,   4.0F       ,   4.5652F    , // IC,WH,RC,PY
                    3.4298629F ,   2.9793895F ,   4.4443F    ,   4.4443F    , // MD,GC,TA,CL
                    4.0953F    ,   3.0785639F ,   3.3625F    ,   8.0F       , // BL,WO,BO,RA
                    2.9793895F ,   2.9793895F ,                             // PD,WI
                },
                {
                    1.6078F    ,   1.0069F    ,   1.343F     ,   1.0701859F , // DF,GW,PP,SP
                    1.2031F    ,   1.4147F    ,   1.65F      ,   1.4147F    , // IC,WH,RC,PY
                    1.3532302F ,   1.5512443F ,   1.7040F    ,   1.7040F    , // MD,GC,TA,CL
                    2.3849F    ,   1.9242211F ,   2.0303F    ,   1.53F      , // BL,WO,BO,RA
                    1.5512443F ,   1.5512443F ,                             // PD,WI
                },
                {
                    -0.009625F  ,   0.0F       ,  -0.0082544F ,   0.0F       , // DF,GW,PP,SP
                    -0.0071858F ,   0.0F       ,   0.0F       ,   0.0F       , // IC,WH,RC,PY
                     0.0F       ,  -0.01416129F,   0.0F       ,   0.0F       , // MD,GC,TA,CL
                    -0.011630F  ,   0.0F       ,  -0.0073307F ,   0.0F       , // BL,WO,BO,RA
                    -0.01416129F,  -0.01416129F,                             // PD,WI
                },
                {
                    88.52F      , 999.99F      ,  81.35F      , 999.99F      , // DF,GW,PP,SP
                    83.71F      , 999.99F      , 999.99F      , 999.99F      , // IC,WH,RC,PY
                    999.99F     ,  54.77F      , 999.99F      , 999.99F      , // MD,GC,TA,CL
                    102.53F     , 999.99F      , 138.93F      , 999.99F      , // BL,WO,BO,RA
                    54.77F      ,  54.77F                                   // PD,WI
                }
            };

            float DBH = D;
            float HT = H;
            float B0 = MCWPAR[0, ISPGRP];
            float B1 = MCWPAR[1, ISPGRP];
            float B2 = MCWPAR[2, ISPGRP];
            float PKDBH = MCWPAR[3, ISPGRP];
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * DBH + B2 * DBH * DBH;
            }
        }
    }
}
