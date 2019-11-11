using System;

namespace Osu.Cof.Organon
{
    internal class TreeGrowth
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulationStep"></param>
        /// <param name="configuration">Organon configuration settings.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="NSPN"></param>
        /// <param name="TCYCLE"></param>
        /// <param name="FCYCLE"></param>
        /// <param name="SBA1"></param>
        /// <param name="BALL1"></param>
        /// <param name="BAL1"></param>
        /// <param name="CALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YT"></param>
        /// <param name="CCH"></param>
        /// <param name="OLD"></param>
        /// <param name="RAAGE"></param>
        /// <param name="RASI"></param>
        /// <param name="CCFLL1"></param>
        /// <param name="CCFL1"></param>
        /// <param name="CCFLL2"></param>
        /// <param name="CCFL2"></param>
        /// <param name="BALL2"></param>
        /// <param name="BAL2"></param>
        public static void GROW(ref int simulationStep, OrganonConfiguration configuration, Stand stand, int NSPN,
                                ref int TCYCLE, ref int FCYCLE, float SBA1, float[] BALL1, float[] BAL1,
                                float[,] CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, 
                                float[] CCH, ref float OLD, float RAAGE, float RASI, float[] CCFLL1, float[] CCFL1,
                                float[] CCFLL2, float[] CCFL2, float[] BALL2, float[] BAL2)
        {
            float DGMOD_GG = 1.0F;
            float HGMOD_GG = 1.0F;
            float DGMOD_SNC = 1.0F;
            float HGMOD_SNC = 1.0F;
            if ((stand.AgeInYears > 0) && configuration.Genetics)
            {
                GrowthModifiers.GG_MODS((float)stand.AgeInYears, configuration.GWDG, configuration.GWHG, out DGMOD_GG, out HGMOD_GG);
            }
            if (configuration.SwissNeedleCast && (configuration.Variant == Variant.Nwo || configuration.Variant == Variant.Smc))
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
                    if (stand.IsBigSixSpecies(treeIndex))
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
            float SI_1 = stand.PrimarySiteIndex - 4.5F;
            float SI_2 = stand.MortalitySiteIndex - 4.5F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    stand.DbhGrowth[treeIndex] = 0.0F;
                }
                else
                {
                    DiameterGrowth.DIAMGRO(configuration.Variant, treeIndex, simulationStep, stand, SI_1, SI_2, SBA1, BALL1, BAL1, CALIB, PN, YF, BABT, BART, YT);
                    if (stand.Species[treeIndex] == FiaCode.PseudotsugaMenziesii)
                    {
                        stand.DbhGrowth[treeIndex] = stand.DbhGrowth[treeIndex] * DGMOD_GG * DGMOD_SNC;
                    }
                }
            }

            // height growth for big six species
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.IsBigSixSpecies(treeIndex))
                {
                    if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        stand.HeightGrowth[treeIndex] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.HTGRO1(treeIndex, configuration.Variant, simulationStep, stand, SI_1, SI_2, CCH, PN, YF, BABT, BART, YT, ref OLD, configuration.PDEN);
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
            Mortality.MORTAL(configuration, simulationStep, stand, BALL1, BAL1, SI_1, SI_2, PN, YF, ref RAAGE);

            // grow tree diameters
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                stand.Dbh[treeIndex] += stand.DbhGrowth[treeIndex];
            }

            // CALC EOG SBA, CCF/TREE, CCF IN LARGER TREES AND STAND CCF
            Stats.SSTATS(configuration.Variant, stand, out float SBA2, out float _, out float _, BAL2, BALL2, CCFL2, CCFLL2);

            // CALCULATE HTGRO FOR 'OTHER' & CROWN ALL SPECIES
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.IsBigSixSpecies(treeIndex) == false)
                {
                    if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        stand.HeightGrowth[treeIndex] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.HTGRO2(treeIndex, configuration.Variant, stand, RASI, CALIB);
                    }
                }
                stand.Height[treeIndex] += stand.HeightGrowth[treeIndex];
            }

            // grow crowns
            CrownGrowth.CrowGro(configuration.Variant, stand, CCFLL1, CCFL1, CCFLL2, CCFL2, SBA1, SBA2, SI_1, SI_2, CALIB, CCH);

            // update stand variables
            if (configuration.Variant != Variant.Rap)
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
            for (int I = 0; I < 3; ++I)
            {
                for (int II = 0; II < NSPN; ++II)
                {
                    if (CALIB[II, I] < 1.0F || CALIB[II, I] > 1.0F)
                    {
                        float MCALIB = (1.0F + CALIB[II, I + 2]) / 2.0F;
                        CALIB[II, I] = MCALIB + (float)Math.Sqrt(0.5) * (CALIB[II, I] - MCALIB);
                    }
                }
            }
        }
    }
}
