using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Organon
{
    public class OrganonVariantSmc : OrganonVariantNwo
    {
        public OrganonVariantSmc()
            : base(TreeModel.OrganonSmc, 120.0F)
        {
        }

        protected override OrganonCrownCoefficients CreateCrownCoefficients(FiaCode species)
        {
            return species switch
            {
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
                FiaCode.AbiesGrandis => new OrganonCrownCoefficients()
                {
                    // Zumrawi and Hann (1989) FRL Research Paper 52
                    HcbB0 = 1.04746F,
                    HcbB1 = -0.0066643F,
                    HcbB2 = -0.0067129F,
                    HcbB3 = 0.0F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 6.1880F,
                    McwB1 = 1.0069F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 0.96F,
                    MhcbB1 = 0.31F,
                    MhcbB2 = -2.450718394F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.028454F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.00308402F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.999291F,
                    CWb2 = 0.0F,
                    CWb3 = -0.0314603F,
                    CWMaxHeightDiameterRatio = 31.0F
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
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 1.01F,
                    MhcbB1 = 0.36F,
                    MhcbB2 = -0.944528054F,
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
                    // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
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
                FiaCode.TaxusBrevifolia => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.0F,
                    HcbB1 = 0.0F,
                    HcbB2 = 0.0F,
                    HcbB3 = 0.0F,
                    HcbB4 = 2.030940382F,
                    HcbB5 = 0.0F,
                    // WH of Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 4.5652F,
                    McwB1 = 1.4147F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 0.85F,
                    MhcbB1 = 0.35F,
                    MhcbB2 = -0.922868139F,
                    MhcbB3 = 0.8F,
                    HeightToCrownBaseRatioLimit = 0.80F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.209806F,
                    // WH of Hann(1997) FRL Research Contribution 17
                    // BUGBUG: all coefficients are zero
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.629785F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.ArbutusMenziesii => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 2.955339267F,
                    HcbB1 = 0.0F,
                    HcbB2 = 0.0F,
                    HcbB3 = -0.798610738F,
                    HcbB4 = 3.095269471F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.700465646F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 3.4298629F,
                    McwB1 = 1.3532302F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 0.981F,
                    MhcbB1 = 0.161F,
                    MhcbB2 = -1.73666044F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.98F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.118621F,
                    LcwB2 = 0.00384872F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.AcerMacrophyllum => new OrganonCrownCoefficients()
                {
                    // BL Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.9411395642F,
                    HcbB1 = -0.00768402F,
                    HcbB2 = -0.005476131F,
                    HcbB3 = 0.0F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
                    McwB0 = 4.0953F,
                    McwB1 = 2.3849F,
                    McwB2 = -0.011630F,
                    DbhLimitForMaxCrownWidth = 102.53F,

                    MhcbB0 = 1.0F,
                    MhcbB1 = 0.45F,
                    MhcbB2 = -1.020016685F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.470180F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.QuercusGarryana => new OrganonCrownCoefficients()
                {
                    // Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26-33
                    HcbB0 = 1.05786632F,
                    HcbB1 = 0.0F,
                    HcbB2 = -0.00183283F,
                    HcbB3 = -0.28644547F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    // Paine and Hann (1982) FRL Research Paper 46
                    McwB0 = 3.0785639F,
                    McwB1 = 1.9242211F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 1.0F,
                    MhcbB1 = 0.3F,
                    MhcbB2 = -0.95634399F,
                    MhcbB3 = 1.1F,
                    HeightToCrownBaseRatioLimit = 0.98F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.3648110F,
                    LcwB2 = 0.0F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.AlnusRubra => new OrganonCrownCoefficients()
                {
                    // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
                    HcbB0 = 0.56713781F,
                    HcbB1 = -0.010377976F,
                    HcbB2 = -0.002066036F,
                    HcbB3 = 0.0F,
                    HcbB4 = 1.39796223F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.0F,
                    // Smith(1966) Proc. 6th World Forestry Conference
                    McwB0 = 8.0F,
                    McwB1 = 1.53F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 0.93F,
                    MhcbB1 = 0.18F,
                    MhcbB2 = -0.928243505F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.92F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.3227140F,
                    LcwB2 = 0.0F,
                    LcwB3 = 0.0F,
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
                    // GC of Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 2.9793895F,
                    McwB1 = 1.5512443F,
                    McwB2 = -0.01416129F,
                    DbhLimitForMaxCrownWidth = 54.77F,

                    MhcbB0 = 1.0F,
                    MhcbB1 = 0.45F,
                    MhcbB2 = -1.020016685F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // GC of Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.61440F,
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
                    // GC of Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 2.9793895F,
                    McwB1 = 1.5512443F,
                    McwB2 = -0.01416129F,
                    DbhLimitForMaxCrownWidth = 54.77F,

                    MhcbB0 = 0.985F,
                    MhcbB1 = 0.285F,
                    MhcbB2 = -0.969750805F,
                    MhcbB3 = 0.9F,
                    HeightToCrownBaseRatioLimit = 0.98F,
                    // GC of Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.61440F,
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
                // Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
                FiaCode.PseudotsugaMenziesii => new OrganonHeightCoefficients()
                {
                    B0 = 7.262195456F,
                    B1 = -5.899759104F,
                    B2 = -0.287207389F,
                    P1 = 0.655258886F,
                    P2 = -0.006322913F,
                    P3 = -0.039409636F,
                    P4 = 0.5F,
                    P5 = 0.597617316F,
                    P6 = 2.0F,
                    P7 = 0.631643636F,
                    P8 = 1.010018427F
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.AbiesGrandis => new OrganonHeightCoefficients()
                {
                    B0 = 7.42808F,
                    B1 = -5.80832F,
                    B2 = -0.240317F,
                    // Ritchie and Hann(1990) FRL Research Paper 54
                    P1 = 1.0F,
                    P2 = -0.0328142F,
                    P3 = -0.0127851F,
                    P4 = 1.0F,
                    P5 = 6.19784F,
                    P6 = 2.0F,
                    P7 = 0.0F,
                    P8 = 1.01F
                },
                // Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
                FiaCode.TsugaHeterophylla => new OrganonHeightCoefficients()
                {
                    B0 = 6.555344622F,
                    B1 = -5.137174162F,
                    B2 = -0.364550800F,
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
                    // not big six species
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.TaxusBrevifolia => new OrganonHeightCoefficients()
                {
                    B0 = 9.30172F,
                    B1 = -7.50951F,
                    B2 = -0.100000F
                    // not big six species
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.ArbutusMenziesii => new OrganonHeightCoefficients()
                {
                    B0 = 5.84487F,
                    B1 = -3.84795F,
                    B2 = -0.289213F
                    // not big six species
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.AcerMacrophyllum => new OrganonHeightCoefficients()
                {
                    B0 = 5.21462F,
                    B1 = -2.70252F,
                    B2 = -0.354756F
                    // not big six species
                },
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
                FiaCode.QuercusGarryana => new OrganonHeightCoefficients()
                {
                    B0 = 4.69753118F,
                    B1 = -3.51586969F,
                    B2 = -0.57665068F
                    // not big six species
                },
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
                FiaCode.AlnusRubra => new OrganonHeightCoefficients()
                {
                    B0 = 5.59759126F,
                    B1 = -3.19942952F,
                    B2 = -0.38783403F
                    // not big six species
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.CornusNuttallii => new OrganonHeightCoefficients()
                {
                    B0 = 4.49727F,
                    B1 = -2.07667F,
                    B2 = -0.388650F
                    // not big six species
                },
                // Wang and Hann(1988) FRL Research Paper 51
                FiaCode.Salix => new OrganonHeightCoefficients()
                {
                    B0 = 4.88361F,
                    B1 = -2.47605F,
                    B2 = -0.309050F
                    // not big six species
                },
                _ => throw Trees.CreateUnhandledSpeciesException(species)
            };
        }

        public override void GrowDiameter(Trees trees, float growthMultiplier, float siteIndexFromDbh, OrganonStandDensity densityBeforeGrowth)
        {
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
                // Hann, Marshall, and Hanus(2006) FRL Research Contribution 49
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -5.34253119F;
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
                // Zumrawi and Hann(1993) FRL Research Contribution 4
                case FiaCode.AbiesGrandis:
                    B0 = -2.34619F;
                    B1 = 0.594640F;
                    B2 = -0.000976092F;
                    B3 = 1.12712F;
                    B4 = 0.555333F;
                    B5 = -0.0000290672F;
                    B6 = -0.0470848F;
                    K1 = 1.0F;
                    K2 = 2.0F;
                    K3 = 2.0F;
                    K4 = 5.0F;
                    speciesMultiplier = 0.8722F;
                    break;
                // Unpublished Equation on File at OSU Dept.Forest Resources
                case FiaCode.TsugaHeterophylla:
                    B0 = -4.87447412F;
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
                    B0 = -11.45456097F;
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
                case FiaCode.TaxusBrevifolia:
                    B0 = -9.15835863F;
                    B1 = 1.0F;
                    B2 = -0.00000035F;
                    B3 = 1.16688474F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = -0.02F;
                    K1 = 4000.0F;
                    K2 = 4.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.ArbutusMenziesii:
                    B0 = -8.84531757F;
                    B1 = 1.5F;
                    B2 = -0.0006F;
                    B3 = 0.51225596F;
                    B4 = 0.418129153F;
                    B5 = -0.00355254593F;
                    B6 = -0.0321315389F;
                    K1 = 110.0F;
                    K2 = 2.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    speciesMultiplier = 0.7928F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.AcerMacrophyllum:
                    B0 = -3.41449922F;
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
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = -7.81267986F;
                    B1 = 1.405616529F;
                    B2 = -0.0603105850F;
                    B3 = 0.64286007F;
                    B4 = 1.037687142F;
                    B5 = 0.0F;
                    B6 = -0.0787012218F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    speciesMultiplier = 1.0F; 
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    B0 = -4.39082007F;
                    B1 = 1.0F;
                    B2 = -0.0945057147F;
                    B3 = 1.06867026F;
                    B4 = 0.685908029F;
                    B5 = -0.00586331028F;
                    B6 = 0.0F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    K3 = 1.0F;
                    K4 = 2.7F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = -8.08352683F;
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

        public override void ReduceExpansionFactors(OrganonStand stand, OrganonStandDensity densityBeforeGrowth, Trees trees, float fertilizationExponent)
        {
            // SMC MORTALITY
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float siteIndex = stand.SiteIndexInFeet;
            switch (trees.Species)
            {
                // Hann, Marshall, and Hanus (2006) FRL Research Contribution 49
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -3.12161659F;
                    B1 = -0.44724396F;
                    B2 = 0.0F;
                    B3 = -2.48387172F;
                    B4 = 0.01843137F;
                    B5 = 0.01353918F;
                    break;
                // unpublished equation on file at OSU Deptartment of Forest Resources
                case FiaCode.AbiesGrandis:
                    B0 = -7.60159F;
                    B1 = -0.200523F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.0441333F;
                    B5 = 0.00063849F;
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
                    siteIndex = stand.HemlockSiteIndexInFeet; // redcedar site index is approximated by hemlock site index here
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.TaxusBrevifolia:
                    B0 = -4.072781265F;
                    B1 = -0.176433475F;
                    B2 = 0.0F;
                    B3 = -1.729453975F;
                    B4 = 0.0F;
                    B5 = 0.012525642F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                    B0 = -6.089598985F;
                    B1 = -0.245615070F;
                    B2 = 0.0F;
                    B3 = -3.208265570F;
                    B4 = 0.033348079F;
                    B5 = 0.013571319F;
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
                // Gould, Marshall, and Harrington (2008) Western Journal of Applied Forestry 23:26-33
                case FiaCode.QuercusGarryana:
                    B0 = -6.00031085F;
                    B1 = -0.10490823F;
                    B2 = 0.0F;
                    B3 = -0.99541909F;
                    B4 = 0.00912739F;
                    B5 = 0.87115652F;
                    break;
                // best guess
                case FiaCode.AlnusRubra:
                    B0 = -2.0F;
                    B1 = -0.5F;
                    B2 = 0.015F;
                    B3 = -3.0F;
                    B4 = 0.015F;
                    B5 = 0.01F;
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
                // buest guess
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

            float[]? mortalityKforRedAlder = null;
            if (trees.Species == FiaCode.AlnusRubra)
            {
                mortalityKforRedAlder = new float[trees.Capacity];
            }

            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    continue;
                }
                float dbhInInches = trees.Dbh[treeIndex];
                float basalAreaLarger = densityBeforeGrowth.GetBasalAreaLarger(dbhInInches);
                if (trees.Species == FiaCode.AbiesGrandis)
                {
                    basalAreaLarger /= dbhInInches;
                }
                else if (trees.Species == FiaCode.QuercusGarryana)
                {
                    basalAreaLarger = MathV.Ln(basalAreaLarger + 5.0F);
                }
                float crownRatio = trees.CrownRatio[treeIndex];
                float PMK = B0 + B1 * dbhInInches + B2 * dbhInInches * dbhInInches + B3 * crownRatio + B4 * siteIndex + B5 * basalAreaLarger + fertilizationExponent;
                if (trees.Species == FiaCode.AlnusRubra)
                {
                    mortalityKforRedAlder![treeIndex] = PMK;
                }

                float XPM = 1.0F / (1.0F + MathV.Exp(-PMK));
                float survivalProbability = 1.0F - XPM;
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

            if ((trees.Species == FiaCode.AlnusRubra) && (this.TreeModel != TreeModel.OrganonRap))
            {
                if (stand.RedAlderGrowthEffectiveAge >= Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears)
                {
                    Debug.Assert(trees.Units == Units.English);
                    float alnusRubraTreesPerAcre = 0.0F;
                    for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
                    {
                        alnusRubraTreesPerAcre += trees.LiveExpansionFactor[treeIndex];
                    }
                    if (alnusRubraTreesPerAcre > 0.0001F)
                    {
                        RedAlder.ReduceExpansionFactor(trees, stand.RedAlderGrowthEffectiveAge, alnusRubraTreesPerAcre, mortalityKforRedAlder!);
                    }
                }
            }
        }
    }
}
