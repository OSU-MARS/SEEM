using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class DiameterGrowth
    {
        public static void DIAMGRO(OrganonVariant variant, int treeIndex, int simulationStep, Stand stand, float SI_1, float SI_2, 
                                   StandDensity densityBeforeGrowth, Dictionary<FiaCode, float[]> CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT)
        {
            // CALCULATES FIVE-YEAR DIAMETER GROWTH RATE OF THE K-TH TREE
            // CALCULATE BASAL AREA IN LARGER TREES
            float dbhInInches = stand.Dbh[treeIndex];
            float SBAL1 = densityBeforeGrowth.GetBasalAreaLarger(dbhInInches);

            FiaCode species = stand.Species[treeIndex];
            float SITE;
            switch(variant.Variant)
            {
                case Variant.Swo:
                    SITE = SI_1;
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        SITE = SI_2;
                    }
                    else
                    {
                        SITE = SI_1;
                    }
                    break;
               case Variant.Rap:
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        SITE = SI_2;
                    }
                    else
                    {
                        SITE = SI_1;
                    }
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
            }

            // CALCULATE DIAMETER GROWTH RATE FOR UNTREATED TREES
            float crownRatio = stand.CrownRatio[treeIndex];
            float dbhGrowthInInches;
            switch(variant.Variant)
            {
                case Variant.Swo:
                    DG_SWO(species, dbhInInches, crownRatio, SITE, SBAL1, densityBeforeGrowth.BasalAreaPerAcre, out dbhGrowthInInches);
                    break;
                case Variant.Nwo:
                    DG_NWO(species, dbhInInches, crownRatio, SITE, SBAL1, densityBeforeGrowth.BasalAreaPerAcre, out dbhGrowthInInches);
                    break;
                case Variant.Smc:
                    DG_SMC(species, dbhInInches, crownRatio, SITE, SBAL1, densityBeforeGrowth.BasalAreaPerAcre, out dbhGrowthInInches);
                    break;
                case Variant.Rap:
                    DG_RAP(species, dbhInInches, crownRatio, SITE, SBAL1, densityBeforeGrowth.BasalAreaPerAcre, out dbhGrowthInInches);
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
            }

            // CALCULATE FERTILIZER ADJUSTMENT
            DG_FERT(species, variant, simulationStep, SI_1, PN, YF, out float FERTADJ);
            // CALCULATE THINNING ADJUSTMENT
            DG_THIN(species, variant, simulationStep, BABT, BART, YT, out float THINADJ);
            // CALCULATE DIAMETER GROWTH RATE FOR UNTREATED OR TREATED TREES
            dbhGrowthInInches *= CALIB[species][2] * FERTADJ * THINADJ;
            stand.DbhGrowth[treeIndex] = dbhGrowthInInches;
        }

        private static void DG_NWO(FiaCode species, float DBH, float CR, float SITE, float SBAL1, float SBA1, out float DG)
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
            switch (species)
            {
                // Zumrawi and Hann(1993) FRL Research Contribution 4
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -4.69624F;
                    B1 = 0.339513F;
                    B2 = -0.000428261F;
                    B3 = 1.19952F;
                    B4 = 1.15612F;
                    B5 = -0.0000446327F;
                    B6 = -0.0237003F;
                    K1 = 1.0F;
                    K2 = 2.0F;
                    K3 = 2.0F;
                    K4 = 5.0F;
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
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = -4.49867F;
                    B1 = 0.362369F;
                    B2 = -0.00153907F;
                    B3 = 1.1557F;
                    B4 = 1.12154F;
                    B5 = -0.0000201041F;
                    B6 = -0.0417388F;
                    K1 = 1.0F;
                    K2 = 2.0F;
                    K3 = 2.0F;
                    K4 = 5.0F;
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
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23:26-33
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
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float LNDG = (float)(B0 + B1 * Math.Log(DBH + K1) + B2 * Math.Pow(DBH, K2) + B3 * Math.Log((CR + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBAL1, K3) / Math.Log(DBH + K4)) + B6 * Math.Sqrt(SBA1));

            // TODO: source of these adjustment factors unknown
            float ADJ;
            if (species == FiaCode.PseudotsugaMenziesii)
            {
                ADJ = 0.7011014F;
            }
            else if (species == FiaCode.AbiesGrandis)
            {
                ADJ = 0.8722F;
            }
            else if (species == FiaCode.TsugaHeterophylla)
            {
                ADJ = 0.7163F;
            }
            else if (species == FiaCode.ArbutusMenziesii)
            {
                ADJ = 0.7928F;
            }
            else if (species == FiaCode.AlnusRubra)
            {
                ADJ = 1.0F;
            }
            else 
            {
                ADJ = 0.8F;
            }

            // CROWN RATIO ADJUSTMENT
            float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
            DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG > 0.0F);
        }

        private static void DG_RAP(FiaCode species, float DBH, float CR, float SITE, float SBAL1, float SBA1, out float DG)
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
            switch (species)
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
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float LNDG = (float)(B0 + B1 * Math.Log(DBH + K1) + B2 * Math.Pow(DBH, K2) + B3 * Math.Log((CR + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBAL1, K3) / Math.Log(DBH + K4)) + B6 * Math.Sqrt(SBA1));

            float ADJ;
            if ((species == FiaCode.AlnusRubra) || (species == FiaCode.PseudotsugaMenziesii) || (species == FiaCode.TsugaHeterophylla))
            {
                ADJ = 1.0F;
            }
            else 
            {
                ADJ = 0.8F;
            }

            float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
            DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG > 0.0F);
        }

        private static void DG_SMC(FiaCode species, float DBH, float CR, float SITE, float SBAL1, float SBA1, out float DG)
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
            switch (species)
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
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float LNDG = (float)(B0 + B1 * Math.Log(DBH + K1) + B2 * Math.Pow(DBH, K2) + B3 * Math.Log((CR + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBAL1, K3) / Math.Log(DBH + K4)) + B6 * Math.Sqrt(SBA1));

            float ADJ;
            if (species == FiaCode.PseudotsugaMenziesii)
            {
                ADJ = 1.0F;
            }
            else if (species == FiaCode.AbiesGrandis)
            {
                ADJ = 0.8722F;
            }
            else if (species == FiaCode.TsugaHeterophylla)
            {
                ADJ = 1.0F;
            }
            else if (species == FiaCode.ArbutusMenziesii)
            {
                ADJ = 0.7928F;
            }
            else if (species == FiaCode.QuercusGarryana)
            {
                ADJ = 1.0F;
            }
            else
            {
                ADJ = 0.8F;
            }

            float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
            DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG > 0.0F);
        }

        private static void DG_SWO(FiaCode species, float DBH, float CR, float SITE, float SBAL1, float SBA1, out float DG)
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
            float LNDG = (float)(B0 + B1 * Math.Log(DBH + K1) + B2 * Math.Pow(DBH, K2) + B3 * Math.Log((CR + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBAL1, K3) / Math.Log(DBH + K4)) + B6 * Math.Sqrt(SBA1));

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
            float CRADJ = TreeGrowth.GetCrownRatioAdjustment(CR);
            DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG > 0.0F);
            Debug.Assert(DG < 5.0F);
        }

        /// <summary>
        /// Find diameter growth multiplier for thinning.
        /// </summary>
        /// <param name="species">FIA species code.</param>
        /// <param name="variant">Organon variant.</param>
        /// <param name="simulationStep">Simulation cycle.</param>
        /// <param name="BABT">Basal area before thinning? (DOUG?)</param>
        /// <param name="BART">Basal area removed by thinning? (DOUG?)</param>
        /// <param name="YT">Thinning year data? (DOUG?)</param>
        /// <param name="THINADJ">Thinning adjustment. (DOUG?)</param>
        /// <remarks>
        /// Has special cases for Douglas-fir, western hemlock, and red alder (only for RAP).
        /// </remarks>
        private static void DG_THIN(FiaCode species, OrganonVariant variant, int simulationStep, float BABT, float[] BART, float[] YT, out float THINADJ)
        {
            // CALCULATE THINNING ADJUSTMENT FOR DIAMETER GROWTH RATE FROM
            // HANN ET AL.(2003) FRL RESEARCH CONTRIBUTION 40
            //
            // SET PARAMETERS FOR ADJUSTMENT
            float PT1;
            float PT2;
            float PT3;
            if (species == FiaCode.TsugaHeterophylla)
            {
                PT1 = 0.723095045F;
                PT2 = 1.0F;
                PT3 = -0.2644085320F;
            }
            else if (species == FiaCode.PseudotsugaMenziesii)
            {
                PT1 = 0.6203827985F;
                PT2 = 1.0F;
                PT3 = -0.2644085320F;
            }
            else if ((variant.Variant == Variant.Rap) && (species == FiaCode.AlnusRubra))
            {
                PT1 = 0.0F;
                PT2 = 1.0F;
                PT3 = 0.0F;
            }
            else 
            {
                PT1 = 0.6203827985F;
                PT2 = 1.0F;
                PT3 = -0.2644085320F;
            }

            float XTIME = Constant.DefaultTimeStepInYears * (float)simulationStep;
            float THINX1 = 0.0F;
            for (int I = 1; I < 5; ++I)
            {
                THINX1 += BART[I] * (float)Math.Exp((PT3 / PT2) * (YT[1] - YT[I]));
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

            THINADJ = 1.0F + (float)(PT1 * Math.Pow(PREM, PT2) * Math.Exp(PT3 * (XTIME - YT[1])));
            Debug.Assert(THINADJ >= 1.0F);
        }

        private static void DG_FERT(FiaCode species, OrganonVariant variant, int simulationStep, float SI_1, float[] PN, float[] YF, out float FERTADJ)
        {
            // CALCULATE FERTILIZER ADJUSTMENT FOR DIAMETER GROWTH RATE
            // FROM HANN ET AL.(2003) FRL RESEARCH CONTRIBUTION 40
            // SET PARAMETERS FOR ADJUSTMENT
            float PF1;
            float PF2;
            float PF3;
            float PF4;
            float PF5;
            if (variant.Variant != Variant.Rap)
            {
                if (species == FiaCode.TsugaHeterophylla)
                {
                    PF1 = 0.0F;
                    PF2 = 1.0F;
                    PF3 = 0.0F;
                    PF4 = 0.0F;
                    PF5 = 1.0F;
                }
                else if (species == FiaCode.PseudotsugaMenziesii)
                {
                    PF1 = 1.368661121F;
                    PF2 = 0.741476964F;
                    PF3 = -0.214741684F;
                    PF4 = -0.851736558F;
                    PF5 = 2.0F;
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
                FERTX1 += (PN[I] / 800.0F) * (float)Math.Exp((PF3 / PF2) * (YF[1] - YF[I]));
            }

            float FERTX2 = (float)Math.Exp(PF3 * (XTIME - YF[1]) + Math.Pow(PF4 * (SI_1 / 100.0), PF5));
            FERTADJ = 1.0F + (float)(PF1 * Math.Pow((PN[1] / 800.0) + FERTX1, PF2) * FERTX2) * FALDWN;
            Debug.Assert(FERTADJ >= 1.0F);
        }
    }
}
