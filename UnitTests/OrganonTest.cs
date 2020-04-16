using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.IO;

namespace Osu.Cof.Ferm.Test
{
    public class OrganonTest
    {
        protected TestStand CreateDefaultStand(OrganonConfiguration configuration)
        {
            TestStand stand = new TestStand(configuration.Variant.TreeModel, 0, TestConstant.Default.SiteIndex);
            switch (configuration.Variant.TreeModel)
            {
                case TreeModel.OrganonNwo:
                case TreeModel.OrganonSmc:
                    stand.Add(new TreeRecord(1, FiaCode.PseudotsugaMenziesii, 0.1F, 0.4F, 10.0F));
                    stand.Add(new TreeRecord(2, FiaCode.PseudotsugaMenziesii, 0.2F, 0.5F, 20.0F));
                    stand.Add(new TreeRecord(3, FiaCode.PseudotsugaMenziesii, 0.3F, 0.6F, 10.0F));
                    stand.Add(new TreeRecord(4, FiaCode.PseudotsugaMenziesii, 10.0F, 0.5F, 10.0F));
                    stand.Add(new TreeRecord(5, FiaCode.AbiesGrandis, 0.1F, 0.6F, 1.0F));
                    stand.Add(new TreeRecord(6, FiaCode.AbiesGrandis, 1.0F, 0.7F, 2.0F));
                    stand.Add(new TreeRecord(7, FiaCode.TsugaHeterophylla, 0.1F, 0.6F, 5.0F));
                    stand.Add(new TreeRecord(8, FiaCode.TsugaHeterophylla, 0.5F, 0.7F, 10.0F));
                    stand.Add(new TreeRecord(9, FiaCode.ThujaPlicata, 0.1F, 0.4F, 10.0F));
                    stand.Add(new TreeRecord(10, FiaCode.ThujaPlicata, 1.0F, 0.5F, 15.0F));

                    stand.Add(new TreeRecord(11, FiaCode.TaxusBrevifolia, 0.1F, 0.7F, 2.0F));
                    stand.Add(new TreeRecord(12, FiaCode.ArbutusMenziesii, 1.0F, 0.5F, 2.0F));
                    stand.Add(new TreeRecord(13, FiaCode.AcerMacrophyllum, 0.1F, 0.5F, 2.0F));
                    stand.Add(new TreeRecord(14, FiaCode.QuercusGarryana, 10.0F, 0.5F, 2.0F));
                    stand.Add(new TreeRecord(15, FiaCode.AlnusRubra, 0.1F, 0.5F, 2.0F));
                    stand.Add(new TreeRecord(16, FiaCode.CornusNuttallii, 0.1F, 0.5F, 2.0F));
                    stand.Add(new TreeRecord(17, FiaCode.Salix, 0.1F, 0.5F, 2.0F));
                    break;
                case TreeModel.OrganonRap:
                    stand.Add(new TreeRecord(1, FiaCode.AlnusRubra, 0.1F, 0.3F, 30.0F));
                    stand.Add(new TreeRecord(2, FiaCode.AlnusRubra, 0.2F, 0.4F, 40.0F));
                    stand.Add(new TreeRecord(3, FiaCode.AlnusRubra, 0.3F, 0.5F, 30.0F));

                    stand.Add(new TreeRecord(4, FiaCode.PseudotsugaMenziesii, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(5, FiaCode.TsugaHeterophylla, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(6, FiaCode.ThujaPlicata, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(7, FiaCode.AcerMacrophyllum, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(8, FiaCode.CornusNuttallii, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(9, FiaCode.Salix, 0.1F, 0.5F, 1.0F));
                    break;
                case TreeModel.OrganonSwo:
                    stand.Add(new TreeRecord(1, FiaCode.PseudotsugaMenziesii, 0.1F, 0.5F, 5.0F));
                    stand.Add(new TreeRecord(2, FiaCode.AbiesConcolor, 0.1F, 0.5F, 5.0F));
                    stand.Add(new TreeRecord(3, FiaCode.AbiesGrandis, 0.1F, 0.5F, 5.0F));
                    stand.Add(new TreeRecord(4, FiaCode.PinusPonderosa, 0.1F, 0.5F, 10.0F));
                    stand.Add(new TreeRecord(5, FiaCode.PinusLambertiana, 0.1F, 0.5F, 10.0F));
                    stand.Add(new TreeRecord(6, FiaCode.CalocedrusDecurrens, 0.1F, 0.5F, 10.0F));

                    stand.Add(new TreeRecord(7, FiaCode.TsugaHeterophylla, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(8, FiaCode.ThujaPlicata, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(9, FiaCode.TaxusBrevifolia, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(10, FiaCode.ArbutusMenziesii, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(11, FiaCode.ChrysolepisChrysophyllaVarChrysophylla, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(12, FiaCode.NotholithocarpusDensiflorus, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(13, FiaCode.QuercusChrysolepis, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(14, FiaCode.AcerMacrophyllum, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(15, FiaCode.QuercusGarryana, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(16, FiaCode.QuercusKelloggii, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(17, FiaCode.AlnusRubra, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(18, FiaCode.CornusNuttallii, 0.1F, 0.5F, 1.0F));
                    stand.Add(new TreeRecord(19, FiaCode.Salix, 0.1F, 0.5F, 1.0F));
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel);
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

        protected void GrowPspStand(PspStand huffmanPeak, TestStand stand, OrganonVariant variant, int startYear, int endYear, string baseFileName)
        {
            OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
            OrganonTreatments treatments = new OrganonTreatments();
            TestStand initialTreeData = stand.Clone();
            TreeLifeAndDeath treeGrowth = new TreeLifeAndDeath();

            Dictionary<FiaCode, float[]> CALIB = configuration.CreateSpeciesCalibration();
            if (configuration.IsEvenAge)
            {
                // stand error if less than one year to grow to breast height
                stand.AgeInYears = stand.BreastHeightAgeInYears + 2;
            }

            TestStandDensity density = new TestStandDensity(stand, variant);
            using StreamWriter densityWriter = density.WriteToCsv(baseFileName + " density.csv", variant, startYear);
            TreeQuantiles quantiles = new TreeQuantiles(stand);
            using StreamWriter quantileWriter = quantiles.WriteToCsv(baseFileName + " quantiles.csv", variant, startYear);
            using StreamWriter treeGrowthWriter = stand.WriteTreesToCsv(baseFileName + " tree growth.csv", variant, startYear);
            for (int simulationStep = 0, year = startYear + variant.TimeStepInYears; year <= endYear; year += variant.TimeStepInYears, ++simulationStep)
            {
                OrganonGrowth.Grow(simulationStep, configuration, stand, CALIB, treatments);
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
            Assert.IsTrue(stand.TreesBySpecies.Count > 0);
            Assert.IsTrue(stand.GetTreeRecordCount() > 0);

            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                Assert.IsTrue(Enum.IsDefined(typeof(FiaCode), species));

                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    // primary tree data
                    float crownRatio = treesOfSpecies.CrownRatio[treeIndex];
                    Assert.IsTrue(crownRatio >= 0.0F);
                    Assert.IsTrue(crownRatio <= 1.0F);
                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    Assert.IsTrue(dbhInInches >= 0.0F);
                    Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DiameterInInches);
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    Assert.IsTrue(expansionFactor >= 0.0F);
                    Assert.IsTrue(expansionFactor <= TestConstant.Maximum.ExpansionFactor);
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    Assert.IsTrue(heightInFeet >= 0.0F);
                    Assert.IsTrue(heightInFeet <= TestConstant.Maximum.HeightInFeet);

                    float deadExpansionFactor = treesOfSpecies.DeadExpansionFactor[treeIndex];
                    Assert.IsTrue(deadExpansionFactor >= 0.0F);
                    Assert.IsTrue(deadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);
                    Assert.IsTrue(expansionFactor + deadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                    // diameter and height growth
                    float diameterGrowthInInches = treesOfSpecies.DbhGrowth[treeIndex];
                    if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowth))
                    {
                        Assert.IsTrue(diameterGrowthInInches > 0.0F, "{0}: {1} {2} did not grow in diameter.", variant.TreeModel, treesOfSpecies.Species, treeIndex);
                        Assert.IsTrue(diameterGrowthInInches <= 0.1F * TestConstant.Maximum.DiameterInInches);
                    }
                    else if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowthOrNoChange))
                    {
                        Assert.IsTrue(diameterGrowthInInches >= 0.0F);
                        Assert.IsTrue(diameterGrowthInInches <= 0.1F * TestConstant.Maximum.DiameterInInches);
                    }
                    else
                    {
                        Assert.IsTrue(diameterGrowthInInches == 0.0F);
                    }
                    float heightGrowthInFeet = treesOfSpecies.HeightGrowth[treeIndex];
                    if (expectedGrowth.HasFlag(ExpectedTreeChanges.HeightGrowth))
                    {
                        Assert.IsTrue(heightGrowthInFeet > 0.0F, "{0}: {1} {2} did not grow in height.", variant.TreeModel, treesOfSpecies.Species, treeIndex);
                        Assert.IsTrue(heightGrowthInFeet <= 0.1F * TestConstant.Maximum.HeightInFeet);
                    }
                    else if (expectedGrowth.HasFlag(ExpectedTreeChanges.HeightGrowthOrNoChange))
                    {
                        Assert.IsTrue(heightGrowthInFeet >= 0.0F, "{0}: {1} {2} decreased in height.", variant.TreeModel, treesOfSpecies.Species, treeIndex);
                        Assert.IsTrue(heightGrowthInFeet <= 0.1F * TestConstant.Maximum.HeightInFeet);
                    }
                    else
                    {
                        Assert.IsTrue(heightGrowthInFeet == 0.0F);
                    }

                    // for now, ignore warnings on height exceeding potential height
                    // Assert.IsTrue(stand.TreeWarnings[treeWarningIndex] == 0);
                }

                for (int treeIndex = treesOfSpecies.Count; treeIndex < treesOfSpecies.Capacity; ++treeIndex)
                {
                    Assert.IsTrue(treesOfSpecies.CrownRatio[treeIndex] == 0.0F);
                    Assert.IsTrue(treesOfSpecies.Dbh[treeIndex] == 0.0F);
                    Assert.IsTrue(treesOfSpecies.DeadExpansionFactor[treeIndex] == 0.0F);
                    Assert.IsTrue(treesOfSpecies.Height[treeIndex] == 0.0F);
                    Assert.IsTrue(treesOfSpecies.LiveExpansionFactor[treeIndex] == 0.0F);
                }
            }

            Assert.IsTrue(stand.Warnings.BigSixHeightAbovePotential == false);
            Assert.IsTrue(stand.Warnings.LessThan50TreeRecords == expectedWarnings.HasFlag(OrganonWarnings.LessThan50TreeRecords));
            Assert.IsTrue(stand.Warnings.HemlockSiteIndexOutOfRange == expectedWarnings.HasFlag(OrganonWarnings.HemlockSiteIndex));
            Assert.IsTrue(stand.Warnings.OtherSpeciesBasalAreaTooHigh == false);
            Assert.IsTrue(stand.Warnings.SiteIndexOutOfRange == false);
            if (variant.TreeModel != TreeModel.OrganonSmc)
            {
                // for now, ignore SMC warning for breast height age < 10
                Assert.IsTrue(stand.Warnings.TreesOld == false);
            }
            // for now, ignore stand.Warnings.TreesYoung
        }

        protected void Verify(ExpectedTreeChanges expectedGrowth, TreeLifeAndDeath treeGrowth, TestStand initialStand, TestStand finalStand)
        {
            foreach (Trees finalTreesOfSpecies in finalStand.TreesBySpecies.Values)
            {
                float[] diameterGrowthOfSpecies = treeGrowth.TotalDbhGrowthInInches[finalTreesOfSpecies.Species];
                float[] deathOfSpecies = treeGrowth.TotalDeadExpansionFactor[finalTreesOfSpecies.Species];
                float[] heightGrowthOfSpecies = treeGrowth.TotalHeightGrowthInFeet[finalTreesOfSpecies.Species];
                Trees initialTreesOfSpecies = initialStand.TreesBySpecies[finalTreesOfSpecies.Species];

                for (int treeIndex = 0; treeIndex < finalTreesOfSpecies.Count; ++treeIndex)
                {
                    float totalDbhGrowth = diameterGrowthOfSpecies[treeIndex];
                    if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowth))
                    {
                        Assert.IsTrue(totalDbhGrowth > 0.0F);
                        Assert.IsTrue(totalDbhGrowth <= TestConstant.Maximum.DiameterInInches);
                    }
                    else if (expectedGrowth.HasFlag(ExpectedTreeChanges.DiameterGrowthOrNoChange))
                    {
                        Assert.IsTrue(totalDbhGrowth >= 0.0F);
                        Assert.IsTrue(totalDbhGrowth <= TestConstant.Maximum.DiameterInInches);
                    }
                    else
                    {
                        Assert.IsTrue(totalDbhGrowth == 0.0F);
                    }

                    float totalHeightGrowth = heightGrowthOfSpecies[treeIndex];
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

                    float totalDeadExpansionFactor = deathOfSpecies[treeIndex];
                    Assert.IsTrue(totalDeadExpansionFactor >= 0.0F);
                    Assert.IsTrue(totalDeadExpansionFactor <= TestConstant.Maximum.ExpansionFactor);

                    float initialTotalExpansionFactor = initialTreesOfSpecies.LiveExpansionFactor[treeIndex] + initialTreesOfSpecies.DeadExpansionFactor[treeIndex];
                    float finalTotalExpansionFactor = finalTreesOfSpecies.LiveExpansionFactor[treeIndex] + totalDeadExpansionFactor;
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

                for (int treeIndex = finalTreesOfSpecies.Count; treeIndex < finalTreesOfSpecies.Capacity; ++treeIndex)
                {
                    Assert.IsTrue(diameterGrowthOfSpecies[treeIndex] == 0.0F);
                    Assert.IsTrue(deathOfSpecies[treeIndex] == 0.0F);
                    Assert.IsTrue(heightGrowthOfSpecies[treeIndex] == 0.0F);
                    Assert.IsTrue(initialTreesOfSpecies.DbhGrowth[treeIndex] == 0.0F);
                    Assert.IsTrue(initialTreesOfSpecies.HeightGrowth[treeIndex] == 0.0F);
                    Assert.IsTrue(finalTreesOfSpecies.DbhGrowth[treeIndex] == 0.0F);
                    Assert.IsTrue(finalTreesOfSpecies.HeightGrowth[treeIndex] == 0.0F);
                }
            }
        }
    }
}
