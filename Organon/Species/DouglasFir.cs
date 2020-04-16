using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Species
{
    internal class DouglasFir
    {
        /// <summary>
        /// Estimate growth effective age for Douglas-fir and grand fir using Bruce's (1981) dominant height model.
        /// </summary>
        /// <param name="siteIndexFromGround">Site index from ground in feet.</param>
        /// <param name="treeHeight">Tree height in feet.</param>
        /// <param name="GP"></param>
        /// <param name="growthEffectiveAge">Growth effective age in years.</param>
        /// <param name="potentialHeightGrowth"></param>
        /// <remarks>
        /// Bruce. 1981. Forest Science 27:711-725.
        /// </remarks>
        public static void BrucePsmeAbgrGrowthEffectiveAge(float siteIndexFromGround, float treeHeight, float GP, out float growthEffectiveAge, out float potentialHeightGrowth)
        {
            float X1 = 13.25F - siteIndexFromGround / 20.0F;
            float X2 = 63.25F - siteIndexFromGround / 20.0F;
            float B2 = -0.447762F - 0.894427F * siteIndexFromGround / 100.0F + 0.793548F * (siteIndexFromGround / 100.0F) * (siteIndexFromGround / 100.0F) - 0.171666F * (siteIndexFromGround / 100.0F) * (siteIndexFromGround / 100.0F) * (siteIndexFromGround / 100.0F);
            float B1 = MathV.Ln(4.5F / siteIndexFromGround) / (MathV.Pow(X1, B2) - MathV.Pow(X2, B2));
            float XX1 = MathV.Ln(treeHeight / siteIndexFromGround) / B1 + MathV.Pow(X2, B2);
            if (XX1 > 0.0F)
            {
                growthEffectiveAge = MathF.Pow(XX1, 1.0F / B2) - X1;
            }
            else
            {
                growthEffectiveAge = 500.0F;
            }
            float potentialHeight = siteIndexFromGround * MathV.Exp(B1 * (MathF.Pow(growthEffectiveAge + GP + X1, B2) - MathF.Pow(X2, B2)));
            potentialHeightGrowth = potentialHeight - treeHeight;
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
        public static void DouglasFirPonderosaHeightGrowth(bool isDouglasFir, float SI, float HT, out float GEAGE, out float PHTGRO)
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
    }
}
