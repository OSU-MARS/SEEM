using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class CrownGrowth
    {
        public static void CALC_CC(OrganonVariant variant, FiaCode species, float HLCW, float LCW, float HT, float DBH, float HCB, float EXPAN, float[] CCH)
        {
            float XHLCW;
            float XLCW;
            if (HCB > HLCW)
            {
                XHLCW = HCB;
                switch (variant.Variant)
                {
                    case Variant.Swo:
                        CW_SWO(species, HLCW, LCW, HT, DBH, XHLCW, out XLCW);
                        break;
                    case Variant.Nwo:
                        CW_NWO(species, HLCW, LCW, HT, DBH, XHLCW, out XLCW);
                        break;
                    case Variant.Smc:
                        CW_SMC(species, HLCW, LCW, HT, DBH, XHLCW, out XLCW);
                        break;
                    case Variant.Rap:
                        CW_RAP(species, HLCW, LCW, HT, DBH, XHLCW, out XLCW);
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
                            CW_SWO(species, HLCW, LCW, HT, DBH, XL, out CW);
                            break;
                        case Variant.Nwo:
                            CW_NWO(species, HLCW, LCW, HT, DBH, XL, out CW);
                            break;
                        case Variant.Smc:
                            CW_SMC(species, HLCW, LCW, HT, DBH, XL, out CW);
                            break;
                        case Variant.Rap:
                            CW_RAP(species, HLCW, LCW, HT, DBH, XL, out CW);
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
                FiaCode species = stand.Species[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float crownRatio = stand.CrownRatio[treeIndex];
                float crownLengthInFeet = crownRatio * heightInFeet;
                float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                switch (variant.Variant)
                {
                    case Variant.Swo:
                        MCW_SWO(species, dbhInInches, heightInFeet, out float MCW);
                        LCW_SWO(species, MCW, crownRatio, dbhInInches, heightInFeet, out float LCW);
                        HLCW_SWO(species, heightInFeet, crownRatio, out float HLCW);
                        CALC_CC(variant, species, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        break;
                    case Variant.Nwo:
                        MCW_NWO(species, dbhInInches, heightInFeet, out MCW);
                        LCW_NWO(species, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        HLCW_NWO(species, heightInFeet, crownRatio, out HLCW);
                        CALC_CC(variant, species, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        break;
                    case Variant.Smc:
                        MCW_SMC(species, dbhInInches, heightInFeet, out MCW);
                        LCW_SMC(species, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        HLCW_SMC(species, heightInFeet, crownRatio, out HLCW);
                        CALC_CC(variant, species, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        break;
                    case Variant.Rap:
                        MCW_RAP(species, dbhInInches, heightInFeet, out MCW);
                        LCW_RAP(species, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        HLCW_RAP(species, heightInFeet, crownRatio, out HLCW);
                        CALC_CC(variant, species, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        break;
                }
            }
            CC = CCH[0];
        }

        public static void CrowGro(OrganonVariant variant, Stand stand, StandDensity densityBeforeGrowth, StandDensity densityAfterGrowth,
                                   float SI_1, float SI_2, Dictionary<FiaCode, float[]> CALIB, float[] CCH)
        {
            // DETERMINE 5-YR CROWN RECESSION
            Mortality.OldGro(variant, stand, -1.0F, out float OG1);
            Mortality.OldGro(variant, stand, 0.0F, out float OG2);
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                // CALCULATE HCB START OF GROWTH
                // CALCULATE STARTING HEIGHT
                float PHT = stand.Height[treeIndex] - stand.HeightGrowth[treeIndex];
                // CALCULATE STARTING DBH
                float PDBH = stand.Dbh[treeIndex] - stand.DbhGrowth[treeIndex];
                FiaCode species = stand.Species[treeIndex];
                float SCCFL1 = densityBeforeGrowth.GetCrownCompetitionFactorLarger(PDBH);
                float PCR1;
                switch (variant.Variant)
                {
                    case Variant.Swo:
                        HCB_SWO(species, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1, out float HCB1);
                        PCR1 = 1.0F - HCB1 / PHT;
                        break;
                    case Variant.Nwo:
                        HCB_NWO(species, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1, out HCB1);
                        PCR1 = CALIB[species][1] * (1.0F - HCB1 / PHT);
                        break;
                    case Variant.Smc:
                        HCB_SMC(species, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1, out HCB1);
                        PCR1 = 1.0F - HCB1 / PHT;
                        break;
                    case Variant.Rap:
                        HCB_RAP(species, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1, out HCB1);
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
                        HCB_SWO(species, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2, out float HCB2);
                        MAXHCB_SWO(species, HT, SCCFL2, out MAXHCB);
                        PCR2 = 1.0F - HCB2 / HT;
                        break;
                    case Variant.Nwo:
                        HCB_NWO(species, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2, out HCB2);
                        MAXHCB_NWO(species, HT, SCCFL2, out MAXHCB);
                        PCR2 = CALIB[species][1] * (1.0F - HCB2 / HT);
                        break;
                    case Variant.Smc:
                        HCB_SMC(species, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2, out HCB2);
                        MAXHCB_SMC(species, HT, SCCFL2, out MAXHCB);
                        PCR2 = 1.0F - HCB2 / HT;
                        break;
                    case Variant.Rap:
                        HCB_RAP(species, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2, out HCB2);
                        MAXHCB_RAP(species, HT, SCCFL2, out MAXHCB);
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

        private static void CW_NWO(FiaCode species, float HLCW, float LCW, float HT, float DBH, float XL, out float CW)
        {
            // CROWN WIDTH ABOVE LARGEST CROWN WIDTH
            float B1;
            float B2;
            float B3;
            switch (species)
            {
                // DF Coefficients from Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.929973F;
                    B2 = -0.135212F;
                    B3 = -0.0157579F;
                    break;
                // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AbiesGrandis:
                    B1 = 0.999291F;
                    B2 = 0.0F;
                    B3 = -0.0314603F;
                    break;
                // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
                case FiaCode.TsugaHeterophylla:
                    B1 = 0.461782F;
                    B2 = 0.552011F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.629785F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                case FiaCode.AcerMacrophyllum:
                case FiaCode.QuercusGarryana:
                case FiaCode.AlnusRubra:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float RP = (HT - XL) / (HT - HLCW);
            float RATIO = HT / DBH;
            if (species == FiaCode.PseudotsugaMenziesii)
            {
                if (RATIO > 50.0F)
                {
                    RATIO = 50.0F;
                }
            }
            else if (species == FiaCode.AbiesGrandis)
            {
                if (RATIO > 31.0F)
                {
                    RATIO = 31.0F;
                }
            }
            CW = (float)(LCW * Math.Pow(RP, B1 + B2 * Math.Sqrt(RP) + B3 * (RATIO)));
        }

        private static void CW_RAP(FiaCode species, float HLCW, float LCW, float HT, float DBH, float XL, out float CW)
        {
            // CROWN WIDTH ABOVE LARGEST CROWN WIDTH
            float B1;
            float B2;
            float B3;
            switch (species)
            {
                // Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
                case FiaCode.AlnusRubra:
                    B1 = 0.63420194F;
                    B2 = 0.17649614F;
                    B3 = -0.02315018F;
                    break;
                // Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.929973F;
                    B2 = -0.135212F;
                    B3 = -0.0157579F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ThujaPlicata:
                case FiaCode.TsugaHeterophylla:
                    B1 = 0.629785F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AcerMacrophyllum:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float RP = (HT - XL) / (HT - HLCW);
            float RATIO = HT / DBH;
            if (species == FiaCode.PseudotsugaMenziesii)
            {
                if (RATIO > 50.0F)
                {
                    RATIO = 50.0F;
                }
            }
            CW = (float)(LCW * Math.Pow(RP, B1 + B2 * Math.Sqrt(RP) + B3 * (RATIO)));
        }

        private static void CW_SMC(FiaCode species, float HLCW, float LCW, float HT, float DBH, float XL, out float CW)
        {
            // CROWN WIDTH ABOVE LARGEST CROWN WIDTH
            float B1;
            float B2;
            float B3;
            switch (species)
            {
                // Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.929973F;
                    B2 = -0.135212F;
                    B3 = -0.0157579F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AbiesGrandis:
                    B1 = 0.999291F;
                    B2 = 0.0F;
                    B3 = -0.0314603F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.TsugaHeterophylla:
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.629785F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AcerMacrophyllum:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.QuercusGarryana:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AlnusRubra:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.CornusNuttallii:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.Salix:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float RP = (HT - XL) / (HT - HLCW);
            float RATIO = HT / DBH;
            if (species == FiaCode.PseudotsugaMenziesii)
            {
                if (RATIO > 50.0F)
                {
                    RATIO = 50.0F;
                }
            }
            else if (species == FiaCode.AbiesGrandis)
            {
                if (RATIO > 31.0)
                {
                    RATIO = 31.0F;
                }
            }
            CW = (float)(LCW * Math.Pow(RP, B1 + B2 * Math.Sqrt(RP) + B3 * (RATIO)));
        }

        private static void CW_SWO(FiaCode species, float HLCW, float LCW, float HT, float DBH, float XL, out float CW)
        {
            // CROWN WIDTH ABOVE LARGEST CROWN WIDTH
            float B1;
            float B2;
            float B3;
            switch (species)
            {
                // DF Coefficients from Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.929973F;
                    B2 = -0.135212F;
                    B3 = -0.0157579F;
                    break;
                // GW Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B1 = 0.999291F;
                    B2 = 0.0F;
                    B3 = -0.0314603F;
                    break;
                // PP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.PinusPonderosa:
                case FiaCode.PinusLambertiana:
                    B1 = 0.755583F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.CalocedrusDecurrens:
                    B1 = 0.629785F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.TsugaHeterophylla:
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.629785F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                case FiaCode.NotholithocarpusDensiflorus:
                case FiaCode.QuercusChrysolepis:
                case FiaCode.AcerMacrophyllum:
                case FiaCode.QuercusGarryana:
                case FiaCode.QuercusKelloggii:
                case FiaCode.AlnusRubra:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.5F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float RP = (HT - XL) / (HT - HLCW);
            float RATIO = HT / DBH;
            if (species == FiaCode.PseudotsugaMenziesii)
            {
                if (RATIO > 50.0F)
                {
                    RATIO = 50.0F;
                }
            }
            else if ((species == FiaCode.AbiesConcolor) || (species == FiaCode.AbiesGrandis))
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
        /// <param name="species">Tree's species.</param>
        /// <param name="HT">Tree height (feet).</param>
        /// <param name="DBH">Tree's diameter at breast height (inches)</param>
        /// <param name="CCFL"></param>
        /// <param name="BA">Stand basal area.</param>
        /// <param name="SI_1">Stand site index.</param>
        /// <param name="SI_2">Stand site index.</param>
        /// <param name="OG"></param>
        /// <param name="HCB">Height to crown base (feet).</param>
        public static void HCB_NWO(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG, out float HCB)
        {
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            switch (species)
            {
                // DF Coefficients from Zumrawi and Hann (1989) FRL Research Paper 52
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 1.94093F;
                    B1 = -0.0065029F;
                    B2 = -0.0048737F;
                    B3 = -0.261573F;
                    B4 = 1.08785F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Zumrawi and Hann (1989) FRL Research Paper 52
                case FiaCode.AbiesGrandis:
                    B0 = 1.04746F;
                    B1 = -0.0066643F;
                    B2 = -0.0067129F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Johnson (2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.92682F;
                    B1 = -0.00280478F;
                    B2 = -0.0011939F;
                    B3 = -0.513134F;
                    B4 = 3.68901F;
                    B5 = 0.00742219F;
                    B6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = 4.49102006F;
                    B1 = 0.0F;
                    B2 = -0.00132412F;
                    B3 = -1.01460531F;
                    B4 = 0.0F;
                    B5 = 0.01340624F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.TaxusBrevifolia:
                    B0 = 0.0F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 2.030940382F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.ArbutusMenziesii:
                    B0 = 2.955339267F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -0.798610738F;
                    B4 = 3.095269471F;
                    B5 = 0.0F;
                    B6 = 0.700465646F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.AcerMacrophyllum:
                    B0 = 0.9411395642F;
                    B1 = -0.00768402F;
                    B2 = -0.005476131F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = 1.05786632F;
                    B1 = 0.0F;
                    B2 = -0.00183283F;
                    B3 = -0.28644547F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    B0 = 0.56713781F;
                    B1 = -0.010377976F;
                    B2 = -0.002066036F;
                    B3 = 0.0F;
                    B4 = 1.39796223F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 0.0F;
                    B1 = 0.0F;
                    B2 = -0.005666559F;
                    B3 = -0.745540494F;
                    B4 = 0.0F;
                    B5 = 0.038476613F;
                    B6 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            if (species == FiaCode.TsugaHeterophylla)
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

        public static void HCB_RAP(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG, out float HCB)
        {
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            float K;
            switch (species)
            {
                // Hann, Bluhm, and Hibbs (2011) Development and Evaluation of the Tree-Level Equations and Their Combined 
                //   Stand-Level Behavior in the Red Alder Plantation Version of Organon
                case FiaCode.AlnusRubra:
                    B0 = 3.73113020F;
                    B1 = -0.021546486F;
                    B2 = -0.0016572840F;
                    B3 = -1.0649544F;
                    B4 = 7.47699601F;
                    B5 = 0.0252953320F;
                    B6 = 0.0F;
                    K = 1.6F;
                    break;
                // Hann and Hanus (2004) FS 34: 1193-2003
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 6.18464679F;
                    B1 = -0.00328764F;
                    B2 = -0.00136555F;
                    B3 = -1.19702220F;
                    B4 = 3.17028263F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    K = 0.0F;
                    break;
                // Johnson (2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.92682F;
                    B1 = -0.00280478F;
                    B2 = -0.0011939F;
                    B3 = -0.513134F;
                    B4 = 3.68901F;
                    B5 = 0.00742219F;
                    B6 = 0.0F;
                    K = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = 4.49102006F;
                    B1 = 0.0F;
                    B2 = -0.00132412F;
                    B3 = -1.01460531F;
                    B4 = 0.0F;
                    B5 = 0.01340624F;
                    B6 = 0.0F;
                    K = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.AcerMacrophyllum:
                    B0 = 0.9411395642F;
                    B1 = -0.00768402F;
                    B2 = -0.005476131F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    K = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 0.0F;
                    B1 = 0.0F;
                    B2 = -0.005666559F;
                    B3 = -0.745540494F;
                    B4 = 0.0F;
                    B5 = 0.038476613F;
                    B6 = 0.0F;
                    K = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            if (species == FiaCode.AlnusRubra)
            {
                HCB = (float)((HT - K) / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SI_1 + B6 * OG * OG)) + K);
            }
            else
            {
                float SITE = SI_2;
                if (species == FiaCode.TsugaHeterophylla)
                {
                    SITE = 0.480F + (1.110F * (SI_2 + 4.5F));
                }
                HCB = (float)(HT / (1.0 + Math.Exp(B0 + B1 * HT + B2 * CCFL + B3 * Math.Log(BA) + B4 * (DBH / HT) + B5 * SITE + B6 * OG * OG)));
            }
            Debug.Assert(HCB >= 0.0F);
            Debug.Assert(HCB <= HT);
        }

        public static void HCB_SMC(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG, out float HCB)
        {
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            switch (species)
            {
                // Hann and Hanus (2004) FS 34: 1193-2003
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 6.18464679F;
                    B1 = -0.00328764F;
                    B2 = -0.00136555F;
                    B3 = -1.19702220F;
                    B4 = 3.17028263F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Zumrawi and Hann (1989) FRL Research Paper 52
                case FiaCode.AbiesGrandis:
                    B0 = 1.04746F;
                    B1 = -0.0066643F;
                    B2 = -0.0067129F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Johnson (2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.92682F;
                    B1 = -0.00280478F;
                    B2 = -0.0011939F;
                    B3 = -0.513134F;
                    B4 = 3.68901F;
                    B5 = 0.00742219F;
                    B6 = 0.0F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = 4.49102006F;
                    B1 = 0.0F;
                    B2 = -0.00132412F;
                    B3 = -1.01460531F;
                    B4 = 0.0F;
                    B5 = 0.01340624F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.TaxusBrevifolia:
                    B0 = 0.0F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 2.030940382F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.ArbutusMenziesii:
                    B0 = 2.955339267F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -0.798610738F;
                    B4 = 3.095269471F;
                    B5 = 0.0F;
                    B6 = 0.700465646F;
                    break;
                // BL Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.AcerMacrophyllum:
                    B0 = 0.9411395642F;
                    B1 = -0.00768402F;
                    B2 = -0.005476131F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = 1.05786632F;
                    B1 = 0.0F;
                    B2 = -0.00183283F;
                    B3 = -0.28644547F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    B0 = 0.56713781F;
                    B1 = -0.010377976F;
                    B2 = -0.002066036F;
                    B3 = 0.0F;
                    B4 = 1.39796223F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 0.0F;
                    B1 = 0.0F;
                    B2 = -0.005666559F;
                    B3 = -0.745540494F;
                    B4 = 0.0F;
                    B5 = 0.038476613F;
                    B6 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            if (species == FiaCode.TsugaHeterophylla)
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

        public static void HCB_SWO(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG, out float HCB)
        {
            // HEIGHT TO CROWN BASE FOR UNDAMAGED TREES ONLY
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            switch(species)
            {
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 1.797136911F;
                    B1 = -0.010188791F;
                    B2 = -0.003346230F;
                    B3 = -0.412217810F;
                    B4 = 3.958656001F;
                    B5 = 0.008526562F;
                    B6 = 0.448909636F;
                    break;
                // GW Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B0 = 3.451045887F;
                    B1 = -0.005985239F;
                    B2 = -0.003211194F;
                    B3 = -0.671479750F;
                    B4 = 3.931095518F;
                    B5 = 0.003115567F;
                    B6 = 0.516180892F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.PinusPonderosa:
                    B0 = 1.656364063F;
                    B1 = -0.002755463F;
                    B2 = 0.0F;
                    B3 = -0.568302547F;
                    B4 = 6.730693919F;
                    B5 = 0.001852526F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.PinusLambertiana:
                    B0 = 3.785155749F;
                    B1 = -0.009012547F;
                    B2 = -0.003318574F;
                    B3 = -0.670270058F;
                    B4 = 2.758645081F;
                    B5 = 0.0F;
                    B6 = 0.841525071F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.CalocedrusDecurrens:
                    B0 = 2.428285297F;
                    B1 = -0.006882851F;
                    B2 = -0.002612590F;
                    B3 = -0.572782216F;
                    B4 = 2.113378338F;
                    B5 = 0.008480754F;
                    B6 = 0.506226895F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.TsugaHeterophylla:
                    B0 = 0.0F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 4.801329946F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = 4.49102006F;
                    B1 = 0.0F;
                    B2 = -0.00132412F;
                    B3 = -1.01460531F;
                    B4 = 0.0F;
                    B5 = 0.01340624F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.TaxusBrevifolia:
                    B0 = 0.0F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 2.030940382F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.ArbutusMenziesii:
                    B0 = 2.955339267F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -0.798610738F;
                    B4 = 3.095269471F;
                    B5 = 0.0F;
                    B6 = 0.700465646F;
                    break;
                // GC Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    B0 = 0.544237656F;
                    B1 = -0.020571754F;
                    B2 = -0.004317523F;
                    B3 = 0.0F;
                    B4 = 3.132713612F;
                    B5 = 0.0F;
                    B6 = 0.483748898F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.NotholithocarpusDensiflorus:
                    B0 = 0.833006499F;
                    B1 = -0.012984204F;
                    B2 = -0.002704717F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.2491242765F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.QuercusChrysolepis:
                    B0 = 0.5376600543F;
                    B1 = -0.018632397F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.AcerMacrophyllum:
                    B0 = 0.9411395642F;
                    B1 = -0.00768402F;
                    B2 = -0.005476131F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26 - 33
                case FiaCode.QuercusGarryana:
                    B0 = 1.05786632F;
                    B1 = 0.0F;
                    B2 = -0.00183283F;
                    B3 = -0.28644547F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.QuercusKelloggii:
                    B0 = 2.60140655F;
                    B1 = 0.0F;
                    B2 = -0.002273616F;
                    B3 = -0.554980629F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    B0 = 0.56713781F;
                    B1 = -0.010377976F;
                    B2 = -0.002066036F;
                    B3 = 0.0F;
                    B4 = 1.39796223F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 0.0F;
                    B1 = 0.0F;
                    B2 = -0.005666559F;
                    B3 = -0.745540494F;
                    B4 = 0.0F;
                    B5 = 0.038476613F;
                    B6 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            if (species == FiaCode.PinusLambertiana)
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
        /// <param name="species">Tree's species.</param>
        /// <param name="HT">Tree's height (feet).</param>
        /// <param name="CR">Tree's crown ratio.</param>
        /// <param name="SCR"></param>
        /// <param name="HLCW">Height to largest crown width (feet)</param>
        public static void HLCW_NWO(FiaCode species, float HT, float CR, out float HLCW)
        {
            // DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH(1 parameter - all species)
            float B1;
            switch (species)
            {
                // Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.062000F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AbiesGrandis:
                    B1 = 0.028454F;
                    break;
                // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
                case FiaCode.TsugaHeterophylla:
                    B1 = 0.355270F;
                    break;
                // WH of Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.209806F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                case FiaCode.AcerMacrophyllum:
                case FiaCode.QuercusGarryana:
                case FiaCode.AlnusRubra:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            float CL = CR * HT;
            HLCW = HT - (1.0F - B1) * CL;
        }

        public static void HLCW_RAP(FiaCode species, float HT, float CR, out float HLCW)
        {
            // DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH(1 parameter - all species)
            float B1;
            float B2;
            switch (species)
            {
                // Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
                case FiaCode.AlnusRubra:
                    B1 = 0.63619616F;
                    B2 = -1.2180562F;
                    break;
                // Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.062000F;
                    B2 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.TsugaHeterophylla:
                case FiaCode.ThujaPlicata:
                    B1 = 0.209806F;
                    B2 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AcerMacrophyllum:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            HLCW = (float)(HT - (1.0F - B1 * Math.Exp(Math.Pow(B2 * (1.0F - HT / 140.0F), 3))) * CL);
        }

        public static void HLCW_SMC(FiaCode species, float HT, float CR, out float HLCW)
        {
            // DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH
            float B1;
            switch (species)
            {
                // Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.062000F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AbiesGrandis:
                    B1 = 0.028454F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.TsugaHeterophylla:
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.209806F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                case FiaCode.AcerMacrophyllum:
                case FiaCode.QuercusGarryana:
                case FiaCode.AlnusRubra:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            HLCW = HT - (1.0F - B1) * CL;
        }

        public static void HLCW_SWO(FiaCode species, float HT, float CR, out float HLCW)
        {
            // DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH(1 parameter - all species)
            float B1;
            switch (species)
            {
                // Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.062000F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B1 = 0.028454F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.PinusPonderosa:
                case FiaCode.PinusLambertiana:
                    B1 = 0.05F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.CalocedrusDecurrens:
                    B1 = 0.20F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.TsugaHeterophylla:
                    B1 = 0.209806F;
                    break;
                // IC
                case FiaCode.ThujaPlicata:
                    B1 = 0.20F;
                    break;
                // WH
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.209806F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                case FiaCode.NotholithocarpusDensiflorus:
                case FiaCode.QuercusChrysolepis:
                case FiaCode.AcerMacrophyllum:
                case FiaCode.QuercusGarryana:
                case FiaCode.QuercusKelloggii:
                case FiaCode.AlnusRubra:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            HLCW = HT - (1.0F - B1) * CL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="MCW">Tree's maximum crown width (feet).</param>
        /// <param name="CR">Tree's crown ratio.</param>
        /// <param name="DBH">Tree's diameter at breast height (inches).</param>
        /// <param name="HT">Tree's height (feet).</param>
        /// <param name="LCW">Tree's largest crown width (feet).</param>
        public static void LCW_NWO(FiaCode species, float MCW, float CR, float DBH, float HT, out float LCW)
        {
            // LARGEST CROWN WIDTH(3 parameters - all species)
            float B1;
            float B2;
            float B3;
            switch (species)
            {
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.0F;
                    B2 = 0.00436324F;
                    B3 = 0.6020020F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AbiesGrandis:
                    B1 = 0.0F;
                    B2 = 0.00308402F;
                    B3 = 0.0F;
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B1 = 0.105590F;
                    B2 = 0.0035662F;
                    B3 = 0.0F;
                    break;
                // IC of Hann(1997) FRL Research Contribution 17
                case FiaCode.ThujaPlicata:
                    B1 = -0.2513890F;
                    B2 = 0.006925120F;
                    B3 = 0.985922F;
                    break;
                // WH of Hann(1997) FRL Research Contribution 17
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.ArbutusMenziesii:
                    B1 = 0.118621F;
                    B2 = 0.00384872F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AcerMacrophyllum:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.470180F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.QuercusGarryana:
                    B1 = 0.3648110F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AlnusRubra:
                    B1 = 0.3227140F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // GC of Hann(1997) FRL Research Contribution 17
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.61440F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            LCW = (float)(MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
        }

        public static void LCW_RAP(FiaCode species, float MCW, float CR, float DBH, float HT, out float LCW)
        {
            // LARGEST CROWN WIDTH(3 parameters - all species)
            float B0;
            float B1;
            float B2;
            float B3;
            switch (species)
            {
                // Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
                case FiaCode.AlnusRubra:
                    B0 = 0.78160725F;
                    B1 = 0.44092737F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 1.0F;
                    B1 = 0.0F;
                    B2 = 0.00436324F;
                    B3 = 0.6020020F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.0F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // IC of Hann(1997) FRL Research Contribution 17
                case FiaCode.ThujaPlicata:
                    B0 = 1.0F;
                    B1 = -0.2513890F;
                    B2 = 0.006925120F;
                    B3 = 0.985922F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AcerMacrophyllum:
                    B0 = 1.0F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.47018F;
                    break;
                // GC of Hann(1997) FRL Research Contribution 17
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 1.0F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.61440F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            LCW = (float)(B0 * MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
        }

        public static void LCW_SMC(FiaCode species, float MCW, float CR, float DBH, float HT, out float LCW)
        {
            // LARGEST CROWN WIDTH(3 parameters - all species)
            float B1;
            float B2;
            float B3;
            switch (species)
            {
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.0F;
                    B2 = 0.00436324F;
                    B3 = 0.6020020F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AbiesGrandis:
                    B1 = 0.0F;
                    B2 = 0.00308402F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                // BUGBUG: all coefficients are zero
                case FiaCode.TsugaHeterophylla:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // IC of Hann(1997) FRL Research Contribution 17
                case FiaCode.ThujaPlicata:
                    B1 = -0.2513890F;
                    B2 = 0.006925120F;
                    B3 = 0.985922F;
                    break;
                // WH of Hann(1997) FRL Research Contribution 17
                // BUGBUG: all coefficients are zero
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.ArbutusMenziesii:
                    B1 = 0.118621F;
                    B2 = 0.00384872F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AcerMacrophyllum:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.470180F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.QuercusGarryana:
                    B1 = 0.3648110F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AlnusRubra:
                    B1 = 0.3227140F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // GC of Hann(1997) FRL Research Contribution 17
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.61440F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            LCW = (float)(MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
        }

        public static void LCW_SWO(FiaCode species, float MCW, float CR, float DBH, float HT, out float LCW)
        {
            // LARGEST CROWN WIDTH(3 parameters - all species)
            float B1;
            float B2;
            float B3;
            switch (species)
            {
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.PseudotsugaMenziesii:
                    B1 = 0.0F;
                    B2 = 0.00371834F;
                    B3 = 0.808121F;
                    break;
                // GW Coefficients from Hann(1997) FRL Research Contribution 17
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B1 = 0.0F;
                    B2 = 0.00308402F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.PinusPonderosa:
                    B1 = 0.355532F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.PinusLambertiana:
                    B1 = 0.0F;
                    B2 = 0.00339675F;
                    B3 = 0.532418F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.CalocedrusDecurrens:
                    B1 = -0.251389F;
                    B2 = 0.00692512F;
                    B3 = 0.985922F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.TsugaHeterophylla:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // IC
                case FiaCode.ThujaPlicata:
                    B1 = -0.251389F;
                    B2 = 0.00692512F;
                    B3 = 0.985922F;
                    break;
                // WH
                case FiaCode.TaxusBrevifolia:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.ArbutusMenziesii:
                    B1 = 0.118621F;
                    B2 = 0.00384872F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.161440F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.NotholithocarpusDensiflorus:
                    B1 = 0.0F;
                    B2 = 0.0111972F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.QuercusChrysolepis:
                    B1 = 0.0F;
                    B2 = 0.0207676F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AcerMacrophyllum:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.47018F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.QuercusGarryana:
                    B1 = 0.364811F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.QuercusKelloggii:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.27196F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AlnusRubra:
                    B1 = 0.3227140F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    break;
                // GC
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 1.161440F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            LCW = (float)(MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
        }

        private static void MAXHCB_NWO(FiaCode species, float HT, float CCFL, out float MAXHCB)
        {
            // MAXIMUM HEIGHT TO CROWN BASE
            float B0;
            float B1;
            float B2;
            float B3;
            float LIMIT;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 0.96F;
                    B1 = 0.26F;
                    B2 = -0.900721383F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.AbiesGrandis:
                    B0 = 0.96F;
                    B1 = 0.31F;
                    B2 = -2.450718394F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.01F;
                    B1 = 0.36F;
                    B2 = -0.944528054F;
                    B3 = 0.6F;
                    LIMIT = 0.96F;
                    break;
                case FiaCode.ThujaPlicata:
                    B0 = 0.96F;
                    B1 = 0.31F;
                    B2 = -1.059636222F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.TaxusBrevifolia:
                    B0 = 0.85F;
                    B1 = 0.35F;
                    B2 = -0.922868139F;
                    B3 = 0.8F;
                    LIMIT = 0.80F;
                    break;
                case FiaCode.ArbutusMenziesii:
                    B0 = 0.981F;
                    B1 = 0.161F;
                    B2 = -1.73666044F;
                    B3 = 1.0F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.020016685F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.QuercusGarryana:
                    B0 = 1.0F;
                    B1 = 0.3F;
                    B2 = -0.95634399F;
                    B3 = 1.1F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.AlnusRubra:
                    B0 = 0.93F;
                    B1 = 0.18F;
                    B2 = -0.928243505F;
                    B3 = 1.0F;
                    LIMIT = 0.92F;
                    break;
                case FiaCode.CornusNuttallii:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.020016685F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.Salix:
                    B0 = 0.985F;
                    B1 = 0.285F;
                    B2 = -0.969750805F;
                    B3 = 0.9F;
                    LIMIT = 0.98F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            float MAXBR = (float)(B0 - B1 * Math.Exp(B2 * Math.Pow(CCFL / 100.0, B3)));
            if (MAXBR > LIMIT)
            {
                MAXBR = LIMIT;
            }
            MAXHCB = MAXBR * HT;
            Debug.Assert(MAXHCB >= 0.0F);
            Debug.Assert(MAXHCB <= 500.0F);
        }

        private static void MAXHCB_RAP(FiaCode species, float HT, float CCFL, out float MAXHCB)
        {
            // MAXIMUM HEIGHT TO CROWN BASE
            float B0;
            float B1;
            float B2;
            float B3;
            float LIMIT;
            switch (species)
            {
                case FiaCode.AlnusRubra:
                    B0 = 0.93F;
                    B1 = 0.18F;
                    B2 = -0.928243505F;
                    B3 = 1.0F;
                    LIMIT = 0.92F;
                    break;
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 0.96F;
                    B1 = 0.26F;
                    B2 = -0.34758F;
                    B3 = 1.5F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.01F;
                    B1 = 0.36F;
                    B2 = 0.944528054F;
                    B3 = 0.6F;
                    LIMIT = 0.96F;
                    break;
                case FiaCode.ThujaPlicata:
                    B0 = 0.96F;
                    B1 = 0.31F;
                    B2 = -1.059636222F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.020016685F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.CornusNuttallii:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.020016685F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.Salix:
                    B0 = 0.985F;
                    B1 = 0.285F;
                    B2 = -0.969750805F;
                    B3 = 0.9F;
                    LIMIT = 0.98F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            float MAXBR = (float)(B0 - B1 * Math.Exp(B2 * Math.Pow(CCFL / 100.0, B3)));
            if (MAXBR > LIMIT)
            {
                MAXBR = LIMIT;
            }
            MAXHCB = MAXBR * HT;
        }

        private static void MAXHCB_SMC(FiaCode species, float HT, float CCFL, out float MAXHCB)
        {
            // MAXIMUM HEIGHT TO CROWN BASE
            float B0;
            float B1;
            float B2;
            float B3;
            float LIMIT;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 0.96F;
                    B1 = 0.26F;
                    B2 = -0.34758F;
                    B3 = 1.5F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.AbiesGrandis:
                    B0 = 0.96F;
                    B1 = 0.31F;
                    B2 = -2.450718394F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.01F;
                    B1 = 0.36F;
                    B2 = -0.944528054F;
                    B3 = 0.6F;
                    LIMIT = 0.96F;
                    break;
                case FiaCode.ThujaPlicata:
                    B0 = 0.96F;
                    B1 = 0.31F;
                    B2 = -1.059636222F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.TaxusBrevifolia:
                    B0 = 0.85F;
                    B1 = 0.35F;
                    B2 = -0.922868139F;
                    B3 = 0.8F;
                    LIMIT = 0.80F;
                    break;
                case FiaCode.ArbutusMenziesii:
                    B0 = 0.981F;
                    B1 = 0.161F;
                    B2 = -1.73666044F;
                    B3 = 1.0F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.020016685F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.QuercusGarryana:
                    B0 = 1.0F;
                    B1 = 0.3F;
                    B2 = -0.95634399F;
                    B3 = 1.1F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.AlnusRubra:
                    B0 = 0.93F;
                    B1 = 0.18F;
                    B2 = -0.928243505F;
                    B3 = 1.0F;
                    LIMIT = 0.92F;
                    break;
                case FiaCode.CornusNuttallii:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.020016685F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.Salix:
                    B0 = 0.985F;
                    B1 = 0.285F;
                    B2 = -0.969750805F;
                    B3 = 0.9F;
                    LIMIT = 0.98F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            float MAXBR = (float)(B0 - B1 * Math.Exp(B2 * Math.Pow(CCFL / 100.0, B3)));
            if (MAXBR > LIMIT)
            {
                MAXBR = LIMIT;
            }
            MAXHCB = MAXBR * HT;
        }

        private static void MAXHCB_SWO(FiaCode species, float HT, float CCFL, out float MAXHCB)
        {
            // MAXIMUM HEIGHT TO CROWN BASE (5 parameters - all species)
            float B0;
            float B1;
            float B2;
            float B3;
            float LIMIT;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 0.96F;
                    B1 = 0.26F;
                    B2 = -0.987864873F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B0 = 0.96F;
                    B1 = 0.31F;
                    B2 = 2.450718394F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.PinusPonderosa:
                    B0 = 1.01F;
                    B1 = 0.36F;
                    B2 = -1.041915784F;
                    B3 = 0.6F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.PinusLambertiana:
                    B0 = 1.02F;
                    B1 = 0.27F;
                    B2 = -0.922718593F;
                    B3 = 0.4F;
                    LIMIT = 0.96F;
                    break;
                case FiaCode.CalocedrusDecurrens:
                    B0 = 0.97F;
                    B1 = 0.22F;
                    B2 = -0.002612590F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.01F;
                    B1 = 0.36F;
                    B2 = -0.944528054F;
                    B3 = 0.6F;
                    LIMIT = 0.96F;
                    break;
                case FiaCode.ThujaPlicata:
                    B0 = 0.96F;
                    B1 = 0.31F;
                    B2 = -1.059636222F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.TaxusBrevifolia:
                    B0 = 0.85F;
                    B1 = 0.35F;
                    B2 = -0.922868139F;
                    B3 = 0.8F;
                    LIMIT = 0.80F;
                    break;
                case FiaCode.ArbutusMenziesii:
                    B0 = 0.981F;
                    B1 = 0.161F;
                    B2 = -1.73666044F;
                    B3 = 1.0F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.219919284F;
                    B3 = 1.2F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.NotholithocarpusDensiflorus:
                    B0 = 0.98F;
                    B1 = 0.33F;
                    B2 = -0.911341687F;
                    B3 = 1.0F;
                    LIMIT = 0.97F;
                    break;
                case FiaCode.QuercusChrysolepis:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -0.922025464F;
                    B3 = 1.0F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.020016685F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.QuercusGarryana:
                    B0 = 1.0F;
                    B1 = 0.3F;
                    B2 = -0.95634399F;
                    B3 = 1.1F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.QuercusKelloggii:
                    B0 = 1.0F;
                    B1 = 0.2F;
                    B2 = -1.053892465F;
                    B3 = 1.0F;
                    LIMIT = 0.98F;
                    break;
                case FiaCode.AlnusRubra:
                    B0 = 0.93F;
                    B1 = 0.18F;
                    B2 = -0.928243505F;
                    B3 = 1.0F;
                    LIMIT = 0.92F;
                    break;
                case FiaCode.CornusNuttallii:
                    B0 = 1.0F;
                    B1 = 0.45F;
                    B2 = -1.020016685F;
                    B3 = 1.0F;
                    LIMIT = 0.95F;
                    break;
                case FiaCode.Salix:
                    B0 = 0.985F;
                    B1 = 0.285F;
                    B2 = -0.969750805F;
                    B3 = 0.9F;
                    LIMIT = 0.98F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

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
        /// <param name="species">Tree's species.</param>
        /// <param name="D">Tree's diameter at breast height (inches).</param>
        /// <param name="H">Tree's height (feet).</param>
        /// <param name="MCW">Estimated maximum crown width.</param>
        public static void MCW_NWO(FiaCode species, float D, float H, out float MCW)
        {
            float B0;
            float B1;
            float B2;
            float PKDBH;
            switch (species)
            {
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 4.6198F;
                    B1 = 1.8426F;
                    B2 = -0.011311F;
                    PKDBH = 81.45F;
                    break;
                // GF Coefficients from Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.AbiesGrandis:
                    B0 = 6.1880F;
                    B1 = 1.0069F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = 4.3586F;
                    B1 = 1.57458F;
                    B2 = 0.0F;
                    PKDBH = 76.70F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.ThujaPlicata:
                    B0 = 4.0F;
                    B1 = 1.65F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // WH of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.TaxusBrevifolia:
                    B0 = 4.5652F;
                    B1 = 1.4147F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.ArbutusMenziesii:
                    B0 = 3.4298629F;
                    B1 = 1.3532302F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
                case FiaCode.AcerMacrophyllum:
                    B0 = 4.0953F;
                    B1 = 2.3849F;
                    B2 = -0.0102651F;
                    PKDBH = 102.53F;
                    break;
                // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
                case FiaCode.QuercusGarryana:
                    B0 = 3.0785639F;
                    B1 = 1.9242211F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.AlnusRubra:
                    B0 = 8.0F;
                    B1 = 1.53F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // GC of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 2.9793895F;
                    B1 = 1.5512443F;
                    B2 = -0.01416129F;
                    PKDBH = 54.77F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            float DBH = D;
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            float HT = H;
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * DBH + B2 * DBH * DBH;
            }
        }

        public static void MCW_RAP(FiaCode species, float D, float H, out float MCW)
        {
            float B0;
            float B1;
            float B2;
            float K;
            float PKDBH;
            switch (species)
            {
                // Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
                case FiaCode.AlnusRubra:
                    B0 = 2.320746348F;
                    B1 = 6.661401926F;
                    B2 = 0.0F;
                    K = 0.6F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 4.6198F;
                    B1 = 1.8426F;
                    B2 = -0.011311F;
                    K = 1.0F;
                    PKDBH = 81.45F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.TsugaHeterophylla:
                    B0 = 4.5652F;
                    B1 = 1.4147F;
                    B2 = 0.0F;
                    K = 1.0F;
                    PKDBH = 999.99F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.ThujaPlicata:
                    B0 = 4.0F;
                    B1 = 1.65F;
                    B2 = 0.0F;
                    K = 1.0F;
                    PKDBH = 999.99F;
                    break;
                // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
                case FiaCode.AcerMacrophyllum:
                    B0 = 4.0953F;
                    B1 = 2.3849F;
                    B2 = -0.011630F;
                    K = 1.0F;
                    PKDBH = 102.53F;
                    break;
                // GC of Paine and Hann (1982) FRL Research Paper 46
                // GC of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 2.9793895F;
                    B1 = 1.5512443F;
                    B2 = -0.01416129F;
                    K = 1.0F;
                    PKDBH = 54.77F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            float DBH = D;
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            float HT = H;
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * (float)Math.Pow(DBH, K) + B2 * DBH * DBH;
            }
        }

        public static void MCW_SMC(FiaCode species, float D, float H, out float MCW)
        {
            float B0;
            float B1;
            float B2;
            float PKDBH;
            switch (species)
            {
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 4.6198F;
                    B1 = 1.8426F;
                    B2 = -0.011311F;
                    PKDBH = 81.45F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.AbiesGrandis:
                    B0 = 6.1880F;
                    B1 = 1.0069F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.TsugaHeterophylla:
                    B0 = 4.5652F;
                    B1 = 1.4147F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.ThujaPlicata:
                    B0 = 4.0F;
                    B1 = 1.65F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // WH of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.TaxusBrevifolia:
                    B0 = 4.5652F;
                    B1 = 1.4147F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.ArbutusMenziesii:
                    B0 = 3.4298629F;
                    B1 = 1.3532302F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
                case FiaCode.AcerMacrophyllum:
                    B0 = 4.0953F;
                    B1 = 2.3849F;
                    B2 = -0.011630F;
                    PKDBH = 102.53F;
                    break;
                // Paine and Hann (1982) FRL Research Paper 46
                case FiaCode.QuercusGarryana:
                    B0 = 3.0785639F;
                    B1 = 1.9242211F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.AlnusRubra:
                    B0 = 8.0F;
                    B1 = 1.53F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // GC of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 2.9793895F;
                    B1 = 1.5512443F;
                    B2 = -0.01416129F;
                    PKDBH = 54.77F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float DBH = D;
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            float HT = H;
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * DBH + B2 * DBH * DBH;
            }
        }

        public static void MCW_SWO(FiaCode species, float D, float H, out float MCW)
        {
            float B0;
            float B1;
            float B2;
            float PKDBH;
            switch (species)
            {
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 4.6366F;
                    B1 = 1.6078F;
                    B2 = -0.009625F;
                    PKDBH = 88.52F;
                    break;
                // GW Coefficients from Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B0 = 6.1880F;
                    B1 = 1.0069F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.PinusPonderosa:
                    B0 = 3.4835F;
                    B1 = 1.343F;
                    B2 = -0.0082544F;
                    PKDBH = 81.35F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.PinusLambertiana:
                    B0 = 4.6600546F;
                    B1 = 1.0701859F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.CalocedrusDecurrens:
                    B0 = 3.2837F;
                    B1 = 1.2031F;
                    B2 = -0.0071858F;
                    PKDBH = 83.71F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.TsugaHeterophylla:
                    B0 = 4.5652F;
                    B1 = 1.4147F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.ThujaPlicata:
                    B0 = 4.0F;
                    B1 = 1.65F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // WH
                case FiaCode.TaxusBrevifolia:
                    B0 = 4.5652F;
                    B1 = 1.4147F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.ArbutusMenziesii:
                    B0 = 3.4298629F;
                    B1 = 1.3532302F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    B0 = 2.9793895F;
                    B1 = 1.5512443F;
                    B2 = -0.01416129F;
                    PKDBH = 54.77F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.NotholithocarpusDensiflorus:
                    B0 = 4.4443F;
                    B1 = 1.7040F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // TA
                case FiaCode.QuercusChrysolepis:
                    B0 = 4.4443F;
                    B1 = 1.7040F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
                case FiaCode.AcerMacrophyllum:
                    B0 = 4.0953F;
                    B1 = 2.3849F;
                    B2 = -0.011630F;
                    PKDBH = 102.53F;
                    break;
                // Paine and Hann (1982) FRL Research Paper 46
                case FiaCode.QuercusGarryana:
                    B0 = 3.0785639F;
                    B1 = 1.9242211F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.QuercusKelloggii:
                    B0 = 3.3625F;
                    B1 = 2.0303F;
                    B2 = -0.0073307F;
                    PKDBH = 138.93F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.AlnusRubra:
                    B0 = 8.0F;
                    B1 = 1.53F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // GC
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 2.9793895F;
                    B1 = 1.5512443F;
                    B2 = -0.01416129F;
                    PKDBH = 54.77F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            float DBH = D;
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            float HT = H;
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
