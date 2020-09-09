using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    internal class OrganonMortality
    {
        private static float GetMortalityFertilizationAdjustment(FiaCode species, TreeModel treeModel, int simulationStep, OrganonTreatments treatments)
        {
            // fertilization mortality effects currently supported only for non-RAP Douglas-fir
            if ((treatments.FertilizationsPerformed < 1) || (species != FiaCode.PseudotsugaMenziesii) || (treeModel == TreeModel.OrganonRap))
            {
                return 0.0F;
            }

            // non-RAP Douglas-fir
            // Hann 2003 Research Contribution 40, Table 37: Parameters for predicting fertlization response of 5-year mortality
            float c5 = 0.0000552859F;
            float PF2 = 1.5F;
            float PF3 = -0.5F;

            float XTIME = Constant.DefaultTimeStepInYears * (float)simulationStep;
            float FERTX1 = 0.0F;
            for (int treatmentIndex = 1; treatmentIndex < 5; ++treatmentIndex)
            {
                // BUGBUG: summation range doesn't match 13 or 18 year periods given in Hann 2003 Table 3
                FERTX1 += treatments.PoundsOfNitrogenPerAcre[treatmentIndex] * MathV.Exp(PF3 / PF2 * (treatments.TimeStepsSinceFertilization[0] - treatments.TimeStepsSinceFertilization[treatmentIndex]));
            }
            float FERTADJ = c5 * MathV.Pow(treatments.PoundsOfNitrogenPerAcre[0] + FERTX1, PF2) * MathV.Exp(PF3 * (XTIME - treatments.TimeStepsSinceFertilization[0]));
            return FERTADJ;
        }

        /// <summary>
        /// Computes old growth index?
        /// </summary>
        /// <param name="stand">Stand data.</param>
        /// <param name="incrementMultiplier">Zero or minus one, usually zero.</param>
        /// <param name="OG">Old growth indicator.</param>
        /// <remarks>
        /// Supports only to 99 inch DBH.
        /// </remarks>
        public static float GetOldGrowthIndicator(OrganonVariant variant, OrganonStand stand)
        {
            // TODO: move diameter class and stand density information into stand for simplified state management and reduced compute
            int diameterClasses = 100;
            float[] treesPerAcreByDiameterClass = new float[diameterClasses];
            float[] weightedDbhByDiameterClass = new float[diameterClasses];
            float[] weightedHeightByDiameterClass = new float[diameterClasses];
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                if (variant.IsBigSixSpecies(treesOfSpecies.Species) == false)
                {
                    continue;
                }
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    int dbhIndex = (int)dbhInInches;
                    if (dbhIndex >= treesPerAcreByDiameterClass.Length)
                    {
                        dbhIndex = treesPerAcreByDiameterClass.Length - 1;
                    }

                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    Debug.Assert(expansionFactor >= 0.0F);
                    Debug.Assert(expansionFactor <= Constant.Maximum.ExpansionFactor);

                    treesPerAcreByDiameterClass[dbhIndex] += expansionFactor;
                    weightedDbhByDiameterClass[dbhIndex] += expansionFactor * dbhInInches;
                    weightedHeightByDiameterClass[dbhIndex] += expansionFactor * treesOfSpecies.Height[treeIndex];
                }
            }

            float largestFiveTpaWeightedHeight = 0.0F;
            float largestFiveTpaWeightedDbh = 0.0F;
            float largeTreeTpa = 0.0F;
            for (int dbhIndex = weightedHeightByDiameterClass.Length - 1; dbhIndex >= 0; --dbhIndex)
            {
                largestFiveTpaWeightedHeight += weightedHeightByDiameterClass[dbhIndex];
                largestFiveTpaWeightedDbh += weightedDbhByDiameterClass[dbhIndex];
                largeTreeTpa += treesPerAcreByDiameterClass[dbhIndex];
                if (largeTreeTpa > 5.0F)
                {
                    float TRDIFF = treesPerAcreByDiameterClass[dbhIndex] - (largeTreeTpa - 5.0F);
                    largestFiveTpaWeightedHeight = largestFiveTpaWeightedHeight - weightedHeightByDiameterClass[dbhIndex] + ((weightedHeightByDiameterClass[dbhIndex] / treesPerAcreByDiameterClass[dbhIndex]) * TRDIFF);
                    largestFiveTpaWeightedDbh = largestFiveTpaWeightedDbh - weightedDbhByDiameterClass[dbhIndex] + ((weightedDbhByDiameterClass[dbhIndex] / treesPerAcreByDiameterClass[dbhIndex]) * TRDIFF);
                    largeTreeTpa = 5.0F;
                    break;
                }
            }

            float oldGrowthIndicator = 0.0F;
            if (largeTreeTpa > 0.0F)
            {
                float HT5 = largestFiveTpaWeightedHeight / largeTreeTpa;
                float DBH5 = largestFiveTpaWeightedDbh / largeTreeTpa;
                oldGrowthIndicator = DBH5 * HT5 / 10000.0F;
                Debug.Assert(oldGrowthIndicator >= 0.0F);
                Debug.Assert(oldGrowthIndicator <= 1000.0F);
            }
            return oldGrowthIndicator;
        }

        public static void ReduceExpansionFactors(OrganonConfiguration configuration, int simulationStep, OrganonStand stand, OrganonStandDensity densityBeforeGrowth)
        {
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                float fertilizationExponent = OrganonMortality.GetMortalityFertilizationAdjustment(species, configuration.Variant.TreeModel, simulationStep, configuration.Treatments);
                configuration.Variant.ReduceExpansionFactors(stand, densityBeforeGrowth, treesOfSpecies, fertilizationExponent);
            }
        }
    }
}
