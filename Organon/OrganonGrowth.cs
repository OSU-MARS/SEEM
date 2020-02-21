using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class OrganonGrowth
    {
        public static float GetCrownRatioAdjustment(float crownRatio)
        {
            return 1.0F - (float)Math.Exp(-(25.0 * 25.0 * crownRatio * crownRatio));
        }

        private float GetDiameterFertilizationAdjustment(FiaCode species, OrganonVariant variant, int simulationStep, float SI_1, float[] PN, float[] YF)
        {
            // CALCULATE FERTILIZER ADJUSTMENT FOR DIAMETER GROWTH RATE
            // FROM HANN ET AL.(2003) FRL RESEARCH CONTRIBUTION 40
            // SET PARAMETERS FOR ADJUSTMENT
            float PF1;
            float PF2;
            float PF3;
            float PF4;
            float PF5;
            if (variant.TreeModel != TreeModel.OrganonRap)
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
            float FERTADJ = 1.0F + (float)(PF1 * Math.Pow((PN[1] / 800.0) + FERTX1, PF2) * FERTX2) * FALDWN;
            Debug.Assert(FERTADJ >= 1.0F);
            return FERTADJ;
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
        private float GetDiameterThinningAdjustment(FiaCode species, OrganonVariant variant, int simulationStep, float BABT, float[] BART, float[] YT)
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
            else if ((variant.TreeModel == TreeModel.OrganonRap) && (species == FiaCode.AlnusRubra))
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

            float THINADJ = 1.0F + (float)(PT1 * Math.Pow(PREM, PT2) * Math.Exp(PT3 * (XTIME - YT[1])));
            Debug.Assert(THINADJ >= 1.0F);
            return THINADJ;
        }

        private float GetHeightFertilizationAdjustment(int simulationStep, OrganonVariant variant, FiaCode species, float siteIndexFromBreastHeight, float[] PN, float[] YF)
        {
            float PF1;
            float PF2;
            float PF3;
            float PF4;
            float PF5;
            if (variant.TreeModel != TreeModel.OrganonRap)
            {
                if (species == FiaCode.PseudotsugaMenziesii)
                {
                    PF1 = 1.0F;
                    PF2 = 0.333333333F;
                    PF3 = -1.107409443F;
                    PF4 = -2.133334346F;
                    PF5 = 1.5F;
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
                FERTX1 += (PN[I] / 800.0F) * (float)Math.Exp((PF3 / PF2) * (YF[0] - YF[I]));
            }
            float FERTX2 = (float)Math.Exp(PF3 * (XTIME - YF[0]) + PF4 * Math.Pow(siteIndexFromBreastHeight / 100.0, PF5));
            float FERTADJ = 1.0F + PF1 * (float)(Math.Pow(PN[0] / 800.0 + FERTX1, PF2) * FERTX2) * FALDWN;
            Debug.Assert(FERTADJ >= 1.0F);
            return FERTADJ;
        }

        private float GetHeightThinningAdjustment(int simulationStep, OrganonVariant variant, FiaCode species, float BABT, float[] BART, float[] YT)
        {
            float PT1;
            float PT2;
            float PT3;
            if (variant.TreeModel != TreeModel.OrganonRap)
            {
                if (species == FiaCode.PseudotsugaMenziesii)
                {
                    PT1 = -0.3197415492F;
                    PT2 = 0.7528887377F;
                    PT3 = -0.2268800162F;
                }
                else
                {
                    PT1 = 0.0F;
                    PT2 = 1.0F;
                    PT3 = 0.0F;
                }
            }
            else
            {
                if (species == FiaCode.AlnusRubra)
                {
                    PT1 = -0.613313694F;
                    PT2 = 1.0F;
                    PT3 = -0.443824038F;
                }
                else if (species == FiaCode.PseudotsugaMenziesii)
                {
                    PT1 = -0.3197415492F;
                    PT2 = 0.7528887377F;
                    PT3 = -0.2268800162F;
                }
                else
                {
                    PT1 = 0.0F;
                    PT2 = 1.0F;
                    PT3 = 0.0F;
                }
            }
            float XTIME = 5.0F * (float)simulationStep;
            float THINX1 = 0.0F;
            for (int I = 1; I < 5; ++I)
            {
                THINX1 += BART[I] * (float)Math.Exp((PT3 / PT2) * (YT[0] - YT[0]));
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
            float THINADJ = 1.0F + PT1 * (float)(Math.Pow(PREM, PT2) * Math.Exp(PT3 * (XTIME - YT[0])));
            Debug.Assert(THINADJ >= 0.0F);
            return THINADJ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulationStep"></param>
        /// <param name="configuration">Organon configuration settings.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="TCYCLE"></param>
        /// <param name="FCYCLE"></param>
        /// <param name="CALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YT"></param>
        /// <param name="CCH"></param>
        /// <param name="OLD"></param>
        /// <param name="RAAGE"></param>
        public void Grow(ref int simulationStep, OrganonConfiguration configuration, Stand stand,
                         ref int TCYCLE, ref int FCYCLE, StandDensity densityBeforeGrowth,
                         Dictionary<FiaCode, float[]> CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, 
                         ref float[] CCH, ref float OLD, float RAAGE, out StandDensity densityAfterGrowth)
        {
            float DGMOD_GG = 1.0F;
            float HGMOD_GG = 1.0F;
            float DGMOD_SNC = 1.0F;
            float HGMOD_SNC = 1.0F;
            if ((stand.AgeInYears > 0) && configuration.Genetics)
            {
                OrganonGrowthModifiers.GG_MODS((float)stand.AgeInYears, configuration.GWDG, configuration.GWHG, out DGMOD_GG, out HGMOD_GG);
            }
            if (configuration.SwissNeedleCast && (configuration.Variant.TreeModel == TreeModel.OrganonNwo || configuration.Variant.TreeModel == TreeModel.OrganonSmc))
            {
                OrganonGrowthModifiers.SNC_MODS(configuration.FR, out DGMOD_SNC, out HGMOD_SNC);
            }

            // diameter growth
            int treeRecordsWithExpansionFactorZero = 0;
            int bigSixRecordsWithExpansionFactorZero = 0;
            int otherSpeciesRecordsWithExpansionFactorZero = 0;
            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    ++treeRecordsWithExpansionFactorZero;
                    if (configuration.Variant.IsBigSixSpecies(stand.Species[treeIndex]))
                    {
                        ++bigSixRecordsWithExpansionFactorZero;
                    }
                    else
                    {
                        ++otherSpeciesRecordsWithExpansionFactorZero;
                    }
                }
            }

            // BUGBUG no check that SITE_1 and SITE_2 indices are greater than 4.5 feet
            float SI_1 = stand.SiteIndex - 4.5F;
            float SI_2 = stand.HemlockSiteIndex - 4.5F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    stand.DbhGrowth[treeIndex] = 0.0F;
                }
                else
                {
                    this.GrowDiameter(configuration.Variant, treeIndex, simulationStep, stand, SI_1, SI_2, densityBeforeGrowth, CALIB, PN, YF, BABT, BART, YT);
                    if (stand.Species[treeIndex] == FiaCode.PseudotsugaMenziesii)
                    {
                        stand.DbhGrowth[treeIndex] = stand.DbhGrowth[treeIndex] * DGMOD_GG * DGMOD_SNC;
                    }
                }
            }

            // height growth for big six species
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (configuration.Variant.IsBigSixSpecies(stand.Species[treeIndex]))
                {
                    if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        stand.HeightGrowth[treeIndex] = 0.0F;
                    }
                    else
                    {
                        this.GrowHeightBigSixSpecies(treeIndex, configuration.Variant, simulationStep, stand, SI_1, SI_2, CCH, PN, YF, BABT, BART, YT, ref OLD, configuration.PDEN);
                        FiaCode species = stand.Species[treeIndex];
                        if (species == FiaCode.PseudotsugaMenziesii)
                        {
                            stand.HeightGrowth[treeIndex] = stand.HeightGrowth[treeIndex] * HGMOD_GG * HGMOD_SNC;
                        }
                    }
                }
            }

            // determine mortality
            // Sets configuration.NO.
            OrganonMortality.MORTAL(configuration, simulationStep, stand, densityBeforeGrowth, SI_1, SI_2, PN, YF, ref RAAGE);

            // grow tree diameters
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                stand.Dbh[treeIndex] += stand.DbhGrowth[treeIndex];
            }

            // CALC EOG SBA, CCF/TREE, CCF IN LARGER TREES AND STAND CCF
            densityAfterGrowth = new StandDensity(stand, configuration.Variant);

            // CALCULATE HTGRO FOR 'OTHER' & CROWN ALL SPECIES
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                if (configuration.Variant.IsBigSixSpecies(stand.Species[treeIndex]) == false)
                {
                    if (stand.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        stand.HeightGrowth[treeIndex] = 0.0F;
                    }
                    else
                    {
                        this.GrowHeightMinorSpecies(treeIndex, configuration.Variant, stand, CALIB);
                    }
                }
                stand.Height[treeIndex] += stand.HeightGrowth[treeIndex];
            }

            // grow crowns
            CCH = this.GrowCrown(configuration.Variant, stand, densityBeforeGrowth, densityAfterGrowth, SI_1, SI_2, CALIB);

            // update stand variables
            if (configuration.Variant.TreeModel != TreeModel.OrganonRap)
            {
                stand.AgeInYears += 5;
                stand.BreastHeightAgeInYears += 5;
            }
            else
            {
                ++stand.AgeInYears;
                ++stand.BreastHeightAgeInYears;
            }
            ++simulationStep;
            if (FCYCLE > 2)
            {
                FCYCLE = 0;
            }
            else if (FCYCLE > 0)
            {
                ++FCYCLE;
            }
            if (TCYCLE > 0)
            {
                ++TCYCLE;
            }

            // reduce calibration ratios
            foreach (float[] speciesCalibration in CALIB.Values)
            {
                for (int index = 0; index < 3; ++index)
                {
                    if (speciesCalibration[index] != 1.0F)
                    {
                        float MCALIB = (1.0F + speciesCalibration[index + 2]) / 2.0F;
                        speciesCalibration[index] = MCALIB + (float)Math.Sqrt(0.5) * (speciesCalibration[index] - MCALIB);
                    }
                }
            }
        }

        public float[] GrowCrown(OrganonVariant variant, Stand stand, StandDensity densityBeforeGrowth, StandDensity densityAfterGrowth,
                                   float SI_1, float SI_2, Dictionary<FiaCode, float[]> CALIB)
        {
            // DETERMINE 5-YR CROWN RECESSION
            OrganonMortality.OldGro(variant, stand, -1.0F, out float OG1);
            OrganonMortality.OldGro(variant, stand, 0.0F, out float OG2);
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
                if (variant.TreeModel == TreeModel.OrganonNwo)
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
                if (variant.TreeModel == TreeModel.OrganonNwo)
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

            return StandDensity.GetCrownCompetitionByHeight(variant, stand);
        }

        public void GrowDiameter(OrganonVariant variant, int treeIndex, int simulationStep, Stand stand, float SI_1, float SI_2,
                                StandDensity densityBeforeGrowth, Dictionary<FiaCode, float[]> CALIB, float[] PN, float[] YF, float BABT, float[] BART, float[] YT)
        {
            // CALCULATES FIVE-YEAR DIAMETER GROWTH RATE OF THE K-TH TREE
            // CALCULATE BASAL AREA IN LARGER TREES
            float dbhInInches = stand.Dbh[treeIndex];
            float SBAL1 = densityBeforeGrowth.GetBasalAreaLarger(dbhInInches);

            FiaCode species = stand.Species[treeIndex];
            float SITE;
            switch (variant.TreeModel)
            {
                case TreeModel.OrganonSwo:
                    SITE = SI_1;
                    break;
                case TreeModel.OrganonNwo:
                case TreeModel.OrganonSmc:
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        SITE = SI_2;
                    }
                    else
                    {
                        SITE = SI_1;
                    }
                    break;
                case TreeModel.OrganonRap:
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
                    throw OrganonVariant.CreateUnhandledModelException(variant.TreeModel);
            }

            // CALCULATE DIAMETER GROWTH RATE FOR UNTREATED TREES
            float crownRatio = stand.CrownRatio[treeIndex];
            float dbhGrowthInInches = variant.GrowDiameter(species, dbhInInches, crownRatio, SITE, SBAL1, densityBeforeGrowth);

            // CALCULATE FERTILIZER ADJUSTMENT
            float FERTADJ = GetDiameterFertilizationAdjustment(species, variant, simulationStep, SI_1, PN, YF);
            // CALCULATE THINNING ADJUSTMENT
            float THINADJ = GetDiameterThinningAdjustment(species, variant, simulationStep, BABT, BART, YT);
            // CALCULATE DIAMETER GROWTH RATE FOR UNTREATED OR TREATED TREES
            dbhGrowthInInches *= CALIB[species][2] * FERTADJ * THINADJ;
            stand.DbhGrowth[treeIndex] = dbhGrowthInInches;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeIndex">Index of tree to grow in tree data.</param>
        /// <param name="variant">Organon variant.</param>
        /// <param name="simulationStep">Simulation cycle.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="SI_1">Primary site index from breast height.</param>
        /// <param name="SI_2">Secondary site index from breast height.</param>
        /// <param name="CCH">Canopy height? (DOUG?)</param>
        /// <param name="PN"></param>
        /// <param name="YF"></param>
        /// <param name="BABT">Thinning?</param>
        /// <param name="BART">Thinning?</param>
        /// <param name="YT">Thinning?</param>
        /// <param name="OLD">Obsolete?</param>
        /// <param name="PDEN">(DOUG?)</param>
        public void GrowHeightBigSixSpecies(int treeIndex, OrganonVariant variant, int simulationStep, Stand stand, float SI_1, float SI_2,
                                                   float[] CCH, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, ref float OLD, float PDEN)
        {
            Debug.Assert(variant.IsBigSixSpecies(stand.Species[treeIndex]));
            // BUGBUG remove M and ON
            // CALCULATE 5-YEAR HEIGHT GROWTH
            float CR = stand.CrownRatio[treeIndex];

            // FOR MAJOR SPECIES
            float XI = 40.0F * (stand.Height[treeIndex] / CCH[40]);
            int I = (int)XI + 2;
            float XXI = (float)I - 1.0F;
            float TCCH;
            if (stand.Height[treeIndex] >= CCH[40])
            {
                TCCH = 0.0F;
            }
            else if (I == 41)
            {
                TCCH = CCH[39] * (40.0F - XI);
            }
            else
            {
                TCCH = CCH[I] + (CCH[I - 1] - CCH[I]) * (XXI - XI);
            }

            // COMPUTE HEIGHT GROWTH OF UNTREATED TREES
            // IDXAGE index age? (DOUG?)
            // BUGBUG: move old index age to variant capabilities
            FiaCode species = stand.Species[treeIndex];
            float growthEffectiveAge;
            float potentialHeightGrowth;
            float oldIndexAge;
            switch (variant.TreeModel)
            {
                case TreeModel.OrganonSwo:
                    float siteIndexFromGround = SI_1;
                    bool treatAsDouglasFir = false;
                    // POTENTIAL HEIGHT GROWTH FROM HANN AND SCRIVANI'S (1987) DOMINANT HEIGHT GROWTH EQUATION
                    if (species == FiaCode.PinusPonderosa)
                    {
                        siteIndexFromGround = SI_2;
                    }
                    else
                    {
                        if (species == FiaCode.CalocedrusDecurrens)
                        {
                            siteIndexFromGround = (SI_1 + 4.5F) * 0.66F - 4.5F;
                        }
                        treatAsDouglasFir = true;
                    }
                    DouglasFir.DouglasFirPonderosaHeightGrowth(treatAsDouglasFir, siteIndexFromGround, stand.Height[treeIndex], out growthEffectiveAge, out potentialHeightGrowth);
                    oldIndexAge = 500.0F;
                    break;
                case TreeModel.OrganonNwo:
                    float GP = 5.0F;
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH
                        siteIndexFromGround = SI_2 + 4.5F;
                        WesternHemlock.F_HG(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    else
                    {
                        // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                        siteIndexFromGround = SI_1 + 4.5F;
                        DouglasFir.BrucePsmeAbgrGrowthEffectiveAge(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    oldIndexAge = 120.0F;
                    break;
                case TreeModel.OrganonSmc:
                    GP = 5.0F;
                    if (species == FiaCode.TsugaHeterophylla)
                    {
                        // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK
                        // DOMINANT HEIGHT GROWTH
                        siteIndexFromGround = SI_2 + 4.5F;
                        WesternHemlock.F_HG(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    else
                    {
                        // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                        siteIndexFromGround = SI_1 + 4.5F;
                        DouglasFir.BrucePsmeAbgrGrowthEffectiveAge(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    oldIndexAge = 120.0F;
                    break;
                case TreeModel.OrganonRap:
                    GP = 1.0F;
                    if (species == FiaCode.AlnusRubra)
                    {
                        // POTENTIAL HEIGHT GROWTH FROM WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM(2009) RED ALDER TOP HEIGHT GROWTH
                        siteIndexFromGround = SI_1 + 4.5F;
                        RedAlder.WHHLB_HG(siteIndexFromGround, PDEN, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    else if (species == FiaCode.TsugaHeterophylla)
                    {
                        // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH
                        siteIndexFromGround = -0.432F + 0.899F * (SI_2 + 4.5F);
                        WesternHemlock.F_HG(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    else
                    {
                        // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                        siteIndexFromGround = SI_2 + 4.5F;
                        DouglasFir.BrucePsmeAbgrGrowthEffectiveAge(siteIndexFromGround, stand.Height[treeIndex], GP, out growthEffectiveAge, out potentialHeightGrowth);
                    }
                    oldIndexAge = 30.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledModelException(variant.TreeModel);
            }

            float heightGrowthInFeet = variant.GrowHeightBigSix(species, potentialHeightGrowth, CR, TCCH);
            if (variant.IsBigSixSpecies(species) && (growthEffectiveAge > oldIndexAge))
            {
                OLD += 1.0F;
            }

            float FERTADJ = GetHeightFertilizationAdjustment(simulationStep, variant, species, SI_1, PN, YF);
            float THINADJ = GetHeightThinningAdjustment(simulationStep, variant, species, BABT, BART, YT);
            stand.HeightGrowth[treeIndex] = heightGrowthInFeet * THINADJ * FERTADJ;
            this.LimitHeightGrowth(variant, species, stand.Dbh[treeIndex], stand.Height[treeIndex], stand.DbhGrowth[treeIndex], ref stand.HeightGrowth[treeIndex]);
        }

        public void GrowHeightMinorSpecies(int treeIndex, OrganonVariant variant, Stand stand, Dictionary<FiaCode, float[]> CALIB)
        {
            float dbhInInches = stand.Dbh[treeIndex];
            FiaCode species = stand.Species[treeIndex];
            Debug.Assert(variant.IsBigSixSpecies(species) == false);

            float previousDbhInInches = dbhInInches - stand.DbhGrowth[treeIndex];
            float PRDHT1 = variant.GetPredictedHeight(species, previousDbhInInches);
            float PRDHT2 = variant.GetPredictedHeight(species, dbhInInches);
            PRDHT1 = 4.5F + CALIB[species][0] * (PRDHT1 - 4.5F);
            PRDHT2 = 4.5F + CALIB[species][0] * (PRDHT2 - 4.5F);
            float PRDHT = (PRDHT2 / PRDHT1) * stand.Height[treeIndex];

            // RED ALDER HEIGHT GROWTH
            if ((species == FiaCode.AlnusRubra) && (variant.TreeModel != TreeModel.OrganonRap))
            {
                float growthEffectiveAge = RedAlder.GetGrowthEffectiveAge(stand.Height[treeIndex], stand.RedAlderSiteIndex);
                if (growthEffectiveAge <= 0.0F)
                {
                    stand.HeightGrowth[treeIndex] = 0.0F;
                }
                else
                {
                    // BUGBUG: this is strange as it appears to assume red alders are always dominant trees
                    float RAH1 = RedAlder.GetH50(growthEffectiveAge, stand.RedAlderSiteIndex);
                    float RAH2 = RedAlder.GetH50(growthEffectiveAge + Constant.DefaultTimeStepInYears, stand.RedAlderSiteIndex);
                    float redAlderHeightGrowth = RAH2 - RAH1;
                    stand.HeightGrowth[treeIndex] = redAlderHeightGrowth;
                }
            }
            else
            {
                stand.HeightGrowth[treeIndex] = PRDHT - stand.Height[treeIndex];
            }
        }

        private void LimitHeightGrowth(OrganonVariant variant, FiaCode species, float DBH, float HT, float DG, ref float HG)
        {
            FiaCode speciesWithSwoTsheOptOut = species;
            if ((species == FiaCode.TsugaHeterophylla) && (variant.TreeModel == TreeModel.OrganonSwo))
            {
                // BUGBUG: not clear why SWO uses default coefficients for hemlock
                speciesWithSwoTsheOptOut = FiaCode.NotholithocarpusDensiflorus;
            }

            float A0;
            float A1;
            float A2;
            switch (speciesWithSwoTsheOptOut)
            {
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.TsugaHeterophylla:
                    A0 = 19.04942539F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    A0 = 16.26279948F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.PinusPonderosa:
                    A0 = 17.11482201F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.PinusLambertiana:
                    A0 = 14.29011403F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.AlnusRubra:
                    A0 = 60.619859F;
                    A1 = -1.59138564F;
                    A2 = 0.496705997F;
                    break;
                default:
                    A0 = 15.80319194F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
            }

            float HT1 = HT - 4.5F;
            float HT2 = HT1 + HG;
            float HT3 = HT2 + HG;
            float DBH1 = DBH;
            float DBH2 = DBH1 + DG;
            float DBH3 = DBH2 + DG;
            float PHT1 = A0 * DBH1 / (1.0F - A1 * (float)Math.Pow(DBH1, A2));
            float PHT2 = A0 * DBH2 / (1.0F - A1 * (float)Math.Pow(DBH2, A2));
            float PHT3 = A0 * DBH3 / (1.0F - A1 * (float)Math.Pow(DBH3, A2));
            float PHGR1 = (PHT2 - PHT1 + HG) / 2.0F;
            float PHGR2 = PHT2 - HT1;
            if (HT2 > PHT2)
            {
                if (PHGR1 < PHGR2)
                {
                    HG = PHGR1;
                }
                else
                {
                    HG = PHGR2;
                }
            }
            else
            {
                if (HT3 > PHT3)
                {
                    HG = PHGR1;
                }
            }
            if (HG < 0.0F)
            {
                HG = 0.0F;
            }
        }
    }
}
