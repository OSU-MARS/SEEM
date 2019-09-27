using System;

namespace Osu.Cof.Organon
{
    internal class GrowthModifiers
    {
        /// <summary>
        /// Get genetic diameter and height growth modifiers.
        /// </summary>
        /// <param name="TAGE">Tree age in years? (DOUG?)</param>
        /// <param name="GWDG">Genetic diameter gain factor. (DOUG?)</param>
        /// <param name="GWHG">Genetic height gain factor. (DOUG?)</param>
        /// <param name="DGMOD">Diameter growth modifier.</param>
        /// <param name="HGMOD">Height growth modifier.</param>
        public static void GG_MODS(float TAGE, float GWDG, float GWHG, out float DGMOD, out float HGMOD)
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
            float A2 = 0.0031F;                          // VALUE FOR TAGE => 10
            float A;
            if (TAGE <= 5.0F)
            {
                A = A1;
            }
            else if (TAGE > 5.0F && TAGE < 10.0F)
            {
                A = A1 - (A1 - A2) * ((TAGE - 5.0F) / 5.0F);
            }
            else
            {
                A = A2;
            }

            // SET THE PARAMETERS FOR THE HEIGHT GROWTH MODIFIER
            float B1 = 0.0062770F;                      // VALUE FOR TAGE = 5
            float B2 = 0.0036F;                         // VALUE FOR TAGE => 10
            float B;
            if (TAGE <= 5.0F)
            {
                B = B1;
            }
            else
            {
                if (TAGE > 5.0F && TAGE < 10.0F)
                {
                    B = B1 - (B1 - B2) * ((TAGE - 5.0F) / 5.0F);
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
        public static void SNC_MODS(float FR, out float DGMOD, out float HGMOD)
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
            DGMOD = 1.0F - (float)Math.Exp(A1 * Math.Pow(XFR, A2));
            // SWISS NEEDLE CAST HEIGHT GROWTH RATE MODIFIER
            HGMOD = 1.0F - (float)Math.Exp(B1 * Math.Pow(XFR, B2));
        }
    }
}