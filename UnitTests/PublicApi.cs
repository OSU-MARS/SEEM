using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Osu.Cof.Organon.Test
{
    [TestClass]
    public class PublicApi : OrganonTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void OrganonStandGrowthApi()
        {
            this.TestContext.WriteLine("variant,simulation step,tree,species,species group,DBH,height,expansion factor,dead expansion factor,crown ratio,diameter growth,height growth");
            foreach (Variant variant in TestConstant.Variants)
            {
                // get crown closure
                VariantCapabilities variantCapabilities = new VariantCapabilities(variant);
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                StandGrowth.CROWN_CLOSURE(variant, stand, variantCapabilities.SpeciesGroupCount, out float crownClosure);
                Assert.IsTrue(crownClosure >= 0.0F);
                Assert.IsTrue(crownClosure <= TestConstant.Maximum.CrownClosure);
                this.Verify(stand, ExpectedTreeChanges.NoDiameterOrHeightGrowth, variantCapabilities);

                // recalculate heights and crown ratios for all trees
                float[,] CALIB = this.CreateCalibrationArray();
                StandGrowth.INGRO_FILL(variant, stand, stand.TreeRecordCount, CALIB);
                this.Verify(stand, ExpectedTreeChanges.NoDiameterOrHeightGrowth, variantCapabilities);

                // run Organon growth simulation
                stand = this.CreateDefaultStand(configuration);
                TestStand initialTreeData = stand.Clone();
                TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath(stand.TreeRecordCount);

                float BABT = 0.0F; // (DOUG? basal area before thinning?)
                float[] BART = new float[5]; // (DOUG? basal area removed by thinning?)
                float[] CCH = new float[41]; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                if (configuration.IsEvenAge)
                {
                    // stand error if less than one year to grow to breast height
                    stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
                }
                float[] YSF = new float[5]; // (DOUG?)
                float[] YST = new float[5]; // (DOUG?)

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    StandGrowth.EXECUTE(simulationStep, configuration, stand, CALIB, PN, YSF, BABT, BART, YST);
                    treeGrowth.AccumulateGrowthAndMortality(stand);
                    stand.WriteAsCsv(this.TestContext, variant, simulationStep);
                    this.Verify(stand, ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, variantCapabilities);
                }

                this.Verify(treeGrowth, initialTreeData, stand);
                this.Verify(CALIB);
            }
        }
    }
}
