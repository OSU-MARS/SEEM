using Osu.Cof.Ferm.Organon;

namespace Osu.Cof.Ferm
{
    public class OsuVolume
    {
        /// <summary>
        /// Find CVTS of tree per hectare.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Cubic volume including top and stump in m³/ha.</returns>
        public float GetCubicVolume(Trees trees, int treeIndex)
        {
            FiaCode species = trees.Species[treeIndex];
            float dbhInCm = Constant.CmPerInch * trees.Dbh[treeIndex];
            if (dbhInCm < Constant.Minimum.DiameterForVolumeInInches)
            {
                return 0.0F;
            }
            float heightInM = Constant.MetersPerFoot * trees.Height[treeIndex];
            if (heightInM < Constant.Minimum.HeightForVolumeInFeet)
            {
                return 0.0F;
            }
            float cvtsPerTreeInCubicM = species switch
            {
                // Poudel K, Temesgen H, Gray AN. 2018. Estimating upper stem diameters and volume of Douglas-fir and Western hemlock
                //   trees in the Pacific northwest. Forest Ecosystems 5:16. https://doi.org/10.1186/s40663-018-0134-2
                // Table 8
                FiaCode.PseudotsugaMenziesii => MathV.Exp(-9.70405F + 1.61812F * MathV.Ln(dbhInCm) + 1.21071F * MathV.Ln(heightInM)),
                FiaCode.TsugaHeterophylla => MathV.Exp(-9.98200F + 1.37228F * MathV.Ln(dbhInCm) + 1.57319F * MathV.Ln(heightInM)),
                _ => throw OrganonVariant.CreateUnhandledSpeciesException(species),
            };
            return cvtsPerTreeInCubicM;
        }
    }
}
