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
                TreeData treeData = this.CreateDefaultTrees(variant);
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    Stats.SSTATS(variant, treeData.UsedRecordCount, treeData.Integer, treeData.Float, out float standBasalAreaStart, 
                                 out float treesPerAcreStart, out float standCompetitionStart, treeCompetitionStartOfStep.SmallTreeBasalAreaLarger, 
                                 treeCompetitionStartOfStep.LargeTreeBasalAreaLarger, treeCompetitionStartOfStep.SmallTreeCrownCompetition, treeCompetitionStartOfStep.LargeTreeCrownCompetition);
                    Stats.SSTATS(variant, treeData.UsedRecordCount, treeData.Integer, treeData.Float, out float standBasalAreaEnd,
                                 out float treesPerAcreEnd, out float standCompetitionEnd, treeCompetitionEndOfStep.SmallTreeBasalAreaLarger, 
                                 treeCompetitionEndOfStep.LargeTreeBasalAreaLarger, treeCompetitionEndOfStep.SmallTreeCrownCompetition, treeCompetitionEndOfStep.LargeTreeCrownCompetition);

                    CrownGrowth.CRNCLO(0, 0.0F, variant, treeData.UsedRecordCount, treeData.Integer, treeData.Float, treeData.ShadowCrownRatio, treeData.MGExpansionFactor, CCH, out float crownClosure);
                    Assert.IsTrue(crownClosure >= 0.0F);
                    Assert.IsTrue(crownClosure <= TestConstant.Maximum.CrownClosure);

                    for (int treeIndex = 0; treeIndex < treeData.UsedRecordCount; ++treeIndex)
                    {
                        float crownRatio = treeData.Float[treeIndex, (int)TreePropertyFloat.CrownRatio];
                        float dbhInInches = treeData.Float[treeIndex, (int)TreePropertyFloat.Dbh];
                        float heightInFeet = treeData.Float[treeIndex, (int)TreePropertyFloat.Height];
                        float shadowCrownRatio = treeData.ShadowCrownRatio[treeIndex, 0];
                        int speciesGroup = treeData.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup];

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
                                CrownGrowth.HCB_NWO(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, treeData.PrimarySiteIndex, treeData.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_NWO(speciesGroup, heightInFeet, crownRatio, shadowCrownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_NWO(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_NWO(speciesGroup, maximumCrownWidthInFeet, crownRatio, shadowCrownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Rap:
                                CrownGrowth.HCB_RAP(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, treeData.PrimarySiteIndex, treeData.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_RAP(speciesGroup, heightInFeet, crownRatio, shadowCrownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_RAP(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_RAP(speciesGroup, maximumCrownWidthInFeet, crownRatio, shadowCrownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Smc:
                                CrownGrowth.HCB_SMC(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, treeData.PrimarySiteIndex, treeData.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
                                CrownGrowth.HLCW_SMC(speciesGroup, heightInFeet, crownRatio, shadowCrownRatio, out heightToLargestCrownWidthInFeet);
                                CrownGrowth.MCW_SMC(speciesGroup, dbhInInches, heightInFeet, out maximumCrownWidthInFeet);
                                CrownGrowth.LCW_SMC(speciesGroup, maximumCrownWidthInFeet, crownRatio, shadowCrownRatio, dbhInInches, heightInFeet, out largestCrownWidthInFeet);
                                break;
                            case Variant.Swo:
                                CrownGrowth.HCB_SWO(speciesGroup, heightInFeet, dbhInInches, crownCompetitionFactor, standBasalAreaStart, treeData.PrimarySiteIndex, treeData.MortalitySiteIndex, OG, out heightToCrownBaseInFeet);
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

                        float expansionFactor = treeData.Float[treeIndex, (int)TreePropertyFloat.ExpansionFactor];
                        CrownGrowth.CALC_CC(variant, speciesGroup, heightToLargestCrownWidthInFeet, largestCrownWidthInFeet, heightInFeet, 
                                            dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
                        for (int cchIndex = 0; cchIndex < CCH.Length; ++cchIndex)
                        {
                            float cch = CCH[cchIndex];
                            Assert.IsTrue(cch >= 0.0F);
                            Assert.IsTrue(cch < 1000.0F);
                        }
                    }

                    CrownGrowth.CrowGro(variant, simulationStep, treeData.UsedRecordCount, treeData.MaxBigSixSpeciesGroupIndex, treeData.Integer, 
                                        treeData.Float, treeData.ShadowCrownRatio, treeData.Growth, treeData.MGExpansionFactor, 
                                        treeData.DeadExpansionFactor, treeCompetitionStartOfStep.LargeTreeCrownCompetition, treeCompetitionStartOfStep.SmallTreeCrownCompetition, 
                                        treeCompetitionEndOfStep.LargeTreeCrownCompetition, treeCompetitionEndOfStep.SmallTreeCrownCompetition, standBasalAreaStart, standBasalAreaEnd,
                                        treeData.PrimarySiteIndex, treeData.MortalitySiteIndex, CALIB, CCH);
                    this.Verify(treeData, variantCapabilities);
                }
            }
        }

        [TestMethod]
        public void DiameterGrowthApi()
        {
            foreach (Variant variant in TestConstant.Variants)
            {
                TreeCompetition treeCompetition = new TreeCompetition();
                TreeData treeData = this.CreateDefaultTrees(variant);
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);

                float BABT = 0.0F; // (DOUG?)
                float[] BART = new float[5]; // (DOUG?)
                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                float[] YF = new float[5]; // (DOUG?)
                float[] YT = new float[5]; // (DOUG?)

                float[] previousTreeDiameters = new float[treeData.UsedRecordCount];
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    Stats.SSTATS(variant, treeData.UsedRecordCount, treeData.Integer, treeData.Float, out float standBasalArea, out float treesPerAcre, 
                                 out float standCompetition, treeCompetition.SmallTreeBasalAreaLarger, treeCompetition.LargeTreeBasalAreaLarger, treeCompetition.SmallTreeCrownCompetition, 
                                 treeCompetition.LargeTreeCrownCompetition);

                    for (int treeIndex = 0; treeIndex < treeData.UsedRecordCount; ++treeIndex)
                    {
                        DiameterGrowth.DIAMGRO(variant, treeIndex, simulationStep, treeData.Integer, treeData.Float, treeData.PrimarySiteIndex, 
                                               treeData.MortalitySiteIndex, standBasalArea, treeCompetition.LargeTreeBasalAreaLarger, treeCompetition.SmallTreeBasalAreaLarger, CALIB, 
                                               PN, YF, BABT, BART, YT, treeData.Growth);
                        float dbhInInches = treeData.Growth[treeIndex, (int)TreePropertyFloat.Dbh];
                        float previousDbhInInches = previousTreeDiameters[treeIndex];
                        Assert.IsTrue(dbhInInches >= previousDbhInInches);
                        Assert.IsTrue(dbhInInches <= TestConstant.Maximum.DbhInInches);

                        previousDbhInInches = dbhInInches;
                    }

                    this.Verify(treeData, variantCapabilities);
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
                int breastHeightAge = 0; // breast height age of stand, but only generates even aged stand warnings? (DOUG?)
                float[,] CALIB = this.CreateCalibrationArray();
                float[] CCH = new float[41]; // (DOUG?)
                int fertlizerCycle = 0;
                float OLD = 0.0F; // (DOUG?)
                float[] PN = new float[5]; // (DOUG?)
                int standAge = 0; // stand age, but only generates even aged stand warnings? (DOUG?)
                int thinningCycle = 0;
                float[] YF = new float[5]; // (DOUG?)
                float[] YT = new float[5]; // (DOUG?)

                OrganonOptions options = new OrganonOptions(variant);
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                TreeCompetition treeCompetitionStartOfStep = new TreeCompetition();
                TreeCompetition treeCompetitionEndOfStep = new TreeCompetition();
                TreeData treeData = this.CreateDefaultTrees(variant);

                int bigSixSpeciesTreeRecords = treeData.GetBigSixSpeciesRecordCount();
                int otherSpeciesTreeRecords = treeData.UsedRecordCount - bigSixSpeciesTreeRecords;

                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; /* incremented by GROW() */)
                {
                    Stats.SSTATS(variant, treeData.UsedRecordCount, treeData.Integer, treeData.Float, out float standBasalAreaStart,
                                 out float treesPerAcreStart, out float standCompetitionStart, treeCompetitionStartOfStep.SmallTreeBasalAreaLarger,
                                 treeCompetitionStartOfStep.LargeTreeBasalAreaLarger, treeCompetitionStartOfStep.SmallTreeCrownCompetition, treeCompetitionStartOfStep.LargeTreeCrownCompetition);
                    Stats.SSTATS(variant, treeData.UsedRecordCount, treeData.Integer, treeData.Float, out float standBasalAreaEnd,
                                 out float treesPerAcreEnd, out float standCompetitionEnd, treeCompetitionEndOfStep.SmallTreeBasalAreaLarger,
                                 treeCompetitionEndOfStep.LargeTreeBasalAreaLarger, treeCompetitionEndOfStep.SmallTreeCrownCompetition, treeCompetitionEndOfStep.LargeTreeCrownCompetition);

                    // (DOUG? POST = true?)
                    Grow.GROW(variant, ref simulationStep, treeData.UsedRecordCount, treeData.MaxBigSixSpeciesGroupIndex, bigSixSpeciesTreeRecords, 
                              otherSpeciesTreeRecords, variantCapabilities.SpeciesGroupCount, ref standAge, ref breastHeightAge, 
                              null, null, treeData.Integer, null, null, null, null, null, null, ref thinningCycle, ref fertlizerCycle,
                              options.Triple, options.WoodQuality, false, options.AdditionalMortality, options.Genetics, 
                              options.SwissNeedleCast, treeData.Float, treeData.PrimarySiteIndex, treeData.MortalitySiteIndex, 
                              standBasalAreaStart, treeCompetitionStartOfStep.LargeTreeBasalAreaLarger, treeCompetitionStartOfStep.SmallTreeBasalAreaLarger, CALIB, PN, YF, 
                              BABT, BART, YT, treeData.Growth, null, null, null, null, null, treeData.ShadowCrownRatio, null, null, 
                              CCH, ref OLD, treeData.MGExpansionFactor, treeData.DeadExpansionFactor,
                              TestConstant.Default.A1, TestConstant.Default.A2, TestConstant.Default.A1MAX, TestConstant.Default.PA1MAX,
                              TestConstant.Default.NO, TestConstant.Default.RD0, TestConstant.Default.RAAGE, TestConstant.Default.RedAlderSiteIndex,
                              treeCompetitionStartOfStep.LargeTreeCrownCompetition, treeCompetitionStartOfStep.SmallTreeCrownCompetition, 
                              treeCompetitionEndOfStep.LargeTreeCrownCompetition, treeCompetitionEndOfStep.SmallTreeCrownCompetition, treeCompetitionEndOfStep.LargeTreeBasalAreaLarger, 
                              treeCompetitionEndOfStep.SmallTreeBasalAreaLarger, options.GWDG, options.GWHG, options.FR, options.PDEN);
                    this.Verify(treeData, variantCapabilities);
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
                TreeData treeData = this.CreateDefaultTrees(variant);

                for (int treeIndex = 0; treeIndex < treeData.UsedRecordCount; ++treeIndex)
                {
                    // predicted heights
                    int speciesGroup = treeData.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup];
                    float dbhInInches = treeData.Float[treeIndex, (int)TreePropertyFloat.Dbh];
                    float heightInFeet = treeData.Float[treeIndex, (int)TreePropertyFloat.Height];
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
                    FiaCode species = (FiaCode)treeData.Integer[treeIndex, (int)TreePropertyInteger.Species];
                    bool verifyAgeAndHeight = false;
                    float growthEffectiveAgeInYears = -1.0F;
                    float potentialHeightGrowth = -1.0F;
                    if ((variant == Variant.Nwo) || (variant == Variant.Smc))
                    {
                        if (species == FiaCode.TsugaHeterophylla)
                        {
                            HeightGrowth.F_HG(treeData.PrimarySiteIndex, heightInFeet, TestConstant.Default.AgeToReachBreastHeightInYears, out growthEffectiveAgeInYears, out potentialHeightGrowth);
                        }
                        else
                        {
                            HeightGrowth.B_HG(treeData.PrimarySiteIndex, heightInFeet, TestConstant.Default.AgeToReachBreastHeightInYears, out growthEffectiveAgeInYears, out potentialHeightGrowth);
                        }
                        verifyAgeAndHeight = true;
                    }
                    else if (variant == Variant.Swo)
                    {
                        if ((species == FiaCode.PinusPonderosa) || (species == FiaCode.PseudotsugaMenziesii))
                        {
                            HeightGrowth.HS_HG(species == FiaCode.PseudotsugaMenziesii ? 1 : 0, treeData.PrimarySiteIndex, heightInFeet, out growthEffectiveAgeInYears, out potentialHeightGrowth);
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

                float[] previousTreeHeights = new float[treeData.UsedRecordCount];
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    for (int treeIndex = 0; treeIndex < treeData.UsedRecordCount; ++treeIndex)
                    {
                        if (treeData.Integer[treeIndex, (int)TreePropertyInteger.SpeciesGroup] <= treeData.MaxBigSixSpeciesGroupIndex)
                        {
                            HeightGrowth.HTGRO1(treeIndex, 0, 0, variant, simulationStep, treeData.MaxBigSixSpeciesGroupIndex, treeData.Integer,
                                                treeData.Float, treeData.PrimarySiteIndex, treeData.MortalitySiteIndex, CCH, PN, YF, 
                                                TestConstant.Default.BABT, BART, YT, ref OLD, TestConstant.Default.PDEN, treeData.Growth);
                        }
                        else
                        {
                            HeightGrowth.HTGRO2(treeIndex, variant, treeData.MaxBigSixSpeciesGroupIndex, treeData.Integer, treeData.Float,
                                                TestConstant.Default.RedAlderSiteIndex, CALIB, treeData.Growth);
                        }

                        float heightInFeet = treeData.Float[treeIndex, (int)TreePropertyFloat.Height];
                        float previousHeightInFeet = previousTreeHeights[treeIndex];
                        // TODO: make upper limit of height species specific
                        Assert.IsTrue(heightInFeet < TestConstant.Maximum.HeightInFeet);

                        previousTreeHeights[treeIndex] = heightInFeet;
                    }

                    this.Verify(treeData, variantCapabilities);
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
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);

                TreeData treeData = this.CreateDefaultTrees(variant);
                float PA1MAX = TestConstant.Default.PA1MAX;
                float NO = TestConstant.Default.NO;
                float RAAGE = TestConstant.Default.RAAGE;
                for (int simulationStep = 0; simulationStep < TestConstant.Default.SimulationCyclesToRun; ++simulationStep)
                {
                    // TODO: MORT = false case for additional mortality disabled
                    // TODO: POST = true case for cycle after thinning
                    Mortality.MORTAL(variant, simulationStep, treeData.UsedRecordCount, treeData.MaxBigSixSpeciesGroupIndex, treeData.Integer, false, true, 
                                     treeData.Float, treeData.ShadowCrownRatio, treeData.Growth, treeData.MGExpansionFactor, 
                                     treeData.DeadExpansionFactor, competition.LargeTreeBasalAreaLarger, competition.SmallTreeBasalAreaLarger, treeData.PrimarySiteIndex, 
                                     treeData.MortalitySiteIndex, PN, YF, TestConstant.Default.A1, TestConstant.Default.A2, 
                                     TestConstant.Default.A1MAX, ref PA1MAX, ref NO,
                                     TestConstant.Default.RD0, ref RAAGE, TestConstant.Default.PDEN);
                    this.Verify(treeData, variantCapabilities);

                    // TODO: xind -1.0 case
                    float xind = 0.0F;
                    Mortality.OldGro(treeData.UsedRecordCount, treeData.MaxBigSixSpeciesGroupIndex, treeData.Integer, treeData.Float, treeData.Growth, 
                                     treeData.DeadExpansionFactor, xind, out float OG);
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
                TreeData treeData = this.CreateDefaultTrees(variant);

                Stats.SSTATS(variant, treeData.UsedRecordCount, treeData.Integer, treeData.Float, out float basalAreaPerAcre, out float treesPerAcre, out float SCCF, 
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

                this.Verify(treeData, variantCapabilities);
            }
        }

        [TestMethod]
        public void SubmaxApi()
        {
            this.TestContext.WriteLine("version, A1, A2");
            foreach (Variant variant in TestConstant.Variants)
            {
                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                TreeData treeData = this.CreateDefaultTrees(variant);

                Submax.SUBMAX(false, variant, treeData.UsedRecordCount, treeData.Integer, treeData.Float, treeData.MGExpansionFactor, 
                              TestConstant.Default.MaximumReinekeStandDensityIndex, TestConstant.Default.MaximumReinekeStandDensityIndex - 5.0F,
                              TestConstant.Default.MaximumReinekeStandDensityIndex - 10.0F, out float a1, out float a2);
                this.TestContext.WriteLine("{0},{1},{2}", variant, a1, a2);

                Assert.IsTrue(a1 < 7.0F);
                Assert.IsTrue(a1 > 5.0F);
                Assert.IsTrue(a2 > 0.60F);
                Assert.IsTrue(a2 < 0.65F);
                this.Verify(treeData, variantCapabilities);
            }
        }

        [TestMethod]
        public void TripleApi()
        {
            this.TestContext.WriteLine("version, A1, A2");
            foreach (Variant variant in TestConstant.Variants)
            {
                TreeData treeData = this.CreateDefaultTrees(variant, 100);
                int bigSixSpeciesTreeRecordCountBeforeTripling = treeData.GetBigSixSpeciesRecordCount();
                int bigSixSpeciesTreeRecordCountAfterTripling = 0;
                int otherTreeSpeciesRecordCountAfterTripling = 0;
                int usedRecordCount = treeData.UsedRecordCount;

                for (int sourceTreeIndex = 0; sourceTreeIndex < treeData.UsedRecordCount; ++sourceTreeIndex)
                {
                    int speciesGroup = treeData.Integer[sourceTreeIndex, (int)TreePropertyInteger.SpeciesGroup];
                    if (speciesGroup > treeData.MaxBigSixSpeciesGroupIndex)
                    {
                        continue;
                    }

                    int triplingDone = 0;
                    int destinationTreeIndex = treeData.UsedRecordCount + bigSixSpeciesTreeRecordCountAfterTripling;
                    Triple.XTRIP(sourceTreeIndex, destinationTreeIndex, 1, triplingDone, bigSixSpeciesTreeRecordCountBeforeTripling, true, 
                                 treeData.MaxBigSixSpeciesGroupIndex, bigSixSpeciesTreeRecordCountAfterTripling, 
                                 treeData.Triple.POINT, treeData.Triple.TREENO, treeData.Integer, treeData.Triple.PruningAge, treeData.Triple.BranchCount, treeData.Triple.BranchHeight, treeData.Triple.BranchDiameter, treeData.Triple.JuvenileCore, treeData.Triple.NPR, treeData.Triple.PruningLH, treeData.Triple.PruningDbhInInches, treeData.Triple.PruningHeightInFeet, treeData.Triple.PruningCrownRatio, treeData.Triple.PREXP, treeData.ShadowCrownRatio, treeData.Triple.VOLTR, treeData.Triple.SYTVOL);

                    Triple.DGTRIP(sourceTreeIndex, ref destinationTreeIndex, 1, ref triplingDone, variant, treeData.MaxBigSixSpeciesGroupIndex, ref bigSixSpeciesTreeRecordCountAfterTripling, ref otherTreeSpeciesRecordCountAfterTripling, treeData.Integer, treeData.Float, treeData.Growth, treeData.MGExpansionFactor, treeData.DeadExpansionFactor);

                    destinationTreeIndex -= 2;
                    bigSixSpeciesTreeRecordCountAfterTripling -= 2;
                    Triple.HGTRIP(sourceTreeIndex, ref destinationTreeIndex, 1, ref triplingDone, variant, ref bigSixSpeciesTreeRecordCountAfterTripling, treeData.Integer, treeData.Float, treeData.Growth, treeData.MGExpansionFactor, treeData.DeadExpansionFactor);

                    usedRecordCount = destinationTreeIndex;
                }

                OrganonCapabilities variantCapabilities = new OrganonCapabilities(variant);
                treeData.UsedRecordCount = usedRecordCount;
                this.Verify(treeData, variantCapabilities);
                this.Verify(treeData.Triple);
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
