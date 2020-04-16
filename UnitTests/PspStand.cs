using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Test
{
    public class PspStand
    {
        private readonly float plotAreaInAcres;
        private int plotCount;

        public SortedList<int, PspTreeMeasurementSeries> MeasurementsByTag { get; set; }
        public HashSet<int> MeasurementYears { get; set; }

        public PspStand(string xlsxFilePath, string worksheetName, float plotAreaInAcres)
        {
            this.MeasurementsByTag = new SortedList<int, PspTreeMeasurementSeries>();
            this.MeasurementYears = new HashSet<int>();
            this.plotAreaInAcres = plotAreaInAcres;
            this.plotCount = 0;

            XlsxReader reader = new XlsxReader();
            reader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);
        }

        public void AddIngrowth(int year, OrganonStand stand, OrganonStandDensity standDensity)
        {
            int firstMeasurementYear = this.GetFirstMeasurementYear();
            float fixedPlotExpansionFactor = this.GetTreesPerAcreExpansionFactor();
            int treeIndex = 0;
            foreach (PspTreeMeasurementSeries tree in this.MeasurementsByTag.Values)
            {
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                if (expansionFactor == 0.0F)
                {
                    int ingrowthYear = tree.GetFirstMeasurementYear();
                    if ((ingrowthYear != firstMeasurementYear) && (ingrowthYear <= year))
                    {
                        float dbhInCentimeters = tree.DbhInCentimetersByYear.Values[0];
                        stand.Dbh[treeIndex] = dbhInCentimeters / Constant.CmPerInch;
                        stand.Height[treeIndex] = TestConstant.FeetPerMeter * TreeRecord.EstimateHeightInMeters(tree.Species, stand.Dbh[treeIndex]);
                        stand.CrownRatio[treeIndex] = tree.EstimateInitialCrownRatio(standDensity);
                        stand.LiveExpansionFactor[treeIndex] = fixedPlotExpansionFactor;
                    }
                }

                ++treeIndex;
            }
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

        private void ParseRow(int rowIndex, string[] rowAsStrings)
        {
            if ((rowIndex == 0) || (rowAsStrings[Constant.Psp.ColumnIndex.Tag] == null))
            {
                return;
            }

            int plot = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Plot]);
            this.plotCount = Math.Max(this.plotCount, plot);

            int tag = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Tag]);
            if (this.MeasurementsByTag.TryGetValue(tag, out PspTreeMeasurementSeries tree) == false)
            {
                string species = rowAsStrings[Constant.Psp.ColumnIndex.Species];
                tree = new PspTreeMeasurementSeries(tag, species);
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

        public TestStand ToStand(OrganonVariant variant, float siteIndex)
        {
            int firstMeasurementYear = this.GetFirstMeasurementYear();

            // populate Organon version of stand
            // Currently, PSP stands are assumed to have IsEvenAge = false, which causes Organon to require a stand age of
            // zero years be passed.
            TestStand stand = new TestStand(variant.TreeModel, 0, this.MeasurementsByTag.Count, siteIndex)
            {
                NumberOfPlots = this.plotCount
            };
            float fixedPlotExpansionFactor = this.GetTreesPerAcreExpansionFactor();
            int treeIndex = 0;
            foreach (PspTreeMeasurementSeries tree in this.MeasurementsByTag.Values)
            {
                float expansionFactor = fixedPlotExpansionFactor;
                if (tree.DbhInCentimetersByYear.TryGetValue(firstMeasurementYear, out float dbhInCentimeters) == false)
                {
                    dbhInCentimeters = tree.DbhInCentimetersByYear.Values[0];
                    expansionFactor = 0.0F;
                }

                FiaCode species = FiaCodeExtensions.Parse(tree.Species);
                if (species == FiaCode.Alnus)
                {
                    // remap Alnus viridis ssp sinuata to Alnus rubra as no Organon variant has support
                    species = FiaCode.AlnusRubra;
                }
                stand.Tag[treeIndex] = tree.Tag;
                stand.Species[treeIndex] = species;
                stand.Dbh[treeIndex] = TestConstant.InchesPerCm * dbhInCentimeters;
                stand.Height[treeIndex] = TreeRecord.EstimateHeightInFeet(species, stand.Dbh[treeIndex]);
                stand.CrownRatio[treeIndex] = TestConstant.Default.CrownRatio;
                stand.LiveExpansionFactor[treeIndex] = expansionFactor;

                if (variant.IsSpeciesSupported(species) == false)
                {
                    if (species == FiaCode.ChrysolepisChrysophyllaVarChrysophylla)
                    {
                        species = FiaCode.CornusNuttallii;
                    }
                    else
                    {
                        throw new NotSupportedException(String.Format("Unsupported species {0}.", species));
                    }

                    stand.Species[treeIndex] = species;
                }

                ++treeIndex;
            }

            // establish growth tracking quantiles
            stand.SetQuantiles();

            // estimate crown ratios
            OrganonStandDensity standDensity = new OrganonStandDensity(stand, variant);
            treeIndex = 0;
            foreach (PspTreeMeasurementSeries tree in this.MeasurementsByTag.Values)
            {
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                if (expansionFactor > 0.0F)
                {
                    stand.CrownRatio[treeIndex] = tree.EstimateInitialCrownRatio(standDensity);
                }

                ++treeIndex;
            }

            return stand;
        }
    }
}
