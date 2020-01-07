using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class CrownGrowth
    {
        public static void CALC_CC(OrganonVariant variant, FiaCode species, float HLCW, float LCW, float HT, float DBH, float HCB, float EXPAN, float[] CCH)
        {
            float XHLCW;
            float XLCW;
            if (HCB > HLCW)
            {
                XHLCW = HCB;
                XLCW = variant.GetCrownWidth(species, HLCW, LCW, HT, DBH, XHLCW);
            }
            else
            {
                XHLCW = HLCW;
                XLCW = LCW;
            }
            for (int II = 39; II >= 0; --II)
            {
                int L = II - 1;
                float XL = (float)(L) * (CCH[40] / 40.0F);
                float CW;
                if (XL <= XHLCW)
                {
                    CW = XLCW;
                }
                else if (XL > XHLCW && XL < HT)
                {
                    CW = variant.GetCrownWidth(species, HLCW, LCW, HT, DBH, XL);
                }
                else
                {
                    CW = 0.0F;
                }
                float CA = (CW * CW) * (.001803F * EXPAN);
                CCH[II] = CCH[II] + CA;
            }
        }

        /// <summary>
        /// Find crown closure. (DOUG? can this be removed as dead code since the value of CC is never consumed?)
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="CCH"></param>
        /// <param name="CC">Crown closure</param>
        public static void CRNCLO(OrganonVariant variant, Stand stand, float[] CCH, out float CC)
        {
            for (int L = 0; L < 40; ++L)
            {
                CCH[L] = 0.0F;
            }
            CCH[40] = stand.Height[0];
            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.Height[treeIndex] > CCH[40])
                {
                    CCH[40] = stand.Height[treeIndex];
                }
            }

            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = stand.Species[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float crownRatio = stand.CrownRatio[treeIndex];
                float crownLengthInFeet = crownRatio * heightInFeet;
                float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                float MCW = variant.GetMaximumCrownWidth(species, dbhInInches, heightInFeet);
                float LCW = variant.GetLargestCrownWidth(species, MCW, crownRatio, dbhInInches, heightInFeet);
                float HLCW = variant.GetHeightToLargestCrownWidth(species, heightInFeet, crownRatio);
                CALC_CC(variant, species, HLCW, LCW, heightInFeet, dbhInInches, heightToCrownBaseInFeet, expansionFactor, CCH);
            }
            CC = CCH[0];
        }

        public static void CrowGro(OrganonVariant variant, Stand stand, StandDensity densityBeforeGrowth, StandDensity densityAfterGrowth,
                                   float SI_1, float SI_2, Dictionary<FiaCode, float[]> CALIB, float[] CCH)
        {
            // DETERMINE 5-YR CROWN RECESSION
            Mortality.OldGro(variant, stand, -1.0F, out float OG1);
            Mortality.OldGro(variant, stand, 0.0F, out float OG2);
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                // CALCULATE HCB START OF GROWTH
                // CALCULATE STARTING HEIGHT
                float PHT = stand.Height[treeIndex] - stand.HeightGrowth[treeIndex];
                // CALCULATE STARTING DBH
                float PDBH = stand.Dbh[treeIndex] - stand.DbhGrowth[treeIndex];
                FiaCode species = stand.Species[treeIndex];
                float SCCFL1 = densityBeforeGrowth.GetCrownCompetitionFactorLarger(PDBH);
                float HCB1 = variant.GetHeightToCrownBase(species, PHT, PDBH, SCCFL1, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1);
                float PCR1 = 1.0F - HCB1 / PHT;
                if (variant.Variant == Variant.Nwo)
                {
                    PCR1 = CALIB[species][1] * (1.0F - HCB1 / PHT);
                }

                float PHCB1 = (1.0F - PCR1) * PHT;

                // CALCULATE HCB END OF GROWTH
                float HT = stand.Height[treeIndex];
                float DBH = stand.Dbh[treeIndex];
                float SCCFL2 = densityAfterGrowth.GetCrownCompetitionFactorLarger(DBH);
                float HCB2 = variant.GetHeightToCrownBase(species, HT, DBH, SCCFL2, densityAfterGrowth.BasalAreaPerAcre, SI_1, SI_2, OG2);
                float MAXHCB = variant.GetMaximumHeightToCrownBase(species, HT, SCCFL2);
                float PCR2 = 1.0F - HCB2 / HT;
                if (variant.Variant == Variant.Nwo)
                {
                    PCR2 = CALIB[species][1] * (1.0F - HCB2 / HT);
                }

                float PHCB2 = (1.0F - PCR2) * HT;

                // DETERMINE CROWN GROWTH
                float HCBG = PHCB2 - PHCB1;
                if (HCBG < 0.0F)
                {
                    HCBG = 0.0F;
                }
                Debug.Assert(HCBG >= 0.0F); // catch NaNs

                float AHCB1 = (1.0F - stand.CrownRatio[treeIndex]) * PHT;
                float AHCB2 = AHCB1 + HCBG;
                if (AHCB1 >= MAXHCB)
                {
                    stand.CrownRatio[treeIndex] = 1.0F - AHCB1 / HT;
                }
                else if (AHCB2 >= MAXHCB)
                {
                    stand.CrownRatio[treeIndex] = 1.0F - MAXHCB / HT;
                }
                else
                {
                    stand.CrownRatio[treeIndex] = 1.0F - AHCB2 / HT;
                }
                Debug.Assert((stand.CrownRatio[treeIndex] >= 0.0F) && (stand.CrownRatio[treeIndex] <= 1.0F));
            }

            CRNCLO(variant, stand, CCH, out float _);
        }
    }
}
