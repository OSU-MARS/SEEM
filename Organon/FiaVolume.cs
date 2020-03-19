using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    public class FiaVolume
    {
        /// <summary>
        /// Find CVTS of tree.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Cubic volume including top and stump in ft³/ac.</returns>
        public double GetCubicFeet(Trees trees, int treeIndex)
        {
            FiaCode species = trees.Species[treeIndex];
            float dbhInInches = trees.Dbh[treeIndex];
            if (dbhInInches < Constant.Minimum.DiameterForVolumeInInches)
            {
                return 0.0;
            }
            float heightInFeet = trees.Height[treeIndex];
            if (heightInFeet < Constant.Minimum.HeightForVolumeInFeet)
            {
                return 0.0;
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

            return Math.Pow(10.0, cvtsl);
        }

        /// <summary>
        /// Get cubic volume to a 10 centimeter (four inch) top.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Cubic volume in m³/ha.</returns>
        public double GetMerchantableCubicFeet(Trees trees, int treeIndex)
        {
            float dbhInInches = trees.Dbh[treeIndex];
            if (dbhInInches < 4.0F)
            {
                // CV4 regression goes negative, unsurprisingly, for trees less than four inches in diameter
                return 0.0;
            }

            double cvts = this.GetCubicFeet(trees, treeIndex);
            if (cvts <= 0.0)
            {
                return 0.0F;
            }

            FiaCode species = trees.Species[treeIndex];
            double basalAreaInSquareFeet = Constant.ForestersEnglish * dbhInInches * dbhInInches;
            double tarif;
            switch (species)
            {
                // Waddell K, Campbell K, Kuegler O, Christensen G. 2014. FIA Volume Equation documentation updated on 9-19-2014:
                //   Volume estimation for PNW Databases NIMS and FIADB. https://ww3.arb.ca.gov/cc/capandtrade/offsets/copupdatereferences/qm_volume_equations_pnw_updated_091914.pdf
                // Douglas-fir and western hemlock use the same tarif and CV4 regressions
                // FIA Equation 1: western Oregon and Washington (Brackett 1973)
                // FIA Equation 6: all of Oregon and California (Chambers 1979)
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.TsugaHeterophylla:
                    tarif = 0.912733 * cvts / (1.033 * (1.0 + 1.382937 * Math.Exp(-4.015292 * dbhInInches / 10.0)) * (basalAreaInSquareFeet + 0.087266) - 0.174533);
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            return tarif * (basalAreaInSquareFeet - 0.087266) / 0.912733;
        }

        /// <summary>
        /// Get Scribner board foot volume for 32 foot logs to a six inch top.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Scribner board feet per acre</returns>
        public double GetScribnerBoardFeet(Trees trees, int treeIndex)
        {
            float dbhInInches = trees.Dbh[treeIndex];
            if (dbhInInches < 6.0F)
            {
                return 0.0;
            }
            double cubicFeet = this.GetCubicFeet(trees, treeIndex);
            if (cubicFeet <= 0.0)
            {
                return 0.0;
            }
            
            FiaCode species = trees.Species[treeIndex];
            double basalAreaInSquareFeet = Constant.ForestersEnglish * dbhInInches * dbhInInches;
            double tarif;
            switch (species)
            {
                // Waddell K, Campbell K, Kuegler O, Christensen G. 2014. FIA Volume Equation documentation updated on 9-19-2014:
                //   Volume estimation for PNW Databases NIMS and FIADB. https://ww3.arb.ca.gov/cc/capandtrade/offsets/copupdatereferences/qm_volume_equations_pnw_updated_091914.pdf
                // Douglas-fir and western hemlock use the same tarif and CV4 regressions
                // FIA Equation 1: western Oregon and Washington (Brackett 1973)
                // FIA Equation 6: all of Oregon and California (Chambers 1979)
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.TsugaHeterophylla:
                    tarif = 0.912733 * cubicFeet / (1.033 * (1.0 + 1.382937 * Math.Exp(-4.015292 * dbhInInches / 10.0)) * (basalAreaInSquareFeet + 0.087266) - 0.174533);
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }
            double cv4 = tarif * (basalAreaInSquareFeet - 0.087266) / 0.912733;

            // conversion to Scribner volumes for 32 foot trees
            // Waddell 2014:32
            double rc6 = 0.993 * (1.0 - Math.Pow(0.62, dbhInInches - 6.0));
            double cv6 = rc6 * cv4;
            double b4 = tarif / 0.912733;
            double logB4 = Math.Log10(b4);
            double rs616 = Math.Pow(10.0, 0.174439 + 0.117594 * Math.Log10(dbhInInches) * logB4 - 8.210585 / (dbhInInches * dbhInInches) + 0.236693 * logB4 - 0.00001345 * b4 * b4 - 0.00001937 * dbhInInches * dbhInInches);
            double sv616 = rs616 * cv6; // Scribner board foot volume to a 6 inch top for 16 foot logs
            double rs632 = 1.001491 - 6.924097 / tarif + 0.00001351 * dbhInInches * dbhInInches;
            double sv632 = rs632 * sv616; // Scribner board foot volume to a 6 inch top for 32 foot logs

            Debug.Assert(rc6 >= 0.0);
            Debug.Assert(rc6 <= 1.0);
            Debug.Assert(rs616 >= 1.0);
            Debug.Assert(rs616 <= 6.8);
            Debug.Assert(rs632 >= 0.0);
            Debug.Assert(rs632 <= 1.0);
            Debug.Assert(sv632 >= 0.0);
            Debug.Assert(sv632 <= 10.0 * 1000.0);
            return sv632;
        }
    }
}
