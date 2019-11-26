using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Osu.Cof.Organon.Test
{
    internal class TreeQuantiles
    {
        public readonly Dictionary<FiaCode, float[]> DeadExpansionFactorBySpecies;
        public readonly Dictionary<FiaCode, float[]> LiveExpansionFactorBySpecies;
        public readonly Dictionary<FiaCode, float[]> MaxDbhInCmBySpecies;
        public readonly Dictionary<FiaCode, float[]> MeanCrownRatioBySpecies;
        public readonly Dictionary<FiaCode, float[]> MeanDbhInCmBySpecies;
        public readonly Dictionary<FiaCode, float[]> MeanHeightInMetersBySpecies;
        public readonly Dictionary<FiaCode, float[]> MinDbhInCmBySpecies;

        protected TreeQuantiles()
        {
            this.DeadExpansionFactorBySpecies = new Dictionary<FiaCode, float[]>();
            this.LiveExpansionFactorBySpecies = new Dictionary<FiaCode, float[]>();
            this.MaxDbhInCmBySpecies = new Dictionary<FiaCode, float[]>();
            this.MeanCrownRatioBySpecies = new Dictionary<FiaCode, float[]>();
            this.MeanDbhInCmBySpecies = new Dictionary<FiaCode, float[]>();
            this.MeanHeightInMetersBySpecies = new Dictionary<FiaCode, float[]>();
            this.MinDbhInCmBySpecies = new Dictionary<FiaCode, float[]>();
        }

        public TreeQuantiles(TestStand stand, PspStand pspStand, int measurementYear)
            : this()
        {
            float perTreeExpansionFactor = pspStand.GetTreesPerHectareExpansionFactor();

            foreach (KeyValuePair<FiaCode, List<int>> treeIndicesForSpecies in stand.TreeIndicesBySpecies)
            {
                // accumulate stand state into quantiles
                List<int> treeIndices = treeIndicesForSpecies.Value;
                int[] quantileCounts = new int[TestConstant.DbhQuantiles];
                float[] quantileLiveExpansionFactor = new float[TestConstant.DbhQuantiles];
                float[] quantileMaxDbh = new float[TestConstant.DbhQuantiles];
                float[] quantileMeanDbh = new float[TestConstant.DbhQuantiles];
                float[] quantileMinDbh = Enumerable.Repeat(TestConstant.CmPerInch * TestConstant.Maximum.DbhInInches, TestConstant.DbhQuantiles).ToArray();

                for (int speciesIndex = 0; speciesIndex < treeIndices.Count; ++speciesIndex)
                {
                    int treeIndex = treeIndices[speciesIndex];
                    int quantile = stand.QuantileByInitialDbh[treeIndex];
                    int tag = stand.Tag[treeIndex];
                    PspTreeMeasurementSeries measurementSeries = pspStand.MeasurementsByTag[tag];
                    if (measurementSeries.DbhInCentimetersByYear.TryGetValue(measurementYear, out float dbh))
                    {
                        quantileCounts[quantile] += 1;
                        quantileLiveExpansionFactor[quantile] += perTreeExpansionFactor;
                        quantileMaxDbh[quantile] = Math.Max(quantileMaxDbh[quantile], dbh);
                        quantileMeanDbh[quantile] += dbh;
                        quantileMinDbh[quantile] = Math.Min(quantileMinDbh[quantile], dbh);
                    }
                }

                for (int quantile = 0; quantile < TestConstant.DbhQuantiles; ++quantile)
                {
                    int quantileCount = quantileCounts[quantile];
                    if (quantileCount > 0)
                    {
                        quantileMeanDbh[quantile] = quantileMeanDbh[quantile] / (float)quantileCount;

                        Debug.Assert(quantileMinDbh[quantile] / quantileMaxDbh[quantile] < 1.0001);
                        Debug.Assert(quantileMinDbh[quantile] / quantileMeanDbh[quantile] < 1.0001);
                        Debug.Assert(quantileMeanDbh[quantile] / quantileMaxDbh[quantile] < 1.0001);
                    }
                }

                FiaCode species = treeIndicesForSpecies.Key;
                this.DeadExpansionFactorBySpecies.Add(species, new float[TestConstant.DbhQuantiles]);
                this.LiveExpansionFactorBySpecies.Add(species, quantileLiveExpansionFactor);
                this.MaxDbhInCmBySpecies.Add(species, quantileMaxDbh);
                this.MeanCrownRatioBySpecies.Add(species, new float[TestConstant.DbhQuantiles]);
                this.MeanDbhInCmBySpecies.Add(species, quantileMeanDbh);
                this.MeanHeightInMetersBySpecies.Add(species, new float[TestConstant.DbhQuantiles]);
                this.MinDbhInCmBySpecies.Add(species, quantileMinDbh);
            }
        }

        public TreeQuantiles(TestStand stand)
            : this()
        {
            foreach (KeyValuePair<FiaCode, List<int>> treeIndicesForSpecies in stand.TreeIndicesBySpecies)
            {
                // accumulate stand state into quantiles
                List<int> treeIndices = treeIndicesForSpecies.Value;
                int[] quantileCounts = new int[TestConstant.DbhQuantiles];
                float[] quantileDeadExpansionFactor = new float[TestConstant.DbhQuantiles];
                float[] quantileLiveExpansionFactor = new float[TestConstant.DbhQuantiles];
                float[] quantileMaxDbh = new float[TestConstant.DbhQuantiles];
                float[] quantileMeanCrownRatio = new float[TestConstant.DbhQuantiles];
                float[] quantileMeanDbh = new float[TestConstant.DbhQuantiles];
                float[] quantileMeanHeight = new float[TestConstant.DbhQuantiles];
                float[] quantileMinDbh = Enumerable.Repeat(TestConstant.CmPerInch * TestConstant.Maximum.DbhInInches, TestConstant.DbhQuantiles).ToArray();
                for (int speciesIndex = 0; speciesIndex < treeIndices.Count; ++speciesIndex)
                {
                    int treeIndex = treeIndices[speciesIndex];
                    Debug.Assert(treeIndicesForSpecies.Key == stand.Species[treeIndex]);

                    int quantile = stand.QuantileByInitialDbh[treeIndex];
                    float liveExpansionFactor = stand.LiveExpansionFactor[treeIndex];
                    if (liveExpansionFactor > 0.0F)
                    {
                        quantileCounts[quantile] += 1;
                        quantileDeadExpansionFactor[quantile] += stand.DeadExpansionFactor[treeIndex];
                        quantileLiveExpansionFactor[quantile] += liveExpansionFactor;
                        quantileMaxDbh[quantile] = Math.Max(quantileMaxDbh[quantile], stand.Dbh[treeIndex]);
                        quantileMeanCrownRatio[quantile] += liveExpansionFactor * stand.CrownRatio[treeIndex];
                        quantileMeanDbh[quantile] += liveExpansionFactor * stand.Dbh[treeIndex];
                        quantileMeanHeight[quantile] += liveExpansionFactor * stand.Height[treeIndex];
                        quantileMinDbh[quantile] = Math.Min(quantileMinDbh[quantile], stand.Dbh[treeIndex]);
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
                        quantileMaxDbh[quantile] = TestConstant.CmPerInch * quantileMaxDbh[quantile];
                        quantileMeanCrownRatio[quantile] /= liveExpansionFactor;
                        quantileMeanDbh[quantile] = TestConstant.CmPerInch * quantileMeanDbh[quantile] / liveExpansionFactor;
                        quantileMeanHeight[quantile] = TestConstant.MetersPerFoot * quantileMeanHeight[quantile] / liveExpansionFactor;
                        quantileMinDbh[quantile] = TestConstant.CmPerInch * quantileMinDbh[quantile];

                        Debug.Assert(quantileMinDbh[quantile] / quantileMaxDbh[quantile] < 1.0001);
                        Debug.Assert(quantileMinDbh[quantile] / quantileMeanDbh[quantile] < 1.0001);
                        Debug.Assert(quantileMeanDbh[quantile] / quantileMaxDbh[quantile] < 1.0001);
                    }
                }

                FiaCode species = treeIndicesForSpecies.Key;
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
            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            StreamWriter writer = new StreamWriter(stream);
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
                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", variant.Variant, year, speciesCode, quantile, 
                                     meanDbhInCmForSpecies.Value[quantile], meanHeight[quantile], liveExpansionFactors[quantile], 
                                     deadExpansionFactors[quantile], meanCrownRatios[quantile], minDbh[quantile], maxDbh[quantile]);
                }
            }
        }
    }
}
