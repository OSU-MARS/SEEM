using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mars.Seem.Data;
using Mars.Seem.Heuristics;
using Mars.Seem.Optimization;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Mars.Seem.Test
{
    [TestClass]
    public class PublicApi : OrganonTest
    {
        private static readonly float biomassTolerance;
        private static readonly float QmdTolerance;
        private static readonly float TopHeightTolerance;
        private static readonly float VolumeTolerance;

        public TestContext? TestContext { get; set; }

        static PublicApi()
        {
            PublicApi.biomassTolerance = 1.01F;
            PublicApi.QmdTolerance = 1.01F;
            PublicApi.TopHeightTolerance = 1.01F;
            PublicApi.VolumeTolerance = 1.01F;
        }

        private static HeuristicStandTrajectories<HeuristicParameters> CreateResults(HeuristicParameters parameters, int firstThinPeriodIndex, int rotationLength)
        {
            List<HeuristicParameters> parameterCombinations = new() { parameters };
            FinancialScenarios financialScenarios = new();
            List<int> firstThinPeriods = new() { firstThinPeriodIndex };
            List<int> noThin = new() { Constant.NoThinPeriod };
            List<int> planningPeriods = new() { rotationLength };
            HeuristicStandTrajectories<HeuristicParameters> results = new(parameterCombinations, firstThinPeriods, noThin, noThin, planningPeriods, financialScenarios, TestConstant.SolutionPoolSize);

            StandTrajectoryCoordinate coordinate = new()
            {
                FinancialIndex = Constant.HeuristicDefault.CoordinateIndex,
                FirstThinPeriodIndex = 0,
                SecondThinPeriodIndex = 0,
                ThirdThinPeriodIndex = 0,
                ParameterIndex = Constant.HeuristicDefault.CoordinateIndex,
                RotationIndex = Constant.HeuristicDefault.CoordinateIndex
            };
            results.AddEvaluatedPosition(coordinate);
            
            return results;
        }

        private static PlotsWithHeight GetNelder()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Malcolm Knapp Nelder 1.xlsx");
            PlotsWithHeight plot = new(new List<int>() { 1 }, defaultExpansionFactorPerHa: 1.327F);
            plot.Read(plotFilePath, "1");
            return plot;
        }

        private static PlotsWithHeight GetPlot14()
        {
            string plotFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OSU", "Organon", "Malcolm Knapp plots 14-18+34 Ministry.xlsx");
            PlotsWithHeight plot = new(new List<int>() { 14 }, defaultExpansionFactorPerHa: 4.48F);
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
        public void NelderCircularHeuristics()
        {
            int thinningPeriod = 4;
            int treeRecords = 100;
            #if DEBUG
                treeRecords = 48;
            #endif

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = new(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToOrganonStand(configuration, ageInYears: 20, Constant.Default.DouglasFirSiteIndexInM, Constant.Default.WesternHemlockSiteIndexInM, treeRecords);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            RunParameters landExpectationValueIts = new(new List<int>() { 9 }, configuration)
            {
                MaximizeForPlanningPeriod = 9
            };
            landExpectationValueIts.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));

            // first improving circular search
            HeuristicStandTrajectories<HeuristicParameters> results = PublicApi.CreateResults(new(), thinningPeriod, landExpectationValueIts.MaximizeForPlanningPeriod);
            // sometimes useful for debugging: start with no trees selected
            // results.ParameterCombinations[0].MinimumConstructionGreediness = Constant.Grasp.FullyGreedyConstructionForMaximization;
            FirstImprovingCircularSearch firstCircular = new(stand, results.ParameterCombinations[0], landExpectationValueIts)
            {
                //IsStochastic = true,
                MaximumIterations = 10 * treeRecords
            };
            StandTrajectoryCoordinate singleThinCoordinate = new();
            PrescriptionPerformanceCounters firstImprovingCounters = firstCircular.Run(singleThinCoordinate, results);
            results.AssimilateIntoCoordinate(firstCircular, singleThinCoordinate, firstImprovingCounters);

            // hero
            Hero hero = new(stand, results.ParameterCombinations[0], landExpectationValueIts)
            {
                //IsStochastic = true,
                MaximumIterations = 10
            };
            // hero.CurrentTrajectory.SetTreeSelection(0, thinningPeriod); // sometimes useful for debugging: don't thin first tree
            PrescriptionPerformanceCounters heroCounters = hero.Run(singleThinCoordinate, results);
            results.AssimilateIntoCoordinate(hero, singleThinCoordinate, heroCounters);

            // prescription coordinate descent
            RunParameters landExpectationValuePrescription = new(new List<int>() { landExpectationValueIts.MaximizeForPlanningPeriod }, configuration)
            {
                MaximizeForPlanningPeriod = landExpectationValueIts.MaximizeForPlanningPeriod
            };
            landExpectationValuePrescription.Treatments.Harvests.Add(new ThinByPrescription(thinningPeriod));
            PrescriptionParameters prescriptionParameters = new()
            {
                MinimumIntensity = 0.0F
            };
            PrescriptionCoordinateAscent prescriptionAscent = new(stand, prescriptionParameters, landExpectationValuePrescription)
            {
                Gradient = true,
                IsStochastic = true,
                RestartOnLocalMaximum = true
            };
            PrescriptionPerformanceCounters prescriptionAscentCounters = prescriptionAscent.Run(singleThinCoordinate, results);
            results.AssimilateIntoCoordinate(prescriptionAscent, singleThinCoordinate, prescriptionAscentCounters);

            this.Verify(firstCircular, firstImprovingCounters);
            this.Verify(hero, heroCounters);
            this.Verify(prescriptionAscent, prescriptionAscentCounters);
            PublicApi.Verify(results, singleThinCoordinate);

            IndividualTreeSelection firstCircularTreeSelection = firstCircular.GetBestTrajectory(singleThinCoordinate).TreeSelectionBySpecies[FiaCode.PseudotsugaMenziesii];
            IndividualTreeSelection heroTreeSelection = hero.GetBestTrajectory(singleThinCoordinate).TreeSelectionBySpecies[FiaCode.PseudotsugaMenziesii];
            IndividualTreeSelection prescriptionTreeSelection = prescriptionAscent.GetBestTrajectory(singleThinCoordinate).TreeSelectionBySpecies[FiaCode.PseudotsugaMenziesii];
            int treesThinnedByFirstCircular = 0;
            int treesThinnedByHero = 0;
            int treesThinnedByPrescription = 0;
            for (int treeIndex = 0; treeIndex < heroTreeSelection.Count; ++treeIndex)
            {
                if (firstCircularTreeSelection[treeIndex] != Constant.RegenerationHarvestPeriod)
                {
                    ++treesThinnedByFirstCircular;
                }
                if (heroTreeSelection[treeIndex] != Constant.RegenerationHarvestPeriod)
                {
                    ++treesThinnedByHero;
                }
                if (prescriptionTreeSelection[treeIndex] != Constant.RegenerationHarvestPeriod)
                {
                    ++treesThinnedByPrescription;
                }
            }

            float highestFirstCircularFinancialValue = firstCircular.FinancialValue.GetHighestValue();
            float highestHeroFinancialValue = hero.FinancialValue.GetHighestValue();
            float highestPrescriptionFinancialValue = prescriptionAscent.FinancialValue.GetHighestValue();

            // location of financial value maxima varies as harvest cost calculations evolve and timber values change
            // Therefore, check for convergence to a whitelist of known solutions. Since the number of trees varies between debug and
            // release builds, so does the content of the whitelist. The unthinned solution has the highest financial value and it is
            // expected prescription search will always locate it it.
            #if DEBUG
                Span<float> minFinancialValues = stackalloc[] { 0.114F };
                Span<int> treesThinned = stackalloc[] { 0 };
            #else
                Span<float> minFinancialValues = stackalloc[] { 1.075F, 1.169F };
                Span<int> treesThinned = stackalloc[] { 2, 0 };
            #endif
            int matchingOptimaIndexFirstCircular = -1;
            int matchingOptimaIndexHero = -1;
            int matchingOptimaIndexPrescription = -1;
            float maxMinFinancialValue = Single.MinValue;
            int maxMinFinancialValueIndex = -1;
            for (int localOptimaIndex = 0; localOptimaIndex < minFinancialValues.Length; ++localOptimaIndex)
            {
                float minFinancialValue = minFinancialValues[localOptimaIndex];
                float maxFinancialValue = (minFinancialValue < 0.0F ? 0.98F : 1.02F) * minFinancialValue;
                if ((highestFirstCircularFinancialValue > minFinancialValue) && (highestFirstCircularFinancialValue < maxFinancialValue))
                {
                    matchingOptimaIndexFirstCircular = localOptimaIndex;
                }
                if ((highestHeroFinancialValue > minFinancialValue) && (highestHeroFinancialValue < maxFinancialValue))
                {
                    matchingOptimaIndexHero = localOptimaIndex;
                }
                if ((highestPrescriptionFinancialValue > minFinancialValue) && (highestPrescriptionFinancialValue < maxFinancialValue))
                {
                    matchingOptimaIndexPrescription = localOptimaIndex;
                }
                if (maxMinFinancialValue < minFinancialValue)
                {
                    maxMinFinancialValue = minFinancialValue;
                    maxMinFinancialValueIndex = localOptimaIndex;
                }
            }
            Assert.IsTrue((matchingOptimaIndexFirstCircular >= 0) && (treesThinnedByFirstCircular == treesThinned[matchingOptimaIndexFirstCircular]), 
                          "First circular financial value " + highestFirstCircularFinancialValue + " with " + treesThinnedByFirstCircular + " trees thinned.");
            Assert.IsTrue((matchingOptimaIndexHero >= 0) && (treesThinnedByHero == treesThinned[matchingOptimaIndexHero]),
                          "Hero financial value " + highestHeroFinancialValue + " with " + treesThinnedByHero + " trees thinned.");
            Assert.IsTrue((matchingOptimaIndexPrescription == maxMinFinancialValueIndex) && (treesThinnedByPrescription == treesThinned[matchingOptimaIndexPrescription]),
                          "Prescription financial value " + highestPrescriptionFinancialValue + " with " + treesThinnedByPrescription + " trees thinned.");
        }

        [TestMethod]
        public void NelderOtherHeuristics()
        {
            int thinningPeriod = 4;
            int treeRecords = 75;
            #if DEBUG
                treeRecords = 25;
            #endif

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToOrganonStand(configuration, ageInYears: 20, Constant.Default.DouglasFirSiteIndexInM, Constant.Default.WesternHemlockSiteIndexInM, treeRecords);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            // heuristics optimizing for LEV
            RunParameters runForLandExpectationValue = new(new List<int>() { 9 }, configuration)
            {
                MaximizeForPlanningPeriod = 9
            };
            runForLandExpectationValue.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            HeuristicStandTrajectories<HeuristicParameters> levResults = PublicApi.CreateResults(new(), thinningPeriod, runForLandExpectationValue.MaximizeForPlanningPeriod);
            StandTrajectoryCoordinate levCoordinate = levResults.CoordinatesEvaluated[0];
            OptimizationObjectiveDistribution levDistribution = levResults[levCoordinate].Distribution;
            PrescriptionPerformanceCounters totalCounters = new();

            GeneticParameters geneticParameters = new(treeRecords)
            {
                PopulationSize = 7,
                MaximumGenerations = 5,
            };
            GeneticAlgorithm genetic = new(stand, geneticParameters, runForLandExpectationValue);
            PrescriptionPerformanceCounters geneticCounters = genetic.Run(levCoordinate, levResults);
            totalCounters += geneticCounters;
            levResults.AssimilateIntoCoordinate(genetic, levCoordinate, geneticCounters);

            GreatDeluge deluge = new(stand, levResults.ParameterCombinations[0], runForLandExpectationValue)
            {
                RainRate = 5,
                LowerWaterAfter = 9,
                StopAfter = 10
            };
            PrescriptionPerformanceCounters delugeCounters = deluge.Run(levCoordinate, levResults);
            totalCounters += delugeCounters;
            levResults.AssimilateIntoCoordinate(deluge, levCoordinate, delugeCounters);

            RecordTravel recordTravel = new(stand, levResults.ParameterCombinations[0], runForLandExpectationValue)
            {
                StopAfter = 10
            };
            PrescriptionPerformanceCounters recordCounters = recordTravel.Run(levCoordinate, levResults);
            levResults.AssimilateIntoCoordinate(recordTravel, levCoordinate, recordCounters);

            SimulatedAnnealing annealer = new(stand, levResults.ParameterCombinations[0], runForLandExpectationValue)
            {
                Iterations = 100
            };
            PrescriptionPerformanceCounters annealerCounters = annealer.Run(levCoordinate, levResults);
            totalCounters += annealerCounters;
            levResults.AssimilateIntoCoordinate(annealer, levCoordinate, annealerCounters);

            TabuParameters tabuParameters = new();
            TabuSearch tabu = new(stand, tabuParameters, runForLandExpectationValue)
            {
                Iterations = 7,
                //Jump = 2,
                MaximumTenure = 5
            };
            PrescriptionPerformanceCounters tabuCounters = tabu.Run(levCoordinate, levResults);
            totalCounters += tabuCounters;
            levResults.AssimilateIntoCoordinate(tabu, levCoordinate, tabuCounters);

            ThresholdAccepting thresholdAcceptor = new(stand, levResults.ParameterCombinations[0], runForLandExpectationValue);
            thresholdAcceptor.IterationsPerThreshold.Clear();
            thresholdAcceptor.Thresholds.Clear();
            thresholdAcceptor.IterationsPerThreshold.Add(10);
            thresholdAcceptor.Thresholds.Add(1.0F);
            PrescriptionPerformanceCounters thresholdCounters = thresholdAcceptor.Run(levCoordinate, levResults);
            totalCounters += thresholdCounters;
            levResults.AssimilateIntoCoordinate(thresholdAcceptor, levCoordinate, thresholdCounters);

            PrescriptionParameters prescriptionParameters = new()
            {
                MaximumIntensity = 60.0F,
                MinimumIntensity = 50.0F,
                DefaultIntensityStepSize = 10.0F,
            };
            runForLandExpectationValue.Treatments.Harvests[0] = new ThinByPrescription(thinningPeriod); // must change from individual tree selection
            PrescriptionEnumeration enumerator = new(stand, prescriptionParameters, runForLandExpectationValue);
            PrescriptionPerformanceCounters enumerationCounters = enumerator.Run(levCoordinate, levResults);
            totalCounters += enumerationCounters;
            levResults.AssimilateIntoCoordinate(enumerator, levCoordinate, enumerationCounters);

            // check solution pool
            SilviculturalPrescriptionPool levEliteSolutions = PublicApi.Verify(levResults, levCoordinate);

            // heuristic optimizing for volume
            RunParameters runForVolume = new(runForLandExpectationValue.RotationLengths, configuration)
            {
                MaximizeForPlanningPeriod = runForLandExpectationValue.MaximizeForPlanningPeriod,
                TimberObjective = TimberObjective.ScribnerVolume
            };
            runForVolume.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));

            HeuristicStandTrajectories<HeuristicParameters> volumeResults = PublicApi.CreateResults(new(), thinningPeriod, runForLandExpectationValue.MaximizeForPlanningPeriod);
            StandTrajectoryCoordinate volumeCoordinate = volumeResults.CoordinatesEvaluated[0];
            AutocorrelatedWalk autocorrelated = new(stand, volumeResults.ParameterCombinations[0], runForVolume)
            {
                Iterations = 4
            };
            PrescriptionPerformanceCounters autocorrlatedCounters = autocorrelated.Run(volumeCoordinate, volumeResults);
            totalCounters += autocorrlatedCounters;
            volumeResults.AssimilateIntoCoordinate(autocorrelated, volumeCoordinate, autocorrlatedCounters);

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
            float maxHeuristicLev = new float[] { deluge.FinancialValue.GetHighestValue(), annealer.FinancialValue.GetHighestValue(), thresholdAcceptor.FinancialValue.GetHighestValue(), genetic.FinancialValue.GetHighestValue(), enumerator.FinancialValue.GetHighestValue(), recordTravel.FinancialValue.GetHighestValue(), tabu.FinancialValue.GetHighestValue() }.Max();
            float maxLevInDistribution = levDistribution.HighestFinancialValueBySolution.Max();
            Assert.IsTrue(maxHeuristicLev == maxLevInDistribution);
            Assert.IsTrue(maxHeuristicLev == levEliteSolutions.High!.FinancialValue);

            int maxMove = levDistribution.GetMaximumMoveIndex();
            Assert.IsTrue(maxMove > 0);
            List<float> landExpectationVaues = new(8);
            for (int moveIndex = 0; moveIndex < maxMove; ++moveIndex)
            {
                PublicApi.TryAppendAcceptedFinancialValue(landExpectationVaues, deluge, moveIndex);
                PublicApi.TryAppendAcceptedFinancialValue(landExpectationVaues, annealer, moveIndex);
                PublicApi.TryAppendAcceptedFinancialValue(landExpectationVaues, thresholdAcceptor, moveIndex);
                PublicApi.TryAppendAcceptedFinancialValue(landExpectationVaues, genetic, moveIndex);
                PublicApi.TryAppendAcceptedFinancialValue(landExpectationVaues, enumerator, moveIndex);
                PublicApi.TryAppendAcceptedFinancialValue(landExpectationVaues, recordTravel, moveIndex);
                PublicApi.TryAppendAcceptedFinancialValue(landExpectationVaues, tabu, moveIndex);
                maxHeuristicLev = landExpectationVaues.Max();
                float minHeuristicLev = landExpectationVaues.Min();

                DistributionStatistics moveStatistics = levDistribution.GetFinancialStatisticsForMove(moveIndex);
                maxLevInDistribution = moveStatistics.Maximum;
                float minDistributionObjective = moveStatistics.Minimum;
                int distributionRunCount = moveStatistics.Count;

                Assert.IsTrue(distributionRunCount > 0);
                Assert.IsTrue(distributionRunCount <= 8);
                Assert.IsTrue(maxLevInDistribution <= maxHeuristicLev);
                Assert.IsTrue(minDistributionObjective >= minHeuristicLev);
                Assert.IsTrue(moveStatistics.Mean >= minHeuristicLev);
                Assert.IsTrue(moveStatistics.Mean <= maxHeuristicLev);
                Assert.IsTrue(moveStatistics.Median >= minHeuristicLev);
                Assert.IsTrue(moveStatistics.Median <= maxHeuristicLev);
                // not enough heuristic runs to check percentiles

                landExpectationVaues.Clear();
            }

            // verify solution pooling
            levResults.GetPoolPerformanceCounters(out int solutionsCached, out int solutionsAccepted, out int solutionsRejected);
            Assert.IsTrue(solutionsCached <= TestConstant.SolutionPoolSize);
            Assert.IsTrue(solutionsAccepted >= solutionsCached);
            Assert.IsTrue(solutionsRejected >= 0);
        }

        [TestMethod]
        public void NelderTrajectory()
        {
            int expectedUnthinnedTreeRecordCount = 661;
            int lastPeriod = 9;

            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToOrganonStand(configuration, ageInYears: 20, Constant.Default.DouglasFirSiteIndexInM, Constant.Default.WesternHemlockSiteIndexInM, maximumTreeRecords: Int32.MaxValue);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            OrganonStandTrajectory unthinnedTrajectory = new(stand, configuration, TreeVolume.Default, lastPeriod);
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
            OrganonStandTrajectory twoThinTrajectory = new(stand, configuration, TreeVolume.Default, lastPeriod);
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
            //                                                                          \s+\w+\.\w+\(\d+\)\s+(\d+.\d{1,2})\d*\s+float\r?\n
            ExpectedStandTrajectory unthinnedExpected = new()
            {
                //                             0       1       2       3       4       5       6       7       8       9
                MinimumQmdInCm = new float[] { 16.84F, 20.96F, 24.63F, 27.94F, 31.01F, 33.97F, 36.86F, 39.73F, 42.58F, 45.42F }, // cm
                //                                  0       1       2       3       4       5       6       7       8       9
                MinimumTopHeightInM = new float[] { 16.50F, 20.68F, 24.48F, 27.91F, 31.04F, 33.89F, 36.50F, 38.89F, 41.10F, 43.13F }, // m
                // Poudel 2018 + Scribner long log net MBF/ha
                // bilinear interpolation: 1 cm diameter classes, 1 m height classes
                MinimumStandingCubicM3PerHa = new float[] { 65.82F, 154.34F, 252.96F, 363.45F, 468.31F, 558.84F, 638.64F, 709.21F, 771.96F, 828.47F },
                MinimumStandingMbfPerHa = new float[] { 9.64F, 18.87F, 30.66F, 46.63F, 61.66F, 74.19F, 87.95F, 102.12F, 114.95F, 126.89F },
                MinimumHarvestCubicM3PerHa = new float[lastPeriod + 1], // no thinning -> all zero
                MinimumHarvestMbfPerHa = new float[lastPeriod + 1] // no thinning -> all zero
            };
            foreach (Stand? unthinnedStand in unthinnedTrajectory.StandByPeriod)
            {
                AssertNullable.IsNotNull(unthinnedStand);
                Assert.IsTrue(unthinnedStand.GetTreeRecordCount() == expectedUnthinnedTreeRecordCount);
            }

            PublicApi.Verify(unthinnedTrajectory, unthinnedExpected, configuration.Variant.TimeStepInYears);

            // verify one thin trajectory
            ExpectedStandTrajectory oneThinExpected = new()
            {
                FirstThinPeriod = firstThinPeriod,
                MinimumTreesSelected = 200,
                MaximumTreesSelected = 400,
                //                             0       1       2       3       4       5       6       7       8       9
                MinimumQmdInCm = new float[] { 16.84F, 20.96F, 24.63F, 30.22F, 34.47F, 38.10F, 41.33F, 44.29F, 47.05F, 49.65F }, // cm
                //                                  0       1       2       3       4       5       6       7       8       9
                MinimumTopHeightInM = new float[] { 16.50F, 20.68F, 24.48F, 26.92F, 29.98F, 32.89F, 35.58F, 38.05F, 40.33F, 42.43F }, // m
                // Poudel 2018 + Scribner long log net MBF/ha
                // bilinear interpolation: 1 cm diameter classes, 1 m height classes
                MinimumStandingCubicM3PerHa = new float[] { 65.82F, 154.34F, 252.96F, 227.28F, 324.42F, 420.62F, 511.58F, 597.88F, 677.49F, 750.30F },
                MinimumStandingMbfPerHa = new float[] { 9.64F, 18.87F, 30.66F, 27.73F, 41.23F, 53.74F, 67.30F, 83.50F, 99.00F, 112.93F },
                MinimumHarvestCubicM3PerHa = new float[] { 0.0F, 0.0F, 0.0F, 107.49F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F },
                MinimumHarvestMbfPerHa = new float[] { 0.0F, 0.0F, 0.0F, 14.79F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F }
            };
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

            PublicApi.Verify(oneThinTrajectory, oneThinExpected, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(oneThinTrajectory, oneThinExpected);

            // verify two thin trajectory
            ExpectedStandTrajectory twoThinExpected = new()
            {
                FirstThinPeriod = firstThinPeriod,
                SecondThinPeriod = secondThinPeriod,
                MinimumTreesSelected = 200,
                MaximumTreesSelected = 400,
                //                             0       1       2       3       4       5       6       7       8       9
                MinimumQmdInCm = new float[] { 16.84F, 20.96F, 24.63F, 30.22F, 34.47F, 38.10F, 41.90F, 45.32F, 48.38F, 51.02F }, // cm
                //                                  0       1       2       3       4       5       6       7       8       9
                MinimumTopHeightInM = new float[] { 16.50F, 20.68F, 24.48F, 26.92F, 29.98F, 32.89F, 35.13F, 37.57F, 39.88F, 42.02F }, // m
                // Poudel 2018 + Scribner long log net MBF/ha
                // bilinear interpolation: 1 cm diameter classes, 1 m height classes
                MinimumStandingCubicM3PerHa = new float[] { 65.82F, 154.34F, 252.96F, 227.28F, 324.42F, 420.62F, 420.97F, 506.76F, 589.38F, 666.28F },
                MinimumStandingMbfPerHa = new float[] { 9.64F, 18.87F, 30.66F, 27.73F, 41.23F, 53.74F, 55.08F, 70.40F, 85.56F, 99.86F },
                MinimumHarvestCubicM3PerHa = new float[] { 0.0F, 0.0F, 0.0F, 107.49F, 0.0F, 0.0F, 81.42F, 0.0F, 0.0F, 0.0F },
                MinimumHarvestMbfPerHa = new float[] { 0.0F, 0.0F, 0.0F, 14.79F, 0.0F, 0.0F, 11.89F, 0.0F, 0.0F, 0.0F }
            };
            float[] minimumTwoThinLiveBiomass = new float[] { 85531F, 146900F, 212987F, 167906F, 226196F, 283469F, 278099F, 329517F, 377780F, 422620F }; // kg/ha

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

            PublicApi.Verify(twoThinTrajectory, twoThinExpected, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(twoThinTrajectory, twoThinExpected);

            for (int periodIndex = 0; periodIndex < twoThinTrajectory.PlanningPeriods; ++periodIndex)
            {
                float liveBiomass = twoThinTrajectory.StandByPeriod[periodIndex]!.GetLiveBiomass();
                Assert.IsTrue(liveBiomass > minimumTwoThinLiveBiomass[periodIndex]);
                Assert.IsTrue(liveBiomass < PublicApi.biomassTolerance * minimumTwoThinLiveBiomass[periodIndex]);
            }

            // verify three thin trajectory
            ExpectedStandTrajectory threeThinExpected = new()
            {
                FirstThinPeriod = firstThinPeriod,
                SecondThinPeriod = secondThinPeriod,
                ThirdThinPeriod = thirdThinPeriod,
                MinimumTreesSelected = 200,
                MaximumTreesSelected = 485,
                //                             0       1       2       3       4       5       6       7       8       9
                MinimumQmdInCm = new float[] { 16.84F, 20.96F, 24.63F, 30.22F, 34.47F, 38.10F, 41.90F, 45.32F, 51.89F, 55.17F }, // cm
                //                                  0       1       2       3       4       5       6       7       8       9
                MinimumTopHeightInM = new float[] { 16.50F, 20.68F, 24.48F, 26.92F, 29.98F, 32.89F, 35.13F, 37.57F, 39.41F, 41.52F }, // m
                // Poudel 2018 + Scribner long log net MBF/ha
                // bilinear interpolation: 1 cm diameter classes, 1 m height classes
                MinimumStandingCubicM3PerHa = new float[] { 65.82F, 154.34F, 252.96F, 227.28F, 324.42F, 420.62F, 420.97F, 506.76F, 483.19F, 559.39F },
                MinimumStandingMbfPerHa = new float[] { 9.64F, 18.87F, 30.66F, 27.73F, 41.23F, 53.74F, 55.08F, 70.40F, 69.92F, 83.92F },
                MinimumHarvestCubicM3PerHa = new float[] { 0.0F, 0.0F, 0.0F, 107.49F, 0.0F, 0.0F, 81.42F, 0.0F, 98.60F, 0.0F },
                MinimumHarvestMbfPerHa = new float[] { 0.0F, 0.0F, 0.0F, 14.79F, 0.0F, 0.0F, 11.89F, 0.0F, 15.17F, 0.0F }
            };
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

            PublicApi.Verify(threeThinTrajectory, threeThinExpected, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(threeThinTrajectory, threeThinExpected);
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
                SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies = configuration.CreateSpeciesCalibration();
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
            OrganonStand stand = plot14.ToOrganonStand(configuration, ageInYears: 30, Constant.Default.DouglasFirSiteIndexInM, Constant.Default.WesternHemlockSiteIndexInM, maximumTreeRecords: Int32.MaxValue);
            stand.PlantingDensityInTreesPerHectare = TestConstant.Plot14ReplantingDensityInTreesPerHectare;

            OrganonStandTrajectory thinnedTrajectory = new(stand, configuration, TreeVolume.Default, lastPeriod);
            thinnedTrajectory.Treatments.Harvests.Add(new ThinByPrescription(thinPeriod)
            {
                FromAbovePercentage = 0.0F,
                ProportionalPercentage = 30.0F,
                FromBelowPercentage = 0.0F
            });
            AssertNullable.IsNotNull(thinnedTrajectory.StandByPeriod[0]);
            Assert.IsTrue(thinnedTrajectory.StandByPeriod[0]!.GetTreeRecordCount() == 222);
            long immediateThinTimeSteps = thinnedTrajectory.Simulate();
            Assert.IsTrue(immediateThinTimeSteps == lastPeriod);

            // verify thinned trajectory
            // find/replace regular expressions: function call \w+\.\w+\[\d+\]\.\w+\(\)\s+(\d+.\d{1,2})\d*\s+float\r?\n
            //                                                 \s+\w+\.\w+\(\d+\)\s+(\d+.\d{1,2})\d*\s+float\r?\n
            //                                   array element \[\d+\]\s+(\d+.\d{1,3})\d*\s+float\r?\n -> $1F, 
            ExpectedStandTrajectory immediateThinExpected = new()
            {
                FirstThinPeriod = thinPeriod,
                MinimumTreesSelected = 65,
                MaximumTreesSelected = 70,
                //                             0       1       2       3       4     
                MinimumQmdInCm = new float[] { 23.33F, 26.87F, 30.16F, 33.12F, 35.92F },
                //                                  0       1       2       3       4     
                MinimumTopHeightInM = new float[] { 28.32F, 30.80F, 33.52F, 36.11F, 38.50F },
                // Poudel 2018 + Scribner long log net MBF/ha
                // bilinear interpolation: 1 cm diameter classes, 1 m height classes
                MinimumStandingCubicM3PerHa = new float[] { 360.59F, 361.88F, 473.05F, 571.58F, 660.29F },
                MinimumStandingMbfPerHa = new float[] { 50.62F, 50.97F, 66.41F, 80.67F, 95.59F },
                MinimumHarvestCubicM3PerHa = new float[] { 0.0F, 104.27F, 0.0F, 0.0F, 0.0F },
                MinimumHarvestMbfPerHa = new float[] { 0.0F, 15.18F, 0.0F, 0.0F, 0.0F }
            };
            PublicApi.Verify(thinnedTrajectory, immediateThinExpected, configuration.Variant.TimeStepInYears);
            PublicApi.Verify(thinnedTrajectory, immediateThinExpected);
            Assert.IsTrue(thinnedTrajectory.GetFirstThinAge() == 35);
            Assert.IsTrue(thinnedTrajectory.StandByPeriod[^1]!.GetTreeRecordCount() == 156);

            // verify snag and log calculations
            SnagDownLogTable snagsAndLogs = new(thinnedTrajectory, Constant.Bucking.DefaultMaximumFinalHarvestDiameterInCentimeters, Constant.Bucking.DiameterClassSizeInCentimeters);
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
            int treeRecordCount = 300;
            #if DEBUG
                runs = 1; // do only functional validation of test on debug builds to reduce test execution time
                treeRecordCount = 10;
            #endif

            List<float> discountRates = new() {  Constant.Financial.DefaultAnnualDiscountRate };
            PlotsWithHeight nelder = PublicApi.GetNelder();
            OrganonConfiguration configuration = PublicApi.CreateOrganonConfiguration(new OrganonVariantNwo());
            OrganonStand stand = nelder.ToOrganonStand(configuration, ageInYears: 20, Constant.Default.DouglasFirSiteIndexInM, Constant.Default.WesternHemlockSiteIndexInM, treeRecordCount);
            stand.PlantingDensityInTreesPerHectare = TestConstant.NelderReplantingDensityInTreesPerHectare;

            RunParameters landExpectationValue = new(new List<int>() { 9 }, configuration)
            {
                MaximizeForPlanningPeriod = 9
            };
            landExpectationValue.Treatments.Harvests.Add(new ThinByIndividualTreeSelection(thinningPeriod));
            HeuristicStandTrajectories<HeuristicParameters> results = PublicApi.CreateResults(new(), thinningPeriod, landExpectationValue.MaximizeForPlanningPeriod);
            StandTrajectoryCoordinate defaultCoordinate = new();

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
                    PrescriptionPerformanceCounters heroCounters = hero.Run(defaultCoordinate, results);
                    runtime += heroCounters.Duration;
                }
            }
            this.TestContext!.WriteLine(runtime.TotalSeconds.ToString());
        }

        private static void TryAppendAcceptedFinancialValue(List<float> objectives, Heuristic heuristic, int moveIndex)
        {
            StandTrajectoryCoordinate defaultCoordinate = new();
            IList<float> acceptedMoveFinancialValues = heuristic.FinancialValue.GetAcceptedValuesWithDefaulting(defaultCoordinate);
            if (acceptedMoveFinancialValues.Count > moveIndex)
            {
                float acceptedFinancialValue = acceptedMoveFinancialValues[moveIndex];
                objectives.Add(acceptedFinancialValue);
                Assert.IsTrue(acceptedFinancialValue >= heuristic.FinancialValue.GetCandidateValuesWithDefaulting(defaultCoordinate)[moveIndex]);
            }
        }

        private void Verify(GeneticAlgorithm genetic, PrescriptionPerformanceCounters perfCounters)
        {
            // do general heuristic validation (disambiguation with <T> or similar required to avoid call to self and stack overflow)
            this.Verify<GeneticParameters>(genetic, perfCounters);

            GeneticParameters geneticParameters = genetic.GetParameters();
            PopulationStatistics statistics = genetic.PopulationStatistics;
            Assert.IsTrue(statistics.Generations <= geneticParameters.MaximumGenerations);
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
                Assert.IsTrue((newIndividuals >= 0) && (newIndividuals <= 2 * geneticParameters.PopulationSize)); // two children per breeding
                Assert.IsTrue((polymorphism >= 0.0F) && (polymorphism <= 1.0F));
            }
        }

        private void Verify<TParameters>(Heuristic<TParameters> heuristic, PrescriptionPerformanceCounters perfCounters) where TParameters : HeuristicParameters
        {
            // check objective functions
            StandTrajectoryCoordinate defaultCoordinate = new();
            OrganonStandTrajectory bestTrajectory = heuristic.GetBestTrajectory(defaultCoordinate);
            Assert.IsTrue(bestTrajectory.PlantingDensityInTreesPerHectare == heuristic.CurrentTrajectory.PlantingDensityInTreesPerHectare);
            Assert.IsTrue(bestTrajectory.PlantingDensityInTreesPerHectare >= TestConstant.NelderReplantingDensityInTreesPerHectare);

            float recalculatedHighestFinancialValue = heuristic.GetFinancialValue(bestTrajectory, Constant.HeuristicDefault.CoordinateIndex);
            float highestFinancialValue = heuristic.FinancialValue.GetHighestValue();
            float highestFinancialValueRatio = highestFinancialValue / recalculatedHighestFinancialValue;
            this.TestContext!.WriteLine("{0} best objective: {1} (recalculation ratio {2})", heuristic.GetName(), highestFinancialValue, highestFinancialValueRatio);
            if (heuristic.RunParameters.TimberObjective == TimberObjective.LandExpectationValue)
            {
                Assert.IsTrue(highestFinancialValue > -3.25F, "Highest financial value found by " + heuristic.GetName() + " is " + highestFinancialValue + ".");
            }
            else
            {
                Assert.IsTrue(highestFinancialValue > 0.0F, "Highest volume is zero or negative."); // actually volume due to incomplete migration of code from 2019
            }

            IList<float> candidateMoveFinancialValues = heuristic.FinancialValue.GetCandidateValuesWithDefaulting(defaultCoordinate);
            float beginFinancialValue = candidateMoveFinancialValues[0];
            Assert.IsTrue(highestFinancialValue >= beginFinancialValue);

            Assert.IsTrue(heuristic.FinancialValue.GetAcceptedValuesWithDefaulting(defaultCoordinate).Count >= 3);
            Assert.IsTrue(highestFinancialValueRatio > 0.99999);
            Assert.IsTrue(highestFinancialValueRatio < 1.00001);

            float recalculatedCurrentObjectiveFunction = heuristic.GetFinancialValue(heuristic.CurrentTrajectory, Constant.HeuristicDefault.CoordinateIndex);
            Assert.IsTrue(recalculatedCurrentObjectiveFunction <= highestFinancialValue);

            // only guaranteed for monotonic heuristics: hero, prescription enumeration, others depending on configuration
            IList<float> acceptedMoveFinancialValues = heuristic.FinancialValue.GetAcceptedValuesWithDefaulting(defaultCoordinate);
            float lastAcceptedFinancialValue = acceptedMoveFinancialValues[^1];
            if ((heuristic is Hero) || (heuristic is ThresholdAccepting))
            {
                Assert.IsTrue(highestFinancialValue == lastAcceptedFinancialValue);
                // may not hold for prescription enumerations initated from higher value heuristic solutions unreachable by the enumeration
                Assert.IsTrue(recalculatedCurrentObjectiveFunction >= beginFinancialValue); 
            }
            else
            {
                if (lastAcceptedFinancialValue >= 0.0F)
                {
                    Assert.IsTrue(highestFinancialValue > 0.95F * lastAcceptedFinancialValue);
                    float fraction = 0.85F;
                    if (heuristic is AutocorrelatedWalk)
                    {
                        fraction = 0.50F;
                    }
                    else if (heuristic is SimulatedAnnealing)
                    {
                        fraction = 0.72F;
                    }
                    Assert.IsTrue(recalculatedCurrentObjectiveFunction > fraction * beginFinancialValue, heuristic.GetName() + ": " + recalculatedCurrentObjectiveFunction + " is " + recalculatedCurrentObjectiveFunction / beginFinancialValue + " of " + beginFinancialValue + ".");
                }
                else
                {
                    Assert.IsTrue(highestFinancialValue >= lastAcceptedFinancialValue);
                }
            }

            float endCandidateFinancialValue = candidateMoveFinancialValues[^1];
            Assert.IsTrue(endCandidateFinancialValue <= highestFinancialValue);

            // check harvest schedule
            int firstThinningPeriod = bestTrajectory.Treatments.GetThinningPeriods()[0];
            foreach (KeyValuePair<FiaCode, IndividualTreeSelection> selectionForSpecies in bestTrajectory.TreeSelectionBySpecies)
            {
                IndividualTreeSelection bestTreeSelection = selectionForSpecies.Value;
                IndividualTreeSelection currentTreeSelection = heuristic.CurrentTrajectory.TreeSelectionBySpecies[selectionForSpecies.Key];
                Assert.IsTrue(bestTreeSelection.Capacity == currentTreeSelection.Capacity);
                Assert.IsTrue(bestTreeSelection.Count == currentTreeSelection.Count);
                for (int treeIndex = 0; treeIndex < bestTreeSelection.Count; ++treeIndex)
                {
                    Assert.IsTrue((bestTreeSelection[treeIndex] == Constant.RegenerationHarvestPeriod) || (bestTreeSelection[treeIndex] == firstThinningPeriod));
                    Assert.IsTrue((currentTreeSelection[treeIndex] == Constant.RegenerationHarvestPeriod) || (currentTreeSelection[treeIndex] == firstThinningPeriod));
                }
            }

            // check volumes
            Assert.IsTrue(firstThinningPeriod != Constant.RegenerationHarvestPeriod);
            bestTrajectory.GetMerchantableVolumes(out StandMerchantableVolume bestStandingVolume, out StandMerchantableVolume bestHarvestedVolume);
            heuristic.CurrentTrajectory.GetMerchantableVolumes(out StandMerchantableVolume currentStandingVolume, out StandMerchantableVolume currentHarvestedVolume);
            CutToLengthHarvest bestThinNpv = new();
            CutToLengthHarvest currentThinNpv = new();
            float previousBestCubicStandingVolume = Single.NaN;
            float previousCurrentCubicStandingVolume = Single.NaN;
            for (int periodIndex = 0; periodIndex < bestTrajectory.PlanningPeriods; ++periodIndex)
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

                if (periodIndex > 1)
                {
                    bestThinNpv = FinancialScenarios.Default.GetNetPresentThinningValue(bestTrajectory, Constant.HeuristicDefault.CoordinateIndex, periodIndex);
                    currentThinNpv = FinancialScenarios.Default.GetNetPresentThinningValue(bestTrajectory, Constant.HeuristicDefault.CoordinateIndex, periodIndex);
                }
                if (periodIndex == firstThinningPeriod)
                {
                    Assert.IsTrue(bestTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] >= 0.0F); // best selection with debug stand is no harvest
                    Assert.IsTrue(bestTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] <= 200.0F);

                    float thinVolumeScribner = bestTrajectory.GetTotalScribnerVolumeThinned(periodIndex);
                    float previousStandingVolumeScribner = bestTrajectory.GetTotalStandingScribnerVolume(periodIndex - 1);
                    Assert.IsTrue(bestTrajectory.GetTotalScribnerVolumeThinned(periodIndex) >= 0.0F);
                    if (thinVolumeScribner > previousStandingVolumeScribner)
                    {
                        Debugger.Break();
                    }
                    Assert.IsTrue(Constant.Default.ThinningPondValueMultiplier * thinVolumeScribner < previousStandingVolumeScribner, "Thinning volume: " + thinVolumeScribner + " MBF/ha in period " + periodIndex + " but previous period's standing volume is " + previousStandingVolumeScribner + " MBF/ha."); // allow for differences between short and long log scaling
                    Assert.IsTrue(bestHarvestedVolume.Scribner2Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner3Saw[periodIndex] >= 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner4Saw[periodIndex] >= 0.0F);

                    Assert.IsTrue(bestCubicThinningVolume <= previousBestCubicStandingVolume);
                    Assert.IsTrue(bestHarvestedVolume.Cubic2Saw[periodIndex] >= 0.0F, "2S cubic volume is negative.");
                    Assert.IsTrue(bestHarvestedVolume.Cubic3Saw[periodIndex] >= 0.0F, "3S cubic volume is negative.");
                    Assert.IsTrue(bestHarvestedVolume.Cubic4Saw[periodIndex] >= 0.0F, "4S cubic volume is negative.");

                    // TODO: investigate deeply negative NPVs
                    Assert.IsTrue(bestThinNpv.PondValue2SawPerHa >= 0.0F, "2S NPV is " + bestThinNpv.PondValue2SawPerHa + ".");
                    Assert.IsTrue(bestThinNpv.PondValue3SawPerHa >= 0.0F, "3S NPV is " + bestThinNpv.PondValue3SawPerHa + ".");
                    Assert.IsTrue(bestThinNpv.PondValue4SawPerHa >= 0.0F, "4S NPV is " + bestThinNpv.PondValue4SawPerHa + "."); // potentially fairly low when only 4S is removed

                    Assert.IsTrue(heuristic.CurrentTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] >= 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] <= 200.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.GetTotalScribnerVolumeThinned(periodIndex) >= 0.0F);
                    Assert.IsTrue(Constant.Default.ThinningPondValueMultiplier * heuristic.CurrentTrajectory.GetTotalScribnerVolumeThinned(periodIndex) < heuristic.CurrentTrajectory.GetTotalStandingScribnerVolume(periodIndex - 1)); // allow for differences between short and long log scaling

                    Assert.IsTrue(currentCubicThinningVolume >= 0.0F);
                    Assert.IsTrue(currentCubicThinningVolume <= previousCurrentCubicStandingVolume);

                    Assert.IsTrue(currentThinNpv.PondValue2SawPerHa >= 0.0F);
                    Assert.IsTrue(currentThinNpv.PondValue3SawPerHa >= 0.0F);
                    Assert.IsTrue(currentThinNpv.PondValue4SawPerHa >= 0.0F);
                }
                else
                {
                    // for now, harvest should occur only in the one indicated period
                    Assert.IsTrue(bestTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] == 0.0F);
                    Assert.IsTrue(bestTrajectory.GetTotalScribnerVolumeThinned(periodIndex) == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Scribner4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(bestCubicThinningVolume == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Cubic2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Cubic3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(bestHarvestedVolume.Cubic4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(bestThinNpv.PondValue2SawPerHa == 0.0F);
                    Assert.IsTrue(bestThinNpv.PondValue3SawPerHa == 0.0F);
                    Assert.IsTrue(bestThinNpv.PondValue4SawPerHa == 0.0F);

                    Assert.IsTrue(heuristic.CurrentTrajectory.GetTotalScribnerVolumeThinned(periodIndex) == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Scribner2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Scribner3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Scribner4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(heuristic.CurrentTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] == 0.0F);
                    Assert.IsTrue(currentCubicThinningVolume == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Cubic2Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Cubic3Saw[periodIndex] == 0.0F);
                    Assert.IsTrue(currentHarvestedVolume.Cubic4Saw[periodIndex] == 0.0F);

                    Assert.IsTrue(currentThinNpv.PondValue2SawPerHa == 0.0F);
                    Assert.IsTrue(currentThinNpv.PondValue3SawPerHa == 0.0F);
                    Assert.IsTrue(currentThinNpv.PondValue4SawPerHa == 0.0F);
                }

                if (periodIndex == 0)
                {
                    // zero merchantable on Nelder 1 at age 20 with Poudel 2018 net volume
                    Assert.IsTrue(bestCubicStandingVolume >= 0.0F, "Standing volume is " + bestCubicStandingVolume + " m³.");
                    Assert.IsTrue(bestTrajectory.GetTotalStandingScribnerVolume(periodIndex) >= 0.0F);
                    Assert.IsTrue(currentCubicStandingVolume >= 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.GetTotalStandingScribnerVolume(periodIndex) >= 0.0F);
                }
                else
                {
                    if (bestCubicStandingVolume == 0.0F)
                    {
                        Debugger.Break(); // trap for investigation to confirm heuristic (usually the genetic algorithm) is thinning all trees
                    }
                    Assert.IsTrue(bestCubicStandingVolume > 0.0F, "Standing volume is " + bestCubicStandingVolume + " m³.");
                    Assert.IsTrue(bestTrajectory.GetTotalStandingScribnerVolume(periodIndex) > 0.0F);
                    Assert.IsTrue(currentCubicStandingVolume > 0.0F);
                    Assert.IsTrue(heuristic.CurrentTrajectory.GetTotalStandingScribnerVolume(periodIndex) > 0.0F);

                    if (periodIndex != firstThinningPeriod)
                    {
                        // for now, assume monotonic increase in standing volumes except in harvest periods
                        Assert.IsTrue(bestCubicStandingVolume > previousBestCubicStandingVolume);
                        Assert.IsTrue(bestTrajectory.GetTotalStandingScribnerVolume(periodIndex) > bestTrajectory.GetTotalStandingScribnerVolume(periodIndex - 1));
                        Assert.IsTrue(currentCubicStandingVolume > previousCurrentCubicStandingVolume);
                        Assert.IsTrue(heuristic.CurrentTrajectory.GetTotalStandingScribnerVolume(periodIndex) > heuristic.CurrentTrajectory.GetTotalStandingScribnerVolume(periodIndex - 1));
                    }
                }

                previousBestCubicStandingVolume = bestCubicStandingVolume;
                previousCurrentCubicStandingVolume = currentCubicStandingVolume;
            }

            // check moves
            Assert.IsTrue(acceptedMoveFinancialValues.Count == candidateMoveFinancialValues.Count);
            Assert.IsTrue(acceptedMoveFinancialValues[0] == candidateMoveFinancialValues[0]);
            for (int moveIndex = 1; moveIndex < acceptedMoveFinancialValues.Count; ++moveIndex)
            {
                float acceptedFinancialValue = acceptedMoveFinancialValues[moveIndex];
                Assert.IsTrue(acceptedFinancialValue <= highestFinancialValue);
                // heuristics capable of reheating and tabu search do not have monotonically increasing accepted move values
                if ((heuristic is GeneticAlgorithm) || (heuristic is Hero))
                {
                    Assert.IsTrue(acceptedMoveFinancialValues[moveIndex - 1] <= acceptedFinancialValue);
                }
                Assert.IsTrue(candidateMoveFinancialValues[moveIndex] <= acceptedFinancialValue);
            }

            HeuristicMoveLog? moveLog = heuristic.GetMoveLog();
            if (moveLog != null)
            {
                string moveCsvHeader = moveLog.GetCsvHeader("prefix");
                Assert.IsTrue(String.IsNullOrWhiteSpace(moveCsvHeader) == false);

                for (int moveIndex = 0; moveIndex < acceptedMoveFinancialValues.Count; ++moveIndex)
                {
                    string moveCsvValues = moveLog.GetCsvValues(defaultCoordinate, moveIndex);
                    Assert.IsTrue(String.IsNullOrWhiteSpace(moveCsvValues) == false);
                }
            }

            // check parameters
            HeuristicParameters parameters = heuristic.GetParameters();
            string parameterCsvHeader = parameters.GetCsvHeader();
            string parameterCsvValues = parameters.GetCsvValues();

            Assert.IsTrue(String.IsNullOrWhiteSpace(parameterCsvHeader) == false);
            Assert.IsTrue(String.IsNullOrWhiteSpace(parameterCsvValues) == false);

            // check performance counters
            Assert.IsTrue(perfCounters.Duration > TimeSpan.Zero);
            Assert.IsTrue(perfCounters.GrowthModelTimesteps > 0);
            Assert.IsTrue(perfCounters.MovesAccepted >= 0); // random guessing may or may not yield improving or disimproving moves
            Assert.IsTrue(perfCounters.MovesRejected >= 0);
            Assert.IsTrue((perfCounters.MovesAccepted + perfCounters.MovesRejected) >= 0);
            
            int treeRecordCount = bestTrajectory.GetInitialTreeRecordCount();
            if (heuristic is GeneticAlgorithm genetic)
            {
                GeneticParameters geneticParameters = genetic.GetParameters();
                Assert.IsTrue(perfCounters.TreesRandomizedInConstruction > 0.4F * geneticParameters.PopulationSize * treeRecordCount);
                Assert.IsTrue(perfCounters.TreesRandomizedInConstruction < 0.6F * geneticParameters.PopulationSize * treeRecordCount);
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

        private static void Verify(OrganonStandTrajectory thinnedTrajectory, ExpectedStandTrajectory expectedTrajectory)
        {
            for (int periodIndex = 0; periodIndex < thinnedTrajectory.PlanningPeriods; ++periodIndex)
            {
                if ((periodIndex == expectedTrajectory.FirstThinPeriod) || 
                    ((expectedTrajectory.SecondThinPeriod != Constant.NoThinPeriod) && (periodIndex == expectedTrajectory.SecondThinPeriod)) ||
                    ((expectedTrajectory.ThirdThinPeriod != Constant.NoThinPeriod) && (periodIndex == expectedTrajectory.ThirdThinPeriod)))
                {
                    OrganonStandDensity? standDensityBeforeThin = thinnedTrajectory.DensityByPeriod[periodIndex - 1];
                    AssertNullable.IsNotNull(standDensityBeforeThin);

                    Assert.IsTrue(thinnedTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] > 0.0F);
                    Assert.IsTrue(thinnedTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] < standDensityBeforeThin.BasalAreaPerAcre); // assume <50% thin by volume
                    Assert.IsTrue(thinnedTrajectory.GetTotalScribnerVolumeThinned(periodIndex) >= expectedTrajectory.MinimumHarvestMbfPerHa[periodIndex]);
                    Assert.IsTrue(thinnedTrajectory.GetTotalScribnerVolumeThinned(periodIndex) <= PublicApi.VolumeTolerance * expectedTrajectory.MinimumHarvestMbfPerHa[periodIndex]);
                }
                else
                {
                    Assert.IsTrue(thinnedTrajectory.Treatments.BasalAreaThinnedByPeriod[periodIndex] == 0.0F);
                    Assert.IsTrue(thinnedTrajectory.GetTotalScribnerVolumeThinned(periodIndex) == 0.0F);
                }
            }
        }

        private static void Verify(OrganonStandTrajectory trajectory, ExpectedStandTrajectory expectedTrajectory, int timeStepInYears)
        {
            trajectory.RecalculateThinningVolumeIfNeeded(0);
            trajectory.RecalculateStandingVolumeIfNeeded(0);

            Assert.IsTrue(trajectory.Treatments.BasalAreaThinnedByPeriod.Count == expectedTrajectory.Length);
            Assert.IsTrue(trajectory.Treatments.BasalAreaThinnedByPeriod[0] == 0.0F);
            Assert.IsTrue(trajectory.GetTotalScribnerVolumeThinned(0) == 0.0F);
            foreach (TreeSpeciesMerchantableVolume thinVolumeForSpecies in trajectory.ThinningVolumeBySpecies.Values)
            {
                Assert.IsTrue(thinVolumeForSpecies.Scribner2Saw.Length == expectedTrajectory.Length);
                Assert.IsTrue(thinVolumeForSpecies.Scribner3Saw.Length == expectedTrajectory.Length);
                Assert.IsTrue(thinVolumeForSpecies.Scribner4Saw.Length == expectedTrajectory.Length);
            }
            Assert.IsTrue(String.IsNullOrEmpty(trajectory.Name) == false);
            Assert.IsTrue(trajectory.PeriodLengthInYears == timeStepInYears);
            Assert.IsTrue(trajectory.PlanningPeriods == expectedTrajectory.Length); // BUGBUG: clean off by one semantic

            Assert.IsTrue(trajectory.GetFirstThinPeriod() == expectedTrajectory.FirstThinPeriod);
            Assert.IsTrue(trajectory.GetSecondThinPeriod() == expectedTrajectory.SecondThinPeriod);

            IList<int> thinningPeriods = trajectory.Treatments.GetHarvestPeriods();
            Assert.IsTrue(thinningPeriods[^1] == Constant.RegenerationHarvestPeriod);
            if (expectedTrajectory.FirstThinPeriod != Constant.NoThinPeriod)
            {
                Assert.IsTrue(thinningPeriods[0] == expectedTrajectory.FirstThinPeriod);
                if (expectedTrajectory.ThirdThinPeriod != Constant.NoThinPeriod)
                {
                    Assert.IsTrue(thinningPeriods[1] == expectedTrajectory.SecondThinPeriod);
                    Assert.IsTrue(thinningPeriods[2] == expectedTrajectory.ThirdThinPeriod);
                    Assert.IsTrue(thinningPeriods.Count == 4);
                }
                else if (expectedTrajectory.SecondThinPeriod != Constant.NoThinPeriod)
                {
                    Assert.IsTrue(thinningPeriods[1] == expectedTrajectory.SecondThinPeriod);
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

            PublicApi.Verify(trajectory.TreeSelectionBySpecies, expectedTrajectory);

            for (int periodIndex = 0; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
            {
                OrganonStandDensity? standDensity = trajectory.DensityByPeriod[periodIndex];
                AssertNullable.IsNotNull(standDensity);
                Assert.IsTrue(standDensity.BasalAreaPerAcre > 0.0F);
                Assert.IsTrue(standDensity.BasalAreaPerAcre <= TestConstant.Maximum.TreeBasalAreaLarger);

                trajectory.RecalculateThinningVolumeIfNeeded(periodIndex);
                trajectory.RecalculateStandingVolumeIfNeeded(periodIndex);

                Assert.IsTrue(trajectory.GetTotalStandingCubicVolume(periodIndex) > expectedTrajectory.MinimumStandingCubicM3PerHa[periodIndex]);
                Assert.IsTrue(trajectory.GetTotalStandingCubicVolume(periodIndex) < PublicApi.VolumeTolerance * expectedTrajectory.MinimumStandingCubicM3PerHa[periodIndex]);
                Assert.IsTrue(trajectory.GetTotalCubicVolumeThinned(periodIndex) >= expectedTrajectory.MinimumHarvestCubicM3PerHa[periodIndex]);
                Assert.IsTrue(trajectory.GetTotalCubicVolumeThinned(periodIndex) <= PublicApi.VolumeTolerance * expectedTrajectory.MinimumHarvestCubicM3PerHa[periodIndex]);

                Assert.IsTrue(trajectory.GetTotalStandingScribnerVolume(periodIndex) > expectedTrajectory.MinimumStandingMbfPerHa[periodIndex]);
                Assert.IsTrue(trajectory.GetTotalStandingScribnerVolume(periodIndex) < PublicApi.VolumeTolerance * expectedTrajectory.MinimumStandingMbfPerHa[periodIndex]);
                Assert.IsTrue(trajectory.GetTotalScribnerVolumeThinned(periodIndex) >= expectedTrajectory.MinimumHarvestMbfPerHa[periodIndex]);
                Assert.IsTrue(trajectory.GetTotalScribnerVolumeThinned(periodIndex) <= PublicApi.VolumeTolerance * expectedTrajectory.MinimumHarvestMbfPerHa[periodIndex]);

                OrganonStand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information missing for period " + periodIndex + ".");
                float qmdInCm = stand.GetQuadraticMeanDiameterInCentimeters();
                float topHeight = stand.GetTopHeightInMeters();
                int treeRecords = stand.GetTreeRecordCount();

                Assert.IsTrue((stand.Name != null) && (trajectory.Name != null) && stand.Name.StartsWith(trajectory.Name));
                Assert.IsTrue(qmdInCm > expectedTrajectory.MinimumQmdInCm[periodIndex]);
                Assert.IsTrue(qmdInCm < PublicApi.QmdTolerance * expectedTrajectory.MinimumQmdInCm[periodIndex]);
                Assert.IsTrue(topHeight > expectedTrajectory.MinimumTopHeightInM[periodIndex]);
                Assert.IsTrue(topHeight < PublicApi.TopHeightTolerance * expectedTrajectory.MinimumTopHeightInM[periodIndex]);
                Assert.IsTrue(treeRecords > 0);
                Assert.IsTrue(treeRecords < 666);

                // TODO: check qmd against QMD from basal area
            }
        }

        private static SilviculturalPrescriptionPool Verify(StandTrajectories trajectories, StandTrajectoryCoordinate coordinate)
        {
            Assert.IsTrue(trajectories.TryGetSelfOrFindNearestNeighbor(coordinate, out SilviculturalPrescriptionPool? eliteSolutions, out StandTrajectoryCoordinate? eliteSolutionsPosition));
            Assert.IsTrue(eliteSolutions!.High != null);
            Assert.IsTrue(eliteSolutions.Low.Trajectory != null);
            Assert.IsTrue(eliteSolutions.SolutionsInPool == Math.Min(eliteSolutions.SolutionsAccepted, TestConstant.SolutionPoolSize));
            Assert.IsTrue(coordinate == eliteSolutionsPosition!);

            for (int solutionIndex = 0; solutionIndex < eliteSolutions.SolutionsInPool; ++solutionIndex)
            {
                // check distance matrix: should be symmetric with a zero diagonal
                Assert.IsTrue(eliteSolutions.DistanceMatrix[solutionIndex, solutionIndex] == 0);
                for (int neighborIndex = solutionIndex + 1; neighborIndex < eliteSolutions.SolutionsInPool; ++neighborIndex)
                {
                    int distance1 = eliteSolutions.DistanceMatrix[solutionIndex, neighborIndex];
                    int distance2 = eliteSolutions.DistanceMatrix[neighborIndex, solutionIndex];
                    Assert.IsTrue(distance1 == distance2);
                }

                // check neighbors
                int nearestNeighborIndex = eliteSolutions.NearestNeighborIndex[solutionIndex];
                if (nearestNeighborIndex != SolutionPool.UnknownNeighbor)
                {
                    int nearestNeighborDistance = eliteSolutions.DistanceMatrix[solutionIndex, nearestNeighborIndex];

                    Assert.IsTrue(nearestNeighborIndex != solutionIndex);
                    Assert.IsTrue((nearestNeighborDistance > 0) || (nearestNeighborDistance == SolutionPool.UnknownDistance));
                }
            }

            IndividualTreeSelectionBySpecies eliteTreeSelection = eliteSolutions.GetRandomEliteTreeSelection();
            Assert.IsTrue(eliteTreeSelection.Count > 0);

            return eliteSolutions;
        }

        private static void Verify(SnagDownLogTable snagsAndDownLogs, OrganonStandTrajectory trajectory)
        {
            Assert.IsTrue((snagsAndDownLogs.DiameterClasses == (int)Constant.Bucking.DefaultMaximumFinalHarvestDiameterInCentimeters + 1) || 
                          (snagsAndDownLogs.DiameterClasses == (int)Constant.Bucking.DefaultMaximumThinningDiameterInCentimeters + 1));
            Assert.IsTrue(snagsAndDownLogs.DiameterClassSizeInCentimeters == Constant.Bucking.DiameterClassSizeInCentimeters);
            Assert.IsTrue((snagsAndDownLogs.MaximumDiameterInCentimeters == Constant.Bucking.DefaultMaximumFinalHarvestDiameterInCentimeters) ||
                          (snagsAndDownLogs.MaximumDiameterInCentimeters == Constant.Bucking.DefaultMaximumThinningHeightInMeters));
            Assert.IsTrue(snagsAndDownLogs.Periods == trajectory.PlanningPeriods);

            Assert.IsTrue(snagsAndDownLogs.LogQmdInCentimetersByPeriod.Length == snagsAndDownLogs.Periods);
            Assert.IsTrue(snagsAndDownLogs.LogsPerHectareByPeriod.Length == snagsAndDownLogs.Periods);
            Assert.IsTrue(snagsAndDownLogs.LogsPerHectareBySpeciesAndDiameterClass.Count == 1);
            Assert.IsTrue(snagsAndDownLogs.LogsPerHectareBySpeciesAndDiameterClass[FiaCode.PseudotsugaMenziesii].GetLength(0) == snagsAndDownLogs.Periods);
            Assert.IsTrue(snagsAndDownLogs.LogsPerHectareBySpeciesAndDiameterClass[FiaCode.PseudotsugaMenziesii].GetLength(1) == snagsAndDownLogs.DiameterClasses);

            Assert.IsTrue(snagsAndDownLogs.SnagQmdInCentimetersByPeriod.Length == snagsAndDownLogs.Periods);
            Assert.IsTrue(snagsAndDownLogs.SnagsPerHectareByPeriod.Length == snagsAndDownLogs.Periods);
            Assert.IsTrue(snagsAndDownLogs.SnagsPerHectareBySpeciesAndDiameterClass.Count == 1);
            Assert.IsTrue(snagsAndDownLogs.SnagsPerHectareBySpeciesAndDiameterClass[FiaCode.PseudotsugaMenziesii].GetLength(0) == snagsAndDownLogs.Periods);
            Assert.IsTrue(snagsAndDownLogs.SnagsPerHectareBySpeciesAndDiameterClass[FiaCode.PseudotsugaMenziesii].GetLength(1) == snagsAndDownLogs.DiameterClasses);

            OrganonStandDensity? standDensity = trajectory.DensityByPeriod[0];
            AssertNullable.IsNotNull(standDensity);
            float initialTreesPerHectare = Constant.AcresPerHectare * standDensity.TreesPerAcre;
            float initialStemsPerHectare = initialTreesPerHectare + snagsAndDownLogs.LogsPerHectareByPeriod[0] + snagsAndDownLogs.SnagsPerHectareByPeriod[0];
            for (int period = 0; period < snagsAndDownLogs.Periods; ++period)
            {
                standDensity = trajectory.DensityByPeriod[period];
                AssertNullable.IsNotNull(standDensity);

                float logsPerHectare = snagsAndDownLogs.LogsPerHectareByPeriod[period];
                float snagPerHectare = snagsAndDownLogs.SnagsPerHectareByPeriod[period];
                float treesPerHectare = Constant.AcresPerHectare * standDensity.TreesPerAcre;
                float stemsPerHectare = treesPerHectare + snagPerHectare + logsPerHectare;

                Assert.IsTrue(snagsAndDownLogs.LogQmdInCentimetersByPeriod[period] >= 0.0F);
                Assert.IsTrue(logsPerHectare >= 0.0F);

                Assert.IsTrue(snagsAndDownLogs.SnagQmdInCentimetersByPeriod[period] >= 0.0F);
                Assert.IsTrue(snagPerHectare >= 0.0F);

                // for now, assume no ingrowth
                Assert.IsTrue(initialStemsPerHectare >= stemsPerHectare);
            }
        }

        private static void Verify(IndividualTreeSelectionBySpecies individualTreeSelectionBySpecies, ExpectedStandTrajectory expectedTrajectory)
        {
            int outOfRangeTrees = 0;
            int treesSelected = 0;
            foreach (IndividualTreeSelection individualTreeSelection in individualTreeSelectionBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < individualTreeSelection.Count; ++treeIndex)
                {
                    int treeSelection = individualTreeSelection[treeIndex];
                    bool isOutOfRange = (treeSelection != 0) && (treeSelection != expectedTrajectory.FirstThinPeriod);
                    if (expectedTrajectory.SecondThinPeriod != Constant.NoThinPeriod)
                    {
                        isOutOfRange &= treeSelection != expectedTrajectory.SecondThinPeriod;
                    }
                    if (expectedTrajectory.ThirdThinPeriod != Constant.NoThinPeriod)
                    {
                        isOutOfRange &= treeSelection != expectedTrajectory.ThirdThinPeriod;
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
            Assert.IsTrue(treesSelected >= expectedTrajectory.MinimumTreesSelected);
            Assert.IsTrue(treesSelected <= expectedTrajectory.MaximumTreesSelected);
        }

        private class ExpectedStandTrajectory
        {
            public int FirstThinPeriod { get; init; }
            public int SecondThinPeriod { get; init; }
            public int ThirdThinPeriod { get; init; }
            public int MaximumTreesSelected { get; init; }
            public int MinimumTreesSelected { get; init; }

            public float[] MinimumHarvestCubicM3PerHa { get; init; }
            public float[] MinimumHarvestMbfPerHa { get; init; }
            public float[] MinimumQmdInCm { get; init; }
            public float[] MinimumStandingCubicM3PerHa { get; init; }
            public float[] MinimumStandingMbfPerHa { get; init; }
            public float[] MinimumTopHeightInM { get; init; }

            public ExpectedStandTrajectory()
            {
                this.FirstThinPeriod = Constant.NoThinPeriod;
                this.SecondThinPeriod = Constant.NoThinPeriod;
                this.ThirdThinPeriod = Constant.NoThinPeriod;
                this.MaximumTreesSelected = 0;
                this.MinimumTreesSelected = 0;
                this.MinimumHarvestCubicM3PerHa = Array.Empty<float>();
                this.MinimumHarvestMbfPerHa = Array.Empty<float>();
                this.MinimumQmdInCm = Array.Empty<float>();
                this.MinimumStandingCubicM3PerHa = Array.Empty<float>();
                this.MinimumStandingMbfPerHa = Array.Empty<float>();
                this.MinimumTopHeightInM = Array.Empty<float>();
            }

            public int Length
            {
                get { return this.MinimumHarvestCubicM3PerHa.Length; }
            }
        }
    }
}
