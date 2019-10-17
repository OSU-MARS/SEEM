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
            this.TestContext.WriteLine("variant,simulation step,tree,species,species group,user data,DBH,height,expansion factor,dead expansion factor,MG expansion factor,crown ratio,shadow crown ratio 0,1,2");
            foreach (Variant variant in TestConstant.Variants)
            {
                // get crown closure
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                TestStand stand = this.CreateDefaultStand(variant);
                StandGrowth.CROWN_CLOSURE(variant, stand, variantCapabilities.SpeciesGroupCount, out float crownClosure);
                Assert.IsTrue(crownClosure >= 0.0F);
                Assert.IsTrue(crownClosure <= TestConstant.Maximum.CrownClosure);
                this.Verify(stand, variantCapabilities);

                // recalculate heights and crown ratios for all trees
                float[,] ACALIB = this.CreateCalibrationArray();
                StandGrowth.INGRO_FILL(variant, stand, stand.TreeRecordsInUse, ACALIB);
                this.Verify(stand, variantCapabilities);

                // run Organon growth simulation
                stand = this.CreateDefaultStand(variant);
                TestStand initialTreeData = stand.Clone();
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath(stand.TreeRecordsInUse);

                float BABT = 0.0F; // (DOUG? basal area before thinning?)
                float[] BART = new float[5]; // (DOUG? basal area removed by thinning?)
                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                if (configuration.IsEvenAge)
                {
                    // stand error if less than one year to grow to breast height
                    stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
                }
                float[] YSF = new float[5]; // (DOUG?)
                float[] YST = new float[5]; // (DOUG?)

                stand.GetEmptyArrays(out float[] dbhInInchesAtEndOfCycle, out float[] heightInFeetAtEndOfCycle, 
                                     out float[] crownRatioAtEndOfCycle, out float[] shadowCrownRatioAtEndOfCycle, 
                                     out float[] expansionFactorAtEndOfCycle);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    stand.ToArrays(out float[] dbhInInchesAtStartOfCycle, out float[] heightInFeetAtStartOfCycle,
                                   out float[] crownRatioAtStartOfCycle, out float[] expansionFactorAtStartOfCycle,
                                   out float[] shadowCrownRatioAtStartOfCycle, out float[] diameterGrowth, out float[] heightGrowth,
                                   out float[] crownChange, out float[] shadowCrownRatioChange);
                    StandGrowth.EXECUTE(simulationStep, configuration, stand, ACALIB, PN, YSF, BABT, BART, YST,
                                        diameterGrowth, heightGrowth, crownChange, shadowCrownRatioChange, out int treeCountAtEndOfCycle, 
                                        dbhInInchesAtEndOfCycle, heightInFeetAtEndOfCycle, crownRatioAtEndOfCycle, shadowCrownRatioAtEndOfCycle, expansionFactorAtEndOfCycle);
                    treeGrowth.Accumulate(dbhInInchesAtStartOfCycle, dbhInInchesAtEndOfCycle, heightInFeetAtStartOfCycle, heightInFeetAtEndOfCycle,
                                          stand.DeadExpansionFactor);
                    stand.FromArrays(dbhInInchesAtEndOfCycle, heightInFeetAtEndOfCycle, crownRatioAtEndOfCycle, 
                                     expansionFactorAtEndOfCycle, shadowCrownRatioAtEndOfCycle, diameterGrowth, heightGrowth,
                                     crownChange, shadowCrownRatioChange);
                    stand.WriteAsCsv(this.TestContext, variant, simulationStep);
                    this.Verify(stand, variantCapabilities);
                }

                this.Verify(treeGrowth, initialTreeData, stand);
            }
        }
    }
}
