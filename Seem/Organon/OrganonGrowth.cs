using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm.Organon
{
    internal class OrganonGrowth
    {
        public static float GetCrownRatioAdjustment(float crownRatio)
        {
            if (crownRatio > 0.11F)
            {
                return 1.0F; // accurate within 0.05%
            }

            // slowdowns typically measured with fifth order polynomial approximation in Douglas-fir benchmark
            // This appears associated with trees falling under the if statement above.
            return 1.0F - MathV.Exp(-(25.0F * 25.0F * crownRatio * crownRatio));
        }

        public static Vector128<float> GetCrownRatioAdjustment(Vector128<float> crownRatio)
        {
            Vector128<float> crownRatioAdjustment = AvxExtensions.BroadcastScalarToVector128(1.0F);
            int exponentMask = Avx.MoveMask(Avx.CompareLessThan(crownRatio, AvxExtensions.BroadcastScalarToVector128(0.11F)));
            if (exponentMask != 0)
            {
                Vector128<float> power = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(-25.0F * 25.0F), Avx.Multiply(crownRatio, crownRatio));
                Vector128<float> exponent = MathV.MaskExp(power, exponentMask);
                crownRatioAdjustment = Avx.Subtract(crownRatioAdjustment, exponent);
            }
            return crownRatioAdjustment;
        }

        private static float GetDiameterFertilizationMultiplier(OrganonConfiguration configuration, OrganonTreatments treatments, FiaCode species, float siteIndexFromDbh)
        {
            // fertilization diameter effects currently supported only for non-RAP Douglas-fir
            if ((species != FiaCode.PseudotsugaMenziesii) || (configuration.Variant.TreeModel != TreeModel.OrganonRap))
            {
                return 1.0F;
            }

            // non-RAP Douglas-fir
            // FR_ΔD from equation 11 of Hann et. al 2003 FRL Research Contribution 40
            float a10 = 1.368661121F;
            float PF2 = 0.741476964F;
            float PF3 = -0.214741684F;
            float PF4 = -0.851736558F;

            float fertX1 = treatments.GetFertX1(configuration.Variant, PF3 / PF2, out float mostRecentFertilization, out int yearsSinceMostRecentFertilization);
            if (mostRecentFertilization == 0.0F)
            {
                // no fertilization treatment
                return 1.0F;
            }
            float fertX2 = MathV.Exp(PF3 * yearsSinceMostRecentFertilization + MathF.Pow(PF4 * (siteIndexFromDbh / 100.0F), 2.0F)); // ΔDMOD_F combined with part of MFR_ΔD

            float diameterFertilizationMultiplier = 1.0F + a10 * MathV.Pow(mostRecentFertilization / 800.0F + fertX1, PF2) * fertX2;
            Debug.Assert(diameterFertilizationMultiplier >= 1.0F);
            Debug.Assert(diameterFertilizationMultiplier < 1.4F);
            return diameterFertilizationMultiplier;
        }

        /// <summary>
        /// Find diameter growth multiplier for thinning.
        /// </summary>
        /// <param name="configuration">Organon configuration.</param>
        /// <param name="species">FIA species code.</param>
        /// <remarks>
        /// Has special cases for Douglas-fir, western hemlock, and red alder (only for RAP).
        /// </remarks>
        private static float GetDiameterThinningMultiplier(OrganonConfiguration configuration, OrganonTreatments treatments, FiaCode species)
        {
            // Hann DW, Marshall DD, Hanus ML. 2003. Equations for predicting height-to-crown-base, 5-year diameter-growth rate, 5 year height
            //   growth rate, 5-year mortality rate, and maximum size-density trajectory for Douglas-fir and western hemlock in the coastal region
            //   of the Pacific Northwest. Research Contribution 40, Forest Research Laboratory, College of Forestry, Oregon State University.
            //   https://ir.library.oregonstate.edu/concern/technical_reports/jd472x893
            // Table 21 (p35)
            float a8;
            float a9;
            if (species == FiaCode.TsugaHeterophylla)
            {
                a8 = 0.723095045F;
                a9 = -0.2644085320F;
            }
            else if (species == FiaCode.PseudotsugaMenziesii)
            {
                a8 = 0.6203827985F;
                a9 = -0.2644085320F;
            }
            else if ((configuration.Variant.TreeModel == TreeModel.OrganonRap) && (species == FiaCode.AlnusRubra))
            {
                // thinning effects not supported
                return 1.0F;
            }
            else
            {
                a8 = 0.6203827985F;
                a9 = -0.2644085320F;
            }

            float prem = treatments.GetPrem(configuration.Variant, a9, out int yearsSinceMostRecentThin);
            if (prem == 0.0F)
            {
                return 1.0F;
            }

            float diameterGrowthMultiplier = 1.0F + a8 * prem * MathV.Exp(a9 * yearsSinceMostRecentThin);
            Debug.Assert(diameterGrowthMultiplier >= 1.0F); // increased diameter growth from increased taper
            Debug.Assert(diameterGrowthMultiplier < 1.5F);
            return diameterGrowthMultiplier;
        }

        private static float GetHeightFertilizationMultiplier(OrganonConfiguration configuration, OrganonTreatments treatments, FiaCode species, float siteIndexFromBreastHeight)
        {
            // fertilization height effects currently supported only for non-RAP Douglas-fir
            if ((species != FiaCode.PseudotsugaMenziesii) || (configuration.Variant.TreeModel != TreeModel.OrganonRap))
            {
                return 1.0F;
            }

            // non-RAP Douglas-fir
            float PF1 = 1.0F;
            float PF2 = 0.333333333F;
            float PF3 = -1.107409443F;
            float PF4 = -2.133334346F;
            float PF5 = 1.5F;

            float fertX1 = treatments.GetFertX1(configuration.Variant, PF3 / PF2, out float mostRecentFertilization, out int yearsSinceMostRecentFertilization);
            if (mostRecentFertilization == 0.0F)
            {
                // no fertilization treatment
                return 1.0F;
            }
            float fertX2 = MathV.Exp(PF3 * yearsSinceMostRecentFertilization + PF4 * MathV.Pow(siteIndexFromBreastHeight / 100.0F, PF5));

            float heightFertilizationMultiplier = 1.0F + PF1 * MathV.Pow(treatments.PoundsOfNitrogenPerAcreByPeriod[0] / 800.0F + fertX1, PF2) * fertX2;
            Debug.Assert(heightFertilizationMultiplier >= 1.0F);
            Debug.Assert(heightFertilizationMultiplier < 1.4F);
            return heightFertilizationMultiplier;
        }

        private static float GetHeightThinningMultiplier(OrganonConfiguration configuration, OrganonTreatments treatments, FiaCode species)
        {
            float b9;
            float b10;
            float b11;
            if (configuration.Variant.TreeModel != TreeModel.OrganonRap)
            {
                if (species == FiaCode.PseudotsugaMenziesii)
                {
                    b9 = -0.3197415492F;
                    b10 = 0.7528887377F;
                    b11 = -0.2268800162F;
                }
                else
                {
                    // thinning effects not supported: avoid subsequent calculation costs
                    return 1.0F;
                }
            }
            else
            {
                if (species == FiaCode.AlnusRubra)
                {
                    b9 = -0.613313694F;
                    b10 = 1.0F;
                    b11 = -0.443824038F;
                }
                else if (species == FiaCode.PseudotsugaMenziesii)
                {
                    b9 = -0.3197415492F;
                    b10 = 0.7528887377F;
                    b11 = -0.2268800162F;
                }
                else
                {
                    // thinning effects not supported
                    return 1.0F;
                }
            }

            float prem = treatments.GetPrem(configuration.Variant, b11 / b10, out int yearsSinceMostRecentThin);
            if (prem == 0.0F)
            {
                return 1.0F;
            }

            float thinMultiplier = 1.0F + b9 * MathF.Pow(prem, b10) * MathV.Exp(b11 * yearsSinceMostRecentThin);
            Debug.Assert(thinMultiplier > 0.6F); // reduced height growth
            Debug.Assert(thinMultiplier <= 1.0F);
            return thinMultiplier;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="periodIndex">Index of the growth period being simulated.</param>
        /// <param name="configuration">Organon growth simulation options and site settings.</param>
        /// <param name="stand"></param>
        /// <param name="previousCalibrationBySpecies">Array of calibration coefficients. Values must be between 0.5 and 2.0.</param>
        public static void Grow(int periodIndex, OrganonConfiguration configuration, OrganonTreatments treatments, OrganonStand stand, SortedList<FiaCode, SpeciesCalibration> previousCalibrationBySpecies)
        {
            // BUGBUG: simulationStep largely duplicates stand age
            OrganonGrowth.ValidateArguments(periodIndex, configuration, treatments, stand, previousCalibrationBySpecies, out int BIG6, out int BNXT);

            SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies = new(previousCalibrationBySpecies.Count);
            foreach (KeyValuePair<FiaCode, SpeciesCalibration> species in previousCalibrationBySpecies)
            {
                SpeciesCalibration speciesCalibration = new();
                calibrationBySpecies.Add(species.Key, speciesCalibration);

                if (configuration.CalibrateHeight)
                {
                    speciesCalibration.Height = (1.0F + previousCalibrationBySpecies[species.Key].Height) / 2.0F + MathV.Pow(0.5F, 0.5F * periodIndex) * ((previousCalibrationBySpecies[species.Key].Height - 1.0F) / 2.0F);
                }
                if (configuration.CalibrateCrownRatio)
                {
                    speciesCalibration.CrownRatio = (1.0F + previousCalibrationBySpecies[species.Key].CrownRatio) / 2.0F + MathV.Pow(0.5F, 0.5F * periodIndex) * ((previousCalibrationBySpecies[species.Key].CrownRatio - 1.0F) / 2.0F);
                }
                if (configuration.CalibrateDiameter)
                {
                    speciesCalibration.Diameter = (1.0F + previousCalibrationBySpecies[species.Key].Diameter) / 2.0F + MathV.Pow(0.5F, 0.5F * periodIndex) * ((previousCalibrationBySpecies[species.Key].Diameter - 1.0F) / 2.0F);
                }
            }

            // density at start of growth
            stand.SetRedAlderSiteIndexAndGrowthEffectiveAge();
            OrganonStandDensity densityBeforeGrowth = new(configuration.Variant, stand);

            // crown competition at start of growth
            float[] crownCompetitionByHeight = OrganonStandDensity.GetCrownCompetitionByHeight(configuration.Variant, stand);
            OrganonGrowth.Grow(configuration, treatments, stand, densityBeforeGrowth, calibrationBySpecies, ref crownCompetitionByHeight, out OrganonStandDensity _, out int oldTreeRecordCount);

            if (configuration.IsEvenAge == false)
            {
                stand.AgeInYears = 0;
                stand.BreastHeightAgeInYears = 0;
            }
            float oldTreePercentage = 100.0F * (float)oldTreeRecordCount / (float)(BIG6 - BNXT);
            if (oldTreePercentage > 50.0F)
            {
                stand.Warnings.TreesOld = true;
            }
            if (configuration.Variant.TreeModel == TreeModel.OrganonSwo)
            {
                if (configuration.IsEvenAge && (stand.BreastHeightAgeInYears > 500.0F))
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else if ((configuration.Variant.TreeModel == TreeModel.OrganonNwo) || (configuration.Variant.TreeModel == TreeModel.OrganonSmc))
            {
                if (configuration.IsEvenAge && (stand.BreastHeightAgeInYears > 120.0F))
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else
            {
                if (configuration.IsEvenAge && (stand.AgeInYears > 30.0F))
                {
                    stand.Warnings.TreesOld = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration">Organon configuration settings.</param>
        /// <param name="treatments"></param>
        /// <param name="stand">Stand data.</param>
        /// <param name="densityInPreviousPeriod">Stand density at end of previous timestep or the initial stand density if this is the first timestep.</param>
        /// <param name="calibrationBySpecies"></param>
        /// <param name="crownCompetitionByHeight"></param>
        /// <param name="densityAfterGrowth"></param>
        /// <param name="oldTreeRecordCount"></param>
        public static void Grow(OrganonConfiguration configuration, OrganonTreatments treatments, OrganonStand stand, OrganonStandDensity densityInPreviousPeriod,
                                SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies, ref float[] crownCompetitionByHeight, out OrganonStandDensity densityAfterGrowth, 
                                out int oldTreeRecordCount)
        {
            // initalize step
            float DGMODgenetic = 1.0F;
            float HGMODgenetic = 1.0F;
            float DGMODSwissNeedleCast = 1.0F;
            float HGMODSwissNeedleCast = 1.0F;
            if ((stand.AgeInYears > 0) && configuration.Genetics)
            {
                OrganonGrowthModifiers.GetGeneticModifiers(stand.AgeInYears, configuration.GWDG, configuration.GWHG, out DGMODgenetic, out HGMODgenetic);
            }
            if (configuration.SwissNeedleCast && (configuration.Variant.TreeModel == TreeModel.OrganonNwo || configuration.Variant.TreeModel == TreeModel.OrganonSmc))
            {
                OrganonGrowthModifiers.GetSwissNeedleCastModifiers(configuration.FR, out DGMODSwissNeedleCast, out HGMODSwissNeedleCast);
            }

            // diameter growth
            int treeRecordsWithExpansionFactorZero = 0;
            int bigSixRecordsWithExpansionFactorZero = 0;
            int otherSpeciesRecordsWithExpansionFactorZero = 0;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    if (treesOfSpecies.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        ++treeRecordsWithExpansionFactorZero;
                        if (configuration.Variant.IsBigSixSpecies(treesOfSpecies.Species))
                        {
                            ++bigSixRecordsWithExpansionFactorZero;
                        }
                        else
                        {
                            ++otherSpeciesRecordsWithExpansionFactorZero;
                        }
                    }
                }
            }

            // BUGBUG no check that SITE_1 and SITE_2 indices are greater than 4.5 feet
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                float geneticDiseaseAndCalibrationMultiplier = calibrationBySpecies[treesOfSpecies.Species].Diameter;
                if (treesOfSpecies.Species == FiaCode.PseudotsugaMenziesii)
                {
                    geneticDiseaseAndCalibrationMultiplier *= DGMODgenetic * DGMODSwissNeedleCast;
                }
                OrganonGrowth.GrowDiameter(configuration, treatments, stand, treesOfSpecies, densityInPreviousPeriod, geneticDiseaseAndCalibrationMultiplier);
            }

            // height growth for big six species
            oldTreeRecordCount = 0;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                if (configuration.Variant.IsBigSixSpecies(species) == false)
                {
                    continue;
                }

                float geneticAndDiseaseMultiplier = 1.0F;
                if (treesOfSpecies.Species == FiaCode.PseudotsugaMenziesii)
                {
                    geneticAndDiseaseMultiplier = HGMODgenetic * HGMODSwissNeedleCast;
                }
                OrganonGrowth.GrowHeightBigSixSpecies(configuration, treatments, stand, treesOfSpecies, geneticAndDiseaseMultiplier, crownCompetitionByHeight, out int oldTreeRecordCountForSpecies);
                oldTreeRecordCount += oldTreeRecordCountForSpecies;
            }

            // determine mortality
            // Sets configuration.NO.
            OrganonMortality.ReduceExpansionFactors(configuration, treatments, stand, densityInPreviousPeriod);

            // grow tree diameters
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    treesOfSpecies.Dbh[treeIndex] += treesOfSpecies.DbhGrowth[treeIndex];
                }
            }

            // recalculate TPA, BA, and CCF based on diameter growth and big six height growth
            // BUGBUG: since is calculated before non-big six height growth, the CCF effects of this height growth on crown growth are lagged by 
            //         a simulation time step
            densityAfterGrowth = new OrganonStandDensity(configuration.Variant, stand);

            // height growth for non-big six species
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                if (configuration.Variant.IsBigSixSpecies(species))
                {
                    continue;
                }

                OrganonGrowth.GrowHeightMinorSpecies(configuration, stand, treesOfSpecies, calibrationBySpecies[species].Height);
            }

            // grow crowns
            crownCompetitionByHeight = OrganonGrowth.GrowCrown(configuration.Variant, stand, densityAfterGrowth, calibrationBySpecies);

            // update stand age and period variables
            stand.AgeInYears += configuration.Variant.TimeStepInYears;
            stand.BreastHeightAgeInYears += configuration.Variant.TimeStepInYears;
            stand.RedAlderGrowthEffectiveAge += configuration.Variant.TimeStepInYears;

            // reduce calibration ratios
            foreach (SpeciesCalibration speciesCalibration in calibrationBySpecies.Values)
            {
                float heightFactor = (1.0F + speciesCalibration.Height) / 2.0F;
                speciesCalibration.Height = heightFactor + 0.7071067812F * (speciesCalibration.Height - heightFactor); // 0.7071067812 = MathF.Sqrt(0.5F)
                float diameterFactor = (1.0F + speciesCalibration.Diameter) / 2.0F;
                speciesCalibration.Diameter = diameterFactor + 0.7071067812F * (speciesCalibration.Diameter - diameterFactor);
            }
        }

        public static float[] GrowCrown(OrganonVariant variant, OrganonStand stand, OrganonStandDensity densityAfterGrowth, SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies)
        {
            float oldGrowthIndicator = OrganonMortality.GetOldGrowthIndicator(variant, stand);
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                variant.GrowCrown(stand, treesOfSpecies, densityAfterGrowth, oldGrowthIndicator, calibrationBySpecies[treesOfSpecies.Species].CrownRatio);
            }

            return OrganonStandDensity.GetCrownCompetitionByHeight(variant, stand);
        }

        public static void GrowDiameter(OrganonConfiguration configuration, OrganonTreatments treatments, OrganonStand stand, Trees trees, OrganonStandDensity densityBeforeGrowth, float geneticDiseaseAndCalibrationMultiplier)
        {
            FiaCode species = trees.Species;
            float siteIndex = stand.SiteIndexInFeet;
            if (species == FiaCode.TsugaHeterophylla)
            {
                siteIndex = stand.HemlockSiteIndexInFeet;
            }
            // questionable descisions retained from Fortran code due to calibration fragility:
            // - ponderosa index isn't used for SWO
            // - red alder index isn't used for red alder

            float fertilizationMultiplier = OrganonGrowth.GetDiameterFertilizationMultiplier(configuration, treatments, species, stand.SiteIndexInFeet - 4.5F);
            float thinningMultiplier = OrganonGrowth.GetDiameterThinningMultiplier(configuration, treatments, species);
            float combinedMultiplier = geneticDiseaseAndCalibrationMultiplier * fertilizationMultiplier * thinningMultiplier;
            configuration.Variant.GrowDiameter(trees, combinedMultiplier, siteIndex, densityBeforeGrowth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration">Organon configuration.</param>
        /// <param name="treatments"></param>
        /// <param name="periodIndex">Simulation cycle.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="trees">Trees to calculate height growth of.</param>
        /// <param name="geneticAndDiseaseMultiplier">Direct multiplier for genetic and disease height growth effects.</param>
        /// <param name="crownCompetitionByHeight"></param>
        /// <param name="oldTreeRecordCount"></param>
        public static void GrowHeightBigSixSpecies(OrganonConfiguration configuration, OrganonTreatments treatments, OrganonStand stand, Trees trees, 
                                                   float geneticAndDiseaseMultiplier, float[] crownCompetitionByHeight, out int oldTreeRecordCount)
        {
            FiaCode species = trees.Species;
            Debug.Assert(configuration.Variant.IsBigSixSpecies(species));
            oldTreeRecordCount = configuration.Variant.GrowHeightBigSix(configuration, stand, trees, crownCompetitionByHeight);

            float fertilizationMultiplier = OrganonGrowth.GetHeightFertilizationMultiplier(configuration, treatments, species, stand.SiteIndexInFeet);
            float thinningMultiplier = OrganonGrowth.GetHeightThinningMultiplier(configuration, treatments, species);
            float combinedMultiplier = geneticAndDiseaseMultiplier * thinningMultiplier * fertilizationMultiplier;
            OrganonGrowth.LimitAndApplyHeightGrowth(configuration.Variant, trees, combinedMultiplier);
        }

        public static void GrowHeightMinorSpecies(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float calibrationMultiplier)
        {
            FiaCode species = trees.Species;
            Debug.Assert(configuration.Variant.IsBigSixSpecies(species) == false);

            // special case for non-RAP red alders
            if ((species == FiaCode.AlnusRubra) && (configuration.Variant.TreeModel != TreeModel.OrganonRap))
            {
                for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
                {
                    if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        trees.HeightGrowth[treeIndex] = 0.0F;
                        continue;
                    }

                    float growthEffectiveAge = RedAlder.GetGrowthEffectiveAge(trees.Height[treeIndex], stand.RedAlderSiteIndexInfeet);
                    if (growthEffectiveAge <= 0.0F)
                    {
                        trees.HeightGrowth[treeIndex] = 0.0F;
                    }
                    else
                    {
                        float RAH1 = RedAlder.GetH50(growthEffectiveAge, stand.RedAlderSiteIndexInfeet);
                        float RAH2 = RedAlder.GetH50(growthEffectiveAge + configuration.Variant.TimeStepInYears, stand.RedAlderSiteIndexInfeet);
                        trees.HeightGrowth[treeIndex] = RAH2 - RAH1;
                        Debug.Assert(trees.HeightGrowth[treeIndex] >= 0.0F);
                        Debug.Assert(trees.HeightGrowth[treeIndex] < Constant.Maximum.HeightIncrementInFeet);
                        trees.Height[treeIndex] += trees.HeightGrowth[treeIndex];
                    }
                }
                return;
            }

            // mainline case for all other species and Organon variants
            // TODO: could previous predicted height or pre-growth height be used?
            configuration.Variant.GetHeightPredictionCoefficients(species, out float B0, out float B1, out float B2);
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    trees.HeightGrowth[treeIndex] = 0.0F;
                    continue;
                }

                float endDbhInInches = trees.Dbh[treeIndex];
                float endPredictedHeight = 4.5F + MathV.Exp(B0 + B1 * MathV.Pow(endDbhInInches, B2));
                endPredictedHeight = 4.5F + calibrationMultiplier * (endPredictedHeight - 4.5F);

                float startDbhInInches = endDbhInInches - trees.DbhGrowth[treeIndex];
                float startPredictedHeight = 4.5F + MathV.Exp(B0 + B1 * MathV.Pow(startDbhInInches, B2));
                startPredictedHeight = 4.5F + calibrationMultiplier * (startPredictedHeight - 4.5F);

                float predictedHeight = (endPredictedHeight / startPredictedHeight) * trees.Height[treeIndex];
                trees.HeightGrowth[treeIndex] = predictedHeight - trees.Height[treeIndex];

                Debug.Assert(trees.HeightGrowth[treeIndex] >= 0.0F);
                Debug.Assert(trees.HeightGrowth[treeIndex] < Constant.Maximum.HeightIncrementInFeet);
                trees.Height[treeIndex] += trees.HeightGrowth[treeIndex];
            }
        }

        private static void LimitAndApplyHeightGrowth(OrganonVariant variant, Trees trees, float combinedAdjustment)
        {
            FiaCode speciesWithSwoTsheOptOut = trees.Species;
            if ((speciesWithSwoTsheOptOut == FiaCode.TsugaHeterophylla) && (variant.TreeModel == TreeModel.OrganonSwo))
            {
                // BUGBUG: not clear why SWO uses default coefficients for hemlock
                speciesWithSwoTsheOptOut = FiaCode.NotholithocarpusDensiflorus;
            }

            float A0;
            float A1;
            float A2 = 1.0F;
            switch (speciesWithSwoTsheOptOut)
            {
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.TsugaHeterophylla:
                    A0 = 19.04942539F;
                    A1 = -0.04484724F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    A0 = 16.26279948F;
                    A1 = -0.04484724F;
                    break;
                case FiaCode.PinusPonderosa:
                    A0 = 17.11482201F;
                    A1 = -0.04484724F;
                    break;
                case FiaCode.PinusLambertiana:
                    A0 = 14.29011403F;
                    A1 = -0.04484724F;
                    break;
                case FiaCode.AlnusRubra:
                    A0 = 60.619859F;
                    A1 = -1.59138564F;
                    A2 = 0.496705997F;
                    break;
                default:
                    A0 = 15.80319194F;
                    A1 = -0.04484724F;
                    break;
            }

            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    continue;
                }

                float HG = combinedAdjustment * trees.HeightGrowth[treeIndex];
                float HT1 = trees.Height[treeIndex] - 4.5F;
                float HT2 = HT1 + HG;
                float HT3 = HT2 + HG;
                float DG = trees.DbhGrowth[treeIndex];
                float DBH1 = trees.Dbh[treeIndex];
                float DBH2 = DBH1 + DG;
                float DBH3 = DBH2 + DG;
                float PHT1;
                float PHT2;
                float PHT3;
                if (A2 == 1.0F)
                {
                    // most species
                    PHT1 = A0 * DBH1 / (1.0F - A1 * DBH1);
                    PHT2 = A0 * DBH2 / (1.0F - A1 * DBH2);
                    PHT3 = A0 * DBH3 / (1.0F - A1 * DBH3);
                }
                else
                {
                    // red alder
                    PHT1 = A0 * DBH1 / (1.0F - A1 * MathV.Pow(DBH1, A2));
                    PHT2 = A0 * DBH2 / (1.0F - A1 * MathV.Pow(DBH2, A2));
                    PHT3 = A0 * DBH3 / (1.0F - A1 * MathV.Pow(DBH3, A2));
                }

                float PHGR1 = (PHT2 - PHT1 + HG) / 2.0F;
                float PHGR2 = PHT2 - HT1;
                if (HT2 > PHT2)
                {
                    if (PHGR1 < PHGR2)
                    {
                        HG = PHGR1;
                    }
                    else
                    {
                        HG = PHGR2;
                    }
                }
                else
                {
                    if (HT3 > PHT3)
                    {
                        HG = PHGR1;
                    }
                }
                if (HG < 0.0F)
                {
                    HG = 0.0F;
                }

                Debug.Assert(HG >= 0.0F);
                Debug.Assert(HG < Constant.Maximum.HeightIncrementInFeet);
                trees.HeightGrowth[treeIndex] = HG;
                trees.Height[treeIndex] += HG;
            }
        }

        /// <summary>
        /// Sets height and crown ratio of ingrowth appended at the end of a list of trees.
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="treesOfSpecies"></param>
        /// <param name="ingrowthCount">Number of new trees added to end of treesOfSpecies.</param>
        /// <param name="calibrationBySpecies"></param>
        public static void SetIngrowthHeightAndCrownRatio(OrganonVariant variant, OrganonStand stand, Trees treesOfSpecies, int ingrowthCount, SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies)
        {
            if ((ingrowthCount < 0) || (ingrowthCount > treesOfSpecies.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(ingrowthCount));
            }

            // ROUTINE TO CALCULATE MISSING CROWN RATIOS
            variant.GetHeightPredictionCoefficients(treesOfSpecies.Species, out float B0, out float B1, out float B2);
            for (int treeIndex = treesOfSpecies.Count - ingrowthCount; treeIndex < treesOfSpecies.Count; ++treeIndex)
            {
                float heightInFeet = treesOfSpecies.Height[treeIndex];
                if (heightInFeet != 0.0F)
                {
                    continue;
                }

                float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                float predictedHeight = 4.5F + MathV.Exp(B0 + B1 * MathV.Pow(dbhInInches, B2));
                treesOfSpecies.Height[treeIndex] = 4.5F + calibrationBySpecies[treesOfSpecies.Species].Height * (predictedHeight - 4.5F);
            }

            if ((stand.SiteIndexInFeet < Constant.Minimum.SiteIndexInFeet) || (stand.HemlockSiteIndexInFeet < Constant.Minimum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(stand));
            }
            float siteIndexFromDbh = stand.SiteIndexInFeet - 4.5F;
            float hemlockSiteIndexFromDbh = stand.HemlockSiteIndexInFeet - 4.5F;

            float oldGrowthIndicator = OrganonMortality.GetOldGrowthIndicator(variant, stand);
            OrganonStandDensity standDensity = new(variant, stand);
            FiaCode species = treesOfSpecies.Species;
            for (int treeIndex = treesOfSpecies.Count - ingrowthCount; treeIndex < treesOfSpecies.Count; ++treeIndex)
            {
                float crownRatio = treesOfSpecies.CrownRatio[treeIndex];
                if (crownRatio != 0.0F)
                {
                    continue;
                }

                float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                float heightInFeet = treesOfSpecies.Height[treeIndex];
                float crownCompetitionFactorLarger = standDensity.GetCrownCompetitionFactorLarger(dbhInInches);
                float heightToCrownBase = variant.GetHeightToCrownBase(species, heightInFeet, dbhInInches, crownCompetitionFactorLarger, standDensity.BasalAreaPerAcre, siteIndexFromDbh, hemlockSiteIndexFromDbh, oldGrowthIndicator);
                if (heightToCrownBase < 0.0F)
                {
                    heightToCrownBase = 0.0F;
                }
                if (heightToCrownBase > 0.95F * heightInFeet)
                {
                    heightToCrownBase = 0.95F * heightInFeet;
                }
                treesOfSpecies.CrownRatio[treeIndex] = (1.0F - (heightToCrownBase / heightInFeet)) * calibrationBySpecies[species].CrownRatio;
            }
        }

        /// <summary>
        /// Does argument checking and raises error flags if problems are found.
        /// </summary>
        /// <param name="periodIndex"></param>
        /// <param name="configuration">Organon configuration settings.</param>
        /// <param name="treatments"></param>
        /// <param name="stand"></param>
        /// <param name="calibrationBySpecies"></param>
        /// <param name="bigSixSpeciesTreeCount"></param>
        /// <param name="bigSixTreesWithNegativeExpansionFactor"></param>
        private static void ValidateArguments(int periodIndex, OrganonConfiguration configuration, OrganonTreatments treatments, 
                                              OrganonStand stand, SortedList<FiaCode, SpeciesCalibration> calibrationBySpecies, 
                                              out int bigSixSpeciesTreeCount, out int bigSixTreesWithNegativeExpansionFactor)
        {
            if (stand.GetTreeRecordCount() < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stand), "Stand does not have any trees.");
            }
            if (Enum.IsDefined(typeof(TreeModel), configuration.Variant.TreeModel) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration), "Unknown Organon variant " + configuration.Variant.TreeModel + ".");
            }
            if (stand.NumberOfPlots < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stand), "Stand has less than one plot.");
            }
            if ((stand.SiteIndexInFeet < Constant.Minimum.SiteIndexInFeet) || (stand.SiteIndexInFeet > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(stand), "Default site index is implausibly small or implausibly large.");
            }
            if ((stand.HemlockSiteIndexInFeet < Constant.Minimum.SiteIndexInFeet) || (stand.HemlockSiteIndexInFeet > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(stand), "Hemlock site index (conifer site index for red alder) is implausibly small or implausibly large.");
            }

            if (configuration.IsEvenAge)
            {
                if (stand.BreastHeightAgeInYears < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(stand.BreastHeightAgeInYears), nameof(stand.BreastHeightAgeInYears) + " must be zero or greater when " + nameof(configuration.IsEvenAge) + " is set.");
                }
                if ((stand.AgeInYears - stand.BreastHeightAgeInYears) < 1)
                {
                    // (DOUG? can stand.AgeInYears ever be less than stand.BreastHeightAgeInYears?)
                    throw new ArgumentException(nameof(stand.AgeInYears) + " must be greater than " + nameof(stand.BreastHeightAgeInYears) + " when " + nameof(configuration.IsEvenAge) + " is set.");
                }
            }
            else
            {
                if (stand.BreastHeightAgeInYears != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(stand.BreastHeightAgeInYears), nameof(stand.BreastHeightAgeInYears) + " must be zero or less when " + nameof(configuration.IsEvenAge) + " is not set.");
                }
            }

            for (int fertilizationindex = 0; fertilizationindex < treatments.PoundsOfNitrogenPerAcreByPeriod.Count; ++fertilizationindex)
            {
                if (fertilizationindex == 0) // TODO: why is only the first treatments subject to range limits?
                {
                    if ((treatments.PoundsOfNitrogenPerAcreByPeriod[fertilizationindex] < 0.0) || (treatments.PoundsOfNitrogenPerAcreByPeriod[fertilizationindex] > 400.0F))
                    {
                        throw new ArgumentException("Nitrogen fertiliation rate must be between 0 and 400 pounds per acre.");
                    }
                }
            }

            foreach (float basalAreaRemoved in treatments.BasalAreaThinnedByPeriod)
            {
                if (basalAreaRemoved < 0.0F)
                {
                    throw new ArgumentException("Prior harvest removed negative basal area.");
                }
            }

            if (periodIndex < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(periodIndex));
            }

            if (configuration.DefaultMaximumSdi > Constant.Maximum.SdiPerAcre)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration), "Default maximum SDI is implausibly large.");
            }
            if (configuration.TrueFirMaximumSdi > Constant.Maximum.SdiPerAcre)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration), "True fir maximum SDI is implausibly large.");
            }
            if (configuration.HemlockMaximumSdi > Constant.Maximum.SdiPerAcre)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration), "Hemlock maximum SDI is implausibly large.");
            }

            if (configuration.Genetics)
            {
                if (!configuration.IsEvenAge)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.Genetics), nameof(configuration.Genetics) + " is supported only when " + nameof(configuration.IsEvenAge) + " is set.");
                }
                if ((configuration.GWDG < 0.0F) || (configuration.GWDG > 20.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration), "GWDG is negative or greater than 20.");
                }
            }
            else
            {
                if (configuration.GWDG != 0.0F)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration), "Genetic modifiers are disabled but GWDG is nonzero.");
                }
            }

            if (configuration.SwissNeedleCast)
            {
                if ((configuration.Variant.TreeModel == TreeModel.OrganonSwo) || (configuration.Variant.TreeModel == TreeModel.OrganonRap))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.Variant), "Swiss needle cast is not supported by the SWO and RAP variants.");
                }
                if (configuration.IsEvenAge == false)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.IsEvenAge), "Swiss needle cast is not supported for uneven age stands.");
                }
                if ((configuration.FR < 0.85F) || (configuration.FR > 7.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration));
                }
                if ((treatments.PoundsOfNitrogenPerAcreByPeriod.Count > 0) && (configuration.FR < 3.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration), nameof(configuration.FR) + " must be 3.0 or greater when " + nameof(configuration.SwissNeedleCast) + " and " + nameof(treatments.PoundsOfNitrogenPerAcreByPeriod) + " isn't empty.");
                }
            }
            else
            {
                if (configuration.FR > 0.0F)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration));
                }
            }

            if ((configuration.Variant.TreeModel == TreeModel.OrganonRap) && (configuration.PDEN < 0.0F))
            {
                throw new NotSupportedException("PDEN is negative for red alder variant.");
            }
            if (!configuration.IsEvenAge && (configuration.Variant.TreeModel == TreeModel.OrganonRap))
            {
                throw new NotSupportedException("Stand for red alder variant is not even age.");
            }

            // TODO: is it desirable to clear existing stand warnings?
            stand.Warnings.BigSixHeightAbovePotential = false;
            stand.Warnings.LessThan50TreeRecords = false;
            stand.Warnings.HemlockSiteIndexOutOfRange = false;
            stand.Warnings.OtherSpeciesBasalAreaTooHigh = false;
            stand.Warnings.SiteIndexOutOfRange = false;
            stand.Warnings.TreesOld = false;
            stand.Warnings.TreesYoung = false;

            foreach (SpeciesCalibration speciesCalibration in calibrationBySpecies.Values)
            {
                if ((speciesCalibration.Height < 0.5F) || (speciesCalibration.Height > 2.0F) ||
                    (speciesCalibration.CrownRatio < 0.5F) || (speciesCalibration.CrownRatio > 2.0F) ||
                    (speciesCalibration.Diameter < 0.5F) || (speciesCalibration.Diameter > 2.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(calibrationBySpecies));
                }
            }

            // check tree records for errors
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    if (configuration.Variant.IsSpeciesSupported(treesOfSpecies.Species) == false)
                    {
                        throw new NotSupportedException(String.Format("{0} does not support {1} (tree {2}).", configuration.Variant.TreeModel, treesOfSpecies.Species, treeIndex));
                    }
                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    if (dbhInInches < 0.09F)
                    {
                        throw new NotSupportedException(String.Format("Diameter of tree {0} is less than 0.1 inches.", treeIndex));
                    }
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    if (heightInFeet < 4.5F)
                    {
                        throw new NotSupportedException(String.Format("Height of tree {0} is less than 4.5 feet.", treeIndex));
                    }
                    float crownRatio = treesOfSpecies.CrownRatio[treeIndex];
                    if ((crownRatio < 0.0F) || (crownRatio > 1.0F))
                    {
                        throw new NotSupportedException(String.Format("Crown ratio of tree {0} is not between 0 and 1.", treeIndex));
                    }
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (expansionFactor < 0.0F)
                    {
                        throw new NotSupportedException(String.Format("Expansion factor of tree {0} is negative.", treeIndex));
                    }
                }
            }

            bigSixSpeciesTreeCount = 0;
            bigSixTreesWithNegativeExpansionFactor = 0;
            float maxGrandFirHeight = 0.0F;
            float maxDouglasFirHeight = 0.0F;
            float maxWesternHemlockHeight = 0.0F;
            float maxPonderosaHeight = 0.0F;
            float maxIncenseCedarHeight = 0.0F;
            float maxRedAlderHeight = 0.0F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    switch (configuration.Variant.TreeModel)
                    {
                        // SWO BIG SIX
                        case TreeModel.OrganonSwo:
                            if ((species == FiaCode.PinusPonderosa) && (heightInFeet > maxPonderosaHeight))
                            {
                                maxPonderosaHeight = heightInFeet;
                            }
                            else if ((species == FiaCode.CalocedrusDecurrens) && (heightInFeet > maxIncenseCedarHeight))
                            {
                                maxIncenseCedarHeight = heightInFeet;
                            }
                            else if ((species == FiaCode.PseudotsugaMenziesii) && (heightInFeet > maxDouglasFirHeight))
                            {
                                maxDouglasFirHeight = heightInFeet;
                            }
                            // BUGBUG: why are true firs and sugar pine being assigned to Douglas-fir max height?
                            else if ((species == FiaCode.AbiesConcolor) && (heightInFeet > maxDouglasFirHeight))
                            {
                                maxDouglasFirHeight = heightInFeet;
                            }
                            else if ((species == FiaCode.AbiesGrandis) && (heightInFeet > maxDouglasFirHeight))
                            {
                                maxDouglasFirHeight = heightInFeet;
                            }
                            else if ((species == FiaCode.PinusLambertiana) && (heightInFeet > maxDouglasFirHeight))
                            {
                                maxDouglasFirHeight = heightInFeet;
                            }
                            break;
                        case TreeModel.OrganonNwo:
                        case TreeModel.OrganonSmc:
                            if ((species == FiaCode.AbiesGrandis) && (heightInFeet > maxGrandFirHeight))
                            {
                                maxGrandFirHeight = heightInFeet;
                            }
                            else if ((species == FiaCode.PseudotsugaMenziesii) && (heightInFeet > maxDouglasFirHeight))
                            {
                                maxDouglasFirHeight = heightInFeet;
                            }
                            else if ((species == FiaCode.TsugaHeterophylla) && (heightInFeet > maxWesternHemlockHeight))
                            {
                                maxWesternHemlockHeight = heightInFeet;
                            }
                            break;
                        case TreeModel.OrganonRap:
                            if ((species == FiaCode.AlnusRubra) && (heightInFeet > maxRedAlderHeight))
                            {
                                maxRedAlderHeight = heightInFeet;
                            }
                            break;
                    }

                    if (configuration.Variant.IsBigSixSpecies(species))
                    {
                        ++bigSixSpeciesTreeCount;
                        if (treesOfSpecies.LiveExpansionFactor[treeIndex] < 0.0F)
                        {
                            ++bigSixTreesWithNegativeExpansionFactor;
                        }
                    }
                }
            }

            // DETERMINE IF SPECIES MIX CORRECT FOR STAND AGE
            float standBasalArea = 0.0F;
            float standBigSixBasalArea = 0.0F;
            float standHardwoodBasalArea = 0.0F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (expansionFactor <= 0.0F)
                    {
                        continue;
                    }

                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    float basalArea = expansionFactor * dbhInInches * dbhInInches;
                    standBasalArea += basalArea;

                    if (configuration.Variant.IsBigSixSpecies(species))
                    {
                        standBigSixBasalArea += basalArea;
                    }
                    if (configuration.Variant.TreeModel == TreeModel.OrganonSwo)
                    {
                        if ((species == FiaCode.ArbutusMenziesii) || (species == FiaCode.ChrysolepisChrysophyllaVarChrysophylla) || (species == FiaCode.QuercusKelloggii))
                        {
                            standHardwoodBasalArea += basalArea;
                        }
                    }
                }
            }

            standBasalArea *= Constant.ForestersEnglish / stand.NumberOfPlots;
            standBigSixBasalArea *= Constant.ForestersEnglish / stand.NumberOfPlots;
            if (standBigSixBasalArea < 0.0F)
            {
                throw new NotSupportedException("Total basal area big six species is negative.");
            }

            if (configuration.Variant.TreeModel >= TreeModel.OrganonRap)
            {
                float PRA;
                if (standBasalArea > 0.0F)
                {
                    PRA = standBigSixBasalArea / standBasalArea;
                }
                else
                {
                    PRA = 0.0F;
                }

                if (PRA < 0.9F)
                {
                    // if needed, make this a warning rather than an error
                    throw new NotSupportedException("Red alder plantation stand is less than 90% by basal area.");
                }
            }

            // DETERMINE WARNINGS (IF ANY)
            // BUGBUG move maximum site indices to variant capabilities
            switch (configuration.Variant.TreeModel)
            {
                case TreeModel.OrganonSwo:
                    if ((stand.SiteIndexInFeet > 0.0F) && ((stand.SiteIndexInFeet < 40.0F) || (stand.SiteIndexInFeet > 150.0F)))
                    {
                        stand.Warnings.SiteIndexOutOfRange = true;
                    }
                    if ((stand.HemlockSiteIndexInFeet > 0.0F) && ((stand.HemlockSiteIndexInFeet < 50.0F) || (stand.HemlockSiteIndexInFeet > 140.0F)))
                    {
                        stand.Warnings.HemlockSiteIndexOutOfRange = true;
                    }
                    break;
                case TreeModel.OrganonNwo:
                case TreeModel.OrganonSmc:
                    if ((stand.SiteIndexInFeet > 0.0F) && ((stand.SiteIndexInFeet < 90.0F) || (stand.SiteIndexInFeet > 142.0F)))
                    {
                        stand.Warnings.SiteIndexOutOfRange = true;
                    }
                    if ((stand.HemlockSiteIndexInFeet > 0.0F) && ((stand.HemlockSiteIndexInFeet < 90.0F) || (stand.HemlockSiteIndexInFeet > 142.0F)))
                    {
                        stand.Warnings.HemlockSiteIndexOutOfRange = true;
                    }
                    break;
                case TreeModel.OrganonRap:
                    if ((stand.SiteIndexInFeet < 20.0F) || (stand.SiteIndexInFeet > 125.0F))
                    {
                        stand.Warnings.SiteIndexOutOfRange = true;
                    }
                    if ((stand.HemlockSiteIndexInFeet > 0.0F) && (stand.HemlockSiteIndexInFeet < 90.0F || stand.HemlockSiteIndexInFeet > 142.0F))
                    {
                        stand.Warnings.HemlockSiteIndexOutOfRange = true;
                    }
                    break;
            }

            // check tallest trees in stand against maximum height for big six species
            // BUGBUG: need an API for maximum heights rather than inline code here
            switch (configuration.Variant.TreeModel)
            {
                case TreeModel.OrganonSwo:
                    if (maxPonderosaHeight > 0.0F)
                    {
                        float maxHeight = (stand.HemlockSiteIndexInFeet - 4.5F) * (1.0F / (1.0F - MathV.Exp(MathF.Pow(-0.164985F * (stand.HemlockSiteIndexInFeet - 4.5F), 0.288169F)))) + 4.5F;
                        if (maxPonderosaHeight > maxHeight)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (maxIncenseCedarHeight > 0.0F)
                    {
                        float incenseCedarSiteIndex = 0.66F * stand.SiteIndexInFeet - 4.5F; // TODO: same as PILA?
                        float maxHeight = incenseCedarSiteIndex * (1.0F / (1.0F - MathV.Exp(MathF.Pow(-0.174929F * incenseCedarSiteIndex, 0.281176F)))) + 4.5F;
                        if (maxIncenseCedarHeight > maxHeight)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (maxDouglasFirHeight > 0.0F)
                    {
                        float maxHeight = (stand.SiteIndexInFeet - 4.5F) * (1.0F / (1.0F - MathV.Exp(MathF.Pow(-0.174929F * (stand.SiteIndexInFeet - 4.5F), 0.281176F)))) + 4.5F;
                        if (maxDouglasFirHeight > maxHeight)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
                case TreeModel.OrganonNwo:
                case TreeModel.OrganonSmc:
                    if (maxDouglasFirHeight > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.SiteIndexInFeet - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (maxDouglasFirHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (maxGrandFirHeight > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.SiteIndexInFeet - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (maxGrandFirHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (maxWesternHemlockHeight > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.HemlockSiteIndexInFeet - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (0.00192F + 0.00007F * Z50);
                        if (maxWesternHemlockHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
                case TreeModel.OrganonRap:
                    if (maxRedAlderHeight > 0.0F)
                    {
                        RedAlder.WHHLB_H40(stand.SiteIndexInFeet, 20.0F, 150.0F, out float MAXHT);
                        if (maxRedAlderHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
            }

            if (configuration.IsEvenAge && (configuration.Variant.TreeModel != TreeModel.OrganonSmc))
            {
                stand.Warnings.TreesYoung = stand.BreastHeightAgeInYears < 10;
            }

            float requiredWellKnownSpeciesBasalAreaFraction = configuration.Variant.TreeModel switch
            {
                TreeModel.OrganonNwo => 0.5F,
                TreeModel.OrganonRap => 0.8F,
                TreeModel.OrganonSmc => 0.5F,
                TreeModel.OrganonSwo => 0.2F,
                _ => throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel),
            };
            if ((standBigSixBasalArea + standHardwoodBasalArea) < (requiredWellKnownSpeciesBasalAreaFraction * standBasalArea))
            {
                stand.Warnings.OtherSpeciesBasalAreaTooHigh = true;
            }
            if (stand.GetTreeRecordCount() < 50)
            {
                stand.Warnings.LessThan50TreeRecords = true;
            }

            // check percentage of trees with high growth effective ages
            int oldTreeRecordCount = 0;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                if (configuration.Variant.IsBigSixSpecies(species) == false)
                {
                    continue;
                }

                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float height = treesOfSpecies.Height[treeIndex];
                    if (height < 4.5F)
                    {
                        continue;
                    }

                    float growthEffectiveAge = configuration.Variant.GetGrowthEffectiveAge(configuration, stand, treesOfSpecies, treeIndex, out float _);
                    if (growthEffectiveAge > configuration.Variant.OldTreeAgeThreshold)
                    {
                        ++oldTreeRecordCount;
                    }
                }
            }

            float percentOldTrees = 100.0F * (float)oldTreeRecordCount / (float)(bigSixSpeciesTreeCount - bigSixTreesWithNegativeExpansionFactor);
            if (percentOldTrees >= 50.0F)
            {
                stand.Warnings.TreesOld = true;
            }
            if (configuration.Variant.TreeModel == TreeModel.OrganonSwo)
            {
                if (configuration.IsEvenAge && stand.BreastHeightAgeInYears > 500)
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else if (configuration.Variant.TreeModel == TreeModel.OrganonNwo || configuration.Variant.TreeModel == TreeModel.OrganonSmc)
            {
                if (configuration.IsEvenAge && stand.BreastHeightAgeInYears > 120)
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else
            {
                if (configuration.IsEvenAge && stand.AgeInYears > 30)
                {
                    stand.Warnings.TreesOld = true;
                }
            }

            // BUGBUG: this is overcomplicated, should just check against maximum stand age using time step from OrganonVapabilities
            int standAgeBudgetAvailableAtNextTimeStep;
            if (configuration.IsEvenAge)
            {
                standAgeBudgetAvailableAtNextTimeStep = configuration.Variant.TreeModel switch
                {
                    TreeModel.OrganonSwo => 500 - stand.AgeInYears - 5,
                    TreeModel.OrganonNwo or 
                    TreeModel.OrganonSmc => 120 - stand.AgeInYears - 5,
                    TreeModel.OrganonRap => 30 - stand.AgeInYears - 1,
                    _ => throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel),
                };
            }
            else
            {
                standAgeBudgetAvailableAtNextTimeStep = configuration.Variant.TreeModel switch
                {
                    TreeModel.OrganonSwo => 500 - (periodIndex + 1) * 5,
                    TreeModel.OrganonNwo or
                    TreeModel.OrganonSmc => 120 - (periodIndex + 1) * 5,
                    TreeModel.OrganonRap => 30 - (periodIndex + 1) * 1,
                    _ => throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel),
                };
            }

            if (standAgeBudgetAvailableAtNextTimeStep < 0)
            {
                stand.Warnings.TreesOld = true;
            }

            float B1 = -0.04484724F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                bool[]? heightWarnings = null;
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float B0;
                    switch (species)
                    {
                        case FiaCode.PseudotsugaMenziesii:
                            B0 = 19.04942539F;
                            break;
                        case FiaCode.TsugaHeterophylla:
                            if ((configuration.Variant.TreeModel == TreeModel.OrganonNwo) || (configuration.Variant.TreeModel == TreeModel.OrganonSmc))
                            {
                                B0 = 19.04942539F;
                            }
                            else if (configuration.Variant.TreeModel == TreeModel.OrganonRap)
                            {
                                B0 = 19.04942539F;
                            }
                            else
                            {
                                // BUGBUG Fortran code leaves B0 unitialized in Version.Swo case but also always treats hemlock as Douglas-fir
                                B0 = 19.04942539F;
                            }
                            break;
                        case FiaCode.AbiesConcolor:
                        case FiaCode.AbiesGrandis:
                            B0 = 16.26279948F;
                            break;
                        case FiaCode.PinusPonderosa:
                            B0 = 17.11482201F;
                            break;
                        case FiaCode.PinusLambertiana:
                            B0 = 14.29011403F;
                            break;
                        default:
                            B0 = 15.80319194F;
                            break;
                    }

                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    float potentialHeight = 4.5F + B0 * dbhInInches / (1.0F - B1 * dbhInInches);
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    if (heightInFeet > potentialHeight)
                    {
                        if (heightWarnings == null)
                        {
                            heightWarnings = stand.TreeHeightWarningBySpecies.GetOrAdd(species, treesOfSpecies.Capacity);
                        }
                        heightWarnings[treeIndex] = true;
                    }
                }
            }
        }
    }
}
