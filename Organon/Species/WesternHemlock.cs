using System;

namespace Osu.Cof.Ferm.Species
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
    // BUGBUG fix most recent cache inherited from Fortran
    internal class WesternHemlock
    {
        private static float ALPHA;
        private static float B1;
        private static float BETA;
        private static float C;
        private static float H1;
        private static float OLD_SI = -1.0F;
        private static float PPSI;
        private static float XK;
        private static float YK;

        /// <summary>
        /// Calculate western hemlock growth effective age and potential height growth using Flewelling's model for dominant individuals.
        /// </summary>
        /// <param name="siteIndexFromGround">Site index (feet) from ground.</param>
        /// <param name="treeHeight">Height of tree.</param>
        /// <param name="GP"></param>
        /// <param name="growthEffectiveAge">Growth effective age of tree.</param>
        /// <param name="potentialHeightGrowth">Potential height growth increment in feet.</param>
        public static void F_HG(float siteIndexFromGround, float treeHeight, float GP, out float growthEffectiveAge, out float potentialHeightGrowth)
        {
            // For Western Hemlock compute Growth Effective Age and 5-year potential
            // or 1-year height growth using the western hemlock top height curves of
            // Flewelling.These subroutines are required:
            // SITECV_F   computes top height from site and age
            // SITEF_C computes model parameters
            // SITEF_SI   calculates an approximate psi for a given site
            // Note: Flewelling's curves are metric.
            // Site Index is not adjusted for stump height.
            float SIM = siteIndexFromGround * 0.3048F;
            float HTM = treeHeight * 0.3048F;

            // find growth effective age within precision of 0.01 years
            float HTOP;
            float AGE = 1.0F;
            for (int index = 0; index < 4; ++index)
            {
                do
                {
                    AGE += 100.0F / MathV.Exp10(index);
                    if (AGE > 500.0F)
                    {
                        growthEffectiveAge = 500.0F;
                        WesternHemlock.SITECV_F(SIM, growthEffectiveAge, out float XHTOP1);
                        WesternHemlock.SITECV_F(SIM, growthEffectiveAge + GP, out float XHTOP2);
                        potentialHeightGrowth = 3.2808F * (XHTOP2 - XHTOP1);
                        return;
                    }
                    WesternHemlock.SITECV_F(SIM, AGE, out HTOP);
                }
                while (HTOP < HTM);
                AGE -= 100.0F / MathV.Exp10(index);
            }
            growthEffectiveAge = AGE;

            // Compute top height and potential height growth
            WesternHemlock.SITECV_F(SIM, growthEffectiveAge + GP, out HTOP);
            float potentialTopHeightInFeet = HTOP * 3.2808F;
            potentialHeightGrowth = potentialTopHeightInFeet - treeHeight;
        }

        /// <summary>
        /// Estimate top height for site index and tree age.
        /// </summary>
        /// <param name="SI">site index (meters) for breast height age of 50 years</param>
        /// <param name="AGE">breast height age (years)</param>
        /// <param name="HTOP">site height (meters) for given site index and age</param>
        /// <remarks>PSI is the pivoted (translated) site index from SITEF_SI.</remarks>
        public static void SITECV_F(float SI, float AGE, out float HTOP)
        {
            // determine if coefficients for this SI are
            // already in siteprm.If not, get them.
            if (SI != OLD_SI)
            {
                OLD_SI = SI;
                SITEF_SI(SI, out float PSI);
                SITEF_C(PSI);
            }

            // apply height-age equation
            float X = AGE - 1.0F;
            if (X < XK)
            {
                HTOP = H1 + PPSI * X + (1.0F - B1) * PPSI * XK / (C + 1.0F) * (MathF.Pow((XK - X) / XK, C + 1.0F) - 1.0F);
            }
            else
            {
                float Z = X - XK;
                HTOP = YK + ALPHA * (1.0F - MathV.Exp(-BETA * Z));
            }
        }

        private static void SITEF_C(float PSI)
        {
            // Purpose:  For a specified psi, calculates all of the height-age
            // model parameters, and stores them in /sitefprm/
            //
            // Current Date: FEB 2, 1994    J. FLEWELLING
            //
            // psi input REAL productivity index (m/yr)
            WesternHemlock.PPSI = PSI;
            WesternHemlock.XK = 128.326F * MathV.Exp(-2.54871F * PSI);
            WesternHemlock.B1 = 0.2F + 0.8F / (1.0F + MathV.Exp(5.33208F - 9.00622F * PSI));
            WesternHemlock.C = 1.0F + 1.2F * PSI;
            WesternHemlock.ALPHA = 52.7948F * PSI;
            WesternHemlock.H1 = 1.3F + (B1 * PSI) / 2.0F;
            WesternHemlock.YK = H1 + PSI * XK * (1.0F - (1.0F - B1) / (C + 1.0F));
            WesternHemlock.BETA = PSI / ALPHA;
        }

        private static void SITEF_SI(float SI, out float PSI)
        {
            // Purpose:  Calculates an approximate psi for a given site index
            // Ref 'Model Fitting: top height increment', Feb 2, 1994.
            //
            // Current Date: FEB 2, 1994    J. FLEWELLING and A. ZUMRAWI
            //
            // si input r*4    site index (top height at BH age 50)
            // psi output r*4    site productivity parameter.
            float SI_PIV = 32.25953F;
            float X = (SI - SI_PIV) / 10.0F;
            if (SI <= SI_PIV)
            {
                PSI = 0.75F + X * (0.299720F + X * (0.116875F + X * (0.074866F + X * (0.032348F + X * (0.006984F + X * 0.000339F)))));
            }
            else 
            {
                PSI = 0.75F + X * (0.290737F + X * (0.129665F + X * (-0.058777F + X * (-0.000669F + X * (0.006003F + X * -0.001060F)))));
            }
        }
    }
}
