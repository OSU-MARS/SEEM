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
        public TestContext TestContext { get; set; }

        private PlotWithHeight GetNelder()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Malcolm Knapp Nelder 1.xlsx");
            PlotWithHeight plot = new PlotWithHeight(1, 1.327F);
            plot.Read(plotFilePath, "1");
            return plot;
        }

        private PlotWithHeight GetPlot14()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Malcolm Knapp plots 14-18+34 Ministry.xlsx");
            PlotWithHeight plot = new PlotWithHeight(14, 4.48F);
            plot.Read(plotFilePath, "0.2 ha");
            return plot;
        }

        [TestMethod]
        public void HuffmanPeakNobleFir()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "HPNF.xlsx");
            PspStand huffmanPeak = new PspStand(plotFilePath, "HPNF", 0.2F);
            OrganonVariant variant = new OrganonVariantSwo(); // SWO allows mapping ABAM -> ABGR and ABPR -> ABCO
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
            TestStand stand = huffmanPeak.ToStand(configuration, 80.0F);
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
        public void NelderHero()
        {
            int thinningPeriod = 4;
            int treeCount = 100;
            float minObjectiveFunctionWithFiaVolume = 3.240F; // USk$/ha
            float minObjectiveFunctionWithScaledVolume = 3.331F; // USk$/ha
            #if DEBUG
            treeCount = 48;
            minObjectiveFunctionWithFiaVolume = 1.201F; // USk$/ha
            minObjectiveFunctionWithScaledVolume = 1.291F; // USk$/ha
            #endif

            PlotWithHeight nelder = this.GetNelder();
            OrganonConfiguration configuration = new OrganonConfiguration(new OrganonVariantNwo());
            configuration.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, treeCount);

            Objective landExpectationValue = new Objective()
            {
                IsLandExpectationValue = true,
                PlanningPeriods = 9
            };
            Hero hero = new Hero(stand, configuration, landExpectationValue, new HeuristicParameters() { UseScaledVolume = false })
            {
                IsStochastic = true,
                MaximumIterations = 10
            };
            hero.RandomizeTreeSelection(TestConstant.Default.SelectionPercentage);
            hero.Run();

            this.Verify(hero);
            this.TestContext.WriteLine("best objective: {0}", hero.BestObjectiveFunction);
            if (hero.BestTrajectory.UseScaledVolume)
            {
                Assert.IsTrue(hero.BestObjectiveFunction > minObjectiveFunctionWithScaledVolume);
                Assert.IsTrue(hero.BestObjectiveFunction < 1.02F * minObjectiveFunctionWithScaledVolume);
            }
            else
            {
                Assert.IsTrue(hero.BestObjectiveFunction > minObjectiveFunctionWithFiaVolume);
                Assert.IsTrue(hero.BestObjectiveFunction < 1.02F * minObjectiveFunctionWithFiaVolume);
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

            PlotWithHeight nelder = this.GetNelder();
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(new OrganonVariantNwo());
            configuration.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, treeCount);

            Objective landExpectationValue = new Objective()
            {
                IsLandExpectationValue = true,
                PlanningPeriods = 9
            };
            Objective volume = new Objective()
            {
                PlanningPeriods = landExpectationValue.PlanningPeriods
            };
            HeuristicParameters defaultParameters = new HeuristicParameters()
            {
                UseScaledVolume = false
            };

            GeneticParameters geneticParameters = new GeneticParameters(treeCount)
            {
                UseScaledVolume = defaultParameters.UseScaledVolume
            };
            GeneticAlgorithm genetic = new GeneticAlgorithm(stand, configuration, landExpectationValue, geneticParameters)
            {
                PopulationSize = 7,
                MaximumGenerations = 5,
            };
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
                UseScaledVolume = defaultParameters.UseScaledVolume
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
                UseScaledVolume = defaultParameters.UseScaledVolume
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
            int lastPeriod = 9;
            bool useScaledVolume = false;

            PlotWithHeight nelder = this.GetNelder();
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F);

            OrganonStandTrajectory unthinnedTrajectory = new OrganonStandTrajectory(stand, configuration, TimberValue.Default, lastPeriod, useScaledVolume);
            unthinnedTrajectory.Simulate();

            int thinPeriod = 3;
            configuration.Treatments.Harvests.Add(new ThinByPrescription(thinPeriod)
            {
                FromAbovePercentage = 20.0F, 
                ProportionalPercentage = 15.0F, 
                FromBelowPercentage = 10.0F
            });
            OrganonStandTrajectory thinnedTrajectory = new OrganonStandTrajectory(stand, configuration, TimberValue.Default, lastPeriod, useScaledVolume);
            thinnedTrajectory.Simulate();

            // verify unthinned trajectory
            //                                          0      1      2      3       4       5       6       7       8       9
            float[] minimumUnthinnedQmd = new float[] { 6.61F, 8.17F, 9.52F, 10.67F, 11.68F, 12.56F, 13.34F, 14.05F, 14.69F, 15.28F }; // in
            //                                                0      1      2      3      4       5       6       7       8       9
            float[] minimumUnthinnedTopHeight = new float[] { 54.1F, 67.9F, 80.3F, 91.6F, 101.9F, 111.3F, 119.9F, 127.8F, 135.0F, 141.7F }; // ft
            float[] minimumUnthinnedVolume;
            if (unthinnedTrajectory.UseScaledVolume)
            {
                minimumUnthinnedVolume = new float[] { 9.754F, 19.01F, 31.07F, 47.24F, 62.21F, 75.09F, 89.60F, 103.8F, 116.5F, 128.9F }; // Poudel 2018 + Scribner long log net MBF/ha
            }
            else
            {
                //                                     0       1       2       3       4       5       6       7       8       9
                minimumUnthinnedVolume = new float[] { 4.428F, 15.02F, 30.49F, 48.39F, 66.72F, 84.45F, 101.1F, 116.4F, 130.6F, 143.6F }; // FIA SV6x32 MBF/ha
            }
            this.Verify(unthinnedTrajectory, minimumUnthinnedQmd, minimumUnthinnedTopHeight, minimumUnthinnedVolume, 0, lastPeriod, 0, 0, configuration.Variant.TimeStepInYears);

            // verify thinned trajectory
            //                                        0      1      2      3       4       5       6       7       8       9
            float[] minimumThinnedQmd = new float[] { 6.61F, 8.17F, 9.52F, 11.41F, 12.95F, 14.33F, 15.56F, 16.65F, 17.62F, 18.51F }; // in
            //                                              0      1      2      3      4      5       6       7       8       9
            float[] minimumThinnedTopHeight = new float[] { 54.1F, 67.9F, 80.3F, 88.3F, 98.4F, 108.0F, 116.9F, 125.0F, 132.5F, 139.4F }; // ft
            float[] minimumThinnedVolume;
            if (thinnedTrajectory.UseScaledVolume)
            {
                minimumThinnedVolume = new float[] { 9.758F, 19.01F, 31.07F, 26.63F, 39.89F, 52.26F, 65.87F, 82.84F, 99.15F, 113.7F }; // Poudel 2018 + Scribner long log net MBF/ha
            }
            else
            {
                //                                   0       1       2       3       4       5       6       7       8       9
                minimumThinnedVolume = new float[] { 4.428F, 15.02F, 30.49F, 28.45F, 44.02F, 61.28F, 79.10F, 96.69F, 113.5F, 129.3F }; // FIA MBF/ha
            }
            this.Verify(thinnedTrajectory, minimumThinnedQmd, minimumThinnedTopHeight, minimumThinnedVolume, thinPeriod, lastPeriod, 200, 400, configuration.Variant.TimeStepInYears);
            this.Verify(thinnedTrajectory, minimumThinnedVolume, thinPeriod);
        }

        [TestMethod]
        public void OrganonStandGrowthApi()
        {
            TestStand.WriteTreeHeader(this.TestContext);
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);

                // check crown competition API
                TestStand stand = this.CreateDefaultStand(configuration);
                float crownCompetitionFactor = OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand)[0];
                Assert.IsTrue(crownCompetitionFactor >= 0.0F);
                Assert.IsTrue(crownCompetitionFactor <= TestConstant.Maximum.CrownCompetitionFactor);
                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // recalculate heights and crown ratios for all trees
                Dictionary<FiaCode, float[]> CALIB = configuration.CreateSpeciesCalibration();
                foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                {
                    OrganonGrowth.SetIngrowthHeightAndCrownRatio(variant, stand, treesOfSpecies, treesOfSpecies.Count, CALIB);
                }
                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // run Organon growth simulation
                stand = this.CreateDefaultStand(configuration);
                if (configuration.IsEvenAge)
                {
                    // stand error if less than one year to grow to breast height
                    stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
                }
                stand.SetQuantiles();
                stand.WriteTreesAsCsv(this.TestContext, variant, 0, false);

                TestStand initialStand = new TestStand(stand);
                TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath();
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonGrowth.Grow(simulationStep, configuration, stand, CALIB);
                    treeGrowth.AccumulateGrowthAndMortality(stand);
                    this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, OrganonWarnings.LessThan50TreeRecords, stand, variant);

                    stand.WriteTreesAsCsv(this.TestContext, variant, variant.GetEndYear(simulationStep), false);
                }

                this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, treeGrowth, initialStand, stand);
                this.Verify(CALIB);
            }
        }

        [TestMethod]
        public void Plot14ImmediateThin()
        {
            int thinPeriod = 1;
            int lastPeriod = 4;
            bool useScaledVolume = true;

            PlotWithHeight plot14 = this.GetPlot14();
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = plot14.ToOrganonStand(configuration, 30, 130.0F);

            configuration.Treatments.Harvests.Add(new ThinByPrescription(thinPeriod)
            {
                FromAbovePercentage = 0.0F,
                ProportionalPercentage = 30.0F,
                FromBelowPercentage = 0.0F
            });
            OrganonStandTrajectory thinnedTrajectory = new OrganonStandTrajectory(stand, configuration, TimberValue.Default, lastPeriod, useScaledVolume);
            thinnedTrajectory.Simulate();

            // verify thinned trajectory
            //                                        0      1       2       3       4     
            float[] minimumThinnedQmd = new float[] { 9.16F, 10.26F, 11.41F, 12.42F, 13.32F }; // in
            //                                              0      1       2       3       4     
            float[] minimumThinnedTopHeight = new float[] { 92.9F, 101.4F, 110.4F, 118.9F, 126.7F }; // ft
            float[] minimumThinnedVolume;
            if (thinnedTrajectory.UseScaledVolume)
            {
                minimumThinnedVolume = new float[] { 51.59F, 49.67F, 64.91F, 80.38F, 95.08F }; // Poudel 2018 + Scribner long log net MBF/ha
            }
            else
            {
                //                                   0       1       2       3       4     
                minimumThinnedVolume = new float[] { 43.74F, 46.59F, 65.78F, 84.94F, 103.1F }; // Browning 1977 (FIA) MBF/ha
            }

            this.Verify(thinnedTrajectory, minimumThinnedQmd, minimumThinnedTopHeight, minimumThinnedVolume, thinPeriod, lastPeriod, 65, 70, configuration.Variant.TimeStepInYears);
            this.Verify(thinnedTrajectory, minimumThinnedVolume, thinPeriod);
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
            int thinningPeriod = 4;
            int runs = 4; // 1 warmup run + measured runs
            int trees = 300;
            #if DEBUG
            runs = 1; // do only functional validation of test on DEBUG builds to reduce test execution time
            trees = 10;
            #endif

            PlotWithHeight nelder = this.GetNelder();
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(new OrganonVariantNwo());
            configuration.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, trees);

            Objective landExpectationValue = new Objective()
            {
                IsLandExpectationValue = true,
                PlanningPeriods = 9
            };
            HeuristicParameters defaultParameters = new HeuristicParameters();

            TimeSpan runtime = TimeSpan.Zero;
            for (int run = 0; run < runs; ++run)
            {
                // after warmup: 3 runs * 300 trees = 900 measured growth simulations on i7-3770 (4th gen, Sandy Bridge)
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
            this.TestContext.WriteLine(runtime.TotalSeconds.ToString());
        }

        private void Verify(GeneticAlgorithm genetic)
        {
            this.Verify((Heuristic)genetic);

            PopulationStatistics statistics = genetic.PopulationStatistics;
            Assert.IsTrue(statistics.Generations <= genetic.MaximumGenerations);
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
                Assert.IsTrue((newIndividuals >= 0) && (newIndividuals <= 2 * genetic.PopulationSize)); // two children per breeding
                Assert.IsTrue((polymorphism >= 0.0F) && (polymorphism <= 1.0F));
            }
        }

        private void Verify(Heuristic heuristic)
        {
            // check objective functions
            float beginObjectiveFunction = heuristic.CandidateObjectiveFunctionByMove.First();
            float recalculatedBestObjectiveFunction = heuristic.GetObjectiveFunction(heuristic.BestTrajectory);
            float bestObjectiveFunctionRatio = heuristic.BestObjectiveFunction / recalculatedBestObjectiveFunction;
            if (heuristic.Objective.IsLandExpectationValue)
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction > -0.68F);
            }
            else
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction > 0.0F);
            }
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
                    Assert.IsTrue(heuristic.BestTrajectory.ThinningVolume.ScribnerTotal[periodIndex] <= heuristic.BestTrajectory.StandingVolume.ScribnerTotal[periodIndex - 1]);
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
                    Assert.IsTrue(heuristic.CurrentTrajectory.ThinningVolume.ScribnerTotal[periodIndex] <= heuristic.CurrentTrajectory.StandingVolume.ScribnerTotal[periodIndex - 1]);
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

            IHeuristicMoveLog moveLog = heuristic.GetMoveLog();
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
                HeuristicParameters parameters = heuristic.GetParameters();
            if (parameters != null)
            {
                string csvHeader = parameters.GetCsvHeader();
                string csvValues = parameters.GetCsvValues();

                Assert.IsTrue(String.IsNullOrWhiteSpace(csvHeader) == false);
                Assert.IsTrue(String.IsNullOrWhiteSpace(csvValues) == false);
            }
        }

        private void Verify(OrganonStandTrajectory thinnedTrajectory, float[] minimumThinnedVolumeScribner, int thinPeriod)
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

        private void Verify(OrganonStandTrajectory trajectory, float[] minimumQmd, float[] minimumTopHeight, float[] minimumStandingVolumeScribner, int thinPeriod, int lastPeriod, int minTrees, int maxTrees, int timeStepInYears)
        {
            Assert.IsTrue(trajectory.BasalAreaRemoved.Length == lastPeriod + 1);
            Assert.IsTrue(trajectory.BasalAreaRemoved[0] == 0.0F);
            Assert.IsTrue(trajectory.HarvestPeriods == thinPeriod + 1); // BUGBUG: clean off by one semantic
            Assert.IsTrue(trajectory.ThinningVolume.ScribnerTotal[0] == 0.0F);
            Assert.IsTrue(trajectory.ThinningVolume.ScribnerTotal.Length == lastPeriod + 1);
            this.Verify(trajectory.IndividualTreeSelectionBySpecies, thinPeriod, minTrees, maxTrees);
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

                OrganonStand stand = trajectory.StandByPeriod[periodIndex];
                float qmd = stand.GetQuadraticMeanDiameter();
                float topHeight = stand.GetTopHeight();
                int treeRecords = stand.GetTreeRecordCount();

                Assert.IsTrue(stand.Name.StartsWith(trajectory.Name));
                Assert.IsTrue(qmd > minimumQmd[periodIndex]);
                Assert.IsTrue(qmd < qmdTolerance * minimumQmd[periodIndex]);
                Assert.IsTrue(topHeight > minimumTopHeight[periodIndex]);
                Assert.IsTrue(topHeight < topHeightTolerance * minimumTopHeight[periodIndex]);
                Assert.IsTrue(treeRecords > 0);
                Assert.IsTrue(treeRecords < 666);

                // TODO: check qmd against QMD from basal area
            }
        }

        private void Verify(SortedDictionary<FiaCode, int[]> individualTreeSelectionBySpecies, int harvestPeriod, int minimumTreesSelected, int maximumTreesSelected)
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
