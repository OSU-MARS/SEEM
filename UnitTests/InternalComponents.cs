using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Osu.Cof.Organon.Test
{
    [TestClass]
    public class Component : OrganonTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CrownGrowthApi()
        {
            foreach (Variant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);

                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                float OG = 0.0F; // (DOUG?)
                VariantCapabilities variantCapabilities = new VariantCapabilities(variant);

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    Stats.SSTATS(variant, stand, out float standBasalAreaStart, 
                                 out float treesPerAcreStart, out float standCompetitionStart, out StandDensity densityStartOfStep);
                    Assert.IsTrue(standBasalAreaStart > 0.0F);
                    Assert.IsTrue(standCompetitionStart > 0.0F);
                    Assert.IsTrue(treesPerAcreStart > 0.0F);

                    CrownGrowth.CRNCLO(variant, stand, CCH, out float crownClosure);
                    Assert.IsTrue(crownClosure >= 0.0F);
                    Assert.IsTrue(crownClosure <= TestConstant.Maximum.CrownClosure);

                    for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                    {
                        float crownRatio = stand.CrownRatio[treeIndex];
                        float dbhInInches = stand.Dbh[treeIndex];
                        float heightInFeet = stand.Height[treeIndex];
                        int speciesGroup = stand.SpeciesGroup[treeIndex];

                        float crownCompetitionFactor = densityStartOfStep.GET_CCFL(dbhInInches);
                        Assert.IsTrue(crownCompetitionFactor >= 0.0F);
                        Assert.IsTrue(crownCompetitionFactor < TestConstant.Maximum.StandCrownCompetitionFactor);

                        float heightToCrownBaseInFeet;
                        float heightToLargestCrownWidthInFeet;
                        float largestCrownWidthInFeet;
                        float maximumCrownWidthInFeet;
                        switch (variant)
                        {
                            case Variant.Nwo:
                                CrownGrowth.HCB_NWO(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, stand.PrimarySiteIndex, stand.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_NWO(speciesGroup, heightInFeet, crownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_NWO(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_NWO(speciesGroup, maximumCrownWidthInFeet, crownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Rap:
                                CrownGrowth.HCB_RAP(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, stand.PrimarySiteIndex, stand.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_RAP(speciesGroup, heightInFeet, crownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_RAP(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_RAP(speciesGroup, maximumCrownWidthInFeet, crownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Smc:
                                CrownGrowth.HCB_SMC(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, stand.PrimarySiteIndex, stand.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_SMC(speciesGroup, heightInFeet, crownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_SMC(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_SMC(speciesGroup, maximumCrownWidthInFeet, crownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Swo:
                                CrownGrowth.HCB_SWO(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, stand.PrimarySiteIndex, stand.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_SWO(speciesGroup, heightInFeet, crownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_SWO(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_SWO(speciesGroup, maximumCrownWidthInFeet, crownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            default:
                                throw VariantExtensions.CreateUnhandledVariantException(variant);
                        }
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
                        CrownGrowth.CALC_CC(variant, speciesGroup, heightToLargestCrownWidthInFeet, largestCrownWidthInFeet, heightInFeet, 
                                            dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        for (int cchIndex = 0; cchIndex < CCH.Length; ++cchIndex)
                        {
                            float cch = CCH[cchIndex];
                            Assert.IsTrue(cch >= 0.0F);
                            Assert.IsTrue(cch < 1000.0F);
                        }
                    }

                    Stats.SSTATS(variant, stand, out float standBasalAreaEnd,
                                 out float treesPerAcreEnd, out float standCompetitionEnd, out StandDensity densityEndOfStep);
                    Assert.IsTrue(standBasalAreaEnd > 0.0F);
                    Assert.IsTrue(standCompetitionEnd > 0.0F);
                    Assert.IsTrue(treesPerAcreEnd > 0.0F);

                    CrownGrowth.CrowGro(variant, stand, densityStartOfStep, densityEndOfStep,
                                        standBasalAreaStart, standBasalAreaEnd, stand.PrimarySiteIndex, stand.MortalitySiteIndex, CALIB, CCH);
                    this.Verify(stand, ExpectedTreeChanges.NoDiameterOrHeightGrowth, variantCapabilities);
                    this.Verify(CALIB);
                }
            }
        }

        [TestMethod]
        public void DiameterGrowthApi()
        {
            foreach (Variant variant in TestConstant.Variants)
            {
                VariantCapabilities variantCapabilities = new VariantCapabilities(variant);
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);

                float BABT = 0.0F; // (DOUG?)
                float[] BART = new float[5]; // (DOUG?)
                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                float[] YF = new float[5]; // (DOUG?)
                float[] YT = new float[5]; // (DOUG?)

                float[] previousTreeDiameters = new float[stand.TreeRecordCount];
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    Stats.SSTATS(variant, stand, out float standBasalArea, out float treesPerAcre, 
                                 out float standCompetition, out StandDensity treeCompetition);

                    for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                    {
                        DiameterGrowth.DIAMGRO(variant, treeIndex, simulationStep, stand, stand.PrimarySiteIndex, 
                                               stand.MortalitySiteIndex, standBasalArea, treeCompetition, CALIB, 
                                               PN, YF, BABT, BART, YT);
                        float dbhInInches = stand.Dbh[treeIndex];
                        float previousDbhInInches = previousTreeDiameters[treeIndex];
                        Assert.IsTrue(dbhInInches >= previousDbhInInches);
                        Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DbhInInches);

                        previousDbhInInches = dbhInInches;
                    }

                    this.Verify(stand, ExpectedTreeChanges.DiameterGrowth, variantCapabilities);
                    this.Verify(CALIB);
                }
            }

            StandDensity densityForLookup = new StandDensity();
            for (float dbhInInches = 0.5F; dbhInInches <= 101.0F; ++dbhInInches)
            {
                float basalAreaLarger = densityForLookup.GET_BAL(dbhInInches);
                Assert.IsTrue(basalAreaLarger >= 0.0F);
                Assert.IsTrue(basalAreaLarger <= 1.0F);
                float crownCompetitionLarger = densityForLookup.GET_CCFL(dbhInInches);
                Assert.IsTrue(crownCompetitionLarger >= 0.0F);
                Assert.IsTrue(crownCompetitionLarger <= 1.0F);
            }
        }

        [TestMethod]
        public void GrowApi()
        {
            foreach (Variant variant in TestConstant.Variants)
            {
                float BABT = 0.0F; // (DOUG?)
                float[] BART = new float[5]; // (DOUG?)
                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                int fertlizerCycle = 0;
                float OLD = 0.0F; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                int thinningCycle = 0;
                float[] YF = new float[5]; // (DOUG?)
                float[] YT = new float[5]; // (DOUG?)

                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                VariantCapabilities variantCapabilities = new VariantCapabilities(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                

                int bigSixSpeciesTreeRecords = stand.GetBigSixSpeciesRecordCount();
                int otherSpeciesTreeRecords = stand.TreeRecordCount - bigSixSpeciesTreeRecords;

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; /* incremented by GROW() */)
                {
                    Stats.SSTATS(variant, stand, out float standBasalAreaStart,
                                 out float treesPerAcreStart, out float standCompetitionStart, out StandDensity densityStartOfStep);
                    Stats.SSTATS(variant, stand, out float standBasalAreaEnd,
                                 out float treesPerAcreEnd, out float standCompetitionEnd, out StandDensity densityEndOfStep);

                    TreeGrowth.GROW(ref simulationStep, configuration, stand,
                                    variantCapabilities.SpeciesGroupCount, ref thinningCycle, ref fertlizerCycle,
                                    standBasalAreaStart, densityStartOfStep, CALIB, PN, YF, 
                                    BABT, BART, YT, CCH, ref OLD, TestConstant.Default.RAAGE, out densityEndOfStep);
                    this.Verify(stand, ExpectedTreeChanges.DiameterGrowth | ExpectedTreeChanges.HeightGrowth, variantCapabilities);
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
                        GrowthModifiers.GG_MODS(treeAgeInYears, diameterGeneticFactor, heightGeneticFactor, out float diameterGrowthModifier, out float heightGrowthModifier);
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
                GrowthModifiers.SNC_MODS(FR, out float diameterGrowthModifier, out float heightGrowthModifier);
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
            float[,] CALIB = this.CreateCalibrationArray();
            float[] CCH = new float[41]; // (DOUG? why is this different from the other competition arrays?)
            float OLD = 0.0F; // (DOUG?)
            float[] PN = new float[5]; // (DOUG?)
            float[] YF = new float[5]; // (DOUG?)
            float[] YT = new float[5]; // (DOUG?)
            foreach (Variant variant in TestConstant.Variants)
            {
                VariantCapabilities variantCapabilities = new VariantCapabilities(variant);
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);

                for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                {
                    // predicted heights
                    int speciesGroup = stand.SpeciesGroup[treeIndex];
                    float dbhInInches = stand.Dbh[treeIndex];
                    float heightInFeet = stand.Height[treeIndex];
                    float predictedHeightInFeet;
                    switch (variant)
                    {
                        case Variant.Nwo:
                            HeightGrowth.HD_NWO(speciesGroup, dbhInInches, out predictedHeightInFeet);
                            break;
                        case Variant.Rap:
                            HeightGrowth.HD_RAP(speciesGroup, dbhInInches, out predictedHeightInFeet);
                            break;
                        case Variant.Smc:
                            HeightGrowth.HD_SMC(speciesGroup, dbhInInches, out predictedHeightInFeet);
                            break;
                        case Variant.Swo:
                            HeightGrowth.HD_SWO(speciesGroup, dbhInInches, out predictedHeightInFeet);
                            break;
                        default:
                            throw VariantExtensions.CreateUnhandledVariantException(variant);
                    }
                    Assert.IsTrue(predictedHeightInFeet >= 0.0F);
                    // TODO: make upper limit of height species specific
                    Assert.IsTrue(predictedHeightInFeet < TestConstant.Maximum.HeightInFeet);

                    // growth effective age and potential height growth
                    FiaCode species = stand.Species[treeIndex];
                    bool verifyAgeAndHeight = false;
                    float growthEffectiveAgeInYears = -1.0F;
                    float potentialHeightGrowth = -1.0F;
                    if ((variant == Variant.Nwo) || (variant == Variant.Smc))
                    {
                        if (species == FiaCode.TsugaHeterophylla)
                        {
                            HeightGrowth.F_HG(stand.PrimarySiteIndex, heightInFeet, TestConstant.Default.AgeToReachBreastHeightInYears, out growthEffectiveAgeInYears, out potentialHeightGrowth);
                        }
                        else
                        {
                            HeightGrowth.BrucePsmeAbgrGrowthEffectiveAge(stand.PrimarySiteIndex, heightInFeet, TestConstant.Default.AgeToReachBreastHeightInYears, out growthEffectiveAgeInYears, out potentialHeightGrowth);
                        }
                        verifyAgeAndHeight = true;
                    }
                    else if (variant == Variant.Swo)
                    {
                        if ((species == FiaCode.PinusPonderosa) || (species == FiaCode.PseudotsugaMenziesii))
                        {
                            HeightGrowth.HS_HG(species == FiaCode.PseudotsugaMenziesii, stand.PrimarySiteIndex, heightInFeet, out growthEffectiveAgeInYears, out potentialHeightGrowth);
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
                        if (stand.IsBigSixSpecies(treeIndex))
                        {
                            HeightGrowth.GrowBigSixSpecies(treeIndex, variant, simulationStep, stand, stand.PrimarySiteIndex, stand.MortalitySiteIndex, 
                                                CCH, PN, YF, TestConstant.Default.BABT, BART, YT, ref OLD, TestConstant.Default.PDEN);
                        }
                        else
                        {
                            HeightGrowth.GrowMinorSpecies(treeIndex, variant, stand, CALIB);
                        }

                        float heightInFeet = stand.Height[treeIndex];
                        float previousHeightInFeet = previousTreeHeights[treeIndex];
                        // TODO: make upper limit of height species specific
                        Assert.IsTrue(heightInFeet < TestConstant.Maximum.HeightInFeet);

                        previousTreeHeights[treeIndex] = heightInFeet;
                    }

                    // since diameter growth is zero in this test any tree which is above its anticipated height for its current diameter 
                    // should have zero growth
                    // This is expected behavior the height growth functions and, potentially, height growth limiting.
                    this.Verify(stand, ExpectedTreeChanges.HeightGrowthOrNoChange, variantCapabilities);
                    this.Verify(CALIB);
                }
            }
        }

        [TestMethod]
        public void MortalityApi()
        {
            StandDensity density = new StandDensity();
            float[] PN = new float[5]; // (DOUG?)
            float[] YF = new float[5]; // (DOUG?)
            
            foreach (Variant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                VariantCapabilities variantCapabilities = new VariantCapabilities(variant);

                TestStand stand = this.CreateDefaultStand(configuration);
                float RAAGE = TestConstant.Default.RAAGE;
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    // TODO: MORT = false case for additional mortality disabled
                    // TODO: POST = true case for cycle after thinning
                    Mortality.MORTAL(configuration, simulationStep, stand, density, stand.PrimarySiteIndex, 
                                     stand.MortalitySiteIndex, PN, YF, ref RAAGE);
                    this.Verify(stand, ExpectedTreeChanges.NoDiameterOrHeightGrowth, variantCapabilities);

                    // TODO: xind -1.0 case
                    float xind = 0.0F;
                    Mortality.OldGro(stand, xind, out float OG);
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

            foreach (Variant variant in TestConstant.Variants)
            {
                VariantCapabilities variantCapabilities = new VariantCapabilities(variant);
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                TestStand stand = this.CreateDefaultStand(configuration);

                Stats.SSTATS(variant, stand, out float basalAreaPerAcre, out float treesPerAcre, out float SCCF, out StandDensity density);

                this.TestContext.WriteLine("{0},{1} ft²/ac,{2} trees per acre,{3} crown competition factor", variant, basalAreaPerAcre, treesPerAcre, SCCF);
                this.TestContext.WriteLine("index,large tree BA larger,large tree CCF larger");
                for (int largeTreeCompetitionIndex = 0; largeTreeCompetitionIndex < density.LargeTreeBasalAreaLarger.Length; ++largeTreeCompetitionIndex)
                {
                    float largeTreeBasalAreaLarger = density.LargeTreeBasalAreaLarger[largeTreeCompetitionIndex];
                    float largeTreeCrownCompetitionFactor = density.LargeTreeCrownCompetition[largeTreeCompetitionIndex];
                    Assert.IsTrue(largeTreeBasalAreaLarger >= 0.0F);
                    Assert.IsTrue(largeTreeBasalAreaLarger < TestConstant.Maximum.TreeBasalAreaLarger);
                    Assert.IsTrue(largeTreeCrownCompetitionFactor >= 0.0F);
                    Assert.IsTrue(largeTreeCrownCompetitionFactor < TestConstant.Maximum.StandCrownCompetitionFactor);
                    this.TestContext.WriteLine("{0},{1}", largeTreeBasalAreaLarger, largeTreeCrownCompetitionFactor);
                }
                this.TestContext.WriteLine("index,small tree BA larger,large tree CCF larger");
                for (int smallTreeCompetitionIndex = 0; smallTreeCompetitionIndex < density.SmallTreeBasalAreaLarger.Length; ++smallTreeCompetitionIndex)
                {
                    float smallTreeBasalAreaLarger = density.SmallTreeBasalAreaLarger[smallTreeCompetitionIndex];
                    float smallTreeCrownCompetitionFactor = density.SmallTreeCrownCompetition[smallTreeCompetitionIndex];
                    Assert.IsTrue(smallTreeBasalAreaLarger >= 0.0F);
                    Assert.IsTrue(smallTreeBasalAreaLarger < TestConstant.Maximum.TreeBasalAreaLarger);
                    Assert.IsTrue(smallTreeCrownCompetitionFactor >= 0.0F);
                    Assert.IsTrue(smallTreeCrownCompetitionFactor < TestConstant.Maximum.StandCrownCompetitionFactor);
                    this.TestContext.WriteLine("{0},{1}", smallTreeBasalAreaLarger, smallTreeCrownCompetitionFactor);
                }
                this.TestContext.WriteLine(String.Empty);

                this.Verify(stand, ExpectedTreeChanges.NoDiameterOrHeightGrowth, variantCapabilities);
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
            foreach (Variant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                VariantCapabilities variantCapabilities = new VariantCapabilities(variant);
                TestStand stand = this.CreateDefaultStand(configuration);
                this.TestContext.WriteLine("{0},{1},{2}", variant, stand.A1, stand.A2);

                Assert.IsTrue(stand.A1 < 7.0F);
                Assert.IsTrue(stand.A1 > 5.0F);
                Assert.IsTrue(stand.A2 > 0.60F);
                Assert.IsTrue(stand.A2 < 0.65F);
                this.Verify(stand, ExpectedTreeChanges.NoDiameterOrHeightGrowth, variantCapabilities);
            }
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
