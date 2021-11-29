using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mars.Seem.Test
{
    public class PspStand
    {
        private readonly float plotAreaInAcres;
        private int plotCount;
        private int yearOfMostRecentIngrowthAdded;

        public SortedDictionary<int, List<PspTreeMeasurementSeries>> IngrowthByYear { get; private set; }
        public SortedList<int, PspTreeMeasurementSeries> MeasurementsByTag { get; private set; }
        public HashSet<int> MeasurementYears { get; private set; }

        public PspStand(string xlsxFilePath, string worksheetName, float plotAreaInAcres)
        {
            this.IngrowthByYear = new SortedDictionary<int, List<PspTreeMeasurementSeries>>();
            this.MeasurementsByTag = new SortedList<int, PspTreeMeasurementSeries>();
            this.MeasurementYears = new HashSet<int>();
            this.plotAreaInAcres = plotAreaInAcres;
            this.plotCount = 0;
            this.yearOfMostRecentIngrowthAdded = Int32.MinValue;

            XlsxReader reader = new();
            XlsxReader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);
        }

        public void AddIngrowth(int year, OrganonStand stand, OrganonStandDensity standDensity)
        {
            List<int> remainingIngrowthYears = this.IngrowthByYear.Keys.Where(key => key > this.yearOfMostRecentIngrowthAdded).ToList();
            if ((remainingIngrowthYears.Count < 1) || (remainingIngrowthYears[0] > year))
            {
                // no ingrowth in this simulation step
                return;
            }
            int ingrowthYear = remainingIngrowthYears[0];
            Debug.Assert((remainingIngrowthYears.Count == 1) || (remainingIngrowthYears[1] > year)); // for now, assume only one ingrowth measurement per simulation timestep

            float fixedPlotExpansionFactor = this.GetTreesPerAcreExpansionFactor();
            foreach (PspTreeMeasurementSeries tree in this.IngrowthByYear[ingrowthYear])
            {
                Trees treesOfSpecies = stand.TreesBySpecies[tree.Species];
                Debug.Assert(treesOfSpecies.Capacity > treesOfSpecies.Count);

                float dbhInInches = Constant.InchesPerCentimeter * tree.DbhInCentimetersByYear.Values[0];
                float heightInFeet = TestConstant.FeetPerMeter * TreeRecord.EstimateHeightInMeters(tree.Species, dbhInInches);
                treesOfSpecies.Add(tree.Plot, tree.Tag, dbhInInches, heightInFeet, tree.EstimateInitialCrownRatio(standDensity), fixedPlotExpansionFactor);
            }

            this.yearOfMostRecentIngrowthAdded = ingrowthYear;
        }

        private Dictionary<FiaCode, int> CountTreesBySpecies()
        {
            Dictionary<FiaCode, int> treeCountBySpecies = new();
            foreach (PspTreeMeasurementSeries tree in this.MeasurementsByTag.Values)
            {
                if (treeCountBySpecies.TryGetValue(tree.Species, out int count) == false)
                {
                    treeCountBySpecies.Add(tree.Species, 1);
                }
                else
                {
                    treeCountBySpecies[tree.Species] = ++count;
                }
            }
            return treeCountBySpecies;
        }

        private int GetFirstMeasurementYear()
        {
            return this.MeasurementYears.Min();
        }

        private float GetTreesPerAcreExpansionFactor()
        {
            return 1.0F / (this.plotAreaInAcres * this.plotCount);
        }

        public float GetTreesPerHectareExpansionFactor()
        {
            return TestConstant.AcresPerHectare * this.GetTreesPerAcreExpansionFactor();
        }

        private static FiaCode MaybeRemapToSupportedSpecies(FiaCode species, OrganonVariant variant)
        {
            if (variant.IsSpeciesSupported(species))
            {
                return species;
            }

            if (species == FiaCode.ChrysolepisChrysophyllaVarChrysophylla)
            {
                return FiaCode.CornusNuttallii;
            }
            else
            {
                throw Trees.CreateUnhandledSpeciesException(species);
            }
        }

        private void ParseRow(int rowIndex, string[] rowAsStrings)
        {
            if ((rowIndex == 0) || (rowAsStrings[Constant.Psp.ColumnIndex.Tag] == null))
            {
                return;
            }

            int plot = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Plot]);
            this.plotCount = Math.Max(this.plotCount, plot);

            int tag = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Tag]);
            if (this.MeasurementsByTag.TryGetValue(tag, out PspTreeMeasurementSeries? tree) == false)
            {
                FiaCode species = FiaCodeExtensions.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Species]);
                if (species == FiaCode.Alnus)
                {
                    // remap Alnus viridis ssp sinuata to Alnus rubra as no Organon variant has support
                    species = FiaCode.AlnusRubra;
                }
                tree = new PspTreeMeasurementSeries(plot, tag, species);
                this.MeasurementsByTag.Add(tag, tree);
            }

            int status = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Status]);
            int year = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Year]);
            if (status < Constant.Psp.TreeStatus.Dead) // dead or not found trees lack diameter measurements
            {
                float dbhInCentimeters = float.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Dbh]);
                Debug.Assert(dbhInCentimeters >= 5.0F);
                if (tree.DbhInCentimetersByYear.TryAdd(year, dbhInCentimeters) == false)
                {
                    // in case of a conflict, use whichever DBH comes last for consistency with the code above.
                    // For example, tree 8824 in RS39 has two 2013 records.
                    tree.DbhInCentimetersByYear[year] = dbhInCentimeters;
                }
            }

            this.MeasurementYears.Add(year);
        }

        public TestStand ToStand(OrganonConfiguration configuration, float siteIndex)
        {
            int firstPlotMeasurementYear = this.GetFirstMeasurementYear();

            // populate Organon version of stand
            // Currently, PSP stands are assumed to have IsEvenAge = false, which causes Organon to require a stand age of
            // zero years be passed.
            TestStand stand = new(configuration.Variant, 0, siteIndex)
            {
                NumberOfPlots = this.plotCount
            };
            foreach (KeyValuePair<FiaCode, int> speciesCount in this.CountTreesBySpecies())
            {
                // skip any unsupported species as they should be remapped in following loops
                if (configuration.Variant.IsSpeciesSupported(speciesCount.Key) == false)
                {
                    continue;
                }

                // metric PSP data is converted to English units for Organon below
                stand.TreesBySpecies.Add(speciesCount.Key, new Trees(speciesCount.Key, speciesCount.Value, Units.English));
            }

            float fixedPlotExpansionFactor = this.GetTreesPerAcreExpansionFactor();
            foreach (PspTreeMeasurementSeries tree in this.MeasurementsByTag.Values)
            {
                int firstTreeMeasurementYear = tree.GetFirstMeasurementYear();
                Debug.Assert(firstTreeMeasurementYear >= firstPlotMeasurementYear);
                if (firstTreeMeasurementYear != firstPlotMeasurementYear)
                {
                    // tree is ingrowth
                    List<PspTreeMeasurementSeries> ingrowthForYear = this.IngrowthByYear.GetOrAdd(firstTreeMeasurementYear);
                    ingrowthForYear.Add(tree);
                    continue;
                }

                FiaCode species = PspStand.MaybeRemapToSupportedSpecies(tree.Species, configuration.Variant);
                Trees treesOfSpecies = stand.TreesBySpecies[species];
                Debug.Assert(treesOfSpecies.Capacity > treesOfSpecies.Count);
                float dbhInInches = TestConstant.InchesPerCm * tree.DbhInCentimetersByYear[firstPlotMeasurementYear];
                float heightInFeet = TreeRecord.EstimateHeightInFeet(species, dbhInInches);
                treesOfSpecies.Add(tree.Plot, tree.Tag, dbhInInches, heightInFeet, TestConstant.Default.CrownRatio, fixedPlotExpansionFactor);
            }

            // estimate crown ratios
            OrganonStandDensity standDensity = new(configuration.Variant, stand);
            Dictionary<FiaCode, int> indexBySpecies = new();
            foreach (PspTreeMeasurementSeries tree in this.MeasurementsByTag.Values)
            {
                int firstTreeMeasurementYear = tree.GetFirstMeasurementYear();
                if (firstTreeMeasurementYear != firstPlotMeasurementYear)
                {
                    continue;
                }

                if (indexBySpecies.TryGetValue(tree.Species, out int treeIndex) == false)
                {
                    treeIndex = 0;
                    indexBySpecies.Add(tree.Species, treeIndex);
                }

                FiaCode species = PspStand.MaybeRemapToSupportedSpecies(tree.Species, configuration.Variant);
                Trees treesOfSpecies = stand.TreesBySpecies[species];
                treesOfSpecies.CrownRatio[treeIndex] = tree.EstimateInitialCrownRatio(standDensity);
                indexBySpecies[tree.Species] = ++treeIndex;
            }

            // complete stand initialization
            stand.EnsureSiteIndicesSet(configuration.Variant);
            stand.SetQuantiles();
            stand.SetRedAlderSiteIndexAndGrowthEffectiveAge();
            stand.SetSdiMax(configuration);

            return stand;
        }
    }
}
