﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm.Test
{
    [TestClass]
    public class Mathematics
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public void Exp()
        {
            float accuracy = 4E-6F; // error of up to 4E-6 expected for 4th order polynomial
            float precision = 5E-7F; // numerical error

            //                              0       1      2      3      4      5      6       7      8     9     10    11    12     13    14    15    16    17    18    19
            float[] values = new float[] { -10.0F, -5.0F, -4.0F, -3.0F, -2.0F, -1.5F, -1.33F, -1.0F, -0.5F, 0.0F, 0.5F, 1.0F, 1.33F, 1.5F, 2.0F, 2.5F, 3.0F, 4.0F, 5.0F, 10.0F };
            for (int index = 0; index < values.Length; ++index)
            {
                float value = values[index];
                float exp = MathV.Exp(value);
                float exp10 = MathV.Exp10(value);
                float exp2 = MathV.Exp2(value);

                double expError = 1.0 - exp / Math.Exp(value);
                double exp10Error = 1.0 - exp10 / Math.Pow(10.0, value);
                double exp2Error = 1.0 - exp2 / Math.Pow(2.0, value);

                double tolerance = accuracy * Math.Abs(value) + precision;
                Assert.IsTrue(Math.Abs(expError) < tolerance);
                Assert.IsTrue(Math.Abs(exp10Error) < tolerance);
                Assert.IsTrue(Math.Abs(exp2Error) < tolerance);
            }

            for (int quadIndex = 0; quadIndex < values.Length; quadIndex += 4)
            {
                Vector128<float> value = Vector128.Create(values[quadIndex], values[quadIndex + 1], values[quadIndex + 2], values[quadIndex + 3]);
                Vector128<float> exp = MathV.Exp(value);
                Vector128<float> exp10 = MathV.Exp10(value);
                Vector128<float> exp2 = MathV.Exp2(value);

                for (int scalarIndex = 0; scalarIndex < 4; ++scalarIndex)
                {
                    float scalarValue = value.GetElement(scalarIndex);
                    float scalarExp = exp.GetElement(scalarIndex);
                    float scalarExp10 = exp10.GetElement(scalarIndex);
                    float scalarExp2 = exp2.GetElement(scalarIndex);

                    double expError = 1.0 - scalarExp / Math.Exp(scalarValue);
                    double exp10Error = 1.0 - scalarExp10 / Math.Pow(10.0, scalarValue);
                    double exp2Error = 1.0 - scalarExp2 / Math.Pow(2.0, scalarValue);

                    double tolerance = accuracy * Math.Abs(scalarValue) + precision;
                    Assert.IsTrue(Math.Abs(expError) < tolerance);
                    Assert.IsTrue(Math.Abs(exp10Error) < tolerance);
                    Assert.IsTrue(Math.Abs(exp2Error) < tolerance);
                }
            }
        }

        [TestMethod]
        public void Log()
        {
            float accuracy = 2E-6F; // error of up to 2E-6 expected for 5th order polynomial
            float precision = 5E-7F; // numerical error

            //                             0       1           2     3     4     5     6                   7      8      9        10        11
            float[] values = new float[] { 1E-30F, 0.0000099F, 0.1F, 0.5F, 1.0F, 2.0F, 2.718281828459045F, 10.0F, 17.0F, 2.22E3F, 3.336E6F, 30E30F };
            for (int index = 0; index < values.Length; ++index)
            {
                float value = values[index];
                float ln = MathV.Ln(value);
                float log10 = MathV.Log10(value);
                float log2 = MathV.Log2(value);

                if (value != 1.0F)
                {
                    double lnError = 1.0 - ln / Math.Log(value);
                    double log10Error = 1.0 - log10 / Math.Log10(value);
                    double log2Error = 1.0 - log2 / Math.Log2(value);

                    double tolerance = accuracy * value + precision;
                    Assert.IsTrue(Math.Abs(lnError) < tolerance);
                    Assert.IsTrue(Math.Abs(log10Error) < tolerance);
                    Assert.IsTrue(Math.Abs(log2Error) < tolerance);
                }
                else
                {
                    Assert.IsTrue(ln == 0.0F);
                    Assert.IsTrue(log10 == 0.0F);
                    Assert.IsTrue(log2 == 0.0F);
                }
            }

            for (int quadIndex = 0; quadIndex < values.Length; quadIndex += 4)
            {
                Vector128<float> value = Vector128.Create(values[quadIndex], values[quadIndex + 1], values[quadIndex + 2], values[quadIndex + 3]);
                Vector128<float> ln = MathV.Ln(value);
                Vector128<float> log10 = MathV.Log10(value);
                Vector128<float> log2 = MathV.Log2(value);

                for (int scalarIndex = 0; scalarIndex < 4; ++scalarIndex)
                {
                    float scalarValue = value.GetElement(scalarIndex);
                    float scalarLn = ln.GetElement(scalarIndex);
                    float scalarLog10 = log10.GetElement(scalarIndex);
                    float scalarLog2 = log2.GetElement(scalarIndex);
                    if (scalarValue != 1.0F)
                    {
                        double lnError = 1.0 - scalarLn / Math.Log(scalarValue);
                        double log10Error = 1.0 - scalarLog10 / Math.Log10(scalarValue);
                        double log2Error = 1.0 - scalarLog2 / Math.Log2(scalarValue);

                        double tolerance = accuracy * scalarValue + precision;
                        Assert.IsTrue(Math.Abs(lnError) < tolerance);
                        Assert.IsTrue(Math.Abs(log10Error) < tolerance);
                        Assert.IsTrue(Math.Abs(log2Error) < tolerance);
                    }
                    else
                    {
                        Assert.IsTrue(scalarLn == 0.0F);
                        Assert.IsTrue(scalarLog10 == 0.0F);
                        Assert.IsTrue(scalarLog2 == 0.0F);
                    }
                }
            }
        }

        [TestMethod]
        public void Population()
        {
            List<int> thinningPeriods = new() { 0, 1 };

            Population binaryPopulation = new(2, 0.5F, 5);
            binaryPopulation.IndividualFitness[0] = 0.0F;
            binaryPopulation.IndividualFitness[1] = 1.0F;
            binaryPopulation.IndividualTreeSelections[0] = new int[] { 0, 0, 0, 0, 0 };
            binaryPopulation.IndividualTreeSelections[1] = new int[] { 1, 1, 1, 1, 1 };

            Population clones = new(2, 0.5F, 5);
            clones.IndividualFitness[0] = 0.0F;
            clones.IndividualFitness[1] = 0.0F;
            clones.IndividualTreeSelections[0] = new int[] { 0, 0, 0, 0, 0 };
            clones.IndividualTreeSelections[1] = new int[] { 0, 0, 0, 0, 0 };

            Population heterozygousPopulation = new(2, 0.5F, 5);
            heterozygousPopulation.IndividualFitness[0] = 0.4F;
            heterozygousPopulation.IndividualFitness[1] = 0.6F;
            heterozygousPopulation.IndividualTreeSelections[0] = new int[] { 1, 0, 0, 1, 0 };
            heterozygousPopulation.IndividualTreeSelections[1] = new int[] { 1, 0, 1, 0, 1 };

            PopulationStatistics statistics = new();
            statistics.AddGeneration(binaryPopulation, thinningPeriods);
            statistics.AddGeneration(clones, thinningPeriods);
            statistics.AddGeneration(heterozygousPopulation, thinningPeriods);

            Assert.IsTrue(statistics.Generations == 3);

            Assert.IsTrue(statistics.CoefficientOfVarianceByGeneration[0] == 1.0F);
            Assert.IsTrue(statistics.MaximumFitnessByGeneration[0] == 1.0F);
            Assert.IsTrue(statistics.MeanFitnessByGeneration[0] == 0.5F);
            Assert.IsTrue(statistics.MinimumFitnessByGeneration[0] == 0.0F);
            Assert.IsTrue(statistics.MeanHeterozygosityByGeneration[0] == 0.5F);
            Assert.IsTrue(statistics.NewIndividualsByGeneration[0] == 0);
            Assert.IsTrue(statistics.PolymorphismByGeneration[0] == 1.0F);

            Assert.IsTrue(statistics.CoefficientOfVarianceByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.MaximumFitnessByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.MeanFitnessByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.MinimumFitnessByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.MeanHeterozygosityByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.NewIndividualsByGeneration[1] == 0);
            Assert.IsTrue(statistics.PolymorphismByGeneration[1] == 0.0F);

            Assert.IsTrue(MathF.Round(statistics.CoefficientOfVarianceByGeneration[2], 6) == 0.2F);
            Assert.IsTrue(statistics.MaximumFitnessByGeneration[2] == 0.6F);
            Assert.IsTrue(statistics.MeanFitnessByGeneration[2] == 0.5F);
            Assert.IsTrue(statistics.MinimumFitnessByGeneration[2] == 0.4F);
            Assert.IsTrue(statistics.MeanHeterozygosityByGeneration[2] == 0.3F);
            Assert.IsTrue(statistics.NewIndividualsByGeneration[2] == 0);
            Assert.IsTrue(statistics.PolymorphismByGeneration[2] == 0.6F);
        }

        [TestMethod]
        public void Pow()
        {
            float accuracy = 4E-6F; // error of up to 2E-6 expected for 5th order polynomial
            float precision = 1.2E-6F; // numerical error

            float[] x = new float[] { 0.0F, 0.0001F, 0.1F,  0.1F,   1.0F,  1.0F, 1.3F, 2.0F, 1E6F };
            float[] y = new float[] { 0.1F, 0.0001F, 1.0F, -0.25F, 80.0F, -1.0F, 0.6F, 2.0F, 2.22F };

            for (int index = 0; index < x.Length; ++index)
            {
                float power = MathV.Pow(x[index], y[index]);

                if (x[index] != 0.0F)
                {
                    double powError = 1.0 - power / Math.Pow(x[index], y[index]);
                    double tolerance = accuracy * Math.Abs(Math.Pow(x[index], y[index])) + precision;
                    Assert.IsTrue(Math.Abs(powError) < tolerance);
                }
                else
                {
                    Assert.IsTrue(power == 0.0F);
                }
            }
        }

        [TestMethod]
        public void Reforestation()
        {
            float reforestationNpv = TimberValue.Default.GetNetPresentReforestationValue(Constant.AcresPerHectare * 380.0F);
            Assert.IsTrue(reforestationNpv > -277.76F);
            Assert.IsTrue(reforestationNpv < -0.999 * 277.76F);
        }

        [TestMethod]
        public void Simd()
        {
            Vector128<float> broadcastFloat = AvxExtensions.BroadcastScalarToVector128(Constant.ForestersEnglish);
            Vector128<int> broadcastInt = AvxExtensions.BroadcastScalarToVector128(1);

            AssertV.IsTrue(Avx.CompareEqual(broadcastFloat, Vector128.Create(Constant.ForestersEnglish)));
            AssertV.IsTrue(Avx.CompareEqual(broadcastInt, Vector128.Create(1)));

            Vector128<int> index = Vector128.Create(0, 1, 2, 3);
            Vector128<int> shuffle1 = Avx.Shuffle(index, Constant.Simd128x4.ShuffleRotateLower1);
            Vector128<int> shuffle2 = Avx.Shuffle(index, Constant.Simd128x4.ShuffleRotateLower2);
            Vector128<int> shuffle3 = Avx.Shuffle(index, Constant.Simd128x4.ShuffleRotateLower3);

            AssertV.IsTrue(Avx.CompareEqual(shuffle1, Vector128.Create(1, 2, 3, 0)));
            AssertV.IsTrue(Avx.CompareEqual(shuffle2, Vector128.Create(2, 3, 0, 1)));
            AssertV.IsTrue(Avx.CompareEqual(shuffle3, Vector128.Create(3, 0, 1, 2)));

            Assert.IsTrue(shuffle1.ToScalar() == 1);
            Assert.IsTrue(shuffle2.ToScalar() == 2);
            Assert.IsTrue(shuffle3.ToScalar() == 3);
        }

        [TestMethod]
        public void VolumeFia()
        {
            int treeCount = 42;

            // TODO: TSHE, THPL, ...
            Trees trees = new(FiaCode.PseudotsugaMenziesii, treeCount, Units.English);
            float[] fiaMerchantableCubicFeetPerAcre = new float[treeCount];
            float[] fiaScribnerBoardFeetPerAcre = new float[treeCount];
            float merchantableCubicFeetPerAcre = 0.0F;
            float merchantableCubicMetersPerHectare = 0.0F;
            float totalCylinderCubicMeterVolumePerAcre = 0.0F;
            float totalScribnerBoardFeetPerAcre = 0.0F;
            for (int compactedTreeIndex = 0; compactedTreeIndex < treeCount; ++compactedTreeIndex)
            {
                // create trees with a range of expansion factors to catch errors in expansion factor management
                float treeRatio = (float)compactedTreeIndex / (float)treeCount;
                TreeRecord tree = new(compactedTreeIndex, trees.Species, (float)compactedTreeIndex, 1.0F - 0.75F * treeRatio, 0.6F + compactedTreeIndex);
                trees.Add(tree.Tag, tree.DbhInInches, tree.HeightInFeet, tree.CrownRatio, tree.LiveExpansionFactor);

                float dbhInMeters = TestConstant.MetersPerInch * tree.DbhInInches;
                float heightInMeters = Constant.MetersPerFoot * tree.HeightInFeet;
                float treeSizedCylinderCubicMeterVolumePerAcre = tree.LiveExpansionFactor * 0.25F * MathF.PI * dbhInMeters * dbhInMeters * heightInMeters;

                fiaMerchantableCubicFeetPerAcre[compactedTreeIndex] = tree.LiveExpansionFactor * FiaVolume.GetMerchantableCubicFeet(trees, compactedTreeIndex);
                merchantableCubicFeetPerAcre += fiaMerchantableCubicFeetPerAcre[compactedTreeIndex];
                fiaScribnerBoardFeetPerAcre[compactedTreeIndex] = tree.LiveExpansionFactor * FiaVolume.GetScribnerBoardFeet(trees, compactedTreeIndex);
                totalScribnerBoardFeetPerAcre += fiaScribnerBoardFeetPerAcre[compactedTreeIndex];

                merchantableCubicMetersPerHectare += PoudelRegressions.GetCubicVolume(trees, compactedTreeIndex);

                // taper coefficient should be in the vicinity of 0.3 for larger trees, but this is not well defined for small trees
                // Lower bound can be made more stringent if necessary.
                Assert.IsTrue(fiaMerchantableCubicFeetPerAcre[compactedTreeIndex] >= 0.0);
                Assert.IsTrue(fiaMerchantableCubicFeetPerAcre[compactedTreeIndex] <= 0.4 * Constant.CubicFeetPerCubicMeter * treeSizedCylinderCubicMeterVolumePerAcre);

                Assert.IsTrue(fiaScribnerBoardFeetPerAcre[compactedTreeIndex] >= 0.0);
                Assert.IsTrue(fiaScribnerBoardFeetPerAcre[compactedTreeIndex] <= 6.5 * 0.4 * Constant.CubicFeetPerCubicMeter * treeSizedCylinderCubicMeterVolumePerAcre);
                totalCylinderCubicMeterVolumePerAcre += treeSizedCylinderCubicMeterVolumePerAcre;
            }

            float totalCylinderCubicFeetVolumePerAcre = Constant.CubicFeetPerCubicMeter * totalCylinderCubicMeterVolumePerAcre;
            Assert.IsTrue(merchantableCubicFeetPerAcre >= 0.05 * totalCylinderCubicFeetVolumePerAcre);
            Assert.IsTrue(merchantableCubicFeetPerAcre <= 0.35 * totalCylinderCubicFeetVolumePerAcre);
            Assert.IsTrue(merchantableCubicFeetPerAcre >= 0.5 * Constant.HectaresPerAcre * Constant.CubicFeetPerCubicMeter * merchantableCubicMetersPerHectare);

            Assert.IsTrue(merchantableCubicMetersPerHectare <= 0.35 * Constant.AcresPerHectare * totalCylinderCubicMeterVolumePerAcre);

            Assert.IsTrue(totalScribnerBoardFeetPerAcre >= 1.75 * 0.35 * totalCylinderCubicFeetVolumePerAcre);
            Assert.IsTrue(totalScribnerBoardFeetPerAcre <= 6.5 * 0.40 * totalCylinderCubicFeetVolumePerAcre);

            // check SIMD 128 result against scalar
            float totalScribnerBoardFeetPerAcre128 = FiaVolume.GetScribnerBoardFeetPerAcre(trees);
            float simdScalarScribnerDifference = totalScribnerBoardFeetPerAcre - totalScribnerBoardFeetPerAcre128;
            Assert.IsTrue(MathF.Abs(simdScalarScribnerDifference) < 0.004 * totalScribnerBoardFeetPerAcre);
        }

        [TestMethod]
        public void VolumePerformance()
        {
            int iterations = 1025; // 5 warmup run + measured runs
            int treeCount = 100 * 1000;
            #if DEBUG
            iterations = 25; // do only functional validation of test on DEBUG builds to reduce test execution time
            treeCount = 125;
            #endif

            Trees trees = new(FiaCode.PseudotsugaMenziesii, treeCount, Units.English);
            float expansionFactor = 0.5F;
            for (int treeIndex = 0; treeIndex < treeCount; ++treeIndex)
            {
                float dbhInInches = (float)(treeIndex % 36 + 4);
                trees.Add(treeIndex, dbhInInches, 16.0F * MathF.Sqrt(dbhInInches) + 4.5F, 0.01F * (float)(treeIndex % 100), expansionFactor);
            }
            FiaVolume volume = new();

            Stopwatch runtime = new();
            float accumulatedBoardFeetPerAcre = 0.0F;
            for (int iteration = 0; iteration < iterations; ++iteration)
            {
                if (iteration > 20)
                {
                    // skip first few runs as warmup runs
                    runtime.Start();
                }

                // 1000 runs * 10,000 trees = 10M volumes on i7-3770
                // .NET standard 2.0 + MathF: 12.854s -> 686k trees/core-s
                //      RS616 pow() -> exp(): 13.254s -> 754k
                //              MathV scalar:  9.451s -> 1.06M
                //          reuse log10(DBH):  8.897s -> 1.12M
                //       aggressive inlining:  7.998s -> 1.20M
                //             internal loop:  7.785s -> 1.28M
                //            single species:  7.619s -> 1.31M
                //                   VEX 128:  2.063s -> 4.85M
                float standBoardFeetPerAcre = FiaVolume.GetScribnerBoardFeetPerAcre(trees);
                #if DEBUG
                Assert.IsTrue(standBoardFeetPerAcre > 35.0F * 1000.0F * expansionFactor);
                Assert.IsTrue(standBoardFeetPerAcre < 40.0F * 1000.0F * expansionFactor);
                #endif
                accumulatedBoardFeetPerAcre += standBoardFeetPerAcre;
            }
            runtime.Stop();

            this.TestContext!.WriteLine(runtime.Elapsed.TotalSeconds.ToString());
        }

        [TestMethod]
        public void VolumeTaper()
        {
            List<ExpectedTreeVolume> trees = new()
            {
                // trees very near height and diameter class breaks may estimate differently between metric and English due to numerical precision
                new ExpectedTreeVolume()
                {
                    Species = FiaCode.PseudotsugaMenziesii,
                    Dbh = 19.4F,
                    Height = 21.2F,
                    ExpansionFactor = 1.0F,
                    MinimumMerchantableStemVolumeFraction = 0.51F,
                    MinimumRegenVolumeCubic = 0.172F,
                    MinimumRegenVolumeScribner = 29.999F,
                    MinimumThinVolumeCubic = 0.154F,
                    MinimumThinVolumeScribner = 29.999F
                },
                new ExpectedTreeVolume()
                {
                    Species = FiaCode.PseudotsugaMenziesii,
                    Dbh = 30.01F,
                    Height = 30.01F,
                    ExpansionFactor = 0.36F,
                    MinimumMerchantableStemVolumeFraction = 0.91F,
                    MinimumRegenVolumeCubic = 0.860F,
                    MinimumRegenVolumeScribner = 119.999F,
                    MinimumThinVolumeCubic = 0.839F,
                    MinimumThinVolumeScribner = 119.999F
                },
                new ExpectedTreeVolume()
                {
                    Species = FiaCode.PseudotsugaMenziesii,
                    Dbh = 46.2F,
                    Height = 41.8F,
                    ExpansionFactor = 4.34F,
                    MinimumMerchantableStemVolumeFraction = 0.95F,
                    MinimumRegenVolumeCubic = 2.64F,
                    MinimumRegenVolumeScribner = 419.999F,
                    MinimumThinVolumeCubic = 2.66F,
                    MinimumThinVolumeScribner = 469.999F
                }
            };

            Stopwatch stopwatch = new();
            stopwatch.Start();
            TimberValue timberValue = new(Constant.Bucking.DefaultMaximumDiameterInCentimeters, Constant.Bucking.DefaultMaximumHeightInMeters, false);
            stopwatch.Stop();
            TimeSpan timberValueTabulationTime = stopwatch.Elapsed;
            this.TestContext!.WriteLine("tabulation: {0:s\\.fff}s for {1:0.0} cm diameter classes and {2:0.0} m height classes", timberValueTabulationTime, Constant.Bucking.DiameterClassSizeInCentimeters, Constant.Bucking.HeightClassSizeInMeters);

            // rounding to nearest indices in volume table
            // 1 cm diameter classes, 0.5 m height classes
            //float volumeHighSideTolerance = 1.01F;
            //float volumeLowSideTolerance = 1.00F;

            // bilinear interpolation
            // 1 cm diameter classes, 0.5 m height classes
            //float volumeHighSideTolerance = 1.16F;
            //float volumeLowSideTolerance = 0.97F;
            // 1 cm diameter classes, 1 m height classes
            float volumeHighSideTolerance = 1.27F;
            float volumeLowSideTolerance = 0.98F;
            // 2 cm diameter classes, 2 m height classes
            //float volumeLowSideTolerance = 0.95F;
            //float volumeHighSideTolerance = 1.26F;
            // 5 cm diameter classes, 5 m height classes
            //float volumeLowSideTolerance = 0.95F;
            //float volumeHighSideTolerance = 1.26F;
            // 10 cm diameter classes, 5 m height classes
            //float volumeLowSideTolerance = 0.95F;
            //float volumeHighSideTolerance = 1.25F;
            foreach (ExpectedTreeVolume tree in trees)
            {
                float dbhInCentimeters = tree.Dbh;
                float heightInMeters = tree.Height;
                Trees psmeEnglish = new(FiaCode.PseudotsugaMenziesii, 1, Units.English);
                psmeEnglish.Add(1, Constant.InchesPerCentimeter * dbhInCentimeters, Constant.FeetPerMeter * heightInMeters, 0.5F, tree.ExpansionFactor);
                Trees psmeMetric = new(FiaCode.PseudotsugaMenziesii, 1, Units.Metric);
                psmeMetric.Add(1, dbhInCentimeters, heightInMeters, 0.5F, tree.ExpansionFactor);

                timberValue.ScaledVolumeRegenerationHarvest.GetScribnerVolume(psmeEnglish, out double regenScribner2SawEnglish, out double regenScribner3SawEnglish, out double regenScribner4SawEnglish);
                double regenScribnerEnglish = regenScribner2SawEnglish + regenScribner3SawEnglish + regenScribner4SawEnglish;
                timberValue.ScaledVolumeRegenerationHarvest.GetGradedVolume(psmeEnglish, out double regenCubic2SawEnglish, out double regenCubic3SawEnglish, out double regenCubic4SawEnglish, out regenScribner2SawEnglish, out regenScribner3SawEnglish, out regenScribner4SawEnglish);
                double regenCubicEnglish = regenCubic2SawEnglish + regenCubic3SawEnglish + regenCubic4SawEnglish;
                double regenScribnerEnglishCheck = regenScribner2SawEnglish + regenScribner3SawEnglish + regenScribner4SawEnglish;

                timberValue.ScaledVolumeRegenerationHarvest.GetScribnerVolume(psmeMetric, out double regenScribner2SawMetric, out double regenScribner3SawMetric, out double regenScribner4SawMetric);
                double regenScribnerMetric = regenScribner2SawMetric + regenScribner3SawMetric + regenScribner4SawMetric;
                timberValue.ScaledVolumeRegenerationHarvest.GetGradedVolume(psmeMetric, out double regenCubic2SawMetric, out double regenCubic3SawMetric, out double regenCubic4SawMetric, out regenScribner2SawMetric, out regenScribner3SawMetric, out regenScribner4SawMetric);
                double regenCubicMetric = regenCubic2SawMetric + regenCubic3SawMetric + regenCubic4SawMetric;
                double regenScribnerMetricCheck = regenScribner2SawMetric + regenScribner3SawMetric + regenScribner4SawMetric;

                timberValue.ScaledVolumeThinning.GetScribnerVolume(psmeEnglish, out double thinScribner2SawEnglish, out double thinScribner3SawEnglish, out double thinScribner4SawEnglish);
                double thinScribnerEnglish = thinScribner2SawEnglish + thinScribner3SawEnglish + thinScribner4SawEnglish;
                timberValue.ScaledVolumeThinning.GetGradedVolume(psmeEnglish, out double thinCubic2SawEnglish, out double thinCubic3SawEnglish, out double thinCubic4SawEnglish, out thinScribner2SawEnglish, out thinScribner3SawEnglish, out thinScribner4SawEnglish);
                double thinCubicEnglish = thinCubic2SawEnglish + thinCubic3SawEnglish + thinCubic4SawEnglish;
                double thinScribnerEnglishCheck = thinScribner2SawEnglish + thinScribner3SawEnglish + thinScribner4SawEnglish;

                timberValue.ScaledVolumeThinning.GetScribnerVolume(psmeMetric, out double thinScribner2SawMetric, out double thinScribner3SawMetric, out double thinScribner4SawMetric);
                double thinScribnerMetric = thinScribner2SawMetric + thinScribner3SawMetric + thinScribner4SawMetric;
                timberValue.ScaledVolumeThinning.GetGradedVolume(psmeMetric, out double thinCubic2SawMetric, out double thinCubic3SawMetric, out double thinCubic4SawMetric, out thinScribner2SawMetric, out thinScribner3SawMetric, out thinScribner4SawMetric);
                double thinCubicMetric = thinCubic2SawMetric + thinCubic3SawMetric + thinCubic4SawMetric;
                double thinScribnerMetricCheck = thinScribner2SawMetric + thinScribner3SawMetric + thinScribner4SawMetric;

                Assert.IsTrue(Math.Abs(regenCubicEnglish - regenCubicMetric) < 0.000003 * regenCubicMetric);
                Assert.IsTrue(Math.Abs(regenScribnerEnglish - regenScribnerMetric) < 0.000003 * regenScribnerMetric);
                Assert.IsTrue(Math.Abs(regenScribnerEnglish - regenScribnerEnglishCheck) < 0.000002 * regenScribnerEnglishCheck);
                Assert.IsTrue(Math.Abs(regenScribnerMetric - regenScribnerMetricCheck) < 0.000002 * regenScribnerMetricCheck);
                Assert.IsTrue(Math.Abs(thinCubicEnglish - thinCubicMetric) < 0.000004 * thinCubicMetric);
                Assert.IsTrue(Math.Abs(thinScribnerEnglish - thinScribnerMetric) < 0.000003 * thinScribnerMetric);
                Assert.IsTrue(Math.Abs(thinScribnerEnglish - thinScribnerEnglishCheck) < 0.000002 * thinScribnerEnglishCheck);
                Assert.IsTrue(Math.Abs(thinScribnerMetric - thinScribnerMetricCheck) < 0.000002 * thinScribnerMetricCheck);

                regenCubicMetric /= tree.ExpansionFactor;
                regenScribnerMetric /= tree.ExpansionFactor;
                thinCubicMetric /= tree.ExpansionFactor;
                thinScribnerMetric /= tree.ExpansionFactor;

                Assert.IsTrue(thinCubicMetric >= volumeLowSideTolerance * tree.MinimumThinVolumeCubic);
                Assert.IsTrue(thinScribnerMetric >= volumeLowSideTolerance * tree.MinimumThinVolumeScribner);
                Assert.IsTrue(thinCubicMetric < volumeHighSideTolerance * tree.MinimumThinVolumeCubic);
                Assert.IsTrue(thinScribnerMetric < volumeHighSideTolerance * tree.MinimumThinVolumeScribner);

                Assert.IsTrue(regenCubicMetric >= volumeLowSideTolerance * tree.MinimumRegenVolumeCubic);
                Assert.IsTrue(regenScribnerMetric >= volumeLowSideTolerance * tree.MinimumRegenVolumeScribner);
                Assert.IsTrue(regenCubicMetric < volumeHighSideTolerance * tree.MinimumRegenVolumeCubic);
                Assert.IsTrue(regenScribnerMetric < volumeHighSideTolerance * tree.MinimumRegenVolumeScribner);

                // ratios must be greater than zero in principle but scaling and CVTS regression error allow crossover
                float poudelTotalCubic = PoudelRegressions.GetCubicVolume(psmeMetric, 0);
                poudelTotalCubic /= tree.ExpansionFactor;
                Assert.IsTrue(regenCubicMetric / poudelTotalCubic > tree.MinimumMerchantableStemVolumeFraction);
                Assert.IsTrue(thinCubicMetric / poudelTotalCubic > tree.MinimumMerchantableStemVolumeFraction);
                Assert.IsTrue(regenCubicMetric / poudelTotalCubic < 1.0F);
                Assert.IsTrue(thinCubicMetric / poudelTotalCubic < 1.0F);

                // log ratios
                this.TestContext.WriteLine("tree: {0:0.0} cm DBH, {1:0.0} m tall", tree.Dbh, tree.Height);
                this.TestContext.WriteLine("thin cubic: {0:0.00}, minimum {1:0.00}, ratio {2:0.000}", thinCubicMetric, tree.MinimumThinVolumeCubic, thinCubicMetric / tree.MinimumThinVolumeCubic);
                this.TestContext.WriteLine("regen cubic: {0:0.00}, minimum {1:0.00}, ratio {2:0.000}", regenCubicMetric, tree.MinimumRegenVolumeCubic, regenCubicMetric / tree.MinimumRegenVolumeCubic);
                this.TestContext.WriteLine("thin Scribner: {0:0.00}, minimum {1:0.00}, ratio {2:0.000}", thinScribnerMetric, tree.MinimumThinVolumeScribner, thinScribnerMetric / tree.MinimumThinVolumeScribner);
                this.TestContext.WriteLine("regen Scribner: {0:0.00}, minimum {1:0.00}, ratio {2:0.000}", regenScribnerMetric, tree.MinimumRegenVolumeScribner, regenScribnerMetric / tree.MinimumRegenVolumeScribner);
            }
        }

        private class ExpectedTreeVolume
        {
            public float Dbh { get; set; }
            public float ExpansionFactor { get; set; }
            public float Height { get; set; }
            public float MinimumMerchantableStemVolumeFraction { get; set; }
            public float MinimumRegenVolumeCubic { get; set; }
            public float MinimumRegenVolumeScribner { get; set; }
            public float MinimumThinVolumeCubic { get; set; }
            public float MinimumThinVolumeScribner { get; set; }
            public FiaCode Species { get; set; }
        }
    }
}
