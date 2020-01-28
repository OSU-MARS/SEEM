using System;

namespace Osu.Cof.Organon
{
    public class OsuVolume
    {
        /// <summary>
        /// Find CVTS of tree per hectare.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Cubic volume including top and stump in m³/ha.</returns>
        public float GetCubicVolumePerHectare(Trees trees, int treeIndex)
        {
            FiaCode species = trees.Species[treeIndex];
            float dbhInCm = Constant.CmPerInch * trees.Dbh[treeIndex];
            if (dbhInCm < Constant.Minimum.DiameterForVolumeInCm)
            {
                return 0.0F;
            }
            float heightInM = Constant.MetersPerFoot * trees.Height[treeIndex];
            if (heightInM < Constant.Minimum.HeightForVolumeInM)
            {
                return 0.0F;
            }

            float cvtsPerTreeInCubicM;
            switch (species)
            {
                // Poudel K, Temesgen H, Gray AN. 2018. Estimating upper stem diameters and volume of Douglas-fir and Western hemlock
                //   trees in the Pacific northwest. Forest Ecosystems 5:16. https://doi.org/10.1186/s40663-018-0134-2
                case FiaCode.PseudotsugaMenziesii:
                    cvtsPerTreeInCubicM = (float)Math.Exp(-9.70405 + 1.61812 * Math.Log(dbhInCm) + 1.21071 * Math.Log(heightInM));
                    break;
                case FiaCode.TsugaHeterophylla:
                    cvtsPerTreeInCubicM = (float)Math.Exp(-9.98200 + 1.37228 * Math.Log(dbhInCm) + 1.57319 * Math.Log(heightInM));
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            }

            float treesPerHectare = Constant.HectaresPerAcre * trees.LiveExpansionFactor[treeIndex];
            return treesPerHectare * cvtsPerTreeInCubicM;
        }
    }
}
