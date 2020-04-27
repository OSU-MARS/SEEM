using Osu.Cof.Ferm.Species;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonVariantSmc : OrganonVariantNwo
    {
        public OrganonVariantSmc()
            : base(TreeModel.OrganonSmc, 120.0F)
        {
        }

        protected override float GetCrownWidth(FiaCode species, float HLCW, float largestCrownWidth, float HT, float DBH, float XL)
        {
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
            else if (species == FiaCode.AbiesGrandis)
            {
                if (RATIO > 31.0)
                {
                    RATIO = 31.0F;
                }
            }

            float crownWidthMultiplier = MathV.Pow(RP, B1 + B2 * MathF.Sqrt(RP) + B3 * RATIO);
            Debug.Assert(crownWidthMultiplier >= 0.0F);
            Debug.Assert(crownWidthMultiplier <= 1.0F);

            float crownWidth = largestCrownWidth * crownWidthMultiplier;
            return crownWidth;
        }

        public override void GetHeightPredictionCoefficients(FiaCode species, out float B0, out float B1, out float B2)
        {
            switch (species)
            {
                // Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 7.262195456F;
                    B1 = -5.899759104F;
                    B2 = -0.287207389F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.AbiesGrandis:
                    B0 = 7.42808F;
                    B1 = -5.80832F;
                    B2 = -0.240317F;
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
                case FiaCode.TaxusBrevifolia:
                    B0 = 9.30172F;
                    B1 = -7.50951F;
                    B2 = -0.100000F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.ArbutusMenziesii:
                    B0 = 5.84487F;
                    B1 = -3.84795F;
                    B2 = -0.289213F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.AcerMacrophyllum:
                    B0 = 5.21462F;
                    B1 = -2.70252F;
                    B2 = -0.354756F;
                    break;
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = 4.69753118F;
                    B1 = -3.51586969F;
                    B2 = -0.57665068F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    B0 = 5.59759126F;
                    B1 = -3.19942952F;
                    B2 = -0.38783403F;
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

        public override float GetHeightToCrownBase(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG)
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
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            float HCB;
            if (species == FiaCode.TsugaHeterophylla)
            {
                HCB = HT / (1.0F + MathV.Exp(B0 + B1 * HT + B2 * CCFL + B3 * MathV.Ln(BA) + B4 * (DBH / HT) + B5 * SI_2 + B6 * OG * OG));
            }
            else
            {
                HCB = HT / (1.0F + MathV.Exp(B0 + B1 * HT + B2 * CCFL + B3 * MathV.Ln(BA) + B4 * (DBH / HT) + B5 * SI_1 + B6 * OG * OG));
            }
            Debug.Assert(HCB >= 0.0F);
            Debug.Assert(HCB <= HT);
            return HCB;
        }

        protected override float GetHeightToLargestCrownWidth(FiaCode species, float HT, float CR)
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
                    throw Trees.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            float HLCW = HT - (1.0F - B1) * CL;
            return HLCW;
        }

        protected override float GetLargestCrownWidth(FiaCode species, float MCW, float CR, float DBH, float HT)
        {
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
                    throw Trees.CreateUnhandledSpeciesException(species);
            }
            float CL = CR * HT;
            float LCW = MCW * MathV.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT));
            return LCW;
        }

        public override float GetMaximumCrownWidth(FiaCode species, float D, float H)
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
                MCW = B0 + B1 * DBH + B2 * DBH * DBH;
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
                // Ritchie and Hann(1990) FRL Research Paper 54
                case FiaCode.AbiesGrandis:
                    P1 = 1.0F;
                    P2 = -0.0328142F;
                    P3 = -0.0127851F;
                    P4 = 1.0F;
                    P5 = 6.19784F;
                    P6 = 2.0F;
                    P7 = 0.0F;
                    P8 = 1.01F;
                    break;
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
                float crownCompetitionIncrement = this.GetCrownCompetitionFactorByHeight(trees.Height[treeIndex], crownCompetitionByHeight);

                float crownRatio = trees.CrownRatio[treeIndex];
                float FCR = -P5 * MathV.Pow(1.0F - crownRatio, P6) * MathV.Exp(P7 * MathF.Sqrt(crownCompetitionIncrement));
                float B0 = P1 * MathV.Exp(P2 * crownCompetitionIncrement);
                float B1 = MathV.Exp(P3 * MathV.Pow(crownCompetitionIncrement, P4));
                float MODIFER = P8 * (B0 + (B1 - B0) * MathF.Exp(FCR)); // changed from MathV due to FCR of -91 being observed
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
            // SMC MORTALITY
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float siteIndex = stand.SiteIndex;
            switch (trees.Species)
            {
                // Hann, Marshall, and Hanus(2006) FRL Research Contribution 49
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -3.12161659F;
                    B1 = -0.44724396F;
                    B2 = 0.0F;
                    B3 = -2.48387172F;
                    B4 = 0.01843137F;
                    B5 = 0.01353918F;
                    break;
                // Unpublished Equation on File at OSU Dept.Forest Resources
                case FiaCode.AbiesGrandis:
                    B0 = -7.60159F;
                    B1 = -0.200523F;
                    B2 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.0441333F;
                    B5 = 0.00063849F;
                    break;
                // Hann, Marshall, Hanus(2003) FRL Research Contribution 40
                case FiaCode.TsugaHeterophylla:
                case FiaCode.ThujaPlicata:
                    B0 = -0.761609F;
                    B1 = -0.529366F;
                    B2 = 0.0F;
                    B3 = -4.74019F;
                    B4 = 0.0119587F;
                    B5 = 0.00756365F;
                    siteIndex = stand.HemlockSiteIndex;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.TaxusBrevifolia:
                    B0 = -4.072781265F;
                    B1 = -0.176433475F;
                    B2 = 0.0F;
                    B3 = -1.729453975F;
                    B4 = 0.0F;
                    B5 = 0.012525642F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                    B0 = -6.089598985F;
                    B1 = -0.245615070F;
                    B2 = 0.0F;
                    B3 = -3.208265570F;
                    B4 = 0.033348079F;
                    B5 = 0.013571319F;
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
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
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

            float[] mortalityKforRedAlder = null;
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
                    mortalityKforRedAlder[treeIndex] = PMK;
                }

                float XPM = 1.0F / (1.0F + MathV.Exp(-PMK));
                float survivalProbability = 1.0F - XPM;
                survivalProbability *= OrganonGrowth.GetCrownRatioAdjustment(crownRatio);
                Debug.Assert(survivalProbability >= 0.0F);
                Debug.Assert(survivalProbability <= 1.0F);

                float newLiveExpansionFactor = survivalProbability * trees.LiveExpansionFactor[treeIndex];
                if (newLiveExpansionFactor < 0.00001F)
                {
                    newLiveExpansionFactor = 0.0F;
                }
                float mortalityExpansionFactor = trees.LiveExpansionFactor[treeIndex] - newLiveExpansionFactor;

                trees.DeadExpansionFactor[treeIndex] = mortalityExpansionFactor;
                trees.LiveExpansionFactor[treeIndex] = newLiveExpansionFactor;
            }

            if ((trees.Species == FiaCode.AlnusRubra) && (this.TreeModel != TreeModel.OrganonRap))
            {
                if (stand.RedAlderGrowthEffectiveAge >= 55.0)
                {
                    Debug.Assert(trees.Units == Units.English);
                    float alnusRubraTreesPerAcre = 0.0F;
                    for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
                    {
                        alnusRubraTreesPerAcre += trees.LiveExpansionFactor[treeIndex];
                    }
                    if (alnusRubraTreesPerAcre > 0.0001F)
                    {
                        RedAlder.ReduceExpansionFactor(trees, stand.RedAlderGrowthEffectiveAge, alnusRubraTreesPerAcre, mortalityKforRedAlder);
                    }
                }
                stand.RedAlderGrowthEffectiveAge += this.TimeStepInYears;
            }
        }
    }
}
