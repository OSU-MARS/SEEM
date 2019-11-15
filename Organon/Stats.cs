using System;

namespace Osu.Cof.Organon
{
    internal static class Stats
    {
        /// <summary>
        /// Calculate stand statistics.
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="standBasalArea">Total basal area per acre.</param>
        /// <param name="treesPerAcre">Total trees per acre.</param>
        /// <param name="standCrownCompetitionFactor">Total crown competition factor.</param>
        /// <param name="density">Basal area and crown competiation.</param>
        /// <remarks>
        /// Trees of DBH larger than 100 inches are treated as if their diameter was 100 inches.
        /// </remarks>
        public static void SSTATS(Variant variant, Stand stand, out float standBasalArea, out float treesPerAcre, out float standCrownCompetitionFactor, out StandDensity density)
        {
            float treeCountAsFloat = (float)stand.TreeRecordCount;
            density = new StandDensity();
            standBasalArea = 0.0F;
            standCrownCompetitionFactor = 0.0F;
            treesPerAcre = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float expansionFactor = stand.LiveExpansionFactor[treeIndex] / treeCountAsFloat;
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
                        throw VariantExtensions.CreateUnhandledVariantException(variant);
                }

                // (DOUG? Where does 0.001803 come from? Why is it species invariant? Todd: check FOR 322 notes.)
                // BUGBUG duplicates index calculations of DiameterGrowth.GET_BAL()
                float crownCompetitionFactor = 0.001803F * maxCrownWidth * maxCrownWidth * expansionFactor;
                standCrownCompetitionFactor += crownCompetitionFactor;
                if (dbhInInches > 50.0F)
                {
                    int largeTreeLimit = (int)(dbhInInches - 50.0F);
                    if (largeTreeLimit > 51)
                    {
                        // (DOUG? Why limit DBH to 100 inches?)
                        largeTreeLimit = 51;
                    }

                    // add large tree to all competition diameter classes
                    // (DOUG? Why are trees 50+ inches DBH all competitors to each other in the 500 vectors?)
                    for (int smallTreeIndex = 0; smallTreeIndex < 500; ++smallTreeIndex)
                    {
                        // (PERF? this is O(500N), would run in O(N) + O(500) if moved to initialization)
                        density.SmallTreeCrownCompetition[smallTreeIndex] += crownCompetitionFactor;
                        density.SmallTreeBasalAreaLarger[smallTreeIndex] += basalArea;
                    }
                    for (int largeTreeIndex = 0; largeTreeIndex < largeTreeLimit - 1; ++largeTreeIndex)
                    {
                        density.LargeTreeCrownCompetition[largeTreeIndex] += crownCompetitionFactor;
                        density.LargeTreeBasalAreaLarger[largeTreeIndex] += basalArea;
                    }
                }
                else
                {
                    int smallTreeLimit = (int)(dbhInInches * 10.0F + 0.5F);
                    for (int smallTreeIndex = 0; smallTreeIndex < smallTreeLimit - 1; ++smallTreeIndex)
                    {
                        density.SmallTreeCrownCompetition[smallTreeIndex] += crownCompetitionFactor;
                        density.SmallTreeBasalAreaLarger[smallTreeIndex] += basalArea;
                    }
                }
            }
        }
    }
}
