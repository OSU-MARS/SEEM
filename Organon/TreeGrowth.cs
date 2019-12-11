using System;
using System.Collections.Generic;

namespace Osu.Cof.Organon
{
    internal class TreeGrowth
    {
        public static float GetCrownRatioAdjustment(float crownRatio)
        {
            return 1.0F - (float)Math.Exp(-(25.0 * 25.0 * crownRatio * crownRatio));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulationStep"></param>
        /// <param name="configuration">Organon configuration settings.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="TCYCLE"></param>
        /// <param name="FCYCLE"></param>
        /// <param name="CALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YT"></param>
        /// <param name="CCH"></param>
        /// <param name="OLD"></param>
        /// <param name="RAAGE"></param>
        public static void GROW(ref int simulationStep, OrganonConfiguration configuration, Stand stand,
                                ref int TCYCLE, ref int FCYCLE, StandDensity densityBeforeGrowth,
                                Dictionary<FiaCode, float[]> CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, 
                                float[] CCH, ref float OLD, float RAAGE, out StandDensity densityAfterGrowth)
        {
            float DGMOD_GG = 1.0F;
            float HGMOD_GG = 1.0F;
            float DGMOD_SNC = 1.0F;
            float HGMOD_SNC = 1.0F;
            if ((stand.AgeInYears > 0) && configuration.Genetics)
            {
                GrowthModifiers.GG_MODS((float)stand.AgeInYears, configuration.GWDG, configuration.GWHG, out DGMOD_GG, out HGMOD_GG);
            }
            if (configuration.SwissNeedleCast && (configuration.Variant.Variant == Variant.Nwo || configuration.Variant.Variant == Variant.Smc))
            {
                GrowthModifiers.SNC_MODS(configuration.FR, out DGMOD_SNC, out HGMOD_SNC);
            }

            // diameter growth
            int treeRecordsWithExpansionFactorZero = 0;
            int bigSixRecordsWithExpansionFactorZero = 0;
            int otherSpeciesRecordsWithExpansionFactorZero = 0;
            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    ++treeRecordsWithExpansionFactorZero;
                    if (configuration.Variant.IsBigSixSpecies(stand.Species[treeIndex]))
                    {
                        ++bigSixRecordsWithExpansionFactorZero;
                    }
                    else
                    {
                        ++otherSpeciesRecordsWithExpansionFactorZero;
                    }
                }
            }

            // BUGBUG no check that SITE_1 and SITE_2 indices are greater than 4.5 feet
            float SI_1 = stand.SiteIndex - 4.5F;
            float SI_2 = stand.HemlockSiteIndex - 4.5F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    stand.DbhGrowth[treeIndex] = 0.0F;
                }
                else
                {
                    DiameterGrowth.DIAMGRO(configuration.Variant, treeIndex, simulationStep, stand, SI_1, SI_2, densityBeforeGrowth, CALIB, PN, YF, BABT, BART, YT);
                    if (stand.Species[treeIndex] == FiaCode.PseudotsugaMenziesii)
                    {
                        stand.DbhGrowth[treeIndex] = stand.DbhGrowth[treeIndex] * DGMOD_GG * DGMOD_SNC;
                    }
                }
            }

            // height growth for big six species
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (configuration.Variant.IsBigSixSpecies(stand.Species[treeIndex]))
                {
                    if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        stand.HeightGrowth[treeIndex] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.GrowBigSixSpecies(treeIndex, configuration.Variant, simulationStep, stand, SI_1, SI_2, CCH, PN, YF, BABT, BART, YT, ref OLD, configuration.PDEN);
                        FiaCode species = stand.Species[treeIndex];
                        if (species == FiaCode.PseudotsugaMenziesii)
                        {
                            stand.HeightGrowth[treeIndex] = stand.HeightGrowth[treeIndex] * HGMOD_GG * HGMOD_SNC;
                        }
                    }
                }
            }

            // determine mortality
            // Sets configuration.NO.
            Mortality.MORTAL(configuration, simulationStep, stand, densityBeforeGrowth, SI_1, SI_2, PN, YF, ref RAAGE);

            // grow tree diameters
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                stand.Dbh[treeIndex] += stand.DbhGrowth[treeIndex];
            }

            // CALC EOG SBA, CCF/TREE, CCF IN LARGER TREES AND STAND CCF
            densityAfterGrowth = new StandDensity(stand, configuration.Variant);

            // CALCULATE HTGRO FOR 'OTHER' & CROWN ALL SPECIES
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (configuration.Variant.IsBigSixSpecies(stand.Species[treeIndex]) == false)
                {
                    if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        stand.HeightGrowth[treeIndex] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.GrowMinorSpecies(treeIndex, configuration.Variant, stand, CALIB);
                    }
                }
                stand.Height[treeIndex] += stand.HeightGrowth[treeIndex];
            }

            // grow crowns
            CrownGrowth.CrowGro(configuration.Variant, stand, densityBeforeGrowth, densityAfterGrowth, SI_1, SI_2, CALIB, CCH);

            // update stand variables
            if (configuration.Variant.Variant != Variant.Rap)
            {
                stand.AgeInYears += 5;
                stand.BreastHeightAgeInYears += 5;
            }
            else
            {
                ++stand.AgeInYears;
                ++stand.BreastHeightAgeInYears;
            }
            ++simulationStep;
            if (FCYCLE > 2)
            {
                FCYCLE = 0;
            }
            else if (FCYCLE > 0)
            {
                ++FCYCLE;
            }
            if (TCYCLE > 0)
            {
                ++TCYCLE;
            }

            // reduce calibration ratios
            foreach (float[] speciesCalibration in CALIB.Values)
            {
                for (int index = 0; index < 3; ++index)
                {
                    if (speciesCalibration[index] != 1.0F)
                    {
                        float MCALIB = (1.0F + speciesCalibration[index + 2]) / 2.0F;
                        speciesCalibration[index] = MCALIB + (float)Math.Sqrt(0.5) * (speciesCalibration[index] - MCALIB);
                    }
                }
            }
        }
    }
}
