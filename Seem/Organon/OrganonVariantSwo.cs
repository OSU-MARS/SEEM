using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Organon
{
    public class OrganonVariantSwo : OrganonVariant
    {
        public OrganonVariantSwo()
            : base(TreeModel.OrganonSwo, 500.0F)
        {
        }

        protected override OrganonCrownCoefficients CreateCrownCoefficients(FiaCode species)
        {
            // height to crown base for undamaged trees only
            return species switch
            {
                FiaCode.PseudotsugaMenziesii => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 1.797136911F,
                    HcbB1 = -0.010188791F,
                    HcbB2 = -0.003346230F,
                    HcbB3 = -0.412217810F,
                    HcbB4 = 3.958656001F,
                    HcbB5 = 0.008526562F,
                    HcbB6 = 0.448909636F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 4.6366F,
                    McwB1 = 1.6078F,
                    McwB2 = -0.009625F,
                    DbhLimitForMaxCrownWidth = 88.52F,

                    MhcbB0 = 0.96F,
                    MhcbB1 = 0.26F,
                    MhcbB2 = -0.987864873F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann(1999) FS 45: 217-225
                    HlcwB1 = 0.062000F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.00371834F,
                    LcwB3 = 0.808121F,
                    // DF Coefficients from Hann(1999) FS 45: 217-225
                    CWb1 = 0.929973F,
                    CWb2 = -0.135212F,
                    CWb3 = -0.0157579F,
                    CWMaxHeightDiameterRatio = 50.0F
                },
                FiaCode.AbiesConcolor or
                FiaCode.AbiesGrandis => new OrganonCrownCoefficients()
                {
                    // GW Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 3.451045887F,
                    HcbB1 = -0.005985239F,
                    HcbB2 = -0.003211194F,
                    HcbB3 = -0.671479750F,
                    HcbB4 = 3.931095518F,
                    HcbB5 = 0.003115567F,
                    HcbB6 = 0.516180892F,
                    // GW Coefficients from Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 6.1880F,
                    McwB1 = 1.0069F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,
                    MhcbB0 = 0.96F,
                    MhcbB1 = 0.31F,
                    MhcbB2 = 2.450718394F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.028454F,
                    // GW Coefficients from Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.00308402F,
                    LcwB3 = 0.0F,
                    // GW Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.999291F,
                    CWb2 = 0.0F,
                    CWb3 = -0.0314603F,
                    CWMaxHeightDiameterRatio = 31.0F
                },
                FiaCode.PinusPonderosa => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 1.656364063F,
                    HcbB1 = -0.002755463F,
                    HcbB2 = 0.0F,
                    HcbB3 = -0.568302547F,
                    HcbB4 = 6.730693919F,
                    HcbB5 = 0.001852526F,
                    HcbB6 = 0.0F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 3.4835F,
                    McwB1 = 1.343F,
                    McwB2 = -0.0082544F,
                    DbhLimitForMaxCrownWidth = 81.35F,

                    MhcbB0 = 1.01F,
                    MhcbB1 = 0.36F,
                    MhcbB2 = -1.041915784F,
                    MhcbB3 = 0.6F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.05F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.355532F,
                    LcwB2 = 0.0F,
                    LcwB3 = 0.0F,
                    // PP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.755583F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.PinusLambertiana => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 3.785155749F,
                    HcbB1 = -0.009012547F,
                    HcbB2 = -0.003318574F,
                    HcbB3 = -0.670270058F,
                    HcbB4 = 2.758645081F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.841525071F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 4.6600546F,
                    McwB1 = 1.0701859F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 1.02F,
                    MhcbB1 = 0.27F,
                    MhcbB2 = -0.922718593F,
                    MhcbB3 = 0.4F,
                    HeightToCrownBaseRatioLimit = 0.96F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.05F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.00339675F,
                    LcwB3 = 0.532418F,
                    // PP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.755583F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.CalocedrusDecurrens => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 2.428285297F,
                    HcbB1 = -0.006882851F,
                    HcbB2 = -0.002612590F,
                    HcbB3 = -0.572782216F,
                    HcbB4 = 2.113378338F,
                    HcbB5 = 0.008480754F,
                    HcbB6 = 0.506226895F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 3.2837F,
                    McwB1 = 1.2031F,
                    McwB2 = -0.0071858F,
                    DbhLimitForMaxCrownWidth = 83.71F,

                    MhcbB0 = 0.97F,
                    MhcbB1 = 0.22F,
                    MhcbB2 = -0.002612590F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.95F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.20F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = -0.251389F,
                    LcwB2 = 0.00692512F,
                    LcwB3 = 0.985922F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.629785F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.TsugaHeterophylla => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.0F,
                    HcbB1 = 0.0F,
                    HcbB2 = 0.0F,
                    HcbB3 = 0.0F,
                    HcbB4 = 4.801329946F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.0F,
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
                    // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
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
                    HcbB6 = 0.0F,
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
                    // IC
                    HlcwB1 = 0.20F,
                    // IC
                    LcwB1 = -0.251389F,
                    LcwB2 = 0.00692512F,
                    LcwB3 = 0.985922F,
                    // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
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
                    HcbB6 = 0.0F,
                    // WH
                    McwB0 = 4.5652F,
                    McwB1 = 1.4147F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 0.85F,
                    MhcbB1 = 0.35F,
                    MhcbB2 = -0.922868139F,
                    MhcbB3 = 0.8F,
                    HeightToCrownBaseRatioLimit = 0.80F,
                    // WH
                    HlcwB1 = 0.209806F,
                    // WH
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 0.0F,
                    // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
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
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.118621F,
                    LcwB2 = 0.00384872F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.ChrysolepisChrysophyllaVarChrysophylla => new OrganonCrownCoefficients()
                {
                    // GC Coefficients from Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.544237656F,
                    HcbB1 = -0.020571754F,
                    HcbB2 = -0.004317523F,
                    HcbB3 = 0.0F,
                    HcbB4 = 3.132713612F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.483748898F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 2.9793895F,
                    McwB1 = 1.5512443F,
                    McwB2 = -0.01416129F,
                    DbhLimitForMaxCrownWidth = 54.77F,

                    MhcbB0 = 1.0F,
                    MhcbB1 = 0.45F,
                    MhcbB2 = -1.219919284F,
                    MhcbB3 = 1.2F,
                    HeightToCrownBaseRatioLimit = 0.98F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.161440F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.NotholithocarpusDensiflorus => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.833006499F,
                    HcbB1 = -0.012984204F,
                    HcbB2 = -0.002704717F,
                    HcbB3 = 0.0F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.2491242765F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 4.4443F,
                    McwB1 = 1.7040F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 0.98F,
                    MhcbB1 = 0.33F,
                    MhcbB2 = -0.911341687F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.97F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0111972F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.QuercusChrysolepis => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.5376600543F,
                    HcbB1 = -0.018632397F,
                    HcbB2 = 0.0F,
                    HcbB3 = 0.0F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.0F,
                    // TA
                    McwB0 = 4.4443F,
                    McwB1 = 1.7040F,
                    McwB2 = 0.0F,
                    DbhLimitForMaxCrownWidth = 999.99F,

                    MhcbB0 = 1.0F,
                    MhcbB1 = 0.45F,
                    MhcbB2 = -0.922025464F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.98F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0207676F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.AcerMacrophyllum => new OrganonCrownCoefficients()
                {
                    // Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 0.9411395642F,
                    HcbB1 = -0.00768402F,
                    HcbB2 = -0.005476131F,
                    HcbB3 = 0.0F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.0F,
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
                FiaCode.QuercusGarryana => new OrganonCrownCoefficients()
                {
                    // Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26 - 33
                    HcbB0 = 1.05786632F,
                    HcbB1 = 0.0F,
                    HcbB2 = -0.00183283F,
                    HcbB3 = -0.28644547F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.0F,
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
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.364811F,
                    LcwB2 = 0.0F,
                    LcwB3 = 0.0F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    CWb1 = 0.5F,
                    CWb2 = 0.0F,
                    CWb3 = 0.0F
                },
                FiaCode.QuercusKelloggii => new OrganonCrownCoefficients()
                {
                    // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                    HcbB0 = 2.60140655F,
                    HcbB1 = 0.0F,
                    HcbB2 = -0.002273616F,
                    HcbB3 = -0.554980629F,
                    HcbB4 = 0.0F,
                    HcbB5 = 0.0F,
                    HcbB6 = 0.0F,
                    // Paine and Hann(1982) FRL Research Paper 46
                    McwB0 = 3.3625F,
                    McwB1 = 2.0303F,
                    McwB2 = -0.0073307F,
                    DbhLimitForMaxCrownWidth = 138.93F,

                    MhcbB0 = 1.0F,
                    MhcbB1 = 0.2F,
                    MhcbB2 = -1.053892465F,
                    MhcbB3 = 1.0F,
                    HeightToCrownBaseRatioLimit = 0.98F,
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
                    // Hann(1997) FRL Research Contribution 17
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.27196F,
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
                    // Hann and Hanus(2001) FRL Research Contribution 34
                    HlcwB1 = 0.0F,
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
                    HcbB6 = 0.0F,
                    // GC
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
                    // GC
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.161440F,
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
                    HcbB6 = 0.0F,
                    // GC
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
                    // GC
                    LcwB1 = 0.0F,
                    LcwB2 = 0.0F,
                    LcwB3 = 1.161440F,
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
            // NEW HEIGHT/DIAMETER PARAMETERS FOR UNDAMAGED TREES.EXCEPT RC, WO, AND RA
            // Big six species from
            //   Hann DW, Hanus ML. 2002. Enhanced height growth rate equations for SWO trees. FRL Research Contribution 41.
            //   https://ir.library.oregonstate.edu/concern/technical_reports/08612q05c
            //   P1 = a1, P2 = a2, P3 = a3, 
            //   P4 = k1, P5 = k2, P6 = k3,
            //   P7 = a5, P8 = a0
            // Always same values for P1, P4, P6, P7, and P8.
            return species switch
            {
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.PseudotsugaMenziesii => new OrganonHeightCoefficients()
                {
                    B0 = 7.133682298F,
                    B1 = -5.433744897F,
                    B2 = -0.266398088F,
                    // Table 8 5.1
                    P1 = 1.0F,
                    P2 = -0.02457621F,
                    P3 = -0.00407303F,
                    P4 = 1.0F,
                    P5 = 2.89556338F,
                    P6 = 2.0F,
                    P7 = 0.0F,
                    P8 = 1.0F
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.AbiesConcolor or
                FiaCode.AbiesGrandis => new OrganonHeightCoefficients()
                {
                    B0 = 6.75286569F,
                    B1 = -5.52614439F,
                    B2 = -0.33012156F,
                    // Tables 8 + 9 + other BUGBUG?
                    P1 = 1.0F,
                    P2 = -0.14889850F,
                    P3 = -0.00407303F,
                    P4 = 1.0F,
                    P5 = 7.69023575F,
                    P6 = 2.0F,
                    P7 = 0.0F,
                    P8 = 1.0F
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.PinusPonderosa => new OrganonHeightCoefficients()
                {
                    B0 = 6.27233557F,
                    B1 = -5.57306985F,
                    B2 = -0.40384171F,
                    // Table 9 6.1
                    P1 = 1.0F,
                    P2 = -0.14889850F,
                    P3 = -0.00322752F,
                    P4 = 1.0F,
                    P5 = 0.92071847F,
                    P6 = 2.0F,
                    P7 = 0.0F,
                    P8 = 1.0F
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.PinusLambertiana => new OrganonHeightCoefficients()
                {
                    B0 = 5.81876360F,
                    B1 = -5.31082668F,
                    B2 = -0.47349388F,
                    // Table 9 + other BUGBUG?
                    P1 = 1.0F,
                    P2 = -0.14889850F,
                    P3 = -0.00678955F,
                    P4 = 1.0F,
                    P5 = 0.92071847F,
                    P6 = 2.0F,
                    P7 = 0.0F,
                    P8 = 1.0F
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.CalocedrusDecurrens => new OrganonHeightCoefficients()
                {
                    B0 = 10.04621768F,
                    B1 = -8.72915115F,
                    B2 = -0.14040106F,
                    // other + other + other BUGBUG?
                    P1 = 1.0F,
                    P2 = -0.01453250F,
                    P3 = -0.00637434F,
                    P4 = 1.0F,
                    P5 = 1.27228638F,
                    P6 = 2.0F,
                    P7 = 0.0F,
                    P8 = 1.0F
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.TsugaHeterophylla => new OrganonHeightCoefficients()
                {
                    B0 = 6.58804F,
                    B1 = -5.25312496F,
                    B2 = -0.31895401F
                    // not a big six species
                },
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
                FiaCode.ThujaPlicata => new OrganonHeightCoefficients()
                {
                    B0 = 6.14817441F,
                    B1 = -5.40092761F,
                    B2 = -0.38922036F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.TaxusBrevifolia => new OrganonHeightCoefficients()
                {
                    B0 = 5.10707208F,
                    B1 = -3.28638769F,
                    B2 = -0.24016101F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.ArbutusMenziesii => new OrganonHeightCoefficients()
                {
                    B0 = 6.53558288F,
                    B1 = -4.69059053F,
                    B2 = -0.24934807F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.ChrysolepisChrysophyllaVarChrysophylla => new OrganonHeightCoefficients()
                {
                    B0 = 9.2251518F,
                    B1 = -7.65310387F,
                    B2 = -0.15480725F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.NotholithocarpusDensiflorus => new OrganonHeightCoefficients()
                {
                    B0 = 8.49655416F,
                    B1 = -6.68904033F,
                    B2 = -0.16105112F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.QuercusChrysolepis => new OrganonHeightCoefficients()
                {
                    B0 = 9.01612971F,
                    B1 = -7.34813829F,
                    B2 = -0.134025626F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.AcerMacrophyllum => new OrganonHeightCoefficients()
                {
                    B0 = 5.20018445F,
                    B1 = -2.86671078F,
                    B2 = -0.42255220F
                    // not a big six species
                },
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
                FiaCode.QuercusGarryana => new OrganonHeightCoefficients()
                {
                    B0 = 4.69753118F,
                    B1 = -3.51586969F,
                    B2 = -0.57665068F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.QuercusKelloggii => new OrganonHeightCoefficients()
                {
                    B0 = 5.04832439F,
                    B1 = -3.32715915F,
                    B2 = -0.43456034F
                    // not a big six species
                },
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
                FiaCode.AlnusRubra => new OrganonHeightCoefficients()
                {
                    B0 = 5.59759126F,
                    B1 = -3.19942952F,
                    B2 = -0.38783403F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.CornusNuttallii => new OrganonHeightCoefficients()
                {
                    B0 = 7.49095931F,
                    B1 = -5.40872209F,
                    B2 = -0.16874962F
                    // not a big six species
                },
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                FiaCode.Salix => new OrganonHeightCoefficients()
                {
                    B0 = 3.26840527F,
                    B1 = -0.95270859F,
                    B2 = -0.98015696F
                    // not a big six species
                },
                _ => throw Trees.CreateUnhandledSpeciesException(species)
            };
        }

        public override float GetGrowthEffectiveAge(OrganonConfiguration configuration, OrganonStand stand, Trees trees, int treeIndex, out float potentialHeightGrowth)
        {
            // GROWTH EFFECTIVE AGE FROM HANN AND SCRIVANI'S (1987) DOMINANT HEIGHT GROWTH EQUATION
            float siteIndexFromDbh;
            bool treatAsDouglasFir = false;
            if (trees.Species == FiaCode.TsugaHeterophylla)
            {
                siteIndexFromDbh = stand.HemlockSiteIndexInFeet - 4.5F;
            }
            else
            {
                siteIndexFromDbh = stand.SiteIndexInFeet - 4.5F;
                if (trees.Species == FiaCode.PinusLambertiana)
                {
                    siteIndexFromDbh = 0.66F * stand.SiteIndexInFeet - 4.5F;
                }
                treatAsDouglasFir = true;
            }
            DouglasFir.GetDouglasFirPonderosaHeightGrowth(treatAsDouglasFir, siteIndexFromDbh, trees.Height[treeIndex], out float growthEffectiveAge, out potentialHeightGrowth);
            return growthEffectiveAge;
        }

        protected override float GetSiteIndex(OrganonStand stand, FiaCode species)
        {
            if (species == FiaCode.PinusLambertiana)
            {
                return stand.HemlockSiteIndexInFeet; // TODO: remove naming artifact
            }
            return stand.SiteIndexInFeet;
        }

        public override void GrowDiameter(Trees trees, float growthMultiplier, float siteIndexFromDbh, OrganonStandDensity densityBeforeGrowth)
        {
            float B0;
            float B1; // DBH
            float B2; // DBH
            float B3; // CR
            float B4; // SI
            float B5; // SBAL1
            float B6; // SBA1
            float K1; // DBH
            float K2; // DBH
            float K3 = 1.0F; // SBAL1
            float K4 = 2.7F;
            float speciesMultiplier = 0.8F;
            switch (trees.Species)
            {
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -5.35558894F;
                    B1 = 0.840528547F;
                    B2 = -0.0427481848F;
                    B3 = 1.15950313F;
                    B4 = 0.954711126F;
                    B5 = -0.00894779670F;
                    B6 = 0.0F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    speciesMultiplier = 0.8938F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B0 = -5.84904111F;
                    B1 = 1.668196109F;
                    B2 = -0.0853271265F;
                    B3 = 1.21222176F;
                    B4 = 0.679346647F;
                    B5 = -0.00809965733F;
                    B6 = 0.0F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    speciesMultiplier = 0.8722F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.PinusPonderosa:
                    B0 = -4.51958940F;
                    B1 = 0.813998712F;
                    B2 = -0.0493858858F;
                    B3 = 1.10249641F;
                    B4 = 0.879440023F;
                    B5 = -0.0108521667F;
                    B6 = 0.0333706948F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.PinusLambertiana:
                    B0 = -4.12342552F;
                    B1 = 0.734988422F;
                    B2 = -0.0425469735F;
                    B3 = 1.05942163F;
                    B4 = 0.808656390F;
                    B5 = -0.0107837565F;
                    B6 = 0.0F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.CalocedrusDecurrens:
                    B0 = -2.08551255F;
                    B1 = 0.596043703F;
                    B2 = -0.0215223077F;
                    B3 = 1.02734556F;
                    B4 = 0.383450822F;
                    B5 = -0.00489046624F;
                    B6 = -0.0609024782F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    speciesMultiplier = 0.7903F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.TsugaHeterophylla:
                    B0 = -5.70052255F;
                    B1 = 0.865087036F;
                    B2 = -0.0432543518F;
                    B3 = 1.10859727F;
                    B4 = 0.977332597F;
                    B5 = 0.0F;
                    B6 = -0.0526263229F;
                    K1 = 5.0F;
                    K2 = 1.0F;
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
                    speciesMultiplier = 0.7928F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    B0 = -7.78451344F;
                    B1 = 1.2F;
                    B2 = -0.07F;
                    B3 = 0.0F;
                    B4 = 1.01436101F;
                    B5 = -0.00834323811F;
                    B6 = 0.0F;
                    K1 = 10.0F;
                    K2 = 1.0F;
                    speciesMultiplier = 0.7259F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.NotholithocarpusDensiflorus:
                    B0 = -3.36821750F;
                    B1 = 1.2F;
                    B2 = -0.07F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = -0.0339813575F;
                    K1 = 10.0F;
                    K2 = 1.0F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.QuercusChrysolepis:
                    B0 = -3.59333060F;
                    B1 = 1.2F;
                    B2 = -0.07F;
                    B3 = 0.51637418F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = -0.02F;
                    K1 = 10.0F;
                    K2 = 1.0F;
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
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.QuercusKelloggii:
                    B0 = -4.43438109F;
                    B1 = 0.930930363F;
                    B2 = -0.0465947242F;
                    B3 = 0.0F;
                    B4 = 0.510717175F;
                    B5 = 0.0F;
                    B6 = -0.0688832423F;
                    K1 = 5.0F;
                    K2 = 1.0F;
                    speciesMultiplier = 0.7667F;
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
                float crownCompetitionIncrement = OrganonVariant.GetCrownCompetitionFactorByHeight(trees.Height[treeIndex], crownCompetitionByHeight);

                // Hann 2002 Equation 2 (p17) as reduced to Eq 5.1 (p19), 5.2 (p20), 5.3 (p20) by setting a1 = 1, k1 = 1, k3 = 2, a5 = 0
                //                                             5.x forms are identical: 5.1 uses SCCH, 5.2 scaled PCCH, and 5.3 PCCH
                // a1    a2            a3           k1    k2           k3    a5    a0
                //       b1            b2                 b3                       b0
                // Note: Equation 2 has SCCH^k1, not SCCH^0.5
                float crownRatio = trees.CrownRatio[treeIndex];
                float FCR = -height.P5 * MathV.Pow(1.0F - crownRatio, height.P6) * MathV.Exp(height.P7 * MathF.Sqrt(crownCompetitionIncrement));
                float B0 = height.P1 * MathV.Exp(height.P2 * crownCompetitionIncrement);
                float B1 = MathV.Exp(height.P3 * MathV.Pow(crownCompetitionIncrement, height.P4));
                float MODIFER = height.P8 * (B0 + (B1 - B0) * MathV.Exp(FCR));
                float CRADJ = OrganonVariant.GetCrownRatioAdjustment(crownRatio);
                float heightGrowth = potentialHeightGrowth * MODIFER * CRADJ;
                Debug.Assert(heightGrowth >= 0.0F);
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
            // NEW SWO MORTALITY WITH REVISED CLO PARAMETERS
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            float B7;
            switch (trees.Species)
            {
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -4.648483270F;
                    B1 = -0.266558690F;
                    B2 = 0.003699110F;
                    B3 = -2.118026640F;
                    B4 = 0.025499430F;
                    B5 = 0.003361340F;
                    B6 = 0.013553950F;
                    B7 = -2.723470950F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B0 = -2.215777201F;
                    B1 = -0.162895666F;
                    B2 = 0.003317290F;
                    B3 = -3.561438261F;
                    B4 = 0.014644689F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.PinusPonderosa:
                    B0 = -1.050000682F;
                    B1 = -0.194363402F;
                    B2 = 0.003803100F;
                    B3 = -3.557300286F;
                    B4 = 0.003971638F;
                    B5 = 0.005573601F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    // BUGBUG: doesn't use ponderosa site index?
                    break;
                // unpublished equation on file at OSU Deptartment of Forest Resources
                case FiaCode.PinusLambertiana:
                    B0 = -1.531051304F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.CalocedrusDecurrens:
                    B0 = -1.922689902F;
                    B1 = -0.136081990F;
                    B2 = 0.002479863F;
                    B3 = -3.178123293F;
                    B4 = 0.0F;
                    B5 = 0.004684133F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.TsugaHeterophylla:
                    B0 = -1.166211991F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -4.602668157F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // adapted from western hemlock in Hann et al. 2003. FRL Research Contribution 40.
                case FiaCode.ThujaPlicata:
                    B0 = -0.761609F;
                    B1 = -0.529366F;
                    B2 = 0.0F;
                    B3 = -4.74019F;
                    B4 = 0.0119587F;
                    B5 = 0.00756365F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.TaxusBrevifolia:
                    B0 = -4.072781265F;
                    B1 = -0.176433475F;
                    B2 = 0.0F;
                    B3 = -1.729453975F;
                    B4 = 0.0F;
                    B5 = 0.012525642F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                    B0 = -6.089598985F;
                    B1 = -0.245615070F;
                    B2 = 0.0F;
                    B3 = -3.208265570F;
                    B4 = 0.033348079F;
                    B5 = 0.013571319F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    B0 = -4.317549852F;
                    B1 = -0.057696253F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.004861355F;
                    B5 = 0.00998129F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.NotholithocarpusDensiflorus:
                    B0 = -2.410756914F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -1.049353753F;
                    B4 = 0.008845583F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.QuercusChrysolepis:
                    B0 = -2.990451960F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.002884840F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.AcerMacrophyllum:
                    B0 = -2.976822456F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -6.223250962F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = -6.00031085F;
                    B1 = -0.10490823F;
                    B2 = 0.0F;
                    B3 = -0.99541909F;
                    B4 = -0.00912739F;
                    B5 = 0.87115652F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.QuercusKelloggii:
                    B0 = -3.108619921F;
                    B1 = -0.570366764F;
                    B2 = 0.018205398F;
                    B3 = -4.584655216F;
                    B4 = 0.014926170F;
                    B5 = 0.012419026F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // best guess
                case FiaCode.AlnusRubra:
                    B0 = -2.0F;
                    B1 = -0.5F;
                    B2 = 0.015F;
                    B3 = -3.0F;
                    B4 = 0.015F;
                    B5 = 0.01F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus (2001) FRL Research Contribution 34
                case FiaCode.CornusNuttallii:
                    B0 = -3.020345211F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = -8.467882343F;
                    B4 = 0.013966388F;
                    B5 = 0.009461545F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // best guess
                case FiaCode.Salix:
                    B0 = -1.386294361F;
                    B1 = 0.0F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }
            float OG1 = OrganonMortality.GetOldGrowthIndicator(this, stand);
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
                if (trees.Species == FiaCode.QuercusGarryana)
                {
                    basalAreaLarger = MathV.Ln(basalAreaLarger + 5.0F);
                }
                float crownRatio = trees.CrownRatio[treeIndex];
                float PMK = B0 + B1 * dbhInInches + B2 * dbhInInches * dbhInInches + B3 * crownRatio + B4 * stand.SiteIndexInFeet + B5 * basalAreaLarger + B6 * basalAreaLarger * MathV.Exp(B7 * OG1) + fertilizationExponent;
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

        public override float ToHemlockSiteIndex(float siteIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(siteIndex, Constant.Minimum.SiteIndexInFeet);
            return 0.940792F * siteIndex;
        }

        public override float ToSiteIndex(float hemlockSiteIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(hemlockSiteIndex, Constant.Minimum.SiteIndexInFeet);
            return 1.062934F * hemlockSiteIndex;
        }
    }
}
