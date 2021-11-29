using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System.Diagnostics;

namespace Mars.Seem.Organon
{
    internal class OrganonMortality
    {
        private static float GetMortalityFertilizationAdjustment(OrganonVariant variant, FiaCode species, OrganonTreatments treatments)
        {
            // fertilization mortality effects currently supported only for non-RAP Douglas-fir
            if ((species != FiaCode.PseudotsugaMenziesii) || (variant.TreeModel == TreeModel.OrganonRap))
            {
                return 0.0F;
            }

            // non-RAP Douglas-fir
            // Hann 2003 Research Contribution 40, Table 37: Parameters for predicting fertilization response of 5-year mortality
            float c5 = 0.0000552859F;
            float PF2 = 1.5F;
            float PF3 = -0.5F;

            float fertX1 = treatments.GetFertX1(variant, PF3 / PF2, out float mostRecentFertilization, out int yearsSinceMostRecentFertilization);
            if (mostRecentFertilization == 0.0F)
            {
                return 0.0F;
            }

            float fertilizationMultiplier = c5 * MathV.Pow(mostRecentFertilization + fertX1, PF2) * MathV.Exp(PF3 * yearsSinceMostRecentFertilization);
            return fertilizationMultiplier;
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

                    float liveExpansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    Debug.Assert(liveExpansionFactor >= 0.0F);
                    Debug.Assert(liveExpansionFactor <= Constant.Maximum.ExpansionFactorPerAcre);

                    treesPerAcreByDiameterClass[dbhIndex] += liveExpansionFactor;
                    weightedDbhByDiameterClass[dbhIndex] += liveExpansionFactor * dbhInInches;
                    weightedHeightByDiameterClass[dbhIndex] += liveExpansionFactor * treesOfSpecies.Height[treeIndex];
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

        public static void ReduceExpansionFactors(OrganonConfiguration configuration, OrganonTreatments treatments, OrganonStand stand, OrganonStandDensity densityBeforeGrowth)
        {
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                float fertilizationExponent = OrganonMortality.GetMortalityFertilizationAdjustment(configuration.Variant, species, treatments);
                configuration.Variant.ReduceExpansionFactors(stand, densityBeforeGrowth, treesOfSpecies, fertilizationExponent);
            }
        }
    }
}
