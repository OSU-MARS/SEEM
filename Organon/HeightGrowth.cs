using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class HeightGrowth
    {
        /// <summary>
        /// Estimate growth effective age for Douglas-fir and grand fir using Bruce's (1981) dominant height model.
        /// </summary>
        /// <param name="siteIndexFromGround">Site index from ground in feet.</param>
        /// <param name="treeHeight">Tree height in feet.</param>
        /// <param name="GP"></param>
        /// <param name="growthEffectiveAge">Growth effective age in years.</param>
        /// <param name="potentialHeightGrowth"></param>
        /// <remarks>
        /// Bruce. 1981. Forest Science 27:711-725.
        /// </remarks>
        public static void BrucePsmeAbgrGrowthEffectiveAge(float siteIndexFromGround, float treeHeight, float GP, out float growthEffectiveAge, out float potentialHeightGrowth)
        {
            float X1 = 13.25F - siteIndexFromGround / 20.0F;
            float X2 = 63.25F - siteIndexFromGround / 20.0F;
            float B2 = -0.447762F - 0.894427F * siteIndexFromGround / 100.0F + 0.793548F * (float)Math.Pow(siteIndexFromGround / 100.0, 2.0) - 0.171666F * (float)Math.Pow(siteIndexFromGround / 100.0, 3.0);
            float B1 = (float)(Math.Log(4.5 / siteIndexFromGround) / (Math.Pow(X1, B2) - Math.Pow(X2, B2)));
            float XX1 = (float)(Math.Log(treeHeight / siteIndexFromGround) / B1 + Math.Pow(X2, B2));
            if (XX1 > 0.0F)
            {
                growthEffectiveAge = (float)Math.Pow(XX1, 1.0 / B2) - X1;
            }
            else
            {
                growthEffectiveAge = 500.0F;
            }
            float potentialHeight = siteIndexFromGround * (float)Math.Exp(B1 * (Math.Pow(growthEffectiveAge + GP + X1, B2) - Math.Pow(X2, B2)));
            potentialHeightGrowth = potentialHeight - treeHeight;
        }

        /// <summary>
        /// Calculate western hemlock growth effective age and potential height growth using Flewelling's model for dominant individuals.
        /// </summary>
        /// <param name="siteIndexFromGround">Site index (feet) from ground.</param>
        /// <param name="treeHeight">Height of tree.</param>
        /// <param name="GP"></param>
        /// <param name="growthEffectiveAge">Growth effective age of tree.</param>
        /// <param name="potentialHeightGrowth">Potential height growth increment in feet.</param>
        public static void F_HG(float siteIndexFromGround, float treeHeight, float GP, out float growthEffectiveAge, out float potentialHeightGrowth)
        {
            // For Western Hemlock compute Growth Effective Age and 5-year potential
            // or 1-year height growth using the western hemlock top height curves of
            // Flewelling.These subroutines are required:
            // SITECV_F   computes top height from site and age
            // SITEF_C computes model parameters
            // SITEF_SI   calculates an approximate psi for a given site
            // Note: Flewelling's curves are metric.
            // Site Index is not adjusted for stump height.
            float SIM = siteIndexFromGround * 0.3048F;
            float HTM = treeHeight * 0.3048F;

            // find growth effective age within precision of 0.01 years
            float HTOP;
            float AGE = 1.0F;
            for (int I = 0; I < 4; ++I)
            {
                do
                {
                    AGE += 100.0F / (float)Math.Pow(10.0, I);
                    if (AGE > 500.0F)
                    {
                        growthEffectiveAge = 500.0F;
                        WesternHemlock.SITECV_F(SIM, growthEffectiveAge, out float XHTOP1);
                        WesternHemlock.SITECV_F(SIM, growthEffectiveAge + GP, out float XHTOP2);
                        potentialHeightGrowth = 3.2808F * (XHTOP2 - XHTOP1);
                        return;
                    }
                    WesternHemlock.SITECV_F(SIM, AGE, out HTOP);
                }
                while (HTOP < HTM);
                AGE -= 100.0F / (float)Math.Pow(10.0, I);
            }
            growthEffectiveAge = AGE; 

            // Compute top height and potential height growth
            WesternHemlock.SITECV_F(SIM, growthEffectiveAge + GP, out HTOP);
            float potentialTopHeightInFeet = HTOP * 3.2808F;
            potentialHeightGrowth = potentialTopHeightInFeet - treeHeight;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeIndex">Index of tree to grow in tree data.</param>
        /// <param name="variant">Organon variant.</param>
        /// <param name="simulationStep">Simulation cycle.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="SI_1">Primary site index from breast height.</param>
        /// <param name="SI_2">Secondary site index from breast height.</param>
        /// <param name="CCH">Canopy height? (DOUG?)</param>
        /// <param name="PN"></param>
        /// <param name="YF"></param>
        /// <param name="BABT">Thinning?</param>
        /// <param name="BART">Thinning?</param>
        /// <param name="YT">Thinning?</param>
        /// <param name="OLD">Obsolete?</param>
        /// <param name="PDEN">(DOUG?)</param>
        public static void GrowBigSixSpecies(int treeIndex, OrganonVariant variant, int simulationStep, Stand stand, float SI_1, float SI_2,
                                             float[] CCH, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, ref float OLD, float PDEN)
        {
            Debug.Assert(variant.IsBigSixSpecies(stand.Species[treeIndex]));
            // BUGBUG remove M and ON
            // CALCULATE 5-YEAR HEIGHT GROWTH
            float CR = stand.CrownRatio[treeIndex];

            // FOR MAJOR SPECIES
            float XI = 40.0F * (stand.Height[treeIndex] / CCH[40]);
            int I = (int)XI + 2;
            float XXI = (float)I - 1.0F;
            float TCCH;
            if (stand.Height[treeIndex] >= CCH[40])
            {
                TCCH = 0.0F;
            }
            else if (I == 41)
            {
                TCCH = CCH[39] * (40.0F - XI);
            }
            else
            {
                TCCH = CCH[I] + (CCH[I - 1] - CCH[I]) * (XXI - XI);
            }

            // COMPUTE HEIGHT GROWTH OF UNTREATED TREES
            // IDXAGE index age? (DOUG?)
            // BUGBUG: move old index age to variant capabilities
            FiaCode species = stand.Species[treeIndex];
            float growthEffectiveAge;
            float potentialHeightGrowth;
            float oldIndexAge;
            switch (variant.Variant)
            {
                case Variant.Swo:
                    float siteIndexFromGround = SI_1;
                    bool treatAsDouglasFir = false;
                    // POTENTIAL HEIGHT GROWTH FROM HANN AND SCRIVANI'S (1987) DOMINANT HEIGHT GROWTH EQUATION
                    if (species == FiaCode.PinusPonderosa)
                    {
                        siteIndexFromGround = SI_2;
                    }
                    else
                    {
                        if (species == FiaCode.CalocedrusDecurrens)
                        {
                            siteIndexFromGround = (SI_1 + 4.5F) * 0.66F - 4.5F;
                        }
                        treatAsDouglasFir = true;
                    }
                    HS_HG(treatAsDouglasFir, siteIndexFromGround, stand.Height[treeIndex], out growthEffectiveAge, out potentialHeightGrowth);
                    oldIndexAge = 500.0F;
                    break;
                case Variant.Nwo:
                    float GP = 5.0F;
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH
                        siteIndexFromGround = SI_2 + 4.5F;
                        F_HG(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    else
                    {
                        // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                        siteIndexFromGround = SI_1 + 4.5F;
                        BrucePsmeAbgrGrowthEffectiveAge(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    oldIndexAge = 120.0F;
                    break;
                case Variant.Smc:
                    GP = 5.0F;
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK
                        // DOMINANT HEIGHT GROWTH
                        siteIndexFromGround = SI_2 + 4.5F;
                        F_HG(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    else
                    {
                        // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                        siteIndexFromGround = SI_1 + 4.5F;
                        BrucePsmeAbgrGrowthEffectiveAge(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    oldIndexAge = 120.0F;
                    break;
                case Variant.Rap:
                    GP = 1.0F;
                    if (species == FiaCode.AlnusRubra)
                    {
                        // POTENTIAL HEIGHT GROWTH FROM WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM(2009) RED ALDER TOP HEIGHT GROWTH
                        siteIndexFromGround = SI_1 + 4.5F;
                        RedAlder.WHHLB_HG(siteIndexFromGround, PDEN, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    else if (species == FiaCode.TsugaHeterophylla)
                    {
                        // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH
                        siteIndexFromGround = -0.432F + 0.899F * (SI_2 + 4.5F);
                        F_HG(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    else
                    {
                        // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                        siteIndexFromGround = SI_2 + 4.5F;
                        BrucePsmeAbgrGrowthEffectiveAge(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    oldIndexAge = 30.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
            }

            float heightGrowthInFeet = variant.GrowHeightBigSix(species, potentialHeightGrowth, CR, TCCH);
            if (variant.IsBigSixSpecies(species) && (growthEffectiveAge > oldIndexAge))
            {
                OLD += 1.0F;
            }

            HG_FERT(simulationStep, variant, species, SI_1, PN, YF, out float FERTADJ);
            HG_THIN(simulationStep, variant, species, BABT, BART, YT, out float THINADJ);
            stand.HeightGrowth[treeIndex] = heightGrowthInFeet * THINADJ * FERTADJ;
            LIMIT(variant, species, stand.Dbh[treeIndex], stand.Height[treeIndex], stand.DbhGrowth[treeIndex], ref stand.HeightGrowth[treeIndex]);
        }

        public static void GrowMinorSpecies(int treeIndex, OrganonVariant variant, Stand stand, Dictionary<FiaCode, float[]> CALIB)
        {
            float dbhInInches = stand.Dbh[treeIndex];
            FiaCode species = stand.Species[treeIndex];
            Debug.Assert(variant.IsBigSixSpecies(species) == false);

            float previousDbhInInches = dbhInInches - stand.DbhGrowth[treeIndex];
            float PRDHT1 = variant.GetPredictedHeight(species, previousDbhInInches);
            float PRDHT2 = variant.GetPredictedHeight(species, dbhInInches);
            PRDHT1 = 4.5F + CALIB[species][0] * (PRDHT1 - 4.5F);
            PRDHT2 = 4.5F + CALIB[species][0] * (PRDHT2 - 4.5F);
            float PRDHT = (PRDHT2 / PRDHT1) * stand.Height[treeIndex];

            // RED ALDER HEIGHT GROWTH
            if ((species == FiaCode.AlnusRubra) && (variant.Variant != Variant.Rap))
            {
                float growthEffectiveAge = RedAlder.GetGrowthEffectiveAge(stand.Height[treeIndex], stand.RedAlderSiteIndex);
                if (growthEffectiveAge <= 0.0F)
                {
                    stand.HeightGrowth[treeIndex] = 0.0F;
                }
                else
                {
                    // BUGBUG: this is strange as it appears to assume red alders are always dominant trees
                    float RAH1 = RedAlder.GetH50(growthEffectiveAge, stand.RedAlderSiteIndex);
                    float RAH2 = RedAlder.GetH50(growthEffectiveAge + Constant.DefaultTimeStepInYears, stand.RedAlderSiteIndex);
                    float redAlderHeightGrowth = RAH2 - RAH1;
                    stand.HeightGrowth[treeIndex] = redAlderHeightGrowth;
                }
            }
            else
            {
                stand.HeightGrowth[treeIndex] = PRDHT - stand.Height[treeIndex];
            }
        }

        private static void HG_FERT(int simulationStep, OrganonVariant variant, FiaCode species, float siteIndexFromBreastHeight, float[] PN, float[] YF, out float FERTADJ)
        {
            float PF1;
            float PF2;
            float PF3;
            float PF4;
            float PF5;
            if (variant.Variant != Variant.Rap)
            {
                if (species == FiaCode.PseudotsugaMenziesii)
                {
                    PF1 = 1.0F;
                    PF2 = 0.333333333F;
                    PF3 = -1.107409443F;
                    PF4 = -2.133334346F;
                    PF5 = 1.5F;
                }
                else
                {
                    PF1 = 0.0F;
                    PF2 = 1.0F;
                    PF3 = 0.0F;
                    PF4 = 0.0F;
                    PF5 = 1.0F;
                }
            }
            else
            {
                PF1 = 0.0F;
                PF2 = 1.0F;
                PF3 = 0.0F;
                PF4 = 0.0F;
                PF5 = 1.0F;
            }

            float FALDWN = 1.0F;
            float XTIME = Constant.DefaultTimeStepInYears * (float)simulationStep;
            float FERTX1 = 0.0F;
            for (int I = 1; I < 5; ++I)
            {
                FERTX1 += (PN[I] / 800.0F) * (float)Math.Exp((PF3 / PF2) * (YF[0] - YF[I]));
            }
            float FERTX2 = (float)Math.Exp(PF3 * (XTIME - YF[0]) + PF4 * Math.Pow(siteIndexFromBreastHeight / 100.0, PF5));
            FERTADJ = 1.0F + PF1 * (float)(Math.Pow(PN[0] / 800.0 + FERTX1, PF2) * FERTX2) * FALDWN;
            Debug.Assert(FERTADJ >= 1.0F);
        }

        private static void HG_THIN(int simulationStep, OrganonVariant variant, FiaCode species, float BABT, float[] BART, float[] YT, out float THINADJ)
        {
            float PT1;
            float PT2;
            float PT3;
            if (variant.Variant != Variant.Rap)
            {
                if (species == FiaCode.PseudotsugaMenziesii)
                {
                    PT1 = -0.3197415492F;
                    PT2 = 0.7528887377F;
                    PT3 = -0.2268800162F;
                }
                else
                {
                    PT1 = 0.0F;
                    PT2 = 1.0F;
                    PT3 = 0.0F;
                }
            }
            else
            {
                if (species == FiaCode.AlnusRubra)
                {
                    PT1 = -0.613313694F;
                    PT2 = 1.0F;
                    PT3 = -0.443824038F;
                }
                else if (species == FiaCode.PseudotsugaMenziesii)
                {
                    PT1 = -0.3197415492F;
                    PT2 = 0.7528887377F;
                    PT3 = -0.2268800162F;
                }
                else
                {
                    PT1 = 0.0F;
                    PT2 = 1.0F;
                    PT3 = 0.0F;
                }
            }
            float XTIME = 5.0F * (float)simulationStep;
            float THINX1 = 0.0F;
            for (int I = 1; I < 5; ++I)
            {
                THINX1 += BART[I] * (float)Math.Exp((PT3 / PT2) * (YT[0] - YT[0]));
            }
            float THINX2 = THINX1 + BART[0];
            float THINX3 = THINX1 + BABT;
            float PREM;
            if (THINX3 <= 0.0F)
            {
                PREM = 0.0F;
            }
            else
            {
                PREM = THINX2 / THINX3;
            }
            if (PREM > 0.75F)
            {
                PREM = 0.75F;
            }
            THINADJ = 1.0F + PT1 * (float)(Math.Pow(PREM, PT2) * Math.Exp(PT3 * (XTIME - YT[0])));
            Debug.Assert(THINADJ >= 0.0F);
        }

        /// <summary>
        /// Calculate Douglas-fir and ponderosa growth effective age and potential height growth for southwest Oregon.
        /// </summary>
        /// <param name="isDouglasFir">Douglas-fir coefficients are used if ISP == 1, ponderosa otherwise.</param>
        /// <param name="SI">Site index (feet) from breast height.</param>
        /// <param name="HT">Height of tree.</param>
        /// <param name="GEAGE">Growth effective age of tree.</param>
        /// <param name="PHTGRO">Potential height growth increment in feet.</param>
        /// <remarks>
        /// Derived from the code in appendix 2 of Hann and Scrivani 1987 (FRL Research Bulletin 59). Growth effective age is introduced in 
        /// Hann and Ritchie 1988 (Height Growth Rate of Douglas-Fir: A Comparison of Model Forms. Forest Science 34(1): 165–175.).
        /// </remarks>
        public static void HS_HG(bool isDouglasFir, float SI, float HT, out float GEAGE, out float PHTGRO)
        {
            // BUGBUG these are a0, a1, and a2 in the paper
            float B0;
            float B1;
            float B2;
            if (isDouglasFir)
            {
                // PSME
                B0 = -6.21693F;
                B1 = 0.281176F;
                B2 = 1.14354F;
            }
            else
            {
                // PIPO
                B0 = -6.54707F;
                B1 = 0.288169F;
                B2 = 1.21297F;
            }

            float BBC = B0 + B1 * (float)Math.Log(SI);
            float X50 = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * 3.912023F));
            float A1A = 1.0F - (HT - 4.5F) * (X50 / SI);
            if (A1A <= 0.0F)
            {
                GEAGE = 500.0F;
                PHTGRO = 0.0F;
            }
            else
            {
                GEAGE = (float)Math.Pow((-1.0F * Math.Log(A1A)) / (Math.Exp(B0) * Math.Pow(SI, B1)), (1.0F / B2));
                float XAI = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * Math.Log(GEAGE)));
                float XAI5 = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * Math.Log(GEAGE + 5.0)));
                PHTGRO = (4.5F + (HT - 4.5F) * (XAI5 / XAI)) - HT;
            }
        }

        private static void LIMIT(OrganonVariant variant, FiaCode species, float DBH, float HT, float DG, ref float HG)
        {
            FiaCode speciesWithSwoTsheOptOut = species;
            if ((species == FiaCode.TsugaHeterophylla) && (variant.Variant == Variant.Swo))
            {
                // BUGBUG: not clear why SWO uses default coefficients for hemlock
                speciesWithSwoTsheOptOut = FiaCode.NotholithocarpusDensiflorus;
            }

            float A0;
            float A1;
            float A2;
            switch (speciesWithSwoTsheOptOut)
            {
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.TsugaHeterophylla:
                    A0 = 19.04942539F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    A0 = 16.26279948F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.PinusPonderosa:
                    A0 = 17.11482201F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.PinusLambertiana:
                    A0 = 14.29011403F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.AlnusRubra:
                    A0 = 60.619859F;
                    A1 = -1.59138564F;
                    A2 = 0.496705997F;
                    break;
                default:
                    A0 = 15.80319194F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
            }

            float HT1 = HT - 4.5F;
            float HT2 = HT1 + HG;
            float HT3 = HT2 + HG;
            float DBH1 = DBH;
            float DBH2 = DBH1 + DG;
            float DBH3 = DBH2 + DG;
            float PHT1 = A0 * DBH1 / (1.0F - A1 * (float)Math.Pow(DBH1, A2));
            float PHT2 = A0 * DBH2 / (1.0F - A1 * (float)Math.Pow(DBH2, A2));
            float PHT3 = A0 * DBH3 / (1.0F - A1 * (float)Math.Pow(DBH3, A2));
            float PHGR1 = (PHT2 - PHT1 + HG) / 2.0F;
            float PHGR2 = PHT2 - HT1;
            if (HT2 > PHT2)
            {
                if (PHGR1 < PHGR2)
                {
                    HG = PHGR1;
                }
                else
                {
                    HG = PHGR2;
                }
            }
            else
            {
                if (HT3 > PHT3)
                {
                    HG = PHGR1;
                }
            }
            if (HG < 0.0F)
            {
                HG = 0.0F;
            }
        }
    }
}
