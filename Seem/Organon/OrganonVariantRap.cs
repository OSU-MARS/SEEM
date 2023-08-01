using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Organon
{
    public class OrganonVariantRap : OrganonVariant
    {
        public OrganonVariantRap()
            : base(TreeModel.OrganonRap, 30.0F)
        {
        }

        protected override OrganonCrownCoefficients CreateCrownCoefficients(FiaCode species)
        {
            return species switch
            {
                FiaCode.AlnusRubra => new OrganonCrownCoefficients()
                {
                    // Hann, Bluhm, and Hibbs (2011) Development and Evaluation of the Tree-Level Equations and Their Combined 
                    //   Stand-Level Behavior in the Red Alder Plantation Version of Organon
                    HcbB0 = 3.73113020F,
                    HcbB1 = -0.021546486F,
                    HcbB2 = -0.0016572840F,
                    HcbB3 = -1.0649544F,
                    HcbB4 = 7.47699601F,
                    HcbB5 = 0.0252953320F,
                    HcbK = 1.6F,
                    // Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
                    McwB0 = 2.320746348F,
                    McwB1 = 6.661401926F,
                    McwB2 = 0.0F,
                    McwK = 0.6F,
                    HeightToCrownBaseRatioLimit = 0.92F,
                    MhcbB0 = 0.93F,
                    MhcbB1 = 0.18F,
                    MhcbB2 = -0.928243505F,
                    MhcbB3 = 1.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,
                    HlcwB1 = 0.63619616F,
                    HlcwB2 = -1.2180562F,
                    LcwB0 = 0.78160725F,
                    LcwB1 = 0.44092737F,
                    LcwB2 = 0.0F,
                    LcwB3 = 0.0F,
                    CWb1 = 0.63420194F,
                    CWb2 = 0.17649614F,
                    CWb3 = -0.02315018F,
                },
                FiaCode.PseudotsugaMenziesii => new OrganonCrownCoefficients()
                {
                    // Hann and Hanus (2004) FS 34: 1193-2003
                    HcbB0 = 6.18464679F,
                    HcbB1 = -0.00328764F,
                    HcbB2 = -0.00136555F,
                    HcbB3 = -1.19702220F,
                    HcbB4 = 3.17028263F,
                    HcbB5 = 0.0F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 4.6198F,
                    McwB1 = 1.8426F,
                    McwB2 = -0.011311F,
                    DbhLimitForMaxCrownWidth = 81.45F,

                    MhcbB0 = 0.96F,
                    MhcbB1 = 0.26F,
                    MhcbB2 = -0.34758F,
                    MhcbB3 = 1.5F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann(1999) FS 45: 217-225
                    HlcwB1 = 0.062000F,
                    HlcwB2 = 0.0F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.00436324F,
                    LcwB3 = 0.6020020F,
                    // Hann(1999) FS 45: 217-225
                    CWb1 = 0.929973F,
                    CWb2 = -0.135212F,
                    CWb3 = -0.0157579F,
                    CWMaxHeightDiameterRatio = 50.0F
                },
                FiaCode.TsugaHeterophylla => new OrganonCrownCoefficients()
                {
                    // Johnson (2002) Willamette Industries Report
                    HcbB0 = 1.92682F,
                    HcbB1 = -0.00280478F,
                    HcbB2 = -0.0011939F,
                    HcbB3 = -0.513134F,
                    HcbB4 = 3.68901F,
                    HcbB5 = 0.00742219F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 4.5652F,
                    McwB1 = 1.4147F,
                    McwB2 = 0.0F,
                    CWMaxHeightDiameterRatio = 999.99F,

                    MhcbB0 = 1.01F,
                    MhcbB1 = 0.36F,
                    MhcbB2 = 0.944528054F,
                    MhcbB3 = 0.6F,
                    HeightToCrownBaseRatioLimit = 0.96F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.209806F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.629785F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.ThujaPlicata => new OrganonCrownCoefficients()
                {
                    // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
                    HcbB0 = 4.49102006F,
                    HcbB1 = 0.0F,
                    HcbB2 = -0.00132412F,
                    HcbB3 = -1.01460531F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.01340624F,
                    // Smith(1966) Proc. 6th World Forestry Conference
                    McwB0 = 4.0F,
                    McwB1 = 1.65F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 0.96F,
                    MhcbB1 = 0.31F,
                    MhcbB2 = -1.059636222F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.209806F,
                    // IC of Hann(1997) FRL Research Contribution 17
                    LcwB1 = -0.2513890F,
                    LcwB2 = 0.006925120F,
                    LcwB3 = 0.985922F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.629785F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.AcerMacrophyllum => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.9411395642F,
                    HcbB1 = -0.00768402F,
                    HcbB2 = -0.005476131F,
                    HcbB3 = 0.0F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    // Ek (1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
                    McwB0 = 4.0953F,
                    McwB1 = 2.3849F,
                    McwB2 = -0.011630F,
                    DbhLimitForMaxCrownWidth = 102.53F,

                    MhcbB0 = 1.0F,
                    MhcbB1 = 0.45F,
                    MhcbB2 = -1.020016685F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.47018F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.CornusNuttallii => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.0F,
                    HcbB1 = 0.0F,
                    HcbB2 = -0.005666559F,
                    HcbB3 = -0.745540494F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.038476613F,
                    // GC of Paine and Hann (1982) FRL Research Paper 46
                    McwB0 = 2.9793895F,
                    McwB1 = 1.5512443F,
                    McwB2 = -0.01416129F,
                    DbhLimitForMaxCrownWidth = 54.77F,

                    MhcbB0 = 1.0F,
                    MhcbB1 = 0.45F,
                    MhcbB2 = -1.020016685F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // GC of Hann (1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.61440F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.Salix => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.0F,
                    HcbB1 = 0.0F,
                    HcbB2 = -0.005666559F,
                    HcbB3 = -0.745540494F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.038476613F,
                    // GC of Paine and Hann (1982) FRL Research Paper 46
                    McwB0 = 2.9793895F,
                    McwB1 = 1.5512443F,
                    McwB2 = -0.01416129F,
                    DbhLimitForMaxCrownWidth = 54.77F,

                    MhcbB0 = 0.985F,
                    MhcbB1 = 0.285F,
                    MhcbB2 = -0.969750805F,
                    MhcbB3 = 0.9F,
                    HeightToCrownBaseRatioLimit = 0.98F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // GC of Hann (1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.61440F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                _ => throw Trees.CreateUnhandledSpeciesException(species)
            };
        }

        protected override OrganonHeightCoefficients CreateHeightCoefficients(FiaCode species)
        {
            return species switch
            {
                // Hann, Bluhm, and Hibbs(2011) Forest Biometrics Research Paper 1
                FiaCode.AlnusRubra => new OrganonHeightCoefficients()
                {
                    B0 = 6.75650139F,
                    B1 = -4.6252377F,
                    B2 = -0.23208200F,
                    // Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
                    P1 = 0.809837005F,
                    P2 = -0.0134163653F,
                    P3 = -0.0609398629F,
                    P4 = 0.5F,
                    P5 = 1.0F,
                    P6 = 2.0F,
                    P7 = 0.1469442410F,
                    P8 = 1.0476380753F
                },
                // Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
                FiaCode.PseudotsugaMenziesii => new OrganonHeightCoefficients()
                {
                    B0 = 7.262195456F,
                    B1 = -5.899759104F,
                    B2 = -0.287207389F,
                    // Hann, Marshall, and Hanus (2006) FRL Research Contribution ??
                    P1 = 0.655258886F,
                    P2 = -0.006322913F,
                    P3 = -0.039409636F,
                    P4 = 0.5F,
                    P5 = 0.597617316F,
                    P6 = 2.0F,
                    P7 = 0.631643636F,
                    P8 = 1.010018427F
                },
                // Hanus, Marshall, and Hann (1999) FRL Research Contribution 25
                FiaCode.TsugaHeterophylla => new OrganonHeightCoefficients()
                {
                    B0 = 6.555344622F,
                    B1 = -5.137174162F,
                    B2 = -0.364550800F,
                    // Johnson (2002) Willamette Industries Report
                    P1 = 1.0F,
                    P2 = -0.0384415F,
                    P3 = -0.0144139F,
                    P4 = 0.5F,
                    P5 = 1.04409F,
                    P6 = 2.0F,
                    P7 = 0.0F,
                    P8 = 1.03F
                },
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
                FiaCode.ThujaPlicata => new OrganonHeightCoefficients()
                {
                    B0 = 6.14817441F,
                    B1 = -5.40092761F,
                    B2 = -0.38922036F
                    // not a big six species for RAP
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.AcerMacrophyllum => new OrganonHeightCoefficients()
                {
                    B0 = 5.21462F,
                    B1 = -2.70252F,
                    B2 = -0.354756F
                    // not a big six species
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.CornusNuttallii => new OrganonHeightCoefficients()
                {
                    B0 = 4.49727F,
                    B1 = -2.07667F,
                    B2 = -0.388650F
                    // not a big six species
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.Salix => new OrganonHeightCoefficients()
                {
                    B0 = 4.88361F,
                    B1 = -2.47605F,
                    B2 = -0.309050F
                    // not a big six species
                },
                _ => throw Trees.CreateUnhandledSpeciesException(species)
            };
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

        public override float GetHeightToCrownBase(OrganonStand stand, FiaCode species, float heightInFeet, float dbhInInches, float CCFL, StandDensity standDensity, float oldGrowthIndex)
        {
            float basalAreaPerAcre = Constant.HectaresPerAcre * Constant.SquareFeetPerSquareMeter * standDensity.BasalAreaPerHa;
            OrganonCrownCoefficients crown = this.GetOrCreateCrownCoefficients(species);
            if (species == FiaCode.AlnusRubra)
            {
                float alderHeightToCrownBase = (heightInFeet - crown.HcbK) / (1.0F + MathV.Exp(crown.HcbB0 + crown.HcbB1 * heightInFeet + crown.HcbB2 * CCFL + crown.HcbB3 * MathV.Ln(basalAreaPerAcre) + crown.HcbB4 * (dbhInInches / heightInFeet) + crown.HcbB5 * (stand.SiteIndexInFeet - 4.5F) + crown.HcbB6 * oldGrowthIndex * oldGrowthIndex)) + crown.HcbK;
                Debug.Assert(alderHeightToCrownBase >= 0.0F);
                Debug.Assert(alderHeightToCrownBase <= heightInFeet);
                return alderHeightToCrownBase;
            }

            float siteIndexFromDbh = this.GetSiteIndex(stand, species) - 4.5F;
            float heightToCrownBase = heightInFeet / (1.0F + MathV.Exp(crown.HcbB0 + crown.HcbB1 * heightInFeet + crown.HcbB2 * CCFL + crown.HcbB3 * MathV.Ln(basalAreaPerAcre) + crown.HcbB4 * (dbhInInches / heightInFeet) + crown.HcbB5 * siteIndexFromDbh + crown.HcbB6 * oldGrowthIndex * oldGrowthIndex));
            Debug.Assert(heightToCrownBase >= 0.0F);
            Debug.Assert(heightToCrownBase <= heightInFeet);
            return heightToCrownBase;
        }

        protected override float GetSiteIndex(OrganonStand stand, FiaCode species)
        {
            if (species == FiaCode.AlnusRubra)
            {
                return stand.SiteIndexInFeet;
            }
            if (species == FiaCode.TsugaHeterophylla)
            {
                return 0.480F + (1.110F * stand.HemlockSiteIndexInFeet); // TODO: remove naming artifact
            }
            return stand.HemlockSiteIndexInFeet;
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
                float crownRatioAdjustment = OrganonVariant.GetCrownRatioAdjustment(crownRatio);
                trees.DbhGrowth[treeIndex] = speciesMultiplier * MathV.Exp(LNDG) * crownRatioAdjustment;
                Debug.Assert(trees.DbhGrowth[treeIndex] > 0.0F);
                Debug.Assert(trees.DbhGrowth[treeIndex] < Constant.Maximum.DiameterIncrementInInches);
            }
        }

        public override int GrowHeightBigSix(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
        {
            if (stand.IsEvenAge != true)
            {
                // placement here is hacky but can't really be improved on until OrganonGrowth.Grow() is moved to OrganonVariant.Grow()
                // This check can be bypassed if a RAP model has only minor species, which appears functionally unimportant.
                throw new ArgumentOutOfRangeException(nameof(stand), "RAP variant supports only even age stands.");
            }

            // WEIGHTED CENTRAL PAI PROCEDURE PARAMETERS FOR RED ALDER
            OrganonHeightCoefficients height = this.GetOrCreateHeightCoefficients(trees.Species);
            // can't merge this with base class implementation due to red alder specific branch in GetGrowthEffectiveAge()
            int oldTreeRecordCount = 0;
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    trees.HeightGrowth[treeIndex] = 0.0F;
                    continue;
                }

                float growthEffectiveAge = this.GetGrowthEffectiveAge(configuration, stand, trees, treeIndex, out float potentialHeightGrowth);
                float crownCompetitionFactor = OrganonVariant.GetCrownCompetitionFactorByHeight(trees.Height[treeIndex], crownCompetitionByHeight);

                float crownRatio = trees.CrownRatio[treeIndex];
                float FCR = -height.P5 * MathV.Pow(1.0F - crownRatio, height.P6) * MathV.Exp(height.P7 * MathF.Sqrt(crownCompetitionFactor));
                float B0 = height.P1 * MathV.Exp(height.P2 * crownCompetitionFactor);
                float B1 = MathV.Exp(height.P3 * MathV.Pow(crownCompetitionFactor, height.P4));
                float MODIFER = height.P8 * (B0 + (B1 - B0) * MathV.Exp(FCR));
                float CRADJ = OrganonVariant.GetCrownRatioAdjustment(crownRatio);
                float heightGrowth = potentialHeightGrowth * MODIFER * CRADJ;
                Debug.Assert(heightGrowth > 0.0F);
                Debug.Assert(heightGrowth < Constant.Maximum.HeightIncrementInFeet);
                trees.HeightGrowth[treeIndex] = heightGrowth;

                if (growthEffectiveAge > this.OldTreeAgeThreshold)
                {
                    ++oldTreeRecordCount;
                }
            }
            return oldTreeRecordCount;
        }

        public override void ReduceExpansionFactors(OrganonStand stand, OrganonStandDensity densityBeforeGrowth, Trees trees, float fertilizationExponent)
        {
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float POW = 0.2F;
            switch (trees.Species)
            {
                // Hann, Bluhm, and Hibbs (?) new red alder equation
                case FiaCode.AlnusRubra:
                    B0 = -4.333150734F;
                    B1 = -0.9856713799F;
                    B2 = 0.0F;
                    B3 = -2.583317081F;
                    B4 = 0.0369852164F;
                    B5 = 0.0394546978F;
                    POW = 1.0F;
                    break;
                // Hann, Marshall, and Hanus (2006) FRL Research Contribution 49
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -3.12161659F;
                    B1 = -0.44724396F;
                    B2 = 0.0F;
                    B3 = -2.48387172F;
                    B4 = 0.01843137F;
                    B5 = 0.01353918F;
                    break;
                // Hann et al. 2003. FRL Research Contribution 40.
                case FiaCode.TsugaHeterophylla:
                case FiaCode.ThujaPlicata:
                    B0 = -0.761609F;
                    B1 = -0.529366F;
                    B2 = 0.0F;
                    B3 = -4.74019F;
                    B4 = 0.0119587F;
                    B5 = 0.00756365F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.AcerMacrophyllum:
                    B0 = -2.976822456F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -6.223250962F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
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
                survivalProbability *= OrganonVariant.GetCrownRatioAdjustment(crownRatio);
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
