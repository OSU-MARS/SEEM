using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    public class OrganonVariantSwo : OrganonVariant
    {
        public OrganonVariantSwo()
            : base(Variant.Swo)
        {
        }

        public override float GetCrownWidth(FiaCode species, float HLCW, float LCW, float HT, float DBH, float XL)
        {
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
            float CW = (float)(LCW * Math.Pow(RP, (B1 + B2 * Math.Sqrt(RP) + B3 * (RATIO))));
            return CW;
        }

        public override float GetHeightToCrownBase(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG)
        {
            // HEIGHT TO CROWN BASE FOR UNDAMAGED TREES ONLY
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            switch (species)
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

            float HCB;
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
            return HCB;
        }

        public override float GetHeightToLargestCrownWidth(FiaCode species, float HT, float CR)
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
            float HLCW = HT - (1.0F - B1) * CL;
            return HLCW;
        }

        public override float GetLargestCrownWidth(FiaCode species, float MCW, float CR, float DBH, float HT)
        {
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
            float LCW = (float)(MCW * Math.Pow(CR, B1 + B2 * CL + B3 * (DBH / HT)));
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

        public override float GetMaximumHeightToCrownBase(FiaCode species, float HT, float CCFL)
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
            float MAXHCB = MAXBR * HT;
            return MAXHCB;
        }

        public override void GetMortalityCoefficients(FiaCode species, float DBH, float CR, float SI_1, float SI_2, float BAL, float OG, out float POW, out float PM)
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
            POW = 1.0F;
            switch (species)
            {
                // DF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
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
                // GW Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
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
                // PP Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.PinusPonderosa:
                    B0 = -1.050000682F;
                    B1 = -0.194363402F;
                    B2 = 0.003803100F;
                    B3 = -3.557300286F;
                    B4 = 0.003971638F;
                    B5 = 0.005573601F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // SP Coefficients from Unpublished Equation on File at OSU Dept.Forest Resources
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
                // IC Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
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
                // WH Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
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
                // RC Coefficients from WH of Hann, Marshall, Hanus(2003) FRL Research Contribution 40
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
                // PY Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
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
                // Hann and Hanus(2001) FRL Research Contribution 34
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
                // Hann and Hanus(2001) FRL Research Contribution 34
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
                // Hann and Hanus(2001) FRL Research Contribution 34
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
                // Hann and Hanus(2001) FRL Research Contribution 34
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
                // Hann and Hanus(2001) FRL Research Contribution 34
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
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = -6.00031085F;
                    B1 = -0.10490823F;
                    B2 = 0.0F;
                    B3 = -0.99541909F;
                    B4 = 0.00912739F;
                    B5 = 0.87115652F;
                    B6 = 0.0F;
                    B7 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
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
                // Best Guess
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
                // Hann and Hanus(2001) FRL Research Contribution 34
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
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            if (species == FiaCode.QuercusGarryana)
            {
                // Oregon White Oak
                PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_1 + 4.5F) + B5 * (float)Math.Log(BAL + 5.0);
            }
            else
            {
                PM = B0 + B1 * DBH + B2 * DBH * DBH + B3 * CR + B4 * (SI_1 + 4.5F) + B5 * BAL + B6 * BAL * (float)Math.Exp(B7 * OG);
            }
        }

        public override float GetPredictedHeight(FiaCode species, float dbhInInches)
        {
            // NEW HEIGHT/DIAMETER PARAMETERS FOR UNDAMAGED TREES.EXCEPT RC, WO, AND RA(3 parameters - all species)
            float B0;
            float B1;
            float B2;
            switch (species)
            {
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 7.133682298F;
                    B1 = -5.433744897F;
                    B2 = -0.266398088F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B0 = 6.75286569F;
                    B1 = -5.52614439F;
                    B2 = -0.33012156F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.PinusPonderosa:
                    B0 = 6.27233557F;
                    B1 = -5.57306985F;
                    B2 = -0.40384171F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.PinusLambertiana:
                    B0 = 5.81876360F;
                    B1 = -5.31082668F;
                    B2 = -0.47349388F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.CalocedrusDecurrens:
                    B0 = 10.04621768F;
                    B1 = -8.72915115F;
                    B2 = -0.14040106F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.TsugaHeterophylla:
                    B0 = 6.58804F;
                    B1 = -5.25312496F;
                    B2 = -0.31895401F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = 6.14817441F;
                    B1 = -5.40092761F;
                    B2 = -0.38922036F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.TaxusBrevifolia:
                    B0 = 5.10707208F;
                    B1 = -3.28638769F;
                    B2 = -0.24016101F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.ArbutusMenziesii:
                    B0 = 6.53558288F;
                    B1 = -4.69059053F;
                    B2 = -0.24934807F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    B0 = 9.2251518F;
                    B1 = -7.65310387F;
                    B2 = -0.15480725F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.NotholithocarpusDensiflorus:
                    B0 = 8.49655416F;
                    B1 = -6.68904033F;
                    B2 = -0.16105112F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.QuercusChrysolepis:
                    B0 = 9.01612971F;
                    B1 = -7.34813829F;
                    B2 = -0.134025626F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.AcerMacrophyllum:
                    B0 = 5.20018445F;
                    B1 = -2.86671078F;
                    B2 = -0.42255220F;
                    break;
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = 4.69753118F;
                    B1 = -3.51586969F;
                    B2 = -0.57665068F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.QuercusKelloggii:
                    B0 = 5.04832439F;
                    B1 = -3.32715915F;
                    B2 = -0.43456034F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    B0 = 5.59759126F;
                    B1 = -3.19942952F;
                    B2 = -0.38783403F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.CornusNuttallii:
                    B0 = 7.49095931F;
                    B1 = -5.40872209F;
                    B2 = -0.16874962F;
                    break;
                // Hanus, Hann and Marshall(1999) FRL Research Contribution 27
                case FiaCode.Salix:
                    B0 = 3.26840527F;
                    B1 = -0.95270859F;
                    B2 = -0.98015696F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float predictedHeightInFeet = 4.5F + (float)Math.Exp(B0 + B1 * Math.Pow(dbhInInches, B2));
            return predictedHeightInFeet;
        }

        public override float GrowDiameter(FiaCode species, float dbhInInches, float crownRatio, float SITE, float SBA1, StandDensity densityBeforeGrowth)
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
            switch (species)
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
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float LNDG = (float)(B0 + B1 * Math.Log(dbhInInches + K1) + B2 * Math.Pow(dbhInInches, K2) + B3 * Math.Log((crownRatio + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBA1, K3) / Math.Log(dbhInInches + K4)) + B6 * Math.Sqrt(SBA1));

            // FULL ADJUSTMENTS
            float ADJ;
            if (species == FiaCode.PseudotsugaMenziesii)
            {
                ADJ = 0.8938F;
            }
            else if ((species == FiaCode.AbiesConcolor) || (species == FiaCode.AbiesGrandis))
            {
                ADJ = 0.8722F;
            }
            else if (species == FiaCode.PinusLambertiana)
            {
                ADJ = 0.7903F;
            }
            else if (species == FiaCode.ArbutusMenziesii)
            {
                ADJ = 0.7928F;
            }
            else if (species == FiaCode.ChrysolepisChrysophyllaVarChrysophylla)
            {
                ADJ = 0.7259F;
            }
            else if (species == FiaCode.QuercusGarryana)
            {
                ADJ = 1.0F;
            }
            else if (species == FiaCode.QuercusKelloggii)
            {
                ADJ = 0.7667F;
            }
            else
            {
                ADJ = 0.8F;
            }

            // CROWN RATIO ADJUSTMENT
            float CRADJ = TreeGrowth.GetCrownRatioAdjustment(crownRatio);
            float DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG > 0.0F);
            Debug.Assert(DG < 5.0F);
            return DG;
        }

        public override float GrowHeightBigSix(FiaCode species, float potentialHeightGrowth, float CR, float SCCH)
        {
            // Hann DW, Hanus ML. 2002. Enhanced height growth rate equations for SWO trees. FRL Research Contribution 41.
            // https://ir.library.oregonstate.edu/concern/technical_reports/08612q05c
            float a1 = 1.0F;
            float a2;
            float a3;
            float k1 = 1.0F;
            float k2;
            float k3 = 2.0F;
            float a5 = 0.0F;
            float a0 = 1.0F;
            switch (species)
            {
                case FiaCode.AbiesConcolor: // Tables 8 + 9 + other BUGBUG?
                case FiaCode.AbiesGrandis:
                    a2 = -0.14889850F;
                    a3 = -0.00407303F;
                    k2 = 7.69023575F;
                    break;
                case FiaCode.CalocedrusDecurrens: // other + other + other BUGBUG?
                    a2 = -0.01453250F;
                    a3 = -0.00637434F;
                    k2 = 1.27228638F;
                    break;
                case FiaCode.PinusLambertiana: // Table 9 + other BUGBUG?
                    a2 = -0.14889850F;
                    a3 = -0.00678955F;
                    k2 = 0.92071847F;
                    break;
                case FiaCode.PinusPonderosa: // Table 9 6.1
                    a2 = -0.14889850F;
                    a3 = -0.00322752F;
                    k2 = 0.92071847F;
                    break;
                case FiaCode.PseudotsugaMenziesii: // Table 8 5.1
                    a2 = -0.02457621F;
                    a3 = -0.00407303F;
                    k2 = 2.89556338F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            // Hann 2002 Equation 2 (p17) as reduced to Eq 5.1 (p19), 5.2 (p20), 5.3 (p20) by setting a1 = 1, k1 = 1, k3 = 2, a5 = 0
            //                                             5.x forms are identical: 5.1 uses SCCH, 5.2 scaled PCCH, and 5.3 PCCH
            // a1    a2            a3           k1    k2           k3    a5    a0
            //       b1            b2                 b3                       b0
            // Note: Equation 2 has SCCH^k1, not SCCH^0.5
            float FCR = (float)(-k2 * Math.Pow(1.0 - CR, k3) * Math.Exp(a5 * Math.Pow(SCCH, 0.5)));
            float B0 = a1 * (float)Math.Exp(a2 * SCCH);
            float B1 = (float)Math.Exp(a3 * Math.Pow(SCCH, k1));
            float MODIFER = a0 * (B0 + (B1 - B0) * (float)Math.Exp(FCR));
            float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
            float HG = potentialHeightGrowth * MODIFER * CRADJ;
            Debug.Assert(HG >= 0.0F);
            return HG;
        }
    }
}
