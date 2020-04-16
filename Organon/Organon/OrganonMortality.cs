using Osu.Cof.Ferm.Species;
using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    internal class OrganonMortality
    {
        // ROUTINE FOR SETTING TREE MORTALITY
        public static void MORTAL(OrganonConfiguration configuration, int simulationStep, OrganonStand stand, 
                                  OrganonStandDensity densityGrowth, float SI_1, float SI_2, OrganonTreatments treatments, ref float RAAGE)
        {
            float A3;
            if (configuration.Variant.TreeModel != TreeModel.OrganonRap)
            {
                A3 = 14.39533971F;
            }
            else
            {
                A3 = 3.88F;
            }
            float RDCC;
            if (configuration.Variant.TreeModel != TreeModel.OrganonRap)
            {
                RDCC = 0.60F;
            }
            else
            {
                RDCC = 0.5211F;
            }

            float standBasalArea = 0.0F;
            float standTreesPerAcre = 0.0F;
            float alnusRubraExpansionFactor = 0.0F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    standBasalArea += treesOfSpecies.GetBasalArea(treeIndex);
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    standTreesPerAcre += expansionFactor;

                    if ((treesOfSpecies.Species == FiaCode.AlnusRubra) && (configuration.Variant.TreeModel != TreeModel.OrganonRap))
                    {
                        // TODO: use species sum
                        alnusRubraExpansionFactor += expansionFactor;
                    }
                }
            }

            if (alnusRubraExpansionFactor <= 0.0001)
            {
                RAAGE = 0.0F;
            }
            float SQMDA = MathF.Sqrt(standBasalArea / (Constant.ForestersEnglish * standTreesPerAcre));
            float RD = standTreesPerAcre / MathV.Exp(stand.A1 / stand.A2 - MathV.Ln(SQMDA) / stand.A2);
            if (simulationStep == 0)
            {
                stand.RD0 = RD;
                stand.NO = 0.0F;
                stand.A1MAX = stand.A1;
            }
            float BAA = 0.0F;
            float NA = 0.0F;
            OldGro(configuration.Variant, stand, 0.0F, out float OG1);

            // INDIVIDUAL TREE MORTALITY EQUATIONS
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                FiaCode species = treesOfSpecies.Species;
                float[] PMK = new float[treesOfSpecies.Count];
                float[] POW = new float[treesOfSpecies.Count];
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    if (treesOfSpecies.LiveExpansionFactor[treeIndex] <= 0.0F)
                    {
                        continue;
                    }
                    float DBH = treesOfSpecies.Dbh[treeIndex];
                    float SBAL1 = densityGrowth.GetBasalAreaLarger(DBH);
                    float CR = treesOfSpecies.CrownRatio[treeIndex];
                    configuration.Variant.GetMortalityCoefficients(species, DBH, CR, SI_1, SI_2, SBAL1, OG1, out POW[treeIndex], out PMK[treeIndex]);
                    if (treatments.HasFertilization)
                    {
                        float FERTADJ = PM_FERT(species, configuration.Variant.TreeModel, simulationStep, treatments);
                        PMK[treeIndex] += FERTADJ;
                    }
                }

                if ((species == FiaCode.AlnusRubra) && (configuration.Variant.TreeModel != TreeModel.OrganonRap))
                {
                    if (RAAGE >= 55.0)
                    {
                        RedAlder.RAMORT(treesOfSpecies, RAAGE, alnusRubraExpansionFactor, PMK);
                    }
                    RAAGE += Constant.DefaultTimeStepInYears;
                }

                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float CR = treesOfSpecies.CrownRatio[treeIndex];
                    float CRADJ = OrganonGrowth.GetCrownRatioAdjustment(CR);
                    float XPM = 1.0F / (1.0F + MathV.Exp(-PMK[treeIndex]));
                    float PS = POW[treeIndex] == 1.0 ? 1.0F - XPM : MathV.Pow(1.0F - XPM, POW[treeIndex]); // RAP is the only variant using POW != 1
                    float PM = 1.0F - PS * CRADJ;

                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    NA += expansionFactor * (1.0F - PM);
                    float dbhAfterGrowth = treesOfSpecies.Dbh[treeIndex] + treesOfSpecies.DbhGrowth[treeIndex];
                    BAA += Constant.ForestersEnglish * dbhAfterGrowth * dbhAfterGrowth * expansionFactor * (1.0F - PM);
                }

                // DETERMINE IF ADDITIONAL MORTALITY MUST BE TAKEN
                if (configuration.AdditionalMortality)
                {
                    float QMDA = MathF.Sqrt(BAA / (Constant.ForestersEnglish * NA));
                    float RDA = NA / MathV.Exp(stand.A1 / stand.A2 - MathV.Ln(QMDA) / stand.A2);
                    if (simulationStep == 0)
                    {
                        // INITALIZATIONS FOR FIRST GROWTH CYCLE
                        if (RD >= 1.0)
                        {
                            if (RDA > RD)
                            {
                                stand.A1MAX = MathV.Ln(SQMDA) + stand.A2 * MathV.Ln(standTreesPerAcre);
                            }
                            else
                            {
                                stand.A1MAX = MathV.Ln(QMDA) + stand.A2 * MathV.Ln(NA);
                            }
                            if (stand.A1MAX < stand.A1)
                            {
                                stand.A1MAX = stand.A1;
                            }
                        }
                        else
                        {
                            if (configuration.Variant.TreeModel != TreeModel.OrganonRap)
                            {
                                if (RD > RDCC)
                                {
                                    float XA3 = -1.0F / A3;
                                    stand.NO = standTreesPerAcre * MathF.Pow(MathV.Ln(RD) / MathV.Ln(RDCC), XA3);
                                }
                                else
                                {
                                    stand.NO = configuration.PDEN;
                                }
                            }
                            // INITIALIZATIONS FOR SUBSEQUENT GROWTH CYCLES
                            else
                            {
                                if (stand.RD0 >= 1.0F)
                                {
                                    stand.A1MAX = MathV.Ln(QMDA) + stand.A2 * MathV.Ln(NA);
                                    if (stand.A1MAX < stand.A1)
                                    {
                                        stand.A1MAX = stand.A1;
                                    }
                                }
                                else
                                {
                                    int IND;
                                    if ((RD >= 1.0F) && (stand.NO <= 0.0F))
                                    {
                                        if (RDA > RD)
                                        {
                                            stand.A1MAX = MathV.Ln(SQMDA) + stand.A2 * MathV.Ln(standTreesPerAcre);
                                        }
                                        else
                                        {
                                            stand.A1MAX = MathV.Ln(QMDA) + stand.A2 * MathV.Ln(NA);
                                        }
                                        IND = 1;
                                        if (stand.A1MAX < stand.A1)
                                        {
                                            stand.A1MAX = stand.A1;
                                        }
                                    }
                                    else
                                    {
                                        IND = 0;
                                        if (configuration.Variant.TreeModel != TreeModel.OrganonRap)
                                        {
                                            if ((RD > RDCC) && (stand.NO <= 0.0F))
                                            {
                                                float XA3 = -1.0F / A3;
                                                stand.NO = standTreesPerAcre * MathF.Pow(MathV.Ln(RD) / MathV.Ln(RDCC), XA3);
                                            }
                                            else
                                            {
                                                stand.NO = configuration.PDEN;
                                            }
                                        }
                                    }

                                    // COMPUTATION OF ADDITIONAL MORTALITY IF NECESSARY
                                    float QMDP;
                                    if ((IND == 0) && (stand.NO > 0.0F))
                                    {
                                        if (configuration.Variant.TreeModel != TreeModel.OrganonRap)
                                        {
                                            QMDP = QUAD1(NA, stand.NO, RDCC, stand.A1);
                                        }
                                        else
                                        {
                                            QMDP = QUAD2(NA, stand.NO, stand.A1);
                                        }
                                    }
                                    else
                                    {
                                        QMDP = MathV.Exp(stand.A1MAX - stand.A2 * MathV.Ln(NA));
                                    }

                                    if ((RD <= RDCC) || (QMDP > QMDA))
                                    {
                                        // NO ADDITIONAL MORTALITY NECESSARY
                                        for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                                        {
                                            float CR = treesOfSpecies.CrownRatio[treeIndex];
                                            float CRADJ = OrganonGrowth.GetCrownRatioAdjustment(CR);
                                            float XPM = 1.0F / (1.0F + MathV.Exp(-PMK[treeIndex]));
                                            float PS = MathV.Pow(1.0F - XPM, POW[treeIndex]);
                                            float PM = 1.0F - PS * CRADJ;
                                            Debug.Assert(PM >= 0.0F);
                                            Debug.Assert(PM <= 1.0F);

                                            treesOfSpecies.DeadExpansionFactor[treeIndex] = PM * treesOfSpecies.LiveExpansionFactor[treeIndex];
                                            treesOfSpecies.LiveExpansionFactor[treeIndex] -= treesOfSpecies.DeadExpansionFactor[treeIndex];
                                        }
                                    }
                                    else
                                    {
                                        // ADJUSTMENT TO MORTALITY NECESSARY
                                        float KR1 = 0.0F;
                                        for (int KK = 0; KK < 7; ++KK)
                                        {
                                            float NK = 10.0F / MathV.Exp10(KK);
                                        kr1: KR1 += NK;
                                            float NAA = 0.0F;
                                            float BAAA = 0.0F;
                                            for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                                            {
                                                if (treesOfSpecies.LiveExpansionFactor[treeIndex] < 0.001F)
                                                {
                                                    continue;
                                                }

                                                float CR = treesOfSpecies.CrownRatio[treeIndex];
                                                float CRADJ = OrganonGrowth.GetCrownRatioAdjustment(CR);
                                                float XPM = 1.0F / (1.0F + MathV.Exp(-(KR1 + PMK[treeIndex])));
                                                float PS = MathV.Pow(1.0F - XPM, POW[treeIndex]);
                                                float PM = 1.0F - PS * CRADJ;

                                                float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                                                NAA += expansionFactor * (1.0F - PM);
                                                
                                                BAAA += treesOfSpecies.GetBasalArea(treeIndex) * (1.0F - PM);
                                            }
                                            QMDA = MathF.Sqrt(BAAA / (Constant.ForestersEnglish * NAA));
                                            if (IND == 0)
                                            {
                                                if (configuration.Variant.TreeModel != TreeModel.OrganonRap)
                                                {
                                                    QMDP = QUAD1(NAA, stand.NO, RDCC, stand.A1);
                                                }
                                                else
                                                {
                                                    QMDP = QUAD2(NAA, stand.NO, stand.A1);
                                                }
                                            }
                                            else
                                            {
                                                QMDP = MathV.Exp(stand.A1MAX - stand.A2 * MathV.Ln(NAA));
                                            }
                                            if (QMDP >= QMDA)
                                            {
                                                KR1 -= NK;
                                            }
                                            else
                                            {
                                                goto kr1;
                                            }
                                        }

                                        for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                                        {
                                            if (treesOfSpecies.LiveExpansionFactor[treeIndex] <= 0.0F)
                                            {
                                                treesOfSpecies.DeadExpansionFactor[treeIndex] = 0.0F;
                                                treesOfSpecies.LiveExpansionFactor[treeIndex] = 0.0F;
                                            }
                                            else
                                            {
                                                float CR = treesOfSpecies.CrownRatio[treeIndex];
                                                float CRADJ = OrganonGrowth.GetCrownRatioAdjustment(CR);
                                                float XPM = 1.0F / (1.0F + MathV.Exp(-(KR1 + PMK[treeIndex])));
                                                float PS = MathV.Pow(1.0F - XPM, POW[treeIndex]);
                                                float PM = 1.0F - PS * CRADJ;
                                                Debug.Assert(PM >= 0.0F);
                                                Debug.Assert(PM <= 1.0F);

                                                treesOfSpecies.DeadExpansionFactor[treeIndex] = PM * treesOfSpecies.LiveExpansionFactor[treeIndex];
                                                treesOfSpecies.LiveExpansionFactor[treeIndex] -= treesOfSpecies.DeadExpansionFactor[treeIndex];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                    {
                        // TODO: avoid recalculation of PS?
                        float CR = treesOfSpecies.CrownRatio[treeIndex];
                        float CRADJ = OrganonGrowth.GetCrownRatioAdjustment(CR);
                        float XPM = 1.0F / (1.0F + MathV.Exp(-PMK[treeIndex]));
                        float PS = POW[treeIndex] == 1.0 ? 1.0F - XPM : MathV.Pow(1.0F - XPM, POW[treeIndex]); // RAP is the only variant using POW != 1
                        float PM = 1.0F - PS * CRADJ;
                        Debug.Assert(PM >= 0.0F);
                        Debug.Assert(PM <= 1.0F);

                        treesOfSpecies.DeadExpansionFactor[treeIndex] = PM * treesOfSpecies.LiveExpansionFactor[treeIndex];
                        treesOfSpecies.LiveExpansionFactor[treeIndex] -= treesOfSpecies.DeadExpansionFactor[treeIndex];
                    }
                }

                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    if (treesOfSpecies.LiveExpansionFactor[treeIndex] < 0.00001F)
                    {
                        treesOfSpecies.LiveExpansionFactor[treeIndex] = 0.0F;
                    }
                }
            }
        }

        /// <summary>
        /// Computes old growth index?
        /// </summary>
        /// <param name="stand">Stand data.</param>
        /// <param name="GROWTH">Tree data.</param>
        /// <param name="XIND">Zero or minus one, usually zero.</param>
        /// <param name="OG">Old growth indicator.</param>
        /// <remarks>
        /// XIND
        ///    0.0: NOT ADD GROWTH VALUES OR MORTALITY VALUES
        ///   -1.0: SUBTRACT GROWTH VALUES AND ADD MORTALITY VALUES
        ///    1.0: ADD GROWTH VALUES AND SUBTRACT MORTALITY VALUES
        /// </remarks>
        // BUGBUG: supports only to 98 DBH
        public static void OldGro(OrganonVariant variant, OrganonStand stand, float XIND, out float OG)
        {
            float[] HTCL = new float[100];
            float[] DCL = new float[100];
            float[] TRCL = new float[100];
            for (int I = 0; I < 100; ++I)
            {
                HTCL[I] = 0.0F;
                DCL[I] = 0.0F;
                TRCL[I] = 0.0F;
            }

            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                if (variant.IsBigSixSpecies(treesOfSpecies.Species) == false)
                {
                    continue;
                }
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float heightInFeet = treesOfSpecies.Height[treeIndex] + XIND * treesOfSpecies.HeightGrowth[treeIndex];
                    float dbhInInches = treesOfSpecies.Dbh[treeIndex] + XIND * treesOfSpecies.DbhGrowth[treeIndex];
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex] - XIND * treesOfSpecies.DeadExpansionFactor[treeIndex];
                    Debug.Assert(expansionFactor >= 0.0F);
                    Debug.Assert(expansionFactor <= 1000.0F);

                    int ID = (int)dbhInInches + 1;
                    if (ID > 99)
                    {
                        ID = 99;
                    }
                    HTCL[ID] = HTCL[ID] + heightInFeet * expansionFactor;
                    DCL[ID] = DCL[ID] + dbhInInches * expansionFactor;
                    TRCL[ID] = TRCL[ID] + expansionFactor;
                }
            }

            float TOTHT = 0.0F;
            float TOTD = 0.0F;
            float TOTTR = 0.0F;
            for (int I = 99; I > 0; --I)
            {
                TOTHT += HTCL[I];
                TOTD += DCL[I];
                TOTTR += TRCL[I];
                if (TOTTR > 5.0F)
                {
                    float TRDIFF = TRCL[I] - (TOTTR - 5.0F);
                    TOTHT = TOTHT - HTCL[I] + ((HTCL[I] / TRCL[I]) * TRDIFF);
                    TOTD = TOTD - DCL[I] + ((DCL[I] / TRCL[I]) * TRDIFF);
                    TOTTR = 5.0F;
                    break;
                }
            }

            // DETERMINE THE OLD GROWTH INDICATOR "OG"
            if (TOTTR > 0.0F)
            {
                float HT5 = TOTHT / TOTTR;
                float DBH5 = TOTD / TOTTR;
                OG = DBH5 * HT5 / 10000.0F;
                Debug.Assert(OG >= 0.0F);
                Debug.Assert(OG <= 1000.0F);
            }
            else
            {
                OG = 0.0F;
            }
        }

        private static float PM_FERT(FiaCode species, TreeModel treeModel, int simulationStep, OrganonTreatments treatments)
        {
            // fertilization mortality effects currently supported only for non-RAP Douglas-fir
            if ((species != FiaCode.PseudotsugaMenziesii) || (treeModel != TreeModel.OrganonRap))
            {
                return 1.0F;
            }

            // non-RAP Douglas-fir
            // Hann 2003 Research Contribution 40, Table 37: Parameters for predicting fertlization response of 5-year mortality
            float c5 = 0.0000552859F;
            float PF2 = 1.5F;
            float PF3 = -0.5F;

            float XTIME = Constant.DefaultTimeStepInYears * (float)simulationStep;
            float FERTX1 = 0.0F;
            for (int treatmentIndex = 1; treatmentIndex < 5; ++treatmentIndex)
            {
                // BUGBUG: summation range doesn't match 13 or 18 year periods given in Hann 2003 Table 3
                FERTX1 += treatments.PoundsOfNitrogenPerAcre[treatmentIndex] * MathV.Exp(PF3 / PF2 * (treatments.FertilizationYears[0] - treatments.FertilizationYears[treatmentIndex]));
            }
            float FERTADJ = c5 * MathV.Pow(treatments.PoundsOfNitrogenPerAcre[0] + FERTX1, PF2) * MathV.Exp(PF3 * (XTIME - treatments.FertilizationYears[0]));
            return FERTADJ;
        }

        private static float QUAD1(float ti, float t1, float RDCC, float A1)
        {
            // Hann 2003 Research Contribution 40, Table 38: Parameters for predicting the maximum size density line for Douglas-fir
            // maximum size density line for measurement i: ln(max QMDi for TPA) = MLQi = g1 + g2 * LTi
            // stand approach to maximum density: LQi = MLQi - (g1 + g2 * LT0 - LQ0) exp(g3 * (LT0 - LTi))
            // A1 = g1 + sum_j(g1_j) + g2 * log(ti)
            float g2 = 0.62305F;
            float g3 = 14.39533971F;
            float A4 = -(MathV.Ln(RDCC) * g2 / A1);
            float X = A1 - g2 * MathV.Ln(ti) - (A1 * A4) * MathV.Exp(-g3 * (MathV.Ln(t1) - MathV.Ln(ti)));
            return MathV.Exp(X);
        }

        private static float QUAD2(float NI, float NO, float A1)
        {
            float A2 = 0.64F;
            float A3 = 3.88F;
            float A4 = 0.07F;
            float X = A1 - A2 * MathV.Ln(NI) - (A1 * A4) * MathV.Exp(-A3 * (MathV.Ln(NO) - MathV.Ln(NI)));
            return MathV.Exp(X);
        }
    }
}
