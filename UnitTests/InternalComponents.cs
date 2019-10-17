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
                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                float OG = 0.0F; // (DOUG?)
                TreeCompetition treeCompetitionStartOfStep = new TreeCompetition();
                TreeCompetition treeCompetitionEndOfStep = new TreeCompetition();
                TestStand stand = this.CreateDefaultStand(variant);
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    Stats.SSTATS(variant, stand, stand.Float, out float standBasalAreaStart, 
                                 out float treesPerAcreStart, out float standCompetitionStart, treeCompetitionStartOfStep.SmallTreeBasalAreaLarger, 
                                 treeCompetitionStartOfStep.LargeTreeBasalAreaLarger, treeCompetitionStartOfStep.SmallTreeCrownCompetition, treeCompetitionStartOfStep.LargeTreeCrownCompetition);
                    Stats.SSTATS(variant, stand, stand.Float, out float standBasalAreaEnd,
                                 out float treesPerAcreEnd, out float standCompetitionEnd, treeCompetitionEndOfStep.SmallTreeBasalAreaLarger, 
                                 treeCompetitionEndOfStep.LargeTreeBasalAreaLarger, treeCompetitionEndOfStep.SmallTreeCrownCompetition, treeCompetitionEndOfStep.LargeTreeCrownCompetition);

                    CrownGrowth.CRNCLO(0, 0.0F, variant, stand, stand.Float, stand.ShadowCrownRatio, stand.MGExpansionFactor, CCH, out float crownClosure);
                    Assert.IsTrue(crownClosure >= 0.0F);
                    Assert.IsTrue(crownClosure <= TestConstant.Maximum.CrownClosure);

                    for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
                    {
                        float crownRatio = stand.Float[treeIndex, (int)TreePropertyFloat.CrownRatio];
                        float dbhInInches = stand.Float[treeIndex, (int)TreePropertyFloat.Dbh];
                        float heightInFeet = stand.Float[treeIndex, (int)TreePropertyFloat.Height];
                        float shadowCrownRatio = stand.ShadowCrownRatio[treeIndex, 0];
                        int speciesGroup = stand.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup];

                        CrownGrowth.GET_CCFL(dbhInInches, treeCompetitionStartOfStep.LargeTreeCrownCompetition, treeCompetitionStartOfStep.SmallTreeCrownCompetition, out float crownCompetitionFactor);
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
                                CrownGrowth.HLCW_NWO(speciesGroup, heightInFeet, crownRatio, shadowCrownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_NWO(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_NWO(speciesGroup, maximumCrownWidthInFeet, crownRatio, shadowCrownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Rap:
                                CrownGrowth.HCB_RAP(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, stand.PrimarySiteIndex, stand.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_RAP(speciesGroup, heightInFeet, crownRatio, shadowCrownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_RAP(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_RAP(speciesGroup, maximumCrownWidthInFeet, crownRatio, shadowCrownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Smc:
                                CrownGrowth.HCB_SMC(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, stand.PrimarySiteIndex, stand.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_SMC(speciesGroup, heightInFeet, crownRatio, shadowCrownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_SMC(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_SMC(speciesGroup, maximumCrownWidthInFeet, crownRatio, shadowCrownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Swo:
                                CrownGrowth.HCB_SWO(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, stand.PrimarySiteIndex, stand.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_SWO(speciesGroup, heightInFeet, crownRatio, shadowCrownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_SWO(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_SWO(speciesGroup, maximumCrownWidthInFeet, crownRatio, shadowCrownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
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

                        float expansionFactor = stand.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor];
                        CrownGrowth.CALC_CC(variant, speciesGroup, heightToLargestCrownWidthInFeet, largestCrownWidthInFeet, heightInFeet, 
                                            dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        for (int cchIndex = 0; cchIndex < CCH.Length; ++cchIndex)
                        {
                            float cch = CCH[cchIndex];
                            Assert.IsTrue(cch >= 0.0F);
                            Assert.IsTrue(cch < 1000.0F);
                        }
                    }

                    CrownGrowth.CrowGro(variant, simulationStep, stand, stand.Float, stand.ShadowCrownRatio, stand.Growth, stand.MGExpansionFactor, 
                                        stand.DeadExpansionFactor, treeCompetitionStartOfStep.LargeTreeCrownCompetition, treeCompetitionStartOfStep.SmallTreeCrownCompetition, 
                                        treeCompetitionEndOfStep.LargeTreeCrownCompetition, treeCompetitionEndOfStep.SmallTreeCrownCompetition, standBasalAreaStart, standBasalAreaEnd,
                                        stand.PrimarySiteIndex, stand.MortalitySiteIndex, CALIB, CCH);
                    this.Verify(stand, variantCapabilities);
                }
            }
        }

        [TestMethod]
        public void DiameterGrowthApi()
        {
            foreach (Variant variant in TestConstant.Variants)
            {
                TreeCompetition treeCompetition = new TreeCompetition();
                TestStand stand = this.CreateDefaultStand(variant);
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);

                float BABT = 0.0F; // (DOUG?)
                float[] BART = new float[5]; // (DOUG?)
                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                float[] YF = new float[5]; // (DOUG?)
                float[] YT = new float[5]; // (DOUG?)

                float[] previousTreeDiameters = new float[stand.TreeRecordsInUse];
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    Stats.SSTATS(variant, stand, stand.Float, out float standBasalArea, out float treesPerAcre, 
                                 out float standCompetition, treeCompetition.SmallTreeBasalAreaLarger, treeCompetition.LargeTreeBasalAreaLarger, treeCompetition.SmallTreeCrownCompetition, 
                                 treeCompetition.LargeTreeCrownCompetition);

                    for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
                    {
                        DiameterGrowth.DIAMGRO(variant, treeIndex, simulationStep, stand.Integer, stand.Float, stand.PrimarySiteIndex, 
                                               stand.MortalitySiteIndex, standBasalArea, treeCompetition.LargeTreeBasalAreaLarger, treeCompetition.SmallTreeBasalAreaLarger, CALIB, 
                                               PN, YF, BABT, BART, YT, stand.Growth);
                        float dbhInInches = stand.Growth[treeIndex, (int)TreePropertyFloat.Dbh];
                        float previousDbhInInches = previousTreeDiameters[treeIndex];
                        Assert.IsTrue(dbhInInches >= previousDbhInInches);
                        Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DbhInInches);

                        previousDbhInInches = dbhInInches;
                    }

                    this.Verify(stand, variantCapabilities);
                }
            }

            TreeCompetition competitionForLookup = new TreeCompetition();
            for (float dbhInInches = 0.5F; dbhInInches <= 101.0F; ++dbhInInches)
            {
                DiameterGrowth.GET_BAL(dbhInInches, competitionForLookup.LargeTreeBasalAreaLarger, competitionForLookup.SmallTreeBasalAreaLarger, out float BAL);
                Assert.IsTrue(BAL >= 0.0F);
                Assert.IsTrue(BAL <= 1.0F);
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
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                TreeCompetition treeCompetitionStartOfStep = new TreeCompetition();
                TreeCompetition treeCompetitionEndOfStep = new TreeCompetition();
                TestStand stand = this.CreateDefaultStand(variant);
                

                int bigSixSpeciesTreeRecords = stand.GetBigSixSpeciesRecordCount();
                int otherSpeciesTreeRecords = stand.TreeRecordsInUse - bigSixSpeciesTreeRecords;

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; /* incremented by GROW() */)
                {
                    Stats.SSTATS(variant, stand, stand.Float, out float standBasalAreaStart,
                                 out float treesPerAcreStart, out float standCompetitionStart, treeCompetitionStartOfStep.SmallTreeBasalAreaLarger,
                                 treeCompetitionStartOfStep.LargeTreeBasalAreaLarger, treeCompetitionStartOfStep.SmallTreeCrownCompetition, treeCompetitionStartOfStep.LargeTreeCrownCompetition);
                    Stats.SSTATS(variant, stand, stand.Float, out float standBasalAreaEnd,
                                 out float treesPerAcreEnd, out float standCompetitionEnd, treeCompetitionEndOfStep.SmallTreeBasalAreaLarger,
                                 treeCompetitionEndOfStep.LargeTreeBasalAreaLarger, treeCompetitionEndOfStep.SmallTreeCrownCompetition, treeCompetitionEndOfStep.LargeTreeCrownCompetition);

                    // (DOUG? POST?)
                    bool POST = false;
                    TreeGrowth.GROW(ref simulationStep, configuration, stand, stand.Float, stand.DeadExpansionFactor, POST, 
                                    variantCapabilities.SpeciesGroupCount, ref thinningCycle, ref fertlizerCycle,
                                    stand.PrimarySiteIndex, stand.MortalitySiteIndex,
                                    standBasalAreaStart, treeCompetitionStartOfStep.LargeTreeBasalAreaLarger, treeCompetitionStartOfStep.SmallTreeBasalAreaLarger, CALIB, PN, YF, 
                                    BABT, BART, YT, stand.Growth, stand.ShadowCrownRatio, 
                                    CCH, ref OLD, TestConstant.Default.RAAGE, TestConstant.Default.RedAlderSiteIndex,
                                    treeCompetitionStartOfStep.LargeTreeCrownCompetition, treeCompetitionStartOfStep.SmallTreeCrownCompetition, 
                                    treeCompetitionEndOfStep.LargeTreeCrownCompetition, treeCompetitionEndOfStep.SmallTreeCrownCompetition, treeCompetitionEndOfStep.LargeTreeBasalAreaLarger, 
                                    treeCompetitionEndOfStep.SmallTreeBasalAreaLarger);
                    this.Verify(stand, variantCapabilities);
                }
            }
        }

        [TestMethod]
        public void GrowthModifiersApi()
        {
            this.TestContext.WriteLine("tree age, diameter genetic factor, height genetic factor, diameter growth modifier, height growth modifier");
            for (float treeAgeInYears = 0.0F; treeAgeInYears <= 50.0F; treeAgeInYears += 5.0F)
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
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                TestStand stand = this.CreateDefaultStand(variant);

                for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
                {
                    // predicted heights
                    int speciesGroup = stand.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup];
                    float dbhInInches = stand.Float[treeIndex, (int)TreePropertyFloat.Dbh];
                    float heightInFeet = stand.Float[treeIndex, (int)TreePropertyFloat.Height];
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
                    FiaCode species = (FiaCode)stand.Integer[treeIndex, (int)TreePropertyInteger.Species];
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
                            HeightGrowth.B_HG(stand.PrimarySiteIndex, heightInFeet, TestConstant.Default.AgeToReachBreastHeightInYears, out growthEffectiveAgeInYears, out potentialHeightGrowth);
                        }
                        verifyAgeAndHeight = true;
                    }
                    else if (variant == Variant.Swo)
                    {
                        if ((species == FiaCode.PinusPonderosa) || (species == FiaCode.PseudotsugaMenziesii))
                        {
                            HeightGrowth.HS_HG(species == FiaCode.PseudotsugaMenziesii ? 1 : 0, stand.PrimarySiteIndex, heightInFeet, out growthEffectiveAgeInYears, out potentialHeightGrowth);
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

                float[] previousTreeHeights = new float[stand.TreeRecordsInUse];
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
                    {
                        if (stand.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup] <= stand.MaxBigSixSpeciesGroupIndex)
                        {
                            HeightGrowth.HTGRO1(treeIndex, variant, simulationStep, stand,
                                                stand.Float, stand.PrimarySiteIndex, stand.MortalitySiteIndex, CCH, PN, YF, 
                                                TestConstant.Default.BABT, BART, YT, ref OLD, TestConstant.Default.PDEN, stand.Growth);
                        }
                        else
                        {
                            HeightGrowth.HTGRO2(treeIndex, variant, stand, stand.Float,
                                                TestConstant.Default.RedAlderSiteIndex, CALIB, stand.Growth);
                        }

                        float heightInFeet = stand.Float[treeIndex, (int)TreePropertyFloat.Height];
                        float previousHeightInFeet = previousTreeHeights[treeIndex];
                        // TODO: make upper limit of height species specific
                        Assert.IsTrue(heightInFeet < TestConstant.Maximum.HeightInFeet);

                        previousTreeHeights[treeIndex] = heightInFeet;
                    }

                    this.Verify(stand, variantCapabilities);
                }
            }

            // no test coverage: one line functions
            // HeightGrowth.WHHLB_GEA();
            // HeightGrowth.WHHLB_H40();
            // HeightGrowth.WHHLB_SI_UC();
            // HeightGrowth.RAGEA();
            // HeightGrowth.RAH40();
        }

        [TestMethod]
        public void MortalityApi()
        {
            TreeCompetition competition = new TreeCompetition();
            float[] PN = new float[5]; // (DOUG?)
            float[] YF = new float[5]; // (DOUG?)
            
            foreach (Variant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);

                TestStand stand = this.CreateDefaultStand(variant);
                float RAAGE = TestConstant.Default.RAAGE;
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    // TODO: MORT = false case for additional mortality disabled
                    // TODO: POST = true case for cycle after thinning
                    Mortality.MORTAL(configuration, simulationStep, stand, false,  
                                     stand.Float, stand.ShadowCrownRatio, stand.Growth, stand.MGExpansionFactor, 
                                     stand.DeadExpansionFactor, competition.LargeTreeBasalAreaLarger, competition.SmallTreeBasalAreaLarger, stand.PrimarySiteIndex, 
                                     stand.MortalitySiteIndex, PN, YF, ref RAAGE);
                    this.Verify(stand, variantCapabilities);

                    // TODO: xind -1.0 case
                    float xind = 0.0F;
                    Mortality.OldGro(stand, stand.Float, stand.Growth, stand.DeadExpansionFactor, xind, out float OG);
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

            TreeCompetition competition = new TreeCompetition();
            foreach (Variant variant in TestConstant.Variants)
            {
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                TestStand stand = this.CreateDefaultStand(variant);

                Stats.SSTATS(variant, stand, stand.Float, out float basalAreaPerAcre, out float treesPerAcre, out float SCCF, 
                             competition.SmallTreeBasalAreaLarger, competition.LargeTreeBasalAreaLarger, competition.SmallTreeCrownCompetition, competition.LargeTreeCrownCompetition);

                this.TestContext.WriteLine("{0},{1} ft²/ac,{2} trees per acre,{3} crown competition factor", variant, basalAreaPerAcre, treesPerAcre, SCCF);
                this.TestContext.WriteLine("index,BALL,CCFLL");
                for (int largeTreeCompetitionIndex = 0; largeTreeCompetitionIndex < competition.LargeTreeBasalAreaLarger.Length; ++largeTreeCompetitionIndex)
                {
                    float largeTreeBasalAreaLarger = competition.LargeTreeBasalAreaLarger[largeTreeCompetitionIndex];
                    float largeTreeCrownCompetitionFactor = competition.LargeTreeCrownCompetition[largeTreeCompetitionIndex];
                    Assert.IsTrue(largeTreeBasalAreaLarger >= 0.0F);
                    Assert.IsTrue(largeTreeBasalAreaLarger < TestConstant.Maximum.TreeBasalAreaLarger);
                    Assert.IsTrue(largeTreeCrownCompetitionFactor >= 0.0F);
                    Assert.IsTrue(largeTreeCrownCompetitionFactor < TestConstant.Maximum.StandCrownCompetitionFactor);
                    this.TestContext.WriteLine("{0},{1}", largeTreeBasalAreaLarger, largeTreeCrownCompetitionFactor);
                }
                this.TestContext.WriteLine("index,BAL,CCFL");
                for (int smallTreeCompetitionIndex = 0; smallTreeCompetitionIndex < competition.SmallTreeBasalAreaLarger.Length; ++smallTreeCompetitionIndex)
                {
                    float smallTreeBasalAreaLarger = competition.SmallTreeBasalAreaLarger[smallTreeCompetitionIndex];
                    float smallTreeCrownCompetitionFactor = competition.SmallTreeCrownCompetition[smallTreeCompetitionIndex];
                    Assert.IsTrue(smallTreeBasalAreaLarger >= 0.0F);
                    Assert.IsTrue(smallTreeBasalAreaLarger < TestConstant.Maximum.TreeBasalAreaLarger);
                    Assert.IsTrue(smallTreeCrownCompetitionFactor >= 0.0F);
                    Assert.IsTrue(smallTreeCrownCompetitionFactor < TestConstant.Maximum.StandCrownCompetitionFactor);
                    this.TestContext.WriteLine("{0},{1}", smallTreeBasalAreaLarger, smallTreeCrownCompetitionFactor);
                }
                this.TestContext.WriteLine(String.Empty);

                this.Verify(stand, variantCapabilities);
            }
        }

        [TestMethod]
        public void SubmaxApi()
        {
            this.TestContext.WriteLine("version, A1, A2");
            foreach (Variant variant in TestConstant.Variants)
            {
                OrganonConfiguration configuration = this.CreateOrganonConfiguration(variant);
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                TestStand stand = this.CreateDefaultStand(variant);

                Submax.SUBMAX(false, configuration, stand, stand.Float);
                this.TestContext.WriteLine("{0},{1},{2}", variant, configuration.A1, configuration.A2);

                Assert.IsTrue(configuration.A1 < 7.0F);
                Assert.IsTrue(configuration.A1 > 5.0F);
                Assert.IsTrue(configuration.A2 > 0.60F);
                Assert.IsTrue(configuration.A2 < 0.65F);
                this.Verify(stand, variantCapabilities);
            }
        }

        [TestMethod]
        public void WesternHemlockHeightApi()
        {
            this.TestContext.WriteLine("siteIndex, age, topHeight");
            for (float siteIndexInMeters = 10.0F; siteIndexInMeters < 60.1F; siteIndexInMeters += 10.0F)
            {
                float previousTopHeight = -1.0F;
                for (float ageInYears = 0.0F; ageInYears < 100.1F; ++ageInYears)
                {
                    WesternHemlockHeight.SITECV_F(siteIndexInMeters, ageInYears, out float topHeightInMeters);
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
