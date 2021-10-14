using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Test
{
    [TestClass]
    public class OrganonInternal : OrganonTest
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public void CrownGrowthApi()
        {
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
                TestStand stand = OrganonTest.CreateDefaultStand(configuration);

                SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies = configuration.CreateSpeciesCalibration();
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonStandDensity densityStartOfStep = new(variant, stand);
                    Assert.IsTrue(densityStartOfStep.BasalAreaPerAcre > 0.0F);
                    Assert.IsTrue(densityStartOfStep.CrownCompetitionFactor > 0.0F);
                    Assert.IsTrue(densityStartOfStep.TreesPerAcre > 0.0F);

                    float[] crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand);
                    OrganonTest.Verify(crownCompetitionByHeight, variant);

                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        variant.AddCrownCompetitionByHeight(treesOfSpecies, crownCompetitionByHeight);
                        OrganonTest.Verify(crownCompetitionByHeight, variant);
                    }

                    OrganonStandDensity densityEndOfStep = new(variant, stand);
                    Assert.IsTrue(densityEndOfStep.BasalAreaPerAcre > 0.0F);
                    Assert.IsTrue(densityEndOfStep.CrownCompetitionFactor > 0.0F);
                    Assert.IsTrue(densityEndOfStep.TreesPerAcre > 0.0F);

                    #pragma warning disable IDE0059 // Unnecessary assignment of a value
                    crownCompetitionByHeight = OrganonGrowth.GrowCrown(variant, stand, densityEndOfStep, calibrationBySpecies);
                    #pragma warning restore IDE0059 // Unnecessary assignment of a value
                    OrganonTest.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);
                    OrganonTest.Verify(calibrationBySpecies);
                }
            }
        }

        [TestMethod]
        public void DiameterGrowthApi()
        {
            OrganonTreatments treatments = new();
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
                TestStand stand = OrganonTest.CreateDefaultStand(configuration);
                SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies = configuration.CreateSpeciesCalibration();

                SortedList<FiaCode, float[]> previousTreeDiametersBySpecies = new();
                foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                {
                    previousTreeDiametersBySpecies.Add(treesOfSpecies.Species, new float[treesOfSpecies.Capacity]);
                }

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonStandDensity treeCompetition = new(variant, stand);
                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        float[] previousTreeDiameters = previousTreeDiametersBySpecies[treesOfSpecies.Species];
                        OrganonGrowth.GrowDiameter(configuration, treatments, stand, treesOfSpecies, treeCompetition, calibrationBySpecies[treesOfSpecies.Species].Diameter);
                        stand.SetSdiMax(configuration);
                        for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                        {
                            float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                            float previousDbhInInches = previousTreeDiameters[treeIndex];
                            Assert.IsTrue(dbhInInches >= previousDbhInInches);
                            Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DiameterInInches);
                        }
                    }

                    OrganonTest.Verify(ExpectedTreeChanges.DiameterGrowth, stand, variant);
                    OrganonTest.Verify(calibrationBySpecies);
                }

                OrganonStandDensity densityForLookup = new(variant, stand);
                for (float dbhInInches = 0.5F; dbhInInches <= 101.0F; ++dbhInInches)
                {
                    float basalAreaLarger = densityForLookup.GetBasalAreaLarger(dbhInInches);
                    Assert.IsTrue(basalAreaLarger >= 0.0F);
                    Assert.IsTrue(basalAreaLarger <= densityForLookup.BasalAreaPerAcre);
                    float crownCompetitionLarger = densityForLookup.GetCrownCompetitionFactorLarger(dbhInInches);
                    Assert.IsTrue(crownCompetitionLarger >= 0.0F);
                    Assert.IsTrue(crownCompetitionLarger <= densityForLookup.CrownCompetitionFactor);
                }
            }
        }

        [TestMethod]
        public void GrowApi()
        {
            OrganonTreatments treatments = new();
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
                TestStand stand = OrganonTest.CreateDefaultStand(configuration);

                SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies = configuration.CreateSpeciesCalibration();
                OrganonStandDensity densityStartOfStep = new(variant, stand);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    float[] crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand);
                    OrganonGrowth.Grow(configuration, treatments, stand, densityStartOfStep, calibrationBySpecies, ref crownCompetitionByHeight, 
                                       out OrganonStandDensity densityEndOfStep, out int _);
                    stand.SetSdiMax(configuration);

                    OrganonTest.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, stand, variant);
                    OrganonTest.Verify(calibrationBySpecies);

                    densityStartOfStep = densityEndOfStep;
                }
            }
        }

        [TestMethod]
        public void GrowthModifiersApi()
        {
            this.TestContext!.WriteLine("tree age, diameter genetic factor, height genetic factor, diameter growth modifier, height growth modifier");
            for (float treeAgeInYears = 0.0F; treeAgeInYears <= 50.0F; treeAgeInYears += Constant.DefaultTimeStepInYears)
            {
                for (float diameterGeneticFactor = 0.0F; diameterGeneticFactor <= 25.0F; diameterGeneticFactor += 5.0F)
                {
                    for (float heightGeneticFactor = 0.0F; heightGeneticFactor <= 25.0F; heightGeneticFactor += 5.0F)
                    {
                        OrganonGrowthModifiers.GetGeneticModifiers(treeAgeInYears, diameterGeneticFactor, heightGeneticFactor, out float diameterGrowthModifier, out float heightGrowthModifier);
                        this.TestContext.WriteLine("{0},{1},{2},{3},{4}", treeAgeInYears, diameterGeneticFactor, heightGeneticFactor, diameterGrowthModifier, heightGrowthModifier);
                        Assert.IsTrue(diameterGrowthModifier >= 1.0F);
                        Assert.IsTrue(diameterGrowthModifier < 2.0F);
                        Assert.IsTrue(heightGrowthModifier >= 1.0F);
                        Assert.IsTrue(heightGrowthModifier < 2.0F);
                    }
                }
            }

            this.TestContext.WriteLine("FR, diameter growth modifier, height growth modifier");
            for (float FR = 0.5F; FR <= 5.0F; FR += 0.5F)
            {
                OrganonGrowthModifiers.GetSwissNeedleCastModifiers(FR, out float diameterGrowthModifier, out float heightGrowthModifier);
                this.TestContext.WriteLine("{0},{1},{2}", FR, diameterGrowthModifier, heightGrowthModifier);
                Assert.IsTrue(diameterGrowthModifier >= 0.0F);
                Assert.IsTrue(diameterGrowthModifier <= 1.0F);
                Assert.IsTrue(heightGrowthModifier >= 0.0F);
                Assert.IsTrue(heightGrowthModifier <= 1.0F);
            }
        }

        [TestMethod]
        public void HeightGrowthApi()
        {
            OrganonTreatments treatments = new();
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
                SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies = configuration.CreateSpeciesCalibration();
                TestStand stand = OrganonTest.CreateDefaultStand(configuration);

                float[] crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand);
                DouglasFir.SiteConstants psmeSite = new(stand.SiteIndexInFeet);
                WesternHemlock.SiteConstants tsheSite = new(stand.HemlockSiteIndexInFeet);

                foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                {
                    variant.GetHeightPredictionCoefficients(treesOfSpecies.Species, out float B0, out float B1, out float B2);
                    for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                    {
                        // predicted heights
                        float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                        float heightInFeet = treesOfSpecies.Height[treeIndex];
                        float predictedHeightInFeet = 4.5F + MathV.Exp(B0 + B1 * MathV.Pow(dbhInInches, B2));
                        Assert.IsTrue(predictedHeightInFeet >= 0.0F);
                        // TODO: make upper limit of height species specific
                        Assert.IsTrue(predictedHeightInFeet < TestConstant.Maximum.HeightInFeet);

                        // growth effective age and potential height growth
                        bool verifyAgeAndHeight = false;
                        float growthEffectiveAgeInYears = -1.0F;
                        float potentialHeightGrowth = -1.0F;
                        if ((variant.TreeModel == TreeModel.OrganonNwo) || (variant.TreeModel == TreeModel.OrganonSmc))
                        {
                            if (treesOfSpecies.Species == FiaCode.TsugaHeterophylla)
                            {
                                growthEffectiveAgeInYears = WesternHemlock.GetFlewellingGrowthEffectiveAge(tsheSite, variant.TimeStepInYears, heightInFeet, out potentialHeightGrowth);
                            }
                            else
                            {
                                growthEffectiveAgeInYears = DouglasFir.GetPsmeAbgrGrowthEffectiveAge(psmeSite, variant.TimeStepInYears, heightInFeet, out potentialHeightGrowth);
                            }
                            verifyAgeAndHeight = true;
                        }
                        else if (variant.TreeModel == TreeModel.OrganonSwo)
                        {
                            if ((treesOfSpecies.Species == FiaCode.PinusPonderosa) || (treesOfSpecies.Species == FiaCode.PseudotsugaMenziesii))
                            {
                                DouglasFir.GetDouglasFirPonderosaHeightGrowth(treesOfSpecies.Species == FiaCode.PseudotsugaMenziesii, stand.SiteIndexInFeet, heightInFeet, out growthEffectiveAgeInYears, out potentialHeightGrowth);
                                verifyAgeAndHeight = true;
                            }
                        }
                        if (verifyAgeAndHeight)
                        {
                            Assert.IsTrue(growthEffectiveAgeInYears >= -2.0F);
                            Assert.IsTrue(growthEffectiveAgeInYears <= 500.0F);
                            Assert.IsTrue(potentialHeightGrowth >= 0.0F);
                            Assert.IsTrue(potentialHeightGrowth < 20.0F);
                        }
                    }
                }

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        if (variant.IsBigSixSpecies(treesOfSpecies.Species))
                        {
                            // TODO: why no height calibration in Organon API?
                            OrganonGrowth.GrowHeightBigSixSpecies(configuration, treatments, stand, treesOfSpecies, 1.0F, crownCompetitionByHeight, out _);
                        }
                        else
                        {
                            OrganonGrowth.GrowHeightMinorSpecies(configuration, stand, treesOfSpecies, calibrationBySpecies[treesOfSpecies.Species].Height);
                        }
                        stand.SetSdiMax(configuration);

                        for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                        {
                            float heightInFeet = treesOfSpecies.Height[treeIndex];
                            // TODO: make upper limit of height species specific
                            Assert.IsTrue(heightInFeet < TestConstant.Maximum.HeightInFeet);
                        }
                    }

                    // since diameter growth is zero in this test any tree which is above its anticipated height for its current diameter 
                    // should have zero growth
                    // This is expected behavior the height growth functions and, potentially, height growth limiting.
                    OrganonTest.Verify(ExpectedTreeChanges.HeightGrowthOrNoChange, stand, variant);
                    OrganonTest.Verify(calibrationBySpecies);
                }
            }
        }

        [TestMethod]
        public void MortalityApi()
        {
            OrganonTreatments treatments = new();
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
                TestStand stand = OrganonTest.CreateDefaultStand(configuration);
                OrganonStandDensity density = new(variant, stand);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonMortality.ReduceExpansionFactors(configuration, treatments, stand, density);
                    stand.SetSdiMax(configuration);
                    OrganonTest.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);
                    float oldGrowthIndicator  = OrganonMortality.GetOldGrowthIndicator(variant, stand);
                    Assert.IsTrue(oldGrowthIndicator >= 0.0F);
                    Assert.IsTrue(oldGrowthIndicator <= 2.0F);
                }
            }
        }

        [TestMethod]
        public void StatsApi()
        {
            // no test coverage: one line function
            // Stats.CON_RASI();

            // no test coverage: one line function
            // Stats.RASITE();

            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
                TestStand stand = OrganonTest.CreateDefaultStand(configuration);
                OrganonStandDensity standDensity = new(variant, stand);

                this.TestContext!.WriteLine("{0},{1} ft²/ac,{2} trees per acre,{3} crown competition factor", variant, standDensity.BasalAreaPerAcre, standDensity.TreesPerAcre, standDensity.CrownCompetitionFactor);
                this.TestContext.WriteLine("index,large tree BA larger,large tree CCF larger");
                for (int largeTreeCompetitionIndex = 0; largeTreeCompetitionIndex < standDensity.LargeTreeBasalAreaLarger.Length; ++largeTreeCompetitionIndex)
                {
                    float largeTreeBasalAreaLarger = standDensity.LargeTreeBasalAreaLarger[largeTreeCompetitionIndex];
                    float largeTreeCrownCompetitionFactor = standDensity.LargeTreeCrownCompetition[largeTreeCompetitionIndex];
                    Assert.IsTrue(largeTreeBasalAreaLarger >= 0.0F);
                    Assert.IsTrue(largeTreeBasalAreaLarger < TestConstant.Maximum.TreeBasalAreaLarger);
                    Assert.IsTrue(largeTreeCrownCompetitionFactor >= 0.0F);
                    Assert.IsTrue(largeTreeCrownCompetitionFactor < TestConstant.Maximum.StandCrownCompetitionFactor);
                    this.TestContext.WriteLine("{0},{1}", largeTreeBasalAreaLarger, largeTreeCrownCompetitionFactor);
                }
                this.TestContext.WriteLine("index,small tree BA larger,large tree CCF larger");
                for (int smallTreeCompetitionIndex = 0; smallTreeCompetitionIndex < standDensity.SmallTreeBasalAreaLarger.Length; ++smallTreeCompetitionIndex)
                {
                    float smallTreeBasalAreaLarger = standDensity.SmallTreeBasalAreaLarger[smallTreeCompetitionIndex];
                    float smallTreeCrownCompetitionFactor = standDensity.SmallTreeCrownCompetition[smallTreeCompetitionIndex];
                    Assert.IsTrue(smallTreeBasalAreaLarger >= 0.0F);
                    Assert.IsTrue(smallTreeBasalAreaLarger < TestConstant.Maximum.TreeBasalAreaLarger);
                    Assert.IsTrue(smallTreeCrownCompetitionFactor >= 0.0F);
                    Assert.IsTrue(smallTreeCrownCompetitionFactor < TestConstant.Maximum.StandCrownCompetitionFactor);
                    this.TestContext.WriteLine("{0},{1}", smallTreeBasalAreaLarger, smallTreeCrownCompetitionFactor);
                }
                this.TestContext.WriteLine(String.Empty);

                OrganonTest.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);
            }
        }

        //[TestMethod]
        //public void RedAlderApi()
        //{
        //    no test coverage: one line or effectively one line functions
        //    RedAlder.RAH40();
        //    RedAlder.RAGEA();
        //    RedAlder.WHHLB_GEA();
        //    RedAlder.WHHLB_H40();
        //    RedAlder.WHHLB_SI_UC();
        //}

        [TestMethod]
        public void SubmaxApi()
        {
            this.TestContext!.WriteLine("version, A1, A2");
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = OrganonTest.CreateOrganonConfiguration(variant);
                TestStand stand = OrganonTest.CreateDefaultStand(configuration);
                this.TestContext.WriteLine("{0},{1},{2}", variant, stand.SdiMaxConstantA1, stand.SdiMaxExponentA2);

                Assert.IsTrue(stand.SdiMaxConstantA1 < 7.0F);
                Assert.IsTrue(stand.SdiMaxConstantA1 > 5.0F);
                Assert.IsTrue(stand.SdiMaxExponentA2 > 0.60F);
                Assert.IsTrue(stand.SdiMaxExponentA2 < 0.65F);
                OrganonTest.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);
            }
        }

        [TestMethod]
        public void WesternHemlockApi()
        {
            this.TestContext!.WriteLine("siteIndex, age, topHeight");
            for (float siteIndexInMeters = 10.0F; siteIndexInMeters < 60.1F; siteIndexInMeters += 10.0F)
            {
                WesternHemlock.SiteConstants tsheSite = new(Constant.FeetPerMeter * siteIndexInMeters);
                float previousTopHeight = -1.0F;
                for (float ageInYears = 0.0F; ageInYears < 100.1F; ++ageInYears)
                {
                    WesternHemlock.SITECV_F(tsheSite, ageInYears, out float topHeightInMeters);
                    this.TestContext.WriteLine("{0},{1},{2}", siteIndexInMeters, ageInYears, topHeightInMeters);

                    Assert.IsTrue(topHeightInMeters >= 0.0F);
                    // could add check that SI at age 50 is close to specified value
                    Assert.IsTrue(topHeightInMeters > previousTopHeight);
                    Assert.IsTrue(topHeightInMeters < 100.0F);

                    previousTopHeight = topHeightInMeters;
                }
            }
        }
    }
}
