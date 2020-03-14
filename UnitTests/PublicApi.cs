using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Organon.Data;
using Osu.Cof.Organon.Heuristics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Osu.Cof.Organon.Test
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
            Stand stand = nelder.ToStand(130.0F, 10);

            Objective netPresentValue = new Objective()
            {
                IsNetPresentValue = true,
                VolumeUnits = VolumeUnits.ScribnerBoardFeetPerAcre
            };
            Objective volume = new Objective();

            int harvestPeriods = 2;
            int planningPeriods = 9;
            GeneticAlgorithm genetic = new GeneticAlgorithm(stand, configuration, harvestPeriods, planningPeriods, netPresentValue)
            {
                EndStandardDeviation = 0.01F, // US$ 10 NPV
                PopulationSize = 5,
                MaximumGenerations = 10,
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

            this.VerifyObjectiveFunction(genetic);
            this.VerifyObjectiveFunction(deluge);
            this.VerifyObjectiveFunction(recordTravel);
            this.VerifyObjectiveFunction(annealer);
            this.VerifyObjectiveFunction(thresholdAcceptor);
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
                float crownCompetitionFactor = StandDensity.GetCrownCompetition(variant, stand);
                Assert.IsTrue(crownCompetitionFactor >= 0.0F);
                Assert.IsTrue(crownCompetitionFactor <= TestConstant.Maximum.CrownCompetitionFactor);
                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // recalculate heights and crown ratios for all trees
                Dictionary<FiaCode, float[]> CALIB = configuration.CreateSpeciesCalibration();
                Organon.SetIngrowthHeightAndCrownRatio(variant, stand, stand.TreeRecordCount, CALIB);
                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // run Organon growth simulation
                stand = this.CreateDefaultStand(configuration);
                TestStand initialTreeData = stand.Clone();
                TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath(stand.TreeRecordCount);

                float BABT = 0.0F; // (DOUG? basal area before thinning?)
                float[] BART = new float[5]; // (DOUG? basal area removed by thinning?)
                float[] PN = new float[5]; // (DOUG?)
                if (configuration.IsEvenAge)
                {
                    // stand error if less than one year to grow to breast height
                    stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
                }
                float[] YSF = new float[5]; // (DOUG?)
                float[] YST = new float[5]; // (DOUG?)

                stand.WriteTreesAsCsv(this.TestContext, variant, 0, false);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    Organon.Grow(simulationStep, configuration, stand, CALIB, PN, YSF, BABT, BART, YST);
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
            Assert.IsTrue(heuristic.ObjectiveFunctionByIteration.Count > 4);

            Assert.IsTrue(bestObjectiveFunctionRatio > 0.99999);
            Assert.IsTrue(bestObjectiveFunctionRatio < 1.00001);
            // currently, this check can fail for genetic algorithms when the best individual doesn't breed and disimprovement results
            // TODO: either change GA to guarantee best individual breeds or relax this check in the GA case
            Assert.IsTrue(endObjectiveFunctionRatio > 0.99999);
            Assert.IsTrue(endObjectiveFunctionRatio < 1.00001);
        }
    }
}
