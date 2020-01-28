using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
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
            float B2 = -0.447762F - 0.894427F * siteIndexFromGround / 100.0F + 0.793548F * (float)Math.Pow(siteIndexFromGround / 100.0, 2.0) - 0.171666F * (float)Math.Pow(siteIndexFromGround / 100.0, 3.0);
            float B1 = (float)(Math.Log(4.5 / siteIndexFromGround) / (Math.Pow(X1, B2) - Math.Pow(X2, B2)));
            float XX1 = (float)(Math.Log(treeHeight / siteIndexFromGround) / B1 + Math.Pow(X2, B2));
            if (XX1 > 0.0F)
            {
                growthEffectiveAge = (float)Math.Pow(XX1, 1.0 / B2) - X1;
            }
            else
            {
                growthEffectiveAge = 500.0F;
            }
            float potentialHeight = siteIndexFromGround * (float)Math.Exp(B1 * (Math.Pow(growthEffectiveAge + GP + X1, B2) - Math.Pow(X2, B2)));
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

            float BBC = B0 + B1 * (float)Math.Log(SI);
            float X50 = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * 3.912023F));
            float A1A = 1.0F - (HT - 4.5F) * (X50 / SI);
            if (A1A <= 0.0F)
            {
                GEAGE = 500.0F;
                PHTGRO = 0.0F;
            }
            else
            {
                GEAGE = (float)Math.Pow(-1.0F * Math.Log(A1A) / (Math.Exp(B0) * Math.Pow(SI, B1)), 1.0F / B2);
                float XAI = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * Math.Log(GEAGE)));
                float XAI5 = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * Math.Log(GEAGE + 5.0)));
                PHTGRO = 4.5F + (HT - 4.5F) * (XAI5 / XAI) - HT;
            }
        }
    }
}
