using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    public class StandGrowth
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CYCLG"></param>
        /// <param name="configuration">Organon growth simulation options and site settings.</param>
        /// <param name="ACALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YSF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YST"></param>
        /// <param name="DGRO"></param>
        /// <param name="HGRO"></param>
        /// <param name="CRCHNG"></param>
        /// <param name="NTREES2"></param>
        /// <param name="DBH2"></param>
        /// <param name="HT2"></param>
        /// <param name="CR2"></param>
        /// <param name="EXPAN2"></param>
        public static void EXECUTE(int CYCLG, OrganonConfiguration configuration, Stand stand, float[,] ACALIB, float[] PN, float[] YSF, 
                                   float BABT, float[] BART, float[] YST, float[] DGRO, float[] HGRO, float[] CRCHNG, 
                                   out int NTREES2, float[] DBH2, float[] HT2, float[] CR2, float[] EXPAN2)
        {
            // BUGBUG: CYCLG duplicates stand age
            // BUGBUG: for negative CYCLG values of A1 and A2 are overwritten by calls to SUBMAX and therefore silently ignored in certain cases
            EDIT(CYCLG, configuration, stand, ACALIB, PN, YSF, BABT, BART, YST, out int NSPN, out int BIG6, out int BNXT);

            int FCYCLE = 0;
            int TCYCLE = 0;
            int YCYCLG = 5 * CYCLG;
            if (configuration.Fertilizer && (YSF[0] == (float)YCYCLG))
            {
                FCYCLE = 1;
            }

            bool POST = false;
            if (configuration.Thin && (YST[0] == (float)YCYCLG))
            {
                TCYCLE = 1;
                POST = true;
            }

            if (DBH2.Length != stand.TreeRecordsInUse)
            {
                throw new ArgumentOutOfRangeException(nameof(DBH2));
            }
            if (HT2.Length != stand.TreeRecordsInUse)
            {
                throw new ArgumentOutOfRangeException(nameof(HT2));
            }
            if (CR2.Length != stand.TreeRecordsInUse)
            {
                throw new ArgumentOutOfRangeException(nameof(CR2));
            }
            if (EXPAN2.Length != stand.TreeRecordsInUse)
            {
                throw new ArgumentOutOfRangeException(nameof(EXPAN2));
            }

            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                DBH2[treeIndex] = 0.0F;
                HT2[treeIndex] = 0.0F;
                CR2[treeIndex] = 0.0F;
                EXPAN2[treeIndex] = 0.0F;
            }

            float[,] GROWTH = new float[stand.TreeRecordsInUse, 4];
            float[] DEADEXP = new float[stand.TreeRecordsInUse];
            float[,] TDATAR = new float[stand.TreeRecordsInUse, 8];
            for (int I = 0; I < stand.TreeRecordsInUse; ++I)
            {
                TDATAR[I, 0] = stand.Float[I, Constant.TreeIndex.Float.DbhInInches];
                TDATAR[I, 1] = stand.Float[I, Constant.TreeIndex.Float.HeightInFeet];
                TDATAR[I, 2] = stand.Float[I, Constant.TreeIndex.Float.CrownRatio];
                TDATAR[I, 3] = stand.Float[I, Constant.TreeIndex.Float.ExpansionFactor];
            }

            float[,] CALIB = new float[18, 6];
            for (int I = 0; I < 18; ++I)
            {
                CALIB[I, 3] = ACALIB[I, 0];
                CALIB[I, 4] = ACALIB[I, 1];
                CALIB[I, 5] = ACALIB[I, 2];
                if (configuration.CALH)
                {
                    CALIB[I, 0] = (1.0F + CALIB[I, 3]) / 2.0F + (float)Math.Pow(0.5, 0.5 * CYCLG) * ((CALIB[I, 3] - 1.0F) / 2.0F);
                }
                else 
                {
                    CALIB[I, 0] = 1.0F;
                }
                if (configuration.CALC)
                {
                    CALIB[I, 1] = (1.0F + CALIB[I, 4]) / 2.0F + (float)Math.Pow(0.5F, 0.5F * CYCLG) * ((CALIB[I, 4] - 1.0F) / 2.0F);
                }
                else 
                {
                    CALIB[I, 1] = 1.0F;
                }
                if (configuration.CALD)
                {
                    CALIB[I, 2] = (1.0F + CALIB[I, 6]) / 2.0F + (float)Math.Pow(0.5F, 0.5F * CYCLG) * ((CALIB[I, 6] - 1.0F) / 2.0F);
                }
                else 
                {
                    CALIB[I, 2] = 1.0F;
                }
            }

            float[] YF = new float[5];
            float[] YT = new float[5];
            for (int I = 0; I < 5; ++I)
            {
                YF[I] = YSF[I];
                YT[I] = YST[I];
            }

            // CALCULATE SPECIES GROUP
            //
            // SPGROUP(configuration.Variant, NSPN, stand.TreeRecordsInUse, TDATAI)
            //
            // BUGBUG MAXRAH, RASI, and RAAGE not initialized in Fortran
            // BUGBUG SI_1 not intialized in Fortran code for Nwo and Smc calculation of SITE_2 in else { }
            float MAXRAH = 0.0F;
            float RASI = 0.0F;
            float RAAGE = 0.0F;
            float SI_1 = 0.0F;
            if (configuration.Variant == Variant.Swo)
            {
                if ((configuration.SITE_1 < 0.0F) && (configuration.SITE_2 > 0.0F))
                {
                    configuration.SITE_1 = 1.062934F * configuration.SITE_2;
                }
                else if (configuration.SITE_2 < 0.0F)
                {
                    configuration.SITE_2 = 0.940792F * configuration.SITE_1;
                }
            }
            else if (configuration.Variant == Variant.Nwo || configuration.Variant == Variant.Smc)
            {
                // Site index conconfiguration.Variant equation from Nigh(1995, Forest Science 41:84-98)
                if ((configuration.SITE_1 < 0.0F) && (configuration.SITE_2 > 0.0F))
                {
                    configuration.SITE_1 = 0.480F + (1.110F * configuration.SITE_2);
                }
                else if (configuration.SITE_2 < 0.0F)
                {
                    configuration.SITE_2 = -0.432F + (0.899F * configuration.SITE_1);
                }
            }
            else
            {
                if (configuration.SITE_2 < 0.0F)
                {
                    configuration.SITE_2 = 4.776377F * (float)Math.Pow(SI_1, 0.763530587);
                }
            }

            // shift expansion factors to stand level from sample level
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                TDATAR[treeIndex, 3] = TDATAR[treeIndex, 3] / (float)stand.TreeRecordsInUse;
                stand.MGExpansionFactor[treeIndex] = stand.MGExpansionFactor[treeIndex] / (float)stand.TreeRecordsInUse;
                TDATAR[treeIndex, 4] = TDATAR[treeIndex, 3];
                TDATAR[treeIndex, 5] = TDATAR[treeIndex, 2];
            }

            // find red alder site index for natural (DOUG? non-plantation?) stands
            if (configuration.Variant < Variant.Smc)
            {
                for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
                {
                    if ((stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species] == 351) && (TDATAR[treeIndex, 1] > MAXRAH))
                    {
                        MAXRAH = TDATAR[treeIndex, 1];
                    }
                }
                // BUGBUG SITE_1 is the site index from ground but the code in CON_RASI appears to expect the breast height site index (SI_1)
                RASI = Stats.CON_RASI(configuration.SITE_1);
            }

            // CALCULATE RED ALDER AGE FOR NATURAL STANDS
            // BUGBUG RAAGE not initialized for MAXRAH <= 0.0 in Fortran code
            if (MAXRAH > 0.0F)
            {
                HeightGrowth.RAGEA(MAXRAH, RASI, out RAAGE);
            }
            if (RAAGE < 0.0F)
            {
                RAAGE = 55.0F;
                Stats.RASITE(MAXRAH, RAAGE, out RASI);
            }
            if (RAAGE > 55.0F)
            {
                RAAGE = 55.0F;
            }

            // BUGBUG no check that SITE_1 and SITE_2 indices are greater than 4.5 feet
            SI_1 = configuration.SITE_1 - 4.5F;
            float SI_2 = configuration.SITE_2 - 4.5F;

            // INITIALIZE A1 AND A2
            // BUGBUG this is inconsistent with EDIT(), which raises an error when CYCLG < 0
            if (CYCLG < 0)
            {
                Submax.SUBMAX(false, configuration, stand, TDATAR);
            }

            // CALCULATE DENSITY VARIABLES AT SOG
            float[] BAL1 = new float[500];
            float[] BALL1 = new float[51];
            float[] CCFL1 = new float[500];
            float[] CCFLL1 = new float[51];
            Stats.SSTATS(configuration.Variant, stand, TDATAR, out float SBA1, out float _, out float _, BAL1, BALL1, CCFL1, CCFLL1);

            // CALCULATE CCH AND CROWN CLOSURE AT SOG
            float[] CCH = new float[41];
            CrownGrowth.CRNCLO(0.0F, configuration.Variant, stand, TDATAR, stand.MGExpansionFactor, CCH, out float _);
            float OLD = 0.0F;
            for (int I = 0; I < stand.TreeRecordsInUse; ++I)
            {
                TDATAR[I, 7] = TDATAR[I, 3];
                TDATAR[I, 6] = TDATAR[I, 2];
            }

            float[] CCFLL2 = new float[51];
            float[] CCFL2 = new float[500];
            float[] BALL2 = new float[51];
            float[] BAL2 = new float[500];
            TreeGrowth.GROW(ref CYCLG, configuration, stand, TDATAR, DEADEXP, POST, NSPN, ref TCYCLE, ref FCYCLE, 
                            SI_1, SI_2, SBA1, BALL1, BAL1, CALIB, PN, YF, BABT, BART,
                            YT, GROWTH, CCH, ref OLD, RAAGE, RASI, CCFLL1, CCFL1, CCFLL2, CCFL2, BALL2, BAL2);
            NTREES2 = stand.TreeRecordsInUse;

            if (configuration.IsEvenAge == false)
            {
                stand.AgeInYears = 0;
                stand.BreastHeightAgeInYears = 0;
            }
            float X = 100.0F * (OLD / (BIG6 - BNXT));
            if (X > 50.0F)
            {
                stand.StandWarnings[6] = 1;
            }
            if (configuration.Variant == Variant.Swo)
            {
                if (configuration.IsEvenAge && (stand.BreastHeightAgeInYears > 500.0F))
                {
                    stand.StandWarnings[6] = 1;
                }
            }
            else if (configuration.Variant == Variant.Nwo || configuration.Variant == Variant.Smc)
            {
                if (configuration.IsEvenAge && (stand.BreastHeightAgeInYears > 120.0F))
                {
                    stand.StandWarnings[6] = 1;
                }
            }
            else
            {
                if (configuration.IsEvenAge && (stand.AgeInYears > 30.0F))
                {
                    stand.StandWarnings[6] = 1;
                }
            }

            for (int treeIndex = 0; treeIndex < NTREES2; ++treeIndex)
            {
                // needed for trees added by tripling but should have no effect for existing trees
                stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species] = stand.Integer[treeIndex, 0];
                stand.MGExpansionFactor[treeIndex] = 0.0F; // BUGBUG: MGExpansionFactor is simulation step state and shouldn't be part of the stand object
                DBH2[treeIndex] = TDATAR[treeIndex, 0];
                HT2[treeIndex] = TDATAR[treeIndex, 1];
                CR2[treeIndex] = TDATAR[treeIndex, 2];
                EXPAN2[treeIndex] = TDATAR[treeIndex, 3] * (float)stand.TreeRecordsInUse;
                HGRO[treeIndex] = GROWTH[treeIndex, 0];
                DGRO[treeIndex] = GROWTH[treeIndex, 1];
                stand.DeadExpansionFactor[treeIndex] = DEADEXP[treeIndex] * (float)stand.TreeRecordsInUse;
                CRCHNG[treeIndex] = TDATAR[treeIndex, 2] - TDATAR[treeIndex, 6];
            }
        }

        /// <summary>
        /// Does argument checking and raises error flags if problems are found.
        /// </summary>
        /// <param name="CYCLG"></param>
        /// <param name="configuration">Organon configuration settings.</param>
        /// <param name="ACALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YSF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YST"></param>
        /// <param name="NSPN">Number of species groups supported by specified Organon variant.</param>
        /// <param name="BIG6"></param>
        /// <param name="BNXT"></param>
        private static void EDIT(int CYCLG, OrganonConfiguration configuration, Stand stand, float[,] ACALIB, float[] PN, float[] YSF, float BABT, float[] BART, float[] YST,
                                 out int NSPN, out int BIG6, out int BNXT)
        {
            if (stand.TreeRecordsInUse < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stand.TreeRecordsInUse));
            }
            if (Enum.IsDefined(typeof(Variant), configuration.Variant) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.Variant));
            }
            if (stand.NPTS < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stand.NPTS));
            }
            if ((configuration.SITE_1 <= 0.0F) || (configuration.SITE_1 > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.SITE_1));
            }
            if ((configuration.SITE_2 <= 0.0F) || (configuration.SITE_2 > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.SITE_2));
            }

            if (configuration.IsEvenAge)
            {
                if (stand.BreastHeightAgeInYears < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(stand.BreastHeightAgeInYears), nameof(stand.BreastHeightAgeInYears) + " must be zero or greater when " + nameof(configuration.IsEvenAge) + " is set.");
                }
                if ((stand.AgeInYears - stand.BreastHeightAgeInYears) < 1)
                {
                    // (DOUG? can stand.AgeInYears ever be less than stand.BreastHeightAgeInYears?)
                    throw new ArgumentException(nameof(stand.AgeInYears) + " must be greater than " + nameof(stand.BreastHeightAgeInYears) + " when " + nameof(configuration.IsEvenAge) + " is set.");
                }
            }
            else
            {
                if (stand.BreastHeightAgeInYears != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(stand.BreastHeightAgeInYears), nameof(stand.BreastHeightAgeInYears) + " must be zero or less when " + nameof(configuration.IsEvenAge) + " is not set.");
                }
                if (configuration.Fertilizer)
                {
                    throw new ArgumentException("If " + nameof(configuration.Fertilizer) + " is set " + nameof(configuration.IsEvenAge) + "must also be set.");
                }
            }
            for (int I = 0; I < 5; ++I)
            {
                if (!configuration.Fertilizer && (YSF[I] != 0 || PN[I] != 0))
                {
                    throw new ArgumentException();
                }
                if (configuration.Fertilizer)
                {
                    if ((YSF[I] > stand.AgeInYears) || (YSF[I] > 70.0F))
                    {
                        throw new ArgumentException();
                    }
                    if (I == 0)
                    {
                        if ((PN[I] < 0.0) || (PN[I] > 400.0F))
                        {
                            throw new ArgumentException();
                        }
                        else
                        {
                            if (PN[I] > 400.0F)
                            {
                                throw new ArgumentException();
                            }
                        }
                    }
                }
            }

            if (configuration.Thin && (BART[0] >= BABT))
            {
                throw new ArgumentException("The first element of " + nameof(BART) + " must be less than " + nameof(BABT) + " when thinning response is enabled.");
            }
            for (int I = 0; I < 5; ++I)
            {
                if (!configuration.Thin && (YST[I] != 0 || BART[I] != 0))
                {
                    throw new ArgumentException();
                }
                if (configuration.Thin)
                {
                    if (configuration.IsEvenAge && YST[I] > stand.AgeInYears)
                    {
                        throw new ArgumentException();
                    }
                    if (I > 1)
                    {
                        if (YST[I] != 0.0F && BART[I] < 0.0F)
                        {
                            throw new ArgumentException();
                        }
                    }
                    if (BABT < 0.0F)
                    {
                        throw new ArgumentException();
                    }
                }
            }

            // (DOUG? why is YCYCLG 5 * CYCLG for all variants? should YCYCLG = CYCLG for SMC?)
            int YCYCLG = 5 * CYCLG;
            if (configuration.Thin && (YST[0] == YCYCLG))
            {
                bool thinningError = true;
                for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
                {
                    if (stand.MGExpansionFactor[treeIndex] > 0.0F)
                    {
                        thinningError = false;
                        break;
                    }
                }
                if (thinningError)
                {
                    throw new ArgumentException();
                }
            }

            if (CYCLG < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(CYCLG));
            }
            for (int I = 0; I < 3; ++I)
            {
                for (int J = 0; J < 18; ++J)
                {
                    if ((ACALIB[J, I] > 2.0F) || (ACALIB[J, I] < 0.5F))
                    {
                        throw new ArgumentOutOfRangeException(nameof(CYCLG));
                    }
                }
            }

            if (configuration.MSDI_1 > Constant.Maximum.MSDI)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.MSDI_1));
            }
            if (configuration.MSDI_2 > Constant.Maximum.MSDI)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.MSDI_2));
            }
            if (configuration.MSDI_3 > Constant.Maximum.MSDI)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.MSDI_3));
            }

            if (configuration.Genetics)
            {
                if (!configuration.IsEvenAge)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.Genetics), nameof(configuration.Genetics) + " is supported only when " + nameof(configuration.IsEvenAge) + " is set.");
                }
                if ((configuration.GWDG < 0.0F) || (configuration.GWDG > 20.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.GWDG));
                }
                if ((configuration.GWHG < 0.0F) || (configuration.GWHG > 20.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.GWHG));
                }
            }
            else
            {
                if (configuration.GWDG != 0.0F)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.GWDG));
                }
                if (configuration.GWHG != 0.0F)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.GWHG));
                }
            }

            if (configuration.SwissNeedleCast)
            {
                if ((configuration.Variant == Variant.Swo) || (configuration.Variant == Variant.Rap))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.Variant), "Swiss needle cast is not supported by the SWO and RAP variants.");
                }
                if (!configuration.IsEvenAge)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.IsEvenAge), "Swiss needle cast is not supported for uneven age stands.");
                }
                if ((configuration.FR < 0.85F) || (configuration.FR > 7.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.FR));
                }
                if (configuration.Fertilizer && (configuration.FR < 3.0))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.FR), nameof(configuration.FR) + " must be 3.0 or greater when " + nameof(configuration.SwissNeedleCast) + " and " + nameof(configuration.Fertilizer) + "are set.");
                }
            }
            else
            {
                if (configuration.FR > 0.0F)
                {
                    throw new ArgumentOutOfRangeException(nameof(configuration.FR));
                }
            }

            if ((configuration.Variant >= Variant.Rap) && (configuration.SITE_1 < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.SITE_1));
            }
            if ((configuration.Variant >= Variant.Rap) && (configuration.PDEN < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.PDEN));
            }
            if (!configuration.IsEvenAge && (configuration.Variant >= Variant.Rap))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.IsEvenAge));
            }

            for (int standWarningIndex = 0; standWarningIndex < 9; ++standWarningIndex)
            {
                // TODO: is it desirable to clear existing stand warnings?
                stand.StandWarnings[standWarningIndex] = 0;
            }

            // BUGBUG: move NSPN into Stand and OrganonConfiguration
            switch (configuration.Variant)
            {
                case Variant.Swo:
                    NSPN = 18;
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    NSPN = 11;
                    break;
                case Variant.Rap:
                    NSPN = 7;
                    break;
                default:
                    throw new NotSupportedException();
            }

            // check tree records for errors
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                int speciesCode = stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species];
                if (StandGrowth.IsSpeciesSupported(configuration.Variant, speciesCode) == false)
                {
                    throw new NotSupportedException(String.Format("Species {0} of tree {1} is not supported by variant {2}.", speciesCode, treeIndex, configuration.Variant));
                }
                float dbhInInches = stand.Float[treeIndex, Constant.TreeIndex.Float.DbhInInches];
                if (dbhInInches < 0.09F)
                {
                    throw new NotSupportedException(String.Format("Diameter of tree {0} is less than 0.1 inches.", treeIndex));
                }
                float heightInFeet = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                if (heightInFeet < 4.5F)
                {
                    throw new NotSupportedException(String.Format("Height of tree {0} is less than 4.5 feet.", treeIndex));
                }
                float crownRatio = stand.Float[treeIndex, Constant.TreeIndex.Float.CrownRatio];
                if ((crownRatio < 0.0F) || (crownRatio > 1.0F))
                {
                    throw new NotSupportedException(String.Format("Crown ratio of tree {0} is not between 0 and 1.", treeIndex));
                }
                float expansionFactor = stand.Float[treeIndex, Constant.TreeIndex.Float.ExpansionFactor];
                if (expansionFactor < 0.0F)
                {
                    throw new NotSupportedException(String.Format("Expansion factor of tree {0} is negative.", treeIndex));
                }
            }

            BIG6 = 0;
            BNXT = 0;
            float MAXGF = 0.0F;
            float MAXDF = 0.0F;
            float MAXWH = 0.0F;
            float MAXPP = 0.0F;
            float MAXIC = 0.0F;
            float MAXRA = 0.0F;
            int IIB = stand.MaxBigSixSpeciesGroupIndex;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                int speciesCode = stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species];
                float heightInFeet = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                switch (configuration.Variant)
                {
                    // SWO BIG SIX
                    case Variant.Swo:
                        if ((speciesCode == 122) && (heightInFeet > MAXPP))
                        {
                            MAXPP = heightInFeet;
                        }
                        else if ((speciesCode == 81) && (heightInFeet > MAXIC))
                        {
                            MAXIC = heightInFeet;
                        }
                        else if ((speciesCode == 202) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        else if ((speciesCode == 15) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        else if ((speciesCode == 17) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        else if ((speciesCode == 117) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        if ((speciesCode == 17) && (heightInFeet > MAXGF))
                        {
                            MAXGF = heightInFeet;
                        }
                        else if ((speciesCode == 202) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        else if ((speciesCode == 263) && (heightInFeet > MAXWH))
                        {
                            MAXWH = heightInFeet;
                        }
                        break;
                    case Variant.Rap:
                        if ((speciesCode == 351) && (heightInFeet > MAXRA))
                        {
                            MAXRA = heightInFeet;
                        }
                        break;
                }

                int speciesGroup = GetSpeciesGroup(configuration.Variant, speciesCode);
                stand.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup] = speciesGroup;
                if (configuration.Variant >= Variant.Rap)
                {
                    // BUGBUG: encapsulation violation - move this into GetSpeciesGroup()
                    IIB = 1;
                }
                if (speciesGroup < IIB)
                {
                    ++BIG6;
                    if (stand.Float[treeIndex, Constant.TreeIndex.Float.ExpansionFactor] < 0.0F)
                    {
                        ++BNXT;
                    }
                }
            }

            // DETERMINE IF SPECIES MIX CORRECT FOR STAND AGE
            float standBasalArea = 0.0F;
            float standBigSixBasalArea = 0.0F;
            float standHardwoodBasalArea = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                float expansionFactor = stand.Float[treeIndex, Constant.TreeIndex.Float.ExpansionFactor];
                if (expansionFactor < 0.0F)
                {
                    continue;
                }

                float dbhInInches = stand.Float[treeIndex, Constant.TreeIndex.Float.ExpansionFactor];
                float basalArea = expansionFactor * dbhInInches * dbhInInches;
                standBasalArea += basalArea;

                int speciesGroup = stand.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup];
                if (speciesGroup < IIB)
                {
                    standBigSixBasalArea += basalArea;
                }
                if (configuration.Variant == Variant.Swo)
                {
                    FiaCode species = (FiaCode)stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species];
                    if ((species == FiaCode.ArbutusMenziesii) || (species == FiaCode.ChrysolepisChrysophyllaVarChrysophylla) || (species == FiaCode.QuercusKelloggii))
                    {
                        standHardwoodBasalArea += basalArea;
                    }
                }
            }

            standBasalArea *= 0.005454154F / (float)stand.NPTS;
            standBigSixBasalArea *= 0.005454154F / (float)stand.NPTS;
            if (standBigSixBasalArea < 0.0F)
            {
                throw new NotSupportedException("Total basal area big six species is negative.");
            }

            if (configuration.Variant >= Variant.Rap)
            {
                float PRA;
                if (standBasalArea > 0.0F)
                {
                    PRA = standBigSixBasalArea / standBasalArea;
                }
                else
                {
                    PRA = 0.0F;
                }

                if (PRA < 0.9F)
                {
                    // if needed, make this a warning rather than an error
                    throw new NotSupportedException("Red alder plantation stand is less than 90% by basal area.");
                }
            }

            // DETERMINE WARNINGS (IF ANY)
            switch (configuration.Variant)
            {
                case Variant.Swo:
                    if ((configuration.SITE_1 > 0.0F) && ((configuration.SITE_1 < 40.0F) || (configuration.SITE_1 > 150.0F)))
                    {
                        stand.StandWarnings[0] = 1;
                    }
                    if ((configuration.SITE_2 > 0.0F) && ((configuration.SITE_2 < 50.0F) || (configuration.SITE_2 > 140.0F)))
                    {
                        stand.StandWarnings[1] = 1;
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if ((configuration.SITE_1 > 0.0F) && ((configuration.SITE_1 < 90.0F) || (configuration.SITE_1 > 142.0F)))
                    {
                        stand.StandWarnings[0] = 1;
                    }
                    if ((configuration.SITE_2 > 0.0F) && ((configuration.SITE_2 < 90.0F) || (configuration.SITE_2 > 142.0F)))
                    {
                        stand.StandWarnings[1] = 1;
                    }
                    break;
                case Variant.Rap:
                    if ((configuration.SITE_1 < 20.0F) || (configuration.SITE_1 > 125.0F))
                    {
                        stand.StandWarnings[0] = 1;
                    }
                    if ((configuration.SITE_2 > 0.0F) && (configuration.SITE_2 < 90.0F || configuration.SITE_2 > 142.0F))
                    {
                        stand.StandWarnings[1] = 1;
                    }
                    break;
            }

            // check tallest trees in stand against maximum height for big six species
            // BUGBUG: need an API for maximum heights rather than inline code here
            switch (configuration.Variant)
            {
                case Variant.Swo:
                    if (MAXPP > 0.0F)
                    {
                        float MAXHT = (configuration.SITE_2 - 4.5F) * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.164985 * (configuration.SITE_2 - 4.5), 0.288169)))) + 4.5F;
                        if (MAXPP > MAXHT)
                        {
                            stand.StandWarnings[2] = 1;
                        }
                    }
                    if (MAXIC > 0.0F)
                    {
                        float ICSI = (0.66F * configuration.SITE_1) - 4.5F;
                        float MAXHT = ICSI * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.174929 * ICSI, 0.281176)))) + 4.5F;
                        if (MAXIC > MAXHT)
                        {
                            stand.StandWarnings[2] = 1;
                        }
                    }
                    if (MAXDF > 0.0F)
                    {
                        float MAXHT = (configuration.SITE_1 - 4.5F) * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.174929 * (configuration.SITE_1 - 4.5), 0.281176)))) + 4.5F;
                        if (MAXDF > MAXHT)
                        {
                            stand.StandWarnings[2] = 1;
                        }
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if (MAXDF > 0.0F)
                    {
                        float Z50 = 2500.0F / (configuration.SITE_1 - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (MAXDF > MAXHT)
                        {
                            stand.StandWarnings[2] = 1;
                        }
                    }
                    if (MAXGF > 0.0F)
                    {
                        float Z50 = 2500.0F / (configuration.SITE_1 - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (MAXGF > MAXHT)
                        {
                            stand.StandWarnings[2] = 1;
                        }
                    }
                    if (MAXWH > 0.0F)
                    {
                        float Z50 = 2500.0F / (configuration.SITE_2 - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (0.00192F + 0.00007F * Z50);
                        if (MAXWH > MAXHT)
                        {
                            stand.StandWarnings[2] = 1;
                        }
                    }
                    break;
                case Variant.Rap:
                    if (MAXRA > 0.0F)
                    {
                        HeightGrowth.WHHLB_H40(configuration.SITE_1, 20.0F, 150.0F, out float MAXHT);
                        if (MAXRA > MAXHT)
                        {
                            stand.StandWarnings[2] = 1;
                        }
                    }
                    break;
            }

            if (configuration.IsEvenAge && (configuration.Variant < Variant.Smc) && (stand.BreastHeightAgeInYears < 10))
            {
                stand.StandWarnings[4] = 1;
            }

            if ((configuration.Variant == Variant.Swo && (standBigSixBasalArea + standHardwoodBasalArea) < standBasalArea * 0.2F) ||
                (configuration.Variant == Variant.Nwo && (standBigSixBasalArea + standHardwoodBasalArea) < standBasalArea * 0.5F) ||
                (configuration.Variant == Variant.Smc && (standBigSixBasalArea + standHardwoodBasalArea) < standBasalArea * 0.5F) ||
                (configuration.Variant == Variant.Rap && (standBigSixBasalArea + standHardwoodBasalArea) < standBasalArea * 0.8F))
            {
                stand.StandWarnings[4] = 1;
            }
            if (stand.TreeRecordsInUse < 50)
            {
                stand.StandWarnings[5] = 1;
            }

            CKAGE(configuration, stand, out float OLD);

            float X = 100.0F * (OLD / (BIG6 - BNXT));
            if (X >= 50.0F)
            {
                stand.StandWarnings[6] = 1;
            }
            if (configuration.Variant == Variant.Swo)
            {
                if (configuration.IsEvenAge && stand.BreastHeightAgeInYears > 500)
                {
                    stand.StandWarnings[7] = 1;
                }
            }
            else if (configuration.Variant == Variant.Nwo || configuration.Variant == Variant.Smc)
            {
                if (configuration.IsEvenAge && stand.BreastHeightAgeInYears > 120)
                {
                    stand.StandWarnings[7] = 1;
                }
            }
            else
            {
                if (configuration.IsEvenAge && stand.AgeInYears > 30)
                {
                    stand.StandWarnings[7] = 1;
                }
            }

            int EXCAGE;
            if (configuration.IsEvenAge)
            {
                switch (configuration.Variant)
                {
                    case Variant.Swo:
                        EXCAGE = 500 - stand.AgeInYears - 5;
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        EXCAGE = 120 - stand.AgeInYears - 5;
                        break;
                    case Variant.Rap:
                        EXCAGE = 30 - stand.AgeInYears - 1;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                switch (configuration.Variant)
                {
                    case Variant.Swo:
                        EXCAGE = 500 - (CYCLG + 1) * 5;
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        EXCAGE = 120 - (CYCLG + 1) * 5;
                        break;
                    case Variant.Rap:
                        EXCAGE = 30 - (CYCLG + 1) * 1;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (EXCAGE < 0)
            {
                stand.StandWarnings[8] = 1;
            }

            float B1 = -0.04484724F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                FiaCode species = (FiaCode)stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species];
                float B0;
                switch (species)
                {
                    case FiaCode.PseudotsugaMenziesii:
                        B0 = 19.04942539F;
                        break;
                    case FiaCode.TsugaHeterophylla:
                        if (configuration.Variant == Variant.Nwo || configuration.Variant == Variant.Smc)
                        {
                            B0 = 19.04942539F;
                        }
                        else if (configuration.Variant == Variant.Rap)
                        {
                            B0 = 19.04942539F;
                        }
                        else
                        {
                            // BUGBUG Fortran code leaves B0 unitialized in Version.Swo case but also always treats hemlock as Douglas-fir
                            B0 = 19.04942539F;
                        }
                        break;
                    case FiaCode.AbiesConcolor:
                    case FiaCode.AbiesGrandis:
                        B0 = 16.26279948F;
                        break;
                    case FiaCode.PinusPonderosa:
                        B0 = 17.11482201F;
                        break;
                    case FiaCode.PinusLambertiana:
                        B0 = 14.29011403F;
                        break;
                    default:
                        B0 = 15.80319194F;
                        break;
                }

                float dbhInInches = stand.Float[treeIndex, Constant.TreeIndex.Float.DbhInInches];
                float potentialHeight = 4.5F + B0 * dbhInInches / (1.0F - B1 * dbhInInches);
                float heightInFeet = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                if (heightInFeet > potentialHeight)
                {
                    stand.TreeWarnings[treeIndex] = 1;
                }
            }
        }

        private static bool IsSpeciesSupported(Variant variant, int speciesCode)
        {
            // check if species FIA code is valid for tree I
            int[] SCODE1 = { 202, 15, 17, 122, 117, 81, 263, 242, 231, 361, 431, 631, 805, 312, 815, 818, 351, 492, 920 };
            int[] SCODE2 = { 202, 17, 263, 242, 231, 361, 312, 815, 351, 492, 920 };
            int[] SCODE3 = { 351, 202, 263, 242, 312, 492, 920 };

            int speciesIndex;
            switch (variant)
            {
                case Variant.Swo:
                    speciesIndex = Array.IndexOf(SCODE1, speciesCode);
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    speciesIndex = Array.IndexOf(SCODE2, speciesCode);
                    break;
                case Variant.Rap:
                    speciesIndex = Array.IndexOf(SCODE3, speciesCode);
                    break;
                default:
                    throw VariantExtensions.CreateUnhandledVariantException(variant);
            }

            return speciesIndex >= 0;
        }

        private static int GetSpeciesGroup(Variant variant, int speciesCode)
        {
            // (DOUG? Why are species assigned to a given group and why does group numbering vary with version? The "group" here is only
            //    202 + 15 for NWO, which is PSME and ABCO.)
            // DETERMINE SPECIES GROUP FOR EACH TREE IN TREE LIST 
            // I = TREE INDICATOR
            // BUGBUG: doesn't iterate over length of SCODE arrays
            int[] SCODE1 = { 202, 15, 17, 122, 117, 81, 263, 242, 231, 361, 431, 631, 805, 312, 815, 818, 351, 492, 920 };
            int[] SCODE2 = { 202, 17, 263, 242, 231, 361, 312, 815, 351, 492, 920 };
            int[] SCODE3 = { 351, 202, 263, 242, 312, 492, 920 };
            int ISX = -9999;
            switch (variant)
            {
                case Variant.Swo:
                    for (int J = 0; J < 19; ++J)
                    {
                        if (speciesCode == SCODE1[J])
                        {
                            ISX = J;
                            if (ISX > 1)
                            {
                                --ISX;
                            }
                            Debug.Assert(ISX < SCODE1.Length - 1);
                            break;
                        }
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    for (int J = 0; J < 11; ++J)
                    {
                        if (speciesCode == SCODE2[J])
                        {
                            ISX = J;
                            Debug.Assert(ISX < SCODE2.Length);
                            break;
                        }
                    }
                    break;
                case Variant.Rap:
                    for (int J = 0; J < 7; ++J)
                    {
                        if (speciesCode == SCODE3[J])
                        {
                            ISX = J;
                            Debug.Assert(ISX < SCODE3.Length);
                            break;
                        }
                    }
                    break;
            }

            return ISX;
        }

        private static void CKAGE(OrganonConfiguration configuration, Stand stand, out float OLD)
        {
            OLD = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                float treeHeightInFeet = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                if (treeHeightInFeet < 4.5F)
                {
                    continue;
                }

                int speciesGroup = stand.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup];
                float growthEffectiveAge = 0.0F; // BUGBUG not intitialized on all Fortran paths
                float IDXAGE;
                int ISISP;
                float SITE;
                switch (configuration.Variant)
                {
                    case Variant.Swo:
                        // GROWTH EFFECTIVE AGE FROM HANN AND SCRIVANI'S (1987) DOMINANT HEIGHT GROWTH EQUATION
                        if (speciesGroup == 3)
                        {
                            SITE = configuration.SITE_2 - 4.5F;
                            ISISP = 2;
                        }
                        else
                        {
                            SITE = configuration.SITE_1 - 4.5F;
                            if (speciesGroup == 5)
                            {
                                SITE = configuration.SITE_1 * 0.66F - 4.5F;
                            }
                            ISISP = 1;
                        }
                        HeightGrowth.HS_HG(ISISP, SITE, treeHeightInFeet, out growthEffectiveAge, out _);
                        IDXAGE = 500.0F;
                        break;
                    case Variant.Nwo:
                        float GP = 5.0F;
                        if (speciesGroup == 3)
                        {
                            // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQATION
                            SITE = configuration.SITE_2;
                            HeightGrowth.F_HG(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        else
                        {
                            // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOR DOUGLAS-FIR AND GRAND FIR
                            SITE = configuration.SITE_1;
                            HeightGrowth.B_HG(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        IDXAGE = 120.0F;
                        break;
                    case Variant.Smc:
                        GP = 5.0F;
                        if (speciesGroup == 3)
                        {
                            // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQUATION
                            SITE = configuration.SITE_2;
                            HeightGrowth.F_HG(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        else
                        {
                            // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOP DOUGLAS-FIR AND GRAND FIR
                            SITE = configuration.SITE_1;
                            HeightGrowth.B_HG(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        IDXAGE = 120.0F;
                        break;
                    case Variant.Rap:
                        if (speciesGroup == 1)
                        {
                            // GROWTH EFFECTIVE AGE FROM WEISKITTEL ET AL.'S (2009) RED ALDER DOMINANT HEIGHT GROWTH EQUATION
                            SITE = configuration.SITE_1;
                            HeightGrowth.WHHLB_SI_UC(SITE, configuration.PDEN, out float SI_UC);
                            HeightGrowth.WHHLB_GEA(treeHeightInFeet, SI_UC, out growthEffectiveAge);
                        }
                        IDXAGE = 30.0F;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                // BUGBUG inconsistent use of < IB rather than <= IB
                if ((speciesGroup < stand.MaxBigSixSpeciesGroupIndex) && (growthEffectiveAge > IDXAGE))
                {
                    OLD += 1.0F;
                }
            }
        }

        public static void CROWN_CLOSURE(Variant variant, Stand stand, int NPTS, out float CC)
        {
            float[] CCH = new float[41];
            CCH[40] = stand.Float[0, Constant.TreeIndex.Float.HeightInFeet];
            for (int treeIndex = 1; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                float heightInFeet = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                if (heightInFeet > CCH[40])
                {
                    CCH[40] = heightInFeet;
                }
            }

            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                int speciesGroup = stand.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup];
                float expansionFactor = stand.Float[treeIndex, Constant.TreeIndex.Float.ExpansionFactor];
                float dbhInInches = stand.Float[treeIndex, Constant.TreeIndex.Float.DbhInInches];
                float heightInFeet  = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                float crownRatio = stand.Float[treeIndex, Constant.TreeIndex.Float.CrownRatio];

                float CL = crownRatio * heightInFeet;
                float HCB = heightInFeet - CL;
                float EXPFAC = expansionFactor / (float)NPTS;
                switch (variant)
                {
                    case Variant.Swo:
                        CrownGrowth.MCW_SWO(speciesGroup, dbhInInches, heightInFeet, out float MCW);
                        CrownGrowth.LCW_SWO(speciesGroup, MCW, crownRatio, dbhInInches, heightInFeet, out float LCW);
                        CrownGrowth.HLCW_SWO(speciesGroup, heightInFeet, crownRatio, out float HLCW);
                        CrownGrowth.CALC_CC(variant, speciesGroup, HLCW, LCW, heightInFeet, dbhInInches, HCB, EXPFAC, CCH);
                        break;
                    case Variant.Nwo:
                        CrownGrowth.MCW_NWO(speciesGroup, dbhInInches, heightInFeet, out MCW);
                        CrownGrowth.LCW_NWO(speciesGroup, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        CrownGrowth.HLCW_NWO(speciesGroup, heightInFeet, crownRatio, out HLCW);
                        CrownGrowth.CALC_CC(variant, speciesGroup, HLCW, LCW, heightInFeet, dbhInInches, HCB, EXPFAC, CCH);
                        break;
                    case Variant.Smc:
                        CrownGrowth.MCW_SMC(speciesGroup, dbhInInches, heightInFeet, out MCW);
                        CrownGrowth.LCW_SMC(speciesGroup, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        CrownGrowth.HLCW_SMC(speciesGroup, heightInFeet, crownRatio, out HLCW);
                        CrownGrowth.CALC_CC(variant, speciesGroup, HLCW, LCW, heightInFeet, dbhInInches, HCB, EXPFAC, CCH);
                        break;
                    case Variant.Rap:
                        CrownGrowth.MCW_RAP(speciesGroup, dbhInInches, heightInFeet, out MCW);
                        CrownGrowth.LCW_RAP(speciesGroup, MCW, crownRatio, dbhInInches, heightInFeet, out LCW);
                        CrownGrowth.HLCW_RAP(speciesGroup, heightInFeet, crownRatio, out HLCW);
                        CrownGrowth.CALC_CC(variant, speciesGroup, HLCW, LCW, heightInFeet, dbhInInches, HCB, EXPFAC, CCH);
                        break;
                }
            }
            CC = CCH[0];
            Debug.Assert(CC >= 0.0F);
            Debug.Assert(CC <= 100.0F);
        }

        /// <summary>
        /// Sets height and crown ratio of last NINGRO tree records in stand.
        /// </summary>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="NINGRO"></param>
        /// <param name="ACALIB"></param>
        public static void INGRO_FILL(Variant variant, Stand stand, int NINGRO, float[,] ACALIB)
        {
            // ROUTINE TO CALCULATE MISSING CROWN RATIOS
            // BUGBUG: does this duplicate site index code elsewhere?
            // NINGRO = NUMBER OF TREES ADDED
            float SITE_1 = stand.PrimarySiteIndex;
            float SITE_2 = stand.MortalitySiteIndex;
            float SI_1;
            if (variant == Variant.Swo)
            {
                if ((SITE_1 < 0.0F) && (SITE_2 > 0.0F))
                {
                    SITE_1 = 1.062934F * SITE_2;
                }
                else if (SITE_2 < 0.0F)
                {
                    SITE_2 = 0.940792F * SITE_1;
                }
            }
            else if (variant == Variant.Nwo || variant == Variant.Smc)
            {
                if ((SITE_1 < 0.0F) && (SITE_2 > 0.0F))
                {
                    SITE_1 = 0.480F + (1.110F * SITE_2);
                }
                else if (SITE_2 < 0.0F)
                {
                    SITE_2 = -0.432F + (0.899F * SITE_1);
                }
            }
            else
            {
                if (SITE_2 < 0.0F)
                {
                    // BUGBUG: not initialized in Fortran code; should this be SITE_1?
                    SI_1 = 0.0F;
                    SITE_2 = 4.776377F * (float)Math.Pow(SI_1, 0.763530587F);
                }
            }

            SI_1 = SITE_1 - 4.5F;
            float SI_2 = SITE_2 - 4.5F;
            // BUGBUG no check that NINGRO <= stand.TreeRecordsInUse
            for (int treeIndex = stand.TreeRecordsInUse - NINGRO; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                float heightInFeet = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                if (heightInFeet != 0.0F)
                {
                    continue;
                }

                int speciesGroup = stand.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup];
                float dbhInInches = stand.Float[treeIndex, Constant.TreeIndex.Float.DbhInInches];
                float RHT;
                switch (variant)
                {
                    case Variant.Swo:
                        HeightGrowth.HD_SWO(speciesGroup, dbhInInches, out RHT);
                        break;
                    case Variant.Nwo:
                        HeightGrowth.HD_NWO(speciesGroup, dbhInInches, out RHT);
                        break;
                    case Variant.Smc:
                        HeightGrowth.HD_SMC(speciesGroup, dbhInInches, out RHT);
                        break;
                    case Variant.Rap:
                        HeightGrowth.HD_RAP(speciesGroup, dbhInInches, out RHT);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet] = 4.5F + ACALIB[speciesGroup, 0] * (RHT - 4.5F);
            }

            float[] DEADEXP = new float[stand.TreeRecordsInUse];
            float[,] GROWTH = new float[stand.TreeRecordsInUse, 4];
            float[,] TDATAR = new float[stand.TreeRecordsInUse, 4];
            for (int treeIndex = 0; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                TDATAR[treeIndex, 0] = stand.Float[treeIndex, Constant.TreeIndex.Float.DbhInInches];
                TDATAR[treeIndex, 1] = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                TDATAR[treeIndex, 2] = stand.Float[treeIndex, Constant.TreeIndex.Float.CrownRatio];
                TDATAR[treeIndex, 3] = stand.Float[treeIndex, Constant.TreeIndex.Float.ExpansionFactor];
            }

            Mortality.OldGro(stand, TDATAR, GROWTH, DEADEXP, 0.0F, out float OG);
            float[] BAL = new float[500];
            float[] BALL = new float[51];
            float[] CCFL = new float[500];
            float[] CCFLL = new float[51];
            Stats.SSTATS(variant, stand, TDATAR, out float SBA, out float _, out float _, BAL, BALL, CCFL, CCFLL);
            for (int treeIndex = stand.TreeRecordsInUse - NINGRO; treeIndex < stand.TreeRecordsInUse; ++treeIndex)
            {
                float crownRatio = stand.Float[treeIndex, Constant.TreeIndex.Float.CrownRatio];
                if (crownRatio != 0.0F)
                {
                    continue;
                }

                // CALCULATE HCB
                int speciesGroup = stand.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup];
                float dbhInInches = stand.Float[treeIndex, Constant.TreeIndex.Float.DbhInInches];
                float heightInFeet = stand.Float[treeIndex, Constant.TreeIndex.Float.HeightInFeet];
                CrownGrowth.GET_CCFL(dbhInInches, CCFLL, CCFL, out float SCCFL);
                float HCB;
                switch (variant)
                {
                    case Variant.Swo:
                        CrownGrowth.HCB_SWO(speciesGroup, heightInFeet, dbhInInches, SCCFL, SBA, SI_1, SI_2, OG, out HCB);
                        break;
                    case Variant.Nwo:
                        CrownGrowth.HCB_NWO(speciesGroup, heightInFeet, dbhInInches, SCCFL, SBA, SI_1, SI_2, OG, out HCB);
                        break;
                    case Variant.Smc:
                        CrownGrowth.HCB_SMC(speciesGroup, heightInFeet, dbhInInches, SCCFL, SBA, SI_1, SI_2, OG, out HCB);
                        break;
                    case Variant.Rap:
                        CrownGrowth.HCB_RAP(speciesGroup, heightInFeet, dbhInInches, SCCFL, SBA, SI_1, SI_2, OG, out HCB);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                if (HCB < 0.0F)
                {
                    HCB = 0.0F;
                }
                if (HCB > 0.95F * heightInFeet)
                {
                    HCB = 0.95F * heightInFeet;
                }
                stand.Float[treeIndex, Constant.TreeIndex.Float.CrownRatio] = (1.0F - (HCB / heightInFeet)) * ACALIB[speciesGroup, 1];
            }
        }
    }
}
