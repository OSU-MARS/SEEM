using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm.Species
{
    public class DouglasFir
    {
        /// <summary>
        /// Estimate growth effective age for Douglas-fir and grand fir using Bruce's (1981) dominant height model.
        /// </summary>
        /// <param name="site">Site growth constants.</param>
        /// <param name="treeHeight">Tree height in feet.</param>
        /// <param name="potentialHeightGrowth"></param>
        /// <returns>Growth effective age in years.</returns>
        /// <remarks>
        /// Bruce. 1981. Forest Science 27:711-725.
        /// </remarks>
        public static float GetBrucePsmeAbgrGrowthEffectiveAge(SiteConstants site, float timeStepInYears, float treeHeight, out float potentialHeightGrowth)
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

        public static Vector128<float> GetBrucePsmeAbgrGrowthEffectiveAge(SiteConstants site, float timeStepInYears, Vector128<float> treeHeight, out Vector128<float> potentialHeightGrowth)
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
            public float B1 { get; private set; }
            public float B2 { get; private set; }
            public float SiteIndexFromGround { get; private set; }
            public float X1 { get; private set; }
            public float X2toB2 { get; private set; }
            public float X2 { get; private set; }
            public float X3 { get; private set; }

            public SiteConstants(float siteIndexFromGround)
            {
                this.X3 = siteIndexFromGround / 100.0F;
                this.X2 = 63.25F - siteIndexFromGround / 20.0F;
                this.X1 = 13.25F - siteIndexFromGround / 20.0F;
                this.SiteIndexFromGround = siteIndexFromGround;
                this.B2 = -0.447762F - 0.894427F * this.X3 + 0.793548F * this.X3 * this.X3 - 0.171666F * this.X3 * this.X3 * this.X3;
                this.X2toB2 = MathV.Pow(this.X2, this.B2);
                this.B1 = MathV.Ln(4.5F / siteIndexFromGround) / (MathV.Pow(X1, B2) - this.X2toB2);
            }
        }
    }
}
