using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    public class Execute2
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CYCLG"></param>
        /// <param name="VERSION"></param>
        /// <param name="NPTS">(DOUG? Number of plots?)</param>
        /// <param name="NTREES1"></param>
        /// <param name="STAGE"></param>
        /// <param name="BHAGE"></param>
        /// <param name="TREENO"></param>
        /// <param name="PTNO"></param>
        /// <param name="SPECIES"></param>
        /// <param name="USER"></param>
        /// <param name="INDS"></param>
        /// <param name="DBH1"></param>
        /// <param name="HT1"></param>
        /// <param name="CR1"></param>
        /// <param name="SCR1"></param>
        /// <param name="EXPAN1"></param>
        /// <param name="MGEXP"></param>
        /// <param name="RVARS"></param>
        /// <param name="ACALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YSF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YST"></param>
        /// <param name="NPR"></param>
        /// <param name="PRAGE"></param>
        /// <param name="PRLH"></param>
        /// <param name="PRDBH"></param>
        /// <param name="PRHT"></param>
        /// <param name="PRCR"></param>
        /// <param name="PREXP"></param>
        /// <param name="BRCNT"></param>
        /// <param name="BRHT"></param>
        /// <param name="BRDIA"></param>
        /// <param name="JCOR"></param>
        /// <param name="SERROR"></param>
        /// <param name="TERROR"></param>
        /// <param name="SWARNING"></param>
        /// <param name="TWARNING"></param>
        /// <param name="IERROR"></param>
        /// <param name="DGRO"></param>
        /// <param name="HGRO"></param>
        /// <param name="CRCHNG"></param>
        /// <param name="SCRCHNG"></param>
        /// <param name="MORTEXP"></param>
        /// <param name="NTREES2"></param>
        /// <param name="DBH2"></param>
        /// <param name="HT2"></param>
        /// <param name="CR2"></param>
        /// <param name="SCR2"></param>
        /// <param name="EXPAN2"></param>
        /// <param name="STOR"></param>
        public static void EXECUTE(int CYCLG, Variant VERSION, int NPTS, int NTREES1, ref int STAGE, ref int BHAGE, int[] TREENO,
                                   int[] PTNO, int[] SPECIES, int[] USER, int[] INDS, float[] DBH1, float[] HT1, float[] CR1, float[] SCR1,
                                   float[] EXPAN1, float[] MGEXP, float[] RVARS, float[,] ACALIB, float[] PN, float[] YSF, float BABT, float[] BART, float[] YST,
                                   int[] NPR, int[,] PRAGE, float[,] PRLH, float[,] PRDBH, float[,] PRHT, float[,] PRCR, float[,] PREXP, int[,] BRCNT,
                                   int[,] BRHT, int[,] BRDIA, int[,] JCOR, int[] SERROR, int[,] TERROR, int[] SWARNING,
                                   int[] TWARNING, int IERROR, float[] DGRO, float[] HGRO, float[] CRCHNG, float[] SCRCHNG,
                                   float[] MORTEXP, out int NTREES2, float[] DBH2, float[] HT2, float[] CR2, float[] SCR2, float[] EXPAN2, float[] STOR)
        {
            // SET THE MAXIMUM TREE LEVEL ARRAY SIZES
            int NTREES = NTREES1;
            int YCYCLG = 5 * CYCLG;
            bool CALH = false;
            bool CALC = false;
            bool CALD = false;
            bool EVEN = false;
            bool TRIPLE = false;
            bool PRUNE = false;
            bool THIN = false;
            bool FERT = false;
            bool MORT = false;
            bool POST = false;
            bool OSTORY = false;
            bool INGRO = false;
            bool B6THIN = false;
            bool GENETICS = false;
            bool SWISSNC = false;

            if (INDS[0] == 1)
            {
                CALH = true;
            }
            if (INDS[1] == 1)
            {
                CALC = true;
            }
            if (INDS[2] == 1)
            {
                CALD = true;
            }
            if (INDS[3] == 1)
            {
                EVEN = true;
            }
            if (INDS[4] == 1)
            {
                TRIPLE = true;
            }
            if (INDS[5] == 1)
            {
                PRUNE = true;
            }
            if (INDS[6] == 1)
            {
                THIN = true;
            }
            if (INDS[7] == 1)
            {
                FERT = true;
            }
            if (INDS[8] == 1)
            {
                MORT = true;
            }
            // INDS[9] sets wood quality below
            if (INDS[10] == 1)
            {
                OSTORY = true;
            }
            if (INDS[11] == 1)
            {
                INGRO = true;
            }
            if (INDS[12] == 1)
            {
                B6THIN = true;
            }
            if (INDS[13] == 1)
            {
                GENETICS = true;
            }
            if (INDS[14] == 1)
            {
                SWISSNC = true;
            }

            float SITE_1 = RVARS[0];
            float SITE_2 = RVARS[1];
            float MSDI_1 = RVARS[2];
            float MSDI_2 = RVARS[3];
            float MSDI_3 = RVARS[4];
            float GWDG = RVARS[5];
            float GWHG = RVARS[6];
            float FR = RVARS[7];
            float PDEN = RVARS[8];
            float NO = STOR[0];
            float RD0 = STOR[1];
            // BUGBUG: values of A1 and A2 are overwritten by calls to SUBMAX and therefore silently ignored in certain cases
            float A1 = STOR[2];
            float A2 = STOR[3];
            float A1MAX = STOR[4];
            float PA1MAX = STOR[5];

            int[] SPGRP = new int[2000];
            EDIT(CYCLG, VERSION, NPTS, NTREES, STAGE, BHAGE, SPECIES, CALH, CALC,
                CALD, EVEN, PRUNE, THIN, FERT, MORT, GENETICS, SWISSNC, DBH1,
                HT1, CR1, EXPAN1, SITE_1, SITE_2, MSDI_1, MSDI_2, MSDI_3, PDEN,
                ACALIB, PN, YSF, BABT, BART, YST, SCR1, MGEXP, GWDG, GWHG, FR, out int IB,
                out int NSPN, out int BIG6, out int OTHER, out int BNXT, out int ONXT, SPGRP, SERROR, TERROR, SWARNING,
                TWARNING, out bool ERROR);
            // BUGBUG dead Fortran code IERROR = 0;
            if (ERROR)
            {
                // BUGBUG dead Fortran code IERROR = 1;
                NTREES2 = -1;
                return;
            }

            float NXT = BNXT + ONXT;
            // BUGBUG dead Fortran code 
            //float IDXAGE;
            //float IDXCYC;
            //if (VERSION == Version.Swo)
            //{
            // IDXAGE = 500F;
            // IDXCYC = 100F;
            //}
            //else if (VERSION == Version.Nwo || VERSION == Version.Smc)
            //{
            // IDXAGE = 120F;
            // IDXCYC = 24F;
            //}
            //else
            //{
            // IDXAGE = 30F;
            // IDXCYC = 30F;
            //}

            int FCYCLE = 0;
            int TCYCLE = 0;
            if (FERT && YSF[0] == (float)YCYCLG)
            {
                FCYCLE = 1;
            }

            if (THIN && YST[0] == (float)YCYCLG)
            {
                TCYCLE = 1;
                POST = true;
            }

            bool WOODQ = false;
            if (INDS[9] == 1) 
            {
                WOODQ = true;
            }
            bool TRIAL = false;

            float[,] GROWTH = new float[2000, 4];
            float[] DEADEXP = new float[2000];
            float[,] SCR = new float[2000, 3];
            float[,] SYTVOL = new float[2000, 2];
            float[,] TDATAR = new float[2000, 8];
            int[,] TDATAI = new int[2000, 3];
            float[,] VOLTR = new float[2000, 4];
            for (int I = 0; I < 2000; ++I)
            {
                GROWTH[I, 2] = 0.0F;
                GROWTH[I, 3] = 0.0F;
                TDATAI[I, 0] = 0;
                TDATAI[I, 1] = 0;
                TDATAI[I, 2] = 0;
                TDATAR[I, 0] = 0.0F;
                TDATAR[I, 1] = 0.0F;
                TDATAR[I, 2] = 0.0F;
                TDATAR[I, 3] = 0.0F;
                SCR[I, 0] = 0.0F;
                SCR[I, 1] = 0.0F;
                SCR[I, 2] = 0.0F;
                DEADEXP[I] = 0.0F;
                // BUGBUG doesn't check passed length of DBH2, HT2, CR2, EXPAN2, or SCR2
                DBH2[I] = 0.0F;
                HT2[I] = 0.0F;
                CR2[I] = 0.0F;
                EXPAN2[I] = 0.0F;
                SCR2[I] = 0.0F;
                for (int II = 0; II < 2; ++II) 
                {
                   SYTVOL[I, II] = 0.0F;
                }
                for (int II = 0; II < 4; ++II)
                {
                   VOLTR[I, II] = 0.0F;
                }
            }

            for (int I = 0; I < NTREES; ++I)
            {
                TDATAI[I, 0] = SPECIES[I];
                TDATAI[I, 1] = SPGRP[I];
                TDATAI[I, 2] = USER[I];
                TDATAR[I, 0] = DBH1[I];
                TDATAR[I, 1] = HT1[I];
                TDATAR[I, 2] = CR1[I];
                TDATAR[I, 3] = EXPAN1[I];
                SCR[I, 0] = SCR1[I];
            }

            float[,] CALIB = new float[18, 6];
            for (int I = 0; I < 18; ++I)
            {
                CALIB[I, 3] = ACALIB[I, 0];
                CALIB[I, 4] = ACALIB[I, 1];
                CALIB[I, 5] = ACALIB[I, 2];
                if (CALH)
                {
                    CALIB[I, 0] = (1.0F + CALIB[I, 3]) / 2.0F + (float)Math.Pow(0.5, 0.5 * CYCLG) * ((CALIB[I, 3] - 1.0F) / 2.0F);
                }
                else 
                {
                    CALIB[I, 0] = 1.0F;
                }
                if (CALC)
                {
                    CALIB[I, 1] = (1.0F + CALIB[I, 4]) / 2.0F + (float)Math.Pow(0.5F, 0.5F * CYCLG) * ((CALIB[I, 4] - 1.0F) / 2.0F);
                }
                else 
                {
                    CALIB[I, 1] = 1.0F;
                }
                if (CALD)
                {
                    CALIB[I, 2] = (1.0F + CALIB[I, 6]) / 2.0F + (float)Math.Pow(0.5F, 0.5F * CYCLG) * ((CALIB[I, 6] - 1.0F) / 2.0F);
                }
                else 
                {
                    CALIB[I, 2] = 1.0F;
                }
            }

            float[] YF = new float[5];
            float[] YT = new float[5];
            for (int I = 0; I < 5; ++I)
            {
                YF[I] = YSF[I];
                YT[I] = YST[I];
            }

            // CALCULATE SPECIES GROUP
            //
            // SPGROUP(VERSION, NSPN, NTREES, TDATAI)
            //
            // BUGBUG MAXRAH, RASI, and RAAGE not initialized in Fortran
            // BUGBUG SI_1 not intialized in Fortran code for Nwo and Smc calculation of SITE_2 in else { }
            float MAXRAH = 0.0F;
            float RASI = 0.0F;
            float RAAGE = 0.0F;
            float SI_1 = 0.0F;
            if (VERSION == Variant.Swo)
            {
                if (SITE_1 < 0.0F && SITE_2 > 0.0F)
                {
                    SITE_1 = 1.062934F * SITE_2;
                }
                else if (SITE_2 < 0.0F)
                {
                    SITE_2 = 0.940792F * SITE_1;
                }
            }
            else if (VERSION == Variant.Nwo || VERSION == Variant.Smc)
            {
                // Site index conversion equation from Nigh(1995, Forest Science 41:84-98)
                if (SITE_1 < 0.0F && SITE_2 > 0.0F)
                {
                    SITE_1 = 0.480F + (1.110F * SITE_2);
                }
                else if (SITE_2 < 0.0F)
                {
                    SITE_2 = -0.432F + (0.899F * SITE_1);
                }
                else
                {
                    if (SITE_2 < 0.0F)
                    {
                        SITE_2 = 4.776377F * (float)Math.Pow(SI_1, 0.763530587);
                    }
                }

                // CHANGE TO EXPANSION FACTOR FOR STAND--NOT SAMPLE
                for (int J = 0; J < NTREES; ++J)
                {
                    TDATAR[J, 3] = TDATAR[J, 3] / (float)NPTS;
                    MGEXP[J] = MGEXP[J] / (float)NPTS;
                    TDATAR[J, 4] = TDATAR[J, 3];

                    if (PRUNE)
                    {
                        TDATAR[J, 5] = SCR[J, 0];
                    }
                    else
                    {
                        TDATAR[J, 5] = TDATAR[J, 2];
                    }
                    if (SCR[J, 0] > 0.0F)
                    {
                        SCR[J, 1] = SCR[J, 0];
                    }
                }

                // CALCULATE RED ALDER SITE INDEX FOR TREES IN NATURAL STANDS
                if (VERSION < Variant.Smc)
                {
                    for (int J = 0; J < NTREES; ++J)
                    {
                        if (TDATAI[J, 0] == 351 && TDATAR[J, 1] > MAXRAH)
                        {
                            MAXRAH = TDATAR[J, 1];
                        }
                    }
                    // BUGBUG SITE_1 is the site index from ground but the code in CON_RASI appears to expect the breast height site index (SI_1)
                    RASI = Stats.CON_RASI(SITE_1);
                }

                // CALCULATE RED ALDER AGE FOR NATURAL STANDS
                // BUGBUG RAAGE not initialized for MAXRAH <= 0.0 in Fortran code
                if (MAXRAH > 0.0F)
                {
                    HeightGrowth.RAGEA(MAXRAH, RASI, out RAAGE);
                }
                if (RAAGE < 0.0F)
                {
                    RAAGE = 55.0F;
                    Stats.RASITE(MAXRAH, RAAGE, out RASI);
                }
                if (RAAGE > 55.0F)
                {
                    RAAGE = 55.0F;
                }
            }

            // BUGBUG no check that SITE_1 and SITE_2 indices are greater than 4.5 feet
            SI_1 = SITE_1 - 4.5F;
            float SI_2 = SITE_2 - 4.5F;

            // INITIALIZE A1 AND A2
            if (CYCLG < 0)
            {
                Submax.SUBMAX(TRIAL, VERSION, NTREES, TDATAI, TDATAR, MGEXP, MSDI_1, MSDI_2, MSDI_3, out A1, out A2);
            }
            else if (OSTORY || INGRO || B6THIN) 
            {
                Submax.SUBMAX(TRIAL, VERSION, NTREES, TDATAI, TDATAR, MGEXP, MSDI_1, MSDI_2, MSDI_3, out A1, out A2);
            }

            // CALCULATE DENSITY VARIABLES AT SOG
            float SBA1;
            float TPA1;
            float SCCF1;
            float[] BAL1 = new float[500];
            float[] BALL1 = new float[51];
            float[] CCFL1 = new float[500];
            float[] CCFLL1 = new float[51];
            Stats.SSTATS(VERSION, NTREES, TDATAI, TDATAR, out SBA1, out TPA1, out SCCF1, BAL1, BALL1, CCFL1, CCFLL1);
            float CON = 0.005454154F;
            float QMD1 = (float)Math.Sqrt(SBA1 / (CON * TPA1));
            float RD1 = TPA1 / (float)Math.Exp(A1 / A2 - Math.Log(QMD1) / A2);

            // CALCULATE H40 AND MAXIMUM CROWN CLOSURE AT SOG
            Stats.HTFORTY(0.0F, VERSION, IB, NTREES, TDATAI, TDATAR, MGEXP, out float HT40);
            float CCMAX;
            if (HT40 < 140.0F)
            {
                CCMAX = 100.0F;
            }
            else 
            {
                CCMAX = 121.0F - 0.15F * HT40;
            }
            if (RD1 < 0.4)
            {
                CCMAX = CCMAX * 0.6F;
            }
            else 
            { 
                if (RD1 >= 0.4F && RD1 < 0.8F) 
                {
                    CCMAX = CCMAX * (0.2F + RD1);
                }
            }

            // CALCULATE CCH AND CROWN CLOSURE AT SOG
            float[] CCH = new float[41];
            CrownGrowth.CRNCLO(0, 0.0F, VERSION, NTREES, TDATAI, TDATAR, SCR, MGEXP, CCH, out float CC);
            float OLD = 0.0F;
            for (int I = 0; I < NTREES; ++I)
            {
                TDATAR[I, 7] = TDATAR[I, 3];

                if (PRUNE)
                {
                    TDATAR[I, 6] = SCR[I, 0];
                }
                else 
                {
                    TDATAR[I, 6] = TDATAR[I, 2];
                }
                if (SCR[I, 0] > 0.0F)
                {
                    SCR[I, 2] = SCR[I, 0];
                }
            }

            float[] CCFLL2 = new float[51];
            float[] CCFL2 = new float[500];
            float[] BALL2 = new float[51];
            float[] BAL2 = new float[500];
            Grow.GROW(VERSION, ref CYCLG, NTREES, IB, BIG6, OTHER, NSPN, ref STAGE, ref BHAGE, PTNO, TREENO, TDATAI, PRAGE, BRCNT, BRHT, BRDIA, JCOR, NPR,
                    ref TCYCLE, ref FCYCLE, TRIPLE, WOODQ, POST, MORT, GENETICS, SWISSNC,
                    TDATAR, SI_1, SI_2, SBA1, BALL1, BAL1, CALIB, PN, YF, BABT, BART,
                    YT, GROWTH, PRLH, PRDBH, PRHT, PRCR, PREXP, SCR, VOLTR, SYTVOL,
                    CCH, ref OLD, MGEXP, DEADEXP, A1, A2, A1MAX, PA1MAX, NO, RD0, RAAGE,
                    RASI, CCFLL1, CCFL1, out float SBA2, CCFLL2, CCFL2, BALL2, BAL2, out float TPA2,
                    out float SCCF2, GWDG, GWHG, FR, PDEN);
            NTREES2 = NTREES;
            POST = false;

            if (EVEN == false)
            {
                BHAGE = 0;
                STAGE = 0;
            }
            float X = 100.0F * (OLD / (BIG6 - BNXT));
            if (X > 50.0F)
            {
                SWARNING[6] = 1;
            }
            if (VERSION == Variant.Swo)
            {
                if (EVEN && BHAGE > 500.0F)
                {
                    SWARNING[6] = 1;
                }
            }
            else if (VERSION == Variant.Nwo || VERSION == Variant.Smc)
            {
                if (EVEN && BHAGE > 120.0F)
                {
                    SWARNING[6] = 1;
                }
            }
            else
            {
                if (EVEN && STAGE > 30.0F)
                {
                    SWARNING[6] = 1;
                }
            }

            for (int I = 0; I < NTREES2; ++I)
            {
                SPECIES[I] = TDATAI[I, 0];
                USER[I] = TDATAI[I, 2];
                MGEXP[I] = 0.0F;
                DBH2[I] = TDATAR[I, 0];
                HT2[I] = TDATAR[I, 1];
                CR2[I] = TDATAR[I, 2];
                EXPAN2[I] = TDATAR[I, 3] * (float)NPTS;
                SCR2[I] = SCR[I, 0];
                HGRO[I] = GROWTH[I, 0];
                DGRO[I] = GROWTH[I, 1];
                MORTEXP[I] = DEADEXP[I] * (float)NPTS;
                float AHCB = (1.0F - TDATAR[I, 2]) * TDATAR[I, 1];
                float SHCB = (1.0F - SCR[I, 0]) * TDATAR[I, 1];

                if (AHCB > SHCB)
                {
                    CRCHNG[I] = 0.0F;
                    SCRCHNG[I] = SCR[I, 0] - TDATAR[I, 6];
                }
                else
                {
                    CRCHNG[I] = TDATAR[I, 2] - TDATAR[I, 6];
                    SCRCHNG[I] = 0.0F;
                }
            }
            STOR[0] = NO;
            STOR[1] = RD0;
            STOR[2] = A1;
            STOR[3] = A2;
            STOR[4] = A1MAX;
            STOR[5] = PA1MAX;
        }

        /// <summary>
        /// Does argument checking and raises error flags if problems are found.
        /// </summary>
        /// <param name="CYCLG"></param>
        /// <param name="VERSION">Organon variant.</param>
        /// <param name="NPTS"></param>
        /// <param name="NTREES"></param>
        /// <param name="STAGE"></param>
        /// <param name="BHAGE"></param>
        /// <param name="SPECIES"></param>
        /// <param name="CALH">Unused.</param>
        /// <param name="CALC">Unused.</param>
        /// <param name="CALD">Unused.</param>
        /// <param name="EVEN"></param>
        /// <param name="PRUNE">Unused.</param>
        /// <param name="THIN"></param>
        /// <param name="FERT"></param>
        /// <param name="MORT">Unused.</param>
        /// <param name="GENETICS"></param>
        /// <param name="SWISSNC"></param>
        /// <param name="DBH"></param>
        /// <param name="HT"></param>
        /// <param name="CR"></param>
        /// <param name="EXPAN"></param>
        /// <param name="SITE_1"></param>
        /// <param name="SITE_2"></param>
        /// <param name="MSDI_1"></param>
        /// <param name="MSDI_2"></param>
        /// <param name="MSDI_3"></param>
        /// <param name="PDEN"></param>
        /// <param name="ACALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YSF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YST"></param>
        /// <param name="SCR"></param>
        /// <param name="MGEXP"></param>
        /// <param name="GWDG"></param>
        /// <param name="GWHG"></param>
        /// <param name="FR"></param>
        /// <param name="IB">Tree species index below which tree is classified as a big six species. 5 for SWO, 3 for other variants.</param>
        /// <param name="NSPN">Number of species groups supported by specified Organon variant.</param>
        /// <param name="BIG6"></param>
        /// <param name="OTHER"></param>
        /// <param name="BNXT"></param>
        /// <param name="ONXT"></param>
        /// <param name="SPGRP"></param>
        /// <param name="SERROR"></param>
        /// <param name="TERROR"></param>
        /// <param name="SWARNING"></param>
        /// <param name="TWARNING"></param>
        /// <param name="ERROR"></param>
        private static void EDIT(int CYCLG, Variant VERSION, int NPTS, int NTREES, int STAGE, int BHAGE, int[] SPECIES,
                                 bool CALH, bool CALC, bool CALD, bool EVEN, bool PRUNE, bool THIN, bool FERT, bool MORT, bool GENETICS,
                                 bool SWISSNC, float[] DBH, float[] HT, float[] CR, float[] EXPAN, float SITE_1, float SITE_2, float MSDI_1,
                                 float MSDI_2, float MSDI_3, float PDEN, float[,] ACALIB, float[] PN, float[] YSF, float BABT, float[] BART, float[] YST,
                                 float[] SCR, float[] MGEXP, float GWDG, float GWHG, float FR, out int IB, out int NSPN, out int BIG6, out int OTHER, out int BNXT,
                                 out int ONXT, int[] SPGRP, int[] SERROR, int[,] TERROR, int[] SWARNING, int[] TWARNING, out bool ERROR)
        {
            ERROR = false;

            int YCYCLG = 5 * CYCLG;
            BIG6 = 0;
            OTHER = 0;
            BNXT = 0;
            ONXT = 0;
            float MAXGF = 0.0F;
            float MAXDF = 0.0F;
            float MAXWH = 0.0F;
            float MAXPP = 0.0F;
            float MAXIC = 0.0F;
            float MAXRA = 0.0F;
            // BUGBUG should use number of trees
            for (int I = 0; I < 2000; ++I)
            {
                for (int J = 0; J < 6; ++J)
                {
                    TERROR[I, J] = 0;
                }
                TWARNING[I] = 0;
            }
            for (int I = 0; I < 35; ++I)
            {
                SERROR[I] = 0;
            }
            for (int I = 0; I < 9; ++I)
            {
                SWARNING[I] = 0;
            }

            if (NTREES < 1 || NTREES > 2000)
            {
                SERROR[0] = 1;
            }
            if (VERSION < Variant.Swo || VERSION > Variant.Rap)
            {
                SERROR[1] = 1;
            }
            if (NPTS < 0)
            {
                SERROR[2] = 1;
            }
            if (SITE_1 < 0.0F && SITE_2 < 0.0F)
            {
                SERROR[3] = 1;
            }
            if (EVEN && BHAGE < 0)
            {
                SERROR[5] = 1;
            }
            if (!EVEN && BHAGE > 0)
            {
                SERROR[6] = 1;
            }
            if (EVEN && (STAGE - BHAGE) < 1)
            {
                SERROR[7] = 1;
            }
            if (!EVEN && FERT)
            {
                SERROR[8] = 1;
            }
            for (int I = 0; I < 5; ++I)
            {
                if (!FERT && (YSF[I] != 0 || PN[I] != 0))
                {
                    SERROR[9] = 1;
                }
                if (FERT)
                {
                    if (YSF[I] > STAGE || YSF[I] > 70.0F)
                    {
                        SERROR[10] = 1;
                    }
                    if (I == 0)
                    {
                        if (PN[I] < 0.0 || PN[I] > 400.0F)
                        {
                            SERROR[11] = 1;
                        }
                        else
                        {
                            if (PN[I] > 400.0F)
                            {
                                SERROR[11] = 1;
                            }
                        }
                    }
                }
            }

            if (THIN && BART[0] >= BABT)
            {
                SERROR[12] = 1;
            }
            for (int I = 0; I < 5; ++I)
            {
                if (!THIN && (YST[I] != 0 || BART[I] != 0))
                {
                    SERROR[13] = 1;
                }
                if (THIN)
                {
                    if (EVEN && YST[I] > STAGE)
                    {
                        SERROR[14] = 1;
                    }
                    if (I > 1)
                    {
                        if (YST[I] != 0.0F && BART[I] < 0.0F)
                        {
                            SERROR[15] = 1;
                        }
                    }
                    if (BABT < 0.0F)
                    {
                        SERROR[16] = 1;
                    }
                }
            }

            if (THIN && YST[0] == YCYCLG)
            {
                SERROR[17] = 1;
                for (int I = 0; I < NTREES; ++I)
                {
                    if (MGEXP[I] > 0.0F)
                    {
                        SERROR[17] = 0;
                    }
                }
            }

            if (CYCLG < 0)
            {
                SERROR[18] = 1;
            }
            for (int I = 0; I < 3; ++I)
            {
                for (int J = 0; J < 18; ++J)
                {
                    if (ACALIB[J, I] > 2.0F || ACALIB[J, I] < 0.5F)
                    {
                        SERROR[19] = 1;
                    }
                }
            }

            if (MSDI_1 > 1000.0F || MSDI_2 > 1000.0F || MSDI_3 > 1000.0F)
            {
                SERROR[20] = 1;
            }


            if (GENETICS)
            {
                if (!EVEN)
                {
                    SERROR[21] = 1;
                }
                if (GWDG < 0.0F || GWHG < 0.0F)
                {
                    SERROR[22] = 1;
                }
                if (GWDG > 20.0F || GWHG > 20.0F)
                {
                    SERROR[23] = 1;
                }
            }
            else
            {
                if (GWDG > 0.0F || GWHG > 0.0F)
                {
                    SERROR[24] = 1;
                }
            }

            if (SWISSNC)
            {
                if (VERSION == Variant.Swo || VERSION == Variant.Rap)
                {
                    SERROR[25] = 1;
                }
                if (!EVEN)
                {
                    SERROR[26] = 1;
                }
                if (FR < 0.85F)
                {
                    SERROR[27] = 1;
                }
                if (FR > 7.0F)
                {
                    SERROR[28] = 1;
                }
                if (FERT && FR < 3.0)
                {
                    SERROR[29] = 1;
                }
            }
            else
            {
                if (FR > 0.0F)
                {
                    SERROR[30] = 1;
                }
            }

            if (VERSION >= Variant.Rap && SITE_1 < 0.0F)
            {
                SERROR[31] = 1;
            }
            if (VERSION >= Variant.Rap && PDEN < 0.0F)
            {
                SERROR[32] = 1;
            }
            if (!EVEN && VERSION >= Variant.Rap)
            {
                SERROR[33] = 1;
            }

            for (int I = 0; I < 34; ++I)
            {
                if (SERROR[I] == 1)
                {
                    ERROR = true;
                    IB = -1;
                    NSPN = -1;
                    return;
                }
            }

            switch (VERSION)
            {
                case Variant.Swo:
                    IB = 4;
                    NSPN = 18;
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    IB = 2;
                    NSPN = 11;
                    break;
                case Variant.Rap:
                    IB = 2;
                    NSPN = 7;
                    break;
                default:
                    throw new NotSupportedException();
            }

            // EDIT TREE RECORDS FOR ERRORS
            for (int I = 0; I < NTREES; ++I)
            {
                CKSP(VERSION, I, SPECIES, TERROR);
                if (DBH[I] < 0.09F)
                {
                    TERROR[I, 1] = 1;
                }
                if (HT[I] < 4.5F)
                {
                    TERROR[I, 2] = 1;
                }
                if (CR[I] < 0.0F || CR[I] > 1.0F)
                {
                    TERROR[I, 3] = 1;
                }
                if (EXPAN[I] < 0.0F)
                {
                    TERROR[I, 4] = 1;
                }
                if (SCR[I] < 0.0 || SCR[I] > 1.0)
                {
                    TERROR[I, 5] = 1;
                }
            }

            for (int I = 0; I < NTREES; ++I)
            {
                for (int J = 0; J < 6; ++J)
                {
                    if (TERROR[I, J] == 1)
                    {
                        ERROR = true;
                        IB = -1;
                        NSPN = -1;
                        return;
                    }
                }
            }

            int IIB = IB;
            for (int I = 0; I < NTREES; ++I)
            {
                switch (VERSION)
                {
                    case Variant.Swo:           // SWO BIG SIX
                        if (SPECIES[I] == 122 && HT[I] > MAXPP)
                        {
                            MAXPP = HT[I];
                        }
                        else if (SPECIES[I] == 81 && HT[I] > MAXIC)
                        {
                            MAXIC = HT[I];
                        }
                        else if (SPECIES[I] == 202 && HT[I] > MAXDF)
                        {
                            MAXDF = HT[I];
                        }
                        else if (SPECIES[I] == 15 && HT[I] > MAXDF)
                        {
                            MAXDF = HT[I];
                        }
                        else if (SPECIES[I] == 17 && HT[I] > MAXDF)
                        {
                            MAXDF = HT[I];
                        }
                        else if (SPECIES[I] == 117 && HT[I] > MAXDF)
                        {
                            MAXDF = HT[I];
                        }
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        if (SPECIES[I] == 17 && HT[I] > MAXGF)
                        {
                            MAXGF = HT[I];
                        }
                        else if (SPECIES[I] == 202 && HT[I] > MAXDF)
                        {
                            MAXDF = HT[I];
                        }
                        else if (SPECIES[I] == 263 && HT[I] > MAXWH)
                        {
                            MAXWH = HT[I];
                        }
                        break;
                    case Variant.Rap:
                        if (SPECIES[I] == 351 && HT[I] > MAXRA)
                        {
                            MAXRA = HT[I];
                        }
                        break;
                }

                SPGROUP(VERSION, I, SPECIES, SPGRP);
                if (VERSION >= Variant.Rap)
                {
                    IIB = 1;
                }
                if (SPGRP[I] < IIB)
                {
                    BIG6 = BIG6 + 1;
                    if (EXPAN[I] < 0.0F)
                    {
                        BNXT = BNXT + 1;
                    }
                }
                else
                {
                    OTHER = OTHER + 1;
                    if (EXPAN[I] < 0.0F)
                    {
                        ONXT = ONXT + 1;
                    }
                }
            }

            // DETERMINE IF SPECIES MIX CORRECT FOR STAND AGE
            float SBA = 0.0F;
            float B6SBA = 0.0F;
            float HWSBA = 0.0F;
            for (int I = 0; I < NTREES; ++I)
            {
                if (EXPAN[I] < 0.0F)
                {
                    continue;
                }
                float BA = DBH[I] * DBH[I] * EXPAN[I];
                SBA = SBA + BA;
                if (SPGRP[I] < IIB)
                {
                    B6SBA = B6SBA + BA;
                }
                if (VERSION == Variant.Swo)
                {
                    if (SPECIES[I] == 361 || SPECIES[I] == 431 || SPECIES[I] == 818)
                    {
                        HWSBA = HWSBA + BA;
                    }
                }
            }

            SBA = SBA * 0.005454154F / (float)NPTS;
            B6SBA = B6SBA * .005454154F / (float)NPTS;
            if (B6SBA < 0)
            {
                SERROR[4] = 1;
                ERROR = true;
                return;
            }

            if (VERSION >= Variant.Rap)
            {
                float PRA;
                if (SBA > 0.0F)
                {
                    PRA = B6SBA / SBA;
                }
                else
                {
                    PRA = 0.0F;
                }

                if (PRA < 0.9F)
                {
                    SERROR[34] = 1;
                    ERROR = true;
                    return;
                }
            }

            // DETERMINE WARNINGS (IF ANY)
            switch (VERSION)
            {
                case Variant.Swo:
                    if ((SITE_1 > 0.0F) && (SITE_1 < 40.0F || SITE_1 > 150.0F))
                    {
                        SWARNING[0] = 1;
                    }
                    if ((SITE_2 > 0.0F) && (SITE_2 < 50.0F || SITE_2 > 140.0F))
                    {
                        SWARNING[1] = 1;
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if ((SITE_1 > 0.0F) && (SITE_1 < 90.0F || SITE_1 > 142.0F))
                    {
                        SWARNING[0] = 1;
                    }
                    if ((SITE_2 > 0.0F) && (SITE_2 < 90.0F || SITE_2 > 142.0F))
                    {
                        SWARNING[1] = 1;
                    }
                    break;
                case Variant.Rap:
                    if (SITE_1 < 20.0F || SITE_1 > 125.0F)
                    {
                        SWARNING[0] = 1;
                    }
                    if ((SITE_2 > 0.0F) && (SITE_2 < 90.0F || SITE_2 > 142.0F))
                    {
                        SWARNING[1] = 1;
                    }
                    break;
            }

            switch (VERSION)
            {
                case Variant.Swo:
                    if (MAXPP > 0.0F)
                    {
                        float MAXHT = (SITE_2 - 4.5F) * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.164985 * (SITE_2 - 4.5), 0.288169)))) + 4.5F;
                        if (MAXPP > MAXHT)
                        {
                            SWARNING[2] = 1;
                        }
                    }
                    if (MAXIC > 0.0F)
                    {
                        float ICSI = (0.66F * SITE_1) - 4.5F;
                        float MAXHT = ICSI * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.174929 * ICSI, 0.281176)))) + 4.5F;
                        if (MAXIC > MAXHT)
                        {
                            SWARNING[2] = 1;
                        }
                    }
                    if (MAXDF > 0.0F)
                    {
                        float MAXHT = (SITE_1 - 4.5F) * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.174929 * (SITE_1 - 4.5), 0.281176)))) + 4.5F;
                        if (MAXDF > MAXHT)
                        {
                            SWARNING[2] = 1;
                        }
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if (MAXDF > 0.0F)
                    {
                        float Z50 = 2500.0F / (SITE_1 - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (MAXDF > MAXHT)
                        {
                            SWARNING[2] = 1;
                        }
                    }
                    if (MAXGF > 0.0F)
                    {
                        float Z50 = 2500.0F / (SITE_1 - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (MAXGF > MAXHT)
                        {
                            SWARNING[2] = 1;
                        }
                    }
                    if (MAXWH > 0.0F)
                    {
                        float Z50 = 2500.0F / (SITE_2 - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (0.00192F + 0.00007F * Z50);
                        if (MAXWH > MAXHT)
                        {
                            SWARNING[2] = 1;
                        }
                    }
                    break;
                case Variant.Rap:
                    if (MAXRA > 0.0F)
                    {
                        HeightGrowth.WHHLB_H40(SITE_1, 20.0F, 150.0F, out float MAXHT);
                        if (MAXRA > MAXHT)
                        {
                            SWARNING[2] = 1;
                        }
                    }
                    break;
            }

            if (EVEN && VERSION < Variant.Smc && BHAGE < 10)
            {
                SWARNING[4] = 1;
            }

            if ((VERSION == Variant.Swo && (B6SBA + HWSBA) < SBA * 0.2F) ||
                (VERSION == Variant.Nwo && (B6SBA + HWSBA) < SBA * 0.5F) ||
                (VERSION == Variant.Smc && (B6SBA + HWSBA) < SBA * 0.5F) ||
                (VERSION == Variant.Rap && (B6SBA + HWSBA) < SBA * 0.8F))
            {
                SWARNING[4] = 1;
            }
            if (NTREES < 50)
            {
                SWARNING[5] = 1;
            }

            CKAGE(VERSION, NTREES, IB, SPGRP, PDEN, SITE_1, SITE_2, HT, out float OLD);

            float X = 100.0F * (OLD / (BIG6 - BNXT));
            if (X >= 50.0F)
            {
                SWARNING[6] = 1;
            }
            if (VERSION == Variant.Swo)
            {
                if (EVEN && BHAGE > 500.0F)
                {
                    SWARNING[7] = 1;
                }
            }
            else if (VERSION == Variant.Nwo || VERSION == Variant.Smc)
            {
                if (EVEN && BHAGE > 120.0F)
                {
                    SWARNING[7] = 1;
                }
            }
            else
            {
                if (EVEN && STAGE > 30.0F)
                {
                    SWARNING[7] = 1;
                }
            }

            int EXCAGE;
            if (EVEN)
            {
                switch (VERSION)
                {
                    case Variant.Swo:
                        EXCAGE = 500 - STAGE - 5;
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        EXCAGE = 120 - STAGE - 5;
                        break;
                    case Variant.Rap:
                        EXCAGE = 30 - STAGE - 1;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                switch (VERSION)
                {
                    case Variant.Swo:
                        EXCAGE = 500 - (CYCLG + 1) * 5;
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        EXCAGE = 120 - (CYCLG + 1) * 5;
                        break;
                    case Variant.Rap:
                        EXCAGE = 30 - (CYCLG + 1) * 1;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (EXCAGE < 0)
            {
                SWARNING[8] = 1;
            }

            float B1 = -0.04484724F;
            for (int I = 0; I < NTREES; ++I)
            {
                float B0;
                switch (SPECIES[I])
                {
                    case 202:
                        B0 = 19.04942539F;
                        break;
                    // TSHE
                    case 263:
                        if (VERSION == Variant.Nwo || VERSION == Variant.Smc)
                        {
                            B0 = 19.04942539F;
                        }
                        else if (VERSION == Variant.Rap)
                        {
                            B0 = 19.04942539F;
                        }
                        else
                        {
                            // BUGBUG Fortran code leaves B0 unitialized in Version.Swo case but also always treats hemlock as Douglas-fir
                            B0 = 19.04942539F;
                        }
                        break;
                    case 15:
                    case 17:
                        B0 = 16.26279948F;
                        break;
                    case 122:
                        B0 = 17.11482201F;
                        break;
                    case 117:
                        B0 = 14.29011403F;
                        break;
                    default:
                        B0 = 15.80319194F;
                        break;
                }

                float PHT = 4.5F + B0 * DBH[I] / (1.0F - B1 * DBH[I]);
                if (HT[I] > PHT)
                {
                    TWARNING[I] = 1;
                }
            }
        }
            
        private static void SPGROUP(Variant VERSION, int I, int[] SPECIES, int[] SPGRP)
        {
            // (DOUG? Why are species assigned to a given group and why does group numbering vary with version? The "group" here is only
            //    202 + 15 for NWO, which is PSME and ABCO.)
            // DETERMINE SPECIES GROUP FOR EACH TREE IN TREE LIST 
            // I = TREE INDICATOR
            // BUGBUG: doesn't iterate over length of SCODE arrays
            int[] SCODE1 = { 202, 15, 17, 122, 117, 81, 263, 242, 231, 361, 431, 631, 805, 312, 815, 818, 351, 492, 920 };
            int[] SCODE2 = { 202, 17, 263, 242, 231, 361, 312, 815, 351, 492, 920 };
            int[] SCODE3 = { 351, 202, 263, 242, 312, 492, 920 };
            int ISX = -9999;
            switch (VERSION)
            {
                case Variant.Swo:
                    for (int J = 0; J < 19; ++J)
                    {
                        if (SPECIES[I] == SCODE1[J])
                        {
                            ISX = J;
                            if (ISX > 1)
                            {
                                ISX = ISX - 1;
                            }
                            Debug.Assert(ISX < SCODE1.Length - 1);
                            break;
                        }
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    for (int J = 0; J < 11; ++J)
                    {
                        if (SPECIES[I] == SCODE2[J])
                        {
                            ISX = J;
                            Debug.Assert(ISX < SCODE2.Length);
                            break;
                        }
                    }
                    break;
                case Variant.Rap:
                    for (int J = 0; J < 7; ++J)
                    {
                        if (SPECIES[I] == SCODE3[J])
                        {
                            ISX = J;
                            Debug.Assert(ISX < SCODE3.Length);
                            break;
                        }
                    }
                    break;
            }

            SPGRP[I] = ISX;
        }

        private static void CKSP(Variant VERSION, int I, int[] SPECIES, int[,] TERROR)
        {
            // check if species FIA code is valid for tree I
            int[] SCODE1 = { 202, 15, 17, 122, 117, 81, 263, 242, 231, 361, 431, 631, 805, 312, 815, 818, 351, 492, 920 };
            int[] SCODE2 = { 202, 17, 263, 242, 231, 361, 312, 815, 351, 492, 920 };
            int[] SCODE3 = { 351, 202, 263, 242, 312, 492, 920 };

            bool BAD = true;
            switch (VERSION)
            {
                case Variant.Swo:
                    for (int J = 0; J < 19; ++J)
                    {
                        if (SPECIES[I] == SCODE1[J])
                        {
                            // BUGBUG remove BAD flag and just return here
                            BAD = false;
                            break;
                        }
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    for (int J = 0; J < 11; ++J)
                    {
                        if (SPECIES[I] == SCODE2[J])
                        {
                            BAD = false;
                            break;
                        }
                    }
                    break;
                case Variant.Rap:
                    for (int J = 0; J < 7; ++J)
                    {
                        if (SPECIES[I] == SCODE3[J])
                        {
                            BAD = false;
                            break;
                        }
                    }
                    break;
            }
            if (BAD)
            {
                TERROR[I, 0] = 1;
            }
        }

        private static void CKAGE(Variant VERSION, int NTREES, int IB, int[] SPGRP, float PDEN, float SITE_1, float SITE_2, float[] HT, out float OLD)
        {
            OLD = 0.0F;
            for (int K = 0; K < NTREES; ++K)
            {
                if (HT[K] < 4.5F)
                {
                    continue;
                }

                float GEAGE = 0.0F; // BUGBUG not intitialized on all Fortran paths
                float IDXAGE;
                int ISISP;
                float SITE;
                switch (VERSION)
                {
                    case Variant.Swo:
                        // GROWTH EFFECTIVE AGE FROM HANN AND SCRIVANI'S (1987) DOMINANT HEIGHT GROWTH EQUATION
                        if (SPGRP[K] == 3)
                        {
                            SITE = SITE_2 - 4.5F;
                            ISISP = 2;
                        }
                        else
                        {
                            SITE = SITE_1 - 4.5F;
                            if (SPGRP[K] == 5)
                            {
                                SITE = SITE_1 * 0.66F - 4.5F;
                            }
                            ISISP = 1;
                        }
                        HeightGrowth.HS_HG(ISISP, SITE, HT[K], out GEAGE, out float PHTGRO);
                        IDXAGE = 500.0F;
                        break;
                    case Variant.Nwo:
                        float GP = 5.0F;
                        if (SPGRP[K] == 3)
                        {
                            // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQATION
                            SITE = SITE_2;
                            HeightGrowth.F_HG(SITE, HT[K], GP, out GEAGE, out PHTGRO);
                        }
                        else
                        {
                            // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOR DOUGLAS-FIR AND GRAND FIR
                            SITE = SITE_1;
                            HeightGrowth.B_HG(SITE, HT[K], GP, out GEAGE, out PHTGRO);
                        }
                        IDXAGE = 120.0F;
                        break;
                    case Variant.Smc:
                        GP = 5.0F;
                        if (SPGRP[K] == 3)
                        {
                            // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQUATION
                            SITE = SITE_2;
                            HeightGrowth.F_HG(SITE, HT[K], GP, out GEAGE, out PHTGRO);
                        }
                        else
                        {
                            // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOP DOUGLAS-FIR AND GRAND FIR
                            SITE = SITE_1;
                            HeightGrowth.B_HG(SITE, HT[K], GP, out GEAGE, out PHTGRO);
                        }
                        IDXAGE = 120.0F;
                        break;
                    case Variant.Rap:
                        GP = 1.0F;
                        if (SPGRP[K] == 1)
                        {
                            // GROWTH EFFECTIVE AGE FROM WEISKITTEL ET AL.'S (2009) RED ALDER DOMINANT HEIGHT GROWTH EQUATION
                            SITE = SITE_1;
                            HeightGrowth.WHHLB_SI_UC(SITE, PDEN, out float SI_UC);
                            HeightGrowth.WHHLB_GEA(HT[K], SI_UC, out GEAGE);
                        }
                        IDXAGE = 30.0F;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                // BUGBUG inconsistent use of < IB rather than <= IB
                if (SPGRP[K] < IB && GEAGE > IDXAGE)
                {
                    OLD = OLD + 1.0F;
                }
            }
        }

        public static void CROWN_CLOSURE(Variant VERSION, int NTREES, int NPTS, int[] SPECIES, float[] DBH, float[] HT, float[] CR, float[] SCR, float[] EXPAN, out float CC)
        {
            int[] ISPGRP = new int[2000];
            for (int I = 0; I < NTREES; ++I)
            {
                SPGROUP(VERSION, I, SPECIES, ISPGRP);
            }

            float[] CCH = new float[41];
            for (int L = 0; L < 40; ++L)
            {
                CCH[L] = 0.0F;
            }

            CCH[40] = HT[0];
            for (int I = 1; I < NTREES; ++I)
            {
                if (HT[I] > CCH[40])
                {
                    CCH[40] = HT[I];
                }
            }
            for (int I = 0; I < NTREES; ++I)
            {
                float CL = CR[I] * HT[I];
                float HCB = HT[I] - CL;
                float EXPFAC = EXPAN[I] / (float)NPTS;
                switch (VERSION)
                {
                    case Variant.Swo:
                        CrownGrowth.MCW_SWO(ISPGRP[I], DBH[I], HT[I], out float MCW);
                        CrownGrowth.LCW_SWO(ISPGRP[I], MCW, CR[I], SCR[I], DBH[I], HT[I], out float LCW);
                        CrownGrowth.HLCW_SWO(ISPGRP[I], HT[I], CR[I], SCR[I], out float HLCW);
                        CrownGrowth.CALC_CC(VERSION, ISPGRP[I], HLCW, LCW, HT[I], DBH[I], HCB, EXPFAC, CCH);
                        break;
                    case Variant.Nwo:
                        CrownGrowth.MCW_NWO(ISPGRP[I], DBH[I], HT[I], out MCW);
                        CrownGrowth.LCW_NWO(ISPGRP[I], MCW, CR[I], SCR[I], DBH[I], HT[I], out LCW);
                        CrownGrowth.HLCW_NWO(ISPGRP[I], HT[I], CR[I], SCR[I], out HLCW);
                        CrownGrowth.CALC_CC(VERSION, ISPGRP[I], HLCW, LCW, HT[I], DBH[I], HCB, EXPFAC, CCH);
                        break;
                    case Variant.Smc:
                        CrownGrowth.MCW_SMC(ISPGRP[I], DBH[I], HT[I], out MCW);
                        CrownGrowth.LCW_SMC(ISPGRP[I], MCW, CR[I], SCR[I], DBH[I], HT[I], out LCW);
                        CrownGrowth.HLCW_SMC(ISPGRP[I], HT[I], CR[I], SCR[I], out HLCW);
                        CrownGrowth.CALC_CC(VERSION, ISPGRP[I], HLCW, LCW, HT[I], DBH[I], HCB, EXPFAC, CCH);
                        break;
                    case Variant.Rap:
                        CrownGrowth.MCW_RAP(ISPGRP[I], DBH[I], HT[I], out MCW);
                        CrownGrowth.LCW_RAP(ISPGRP[I], MCW, CR[I], SCR[I], DBH[I], HT[I], out LCW);
                        CrownGrowth.HLCW_RAP(ISPGRP[I], HT[I], CR[I], SCR[I], out HLCW);
                        CrownGrowth.CALC_CC(VERSION, ISPGRP[I], HLCW, LCW, HT[I], DBH[I], HCB, EXPFAC, CCH);
                        break;
                }
            }

            CC = CCH[0];
            Debug.Assert(CC >= 0.0F);
            Debug.Assert(CC <= 100.0F);
        }

        public static void INGRO_FILL(Variant VERSION, int NTREES, int NINGRO, int[] SPECIES, float SITE_1, float SITE_2, float[,] ACALIB, float[] DBH, float[] HT, float[] CR, float[] EXPAN)
        {
            // ROUTINE TO CALCULATE MISSING CROWN RATIOS
            //
            // NINGRO = NUMBER OF TREES ADDED
            float SI_1;
            if (VERSION == Variant.Swo)
            {
                if (SITE_1 < 0.0F && SITE_2 > 0.0F)
                {
                    SITE_1 = 1.062934F * SITE_2;
                }
                else if (SITE_2 < 0.0F)
                {
                    SITE_2 = 0.940792F * SITE_1;
                }
            }
            else if (VERSION == Variant.Nwo || VERSION == Variant.Smc)
            {
                if (SITE_1 < 0.0F && SITE_2 > 0.0F)
                {
                    SITE_1 = 0.480F + (1.110F * SITE_2);
                }
                else if (SITE_2 < 0.0F)
                {
                    SITE_2 = -0.432F + (0.899F * SITE_1);
                }
            }
            else
            {
                if (SITE_2 < 0.0F)
                {
                    // BUGBUG: not initialized in Fortran code; should this be SITE_1?
                    SI_1 = 0.0F;
                    SITE_2 = 4.776377F * (float)Math.Pow(SI_1, 0.763530587F);
                }
            }

            int IB;
            switch (VERSION)
            {
                case Variant.Swo:
                    IB = 5;
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    IB = 3;
                    break;
                case Variant.Rap:
                    IB = 3;
                    break;
                default:
                    throw new NotSupportedException();
            }

            SI_1 = SITE_1 - 4.5F;
            float SI_2 = SITE_2 - 4.5F;
            int[] SPGRP = new int[2000];
            // BUGBUG no check that NINGRO <= NTREES
            for (int I = NTREES - NINGRO; I < NTREES; ++I)
            {
                if (HT[I] != 0.0F)
                {
                    continue;
                }
                SPGROUP(VERSION, I, SPECIES, SPGRP);

                // CALCULATE HCB
                float RHT;
                switch (VERSION)
                {
                    case Variant.Swo:
                        HeightGrowth.HD_SWO(SPGRP[I], DBH[I], out RHT);
                        break;
                    case Variant.Nwo:
                        HeightGrowth.HD_NWO(SPGRP[I], DBH[I], out RHT);
                        break;
                    case Variant.Smc:
                        HeightGrowth.HD_SMC(SPGRP[I], DBH[I], out RHT);
                        break;
                    case Variant.Rap:
                        HeightGrowth.HD_RAP(SPGRP[I], DBH[I], out RHT);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                HT[I] = 4.5F + ACALIB[SPGRP[I], 0] * (RHT - 4.5F);
            }

            // BUGBUG: should be NTREES in Fortran code, not 2000
            float[] DEADEXP = new float[2000];
            float[,] GROWTH = new float[2000, 4];
            float[,] TDATAR = new float[2000, 4];
            int[,] TDATAI = new int[2000, 3];
            for (int I = 0; I < 2000; ++I)
            {
                GROWTH[I, 2] = 0.0F;
                GROWTH[I, 3] = 0.0F;
                TDATAI[I, 0] = 0;
                TDATAI[I, 1] = 0;
                TDATAI[I, 2] = 0; // BUGBUG: not clear why user data needs to be zeroed here
                TDATAR[I, 0] = 0.0F;
                TDATAR[I, 1] = 0.0F;
                TDATAR[I, 2] = 0.0F;
                TDATAR[I, 3] = 0.0F;
                DEADEXP[I] = 0.0F;
            }

            for (int I = 0; I < NTREES; ++I)
            {
                TDATAI[I, 0] = SPECIES[I];
                TDATAI[I, 1] = SPGRP[I];
                TDATAR[I, 0] = DBH[I];
                TDATAR[I, 1] = HT[I];
                TDATAR[I, 2] = CR[I];
                TDATAR[I, 3] = EXPAN[I];
            }

            Mortality.OldGro(NTREES, IB, TDATAI, TDATAR, GROWTH, DEADEXP, 0.0F, out float OG);
            float[] BAL = new float[500];
            float[] BALL = new float[51];
            float[] CCFL = new float[500];
            float[] CCFLL = new float[51];
            Stats.SSTATS(VERSION, NTREES, TDATAI, TDATAR, out float SBA, out float TPA, out float SCCF, BAL, BALL, CCFL, CCFLL);
            for (int I = NTREES - NINGRO; I < NTREES; ++I)
            {
                if (CR[I] != 0.0F)
                {
                    continue;
                }

                // CALCULATE HCB
                CrownGrowth.GET_CCFL(DBH[I], CCFLL, CCFL, out float SCCFL);
                float HCB;
                switch (VERSION)
                {
                    case Variant.Swo:
                        CrownGrowth.HCB_SWO(SPGRP[I], HT[I], DBH[I], SCCFL, SBA, SI_1, SI_2, OG, out HCB);
                        break;
                    case Variant.Nwo:
                        CrownGrowth.HCB_NWO(SPGRP[I], HT[I], DBH[I], SCCFL, SBA, SI_1, SI_2, OG, out HCB);
                        break;
                    case Variant.Smc:
                        CrownGrowth.HCB_SMC(SPGRP[I], HT[I], DBH[I], SCCFL, SBA, SI_1, SI_2, OG, out HCB);
                        break;
                    case Variant.Rap:
                        CrownGrowth.HCB_RAP(SPGRP[I], HT[I], DBH[I], SCCFL, SBA, SI_1, SI_2, OG, out HCB);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                if (HCB < 0.0F)
                {
                    HCB = 0.0F;
                }
                if (HCB > 0.95F * HT[I])
                {
                    HCB = 0.95F * HT[I];
                }
                CR[I] = (1.0F - (HCB / HT[I])) * ACALIB[SPGRP[I], 1];
            }
        }
    }
}
