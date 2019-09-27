using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Osu.Cof.Organon.Test
{
    [TestClass]
    public class PublicApi : OrganonTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Execute2Api()
        {
            this.TestContext.WriteLine("variant,simulation step,tree,species,species group,user data,DBH,height,expansion factor,dead expansion factor,MG expansion factor,crown ratio,shadow crown ratio 0,1,2");
            foreach (Variant variant in TestConstant.Variants)
            {
                // get crown closure
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                TreeData treeDataForStep = this.CreateDefaultTrees(variant);
                treeDataForStep.ToArrays(out int[] species, out float[] dbhInInches, out float[] heightInFeet, out float[] crownRatio, out float[] expansionFactor, out float[] shadowCrownRatio);
                Execute2.CROWN_CLOSURE(variant, treeDataForStep.UsedRecordCount, variantCapabilities.SpeciesGroupCount, species, dbhInInches, heightInFeet,
                                       crownRatio, shadowCrownRatio, expansionFactor, out float crownClosure);
                Assert.IsTrue(crownClosure >= 0.0F);
                Assert.IsTrue(crownClosure <= TestConstant.Maximum.CrownClosure);
                treeDataForStep.FromArrays(species, dbhInInches, heightInFeet, crownRatio, expansionFactor, shadowCrownRatio);
                this.Verify(treeDataForStep, variantCapabilities);

                // recalculate crown ratios for all trees
                float[,] ACALIB = this.CreateCalibrationArray();
                Execute2.INGRO_FILL(variant, treeDataForStep.UsedRecordCount, treeDataForStep.UsedRecordCount, species, treeDataForStep.PrimarySiteIndex, treeDataForStep.MortalitySiteIndex, ACALIB, dbhInInches, heightInFeet, crownRatio, expansionFactor);
                this.Verify(treeDataForStep, variantCapabilities);

                // run Organon growth simulation
                treeDataForStep = this.CreateDefaultTrees(variant, 2000);
                TreeData initialTreeData = treeDataForStep.Clone();
                OrganonErrorsAndWarnings errorsAndWarnings = new OrganonErrorsAndWarnings(treeDataForStep.UsedRecordCount);
                OrganonOptions options = new OrganonOptions(variant);
                TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath(treeDataForStep.UsedRecordCount);

                float BABT = 0.0F; // (DOUG? basal area before thinning?)
                float[] BART = new float[5]; // (DOUG? basal area removed by thinning?)
                int breastHeightAge = 0;
                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                int[] featureFlagsArray = options.ToFeatureFlagsArray();
                int NPTS = 1;
                float[] PN = new float[5]; // (DOUG?)
                float[] siteVariables = options.ToSiteVariablesArray();
                int standAge = breastHeightAge + 2; // stand error if less than one year to grow to breast height
                float[] standParameters = options.ToStandParametersArray();
                float[] YSF = new float[5]; // (DOUG?)
                float[] YST = new float[5]; // (DOUG?)
                int unusedErrorFlag = -1;

                treeDataForStep.GetEmptyArrays(out float[] dbhInInchesAtEndOfCycle, out float[] heightInFeetAtEndOfCycle, 
                                        out float[] crownRatioAtEndOfCycle, out float[] shadowCrownRatioAtEndOfCycle, 
                                        out float[] expansionFactorAtEndOfCycle);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    treeDataForStep.ToArrays(out species, out int[] user, out float[] dbhInInchesAtStartOfCycle, out float[] heightInFeetAtStartOfCycle,
                                      out float[] crownRatioAtStartOfCycle, out float[] expansionFactorAtStartOfCycle,
                                      out float[] shadowCrownRatioAtStartOfCycle, out float[] diameterGrowth, out float[] heightGrowth,
                                      out float[] crownChange, out float[] shadowCrownRatioChange);
                    Execute2.EXECUTE(simulationStep, variant, NPTS, treeDataForStep.UsedRecordCount, ref standAge, ref breastHeightAge, null, null, species, 
                                     user, featureFlagsArray, dbhInInchesAtStartOfCycle, heightInFeetAtStartOfCycle, crownRatioAtStartOfCycle,
                                     shadowCrownRatioAtStartOfCycle, expansionFactorAtStartOfCycle, treeDataForStep.MGExpansionFactor, siteVariables, 
                                     ACALIB, PN, YSF, BABT, BART, YST, null, null, null, null, null, null, null, null, null, null, null,
                                     errorsAndWarnings.StandErrors, errorsAndWarnings.TreeErrors, errorsAndWarnings.StandWarnings,
                                     errorsAndWarnings.TreeWarnings, unusedErrorFlag, diameterGrowth, heightGrowth, crownChange, 
                                     shadowCrownRatioChange, treeDataForStep.DeadExpansionFactor, out int treeCountAtEndOfCycle, dbhInInchesAtEndOfCycle, 
                                     heightInFeetAtEndOfCycle, crownRatioAtEndOfCycle, shadowCrownRatioAtEndOfCycle, expansionFactorAtEndOfCycle, 
                                     standParameters);
                    treeGrowth.Accumulate(dbhInInchesAtStartOfCycle, dbhInInchesAtEndOfCycle, heightInFeetAtStartOfCycle, heightInFeetAtEndOfCycle,
                                          treeDataForStep.DeadExpansionFactor);
                    treeDataForStep.FromArrays(species, user, dbhInInchesAtEndOfCycle, heightInFeetAtEndOfCycle, crownRatioAtEndOfCycle, 
                                        expansionFactorAtEndOfCycle, shadowCrownRatioAtEndOfCycle, diameterGrowth, heightGrowth,
                                        crownChange, shadowCrownRatioChange);
                    treeDataForStep.WriteAsCsv(this.TestContext, variant, simulationStep);
                    this.Verify(errorsAndWarnings, variant);
                    this.Verify(treeDataForStep, variantCapabilities);
                }

                this.Verify(treeGrowth, initialTreeData, treeDataForStep);
            }
        }
    }
}
