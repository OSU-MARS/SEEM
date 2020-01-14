using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    // port of ORGVOL.FOR and VOLEQNS.FOR.
    internal class LegacyVolume
    {
        /// <summary>
        /// Get tree's merchantable cubic foot volume for given top diameter and cut height.
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="species">Tree species.</param>
        /// <param name="DBH">Tree diameter at breast height (inches).</param>
        /// <param name="HT">Tree height (feet).</param>
        /// <param name="CR">Tree crown ratio.</param>
        /// <param name="CFTD">Top diameter (inches).</param>
        /// <param name="CFSH">Stump cut height (feet?).</param>
        /// <returns>Tree's volume in cubic feet.</returns>
        public static float CF(Variant variant, FiaCode species, float DBH, float HT, float CR, float CFTD, float CFSH)
        {
            // CALCULATE ENDING OR STARTING CF VOLUME
            //
            // VALU = CF VOLUME TO BE CALCULATED
            float DIB;
            float DIB1FT;
            switch (variant)
            {
                case Variant.Swo:
                    DIB = SWO_DIB(species, DBH, CR);
                    DIB1FT = SWO_DIB1FT(species, DBH, CR);
                    break;
                case Variant.Nwo: // western Willamette Valley
                case Variant.Smc:
                    DIB = NWO_DIB(species, DBH, CR);
                    DIB1FT = NWO_DIB1FT(species, DBH, CR);
                    break;
                case Variant.Rap:
                    DIB = RAP_DIB(species, DBH);
                    DIB1FT = RAP_DIB1FT(species, DBH, CR);
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledVariantException(variant);
            }

            if (species <= (FiaCode)300)
            {
                switch (variant)
                {
                    case Variant.Swo:
                        return SWO_CCFV(species, DBH, HT, CR, DIB, DIB1FT, CFTD, CFSH);
                    case Variant.Nwo:
                    case Variant.Smc:
                        return NWO_CCFV(species, DBH, HT, CR, DIB, DIB1FT, CFTD, CFSH);
                    case Variant.Rap:
                        return RAP_CCFV(species, DBH, HT, CR, DIB, DIB1FT, CFTD, CFSH);
                    default:
                        throw OrganonVariant.CreateUnhandledVariantException(variant);
                }
            }
            else
            {
                switch (variant)
                {
                    case Variant.Swo:
                        return SWO_HCFV(species, DBH, HT, DIB, DIB1FT, CFTD, CFSH);
                    case Variant.Nwo:
                    case Variant.Smc:
                        return NWO_HCFV(species, DBH, HT, DIB, DIB1FT, CFTD, CFSH);
                    case Variant.Rap:
                        return RAP_HCFV(species, DBH, HT, DIB, DIB1FT, CFTD, CFSH);
                    default:
                        throw OrganonVariant.CreateUnhandledVariantException(variant);
                }
            }
        }

        private static bool EDIT(Variant variant, FiaCode species, float CFTD, float CFSH, float LOGLL, float LOGML, float LOGTD, float LOGSH, float LOGTA, float DBH, float HT, out int[] VERROR, out int[] VWARNING, out int TWARNING)
        {
            //IMPLICIT NONE
            //INTEGER*4 VERS,SPP
            //INTEGER*4 I,J,SPGRP,VERROR(5),TERROR(4),VWARNING(5),TWARNING,TEMP
            //REAL*4 CFTD,CFSH,LOGLL,LOGML,LOGTD,LOGSH,LOGTA,DBH,HT,CR
            //REAL*4 B0,B1,PHT
            //LOGICAL*2 ERROR
            VERROR = new int[5];
            VWARNING = new int[5];

            // EDIT VOLUME SPECIFICATIONS FOR ERRORS
            if (CFSH > 4.5F)
            {
                VERROR[0] = 1;
            }
            if (LOGML > LOGLL)
            {
                VERROR[1] = 1;
            }
            if (LOGTD > 0.0F && LOGTD < 1.0F)
            {
                VERROR[2] = 1;
            }
            if (LOGSH > 4.5F)
            {
                VERROR[3] = 1;
            }
            if (LOGTA > 0.0F && LOGTA < 1.0F)
            {
                VERROR[4] = 1;
            }

            bool ERROR = false;
            for (int I = 0; I < VERROR.Length; ++I)
            {
                if (VERROR[0] == 1)
                {
                    ERROR = true;
                }
            }

            // EDIT VOLUME SPECIFICATIONS FOR ERRORS
            if (CFTD > 12.0F)
            {
                VWARNING[0] = 1;
            }
            if (LOGLL < 8.0 || LOGLL > 40.0F)
            {
                VWARNING[1] = 1;
            }
            if (LOGML < 8.0F || LOGML > 40.0F)
            {
                VWARNING[2] = 1;
            }
            if (LOGTD > 12.0F)
            {
                VWARNING[3] = 1;
            }
            if (LOGTA > 12.0F)
            {
                VWARNING[4] = 1;
            }

            float B0 = 0.0F;
            float B1 = -0.04484724F;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 19.04942539F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    if (variant != Variant.Swo)
                    {
                        B0 = 19.04942539F;
                    }
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    B0 = 16.26279948F;
                    break;
                case FiaCode.PinusPonderosa:
                    B0 = 17.11482201F;
                    break;
                case FiaCode.PinusLambertiana:
                    B0 = 14.29011403F;
                    break;
                default:
                    B0 = 15.80319194F;
                    break;
            }

            TWARNING = 0;
            if (HT > 4.5F)
            {
                float PHT = 4.5F + B0 * DBH / (1.0F - B1 * DBH);
                if (HT > PHT)
                {
                    TWARNING = 1;
                }
            }

            return ERROR;
        }

        /// <summary>
        /// Get log volume table for stand.
        /// </summary>
        /// <param name="configuration">Organon configuration.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="XLOGLL"></param>
        /// <param name="NL">Number of logs table.</param>
        /// <param name="LVOL">Log volume table.</param>
        public static void LOG_TABLE(OrganonConfiguration configuration, Stand stand, float XLOGLL, out float[,] NL, out float[,] LVOL)
        {
            float LOGTA = configuration.Bucking.Trim / 12.0F;
            int LOGLL = (int)XLOGLL;
            NL = new float[40, 4];
            LVOL = new float[40, 4];

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                // GET LOG TABLE SPECIES GROUP AND EXP FACTOR
                float dbh = stand.Dbh[treeIndex];
                if (dbh <= configuration.Bucking.TopDiameter)
                {
                    continue;
                }

                FiaCode species = stand.Species[treeIndex];
                int speciesGroup;
                if (configuration.Variant.Variant != Variant.Rap)
                {
                    if (species <= (FiaCode)300)
                    {
                        if (species == FiaCode.PseudotsugaMenziesii)
                        {
                            speciesGroup = 1;
                        }
                        else if (species == FiaCode.AbiesConcolor || species == FiaCode.AbiesGrandis)
                        {
                            speciesGroup = 2;
                        }
                        else if ((species == FiaCode.PinusLambertiana || species == FiaCode.PinusPonderosa) && configuration.Variant.Variant == Variant.Swo)
                        {
                            speciesGroup = 3;
                        }
                        else if (species == FiaCode.TsugaHeterophylla && configuration.Variant.Variant != Variant.Swo)
                        {
                            speciesGroup = 3;
                        }
                        else
                        {
                            speciesGroup = 4;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (species <= (FiaCode)300 || species == FiaCode.AlnusRubra)
                    {
                        if (species == FiaCode.AlnusRubra)
                        {
                            speciesGroup = 1;
                        }
                        else if (species == FiaCode.PseudotsugaMenziesii)
                        {
                            speciesGroup = 2;
                        }
                        else if (species == FiaCode.TsugaHeterophylla)
                        {
                            speciesGroup = 3;
                        }
                        else
                        {
                            speciesGroup = 4;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                float height = stand.Height[treeIndex];
                float crownRatio = stand.CrownRatio[treeIndex];
                float b11;
                float b12;
                float b13;
                float b21;
                float alpha;
                float predictedDbhInsideBark;
                switch (configuration.Variant.Variant)
                {
                    case Variant.Swo:
                        predictedDbhInsideBark = SWO_DIB(species, dbh, crownRatio);
                        alpha = SWO_TAPER(species, out b11, out b12, out b13, out b21);
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        predictedDbhInsideBark = NWO_DIB(species, dbh, crownRatio);
                        alpha = NWO_TAPER(species, out b11, out b12, out b13, out b21);
                        break;
                    case Variant.Rap:
                        predictedDbhInsideBark = RAP_DIB(species, dbh);
                        alpha = RAP_TAPER(species, out b11, out b12, out b13, out b21);
                        break;
                    default:
                        throw OrganonVariant.CreateUnhandledVariantException(configuration.Variant.Variant);
                }
                if (predictedDbhInsideBark <= configuration.Bucking.TopDiameter)
                {
                    continue;
                }

                // COMPUTE MERCHANTABLE HEIGHT
                // variables requiring initialization before first goto
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                float heightToCrownBase = (1.0F - crownRatio) * height;
                float heightAboveBreast = height - 4.5F;
                // Walters 1986b:34 x1 form: known biased and results in negative DIBs in LOGVOL()
                 float x1 = b11 + b12 * (heightAboveBreast / dbh) + b13 * (heightAboveBreast / dbh) * (heightAboveBreast / dbh);
                // Hann 2011: avoids negative DIBs but is incompatible with the quadratic root finding in the code below
                // float x1 = b11 + b12 * (float)Math.Exp(b13 * (heightAboveBreast / dbh) * (heightAboveBreast / dbh)); // Hann 2011
                float TLL = 0.0F; // maybe an initialization issue in Fortran?
                float WLT = (alpha * heightToCrownBase - 4.5F) / heightAboveBreast;
                float[,] TOTS = new float[2, 4];

                float MH;
                if (configuration.Variant.Variant == Variant.Rap && species == FiaCode.AlnusRubra)
                {
                    MH = RedAlder.RA_MH(dbh, height, crownRatio, configuration.Bucking.TopDiameter);
                    goto logVolumes;
                }

                float D = 0.0F;
                float H = alpha * heightToCrownBase;
                LOGVOL(1, speciesGroup, LOGLL, WLT, H, heightAboveBreast, x1, b21, predictedDbhInsideBark, D, TLL, expansionFactor, out float diameterInsideBark, out float _, NL, LVOL, TOTS);
                float A;
                float B;
                float C;
                if (WLT <= 0.0F)
                {
                    A = -(x1 + 1.0F) / (heightAboveBreast * heightAboveBreast);
                    B = x1 / heightAboveBreast;
                    C = 1.0F - configuration.Bucking.TopDiameter / predictedDbhInsideBark;
                }
                else if (WLT > 0.0F && diameterInsideBark <= configuration.Bucking.TopDiameter)
                {
                    A = b21 / (heightAboveBreast * heightAboveBreast);
                    B = x1 / heightAboveBreast;
                    C = 1.0F - configuration.Bucking.TopDiameter / predictedDbhInsideBark;
                }
                else
                {
                    A = (b21 * WLT * WLT - x1 - 2.0F * b21 * WLT - 1.0F) / (heightAboveBreast * heightAboveBreast * (WLT - 1.0F) * (WLT - 1.0F));
                    B = ((2.0F * WLT - 1.0F + b21 * WLT * WLT + x1 * WLT * WLT) - (b21 * WLT * WLT - x1 - 2.0F * b21 * WLT - 1.0F)) / (heightAboveBreast * (WLT - 1.0F) * (WLT - 1.0F));
                    C = -(configuration.Bucking.TopDiameter / predictedDbhInsideBark + (2.0F * WLT - 1.0F + b21 * WLT * WLT + x1 * WLT * WLT) / (WLT - 1.0F) * (WLT - 1.0F));
                }
                float ROOT = B * B - 4 * A * C;
                if (ROOT < 0.0F)
                {
                    MH = 0.0F;
                    goto logVolumes;
                }
                float HM1 = (-B + (float)Math.Sqrt(ROOT)) / (2 * A);
                float HM2 = (-B - (float)Math.Sqrt(ROOT)) / (2 * A);

                // CHECK MERCHANTABLE LOG DIAMETERS
                if (HM1 > 0.0F)
                {
                    D = 1.0F;
                    H = HM1 + 4.5F;

                    LOGVOL(4, speciesGroup, LOGLL, WLT, H, heightAboveBreast, x1, b21, predictedDbhInsideBark, D, TLL, expansionFactor, out diameterInsideBark, out _, NL, LVOL, TOTS);
                    float NDI = (float)Math.Round(10.0F * diameterInsideBark);
                    float NLOGTD = (float)Math.Round(10.0F * configuration.Bucking.TopDiameter);
                    if (NDI != NLOGTD)
                    {
                        HM1 = 0.0F;
                    }
                }
                else
                {
                    HM1 = 0.0F;
                }

                if (HM2 > 0.0F)
                {
                    D = 1.0F;
                    H = HM2 + 4.5F;
                    LOGVOL(4, speciesGroup, LOGLL, WLT, H, heightAboveBreast, x1, b21, predictedDbhInsideBark, D, TLL, expansionFactor, out diameterInsideBark, out _, NL, LVOL, TOTS);
                    float NDI = (float)Math.Round(10.0F * diameterInsideBark);
                    float NLOGTD = (float)Math.Round(10.0F * configuration.Bucking.TopDiameter);
                    if (NDI != NLOGTD)
                    {
                        HM2 = 0.0F;
                    }
                }
                else
                {
                    HM2 = 0.0F;
                }

                if (HM1 < 0.0F || HM1 > heightAboveBreast)
                {
                    if (HM2 < 0.0F || HM2 > heightAboveBreast)
                    {
                        MH = 0.0F;
                        goto logVolumes;
                    }
                    else
                    {
                        MH = HM2;
                    }
                }
                else if (HM2 < 0.0F || HM2 > heightAboveBreast)
                {
                    MH = HM1;
                }
                else
                {
                    MH = Math.Max(HM1, HM2);
                }
                MH += 4.5F;

            logVolumes:
                // CALCULATE LOG VOLUMES
                int NW = (int)((MH - configuration.Bucking.StumpCutHeight) / ((float)LOGLL + LOGTA));
                if (NW < 0)
                {
                    NW = 0;
                }
                TLL = MH - configuration.Bucking.StumpCutHeight - (float)NW * ((float)LOGLL + LOGTA);
                H = configuration.Bucking.StumpCutHeight;
                for (int II = 0; II < NW; ++II)
                {
                    D = 1.0F;
                    H = H + (float)LOGLL + LOGTA;
                    if (configuration.Variant.Variant == Variant.Rap && species == FiaCode.AlnusRubra)
                    {
                        RedAlder.RA_LOGVOL(2, dbh, height, crownRatio, speciesGroup, LOGLL, H, D, TLL, expansionFactor, out _, out _, NL, LVOL, TOTS);
                    }
                    else
                    {
                        LOGVOL(2, speciesGroup, LOGLL, WLT, H, heightAboveBreast, x1, b21, predictedDbhInsideBark, D, TLL, expansionFactor, out _, out _, NL, LVOL, TOTS);
                    }
                }

                // COMPUTE VOLUME OF TOP LOG
                if (TLL >= (configuration.Bucking.PreferredLogLength + LOGTA))
                {
                    int J = (int)(TLL - LOGTA);
                    TLL = (float)J + LOGTA;
                    D = TLL / (float)LOGLL;
                    H += TLL;
                    if (configuration.Variant.Variant == Variant.Rap && species == FiaCode.AlnusRubra)
                    {
                        RedAlder.RA_LOGVOL(2, dbh, height, crownRatio, speciesGroup, LOGLL, H, D, TLL, expansionFactor, out _, out _, NL, LVOL, TOTS);
                    }
                    else
                    {
                        LOGVOL(2, speciesGroup, LOGLL, WLT, H, heightAboveBreast, x1, b21, predictedDbhInsideBark, D, TLL, expansionFactor, out _, out _, NL, LVOL, TOTS);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculation"></param>
        /// <param name="speciesGroup"></param>
        /// <param name="LOGLL"></param>
        /// <param name="WLT">Regression coefficient.</param>
        /// <param name="H">Tree height (feet).</param>
        /// <param name="HABH">Height above breast height to diameter of interest (feet).</param>
        /// <param name="x1">Regression coefficient.</param>
        /// <param name="x2">Regression coefficient.</param>
        /// <param name="predictedDiameterInsideBark"></param>
        /// <param name="dbh">Diameter at breast height (inches).</param>
        /// <param name="TLL"></param>
        /// <param name="expansionFactor">Tree's expansion factor.</param>
        /// <param name="diameterInsideBark"></param>
        /// <param name="V"></param>
        /// <param name="NL"></param>
        /// <param name="LVOL"></param>
        /// <param name="TOTS"></param>
        private static void LOGVOL(int calculation, int speciesGroup, int LOGLL, float WLT, float H, float HABH, float x1, float x2, float predictedDiameterInsideBark, float dbh, float TLL, float expansionFactor, out float diameterInsideBark, out float V, float[,] NL, float[,] LVOL, float[,] TOTS)
        {
            // Hann DW. 2011. Revised Volume and Taper Equations for Six Major Conifer Species in Southwest Oregon. Forest Biometrics 
            //   Research Paper 2. http://cips.forestry.oregonstate.edu/organon-publications
            // Gabriel RS. 2017. The Use of Terrestrial Laser Scanning and Computer Vision in Tree Level Modeling. 
            //   https://ir.library.oregonstate.edu/concern/graduate_thesis_or_dissertations/qv33s274b
            // SUBROUTINE LOGVOL(N, SVOL, EX, D, V)
            // ROUTINE TO CALCULATE LOG VOLUME AND ADD TO APPROPRIATE CELL
            //
            // N = TYPE OF CALCULATION
            //       = 1  MERCHANTABLE HEIGHT
            //       = 2  LOG VOLUME
            //       = 3  TREE VOLUME
            //       = 4  TOP DIAMETER CHECK
            //
            // SVOL = SPECIES GROUP FOR LOG REPORT
            // EX = TREE RESIDUAL OR CUT EXPANSION FACTOR
            // D = RATIO OF LOG LENGTH TO SPECIFIED LOG LENGTH
            // REAL*4     NL(40,4), LVOL(40,4),TOTS(2,4)

            //     USE TAPER EQUATION TO DETERMINE DIAMETER AT TOP OF LOG
            float RH;
            if (calculation == 1)
            {
                RH = WLT;
            }
            else
            {
                RH = (H - 4.5F) / HABH;
            }
            Debug.Assert(RH >= 0.0F);
            Debug.Assert(RH <= 1.0F);

            float i1 = RH >= 0.0F && RH <= WLT ? 0.0F : 1.0F;
            float i2 = WLT <= 0.0F ? 0.0F : 1.0F;
            float jp1 = (RH - 1.0F) / (WLT - 1.0F);
            float jp2 = (WLT - RH) / (WLT - 1.0F);
            float z0 = 1.0F - RH + i2 * (RH + i1 * (jp1 * (1.0F + jp2) - 1.0F)) - (RH - 1.0F) * (RH - i2 * RH);
            float z1 = i2 * (RH + i1 * (jp1 * (RH + WLT * jp2) - RH)) - (RH - 1.0F) * (RH - i2 * RH);
            float z2 = i2 * (RH * RH + i1 * (jp1 * WLT * (2.0F * RH - WLT + WLT * jp2) - RH * RH));
            diameterInsideBark = predictedDiameterInsideBark * (z0 + x1 * z1 + x2 * z2);
            Debug.Assert(diameterInsideBark > 0.0F);
            Debug.Assert(diameterInsideBark < Constant.Maximum.DiameterInInches);
            if (calculation == 1 || calculation == 4)
            {
                V = 0.0F;
                return;
            }

            // EXTRACT VOLUME FROM VOLUME TABLES
            int length;
            if (dbh != 1.0F)
            {
                length = (int)TLL;
            }
            else
            {
                length = LOGLL;
            }

            float[] SVTBL = new float[] { 0.0F, 0.143F, 0.39F, 0.676F, 1.07F, 1.160F, 1.4F, 1.501F, 2.084F,
                                          3.126F, 3.749F, 4.9F, 6.043F, 7.14F, 8.88F, 10.0F, 11.528F,
                                          13.29F, 14.99F, 17.499F, 18.99F, 20.88F, 23.51F, 25.218F,
                                          28.677F, 31.249F, 34.22F, 36.376F, 38.04F, 41.06F, 44.376F,
                                          45.975F, 48.99F, 50.0F, 54.688F, 57.66F, 64.319F, 66.73F, 70.0F,
                                          75.24F, 79.48F, 83.91F, 87.19F, 92.501F, 94.99F, 99.075F,
                                          103.501F, 107.97F, 112.292F, 116.99F, 121.65F, 126.525F,
                                          131.51F, 136.51F, 141.61F, 146.912F, 152.21F, 157.71F,
                                          163.288F, 168.99F, 174.85F, 180.749F, 186.623F, 193.17F,
                                          199.12F, 205.685F, 211.81F, 218.501F, 225.685F, 232.499F,
                                          239.317F, 246.615F, 254.04F, 261.525F, 269.04F, 276.63F,
                                          284.26F, 292.501F, 300.655F, 308.9F };
            float[] SVTBL16 = new float[] { 1.249F, 1.608F, 1.854F, 2.410F, 3.542F, 4.167F };
            float[] SVTBL32 = new float[] { 1.57F, 1.8F, 2.2F, 2.9F, 3.815F, 4.499F };
            int DII = (int)diameterInsideBark - 1;
            if (DII >= 5 && DII <= 10)
            {
                if (length >= 16 && length <= 31)
                {
                    V = SVTBL16[DII - 5] * (float)length * expansionFactor;
                }
                else if (length >= 32 && length <= 40)
                {
                    V = SVTBL32[DII - 5] * (float)length * expansionFactor;
                }
                else
                {
                    V = SVTBL[DII] * (float)length * expansionFactor;
                }
            }
            else
            {
                V = SVTBL[DII] * (float)length * expansionFactor;
            }
            if (calculation == 3)
            {
                return;
            }
            DII /= 2;
            NL[DII, speciesGroup] = NL[DII, speciesGroup] + expansionFactor;
            LVOL[DII, speciesGroup] = LVOL[DII, speciesGroup] + V;
            TOTS[1, speciesGroup] = TOTS[1, speciesGroup] + V;
        }

        private static float NWO_CCFV(FiaCode species, float DBH, float HT, float CR, float DIB, float DIB1FT, float CFTD, float CFSH)
        {
            // CONIFER CUBIC MERCH VOLUME
            float A1;
            float A2;
            float A3;
            float A4;
            float A5;
            float EQNO1; // TODO: make bool
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            float B7;
            float EQNO2;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    A1 = -3.39101798F;
                    A2 = 0.918583494F;
                    A3 = 1.3330217F;
                    A4 = -0.935974246F;
                    A5 = 3.0F;
                    EQNO1 = 1.0F;
                    B1 = 1.24485628F;
                    B2 = 0.346490193F;
                    B3 = -0.56574969F;
                    B4 = 0.632239239F;
                    B5 = -0.152406551F;
                    B6 = 4.55802463F;
                    B7 = -0.051186711F;
                    EQNO2 = 1.0F;
                    break;
                case FiaCode.AbiesGrandis:
                    A1 = -0.76519904F;
                    A2 = 0.25F;
                    A3 = 3.80136398F;
                    A4 = -1.7902001F;
                    A5 = 1.0F;
                    EQNO1 = 1.0F;
                    B1 = 1.33397259F;
                    B2 = 0.357808283F;
                    B3 = -0.755355039F;
                    B4 = 0.5F;
                    B5 = -0.261766125F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 1.0F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    A1 = 0.930057F;
                    A2 = 3.74152F;
                    A3 = 0.0F;
                    A4 = 0.0F;
                    A5 = 0.0F;
                    EQNO1 = 0.0F;
                    B1 = 1.168F;
                    B2 = 0.265430F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 0.0F;
                    break;
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    A1 = -3.75729892F;
                    A2 = 1.23328561F;
                    A3 = 1.17859869F;
                    A4 = -0.45135743F;
                    A5 = 2.0F;
                    EQNO1 = 1.0F;
                    B1 = 0.90763281F;
                    B2 = 0.34284673F;
                    B3 = -0.63865388F;
                    B4 = 1.5857204F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            B1 /= 1000.0F;

            // CALCULATE CROWN RATIO ABOVE BREAST HEIGHT
            float HCB = HT - CR * HT;
            float CRABH;
            if (HCB > 4.5F)
            {
                CRABH = (HT - HCB) / (HT - 4.5F);
            }
            else
            {
                CRABH = 1.0F;
            }

            // CALCULATE VOLUME ABOVE BREAST HEIGHT
            float VABH = 0.0F;
            if (DIB > CFTD)
            {
                float RAT;
                if (EQNO1 < 0.5F)
                {
                    RAT = 1.0F - A1 * (float)Math.Pow(CFTD / DIB, A2);
                }
                else
                {
                    RAT = 1.0F - (float)(Math.Exp(A1 * Math.Pow(1.0F - CFTD / DIB, A2)) * Math.Pow(CFTD / DIB, A3 + A4 * Math.Pow(CR, A5)));
                }
                float X1 = (float)Math.Pow((HT - 4.5F) / DBH, B2 * Math.Pow(1.0F - Math.Exp(Math.Pow(B3 * DBH, B4)), EQNO2));
                float X2 = (float)Math.Exp(Math.Pow(B5 * CRABH, B6));
                float X3 = (float)Math.Pow(DBH, B7);
                VABH = RAT * B1 * X1 * X2 * X3 * DBH * DBH * (HT - 4.5F);
            }

            // VOLUME BELOW BREAST HEIGHT
            float DIBD = (float)Math.Pow(DIB / DIB1FT, 2.0F / 3.0F);
            float SDIB = (float)Math.Pow((4.5F - DIBD - CFSH * (1.0F - DIBD)) / 3.5F, 1.5F) * DIB1FT;
            float VBBH = 0.0F;
            if (SDIB > CFTD)
            {
                VBBH = 0.25F * (float)Math.PI * DIB1FT * DIB1FT *
                           (1.0F / 43904.0F * (729.0F + 81.0F * DIBD +
                                297.0F * (float)Math.Pow(DIB / DIB1FT, 4.0F / 3.0F) +
                                265.0F * (DIB / DIB1FT) * (DIB / DIB1FT)) -
                       1.0F / 6174.0F * ((float)Math.Pow(4.5F - DIBD, 3.0F) * CFSH -
                                1.5F * (4.5F - DIBD) * (4.5F - DIBD) * (1.0F - DIBD) * CFSH * CFSH +
                                (4.5F - DIBD) * (1.0F - DIBD) * (1.0F - DIBD) * CFSH * CFSH * CFSH -
                                0.25F * (float)Math.Pow((1.0F - DIBD), 3.0F) * CFSH * CFSH * CFSH * CFSH));
            }

            // DETERMINE TREE'S VOLUME
            float VALU = VABH + VBBH;
            if (VALU < 0.0F)
            {
                VALU = 0.0F;
            }
            return VALU;
        }

        private static float NWO_DIB(FiaCode species, float DBH, float CR)
        {
            float E1;
            float E2;
            float E3;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    E1 = 0.971330F;
                    E2 = 0.966365F;
                    E3 = 0.0F;
                    break;
                case FiaCode.AbiesGrandis:
                    E1 = 0.903563F;
                    E2 = 0.989388F;
                    E3 = 0.0F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    E1 = 0.933707F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.ThujaPlicata:
                    E1 = 0.9497F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.TaxusBrevifolia:
                    E1 = 0.97F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.ArbutusMenziesii:
                    E1 = 0.96317F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    E1 = 0.97059F;
                    E2 = 0.993585F;
                    E3 = 0.0F;
                    break;
                case FiaCode.QuercusGarryana:
                    E1 = 0.878457F;
                    E2 = 1.02393F;
                    E3 = 0.0F;
                    break;
                case FiaCode.AlnusRubra:
                    E1 = 0.947F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    E1 = 0.94448F;
                    E2 = 0.987517F;
                    E3 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float DIB = E1 * (float)Math.Pow(DBH, E2) * (float)Math.Exp(E3 * Math.Sqrt((1.0F - CR)));
            Debug.Assert(DIB > 0.0F);
            return DIB;
        }

        private static float NWO_DIB1FT(FiaCode species, float DBH, float CR)
        {
            float G0;
            float G1;
            float G2;
            float G3;
            float G4;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    G0 = 0.149809111F;
                    G1 = 0.900790279F;
                    G2 = 0.133648456F;
                    G3 = 3.67532829F;
                    G4 = 1.0213663112F;
                    break;
                case FiaCode.AbiesGrandis:
                    G0 = 0.393048214F;
                    G1 = 0.729932627F;
                    G2 = 0.120814754F;
                    G3 = 1.0F;
                    G4 = 1.097851010F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    G0 = 0.0F;
                    G1 = 0.989819F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    G0 = 0.451569966F;
                    G1 = 0.831752493F;
                    G2 = 0.216216295F;
                    G3 = 7.00446878F;
                    G4 = 1.0560026859F;
                    break;
                case FiaCode.ArbutusMenziesii:
                case FiaCode.AcerMacrophyllum:
                case FiaCode.QuercusGarryana:
                case FiaCode.AlnusRubra:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    G0 = 0.0F;
                    G1 = 1.0F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float DIB1FT = G0 + G1 * (float)Math.Pow(G2 * CR, G3) * (float)Math.Pow(DBH, G4);
            return DIB1FT;
        }

        // same as SWO_HCFV except for species supported
        private static float NWO_HCFV(FiaCode species, float DBH, float HT, float DIB, float DIB1FT, float CFTD, float CFSH)
        {
            // CALCULATES CUBIC FOOT MERCH VOLUMES OF HARDWOODS USING
            // EQUATIONS DEVELOPED FROM THE DATA IN J.A.SNELL AND S.N.LITTLE.
            //          1983.  PREDICTING CROWN WEIGHT AND BOLE VOLUME OF FIVE WESTERN
            //          HARDWOODS.GENERAL TECHNICAL REPORT PNW-151.
            float A1;
            float A2;
            float A3;
            float B0;
            float B1;
            switch (species)
            {
                case FiaCode.ArbutusMenziesii:
                    A1 = -0.2391F;
                    A2 = 2.951F;
                    A3 = -2.512F;
                    B0 = 0.09515994F;
                    B1 = 0.00247940F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    A1 = -0.4270F;
                    A2 = 2.348F;
                    A3 = -2.276F;
                    B0 = 0.06345756F;
                    B1 = 0.00208240F;
                    break;
                case FiaCode.QuercusGarryana:
                    A1 = -0.3741F;
                    A2 = 3.642F;
                    A3 = -3.406F;
                    B0 = 0.06345756F;
                    B1 = 0.00208240F;
                    break;
                case FiaCode.AlnusRubra:
                    A1 = -0.4280F;
                    A2 = 3.465F;
                    A3 = -3.269F;
                    B0 = 0.04969879F;
                    B1 = 0.00247940F;
                    break;
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    A1 = -0.3741F;
                    A2 = 3.642F;
                    A3 = -3.406F;
                    B0 = 0.04969879F;
                    B1 = 0.00247940F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            // CALCUATE STUMP VOLUME
            float DIBD = (float)Math.Pow(DIB / DIB1FT, 2.0F / 3.0F);
            float SDIB = (float)Math.Pow((4.5F - DIBD - CFSH * (1.0 - DIBD)) / 3.5F, 1.5F) * DIB1FT;
            if (SDIB < CFTD)
            {
                return 0.0F;
            };

            float CVTS = B0 + B1 * DBH * DBH * HT;
            float CVM = CVTS * (1.0F + A1 * (float)Math.Pow(CFTD, A2) * (float)Math.Pow(DBH, A3));
            if (CVM < 0.0F)
            {
                CVM = 0.0F;
            }

            // stump volume
            float CVS = 0.25F * (float)Math.PI * DIB1FT * DIB1FT * (1.0F / 6174.0F * ((float)Math.Pow(4.5F - DIBD, 3.0F) * CFSH -
                 1.5F * (4.5F - DIBD) * (4.5F - DIBD) * (1.0F - DIBD) * CFSH * CFSH +
                (4.5F - DIBD) * (1.0F - DIBD) * (1.0F - DIBD) * CFSH * CFSH * CFSH -
                 0.25F * (float)Math.Pow(1.0F - DIBD, 3.0F) * CFSH * CFSH * CFSH * CFSH));

            // CALCUATE TREE'S VOLUME
            float VALU = CVM - CVS;
            if (VALU < 0.0F)
            {
                VALU = 0.0F;
            }
            return VALU;
        }

        // same coefficients as SWO_TAPER
        private static float NWO_TAPER(FiaCode species, out float b11, out float b12, out float b13, out float b21)
        {
            float alpha;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    b11 = -0.55029801F;
                    b12 = -0.69479837F;
                    b13 = -0.0613100423F;
                    b21 = 0.35697451F;
                    alpha = 0.5F;
                    break;
                case FiaCode.AbiesGrandis:
                    b11 = -0.342017552F;
                    b12 = -0.777574201F;
                    b13 = -0.0433569876F;
                    b21 = 0.672963393F;
                    alpha = 0.33F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    b11 = -0.55029801F;
                    b12 = -0.69479837F;
                    b13 = -0.0613100423F;
                    b21 = 0.35697451F;
                    alpha = 0.5F;
                    break;
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    b11 = -0.596278066F;
                    b12 = -0.83987883F;
                    b13 = -0.0685768402F;
                    b21 = 0.134178717F;
                    alpha = 0.71F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            return alpha;
        }

        private static float RAP_CCFV(FiaCode species, float DBH, float HT, float CR, float DIB, float DIB1FT, float CFTD, float CFSH)
        {
            // CONIFER CUBIC MERCH VOLUME
            float A1;
            float A2;
            float A3;
            float A4;
            float A5;
            float EQNO1; // TODO: make bool
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            float B7;
            float EQNO2;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    A1 = -3.39101798F;
                    A2 = 0.918583494F;
                    A3 = 1.3330217F;
                    A4 = -0.935974246F;
                    A5 = 3.0F;
                    EQNO1 = 1.0F;
                    B1 = 1.24485628F;
                    B2 = 0.346490193F;
                    B3 = -0.56574969F;
                    B4 = 0.632239239F;
                    B5 = -0.152406551F;
                    B6 = 4.55802463F;
                    B7 = -0.051186711F;
                    EQNO2 = 1.0F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    A1 = 0.930057F;
                    A2 = 3.74152F;
                    A3 = 0.0F;
                    A4 = 0.0F;
                    A5 = 1.0F;
                    EQNO1 = 0.0F;
                    B1 = 1.168F;
                    B2 = 0.265430F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 0.0F;
                    break;
                case FiaCode.ThujaPlicata:
                    A1 = 0.930057F;
                    A2 = 1.23328561F;
                    A3 = 1.17859869F;
                    A4 = -0.45135743F;
                    A5 = 2.0F;
                    EQNO1 = 1.0F;
                    B1 = 0.90763281F;
                    B2 = 0.34284673F;
                    B3 = -0.63865388F;
                    B4 = 1.5857204F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 1.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            B1 /= 1000.0F;

            // CALCULATE CROWN RATIO ABOVE BREAST HEIGHT
            float HCB = HT - CR * HT;
            float CRABH;
            if (HCB > 4.5F)
            {
                CRABH = (HT - HCB) / (HT - 4.5F);
            }
            else
            {
                CRABH = 1.0F;
            }

            // CALCULATE VOLUME ABOVE BREAST HEIGHT
            float VABH = 0.0F;
            if (DIB > CFTD)
            {
                float RAT;
                if (EQNO1 < 0.5F)
                {
                    RAT = 1.0F - A1 * (float)Math.Pow(CFTD / DIB, A2);
                }
                else
                {
                    RAT = 1.0F - (float)(Math.Exp(A1 * Math.Pow(1.0F - CFTD / DIB, A2)) * Math.Pow(CFTD / DIB, A3 + A4 * Math.Pow(CR, A5)));
                }
                float X1 = (float)Math.Pow((HT - 4.5F) / DBH, B2 * Math.Pow(1.0F - Math.Pow(B3 * DBH, B4), EQNO2));
                float X2 = (float)Math.Pow(B5 * CRABH, B6);
                float X3 = (float)Math.Pow(DBH, B7);
                VABH = RAT * B1 * X1 * X2 * X3 * DBH * DBH * (HT - 4.5F);
            }

            // VOLUME BELOW BREAST HEIGHT
            float DIBD = (float)Math.Pow(DIB / DIB1FT, 2.0F / 3.0F);
            float SDIB = (float)Math.Pow((4.5F - DIBD - CFSH * (1.0F - DIBD)) / 3.5F, 1.5F) * DIB1FT;
            float VBBH = 0.0F;
            if (SDIB > CFTD)
            {
                VBBH = 0.25F * (float)Math.PI * DIB1FT * DIB1FT *
                           (1.0F / 43904.0F * (729.0F + 81.0F * DIBD +
                                297.0F * (float)Math.Pow(DIB / DIB1FT, 4.0F / 3.0F) +
                                265.0F * (DIB / DIB1FT) * (DIB / DIB1FT)) -
                       1.0F / 6174.0F * ((float)Math.Pow(4.5F - DIBD, 3.0F) * CFSH -
                                1.5F * (4.5F - DIBD) * (4.5F - DIBD) * (1.0F - DIBD) * CFSH * CFSH +
                                (4.5F - DIBD) * (1.0F - DIBD) * (1.0F - DIBD) * CFSH * CFSH * CFSH -
                                0.25F * (float)Math.Pow((1.0F - DIBD), 3.0F) * CFSH * CFSH * CFSH * CFSH));
            }

            // DETERMINE TREE'S VOLUME
            float VALU = VABH + VBBH;
            if (VALU < 0.0F)
            {
                VALU = 0.0F;
            }
            return VALU;
        }

        private static float RAP_DIB(FiaCode species, float DBH)
        {
            float E1;
            float E2;
            switch (species)
            {
                case FiaCode.AlnusRubra:
                    E1 = 0.947F;
                    E2 = 1.0F;
                    break;
                case FiaCode.PseudotsugaMenziesii:
                    E1 = 0.971330F;
                    E2 = 0.966365F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    E1 = 0.933707F;
                    E2 = 1.0F;
                    break;
                case FiaCode.ThujaPlicata:
                    E1 = 0.9497F;
                    E2 = 1.0F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    E1 = 0.97059F;
                    E2 = 0.993585F;
                    break;
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    E1 = 0.94448F;
                    E2 = 0.987517F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float DIB = E1 * (float)Math.Pow(DBH, E2);
            Debug.Assert(DIB > 0.0F);
            return DIB;
        }

        private static float RAP_DIB1FT(FiaCode species, float DBH, float CR)
        {
            float G0;
            float G1;
            float G2;
            float G3;
            float G4;
            switch (species)
            {
                case FiaCode.AlnusRubra:
                    G0 = 0.0F;
                    G1 = 1.0F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                case FiaCode.PseudotsugaMenziesii:
                    G0 = 0.149809111F;
                    G1 = 0.900790279F;
                    G2 = 0.133648456F;
                    G3 = 3.67532829F;
                    G4 = 1.0213663112F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    G0 = 0.0F;
                    G1 = 0.989819F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                case FiaCode.ThujaPlicata:
                    G0 = 0.451569966F;
                    G1 = 0.831752493F;
                    G2 = 0.216216295F;
                    G3 = 7.00446878F;
                    G4 = 1.0560026859F;
                    break;
                case FiaCode.AcerMacrophyllum:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    G0 = 0.0F;
                    G1 = 1.0F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float DIB1FT = G0 + G1 * (float)Math.Pow(G2 * CR, G3) * (float)Math.Pow(DBH, G4);
            return DIB1FT;
        }

        private static float RAP_HCFV(FiaCode species, float DBH, float HT, float DIB, float DIB1FT, float CFTD, float CFSH)
        {
            if (species == FiaCode.AlnusRubra)
            {
                // CUBIC FOOT VOLUME OF RED ALDER USING HIBBS, BLUHM, AND GARBER 2007
                return 0.04969879F + 0.00247940F * DBH * DBH * HT;
            }

            // CALCULATES CUBIC FOOT MERCH VOLUMES OF HARDWOODS USING
            // EQUATIONS DEVELOPED FROM THE DATA IN J.A.SNELL AND S.N.LITTLE.
            //          1983.  PREDICTING CROWN WEIGHT AND BOLE VOLUME OF FIVE WESTERN
            //          HARDWOODS.GENERAL TECHNICAL REPORT PNW-151.
            float A1;
            float A2;
            float A3;
            float B0;
            float B1;
            switch (species)
            {
                case FiaCode.AcerMacrophyllum:
                    A1 = -0.4270F;
                    A2 = 2.348F;
                    A3 = -2.276F;
                    B0 = 0.06345756F;
                    B1 = 0.00208240F;
                    break;
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    A1 = -0.3741F;
                    A2 = 3.642F;
                    A3 = -3.406F;
                    B0 = 0.04969879F;
                    B1 = 0.00247940F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            // CALCUATE STUMP VOLUME
            float DIBD = (float)Math.Pow(DIB / DIB1FT, 2.0F / 3.0F);
            float SDIB = (float)Math.Pow((4.5F - DIBD - CFSH * (1.0 - DIBD)) / 3.5F, 1.5F) * DIB1FT;
            if (SDIB < CFTD)
            {
                return 0.0F;
            };

            float CVTS = B0 + B1 * DBH * DBH * HT;
            float CVM = CVTS * (1.0F + A1 * (float)Math.Pow(CFTD, A2) * (float)Math.Pow(DBH, A3));
            if (CVM < 0.0F)
            {
                CVM = 0.0F;
            }

            // stump volume
            float CVS = 0.25F * (float)Math.PI * DIB1FT * DIB1FT * (1.0F / 6174.0F * ((float)Math.Pow(4.5F - DIBD, 3.0F) * CFSH -
                 1.5F * (4.5F - DIBD) * (4.5F - DIBD) * (1.0F - DIBD) * CFSH * CFSH +
                (4.5F - DIBD) * (1.0F - DIBD) * (1.0F - DIBD) * CFSH * CFSH * CFSH -
                 0.25F * (float)Math.Pow(1.0F - DIBD, 3.0F) * CFSH * CFSH * CFSH * CFSH));

            // CALCUATE TREE'S VOLUME
            float VALU = CVM - CVS;
            if (VALU < 0.0F)
            {
                VALU = 0.0F;
            }
            return VALU;
        }

        // same coefficients as SWO_TAPER
        // BUGBUG: doesn't support red alder though LOG_TABLE() expects it to!
        private static float RAP_TAPER(FiaCode species, out float b11, out float b12, out float b13, out float b21)
        {
            float alpha;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    b11 = -0.55029801F;
                    b12 = -0.69479837F;
                    b13 = -0.0613100423F;
                    b21 = 0.35697451F;
                    alpha = 0.5F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    b11 = -0.55029801F;
                    b12 = -0.69479837F;
                    b13 = -0.0613100423F;
                    b21 = 0.35697451F;
                    alpha = 0.5F;
                    break;
                case FiaCode.ThujaPlicata:
                    b11 = -0.596278066F;
                    b12 = -0.83987883F;
                    b13 = -0.0685768402F;
                    b21 = 0.134178717F;
                    alpha = 0.71F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            return alpha;
        }

        private static float SCRIB(Variant VERSION, FiaCode ISP, int SVOL, int LOGLL, float LOGTD, float LOGSH, float LOGTA,
                                   float LOGML, float DOB, float HT, float CR)
        {
            // VALUE = SCRIBNER VOLUME CALCULATED
            if (ISP > (FiaCode)300)
            {
                return 0.0F;
            }
            if (VERSION == Variant.Rap && ISP == FiaCode.AlnusRubra)
            {
                return RedAlder.RA_SCRIB(SVOL, LOGLL, LOGTD, LOGSH, LOGTA, LOGML, DOB, HT, CR);
            }

            float AA1;
            float AA2;
            float A3;
            float A4;
            float ALP;
            float PDIB;
            switch (VERSION)
            {
                case Variant.Swo:
                    PDIB = SWO_DIB(ISP, DOB, CR);
                    ALP = SWO_TAPER(ISP, out AA1, out AA2, out A3, out A4);
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    PDIB = NWO_DIB(ISP, DOB, CR);
                    ALP = NWO_TAPER(ISP, out AA1, out AA2, out A3, out A4);
                    break;
                case Variant.Rap:
                    PDIB = RAP_DIB(ISP, DOB);
                    ALP = RAP_TAPER(ISP, out AA1, out AA2, out A3, out A4);
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledVariantException(VERSION);
            }
            if (PDIB <= LOGTD)
            {
                return 0.0F;
            }

            float HCB = (1.0F - CR) * HT;
            float HABH = HT - 4.5F;
            float WLT = (ALP * HCB - 4.5F) / HABH;
            float PP1 = AA1 + AA2 * (float)Math.Exp(A3 * (HABH / DOB) * A3 * (HABH / DOB));
            float b21 = A4;

            //     COMPUTE MERCHANTABLE HEIGHT
            float D = 0.0F;
            float EX = 1.0F;
            float H = ALP * HCB;
            float TLL = 0.0F;
            float[,] NL = new float[40, 4];
            float[,] LVOL = new float[40, 4];
            float[,] TOTS = new float[2, 4];
            LOGVOL(1, SVOL, LOGLL, WLT, H, HABH, PP1, b21, PDIB, D, TLL, EX, out float DI, out float _, NL, LVOL, TOTS);
            float A;
            float B;
            float C;
            if (WLT <= 0.0F)
            {
                A = -(PP1 + 1.0F) / (HABH * HABH);
                B = PP1 / HABH;
                C = 1.0F - LOGTD / PDIB;
            }
            else if (WLT > 0.0F && DI <= LOGTD)
            {
                A = b21 / (HABH * HABH);
                B = PP1 / HABH;
                C = 1.0F - LOGTD / PDIB;
            }
            else
            {
                A = (b21 * WLT * WLT - PP1 - 2.0F * b21 * WLT - 1.0F) / (HABH * HABH * (WLT - 1.0F) * (WLT - 1.0F));
                B = ((2.0F * WLT - 1.0F + b21 * WLT * WLT + PP1 * WLT * WLT) - (b21 * WLT * WLT - PP1 - 2.0F * b21 * WLT - 1.0F)) / (HABH * (WLT - 1.0F) * (WLT - 1.0F));
                C = -(LOGTD / PDIB + (2.0F * WLT - 1.0F + b21 * WLT * WLT + PP1 * WLT * WLT) / (WLT - 1.0F) * (WLT - 1.0F));
            }

            float ROOT = B * B - 4 * A * C;
            float MH;
            if (ROOT < 0.0F)
            {
                MH = 0.0F;
                goto logVolumes;
            }

            float HM1 = (-B + (float)Math.Sqrt(ROOT)) / (2.0F * A);
            float HM2 = (-B - (float)Math.Sqrt(ROOT)) / (2.0F * A);
            if (HM1 > 0.0F && HM1 <= HABH)
            {
                H = HM1 + 4.5F;
                LOGVOL(4, SVOL, LOGLL, WLT, H, HABH, PP1, b21, PDIB, D, TLL, EX, out DI, out _, NL, LVOL, TOTS);
                int NDI = (int)Math.Round(10.0F * DI);
                int NLOGTD = (int)Math.Round(10.0F * LOGTD);
                if (NDI <= NLOGTD)
                {
                    HM1 = 0.0F;
                }
            }
            else
            {
                HM1 = 0.0F;
            }

            if (HM2 > 0.0F && HM2 <= HABH)
            {
                H = HM2 + 4.5F;
                LOGVOL(4, SVOL, LOGLL, WLT, H, HABH, PP1, b21, PDIB, D, TLL, EX, out DI, out _, NL, LVOL, TOTS);
                int NDI = (int)Math.Round(10.0F * DI);
                int NLOGTD = (int)Math.Round(10.0F * LOGTD);
                if (NDI != NLOGTD)
                {
                    HM2 = 0.0F;
                }
            }
            else
            {
                HM2 = 0.0F;
            }

            if (HM1 <= 0.0F || HM1 > HABH)
            {
                if (HM2 <= 0.0F || HM2 > HABH)
                {
                    MH = 0.0F;
                    goto logVolumes;
                }
                else
                {
                    MH = HM2;
                }
            }
            else if (HM2 < 0.0F || HM2 > HABH)
            {
                MH = HM1;
            }
            else
            {
                MH = Math.Max(HM1, HM2);
            }

            MH += 4.5F;

        logVolumes:
            // CALCULATE LOG VOLUMES
            int NW = (int)((MH - LOGSH) / ((float)LOGLL + LOGTA));
            if (NW < 0)
            {
                NW = 0;
            }
            TLL = MH - LOGSH - (float)NW * (float)LOGLL + LOGTA;

            H = LOGSH;
            float VALU = 0.0F;
            for (int II = 0; II < NW; ++II)
            {
                H = H + (float)LOGLL + LOGTA;
                LOGVOL(3, SVOL, LOGLL, WLT, H, HABH, PP1, b21, PDIB, 1.0F, TLL, EX, out _, out float VOLG, NL, LVOL, TOTS);
                VALU += VOLG;
            }

            // COMPUTE VOLUME OF TOP LOG
            if (TLL >= (LOGML + LOGTA))
            {
                int J = (int)(TLL - LOGTA);
                TLL = (float)J + LOGTA;
                D = TLL / (float)LOGLL;
                H += TLL;
                LOGVOL(3, SVOL, LOGLL, WLT, H, HABH, PP1, b21, PDIB, D, TLL, EX, out _, out float VOLG, NL, LVOL, TOTS);
                VALU += VOLG;
            }
            if (VALU < 0.0F)
            {
                VALU = 0.0F;
            }
            return VALU;
        }

        private static float SWO_CCFV(FiaCode species, float DBH, float HT, float CR, float DIB, float DIB1FT, float CFTD, float CFSH)
        {
            // CONIFER CUBIC MERCH VOLUME
            float A1;
            float A2;
            float A3;
            float A4;
            float A5;
            float EQNO1; // TODO: make bool
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            float B7;
            float EQNO2;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    A1 = -3.39101798F;
                    A2 = 0.918583494F;
                    A3 = 1.3330217F;
                    A4 = -0.935974246F;
                    A5 = 3.0F;
                    EQNO1 = 1.0F;
                    B1 = 1.24485628F;
                    B2 = 0.346490193F;
                    B3 = -0.56574969F;
                    B4 = 0.632239239F;
                    B5 = -0.152406551F;
                    B6 = 4.55802463F;
                    B7 = -0.051186711F;
                    EQNO2 = 1.0F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    A1 = -0.76519904F;
                    A2 = 0.25F;
                    A3 = 3.80136398F;
                    A4 = -1.7902001F;
                    A5 = 1.0F;
                    EQNO1 = 1.0F;
                    B1 = 1.33397259F;
                    B2 = 0.357808283F;
                    B3 = -0.755355039F;
                    B4 = 0.5F;
                    B5 = -0.261766125F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 1.0F;
                    break;
                case FiaCode.PinusPonderosa:
                    A1 = -4.87435933F;
                    A2 = 1.19484691F;
                    A3 = 0.634341265F;
                    A4 = 0.0F;
                    A5 = 1.0F;
                    EQNO1 = 1.0F;
                    B1 = 1.27677676F;
                    B2 = 0.162198194F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 0.0F;
                    break;
                case FiaCode.PinusLambertiana:
                    A1 = -4.87435933F;
                    A2 = 1.27588884F;
                    A3 = 0.63434126F;
                    A4 = 0.0F;
                    A5 = 1.0F;
                    EQNO1 = 1.0F;
                    B1 = 0.855844369F;
                    B2 = 0.388366991F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 0.0F;
                    break;
                case FiaCode.CalocedrusDecurrens:
                    A1 = -3.75729892F;
                    A2 = 1.23328561F;
                    A3 = 1.17859869F;
                    A4 = -0.45135743F;
                    A5 = 2.0F;
                    EQNO1 = 1.0F;
                    B1 = 0.90763281F;
                    B2 = 0.34284673F;
                    B3 = -0.63865388F;
                    B4 = 1.5857204F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 1.0F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    A1 = 0.930057F;
                    A2 = 3.74152F;
                    A3 = 0.0F;
                    A4 = 0.0F;
                    A5 = 0.0F;
                    EQNO1 = 0.0F;
                    B1 = 1.168F;
                    B2 = 0.265430F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 1.0F;
                    break;
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    A1 = 0.885038F;
                    A2 = 3.29655F;
                    A3 = 0.0F;
                    A4 = 0.0F;
                    A5 = 0.0F;
                    EQNO1 = 0.0F;
                    B1 = 0.887F;
                    B2 = 0.367622F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = 1.0F;
                    B7 = 0.0F;
                    EQNO2 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            B1 /= 1000.0F;

            // CALCULATE CROWN RATIO ABOVE BREAST HEIGHT
            float HCB = HT - CR * HT;
            float CRABH;
            if (HCB > 4.5F)
            {
                CRABH = (HT - HCB) / (HT - 4.5F);
            }
            else
            {
                CRABH = 1.0F;
            }

            // CALCULATE VOLUME ABOVE BREAST HEIGHT
            float VABH = 0.0F;
            if (DIB > CFTD)
            {
                float RAT;
                if (EQNO1 < 0.5F)
                {
                    RAT = 1.0F - A1 * (float)Math.Pow(CFTD / DIB, A2);
                }
                else
                {
                    RAT = 1.0F - (float)(Math.Exp(A1 * Math.Pow(1.0F - CFTD / DIB, A2)) * Math.Pow(CFTD / DIB, A3 + A4 * Math.Pow(CR, A5)));
                }
                float X1 = (float)Math.Pow((HT - 4.5F) / DBH, B2 * Math.Pow(1.0F - Math.Exp(Math.Pow(B3 * DBH, B4)), EQNO2));
                float X2 = (float)Math.Exp(Math.Pow(B5 * CRABH, B6));
                float X3 = (float)Math.Pow(DBH, B7);
                VABH = RAT * B1 * X1 * X2 * X3 * DBH * DBH * (HT - 4.5F);
            }

            // VOLUME BELOW BREAST HEIGHT
            float DIBD = (float)Math.Pow(DIB / DIB1FT, 2.0F / 3.0F);
            float SDIB = (float)Math.Pow((4.5F - DIBD - CFSH * (1.0F - DIBD)) / 3.5F, 1.5F) * DIB1FT;
            float VBBH = 0.0F;
            if (SDIB > CFTD)
            {
                VBBH = 0.25F * (float)Math.PI * DIB1FT * DIB1FT *
                           (1.0F / 43904.0F * (729.0F + 81.0F * DIBD +
                                297.0F * (float)Math.Pow(DIB / DIB1FT, 4.0F / 3.0F) +
                                265.0F * (DIB / DIB1FT) * (DIB / DIB1FT)) -
                       1.0F / 6174.0F * ((float)Math.Pow(4.5F - DIBD, 3.0F) * CFSH -
                                1.5F * (4.5F - DIBD) * (4.5F - DIBD) * (1.0F - DIBD) * CFSH * CFSH +
                                (4.5F - DIBD) * (1.0F - DIBD) * (1.0F - DIBD) * CFSH * CFSH * CFSH -
                                0.25F * (float)Math.Pow((1.0F - DIBD), 3.0F) * CFSH * CFSH * CFSH * CFSH));
            }

            // DETERMINE TREE'S VOLUME
            float VALU = VABH + VBBH;
            if (VALU < 0.0F)
            {
                VALU = 0.0F;
            }
            return VALU;
        }

        private static float SWO_DIB(FiaCode species, float DBH, float CR)
        {
            float E1;
            float E2;
            float E3;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    E1 = 0.92443655F;
                    E2 = 0.98886654F;
                    E3 = -0.03414550F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    E1 = 0.92162494F;
                    E2 = 1.0F;
                    E3 = -0.03415396F;
                    break;
                case FiaCode.PinusPonderosa:
                    E1 = 0.80860026F;
                    E2 = 1.01742589F;
                    E3 = 0.0F;
                    break;
                case FiaCode.PinusLambertiana:
                    E1 = 0.85897904F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.CalocedrusDecurrens:
                    E1 = 0.87875535F;
                    E2 = 1.0F;
                    E3 = -0.07696055F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    E1 = 0.933707F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.ThujaPlicata:
                    E1 = 0.9497F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.TaxusBrevifolia:
                    E1 = 0.97F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.ArbutusMenziesii:
                    E1 = 0.96317F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    E1 = 0.94448F;
                    E2 = 0.9875170F;
                    E3 = 0.0F;
                    break;
                case FiaCode.NotholithocarpusDensiflorus:
                    E1 = 0.859151F;
                    E2 = 1.0178109F;
                    E3 = 0.0F;
                    break;
                case FiaCode.QuercusChrysolepis:
                    E1 = 0.910499F;
                    E2 = 1.01475F;
                    E3 = 0.0F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    E1 = 0.97059F;
                    E2 = 0.993585F;
                    E3 = 0.0F;
                    break;
                case FiaCode.QuercusGarryana:
                    E1 = 0.878457F;
                    E2 = 1.02393F;
                    E3 = 0.0F;
                    break;
                case FiaCode.QuercusKelloggii:
                    E1 = 0.889703F;
                    E2 = 1.0104062F;
                    E3 = 0.0F;
                    break;
                case FiaCode.AlnusRubra:
                    E1 = 0.947F;
                    E2 = 1.0F;
                    E3 = 0.0F;
                    break;
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    E1 = 0.94448F;
                    E2 = 0.9875170F;
                    E3 = 0.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float DIB = E1 * (float)Math.Pow(DBH, E2) * (float)Math.Exp(E3 * Math.Sqrt((1.0F - CR)));
            Debug.Assert(DIB > 0.0F);
            return DIB;
        }

        private static float SWO_DIB1FT(FiaCode species, float DBH, float CR)
        {
            float G0;
            float G1;
            float G2;
            float G3;
            float G4;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    G0 = 0.149809111F;
                    G1 = 0.900790279F;
                    G2 = 0.133648456F;
                    G3 = 3.67532829F;
                    G4 = 1.0213663112F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    G0 = 0.393048214F;
                    G1 = 0.729932627F;
                    G2 = 0.120814754F;
                    G3 = 1.0F;
                    G4 = 1.097851010F;
                    break;
                case FiaCode.PinusPonderosa:
                    G0 = 0.0F;
                    G1 = 1.0F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                case FiaCode.PinusLambertiana:
                    G0 = 0.0F;
                    G1 = 1.04030514F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                case FiaCode.CalocedrusDecurrens:
                    G0 = 0.451569966F;
                    G1 = 0.831752493F;
                    G2 = 0.216216295F;
                    G3 = 7.00446878F;
                    G4 = 1.0560026859F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    G0 = 0.0F;
                    G1 = 0.989819F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                case FiaCode.ThujaPlicata:
                    G0 = 0.451569966F;
                    G1 = 0.831752493F;
                    G2 = 0.216216295F;
                    G3 = 7.00446878F;
                    G4 = 1.056002686F;
                    break;
                case FiaCode.TaxusBrevifolia:
                    G0 = 0.451569966F;
                    G1 = 0.831752493F;
                    G2 = 0.216216295F;
                    G3 = 7.00446878F;
                    G4 = 1.056002686F;
                    break;
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
                    G0 = 0.0F;
                    G1 = 1.0F;
                    G2 = 0.0F;
                    G3 = 1.0F;
                    G4 = 1.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            float DIB1FT = G0 + G1 * (float)Math.Pow(G2 * CR, G3) * (float)Math.Pow(DBH, G4);
            return DIB1FT;
        }

        private static float SWO_HCFV(FiaCode species, float DBH, float HT, float DIB, float DIB1FT, float CFTD, float CFSH)
        {
            // CALCULATES CUBIC FOOT MERCH VOLUMES OF HARDWOODS USING
            // EQUATIONS DEVELOPED FROM THE DATA IN J.A.SNELL AND S.N.LITTLE.
            //          1983.  PREDICTING CROWN WEIGHT AND BOLE VOLUME OF FIVE WESTERN
            //          HARDWOODS.GENERAL TECHNICAL REPORT PNW-151.
            float A1;
            float A2;
            float A3;
            float B0;
            float B1;
            switch (species)
            {
                case FiaCode.ArbutusMenziesii:
                    A1 = -0.2391F;
                    A2 = 2.951F;
                    A3 = -2.512F;
                    B0 = 0.09515994F;
                    B1 = 0.00247940F;
                    break;
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                    A1 = -0.3741F;
                    A2 = 3.642F;
                    A3 = -3.406F;
                    B0 = 0.04969879F;
                    B1 = 0.00247940F;
                    break;
                case FiaCode.NotholithocarpusDensiflorus:
                    A1 = -0.2792F;
                    A2 = 3.038F;
                    A3 = -2.603F;
                    B0 = 0.06345756F;
                    B1 = 0.00208240F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    A1 = -0.4270F;
                    A2 = 2.348F;
                    A3 = -2.276F;
                    B0 = 0.06345756F;
                    B1 = 0.00208240F;
                    break;
                case FiaCode.QuercusChrysolepis:
                case FiaCode.QuercusGarryana:
                case FiaCode.QuercusKelloggii:
                    A1 = -0.3741F;
                    A2 = 3.642F;
                    A3 = -3.406F;
                    B0 = 0.06345756F;
                    B1 = 0.00208240F;
                    break;
                case FiaCode.AlnusRubra:
                    A1 = -0.4280F;
                    A2 = 3.465F;
                    A3 = -3.269F;
                    B0 = 0.04969879F;
                    B1 = 0.00247940F;
                    break;
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    A1 = -0.3741F;
                    A2 = 3.642F;
                    A3 = -3.406F;
                    B0 = 0.04969879F;
                    B1 = 0.00247940F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            // CALCUATE STUMP VOLUME
            float DIBD = (float)Math.Pow(DIB / DIB1FT, 2.0F / 3.0F);
            float SDIB = (float)Math.Pow((4.5F - DIBD - CFSH * (1.0 - DIBD)) / 3.5F, 1.5F) * DIB1FT;
            if (SDIB < CFTD)
            {
                return 0.0F;
            };

            float CVTS = B0 + B1 * DBH * DBH * HT;
            float CVM = CVTS * (1.0F + A1 * (float)Math.Pow(CFTD, A2) * (float)Math.Pow(DBH, A3));
            if (CVM < 0.0F)
            {
                CVM = 0.0F;
            }

            // stump volume
            float CVS = 0.25F * (float)Math.PI * DIB1FT * DIB1FT * (1.0F / 6174.0F * ((float)Math.Pow(4.5F - DIBD, 3.0F) * CFSH -
                 1.5F * (4.5F - DIBD) * (4.5F - DIBD) * (1.0F - DIBD) * CFSH * CFSH +
                (4.5F - DIBD) * (1.0F - DIBD) * (1.0F - DIBD) * CFSH * CFSH * CFSH -
                 0.25F * (float)Math.Pow(1.0F - DIBD, 3.0F) * CFSH * CFSH * CFSH * CFSH));

            // CALCUATE TREE'S VOLUME
            float VALU = CVM - CVS;
            if (VALU < 0.0F)
            {
                VALU = 0.0F;
            }
            return VALU;
        }

        private static float SWO_TAPER(FiaCode species, out float b11, out float b12, out float b13, out float b21)
        {
            float alpha;
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    b11 = -0.55029801F;
                    b12 = -0.69479837F;
                    b13 = -0.0613100423F;
                    b21 = 0.35697451F;
                    alpha = 0.5F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    b11 = -0.342017552F;
                    b12 = -0.777574201F;
                    b13 = -0.0433569876F;
                    b21 = 0.672963393F;
                    alpha = 0.33F;
                    break;
                case FiaCode.PinusPonderosa:
                    b11 = -0.595823501F;
                    b12 = -1.25803662F;
                    b13 = -0.13867406F;
                    b21 = 0.0998711245F;
                    alpha = 0.6F;
                    break;
                case FiaCode.PinusLambertiana:
                    b11 = -0.6F;
                    b12 = -0.48435806F;
                    b13 = -0.033249206F;
                    b21 = 0.10862035F;
                    alpha = 0.74F;
                    break;
                case FiaCode.CalocedrusDecurrens:
                    b11 = -0.596278066F;
                    b12 = -0.83987883F;
                    b13 = -0.00685768402F;
                    b21 = 0.134178717F;
                    alpha = 0.71F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    b11 = -0.55029801F;
                    b12 = -0.69479837F;
                    b13 = -0.0613100423F;
                    b21 = 0.35697451F;
                    alpha = 0.5F;
                    break;
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    b11 = -0.596278066F;
                    b12 = -0.83987883F;
                    b13 = -0.0685768402F;
                    b21 = 0.134178717F;
                    alpha = 0.71F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            return alpha;
        }

        /// <summary>
        /// Get board foot and cubic foot volumes for a tree.
        /// </summary>
        /// <param name="VERSION">Organon variant.</param>
        /// <param name="SPP">Tree species.</param>
        /// <param name="CFTD">Cubic foot? merchantable top diameter (inches).</param>
        /// <param name="CFSH">Cubic foot? stump cut height (feet).</param>
        /// <param name="LOGLL">Default log length (feet). Valid range is 8-40 feet.</param>
        /// <param name="LOGML">Minimum merchantable log length (feet).</param>
        /// <param name="LOGTD">Sawlog? merchantable top diameter (inches).</param>
        /// <param name="LOGSH">Sawlog? stump cut height (feet).</param>
        /// <param name="LOGTA"></param>
        /// <param name="DBH">Tree diameter at breast height (inches).</param>
        /// <param name="HT">Tree height (feet).</param>
        /// <param name="CR">Tree crown ratio.</param>
        /// <param name="VERROR">Has elements set to 1 if there is a problem with CFSH, LOGML, LOGLL, LOGTD, LOGSH, or LOGTA. 0 otherwise.</param>
        /// <param name="VWARNING">Has elemetns set if CFTD, LOGTD, or LOGTA exceeds 12 or if LOGML is out of range.</param>
        /// <param name="TWARNING">1 if a tree exceeds its potential height, 0 otherwise.</param>
        /// <param name="CFVOL">Merchantable cubic foot volume.</param>
        /// <param name="BFVOL">Scribner board foot volume.</param>
        /// <returns>True if successful, false if an error was encountered.</returns>
        public static bool VOLCAL(Variant VERSION, FiaCode SPP, float CFTD, float CFSH, float LOGLL, float LOGML, float LOGTD, float LOGSH,
                                  float LOGTA, float DBH, float HT, float CR, out int[] VERROR, out int[] VWARNING,
                                  out int TWARNING, out float CFVOL, out float BFVOL)
        {
            // IMPLICIT NONE
            // INTEGER*4   VERSION,IERROR,SPP
            // INTEGER*4   SPGRP,VERROR(5),TERROR(4),VWARNING(5),TWARNING,
            //1            ILOGLL,VERS,SVOL
            // REAL*4      CFTD,CFSH,LOGLL,LOGML,LOGTD,LOGSH,LOGTA,DBH,HT,CR,
            //1            XLOGTA,CFVOL,BFVOL
            // LOGICAL*2   ERROR
            if (CFTD < 0.0F)
            {
                CFTD = 0.0F;
            }
            if (CFSH < 0.0F)
            {
                CFSH = 0.0F;
            }
            if (LOGTD <= 0.0F)
            {
                LOGTD = 6.0F;
            }
            if (LOGSH <= 0.0F)
            {
                LOGSH = 0.5F;
            }
            if (LOGTA <= 0.0F)
            {
                LOGTA = 8.0F;
            }
            if (LOGLL <= 0.0F)
            {
                LOGLL = 32.0F;
            }
            if (LOGML <= 0.0F)
            {
                LOGML = 8.0F;
            }
            bool ERROR = EDIT(VERSION, SPP, CFTD, CFSH, LOGLL, LOGML, LOGTD, LOGSH, LOGTA, DBH, HT, out VERROR, out VWARNING, out TWARNING);
            if (ERROR)
            {
                CFVOL = -1.0F;
                BFVOL = -1.0F;
                return false;
            }

            // CALCULATE CF VOLUME FOR TREE
            CFVOL = CF(VERSION, SPP, DBH, HT, CR, CFTD, CFSH);
            int SVOL = 1;
            int ILOGLL = (int)LOGLL;
            float XLOGTA = LOGTA / 12.0F;
            BFVOL = SCRIB(VERSION, SPP, SVOL, ILOGLL, LOGTD, LOGSH, XLOGTA, LOGML, DBH, HT, CR);
            return true;
        }
    }
}
