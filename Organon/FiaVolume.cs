using System;

namespace Osu.Cof.Organon
{
    public class FiaVolume
    {
        /// <summary>
        /// Find CVTS of stand.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <returns>Cubic volume including top and stump in m³/ha.</returns>
        public float GetCubicVolumePerHectare(Trees trees)
        {
            float cvts = 0.0F;
            for (int treeIndex = 0; treeIndex < trees.TreeRecordCount; ++treeIndex)
            {
                cvts += this.GetCubicVolumePerHectare(trees, treeIndex);
            }
            return cvts;
        }

        /// <summary>
        /// Find CVTS of tree per hectare.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Cubic volume including top and stump in m³/ha.</returns>
        public float GetCubicVolumePerHectare(Trees trees, int treeIndex)
        {
            FiaCode species = trees.Species[treeIndex];
            float dbhInInches = trees.Dbh[treeIndex];
            float dbhInCm = Constant.CmPerInch * dbhInInches;
            if (dbhInCm < Constant.Minimum.DiameterForVolumeInCm)
            {
                return 0.0F;
            }
            float heightInFeet = trees.Height[treeIndex];
            float heightInM = Constant.MetersPerFoot * heightInFeet;
            if (heightInM < Constant.Minimum.HeightForVolumeInM)
            {
                return 0.0F;
            }

            double logDbhInInches = Math.Log10(dbhInInches);
            double logHeightInFeet = Math.Log10(heightInFeet);
            double cvtsl = species switch
            {
                // Waddell K, Campbell K, Kuegler O, Christensen G. 2014. FIA Volume Equation documentation updated on 9-19-2014:
                //   Volume estimation for PNW Databases NIMS and FIADB. https://ww3.arb.ca.gov/cc/capandtrade/offsets/copupdatereferences/qm_volume_equations_pnw_updated_091914.pdf
                // Equation 1: western Oregon and Washington (Brackett 1973)
                FiaCode.PseudotsugaMenziesii => -3.21809 + 0.04948 * logHeightInFeet * logDbhInInches - 0.15664 * logDbhInInches * logDbhInInches +
                                                 2.02132 * logDbhInInches + 1.63408 * logHeightInFeet - 0.16184 * logHeightInFeet * logHeightInFeet,
                // FIA Equation 6: all of Oregon and California (Chambers 1979)
                FiaCode.TsugaHeterophylla => -2.72170 + 2.00857 * logDbhInInches + 1.08620 * logHeightInFeet - 0.00568 * dbhInInches,
                _ => throw OrganonVariant.CreateUnhandledSpeciesException(species)
            };
            float treeCvtsInCubicM = Constant.CubicMetersPerCubicFoot * (float)Math.Pow(10.0, cvtsl);

            float treesPerHectare = Constant.HectaresPerAcre * trees.LiveExpansionFactor[treeIndex];
            return treesPerHectare * treeCvtsInCubicM;
        }

        /// <summary>
        /// Get cubic volume to a four inch top.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <returns>Cubic volume including top and stump in m³/ha.</returns>
        public float GetMerchantableCubicVolumePerHectare(Trees trees)
        {
            float cvts4 = 0.0F;
            for (int treeIndex = 0; treeIndex < trees.TreeRecordCount; ++treeIndex)
            {
                cvts4 += this.GetMerchantableCubicVolumePerHectare(trees, treeIndex);
            }
            return cvts4;
        }

        /// <summary>
        /// Get cubic volume to a four inch top.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Cubic volume in m³/ha.</returns>
        public float GetMerchantableCubicVolumePerHectare(Trees trees, int treeIndex)
        {
            float dbhInInches = trees.Dbh[treeIndex];
            if (dbhInInches < 4.0F)
            {
                // CV4 regression goes negative, unsurprisingly, for trees less than four inches in diameter
                return 0.0F;
            }

            float cvtsInCubicMeters = this.GetCubicVolumePerHectare(trees, treeIndex);
            if (cvtsInCubicMeters == 0.0F)
            {
                return 0.0F;
            }

            FiaCode species = trees.Species[treeIndex]; 
            float basalAreaInSquareFeet = Constant.ForestersEnglish * dbhInInches * dbhInInches;
            switch (species)
            {
                // Waddell K, Campbell K, Kuegler O, Christensen G. 2014. FIA Volume Equation documentation updated on 9-19-2014:
                //   Volume estimation for PNW Databases NIMS and FIADB. https://ww3.arb.ca.gov/cc/capandtrade/offsets/copupdatereferences/qm_volume_equations_pnw_updated_091914.pdf
                // Douglas-fir and western hemlock use the same tarif and CV4 regressions
                // FIA Equation 1: western Oregon and Washington (Brackett 1973)
                // FIA Equation 6: all of Oregon and California (Chambers 1979)
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.TsugaHeterophylla:
                    double tarif = 0.912733 * cvtsInCubicMeters / (1.033 * (1.0 + 1.382937 * Math.Exp(-4.015292 * dbhInInches / 10.0)) * (basalAreaInSquareFeet + 0.087266) - 0.174533);
                    return (float)(tarif * (basalAreaInSquareFeet - 0.087266) / 0.912733);
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
        }
    }
}
