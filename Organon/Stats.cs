using System;

namespace Osu.Cof.Organon
{
    internal static class Stats
    {
        /// <summary>
        /// Convert conifer site index to red alder site index?
        /// </summary>
        /// <param name="SI_1">Conifer site index at breast height?</param>
        /// <returns>Red alder site index.</returns>
        public static float CON_RASI(float SI_1)
        {
            return 9.73F + 0.64516F * (SI_1 + 4.5F);
        }

        /// <summary>
        /// Finds height of the first 40 trees of big six species in the stand based on expansion factors. Ignores other species.
        /// </summary>
        /// <param name="CTMUL"></param>
        /// <param name="VERSION"></param>
        /// <param name="IB">Threshold for big six species membership.</param>
        /// <param name="NTREES">Number of trees available in tree data.</param>
        /// <param name="TDATAI">Tree data.</param>
        /// <param name="TDATAR">Tree data.</param>
        /// <param name="MGEXP">Tree data.</param>
        /// <param name="HT40"></param>
        public static void HTFORTY(float CTMUL, Variant VERSION, int IB, int NTREES, int[,] TDATAI, float[,] TDATAR, float[] MGEXP, out float HT40)
        {
            // find total expansion factor and weighted heights for all trees in the stand
            // BUGBUG Effectively assumes the trees are sorted from largest to smallest. Generally won't calculate H40 in other cases.
            float[] HTCL = new float[100]; // running sum of heights weighted by expansion factors
            float[] TRCL = new float[100]; // running sum of expansion factors
            for (int I = 0; I < 100; ++I)
            {
                HTCL[I] = 0.0F;
                TRCL[I] = 0.0F;
            }

            int IIB = IB;
            if (VERSION == Variant.Rap)
            {
                // BUGBUG not consistent with IB = 3 elsewhere for red alder variant
                IIB = 1;
            }
            for (int I = 0; I < NTREES; ++I)
            {
                if (TDATAI[I, 1] <= IIB)
                {
                    int ID = (int)(TDATAR[I, 0]) + 1;
                    if (ID > 100)
                    {
                        ID = 100;
                    }
                    float EXPAN = TDATAR[I, 3] + CTMUL * MGEXP[I];
                    HTCL[ID] = HTCL[ID] + TDATAR[I, 1] * EXPAN;
                    TRCL[ID] = TRCL[ID] + EXPAN;
                }
            }

            // BUGBUG Assumes trees in stand are of species and have expansion factors such that the first 40 trees occur in the first 
            //        100 tree entries. This isn't necessarily true and the method is additionally fragile as 1) it squashes past index
            //        100 into the 100th row and 2) relies on uninitialized memory being set to zero when there are less than 100 tree
            //        entries.
            float TOTHT = 0.0F; // 
            float TOTTR = 0.0F;
            for (int I = 99; I >= 0; --I)
            {
                TOTHT = TOTHT + HTCL[I];
                TOTTR = TOTTR + TRCL[I];
                if (TOTTR > 40.0F)
                {
                    float TRDIFF = TRCL[I] - (TOTTR - 40.0F);
                    TOTHT = TOTHT - HTCL[I] + ((HTCL[I] / TRCL[I]) * TRDIFF);
                    TOTTR = 40.0F;
                    break;
                }
            }
            if (TOTTR > 0.0F)
            {
                HT40 = TOTHT / TOTTR;
            }
            else
            {
                HT40 = 0.0F;
            }
        }

        public static void RASITE(float H, float A, out float SI)
        {
            // RED ALDER SITE INDEX EQUATION FROM WORTHINGTON, JOHNSON, STAEBLER AND LLOYD(1960) PNW RESEARCH PAPER 36
            SI = (0.60924F + 19.538F / A) * H;
        }

        /// <summary>
        /// Calculate stand statistics.
        /// </summary>
        /// <param name="VERSION">Organon variant.</param>
        /// <param name="NTREES">Number of trees in tree data.</param>
        /// <param name="TDATAI">Tree data.</param>
        /// <param name="TDATAR">Tree data.</param>
        /// <param name="SBA">Total basal area per acre.</param>
        /// <param name="TPA">Total trees per acre.</param>
        /// <param name="SCCF">Total crown competition factor.</param>
        /// <param name="BAL">Basal area competition range vector of length 500 indexed by DBH in tenths of an inch.</param>
        /// <param name="BALL">Basal area competition range vector of length 51 for trees 50-100 inches DBH.</param>
        /// <param name="CCFL">Crown competition factor range vector of length 500 indexed by DBH in tenths of an inch.</param>
        /// <param name="CCFLL">Crown competition factor range vector of length 51 for trees 50-100 inches DBH.</param>
        /// <remarks>
        /// Trees of DBH larger than 100 inches are treated as if their diameter was 100 inches.
        /// </remarks>
        public static void SSTATS(Variant VERSION, int NTREES, int[,] TDATAI, float[,] TDATAR, out float SBA, out float TPA, out float SCCF, float[] BAL, float[] BALL, float[] CCFL, float[] CCFLL)
        {
            // BUGBUG doesn't check length of CCFL, BAL, CCFLL, and BALL
            for (int I = 0; I < 500; ++I)
            {
                CCFL[I] = 0.0F;
                BAL[I] = 0.0F;
            }
            for (int I = 0; I < 51; ++I)
            {
                CCFLL[I] = 0.0F;
                BALL[I] = 0.0F;
            }

            SBA = 0.0F;
            SCCF = 0.0F;
            TPA = 0.0F;
            for (int I = 0; I < NTREES; ++I)
            {
                if (TDATAR[I, 3] < 0.0001F)
                {
                    continue;
                }
                int ISPGRP = TDATAI[I, 1];
                float DBH = TDATAR[I, 0];
                float HT = TDATAR[I, 1];
                float EXPAN = TDATAR[I, 3];
                float BA = DBH * DBH * EXPAN * 0.005454154F;
                SBA = SBA + BA;
                TPA = TPA + EXPAN;

                float MCW;
                switch (VERSION)
                {
                    case Variant.Swo:
                        CrownGrowth.MCW_SWO(ISPGRP, DBH, HT, out MCW);
                        break;
                    case Variant.Nwo:
                        CrownGrowth.MCW_NWO(ISPGRP, DBH, HT, out MCW);
                        break;
                    case Variant.Smc:
                        CrownGrowth.MCW_SMC(ISPGRP, DBH, HT, out MCW);
                        break;
                    case Variant.Rap:
                        CrownGrowth.MCW_RAP(ISPGRP, DBH, HT, out MCW);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                // (DOUG? Where does 0.001803 come from? Why is it species invariant? Todd: check FOR 322 notes.)
                // BUGBUG duplicates index calculations of DiameterGrowth.GET_BAL()
                float CCF = 0.001803F * MCW * MCW * EXPAN;
                SCCF = SCCF + CCF;
                if (DBH > 50.0F)
                {
                    int L = (int)(DBH - 50.0F);
                    if (L > 51)
                    {
                        // (DOUG? Why limit DBH to 100 inches?)
                        L = 51;
                    }

                    // add large tree to all competition diameter classes
                    // (DOUG? Why are trees 50+ inches DBH all competitors to each other in the 500 vectors?)
                    for (int K = 0; K < 500; ++K)
                    {
                        // (PERF? this is O(500N), would run in O(N) + O(500) if moved to initialization)
                        CCFL[K] = CCFL[K] + CCF;
                        BAL[K] = BAL[K] + BA;
                    }
                    for (int K = 0; K < L - 1; ++K)
                    {
                        CCFLL[K] = CCFLL[K] + CCF;
                        BALL[K] = BALL[K] + BA;
                    }
                }
                else
                {
                    int L = (int)(DBH * 10.0F + 0.5F);
                    for (int K = 0; K < L - 1; ++K)
                    {
                        CCFL[K] = CCFL[K] + CCF;
                        BAL[K] = BAL[K] + BA;
                    }
                }
            }
        }
    }
}
