namespace Osu.Cof.Organon
{
    internal class StandDensity
    {
        /// <summary>
        /// Basal area competition range vector of length 51 for trees 50-100 inches DBH. (BALL)
        /// </summary>
        public float[] LargeTreeBasalAreaLarger { get; private set; }
        /// <summary>
        /// Crown competition factor range vector of length 51 for trees 50-100 inches DBH. (CCFLL)
        /// </summary>
        public float[] LargeTreeCrownCompetition { get; private set; }

        /// <summary>
        /// Basal area competition range vector of length 500 indexed by DBH in tenths of an inch. (BAL)
        /// </summary>
        public float[] SmallTreeBasalAreaLarger { get; private set; }

        /// <summary>
        /// Crown competition factor range vector of length 500 indexed by DBH in tenths of an inch. (CCFL)
        /// </summary>
        public float[] SmallTreeCrownCompetition { get; private set; }

        public StandDensity()
        {
            this.LargeTreeBasalAreaLarger = new float[51];
            this.LargeTreeCrownCompetition = new float[51];
            this.SmallTreeBasalAreaLarger = new float[500];
            this.SmallTreeCrownCompetition = new float[500];
        }

        public float GET_BAL(float dbhInInches)
        {
            if (dbhInInches > 100.0F)
            {
                return 0.0F;
            }
            
            if (dbhInInches > 50.0F)
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
        public float GET_CCFL(float dbhInInches)
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
