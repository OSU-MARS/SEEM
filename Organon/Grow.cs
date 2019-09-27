using System;

namespace Osu.Cof.Organon
{
    internal class Grow
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="VERSION"></param>
        /// <param name="CYCLG"></param>
        /// <param name="NTREES"></param>
        /// <param name="IB"></param>
        /// <param name="BIG6">Number of tree records for big "six" species. Used to determine tree tripling method.</param>
        /// <param name="OTHER">Total number of trees of other (non-big "six") species.</param>
        /// <param name="NSPN"></param>
        /// <param name="STAGE"></param>
        /// <param name="BHAGE"></param>
        /// <param name="POINT">Only used by XTRIP().</param>
        /// <param name="TREENO">Only used by XTRIP().</param>
        /// <param name="TDATAI">Tree data.</param>
        /// <param name="PRAGE">Only used by XTRIP().</param>
        /// <param name="BRCNT">Only used by XTRIP().</param>
        /// <param name="BRHT">Only used by XTRIP().</param>
        /// <param name="BRDIA">Only used by XTRIP().</param>
        /// <param name="JCORE">Only used by XTRIP().</param>
        /// <param name="NPR">Only used by XTRIP().</param>
        /// <param name="TCYCLE"></param>
        /// <param name="FCYCLE"></param>
        /// <param name="TRIPLE"></param>
        /// <param name="WOODQ"></param>
        /// <param name="POST"></param>
        /// <param name="MORT"></param>
        /// <param name="GENETICS"></param>
        /// <param name="SWISSNC"></param>
        /// <param name="TDATAR"></param>
        /// <param name="SI_1"></param>
        /// <param name="SI_2"></param>
        /// <param name="SBA1"></param>
        /// <param name="BALL1"></param>
        /// <param name="BAL1"></param>
        /// <param name="CALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YT"></param>
        /// <param name="GROWTH"></param>
        /// <param name="PRLH">Only used by XTRIP().</param>
        /// <param name="PRDBH">Only used by XTRIP().</param>
        /// <param name="PRHT">Only used by XTRIP().</param>
        /// <param name="PRCR">Only used by XTRIP().</param>
        /// <param name="PREXP">Only used by XTRIP().</param>
        /// <param name="SCR"></param>
        /// <param name="VOLTR">Only used by XTRIP().</param>
        /// <param name="SYTVOL">Only used by XTRIP().</param>
        /// <param name="CCH"></param>
        /// <param name="OLD"></param>
        /// <param name="MGEXP">Tree data.</param>
        /// <param name="DEADEXP">Tree data.</param>
        /// <param name="A1"></param>
        /// <param name="A2"></param>
        /// <param name="A1MAX"></param>
        /// <param name="PA1MAX"></param>
        /// <param name="NO"></param>
        /// <param name="RD0"></param>
        /// <param name="RAAGE"></param>
        /// <param name="RASI"></param>
        /// <param name="CCFLL1"></param>
        /// <param name="CCFL1"></param>
        /// <param name="SBA2"></param>
        /// <param name="CCFLL2"></param>
        /// <param name="CCFL2"></param>
        /// <param name="BALL2"></param>
        /// <param name="BAL2"></param>
        /// <param name="TPA2"></param>
        /// <param name="SCCF2"></param>
        /// <param name="GWDG"></param>
        /// <param name="GWHG"></param>
        /// <param name="FR"></param>
        /// <param name="PDEN"></param>
        public static void GROW(Variant VERSION, ref int CYCLG, int NTREES, int IB, int BIG6, int OTHER, int NSPN, ref int STAGE,
                     ref int BHAGE, int[] POINT, int[] TREENO, int[,] TDATAI, int[,] PRAGE, int[,] BRCNT, int[,] BRHT, int[,] BRDIA,
                     int[,] JCORE, int[] NPR, ref int TCYCLE, ref int FCYCLE, bool TRIPLE, bool WOODQ, bool POST, bool MORT,
                     bool GENETICS, bool SWISSNC, float[,] TDATAR, float SI_1, float SI_2, float SBA1, float[] BALL1, float[] BAL1,
                     float[,] CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, float[,] GROWTH, float[,] PRLH, float[,] PRDBH, float[,] PRHT,
                     float[,] PRCR, float[,] PREXP, float[,] SCR, float[,] VOLTR, float[,] SYTVOL, float[] CCH, ref float OLD, float[] MGEXP, float[] DEADEXP,
                     float A1, float A2, float A1MAX, float PA1MAX, float NO, float RD0, float RAAGE, float RASI, float[] CCFLL1, float[] CCFL1,
                     out float SBA2, float[] CCFLL2, float[] CCFL2, float[] BALL2, float[] BAL2, out float TPA2, out float SCCF2, float GWDG, float GWHG,
                     float FR, float PDEN)
        {
            //---------------------------------------------------------
            // M = 0 NO TRIPLING
            // M = 1 TRIPLE EVERY TREE
            // M=2 TRIPLE EVERY OTHER TREE
            // M = 3 RANDOM ERROR
            //---------------------------------------------------------
            float DGMOD_GG = 1.0F;
            float HGMOD_GG = 1.0F;
            float DGMOD_SNC = 1.0F;
            float HGMOD_SNC = 1.0F;
            if (STAGE > 0 && GENETICS)
            {
                float TAGE = (float)STAGE;
                GrowthModifiers.GG_MODS(TAGE, GWDG, GWHG, out DGMOD_GG, out HGMOD_GG);
            }
            if (SWISSNC && (VERSION == Variant.Nwo || VERSION == Variant.Smc))
            {
                GrowthModifiers.SNC_MODS(FR, out DGMOD_SNC, out HGMOD_SNC);
            }
            // GROWTH 1
            // CALCULATE DIAMGRO
            int NZERO = 0;
            int BZERO = 0;
            int OZERO = 0;
            for (int I = 1; I < NTREES; ++I)
            {
                if (TDATAR[I, 3] <= 0.0F)
                {
                    NZERO = NZERO + 1;
                    if (TDATAI[I, 1] <= IB)
                    {
                        BZERO = BZERO + 1;
                    }
                    else
                    {
                        OZERO = OZERO + 1;
                    }
                }
            }
            int J = NTREES + 1;
            int NTCAL1 = (NTREES - NZERO) * 3;
            int NTCAL2 = NZERO + NTCAL1;
            int NTCAL3 = (NTREES - NZERO) * 2;
            int NTCAL4 = NZERO + NTCAL3;
            int NTCAL5 = BIG6 + 2 * (BIG6 - BZERO);
            int NTCAL6 = BIG6 + (BIG6 - BZERO);
            int M = 0;
            if (TRIPLE)
            {
                if (NTCAL1 <= 2000 && NTCAL2 <= 2000)
                {
                    M = 1;
                }
                if (WOODQ && NTCAL5 > 2000)
                {
                    M = 0;
                }
                else if (NTCAL3 <= 2000 && NTCAL4 <= 2000)
                {
                    M = 2;
                    if (WOODQ && NTCAL6 > 2000)
                    {
                        M = 0;
                    }
                }
            }
            int ON = 0;
            int IWQ = 0;
            for (int I = 0; I < NTREES; ++I)
            {
                if (WOODQ && TDATAI[I, 1] <= IB)
                {
                    IWQ = IWQ + 1;
                }
                if (TDATAR[I, 3] <= 0.0F)
                {
                    GROWTH[I, 1] = 0.0F;
                }
                else
                {
                    DiameterGrowth.DIAMGRO(VERSION, I, CYCLG, TDATAI, TDATAR, SI_1, SI_2, SBA1, BALL1, BAL1, CALIB, PN, YF, BABT, BART, YT, GROWTH);
                    if (TDATAI[I, 0] == 202)
                    {
                        GROWTH[I, 1] = GROWTH[I, 1] * DGMOD_GG * DGMOD_SNC;
                    }
                    Triple.XTRIP(I, J, M, ON, IWQ, WOODQ, IB, BIG6, POINT, TREENO, TDATAI, PRAGE, BRCNT, BRHT, BRDIA, JCORE, NPR, PRLH, PRDBH, PRHT, PRCR, PREXP, SCR, VOLTR, SYTVOL);
                    Triple.DGTRIP(I, ref J, M, ON, VERSION, IB, ref BIG6, ref OTHER, TDATAI, TDATAR, GROWTH, MGEXP, DEADEXP);
                }
            }
            if (TRIPLE)
            {
                NTREES = J - 1;
            }

            // GROWTH 2
            // CALCULATE HTGRO FOR BIG6
            NTCAL1 = (BIG6 - BZERO) * 3 + (OTHER - OZERO);
            NTCAL2 = BZERO + OZERO + NTCAL1;
            NTCAL3 = (BIG6 - BZERO) * 2 + (OTHER - OZERO);
            NTCAL4 = BZERO + OZERO + NTCAL3;
            NTCAL5 = BIG6 + 2 * (BIG6 - BZERO);
            NTCAL6 = BIG6 + (BIG6 - BZERO);
            M = 0;
            if (TRIPLE)
            {
                if (NTCAL1 <= 2000 && NTCAL2 <= 2000)
                {
                    M = 1;
                }
                if (WOODQ && NTCAL5 > 2000)
                {
                    M = 0;
                }
                else if (NTCAL3 <= 2000 && NTCAL4 <= 2000)
                {
                    M = 2;
                    if (WOODQ && NTCAL6 > 2000)
                    {
                        M = 0;
                    }
                }
            }
            ON = 0;
            IWQ = 0;
            for (int I = 0; I < NTREES; ++I)
            {
                if (TDATAI[I, 1] <= IB)
                {
                    if (WOODQ)
                    {
                        IWQ = IWQ + 1;
                    }
                    if (TDATAR[I, 3] <= 0.0F)
                    {
                        GROWTH[I, 0] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.HTGRO1(I, M, ON, VERSION, CYCLG, IB, TDATAI, TDATAR, SI_1, SI_2, CCH, CALIB, PN, YF, BABT, BART, YT, ref OLD, PDEN, GROWTH);
                        if (TDATAI[I, 0] == 202)
                        {
                            GROWTH[I, 0] = GROWTH[I, 0] * HGMOD_GG * HGMOD_SNC;
                        }
                        Triple.XTRIP(I, J, M, ON, IWQ, WOODQ, IB, BIG6, POINT, TREENO, TDATAI, PRAGE, BRCNT, BRHT, BRDIA, JCORE, NPR, PRLH, PRDBH, PRHT, PRCR, PREXP, SCR, VOLTR, SYTVOL);
                        Triple.HGTRIP(I, ref J, M, ON, VERSION, ref BIG6, ref OTHER, TDATAI, TDATAR, GROWTH, MGEXP, DEADEXP);
                    }
                }
            }
            if (TRIPLE)
            {
                NTREES = J - 1;
            }

            // DETERMINE MORTALITY, IF REQUIRED
            Mortality.MORTAL(VERSION, CYCLG, NTREES, IB, TDATAI, POST, MORT, TDATAR, SCR, GROWTH, MGEXP, DEADEXP, BALL1, BAL1, SI_1, SI_2, PN, YF, A1, A2, A1MAX, PA1MAX, NO, RD0, RAAGE, PDEN);

            // UPDATE DIAMETERS
            for (int I = 0; I < NTREES; ++I)
            {
                TDATAR[I, 0] = TDATAR[I, 0] + GROWTH[I, 1];
            }

            // CALC EOG SBA, CCF/TREE, CCF IN LARGER TREES AND STAND CCF
            Stats.SSTATS(VERSION, NTREES, TDATAI, TDATAR, out SBA2, out TPA2, out SCCF2, BAL2, BALL2, CCFL2, CCFLL2);

            // CALCULATE HTGRO FOR 'OTHER' & CROWN ALL SPECIES
            IWQ = 0;
            for (int I = 0; I < NTREES; ++I)
            {
                if (TDATAI[I, 1] > IB)
                {
                    if (TDATAR[I, 3] <= 0.0F)
                    {
                        GROWTH[I, 0] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.HTGRO2(I, VERSION, IB, TDATAI, TDATAR, RASI, CALIB, GROWTH);
                    }
                }
                TDATAR[I, 1] = TDATAR[I, 1] + GROWTH[I, 0];
            }

            // CALC CROWN GROWTH
            CrownGrowth.CrowGro(VERSION, CYCLG, NTREES, IB, TDATAI, TDATAR, SCR, GROWTH, MGEXP, DEADEXP, CCFLL1, CCFL1, CCFLL2, CCFL2, SBA1, SBA2, SI_1, SI_2, CALIB, CCH);

            // UPDATE STAND VARIABLES
            if (VERSION <= Variant.Smc)
            {
                STAGE = STAGE + 5;
                BHAGE = BHAGE + 5;
            }
            else
            {
                STAGE = STAGE + 1;
                BHAGE = BHAGE + 1;
            }
            CYCLG = CYCLG + 1;
            if (FCYCLE > 2)
            {
                FCYCLE = 0;
            }
            else if (FCYCLE > 0)
            {
                FCYCLE = FCYCLE + 1;
            }
            if (TCYCLE > 0)
            {
                TCYCLE = TCYCLE + 1;
            }
            // REDUCE CALIBRATION RATIOS
            for (int I = 0; I < 3; ++I)
            {
                for (int II = 0; II < NSPN; ++II)
                {
                    if (CALIB[II, I] < 1.0F || CALIB[II, I] > 1.0F)
                    {
                        float MCALIB = (1.0F + CALIB[II, I + 2]) / 2.0F;
                        CALIB[II, I] = MCALIB + (float)Math.Sqrt(0.5) * (CALIB[II, I] - MCALIB);
                    }
                }
            }
        }
    }
}
