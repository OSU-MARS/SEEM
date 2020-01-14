using System.Diagnostics;

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

        public static float GetCrownCompetition(OrganonVariant variant, Stand stand)
        {
            float[] crownCompetitionFactorByHeight = new float[41];
            crownCompetitionFactorByHeight[40] = stand.Height[0];
            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float heightInFeet = stand.Height[treeIndex];
                if (heightInFeet > crownCompetitionFactorByHeight[40])
                {
                    crownCompetitionFactorByHeight[40] = heightInFeet;
                }
            }

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = stand.Species[treeIndex];
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float crownRatio = stand.CrownRatio[treeIndex];

                float CL = crownRatio * heightInFeet;
                float HCB = heightInFeet - CL;
                float EXPFAC = expansionFactor / (float)stand.NumberOfPlots;
                float MCW = variant.GetMaximumCrownWidth(species, dbhInInches, heightInFeet);
                float LCW = variant.GetLargestCrownWidth(species, MCW, crownRatio, dbhInInches, heightInFeet);
                float HLCW = variant.GetHeightToLargestCrownWidth(species, heightInFeet, crownRatio);
                StandDensity.GetCrownCompetitionByHeight(variant, species, HLCW, LCW, heightInFeet, dbhInInches, HCB, EXPFAC, crownCompetitionFactorByHeight);
            }
            float crownCompetitionFactor = crownCompetitionFactorByHeight[0];
            Debug.Assert(crownCompetitionFactor >= 0.0F);
            Debug.Assert(crownCompetitionFactor <= 100.0F);
            return crownCompetitionFactor;
        }


        public static void GetCrownCompetitionByHeight(OrganonVariant variant, FiaCode species, float heightToLargestCrownWidth, float largestCrownWidth, float HT, float DBH, float heightToCrownBase, float EXPAN, float[] crownCompetitionByHeight)
        {
            float XHLCW = heightToLargestCrownWidth;
            float XLCW = largestCrownWidth;
            if (heightToCrownBase > heightToLargestCrownWidth)
            {
                XHLCW = heightToCrownBase;
                XLCW = variant.GetCrownWidth(species, heightToLargestCrownWidth, largestCrownWidth, HT, DBH, XHLCW);
            }

            for (int heightIndex = crownCompetitionByHeight.Length - 2; heightIndex >= 0; --heightIndex)
            {
                float relativeHeight = (float)(heightIndex - 1) * (crownCompetitionByHeight[crownCompetitionByHeight.Length - 1] / 40.0F);
                float crownWidth = 0.0F;
                if (relativeHeight <= XHLCW)
                {
                    crownWidth = XLCW;
                }
                else if (relativeHeight > XHLCW && relativeHeight < HT)
                {
                    crownWidth = variant.GetCrownWidth(species, heightToLargestCrownWidth, largestCrownWidth, HT, DBH, relativeHeight);
                }
                float crownCompetitionFactor = 0.001803F * EXPAN * crownWidth * crownWidth;
                crownCompetitionByHeight[heightIndex] = crownCompetitionByHeight[heightIndex] + crownCompetitionFactor;
            }
        }

        /// <summary>
        /// Find crown closure.
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <returns>Array indicating crown closure at height relative to tallest tree in stand with last value being height of tallest tree.</returns>
        public static float[] GetCrownCompetitionByHeight(OrganonVariant variant, Stand stand)
        {
            float[] crownClosureByRelativeHeight = new float[41];
            crownClosureByRelativeHeight[40] = stand.Height[0];
            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.Height[treeIndex] > crownClosureByRelativeHeight[40])
                {
                    crownClosureByRelativeHeight[40] = stand.Height[treeIndex];
                }
            }

            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = stand.Species[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float crownRatio = stand.CrownRatio[treeIndex];
                float crownLengthInFeet = crownRatio * heightInFeet;
                float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                float MCW = variant.GetMaximumCrownWidth(species, dbhInInches, heightInFeet);
                float LCW = variant.GetLargestCrownWidth(species, MCW, crownRatio, dbhInInches, heightInFeet);
                float HLCW = variant.GetHeightToLargestCrownWidth(species, heightInFeet, crownRatio);
                GetCrownCompetitionByHeight(variant, species, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, crownClosureByRelativeHeight);
            }
            return crownClosureByRelativeHeight;
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
