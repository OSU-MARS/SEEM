using Mars.Seem.Extensions;
using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Tree
{
    // Subroutines required for the calculation of
    // height growth using the western hemlock top height curves of
    // Flewelling.These subroutines are required:
    // SITECV_F   computes top height from site and age
    // SITEF_C computes model parameters
    // SITEF_SI   calculates an approximate psi for a given site
    // Note: Flewelling's curves are metric.
    // Site Index is not adjusted for stump height.
    // These subroutines contain unique commons not in the include file.
    /// <summary>
    /// Calculates Flewelling's top heights for western hemlock (Tsuga heterophylla).
    /// </summary>
    /// <remarks>
    /// Flewelling's 1993 hemlock growth papers (<a href="https://doi.org/10.1139/x93-070">part 1</a> and 
    /// <a href="https://doi.org/10.1139/x93-071">part 2</a>) do not use this method. Assuming they use the same data the
    /// likely range of validity is for heights up to 60-70 meters and DBH to 80 cm for hemlocks growing in coastal Washington.
    /// 
    /// Marshall 2003 (https://www.nrcresearchpress.com/doi/pdf/10.1139/x03-126) suggests the equations here might be from a 
    /// 1994 report to the Northwest Taper Cooperative which does not appear to be online.
    /// </remarks>
    internal class WesternHemlock
    {
        public static TreeSpeciesProperties Properties { get; private set; }

        static WesternHemlock()
        {
            // Miles PD, Smith BW. 2009. Specific gravity and other properties of wood and bark for 156 tree species found in North
            //   America (No. NRS-RN-38). Northern Research Station, US Forest Service. https://doi.org/10.2737/NRS-RN-38
            WesternHemlock.Properties = new TreeSpeciesProperties(greenWoodDensity: 657.0F, // kg/m³
                barkFraction: 0.158F,
                barkDensity: 1009.0F, // kg/m³
                processingBarkLoss: 0.30F, // loss with spiked feed rollers
                yardingBarkLoss: 0.15F); // dragging abrasion loss over full corridor (if needed, this could be reparameterized to a function of corridor length)
        }

        public static float GetDiameterOutsideBark(float dbhInCm, float heightInM, float evaluationHeightInM)
        {
            // call to PoudelRegressions checks evaluationHeightInM < heightInM
            if ((evaluationHeightInM < 0.1F) || (evaluationHeightInM > Constant.DbhHeightInM))
            {
                throw new ArgumentOutOfRangeException(nameof(evaluationHeightInM));
            }

            // for now, a quick approximation assuming bark thickness decreases linearly with height
            // Could also use taper equations in
            //   Flewelling JW, Raynes LM. 1993. Variable-shape stem-profile predictions for western hemlock. Part I. Predictions from DBH and total
            //     height. Canadian Journal of Forestry 23:520-536. https://doi.org/10.1139/x93-070
            // Meyer (1946) k values of 0.910-0.931 from Table 4 of
            //   Kozak AK, Yang RC. 1981. Equations for Estimating Bark Volume and Thickness of Commercial Trees in British Columbia. The Forestry
            //     Chronicle 57(3):112-115. https://doi.org/10.5558/tfc57112-3
            // Meyer's equation is simply bark thickness B at DBH = 0.5 DBH (1 - k) => double bark thickness DBT = (1 - k) DBH
            float diameterInsideBark = PoudelRegressions.GetWesternHemlockDiameterInsideBark(dbhInCm, heightInM, evaluationHeightInM);
            float doubleBarkThicknessAtDbh = dbhInCm - PoudelRegressions.GetWesternHemlockDiameterInsideBark(dbhInCm, heightInM, Constant.DbhHeightInM);
            return diameterInsideBark + doubleBarkThicknessAtDbh * (heightInM - evaluationHeightInM)/ (heightInM - Constant.DbhHeightInM);
        }

        /// <summary>
        /// Calculate western hemlock growth effective age and potential height growth using Flewelling's model for dominant individuals.
        /// </summary>
        /// <param name="site">Site growth constants.</param>
        /// <param name="treeHeight">Height of tree.</param>
        /// <param name="timeStepInYears"></param>
        /// <param name="growthEffectiveAge">Growth effective age of tree.</param>
        /// <param name="potentialHeightGrowth">Potential height growth increment in feet.</param>
        public static float GetFlewellingGrowthEffectiveAge(SiteConstants site, float timeStepInYears, float treeHeight, out float potentialHeightGrowth)
        {
            // For Western Hemlock compute Growth Effective Age and 5-year potential
            // or 1-year height growth using the western hemlock top height curves of
            // Flewelling.These subroutines are required:
            // SITECV_F   computes top height from site and age
            // SITEF_C computes model parameters
            // SITEF_SI   calculates an approximate psi for a given site
            // Note: Flewelling's curves are metric.
            // Site Index is not adjusted for stump height.
            float heightInM = Constant.MetersPerFoot * treeHeight;

            // find growth effective age within precision of 0.01 years
            float topHeightInM;
            float ageInYears = 1.0F;
            float growthEffectiveAge;
            for (int index = 0; index < 4; ++index)
            {
                do
                {
                    ageInYears += 100.0F / MathV.Exp10(index);
                    if (ageInYears > 500.0F)
                    {
                        growthEffectiveAge = 500.0F;
                        float XHTOP1 = WesternHemlock.GetTopHeight(site, growthEffectiveAge);
                        float XHTOP2 = WesternHemlock.GetTopHeight(site, growthEffectiveAge + timeStepInYears);
                        potentialHeightGrowth = Constant.FeetPerMeter * (XHTOP2 - XHTOP1);
                        return growthEffectiveAge;
                    }
                    topHeightInM = WesternHemlock.GetTopHeight(site, ageInYears);
                }
                while (topHeightInM < heightInM);
                ageInYears -= 100.0F / MathV.Exp10(index);
            }
            growthEffectiveAge = ageInYears;

            // Compute top height and potential height growth
            topHeightInM = WesternHemlock.GetTopHeight(site, growthEffectiveAge + timeStepInYears);
            float potentialTopHeightInFeet = Constant.FeetPerMeter * topHeightInM;
            potentialHeightGrowth = potentialTopHeightInFeet - treeHeight;

            return growthEffectiveAge;
        }

        public static Vector256<float> GetFlewellingGrowthEffectiveAgeAvx(SiteConstants site, float timeStepInYears, Vector256<float> treeHeight, out Vector256<float> potentialHeightGrowth)
        {
            // for now, just a Vector256 to Vector128 fallback for caller convenience
            Vector128<float> treeHeightLower = Avx.ExtractVector128(treeHeight, Constant.Simd256x8.ExtractLower128);
            Vector128<float> growthEffectiveAgeLower = WesternHemlock.GetFlewellingGrowthEffectiveAgeVex128(site, timeStepInYears, treeHeightLower, out Vector128<float> potentialHeightGrowthLower);
            Vector128<float> treeHeightUpper = Avx.ExtractVector128(treeHeight, Constant.Simd256x8.ExtractUpper128);
            Vector128<float> growthEffectiveAgeUpper = WesternHemlock.GetFlewellingGrowthEffectiveAgeVex128(site, timeStepInYears, treeHeightUpper, out Vector128<float> potentialHeightGrowthUpper);

            potentialHeightGrowth = Vector256.Create(potentialHeightGrowthLower, potentialHeightGrowthUpper);
            Vector256<float> growthEffectiveAge = Vector256.Create(growthEffectiveAgeLower, growthEffectiveAgeUpper);
            return growthEffectiveAge;
        }

        public static Vector512<float> GetFlewellingGrowthEffectiveAgeAvx512(SiteConstants site, float timeStepInYears, Vector512<float> treeHeight, out Vector512<float> potentialHeightGrowth)
        {
            // for now, just a Vector512 to Vector256 fallback for caller convenience
            Vector256<float> treeHeightLower = Avx512F.ExtractVector256(treeHeight, Constant.Simd512x16.ExtractLower256);
            Vector256<float> growthEffectiveAgeLower = WesternHemlock.GetFlewellingGrowthEffectiveAgeAvx(site, timeStepInYears, treeHeightLower, out Vector256<float> potentialHeightGrowthLower);
            Vector256<float> treeHeightUpper = Avx512F.ExtractVector256(treeHeight, Constant.Simd512x16.ExtractUpper256);
            Vector256<float> growthEffectiveAgeUpper = WesternHemlock.GetFlewellingGrowthEffectiveAgeAvx(site, timeStepInYears, treeHeightUpper, out Vector256<float> potentialHeightGrowthUpper);

            potentialHeightGrowth = Vector512.Create(potentialHeightGrowthLower, potentialHeightGrowthUpper);
            Vector512<float> growthEffectiveAge = Vector512.Create(growthEffectiveAgeLower, growthEffectiveAgeUpper);
            return growthEffectiveAge;
        }

        public static Vector128<float> GetFlewellingGrowthEffectiveAgeVex128(SiteConstants site, float timeStepInYears, Vector128<float> treeHeight, out Vector128<float> potentialHeightGrowth)
        {
            // for now, just a vector to scalar shim for caller convenience
            // TODO: SIMD implementation
            float growthEffectiveAge0 = WesternHemlock.GetFlewellingGrowthEffectiveAge(site, timeStepInYears, Avx.Extract(treeHeight, 0), out float potentialHeightGrowth0);
            float growthEffectiveAge1 = WesternHemlock.GetFlewellingGrowthEffectiveAge(site, timeStepInYears, Avx.Extract(treeHeight, 1), out float potentialHeightGrowth1);
            float growthEffectiveAge2 = WesternHemlock.GetFlewellingGrowthEffectiveAge(site, timeStepInYears, Avx.Extract(treeHeight, 2), out float potentialHeightGrowth2);
            float growthEffectiveAge3 = WesternHemlock.GetFlewellingGrowthEffectiveAge(site, timeStepInYears, Avx.Extract(treeHeight, 3), out float potentialHeightGrowth3);

            potentialHeightGrowth = Vector128.Create(potentialHeightGrowth0, potentialHeightGrowth1, potentialHeightGrowth2, potentialHeightGrowth3);
            Vector128<float> growthEffectiveAge = Vector128.Create(growthEffectiveAge0, growthEffectiveAge1, growthEffectiveAge2, growthEffectiveAge3);
            return growthEffectiveAge;
        }

        internal static float GetNeiloidHeight(float dbhInCm, float heightInM)
        {
            // approximation from plotting families of Poudel et al. 2018 dib curves in R and fitting the neiloid inflection point
            // from linear regressions in PoudelRegressions.R
            // Poudel et al's fitting is problematic as diameter inside bark becomes non-monotonic with height for slender trees, predicting
            // logs with bottom diameters inside bark which are smaller than their top diameters or, in first logs, smaller than DBH. The
            // neiloid height regressions are constructed to avoid decreasing basal diameters.
            float heightDiameterRatio = heightInM / (0.01F * dbhInCm);
            float neiloidHeightInM = 0.4F - 1.0F / (0.035F * heightDiameterRatio) + 0.01F * (3.25F + 0.025F * heightDiameterRatio) * dbhInCm;
            return MathF.Max(neiloidHeightInM, Constant.Bucking.DefaultStumpHeightInM);
        }

        /// <summary>
        /// Estimate top height for site index and tree age.
        /// </summary>
        /// <param name="SI">site index (meters) for breast height age of 50 years</param>
        /// <param name="breastHeightAgeInYears">breast height age (years)</param>
        /// <param name="topHeight">site height (meters) for given site index and age</param>
        /// <remarks>PSI is the pivoted (translated) site index from SITEF_SI.</remarks>
        private static float GetTopHeight(SiteConstants site, float breastHeightAgeInYears)
        {
            // apply height-age equation
            float X = breastHeightAgeInYears - 1.0F;
            float topHeight;
            if (X < site.Xk)
            {
                topHeight = site.H1 + site.PPSI * X + (1.0F - site.B1) * site.PPSI * site.Xk / (site.C + 1.0F) * (MathF.Pow((site.Xk - X) / site.Xk, site.C + 1.0F) - 1.0F);
            }
            else
            {
                float Z = X - site.Xk;
                topHeight = site.Yk + site.Alpha * (1.0F - MathV.Exp(-site.Beta * Z));
            }

            return topHeight;
        }

        public class SiteConstants
        {
            public float Alpha { get; private init; }
            public float B1 { get; private init; }
            public float Beta { get; private init; }
            public float C { get; private init; }
            public float H1 { get; private init; }
            public float PPSI { get; private init; }
            public float Xk { get; private init; }
            public float Yk { get; private init; }

            public SiteConstants(float siteIndexFromInFeet) // site index from ground, not breast height
            {
                float siteIndexInM = Constant.MetersPerFoot * siteIndexFromInFeet;

                // Purpose:  Calculates an approximate psi for a given site index
                // Ref 'Model Fitting: top height increment', Feb 2, 1994.
                //
                // Current Date: FEB 2, 1994    J. FLEWELLING and A. ZUMRAWI
                //
                // si input r*4    site index (top height at BH age 50)
                // psi output r*4    site productivity parameter.
                float SI_PIV = 32.25953F;
                float X = (siteIndexInM - SI_PIV) / 10.0F;
                float PSI;
                if (siteIndexInM <= SI_PIV)
                {
                    PSI = 0.75F + X * (0.299720F + X * (0.116875F + X * (0.074866F + X * (0.032348F + X * (0.006984F + X * 0.000339F)))));
                }
                else
                {
                    PSI = 0.75F + X * (0.290737F + X * (0.129665F + X * (-0.058777F + X * (-0.000669F + X * (0.006003F + X * -0.001060F)))));
                }

                // Purpose:  For a specified psi, calculates all of the height-age
                // model parameters, and stores them in /sitefprm/
                //
                // Current Date: FEB 2, 1994    J. FLEWELLING
                //
                // psi input REAL productivity index (m/yr)
                this.PPSI = PSI;
                this.Xk = 128.326F * MathV.Exp(-2.54871F * PSI);
                this.B1 = 0.2F + 0.8F / (1.0F + MathV.Exp(5.33208F - 9.00622F * PSI));
                this.C = 1.0F + 1.2F * PSI;
                this.Alpha = 52.7948F * PSI;
                this.H1 = 1.3F + (B1 * PSI) / 2.0F;
                this.Yk = H1 + PSI * Xk * (1.0F - (1.0F - B1) / (C + 1.0F));
                this.Beta = PSI / Alpha;
            }
        }
    }
}
