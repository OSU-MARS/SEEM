using Osu.Cof.Ferm.Extensions;
using System;

namespace Osu.Cof.Ferm.Organon
{
    internal class OrganonGrowthModifiers
    {
        /// <summary>
        /// Get genetic diameter and height growth modifiers.
        /// </summary>
        /// <param name="treeAgeInYears">Tree age in years.</param>
        /// <param name="GWDG">Genetic diameter gain factor.</param>
        /// <param name="GWHG">Genetic height gain factor.</param>
        /// <param name="DGMOD">Diameter growth modifier.</param>
        /// <param name="HGMOD">Height growth modifier.</param>
        public static void GetGeneticModifiers(float treeAgeInYears, float GWDG, float GWHG, out float DGMOD, out float HGMOD)
        {
            float XGWHG = GWDG;
            if (GWHG > 20.0F)
            {
                XGWHG = 20.0F;
            }
            if (GWHG < 0.0F)
            {
                XGWHG = 0.0F;
            }
            float XGWDG = GWDG;
            if (GWDG > 20.0F)
            {
                XGWDG = 20.0F;
            }
            if (GWDG < 0.0F)
            {
                XGWDG = 0.0F;
            }

            // SET THE PARAMETERS FOR THE DIAMETER GROWTH MODIFIER
            float A1 = 0.0101054F; // VALUE FOR TAGE = 5
            float A2 = 0.0031F;    // VALUE FOR TAGE => 10
            float A;
            if (treeAgeInYears <= 5.0F)
            {
                A = A1;
            }
            else if ((treeAgeInYears > 5.0F) && (treeAgeInYears < 10.0F))
            {
                A = A1 - (A1 - A2) * (treeAgeInYears - 5.0F) / 5.0F;
            }
            else
            {
                A = A2;
            }

            // SET THE PARAMETERS FOR THE HEIGHT GROWTH MODIFIER
            float B1 = 0.0062770F;                      // VALUE FOR TAGE = 5
            float B2 = 0.0036F;                         // VALUE FOR TAGE => 10
            float B;
            if (treeAgeInYears <= 5.0F)
            {
                B = B1;
            }
            else
            {
                if ((treeAgeInYears > 5.0F) && (treeAgeInYears < 10.0F))
                {
                    B = B1 - (B1 - B2) * (treeAgeInYears - 5.0F) / 5.0F;
                }
                else
                {
                    B = B2;
                }
            }

            // GENETIC GAIN DIAMETER GROWTH RATE MODIFIER
            DGMOD = 1.0F + A * XGWDG;

            // GENETIC GAIN HEIGHT GROWTH RATE MODIFIER
            HGMOD = 1.0F + B * XGWHG;
        }

        /// <summary>
        /// Get Swiss needle cast diameter and height growth modifiers.
        /// </summary>
        /// <param name="FR">Foliage retention? (DOUG?)</param>
        /// <param name="DGMOD">Diameter growth modifier.</param>
        /// <param name="HGMOD">Height growth modifier.</param>
        public static void GetSwissNeedleCastModifiers(float FR, out float DGMOD, out float HGMOD)
        {
            float XFR = FR;
            if (FR > 4.0F)
            {
                XFR = 4.0F;
            }
            if (FR < 0.85F)
            {
                XFR = 0.85F;
            }

            // SET THE PARAMETERS FOR THE DIAMETER GROWTH MODIFIER
            float A1 = -0.5951664F;
            float A2 = 1.7121299F;
            // SET THE PARAMETERS FOR THE HEIGHT GROWTH MODIFIER
            float B1 = -1.0021090F;
            float B2 = 1.2801740F;

            // SWISS NEEDLE CAST DIAMETER GROWTH RATE MODIFIER
            DGMOD = 1.0F - MathV.Exp(A1 * MathF.Pow(XFR, A2));
            // SWISS NEEDLE CAST HEIGHT GROWTH RATE MODIFIER
            HGMOD = 1.0F - MathV.Exp(B1 * MathF.Pow(XFR, B2));
        }
    }
}