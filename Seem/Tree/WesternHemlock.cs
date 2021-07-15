using Osu.Cof.Ferm.Extensions;
using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm.Tree
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
            float HTM = Constant.MetersPerFoot * treeHeight;

            // find growth effective age within precision of 0.01 years
            float HTOP;
            float AGE = 1.0F;
            float growthEffectiveAge;
            for (int index = 0; index < 4; ++index)
            {
                do
                {
                    AGE += 100.0F / MathV.Exp10(index);
                    if (AGE > 500.0F)
                    {
                        growthEffectiveAge = 500.0F;
                        WesternHemlock.SITECV_F(site, growthEffectiveAge, out float XHTOP1);
                        WesternHemlock.SITECV_F(site, growthEffectiveAge + timeStepInYears, out float XHTOP2);
                        potentialHeightGrowth = Constant.FeetPerMeter * (XHTOP2 - XHTOP1);
                        return growthEffectiveAge;
                    }
                    WesternHemlock.SITECV_F(site, AGE, out HTOP);
                }
                while (HTOP < HTM);
                AGE -= 100.0F / MathV.Exp10(index);
            }
            growthEffectiveAge = AGE;

            // Compute top height and potential height growth
            WesternHemlock.SITECV_F(site, growthEffectiveAge + timeStepInYears, out HTOP);
            float potentialTopHeightInFeet = Constant.FeetPerMeter * HTOP;
            potentialHeightGrowth = potentialTopHeightInFeet - treeHeight;

            return growthEffectiveAge;
        }

        public static Vector128<float> GetFlewellingGrowthEffectiveAge(SiteConstants site, float timeStepInYears, Vector128<float> treeHeight, out Vector128<float> potentialHeightGrowth)
        {
            // for now, just a vector to scalar shim for caller convenience
            // TODO: SIMD implementation
            float growthEffectiveAge0 = WesternHemlock.GetFlewellingGrowthEffectiveAge(site, timeStepInYears, Avx.Extract(treeHeight, 0), out float potentialHeightGrowth0);
            float growthEffectiveAge1 = WesternHemlock.GetFlewellingGrowthEffectiveAge(site, timeStepInYears, Avx.Extract(treeHeight, 0), out float potentialHeightGrowth1);
            float growthEffectiveAge2 = WesternHemlock.GetFlewellingGrowthEffectiveAge(site, timeStepInYears, Avx.Extract(treeHeight, 0), out float potentialHeightGrowth2);
            float growthEffectiveAge3 = WesternHemlock.GetFlewellingGrowthEffectiveAge(site, timeStepInYears, Avx.Extract(treeHeight, 0), out float potentialHeightGrowth3);

            potentialHeightGrowth = Vector128.Create(potentialHeightGrowth0, potentialHeightGrowth1, potentialHeightGrowth2, potentialHeightGrowth3);
            Vector128<float> growthEffectiveAge = Vector128.Create(growthEffectiveAge0, growthEffectiveAge1, growthEffectiveAge2, growthEffectiveAge3);
            return growthEffectiveAge;
        }

        /// <summary>
        /// Estimate top height for site index and tree age.
        /// </summary>
        /// <param name="SI">site index (meters) for breast height age of 50 years</param>
        /// <param name="AGE">breast height age (years)</param>
        /// <param name="HTOP">site height (meters) for given site index and age</param>
        /// <remarks>PSI is the pivoted (translated) site index from SITEF_SI.</remarks>
        public static void SITECV_F(SiteConstants site, float AGE, out float HTOP)
        {
            // apply height-age equation
            float X = AGE - 1.0F;
            if (X < site.Xk)
            {
                HTOP = site.H1 + site.PPSI * X + (1.0F - site.B1) * site.PPSI * site.Xk / (site.C + 1.0F) * (MathF.Pow((site.Xk - X) / site.Xk, site.C + 1.0F) - 1.0F);
            }
            else
            {
                float Z = X - site.Xk;
                HTOP = site.Yk + site.Alpha * (1.0F - MathV.Exp(-site.Beta * Z));
            }
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

            public SiteConstants(float siteIndexFromGroundEnglish)
            {
                float siteIndexMetric = Constant.MetersPerFoot * siteIndexFromGroundEnglish;

                // Purpose:  Calculates an approximate psi for a given site index
                // Ref 'Model Fitting: top height increment', Feb 2, 1994.
                //
                // Current Date: FEB 2, 1994    J. FLEWELLING and A. ZUMRAWI
                //
                // si input r*4    site index (top height at BH age 50)
                // psi output r*4    site productivity parameter.
                float SI_PIV = 32.25953F;
                float X = (siteIndexMetric - SI_PIV) / 10.0F;
                float PSI;
                if (siteIndexMetric <= SI_PIV)
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
