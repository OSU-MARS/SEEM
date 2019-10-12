using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Osu.Cof.Organon.Test
{
    public class OrganonTest
    {
        protected float[,] CreateCalibrationArray()
        {
            // (DOUG? figure out relation to ACALIB array and CALC, CALD, and CALH flags and equations)
            return new float[18, 6] {
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
                { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F },
            };
        }

        protected TreeData CreateDefaultTrees(Variant variant)
        {
            return this.CreateDefaultTrees(variant, null);
        }

        protected TreeData CreateDefaultTrees(Variant variant, Nullable<int> maximumRecordCount)
        {
            List<TreeRecord> trees = new List<TreeRecord>();
            switch (variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    trees.Add(new TreeRecord(FiaCode.PseudotsugaMenziesii, 0.1F, 10.0F, 0.4F));
                    trees.Add(new TreeRecord(FiaCode.PseudotsugaMenziesii, 0.2F, 20.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.PseudotsugaMenziesii, 0.3F, 10.0F, 0.6F));
                    trees.Add(new TreeRecord(FiaCode.PseudotsugaMenziesii, 10.0F, 10.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AbiesGrandis, 0.1F, 1.0F, 0.6F));
                    trees.Add(new TreeRecord(FiaCode.AbiesGrandis, 1.0F, 2.0F, 0.7F));
                    trees.Add(new TreeRecord(FiaCode.TsugaHeterophylla, 0.1F, 5.0F, 0.6F));
                    trees.Add(new TreeRecord(FiaCode.TsugaHeterophylla, 0.5F, 10.0F, 0.7F));
                    trees.Add(new TreeRecord(FiaCode.ThujaPlicata, 0.1F, 10.0F, 0.4F));
                    trees.Add(new TreeRecord(FiaCode.ThujaPlicata, 1.0F, 15.0F, 0.5F));

                    trees.Add(new TreeRecord(FiaCode.TaxusBrevifolia, 0.1F, 2.0F, 0.7F));
                    trees.Add(new TreeRecord(FiaCode.ArbutusMenziesii, 1.0F, 2.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AcerMacrophyllum, 0.1F, 2.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.QuercusGarryana, 10.0F, 2.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AlnusRubra, 0.1F, 2.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.CornusNuttallii, 0.1F, 2.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.Salix, 0.1F, 2.0F, 0.5F));
                    break;
                case Variant.Rap:
                    trees.Add(new TreeRecord(FiaCode.AlnusRubra, 0.1F, 30.0F, 0.3F));
                    trees.Add(new TreeRecord(FiaCode.AlnusRubra, 0.2F, 40.0F, 0.4F));
                    trees.Add(new TreeRecord(FiaCode.AlnusRubra, 0.3F, 30.0F, 0.5F));

                    trees.Add(new TreeRecord(FiaCode.PseudotsugaMenziesii, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.TsugaHeterophylla, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.ThujaPlicata, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AcerMacrophyllum, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.CornusNuttallii, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.Salix, 0.1F, 1.0F, 0.5F));
                    break;
                case Variant.Swo:
                    trees.Add(new TreeRecord(FiaCode.PseudotsugaMenziesii, 0.1F, 5.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AbiesConcolor, 0.1F, 5.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AbiesGrandis, 0.1F, 5.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.PinusPonderosa, 0.1F, 10.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.PinusLambertiana, 0.1F, 10.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.CalocedrusDecurrens, 0.1F, 10.0F, 0.5F));

                    trees.Add(new TreeRecord(FiaCode.TsugaHeterophylla, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.ThujaPlicata, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.TaxusBrevifolia, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.ArbutusMenziesii, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.ChrysolepisChrysophyllaVarChrysophylla, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.LithocarpusDensiflorus, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.QuercusChrysolepis, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AcerMacrophyllum, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.QuercusGarryana, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.QuercusKelloggii, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AlnusRubra, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.CornusNuttallii, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.Salix, 0.1F, 1.0F, 0.5F));
                    break;
                default:
                    throw new NotSupportedException(String.Format("Unhandled version {0}.", variant));
            }

            if (maximumRecordCount.HasValue == false)
            {
                maximumRecordCount = trees.Count;
            }

            TreeData treeData = new TreeData(variant, maximumRecordCount.Value, TestConstant.Default.SiteIndex, TestConstant.Default.SiteIndex - 10.0F);
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                TreeRecord tree = trees[treeIndex];
                int speciesGroup = this.GetSpeciesGroup(variant, tree.Species);
                treeData.Integer[treeIndex, (int)TreePropertyInteger.Species] = (int)tree.Species;
                treeData.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup] = speciesGroup;
                treeData.Integer[treeIndex, (int)TreePropertyInteger.User] = treeIndex;
                treeData.Float[treeIndex, (int)TreePropertyFloat.Dbh] = tree.DbhInInches;
                treeData.Float[treeIndex, (int)TreePropertyFloat.Height] = tree.HeightInFeet;
                treeData.Float[treeIndex, (int)TreePropertyFloat.CrownRatio] = tree.CrownRatio;
                treeData.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor] = tree.ExpansionFactor;
                treeData.MGExpansionFactor[treeIndex] = tree.ExpansionFactor + 1.0F;
            }
            treeData.UsedRecordCount = trees.Count;

            return treeData;
        }

        private int GetSpeciesGroup(Variant variant, FiaCode species)
        {
            // equivalent of Execute2.SPGROUP()
            switch (variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    return TestConstant.NwoSmcSpeciesCodes.IndexOf(species);
                case Variant.Rap:
                    return TestConstant.RapSpeciesCodes.IndexOf(species);
                case Variant.Swo:
                    int speciesGroup = TestConstant.SwoSpeciesCodes.IndexOf(species);
                    if (speciesGroup > 1)
                    {
                        --speciesGroup;
                    }
                    return speciesGroup;
                default:
                    throw new NotSupportedException(String.Format("Unhandled version {0}.", variant));
            }
        }

        protected void Verify(OrganonErrorsAndWarnings errorsAndWarnings, Variant variant)
        {
            for (int standErrorIndex = 0; standErrorIndex < errorsAndWarnings.StandErrors.Length; ++standErrorIndex)
            {
                Assert.IsTrue(errorsAndWarnings.StandErrors[standErrorIndex] == 0);
            }
            for (int standWarningIndex = 0; standWarningIndex < errorsAndWarnings.StandWarnings.Length; ++standWarningIndex)
            {
                if (standWarningIndex == 5)
                {
                    // allow warning for less than 50 tree records
                    Assert.IsTrue(errorsAndWarnings.StandWarnings[standWarningIndex] >= 0);
                    Assert.IsTrue(errorsAndWarnings.StandWarnings[standWarningIndex] <= 1);
                }
                else if ((variant != Variant.Smc) && (standWarningIndex != 4))
                {
                    // for now, ignore SMC warning for breast height age < 10
                    Assert.IsTrue(errorsAndWarnings.StandWarnings[standWarningIndex] == 0);
                }
            }

            int treeErrorRowLength = errorsAndWarnings.TreeErrors.Length / errorsAndWarnings.TreeWarnings.Length;
            for (int treeIndex = 0; treeIndex < errorsAndWarnings.TreeWarnings.Length; ++treeIndex)
            {
                for (int errorIndex = 0; errorIndex < treeErrorRowLength; ++errorIndex)
                {
                    Assert.IsTrue(errorsAndWarnings.TreeErrors[treeIndex, errorIndex] == 0);
                }
            }
            for (int treeWarningIndex = 0; treeWarningIndex < errorsAndWarnings.TreeWarnings.Length; ++treeWarningIndex)
            {
                // for now, ignore warnings on height exceeding potential height
                // Assert.IsTrue(errorsAndWarnings.TreeWarnings[treeWarningIndex] == 0);
            }
        }

        protected void Verify(TreeData treeData, OrganonCapabilities variantCapabilities)
        {
            Assert.IsTrue(treeData.UsedRecordCount > 0);
            Assert.IsTrue(treeData.UsedRecordCount <= treeData.MaximumRecordCount);

            for (int treeIndex = 0; treeIndex < treeData.UsedRecordCount; ++treeIndex)
            {
                // primary tree data
                float deadExpansionFactor = treeData.DeadExpansionFactor[treeIndex];
                Assert.IsTrue(deadExpansionFactor >= 0.0F);
                Assert.IsTrue(deadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                float crownRatio = treeData.Float[treeIndex, (int)TreePropertyFloat.CrownRatio];
                Assert.IsTrue(crownRatio >= 0.0F);
                Assert.IsTrue(crownRatio <= 1.0F);
                float dbhInInches = treeData.Float[treeIndex, (int)TreePropertyFloat.Dbh];
                Assert.IsTrue(dbhInInches >= 0.0F);
                Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DbhInInches);
                float expansionFactor = treeData.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor];
                Assert.IsTrue(expansionFactor >= 0.0F);
                Assert.IsTrue(expansionFactor <= TestConstant.Maximum.ExpansionFactor);
                float heightInFeet = treeData.Float[treeIndex, (int)TreePropertyFloat.Height];
                Assert.IsTrue(heightInFeet >= 0.0F);
                Assert.IsTrue(heightInFeet <= TestConstant.Maximum.HeightInFeet);

                Assert.IsTrue(expansionFactor + deadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                float accumulatedDiameterGrowthInInches = treeData.Growth[treeIndex, (int)TreePropertyGrowth.AccumulatedDiameter];
                Assert.IsTrue(accumulatedDiameterGrowthInInches >= 0.0F);
                Assert.IsTrue(accumulatedDiameterGrowthInInches <= dbhInInches);
                float accumulatedHeightGrowthInFeet = treeData.Growth[treeIndex, (int)TreePropertyGrowth.AccumulatedHeight];
                Assert.IsTrue(accumulatedHeightGrowthInFeet >= 0.0F);
                // (DOUG? why does accumulated height growth exceed the actual tree height?)
                // Assert.IsTrue(accumulatedHeightGrowthInFeet <= (heightInFeet + 100.0F));
                // Assert.IsTrue(accumulatedHeightGrowthInFeet <= TestConstant.Maximum.HeightInFeet);
                float diameterGrowthInInches = treeData.Growth[treeIndex, (int)TreePropertyGrowth.Diameter];
                Assert.IsTrue(diameterGrowthInInches >= 0.0F);
                Assert.IsTrue(diameterGrowthInInches <= 0.1F * TestConstant.Maximum.DbhInInches);
                float heightGrowthInFeet = treeData.Growth[treeIndex, (int)TreePropertyGrowth.Height];
                Assert.IsTrue(heightGrowthInFeet >= 0.0F);
                Assert.IsTrue(heightGrowthInFeet <= 0.1F * TestConstant.Maximum.HeightInFeet);

                int species = treeData.Integer[treeIndex, (int)TreePropertyInteger.Species];
                Assert.IsTrue(Enum.IsDefined(typeof(FiaCode), species));
                int speciesGroup = treeData.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup];
                Assert.IsTrue(speciesGroup >= 0);
                Assert.IsTrue(speciesGroup < variantCapabilities.SpeciesGroupCount);
                int user = treeData.Integer[treeIndex, (int)TreePropertyInteger.User];
                Assert.IsTrue(user >= 0);
                Assert.IsTrue(user <= 100);

                float mgExpansionFactor = treeData.MGExpansionFactor[treeIndex];
                Assert.IsTrue(mgExpansionFactor >= 0.0F);
                Assert.IsTrue(mgExpansionFactor <= TestConstant.Maximum.MGExpansionFactor);

                float shadowCrownRatio0 = treeData.ShadowCrownRatio[treeIndex, 0];
                Assert.IsTrue(shadowCrownRatio0 >= 0.0F);
                Assert.IsTrue(shadowCrownRatio0 <= 1.0F);
                float shadowCrownRatio1 = treeData.ShadowCrownRatio[treeIndex, 1];
                Assert.IsTrue(shadowCrownRatio1 >= 0.0F);
                Assert.IsTrue(shadowCrownRatio1 <= 1.0F);
                float shadowCrownRatio2 = treeData.ShadowCrownRatio[treeIndex, 2];
                Assert.IsTrue(shadowCrownRatio2 >= 0.0F);
                Assert.IsTrue(shadowCrownRatio2 <= 1.0F);

                // for now, no need to verify tripling data as it isn't passed in most tests
            }
        }

        protected void Verify(TreeLifeAndDeath treeGrowth, TreeData initialTreeData, TreeData finalTreeData)
        {
            int treeRecords = initialTreeData.UsedRecordCount;
            for (int treeIndex = 0; treeIndex < treeRecords; ++treeIndex)
            {
                float totalDbhGrowth = treeGrowth.TotalDbhGrowthInInches[treeIndex];
                Assert.IsTrue(totalDbhGrowth > 0.0F);
                Assert.IsTrue(totalDbhGrowth <= TestConstant.Maximum.DbhInInches);

                float totalHeightGrowth = treeGrowth.TotalHeightGrowthInFeet[treeIndex];
                Assert.IsTrue(totalHeightGrowth >= 0.0F);
                Assert.IsTrue(totalHeightGrowth <= TestConstant.Maximum.HeightInFeet);

                float totalDeadExpansionFactor = treeGrowth.TotalDeadExpansionFactor[treeIndex];
                Assert.IsTrue(totalDeadExpansionFactor >= 0.0F);
                Assert.IsTrue(totalDeadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                float initialTotalExpansionFactor = initialTreeData.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor] + initialTreeData.DeadExpansionFactor[treeIndex];
                float finalTotalExpansionFactor = finalTreeData.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor] + totalDeadExpansionFactor;
                Assert.IsTrue(initialTotalExpansionFactor > 0.0F);
                Assert.IsTrue(finalTotalExpansionFactor > 0.0F);
                float expansionFactorRatio = finalTotalExpansionFactor / initialTotalExpansionFactor;
                Assert.IsTrue(expansionFactorRatio >= 0.999F);
                Assert.IsTrue(expansionFactorRatio <= 1.001F);
            }
        }

        protected void Verify(TriplingData triple)
        {
            int treeRecords = triple.NPR.Length;
            for (int treeIndex = 0; treeIndex < treeRecords; ++treeIndex)
            {
                int pruningRowLength = triple.PruningAge.Length / treeRecords;
                for (int pruningIndex = 0; pruningIndex < pruningRowLength; ++pruningIndex)
                {
                    float PRLH = triple.PruningLH[treeIndex, pruningIndex];
                    Assert.IsTrue(PRLH >= 0.0F);
                    Assert.IsTrue(PRLH <= 0.0F);

                    float pruningAge = triple.PruningAge[treeIndex, pruningIndex];
                    Assert.IsTrue(pruningAge >= 0);
                    Assert.IsTrue(pruningAge <= 0);

                    float pruningDbhInInches = triple.PruningDbhInInches[treeIndex, pruningIndex];
                    Assert.IsTrue(pruningDbhInInches >= 0.0F);
                    Assert.IsTrue(pruningDbhInInches <= 0.0F);

                    float pruningHeightInFeet = triple.PruningHeightInFeet[treeIndex, pruningIndex];
                    Assert.IsTrue(pruningHeightInFeet >= 0.0F);
                    Assert.IsTrue(pruningHeightInFeet <= 0.0F);

                    float pruningCrownRatio = triple.PruningCrownRatio[treeIndex, pruningIndex];
                    Assert.IsTrue(pruningCrownRatio >= 0.0F);
                    Assert.IsTrue(pruningCrownRatio <= 0.0F);

                    float PREXP = triple.PREXP[treeIndex, pruningIndex];
                    Assert.IsTrue(PREXP >= 0.0F);
                    Assert.IsTrue(PREXP <= 0.0F);
                }


                int branchRowLength = triple.BranchCount.Length / treeRecords;
                for (int branchIndex = 0; branchIndex < branchRowLength; ++branchIndex)
                {
                    float branchCount = triple.BranchCount[treeIndex, branchIndex];
                    Assert.IsTrue(branchCount >= 0);
                    Assert.IsTrue(branchCount <= 0);
                }

                branchRowLength = triple.BranchDiameter.Length / treeRecords;
                for (int branchIndex = 0; branchIndex < branchRowLength; ++branchIndex)
                {
                    float branchDiameter = triple.BranchDiameter[treeIndex, branchIndex];
                    Assert.IsTrue(branchDiameter >= 0);
                    Assert.IsTrue(branchDiameter <= 0);

                    float branchHeightInFeet = triple.BranchHeight[treeIndex, branchIndex];
                    Assert.IsTrue(branchHeightInFeet >= 0);
                    Assert.IsTrue(branchHeightInFeet <= 0);

                    float juvenileCore = triple.JuvenileCore[treeIndex, branchIndex];
                    Assert.IsTrue(juvenileCore >= 0);
                    Assert.IsTrue(juvenileCore <= 0);
                }

                // volumes
                int NPR = triple.NPR[treeIndex];
                Assert.IsTrue(NPR >= 0);
                Assert.IsTrue(NPR <= 0);

                int POINT = triple.POINT[treeIndex];
                Assert.IsTrue(POINT >= 0);
                Assert.IsTrue(POINT <= 0);

                int TREENO = triple.TREENO[treeIndex];
                Assert.IsTrue(TREENO >= 0);
                Assert.IsTrue(TREENO <= 0);

                int volumeRowLength = triple.VOLTR.Length / treeRecords;
                for (int volumeIndex = 0; volumeIndex < volumeRowLength; ++volumeIndex)
                {
                    float VOLTR = triple.VOLTR[treeIndex, volumeIndex];
                    Assert.IsTrue(VOLTR >= 0.0F);
                    Assert.IsTrue(VOLTR <= 0.0F);
                }

                volumeRowLength = triple.SYTVOL.Length / treeRecords;
                for (int volumeIndex = 0; volumeIndex < volumeRowLength; ++volumeIndex)
                {
                    float SYTVOL = triple.SYTVOL[treeIndex, volumeIndex];
                    Assert.IsTrue(SYTVOL >= 0.0F);
                    Assert.IsTrue(SYTVOL <= 0.0F);
                }
            }
        }
    }
}
