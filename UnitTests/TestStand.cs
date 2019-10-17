using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Osu.Cof.Organon.Test
{
    public class TestStand : Stand
    {
        public TestStand(Variant variant, int ageInYears, int treeCount, float primarySiteIndex, float mortalitySiteIndex)
            : base(ageInYears, treeCount, primarySiteIndex, mortalitySiteIndex, (variant == Variant.Swo) ? 4 : 2)
        {
        }

        protected TestStand(TestStand other)
            : base(other)
        {
        }

        public TestStand Clone()
        {
            return new TestStand(this);
        }

        public void FromArrays(float[] dbhInInches, float[] heightInFeet, float[] crownRatio, float[] expansionFactor)
        {
            if (dbhInInches.Length != this.MaximumTreeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInInches));
            }
            if (heightInFeet.Length != this.MaximumTreeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(heightInFeet));
            }
            if (crownRatio.Length != this.MaximumTreeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(crownRatio));
            }
            if (expansionFactor.Length != this.MaximumTreeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(expansionFactor));
            }

            for (int treeIndex = 0; treeIndex < this.MaximumTreeRecords; ++treeIndex)
            {
                this.Float[treeIndex, (int)TreePropertyFloat.Dbh] = dbhInInches[treeIndex];
                this.Float[treeIndex, (int)TreePropertyFloat.Height] = heightInFeet[treeIndex];
                this.Float[treeIndex, (int)TreePropertyFloat.CrownRatio] = crownRatio[treeIndex];
                this.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor] = expansionFactor[treeIndex];
            }
        }

        public void FromArrays(float[] dbhInInches, float[] heightInFeet, float[] crownRatio, float[] expansionFactor,
                               float[] diameterGrowthInInches, float[] heightGrowthInFeet, float[] crownChange)
        {
            if (diameterGrowthInInches.Length != this.MaximumTreeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(diameterGrowthInInches));
            }
            if (heightGrowthInFeet.Length != this.MaximumTreeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(heightGrowthInFeet));
            }
            if (crownChange.Length != this.MaximumTreeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(crownChange));
            }

            this.FromArrays(dbhInInches, heightInFeet, crownRatio, expansionFactor);
        }

        public int GetBigSixSpeciesRecordCount()
        {
            int bigSixRecords = 0;
            for (int treeIndex = 0; treeIndex < this.TreeRecordsInUse; ++treeIndex)
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
                                   out float[] expansionFactorAtEndOfCycle)
        {
            dbhInInchesAtEndOfCycle = new float[this.MaximumTreeRecords];
            heightAtEndOfCycle = new float[this.MaximumTreeRecords];
            crownRatioAtEndOfCycle = new float[this.MaximumTreeRecords];
            expansionFactorAtEndOfCycle = new float[this.MaximumTreeRecords];
        }

        public void ToArrays(out float[] dbhInInches, out float[] heightInFeet, out float[] crownRatio,
                             out float[] expansionFactor)
        {
            dbhInInches = new float[this.MaximumTreeRecords];
            heightInFeet = new float[this.MaximumTreeRecords];
            crownRatio = new float[this.MaximumTreeRecords];
            expansionFactor = new float[this.MaximumTreeRecords];
            for (int treeIndex = 0; treeIndex < this.MaximumTreeRecords; ++treeIndex)
            {
                dbhInInches[treeIndex] = this.Float[treeIndex, (int)TreePropertyFloat.Dbh];
                heightInFeet[treeIndex] = this.Float[treeIndex, (int)TreePropertyFloat.Height];
                crownRatio[treeIndex] = this.Float[treeIndex, (int)TreePropertyFloat.CrownRatio];
                expansionFactor[treeIndex] = this.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor];
            }
        }

        public void ToArrays(out float[] dbhInInches, out float[] heightInFeet, out float[] crownRatio,
                             out float[] expansionFactor, out float[] diameterGrowthInInches, out float[] heightGrowthInFeet,
                             out float[] crownChange, out float[] shadowCrownRatioChange)
        {
            this.ToArrays(out dbhInInches, out heightInFeet, out crownRatio, out expansionFactor);

            diameterGrowthInInches = new float[this.MaximumTreeRecords];
            heightGrowthInFeet = new float[this.MaximumTreeRecords];
            crownChange = new float[this.MaximumTreeRecords];
            shadowCrownRatioChange = new float[this.MaximumTreeRecords];
        }

        public void WriteAsCsv(TestContext testContext, Variant variant, int simulationStep)
        {
            for (int treeIndex = 0; treeIndex < this.TreeRecordsInUse; ++treeIndex)
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
                testContext.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                      variant, simulationStep, treeIndex, treeSpecies, treeSpeciesGroup, treeUserData,
                                      treeDbhInInches, treeHeightInFeet, treeExpansionFactor, treeDeadExpansionFactor,
                                      treeMGExpansionFactor, treeCrownRatio);
            }
        }
    }
}
