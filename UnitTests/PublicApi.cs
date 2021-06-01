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

        private static HeuristicSolutionPosition CreateDefaultSolutionPosition()
        {
            HeuristicSolutionPosition position = new()
            {
                ParameterIndex = 0,
                DiscountRateIndex = 0,
                FirstThinPeriodIndex = 0,
                SecondThinPeriodIndex = 0,
                ThirdThinPeriodIndex = 0,
                PlanningPeriodIndex = 0
            };
            return position;
        }

        private static void AppendObjective(List<float> objectives, Heuristic heuristic, int moveIndex)
        {
            if (heuristic.AcceptedObjectiveFunctionByMove.Count > moveIndex)
            {
                objectives.Add(heuristic.AcceptedObjectiveFunctionByMove[moveIndex]);

                Assert.IsTrue(heuristic.AcceptedObjectiveFunctionByMove[moveIndex] >= heuristic.CandidateObjectiveFunctionByMove[moveIndex]);
            }
        }

        private static HeuristicResultSet<HeuristicParameters> CreateResultsSet(HeuristicParameters parameters, int treeCount, int planningPeriods)
        {
            List<HeuristicParameters> parameterCombinations = new() { parameters };
            List<float> discountRate = new() { Constant.DefaultAnnualDiscountRate };
            List<int> noThin = new() { Constant.NoThinPeriod };
            List<int> planningPeriod = new() { planningPeriods };
            HeuristicResultSet<HeuristicParameters> results = new(parameterCombinations, discountRate, noThin, noThin, noThin, planningPeriod, TestConstant.SolutionPoolSize);

            HeuristicDistribution distribution = new(treeCount)
            {
                DiscountRateIndex = 0,
                FirstThinPeriodIndex = 0,
                SecondThinPeriodIndex = 0,
                ThirdThinPeriodIndex = 0,
                ParameterIndex = 0,
                PlanningPeriodIndex = 0
            };
            results.Distributions.Add(distribution);
            
            return results;
        }

        private static PlotsWithHeight GetNelder()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Malcolm Knapp Nelder 1.xlsx");
            PlotsWithHeight plot = new(new List<int>() { 1 }, 1.327F);
            plot.Read(plotFilePath, "1");
            return plot;
        }

        private static PlotsWithHeight GetPlot14()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Malcolm Knapp plots 14-18+34 Ministry.xlsx");
            PlotsWithHeight plot = new(new List<int>() { 14 }, 4.48F);
            plot.Read(plotFilePath, "0.2 ha");
            return plot;
        }

        [TestMethod]
        public void HuffmanPeakNobleFir()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "HPNF.xlsx");
            PspStand huffmanPeak = new(plotFilePath, "HPNF", 0.2F);
            OrganonVariant variant = new OrganonVariantSwo(); // SWO allows mapping ABAM -> ABGR and ABPR -> ABCO
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
            TestStand stand = huffmanPeak.ToStand(configuration, 80.0F);
            int startYear = 1980;
            stand.WriteCompetitionAsCsv("HPNF initial competition.csv", variant, startYear);
            OrganonTest.GrowPspStand(huffmanPeak, stand, variant, startYear, 2015, Path.GetFileNameWithoutExtension(plotFilePath));

            TreeQuantiles measuredQuantiles = new(stand, huffmanPeak, startYear);
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
            float minObjectiveFunctionWithScaledVolume = 4.593F; // USk$/ha
            #if DEBUG
            treeCount = 48;
            // minObjectiveFunctionWithScaledVolume = ; // USk$/ha, nearest 1 cm diameter class and 0.5 cm height class
            minObjectiveFunctionWithScaledVolume = 2.482F; // bilinear interpolation: 1 cm diameter classes, 1 m height classes
            // minObjectiveFunctionWithScaledVolume = ; // bilinear interpolation: 2 cm diameter classes, 2 m height classes
            // minObjectiveFunctionWithScaledVolume = ; // bilinear interpolation: 5 cm diameter classes, 5 m height classes
            #endif

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = new(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, treeCount);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            RunParameters landExpectationValue = new(configuration)
            {
                PlanningPeriods = 9
            };
            landExpectationValue.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));

            HeuristicResultSet<HeuristicParameters> results = PublicApi.CreateResultsSet(new(), stand.GetTreeRecordCount(), landExpectationValue.PlanningPeriods);
            Hero hero = new(stand, results.ParameterCombinations[0], landExpectationValue)
            {
                //IsStochastic = true,
                MaximumIterations = 10
            };
            // debugging note: it can be helpful to set fully greedy heuristic parameters so the initial prescription remains as no trees selected
            //hero.CurrentTrajectory.SetTreeSelection(0, thinningPeriod);
            HeuristicPerformanceCounters heroCounters = hero.Run(PublicApi.CreateDefaultSolutionPosition(), results.SolutionIndex);

            this.Verify(hero, heroCounters);

            int[] treeSelection = hero.BestTrajectory.IndividualTreeSelectionBySpecies[FiaCode.PseudotsugaMenziesii];
            int treesSelected = treeSelection.Sum() / thinningPeriod;

            this.TestContext!.WriteLine("best objective: {0} observed, near {1} expected", hero.BestObjectiveFunction, minObjectiveFunctionWithScaledVolume);
            Assert.IsTrue(hero.BestObjectiveFunction > minObjectiveFunctionWithScaledVolume);
            Assert.IsTrue(hero.BestObjectiveFunction < 1.02F * minObjectiveFunctionWithScaledVolume);
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
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, treeCount);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            // heuristics optimizing for LEV
            RunParameters runForLandExpectationValue = new(configuration)
            {
                PlanningPeriods = 9
            };
            runForLandExpectationValue.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            HeuristicResultSet<HeuristicParameters> levResults = PublicApi.CreateResultsSet(new(), stand.GetTreeRecordCount(), runForLandExpectationValue.PlanningPeriods);
            HeuristicDistribution levDistribution = levResults.Distributions[0]; 
            int levSolutionsAccepted = 0;
            HeuristicPerformanceCounters totalCounters = new();

            GeneticParameters geneticParameters = new(treeCount)
            {
                PopulationSize = 7,
                MaximumGenerations = 5,
            };
            GeneticAlgorithm genetic = new(stand, geneticParameters, runForLandExpectationValue);
            HeuristicPerformanceCounters geneticCounters = genetic.Run(levDistribution, levResults.SolutionIndex);
            totalCounters += geneticCounters;
            levDistribution.AddRun(genetic, geneticCounters, levResults.ParameterCombinations[0]);
            if (levResults.SolutionIndex[levDistribution].TryAddOrReplace(genetic))
            {
                ++levSolutionsAccepted;
            }

            GreatDeluge deluge = new(stand, levResults.ParameterCombinations[0], runForLandExpectationValue)
            {
                RainRate = 5,
                LowerWaterAfter = 9,
                StopAfter = 10
            };
            HeuristicPerformanceCounters delugeCounters = deluge.Run(levDistribution, levResults.SolutionIndex);
            totalCounters += delugeCounters;
            levDistribution.AddRun(deluge, delugeCounters, levResults.ParameterCombinations[0]);
            if (levResults.SolutionIndex[levDistribution].TryAddOrReplace(deluge))
            {
                ++levSolutionsAccepted;
            }

            RecordTravel recordTravel = new(stand, levResults.ParameterCombinations[0], runForLandExpectationValue)
            {
                StopAfter = 10
            };
            HeuristicPerformanceCounters recordCounters = recordTravel.Run(levDistribution, levResults.SolutionIndex);
            levDistribution.AddRun(recordTravel, recordCounters, levResults.ParameterCombinations[0]);
            if (levResults.SolutionIndex[levDistribution].TryAddOrReplace(recordTravel))
            {
                ++levSolutionsAccepted;
            }

            SimulatedAnnealing annealer = new(stand, levResults.ParameterCombinations[0], runForLandExpectationValue)
            {
                Iterations = 100
            };
            HeuristicPerformanceCounters annealerCounters = annealer.Run(levDistribution, levResults.SolutionIndex);
            totalCounters += annealerCounters;
            levDistribution.AddRun(annealer, annealerCounters, levResults.ParameterCombinations[0]);
            if (levResults.SolutionIndex[levDistribution].TryAddOrReplace(annealer))
            {
                ++levSolutionsAccepted;
            }

            TabuParameters tabuParameters = new();
            TabuSearch tabu = new(stand, tabuParameters, runForLandExpectationValue)
            {
                Iterations = 7,
                //Jump = 2,
                MaximumTenure = 5
            };
            HeuristicPerformanceCounters tabuCounters = tabu.Run(levDistribution, levResults.SolutionIndex);
            totalCounters += tabuCounters;
            levDistribution.AddRun(tabu, tabuCounters, levResults.ParameterCombinations[0]);
            if (levResults.SolutionIndex[levDistribution].TryAddOrReplace(tabu))
            {
                ++levSolutionsAccepted;
            }

            ThresholdAccepting thresholdAcceptor = new(stand, levResults.ParameterCombinations[0], runForLandExpectationValue);
            thresholdAcceptor.IterationsPerThreshold.Clear();
            thresholdAcceptor.Thresholds.Clear();
            thresholdAcceptor.IterationsPerThreshold.Add(10);
            thresholdAcceptor.Thresholds.Add(1.0F);
            HeuristicPerformanceCounters thresholdCounters = thresholdAcceptor.Run(levDistribution, levResults.SolutionIndex);
            totalCounters += thresholdCounters;
            levDistribution.AddRun(thresholdAcceptor, thresholdCounters, levResults.ParameterCombinations[0]);
            if (levResults.SolutionIndex[levDistribution].TryAddOrReplace(thresholdAcceptor))
            {
                ++levSolutionsAccepted;
            }

            PrescriptionParameters prescriptionParameters = new()
            {
                MaximumIntensity = 60.0F,
                MinimumIntensity = 50.0F,
                DefaultIntensityStepSize = 10.0F,
            };
            runForLandExpectationValue.Treatments.Harvests[0] = new ThinByPrescription(thinningPeriod); // must change from individual tree selection
            PrescriptionEnumeration enumerator = new(stand, runForLandExpectationValue, prescriptionParameters);
            HeuristicPerformanceCounters enumerationCounters = enumerator.Run(levDistribution, levResults.SolutionIndex);
            totalCounters += enumerationCounters;
            levDistribution.AddRun(enumerator, enumerationCounters, levResults.ParameterCombinations[0]);
            if (levResults.SolutionIndex[levDistribution].TryAddOrReplace(enumerator))
            {
                ++levSolutionsAccepted;
            }

            // check retrieval from solution pool
            Assert.IsTrue(levResults.SolutionIndex.TryFindMatchingThinnings(levDistribution, out HeuristicSolutionPool? levEliteSolutions));
            Assert.IsTrue(levEliteSolutions!.SolutionsInPool == Math.Min(levSolutionsAccepted, TestConstant.SolutionPoolSize));
            OrganonStandTrajectory eliteSolution = levEliteSolutions.GetEliteSolution();
            Assert.IsTrue(eliteSolution.EarliestPeriodChangedSinceLastSimulation == runForLandExpectationValue.PlanningPeriods + 1);
            levDistribution.OnRunsComplete();

            // heuristic optimizing for volume
            RunParameters runForVolume = new(configuration)
            {
                PlanningPeriods = runForLandExpectationValue.PlanningPeriods,
                TimberObjective = TimberObjective.ScribnerVolume
            };
            runForVolume.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));

            HeuristicResultSet<HeuristicParameters> volumeResults = PublicApi.CreateResultsSet(new(), stand.GetTreeRecordCount(), runForLandExpectationValue.PlanningPeriods);
            HeuristicDistribution volumeDistribution = volumeResults.Distributions[0];
            AutocorrelatedWalk autocorrelated = new(stand, volumeResults.ParameterCombinations[0], runForVolume)
            {
                Iterations = 4
            };
            HeuristicPerformanceCounters autocorrlatedCounters = autocorrelated.Run(volumeDistribution, volumeResults.SolutionIndex);
            totalCounters += autocorrlatedCounters;
            volumeDistribution.AddRun(autocorrelated, autocorrlatedCounters, volumeResults.ParameterCombinations[0]);
            volumeResults.SolutionIndex[volumeDistribution].TryAddOrReplace(autocorrelated);

            // heuristics assigned to net present value optimization
            this.Verify(genetic, geneticCounters);
            this.Verify(deluge, delugeCounters);
            this.Verify(recordTravel, recordCounters);
            this.Verify(annealer, annealerCounters);
            this.Verify(tabu, tabuCounters);
            this.Verify(thresholdAcceptor, thresholdCounters);
            this.Verify(enumerator, enumerationCounters);

            // heuristic assigned to volume optimization
            this.Verify(autocorrelated, autocorrlatedCounters);

            // verify distribution
            float maxHeuristicLev = new float[] { deluge.BestObjectiveFunction, annealer.BestObjectiveFunction, thresholdAcceptor.BestObjectiveFunction, genetic.BestObjectiveFunction, enumerator.BestObjectiveFunction, recordTravel.BestObjectiveFunction, tabu.BestObjectiveFunction }.Max();
            float maxLevInDistribution = levDistribution.BestObjectiveFunctionBySolution.Max();
            Assert.IsTrue(maxHeuristicLev == maxLevInDistribution);
            Assert.IsTrue(maxHeuristicLev == levEliteSolutions.High!.BestObjectiveFunction);
            Assert.IsTrue(Object.ReferenceEquals(levDistribution.HeuristicParameters, levResults.ParameterCombinations[0]));

            List<float> levObjectives = new(8);
            for (int moveIndex = 0; moveIndex < levDistribution.CountByMove.Count; ++moveIndex)
            {
                PublicApi.AppendObjective(levObjectives, deluge, moveIndex);
                PublicApi.AppendObjective(levObjectives, annealer, moveIndex);
                PublicApi.AppendObjective(levObjectives, thresholdAcceptor, moveIndex);
                PublicApi.AppendObjective(levObjectives, genetic, moveIndex);
                PublicApi.AppendObjective(levObjectives, enumerator, moveIndex);
                PublicApi.AppendObjective(levObjectives, recordTravel, moveIndex);
                PublicApi.AppendObjective(levObjectives, tabu, moveIndex);
                maxHeuristicLev = levObjectives.Max();
                float minHeuristicLev = levObjectives.Min();

                maxLevInDistribution = levDistribution.MaximumObjectiveFunctionByMove[moveIndex];
                float minDistributionObjective = levDistribution.MinimumObjectiveFunctionByMove[moveIndex];
                int distributionRunCount = levDistribution.CountByMove[moveIndex];

                Assert.IsTrue(distributionRunCount > 0);
                Assert.IsTrue(distributionRunCount <= 8);
                Assert.IsTrue(maxLevInDistribution <= maxHeuristicLev);
                Assert.IsTrue(minDistributionObjective >= minHeuristicLev);
                Assert.IsTrue(levDistribution.MeanObjectiveFunctionByMove[moveIndex] >= minHeuristicLev);
                Assert.IsTrue(levDistribution.MeanObjectiveFunctionByMove[moveIndex] <= maxHeuristicLev);
                Assert.IsTrue(levDistribution.MedianObjectiveFunctionByMove[moveIndex] >= minHeuristicLev);
                Assert.IsTrue(levDistribution.MedianObjectiveFunctionByMove[moveIndex] <= maxHeuristicLev);
                // not enough heuristic runs to check percentiles
                Assert.IsTrue(distributionRunCount == levDistribution.ObjectiveFunctionValuesByMove[moveIndex].Count);

                levObjectives.Clear();
            }
        }

        [TestMethod]
        public void NelderTrajectory()
        {
            int expectedUnthinnedTreeRecordCount = 661;
            int lastPeriod = 9;

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            OrganonStandTrajectory unthinnedTrajectory = new(stand, configuration, TimberValue.Default, lastPeriod);
            int unthinnedTimeSteps = unthinnedTrajectory.Simulate();
            Assert.IsTrue(unthinnedTimeSteps == lastPeriod);

            int firstThinPeriod = 3;
            ThinByPrescription firstThinPrescription = new(firstThinPeriod)
            {
                FromAbovePercentage = 20.0F, // by basal area
                ProportionalPercentage = 15.0F,
                FromBelowPercentage = 10.0F
            };
            OrganonStandTrajectory oneThinTrajectory = new(unthinnedTrajectory); // cover caching of previously simulated timesteps
            oneThinTrajectory.Treatments.Harvests.Add(firstThinPrescription);
            Assert.IsTrue(oneThinTrajectory.StandByPeriod[0]!.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            int oneThinTimeSteps = oneThinTrajectory.Simulate();
            Assert.IsTrue(oneThinTimeSteps == lastPeriod - firstThinPeriod + 1);

            int secondThinPeriod = 6;
            ThinByPrescription secondThinPrescription = new(secondThinPeriod)
            {
                FromAbovePercentage = 0.0F,
                ProportionalPercentage = 20.0F,
                FromBelowPercentage = 0.0F
            };
            OrganonStandTrajectory twoThinTrajectory = new(stand, configuration, TimberValue.Default, lastPeriod);
            twoThinTrajectory.Treatments.Harvests.Add(firstThinPrescription);
            twoThinTrajectory.Treatments.Harvests.Add(secondThinPrescription);
            AssertNullable.IsNotNull(twoThinTrajectory.StandByPeriod[0]);
            Assert.IsTrue(twoThinTrajectory.StandByPeriod[0]!.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            int twoThinTimeSteps = twoThinTrajectory.Simulate(); // resimulate as cross check on unthinned trajectory
            Assert.IsTrue(twoThinTimeSteps == lastPeriod);

            int thirdThinPeriod = 8;
            OrganonStandTrajectory threeThinTrajectory = new(twoThinTrajectory);  // cover caching of previously simulated timesteps and thinning state
            threeThinTrajectory.Treatments.Harvests.Add(new ThinByPrescription(thirdThinPeriod)
            {
                FromAbovePercentage = 0.0F,
                ProportionalPercentage = 10.0F,
                FromBelowPercentage = 10.0F
            });
            AssertNullable.IsNotNull(threeThinTrajectory.StandByPeriod[0]);
            Assert.IsTrue(threeThinTrajectory.StandByPeriod[0]!.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            int threeThinTimeSteps = threeThinTrajectory.Simulate();
            Assert.IsTrue(threeThinTimeSteps == lastPeriod - thirdThinPeriod + 1);

            // verify unthinned trajectory
            // find/replace regular expression for cleaning up watch window copy/paste: \s+\w+\.\w+\[\d+\]\.\w+\(\)\s+(\d+.\d{1,2})\d*\s+float\r?\n -> $1F, 
            //                                          0       1       2       3       4       5       6       7       8       9
            float[] minimumUnthinnedQmd = new float[] { 16.84F, 20.97F, 24.63F, 27.94F, 31.02F, 33.98F, 36.88F, 39.74F, 42.60F, 45.44F }; // cm
            //                                                0       1       2       3       4       5       6       7       8       9
            float[] minimumUnthinnedTopHeight = new float[] { 16.50F, 20.69F, 24.50F, 27.94F, 31.08F, 33.94F, 36.56F, 38.96F, 41.17F, 43.21F }; // m
            // Poudel 2018 + Scribner long log net MBF/ha
            // nearest 1 cm diameter class and 0.5 m height class
            ///minimumUnthinnedVolume = new float[] { 9.758F, 19.01F, 31.07F, 47.24F, 62.21F, 75.09F, 89.64F, 103.7F, 116.5F, 128.9F };
            // bilinear interpolation
            // minimumUnthinnedVolume = new float[] { 9.669F, 18.93F, 31.07F, 47.22F, 62.24F, 75.22F, 89.55F, 103.8F, 116.7F, 128.8F }; // 0.5 cm diameter classes, 1 m height classes
            float[] minimumUnthinnedStandingVolume = new float[] { 9.648F, 18.88F, 31.01F, 47.24F, 62.33F, 75.18F, 89.45F, 103.8F, 116.8F, 128.7F }; // 1 cm diameter classes, 1 m height classes
            // minimumUnthinnedVolume = new float[] { 9.433F, 18.50F, 30.67F, 46.98F, 61.97F, 75.17F, 89.22F, 103.9F, 116.8F, 128.7F }; // 2 cm diameter classes, 2 m height classes
            // minimumUnthinnedVolume = new float[] { 9.788F, 18.84F, 32.06F, 48.27F, 63.40F, 76.92F, 90.37F, 104.7F, 117.5F, 129.0F }; // 5 cm diameter classes, 5 m height classes
            float[] minimumUnthinnedHarvestVolume = new float[minimumUnthinnedStandingVolume.Length];

            foreach (Stand? unthinnedStand in unthinnedTrajectory.StandByPeriod)
            {
                AssertNullable.IsNotNull(unthinnedStand);
                Assert.IsTrue(unthinnedStand.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            }

            PublicApi.Verify(unthinnedTrajectory, minimumUnthinnedQmd, minimumUnthinnedTopHeight, minimumUnthinnedStandingVolume, minimumUnthinnedHarvestVolume, Constant.NoThinPeriod, Constant.NoThinPeriod, Constant.NoThinPeriod, lastPeriod, 0, 0, configuration.Variant.TimeStepInYears);

            // verify one thin trajectory
            //                                        0       1       2       3       4       5       6       7       8       9
            float[] minimumOneThinQmd = new float[] { 16.84F, 20.97F, 24.63F, 30.23F, 34.49F, 38.11F, 41.35F, 44.31F, 47.06F, 49.67F }; // cm
            //                                              0       1       2       3       4       5       6       7       8       9
            float[] minimumOneThinTopHeight = new float[] { 16.50F, 20.69F, 24.50F, 26.95F, 30.02F, 32.94F, 35.64F, 38.12F, 40.40F, 42.51F }; // ft
            // Poudel 2018 + Scribner long log net MBF/ha
            // nearest 1 cm diameter class and 0.5 m height class
            // minimumThinnedVolume = new float[] { 9.758F, 19.01F, 31.07F, 28.25F, 41.77F, 54.37F, 68.44F, 85.10F, 100.4F, 114.7F };
            // minimumOneThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }; // TODO
            // bilinear interpolation
            // minimumThinnedVolume = new float[] { 9.669F, 18.93F, 31.07F, 28.22F, 41.68F, 54.31F, 68.33F, 84.91F, 100.5F, 114.6F }; // 1 cm diameter classes, 0.5 m height classes
            // minimumOneThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F };  // TODO
            float[] minimumOneThinStandingVolume = new float[] { 9.648F, 18.88F, 31.01F, 28.13F, 41.60F, 54.34F, 68.41F, 84.91F, 100.4F, 114.6F }; // 1 cm diameter classes, 1 m height classes
            float[] minimumOneThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 15.14F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F };
            // minimumThinnedVolume = new float[] { 9.433F, 18.50F, 30.67F, 28.05F, 41.55F, 54.22F, 68.39F, 85.09F, 100.0F, 114.7F }; // 2 cm diameter classes, 2 m height classes
            // minimumOneThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F };  // TODO
            // minimumThinnedVolume = new float[] { 9.788F, 18.84F, 32.06F, 29.34F, 42.72F, 55.94F, 69.84F, 85.99F, 101.3F, 115.0F }; // 5 cm diameter classes, 5 m height classes
            // minimumOneThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F };  // TODO

            for (int periodIndex = 0; periodIndex < firstThinPeriod; ++periodIndex)
            {
                Stand? unthinnedStand = oneThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(unthinnedStand);
                Assert.IsTrue(unthinnedStand.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            }
            int expectedFirstThinTreeRecordCount = 328; // must be updated if prescription changes
            for (int periodIndex = firstThinPeriod; periodIndex < oneThinTrajectory.PlanningPeriods; ++periodIndex)
            {
                Stand? thinnedStand = oneThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(thinnedStand);
                Assert.IsTrue(thinnedStand.GetTreeRecordCount() == expectedFirstThinTreeRecordCount);
            }

            PublicApi.Verify(oneThinTrajectory, minimumOneThinQmd, minimumOneThinTopHeight, minimumOneThinStandingVolume, minimumOneThinHarvestVolume, firstThinPeriod, Constant.NoThinPeriod, Constant.NoThinPeriod, lastPeriod, 200, 400, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(oneThinTrajectory, minimumOneThinStandingVolume, firstThinPeriod, null, null);

            // verify two thin trajectory
            //                                        0       1       2       3       4       5       6       7       8       9
            float[] minimumTwoThinQmd = new float[] { 16.84F, 20.97F, 24.63F, 30.23F, 34.49F, 38.11F, 41.92F, 45.34F, 48.40F, 51.22F }; // cm
            //                                              0       1       2       3       4       5       6       7       8       9
            float[] minimumTwoThinTopHeight = new float[] { 16.50F, 20.69F, 24.50F, 26.95F, 30.02F, 32.94F, 35.12F, 37.58F, 39.89F, 42.04F }; // ft
            float[] minimumTwoThinLiveBiomass = new float[] { 85531F, 146983F, 213170F, 168041F, 226421F, 283782F, 278175F, 329697F, 378050F, 422956F }; // kg/ha
            // Poudel 2018 + Scribner long log net MBF/ha
            // nearest 1 cm diameter class and 0.5 m height class
            // minimumThinnedVolume = new float[] { 9.758F, 19.01F, 31.07F, 28.25F, 41.77F, 54.37F,  };
            // bilinear interpolation
            // minimumThinnedVolume = new float[] { 9.669F, 18.93F, 31.07F, 28.22F, 41.68F, 54.31F,  }; // 1 cm diameter classes, 0.5 m height classes
            // minimumTwoThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }; // TODO
            float[] minimumTwoThinStandingVolume = new float[] { 9.648F, 18.88F, 31.01F, 28.13F, 41.60F, 54.34F, 55.84F, 71.49F, 86.87F, 101.5F }; // 1 cm diameter classes, 1 m height classes
            float[] minimumTwoThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 15.14F, 0.0F, 0.0F, 12.360F, 0.0F, 0.0F, 0.0F };
            // minimumThinnedVolume = new float[] { 9.433F, 18.50F, 30.67F, 28.05F, 41.55F, 54.22F,  }; // 2 cm diameter classes, 2 m height classes
            // minimumTwoThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }; // TODO
            // minimumThinnedVolume = new float[] { 9.788F, 18.84F, 32.06F, 29.34F, 42.72F, 55.94F,  }; // 5 cm diameter classes, 5 m height classes
            // minimumTwoThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }; // TODO

            for (int periodIndex = 0; periodIndex < firstThinPeriod; ++periodIndex)
            {
                Stand? unthinnedStand = twoThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(unthinnedStand);
                Assert.IsTrue(unthinnedStand.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            }
            for (int periodIndex = firstThinPeriod; periodIndex < secondThinPeriod; ++periodIndex)
            {
                Stand? thinnedStand = twoThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(thinnedStand);
                Assert.IsTrue(thinnedStand.GetTreeRecordCount() == expectedFirstThinTreeRecordCount);
            }
            int expectedSecondThinTreeRecordCount = 263; // must be updated if prescription changes
            for (int periodIndex = secondThinPeriod; periodIndex < twoThinTrajectory.PlanningPeriods; ++periodIndex)
            {
                Stand? thinnedStand = twoThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(thinnedStand);
                Assert.IsTrue(thinnedStand.GetTreeRecordCount() == expectedSecondThinTreeRecordCount);
            }

            PublicApi.Verify(twoThinTrajectory, minimumTwoThinQmd, minimumTwoThinTopHeight, minimumTwoThinStandingVolume, minimumTwoThinHarvestVolume, firstThinPeriod, secondThinPeriod, Constant.NoThinPeriod, lastPeriod, 200, 400, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(twoThinTrajectory, minimumTwoThinStandingVolume, firstThinPeriod, secondThinPeriod, null);

            float biomassTolerance = 1.01F;
            for (int periodIndex = 0; periodIndex < twoThinTrajectory.PlanningPeriods; ++periodIndex)
            {
                float liveBiomass = twoThinTrajectory.StandByPeriod[periodIndex]!.GetLiveBiomass();
                Assert.IsTrue(liveBiomass > minimumTwoThinLiveBiomass[periodIndex]);
                Assert.IsTrue(liveBiomass < biomassTolerance * minimumTwoThinLiveBiomass[periodIndex]);
            }

            // verify three thin trajectory
            //                                          0       1       2       3       4       5       6       7       8       9
            float[] minimumThreeThinQmd = new float[] { 16.84F, 20.97F, 24.63F, 30.23F, 34.49F, 38.11F, 41.92F, 45.34F, 51.91F, 55.19F }; // cm
            //                                                0       1       2       3       4       5       6       7       8       9
            float[] minimumThreeThinTopHeight = new float[] { 16.50F, 20.69F, 24.50F, 26.95F, 30.02F, 32.94F, 35.12F, 37.58F, 39.42F, 41.54F }; // ft
            // Poudel 2018 + Scribner long log net MBF/ha
            // bilinear interpolation
            float[] minimumThreeThinStandingVolume = new float[] { 9.648F, 18.88F, 31.01F, 28.13F, 41.60F, 54.34F, 55.84F, 71.49F, 71.01F, 85.07F }; // 1 cm diameter classes, 1 m height classes
            float[] minimumThreeThinHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 15.14F, 0.0F, 0.0F, 12.360F, 0.0F, 15.787F, 0.0F };

            for (int periodIndex = 0; periodIndex < firstThinPeriod; ++periodIndex)
            {
                Stand? unthinnedStand = threeThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(unthinnedStand);
                Assert.IsTrue(unthinnedStand.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            }
            for (int periodIndex = firstThinPeriod; periodIndex < secondThinPeriod; ++periodIndex)
            {
                Stand? thinnedStand = threeThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(thinnedStand);
                Assert.IsTrue(thinnedStand.GetTreeRecordCount() == expectedFirstThinTreeRecordCount);
            }
            for (int periodIndex = secondThinPeriod; periodIndex < thirdThinPeriod; ++periodIndex)
            {
                Stand? thinnedStand = threeThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(thinnedStand);
                Assert.IsTrue(thinnedStand.GetTreeRecordCount() == expectedSecondThinTreeRecordCount);
            }
            int expectedThirdThinTreeRecordCount = 180; // must be updated if prescription changes
            for (int periodIndex = thirdThinPeriod; periodIndex < threeThinTrajectory.PlanningPeriods; ++periodIndex)
            {
                Stand? thinnedStand = threeThinTrajectory.StandByPeriod[periodIndex];
                AssertNullable.IsNotNull(thinnedStand);
                Assert.IsTrue(thinnedStand.GetTreeRecordCount() == expectedThirdThinTreeRecordCount);
            }

            PublicApi.Verify(threeThinTrajectory, minimumThreeThinQmd, minimumThreeThinTopHeight, minimumThreeThinStandingVolume, minimumThreeThinHarvestVolume, firstThinPeriod, secondThinPeriod, thirdThinPeriod, lastPeriod, 200, 485, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(threeThinTrajectory, minimumThreeThinStandingVolume, firstThinPeriod, secondThinPeriod, thirdThinPeriod);
        }

        [TestMethod]
        public void OrganonStandGrowthApi()
        {
            TestStand.WriteTreeHeader(this.TestContext!);
            OrganonTreatments treatments = new();
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

                TestStand initialStand = new(stand);
                TreeLifeAndDeath treeGrowth = new();
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonGrowth.Grow(simulationStep + 1, configuration, treatments, stand, calibrationBySpecies);
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

            PlotsWithHeight plot14 = PublicApi.GetPlot14();
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = plot14.ToOrganonStand(configuration, 30, 130.0F);
            stand.PlantingDensityInTreesPerHectare = TestConstant.Plot14ReplantingDensityInTreesPerHectare;

            OrganonStandTrajectory thinnedTrajectory = new(stand, configuration, TimberValue.Default, lastPeriod);
            thinnedTrajectory.Treatments.Harvests.Add(new ThinByPrescription(thinPeriod)
            {
                FromAbovePercentage = 0.0F,
                ProportionalPercentage = 30.0F,
                FromBelowPercentage = 0.0F
            });
            AssertNullable.IsNotNull(thinnedTrajectory.StandByPeriod[0]);
            Assert.IsTrue(thinnedTrajectory.StandByPeriod[0]!.GetTreeRecordCount() == 222);
            int immediateThintimeSteps = thinnedTrajectory.Simulate();
            Assert.IsTrue(immediateThintimeSteps == lastPeriod);

            // verify thinned trajectory
            // find/replace regular expressions: function call \w+\.\w+\[\d+\]\.\w+\(\)\s+(\d+.\d{1,2})\d*\s+float\r?\n
            //                                   array element \[\d+\]\s+(\d+.\d{1,3})\d*\s+float\r?\n -> $1F, 
            //                                 0       1       2       3       4     
            float[] minimumQmd = new float[] { 23.33F, 26.88F, 30.17F, 33.13F, 35.93F }; // cm
            //                                       0       1       2       3       4     
            float[] minimumTopHeight = new float[] { 28.32F, 30.81F, 33.54F, 36.14F, 38.54F }; // m
            // Poudel 2018 + Scribner long log net MBF/ha
            // nearest 1 cm diameter class and 0.5 m height class
            // minimumThinnedVolume = new float[] { 51.59F, 51.75F, 66.71F, 81.88F, 97.72F };
            // minimumHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }; // TODO
            // bilinear interpolation: 
            // minimumThinnedVolume = new float[] { 51.333F, 51.521F, 66.819F, 81.757F, 97.212F }; // 1 cm diameter classes, 0.5 m height classes
            // minimumHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }; // TODO
            float[] minimumStandingVolume = new float[] { 51.244F, 51.561F, 67.151F, 81.817F, 97.327F }; // 1 cm diameter classes, 1 m height classes
            float[] minimumHarvestVolume = new float[] { 0.0F, 15.742F, 0.0F, 0.0F, 0.0F }; // TODO
            // minimumThinnedVolume = new float[] { 50.444F, 51.261F, 66.582F, 81.800F, 97.521F }; // 2 cm diameter classes, 2 m height classes
            // minimumHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }; // TODO
            // minimumThinnedVolume = new float[] { 52.466F, 52.364F, 68.895F, 83.383F, 98.442F }; // 5 cm diameter classes, 5 m height classes
            // minimumHarvestVolume = new float[] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }; // TODO

            PublicApi.Verify(thinnedTrajectory, minimumQmd, minimumTopHeight, minimumStandingVolume, minimumHarvestVolume, thinPeriod, Constant.NoThinPeriod, Constant.NoThinPeriod, lastPeriod, 65, 70, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(thinnedTrajectory, minimumStandingVolume, thinPeriod, null, null);
            Assert.IsTrue(thinnedTrajectory.GetFirstThinAge() == 30);
            Assert.IsTrue(thinnedTrajectory.StandByPeriod[^1]!.GetTreeRecordCount() == 156);

            // verify snag and log calculations
            SnagLogTable snagsAndLogs = new(thinnedTrajectory, Constant.Bucking.DefaultMaximumDiameterInCentimeters, Constant.Bucking.DiameterClassSizeInCentimeters);
            PublicApi.Verify(snagsAndLogs, thinnedTrajectory);
        }

        [TestMethod]
        public void RS39()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "RS39 lower half.xlsx");
            PspStand rs39 = new(plotFilePath, "RS39 lower half", 0.154441F);
            OrganonVariant variant = new OrganonVariantNwo();
            OrganonConfiguration configuration = new(variant);
            TestStand stand = rs39.ToStand(configuration, 105.0F);
            int startYear = 1992;
            stand.WriteCompetitionAsCsv("RS39 lower half initial competition.csv", variant, startYear);
            OrganonTest.GrowPspStand(rs39, stand, variant, startYear, 2019, Path.GetFileNameWithoutExtension(plotFilePath));

            TreeQuantiles measuredQuantiles = new(stand, rs39, startYear);
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
            OrganonStand stand = nelder.ToOrganonStand(configuration, 20, 130.0F, trees);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            RunParameters landExpectationValue = new(configuration)
            {
                PlanningPeriods = 9
            };
            landExpectationValue.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            HeuristicResultSet<HeuristicParameters> results = PublicApi.CreateResultsSet(new(), stand.GetTreeRecordCount(), landExpectationValue.PlanningPeriods);
            HeuristicSolutionPosition defaultPosition = PublicApi.CreateDefaultSolutionPosition();

            TimeSpan runtime = TimeSpan.Zero;
            for (int run = 0; run < runs; ++run)
            {
                // after warmup: 3 runs * 300 trees = 900 measured growth simulations on i7-3770 (4th gen, Sandy Bridge)
                // dispersion of 5 runs                   min   mean  median  max
                // .NET 5.0 with removed tree compaction  1.67  1.72  1.72    1.82
                Hero hero = new(stand, results.ParameterCombinations[0], landExpectationValue)
                {
                    IsStochastic = false,
                    MaximumIterations = 2
                };
                if (run > 0)
                {
                    // skip first run as a warmup run
                    HeuristicPerformanceCounters heroCounters = hero.Run(defaultPosition, results.SolutionIndex);
                    runtime += heroCounters.Duration;
                }
            }
            this.TestContext!.WriteLine(runtime.TotalSeconds.ToString());
        }

        private void Verify(GeneticAlgorithm genetic, HeuristicPerformanceCounters perfCounters)
        {
            this.Verify((Heuristic<GeneticParameters>)genetic, perfCounters);

            PopulationStatistics statistics = genetic.PopulationStatistics;
            Assert.IsTrue(statistics.Generations <= genetic.HeuristicParameters.MaximumGenerations);
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
                Assert.IsTrue((newIndividuals >= 0) && (newIndividuals <= 2 * genetic.HeuristicParameters.PopulationSize)); // two children per breeding
                Assert.IsTrue((polymorphism >= 0.0F) && (polymorphism <= 1.0F));
            }
        }

        private void Verify<TParameters>(Heuristic<TParameters> heuristic, HeuristicPerformanceCounters perfCounters) where TParameters : HeuristicParameters
        {
            // check objective functions
            Assert.IsTrue(heuristic.BestTrajectory.PlantingDensityInTreesPerHectare == heuristic.CurrentTrajectory.PlantingDensityInTreesPerHectare);
            Assert.IsTrue(heuristic.BestTrajectory.PlantingDensityInTreesPerHectare >= TestConstant.NelderReplantingDensityInTreesPerHectare);

            float recalculatedBestObjectiveFunction = heuristic.GetObjectiveFunction(heuristic.BestTrajectory);
            float bestObjectiveFunctionRatio = heuristic.BestObjectiveFunction / recalculatedBestObjectiveFunction;
            this.TestContext!.WriteLine("best objective: {0}", heuristic.BestObjectiveFunction);
            if (heuristic.RunParameters.TimberObjective == TimberObjective.LandExpectationValue)
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction > -0.70F);
            }
            else
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction > 0.0F);
            }

            float beginObjectiveFunction = heuristic.CandidateObjectiveFunctionByMove.First();
            Assert.IsTrue(heuristic.BestObjectiveFunction >= beginObjectiveFunction);

            Assert.IsTrue(heuristic.AcceptedObjectiveFunctionByMove.Count >= 3);
            Assert.IsTrue(bestObjectiveFunctionRatio > 0.99999);
            Assert.IsTrue(bestObjectiveFunctionRatio < 1.00001);

            float recalculatedCurrentObjectiveFunction = heuristic.GetObjectiveFunction(heuristic.CurrentTrajectory);
            Assert.IsTrue(recalculatedCurrentObjectiveFunction <= heuristic.BestObjectiveFunction);

            // only guaranteed for monotonic heuristics: hero, prescription enumeration, others depending on configuration
            if ((heuristic is Hero) || (heuristic is PrescriptionEnumeration) || (heuristic is ThresholdAccepting))
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction == heuristic.AcceptedObjectiveFunctionByMove[^1]);
                Assert.IsTrue(recalculatedCurrentObjectiveFunction >= beginObjectiveFunction);
            }
            else
            {
                Assert.IsTrue(heuristic.BestObjectiveFunction > 0.95F * heuristic.AcceptedObjectiveFunctionByMove[^1]);
                if (heuristic is AutocorrelatedWalk)
                {
                    Assert.IsTrue(recalculatedCurrentObjectiveFunction > 0.5F * beginObjectiveFunction);
                }
                else if (heuristic is SimulatedAnnealing)
                {
                    Assert.IsTrue(recalculatedCurrentObjectiveFunction > 0.75F * beginObjectiveFunction);
                }
                else
                {
                    Assert.IsTrue(recalculatedCurrentObjectiveFunction > 0.85F * beginObjectiveFunction);
                }
            }

            float endObjectiveFunction = heuristic.CandidateObjectiveFunctionByMove.Last();
            Assert.IsTrue(endObjectiveFunction <= heuristic.BestObjectiveFunction);

            // check harvest schedule
            int firstThinningPeriod = heuristic.BestTrajectory.GetFirstThinPeriod(); // returns -1 if heuristic selects no trees
            if (firstThinningPeriod == Constant.NoThinPeriod)
            {
                firstThinningPeriod = heuristic.BestTrajectory.Treatments.GetValidThinningPeriods()[1];
            }
            foreach (KeyValuePair<FiaCode, int[]> selectionForSpecies in heuristic.BestTrajectory.IndividualTreeSelectionBySpecies)
            {
                int[] bestTreeSelection = selectionForSpecies.Value;
                int[] currentTreeSelection = heuristic.CurrentTrajectory.IndividualTreeSelectionBySpecies[selectionForSpecies.Key];
                Assert.IsTrue(bestTreeSelection.Length == currentTreeSelection.Length);
                for (int treeIndex = 0; treeIndex < bestTreeSelection.Length; ++treeIndex)
                {
                    Assert.IsTrue((bestTreeSelection[treeIndex] == Constant.NoHarvestPeriod) || (bestTreeSelection[treeIndex] == firstThinningPeriod));
                    Assert.IsTrue((currentTreeSelection[treeIndex] == Constant.NoHarvestPeriod) || (currentTreeSelection[treeIndex] == firstThinningPeriod));
                }
            }

            // check volumes
            Assert.IsTrue((firstThinningPeriod == Constant.NoThinPeriod) || (firstThinningPeriod > 0));
            heuristic.BestTrajectory.GetGradedVolumes(out StandCubicAndScribnerVolume bestStandingVolume, out StandCubicAndScribnerVolume bestHarvestedVolume);
            heuristic.CurrentTrajectory.GetGradedVolumes(out StandCubicAndScribnerVolume currentStandingVolume, out StandCubicAndScribnerVolume currentHarvestedVolume);
            float previousBestCubicStandingVolume = Single.NaN;
            float previousCurrentCubicStandingVolume = Single.NaN;
            for (int periodIndex = 0; periodIndex < heuristic.BestTrajectory.PlanningPeriods; ++periodIndex)
            {
                float bestCubicStandingVolume = bestStandingVolume.GetCubicTotal(periodIndex);
                float bestCubicThinningVolume = bestHarvestedVolume.GetCubicTotal(periodIndex);
                float currentCubicStandingVolume = currentStandingVolume.GetCubicTotal(periodIndex);
                float currentCubicThinningVolume = currentHarvestedVolume.GetCubicTotal(periodIndex);

                float bestCubicStandingCheckVolume = bestStandingVolume.Cubic2Saw[periodIndex] + bestStandingVolume.Cubic3Saw[periodIndex] + bestStandingVolume.Cubic4Saw[periodIndex];
                float bestCubicThinningCheckVolume = bestHarvestedVolume.Cubic2Saw[periodIndex] + bestHarvestedVolume.Cubic3Saw[periodIndex] + bestHarvestedVolume.Cubic4Saw[periodIndex];
                float currentCubicStandingCheckVolume = currentStandingVolume.Cubic2Saw[periodIndex] + currentStandingVolume.Cubic3Saw[periodIndex] + currentStandingVolume.Cubic4Saw[periodIndex];
                float currentCubicThinningCheckVolume = currentHarvestedVolume.Cubic2Saw[periodIndex] + currentHarvestedVolume.Cubic3Saw[periodIndex] + currentHarvestedVolume.Cubic4Saw[periodIndex];
                Assert.IsTrue(MathF.Abs(bestCubicStandingVolume - bestCubicStandingCheckVolume) < 0.000001F);
                Assert.IsTrue(MathF.Abs(bestCubicThinningVolume - bestCubicThinningCheckVolume) < 0.000001F);
                Assert.IsTrue(MathF.Abs(currentCubicStandingVolume - currentCubicStandingCheckVolume) < 0.000001F);
                Assert.IsTrue(MathF.Abs(currentCubicThinningVolume - currentCubicThinningCheckVolume) < 0.000001F);

                float bestThinNpv = heuristic.BestTrajectory.GetNetPresentThinningValue(heuristic.RunParameters.DiscountRate, periodIndex, out float bestThin2SawNpv, out float bestThin3SawNpv, out float bestThin4SawNpv);
                float currentThinNpv = heuristic.BestTrajectory.GetNetPresentThinningValue(heuristic.RunParameters.DiscountRate, periodIndex, out float currentThin2SawNpv, out float currentThin3SawNpv, out float currentThin4SawNpv);
                if (periodIndex == firstThinningPeriod)
                {
                    Assert.IsTrue(heuristic.BestTrajectory.BasalAreaRemoved[periodIndex] >= 0.0F); // best selection with debug stand is no harvest
                    Assert.IsTrue(heuristic.BestTrajectory.BasalAreaRemoved[periodIndex] <= 200.0F);

                    Assert.IsTrue(heuristic.BestTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) >= 0.0F);
                    Assert.IsTrue(heuristic.BestTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) < heuristic.BestTrajectory.StandingVolume.GetScribnerTotal(periodIndex - 1) + 0.000001F); // allow for numerical error in case where all trees are harvested
                    Assert.IsTrue(bestHarvestedVolume.Scribner2Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner3Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner4Saw[periodIndex] >= 0.0F);

                    Assert.IsTrue(bestCubicThinningVolume <= previousBestCubicStandingVolume);
                    Assert.IsTrue(bestHarvestedVolume.Cubic2Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Cubic3Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Cubic4Saw[periodIndex] >= 0.0F);

                    Assert.IsTrue(bestThin2SawNpv >= 0.0F);
                    Assert.IsTrue(bestThin3SawNpv >= 0.0F);
                    Assert.IsTrue(bestThin4SawNpv >= 0.0F);

                    Assert.IsTrue(heuristic.CurrentTrajectory.BasalAreaRemoved[periodIndex] >= 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.BasalAreaRemoved[periodIndex] <= 200.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) >= 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) < heuristic.CurrentTrajectory.StandingVolume.GetScribnerTotal(periodIndex - 1) + 0.000001F); // numerical error

                    Assert.IsTrue(currentCubicThinningVolume >= 0.0F);
                    Assert.IsTrue(currentCubicThinningVolume <= previousCurrentCubicStandingVolume);

                    Assert.IsTrue(currentThin2SawNpv >= 0.0F);
                    Assert.IsTrue(currentThin3SawNpv >= 0.0F);
                    Assert.IsTrue(currentThin4SawNpv >= 0.0F);
                }
                else
                {
                    // for now, harvest should occur only in the one indicated period
                    Assert.IsTrue(heuristic.BestTrajectory.BasalAreaRemoved[periodIndex] == 0.0F);
                    Assert.IsTrue(heuristic.BestTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(bestCubicThinningVolume == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Cubic2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Cubic3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Cubic4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(bestThin2SawNpv == 0.0F);
                    Assert.IsTrue(bestThin3SawNpv == 0.0F);
                    Assert.IsTrue(bestThin4SawNpv == 0.0F);

                    Assert.IsTrue(heuristic.CurrentTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Scribner2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Scribner3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Scribner4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(heuristic.CurrentTrajectory.BasalAreaRemoved[periodIndex] == 0.0F);
                    Assert.IsTrue(currentCubicThinningVolume == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Cubic2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Cubic3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Cubic4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(currentThin2SawNpv == 0.0F);
                    Assert.IsTrue(currentThin3SawNpv == 0.0F);
                    Assert.IsTrue(currentThin4SawNpv == 0.0F);
                }

                if (periodIndex == 0)
                {
                    // zero merchantable on Nelder 1 at age 20 with Poudel 2018 net volume
                    Assert.IsTrue(bestCubicStandingVolume >= 0.0F);
                    Assert.IsTrue(heuristic.BestTrajectory.StandingVolume.GetScribnerTotal(periodIndex) >= 0.0F);
                    Assert.IsTrue(currentCubicStandingVolume >= 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.StandingVolume.GetScribnerTotal(periodIndex) >= 0.0F);
                }
                else
                {
                    Assert.IsTrue(bestCubicStandingVolume > 0.0F);
                    Assert.IsTrue(heuristic.BestTrajectory.StandingVolume.GetScribnerTotal(periodIndex) > 0.0F);
                    Assert.IsTrue(currentCubicStandingVolume > 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.StandingVolume.GetScribnerTotal(periodIndex) > 0.0F);

                    if (periodIndex != firstThinningPeriod)
                    {
                        // for now, assume monotonic increase in standing volumes except in harvest periods
                        Assert.IsTrue(bestCubicStandingVolume > previousBestCubicStandingVolume);
                        Assert.IsTrue(heuristic.BestTrajectory.StandingVolume.GetScribnerTotal(periodIndex) > heuristic.BestTrajectory.StandingVolume.GetScribnerTotal(periodIndex - 1));
                        Assert.IsTrue(currentCubicStandingVolume > previousCurrentCubicStandingVolume);
                        Assert.IsTrue(heuristic.CurrentTrajectory.StandingVolume.GetScribnerTotal(periodIndex) > heuristic.CurrentTrajectory.StandingVolume.GetScribnerTotal(periodIndex - 1));
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
                string csvHeader = moveLog.GetCsvHeader("prefix");
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

            // check performance counters
            Assert.IsTrue(perfCounters.Duration > TimeSpan.Zero);
            Assert.IsTrue(perfCounters.GrowthModelTimesteps > 0);
            Assert.IsTrue(perfCounters.MovesAccepted >= 0); // random guessing may or may not yield improving or disimproving moves
            Assert.IsTrue(perfCounters.MovesRejected >= 0);
            Assert.IsTrue((perfCounters.MovesAccepted + perfCounters.MovesRejected) >= 0);
            
            int treeRecordCount = heuristic.BestTrajectory.GetInitialTreeRecordCount();
            if (heuristic is GeneticAlgorithm genetic)
            {
                Assert.IsTrue(perfCounters.TreesRandomizedInConstruction > 0.4F * genetic.HeuristicParameters.PopulationSize * treeRecordCount);
                Assert.IsTrue(perfCounters.TreesRandomizedInConstruction < 0.6F * genetic.HeuristicParameters.PopulationSize * treeRecordCount);
            }
            else if (heuristic is PrescriptionEnumeration)
            {
                Assert.IsTrue(perfCounters.TreesRandomizedInConstruction == 0);
            }
            else
            {
                Assert.IsTrue(perfCounters.TreesRandomizedInConstruction >= 0);
                Assert.IsTrue(perfCounters.TreesRandomizedInConstruction <= treeRecordCount);
            }
        }

        private static void Verify(OrganonStandTrajectory thinnedTrajectory, float[] minimumThinnedVolumeScribner, int firstThinPeriod, int? secondThinPeriod, int? thirdThinPeriod)
        {
            for (int periodIndex = 0; periodIndex < thinnedTrajectory.PlanningPeriods; ++periodIndex)
            {
                if ((periodIndex == firstThinPeriod) || 
                    (secondThinPeriod.HasValue && periodIndex == secondThinPeriod.Value) ||
                    (thirdThinPeriod.HasValue && periodIndex == thirdThinPeriod.Value))
                {
                    Assert.IsTrue(thinnedTrajectory.BasalAreaRemoved[periodIndex] > 0.0F);
                    Assert.IsTrue(thinnedTrajectory.BasalAreaRemoved[periodIndex] < thinnedTrajectory.DensityByPeriod[periodIndex - 1].BasalAreaPerAcre); // assume <50% thin by volume
                    Assert.IsTrue(thinnedTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) > 0.0F);
                    Assert.IsTrue(thinnedTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) < minimumThinnedVolumeScribner[periodIndex]);
                }
                else
                {
                    Assert.IsTrue(thinnedTrajectory.BasalAreaRemoved[periodIndex] == 0.0F);
                    Assert.IsTrue(thinnedTrajectory.ThinningVolume.GetScribnerTotal(periodIndex) == 0.0F);
                }
            }
        }

        private static void Verify(OrganonStandTrajectory trajectory, float[] minimumQmd, float[] minimumTopHeight, float[] minimumStandingVolumeScribner, float[] minimumHarvestVolumeScribner, int firstThinPeriod, int secondThinPeriod, int thirdThinPeriod, int lastPeriod, int minTrees, int maxTrees, int timeStepInYears)
        {
            Assert.IsTrue(trajectory.BasalAreaRemoved.Length == lastPeriod + 1);
            Assert.IsTrue(trajectory.BasalAreaRemoved[0] == 0.0F);
            Assert.IsTrue(trajectory.ThinningVolume.GetScribnerTotal(0) == 0.0F);
            Assert.IsTrue(trajectory.ThinningVolume.Scribner2Saw.Length == lastPeriod + 1);
            Assert.IsTrue(trajectory.ThinningVolume.Scribner3Saw.Length == lastPeriod + 1);
            Assert.IsTrue(trajectory.ThinningVolume.Scribner4Saw.Length == lastPeriod + 1);
            Assert.IsTrue(String.IsNullOrEmpty(trajectory.Name) == false);
            Assert.IsTrue(trajectory.PeriodLengthInYears == timeStepInYears);
            Assert.IsTrue(trajectory.PlanningPeriods == lastPeriod + 1); // BUGBUG: clean off by one semantic

            Assert.IsTrue(trajectory.GetFirstThinPeriod() == firstThinPeriod);
            Assert.IsTrue(trajectory.GetSecondThinPeriod() == secondThinPeriod);

            IList<int> thinningPeriods = trajectory.Treatments.GetValidThinningPeriods();
            Assert.IsTrue(thinningPeriods[0] == Constant.NoHarvestPeriod);
            if (firstThinPeriod != Constant.NoThinPeriod)
            {
                Assert.IsTrue(thinningPeriods[1] == firstThinPeriod);
                if (thirdThinPeriod != Constant.NoThinPeriod)
                {
                    Assert.IsTrue(thinningPeriods[2] == secondThinPeriod);
                    Assert.IsTrue(thinningPeriods[3] == thirdThinPeriod);
                    Assert.IsTrue(thinningPeriods.Count == 4);
                }
                else if (secondThinPeriod != Constant.NoThinPeriod)
                {
                    Assert.IsTrue(thinningPeriods[2] == secondThinPeriod);
                    Assert.IsTrue(thinningPeriods.Count == 3);
                }
                else
                {
                    Assert.IsTrue(thinningPeriods.Count == 2);
                }
            }
            else
            {
                Assert.IsTrue(thinningPeriods.Count == 1);
            }

            PublicApi.Verify(trajectory.IndividualTreeSelectionBySpecies, firstThinPeriod, secondThinPeriod, thirdThinPeriod, minTrees, maxTrees);

            float qmdTolerance = 1.01F;
            float topHeightTolerance = 1.01F;
            float volumeTolerance = 1.01F;
            for (int periodIndex = 0; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
            {
                Assert.IsTrue(trajectory.DensityByPeriod[periodIndex].BasalAreaPerAcre > 0.0F);
                Assert.IsTrue(trajectory.DensityByPeriod[periodIndex].BasalAreaPerAcre <= TestConstant.Maximum.TreeBasalAreaLarger);
                Assert.IsTrue(trajectory.StandingVolume.GetScribnerTotal(periodIndex) > minimumStandingVolumeScribner[periodIndex]);
                Assert.IsTrue(trajectory.StandingVolume.GetScribnerTotal(periodIndex) < volumeTolerance * minimumStandingVolumeScribner[periodIndex]);
                Assert.IsTrue(trajectory.ThinningVolume.GetScribnerTotal(periodIndex) >= minimumHarvestVolumeScribner[periodIndex]);
                Assert.IsTrue(trajectory.ThinningVolume.GetScribnerTotal(periodIndex) <= volumeTolerance * minimumHarvestVolumeScribner[periodIndex]);

                OrganonStand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information missing for period " + periodIndex + ".");
                float qmdInCm = stand.GetQuadraticMeanDiameterInCentimeters();
                float topHeight = stand.GetTopHeightInMeters();
                int treeRecords = stand.GetTreeRecordCount();

                Assert.IsTrue((stand.Name != null) && (trajectory.Name != null) && stand.Name.StartsWith(trajectory.Name));
                Assert.IsTrue(qmdInCm > minimumQmd[periodIndex]);
                Assert.IsTrue(qmdInCm < qmdTolerance * minimumQmd[periodIndex]);
                Assert.IsTrue(topHeight > minimumTopHeight[periodIndex]);
                Assert.IsTrue(topHeight < topHeightTolerance * minimumTopHeight[periodIndex]);
                Assert.IsTrue(treeRecords > 0);
                Assert.IsTrue(treeRecords < 666);

                // TODO: check qmd against QMD from basal area
            }
        }

        private static void Verify(SnagLogTable snagsAndLogs, OrganonStandTrajectory trajectory)
        {
            Assert.IsTrue(snagsAndLogs.DiameterClasses == 121);
            Assert.IsTrue(snagsAndLogs.DiameterClassSizeInCentimeters == Constant.Bucking.DiameterClassSizeInCentimeters);
            Assert.IsTrue(snagsAndLogs.MaximumDiameterInCentimeters == Constant.Bucking.DefaultMaximumDiameterInCentimeters);
            Assert.IsTrue(snagsAndLogs.Periods == trajectory.PlanningPeriods);

            Assert.IsTrue(snagsAndLogs.LogQmdInCentimetersByPeriod.Length == snagsAndLogs.Periods);
            Assert.IsTrue(snagsAndLogs.LogsPerHectareByPeriod.Length == snagsAndLogs.Periods);
            Assert.IsTrue(snagsAndLogs.LogsPerHectareBySpeciesAndDiameterClass.Count == 1);
            Assert.IsTrue(snagsAndLogs.LogsPerHectareBySpeciesAndDiameterClass[FiaCode.PseudotsugaMenziesii].GetLength(0) == snagsAndLogs.Periods);
            Assert.IsTrue(snagsAndLogs.LogsPerHectareBySpeciesAndDiameterClass[FiaCode.PseudotsugaMenziesii].GetLength(1) == snagsAndLogs.DiameterClasses);

            Assert.IsTrue(snagsAndLogs.SnagQmdInCentimetersByPeriod.Length == snagsAndLogs.Periods);
            Assert.IsTrue(snagsAndLogs.SnagsPerHectareByPeriod.Length == snagsAndLogs.Periods);
            Assert.IsTrue(snagsAndLogs.SnagsPerHectareBySpeciesAndDiameterClass.Count == 1);
            Assert.IsTrue(snagsAndLogs.SnagsPerHectareBySpeciesAndDiameterClass[FiaCode.PseudotsugaMenziesii].GetLength(0) == snagsAndLogs.Periods);
            Assert.IsTrue(snagsAndLogs.SnagsPerHectareBySpeciesAndDiameterClass[FiaCode.PseudotsugaMenziesii].GetLength(1) == snagsAndLogs.DiameterClasses);

            float initialTreesPerHectare = Constant.AcresPerHectare * trajectory.DensityByPeriod[0].TreesPerAcre;
            float initialStemsPerHectare = initialTreesPerHectare + snagsAndLogs.LogsPerHectareByPeriod[0] + snagsAndLogs.SnagsPerHectareByPeriod[0];
            for (int period = 0; period < snagsAndLogs.Periods; ++period)
            {
                float logsPerHectare = snagsAndLogs.LogsPerHectareByPeriod[period];
                float snagPerHectare = snagsAndLogs.SnagsPerHectareByPeriod[period];
                float treesPerHectare = Constant.AcresPerHectare * trajectory.DensityByPeriod[period].TreesPerAcre;
                float stemsPerHectare = treesPerHectare + snagPerHectare + logsPerHectare;

                Assert.IsTrue(snagsAndLogs.LogQmdInCentimetersByPeriod[period] >= 0.0F);
                Assert.IsTrue(logsPerHectare >= 0.0F);

                Assert.IsTrue(snagsAndLogs.SnagQmdInCentimetersByPeriod[period] >= 0.0F);
                Assert.IsTrue(snagPerHectare >= 0.0F);

                // for now, assume no ingrowth
                Assert.IsTrue(initialStemsPerHectare >= stemsPerHectare);
            }
        }

        private static void Verify(SortedDictionary<FiaCode, int[]> individualTreeSelectionBySpecies, int firstThinPeriod, int? secondThinPeriod, int? thirdThinPeriod, int minimumTreesSelected, int maximumTreesSelected)
        {
            int outOfRangeTrees = 0;
            int treesSelected = 0;
            foreach (int[] individualTreeSelection in individualTreeSelectionBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < individualTreeSelection.Length; ++treeIndex)
                {
                    int treeSelection = individualTreeSelection[treeIndex];
                    bool isOutOfRange = (treeSelection != 0) && (treeSelection != firstThinPeriod);
                    if (secondThinPeriod.HasValue)
                    {
                        isOutOfRange &= treeSelection != secondThinPeriod.Value;
                    }
                    if (thirdThinPeriod.HasValue)
                    {
                        isOutOfRange &= treeSelection != thirdThinPeriod.Value;
                    }
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
