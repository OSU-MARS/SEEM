﻿using Mars.Seem.Extensions;
using System;
using System.Diagnostics;

namespace Mars.Seem.Tree
{
    public class PoudelRegressions
    {
        public const float MinimumKozakHeightInM = 1.3F;

        /// <summary>
        /// Find cubic volume of tree per hectare.
        /// </summary>
        /// <param name="treesOfSpecies">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Cubic volume including top and stump in m³/ha.</returns>
        public static float GetCubicVolume(Trees treesOfSpecies, int treeIndex)
        {
            float dbhInCm = treesOfSpecies.Dbh[treeIndex];
            float heightInM = treesOfSpecies.Height[treeIndex];
            float expansionFactorPerHa = treesOfSpecies.LiveExpansionFactor[treeIndex];
            if (treesOfSpecies.Units == Units.English)
            {
                dbhInCm *= Constant.CentimetersPerInch;
                heightInM *= Constant.MetersPerFoot;
                expansionFactorPerHa *= Constant.AcresPerHectare;
            }
            if (dbhInCm <= 0.0F)
            {
                Debug.Assert(dbhInCm == 0.0F);
                return 0.0F;
            }

            float cvtsPerTreeInCubicM = treesOfSpecies.Species switch
            {
                // Poudel K, Temesgen H, Gray AN. 2018. Estimating upper stem diameters and volume of Douglas-fir and Western hemlock
                //   trees in the Pacific northwest. Forest Ecosystems 5:16. https://doi.org/10.1186/s40663-018-0134-2
                // Table 8: equation form is exp(a + b log(dbh) + c log(height)) = exp(a) exp(log(dbh) b) exp(log(height) c)
                //                                                               = exp(a) dbh^b height^c
                // Since evaluating a power requires both an exponent and a log, maintaining the equation form saves one exponentiation.
                FiaCode.PseudotsugaMenziesii => MathV.Exp(-9.70405F + 1.61812F * MathV.Ln(dbhInCm) + 1.21071F * MathV.Ln(heightInM)),
                FiaCode.TsugaHeterophylla => MathV.Exp(-9.98200F + 1.37228F * MathV.Ln(dbhInCm) + 1.57319F * MathV.Ln(heightInM)),
                _ => throw Trees.CreateUnhandledSpeciesException(treesOfSpecies.Species),
            };
            return expansionFactorPerHa * cvtsPerTreeInCubicM;
        }

        public static float GetDouglasFirDiameterInsideBark(float dbhInCm, float heightInM, float evaluationHeightInM)
        {
            // Poudel K, Temesgen H, Gray AN. 2018. Estimating upper stem diameters and volume of Douglas-fir and Western hemlock
            //   trees in the Pacific northwest. Forest Ecosystems 5:16. https://doi.org/10.1186/s40663-018-0134-2
            // Table 4: M4 (Kozak 2004) form
            // Regression's fitted diameter range: 1.8-114 cm
            //                     height range: 2.4-64 m
            // Allow extrapolation beyond fit range in lieu of a better predictor.
            if ((dbhInCm < 0.0F) || (dbhInCm > 135.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInCm), "Diameter of " + dbhInCm.ToString(Constant.Default.DiameterInCmFormat) + " cm is either negative or exceeds regression limit of 135.0 cm.");
            }
            if ((heightInM < PoudelRegressions.MinimumKozakHeightInM) || (heightInM > 80.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(heightInM), "Height of " + heightInM.ToString(Constant.Default.HeightInMFormat) + " m is either less than the Kozak 2004 regression form's minimum of 1.3 m or exceeds regression limit of 75.0 m.");
            }
            if ((evaluationHeightInM < 0.0F) || (evaluationHeightInM > heightInM))
            {
                throw new ArgumentOutOfRangeException(nameof(evaluationHeightInM), "Evaluation height of " + evaluationHeightInM.ToString(Constant.Default.HeightInMFormat) + " m is negative or exceeds tree height of " + heightInM.ToString(Constant.Default.HeightInMFormat) + " m.");
            }

            const float b1 = 1.04208F;
            const float b2 = 0.99771F;
            const float b3 = -0.03111F;
            const float b4 = 0.53788F;
            const float b5 = -1.01291F;
            const float b6 = 0.56813F;
            const float b7 = 4.96019F;
            const float b8 = 0.04124F;
            const float b9 = -0.34417F;
            float t = evaluationHeightInM / heightInM;
            float k = 1.3F / heightInM; // greater than 1 if tree is less than 1.3 m tall
            float oneMinusCubeRootT = 1 - MathV.Pow(t, 1.0F / 3.0F);
            float tkRatio = oneMinusCubeRootT / (1 - MathV.Pow(k, 1.0F / 3.0F)); // dib calculation fails for k >= 1 since tkRatio is infinite at k = 1 and negative for k > 1
            float dibInCm = b1 * MathV.Pow(dbhInCm, b2) * MathV.Pow(heightInM, b3) * MathV.Pow(tkRatio, b4 * t * t * t * t + b5 / MathV.Exp(dbhInCm / heightInM) + b6 * MathV.Pow(tkRatio, 0.1F) + b7 / dbhInCm + b8 * MathV.Pow(heightInM, oneMinusCubeRootT) + b9 * tkRatio);
            // float dibMathFcheck = b1 * MathF.Pow(dbhInCm, b2) * MathF.Pow(heightInM, b3) * MathF.Pow(tkRatio, b4 * t * t * t * t + b5 / MathF.Exp(dbhInCm / heightInM) + b6 * MathF.Pow(tkRatio, 0.1F) + b7 / dbhInCm + b8 * MathF.Pow(heightInM, 1 - MathF.Pow(evaluationHeightInM / heightInM, 1.0F / 3.0F)) + b9 * tkRatio);
            return dibInCm;
        }

        public static float GetWesternHemlockDiameterInsideBark(float dbhInCm, float heightInM, float evaluationHeightInM)
        {
            // same code as GetDouglasFirDiameterInsideBark() with western hemlock values of b1-b9, also from Table 4: M4 (Kozak 2004) form
            if ((dbhInCm < 0.0F) || (dbhInCm > 135.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInCm), "Diameter of " + dbhInCm.ToString(Constant.Default.DiameterInCmFormat) + " cm is either negative or exceeds regression limit of 135.0 cm.");
            }
            if ((heightInM < PoudelRegressions.MinimumKozakHeightInM) || (heightInM > 75.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(heightInM), "Height of " + heightInM.ToString(Constant.Default.HeightInMFormat) + " m is either less than the Kozak 2004 regression form's minimum of 1.3 m or exceeds regression limit of 75.0 m.");
            }
            if ((evaluationHeightInM < 0.0F) || (evaluationHeightInM > heightInM))
            {
                throw new ArgumentOutOfRangeException(nameof(evaluationHeightInM), "Evaluation height of " + evaluationHeightInM.ToString(Constant.Default.HeightInMFormat) + " m is negative or exceeds tree height of " + heightInM.ToString(Constant.Default.HeightInMFormat) + " m.");
            }
            if (evaluationHeightInM == heightInM)
            {
                return 0.0F; // numerical edge case
            }

            const float b1 = 1.05981F;
            const float b2 = 0.99433F;
            const float b3 = -0.01684F;
            const float b4 = 0.64632F;
            const float b5 = -1.56599F;
            const float b6 = 0.74293F;
            const float b7 = 4.75618F;
            const float b8 = 0.0389F;
            const float b9 = -0.19425F;
            float t = evaluationHeightInM / heightInM;
            float k = 1.3F / heightInM; // greater than 1 if tree is less than 1.3 m tall
            float oneMinusCubeRootT = 1 - MathV.Pow(t, 1.0F / 3.0F);
            float tkRatio = oneMinusCubeRootT / (1 - MathV.Pow(k, 1.0F / 3.0F)); // dib calculation fails for k >= 1 since tkRatio is infinite at k = 1 and negative for k > 1
            float dibInCm = b1 * MathV.Pow(dbhInCm, b2) * MathV.Pow(heightInM, b3) * MathV.Pow(tkRatio, b4 * t * t * t * t + b5 / MathV.Exp(dbhInCm / heightInM) + b6 * MathV.Pow(tkRatio, 0.1F) + b7 / dbhInCm + b8 * MathV.Pow(heightInM, oneMinusCubeRootT) + b9 * tkRatio);
            // float dibMathFcheck = b1 * MathF.Pow(dbhInCm, b2) * MathF.Pow(heightInM, b3) * MathF.Pow(tkRatio, b4 * t * t * t * t + b5 / MathF.Exp(dbhInCm / heightInM) + b6 * MathF.Pow(tkRatio, 0.1F) + b7 / dbhInCm + b8 * MathF.Pow(heightInM, 1 - MathF.Pow(evaluationHeightInM / heightInM, 1.0F / 3.0F)) + b9 * tkRatio);
            return dibInCm;
        }

        /// <summary>
        /// Find total per hectare biomass of trees.
        /// </summary>
        /// <param name="treesOfSpecies">Trees.</param>
        /// <returns>Live biomass of trees in kg/ha.</returns>
        public static float GetLiveBiomass(Trees treesOfSpecies)
        {
            // Poudel KP, Temesgen H, Radtke PJ, Gray AN. 2019. Estimating individual-tree aboveground biomass of tree species
            //  in the western U.S.A. Canadian Journal of Forest Resources 49:701–714. https://dx.doi.org/10.1139/cjfr-2018-0361
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
                float dbhInCm = treesOfSpecies.Dbh[compactedTreeIndex];
                float heightInM = treesOfSpecies.Height[compactedTreeIndex];
                float expansionFactorPerHa = treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                if (treesOfSpecies.Units == Units.English)
                {
                    dbhInCm *= Constant.CentimetersPerInch;
                    heightInM *= Constant.MetersPerFoot;
                    expansionFactorPerHa *= Constant.AcresPerHectare;
                }

                float treeBiomassInKgPerHa = expansionFactorPerHa * correctionFactor * MathV.Exp(a + b * MathV.Ln(dbhInCm) + c * MathV.Ln(heightInM));
                liveBiomassInKgPerHa += treeBiomassInKgPerHa;
            }

            return liveBiomassInKgPerHa;
        }
    }
}
