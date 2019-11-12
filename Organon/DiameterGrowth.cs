using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class DiameterGrowth
    {
        public static void DIAMGRO(Variant variant, int treeIndex, int simulationStep, Stand stand, float SI_1, float SI_2, float SBA1, 
                                   TreeCompetition competitionBeforeGrowth, float[,] CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT)
        {
            // CALCULATES FIVE-YEAR DIAMETER GROWTH RATE OF THE K-TH TREE
            // CALCULATE BASAL AREA IN LARGER TREES
            float dbhInInches = stand.Dbh[treeIndex];
            float SBAL1 = competitionBeforeGrowth.GET_BAL(dbhInInches);

            FiaCode species = stand.Species[treeIndex];
            float SITE;
            switch(variant)
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
                    throw new NotSupportedException();
            }

            // CALCULATE DIAMETER GROWTH RATE FOR UNTREATED TREES
            float crownRatio = stand.CrownRatio[treeIndex];
            int speciesGroup = stand.SpeciesGroup[treeIndex];
            float DG;
            switch(variant)
            {
                case Variant.Swo:
                    DG_SWO(speciesGroup, dbhInInches, crownRatio, SITE, SBAL1, SBA1, out DG);
                    break;
                case Variant.Nwo:
                    DG_NWO(speciesGroup, dbhInInches, crownRatio, SITE, SBAL1, SBA1, out DG);
                    break;
                case Variant.Smc:
                    DG_SMC(speciesGroup, dbhInInches, crownRatio, SITE, SBAL1, SBA1, out DG);
                    break;
                case Variant.Rap:
                    DG_RAP(speciesGroup, dbhInInches, crownRatio, SITE, SBAL1, SBA1, out DG);
                    break;
                default:
                    throw new NotSupportedException();
            }

            // CALCULATE FERTILIZER ADJUSTMENT
            DG_FERT(species, variant, simulationStep, SI_1, PN, YF, out float FERTADJ);
            // CALCULATE THINNING ADJUSTMENT
            DG_THIN(species, variant, simulationStep, BABT, BART, YT, out float THINADJ);
            // CALCULATE DIAMETER GROWTH RATE FOR UNTREATED OR TREATED TREES
            float DGRO = DG * CALIB[speciesGroup, 2] * FERTADJ* THINADJ;
            stand.DbhGrowth[treeIndex] = DGRO;
        }

        private static void DG_NWO(int ISPGRP, float DBH, float CR, float SITE, float SBAL1, float SBA1, out float DG)
        {
            // DIAMETER GROWTH(11 parameters - all species)
            //
            // DF Coefficients from Zumrawi and Hann(1993) FRL Research Contribution 4
            // GF Coefficients from Zumrawi and Hann(1993) FRL Research Contribution 4
            // WH Coefficients from Johnson(2002) Willamette Industries Report
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // MD Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // BL Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // RA Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // WI Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            //
            float[,] DGPAR = {
                {
                    -4.69624F, -2.34619F, -4.49867F, // DF,GF,WH
                    -11.45456097F, -9.15835863F, -8.84531757F, // RC,PY,MD
                    -3.41449922F, -7.81267986F, -4.39082007F, // BL,WO,RA
                    -8.08352683F, -8.08352683F,                    // PD,WI
                },
                {
                    0.339513F, 0.594640F, 0.362369F, // DF,GF,WH
                    0.784133664F, 1.0F, 1.5F, // RC,PY,MD
                    1.0F, 1.405616529F, 1.0F, // BL,WO,RA
                    1.0F, 1.0F,                    // PD,WI
                },
                {
                    -0.000428261F, -0.000976092F, -0.00153907F, // DF,GF,WH
                    -0.0261377888F, -0.00000035F, -0.0006F, // RC,PY,MD
                    -0.05F, -0.0603105850F, -0.0945057147F, // BL,WO,RA
                    -0.00000035F, -0.00000035F,                    // PD,WI
                },
                {
                    1.19952F, 1.12712F, 1.1557F, // DF,GF,WH
                    0.70174783F, 1.16688474F, 0.51225596F, // RC,PY,MD
                    0.0F, 0.64286007F, 1.06867026F, // BL,WO,RA
                    0.31176647F, 0.31176647F,                    // PD,WI
                },
                {
                    1.15612F, 0.555333F, 1.12154F, // DF,GF,WH
                    2.057236260F, 0.0F, 0.418129153F, // RC,PY,MD
                    0.324349277F, 1.037687142F, 0.685908029F, // BL,WO,RA
                    0.0F, 0.0F,                    // PD,WI
                },
                {
                    -0.0000446327F, -0.0000290672F, -0.0000201041F, // DF,GF,WH
                    -0.00415440257F, 0.0F, -0.00355254593F, // RC,PY,MD
                    0.0F, 0.0F, -0.00586331028F, // BL,WO,RA
                    0.0F, 0.0F,                    // PD,WI
                },
                {
                    -0.0237003F, -0.0470848F, -0.0417388F, // DF,GF,WH
                    0.0F, -0.02F, -0.0321315389F, // RC,PY,MD
                    -0.0989519477F, -0.0787012218F, 0.0F, // BL,WO,RA
                    -0.0730788052F, -0.0730788052F,                    // PD,WI
                },
                {
                    1.0F, 1.0F, 1.0F, // DF,GF,WH
                    5.0F, 4000.0F, 110.0F, // RC,PY,MD
                    10.0F, 5.0F, 5.0F, // BL,WO,RA
                    4000.0F, 4000.0F,                    // PD,WI
                },
                {
                    2.0F, 2.0F, 2.0F, // DF,GF,WH
                    1.0F, 4.0F, 2.0F, // RC,PY,MD
                    1.0F, 1.0F, 1.0F, // BL,WO,RA
                    4.0F, 4.0F,                    // PD,WI
                },
                {
                    2.0F, 2.0F, 2.0F, // DF,GF,WH
                    1.0F, 1.0F, 1.0F, // RC,PY,MD
                    1.0F, 1.0F, 1.0F, // BL,WO,RA
                    1.0F, 1.0F,                    // PD,WI
                },
                {
                    5.0F, 5.0F, 5.0F, // DF,GF,WH
                    2.7F, 2.7F, 2.7F, // RC,PY,MD
                    2.7F, 2.7F, 2.7F, // BL,WO,RA
                    2.7F, 2.7F                               // PD,WI
                }
            };

            float B0 = DGPAR[0, ISPGRP];
            float B1 = DGPAR[1, ISPGRP];
            float B2 = DGPAR[2, ISPGRP];
            float B3 = DGPAR[3, ISPGRP];
            float B4 = DGPAR[4, ISPGRP];
            float B5 = DGPAR[5, ISPGRP];
            float B6 = DGPAR[6, ISPGRP];
            float K1 = DGPAR[7, ISPGRP];
            float K2 = DGPAR[8, ISPGRP];
            float K3 = DGPAR[9, ISPGRP];
            float K4 = DGPAR[10, ISPGRP];
            float LNDG = (float)(B0 + B1 * Math.Log(DBH + K1) + B2 * Math.Pow(DBH, K2) + B3 * Math.Log((CR + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBAL1, K3) / Math.Log(DBH + K4)) + B6 * Math.Sqrt(SBA1));

            // CROWN RATIO ADJUSTMENT
            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));

            float ADJ;
            if (ISPGRP == 0)
            {
                ADJ = 0.7011014F;
            }
            else if (ISPGRP == 1)
            {
                ADJ = 0.8722F;
            }
            else if (ISPGRP == 2)
            {
                ADJ = 0.7163F;
            }
            else if (ISPGRP == 5)
            {
                ADJ = 0.7928F;
            }
            else if (ISPGRP == 7)
            {
                ADJ = 1.0F;
            }
            else 
            {
                ADJ = 0.8F;
            }

            DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG >= 0.0F);
        }

        private static void DG_RAP(int ISPGRP, float DBH, float CR, float SITE, float SBAL1, float SBA1, out float DG)
        {
            // DIAMETER GROWTH PARAMETERS(11 parameters - all species)
            //
            // RA Coefficients from Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
            //
            // The following species were annualized by adding ln(0.2) to the intercept terms
            //
            // DF Coefficients from Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
            // WH Coefficients from Unpublished Equation on File at OSU Dept.Forest Resources
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // BL Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // PD Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // WI Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            //
            float[,] DGPAR = {
                {
                   - 4.622849554F,     -6.95196910F,     -6.48391203F   ,// RA,DF,WH
                  - 13.06399888F,     -5.02393713F,     -9.69296474F   ,// RC,BL,PD
                  - 9.69296474F,                                        // WI
                },
                {
                    0.5112200516F,      1.098406840F,      0.4150723209F,// RA,DF,WH
                    0.784133664F,      1.0F,       1.0F          ,// RC,BL,PD
                    1.0F,                                         // WI
                },
                {
                 - .1040194568F,     -0.05218621F,     -0.023744997F,// RA,DF,WH
                  - 0.0261377888F,     -0.05F,     -0.00000035F   ,// RC,BL,PD
                  - 0.00000035F,                                        // WI
                },
                {
                    0.9536538143F,      1.01380810F,      0.907837299F  ,// RA,DF,WH
                    0.70174783F,      0.0F,       0.31176647F   ,// RC,BL,PD
                    0.31176647F,                                        // WI
                },
                {
                    1.0659344724F,      0.91202025F,      1.1346766989F,// RA,DF,WH
                    2.057236260F,      0.324349277F,      0.0F         ,// RC,BL,PD
                    0.0F,                                         // WI
                },
                {
                    - .0193047405F,     -0.01756220F,     -0.015333503F,// RA,DF,WH
                    - 0.00415440257F,      0.0F,       0.0F          ,// RC,BL,PD
                    0.0F,                                         // WI
                },
                {
                    - 0.0773539455F,     -0.05168923F,     -0.03309787F   ,// RA,DF,WH
                    0.0F,      -0.0989519477F,     -0.0730788052F,// RC,BL,PD
                    - 0.0730788052F,                                        // WI
                },
                {
                    1.0F,       6.0F,       5.0F          ,// RA,DF,WH
                    5.0F,      10.0F,    4000.0F          ,// RC,BL,PD
                    4000.0F,                                         // WI
                },
                {
                    1.0F,       1.0F,       1.0F          ,// RA,DF,WH
                    1.0F,       1.0F,       4.0F          ,// RC,BL,PD
                    4.0F,                                         // BL
                },
                {
                    1.0F,       1.0F,       1.0F          ,// RA,DF,WH
                    1.0F,       1.0F,       1.0F          ,// RC,BL,PD
                    1.0F,                                         // WI
                },
                {
                    1.0F,       2.7F,       2.7F          ,// RA,DF,WH
                    2.7F,       2.7F,       2.7F          ,// RC,BL,PD
                    2.7F                                   // WI
                }
            };

            float B0 = DGPAR[0, ISPGRP];
            float B1 = DGPAR[1, ISPGRP];
            float B2 = DGPAR[2, ISPGRP];
            float B3 = DGPAR[3, ISPGRP];
            float B4 = DGPAR[4, ISPGRP];
            float B5 = DGPAR[5, ISPGRP];
            float B6 = DGPAR[6, ISPGRP];
            float K1 = DGPAR[7, ISPGRP];
            float K2 = DGPAR[8, ISPGRP];
            float K3 = DGPAR[9, ISPGRP];
            float K4 = DGPAR[10, ISPGRP];
            float LNDG = (float)(B0 + B1 * Math.Log(DBH + K1) + B2 * Math.Pow(DBH, K2) + B3 * Math.Log((CR + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBAL1, K3) / Math.Log(DBH + K4)) + B6 * Math.Sqrt(SBA1));

            // CROWN RATIO ADJUSTMENT
            //
            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
            float ADJ;
            if (ISPGRP <= 2)
            {
                ADJ = 1.0F;
            }
            else 
            {
                ADJ = 0.8F;
            }

            DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG >= 0.0F);
        }

        private static void DG_SMC(int ISPGRP, float DBH, float CR, float SITE, float SBAL1, float SBA1, out float DG)
        {
            // DIAMETER GROWTH PARAMETERS(11 parameters - all species)
            //
            // DF Coefficients from Hann, Marshall, and Hanus(2006) FRL Research Contribution 49
            // GF Coefficients from Zumrawi and Hann(1993) FRL Research Contribution 4
            // WH Coefficients from Unpublished Equation on File at OSU Dept.Forest Resources
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // MD Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // BL Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // RA Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // WI Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            //
            float[,] DGPAR = {
                {
                    -5.34253119F, -2.34619F, -4.87447412F, // DF,GF,WH
                    -11.45456097F, -9.15835863F, -8.84531757F, // RC,PY,MD
                    -3.41449922F, -7.81267986F, -4.39082007F, // BL,WO,RA
                    -8.08352683F, -8.08352683F,                    // PD,WI
                },
                {
                    1.098406840F, 0.594640F, 0.4150723209F, // DF,GF,WH
                    0.784133664F, 1.0F, 1.5F, // RC,PY,MD
                    1.0F, 1.405616529F, 1.0F, // BL,WO,RA
                    1.0F, 1.0F,                    // PD,WI
                },
                {
                    -0.05218621F, -0.000976092F, -0.023744997F, // DF,GF,WH
                    -0.0261377888F, -0.00000035F, -0.0006F, // RC,PY,MD
                    -0.05F, -0.0603105850F, -0.0945057147F, // BL,WO,RA
                    -0.00000035F, -0.00000035F,                    // PD,WI
                },
                {
                    1.01380810F, 1.12712F, 0.907837299F, // DF,GF,WH
                    0.70174783F, 1.16688474F, 0.51225596F, // RC,PY,MD
                    0.0F, 0.64286007F, 1.06867026F, // BL,WO,RA
                    0.31176647F, 0.31176647F,                    // PD,WI
                },
                {
                    0.91202025F, 0.555333F, 1.1346766989F, // DF,GF,WH
                    2.057236260F, 0.0F, 0.418129153F, // RC,PY,MD
                    0.324349277F, 1.037687142F, 0.685908029F, // BL,WO,RA
                    0.0F, 0.0F,                    // PD,WI
                },
                {
                    -0.01756220F, -0.0000290672F, -0.015333503F, // DF,GF,WH
                    -0.00415440257F, 0.0F, -0.00355254593F, // RC,PY,MD
                    0.0F, 0.0F, -0.00586331028F, // BL,WO,RA
                    0.0F, 0.0F,                    // PD,WI
                },
                {
                    -0.05168923F, -0.0470848F, -0.03309787F, // DF,GF,WH
                    0.0F, -0.02F, -0.0321315389F, // RC,PY,MD
                    -0.0989519477F, -0.0787012218F, 0.0F, // BL,WO,RA
                    -0.0730788052F, -0.0730788052F,                    // PD,WI
                },
                {
                    6.0F, 1.0F, 5.0F, // DF,GF,WH
                    5.0F, 4000.0F, 110.0F, // RC,PY,MD
                    10.0F, 5.0F, 5.0F, // BL,WO,RA
                    4000.0F, 4000.0F,  // PD,WI
                },
                {
                    1.0F, 2.0F, 1.0F, // DF,GF,WH
                    1.0F, 4.0F, 2.0F, // RC,PY,MD
                    1.0F, 1.0F, 1.0F, // BL,WO,RA
                    4.0F, 4.0F,       // PD,WI
                },
                {
                    1.0F, 2.0F, 1.0F, // DF,GF,WH
                    1.0F, 1.0F, 1.0F, // RC,PY,MD
                    1.0F, 1.0F, 1.0F, // BL,WO,RA
                    1.0F, 1.0F,       // PD,WI
                },
                {
                    2.7F, 5.0F, 2.7F, // DF,GF,WH
                    2.7F, 2.7F, 2.7F, // RC,PY,MD
                    2.7F, 2.7F, 2.7F, // BL,WO,RA
                    2.7F, 2.7F        // PD,WI
                }
            };

            float B0 = DGPAR[0, ISPGRP];
            float B1 = DGPAR[1, ISPGRP];
            float B2 = DGPAR[2, ISPGRP];
            float B3 = DGPAR[3, ISPGRP];
            float B4 = DGPAR[4, ISPGRP];
            float B5 = DGPAR[5, ISPGRP];
            float B6 = DGPAR[6, ISPGRP];
            float K1 = DGPAR[7, ISPGRP];
            float K2 = DGPAR[8, ISPGRP];
            float K3 = DGPAR[9, ISPGRP];
            float K4 = DGPAR[10, ISPGRP];
            float LNDG = (float)(B0 + B1 * Math.Log(DBH + K1) + B2 * Math.Pow(DBH, K2) + B3 * Math.Log((CR + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBAL1, K3) / Math.Log(DBH + K4)) + B6 * Math.Sqrt(SBA1));

            // CROWN RATIO ADJUSTMENT
            //
            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
            float ADJ;
            if (ISPGRP == 0)
            {
                ADJ = 1.0F;
            }
            else if (ISPGRP == 1)
            {
                ADJ = 0.8722F;
            }
            else if (ISPGRP == 2)
            {
                ADJ = 1.0F;
            }
            else if (ISPGRP == 5)
            {
                ADJ = 0.7928F;
            }
            else if (ISPGRP == 7)
            {
                ADJ = 1.0F;
            }
            else
            {
                ADJ = 0.8F;
            }

            DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG >= 0.0F);
        }

        private static void DG_SWO(int ISPGRP, float DBH, float CR, float SITE, float SBAL1, float SBA1, out float DG)
        {
            // DIAMETER GROWTH PARAMETERS FOR SOUTHWEST OREGON(11 parameters - all species)
            //
            // DF Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // GW Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // PP Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // SP Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // IC Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // WH Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // MD Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // GC Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // TA Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // CL Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // BL Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // BO Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // RA Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            // WI Coefficients from Hann and Hanus(2002) FRL Research Contribution 39
            float[,] DGPAR = {
                {
                    -5.35558894F, -5.84904111F, -4.51958940F,      // DF,GW,PP
                    -4.12342552F, -2.08551255F, -5.70052255F,      // SP,IC,WH
                    -11.45456097F, -9.15835863F, -8.84531757F,      // RC,PY,MD
                    -7.78451344F, -3.36821750F, -3.59333060F,      // GC,TA,CL
                    -3.41449922F, -7.81267986F, -4.43438109F,      // BL,WO,BO
                    -4.39082007F, -8.08352683F, -8.08352683F,      // RA,PD,WI
                },
                {
                    0.840528547F, 1.668196109F, 0.813998712F,     // DF,GW,PP
                    0.734988422F, 0.596043703F, 0.865087036F,     // SP,IC,WH
                    0.784133664F, 1.0F, 1.5F,     // RC,PY,MD
                    1.2F, 1.2F, 1.2F,     // GC,TA,CL
                    1.0F, 1.405616529F, 0.930930363F,     // BL,WO,BO
                    1.0F, 1.0F, 1.0F,     // RA,PD,WI
                },
                {
                    -0.0427481848F, -0.0853271265F, -0.0493858858F,    // DF,GW,PP
                    -0.0425469735F, -0.0215223077F, -0.0432543518F,    // SP,IC,WH
                    -0.0261377888F, -0.00000035F, -0.0006F,    // RC,PY,MD
                    -0.07F, -0.07F, -0.07F,    // GC,TA,CL
                    -0.05F, -0.0603105850F, -0.0465947242F,    // BL,WO,BO
                    -0.0945057147F, -0.00000035F, -0.00000035F,    // RA,PD,WI
                },
                {
                    1.15950313F, 1.21222176F, 1.10249641F,    // DF,GW,PP
                    1.05942163F, 1.02734556F, 1.10859727F,    // SP,IC,WH
                    0.70174783F, 1.16688474F, 0.51225596F,    // RC,PY,MD
                    0.0F, 0.0F, 0.51637418F,    // GC,TA,CL
                    0.0F, 0.64286007F, 0.0F,    // BL,WO,BO
                    1.06867026F, 0.31176647F, 0.31176647F,    // RA,PD,WI
                },
                {
                    0.954711126F, 0.679346647F, 0.879440023F,    // DF,GW,PP
                    0.808656390F, 0.383450822F, 0.977332597F,    // SP,IC,WH
                    2.057236260F, 0.0F, 0.418129153F,    // RC,PY,MD
                    1.01436101F, 0.0F, 0.0F,    // GC,TA,CL
                    0.324349277F, 1.037687142F, 0.510717175F,    // BL,WO,BO
                    0.685908029F, 0.0F, 0.0F,    // RA,PD,WI
                },
                {
                    -0.00894779670F, -0.00809965733F, -0.0108521667F,    // DF,GW,PP
                    -0.0107837565F, -0.00489046624F, 0.0F,    // SP,IC,WH
                    -0.00415440257F, 0.0F, -0.00355254593F,   // RC,PY,MD
                    -0.00834323811F, 0.0F, 0.0F,   // GC,TA,CL
                    0.0F, 0.0F, 0.0F,   // BL,WO,BO
                    -0.00586331028F, 0.0F, 0.0F,   // RA,PD,WI
                },
                {
                    0.0F, 0.0F, -0.0333706948F,    // DF,GW,PP
                    0.0F, -0.0609024782F, -0.0526263229F,    // SP,IC,WH
                    0.0F, -0.02F, -0.0321315389F,    // RC,PY,MD
                    0.0F, -0.0339813575F, -0.02F,    // GC,TA,CL
                    -0.0989519477F, -0.0787012218F, -0.0688832423F,    // BL,WO,BO
                    0.0F, -0.0730788052F, -0.0730788052F,    // RA,PD,WI
                },
                {
                    5.0F, 5.0F, 5.0F,    // DF,GW,PP
                    5.0F, 5.0F, 5.0F,    // SP,IC,WH
                    5.0F, 4000.0F, 110.0F,    // RC,PY,MD
                    10.0F, 10.0F, 10.0F,    // GC,TA,CL
                    10.0F, 5.0F, 5.0F,    // BL,WO,BO
                    5.0F, 4000.0F, 4000.0F,    // RA,PD,WI
                },
                {
                    1.0F, 1.0F, 1.0F,    // DF,GW,PP
                    1.0F, 1.0F, 1.0F,    // SP,IC,WH
                    1.0F, 4.0F, 2.0F,    // RC,PY,MD
                    1.0F, 1.0F, 1.0F,    // GC,TA,CL
                    1.0F, 1.0F, 1.0F,    // BL,WO,BO
                    1.0F, 4.0F, 4.0F,    // RA,PD,WI
                },
                {
                    1.0F, 1.0F, 1.0F,    // DF,GW,PP
                    1.0F, 1.0F, 1.0F,    // SP,IC,WH
                    1.0F, 1.0F, 1.0F,    // RC,PY,MD
                    1.0F, 1.0F, 1.0F,    // GC,TA,CL
                    1.0F, 1.0F, 1.0F,    // BL,WO,BO
                    1.0F, 1.0F, 1.0F,    // RA,PD,WI
                },
                {
                    2.7F, 2.7F, 2.7F,    // DF,GW,PP
                    2.7F, 2.7F, 2.7F,    // SP,IC,WH
                    2.7F, 2.7F, 2.7F,    // RC,PY,MD
                    2.7F, 2.7F, 2.7F,    // GC,TA,CL
                    2.7F, 2.7F, 2.7F,    // BL,WO,BO
                    2.7F, 2.7F, 2.7F             // RA,PD,WI
                }
            };

            float B0 = DGPAR[0, ISPGRP];
            float B1 = DGPAR[1, ISPGRP];
            float B2 = DGPAR[2, ISPGRP];
            float B3 = DGPAR[3, ISPGRP];
            float B4 = DGPAR[4, ISPGRP];
            float B5 = DGPAR[5, ISPGRP];
            float B6 = DGPAR[6, ISPGRP];
            float K1 = DGPAR[7, ISPGRP];
            float K2 = DGPAR[8, ISPGRP];
            float K3 = DGPAR[9, ISPGRP];
            float K4 = DGPAR[10, ISPGRP];
            float LNDG = (float)(B0 + B1 * Math.Log(DBH + K1) + B2 * Math.Pow(DBH, K2) + B3 * Math.Log((CR + 0.2) / 1.2) + B4 * Math.Log(SITE) + B5 * (Math.Pow(SBAL1, K3) / Math.Log(DBH + K4)) + B6 * Math.Sqrt(SBA1));

            // CROWN RATIO ADJUSTMENT
            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));

            // FULL ADJUSTMENTS
            float ADJ;
            if (ISPGRP == 0)
            {
                ADJ = 0.8938F;
            }
            else if (ISPGRP == 1)
            {
                ADJ = 0.8722F;
            }
            else if (ISPGRP == 3)
            {
                ADJ = 0.7903F;
            }
            else if (ISPGRP == 8)
            {
                ADJ = 0.7928F;
            }
            else if (ISPGRP == 9)
            {
                ADJ = 0.7259F;
            }
            else if (ISPGRP == 13)
            {
                ADJ = 1.0F;
            }
            else if (ISPGRP == 14)
            {
                ADJ = 0.7667F;
            }
            else
            {
                ADJ = 0.8F;
            }

            DG = (float)Math.Exp(LNDG) * CRADJ * ADJ;
            Debug.Assert(DG >= 0.0F);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species">FIA species code.</param>
        /// <param name="variant">Organon variant.</param>
        /// <param name="simulationStep">Simulation cycle.</param>
        /// <param name="BABT">Basal area before treatment? (DOUG?)</param>
        /// <param name="BART">Basal area removed by treatment? (DOUG?)</param>
        /// <param name="YT">Thinning year data? (DOUG?)</param>
        /// <param name="THINADJ">Thinning adjustment. (DOUG?)</param>
        /// <remarks>
        /// Has special cases for Douglas-fir, western hemlock, and red alder (only for RAP).
        /// </remarks>
        private static void DG_THIN(FiaCode species, Variant variant, int simulationStep, float BABT, float[] BART, float[] YT, out float THINADJ)
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
            else if ((variant == Variant.Rap) && (species == FiaCode.AlnusRubra))
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

            float XTIME = (float)(simulationStep) * 5.0F;
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

        private static void DG_FERT(FiaCode species, Variant variant, int simulationStep, float SI_1, float[] PN, float[] YF, out float FERTADJ)
        {
            // CALCULATE FERTILIZER ADJUSTMENT FOR DIAMETER GROWTH RATE
            // FROM HANN ET AL.(2003) FRL RESEARCH CONTRIBUTION 40
            // SET PARAMETERS FOR ADJUSTMENT
            float PF1;
            float PF2;
            float PF3;
            float PF4;
            float PF5;
            if (variant <= Variant.Smc)
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
            float XTIME = (float)simulationStep * 5.0F;
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
