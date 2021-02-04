using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Osu.Cof.Ferm.Test
{
    public class TestStand : OrganonStand
    {
        public SortedDictionary<FiaCode, int[]> InitialDbhQuantileBySpecies { get; private set; }

        public TestStand(OrganonVariant variant, int ageInYears, float primarySiteIndex)
            : base(ageInYears, primarySiteIndex)
        {
            this.InitialDbhQuantileBySpecies = new SortedDictionary<FiaCode, int[]>();

            this.EnsureSiteIndicesSet(variant);
        }

        public TestStand(TestStand other)
            : base(other)
        {
            // for now, assume DBH quantiles are fixed => shallow copy doesn't raise ownership or mutability concerns
            this.InitialDbhQuantileBySpecies = other.InitialDbhQuantileBySpecies;

            // don't need to call SetDefaultAndMortalitySiteIndices() as Stand's constructor copies site indicies
        }

        public void Add(TreeRecord tree)
        {
            if (this.TreesBySpecies.TryGetValue(tree.Species, out Trees? treesOfSpecies) == false)
            {
                treesOfSpecies = new Trees(tree.Species, 1, Units.English);
                this.TreesBySpecies.Add(tree.Species, treesOfSpecies);
            }

            Debug.Assert(treesOfSpecies.Units == Units.English);
            treesOfSpecies.Add(tree.Tag, tree.DbhInInches, tree.HeightInFeet, tree.CrownRatio, tree.LiveExpansionFactor);
        }

        public void SetQuantiles()
        {
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                // find DBH sort order of trees in each species
                // Since trees are entered by their initial diameter regardless of their ingrowth time, this includes ingrowth in quintiles
                // even though it doesn't yet exist.

                // sort trees by DBH and capture the sort order to dbhSortIndices
                float[] dbh = new float[treesOfSpecies.Count];
                int[] dbhSortIndices = new int[treesOfSpecies.Count];
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    dbh[treeIndex] = treesOfSpecies.Dbh[treeIndex];
                    dbhSortIndices[treeIndex] = treeIndex;
                }
                Array.Sort(dbh, dbhSortIndices);

                // assign trees to quantiles
                float dbhQuantilesAsSingle = (float)TestConstant.DbhQuantiles;
                float treeCountAsSingle = (float)treesOfSpecies.Count;

                int[] initialDbhQuantileBySpecies = this.InitialDbhQuantileBySpecies.GetOrAdd(treesOfSpecies.Species, treesOfSpecies.Capacity);
                for (int quantileAssignmentIndex = 0; quantileAssignmentIndex < treesOfSpecies.Count; ++quantileAssignmentIndex)
                {
                    int dbhSortIndex = dbhSortIndices[quantileAssignmentIndex];
                    initialDbhQuantileBySpecies[dbhSortIndex] = (int)MathF.Floor(dbhQuantilesAsSingle * (float)quantileAssignmentIndex / treeCountAsSingle);
                }
            }
        }

        public void WriteCompetitionAsCsv(string filePath, OrganonVariant variant, int year)
        {
            OrganonStandDensity density = new OrganonStandDensity(this, variant);

            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("variant,year,tree,species,BAL,CCFL,DBH,height,expansion factor,crown ratio");
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    int id = treesOfSpecies.Tag[treeIndex];
                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    float heightInMeters = Constant.MetersPerFoot * treesOfSpecies.Height[treeIndex];
                    float crownRatio = treesOfSpecies.CrownRatio[treeIndex];
                    float liveExpansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];

                    float basalAreaLarger = TestConstant.AcresPerHectare * TestConstant.SquareMetersPerSquareFoot * density.GetBasalAreaLarger(dbhInInches);
                    float ccfLarger = density.GetCrownCompetitionFactorLarger(dbhInInches);
                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                     variant.TreeModel, year, id, treesOfSpecies.Species, basalAreaLarger, ccfLarger, Constant.CentimetersPerInch * dbhInInches,
                                     heightInMeters, liveExpansionFactor, crownRatio);
                }
            }
        }

        public void WriteTreesAsCsv(TestContext testContext, OrganonVariant variant, int year, bool omitExpansionFactorZeroTrees)
        {
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                int[] initialDbhQuantile = this.InitialDbhQuantileBySpecies[treesOfSpecies.Species];
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (omitExpansionFactorZeroTrees && (expansionFactor == 0))
                    {
                        continue;
                    }

                    int id = treesOfSpecies.Tag[treeIndex];
                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    float crownRatio = treesOfSpecies.CrownRatio[treeIndex];
                    float deadExpansionFactor = treesOfSpecies.DeadExpansionFactor[treeIndex];
                    float dbhGrowth = treesOfSpecies.DbhGrowth[treeIndex];
                    float heightGrowth = treesOfSpecies.HeightGrowth[treeIndex];
                    int quantile = initialDbhQuantile[treeIndex];
                    testContext.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                          variant.TreeModel, year, id, treesOfSpecies.Species,
                                          dbhInInches, heightInFeet, expansionFactor, deadExpansionFactor,
                                          crownRatio, dbhGrowth, heightGrowth, quantile);
                }
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
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                int[] initialDbhQuantile = this.InitialDbhQuantileBySpecies[treesOfSpecies.Species];
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float expansionFactor = TestConstant.AcresPerHectare * treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (expansionFactor == 0)
                    {
                        continue;
                    }

                    int id = treesOfSpecies.Tag[treeIndex];
                    float dbhInCentimeters = Constant.CentimetersPerInch * treesOfSpecies.Dbh[treeIndex];
                    float heightInMeters = Constant.MetersPerFoot * treesOfSpecies.Height[treeIndex];
                    float crownRatio = treesOfSpecies.CrownRatio[treeIndex];
                    float deadExpansionFactor = TestConstant.AcresPerHectare * treesOfSpecies.DeadExpansionFactor[treeIndex];
                    float dbhGrowth = Constant.CentimetersPerInch * treesOfSpecies.DbhGrowth[treeIndex];
                    float heightGrowth = Constant.MetersPerFoot * treesOfSpecies.HeightGrowth[treeIndex];
                    int quantile = initialDbhQuantile[treeIndex];
                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                     variant.TreeModel, year, id, treesOfSpecies.Species, dbhInCentimeters, heightInMeters,
                                     expansionFactor, deadExpansionFactor, crownRatio, dbhGrowth, heightGrowth, quantile);
                }
            }
        }
    }
}
