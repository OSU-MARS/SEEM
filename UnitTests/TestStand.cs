using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Osu.Cof.Organon.Test
{
    public class TestStand : Stand
    {
        public TestStand(Variant variant, int ageInYears, int treeCount, float primarySiteIndex)
            : base(ageInYears, treeCount, primarySiteIndex, (variant == Variant.Swo) ? 4 : 2)
        {
            this.SetDefaultAndMortalitySiteIndices(variant);
        }

        protected TestStand(TestStand other)
            : base(other)
        {
        }

        public TestStand Clone()
        {
            return new TestStand(this);
        }

        public int GetBigSixSpeciesRecordCount()
        {
            int bigSixRecords = 0;
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                int speciesGroup = this.SpeciesGroup[treeIndex];
                if (speciesGroup <= this.MaxBigSixSpeciesGroupIndex)
                {
                    ++bigSixRecords;
                }
            }
            return bigSixRecords;
        }

        public void WriteAsCsv(TestContext testContext, Variant variant, int simulationStep)
        {
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                FiaCode treeSpecies = this.Species[treeIndex];
                int treeSpeciesGroup = this.SpeciesGroup[treeIndex];
                float treeDbhInInches = this.Dbh[treeIndex];
                float treeHeightInFeet = this.Height[treeIndex];
                float treeExpansionFactor = this.LiveExpansionFactor[treeIndex];
                float treeCrownRatio = this.CrownRatio[treeIndex];
                float treeDeadExpansionFactor = this.DeadExpansionFactor[treeIndex];
                float treeDbhGrowth = this.DbhGrowth[treeIndex];
                float treeHeightGrowth = this.HeightGrowth[treeIndex];
                testContext.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                      variant, simulationStep, treeIndex, treeSpecies, treeSpeciesGroup, 
                                      treeDbhInInches, treeHeightInFeet, treeExpansionFactor, treeDeadExpansionFactor,
                                      treeCrownRatio, treeDbhGrowth, treeHeightGrowth);
            }
        }
    }
}
