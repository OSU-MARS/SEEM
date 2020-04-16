using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Data;
using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Osu.Cof.Ferm.Test
{
    [TestClass]
    public class PublicApi : OrganonTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void HuffmanPeakNobleFir()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "HPNF.xlsx");
            PspStand huffmanPeak = new PspStand(plotFilePath, "HPNF", 0.2F);
            OrganonVariant variant = new OrganonVariantSwo(); // SWO allows mapping ABAM -> ABGR and ABPR -> ABCO
            TestStand stand = huffmanPeak.ToStand(variant, 80.0F);
            int startYear = 1980;
            stand.WriteCompetitionAsCsv("HPNF initial competition.csv", variant, startYear);
            this.GrowPspStand(huffmanPeak, stand, variant, startYear, 2015, Path.GetFileNameWithoutExtension(plotFilePath));

            TreeQuantiles measuredQuantiles = new TreeQuantiles(stand, huffmanPeak, startYear);
            using StreamWriter quantileWriter = measuredQuantiles.WriteToCsv("HPNF measured quantiles.csv", variant, startYear);
            foreach (int measurementYear in huffmanPeak.MeasurementYears)
            {
                if (measurementYear != startYear)
                {
                    measuredQuantiles = new TreeQuantiles(stand, huffmanPeak, measurementYear);
                    measuredQuantiles.WriteToCsv(quantileWriter, variant, measurementYear);
                }
            }
        }

        [TestMethod]
        public void Nelder()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Nelder20.xlsx");
            NelderPlot nelder = new NelderPlot(plotFilePath, "1");
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToStand(130.0F, 10);

            Objective netPresentValue = new Objective()
            {
                IsNetPresentValue = true,
                VolumeUnits = VolumeUnits.ScribnerBoardFeetPerAcre
            };
            Objective volume = new Objective();

            int harvestPeriods = 4;
            int planningPeriods = 9;
            GeneticAlgorithm genetic = new GeneticAlgorithm(stand, configuration, harvestPeriods, planningPeriods, netPresentValue)
            {
                EndStandardDeviation = 0.001F, // US$ 1 NPV
                PopulationSize = 6,
                MaximumGenerations = 8,
            };
            genetic.Run();

            GreatDeluge deluge = new GreatDeluge(stand, configuration, harvestPeriods, planningPeriods, volume)
            {
                RainRate = 5,
                StopAfter = 10
            };
            deluge.RandomizeSchedule();
            deluge.Run();

            RecordToRecordTravel recordTravel = new RecordToRecordTravel(stand, configuration, harvestPeriods, planningPeriods, netPresentValue)
            {
                StopAfter = 10
            };
            recordTravel.RandomizeSchedule();
            recordTravel.Run();

            SimulatedAnnealing annealer = new SimulatedAnnealing(stand, configuration, harvestPeriods, planningPeriods, volume)
            {
                Alpha = 0.9F
            };
            annealer.RandomizeSchedule();
            annealer.Run();

            TabuSearch tabu = new TabuSearch(stand, configuration, harvestPeriods, planningPeriods, netPresentValue)
            {
                Iterations = 5
            };
            tabu.RandomizeSchedule();
            tabu.Run();

            ThresholdAccepting thresholdAcceptor = new ThresholdAccepting(stand, configuration, harvestPeriods, planningPeriods, volume)
            {
                IterationsPerThreshold = 10
            };
            thresholdAcceptor.RandomizeSchedule();
            thresholdAcceptor.Run();

            Hero hero = new Hero(stand, configuration, harvestPeriods, planningPeriods, netPresentValue)
            {
                Iterations = 10
            };
            hero.RandomizeSchedule();
            hero.Run();

            // heuristics assigned to volume optimization
            this.VerifyObjectiveFunction(deluge);
            this.VerifyObjectiveFunction(annealer);
            this.VerifyObjectiveFunction(thresholdAcceptor);

            // heuristics assigned to net present value optimization
            this.VerifyObjectiveFunction(genetic);
            this.VerifyObjectiveFunction(recordTravel);
            this.VerifyObjectiveFunction(tabu);
            this.VerifyObjectiveFunction(hero);
        }

        [TestMethod]
        public void OrganonStandGrowthApi()
        {
            TestStand.WriteTreeHeader(this.TestContext);
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                // get crown closure
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                OrganonTreatments treatments = new OrganonTreatments();
                float crownCompetitionFactor = OrganonStandDensity.GetCrownCompetition(variant, stand);
                Assert.IsTrue(crownCompetitionFactor >= 0.0F);
                Assert.IsTrue(crownCompetitionFactor <= TestConstant.Maximum.CrownCompetitionFactor);
                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // recalculate heights and crown ratios for all trees
                Dictionary<FiaCode, float[]> CALIB = configuration.CreateSpeciesCalibration();
                OrganonGrowth.SetIngrowthHeightAndCrownRatio(variant, stand, stand.TreeRecordCount, CALIB);
                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // run Organon growth simulation
                stand = this.CreateDefaultStand(configuration);
                TestStand initialTreeData = stand.Clone();
                TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath(stand.TreeRecordCount);

                if (configuration.IsEvenAge)
                {
                    // stand error if less than one year to grow to breast height
                    stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
                }

                stand.WriteTreesAsCsv(this.TestContext, variant, 0, false);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonGrowth.Grow(simulationStep, configuration, stand, CALIB, treatments);
                    treeGrowth.AccumulateGrowthAndMortality(stand);
                    stand.SetSdiMax(configuration);
                    this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, OrganonWarnings.LessThan50TreeRecords, stand, variant);

                    stand.WriteTreesAsCsv(this.TestContext, variant, variant.GetEndYear(simulationStep), false);
                }

                this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, treeGrowth, initialTreeData, stand);
                this.Verify(CALIB);
            }
        }

        [TestMethod]
        public void RS39()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "RS39 lower half.xlsx");
            PspStand rs39 = new PspStand(plotFilePath, "RS39 lower half", 0.154441F);
            OrganonVariant variant = new OrganonVariantNwo();
            TestStand stand = rs39.ToStand(variant, 105.0F);
            int startYear = 1992;
            stand.WriteCompetitionAsCsv("RS39 lower half initial competition.csv", variant, startYear);
            this.GrowPspStand(rs39, stand, variant, startYear, 2019, Path.GetFileNameWithoutExtension(plotFilePath));

            TreeQuantiles measuredQuantiles = new TreeQuantiles(stand, rs39, startYear);
            using StreamWriter quantileWriter = measuredQuantiles.WriteToCsv("RS39 lower half measured quantiles.csv", variant, startYear);
            foreach (int measurementYear in rs39.MeasurementYears)
            {
                if (measurementYear != startYear)
                {
                    measuredQuantiles = new TreeQuantiles(stand, rs39, measurementYear);
                    measuredQuantiles.WriteToCsv(quantileWriter, variant, measurementYear);
                }
            }
        }

        // basic growth model performance benchmark: 3 runs of hero with 300 trees
        [TestMethod]
        public void StandGrowthPerformance()
        {
            int runs = 4; // 1 warmup run + measured runs
            int trees = 300;
            #if DEBUG
            runs = 1; // do only functional validation of test on DEBUG builds to reduce test execution time
            trees = 10;
            #endif

            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Nelder20.xlsx");
            NelderPlot nelder = new NelderPlot(plotFilePath, "1");
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToStand(130.0F, trees);

            Objective netPresentValue = new Objective()
            {
                IsNetPresentValue = true,
                VolumeUnits = VolumeUnits.ScribnerBoardFeetPerAcre
            };

            int harvestPeriods = 4;
            int planningPeriods = 9;

            TimeSpan runtime = TimeSpan.Zero;
            for (int run = 0; run < runs; ++run)
            {
                // 3 runs * 300 trees = 900 growth simulations on i7-3770 (4th gen, Sandy Bridge)
                // .NET standard 2.0 + Math: 12.978s -> 69.3 sims/core-s       97.8% StandTrajectory.Simulate(): 89.5% Organon, 7.6% FIA Scribner volume
                // .NET standard 2.0 + MathF: 9.670s -> 93.1 sims/core-s  +34% 
                Hero hero = new Hero(stand, configuration, harvestPeriods, planningPeriods, netPresentValue)
                {
                    Iterations = 2
                };
                hero.RandomizeSchedule();
                if (run > 0)
                {
                    // skip first run as a warmup run
                    runtime += hero.Run();
                }
            }
            this.TestContext.WriteLine(runtime.TotalSeconds.ToString());
        }

        private void VerifyObjectiveFunction(Heuristic heuristic)
        {
            double beginObjectiveFunction = heuristic.ObjectiveFunctionByIteration.First();
            double endObjectiveFunction = heuristic.ObjectiveFunctionByIteration.Last();
            double recalculatedBestObjectiveFunction = heuristic.GetObjectiveFunction(heuristic.BestTrajectory);
            double recalculatedEndObjectiveFunction = heuristic.GetObjectiveFunction(heuristic.CurrentTrajectory);

            double bestObjectiveFunctionRatio = heuristic.BestObjectiveFunction / recalculatedBestObjectiveFunction;
            double endObjectiveFunctionRatio = endObjectiveFunction / recalculatedEndObjectiveFunction;

            Assert.IsTrue(heuristic.BestObjectiveFunction >= beginObjectiveFunction);
            Assert.IsTrue(heuristic.BestObjectiveFunction >= endObjectiveFunction);
            Assert.IsTrue(heuristic.ObjectiveFunctionByIteration.Count >= 3);

            Assert.IsTrue(bestObjectiveFunctionRatio > 0.99999);
            Assert.IsTrue(bestObjectiveFunctionRatio < 1.00001);
            // currently, this check can fail for genetic algorithms when the best individual doesn't breed and disimprovement results
            // TODO: either change GA to guarantee best individual breeds or relax this check in the GA case
            Assert.IsTrue(endObjectiveFunctionRatio > 0.99999);
            Assert.IsTrue(endObjectiveFunctionRatio < 1.00001);
        }

        [TestMethod]
        public void Volume()
        {
            FiaVolume fiaVolume = new FiaVolume();
            OsuVolume osuVolume = new OsuVolume();
            Trees trees = new Trees(40);
            float[] fiaMerchantableCubicFeetPerAcre = new float[trees.TreeRecordCount];
            float[] fiaScribnerBoardFeetPerAcre = new float[trees.TreeRecordCount];
            float[] osuMerchantableCubicMetersPerHectare = new float[trees.TreeRecordCount];
            bool isDouglasFir = true;
            float merchantableCubicFeetPerAcre = 0.0F;
            float merchantableCubicMetersPerHectare = 0.0F;
            float totalCylinderCubicMeterVolumePerAcre = 0.0F;
            float totalScribnerBoardFeetPerAcre = 0.0F;
            for (int treeIndex = 0; treeIndex < trees.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = isDouglasFir ? FiaCode.PseudotsugaMenziesii : FiaCode.TsugaHeterophylla;
                TreeRecord tree = new TreeRecord(species, (float)treeIndex, 1.0F, 1.0F - 3.0F * (float)trees.TreeRecordCount / (float)treeIndex);
                trees.Species[treeIndex] = tree.Species;
                trees.Dbh[treeIndex] = tree.DbhInInches;
                trees.Height[treeIndex] = tree.HeightInFeet;
                trees.CrownRatio[treeIndex] = tree.CrownRatio;
                trees.LiveExpansionFactor[treeIndex] = tree.ExpansionFactor;
                float dbhInMeters = TestConstant.MetersPerInch * tree.DbhInInches;
                float heightInMeters = Constant.MetersPerFoot * tree.HeightInFeet;
                float treeSizedCylinderCubicMeterVolumePerAcre = tree.ExpansionFactor * 0.25F * MathF.PI * dbhInMeters * dbhInMeters * heightInMeters;

                fiaMerchantableCubicFeetPerAcre[treeIndex] = tree.ExpansionFactor * fiaVolume.GetMerchantableCubicFeet(trees, treeIndex);
                merchantableCubicFeetPerAcre += fiaMerchantableCubicFeetPerAcre[treeIndex];
                fiaScribnerBoardFeetPerAcre[treeIndex] = tree.ExpansionFactor * fiaVolume.GetScribnerBoardFeet(trees, treeIndex);
                totalScribnerBoardFeetPerAcre += fiaScribnerBoardFeetPerAcre[treeIndex];

                osuMerchantableCubicMetersPerHectare[treeIndex] = Constant.HectaresPerAcre * tree.ExpansionFactor * osuVolume.GetCubicVolume(trees, treeIndex);
                merchantableCubicMetersPerHectare += osuMerchantableCubicMetersPerHectare[treeIndex];

                // taper coefficient should be in the vicinity of 0.3 for larger trees, but this is not well defined for small trees
                // Lower bound can be made more stringent if necessary.
                Debug.Assert(fiaMerchantableCubicFeetPerAcre[treeIndex] >= 0.0);
                Debug.Assert(fiaMerchantableCubicFeetPerAcre[treeIndex] <= 0.4 * Constant.CubicFeetPerCubicMeter * treeSizedCylinderCubicMeterVolumePerAcre);

                Debug.Assert(fiaScribnerBoardFeetPerAcre[treeIndex] >= 0.0);
                Debug.Assert(fiaScribnerBoardFeetPerAcre[treeIndex] <= 6.5 * 0.4 * Constant.CubicFeetPerCubicMeter * treeSizedCylinderCubicMeterVolumePerAcre);
                totalCylinderCubicMeterVolumePerAcre += treeSizedCylinderCubicMeterVolumePerAcre;
            }

            float totalCylinderCubicFeetVolumePerAcre = Constant.CubicFeetPerCubicMeter * totalCylinderCubicMeterVolumePerAcre;
            Debug.Assert(merchantableCubicFeetPerAcre >= 0.05 * totalCylinderCubicFeetVolumePerAcre);
            Debug.Assert(merchantableCubicFeetPerAcre <= 0.35 * totalCylinderCubicFeetVolumePerAcre);
            Debug.Assert(merchantableCubicFeetPerAcre >= 0.5 * Constant.AcresPerHectare * Constant.CubicFeetPerCubicMeter * merchantableCubicMetersPerHectare);

            Debug.Assert(merchantableCubicMetersPerHectare <= 0.35 * Constant.AcresPerHectare * totalCylinderCubicMeterVolumePerAcre);

            Debug.Assert(totalScribnerBoardFeetPerAcre >= 1.75 * 0.35 * totalCylinderCubicFeetVolumePerAcre);
            Debug.Assert(totalScribnerBoardFeetPerAcre <= 6.5 * 0.40 * totalCylinderCubicFeetVolumePerAcre);

            // check SIMD 128 result against scalar
            float totalScribnerBoardFeetPerAcre128 = fiaVolume.GetScribnerBoardFeet(trees);
            float simdScalarScribnerDifference = totalScribnerBoardFeetPerAcre - totalScribnerBoardFeetPerAcre128;
            Debug.Assert(MathF.Abs(simdScalarScribnerDifference) < 1E-7 * totalScribnerBoardFeetPerAcre);
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

            Trees trees = new Trees(treeCount);
            for (int treeIndex = 0; treeIndex < trees.TreeRecordCount; ++treeIndex)
            {
                trees.Species[treeIndex] = FiaCode.PseudotsugaMenziesii;
                trees.CrownRatio[treeIndex] = (float)treeIndex % 100;
                trees.Dbh[treeIndex] = (float)(treeIndex % 36 + 4);
                trees.Height[treeIndex] = 16.0F * MathF.Sqrt(trees.Dbh[treeIndex]) + 4.5F;
                trees.LiveExpansionFactor[treeIndex] = 1.0F;
            }
            FiaVolume volume = new FiaVolume();

            Stopwatch runtime = new Stopwatch();
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
                float standBoardFeetPerAcre = volume.GetScribnerBoardFeet(trees);
                Debug.Assert(standBoardFeetPerAcre > 35.0F * 1000.0F);
                Debug.Assert(standBoardFeetPerAcre < 40.0F * 1000.0F);
                accumulatedBoardFeetPerAcre += standBoardFeetPerAcre;
            }
            runtime.Stop();

            this.TestContext.WriteLine(runtime.Elapsed.TotalSeconds.ToString());
        }
    }
}
