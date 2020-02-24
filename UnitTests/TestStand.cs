using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Osu.Cof.Organon.Test
{
    public class TestStand : Stand
    {
        public int[] QuantileByInitialDbh { get; private set; }
        public Dictionary<FiaCode, List<int>> TreeIndicesBySpecies { get; private set; }

        public TestStand(TreeModel treeModel, int ageInYears, int treeCount, float primarySiteIndex)
            : base(ageInYears, treeCount, primarySiteIndex)
        {
            this.QuantileByInitialDbh = new int[this.TreeRecordCount];
            this.TreeIndicesBySpecies = new Dictionary<FiaCode, List<int>>(this.TreeRecordCount);

            this.SetDefaultAndMortalitySiteIndices(treeModel);
        }

        protected TestStand(TestStand other)
            : base(other)
        {
        }

        public new TestStand Clone()
        {
            return new TestStand(this);
        }

        public void SetQuantiles()
        {
            // index trees by species
            this.TreeIndicesBySpecies.Clear();
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = this.Species[treeIndex];
                if (this.TreeIndicesBySpecies.TryGetValue(species, out List<int> speciesIndices) == false)
                {
                    speciesIndices = new List<int>();
                    this.TreeIndicesBySpecies.Add(species, speciesIndices);
                }

                speciesIndices.Add(treeIndex);
            }

            // find DBH sort order of trees in each species
            // Since trees are entered by their initial diameter regardless of their ingrowth time, this includes ingrowth in quintiles
            // even though it doesn't yet exist.
            foreach (KeyValuePair<FiaCode, List<int>> treeIndicesForSpecies in this.TreeIndicesBySpecies)
            {
                // gather diameters of trees of this species and find their sort order
                List<int> speciesIndices = treeIndicesForSpecies.Value;
                float[] dbh = new float[speciesIndices.Count];
                for (int index = 0; index < speciesIndices.Count; ++index)
                {
                    dbh[index] = this.Dbh[speciesIndices[index]];
                }
                int[] dbhIndices = Enumerable.Range(0, speciesIndices.Count).ToArray();
                Array.Sort(dbh, dbhIndices);

                // assign trees to quantiles
                double dbhQuantilesAsDouble = (double)TestConstant.DbhQuantiles;
                double speciesCountAsDouble = (double)speciesIndices.Count;
                for (int index = 0; index < speciesIndices.Count; ++index)
                {
                    int treeIndex = speciesIndices[dbhIndices[index]];
                    this.QuantileByInitialDbh[treeIndex] = (int)Math.Floor(dbhQuantilesAsDouble * (double)index / speciesCountAsDouble);
                }
            }
        }

        public void WriteCompetitionAsCsv(string filePath, OrganonVariant variant, int year)
        {
            StandDensity density = new StandDensity(this, variant);

            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("variant,year,tree,species,BAL,CCFL,DBH,height,expansion factor,crown ratio");
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                int id = this.Tag[treeIndex] > 0 ? this.Tag[treeIndex] : treeIndex;
                FiaCode species = this.Species[treeIndex];
                float dbhInInches = this.Dbh[treeIndex];
                float heightInMeters = Constant.MetersPerFoot * this.Height[treeIndex];
                float crownRatio = this.CrownRatio[treeIndex];
                float liveExpansionFactor = this.LiveExpansionFactor[treeIndex];

                float basalAreaLarger = TestConstant.AcresPerHectare * TestConstant.SquareMetersPerSquareFoot * density.GetBasalAreaLarger(dbhInInches);
                float ccfLarger = density.GetCrownCompetitionFactorLarger(dbhInInches);
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", 
                                 variant.TreeModel, year, id, species, basalAreaLarger, ccfLarger, Constant.CmPerInch * dbhInInches, 
                                 heightInMeters, liveExpansionFactor, crownRatio);
            }
        }

        public void WriteTreesAsCsv(TestContext testContext, OrganonVariant variant, int year, bool omitExpansionFactorZeroTrees)
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
                float dbhInInches = this.Dbh[treeIndex];
                float heightInFeet = this.Height[treeIndex];
                float crownRatio = this.CrownRatio[treeIndex];
                float deadExpansionFactor = this.DeadExpansionFactor[treeIndex];
                float dbhGrowth = this.DbhGrowth[treeIndex];
                float heightGrowth = this.HeightGrowth[treeIndex];
                int quantile = this.QuantileByInitialDbh[treeIndex];
                testContext.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                      variant.TreeModel, year, id, species, 
                                      dbhInInches, heightInFeet, expansionFactor, deadExpansionFactor,
                                      crownRatio, dbhGrowth, heightGrowth, quantile);
            }
        }

        public static void WriteTreeHeader(TestContext testContext)
        {
            testContext.WriteLine("variant,year,tree,species,DBH,height,expansion factor,dead expansion factor,crown ratio,diameter growth,height growth,quantile");
        }

        public StreamWriter WriteTreesToCsv(string filePath, OrganonVariant variant, int year)
        {
            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("variant,year,tree,species,DBH,height,expansion factor,dead expansion factor,crown ratio,diameter growth,height growth,quantile");
            this.WriteTreesToCsv(writer, variant, year);
            return writer;
        }

        public void WriteTreesToCsv(StreamWriter writer, OrganonVariant variant, int year)
        {
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                float expansionFactor = TestConstant.AcresPerHectare * this.LiveExpansionFactor[treeIndex];
                if (expansionFactor == 0)
                {
                    continue;
                }

                int id = this.Tag[treeIndex] > 0 ? this.Tag[treeIndex] : treeIndex;
                FiaCode species = this.Species[treeIndex];
                float dbhInCentimeters = Constant.CmPerInch * this.Dbh[treeIndex];
                float heightInMeters = Constant.MetersPerFoot * this.Height[treeIndex];
                float crownRatio = this.CrownRatio[treeIndex];
                float deadExpansionFactor = TestConstant.AcresPerHectare * this.DeadExpansionFactor[treeIndex];
                float dbhGrowth = Constant.CmPerInch * this.DbhGrowth[treeIndex];
                float heightGrowth = Constant.MetersPerFoot * this.HeightGrowth[treeIndex];
                int quantile = this.QuantileByInitialDbh[treeIndex];
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                 variant.TreeModel, year, id, species, dbhInCentimeters, heightInMeters, 
                                 expansionFactor, deadExpansionFactor, crownRatio, dbhGrowth, heightGrowth, quantile);
            }
        }
    }
}
