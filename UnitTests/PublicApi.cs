using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Cmdlets;
using Osu.Cof.Ferm.Data;
using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Osu.Cof.Ferm.Test
{
    [TestClass]
    public class PublicApi : OrganonTest
    {
        public TestContext? TestContext { get; set; }

        private static PlotsWithHeight GetNelder()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Malcolm Knapp Nelder 1.xlsx");
            PlotsWithHeight plot = new PlotsWithHeight(new List<int>() { 1 }, 1.327F);
            plot.Read(plotFilePath, "1");
            return plot;
        }

        private static PlotsWithHeight GetPlot14()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Malcolm Knapp plots 14-18+34 Ministry.xlsx");
            PlotsWithHeight plot = new PlotsWithHeight(new List<int>() { 14 }, 4.48F);
            plot.Read(plotFilePath, "0.2 ha");
            return plot;
        }

        [TestMethod]
        public void HuffmanPeakNobleFir()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "HPNF.xlsx");
            PspStand huffmanPeak = new PspStand(plotFilePath, "HPNF", 0.2F);
            OrganonVariant variant = new OrganonVariantSwo(); // SWO allows mapping ABAM -> ABGR and ABPR -> ABCO
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
            TestStand stand = huffmanPeak.ToStand(configuration, 80.0F);
            int startYear = 1980;
            stand.WriteCompetitionAsCsv("HPNF initial competition.csv", variant, startYear);
            OrganonTest.GrowPspStand(huffmanPeak, stand, variant, startYear, 2015, Path.GetFileNameWithoutExtension(plotFilePath));

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
        public void NelderHero()
        {
            int thinningPeriod = 4;
            int treeCount = 100;
            int expectedTreesSelectedWithFiaVolume = 0;
            float minObjectiveFunctionWithFiaVolume = 4.154F; // USk$/ha
            float minObjectiveFunctionWithScaledVolume = 3.454F; // USk$/ha
            float minThinnedMbfWithFiaVolume = 0.0F;
            #if DEBUG
            treeCount = 48;
            expectedTreesSelectedWithFiaVolume = 0;
            minObjectiveFunctionWithFiaVolume = 1.790F; // USk$/ha
            // minObjectiveFunctionWithScaledVolume = 1.357F; // USk$/ha, nearest 1 cm diameter class and 0.5 cm height class
            minObjectiveFunctionWithScaledVolume = 1.348F; // bilinear interpolation: 1 cm diameter classes, 1 m height classes
            // minObjectiveFunctionWithScaledVolume = 1.332F; // bilinear interpolation: 2 cm diameter classes, 2 m height classes
            // minObjectiveFunctionWithScaledVolume = 1.352F; // bilinear interpolation: 5 cm diameter classes, 5 m height classes
            minThinnedMbfWithFiaVolume = 0.0F;
            #endif

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = new OrganonConfiguration(new OrganonVariantNwo());
            configuration.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, treeCount);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            Objective landExpectationValue = new Objective()
            {
                PlanningPeriods = 9
            };
            Hero hero = new Hero(stand, configuration, landExpectationValue, new HeuristicParameters() { UseFiaVolume = false })
            {
                //IsStochastic = true,
                MaximumIterations = 10
            };
            //hero.RandomizeTreeSelection(TestConstant.Default.SelectionPercentage);
            hero.CurrentTrajectory.SetTreeSelection(0, thinningPeriod);
            hero.Run();

            this.Verify(hero);

            int[] treeSelection = hero.BestTrajectory.IndividualTreeSelectionBySpecies[FiaCode.PseudotsugaMenziesii];
            int treesSelected = treeSelection.Sum() / thinningPeriod;

            if (hero.BestTrajectory.UseFiaVolume)
            {
                // expected standing volume, MBF, for no trees harvested
                // 0.122, 0.971, 3.18, 6.749, 11.56, 17.54, 24.56, 32.44, 40.97, 49.98
                // expected NPV, US$/ha, for no trees harvested
                // -90.96, 61.62, 373.49, 781.90, 1228.99, 1674.68, 2085.93, 2438.25, 2717.76, 2918.96
                this.TestContext!.WriteLine("best objective: {0} observed, near {1} expected", hero.BestObjectiveFunction, minObjectiveFunctionWithFiaVolume);
                Assert.IsTrue(hero.BestObjectiveFunction > minObjectiveFunctionWithFiaVolume);
                Assert.IsTrue(hero.BestObjectiveFunction < 1.01F * minObjectiveFunctionWithFiaVolume);
                Assert.IsTrue(hero.BestTrajectory.ThinningVolume.ScribnerTotal[thinningPeriod] >= minThinnedMbfWithFiaVolume);
                Assert.IsTrue(hero.BestTrajectory.ThinningVolume.ScribnerTotal[thinningPeriod] <= 1.01 * minThinnedMbfWithFiaVolume);
                Assert.IsTrue(treesSelected == expectedTreesSelectedWithFiaVolume);
            }
            else
            {
                this.TestContext!.WriteLine("best objective: {0} observed, near {1} expected", hero.BestObjectiveFunction, minObjectiveFunctionWithScaledVolume);
                Assert.IsTrue(hero.BestObjectiveFunction > minObjectiveFunctionWithScaledVolume);
                Assert.IsTrue(hero.BestObjectiveFunction < 1.02F * minObjectiveFunctionWithScaledVolume);
            }
        }

        [TestMethod]
        public void NelderOtherHeuristics()
        {
            int thinningPeriod = 4;
            int treeCount = 75;
            #if DEBUG
            treeCount = 25;
            #endif

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(new OrganonVariantNwo());
            configuration.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, treeCount);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            Objective landExpectationValue = new Objective()
            {
                PlanningPeriods = 9
            };
            Objective volume = new Objective()
            {
                PlanningPeriods = landExpectationValue.PlanningPeriods,
                TimberObjective = TimberObjective.ScribnerVolume
            };
            HeuristicParameters defaultParameters = new HeuristicParameters()
            {
                UseFiaVolume = false
            };

            GeneticParameters geneticParameters = new GeneticParameters(treeCount)
            {
                PopulationSize = 7,
                MaximumGenerations = 5,
                UseFiaVolume = defaultParameters.UseFiaVolume
            };
            GeneticAlgorithm genetic = new GeneticAlgorithm(stand, configuration, landExpectationValue, geneticParameters);
            TimeSpan geneticRuntime = genetic.Run();

            GreatDeluge deluge = new GreatDeluge(stand, configuration, volume, defaultParameters)
            {
                RainRate = 5,
                LowerWaterAfter = 9,
                StopAfter = 10
            };
            deluge.RandomizeTreeSelection(TestConstant.Default.SelectionPercentage);
            TimeSpan delugeRuntime = deluge.Run();

            RecordTravel recordTravel = new RecordTravel(stand, configuration, landExpectationValue, defaultParameters)
            {
                StopAfter = 10
            };
            recordTravel.RandomizeTreeSelection(TestConstant.Default.SelectionPercentage);
            TimeSpan recordRuntime = recordTravel.Run();

            SimulatedAnnealing annealer = new SimulatedAnnealing(stand, configuration, volume, defaultParameters)
            {
                Iterations = 100
            };
            annealer.RandomizeTreeSelection(TestConstant.Default.SelectionPercentage);
            TimeSpan annealerRuntime = annealer.Run();

            TabuParameters tabuParameters = new TabuParameters()
            {
                UseFiaVolume = defaultParameters.UseFiaVolume
            };
            TabuSearch tabu = new TabuSearch(stand, configuration, landExpectationValue, tabuParameters)
            {
                Iterations = 7,
                //Jump = 2,
                MaximumTenure = 5
            };
            tabu.RandomizeTreeSelection(TestConstant.Default.SelectionPercentage);
            TimeSpan tabuRuntime = tabu.Run();

            ThresholdAccepting thresholdAcceptor = new ThresholdAccepting(stand, configuration, volume, defaultParameters);
            thresholdAcceptor.IterationsPerThreshold.Clear();
            thresholdAcceptor.Thresholds.Clear();
            thresholdAcceptor.IterationsPerThreshold.Add(10);
            thresholdAcceptor.Thresholds.Add(1.0F);
            thresholdAcceptor.RandomizeTreeSelection(TestConstant.Default.SelectionPercentage);
            TimeSpan acceptorRuntime = thresholdAcceptor.Run();

            RandomGuessing random = new RandomGuessing(stand, configuration, volume, defaultParameters)
            {
                Iterations = 4
            };
            TimeSpan randomRuntime = random.Run();

            configuration.Treatments.Harvests.Clear();
            configuration.Treatments.Harvests.Add(new ThinByPrescription(thinningPeriod));
            PrescriptionParameters prescriptionParameters = new PrescriptionParameters()
            {
                Maximum = 60.0F,
                Minimum = 50.0F,
                Step = 10.0F,
                UseFiaVolume = defaultParameters.UseFiaVolume
            };
            PrescriptionEnumeration enumerator = new PrescriptionEnumeration(stand, configuration, landExpectationValue, prescriptionParameters);
            TimeSpan enumerationRuntime = enumerator.Run();

            // heuristics assigned to volume optimization
            this.Verify(deluge);
            this.Verify(annealer);
            this.Verify(thresholdAcceptor);
            this.Verify(random);

            // heuristics assigned to net present value optimization
            this.Verify(genetic);
            this.Verify(enumerator);
            this.Verify(recordTravel);
            this.Verify(tabu);

            HeuristicSolutionDistribution distribution = new HeuristicSolutionDistribution(1, thinningPeriod, treeCount);
            distribution.AddRun(annealer, annealerRuntime, defaultParameters);
            distribution.AddRun(deluge, delugeRuntime, defaultParameters);
            distribution.AddRun(thresholdAcceptor, acceptorRuntime, defaultParameters);
            distribution.AddRun(genetic, geneticRuntime, defaultParameters);
            distribution.AddRun(enumerator, enumerationRuntime, defaultParameters);
            distribution.AddRun(recordTravel, recordRuntime, defaultParameters);
            distribution.AddRun(tabu, tabuRuntime, defaultParameters);
            distribution.AddRun(random, randomRuntime, defaultParameters);
            distribution.OnRunsComplete();
        }

        [TestMethod]
        public void NelderTrajectory()
        {
            int expectedUnthinnedTreeRecordCount = 661;
            int lastPeriod = 9;
            bool useFiaVolume = false;

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            OrganonStandTrajectory unthinnedTrajectory = new OrganonStandTrajectory(stand, configuration, TimberValue.Default, lastPeriod, useFiaVolume);
            unthinnedTrajectory.Simulate();
            foreach (Stand? unthinnedStand in unthinnedTrajectory.StandByPeriod)
            {
                AssertNullable.IsNotNull(unthinnedStand);
                Assert.IsTrue(unthinnedStand.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            }

            int thinPeriod = 3;
            configuration.Treatments.Harvests.Add(new ThinByPrescription(thinPeriod)
                                                  {
                                                      FromAbovePercentage = 20.0F,
                                                      ProportionalPercentage = 15.0F,
                                                      FromBelowPercentage = 10.0F
                                                  });
            OrganonStandTrajectory thinnedTrajectory = new OrganonStandTrajectory(stand, configuration, TimberValue.Default, lastPeriod, useFiaVolume);
            AssertNullable.IsNotNull(thinnedTrajectory.StandByPeriod[0]);
            Assert.IsTrue(thinnedTrajectory.StandByPeriod[0]!.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);

            thinnedTrajectory.Simulate();

            for (int periodIndex = 0; periodIndex < thinPeriod; ++periodIndex)
            {
                Stand? unthinnedStand = thinnedTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(unthinnedStand);
                Assert.IsTrue(unthinnedStand.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            }
            int expectedThinnedTreeRecordCount = 328; // must be updated if prescription changes
            for (int periodIndex = thinPeriod; periodIndex < thinnedTrajectory.PlanningPeriods; ++periodIndex)
            {
                Stand? thinnedStand = thinnedTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(thinnedStand);
                Assert.IsTrue(thinnedStand.GetTreeRecordCount() == expectedThinnedTreeRecordCount);
            }

            // verify unthinned trajectory
            // find/replace regular expression for cleaning up watch window copy/paste: \[\d+\]\s+(\d+.\d{1,3})\d*\s+float\r?\n -> $1F, 
            //                                          0      1      2      3       4       5       6       7       8       9
            float[] minimumUnthinnedQmd = new float[] { 6.61F, 8.17F, 9.52F, 10.67F, 11.68F, 12.56F, 13.34F, 14.05F, 14.69F, 15.28F }; // in
            //                                                0      1      2      3      4       5       6       7       8       9
            float[] minimumUnthinnedTopHeight = new float[] { 54.1F, 67.9F, 80.3F, 91.6F, 101.9F, 111.3F, 119.9F, 127.8F, 135.0F, 141.7F }; // ft
            float[] minimumUnthinnedVolume;
            if (unthinnedTrajectory.UseFiaVolume)
            {
                //                                     0       1       2       3       4       5       6       7       8       9
                minimumUnthinnedVolume = new float[] { 4.428F, 15.02F, 30.49F, 48.39F, 66.72F, 84.45F, 101.1F, 116.4F, 130.6F, 143.6F }; // FIA SV6x32 MBF/ha
            }
            else
            {
                // Poudel 2018 + Scribner long log net MBF/ha
                // nearest 1 cm diameter class and 0.5 m height class
                ///minimumUnthinnedVolume = new float[] { 9.758F, 19.01F, 31.07F, 47.24F, 62.21F, 75.09F, 89.64F, 103.7F, 116.5F, 128.9F };
                // bilinear interpolation
                // minimumUnthinnedVolume = new float[] { 9.669F, 18.93F, 31.07F, 47.22F, 62.24F, 75.22F, 89.55F, 103.8F, 116.7F, 128.8F }; // 0.5 cm diameter classes, 1 m height classes
                minimumUnthinnedVolume = new float[] { 9.648F, 18.88F, 31.01F, 47.24F, 62.33F, 75.18F, 89.45F, 103.8F, 116.8F, 128.7F }; // 1 cm diameter classes, 1 m height classes
                // minimumUnthinnedVolume = new float[] { 9.433F, 18.50F, 30.67F, 46.98F, 61.97F, 75.17F, 89.22F, 103.9F, 116.8F, 128.7F }; // 2 cm diameter classes, 2 m height classes
                // minimumUnthinnedVolume = new float[] { 9.788F, 18.84F, 32.06F, 48.27F, 63.40F, 76.92F, 90.37F, 104.7F, 117.5F, 129.0F }; // 5 cm diameter classes, 5 m height classes
            }
            PublicApi.Verify(unthinnedTrajectory, minimumUnthinnedQmd, minimumUnthinnedTopHeight, minimumUnthinnedVolume, 0, lastPeriod, 0, 0, configuration.Variant.TimeStepInYears);

            // verify thinned trajectory
            //                                        0      1      2      3       4       5       6       7       8       9
            float[] minimumThinnedQmd = new float[] { 6.61F, 8.19F, 9.53F, 11.81F, 13.44F, 14.80F, 15.99F, 17.05F, 18.00F, 18.85F }; // in
            //                                              0      1      2      3      4      5       6       7       8       9
            float[] minimumThinnedTopHeight = new float[] { 54.1F, 67.9F, 80.3F, 88.3F, 98.4F, 108.0F, 116.9F, 125.0F, 132.5F, 139.4F }; // ft
            float[] minimumThinnedVolume;
            if (thinnedTrajectory.UseFiaVolume)
            {
                //                                   0       1       2       3       4       5       6       7       8       9
                minimumThinnedVolume = new float[] { 4.428F, 15.02F, 30.50F, 30.64F, 47.26F, 64.81F, 82.54F, 99.87F, 116.3F, 131.7F }; // FIA MBF/ha
            }
            else
            {
                // Poudel 2018 + Scribner long log net MBF/ha
                // nearest 1 cm diameter class and 0.5 m height class
                // minimumThinnedVolume = new float[] { 9.758F, 19.01F, 31.07F, 28.25F, 41.77F, 54.37F, 68.44F, 85.10F, 100.4F, 114.7F };
                // bilinear interpolation
                // minimumThinnedVolume = new float[] { 9.669F, 18.93F, 31.07F, 28.22F, 41.68F, 54.31F, 68.33F, 84.91F, 100.5F, 114.6F }; // 1 cm diameter classes, 0.5 m height classes
                minimumThinnedVolume = new float[] { 9.648F, 18.88F, 31.01F, 28.13F, 41.60F, 54.34F, 68.41F, 84.91F, 100.4F, 114.6F }; // 1 cm diameter classes, 1 m height classes
                // minimumThinnedVolume = new float[] { 9.433F, 18.50F, 30.67F, 28.05F, 41.55F, 54.22F, 68.39F, 85.09F, 100.0F, 114.7F }; // 2 cm diameter classes, 2 m height classes
                // minimumThinnedVolume = new float[] { 9.788F, 18.84F, 32.06F, 29.34F, 42.72F, 55.94F, 69.84F, 85.99F, 101.3F, 115.0F }; // 5 cm diameter classes, 5 m height classes
            }
            PublicApi.Verify(thinnedTrajectory, minimumThinnedQmd, minimumThinnedTopHeight, minimumThinnedVolume, thinPeriod, lastPeriod, 200, 400, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(thinnedTrajectory, minimumThinnedVolume, thinPeriod);
        }

        [TestMethod]
        public void OrganonStandGrowthApi()
        {
            TestStand.WriteTreeHeader(this.TestContext!);
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);

                // check crown competition API
                TestStand stand = OrganonTest.CreateDefaultStand(configuration);
                float crownCompetitionFactor = OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand)[0];
                Assert.IsTrue(crownCompetitionFactor >= 0.0F);
                Assert.IsTrue(crownCompetitionFactor <= TestConstant.Maximum.CrownCompetitionFactor);
                OrganonTest.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // recalculate heights and crown ratios for all trees
                Dictionary<FiaCode, SpeciesCalibration> calibrationBySpecies = configuration.CreateSpeciesCalibration();
                foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                {
                    OrganonGrowth.SetIngrowthHeightAndCrownRatio(variant, stand, treesOfSpecies, treesOfSpecies.Count, calibrationBySpecies);
                }
                OrganonTest.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // run Organon growth simulation
                stand = OrganonTest.CreateDefaultStand(configuration);
                if (configuration.IsEvenAge)
                {
                    // stand error if less than one year to grow to breast height
                    stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
                }
                stand.SetQuantiles();
                stand.WriteTreesAsCsv(this.TestContext!, variant, 0, false);

                TestStand initialStand = new TestStand(stand);
                TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath();
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonGrowth.Grow(simulationStep, configuration, stand, calibrationBySpecies);
                    treeGrowth.AccumulateGrowthAndMortality(stand);
                    OrganonTest.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, OrganonWarnings.LessThan50TreeRecords, stand, variant);

                    stand.WriteTreesAsCsv(this.TestContext!, variant, variant.GetEndYear(simulationStep), false);
                }

                OrganonTest.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, treeGrowth, initialStand, stand);
                OrganonTest.Verify(calibrationBySpecies);
            }
        }

        [TestMethod]
        public void Plot14ImmediateThin()
        {
            int thinPeriod = 1;
            int lastPeriod = 4;
            bool useFiaVolume = false;

            PlotsWithHeight plot14 = PublicApi.GetPlot14();
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = plot14.ToOrganonStand(configuration, 30, 130.0F);
            stand.PlantingDensityInTreesPerHectare = TestConstant.Plot14ReplantingDensityInTreesPerHectare;

            configuration.Treatments.Harvests.Add(new ThinByPrescription(thinPeriod)
            {
                FromAbovePercentage = 0.0F,
                ProportionalPercentage = 30.0F,
                FromBelowPercentage = 0.0F
            });
            OrganonStandTrajectory thinnedTrajectory = new OrganonStandTrajectory(stand, configuration, TimberValue.Default, lastPeriod, useFiaVolume);
            AssertNullable.IsNotNull(thinnedTrajectory.StandByPeriod[0]);
            Assert.IsTrue(thinnedTrajectory.StandByPeriod[0]!.GetTreeRecordCount() == 222);
            thinnedTrajectory.Simulate();

            // verify thinned trajectory
            // find/replace regular expression: \[\d+\]\s+(\d+.\d{1,3})\d*\s+float\r?\n -> $1F, 
            //                                        0      1       2       3       4     
            float[] minimumThinnedQmd = new float[] { 9.16F, 10.47F, 11.64F, 12.64F, 13.52F }; // in
            //                                              0      1       2       3       4     
            float[] minimumThinnedTopHeight = new float[] { 92.9F, 101.4F, 110.4F, 118.9F, 126.7F }; // ft
            float[] minimumThinnedVolume;
            if (thinnedTrajectory.UseFiaVolume)
            {
                //                                   0       1       2       3       4     
                minimumThinnedVolume = new float[] { 43.74F, 49.00F, 68.86F, 87.95F, 105.8F }; // Browning 1977 (FIA) MBF/ha
            }
            else
            {
                // Poudel 2018 + Scribner long log net MBF/ha
                // nearest 1 cm diameter class and 0.5 m height class
                // minimumThinnedVolume = new float[] { 51.59F, 51.75F, 66.71F, 81.88F, 97.72F };
                // bilinear interpolation: 
                // minimumThinnedVolume = new float[] { 51.333F, 51.521F, 66.819F, 81.757F, 97.212F }; // 1 cm diameter classes, 0.5 m height classes
                minimumThinnedVolume = new float[] { 51.244F, 51.549F, 66.957F, 81.836F, 97.184F }; // 1 cm diameter classes, 1 m height classes
                // minimumThinnedVolume = new float[] { 50.444F, 51.261F, 66.582F, 81.800F, 97.521F }; // 2 cm diameter classes, 2 m height classes
                // minimumThinnedVolume = new float[] { 52.466F, 52.364F, 68.895F, 83.383F, 98.442F }; // 5 cm diameter classes, 5 m height classes
            }

            PublicApi.Verify(thinnedTrajectory, minimumThinnedQmd, minimumThinnedTopHeight, minimumThinnedVolume, thinPeriod, lastPeriod, 65, 70, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(thinnedTrajectory, minimumThinnedVolume, thinPeriod);
            Assert.IsTrue(thinnedTrajectory.GetFirstHarvestAge() == 30);
        }

        [TestMethod]
        public void RS39()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "RS39 lower half.xlsx");
            PspStand rs39 = new PspStand(plotFilePath, "RS39 lower half", 0.154441F);
            OrganonVariant variant = new OrganonVariantNwo();
            OrganonConfiguration configuration = new OrganonConfiguration(variant);
            TestStand stand = rs39.ToStand(configuration, 105.0F);
            int startYear = 1992;
            stand.WriteCompetitionAsCsv("RS39 lower half initial competition.csv", variant, startYear);
            OrganonTest.GrowPspStand(rs39, stand, variant, startYear, 2019, Path.GetFileNameWithoutExtension(plotFilePath));

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
            int thinningPeriod = 4;
            int runs = 4; // 1 warmup run + measured runs
            int trees = 300;
            #if DEBUG
            runs = 1; // do only functional validation of test on DEBUG builds to reduce test execution time
            trees = 10;
            #endif

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = PublicApi.CreateOrganonConfiguration(new OrganonVariantNwo());
            configuration.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, trees);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            Objective landExpectationValue = new Objective()
            {
                PlanningPeriods = 9
            };
            HeuristicParameters defaultParameters = new HeuristicParameters();

            TimeSpan runtime = TimeSpan.Zero;
            for (int run = 0; run < runs; ++run)
            {
                // after warmup: 3 runs * 300 trees = 900 measured growth simulations on i7-3770 (4th gen, Sandy Bridge)
                // dispersion of 5 runs                   min   mean  median  max
                // .NET 5.0 with removed tree compaction  1.67  1.72  1.72    1.82
                Hero hero = new Hero(stand, configuration, landExpectationValue, defaultParameters)
                {
                    IsStochastic = false,
                    MaximumIterations = 2
                };
                hero.RandomizeTreeSelection(TestConstant.Default.SelectionPercentage);
                if (run > 0)
                {
                    // skip first run as a warmup run
                    runtime += hero.Run();
                }
            }
            this.TestContext!.WriteLine(runtime.TotalSeconds.ToString());
        }

        private void Verify(GeneticAlgorithm genetic)
        {
            this.Verify((Heuristic)genetic);

            PopulationStatistics statistics = genetic.PopulationStatistics;
            Assert.IsTrue(statistics.Generations <= genetic.Parameters.MaximumGenerations);
            Assert.IsTrue(statistics.CoefficientOfVarianceByGeneration.Count == statistics.Generations);
            Assert.IsTrue(statistics.MeanAllelesPerLocusByGeneration.Count == statistics.Generations);
            Assert.IsTrue(statistics.MeanHeterozygosityByGeneration.Count == statistics.Generations);
            Assert.IsTrue(statistics.NewIndividualsByGeneration.Count == statistics.Generations);
            Assert.IsTrue(statistics.PolymorphismByGeneration.Count == statistics.Generations);
            for (int generationIndex = 0; generationIndex < statistics.Generations; ++generationIndex)
            {
                float coefficientOfVariation = statistics.CoefficientOfVarianceByGeneration[generationIndex];
                float meanAllelesPerLocus = statistics.MeanAllelesPerLocusByGeneration[generationIndex];
                float meanHeterozygosity = statistics.MeanHeterozygosityByGeneration[generationIndex];
                int newIndividuals = statistics.NewIndividualsByGeneration[generationIndex];
                float polymorphism = statistics.PolymorphismByGeneration[generationIndex];

                Assert.IsTrue(coefficientOfVariation >= 0.0F);
                Assert.IsTrue((meanAllelesPerLocus >= 0.0F) && (meanAllelesPerLocus <= 2.0F)); // assumes HarvestPeriodSelection.All
                Assert.IsTrue((meanHeterozygosity >= 0.0F) && (meanHeterozygosity <= 1.0F));
                Assert.IsTrue((newIndividuals >= 0) && (newIndividuals <= 2 * genetic.Parameters.PopulationSize)); // two children per breeding
                Assert.IsTrue((polymorphism >= 0.0F) && (polymorphism <= 1.0F));
            }
        }

        private void Verify(Heuristic heuristic)
        {
            // check objective functions
            Assert.IsTrue(heuristic.BestTrajectory.PlantingDensityInTreesPerHectare == heuristic.CurrentTrajectory.PlantingDensityInTreesPerHectare);
            Assert.IsTrue(heuristic.BestTrajectory.PlantingDensityInTreesPerHectare >= TestConstant.NelderReplantingDensityInTreesPerHectare);

            float recalculatedBestObjectiveFunction = heuristic.GetObjectiveFunction(heuristic.BestTrajectory);
            float bestObjectiveFunctionRatio = heuristic.BestObjectiveFunction / recalculatedBestObjectiveFunction;
            this.TestContext!.WriteLine("best objective: {0}", heuristic.BestObjectiveFunction);
            if (heuristic.Objective.TimberObjective == TimberObjective.LandExpectationValue)
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction > -0.70F);
            }
            else
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction > 0.0F);
            }

            float beginObjectiveFunction = heuristic.CandidateObjectiveFunctionByMove.First();
            Assert.IsTrue(heuristic.BestObjectiveFunction >= beginObjectiveFunction);

            // only guaranteed for monotonic heuristics: hero, prescription enumeration, others depending on configuration
            if ((heuristic is SimulatedAnnealing == false) && (heuristic is TabuSearch == false))
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction == heuristic.AcceptedObjectiveFunctionByMove[^1]);
            }
            Assert.IsTrue(heuristic.AcceptedObjectiveFunctionByMove.Count >= 3);
            Assert.IsTrue(bestObjectiveFunctionRatio > 0.99999);
            Assert.IsTrue(bestObjectiveFunctionRatio < 1.00001);

            float recalculatedCurrentObjectiveFunction = heuristic.GetObjectiveFunction(heuristic.CurrentTrajectory);
            if (heuristic is RandomGuessing == false)
            {
                Assert.IsTrue(recalculatedCurrentObjectiveFunction >= beginObjectiveFunction);
            }
            Assert.IsTrue(recalculatedCurrentObjectiveFunction <= heuristic.BestObjectiveFunction);

            float endObjectiveFunction = heuristic.CandidateObjectiveFunctionByMove.Last();
            Assert.IsTrue(endObjectiveFunction <= heuristic.BestObjectiveFunction);

            // check harvest schedule
            int harvestPeriod = heuristic.BestTrajectory.HarvestPeriods - 1;
            foreach (KeyValuePair<FiaCode, int[]> selectionForSpecies in heuristic.BestTrajectory.IndividualTreeSelectionBySpecies)
            {
                int[] bestTreeSelection = selectionForSpecies.Value;
                int[] currentTreeSelection = heuristic.CurrentTrajectory.IndividualTreeSelectionBySpecies[selectionForSpecies.Key];
                Assert.IsTrue(bestTreeSelection.Length == currentTreeSelection.Length);
                for (int treeIndex = 0; treeIndex < bestTreeSelection.Length; ++treeIndex)
                {
                    Assert.IsTrue((bestTreeSelection[treeIndex] == 0) || (bestTreeSelection[treeIndex] == harvestPeriod));
                    Assert.IsTrue((currentTreeSelection[treeIndex] == 0) || (currentTreeSelection[treeIndex] == harvestPeriod));
                }
            }

            // check volumes
            Assert.IsTrue(harvestPeriod > 0);
            heuristic.BestTrajectory.GetGradedVolumes(out StandGradedVolume bestGradedVolumeStanding, out StandGradedVolume bestGradedVolumeHarvested);
            heuristic.CurrentTrajectory.GetGradedVolumes(out StandGradedVolume currentGradedVolumeStanding, out StandGradedVolume currentGradedVolumeHarvested);
            float previousBestCubicStandingVolume = Single.NaN;
            float previousCurrentCubicStandingVolume = Single.NaN;
            for (int periodIndex = 0; periodIndex < heuristic.BestTrajectory.PlanningPeriods; ++periodIndex)
            {
                float bestCubicStandingVolume = bestGradedVolumeStanding.GetCubicTotal(periodIndex);
                float bestCubicThinningVolume = bestGradedVolumeHarvested.GetCubicTotal(periodIndex);
                float currentCubicStandingVolume = currentGradedVolumeStanding.GetCubicTotal(periodIndex);
                float currentCubicThinningVolume = currentGradedVolumeHarvested.GetCubicTotal(periodIndex);

                float bestCubicStandingCheckVolume = bestGradedVolumeStanding.Cubic2Saw[periodIndex] + bestGradedVolumeStanding.Cubic3Saw[periodIndex] + bestGradedVolumeStanding.Cubic4Saw[periodIndex];
                float bestCubicThinningCheckVolume = bestGradedVolumeHarvested.Cubic2Saw[periodIndex] + bestGradedVolumeHarvested.Cubic3Saw[periodIndex] + bestGradedVolumeHarvested.Cubic4Saw[periodIndex];
                float currentCubicStandingCheckVolume = currentGradedVolumeStanding.Cubic2Saw[periodIndex] + currentGradedVolumeStanding.Cubic3Saw[periodIndex] + currentGradedVolumeStanding.Cubic4Saw[periodIndex];
                float currentCubicThinningCheckVolume = currentGradedVolumeHarvested.Cubic2Saw[periodIndex] + currentGradedVolumeHarvested.Cubic3Saw[periodIndex] + currentGradedVolumeHarvested.Cubic4Saw[periodIndex];
                Assert.IsTrue(MathF.Abs(bestCubicStandingVolume - bestCubicStandingCheckVolume) < 0.000001F);
                Assert.IsTrue(MathF.Abs(bestCubicThinningVolume - bestCubicThinningCheckVolume) < 0.000001F);
                Assert.IsTrue(MathF.Abs(currentCubicStandingVolume - currentCubicStandingCheckVolume) < 0.000001F);
                Assert.IsTrue(MathF.Abs(currentCubicThinningVolume - currentCubicThinningCheckVolume) < 0.000001F);

                if (periodIndex == harvestPeriod)
                {
                    Assert.IsTrue(heuristic.BestTrajectory.BasalAreaRemoved[periodIndex] >= 0.0F); // best selection with debug stand is no harvest
                    Assert.IsTrue(heuristic.BestTrajectory.BasalAreaRemoved[periodIndex] <= 200.0F);
                    Assert.IsTrue(heuristic.BestTrajectory.ThinningVolume.ScribnerTotal[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestCubicThinningVolume <= previousBestCubicStandingVolume);
                    Assert.IsTrue(heuristic.BestTrajectory.ThinningVolume.ScribnerTotal[periodIndex] < heuristic.BestTrajectory.StandingVolume.ScribnerTotal[periodIndex - 1] + 0.000001F); // allow for numerical error in case where all trees are harvested
                    Assert.IsTrue(bestGradedVolumeHarvested.Cubic2Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Cubic3Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Cubic4Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.NetPresentValue2Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.NetPresentValue3Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.NetPresentValue4Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Scribner2Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Scribner3Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Scribner4Saw[periodIndex] >= 0.0F);

                    Assert.IsTrue(heuristic.CurrentTrajectory.BasalAreaRemoved[periodIndex] >= 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.BasalAreaRemoved[periodIndex] <= 200.0F);
                    Assert.IsTrue(currentCubicThinningVolume >= 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.ThinningVolume.ScribnerTotal[periodIndex] >= 0.0F);
                    Assert.IsTrue(currentCubicThinningVolume <= previousCurrentCubicStandingVolume);
                    Assert.IsTrue(heuristic.CurrentTrajectory.ThinningVolume.ScribnerTotal[periodIndex] < heuristic.CurrentTrajectory.StandingVolume.ScribnerTotal[periodIndex - 1] + 0.000001F); // numerical error
                }
                else
                {
                    // for now, harvest should occur only in the one indicated period
                    Assert.IsTrue(heuristic.BestTrajectory.BasalAreaRemoved[periodIndex] == 0.0F);
                    Assert.IsTrue(bestCubicThinningVolume == 0.0F);
                    Assert.IsTrue(heuristic.BestTrajectory.ThinningVolume.ScribnerTotal[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Cubic2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Cubic3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Cubic4Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.NetPresentValue2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.NetPresentValue3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.NetPresentValue4Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Scribner2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Scribner3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestGradedVolumeHarvested.Scribner4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(heuristic.CurrentTrajectory.BasalAreaRemoved[periodIndex] == 0.0F);
                    Assert.IsTrue(currentCubicThinningVolume == 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.ThinningVolume.ScribnerTotal[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.Cubic2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.Cubic3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.Cubic4Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.NetPresentValue2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.NetPresentValue3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.NetPresentValue4Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.Scribner2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.Scribner3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentGradedVolumeHarvested.Scribner4Saw[periodIndex] == 0.0F);
                }

                if (periodIndex == 0)
                {
                    // zero merchantable on Nelder 1 at age 20 with Poudel 2018 net volume
                    Assert.IsTrue(bestCubicStandingVolume >= 0.0F);
                    Assert.IsTrue(heuristic.BestTrajectory.StandingVolume.ScribnerTotal[periodIndex] >= 0.0F);
                    Assert.IsTrue(currentCubicStandingVolume >= 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.StandingVolume.ScribnerTotal[periodIndex] >= 0.0F);
                }
                else
                {
                    Assert.IsTrue(bestCubicStandingVolume > 0.0F);
                    Assert.IsTrue(heuristic.BestTrajectory.StandingVolume.ScribnerTotal[periodIndex] > 0.0F);
                    Assert.IsTrue(currentCubicStandingVolume > 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.StandingVolume.ScribnerTotal[periodIndex] > 0.0F);

                    if (periodIndex != harvestPeriod)
                    {
                        // for now, assume monotonic increase in standing volumes except in harvest periods
                        Assert.IsTrue(bestCubicStandingVolume > previousBestCubicStandingVolume);
                        Assert.IsTrue(heuristic.BestTrajectory.StandingVolume.ScribnerTotal[periodIndex] > heuristic.BestTrajectory.StandingVolume.ScribnerTotal[periodIndex - 1]);
                        Assert.IsTrue(currentCubicStandingVolume > previousCurrentCubicStandingVolume);
                        Assert.IsTrue(heuristic.CurrentTrajectory.StandingVolume.ScribnerTotal[periodIndex] > heuristic.CurrentTrajectory.StandingVolume.ScribnerTotal[periodIndex - 1]);
                    }
                }

                previousBestCubicStandingVolume = bestCubicStandingVolume;
                previousCurrentCubicStandingVolume = currentCubicStandingVolume;
            }

            // check moves
            Assert.IsTrue(heuristic.AcceptedObjectiveFunctionByMove.Count == heuristic.CandidateObjectiveFunctionByMove.Count);
            Assert.IsTrue(heuristic.AcceptedObjectiveFunctionByMove[0] == heuristic.CandidateObjectiveFunctionByMove[0]);
            for (int moveIndex = 1; moveIndex < heuristic.AcceptedObjectiveFunctionByMove.Count; ++moveIndex)
            {
                Assert.IsTrue(heuristic.AcceptedObjectiveFunctionByMove[moveIndex] <= heuristic.BestObjectiveFunction);
                // doesn't necessarily apply to tabu search
                // Assert.IsTrue(heuristic.AcceptedObjectiveFunctionByMove[moveIndex - 1] <= heuristic.AcceptedObjectiveFunctionByMove[moveIndex]);
                Assert.IsTrue(heuristic.CandidateObjectiveFunctionByMove[moveIndex] <= heuristic.AcceptedObjectiveFunctionByMove[moveIndex]);
            }

            IHeuristicMoveLog? moveLog = heuristic.GetMoveLog();
            if (moveLog != null)
            {
                string csvHeader = moveLog.GetCsvHeader("prefix ");
                Assert.IsTrue(String.IsNullOrWhiteSpace(csvHeader) == false);

                for (int moveIndex = 0; moveIndex < heuristic.AcceptedObjectiveFunctionByMove.Count; ++moveIndex)
                {
                    string csvValues = moveLog.GetCsvValues(moveIndex);
                    Assert.IsTrue(String.IsNullOrWhiteSpace(csvValues) == false);
                }
            }

            // check parameters
            HeuristicParameters? parameters = heuristic.GetParameters();
            if (parameters != null)
            {
                string csvHeader = parameters.GetCsvHeader();
                string csvValues = parameters.GetCsvValues();

                Assert.IsTrue(String.IsNullOrWhiteSpace(csvHeader) == false);
                Assert.IsTrue(String.IsNullOrWhiteSpace(csvValues) == false);
            }
        }

        private static void Verify(OrganonStandTrajectory thinnedTrajectory, float[] minimumThinnedVolumeScribner, int thinPeriod)
        {
            for (int periodIndex = 0; periodIndex < thinnedTrajectory.PlanningPeriods; ++periodIndex)
            {
                if (periodIndex == thinPeriod)
                {
                    Assert.IsTrue(thinnedTrajectory.BasalAreaRemoved[periodIndex] > 0.0F);
                    Assert.IsTrue(thinnedTrajectory.BasalAreaRemoved[periodIndex] < thinnedTrajectory.DensityByPeriod[periodIndex - 1].BasalAreaPerAcre); // assume <50% thin by volume
                    Assert.IsTrue(thinnedTrajectory.ThinningVolume.ScribnerTotal[periodIndex] > 0.0F);
                    Assert.IsTrue(thinnedTrajectory.ThinningVolume.ScribnerTotal[periodIndex] < minimumThinnedVolumeScribner[periodIndex]);
                }
                else
                {
                    Assert.IsTrue(thinnedTrajectory.BasalAreaRemoved[periodIndex] == 0.0F);
                    Assert.IsTrue(thinnedTrajectory.ThinningVolume.ScribnerTotal[periodIndex] == 0.0F);
                }
            }
        }

        private static void Verify(OrganonStandTrajectory trajectory, float[] minimumQmd, float[] minimumTopHeight, float[] minimumStandingVolumeScribner, int thinPeriod, int lastPeriod, int minTrees, int maxTrees, int timeStepInYears)
        {
            Assert.IsTrue(trajectory.BasalAreaRemoved.Length == lastPeriod + 1);
            Assert.IsTrue(trajectory.BasalAreaRemoved[0] == 0.0F);
            Assert.IsTrue(trajectory.HarvestPeriods == thinPeriod + 1); // BUGBUG: clean off by one semantic
            Assert.IsTrue(trajectory.ThinningVolume.ScribnerTotal[0] == 0.0F);
            Assert.IsTrue(trajectory.ThinningVolume.ScribnerTotal.Length == lastPeriod + 1);
            PublicApi.Verify(trajectory.IndividualTreeSelectionBySpecies, thinPeriod, minTrees, maxTrees);
            Assert.IsTrue(String.IsNullOrEmpty(trajectory.Name) == false);
            Assert.IsTrue(trajectory.PeriodLengthInYears == timeStepInYears);
            Assert.IsTrue(trajectory.PlanningPeriods == lastPeriod + 1); // BUGBUG: clean off by one semantic

            float qmdTolerance = 1.01F;
            float topHeightTolerance = 1.01F;
            float volumeTolerance = 1.01F;
            for (int periodIndex = 0; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
            {
                Assert.IsTrue(trajectory.DensityByPeriod[periodIndex].BasalAreaPerAcre > 0.0F);
                Assert.IsTrue(trajectory.DensityByPeriod[periodIndex].BasalAreaPerAcre <= TestConstant.Maximum.TreeBasalAreaLarger);
                Assert.IsTrue(trajectory.StandingVolume.ScribnerTotal[periodIndex] > minimumStandingVolumeScribner[periodIndex]);
                Assert.IsTrue(trajectory.StandingVolume.ScribnerTotal[periodIndex] < volumeTolerance * minimumStandingVolumeScribner[periodIndex]);

                OrganonStand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information missing for period " + periodIndex + ".");
                float qmd = stand.GetQuadraticMeanDiameter();
                float topHeight = stand.GetTopHeight();
                int treeRecords = stand.GetTreeRecordCount();

                Assert.IsTrue((stand.Name != null) && (trajectory.Name != null) && stand.Name.StartsWith(trajectory.Name));
                Assert.IsTrue(qmd > minimumQmd[periodIndex]);
                Assert.IsTrue(qmd < qmdTolerance * minimumQmd[periodIndex]);
                Assert.IsTrue(topHeight > minimumTopHeight[periodIndex]);
                Assert.IsTrue(topHeight < topHeightTolerance * minimumTopHeight[periodIndex]);
                Assert.IsTrue(treeRecords > 0);
                Assert.IsTrue(treeRecords < 666);

                // TODO: check qmd against QMD from basal area
            }
        }

        private static void Verify(SortedDictionary<FiaCode, int[]> individualTreeSelectionBySpecies, int harvestPeriod, int minimumTreesSelected, int maximumTreesSelected)
        {
            int outOfRangeTrees = 0;
            int treesSelected = 0;
            foreach (int[] individualTreeSelection in individualTreeSelectionBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < individualTreeSelection.Length; ++treeIndex)
                {
                    int treeSelection = individualTreeSelection[treeIndex];
                    bool isOutOfRange = (treeSelection != 0) && (treeSelection != harvestPeriod);
                    if (isOutOfRange)
                    {
                        ++outOfRangeTrees;
                    }

                    bool isSelected = treeSelection != 0;
                    if (isSelected)
                    {
                        ++treesSelected;
                    }
                }
            }

            Assert.IsTrue(outOfRangeTrees == 0);
            Assert.IsTrue(treesSelected >= minimumTreesSelected);
            Assert.IsTrue(treesSelected <= maximumTreesSelected);
        }
    }
}
