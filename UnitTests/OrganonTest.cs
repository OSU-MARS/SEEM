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

        protected TestStand CreateDefaultStand(OrganonConfiguration configuration)
        {
            List<TreeRecord> trees = new List<TreeRecord>();
            switch (configuration.Variant)
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
                    throw VariantExtensions.CreateUnhandledVariantException(configuration.Variant);
            }

            TestStand stand = new TestStand(configuration.Variant, 0, trees.Count, TestConstant.Default.SiteIndex);
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                TreeRecord tree = trees[treeIndex];
                int speciesGroup = this.GetSpeciesGroup(configuration.Variant, tree.Species);
                stand.Species[treeIndex] = tree.Species;
                stand.SpeciesGroup[treeIndex] = speciesGroup;
                stand.Dbh[treeIndex] = tree.DbhInInches;
                stand.Height[treeIndex] = tree.HeightInFeet;
                stand.CrownRatio[treeIndex] = tree.CrownRatio;
                stand.LiveExpansionFactor[treeIndex] = tree.ExpansionFactor;
            }
            stand.SetRedAlderSiteIndex();
            stand.SetSdiMax(configuration);
            return stand;
        }

        protected OrganonConfiguration CreateOrganonConfiguration(Variant variant)
        {
            OrganonConfiguration configuration = new OrganonConfiguration(variant)
            {
                MSDI_1 = TestConstant.Default.MaximumReinekeStandDensityIndex,
                MSDI_2 = TestConstant.Default.MaximumReinekeStandDensityIndex,
                MSDI_3 = TestConstant.Default.MaximumReinekeStandDensityIndex,
            };

            return configuration;
        }

        private int GetSpeciesGroup(Variant variant, FiaCode species)
        {
            // copy of StandGrowth.GetSpeciesGroup()
            switch (variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    return Constant.NwoSmcSpecies.IndexOf(species);
                case Variant.Rap:
                    return Constant.RapSpecies.IndexOf(species);
                case Variant.Swo:
                    int speciesGroup = Constant.SwoSpecies.IndexOf(species);
                    if (speciesGroup > 1)
                    {
                        --speciesGroup;
                    }
                    return speciesGroup;
                default:
                    throw VariantExtensions.CreateUnhandledVariantException(variant);
            }
        }

        protected void Verify(float[,] calibration)
        {
            int speciesGroupCount = calibration.GetLength(0);
            for (int speciesGroupIndex = 0; speciesGroupIndex < speciesGroupCount; ++speciesGroupIndex)
            {
                Assert.IsTrue(calibration[speciesGroupIndex, 0] == 1.0F);
                Assert.IsTrue(calibration[speciesGroupIndex, 1] == 1.0F);
                Assert.IsTrue(calibration[speciesGroupIndex, 2] == 1.0F);
            }
        }

        protected void Verify(TestStand stand, ExpectedTreeChanges expectedGrowth, VariantCapabilities variantCapabilities)
        {
            Assert.IsTrue(stand.AgeInYears >= 0);
            Assert.IsTrue(stand.AgeInYears <= TestConstant.Maximum.StandAgeInYears);
            Assert.IsTrue(stand.BreastHeightAgeInYears >= 0);
            Assert.IsTrue(stand.BreastHeightAgeInYears <= TestConstant.Maximum.StandAgeInYears);
            Assert.IsTrue(stand.NumberOfPlots == 1);
            Assert.IsTrue(stand.TreeRecordCount > 0);
            Assert.IsTrue(stand.TreeRecordCount <= stand.TreeRecordCount);

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                // primary tree data
                float deadExpansionFactor = stand.DeadExpansionFactor[treeIndex];
                Assert.IsTrue(deadExpansionFactor >= 0.0F);
                Assert.IsTrue(deadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                float crownRatio = stand.CrownRatio[treeIndex];
                Assert.IsTrue(crownRatio >= 0.0F);
                Assert.IsTrue(crownRatio <= 1.0F);
                float dbhInInches = stand.Dbh[treeIndex];
                Assert.IsTrue(dbhInInches >= 0.0F);
                Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DbhInInches);
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                Assert.IsTrue(expansionFactor >= 0.0F);
                Assert.IsTrue(expansionFactor <= TestConstant.Maximum.ExpansionFactor);
                float heightInFeet = stand.Height[treeIndex];
                Assert.IsTrue(heightInFeet >= 0.0F);
                Assert.IsTrue(heightInFeet <= TestConstant.Maximum.HeightInFeet);

                Assert.IsTrue(expansionFactor + deadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                // diameter and height growth
                float diameterGrowthInInches = stand.DbhGrowth[treeIndex];
                if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowth))
                {
                    Assert.IsTrue(diameterGrowthInInches > 0.0F);
                    Assert.IsTrue(diameterGrowthInInches <= 0.1F * TestConstant.Maximum.DbhInInches);
                }
                else
                {
                    Assert.IsTrue(diameterGrowthInInches == 0.0F);
                }
                float heightGrowthInFeet = stand.HeightGrowth[treeIndex];
                if (expectedGrowth.HasFlag(ExpectedTreeChanges.HeightGrowth))
                {
                    Assert.IsTrue(heightGrowthInFeet > 0.0F, "{0}: {1} {2} did not grow in height.", variantCapabilities.Variant, stand.Species[treeIndex], treeIndex);
                    Assert.IsTrue(heightGrowthInFeet <= 0.1F * TestConstant.Maximum.HeightInFeet);
                }
                else if (expectedGrowth.HasFlag(ExpectedTreeChanges.HeightGrowthOrNoChange))
                {
                    Assert.IsTrue(heightGrowthInFeet >= 0.0F, "{0}: {1} {2} decreased in height.", variantCapabilities.Variant, stand.Species[treeIndex], treeIndex);
                    Assert.IsTrue(heightGrowthInFeet <= 0.1F * TestConstant.Maximum.HeightInFeet);
                }
                else
                {
                    Assert.IsTrue(heightGrowthInFeet == 0.0F);
                }

                FiaCode species = stand.Species[treeIndex];
                Assert.IsTrue(Enum.IsDefined(typeof(FiaCode), species));
                int speciesGroup = stand.SpeciesGroup[treeIndex];
                Assert.IsTrue(speciesGroup >= 0);
                Assert.IsTrue(speciesGroup < variantCapabilities.SpeciesGroupCount);

                // for now, ignore warnings on height exceeding potential height
                // Assert.IsTrue(stand.TreeWarnings[treeWarningIndex] == 0);
            }

            Assert.IsTrue(stand.Warnings.BigSixHeightAbovePotential == false);
            // ignore stand.Warnings.LessThan50TreeRecords
            Assert.IsTrue(stand.Warnings.MortalitySiteIndexOutOfRange == false);
            Assert.IsTrue(stand.Warnings.OtherSpeciesBasalAreaTooHigh == false);
            Assert.IsTrue(stand.Warnings.PrimarySiteIndexOutOfRange == false);
            if (variantCapabilities.Variant != Variant.Smc)
            {
                // for now, ignore SMC warning for breast height age < 10
                Assert.IsTrue(stand.Warnings.TreesOld == false);
            }
            // for now, ignore stand.Warnings.TreesYoung
        }

        protected void Verify(TreeLifeAndDeath treeGrowth, TestStand initialTreeData, TestStand finalTreeData)
        {
            int treeRecords = initialTreeData.TreeRecordCount;
            for (int treeIndex = 0; treeIndex < treeRecords; ++treeIndex)
            {
                float totalDbhGrowth = treeGrowth.TotalDbhGrowthInInches[treeIndex];
                Assert.IsTrue(totalDbhGrowth > 0.0F);
                Assert.IsTrue(totalDbhGrowth <= TestConstant .Maximum.DbhInInches);

                float totalHeightGrowth = treeGrowth.TotalHeightGrowthInFeet[treeIndex];
                Assert.IsTrue(totalHeightGrowth >= 0.0F);
                Assert.IsTrue(totalHeightGrowth <= TestConstant.Maximum.HeightInFeet);

                float totalDeadExpansionFactor = treeGrowth.TotalDeadExpansionFactor[treeIndex];
                Assert.IsTrue(totalDeadExpansionFactor >= 0.0F);
                Assert.IsTrue(totalDeadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                float initialTotalExpansionFactor = initialTreeData.LiveExpansionFactor[treeIndex] + initialTreeData.DeadExpansionFactor[treeIndex];
                float finalTotalExpansionFactor = finalTreeData.LiveExpansionFactor[treeIndex] + totalDeadExpansionFactor;
                Assert.IsTrue(initialTotalExpansionFactor > 0.0F);
                Assert.IsTrue(finalTotalExpansionFactor > 0.0F);
                float expansionFactorRatio = finalTotalExpansionFactor / initialTotalExpansionFactor;
                Assert.IsTrue(expansionFactorRatio >= 0.999F);
                Assert.IsTrue(expansionFactorRatio <= 1.001F);
            }
        }
    }
}
