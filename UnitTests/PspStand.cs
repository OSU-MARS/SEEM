using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Organon.Test
{
    internal class PspStand
    {
        private readonly SortedList<int, PspTreeMeasurementSeries> measurementsByTag;
        private readonly HashSet<int> measurementYears;
        private readonly float plotAreaInAcres;

        public PspStand(string xlsxFilePath, string worksheetName, float plotAreaInAcres)
        {
            this.measurementsByTag = new SortedList<int, PspTreeMeasurementSeries>();
            this.measurementYears = new HashSet<int>();
            this.plotAreaInAcres = plotAreaInAcres;

            XlsxReader reader = new XlsxReader();
            reader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);
        }

        public void AddIngrowth(int year, Stand stand, StandDensity standDensity)
        {
            float fixedPlotExpansionFactor = 1.0F / this.plotAreaInAcres;
            int treeIndex = 0;
            foreach (PspTreeMeasurementSeries tree in this.measurementsByTag.Values)
            {
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                if (expansionFactor == 0.0F)
                {
                    if (tree.DbhInCentimetersByYear.Keys[0] <= year)
                    {
                        float dbhInCentimeters = tree.DbhInCentimetersByYear.Values[0];
                        stand.Dbh[treeIndex] = dbhInCentimeters / 2.54F;
                        stand.Height[treeIndex] = 3.048F * tree.EstimateInitialHeightInMeters();
                        stand.CrownRatio[treeIndex] = tree.EstimateInitialCrownRatio(standDensity);
                        stand.LiveExpansionFactor[treeIndex] = fixedPlotExpansionFactor;
                    }
                }

                ++treeIndex;
            }
        }

        private void ParseRow(int rowIndex, string[] rowAsStrings)
        {
            if ((rowIndex == 0) || (rowAsStrings[Constant.Psp.ColumnIndex.Tag] == null))
            {
                return;
            }

            int tag = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Tag]);
            if (this.measurementsByTag.TryGetValue(tag, out PspTreeMeasurementSeries tree) == false)
            {
                string species = rowAsStrings[Constant.Psp.ColumnIndex.Species];
                tree = new PspTreeMeasurementSeries(tag, species);
                this.measurementsByTag.Add(tag, tree);
            }

            int status = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Status]);
            int year = Int32.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Year]);
            if (status != Constant.Psp.TreeStatus.Dead)
            {
                float dbhInCentimeters = float.Parse(rowAsStrings[Constant.Psp.ColumnIndex.Dbh]);
                Debug.Assert(dbhInCentimeters >= 5.0F);
                tree.DbhInCentimetersByYear.Add(year, dbhInCentimeters);
            }

            this.measurementYears.Add(year);
        }

        public TestStand ToOrganonStand(OrganonVariant variant, int ageInYears, float siteIndex)
        {
            int earliestMeasurementYear = this.measurementYears.Min();

            // populate Organon version of stand
            TestStand stand = new TestStand(variant, ageInYears, this.measurementsByTag.Count, siteIndex);
            float fixedPlotExpansionFactor = 1.0F / this.plotAreaInAcres;
            int treeIndex = 0;
            foreach (PspTreeMeasurementSeries tree in this.measurementsByTag.Values)
            {
                float expansionFactor = fixedPlotExpansionFactor;
                if (tree.DbhInCentimetersByYear.TryGetValue(earliestMeasurementYear, out float dbhInCentimeters) == false)
                {
                    dbhInCentimeters = tree.DbhInCentimetersByYear.Values[0];
                    expansionFactor = 0.0F;
                }

                FiaCode species = tree.GetFiaCode();
                stand.Tag[treeIndex] = tree.Tag;
                stand.Species[treeIndex] = species;
                stand.SpeciesGroup[treeIndex] = variant.GetSpeciesGroup(species);
                stand.Dbh[treeIndex] = dbhInCentimeters / 2.54F;
                stand.Height[treeIndex] = 3.048F * tree.EstimateInitialHeightInMeters();
                stand.CrownRatio[treeIndex] = TestConstant.Default.CrownRatio;
                stand.LiveExpansionFactor[treeIndex] = expansionFactor;

                ++treeIndex;
            }

            // estimate crown ratios
            StandDensity standDensity = new StandDensity(stand, variant);
            treeIndex = 0;
            foreach (PspTreeMeasurementSeries tree in this.measurementsByTag.Values)
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
