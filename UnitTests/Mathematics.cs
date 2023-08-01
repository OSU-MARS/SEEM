using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mars.Seem.Extensions;
using Mars.Seem.Heuristics;
using Mars.Seem.Optimization;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Test
{
    [TestClass]
    public class Mathematics
    {
        private static readonly float BarkThicknessTolerance;

        public TestContext? TestContext { get; set; }

        static Mathematics()
        {
            Mathematics.BarkThicknessTolerance = 1.01F;
        }

        [TestMethod]
        public void BarkThickness()
        {
            List<ExpectedTreeBarkThickness> trees = new()
            {
                new()
                {
                    Dbh = 20.0F,
                    Height = 25.0F,
                    HeightToCrownBase = 15.0F,
                    EvaluationHeights = new float[] { Constant.MetersPerFoot, Constant.Bucking.DefaultStumpHeightInM + Constant.Bucking.ProcessingHeadFeedRollerHeightInM, Constant.DbhHeightInM, 2.0F, 3.0F, 5.0F, 7.5F, 10.0F, 15.0F, 20.0F, 25.0F },
                    ExpectedDoubleBarkThickness = new float[] { 3.24F, 2.25F, 1.33F, 1.20F, 1.01F, 0.72F, 0.50F, 0.42F, 0.28F, 0.14F, 0.00F } // diameter inside bark at DBH from Poudel et al. 2018
                    // ExpectedDoubleBarkThickness = new float[] { 3.24F, 2.76F, 2.32F, 2.09F, 1.76F, 1.25F, 0.87F, 0.73F, 0.50F, 0.25F, 0.00F } // diameter inside bark at DBH from Larson and Hann 1985 (via Maguire and Hann 1990)
                },
                new()
                {
                    Dbh = 80.0F,
                    Height = 42.0F,
                    HeightToCrownBase = 21.0F,
                    EvaluationHeights = new float[] { Constant.MetersPerFoot, Constant.Bucking.DefaultStumpHeightInM + Constant.Bucking.ProcessingHeadFeedRollerHeightInM, Constant.DbhHeightInM, 2.0F, 3.0F, 5.0F, 7.5F, 10.0F, 20.0F, 30.0F, 40.0F },
                    ExpectedDoubleBarkThickness = new float[] { 12.82F, 9.75F, 6.91F, 6.51F, 5.91F, 4.84F, 3.78F, 3.00F, 1.89F, 1.04F, 0.17F } // diameter inside bark at DBH from Poudel et al. 2018
                    // ExpectedDoubleBarkThickness = new float[] { 12.82F, 11.52F, 10.31F, 9.71F, 8.81F, 7.22F, 5.63F, 4.48F, 2.82F, 1.56F, 0.26F } // diameter inside bark at DBH from Larson and Hann 1985 (via Maguire and Hann 1990)
                }
            };

            foreach (ExpectedTreeBarkThickness tree in trees)
            {
                for (int evaluationHeightIndex = 0; evaluationHeightIndex < tree.EvaluationHeights.Length; ++evaluationHeightIndex)
                {
                    float evaluationHeightInM = tree.EvaluationHeights[evaluationHeightIndex];
                    float expectedDoubleBarkThicknessInCm = tree.ExpectedDoubleBarkThickness[evaluationHeightIndex];

                    float doubleBarkThicknessInCm = DouglasFir.GetDoubleBarkThickness(tree.Dbh, tree.Height, tree.HeightToCrownBase, evaluationHeightInM);
                    Assert.IsTrue(doubleBarkThicknessInCm >= expectedDoubleBarkThicknessInCm);
                    Assert.IsTrue(expectedDoubleBarkThicknessInCm <= Mathematics.BarkThicknessTolerance * expectedDoubleBarkThicknessInCm);
                }
            }
        }

        [TestMethod]
        public void Exp()
        {
            const float accuracyExp = 1E-7F;
            const float precisionExp = 3.6E-7F; // numerical error, including truncation at large negative powers
            const float accuracyExp2 = 3.2E-7F; // higher errors possible for 4th order polynomial
            const float precisionExp2 = 4.5E-6F; // numerical error, including truncation

            const float exp10maxPower = 38.5318F; // upper bound of 10^power from log10(Single.MaxValue = 3.4e38

            //                              0        1        2        3        4        5        6        7        8        9        10    
            float[] powers = new float[] { -152.3F, -127.1F, -88.05F, -87.99F, -36.83F, -10.45F, -6.694F, -5.883F, -5.223F, -4.776F, -4.234F,
                                           -3.766F, -2.294F, -1.605F, -1.330F, -1.012F, -0.567F, -0.500F, -0.333F, -0.298F, -0.116F,  0.0F,    
                                            0.176F,  0.333F,  0.474F,  0.500F,  0.753F,  1.318F,  1.333F,  1.605F,  1.856F,  2.0F,    2.566F,
                                            2.789F,  3.888F,  4.000F,  4.779F,  5.128F, 10.0F,   12.6F,   28.3F,   48.9F,   69.6F,   87.3F };
            for (int index = 0; index < powers.Length; ++index)
            {
                float power = powers[index];
                float exp = MathV.Exp(power);
                float exp2 = MathV.Exp2(power);

                double expectedExp = Math.Exp(power);
                double expError = exp > 1E-7 * precisionExp ? 1.0 - exp / expectedExp : expectedExp;
                double toleranceExp = accuracyExp * Math.Abs(expectedExp) + precisionExp;

                double expectedExp2 = Math.Pow(2.0, power);
                double exp2Error = exp > 1E-7 * precisionExp2 ? 1.0 - exp2 / expectedExp2 : expectedExp2;
                double toleranceExp2 = accuracyExp2 * Math.Abs(expectedExp2) + precisionExp2;

                Assert.IsTrue((exp >= 0.0F) && Math.Abs(expError) < toleranceExp);
                Assert.IsTrue((exp2 >= 0.0F) && Math.Abs(exp2Error) < toleranceExp2);

                if (power <= exp10maxPower)
                {
                    float exp10 = MathV.Exp10(power);
                    double expectedExp10 = Math.Pow(10.0, power);

                    double exp10Error = exp > 1E-7 * precisionExp2 ? 1.0 - exp10 / expectedExp10 : expectedExp10;
                    double toleranceExp10 = accuracyExp2 * Math.Abs(expectedExp10) + precisionExp2;
                    Assert.IsTrue((exp10 >= 0.0F) && Math.Abs(exp10Error) < toleranceExp10);
                }
            }

            for (int quadIndex = 0; quadIndex < powers.Length; quadIndex += 4)
            {
                float power0 = powers[quadIndex];
                float power1 = powers[quadIndex + 1];
                float power2 = powers[quadIndex + 2];
                float power3 = powers[quadIndex + 3];
                bool exp10inRange = (power0 <= exp10maxPower) && (power1 <= exp10maxPower) && (power2 <= exp10maxPower) && (power3 <= exp10maxPower);

                Vector128<float> value = Vector128.Create(power0, power1, power2, power3);
                Vector128<float> exp_m128 = MathV.Exp(value);
                Vector128<float> exp2_m128 = MathV.Exp2(value);
                Vector128<float> exp10_m128 = exp10inRange ? MathV.Exp10(value) : Vector128<float>.Zero;

                for (int scalarIndex = 0; scalarIndex < 4; ++scalarIndex)
                {
                    float power = value.GetElement(scalarIndex);
                    float exp = exp_m128.GetElement(scalarIndex);
                    float exp2 = exp2_m128.GetElement(scalarIndex);
                    float exp10 = exp10_m128.GetElement(scalarIndex);

                    double expectedExp = Math.Exp(power);
                    double expError = exp > 1E-7 * precisionExp ? 1.0 - exp / expectedExp : expectedExp;
                    double toleranceExp = accuracyExp * Math.Abs(expectedExp) + precisionExp;

                    double expectedExp2 = Math.Pow(2.0, power);
                    double exp2Error = exp > 1E-7 * precisionExp2 ? 1.0 - exp2 / expectedExp2 : expectedExp2;
                    double toleranceExp2 = accuracyExp2 * Math.Abs(expectedExp2) + precisionExp2;

                    Assert.IsTrue((exp >= 0.0F) && Math.Abs(expError) < toleranceExp);
                    Assert.IsTrue((exp2 >= 0.0F) && Math.Abs(exp2Error) < toleranceExp2);

                    if (exp10inRange)
                    {
                        double expectedExp10 = Math.Pow(10.0, power);
                        double exp10Error = exp > 1E-7 * precisionExp2 ? 1.0 - exp10 / expectedExp10 : expectedExp10;
                        double toleranceExp10 = accuracyExp2 * Math.Abs(expectedExp10) + precisionExp2;
                        Assert.IsTrue((exp10 >= 0.0F) && Math.Abs(exp10Error) < toleranceExp10);
                    }
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
            binaryPopulation.IndividualTreeSelections[0] = new IndividualTreeSelectionBySpecies { { FiaCode.AbiesAmabalis, new(new int[] { 0, 0, 0, 0, 0 }) } };
            binaryPopulation.IndividualTreeSelections[1] = new IndividualTreeSelectionBySpecies { { FiaCode.AbiesAmabalis, new(new int[] { 1, 1, 1, 1, 1 }) } };
            binaryPopulation.SetDistancesForNewIndividual(0);
            binaryPopulation.SetDistancesForNewIndividual(1);
            binaryPopulation.InsertFitness(0, 0.0F);
            binaryPopulation.InsertFitness(1, 1.0F);

            Population clones = new(2, 0.5F, 5);
            clones.IndividualTreeSelections[0] = new IndividualTreeSelectionBySpecies { { FiaCode.AbiesAmabalis, new(new int[] { 0, 0, 0, 0, 0 }) } };
            clones.IndividualTreeSelections[1] = new IndividualTreeSelectionBySpecies { { FiaCode.AbiesAmabalis, new(new int[] { 0, 0, 0, 0, 0 }) } };
            clones.SetDistancesForNewIndividual(0);
            clones.SetDistancesForNewIndividual(1);
            clones.InsertFitness(0, 0.0F);
            clones.InsertFitness(1, 0.0F);

            Population heterozygousPopulation = new(2, 0.5F, 5);
            heterozygousPopulation.IndividualTreeSelections[0] = new IndividualTreeSelectionBySpecies { { FiaCode.AbiesAmabalis, new(new int[] { 1, 0, 0, 1, 0 }) } };
            heterozygousPopulation.IndividualTreeSelections[1] = new IndividualTreeSelectionBySpecies { { FiaCode.AbiesAmabalis, new(new int[] { 1, 0, 1, 0, 1 }) } };
            heterozygousPopulation.SetDistancesForNewIndividual(0);
            heterozygousPopulation.SetDistancesForNewIndividual(1);
            heterozygousPopulation.InsertFitness(0, 0.4F);
            heterozygousPopulation.InsertFitness(1, 0.6F);

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
            Assert.IsTrue(statistics.NewIndividualsByGeneration[0] == 2);
            Assert.IsTrue(statistics.PolymorphismByGeneration[0] == 1.0F);

            Assert.IsTrue(statistics.CoefficientOfVarianceByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.MaximumFitnessByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.MeanFitnessByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.MinimumFitnessByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.MeanHeterozygosityByGeneration[1] == 0.0F);
            Assert.IsTrue(statistics.NewIndividualsByGeneration[1] == 2);
            Assert.IsTrue(statistics.PolymorphismByGeneration[1] == 0.0F);

            Assert.IsTrue(MathF.Round(statistics.CoefficientOfVarianceByGeneration[2], 6) == 0.2F);
            Assert.IsTrue(statistics.MaximumFitnessByGeneration[2] == 0.6F);
            Assert.IsTrue(statistics.MeanFitnessByGeneration[2] == 0.5F);
            Assert.IsTrue(statistics.MinimumFitnessByGeneration[2] == 0.4F);
            Assert.IsTrue(statistics.MeanHeterozygosityByGeneration[2] == 0.3F);
            Assert.IsTrue(statistics.NewIndividualsByGeneration[2] == 2);
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
            float reforestationNpv = FinancialScenarios.Default.GetNetPresentReforestationValue(Constant.HeuristicDefault.CoordinateIndex, Constant.AcresPerHectare * 380.0F);
            Assert.IsTrue(reforestationNpv > -444.72F);
            Assert.IsTrue(reforestationNpv < -0.999 * 444.72F);
        }

        [TestMethod]
        public void Simd()
        {
            // 128 bit
            Vector128<float> broadcastFloat128 = AvxExtensions.BroadcastScalarToVector128(Constant.ForestersEnglish);
            Vector128<int> broadcastInt128 = AvxExtensions.Set128(1);

            AssertV.IsTrue(Avx.CompareEqual(broadcastFloat128, Vector128.Create(Constant.ForestersEnglish)));
            AssertV.IsTrue(Avx.CompareEqual(broadcastInt128, Vector128.Create(1)));

            Vector128<int> index128 = Vector128.Create(0, 1, 2, 3);
            Vector128<int> shuffle128_1 = Avx.Shuffle(index128, Constant.Simd128x4.ShuffleRotateLower1);
            Vector128<int> shuffle128_2 = Avx.Shuffle(index128, Constant.Simd128x4.ShuffleRotateLower2);
            Vector128<int> shuffle128_3 = Avx.Shuffle(index128, Constant.Simd128x4.ShuffleRotateLower3);

            AssertV.IsTrue(Avx.CompareEqual(shuffle128_1, Vector128.Create(1, 2, 3, 0)));
            AssertV.IsTrue(Avx.CompareEqual(shuffle128_2, Vector128.Create(2, 3, 0, 1)));
            AssertV.IsTrue(Avx.CompareEqual(shuffle128_3, Vector128.Create(3, 0, 1, 2)));

            Assert.IsTrue(shuffle128_1.ToScalar() == 1);
            Assert.IsTrue(shuffle128_2.ToScalar() == 2);
            Assert.IsTrue(shuffle128_3.ToScalar() == 3);

            if (Avx2.IsSupported)
            {
                // 256 bit
                Vector256<float> broadcastFloat256 = AvxExtensions.BroadcastScalarToVector256(Constant.ForestersEnglish);
                Vector256<int> broadcastInt256 = AvxExtensions.Set256(1);

                AssertV.IsTrue(Avx.CompareEqual(broadcastFloat256, Vector256.Create(Constant.ForestersEnglish)));
                AssertV.IsTrue(Avx2.CompareEqual(broadcastInt256, Vector256.Create(1)));

                Vector256<int> index256 = Vector256.Create(0, 1, 2, 3, 0, 1, 2, 3);
                Vector256<int> shuffle256_1 = Avx2.Shuffle(index256, Constant.Simd128x4.ShuffleRotateLower1);
                Vector256<int> shuffle256_2 = Avx2.Shuffle(index256, Constant.Simd128x4.ShuffleRotateLower2);
                Vector256<int> shuffle256_3 = Avx2.Shuffle(index256, Constant.Simd128x4.ShuffleRotateLower3);

                AssertV.IsTrue(Avx2.CompareEqual(shuffle256_1, Vector256.Create(1, 2, 3, 0, 1, 2, 3, 0)));
                AssertV.IsTrue(Avx2.CompareEqual(shuffle256_2, Vector256.Create(2, 3, 0, 1, 2, 3, 0, 1)));
                AssertV.IsTrue(Avx2.CompareEqual(shuffle256_3, Vector256.Create(3, 0, 1, 2, 3, 0, 1, 2)));

                Assert.IsTrue(shuffle256_1.ToScalar() == 1);
                Assert.IsTrue(shuffle256_2.ToScalar() == 2);
                Assert.IsTrue(shuffle256_3.ToScalar() == 3);
            }
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
                    DbhInCm = 19.4F,
                    HeightInM = 21.2F,
                    ExpansionFactorPerHa = 1.0F,
                    MinimumMerchantableStemVolumeFraction = 0.59F,
                    MinimumRegenVolumeCubic = 0.191F,
                    MinimumRegenVolumeMbf = 0.029999F,
                    MinimumThinVolumeCubic = 0.177F,
                    MinimumThinVolumeMbf = 0.029999F
                },
                new ExpectedTreeVolume()
                {
                    Species = FiaCode.PseudotsugaMenziesii,
                    DbhInCm = 30.01F,
                    HeightInM = 30.01F,
                    ExpansionFactorPerHa = 0.36F,
                    MinimumMerchantableStemVolumeFraction = 0.87F,
                    MinimumRegenVolumeCubic = 0.820F,
                    MinimumRegenVolumeMbf = 0.119999F,
                    MinimumThinVolumeCubic = 0.807F,
                    MinimumThinVolumeMbf = 0.119999F
                },
                new ExpectedTreeVolume()
                {
                    Species = FiaCode.PseudotsugaMenziesii,
                    DbhInCm = 46.2F,
                    HeightInM = 41.8F,
                    ExpansionFactorPerHa = 4.34F,
                    MinimumMerchantableStemVolumeFraction = 0.92F,
                    MinimumRegenVolumeCubic = 2.63F,
                    MinimumRegenVolumeMbf = 0.419999F,
                    MinimumThinVolumeCubic = 2.55F,
                    MinimumThinVolumeMbf = 0.4527F
                }
            };

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

            TreeVolume treeVolume = TreeVolume.Default;
            foreach (ExpectedTreeVolume tree in trees)
            {
                float dbhInCentimeters = tree.DbhInCm;
                float heightInMeters = tree.HeightInM;
                Trees psmeEnglish = new(FiaCode.PseudotsugaMenziesii, 1, Units.English);
                psmeEnglish.Add(1, 1, Constant.InchesPerCentimeter * dbhInCentimeters, Constant.FeetPerMeter * heightInMeters, 0.5F, Constant.HectaresPerAcre * tree.ExpansionFactorPerHa);
                Trees psmeMetric = new(FiaCode.PseudotsugaMenziesii, 1, Units.Metric);
                psmeMetric.Add(1, 1, dbhInCentimeters, heightInMeters, 0.5F, tree.ExpansionFactorPerHa);

                Assert.IsTrue(treeVolume.TryGetLongLogVolumeTable(FiaCode.PseudotsugaMenziesii, out TreeSpeciesMerchantableVolumeTable? psmeLongLogVolumeTable));
                Assert.IsTrue(psmeLongLogVolumeTable != null);
                TreeSpeciesMerchantableVolumeForPeriod standingEnglish = psmeLongLogVolumeTable.GetStandingVolume(psmeEnglish);
                float standingCubicEnglish = standingEnglish.Cubic2Saw + standingEnglish.Cubic3Saw + standingEnglish.Cubic4Saw;
                float standingScribnerEnglish = standingEnglish.Scribner2Saw + standingEnglish.Scribner3Saw + standingEnglish.Scribner4Saw;

                TreeSpeciesMerchantableVolumeForPeriod standingMetric = psmeLongLogVolumeTable.GetStandingVolume(psmeMetric);
                float standingCubicMetric = standingMetric.Cubic2Saw + standingMetric.Cubic3Saw + standingMetric.Cubic4Saw;
                float standingScribnerMetric = standingMetric.Scribner2Saw + standingMetric.Scribner3Saw + standingMetric.Scribner4Saw;

                Assert.IsTrue(treeVolume.TryGetForwarderVolumeTable(FiaCode.PseudotsugaMenziesii, out TreeSpeciesMerchantableVolumeTable? psmeForwarderVolumeTable));
                Assert.IsTrue(psmeForwarderVolumeTable != null);
                TreeSpeciesMerchantableVolumeForPeriod thinEnglish = psmeForwarderVolumeTable.GetStandingVolume(psmeEnglish);
                float thinCubicEnglish = thinEnglish.Cubic2Saw + thinEnglish.Cubic3Saw + thinEnglish.Cubic4Saw;
                float thinScribnerEnglish = thinEnglish.Scribner2Saw + thinEnglish.Scribner3Saw + thinEnglish.Scribner4Saw;

                TreeSpeciesMerchantableVolumeForPeriod thinMetric = psmeForwarderVolumeTable.GetStandingVolume(psmeMetric);
                float thinCubicMetric = thinMetric.Cubic2Saw + thinMetric.Cubic3Saw + thinMetric.Cubic4Saw;
                float thinScribnerMetric = thinMetric.Scribner2Saw + thinMetric.Scribner3Saw + thinMetric.Scribner4Saw;

                Assert.IsTrue(Math.Abs(standingCubicEnglish - standingCubicMetric) < 0.000003F * standingCubicMetric);
                Assert.IsTrue(Math.Abs(standingScribnerEnglish - standingScribnerMetric) < 0.000004F * standingScribnerMetric);
                Assert.IsTrue(Math.Abs(thinCubicEnglish - thinCubicMetric) < 0.000004F * thinCubicMetric);
                Assert.IsTrue(Math.Abs(thinScribnerEnglish - thinScribnerMetric) < 0.000004F * thinScribnerMetric);

                standingCubicMetric /= tree.ExpansionFactorPerHa;
                standingScribnerMetric /= tree.ExpansionFactorPerHa;
                thinCubicMetric /= tree.ExpansionFactorPerHa;
                thinScribnerMetric /= tree.ExpansionFactorPerHa;

                Assert.IsTrue(thinCubicMetric >= volumeLowSideTolerance * tree.MinimumThinVolumeCubic);
                Assert.IsTrue(thinScribnerMetric >= volumeLowSideTolerance * tree.MinimumThinVolumeMbf);
                Assert.IsTrue(thinCubicMetric < volumeHighSideTolerance * tree.MinimumThinVolumeCubic);
                Assert.IsTrue(thinScribnerMetric < volumeHighSideTolerance * tree.MinimumThinVolumeMbf);

                Assert.IsTrue(standingCubicMetric >= volumeLowSideTolerance * tree.MinimumRegenVolumeCubic);
                Assert.IsTrue(standingScribnerMetric >= volumeLowSideTolerance * tree.MinimumRegenVolumeMbf);
                Assert.IsTrue(standingCubicMetric < volumeHighSideTolerance * tree.MinimumRegenVolumeCubic);
                Assert.IsTrue(standingScribnerMetric < volumeHighSideTolerance * tree.MinimumRegenVolumeMbf);

                // ratios must be greater than zero in principle but scaling and CVTS regression error allow crossover
                float poudelTotalCubic = PoudelRegressions.GetCubicVolume(psmeMetric, 0);
                poudelTotalCubic /= tree.ExpansionFactorPerHa;
                Assert.IsTrue(standingCubicMetric / poudelTotalCubic > tree.MinimumMerchantableStemVolumeFraction);
                Assert.IsTrue(thinCubicMetric / poudelTotalCubic > tree.MinimumMerchantableStemVolumeFraction);
                Assert.IsTrue(standingCubicMetric / poudelTotalCubic < 1.0F);
                Assert.IsTrue(thinCubicMetric / poudelTotalCubic < 1.0F);

                // log ratios
                this.TestContext!.WriteLine("tree: {0:0.0} cm DBH, {1:0.0} m tall", tree.DbhInCm, tree.HeightInM);
                this.TestContext.WriteLine("thin cubic: {0:0.00}, minimum {1:0.00}, ratio {2:0.000}", thinCubicMetric, tree.MinimumThinVolumeCubic, thinCubicMetric / tree.MinimumThinVolumeCubic);
                this.TestContext.WriteLine("regen cubic: {0:0.00}, minimum {1:0.00}, ratio {2:0.000}", standingCubicMetric, tree.MinimumRegenVolumeCubic, standingCubicMetric / tree.MinimumRegenVolumeCubic);
                this.TestContext.WriteLine("thin Scribner: {0:0.00}, minimum {1:0.00}, ratio {2:0.000}", thinScribnerMetric, tree.MinimumThinVolumeMbf, thinScribnerMetric / tree.MinimumThinVolumeMbf);
                this.TestContext.WriteLine("regen Scribner: {0:0.00}, minimum {1:0.00}, ratio {2:0.000}", standingScribnerMetric, tree.MinimumRegenVolumeMbf, standingScribnerMetric / tree.MinimumRegenVolumeMbf);
            }
        }

        private class ExpectedTreeBarkThickness
        {
            public float Dbh { get; init; }
            public float Height { get; init; }
            public float HeightToCrownBase { get; init; }
            public float[] EvaluationHeights { get; init; }
            public float[] ExpectedDoubleBarkThickness { get; init; }

            public ExpectedTreeBarkThickness()
            {
                this.EvaluationHeights = Array.Empty<float>();
                this.ExpectedDoubleBarkThickness = Array.Empty<float>();
            }
        }

        private class ExpectedTreeVolume
        {
            public float DbhInCm { get; init; }
            public float ExpansionFactorPerHa { get; init; }
            public float HeightInM { get; init; }
            public float MinimumMerchantableStemVolumeFraction { get; init; }
            public float MinimumRegenVolumeCubic { get; init; }
            public float MinimumRegenVolumeMbf { get; init; }
            public float MinimumThinVolumeCubic { get; init; }
            public float MinimumThinVolumeMbf { get; init; }
            public FiaCode Species { get; init; }
        }
    }
}
