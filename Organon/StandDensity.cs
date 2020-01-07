namespace Osu.Cof.Organon
{
    public class StandDensity
    {
        /// <summary>
        /// Basal area in square feet per acre.
        /// </summary>
        public float BasalAreaPerAcre { get; set; }

        public float CrownCompetitionFactor { get; set; }

        /// <summary>
        /// Basal area competition range vector of length 51 for trees 50-100 inches DBH. (BALL)
        /// </summary>
        public float[] LargeTreeBasalAreaLarger { get; private set; }
        /// <summary>
        /// Crown competition factor range vector of length 51 for trees 50-100 inches DBH. (CCFLL)
        /// </summary>
        public float[] LargeTreeCrownCompetition { get; private set; }

        /// <summary>
        /// Basal area competition range vector of length 501 indexed by DBH in tenths of an inch. (BAL)
        /// </summary>
        public float[] SmallTreeBasalAreaLarger { get; private set; }

        /// <summary>
        /// Crown competition factor range vector of length 501 indexed by DBH in tenths of an inch. (CCFL)
        /// </summary>
        public float[] SmallTreeCrownCompetition { get; private set; }

        public float TreesPerAcre { get; set; }

        /// <remarks>
        /// Trees of DBH larger than 100 inches are treated as if their diameter was 100 inches.
        /// </remarks>
        public StandDensity(Stand stand, OrganonVariant variant)
        {
            this.BasalAreaPerAcre = 0.0F;
            this.CrownCompetitionFactor = 0.0F;
            this.LargeTreeBasalAreaLarger = new float[51];
            this.LargeTreeCrownCompetition = new float[51];
            this.SmallTreeBasalAreaLarger = new float[501];
            this.SmallTreeCrownCompetition = new float[501];
            this.TreesPerAcre = 0.0F;

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                if (expansionFactor < 0.0001F)
                {
                    continue;
                }

                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float basalArea = stand.GetBasalArea(treeIndex);
                this.BasalAreaPerAcre += basalArea;
                this.TreesPerAcre += expansionFactor;

                FiaCode species = stand.Species[treeIndex];
                float maxCrownWidth = variant.GetMaximumCrownWidth(species, dbhInInches, heightInFeet);

                // 0.001803 = 100 * pi / (4 * 42560) from definition of crown competition factor
                float crownCompetitionFactor = 0.001803F * maxCrownWidth * maxCrownWidth * expansionFactor;
                this.CrownCompetitionFactor += crownCompetitionFactor;
                if (dbhInInches > 50.0F)
                {
                    int largeTreeLimit = (int)(dbhInInches - 50.0F);
                    if (largeTreeLimit > 51)
                    {
                        largeTreeLimit = 51;
                    }

                    // add large tree to all competition diameter classes
                    for (int smallTreeIndex = 0; smallTreeIndex < this.SmallTreeCrownCompetition.Length; ++smallTreeIndex)
                    {
                        // (PERF? this is O(500N), would run in O(N) + O(500) if moved to initialization)
                        this.SmallTreeCrownCompetition[smallTreeIndex] += crownCompetitionFactor;
                        this.SmallTreeBasalAreaLarger[smallTreeIndex] += basalArea;
                    }
                    for (int largeTreeIndex = 0; largeTreeIndex < largeTreeLimit - 1; ++largeTreeIndex)
                    {
                        this.LargeTreeCrownCompetition[largeTreeIndex] += crownCompetitionFactor;
                        this.LargeTreeBasalAreaLarger[largeTreeIndex] += basalArea;
                    }
                }
                else
                {
                    int smallTreeLimit = (int)(dbhInInches * 10.0F + 0.5F);
                    for (int smallTreeIndex = 0; smallTreeIndex < smallTreeLimit - 1; ++smallTreeIndex)
                    {
                        this.SmallTreeCrownCompetition[smallTreeIndex] += crownCompetitionFactor;
                        this.SmallTreeBasalAreaLarger[smallTreeIndex] += basalArea;
                    }
                }
            }
        }

        public float GetBasalAreaLarger(float dbhInInches)
        {
            if (dbhInInches > 100.0F)
            {
                return 0.0F;
            }
            
            if (dbhInInches >= 50.0F)
            {
                // BUGBUG missing clamp to avoid lookups beyond end of BALL1
                int largeTreeIndex = (int)(dbhInInches - 50.0F);
                return this.LargeTreeBasalAreaLarger[largeTreeIndex];
            }

            int smallTreeIndex = (int)(dbhInInches * 10.0F + 0.5F);
            return this.SmallTreeBasalAreaLarger[smallTreeIndex];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbhInInches">Tree's diameter at breast height (inches)</param>
        /// <param name="CCFLL1">Stand crown competitition data.</param>
        /// <param name="CCFL1">Stand crown competitition data.</param>
        /// <returns>Crown competition factor for specified DBH.</returns>
        public float GetCrownCompetitionFactorLarger(float dbhInInches)
        {
            if (dbhInInches > 100.0F)
            {
                return 0.0F;
            }

            if (dbhInInches > 50.0F)
            {
                int largeTreeIndex = (int)(dbhInInches - 49.0F) - 1;
                return this.LargeTreeCrownCompetition[largeTreeIndex];
            }

            int smallTreeIndex = (int)(dbhInInches * 10.0 + 0.5) - 1;
            return this.SmallTreeCrownCompetition[smallTreeIndex];
        }
    }
}
