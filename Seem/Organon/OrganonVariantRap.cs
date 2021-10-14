using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Tree;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonVariantRap : OrganonVariant
    {
        public OrganonVariantRap()
            : base(TreeModel.OrganonRap, 30.0F)
        {
        }

        protected override float GetCrownWidth(FiaCode species, float HLCW, float largestCrownWidth, float HT, float DBH, float XL)
        {
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
                    throw Trees.CreateUnhandledSpeciesException(species);
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

            float crownWidthMultiplier = MathV.Pow(RP, B1 + B2 * MathF.Sqrt(RP) + B3 * RATIO);
            Debug.Assert(crownWidthMultiplier >= 0.0F);
            Debug.Assert(crownWidthMultiplier <= 3.5F); // BUGBUG: red alder coefficients inherited from Fortran lead to negative powers of RP

            float crownWidth = largestCrownWidth * crownWidthMultiplier;
            return crownWidth;
        }

        public override float GetGrowthEffectiveAge(OrganonConfiguration configuration, OrganonStand stand, Trees trees, int treeIndex, out float potentialHeightGrowth)
        {
            float growthEffectiveAge;
            if (trees.Species == FiaCode.AlnusRubra)
            {
                // GROWTH EFFECTIVE AGE AND POTENTIAL HEIGHT GROWTH FROM WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM (2009) RED ALDER TOP HEIGHT GROWTH
                float siteIndexFromGround = stand.SiteIndexInFeet + 4.5F;
                RedAlder.WHHLB_HG(siteIndexFromGround, configuration.PDEN, trees.Height[treeIndex], 1.0F, out growthEffectiveAge, out potentialHeightGrowth);
            }
            else if (trees.Species == FiaCode.TsugaHeterophylla)
            {
                // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH
                // BUGBUG: why isn't redcedar also on this code path?
                float siteIndexFromGround = OrganonVariantNwo.ToHemlockSiteIndexStatic(stand.HemlockSiteIndexInFeet); // stand.HemlockSiteIndex is interpreted as PSME site index
                WesternHemlock.SiteConstants tsheSite = new(siteIndexFromGround);
                growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAge(tsheSite, this.TimeStepInYears, trees.Height[treeIndex], out potentialHeightGrowth);
            }
            else
            {
                // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                DouglasFir.SiteConstants psmeSite = new(stand.HemlockSiteIndexInFeet); // stand.HemlockSiteIndex is interpreted as PSME site index
                growthEffectiveAge = DouglasFir.GetPsmeAbgrGrowthEffectiveAge(psmeSite, this.TimeStepInYears, trees.Height[treeIndex], out potentialHeightGrowth);
            }
            return growthEffectiveAge;
        }

        public override void GetHeightPredictionCoefficients(FiaCode species, out float B0, out float B1, out float B2)
        {
            switch (species)
            {
                // Hann, Bluhm, and Hibbs(2011) Forest Biometrics Research Paper 1
                case FiaCode.AlnusRubra:
                    B0 = 6.75650139F;
                    B1 = -4.6252377F;
                    B2 = -0.23208200F;
                    break;
                // Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 7.262195456F;
                    B1 = -5.899759104F;
                    B2 = -0.287207389F;
                    break;
                // Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
                case FiaCode.TsugaHeterophylla:
                    B0 = 6.555344622F;
                    B1 = -5.137174162F;
                    B2 = -0.364550800F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = 6.14817441F;
                    B1 = -5.40092761F;
                    B2 = -0.38922036F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.AcerMacrophyllum:
                    B0 = 5.21462F;
                    B1 = -2.70252F;
                    B2 = -0.354756F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.CornusNuttallii:
                    B0 = 4.49727F;
                    B1 = -2.07667F;
                    B2 = -0.388650F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.Salix:
                    B0 = 4.88361F;
                    B1 = -2.47605F;
                    B2 = -0.309050F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }
        }

        // OG unused
        public override float GetHeightToCrownBase(FiaCode species, float HT, float DBH, float CCFL, float BA, float siteIndex, float hemlockSiteIndex, float OG)
        {
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            // float B6 = 0.0F; // always zero
            float K = 0.0F;
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
                    break;
                // Johnson (2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = 1.92682F;
                    B1 = -0.00280478F;
                    B2 = -0.0011939F;
                    B3 = -0.513134F;
                    B4 = 3.68901F;
                    B5 = 0.00742219F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = 4.49102006F;
                    B1 = 0.0F;
                    B2 = -0.00132412F;
                    B3 = -1.01460531F;
                    B4 = 0.0F;
                    B5 = 0.01340624F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.AcerMacrophyllum:
                    B0 = 0.9411395642F;
                    B1 = -0.00768402F;
                    B2 = -0.005476131F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
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
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            float HCB;
            if (species == FiaCode.AlnusRubra)
            {
                // HCB = (HT - K) / (1.0F + MathV.Exp(B0 + B1 * HT + B2 * CCFL + B3 * MathV.Ln(BA) + B4 * (DBH / HT) + B5 * SI_1 + B6 * OG * OG)) + K;
                HCB = (HT - K) / (1.0F + MathV.Exp(B0 + B1 * HT + B2 * CCFL + B3 * MathV.Ln(BA) + B4 * (DBH / HT) + B5 * siteIndex)) + K;
            }
            else
            {
                float SITE = hemlockSiteIndex;
                if (species == FiaCode.TsugaHeterophylla)
                {
                    SITE = 0.480F + (1.110F * (hemlockSiteIndex + 4.5F));
                }
                // HCB = HT / (1.0F + MathV.Exp(B0 + B1 * HT + B2 * CCFL + B3 * MathV.Ln(BA) + B4 * (DBH / HT) + B5 * SITE + B6 * OG * OG));
                HCB = HT / (1.0F + MathV.Exp(B0 + B1 * HT + B2 * CCFL + B3 * MathV.Ln(BA) + B4 * (DBH / HT) + B5 * SITE));
            }
            Debug.Assert(HCB >= 0.0F);
            Debug.Assert(HCB <= HT);
            return HCB;
        }

        protected override float GetHeightToLargestCrownWidth(FiaCode species, float HT, float CR)
        {
            // DISTANCE ABOVE CROWN BASE TO LARGEST CROWN WIDTH
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
                    throw Trees.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            float HLCW = HT - (1.0F - B1 * MathV.Exp(MathF.Pow(B2 * (1.0F - HT / 140.0F), 3))) * CL;
            return HLCW;
        }

        protected override float GetLargestCrownWidth(FiaCode species, float MCW, float CR, float DBH, float HT)
        {
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
                    throw Trees.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            float LCW = B0 * MCW * MathV.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT));
            return LCW;
        }

        public override float GetMaximumCrownWidth(FiaCode species, float D, float H)
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
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            float DBH = D;
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            float HT = H;
            float MCW;
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * MathV.Pow(DBH, K) + B2 * DBH * DBH;
            }
            return MCW;
        }

        protected override float GetMaximumHeightToCrownBase(FiaCode species, float HT, float CCFL)
        {
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
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            float MAXBR = B0 - B1 * MathV.Exp(B2 * MathV.Pow(CCFL / 100.0F, B3));
            if (MAXBR > LIMIT)
            {
                MAXBR = LIMIT;
            }
            float MAXHCB = MAXBR * HT;
            return MAXHCB;
        }

        public override void GrowDiameter(Trees trees, float growthMultiplier, float siteIndexFromDbh, OrganonStandDensity densityBeforeGrowth)
        {
            // These species were annualized by adding ln(0.2) to the intercept terms: DF, WH, RC, ACMA3, Cornus, Salix
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            float K1;
            float K2;
            float K3;
            float K4;
            float speciesMultiplier = 0.8F;
            switch (trees.Species)
            {
                // Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
                case FiaCode.AlnusRubra:
                    B0 = -4.622849554F;
                    B1 = 0.5112200516F;
                    B2 = -0.1040194568F;
                    B3 = 0.9536538143F;
                    B4 = 1.0659344724F;
                    B5 = -0.0193047405F;
                    B6 = -0.0773539455F;
                    K1 = 1.0F;
                    K2 = 1.0F;
                    K3 = 1.0F;
                    K4 = 1.0F;
                    speciesMultiplier = 1.0F;
                    break;
                // Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -6.95196910F;
                    B1 = 1.098406840F;
                    B2 = -0.05218621F;
                    B3 = 1.01380810F;
                    B4 = 0.91202025F;
                    B5 = -0.01756220F;
                    B6 = -0.05168923F;
                    K1 = 6.0F;
                    K2 = 1.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    speciesMultiplier = 1.0F;
                    break;
                // Unpublished equation on file at OSU Deptartment of Forest Resources
                case FiaCode.TsugaHeterophylla:
                    B0 = -6.48391203F;
                    B1 = 0.4150723209F;
                    B2 = -0.023744997F;
                    B3 = 0.907837299F;
                    B4 = 1.1346766989F;
                    B5 = -0.015333503F;
                    B6 = -0.03309787F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    speciesMultiplier = 1.0F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = -13.06399888F;
                    B1 = 0.784133664F;
                    B2 = -0.0261377888F;
                    B3 = 0.70174783F;
                    B4 = 2.057236260F;
                    B5 = -0.00415440257F;
                    B6 = 0.0F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.AcerMacrophyllum:
                    B0 = -5.02393713F;
                    B1 = 1.0F;
                    B2 = -0.05F;
                    B3 = 0.0F;
                    B4 = 0.324349277F;
                    B5 = 0.0F;
                    B6 = -0.0989519477F;
                    K1 = 10.0F;
                    K2 = 1.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = -9.69296474F;
                    B1 = 1.0F;
                    B2 = -0.00000035F;
                    B3 = 0.31176647F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = -0.0730788052F;
                    K1 = 4000.0F;
                    K2 = 4.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }

            speciesMultiplier *= growthMultiplier;
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    trees.DbhGrowth[treeIndex] = 0.0F;
                    continue;
                }

                float dbhInInches = trees.Dbh[treeIndex];
                float basalAreaLarger = densityBeforeGrowth.GetBasalAreaLarger(dbhInInches);
                float crownRatio = trees.CrownRatio[treeIndex];
                float LNDG = B0 + B1 * MathV.Ln(dbhInInches + K1) + B2 * MathV.Pow(dbhInInches, K2) + B3 * MathV.Ln((crownRatio + 0.2F) / 1.2F) + B4 * MathV.Ln(siteIndexFromDbh) + B5 * (MathV.Pow(basalAreaLarger, K3) / MathV.Ln(dbhInInches + K4)) + B6 * MathF.Sqrt(basalAreaLarger);
                float crownRatioAdjustment = OrganonGrowth.GetCrownRatioAdjustment(crownRatio);
                trees.DbhGrowth[treeIndex] = speciesMultiplier * MathV.Exp(LNDG) * crownRatioAdjustment;
                Debug.Assert(trees.DbhGrowth[treeIndex] > 0.0F);
                Debug.Assert(trees.DbhGrowth[treeIndex] < Constant.Maximum.DiameterIncrementInInches);
            }
        }

        public override int GrowHeightBigSix(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
        {
            // WEIGHTED CENTRAL PAI PROCEDURE PARAMETERS FOR RED ALDER
            float P1;
            float P2;
            float P3;
            float P4;
            float P5;
            float P6;
            float P7;
            float P8;
            switch (trees.Species)
            {
                // Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
                case FiaCode.AlnusRubra:
                    P1 = 0.809837005F;
                    P2 = -0.0134163653F;
                    P3 = -0.0609398629F;
                    P4 = 0.5F;
                    P5 = 1.0F;
                    P6 = 2.0F;
                    P7 = 0.1469442410F;
                    P8 = 1.0476380753F;
                    break;
                // Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
                case FiaCode.PseudotsugaMenziesii:
                    P1 = 0.655258886F;
                    P2 = -0.006322913F;
                    P3 = -0.039409636F;
                    P4 = 0.5F;
                    P5 = 0.597617316F;
                    P6 = 2.0F;
                    P7 = 0.631643636F;
                    P8 = 1.010018427F;
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    P1 = 1.0F;
                    P2 = -0.0384415F;
                    P3 = -0.0144139F;
                    P4 = 0.5F;
                    P5 = 1.04409F;
                    P6 = 2.0F;
                    P7 = 0.0F;
                    P8 = 1.03F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }

            int oldTreeRecordCount = 0;
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    trees.HeightGrowth[treeIndex] = 0.0F;
                    continue;
                }

                float growthEffectiveAge = configuration.Variant.GetGrowthEffectiveAge(configuration, stand, trees, treeIndex, out float potentialHeightGrowth);
                float crownCompetitionIncrement = OrganonVariant.GetCrownCompetitionFactorByHeight(trees.Height[treeIndex], crownCompetitionByHeight);

                float crownRatio = trees.CrownRatio[treeIndex];
                float FCR = -P5 * MathV.Pow(1.0F - crownRatio, P6) * MathV.Exp(P7 * MathF.Sqrt(crownCompetitionIncrement));
                float B0 = P1 * MathV.Exp(P2 * crownCompetitionIncrement);
                float B1 = MathV.Exp(P3 * MathV.Pow(crownCompetitionIncrement, P4));
                float MODIFER = P8 * (B0 + (B1 - B0) * MathV.Exp(FCR));
                float CRADJ = OrganonGrowth.GetCrownRatioAdjustment(crownRatio);
                float heightGrowth = potentialHeightGrowth * MODIFER * CRADJ;
                Debug.Assert(heightGrowth > 0.0F);
                trees.HeightGrowth[treeIndex] = heightGrowth;

                if (growthEffectiveAge > configuration.Variant.OldTreeAgeThreshold)
                {
                    ++oldTreeRecordCount;
                }
            }
            return oldTreeRecordCount;
        }

        public override void ReduceExpansionFactors(OrganonStand stand, OrganonStandDensity densityBeforeGrowth, Trees trees, float fertilizationExponent)
        {
            // RAP MORTALITY
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float POW = 0.2F;
            switch (trees.Species)
            {
                // RA Coefficients from Hann, Bluhm, and Hibbs New Red Alder Equation
                case FiaCode.AlnusRubra:
                    B0 = -4.333150734F;
                    B1 = -0.9856713799F;
                    B2 = 0.0F;
                    B3 = -2.583317081F;
                    B4 = 0.0369852164F;
                    B5 = 0.0394546978F;
                    POW = 1.0F;
                    break;
                // Hann, Marshall, and Hanus(2006) FRL Research Contribution 49
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -3.12161659F;
                    B1 = -0.44724396F;
                    B2 = 0.0F;
                    B3 = -2.48387172F;
                    B4 = 0.01843137F;
                    B5 = 0.01353918F;
                    break;
                // Hann, Marshall, Hanus(2003) FRL Research Contribution 40
                case FiaCode.TsugaHeterophylla:
                    B0 = -0.761609F;
                    B1 = -0.529366F;
                    B2 = 0.0F;
                    B3 = -4.74019F;
                    B4 = 0.0119587F;
                    B5 = 0.00756365F;
                    break;
                // WH of Hann, Marshall, Hanus(2003) FRL Research Contribution 40
                case FiaCode.ThujaPlicata:
                    B0 = -0.761609F;
                    B1 = -0.529366F;
                    B2 = 0.0F;
                    B3 = -4.74019F;
                    B4 = 0.0119587F;
                    B5 = 0.00756365F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AcerMacrophyllum:
                    B0 = -2.976822456F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -6.223250962F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.CornusNuttallii:
                    B0 = -3.020345211F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -8.467882343F;
                    B4 = 0.013966388F;
                    B5 = 0.009461545F;
                    break;
                // best guess
                case FiaCode.Salix:
                    B0 = -1.386294361F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }

            // BUGBUG: Fortran code didn't use red alder site index for red alder and used hemlock site index for all conifers, not just hemlock and redcedar
            float siteIndex = stand.HemlockSiteIndexInFeet; // interpreted as Douglas-fir site index
            if (trees.Species == FiaCode.AlnusRubra)
            {
                siteIndex = stand.SiteIndexInFeet;
            }
            else if ((trees.Species == FiaCode.TsugaHeterophylla) || (trees.Species == FiaCode.ThujaPlicata))
            {
                siteIndex = OrganonVariantNwo.ToHemlockSiteIndexStatic(siteIndex); // convert from Douglas-fir site index to actual hemlock site inde
            }

            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    continue;
                }
                float dbhInInches = trees.Dbh[treeIndex];
                float basalAreaLarger = densityBeforeGrowth.GetBasalAreaLarger(dbhInInches);
                float crownRatio = trees.CrownRatio[treeIndex];
                float PMK = B0 + B1 * dbhInInches + B2 * dbhInInches * dbhInInches + B3 * crownRatio + B4 * siteIndex + B5 * basalAreaLarger + fertilizationExponent;

                float XPM = 1.0F / (1.0F + MathV.Exp(-PMK));
                float survivalProbability = POW == 1.0 ? 1.0F - XPM : MathV.Pow(1.0F - XPM, POW); // RAP is the only variant using unfertlized POW != 1
                survivalProbability *= OrganonGrowth.GetCrownRatioAdjustment(crownRatio);
                Debug.Assert(survivalProbability >= 0.0F);
                Debug.Assert(survivalProbability <= 1.0F);

                float newLiveExpansionFactor = survivalProbability * trees.LiveExpansionFactor[treeIndex];
                if (newLiveExpansionFactor < 0.00001F)
                {
                    newLiveExpansionFactor = 0.0F;
                }
                float newlyDeadExpansionFactor = trees.LiveExpansionFactor[treeIndex] - newLiveExpansionFactor;

                trees.DeadExpansionFactor[treeIndex] = newlyDeadExpansionFactor;
                trees.LiveExpansionFactor[treeIndex] = newLiveExpansionFactor;
            }
        }

        public override float ToHemlockSiteIndex(float siteIndex)
        {
            if (siteIndex < Constant.Minimum.SiteIndexInFeet)
            {
                throw new ArgumentOutOfRangeException(nameof(siteIndex));
            }
            return siteIndex;
        }

        public override float ToSiteIndex(float hemlockSiteIndex)
        {
            throw new NotSupportedException();
        }
    }
}
