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
        /// <param name="TDATAR">Tree data for simulation step.</param>
        /// <param name="POST"></param>
        /// <param name="NSPN"></param>
        /// <param name="TCYCLE"></param>
        /// <param name="FCYCLE"></param>
        /// <param name="SI_1"></param>
        /// <param name="SI_2"></param>
        /// <param name="SBA1"></param>
        /// <param name="BALL1"></param>
        /// <param name="BAL1"></param>
        /// <param name="CALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YT"></param>
        /// <param name="GROWTH"></param>
        /// <param name="SCR"></param>
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
        public static void GROW(ref int simulationStep, OrganonConfiguration configuration, Stand stand, float[,] TDATAR, float[] DEADEXP, bool POST, int NSPN,
                                ref int TCYCLE, ref int FCYCLE, float SI_1, float SI_2, float SBA1, float[] BALL1, float[] BAL1,
                                float[,] CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, float[,] GROWTH,
                                float[,] SCR, float[] CCH, ref float OLD, 
                                float RAAGE, float RASI, float[] CCFLL1, float[] CCFL1,
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
            for (int treeIndex = 1; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                if (TDATAR[treeIndex, 3] <= 0.0F)
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

            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                if (TDATAR[treeIndex, 3] <= 0.0F)
                {
                    GROWTH[treeIndex, 1] = 0.0F;
                }
                else
                {
                    DiameterGrowth.DIAMGRO(configuration.Variant, treeIndex, simulationStep, stand.Integer, TDATAR, SI_1, SI_2, SBA1, BALL1, BAL1, CALIB, PN, YF, BABT, BART, YT, GROWTH);
                    if (stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species] == (int)FiaCode.PseudotsugaMenziesii)
                    {
                        GROWTH[treeIndex, 1] = GROWTH[treeIndex, 1] * DGMOD_GG * DGMOD_SNC;
                    }
                }
            }

            // height growth for big six species
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                if (stand.IsBigSixSpecies(treeIndex))
                {
                    if (TDATAR[treeIndex, 3] <= 0.0F)
                    {
                        GROWTH[treeIndex, 0] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.HTGRO1(treeIndex, configuration.Variant, simulationStep, stand, TDATAR, SI_1, SI_2, CCH, PN, YF, BABT, BART, YT, ref OLD, configuration.PDEN, GROWTH);
                        FiaCode species = (FiaCode)stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species];
                        if (species == FiaCode.PseudotsugaMenziesii)
                        {
                            GROWTH[treeIndex, 0] = GROWTH[treeIndex, 0] * HGMOD_GG * HGMOD_SNC;
                        }
                    }
                }
            }

            // determine mortality
            // Sets configuration.PA1MAX and NO.
            Mortality.MORTAL(configuration, simulationStep, stand, POST, TDATAR, SCR, GROWTH, stand.MGExpansionFactor, DEADEXP, BALL1, BAL1, SI_1, SI_2, PN, YF, ref RAAGE);

            // grow tree diameters
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                TDATAR[treeIndex, 0] = TDATAR[treeIndex, 0] + GROWTH[treeIndex, 1];
            }

            // CALC EOG SBA, CCF/TREE, CCF IN LARGER TREES AND STAND CCF
            Stats.SSTATS(configuration.Variant, stand, TDATAR, out float SBA2, out float _, out float _, BAL2, BALL2, CCFL2, CCFLL2);

            // CALCULATE HTGRO FOR 'OTHER' & CROWN ALL SPECIES
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                if (stand.IsBigSixSpecies(treeIndex) == false)
                {
                    if (TDATAR[treeIndex, 3] <= 0.0F)
                    {
                        GROWTH[treeIndex, 0] = 0.0F;
                    }
                    else
                    {
                        HeightGrowth.HTGRO2(treeIndex, configuration.Variant, stand, TDATAR, RASI, CALIB, GROWTH);
                    }
                }
                TDATAR[treeIndex, 1] = TDATAR[treeIndex, 1] + GROWTH[treeIndex, 0];
            }

            // grow growns
            CrownGrowth.CrowGro(configuration.Variant, simulationStep, stand, TDATAR, SCR, GROWTH, stand.MGExpansionFactor, DEADEXP, CCFLL1, CCFL1, CCFLL2, CCFL2, SBA1, SBA2, SI_1, SI_2, CALIB, CCH);

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
