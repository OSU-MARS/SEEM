using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Species;
using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Test
{
    [TestClass]
    public class Component : OrganonTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CrownGrowthApi()
        {
            OrganonGrowth treeGrowth = new OrganonGrowth();
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);

                Dictionary<FiaCode, float[]> CALIB = configuration.CreateSpeciesCalibration();
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonStandDensity densityStartOfStep = new OrganonStandDensity(stand, variant);
                    Assert.IsTrue(densityStartOfStep.BasalAreaPerAcre > 0.0F);
                    Assert.IsTrue(densityStartOfStep.CrownCompetitionFactor > 0.0F);
                    Assert.IsTrue(densityStartOfStep.TreesPerAcre > 0.0F);

                    float[] crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand);
                    this.Verify(crownCompetitionByHeight, variant);

                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        variant.AddCrownCompetitionByHeight(treesOfSpecies, crownCompetitionByHeight);
                        this.Verify(crownCompetitionByHeight, variant);
                    }

                    OrganonStandDensity densityEndOfStep = new OrganonStandDensity(stand, variant);
                    Assert.IsTrue(densityEndOfStep.BasalAreaPerAcre > 0.0F);
                    Assert.IsTrue(densityEndOfStep.CrownCompetitionFactor > 0.0F);
                    Assert.IsTrue(densityEndOfStep.TreesPerAcre > 0.0F);

                    #pragma warning disable IDE0059 // Unnecessary assignment of a value
                    crownCompetitionByHeight = treeGrowth.GrowCrown(variant, stand, densityEndOfStep, CALIB);
                    #pragma warning restore IDE0059 // Unnecessary assignment of a value
                    this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);
                    this.Verify(CALIB);
                }
            }
        }

        [TestMethod]
        public void DiameterGrowthApi()
        {
            OrganonGrowth treeGrowth = new OrganonGrowth();
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                OrganonTreatments treatments = new OrganonTreatments();
                Dictionary<FiaCode, float[]> CALIB = configuration.CreateSpeciesCalibration();

                Dictionary<FiaCode, float[]> previousTreeDiametersBySpecies = new Dictionary<FiaCode, float[]>();
                foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                {
                    previousTreeDiametersBySpecies.Add(treesOfSpecies.Species, new float[treesOfSpecies.Capacity]);
                }

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonStandDensity treeCompetition = new OrganonStandDensity(stand, variant);
                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        float[] previousTreeDiameters = previousTreeDiametersBySpecies[treesOfSpecies.Species];
                        treeGrowth.GrowDiameter(variant, simulationStep, stand, treesOfSpecies, treeCompetition, CALIB[treesOfSpecies.Species][2], treatments);
                        stand.SetSdiMax(configuration);
                        for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                        {
                            float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                            float previousDbhInInches = previousTreeDiameters[treeIndex];
                            Assert.IsTrue(dbhInInches >= previousDbhInInches);
                            Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DiameterInInches);
                        }
                    }

                    this.Verify(ExpectedTreeChanges.DiameterGrowth, stand, variant);
                    this.Verify(CALIB);
                }

                OrganonStandDensity densityForLookup = new OrganonStandDensity(stand, variant);
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
            OrganonGrowth treeGrowth = new OrganonGrowth();
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                OrganonTreatments treatments = new OrganonTreatments();

                Dictionary<FiaCode, float[]> CALIB = configuration.CreateSpeciesCalibration();
                OrganonStandDensity densityStartOfStep = new OrganonStandDensity(stand, variant);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; /* incremented by GROW() */)
                {
                    float[] crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand);
                    treeGrowth.Grow(ref simulationStep, configuration, stand, densityStartOfStep, CALIB, treatments, ref crownCompetitionByHeight, 
                                    out OrganonStandDensity densityEndOfStep, out int _);
                    stand.SetSdiMax(configuration);

                    this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, stand, variant);
                    this.Verify(CALIB);

                    densityStartOfStep = densityEndOfStep;
                }
            }
        }

        [TestMethod]
        public void GrowthModifiersApi()
        {
            this.TestContext.WriteLine("tree age, diameter genetic factor, height genetic factor, diameter growth modifier, height growth modifier");
            for (float treeAgeInYears = 0.0F; treeAgeInYears <= 50.0F; treeAgeInYears += Constant.DefaultTimeStepInYears)
            {
                for (float diameterGeneticFactor = 0.0F; diameterGeneticFactor <= 25.0F; diameterGeneticFactor += 5.0F)
                {
                    for (float heightGeneticFactor = 0.0F; heightGeneticFactor <= 25.0F; heightGeneticFactor += 5.0F)
                    {
                        OrganonGrowthModifiers.GG_MODS(treeAgeInYears, diameterGeneticFactor, heightGeneticFactor, out float diameterGrowthModifier, out float heightGrowthModifier);
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
                OrganonGrowthModifiers.SNC_MODS(FR, out float diameterGrowthModifier, out float heightGrowthModifier);
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
            OrganonGrowth treeGrowth = new OrganonGrowth();
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                Dictionary<FiaCode, float[]> CALIB = configuration.CreateSpeciesCalibration();
                TestStand stand = this.CreateDefaultStand(configuration);
                OrganonTreatments treatments = new OrganonTreatments();

                float[] crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand);
                DouglasFir.SiteConstants psmeSite = new DouglasFir.SiteConstants(stand.SiteIndex);
                WesternHemlock.SiteConstants tsheSite = new WesternHemlock.SiteConstants(stand.HemlockSiteIndex);

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
                                growthEffectiveAgeInYears = DouglasFir.GetBrucePsmeAbgrGrowthEffectiveAge(psmeSite, variant.TimeStepInYears, heightInFeet, out potentialHeightGrowth);
                            }
                            verifyAgeAndHeight = true;
                        }
                        else if (variant.TreeModel == TreeModel.OrganonSwo)
                        {
                            if ((treesOfSpecies.Species == FiaCode.PinusPonderosa) || (treesOfSpecies.Species == FiaCode.PseudotsugaMenziesii))
                            {
                                DouglasFir.GetDouglasFirPonderosaHeightGrowth(treesOfSpecies.Species == FiaCode.PseudotsugaMenziesii, stand.SiteIndex, heightInFeet, out growthEffectiveAgeInYears, out potentialHeightGrowth);
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
                            treeGrowth.GrowHeightBigSixSpecies(configuration, simulationStep, stand, treesOfSpecies, 1.0F, crownCompetitionByHeight, treatments, out _);
                        }
                        else
                        {
                            treeGrowth.GrowHeightMinorSpecies(configuration, stand, treesOfSpecies, CALIB[treesOfSpecies.Species][0]);
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
                    this.Verify(ExpectedTreeChanges.HeightGrowthOrNoChange, stand, variant);
                    this.Verify(CALIB);
                }
            }
        }

        [TestMethod]
        public void MortalityApi()
        {
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                OrganonTreatments treatments = new OrganonTreatments();

                TestStand stand = this.CreateDefaultStand(configuration);
                OrganonStandDensity density = new OrganonStandDensity(stand, variant);
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonMortality.ReduceExpansionFactors(configuration, simulationStep, stand, density, treatments);
                    stand.SetSdiMax(configuration);
                    this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);
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
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                OrganonStandDensity standDensity = new OrganonStandDensity(stand, variant);

                this.TestContext.WriteLine("{0},{1} ft²/ac,{2} trees per acre,{3} crown competition factor", variant, standDensity.BasalAreaPerAcre, standDensity.TreesPerAcre, standDensity.CrownCompetitionFactor);
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

                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);
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
            this.TestContext.WriteLine("version, A1, A2");
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                this.TestContext.WriteLine("{0},{1},{2}", variant, stand.A1, stand.A2);

                Assert.IsTrue(stand.A1 < 7.0F);
                Assert.IsTrue(stand.A1 > 5.0F);
                Assert.IsTrue(stand.A2 > 0.60F);
                Assert.IsTrue(stand.A2 < 0.65F);
                this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);
            }
        }

        [TestMethod]
        public void WesternHemlockApi()
        {
            this.TestContext.WriteLine("siteIndex, age, topHeight");
            for (float siteIndexInMeters = 10.0F; siteIndexInMeters < 60.1F; siteIndexInMeters += 10.0F)
            {
                WesternHemlock.SiteConstants tsheSite = new WesternHemlock.SiteConstants(Constant.FeetPerMeter * siteIndexInMeters);
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
