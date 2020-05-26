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
            this.LargeTreeBasalAreaLarger = new float[100 - 50 + 1];
            this.LargeTreeCrownCompetition = new float[100 - 50 + 1];
            this.SmallTreeBasalAreaLarger = new float[10 * 50 + 1];
            this.SmallTreeCrownCompetition = new float[10 * 50 + 1];
            this.TreesPerAcre = 0.0F;

            // find each tree's diameter class and add its CCF to its diameter class and all smaller diameter classes
            // Trees less than 50 inches DBH are considered small and tracked by tenth inch diameter classes. Trees up to 100 inches DBH use one inch
            // diameter classes from 50 to 100 inches. This is done in two passes to run in O(N trees + n diameter classes).
            // 1) Accumulate each tree's BA and CCF in its diameter class.
            // 2) Add the running BA and CCF sums of all larger diameter classes to all smaller ones.
            // (The Fortran version of this used a single pass and therefore ran in O(Nn) time.)
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (expansionFactor <= 0.0F)
                    {
                        continue;
                    }
                    this.TreesPerAcre += expansionFactor;

                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    float basalAreaPerAcre = treesOfSpecies.GetBasalArea(treeIndex);
                    float maxCrownWidth = variant.GetMaximumCrownWidth(treesOfSpecies.Species, dbhInInches, heightInFeet);
                    float crownCompetitionFactor = Constant.CrownCompetionConstantEnglish * maxCrownWidth * maxCrownWidth * expansionFactor;

                    // keep diameter class calculation in sync with GetBasalAreaLarger() and GetCrownCompetitionFactorLarger()
                    if (dbhInInches > 50.0F)
                    {
                        int largeTreeDiameterClass = (int)(dbhInInches - 50.0F);
                        if (largeTreeDiameterClass > 50)
                        {
                            largeTreeDiameterClass = 50;
                        }

                        this.LargeTreeBasalAreaLarger[largeTreeDiameterClass] += basalAreaPerAcre;
                        this.LargeTreeCrownCompetition[largeTreeDiameterClass] += crownCompetitionFactor;
                        this.SmallTreeBasalAreaLarger[^1] += basalAreaPerAcre;
                        this.SmallTreeCrownCompetition[^1] += crownCompetitionFactor;
                    }
                    else
                    {
                        int smallTreeDiameterClass = (int)(10.0F * dbhInInches);
                        this.SmallTreeBasalAreaLarger[smallTreeDiameterClass] += basalAreaPerAcre;
                        this.SmallTreeCrownCompetition[smallTreeDiameterClass] += crownCompetitionFactor;
                    }
                }
            }

            for (int largeDiameterClassIndex = this.LargeTreeBasalAreaLarger.Length - 1; largeDiameterClassIndex >= 0; --largeDiameterClassIndex)
            {
                this.LargeTreeBasalAreaLarger[largeDiameterClassIndex] += this.BasalAreaPerAcre;
                this.LargeTreeCrownCompetition[largeDiameterClassIndex] += this.CrownCompetitionFactor;

                this.BasalAreaPerAcre = this.LargeTreeBasalAreaLarger[largeDiameterClassIndex];
                this.CrownCompetitionFactor = this.LargeTreeCrownCompetition[largeDiameterClassIndex];
            }
            for (int smallDiameterClassIndex = this.SmallTreeBasalAreaLarger.Length - 1; smallDiameterClassIndex >= 0; --smallDiameterClassIndex)
            {
                this.SmallTreeBasalAreaLarger[smallDiameterClassIndex] += this.BasalAreaPerAcre;
                this.SmallTreeCrownCompetition[smallDiameterClassIndex] += this.CrownCompetitionFactor;

                this.BasalAreaPerAcre = this.SmallTreeBasalAreaLarger[smallDiameterClassIndex];
                this.CrownCompetitionFactor = this.SmallTreeCrownCompetition[smallDiameterClassIndex];
            }
        }

        public void CopyFrom(OrganonStandDensity other)
        {
            Debug.Assert(Object.ReferenceEquals(this, other) == false);

            this.BasalAreaPerAcre = other.BasalAreaPerAcre;
            this.CrownCompetitionFactor = other.CrownCompetitionFactor;
            Array.Copy(other.LargeTreeBasalAreaLarger, 0, this.LargeTreeBasalAreaLarger, 0, other.LargeTreeBasalAreaLarger.Length);
            Array.Copy(other.LargeTreeCrownCompetition, 0, this.LargeTreeCrownCompetition, 0, other.LargeTreeCrownCompetition.Length);
            Array.Copy(other.SmallTreeBasalAreaLarger, 0, this.SmallTreeBasalAreaLarger, 0, other.SmallTreeBasalAreaLarger.Length);
            Array.Copy(other.SmallTreeCrownCompetition, 0, this.SmallTreeCrownCompetition, 0, other.SmallTreeCrownCompetition.Length);
            this.TreesPerAcre = other.TreesPerAcre;
        }

        public float GetBasalAreaLarger(float dbhInInches)
        {
            // keep diameter class calculation in sync with GetBasalAreaLarger() and GetCrownCompetitionFactorLarger()
            if (dbhInInches > 100.0F)
            {
                return 0.0F;
            }
            
            if (dbhInInches >= 50.0F)
            {
                int largeTreeIndex = (int)(dbhInInches - 50.0F);
                return this.LargeTreeBasalAreaLarger[largeTreeIndex];
            }

            int smallTreeIndex = (int)(10.0F * dbhInInches);
            return this.SmallTreeBasalAreaLarger[smallTreeIndex];
        }

        /// <summary>
        /// Find crown closure.
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <returns>Array indicating crown closure at height relative to tallest tree in stand with last value being height of tallest tree.</returns>
        public static float[] GetCrownCompetitionByHeight(OrganonVariant variant, OrganonStand stand)
        {
            // find tallest tree
            float[] crownClosureByRelativeHeight = new float[Constant.HeightStrata + 1];
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    if (heightInFeet > crownClosureByRelativeHeight[^1])
                    {
                        crownClosureByRelativeHeight[^1] = heightInFeet;
                    }
                }
            }

            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                variant.AddCrownCompetitionByHeight(treesOfSpecies, crownClosureByRelativeHeight);
            }
            return crownClosureByRelativeHeight;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbhInInches">Tree's diameter at breast height (inches)</param>
        /// <returns>Crown competition factor for specified DBH.</returns>
        public float GetCrownCompetitionFactorLarger(float dbhInInches)
        {
            // keep diameter class calculation in sync with GetBasalAreaLarger() and GetCrownCompetitionFactorLarger()
            if (dbhInInches > 100.0F)
            {
                return 0.0F;
            }

            if (dbhInInches > 50.0F)
            {
                int largeTreeDiameterClass = (int)(dbhInInches - 50.0F);
                return this.LargeTreeCrownCompetition[largeTreeDiameterClass];
            }

            int smallTreeDiameterClass = (int)(10.0 * dbhInInches);
            return this.SmallTreeCrownCompetition[smallTreeDiameterClass];
        }
    }
}
