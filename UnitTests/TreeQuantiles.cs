﻿using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Mars.Seem.Test
{
    internal class TreeQuantiles
    {
        public readonly SortedDictionary<FiaCode, float[]> DeadExpansionFactorBySpecies;
        public readonly SortedDictionary<FiaCode, float[]> LiveExpansionFactorBySpecies;
        public readonly SortedDictionary<FiaCode, float[]> MaxDbhInCmBySpecies;
        public readonly SortedDictionary<FiaCode, float[]> MeanCrownRatioBySpecies;
        public readonly SortedDictionary<FiaCode, float[]> MeanDbhInCmBySpecies;
        public readonly SortedDictionary<FiaCode, float[]> MeanHeightInMetersBySpecies;
        public readonly SortedDictionary<FiaCode, float[]> MinDbhInCmBySpecies;

        protected TreeQuantiles()
        {
            this.DeadExpansionFactorBySpecies = [];
            this.LiveExpansionFactorBySpecies = [];
            this.MaxDbhInCmBySpecies = [];
            this.MeanCrownRatioBySpecies = [];
            this.MeanDbhInCmBySpecies = [];
            this.MeanHeightInMetersBySpecies = [];
            this.MinDbhInCmBySpecies = [];
        }

        public TreeQuantiles(TestStand stand, PspStand pspStand, int measurementYear)
            : this()
        {
            float perTreeExpansionFactor = pspStand.GetTreesPerHectareExpansionFactor();

            foreach (KeyValuePair<FiaCode, int[]> initialDbhQuantile in stand.InitialDbhQuantileBySpecies)
            {
                // accumulate stand state into quantiles
                int[] speciesQuantileCounts = new int[TestConstant.DbhQuantiles];
                float[] speciesQuantileLiveExpansionFactor = new float[TestConstant.DbhQuantiles];
                float[] speciesQuantileMaxDbh = new float[TestConstant.DbhQuantiles];
                float[] speciesQuantileMeanDbh = new float[TestConstant.DbhQuantiles];
                float[] speciesQuantileMinDbh = Enumerable.Repeat(Constant.CentimetersPerInch * TestConstant.Maximum.DiameterInInches, TestConstant.DbhQuantiles).ToArray();

                Trees treesOfSpecies = stand.TreesBySpecies[initialDbhQuantile.Key];
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    int quantile = initialDbhQuantile.Value[treeIndex];
                    int tag = treesOfSpecies.Tag[treeIndex];
                    PspTreeMeasurementSeries measurementSeries = pspStand.MeasurementsByTag[tag];
                    if (measurementSeries.DbhInCentimetersByYear.TryGetValue(measurementYear, out float dbh))
                    {
                        speciesQuantileCounts[quantile] += 1;
                        speciesQuantileLiveExpansionFactor[quantile] += perTreeExpansionFactor;
                        speciesQuantileMaxDbh[quantile] = MathF.Max(speciesQuantileMaxDbh[quantile], dbh);
                        speciesQuantileMeanDbh[quantile] += dbh;
                        speciesQuantileMinDbh[quantile] = MathF.Min(speciesQuantileMinDbh[quantile], dbh);
                    }
                }

                for (int quantile = 0; quantile < TestConstant.DbhQuantiles; ++quantile)
                {
                    int quantileCount = speciesQuantileCounts[quantile];
                    if (quantileCount > 0)
                    {
                        speciesQuantileMeanDbh[quantile] = speciesQuantileMeanDbh[quantile] / (float)quantileCount;

                        Debug.Assert(speciesQuantileMinDbh[quantile] / speciesQuantileMaxDbh[quantile] < 1.0001);
                        Debug.Assert(speciesQuantileMinDbh[quantile] / speciesQuantileMeanDbh[quantile] < 1.0001);
                        Debug.Assert(speciesQuantileMeanDbh[quantile] / speciesQuantileMaxDbh[quantile] < 1.0001);
                    }
                }

                FiaCode species = initialDbhQuantile.Key;
                this.DeadExpansionFactorBySpecies.Add(species, new float[TestConstant.DbhQuantiles]);
                this.LiveExpansionFactorBySpecies.Add(species, speciesQuantileLiveExpansionFactor);
                this.MaxDbhInCmBySpecies.Add(species, speciesQuantileMaxDbh);
                this.MeanCrownRatioBySpecies.Add(species, new float[TestConstant.DbhQuantiles]);
                this.MeanDbhInCmBySpecies.Add(species, speciesQuantileMeanDbh);
                this.MeanHeightInMetersBySpecies.Add(species, new float[TestConstant.DbhQuantiles]);
                this.MinDbhInCmBySpecies.Add(species, speciesQuantileMinDbh);
            }
        }

        public TreeQuantiles(TestStand stand)
            : this()
        {
            foreach (KeyValuePair<FiaCode, int[]> initialDbhQuantile in stand.InitialDbhQuantileBySpecies)
            {
                // accumulate stand state into quantiles
                Trees treesOfSpecies = stand.TreesBySpecies[initialDbhQuantile.Key];

                int[] quantileCounts = new int[TestConstant.DbhQuantiles];
                float[] quantileDeadExpansionFactor = new float[TestConstant.DbhQuantiles];
                float[] quantileLiveExpansionFactor = new float[TestConstant.DbhQuantiles];
                float[] quantileMaxDbh = new float[TestConstant.DbhQuantiles];
                float[] quantileMeanCrownRatio = new float[TestConstant.DbhQuantiles];
                float[] quantileMeanDbh = new float[TestConstant.DbhQuantiles];
                float[] quantileMeanHeight = new float[TestConstant.DbhQuantiles];
                float[] quantileMinDbh = Enumerable.Repeat(Constant.CentimetersPerInch * TestConstant.Maximum.DiameterInInches, TestConstant.DbhQuantiles).ToArray();
                for (int treeIndex = 0; treeIndex < initialDbhQuantile.Value.Length; ++treeIndex)
                {
                    int quantile = initialDbhQuantile.Value[treeIndex];
                    float liveExpansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (liveExpansionFactor > 0.0F)
                    {
                        quantileCounts[quantile] += 1;
                        quantileDeadExpansionFactor[quantile] += treesOfSpecies.DeadExpansionFactor[treeIndex];
                        quantileLiveExpansionFactor[quantile] += liveExpansionFactor;
                        quantileMaxDbh[quantile] = MathF.Max(quantileMaxDbh[quantile], treesOfSpecies.Dbh[treeIndex]);
                        quantileMeanCrownRatio[quantile] += liveExpansionFactor * treesOfSpecies.CrownRatio[treeIndex];
                        quantileMeanDbh[quantile] += liveExpansionFactor * treesOfSpecies.Dbh[treeIndex];
                        quantileMeanHeight[quantile] += liveExpansionFactor * treesOfSpecies.Height[treeIndex];
                        quantileMinDbh[quantile] = MathF.Min(quantileMinDbh[quantile], treesOfSpecies.Dbh[treeIndex]);
                    }
                }

                // take averages and convert from Organon's English units to metric
                for (int quantile = 0; quantile < TestConstant.DbhQuantiles; ++quantile)
                {
                    int quantileCount = quantileCounts[quantile];
                    if (quantileCount > 0)
                    {
                        float liveExpansionFactor = quantileLiveExpansionFactor[quantile];
                        quantileDeadExpansionFactor[quantile] = TestConstant.AcresPerHectare * quantileDeadExpansionFactor[quantile];
                        quantileLiveExpansionFactor[quantile] = TestConstant.AcresPerHectare * liveExpansionFactor;
                        quantileMaxDbh[quantile] = Constant.CentimetersPerInch * quantileMaxDbh[quantile];
                        quantileMeanCrownRatio[quantile] /= liveExpansionFactor;
                        quantileMeanDbh[quantile] = Constant.CentimetersPerInch * quantileMeanDbh[quantile] / liveExpansionFactor;
                        quantileMeanHeight[quantile] = Constant.MetersPerFoot * quantileMeanHeight[quantile] / liveExpansionFactor;
                        quantileMinDbh[quantile] = Constant.CentimetersPerInch * quantileMinDbh[quantile];

                        Debug.Assert(quantileMinDbh[quantile] / quantileMaxDbh[quantile] < 1.0001);
                        Debug.Assert(quantileMinDbh[quantile] / quantileMeanDbh[quantile] < 1.0001);
                        Debug.Assert(quantileMeanDbh[quantile] / quantileMaxDbh[quantile] < 1.0001);
                    }
                }

                FiaCode species = initialDbhQuantile.Key;
                this.DeadExpansionFactorBySpecies.Add(species, quantileDeadExpansionFactor);
                this.LiveExpansionFactorBySpecies.Add(species, quantileLiveExpansionFactor);
                this.MaxDbhInCmBySpecies.Add(species, quantileMaxDbh);
                this.MeanCrownRatioBySpecies.Add(species, quantileMeanCrownRatio);
                this.MeanDbhInCmBySpecies.Add(species, quantileMeanDbh);
                this.MeanHeightInMetersBySpecies.Add(species, quantileMeanHeight);
                this.MinDbhInCmBySpecies.Add(species, quantileMinDbh);
            }
        }

        public StreamWriter WriteToCsv(string filePath, OrganonVariant variant, int year)
        {
            FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            StreamWriter writer = new(stream);
            writer.WriteLine("variant,year,species,quantile,mean DBH,mean height,expansion factor,dead expansion factor,crown ratio,min DBH,max DBH");
            this.WriteToCsv(writer, variant, year);
            return writer;
        }

        public void WriteToCsv(StreamWriter writer, OrganonVariant variant, int year)
        {
            foreach (KeyValuePair<FiaCode, float[]> meanDbhInCmForSpecies in this.MeanDbhInCmBySpecies)
            {
                FiaCode species = meanDbhInCmForSpecies.Key;
                string speciesCode = species.ToFourLetterCode();
                // BUGBUG: Huffman Peak hack
                if (species == FiaCode.AbiesConcolor)
                {
                    speciesCode = FiaCode.AbiesProcera.ToFourLetterCode();
                }
                else if (species == FiaCode.AbiesGrandis)
                {
                    speciesCode = FiaCode.AbiesAmabalis.ToFourLetterCode();
                }
                float[] deadExpansionFactors = this.DeadExpansionFactorBySpecies[species];
                float[] liveExpansionFactors = this.LiveExpansionFactorBySpecies[species];
                float[] maxDbh = this.MaxDbhInCmBySpecies[species];
                float[] meanCrownRatios = this.MeanCrownRatioBySpecies[species];
                float[] meanHeight = this.MeanHeightInMetersBySpecies[species];
                float[] minDbh = this.MinDbhInCmBySpecies[species];

                for (int quantile = 0; quantile < TestConstant.DbhQuantiles; ++quantile)
                {
                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", variant.TreeModel, year, speciesCode, quantile, 
                                     meanDbhInCmForSpecies.Value[quantile], meanHeight[quantile], liveExpansionFactors[quantile], 
                                     deadExpansionFactors[quantile], meanCrownRatios[quantile], minDbh[quantile], maxDbh[quantile]);
                }
            }
        }
    }
}
