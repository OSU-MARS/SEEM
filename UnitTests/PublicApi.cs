using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Osu.Cof.Organon.Test
{
    [TestClass]
    public class PublicApi : OrganonTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem("HPNF plot 1.xlsx")]
        public void HuffmanPeakNobleFir()
        {
            // Data were provided by the HJ Andrews Experimental Forest research program, funded by the National Science Foundation's 
            // Long-Term Ecological Research Program (DEB 1440409), US Forest Service Pacific Northwest Research Station, and Oregon 
            // State University.
            OrganonVariant variant = new OrganonVariant(Variant.Swo); // allows mapping ABAM -> ABGR and ABPR -> ABCO
            PspStand huffmanPeakPlot1 = new PspStand("HPNF plot 1.xlsx", "HPNF", 0.2F);
            TestStand stand = huffmanPeakPlot1.ToOrganonStand(variant, 0, 55.0F);

            OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
            TestStand initialTreeData = stand.Clone();
            TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath(stand.TreeRecordCount);

            float BABT = 0.0F;
            float[] BART = new float[5];
            float[,] CALIB = this.CreateCalibrationArray(); 
            float[] PN = new float[5];
            if (configuration.IsEvenAge)
            {
                // stand error if less than one year to grow to breast height
                stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
            }
            float[] YSF = new float[5];
            float[] YST = new float[5];

            TestStandDensity density = new TestStandDensity(stand, variant);
            using StreamWriter densityWriter = density.WriteToCsv("HPNF plot 1 density.csv", variant, 1980);
            using StreamWriter treeGrowthWriter = stand.WriteTreesToCsv("HPNF plot 1 tree growth.csv", variant, 1980);
            stand.LogAsCsv(this.TestContext, variant, 1980, true);
            for (int simulationStep = 0; simulationStep < 7; ++simulationStep)
            {
                StandGrowth.EXECUTE(simulationStep, configuration, stand, CALIB, PN, YSF, BABT, BART, YST);
                treeGrowth.AccumulateGrowthAndMortality(stand);

                int endYear = 1980 + variant.GetEndYear(simulationStep);
                huffmanPeakPlot1.AddIngrowth(endYear, stand, density);
                density = new TestStandDensity(stand, variant);
                density.WriteToCsv(densityWriter, variant, endYear);
                stand.WriteTreesToCsv(treeGrowthWriter, variant, endYear);
                this.Verify(ExpectedTreeChanges.DiameterGrowthOrNoChange | ExpectedTreeChanges.HeightGrowthOrNoChange, stand, variant);
            }

            this.Verify(ExpectedTreeChanges.ExpansionFactorConservedOrIncreased | ExpectedTreeChanges.DiameterGrowthOrNoChange | ExpectedTreeChanges.HeightGrowthOrNoChange, treeGrowth, initialTreeData, stand);
            this.Verify(CALIB);
        }

        [TestMethod]
        public void OrganonStandGrowthApi()
        {
            TestStand.LogCsvHeader(this.TestContext);
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                // get crown closure
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                StandGrowth.CROWN_CLOSURE(variant, stand, variant.SpeciesGroupCount, out float crownClosure);
                Assert.IsTrue(crownClosure >= 0.0F);
                Assert.IsTrue(crownClosure <= TestConstant.Maximum.CrownClosure);
                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth | ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                // recalculate heights and crown ratios for all trees
                float[,] CALIB = this.CreateCalibrationArray();
                StandGrowth.INGRO_FILL(variant, stand, stand.TreeRecordCount, CALIB);
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

                stand.LogAsCsv(this.TestContext, variant, 0, false);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    StandGrowth.EXECUTE(simulationStep, configuration, stand, CALIB, PN, YSF, BABT, BART, YST);
                    treeGrowth.AccumulateGrowthAndMortality(stand);
                    stand.LogAsCsv(this.TestContext, variant, variant.GetEndYear(simulationStep), false);
                    this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, OrganonWarnings.LessThan50TreeRecords, stand, variant);
                }

                this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, treeGrowth, initialTreeData, stand);
                this.Verify(CALIB);
            }
        }
    }
}
