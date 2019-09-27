using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Osu.Cof.Organon.Test
{
    public class TreeData
    {
        // accumulated expansion factor of dead trees from mortality chipping? (DOUG?)
        public float[] DeadExpansionFactor { get; private set; }

        // TDATAR[, 5] is variously assigned in rundll and wood quality to DBH, CR, and EF but is never consumed
        //   this suggests it's an output trace
        //     0    1       2            3                 4                     5  6              7
        //     DBH, height, crown ratio, expansion factor, crown ratio? (DOUG?), ?, crown ratio 2, expansion factor 2
        //                                                 expansion factor?
        // { { DBH, HT,     CR,          EF,               unused?,              ?, CR t + 1?,     CR t + 1? } }
        public float[,] Float { get; private set; }

        //     0,             1                2,                         3
        //     height growth, diameter growth, accumulated height growth, accumulated diameter growth 
        // { { HGRO,          DGRO,            GROWTH + GROWTH,           GROWTH + GROWTH } }
        public float[,] Growth { get; private set; }

        // valid range for species group is [0, 17]
        //     0        1              2
        //     species, species group, user data passthrough, not used by Organon
        // { { ISP,     ISPGRP,        USER } }
        public int[,] Integer { get; private set; }

        // IB, sometimes also named IIB
        public int MaxBigSixSpeciesGroupIndex { get; private set; }

        // ? (DOUG?)
        public float[] MGExpansionFactor { get; private set; }

        // primary site index
        public float PrimarySiteIndex { get; private set; }

        // secondary site index used for additional mortality calculations
        public float MortalitySiteIndex { get; private set; }

        // shadown crown ratio, apparently unused (DOUG?)
        public float[,] ShadowCrownRatio { get; private set; }

        public TriplingData Triple { get; private set; }

        public int UsedRecordCount { get; set; }

        protected TreeData(TreeData other)
            : this(other.MaxBigSixSpeciesGroupIndex, other.MaximumRecordCount, other.PrimarySiteIndex, other.MortalitySiteIndex)
        {
            other.DeadExpansionFactor.CopyTo(this.DeadExpansionFactor, 0);
            Buffer.BlockCopy(other.Float, 0, this.Float, 0, sizeof(float) * other.Float.Length);
            Buffer.BlockCopy(other.Growth, 0, this.Growth, 0, sizeof(float) * other.Growth.Length);
            Buffer.BlockCopy(other.Integer, 0, this.Integer, 0, sizeof(int) * other.Integer.Length);
            other.MGExpansionFactor.CopyTo(this.MGExpansionFactor, 0);
            Buffer.BlockCopy(other.ShadowCrownRatio, 0, this.ShadowCrownRatio, 0, sizeof(float) * other.ShadowCrownRatio.Length);
            this.Triple = new TriplingData(other.Triple);
            this.UsedRecordCount = other.UsedRecordCount;
        }

        protected TreeData(int maxBigSixSpeciesGroupIndex, int treeCount, float primarySiteIndex, float mortalitySiteIndex)
        {
            this.DeadExpansionFactor = new float[treeCount];
            this.Float = new float[treeCount, 8];
            this.Growth = new float[treeCount, 4];
            this.Integer = new int[treeCount, 3];
            this.MaxBigSixSpeciesGroupIndex = maxBigSixSpeciesGroupIndex;
            this.MGExpansionFactor = new float[treeCount];
            this.PrimarySiteIndex = primarySiteIndex;
            this.MortalitySiteIndex = mortalitySiteIndex;
            this.ShadowCrownRatio = new float[treeCount, 3];
            this.Triple = new TriplingData(treeCount);
            this.UsedRecordCount = 0;
        }

        public TreeData(Variant variant, int treeCount, float primarySiteIndex, float mortalitySiteIndex)
            : this((variant == Variant.Swo) ? 4 : 2, treeCount, primarySiteIndex, mortalitySiteIndex)
        {
        }

        public int MaximumRecordCount
        {
            get { return this.MGExpansionFactor.Length; }
        }

        public TreeData Clone()
        {
            return new TreeData(this);
        }

        public void FromArrays(int[] species, float[] dbhInInches, float[] heightInFeet, float[] crownRatio, float[] expansionFactor, float[] shadowCrownRatio)
        {
            if (species.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(species));
            }
            if (dbhInInches.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInInches));
            }
            if (heightInFeet.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(heightInFeet));
            }
            if (crownRatio.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(crownRatio));
            }
            if (expansionFactor.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(expansionFactor));
            }
            if (shadowCrownRatio.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(shadowCrownRatio));
            }

            for (int treeIndex = 0; treeIndex < this.MaximumRecordCount; ++treeIndex)
            {
                this.Integer[treeIndex, (int)TreePropertyInteger.Species] = species[treeIndex];
                this.Float[treeIndex, (int)TreePropertyFloat.Dbh] = dbhInInches[treeIndex];
                this.Float[treeIndex, (int)TreePropertyFloat.Height] = heightInFeet[treeIndex];
                this.Float[treeIndex, (int)TreePropertyFloat.CrownRatio] = crownRatio[treeIndex];
                this.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor] = expansionFactor[treeIndex];
                this.ShadowCrownRatio[treeIndex, 0] = shadowCrownRatio[treeIndex];
            }
        }

        public void FromArrays(int[] species, int[] user, float[] dbhInInches, float[] heightInFeet, float[] crownRatio,
                               float[] expansionFactor, float[] shadowCrownRatio, float[] diameterGrowthInInches, float[] heightGrowthInFeet,
                               float[] crownChange, float[] shadowCrownRatioChange)
        {
            if (user.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(user));
            }
            if (diameterGrowthInInches.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(diameterGrowthInInches));
            }
            if (heightGrowthInFeet.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(heightGrowthInFeet));
            }
            if (crownChange.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(crownChange));
            }
            if (shadowCrownRatioChange.Length != this.MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(shadowCrownRatioChange));
            }

            this.FromArrays(species, dbhInInches, heightInFeet, crownRatio, expansionFactor, shadowCrownRatio);
        }

        public int GetBigSixSpeciesRecordCount()
        {
            int bigSixRecords = 0;
            for (int treeIndex = 0; treeIndex < this.UsedRecordCount; ++treeIndex)
            {
                int speciesGroup = this.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup];
                if (speciesGroup <= this.MaxBigSixSpeciesGroupIndex)
                {
                    ++bigSixRecords;
                }
            }
            return bigSixRecords;
        }

        public void GetEmptyArrays(out float[] dbhInInchesAtEndOfCycle, out float[] heightAtEndOfCycle, out float[] crownRatioAtEndOfCycle,
                                   out float[] shadowCrownRatioAtEndOfCycle, out float[] expansionFactorAtEndOfCycle)
        {
            dbhInInchesAtEndOfCycle = new float[this.MaximumRecordCount];
            heightAtEndOfCycle = new float[this.MaximumRecordCount];
            crownRatioAtEndOfCycle = new float[this.MaximumRecordCount];
            shadowCrownRatioAtEndOfCycle = new float[this.MaximumRecordCount];
            expansionFactorAtEndOfCycle = new float[this.MaximumRecordCount];
        }

        public void ToArrays(out int[] species, out float[] dbhInInches, out float[] heightInFeet, out float[] crownRatio,
                             out float[] expansionFactor, out float[] shadowCrownRatio)
        {
            species = new int[this.MaximumRecordCount];
            dbhInInches = new float[this.MaximumRecordCount];
            heightInFeet = new float[this.MaximumRecordCount];
            crownRatio = new float[this.MaximumRecordCount];
            expansionFactor = new float[this.MaximumRecordCount];
            shadowCrownRatio = new float[this.MaximumRecordCount];
            for (int treeIndex = 0; treeIndex < this.MaximumRecordCount; ++treeIndex)
            {
                species[treeIndex] = this.Integer[treeIndex, (int)TreePropertyInteger.Species];
                dbhInInches[treeIndex] = this.Float[treeIndex, (int)TreePropertyFloat.Dbh];
                heightInFeet[treeIndex] = this.Float[treeIndex, (int)TreePropertyFloat.Height];
                crownRatio[treeIndex] = this.Float[treeIndex, (int)TreePropertyFloat.CrownRatio];
                expansionFactor[treeIndex] = this.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor];
                shadowCrownRatio[treeIndex] = this.ShadowCrownRatio[treeIndex, 0];
            }
        }

        public void ToArrays(out int[] species, out int[] user, out float[] dbhInInches, out float[] heightInFeet, out float[] crownRatio,
                             out float[] expansionFactor, out float[] shadowCrownRatio, out float[] diameterGrowthInInches,
                             out float[] heightGrowthInFeet, out float[] crownChange, out float[] shadowCrownRatioChange)
        {
            this.ToArrays(out species, out dbhInInches, out heightInFeet, out crownRatio, out expansionFactor, out shadowCrownRatio);

            user = new int[this.MaximumRecordCount];
            diameterGrowthInInches = new float[this.MaximumRecordCount];
            heightGrowthInFeet = new float[this.MaximumRecordCount];
            crownChange = new float[this.MaximumRecordCount];
            shadowCrownRatioChange = new float[this.MaximumRecordCount];
            for (int treeIndex = 0; treeIndex < this.MaximumRecordCount; ++treeIndex)
            {
                user[treeIndex] = treeIndex;
            }
        }

        public void WriteAsCsv(TestContext testContext, Variant variant, int simulationStep)
        {
            for (int treeIndex = 0; treeIndex < this.UsedRecordCount; ++treeIndex)
            {
                int treeSpecies = this.Integer[treeIndex, (int)TreePropertyInteger.Species];
                int treeSpeciesGroup = this.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup];
                int treeUserData = this.Integer[treeIndex, (int)TreePropertyInteger.User];
                float treeDbhInInches = this.Float[treeIndex, (int)TreePropertyFloat.Dbh];
                float treeHeightInFeet = this.Float[treeIndex, (int)TreePropertyFloat.Height];
                float treeExpansionFactor = this.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor];
                float treeCrownRatio = this.Float[treeIndex, (int)TreePropertyFloat.CrownRatio];
                float treeDeadExpansionFactor = this.DeadExpansionFactor[treeIndex];
                float treeMGExpansionFactor = this.MGExpansionFactor[treeIndex];
                float treeShadowCrownRatio0 = this.ShadowCrownRatio[treeIndex, 0];
                float treeShadowCrownRatio1 = this.ShadowCrownRatio[treeIndex, 1];
                float treeShadowCrownRatio2 = this.ShadowCrownRatio[treeIndex, 2];
                testContext.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                                      variant, simulationStep, treeIndex, treeSpecies, treeSpeciesGroup, treeUserData,
                                      treeDbhInInches, treeHeightInFeet, treeExpansionFactor, treeDeadExpansionFactor,
                                      treeMGExpansionFactor, treeCrownRatio, treeShadowCrownRatio0, treeShadowCrownRatio1,
                                      treeShadowCrownRatio2);
            }
        }
    }
}
