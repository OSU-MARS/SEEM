using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Osu.Cof.Organon.Test
{
    // Data were provided by the HJ Andrews Experimental Forest research program, funded by the National Science Foundation's 
    // Long-Term Ecological Research Program (DEB 1440409), US Forest Service Pacific Northwest Research Station, and Oregon 
    // State University.
    [TestClass]
    public class PublicApi : OrganonTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem("HPNF.xlsx")]
        //[DeploymentItem("HPNF plot 1.xlsx")]
        public void HuffmanPeakNobleFir()
        {
            string plotFileName = "HPNF.xlsx";
            //string plotFileName = "HPNF plot 1.xlsx";
            PspStand huffmanPeak = new PspStand(plotFileName, "HPNF", 0.2F);
            OrganonVariant variant = new OrganonVariant(Variant.Swo); // SWO allows mapping ABAM -> ABGR and ABPR -> ABCO
            TestStand stand = huffmanPeak.ToOrganonStand(variant, 80.0F);
            int startYear = 1980;
            stand.WriteCompetitionAsCsv("HPNF initial competition.csv", variant, startYear);
            this.GrowPspStand(huffmanPeak, stand, variant, startYear, 2015, Path.GetFileNameWithoutExtension(plotFileName));

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
        public void OrganonStandGrowthApi()
        {
            TestStand.WriteTreeHeader(this.TestContext);
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

                stand.WriteTreesAsCsv(this.TestContext, variant, 0, false);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    StandGrowth.EXECUTE(simulationStep, configuration, stand, CALIB, PN, YSF, BABT, BART, YST);
                    treeGrowth.AccumulateGrowthAndMortality(stand);
                    stand.WriteTreesAsCsv(this.TestContext, variant, variant.GetEndYear(simulationStep), false);
                    this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, OrganonWarnings.LessThan50TreeRecords, stand, variant);
                }

                this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, treeGrowth, initialTreeData, stand);
                this.Verify(CALIB);
            }
        }

        [TestMethod]
        [DeploymentItem("RS39 lower half.xlsx")]
        public void RS39()
        {
            string plotFileName = "RS39 lower half.xlsx";
            PspStand rs39 = new PspStand(plotFileName, "RS39 lower half", 0.154441F);
            OrganonVariant variant = new OrganonVariant(Variant.Nwo);
            TestStand stand = rs39.ToOrganonStand(variant, 105.0F);
            int startYear = 1992;
            stand.WriteCompetitionAsCsv("RS39 lower half initial competition.csv", variant, startYear);
            this.GrowPspStand(rs39, stand, variant, startYear, 2019, Path.GetFileNameWithoutExtension(plotFileName));

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
    }
}
