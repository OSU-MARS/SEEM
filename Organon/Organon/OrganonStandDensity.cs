using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonStandDensity
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

        public OrganonStandDensity(OrganonStandDensity other)
        {
            this.BasalAreaPerAcre = other.BasalAreaPerAcre;
            this.CrownCompetitionFactor = other.CrownCompetitionFactor;
            this.LargeTreeBasalAreaLarger = new float[other.LargeTreeBasalAreaLarger.Length];
            this.LargeTreeCrownCompetition = new float[other.LargeTreeCrownCompetition.Length];
            this.SmallTreeBasalAreaLarger = new float[other.SmallTreeBasalAreaLarger.Length];
            this.SmallTreeCrownCompetition = new float[other.SmallTreeCrownCompetition.Length];
            this.TreesPerAcre = other.TreesPerAcre;

            Array.Copy(other.LargeTreeBasalAreaLarger, 0, this.LargeTreeBasalAreaLarger, 0, other.LargeTreeBasalAreaLarger.Length);
            Array.Copy(other.LargeTreeCrownCompetition, 0, this.LargeTreeCrownCompetition, 0, other.LargeTreeCrownCompetition.Length);
            Array.Copy(other.SmallTreeBasalAreaLarger, 0, this.SmallTreeBasalAreaLarger, 0, other.SmallTreeBasalAreaLarger.Length);
            Array.Copy(other.SmallTreeCrownCompetition, 0, this.SmallTreeCrownCompetition, 0, other.SmallTreeCrownCompetition.Length);
        }

        /// <remarks>
        /// Trees of DBH larger than 100 inches are treated as if their diameter was 100 inches.
        /// </remarks>
        public OrganonStandDensity(OrganonStand stand, OrganonVariant variant)
        {
            this.BasalAreaPerAcre = 0.0F;
            this.CrownCompetitionFactor = 0.0F;
            this.LargeTreeBasalAreaLarger = new float[51];
            this.LargeTreeCrownCompetition = new float[51];
            this.SmallTreeBasalAreaLarger = new float[501];
            this.SmallTreeCrownCompetition = new float[501];
            this.TreesPerAcre = 0.0F;

            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (expansionFactor < 0.0001F)
                    {
                        continue;
                    }

                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    float basalArea = treesOfSpecies.GetBasalArea(treeIndex);
                    this.BasalAreaPerAcre += basalArea;
                    this.TreesPerAcre += expansionFactor;

                    float maxCrownWidth = variant.GetMaximumCrownWidth(treesOfSpecies.Species, dbhInInches, heightInFeet);

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
                float relativeHeight = (float)(heightIndex - 1) * (crownCompetitionByHeight[^1] / 40.0F);
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
        public static float[] GetCrownCompetitionByHeight(OrganonVariant variant, OrganonStand stand)
        {
            float[] crownClosureByRelativeHeight = new float[41];
            crownClosureByRelativeHeight[40] = Single.MinValue;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    if (heightInFeet > crownClosureByRelativeHeight[40])
                    {
                        crownClosureByRelativeHeight[40] = heightInFeet;
                    }
                }
            }

            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    float crownRatio = treesOfSpecies.CrownRatio[treeIndex];
                    float crownLengthInFeet = crownRatio * heightInFeet;
                    float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    float maxCrownWidth = variant.GetMaximumCrownWidth(species, dbhInInches, heightInFeet);
                    float largestCrownWidth = variant.GetLargestCrownWidth(species, maxCrownWidth, crownRatio, dbhInInches, heightInFeet);
                    float heightToLargestCrownWidth = variant.GetHeightToLargestCrownWidth(species, heightInFeet, crownRatio);
                    OrganonStandDensity.GetCrownCompetitionByHeight(variant, species, heightToLargestCrownWidth, largestCrownWidth, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, crownClosureByRelativeHeight);
                }
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
