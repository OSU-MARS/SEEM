using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class DiameterGrowth
    {
        public static void DIAMGRO(OrganonVariant variant, int treeIndex, int simulationStep, Stand stand, float SI_1, float SI_2, 
                                   StandDensity densityBeforeGrowth, Dictionary<FiaCode, float[]> CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT)
        {
            // CALCULATES FIVE-YEAR DIAMETER GROWTH RATE OF THE K-TH TREE
            // CALCULATE BASAL AREA IN LARGER TREES
            float dbhInInches = stand.Dbh[treeIndex];
            float SBAL1 = densityBeforeGrowth.GetBasalAreaLarger(dbhInInches);

            FiaCode species = stand.Species[treeIndex];
            float SITE;
            switch(variant.Variant)
            {
                case Variant.Swo:
                    SITE = SI_1;
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        SITE = SI_2;
                    }
                    else
                    {
                        SITE = SI_1;
                    }
                    break;
               case Variant.Rap:
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        SITE = SI_2;
                    }
                    else
                    {
                        SITE = SI_1;
                    }
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledVariantException(variant.Variant);
            }

            // CALCULATE DIAMETER GROWTH RATE FOR UNTREATED TREES
            float crownRatio = stand.CrownRatio[treeIndex];
            float dbhGrowthInInches = variant.GrowDiameter(species, dbhInInches, crownRatio, SITE, SBAL1, densityBeforeGrowth);

            // CALCULATE FERTILIZER ADJUSTMENT
            DG_FERT(species, variant, simulationStep, SI_1, PN, YF, out float FERTADJ);
            // CALCULATE THINNING ADJUSTMENT
            DG_THIN(species, variant, simulationStep, BABT, BART, YT, out float THINADJ);
            // CALCULATE DIAMETER GROWTH RATE FOR UNTREATED OR TREATED TREES
            dbhGrowthInInches *= CALIB[species][2] * FERTADJ * THINADJ;
            stand.DbhGrowth[treeIndex] = dbhGrowthInInches;
        }

        /// <summary>
        /// Find diameter growth multiplier for thinning.
        /// </summary>
        /// <param name="species">FIA species code.</param>
        /// <param name="variant">Organon variant.</param>
        /// <param name="simulationStep">Simulation cycle.</param>
        /// <param name="BABT">Basal area before thinning? (DOUG?)</param>
        /// <param name="BART">Basal area removed by thinning? (DOUG?)</param>
        /// <param name="YT">Thinning year data? (DOUG?)</param>
        /// <param name="THINADJ">Thinning adjustment. (DOUG?)</param>
        /// <remarks>
        /// Has special cases for Douglas-fir, western hemlock, and red alder (only for RAP).
        /// </remarks>
        private static void DG_THIN(FiaCode species, OrganonVariant variant, int simulationStep, float BABT, float[] BART, float[] YT, out float THINADJ)
        {
            // CALCULATE THINNING ADJUSTMENT FOR DIAMETER GROWTH RATE FROM
            // HANN ET AL.(2003) FRL RESEARCH CONTRIBUTION 40
            //
            // SET PARAMETERS FOR ADJUSTMENT
            float PT1;
            float PT2;
            float PT3;
            if (species == FiaCode.TsugaHeterophylla)
            {
                PT1 = 0.723095045F;
                PT2 = 1.0F;
                PT3 = -0.2644085320F;
            }
            else if (species == FiaCode.PseudotsugaMenziesii)
            {
                PT1 = 0.6203827985F;
                PT2 = 1.0F;
                PT3 = -0.2644085320F;
            }
            else if ((variant.Variant == Variant.Rap) && (species == FiaCode.AlnusRubra))
            {
                PT1 = 0.0F;
                PT2 = 1.0F;
                PT3 = 0.0F;
            }
            else 
            {
                PT1 = 0.6203827985F;
                PT2 = 1.0F;
                PT3 = -0.2644085320F;
            }

            float XTIME = Constant.DefaultTimeStepInYears * (float)simulationStep;
            float THINX1 = 0.0F;
            for (int I = 1; I < 5; ++I)
            {
                THINX1 += BART[I] * (float)Math.Exp((PT3 / PT2) * (YT[1] - YT[I]));
            }
            float THINX2 = THINX1 + BART[0];
            float THINX3 = THINX1 + BABT;

            float PREM;
            if (THINX3 <= 0.0F)
            {
                PREM = 0.0F;
            }
            else 
            {
                PREM = THINX2 / THINX3;
            }
            if (PREM > 0.75F)
            {
                PREM = 0.75F;
            }

            THINADJ = 1.0F + (float)(PT1 * Math.Pow(PREM, PT2) * Math.Exp(PT3 * (XTIME - YT[1])));
            Debug.Assert(THINADJ >= 1.0F);
        }

        private static void DG_FERT(FiaCode species, OrganonVariant variant, int simulationStep, float SI_1, float[] PN, float[] YF, out float FERTADJ)
        {
            // CALCULATE FERTILIZER ADJUSTMENT FOR DIAMETER GROWTH RATE
            // FROM HANN ET AL.(2003) FRL RESEARCH CONTRIBUTION 40
            // SET PARAMETERS FOR ADJUSTMENT
            float PF1;
            float PF2;
            float PF3;
            float PF4;
            float PF5;
            if (variant.Variant != Variant.Rap)
            {
                if (species == FiaCode.TsugaHeterophylla)
                {
                    PF1 = 0.0F;
                    PF2 = 1.0F;
                    PF3 = 0.0F;
                    PF4 = 0.0F;
                    PF5 = 1.0F;
                }
                else if (species == FiaCode.PseudotsugaMenziesii)
                {
                    PF1 = 1.368661121F;
                    PF2 = 0.741476964F;
                    PF3 = -0.214741684F;
                    PF4 = -0.851736558F;
                    PF5 = 2.0F;
                }
                else
                {
                    PF1 = 0.0F;
                    PF2 = 1.0F;
                    PF3 = 0.0F;
                    PF4 = 0.0F;
                    PF5 = 1.0F;
                }
            }
            else
            {
                PF1 = 0.0F;
                PF2 = 1.0F;
                PF3 = 0.0F;
                PF4 = 0.0F;
                PF5 = 1.0F;
            }
                
            float FALDWN = 1.0F;
            float XTIME = Constant.DefaultTimeStepInYears * (float)simulationStep;
            float FERTX1 = 0.0F;
            for (int I = 1; I < 5; ++I)
            {
                FERTX1 += (PN[I] / 800.0F) * (float)Math.Exp((PF3 / PF2) * (YF[1] - YF[I]));
            }

            float FERTX2 = (float)Math.Exp(PF3 * (XTIME - YF[1]) + Math.Pow(PF4 * (SI_1 / 100.0), PF5));
            FERTADJ = 1.0F + (float)(PF1 * Math.Pow((PN[1] / 800.0) + FERTX1, PF2) * FERTX2) * FALDWN;
            Debug.Assert(FERTADJ >= 1.0F);
        }
    }
}
