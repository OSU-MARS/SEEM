using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Osu.Cof.Organon.Test
{
    public class OrganonTest
    {
        protected TestStand CreateDefaultStand(OrganonConfiguration configuration)
        {
            List<TreeRecord> trees = new List<TreeRecord>();
            switch (configuration.Variant.Variant)
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
                    trees.Add(new TreeRecord(FiaCode.NotholithocarpusDensiflorus, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.QuercusChrysolepis, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AcerMacrophyllum, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.QuercusGarryana, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.QuercusKelloggii, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.AlnusRubra, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.CornusNuttallii, 0.1F, 1.0F, 0.5F));
                    trees.Add(new TreeRecord(FiaCode.Salix, 0.1F, 1.0F, 0.5F));
                    break;
                default:
                    throw Organon.OrganonVariant.CreateUnhandledVariantException(configuration.Variant.Variant);
            }

            TestStand stand = new TestStand(configuration.Variant, 0, trees.Count, TestConstant.Default.SiteIndex);
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                TreeRecord tree = trees[treeIndex];
                stand.Species[treeIndex] = tree.Species;
                stand.Dbh[treeIndex] = tree.DbhInInches;
                stand.Height[treeIndex] = tree.HeightInFeet;
                stand.CrownRatio[treeIndex] = tree.CrownRatio;
                stand.LiveExpansionFactor[treeIndex] = tree.ExpansionFactor;
            }
            stand.SetRedAlderSiteIndex();
            stand.SetSdiMax(configuration);
            return stand;
        }

        protected OrganonConfiguration CreateOrganonConfiguration(OrganonVariant variant)
        {
            OrganonConfiguration configuration = new OrganonConfiguration(variant)
            {
                DefaultMaximumSdi = TestConstant.Default.MaximumReinekeStandDensityIndex,
                TrueFirMaximumSdi = TestConstant.Default.MaximumReinekeStandDensityIndex,
                HemlockMaximumSdi = TestConstant.Default.MaximumReinekeStandDensityIndex,
            };

            return configuration;
        }

        protected Dictionary<FiaCode, float[]> CreateSpeciesCalibration(OrganonVariant variant)
        {
            ReadOnlyCollection<FiaCode> speciesList;
            switch (variant.Variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    speciesList = Constant.NwoSmcSpecies;
                    break;
                case Variant.Rap:
                    speciesList = Constant.RapSpecies;
                    break;
                case Variant.Swo:
                    speciesList = Constant.SwoSpecies;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
            }

            Dictionary<FiaCode, float[]> calibration = new Dictionary<FiaCode, float[]>();
            foreach (FiaCode species in speciesList)
            {
                calibration.Add(species, new float[] { 1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 0.0F });
            }
            return calibration;
        }

        protected void GrowPspStand(PspStand huffmanPeak, TestStand stand, OrganonVariant variant, int startYear, int endYear, string baseFileName)
        {
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
            TestStand initialTreeData = stand.Clone();
            TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath(stand.TreeRecordCount);

            float BABT = 0.0F;
            float[] BART = new float[5];
            Dictionary<FiaCode, float[]> CALIB = this.CreateSpeciesCalibration(variant);
            float[] PN = new float[5];
            if (configuration.IsEvenAge)
            {
                // stand error if less than one year to grow to breast height
                stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
            }
            float[] YSF = new float[5];
            float[] YST = new float[5];

            TestStandDensity density = new TestStandDensity(stand, variant);
            using StreamWriter densityWriter = density.WriteToCsv(baseFileName + " density.csv", variant, startYear);
            TreeQuantiles quantiles = new TreeQuantiles(stand);
            using StreamWriter quantileWriter = quantiles.WriteToCsv(baseFileName + " quantiles.csv", variant, startYear);
            using StreamWriter treeGrowthWriter = stand.WriteTreesToCsv(baseFileName + " tree growth.csv", variant, startYear);
            for (int simulationStep = 0, year = startYear + variant.TimeStepInYears; year <= endYear; year += variant.TimeStepInYears, ++simulationStep)
            {
                StandGrowth.Grow(simulationStep, configuration, stand, CALIB, PN, YSF, BABT, BART, YST);
                treeGrowth.AccumulateGrowthAndMortality(stand);
                huffmanPeak.AddIngrowth(year, stand, density);
                stand.SetSdiMax(configuration);
                this.Verify(ExpectedTreeChanges.DiameterGrowthOrNoChange | ExpectedTreeChanges.HeightGrowthOrNoChange, stand, variant);

                density = new TestStandDensity(stand, variant);
                density.WriteToCsv(densityWriter, variant, year);
                quantiles = new TreeQuantiles(stand);
                quantiles.WriteToCsv(quantileWriter, variant, year);
                stand.WriteTreesToCsv(treeGrowthWriter, variant, year);
            }

            this.Verify(ExpectedTreeChanges.ExpansionFactorConservedOrIncreased | ExpectedTreeChanges.DiameterGrowthOrNoChange | ExpectedTreeChanges.HeightGrowthOrNoChange, treeGrowth, initialTreeData, stand);
            this.Verify(CALIB);
        }

        protected void Verify(Dictionary<FiaCode, float[]> calibration)
        {
            foreach (KeyValuePair<FiaCode, float[]> species in calibration)
            {
                Assert.IsTrue(species.Value[0] == 1.0F);
                Assert.IsTrue(species.Value[1] == 1.0F);
                Assert.IsTrue(species.Value[2] == 1.0F);
            }
        }

        protected void Verify(ExpectedTreeChanges expectedGrowth, TestStand stand, OrganonVariant variant)
        {
            this.Verify(expectedGrowth, OrganonWarnings.None, stand, variant);
        }

        protected void Verify(ExpectedTreeChanges expectedGrowth, OrganonWarnings expectedWarnings, TestStand stand, OrganonVariant variant)
        {
            Assert.IsTrue(stand.AgeInYears >= 0);
            Assert.IsTrue(stand.AgeInYears <= TestConstant.Maximum.StandAgeInYears);
            Assert.IsTrue(stand.BreastHeightAgeInYears >= 0);
            Assert.IsTrue(stand.BreastHeightAgeInYears <= TestConstant.Maximum.StandAgeInYears);
            Assert.IsTrue(stand.NumberOfPlots >= 1);
            Assert.IsTrue(stand.NumberOfPlots <= 36);
            Assert.IsTrue(stand.TreeRecordCount > 0);
            Assert.IsTrue(stand.TreeRecordCount <= stand.TreeRecordCount);

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                // primary tree data
                FiaCode species = stand.Species[treeIndex];
                Assert.IsTrue(Enum.IsDefined(typeof(FiaCode), species));

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

                float deadExpansionFactor = stand.DeadExpansionFactor[treeIndex];
                Assert.IsTrue(deadExpansionFactor >= 0.0F);
                Assert.IsTrue(deadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);
                Assert.IsTrue(expansionFactor + deadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                // diameter and height growth
                float diameterGrowthInInches = stand.DbhGrowth[treeIndex];
                if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowth))
                {
                    Assert.IsTrue(diameterGrowthInInches > 0.0F, "{0}: {1} {2} did not grow in diameter.", variant.Variant, stand.Species[treeIndex], treeIndex);
                    Assert.IsTrue(diameterGrowthInInches <= 0.1F * TestConstant.Maximum.DbhInInches);
                }
                else if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowthOrNoChange))
                {
                    Assert.IsTrue(diameterGrowthInInches >= 0.0F);
                    Assert.IsTrue(diameterGrowthInInches <= 0.1F * TestConstant.Maximum.DbhInInches);
                }
                else
                {
                    Assert.IsTrue(diameterGrowthInInches == 0.0F);
                }
                float heightGrowthInFeet = stand.HeightGrowth[treeIndex];
                if (expectedGrowth.HasFlag(ExpectedTreeChanges.HeightGrowth))
                {
                    Assert.IsTrue(heightGrowthInFeet > 0.0F, "{0}: {1} {2} did not grow in height.", variant.Variant, stand.Species[treeIndex], treeIndex);
                    Assert.IsTrue(heightGrowthInFeet <= 0.1F * TestConstant.Maximum.HeightInFeet);
                }
                else if (expectedGrowth.HasFlag(ExpectedTreeChanges.HeightGrowthOrNoChange))
                {
                    Assert.IsTrue(heightGrowthInFeet >= 0.0F, "{0}: {1} {2} decreased in height.", variant.Variant, stand.Species[treeIndex], treeIndex);
                    Assert.IsTrue(heightGrowthInFeet <= 0.1F * TestConstant.Maximum.HeightInFeet);
                }
                else
                {
                    Assert.IsTrue(heightGrowthInFeet == 0.0F);
                }

                // for now, ignore warnings on height exceeding potential height
                // Assert.IsTrue(stand.TreeWarnings[treeWarningIndex] == 0);
            }

            Assert.IsTrue(stand.Warnings.BigSixHeightAbovePotential == false);
            Assert.IsTrue(stand.Warnings.LessThan50TreeRecords == expectedWarnings.HasFlag(OrganonWarnings.LessThan50TreeRecords));
            Assert.IsTrue(stand.Warnings.HemlockSiteIndexOutOfRange == expectedWarnings.HasFlag(OrganonWarnings.HemlockSiteIndex));
            Assert.IsTrue(stand.Warnings.OtherSpeciesBasalAreaTooHigh == false);
            Assert.IsTrue(stand.Warnings.SiteIndexOutOfRange == false);
            if (variant.Variant != Variant.Smc)
            {
                // for now, ignore SMC warning for breast height age < 10
                Assert.IsTrue(stand.Warnings.TreesOld == false);
            }
            // for now, ignore stand.Warnings.TreesYoung
        }

        protected void Verify(ExpectedTreeChanges expectedGrowth, TreeLifeAndDeath treeGrowth, TestStand initialTreeData, TestStand finalTreeData)
        {
            int treeRecords = initialTreeData.TreeRecordCount;
            for (int treeIndex = 0; treeIndex < treeRecords; ++treeIndex)
            {
                float totalDbhGrowth = treeGrowth.TotalDbhGrowthInInches[treeIndex];
                if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowth))
                {
                    Assert.IsTrue(totalDbhGrowth > 0.0F);
                    Assert.IsTrue(totalDbhGrowth <= TestConstant.Maximum.DbhInInches);
                }
                else if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowthOrNoChange))
                {
                    Assert.IsTrue(totalDbhGrowth >= 0.0F);
                    Assert.IsTrue(totalDbhGrowth <= TestConstant.Maximum.DbhInInches);
                }
                else
                {
                    Assert.IsTrue(totalDbhGrowth == 0.0F);
                }

                float totalHeightGrowth = treeGrowth.TotalHeightGrowthInFeet[treeIndex];
                if (expectedGrowth.HasFlag(ExpectedTreeChanges.HeightGrowth))
                {
                    Assert.IsTrue(totalHeightGrowth > 0.0F);
                    Assert.IsTrue(totalHeightGrowth <= TestConstant.Maximum.HeightInFeet);
                }
                else if (expectedGrowth.HasFlag(ExpectedTreeChanges.HeightGrowthOrNoChange))
                {
                    Assert.IsTrue(totalHeightGrowth >= 0.0F);
                    Assert.IsTrue(totalHeightGrowth <= TestConstant.Maximum.HeightInFeet);
                }
                else
                {
                    Assert.IsTrue(totalHeightGrowth == 0.0F);
                }

                float totalDeadExpansionFactor = treeGrowth.TotalDeadExpansionFactor[treeIndex];
                Assert.IsTrue(totalDeadExpansionFactor >= 0.0F);
                Assert.IsTrue(totalDeadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                float initialTotalExpansionFactor = initialTreeData.LiveExpansionFactor[treeIndex] + initialTreeData.DeadExpansionFactor[treeIndex];
                float finalTotalExpansionFactor = finalTreeData.LiveExpansionFactor[treeIndex] + totalDeadExpansionFactor;
                float expansionFactorRatio = finalTotalExpansionFactor / initialTotalExpansionFactor;
                Assert.IsTrue(expansionFactorRatio >= 0.999F);
                if (expectedGrowth.HasFlag(ExpectedTreeChanges.ExpansionFactorConservedOrIncreased))
                {
                    Assert.IsTrue(initialTotalExpansionFactor >= 0.0F);
                }
                else
                {
                    Assert.IsTrue(initialTotalExpansionFactor > 0.0F);
                    Assert.IsTrue(finalTotalExpansionFactor > 0.0F);
                    Assert.IsTrue(expansionFactorRatio <= 1.001F);
                }
                Assert.IsTrue(finalTotalExpansionFactor <= TestConstant.Maximum.ExpansionFactor);
            }
        }
    }
}
