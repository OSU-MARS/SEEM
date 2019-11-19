using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Osu.Cof.Organon.Test
{
    public class TestStand : Stand
    {
        public TestStand(OrganonVariant variant, int ageInYears, int treeCount, float primarySiteIndex)
            : base(ageInYears, treeCount, primarySiteIndex, (variant.Variant == Variant.Swo) ? 4 : 2)
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

        public void LogAsCsv(TestContext testContext, OrganonVariant variant, int year, bool omitExpansionFactorZeroTrees)
        {
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                float expansionFactor = this.LiveExpansionFactor[treeIndex];
                if (omitExpansionFactorZeroTrees && (expansionFactor == 0))
                {
                    continue;
                }

                int id = this.Tag[treeIndex] > 0 ? this.Tag[treeIndex] : treeIndex;
                FiaCode species = this.Species[treeIndex];
                int speciesGroup = this.SpeciesGroup[treeIndex];
                float dbhInInches = this.Dbh[treeIndex];
                float heightInFeet = this.Height[treeIndex];
                float crownRatio = this.CrownRatio[treeIndex];
                float deadExpansionFactor = this.DeadExpansionFactor[treeIndex];
                float dbhGrowth = this.DbhGrowth[treeIndex];
                float heightGrowth = this.HeightGrowth[treeIndex];
                testContext.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                      variant.Variant, year, id, species, speciesGroup, 
                                      dbhInInches, heightInFeet, expansionFactor, deadExpansionFactor,
                                      crownRatio, dbhGrowth, heightGrowth);
            }
        }

        public static void LogCsvHeader(TestContext testContext)
        {
            testContext.WriteLine("variant,year,tree,species,species group,DBH,height,expansion factor,dead expansion factor,crown ratio,diameter growth,height growth");
        }

        public StreamWriter WriteTreesToCsv(string filePath, OrganonVariant variant, int year)
        {
            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("variant,year,tree,species,species group,DBH,height,expansion factor,dead expansion factor,crown ratio,diameter growth,height growth");
            this.WriteTreesToCsv(writer, variant, year);
            return writer;
        }

        public void WriteTreesToCsv(StreamWriter writer, OrganonVariant variant, int year)
        {
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                float expansionFactor = 2.47105F * this.LiveExpansionFactor[treeIndex];
                if (expansionFactor == 0)
                {
                    continue;
                }

                int id = this.Tag[treeIndex] > 0 ? this.Tag[treeIndex] : treeIndex;
                FiaCode species = this.Species[treeIndex];
                int speciesGroup = this.SpeciesGroup[treeIndex];
                float dbhInCentimeters = 2.54F * this.Dbh[treeIndex];
                float heightInMeters = 0.3048F * this.Height[treeIndex];
                float crownRatio = this.CrownRatio[treeIndex];
                float deadExpansionFactor = 2.47105F * this.DeadExpansionFactor[treeIndex];
                float dbhGrowth = 2.54F * this.DbhGrowth[treeIndex];
                float heightGrowth = 0.3048F * this.HeightGrowth[treeIndex];
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                 variant.Variant, year, id, species, speciesGroup, dbhInCentimeters, heightInMeters, 
                                 expansionFactor, deadExpansionFactor, crownRatio, dbhGrowth, heightGrowth);
            }
        }
    }
}
