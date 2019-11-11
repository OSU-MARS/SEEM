using System;

namespace Osu.Cof.Organon
{
    internal static class Stats
    {
        /// <summary>
        /// Estimate red alder site index from conifer site index
        /// </summary>
        /// <param name="SITE_1">Conifer site index from ground.</param>
        /// <returns>Red alder site index from ground?</returns>
        public static float ConiferToRedAlderSiteIndex(float SITE_1)
        {
            return 9.73F + 0.64516F * SITE_1;
        }

        public static void RASITE(float H, float A, out float SI)
        {
            // RED ALDER SITE INDEX EQUATION FROM WORTHINGTON, JOHNSON, STAEBLER AND LLOYD(1960) PNW RESEARCH PAPER 36
            SI = (0.60924F + 19.538F / A) * H;
        }

        /// <summary>
        /// Calculate stand statistics.
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="standBasalArea">Total basal area per acre.</param>
        /// <param name="treesPerAcre">Total trees per acre.</param>
        /// <param name="standCrownCompetitionFactor">Total crown competition factor.</param>
        /// <param name="BAL">Basal area competition range vector of length 500 indexed by DBH in tenths of an inch.</param>
        /// <param name="BALL">Basal area competition range vector of length 51 for trees 50-100 inches DBH.</param>
        /// <param name="CCFL">Crown competition factor range vector of length 500 indexed by DBH in tenths of an inch.</param>
        /// <param name="CCFLL">Crown competition factor range vector of length 51 for trees 50-100 inches DBH.</param>
        /// <remarks>
        /// Trees of DBH larger than 100 inches are treated as if their diameter was 100 inches.
        /// </remarks>
        public static void SSTATS(Variant variant, Stand stand, out float standBasalArea, out float treesPerAcre, out float standCrownCompetitionFactor, float[] BAL, float[] BALL, float[] CCFL, float[] CCFLL)
        {
            // BUGBUG doesn't check length of CCFL, BAL, CCFLL, and BALL
            for (int competitionIndex = 0; competitionIndex < 500; ++competitionIndex)
            {
                CCFL[competitionIndex] = 0.0F;
                BAL[competitionIndex] = 0.0F;
            }
            for (int competitionIndex = 0; competitionIndex < 51; ++competitionIndex)
            {
                CCFLL[competitionIndex] = 0.0F;
                BALL[competitionIndex] = 0.0F;
            }

            standBasalArea = 0.0F;
            standCrownCompetitionFactor = 0.0F;
            treesPerAcre = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                if (expansionFactor < 0.0001F)
                {
                    continue;
                }

                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float basalArea = 0.005454154F * dbhInInches * dbhInInches * expansionFactor;
                standBasalArea += basalArea;
                treesPerAcre += expansionFactor;

                int speciesGroup = stand.SpeciesGroup[treeIndex];
                float maxCrownWidth;
                switch (variant)
                {
                    case Variant.Swo:
                        CrownGrowth.MCW_SWO(speciesGroup, dbhInInches, heightInFeet, out maxCrownWidth);
                        break;
                    case Variant.Nwo:
                        CrownGrowth.MCW_NWO(speciesGroup, dbhInInches, heightInFeet, out maxCrownWidth);
                        break;
                    case Variant.Smc:
                        CrownGrowth.MCW_SMC(speciesGroup, dbhInInches, heightInFeet, out maxCrownWidth);
                        break;
                    case Variant.Rap:
                        CrownGrowth.MCW_RAP(speciesGroup, dbhInInches, heightInFeet, out maxCrownWidth);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                // (DOUG? Where does 0.001803 come from? Why is it species invariant? Todd: check FOR 322 notes.)
                // BUGBUG duplicates index calculations of DiameterGrowth.GET_BAL()
                float crownCompetitionFactor = 0.001803F * maxCrownWidth * maxCrownWidth * expansionFactor;
                standCrownCompetitionFactor += crownCompetitionFactor;
                if (dbhInInches > 50.0F)
                {
                    int L = (int)(dbhInInches - 50.0F);
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
                        CCFL[K] = CCFL[K] + crownCompetitionFactor;
                        BAL[K] = BAL[K] + basalArea;
                    }
                    for (int K = 0; K < L - 1; ++K)
                    {
                        CCFLL[K] = CCFLL[K] + crownCompetitionFactor;
                        BALL[K] = BALL[K] + basalArea;
                    }
                }
                else
                {
                    int L = (int)(dbhInInches * 10.0F + 0.5F);
                    for (int K = 0; K < L - 1; ++K)
                    {
                        CCFL[K] = CCFL[K] + crownCompetitionFactor;
                        BAL[K] = BAL[K] + basalArea;
                    }
                }
            }

            float treeCountAsFloat = (float)stand.TreeRecordCount;
            standBasalArea /= treeCountAsFloat;
            standCrownCompetitionFactor /= treeCountAsFloat;
            treesPerAcre /= treeCountAsFloat;
        }
    }
}
