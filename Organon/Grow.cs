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
        /// <param name="treeRecordCount"></param>
        /// <param name="maxBigSixSpeciesGroupIndex"></param>
        /// <param name="bigSixTreeRecordCount">Number of tree records for big "six" species. Used to determine tree tripling method.</param>
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
        public static void GROW(Variant VERSION, ref int CYCLG, int treeRecordCount, int maxBigSixSpeciesGroupIndex, int bigSixTreeRecordCount, int OTHER, int NSPN, ref int STAGE,
                     ref int BHAGE, int[] POINT, int[] TREENO, int[,] TDATAI, int[,] PRAGE, int[,] BRCNT, int[,] BRHT, int[,] BRDIA,
                     int[,] JCORE, int[] NPR, ref int TCYCLE, ref int FCYCLE, bool TRIPLE, bool WOODQ, bool POST, bool MORT,
                     bool GENETICS, bool SWISSNC, float[,] TDATAR, float SI_1, float SI_2, float SBA1, float[] BALL1, float[] BAL1,
                     float[,] CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, float[,] GROWTH, float[,] PRLH, float[,] PRDBH, float[,] PRHT,
                     float[,] PRCR, float[,] PREXP, float[,] SCR, float[,] VOLTR, float[,] SYTVOL, float[] CCH, ref float OLD, float[] MGEXP, float[] DEADEXP,
                     float A1, float A2, float A1MAX, float PA1MAX, float NO, float RD0, float RAAGE, float RASI, float[] CCFLL1, float[] CCFL1,
                     float[] CCFLL2, float[] CCFL2, float[] BALL2, float[] BAL2, float GWDG, float GWHG,
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

            // diameter growth
            int treeRecordsWithExpansionFactorZero = 0;
            int bigSixRecordsWithExpansionFactorZero = 0;
            int otherSpeciesRecordsWithExpansionFactorZero = 0;
            for (int I = 1; I < treeRecordCount; ++I)
            {
                if (TDATAR[I, 3] <= 0.0F)
                {
                    ++treeRecordsWithExpansionFactorZero;
                    if (TDATAI[I, 1] <= maxBigSixSpeciesGroupIndex)
                    {
                        ++bigSixRecordsWithExpansionFactorZero;
                    }
                    else
                    {
                        ++otherSpeciesRecordsWithExpansionFactorZero;
                    }
                }
            }

            int J = treeRecordCount + 1;
            int tripledTreeRecordCount = 3 * (treeRecordCount - treeRecordsWithExpansionFactorZero);
            int totalTreeRecordCount = treeRecordsWithExpansionFactorZero + tripledTreeRecordCount;
            int potentialNewTreeRecordsFromTripling = 2 * (treeRecordCount - treeRecordsWithExpansionFactorZero);
            int NTCAL4 = treeRecordsWithExpansionFactorZero + potentialNewTreeRecordsFromTripling;
            int potentialBixSixRecordsFromTripling = bigSixTreeRecordCount + 2 * (bigSixTreeRecordCount - bigSixRecordsWithExpansionFactorZero);
            int NTCAL6 = bigSixTreeRecordCount + (bigSixTreeRecordCount - bigSixRecordsWithExpansionFactorZero);
            int triplingMethod = 0;
            if (TRIPLE)
            {
                int currentTreeRecordCount = MGEXP.Length;
                if ((tripledTreeRecordCount <= currentTreeRecordCount) && (totalTreeRecordCount <= currentTreeRecordCount))
                {
                    triplingMethod = 1;
                }
                if (WOODQ && (potentialBixSixRecordsFromTripling > currentTreeRecordCount))
                {
                    triplingMethod = 0;
                }
                else if ((potentialNewTreeRecordsFromTripling <= currentTreeRecordCount) && (NTCAL4 <= currentTreeRecordCount))
                {
                    triplingMethod = 2;
                    if (WOODQ && (NTCAL6 > currentTreeRecordCount))
                    {
                        triplingMethod = 0;
                    }
                }
            }
            int ON = 0;
            int IWQ = 0;
            for (int I = 0; I < treeRecordCount; ++I)
            {
                if (WOODQ && TDATAI[I, 1] <= maxBigSixSpeciesGroupIndex)
                {
                    ++IWQ;
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
                    Triple.XTRIP(I, J, triplingMethod, ON, IWQ, WOODQ, maxBigSixSpeciesGroupIndex, bigSixTreeRecordCount, POINT, TREENO, TDATAI, PRAGE, BRCNT, BRHT, BRDIA, JCORE, NPR, PRLH, PRDBH, PRHT, PRCR, PREXP, SCR, VOLTR, SYTVOL);
                    Triple.DGTRIP(I, ref J, triplingMethod, ref ON, VERSION, maxBigSixSpeciesGroupIndex, ref bigSixTreeRecordCount, ref OTHER, TDATAI, TDATAR, GROWTH, MGEXP, DEADEXP);
                }
            }
            if (TRIPLE)
            {
                treeRecordCount = J - 1;
            }

            // GROWTH 2
            // CALCULATE HTGRO FOR BIG6
            tripledTreeRecordCount = (bigSixTreeRecordCount - bigSixRecordsWithExpansionFactorZero) * 3 + (OTHER - otherSpeciesRecordsWithExpansionFactorZero);
            totalTreeRecordCount = bigSixRecordsWithExpansionFactorZero + otherSpeciesRecordsWithExpansionFactorZero + tripledTreeRecordCount;
            potentialNewTreeRecordsFromTripling = (bigSixTreeRecordCount - bigSixRecordsWithExpansionFactorZero) * 2 + (OTHER - otherSpeciesRecordsWithExpansionFactorZero);
            NTCAL4 = bigSixRecordsWithExpansionFactorZero + otherSpeciesRecordsWithExpansionFactorZero + potentialNewTreeRecordsFromTripling;
            potentialBixSixRecordsFromTripling = bigSixTreeRecordCount + 2 * (bigSixTreeRecordCount - bigSixRecordsWithExpansionFactorZero);
            NTCAL6 = bigSixTreeRecordCount + (bigSixTreeRecordCount - bigSixRecordsWithExpansionFactorZero);
            triplingMethod = 0;
            if (TRIPLE)
            {
                if (tripledTreeRecordCount <= 2000 && totalTreeRecordCount <= 2000)
                {
                    triplingMethod = 1;
                }
                if (WOODQ && potentialBixSixRecordsFromTripling > 2000)
                {
                    triplingMethod = 0;
                }
                else if (potentialNewTreeRecordsFromTripling <= 2000 && NTCAL4 <= 2000)
                {
                    triplingMethod = 2;
                    if (WOODQ && NTCAL6 > 2000)
                    {
                        triplingMethod = 0;
                    }
                }
            }
            ON = 0;
            IWQ = 0;
            for (int I = 0; I < treeRecordCount; ++I)
            {
                if (TDATAI[I, 1] <= maxBigSixSpeciesGroupIndex)
                {
                    if (WOODQ)
                    {
                        ++IWQ;
                    }
                    if (TDATAR[I, 3] <= 0.0F)
                    {
                        GROWTH[I, 0] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.HTGRO1(I, triplingMethod, ON, VERSION, CYCLG, maxBigSixSpeciesGroupIndex, TDATAI, TDATAR, SI_1, SI_2, CCH, PN, YF, BABT, BART, YT, ref OLD, PDEN, GROWTH);
                        if (TDATAI[I, 0] == 202)
                        {
                            GROWTH[I, 0] = GROWTH[I, 0] * HGMOD_GG * HGMOD_SNC;
                        }
                        Triple.XTRIP(I, J, triplingMethod, ON, IWQ, WOODQ, maxBigSixSpeciesGroupIndex, bigSixTreeRecordCount, POINT, TREENO, TDATAI, PRAGE, BRCNT, BRHT, BRDIA, JCORE, NPR, PRLH, PRDBH, PRHT, PRCR, PREXP, SCR, VOLTR, SYTVOL);
                        Triple.HGTRIP(I, ref J, triplingMethod, ref ON, VERSION, ref bigSixTreeRecordCount, TDATAI, TDATAR, GROWTH, MGEXP, DEADEXP);
                    }
                }
            }
            if (TRIPLE)
            {
                treeRecordCount = J - 1;
            }

            // DETERMINE MORTALITY, IF REQUIRED
            Mortality.MORTAL(VERSION, CYCLG, treeRecordCount, maxBigSixSpeciesGroupIndex, TDATAI, POST, MORT, TDATAR, SCR, GROWTH, MGEXP, DEADEXP, BALL1, BAL1, SI_1, SI_2, PN, YF, A1, A2, A1MAX, ref PA1MAX, ref NO, RD0, ref RAAGE, PDEN);

            // UPDATE DIAMETERS
            for (int I = 0; I < treeRecordCount; ++I)
            {
                TDATAR[I, 0] = TDATAR[I, 0] + GROWTH[I, 1];
            }

            // CALC EOG SBA, CCF/TREE, CCF IN LARGER TREES AND STAND CCF
            Stats.SSTATS(VERSION, treeRecordCount, TDATAI, TDATAR, out float SBA2, out float _, out float _, BAL2, BALL2, CCFL2, CCFLL2);

            // CALCULATE HTGRO FOR 'OTHER' & CROWN ALL SPECIES
            for (int I = 0; I < treeRecordCount; ++I)
            {
                if (TDATAI[I, 1] > maxBigSixSpeciesGroupIndex)
                {
                    if (TDATAR[I, 3] <= 0.0F)
                    {
                        GROWTH[I, 0] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.HTGRO2(I, VERSION, maxBigSixSpeciesGroupIndex, TDATAI, TDATAR, RASI, CALIB, GROWTH);
                    }
                }
                TDATAR[I, 1] = TDATAR[I, 1] + GROWTH[I, 0];
            }

            // CALC CROWN GROWTH
            CrownGrowth.CrowGro(VERSION, CYCLG, treeRecordCount, maxBigSixSpeciesGroupIndex, TDATAI, TDATAR, SCR, GROWTH, MGEXP, DEADEXP, CCFLL1, CCFL1, CCFLL2, CCFL2, SBA1, SBA2, SI_1, SI_2, CALIB, CCH);

            // UPDATE STAND VARIABLES
            if (VERSION != Variant.Rap)
            {
                STAGE += 5;
                BHAGE += 5;
            }
            else
            {
                ++STAGE;
                ++BHAGE;
            }
            ++CYCLG;
            if (FCYCLE > 2)
            {
                FCYCLE = 0;
            }
            else if (FCYCLE > 0)
            {
                ++FCYCLE;
            }
            if (TCYCLE > 0)
            {
                ++TCYCLE;
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
