using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class PoudelRegressions
    {
        /// <summary>
        /// Find cubic volume of tree per hectare.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Cubic volume including top and stump in m³/ha.</returns>
        public static float GetCubicVolume(Trees trees, int treeIndex)
        {
            float dbhInCm = trees.Dbh[treeIndex];
            float heightInM = trees.Height[treeIndex];
            float expansionFactor = trees.LiveExpansionFactor[treeIndex];
            if (trees.Units == Units.English)
            {
                dbhInCm *= Constant.CentimetersPerInch;
                heightInM *= Constant.MetersPerFoot;
                expansionFactor *= Constant.AcresPerHectare;
            }
            if (dbhInCm <= 0.0F)
            {
                Debug.Assert(dbhInCm == 0.0F);
                return 0.0F;
            }

            float cvtsPerTreeInCubicM = trees.Species switch
            {
                // Poudel K, Temesgen H, Gray AN. 2018. Estimating upper stem diameters and volume of Douglas-fir and Western hemlock
                //   trees in the Pacific northwest. Forest Ecosystems 5:16. https://doi.org/10.1186/s40663-018-0134-2
                // Table 8
                FiaCode.PseudotsugaMenziesii => MathV.Exp(-9.70405F + 1.61812F * MathV.Ln(dbhInCm) + 1.21071F * MathV.Ln(heightInM)),
                FiaCode.TsugaHeterophylla => MathV.Exp(-9.98200F + 1.37228F * MathV.Ln(dbhInCm) + 1.57319F * MathV.Ln(heightInM)),
                _ => throw Trees.CreateUnhandledSpeciesException(trees.Species),
            };
            return expansionFactor * cvtsPerTreeInCubicM;
        }

        /// <summary>
        /// Find total per hectare biomass of trees.
        /// </summary>
        /// <param name="treesOfSpecies">Trees.</param>
        /// <returns>Live biomass of trees in kg/ha.</returns>
        public static float GetLiveBiomass(Trees treesOfSpecies)
        {
            float correctionFactor;
            float a;
            float b;
            float c;
            switch (treesOfSpecies.Species)
            {
                case FiaCode.AlnusRubra:
                    correctionFactor = 1.0281F;
                    a = -3.8516F;
                    b = 2.3174F;
                    c = 0.6283F;
                    break;
                case FiaCode.PiceaSitchensis:
                    correctionFactor = 1.0091F;
                    a = -2.5826F;
                    b = 1.8196F;
                    c = 0.7108F;
                    break;
                case FiaCode.PseudotsugaMenziesii:
                    correctionFactor = 1.0000F;
                    a = -2.8246F;
                    b = 1.6385F;
                    c = 1.0474F;
                    break;
                case FiaCode.ThujaPlicata:
                    correctionFactor = 1.0151F;
                    a = -2.5443F;
                    b = 1.5701F;
                    c = 0.9162F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    correctionFactor = 1.0209F;
                    a = -2.7552F;
                    b = 1.8521F;
                    c = 0.7642F;
                    break;
                default:
                    throw new NotSupportedException("Unhandled tree species " + treesOfSpecies.Species + ".");
            }

            float liveBiomassInKgPerHa = 0.0F;
            for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
            {
                float dbh = treesOfSpecies.Dbh[compactedTreeIndex];
                float height = treesOfSpecies.Height[compactedTreeIndex];
                float expansionFactor = treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                if (treesOfSpecies.Units == Units.English)
                {
                    dbh *= Constant.CentimetersPerInch;
                    height *= Constant.MetersPerFoot;
                    expansionFactor *= Constant.AcresPerHectare;
                }

                float treeBiomassInKgPerHa = expansionFactor * correctionFactor * MathV.Exp(a + b * MathV.Ln(dbh) + c * MathV.Ln(height));
                liveBiomassInKgPerHa += treeBiomassInKgPerHa;
            }

            return liveBiomassInKgPerHa;
        }
    }
}
