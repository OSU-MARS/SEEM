using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon.Test
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

                Dictionary<FiaCode, float[]> CALIB = this.CreateSpeciesCalibration(variant);
                float OG = 0.0F; // (DOUG?)

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    StandDensity densityStartOfStep = new StandDensity(stand, variant);
                    Assert.IsTrue(densityStartOfStep.BasalAreaPerAcre > 0.0F);
                    Assert.IsTrue(densityStartOfStep.CrownCompetitionFactor > 0.0F);
                    Assert.IsTrue(densityStartOfStep.TreesPerAcre > 0.0F);

                    float[] CCH = StandDensity.GetCrownCompetitionByHeight(variant, stand);
                    float crownCompetitionFactor = CCH[0];
                    Assert.IsTrue(crownCompetitionFactor >= 0.0F);
                    Assert.IsTrue(crownCompetitionFactor <= TestConstant.Maximum.CrownCompetitionFactor);

                    for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                    {
                        float crownRatio = stand.CrownRatio[treeIndex];
                        float dbhInInches = stand.Dbh[treeIndex];
                        float heightInFeet = stand.Height[treeIndex];
                        FiaCode species = stand.Species[treeIndex];

                        float crownCompetitionFactorLarger = densityStartOfStep.GetCrownCompetitionFactorLarger(dbhInInches);
                        Assert.IsTrue(crownCompetitionFactorLarger >= 0.0F);
                        Assert.IsTrue(crownCompetitionFactorLarger < TestConstant.Maximum.StandCrownCompetitionFactor);

                        float maximumCrownWidthInFeet = variant.GetMaximumCrownWidth(species, dbhInInches, heightInFeet);
                        float largestCrownWidthInFeet = variant.GetLargestCrownWidth(species, maximumCrownWidthInFeet, crownRatio, dbhInInches, heightInFeet);
                        float heightToCrownBaseInFeet = variant.GetHeightToCrownBase(species, heightInFeet, dbhInInches, crownCompetitionFactorLarger, densityStartOfStep.BasalAreaPerAcre, stand.SiteIndex, stand.HemlockSiteIndex, OG);
                        float heightToLargestCrownWidthInFeet = variant.GetHeightToLargestCrownWidth(species, heightInFeet, crownRatio);
                        // (DOUG? can height to largest crown width be less than height to crown base?)
                        Assert.IsTrue(heightToCrownBaseInFeet >= 0.0F);
                        Assert.IsTrue(heightToCrownBaseInFeet <= heightInFeet);
                        Assert.IsTrue(heightToLargestCrownWidthInFeet >= 0.0F);
                        Assert.IsTrue(heightToLargestCrownWidthInFeet <= heightInFeet);
                        // (DOUG? bound largest or maximum crown width based on the other?)
                        Assert.IsTrue(largestCrownWidthInFeet >= 0.0F);
                        Assert.IsTrue(largestCrownWidthInFeet < TestConstant.Maximum.LargestCrownWidthInFeet);
                        Assert.IsTrue(maximumCrownWidthInFeet >= 0.0F);
                        Assert.IsTrue(maximumCrownWidthInFeet < TestConstant.Maximum.MaximumCrownWidthInFeet);

                        float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                        StandDensity.GetCrownCompetitionByHeight(variant, species, heightToLargestCrownWidthInFeet, largestCrownWidthInFeet, heightInFeet, 
                                            dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        for (int cchIndex = 0; cchIndex < CCH.Length; ++cchIndex)
                        {
                            float cch = CCH[cchIndex];
                            Assert.IsTrue(cch >= 0.0F);
                            Assert.IsTrue(cch < 1000.0F);
                        }
                    }

                    StandDensity densityEndOfStep = new StandDensity(stand, variant);
                    Assert.IsTrue(densityEndOfStep.BasalAreaPerAcre > 0.0F);
                    Assert.IsTrue(densityEndOfStep.CrownCompetitionFactor > 0.0F);
                    Assert.IsTrue(densityEndOfStep.TreesPerAcre > 0.0F);

                    CCH = treeGrowth.GrowCrown(variant, stand, densityStartOfStep, densityEndOfStep, stand.SiteIndex, stand.HemlockSiteIndex, CALIB);
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

                float BABT = 0.0F; // (DOUG?)
                float[] BART = new float[5]; // (DOUG?)
                Dictionary<FiaCode, float[]> CALIB = this.CreateSpeciesCalibration(variant);
                float[] CCH = new float[41]; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                float[] YF = new float[5]; // (DOUG?)
                float[] YT = new float[5]; // (DOUG?)

                float[] previousTreeDiameters = new float[stand.TreeRecordCount];
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    StandDensity treeCompetition = new StandDensity(stand, variant);
                    for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                    {
                        treeGrowth.GrowDiameter(variant, treeIndex, simulationStep, stand, stand.SiteIndex, 
                                                stand.HemlockSiteIndex, treeCompetition, CALIB, 
                                                PN, YF, BABT, BART, YT);
                        stand.SetSdiMax(configuration);

                        float dbhInInches = stand.Dbh[treeIndex];
                        float previousDbhInInches = previousTreeDiameters[treeIndex];
                        Assert.IsTrue(dbhInInches >= previousDbhInInches);
                        Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DiameterInInches);

                        previousDbhInInches = dbhInInches;
                    }

                    this.Verify(ExpectedTreeChanges.DiameterGrowth, stand, variant);
                    this.Verify(CALIB);
                }

                StandDensity densityForLookup = new StandDensity(stand, variant);
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
                float BABT = 0.0F; // (DOUG?)
                float[] BART = new float[5]; // (DOUG?)
                Dictionary<FiaCode, float[]> CALIB = this.CreateSpeciesCalibration(variant);
                float[] CCH = new float[41]; // (DOUG?)
                int fertlizerCycle = 0;
                float OLD = 0.0F; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                int thinningCycle = 0;
                float[] YF = new float[5]; // (DOUG?)
                float[] YT = new float[5]; // (DOUG?)

                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; /* incremented by GROW() */)
                {
                    StandDensity densityStartOfStep = new StandDensity(stand, variant);
                    treeGrowth.Grow(ref simulationStep, configuration, stand, ref thinningCycle, ref fertlizerCycle,
                                    densityStartOfStep, CALIB, PN, YF, 
                                    BABT, BART, YT, ref CCH, ref OLD, TestConstant.Default.RAAGE, out StandDensity densityEndOfStep);
                    stand.SetSdiMax(configuration);

                    this.Verify(ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, stand, variant);
                    this.Verify(CALIB);
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
            float[] BART = new float[5]; // (DOUG?)
            float[] CCH = new float[41]; // (DOUG? why is this different from the other competition arrays?)
            float OLD = 0.0F; // (DOUG?)
            float[] PN = new float[5]; // (DOUG?)
            OrganonGrowth treeGrowth = new OrganonGrowth();
            float[] YF = new float[5]; // (DOUG?)
            float[] YT = new float[5]; // (DOUG?)
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                Dictionary<FiaCode, float[]> CALIB = this.CreateSpeciesCalibration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);

                for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                {
                    // predicted heights
                    FiaCode species = stand.Species[treeIndex];
                    float dbhInInches = stand.Dbh[treeIndex];
                    float heightInFeet = stand.Height[treeIndex];
                    float predictedHeightInFeet = variant.GetPredictedHeight(species, dbhInInches);
                    Assert.IsTrue(predictedHeightInFeet >= 0.0F);
                    // TODO: make upper limit of height species specific
                    Assert.IsTrue(predictedHeightInFeet < TestConstant.Maximum.HeightInFeet);

                    // growth effective age and potential height growth
                    bool verifyAgeAndHeight = false;
                    float growthEffectiveAgeInYears = -1.0F;
                    float potentialHeightGrowth = -1.0F;
                    if ((variant.TreeModel == TreeModel.OrganonNwo) || (variant.TreeModel == TreeModel.OrganonSmc))
                    {
                        if (species == FiaCode.TsugaHeterophylla)
                        {
                            WesternHemlock.F_HG(stand.SiteIndex, heightInFeet, TestConstant.Default.AgeToReachBreastHeightInYears, out growthEffectiveAgeInYears, out potentialHeightGrowth);
                        }
                        else
                        {
                            DouglasFir.BrucePsmeAbgrGrowthEffectiveAge(stand.SiteIndex, heightInFeet, TestConstant.Default.AgeToReachBreastHeightInYears, out growthEffectiveAgeInYears, out potentialHeightGrowth);
                        }
                        verifyAgeAndHeight = true;
                    }
                    else if (variant.TreeModel == TreeModel.OrganonSwo)
                    {
                        if ((species == FiaCode.PinusPonderosa) || (species == FiaCode.PseudotsugaMenziesii))
                        {
                            DouglasFir.DouglasFirPonderosaHeightGrowth(species == FiaCode.PseudotsugaMenziesii, stand.SiteIndex, heightInFeet, out growthEffectiveAgeInYears, out potentialHeightGrowth);
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

                float[] previousTreeHeights = new float[stand.TreeRecordCount];
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                    {
                        if (variant.IsBigSixSpecies(stand.Species[treeIndex]))
                        {
                            treeGrowth.GrowHeightBigSixSpecies(treeIndex, variant, simulationStep, stand, stand.SiteIndex, stand.HemlockSiteIndex, 
                                                CCH, PN, YF, TestConstant.Default.BABT, BART, YT, ref OLD, TestConstant.Default.PDEN);
                        }
                        else
                        {
                            treeGrowth.GrowHeightMinorSpecies(treeIndex, variant, stand, CALIB);
                        }
                        stand.SetSdiMax(configuration);

                        float heightInFeet = stand.Height[treeIndex];
                        float previousHeightInFeet = previousTreeHeights[treeIndex];
                        // TODO: make upper limit of height species specific
                        Assert.IsTrue(heightInFeet < TestConstant.Maximum.HeightInFeet);

                        previousTreeHeights[treeIndex] = heightInFeet;
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
            float[] PN = new float[5]; // (DOUG?)
            float[] YF = new float[5]; // (DOUG?)
            
            foreach (OrganonVariant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);

                TestStand stand = this.CreateDefaultStand(configuration);
                StandDensity density = new StandDensity(stand, variant);
                float RAAGE = TestConstant.Default.RAAGE;
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    OrganonMortality.MORTAL(configuration, simulationStep, stand, density, stand.SiteIndex, 
                                     stand.HemlockSiteIndex, PN, YF, ref RAAGE);
                    stand.SetSdiMax(configuration);
                    this.Verify(ExpectedTreeChanges.NoDiameterOrHeightGrowth, stand, variant);

                    // TODO: xind -1.0 case
                    float xind = 0.0F;
                    OrganonMortality.OldGro(variant, stand, xind, out float OG);
                    Assert.IsTrue(OG >= 0.0F);
                    Assert.IsTrue(OG <= 2.0F);
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
                StandDensity standDensity = new StandDensity(stand, variant);

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
        public void TreeVolumeApi()
        {
            FiaVolume fiaVolume = new FiaVolume();
            OsuVolume osuVolume = new OsuVolume();
            Trees trees = new Trees(40);
            float[] fiaMerchantableCvtsByTreeInCubicMeters = new float[trees.TreeRecordCount];
            float[] osuCvtsByTreeInCubicMeters = new float[trees.TreeRecordCount];
            bool isDouglasFir = true;
            float cvtsPerHa = 0.0F;
            float merchantableCvtsPerHa = 0.0F;
            float totalCylinderVolume = 0.0F;
            for (int treeIndex = 0; treeIndex < trees.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = isDouglasFir ? FiaCode.PseudotsugaMenziesii : FiaCode.TsugaHeterophylla;
                TreeRecord tree = new TreeRecord(species, (float)treeIndex, 1.0F, 1.0F - 3.0F * (float)trees.TreeRecordCount / (float)treeIndex);
                trees.Species[treeIndex] = tree.Species;
                trees.Dbh[treeIndex] = tree.DbhInInches;
                trees.Height[treeIndex] = tree.HeightInFeet;
                trees.CrownRatio[treeIndex] = tree.CrownRatio;
                trees.LiveExpansionFactor[treeIndex] = tree.ExpansionFactor;
                float dbhInMeters = TestConstant.MetersPerInch * tree.DbhInInches;
                float heightInMeters = Constant.MetersPerFoot * tree.HeightInFeet;
                float treeSizedCylinderVolume = (float)Math.PI * dbhInMeters * dbhInMeters * heightInMeters;

                fiaMerchantableCvtsByTreeInCubicMeters[treeIndex] = fiaVolume.GetMerchantableCubicVolumePerHectare(trees, treeIndex);
                merchantableCvtsPerHa += fiaMerchantableCvtsByTreeInCubicMeters[treeIndex];

                osuCvtsByTreeInCubicMeters[treeIndex] = osuVolume.GetCubicVolumePerHectare(trees, treeIndex);
                cvtsPerHa += osuCvtsByTreeInCubicMeters[treeIndex];

                // taper coefficient should be in the vicinity of 0.3 for larger trees, but this is not well defined for small trees
                // Lower bound can be made more stringent if necessary.
                Debug.Assert(fiaMerchantableCvtsByTreeInCubicMeters[treeIndex] >= 0.0F);
                Debug.Assert(fiaMerchantableCvtsByTreeInCubicMeters[treeIndex] <= 0.35 * treeSizedCylinderVolume);
                totalCylinderVolume += treeSizedCylinderVolume;
            }

            Debug.Assert(merchantableCvtsPerHa >= 0.05 * totalCylinderVolume);
            Debug.Assert(merchantableCvtsPerHa <= 0.35 * totalCylinderVolume);
            Debug.Assert(merchantableCvtsPerHa <= cvtsPerHa);
            Debug.Assert(cvtsPerHa <= 0.35 * totalCylinderVolume);

            float standCvtsRatio = merchantableCvtsPerHa / fiaVolume.GetMerchantableCubicVolumePerHectare(trees);
            Debug.Assert(standCvtsRatio >= 0.999);
            Debug.Assert(standCvtsRatio <= 1.001);
        }

        [TestMethod]
        public void WesternHemlockApi()
        {
            this.TestContext.WriteLine("siteIndex, age, topHeight");
            for (float siteIndexInMeters = 10.0F; siteIndexInMeters < 60.1F; siteIndexInMeters += 10.0F)
            {
                float previousTopHeight = -1.0F;
                for (float ageInYears = 0.0F; ageInYears < 100.1F; ++ageInYears)
                {
                    WesternHemlock.SITECV_F(siteIndexInMeters, ageInYears, out float topHeightInMeters);
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
