using Osu.Cof.Ferm.Extensions;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm.Tree
{
    public class DouglasFir
    {
        // Diameter outside bark at heights of 30 cm to 1.37 m from
        // Maguire DA, Hann DW. 1990. Bark Thickness and Bark Volume in Southwestern Oregon Douglas-Fir. Western Journal of Applied
        //   Forestry 5(1):5–8. https://doi.org/10.1093/wjaf/5.1.5
        // Curtis RO, Arney JD. 1977. Estimating D.B.H. from stump diameters in second-growth Douglas-fir. Research Note PNW-297, US
        //   Forest Service. https://www.fs.fed.us/pnw/olympia/silv/publications/opt/167_CurtisArney1977.pdf
        public static float GetDiameterOutsideBark(float dbhInCm, float heightInM, float heightToCrownBaseInM, float evaluationHeightInM)
        {
            if ((heightInM < 0.0F) || (heightInM > 75.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(heightInM), "Height of " + heightInM.ToString("0.0") + " m is either negative or exceeds regression limit of 75.0 m.");
            }
            if ((evaluationHeightInM < 0.25F * Constant.MetersPerFoot) || (evaluationHeightInM > Constant.DbhHeightInM))
            {
                // If needed, support for estimating diameter outside bark above breast height can be implemented by using Poudel et al.
                // 2018's diameter inside bark regressions and adding bark thickness from Maguire and Hann 1990.
                throw new ArgumentOutOfRangeException(nameof(evaluationHeightInM), "Evaluation height of " + evaluationHeightInM.ToString("0.00") + " m is less than the regression limit of 0.25 feet or exceeds breast height (" + Constant.DbhHeightInM + ").");
            }

            // for now, no effort is made to resolve discontinuities between Curtis and Arney 1977 and Maguire and Hann 1990
            float dbhInInches = Constant.InchesPerCentimeter * dbhInCm;
            float evaluationHeightInFeet = Constant.FeetPerMeter * evaluationHeightInM;
            float diameterOutsideBark;
            if (evaluationHeightInM < Constant.MetersPerFoot)
            {
                // Curtis and Arney's dataset includes one stem of 28 inches DBH (71 cm), the rest are 24 inches (61 cm) and smaller
                if ((dbhInCm < 0.0F) || (dbhInCm > 71.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(dbhInCm), "Diameter of " + dbhInCm.ToString("0.0") + " cm is either negative or exceeds the regression limit of Curtis and Arney 1977.");
                }

                // solution of Curtis and Arney 1977, equation 1 (valid for heights of 0.25-2 feet), for stump diameter outside of bark
                // using Cardano's method for the roots of third order polynomials
                // (Curtis and Arney assign polynomial coefficients a-d to increasing powers, a reversal of the common order.)
                float a = 0.12327F - 0.027392F * evaluationHeightInFeet - dbhInInches; // convert dbh = a + b dob + c dob^2 + d dob^3 to d dob^3 + c dob^2 + b dob + a - dbh = 0 for finding roots
                float b = 0.64885F + 0.27258F * evaluationHeightInFeet - 0.113191F * evaluationHeightInFeet * evaluationHeightInFeet + 0.025339F * evaluationHeightInFeet * evaluationHeightInFeet * evaluationHeightInFeet - 0.00217612F * evaluationHeightInFeet * evaluationHeightInFeet * evaluationHeightInFeet * evaluationHeightInFeet;
                float c = 0.0025583F - 0.0011370F * evaluationHeightInFeet + 0.00012634F * evaluationHeightInFeet * evaluationHeightInFeet;
                float d = -0.000066158F + 0.000014702F * evaluationHeightInFeet;

                float r = (9.0F * d * c * b - 27.0F * d*d * a - 2.0F * c*c*c) / (54.0F * d*d*d);
                float q = (3.0F * d * b - c*c) / (9.0F * d*d);
                Complex s = Complex.Pow(r + Complex.Sqrt(q*q*q + r*r), 1.0F / 3.0F);
                Complex t = Complex.Pow(r - Complex.Sqrt(q * q * q + r * r), 1.0F / 3.0F);
                // Complex root1 = s + t - c / (3.0F * d);
                // Complex root2 = -0.5 * (s + t) - c / (3.0F * d) + new Complex(0.0, 0.5 * Math.Sqrt(3.0)) * (s - t);
                Complex root3 = -0.5 * (s + t) - c / (3.0F * d) - new Complex(0.0, 0.5 * Math.Sqrt(3.0)) * (s - t);
                if (Math.Abs(root3.Imaginary) < 0.000001)
                {
                    // At small diameters polynomial roots 1 and 2 are a complex conjugate pair and the third root is real and positive.
                    diameterOutsideBark = Constant.CentimetersPerInch * (float)root3.Real;
                }
                else
                {
                    // At diameters above a meter or so, roots 1 and 3 become a complex conjugate pair. Root 2 becomes real but is negative.
                    throw new ArgumentOutOfRangeException(nameof(dbhInCm), "DBH of " + dbhInCm + " cm is beyond the regression fitting range of Curtis and Arney 1977.");
                }
            }
            else
            {
                // actual data limit of the paper is 109 cm but, since regression is well behaved, allow use with somewhat larger stems
                if ((dbhInCm < 0.0F) || (dbhInCm > 120.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(dbhInCm), "Diameter of " + dbhInCm.ToString("0.0") + " cm is either negative or exceeds the regression limit of Maguire and Hann 1990.");
                }

                // Maguire and Hann 1990, equations 2 and 3, valid for heights of 1.0-4.5 feet
                float diameterOutsideBarkAt1Foot = 1.10767F * MathF.Exp(0.0710044F * (heightInM - heightToCrownBaseInM) / heightInM) * dbhInInches;
                float outsideRatioDbhTo1Foot = dbhInInches / diameterOutsideBarkAt1Foot;
                float outsideRatio = MathF.Pow(1.0F / 3.5F * (4.5F - MathF.Pow(outsideRatioDbhTo1Foot, 2.0F / 3.0F) - evaluationHeightInFeet * (1.0F - MathF.Pow(outsideRatioDbhTo1Foot, 2.0F / 3.0F))), 1.5F);
                diameterOutsideBark = Constant.CentimetersPerInch * (outsideRatio * diameterOutsideBarkAt1Foot);
            }
            return diameterOutsideBark;
        }

        // Maguire DA, Hann DW. 1990. Bark Thickness and Bark Volume in Southwestern Oregon Douglas-Fir. Western Journal of Applied
        //   Forestry 5(1):5–8. https://doi.org/10.1093/wjaf/5.1.5
        public static float GetDoubleBarkThickness(float dbhInCm, float heightInM, float heightToCrownBaseInM, float evaluationHeightInM)
        {
            if ((dbhInCm < 0.0F) || (dbhInCm > 135.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInCm), "Diameter of " + dbhInCm.ToString("0.0") + " cm is either negative or exceeds regression limit of 135.0 cm.");
            }
            if ((heightInM < 0.0F) || (heightInM > 75.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(heightInM), "Height of " + heightInM.ToString("0.0") + " m is either negative or exceeds regression limit of 75.0 m.");
            }
            if ((evaluationHeightInM < Constant.MetersPerFoot) || (evaluationHeightInM > heightInM))
            {
                throw new ArgumentOutOfRangeException(nameof(evaluationHeightInM), "Evaluation height of " + evaluationHeightInM.ToString("0.00") + " m is less than the regression limit of 1.0 feet or exceeds tree height of " + heightInM.ToString("0.00") + " m.");
            }

            float dbhInInches = Constant.InchesPerCentimeter * dbhInCm;
            //float diameterInsideBarkAtDbhInInches = 0.903563F * MathF.Pow(dbhInInches, 0.98938F); // overpredicts compared to Poudel 2018, by 1.5x at 80 cm DBH (10.3 vs 6.92 cm)
            float diameterInsideBarkAtDbhInInches = Constant.InchesPerCentimeter * PoudelRegressions.GetDouglasFirDiameterInsideBark(dbhInCm, heightInM, Constant.DbhHeightInM);
            Debug.Assert(dbhInInches > diameterInsideBarkAtDbhInInches);
            float doubleBarkThicknessInCm;
            if (evaluationHeightInM >= Constant.DbhHeightInM)
            {
                // above DBH
                // dbti = predicted double bark thickness(in.) at any height, hi
                // DBT = DOB - DIB = predicted double bark thickness(in) at breast height(4.5 ft)
                // DOB = measured breast height dob(in.) DIB = estimated breast height dib (in.) from Equation (A1)
                float X = (Constant.FeetPerMeter * evaluationHeightInM - 4.5F) / (Constant.FeetPerMeter * heightInM - 4.5F);
                float k = 0.3F;
                float I = X <= k ? 0.0F : 1.0F;
                float Z1 = I * ((X - 1) / (k - 1) * (1 + (k - X) / (k - 1)) - 1);
                float Z2 = X + I * ((X - 1) / (k - 1) * (X + k * (k - X) / (k - 1)) - X);
                float Z3 = X * X + I * (k * ((X - 1) / (k - 1)) * (2.0F * X - k + k * (k - X) / (k - 1)) - X * X);
                float barkThicknessRatioAtHeight = 1 + Z1 - 3.856886F * Z2 + 5.634181F * Z3;
                float doubleBarkThicknessAtDbhInInches = dbhInInches - diameterInsideBarkAtDbhInInches;
                doubleBarkThicknessInCm = Constant.CentimetersPerInch * (barkThicknessRatioAtHeight * doubleBarkThicknessAtDbhInInches);
            }
            else
            {
                // DBH to 1 foot
                float evaluationHeightInFeet = Constant.FeetPerMeter * evaluationHeightInM;
                float diameterOutsideBarkAt1foot = 1.10767F * MathF.Exp(0.0710044F * (heightInM - heightToCrownBaseInM) / heightInM) * dbhInInches;
                float outsideRatioDbhTo1Foot = dbhInInches / diameterOutsideBarkAt1foot;
                float outsideRatio = MathF.Pow(1.0F / 3.5F * (4.5F - MathF.Pow(outsideRatioDbhTo1Foot, 2.0F / 3.0F) - evaluationHeightInFeet * (1.0F - MathF.Pow(outsideRatioDbhTo1Foot, 2.0F / 3.0F))), 1.5F);
                float diameterOutsideBark = outsideRatio * diameterOutsideBarkAt1foot;

                float diameterInsideBarkAt1Foot = 0.938343F * MathF.Exp(0.101792F * (heightInM - heightToCrownBaseInM) / heightInM) * dbhInInches;
                float insideRatioDbhTo1Foot = diameterInsideBarkAtDbhInInches / diameterInsideBarkAt1Foot;
                float insideRatio = MathF.Pow(1.0F / 3.5F * (4.5F - MathF.Pow(insideRatioDbhTo1Foot, 2.0F / 3.0F) - evaluationHeightInFeet * (1.0F - MathF.Pow(insideRatioDbhTo1Foot, 2.0F / 3.0F))), 1.5F);
                float diameterInsideBark = insideRatio * diameterInsideBarkAt1Foot;
                // Kozak 2004 form is prone to overpredicting neiloid flare, sometimes dramatically
                // E.g. 1 cm DBH, 2 m height -> 48 cm diameter inside bark at 30 cm height.
                // float dibi = Constant.InchesPerCentimeter * PoudelRegressions.GetDouglasFirDiameterInsideBark(dbhInCm, heightInM, evaluationHeightInM);

                doubleBarkThicknessInCm = Constant.CentimetersPerInch * (diameterOutsideBark - diameterInsideBark);
            }

            Debug.Assert((doubleBarkThicknessInCm >= 0.0F) && ((doubleBarkThicknessInCm < 0.2F * dbhInCm) || (doubleBarkThicknessInCm < 0.5F)));
            return doubleBarkThicknessInCm;
        }

        internal static float GetNeiloidHeight(float dbhInCm)
        {
            return Constant.DbhHeightInM + 0.01F * 5.0F * (dbhInCm - 20.0F); // approximation from R plots
        }

        /// <summary>
        /// Estimate growth effective age for Douglas-fir and grand fir using Bruce's (1981) dominant height model.
        /// </summary>
        /// <param name="site">Site growth constants.</param>
        /// <param name="treeHeight">Tree height in feet.</param>
        /// <param name="potentialHeightGrowth"></param>
        /// <returns>Growth effective age in years.</returns>
        /// <remarks>
        /// Bruce D. 1981. Consistent Height-Growth and Growth-Rate Estimates for Remeasured Plots. Forest Science 27(4):711-725. 
        ///   https://doi.org/10.1093/forestscience/27.4.711
        /// </remarks>
        public static float GetPsmeAbgrGrowthEffectiveAge(SiteConstants site, float timeStepInYears, float treeHeight, out float potentialHeightGrowth)
        {
            float XX1 = MathV.Ln(treeHeight / site.SiteIndexFromGround) / site.B1 + site.X2toB2;
            float growthEffectiveAge = 500.0F;
            if (XX1 > 0.0F)
            {
                growthEffectiveAge = MathV.Pow(XX1, 1.0F / site.B2) - site.X1;
            }

            float potentialHeight = site.SiteIndexFromGround * MathV.Exp(site.B1 * (MathV.Pow(growthEffectiveAge + timeStepInYears + site.X1, site.B2) - site.X2toB2));
            potentialHeightGrowth = potentialHeight - treeHeight;

            return growthEffectiveAge;
        }

        public static Vector128<float> GetPsmeAbgrGrowthEffectiveAge(SiteConstants site, float timeStepInYears, Vector128<float> treeHeight, out Vector128<float> potentialHeightGrowth)
        {
            Vector128<float> B1 = AvxExtensions.BroadcastScalarToVector128(site.B1);
            Vector128<float> B2 = AvxExtensions.BroadcastScalarToVector128(site.B2);
            Vector128<float> X2toB2 = AvxExtensions.BroadcastScalarToVector128(site.X2toB2);
            Vector128<float> siteIndexFromGround128 = AvxExtensions.BroadcastScalarToVector128(site.SiteIndexFromGround);
            Vector128<float> X1 = AvxExtensions.BroadcastScalarToVector128(site.X1);

            Vector128<float> XX1 = Avx.Add(Avx.Divide(MathV.Ln(Avx.Divide(treeHeight, siteIndexFromGround128)), B1), X2toB2);
            Vector128<float> xx1lessThanZero = Avx.CompareLessThanOrEqual(XX1, Vector128<float>.Zero);
            Vector128<float> growthEffectiveAge = Avx.Subtract(MathV.Pow(XX1, Avx.Reciprocal(B2)), X1);
                             growthEffectiveAge = Avx.BlendVariable(growthEffectiveAge, AvxExtensions.BroadcastScalarToVector128(500.0F), xx1lessThanZero);

            Vector128<float> timeStepInYearsPlusX1 = AvxExtensions.BroadcastScalarToVector128(timeStepInYears + site.X1);
            Vector128<float> potentialHeightPower = Avx.Multiply(B1, Avx.Subtract(MathV.Pow(Avx.Add(growthEffectiveAge, timeStepInYearsPlusX1), B2), X2toB2));
            Vector128<float> potentialHeight = Avx.Multiply(siteIndexFromGround128, MathV.Exp(potentialHeightPower));
            potentialHeightGrowth = Avx.Subtract(potentialHeight, treeHeight);

            return growthEffectiveAge;
        }

        /// <summary>
        /// Calculate Douglas-fir and ponderosa growth effective age and potential height growth for southwest Oregon.
        /// </summary>
        /// <param name="isDouglasFir">Douglas-fir coefficients are used if ISP == 1, ponderosa otherwise.</param>
        /// <param name="SI">Site index (feet) from breast height.</param>
        /// <param name="HT">Height of tree.</param>
        /// <param name="GEAGE">Growth effective age of tree.</param>
        /// <param name="PHTGRO">Potential height growth increment in feet.</param>
        /// <remarks>
        /// Derived from the code in appendix 2 of Hann and Scrivani 1987 (FRL Research Bulletin 59). Growth effective age is introduced in 
        /// Hann and Ritchie 1988 (Height Growth Rate of Douglas-Fir: A Comparison of Model Forms. Forest Science 34(1):165–175).
        /// </remarks>
        public static void GetDouglasFirPonderosaHeightGrowth(bool isDouglasFir, float SI, float HT, out float GEAGE, out float PHTGRO)
        {
            // range of regression validity is undocumented, assume at least Organon 2.2.4 minimum height is required
            // Shorter trees can cause the growth effective age to become imaginary.
            Debug.Assert(HT >= 4.5F);

            // BUGBUG these are a0, a1, and a2 in the paper
            float B0;
            float B1;
            float B2;
            if (isDouglasFir)
            {
                // PSME
                B0 = -6.21693F;
                B1 = 0.281176F;
                B2 = 1.14354F;
            }
            else
            {
                // PIPO
                B0 = -6.54707F;
                B1 = 0.288169F;
                B2 = 1.21297F;
            }

            float BBC = B0 + B1 * MathV.Ln(SI);
            float X50 = 1.0F - MathV.Exp(-1.0F * MathV.Exp(BBC + B2 * 3.912023F));
            float A1A = 1.0F - (HT - 4.5F) * (X50 / SI);
            if (A1A <= 0.0F)
            {
                GEAGE = 500.0F;
                PHTGRO = 0.0F;
            }
            else
            {
                GEAGE = MathF.Pow(-1.0F * MathV.Ln(A1A) / (MathV.Exp(B0) * MathF.Pow(SI, B1)), 1.0F / B2);
                float XAI = 1.0F - MathV.Exp(-1.0F * MathV.Exp(BBC + B2 * MathV.Ln(GEAGE)));
                float XAI5 = 1.0F - MathV.Exp(-1.0F * MathV.Exp(BBC + B2 * MathV.Ln(GEAGE + 5.0F)));
                PHTGRO = 4.5F + (HT - 4.5F) * (XAI5 / XAI) - HT;
            }
        }

        // Collection of pre-calculable site constants used by Bruce 1981's height growth increments.
        public class SiteConstants
        {
            public float B1 { get; private init; }
            public float B2 { get; private init; }
            public float SiteIndexFromGround { get; private init; }
            public float X1 { get; private init; }
            public float X2toB2 { get; private init; }
            public float X2 { get; private init; }
            public float X3 { get; private init; }

            public SiteConstants(float siteIndexFromGroundInFeet)
            {
                if ((siteIndexFromGroundInFeet < Constant.Minimum.SiteIndexInFeet) || (siteIndexFromGroundInFeet > Constant.Maximum.SiteIndexInFeet))
                {
                    throw new ArgumentOutOfRangeException(nameof(siteIndexFromGroundInFeet));
                }

                this.X3 = siteIndexFromGroundInFeet / 100.0F;
                this.X2 = 63.25F - siteIndexFromGroundInFeet / 20.0F;
                this.X1 = 13.25F - siteIndexFromGroundInFeet / 20.0F;
                this.SiteIndexFromGround = siteIndexFromGroundInFeet;
                this.B2 = -0.447762F - 0.894427F * this.X3 + 0.793548F * this.X3 * this.X3 - 0.171666F * this.X3 * this.X3 * this.X3;
                this.X2toB2 = MathV.Pow(this.X2, this.B2);
                this.B1 = MathV.Ln(4.5F / siteIndexFromGroundInFeet) / (MathV.Pow(X1, B2) - this.X2toB2);
            }
        }
    }
}
