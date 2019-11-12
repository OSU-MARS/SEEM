using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    public class StandGrowth
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulationStep"></param>
        /// <param name="configuration">Organon growth simulation options and site settings.</param>
        /// <param name="ACALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YSF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YST"></param>
        public static void EXECUTE(int simulationStep, OrganonConfiguration configuration, Stand stand, float[,] ACALIB, float[] PN, float[] YSF, 
                                   float BABT, float[] BART, float[] YST)
        {
            // BUGBUG: simulationStep largely duplicates stand age
            EDIT(simulationStep, configuration, stand, ACALIB, PN, YSF, BABT, BART, YST, out int NSPN, out int BIG6, out int BNXT);

            // BUGBUG: 5 * simulationStep is incorrect for RAP
            int simulationYear = 5 * simulationStep;
            int FCYCLE = 0;
            int TCYCLE = 0;
            if (configuration.Fertilizer && (YSF[0] == (float)simulationYear))
            {
                FCYCLE = 1;
            }

            float[,] CALIB = new float[18, 6];
            for (int I = 0; I < 18; ++I)
            {
                CALIB[I, 3] = ACALIB[I, 0];
                CALIB[I, 4] = ACALIB[I, 1];
                CALIB[I, 5] = ACALIB[I, 2];
                if (configuration.CALH)
                {
                    CALIB[I, 0] = (1.0F + CALIB[I, 3]) / 2.0F + (float)Math.Pow(0.5, 0.5 * simulationStep) * ((CALIB[I, 3] - 1.0F) / 2.0F);
                }
                else 
                {
                    CALIB[I, 0] = 1.0F;
                }
                if (configuration.CALC)
                {
                    CALIB[I, 1] = (1.0F + CALIB[I, 4]) / 2.0F + (float)Math.Pow(0.5F, 0.5F * simulationStep) * ((CALIB[I, 4] - 1.0F) / 2.0F);
                }
                else 
                {
                    CALIB[I, 1] = 1.0F;
                }
                if (configuration.CALD)
                {
                    CALIB[I, 2] = (1.0F + CALIB[I, 6]) / 2.0F + (float)Math.Pow(0.5F, 0.5F * simulationStep) * ((CALIB[I, 6] - 1.0F) / 2.0F);
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

            // find red alder site index for natural (DOUG? non-plantation?) stands
            // BUGBUG heightOfTallestRedAlderInFeet, RASI, and RAAGE not initialized in Fortran
            float heightOfTallestRedAlderInFeet = 0.0F;
            float redAlderSiteIndex = 0.0F;
            if ((configuration.Variant == Variant.Nwo) || (configuration.Variant == Variant.Swo))
            {
                for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
                {
                    if (stand.Species[treeIndex] == FiaCode.AlnusRubra)
                    {
                        float alderHeightInFeet = stand.Height[treeIndex];
                        if (alderHeightInFeet > heightOfTallestRedAlderInFeet)
                        {
                            heightOfTallestRedAlderInFeet = alderHeightInFeet;
                        }
                    }
                }
                redAlderSiteIndex = Stats.ConiferToRedAlderSiteIndex(stand.PrimarySiteIndex);
            }

            // CALCULATE RED ALDER AGE FOR NATURAL STANDS
            // BUGBUG RAAGE not initialized for MAXRAH <= 0.0 in Fortran code
            float redAlderAge = 0.0F;
            if (heightOfTallestRedAlderInFeet > 0.0F)
            {
                RedAlder.RAGEA(heightOfTallestRedAlderInFeet, redAlderSiteIndex, out redAlderAge);
            }
            if (redAlderAge < 0.0F)
            {
                redAlderAge = 55.0F;
                Stats.RASITE(heightOfTallestRedAlderInFeet, redAlderAge, out redAlderSiteIndex);
            }
            if (redAlderAge > 55.0F)
            {
                redAlderAge = 55.0F;
            }

            // CALCULATE DENSITY VARIABLES AT SOG
            Stats.SSTATS(configuration.Variant, stand, out float SBA1, out float _, out float _, out TreeCompetition competitionBeforeGrowth);

            // CALCULATE CCH AND CROWN CLOSURE AT SOG
            float[] CCH = new float[41];
            CrownGrowth.CRNCLO(configuration.Variant, stand, CCH, out float _);
            float OLD = 0.0F;
            TreeGrowth.GROW(ref simulationStep, configuration, stand, NSPN, ref TCYCLE, ref FCYCLE, 
                            SBA1, competitionBeforeGrowth, CALIB, PN, YF, BABT, BART,
                            YT, CCH, ref OLD, redAlderAge, redAlderSiteIndex, out TreeCompetition _);

            if (configuration.IsEvenAge == false)
            {
                stand.AgeInYears = 0;
                stand.BreastHeightAgeInYears = 0;
            }
            float X = 100.0F * (OLD / (BIG6 - BNXT));
            if (X > 50.0F)
            {
                stand.Warnings.TreesOld = true;
            }
            if (configuration.Variant == Variant.Swo)
            {
                if (configuration.IsEvenAge && (stand.BreastHeightAgeInYears > 500.0F))
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else if (configuration.Variant == Variant.Nwo || configuration.Variant == Variant.Smc)
            {
                if (configuration.IsEvenAge && (stand.BreastHeightAgeInYears > 120.0F))
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else
            {
                if (configuration.IsEvenAge && (stand.AgeInYears > 30.0F))
                {
                    stand.Warnings.TreesOld = true;
                }
            }
        }

        /// <summary>
        /// Does argument checking and raises error flags if problems are found.
        /// </summary>
        /// <param name="simulationStep"></param>
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
        private static void EDIT(int simulationStep, OrganonConfiguration configuration, Stand stand, float[,] ACALIB, float[] PN, float[] YSF, float BABT, float[] BART, float[] YST,
                                 out int NSPN, out int BIG6, out int BNXT)
        {
            if (stand.TreeRecordCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stand.TreeRecordCount));
            }
            if (Enum.IsDefined(typeof(Variant), configuration.Variant) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.Variant));
            }
            if (stand.NPTS < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stand.NPTS));
            }
            if ((stand.PrimarySiteIndex <= 0.0F) || (stand.PrimarySiteIndex > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(stand.PrimarySiteIndex));
            }
            if ((stand.MortalitySiteIndex <= 0.0F) || (stand.MortalitySiteIndex > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(stand.MortalitySiteIndex));
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

            if (simulationStep < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(simulationStep));
            }
            for (int I = 0; I < 3; ++I)
            {
                for (int J = 0; J < 18; ++J)
                {
                    if ((ACALIB[J, I] > 2.0F) || (ACALIB[J, I] < 0.5F))
                    {
                        throw new ArgumentOutOfRangeException(nameof(simulationStep));
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

            if ((configuration.Variant >= Variant.Rap) && (stand.PrimarySiteIndex < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(stand.PrimarySiteIndex));
            }
            if ((configuration.Variant >= Variant.Rap) && (configuration.PDEN < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.PDEN));
            }
            if (!configuration.IsEvenAge && (configuration.Variant >= Variant.Rap))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.IsEvenAge));
            }

            // TODO: is it desirable to clear existing stand warnings?
            stand.Warnings.BigSixHeightAbovePotential = false;
            stand.Warnings.LessThan50TreeRecords = false;
            stand.Warnings.MortalitySiteIndexOutOfRange = false;
            stand.Warnings.OtherSpeciesBasalAreaTooHigh = false;
            stand.Warnings.PrimarySiteIndexOutOfRange = false;
            stand.Warnings.TreesOld = false;
            stand.Warnings.TreesYoung = false;

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
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = stand.Species[treeIndex];
                if (StandGrowth.IsSpeciesSupported(configuration.Variant, species) == false)
                {
                    throw new NotSupportedException(String.Format("Species {0} of tree {1} is not supported by variant {2}.", species, treeIndex, configuration.Variant));
                }
                float dbhInInches = stand.Dbh[treeIndex];
                if (dbhInInches < 0.09F)
                {
                    throw new NotSupportedException(String.Format("Diameter of tree {0} is less than 0.1 inches.", treeIndex));
                }
                float heightInFeet = stand.Height[treeIndex];
                if (heightInFeet < 4.5F)
                {
                    throw new NotSupportedException(String.Format("Height of tree {0} is less than 4.5 feet.", treeIndex));
                }
                float crownRatio = stand.CrownRatio[treeIndex];
                if ((crownRatio < 0.0F) || (crownRatio > 1.0F))
                {
                    throw new NotSupportedException(String.Format("Crown ratio of tree {0} is not between 0 and 1.", treeIndex));
                }
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
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
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode speciesCode = stand.Species[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                switch (configuration.Variant)
                {
                    // SWO BIG SIX
                    case Variant.Swo:
                        if ((speciesCode == FiaCode.PinusPonderosa) && (heightInFeet > MAXPP))
                        {
                            MAXPP = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.CalocedrusDecurrens) && (heightInFeet > MAXIC))
                        {
                            MAXIC = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.PseudotsugaMenziesii) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        // BUGBUG: why are true firs and sugar pine being assigned to Douglas-fir max height?
                        else if ((speciesCode == FiaCode.AbiesConcolor) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.AbiesGrandis) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.PinusLambertiana) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        if ((speciesCode == FiaCode.AbiesGrandis) && (heightInFeet > MAXGF))
                        {
                            MAXGF = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.PseudotsugaMenziesii) && (heightInFeet > MAXDF))
                        {
                            MAXDF = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.TsugaHeterophylla) && (heightInFeet > MAXWH))
                        {
                            MAXWH = heightInFeet;
                        }
                        break;
                    case Variant.Rap:
                        if ((speciesCode == FiaCode.AlnusRubra) && (heightInFeet > MAXRA))
                        {
                            MAXRA = heightInFeet;
                        }
                        break;
                }

                int speciesGroup = GetSpeciesGroup(configuration.Variant, speciesCode);
                stand.SpeciesGroup[treeIndex] = speciesGroup;
                if (configuration.Variant >= Variant.Rap)
                {
                    // BUGBUG: encapsulation violation - move this into GetSpeciesGroup()
                    IIB = 1;
                }
                if (speciesGroup < IIB)
                {
                    ++BIG6;
                    if (stand.LiveExpansionFactor[treeIndex] < 0.0F)
                    {
                        ++BNXT;
                    }
                }
            }

            // DETERMINE IF SPECIES MIX CORRECT FOR STAND AGE
            float standBasalArea = 0.0F;
            float standBigSixBasalArea = 0.0F;
            float standHardwoodBasalArea = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                if (expansionFactor < 0.0F)
                {
                    continue;
                }

                float dbhInInches = stand.Dbh[treeIndex];
                float basalArea = expansionFactor * dbhInInches * dbhInInches;
                standBasalArea += basalArea;

                int speciesGroup = stand.SpeciesGroup[treeIndex];
                if (speciesGroup < IIB)
                {
                    standBigSixBasalArea += basalArea;
                }
                if (configuration.Variant == Variant.Swo)
                {
                    FiaCode species = stand.Species[treeIndex];
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
            // BUGBUG move maximum site indices to variant capabilities
            switch (configuration.Variant)
            {
                case Variant.Swo:
                    if ((stand.PrimarySiteIndex > 0.0F) && ((stand.PrimarySiteIndex < 40.0F) || (stand.PrimarySiteIndex > 150.0F)))
                    {
                        stand.Warnings.PrimarySiteIndexOutOfRange = true;
                    }
                    if ((stand.MortalitySiteIndex > 0.0F) && ((stand.MortalitySiteIndex < 50.0F) || (stand.MortalitySiteIndex > 140.0F)))
                    {
                        stand.Warnings.MortalitySiteIndexOutOfRange = true;
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if ((stand.PrimarySiteIndex > 0.0F) && ((stand.PrimarySiteIndex < 90.0F) || (stand.PrimarySiteIndex > 142.0F)))
                    {
                        stand.Warnings.PrimarySiteIndexOutOfRange = true;
                    }
                    if ((stand.MortalitySiteIndex > 0.0F) && ((stand.MortalitySiteIndex < 90.0F) || (stand.MortalitySiteIndex > 142.0F)))
                    {
                        stand.Warnings.MortalitySiteIndexOutOfRange = true;
                    }
                    break;
                case Variant.Rap:
                    if ((stand.PrimarySiteIndex < 20.0F) || (stand.PrimarySiteIndex > 125.0F))
                    {
                        stand.Warnings.PrimarySiteIndexOutOfRange = true;
                    }
                    if ((stand.MortalitySiteIndex > 0.0F) && (stand.MortalitySiteIndex < 90.0F || stand.MortalitySiteIndex > 142.0F))
                    {
                        stand.Warnings.MortalitySiteIndexOutOfRange = true;
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
                        float MAXHT = (stand.MortalitySiteIndex - 4.5F) * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.164985 * (stand.MortalitySiteIndex - 4.5), 0.288169)))) + 4.5F;
                        if (MAXPP > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (MAXIC > 0.0F)
                    {
                        float ICSI = (0.66F * stand.PrimarySiteIndex) - 4.5F;
                        float MAXHT = ICSI * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.174929 * ICSI, 0.281176)))) + 4.5F;
                        if (MAXIC > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (MAXDF > 0.0F)
                    {
                        float MAXHT = (stand.PrimarySiteIndex - 4.5F) * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.174929 * (stand.PrimarySiteIndex - 4.5), 0.281176)))) + 4.5F;
                        if (MAXDF > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if (MAXDF > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.PrimarySiteIndex - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (MAXDF > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (MAXGF > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.PrimarySiteIndex - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (MAXGF > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (MAXWH > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.MortalitySiteIndex - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (0.00192F + 0.00007F * Z50);
                        if (MAXWH > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
                case Variant.Rap:
                    if (MAXRA > 0.0F)
                    {
                        RedAlder.WHHLB_H40(stand.PrimarySiteIndex, 20.0F, 150.0F, out float MAXHT);
                        if (MAXRA > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
            }

            if (configuration.IsEvenAge && (configuration.Variant != Variant.Smc))
            {
                stand.Warnings.TreesYoung = stand.BreastHeightAgeInYears < 10;
            }

            float requiredWellKnownSpeciesBasalAreaFraction;
            switch (configuration.Variant)
            {
                case Variant.Nwo:
                    requiredWellKnownSpeciesBasalAreaFraction = 0.5F;
                    break;
                case Variant.Rap:
                    requiredWellKnownSpeciesBasalAreaFraction = 0.8F;
                    break;
                case Variant.Smc:
                    requiredWellKnownSpeciesBasalAreaFraction = 0.5F;
                    break;
                case Variant.Swo:
                    requiredWellKnownSpeciesBasalAreaFraction = 0.2F;
                    break;
                default:
                    throw new NotSupportedException(String.Format("Unhandled Organon variant {0}.", configuration.Variant));
            }
            if ((standBigSixBasalArea + standHardwoodBasalArea) < (requiredWellKnownSpeciesBasalAreaFraction * standBasalArea))
            {
                stand.Warnings.OtherSpeciesBasalAreaTooHigh = true;
            }
            if (stand.TreeRecordCount < 50)
            {
                stand.Warnings.LessThan50TreeRecords = true;
            }

            CKAGE(configuration, stand, out float OLD);

            float X = 100.0F * (OLD / (BIG6 - BNXT));
            if (X >= 50.0F)
            {
                stand.Warnings.TreesOld = true;
            }
            if (configuration.Variant == Variant.Swo)
            {
                if (configuration.IsEvenAge && stand.BreastHeightAgeInYears > 500)
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else if (configuration.Variant == Variant.Nwo || configuration.Variant == Variant.Smc)
            {
                if (configuration.IsEvenAge && stand.BreastHeightAgeInYears > 120)
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else
            {
                if (configuration.IsEvenAge && stand.AgeInYears > 30)
                {
                    stand.Warnings.TreesOld = true;
                }
            }

            // BUGBUG: this is overcomplicated, should just check against maximum stand age using time step from OrganonVapabilities
            int standAgeBudgetAvailableAtNextTimeStep;
            if (configuration.IsEvenAge)
            {
                switch (configuration.Variant)
                {
                    case Variant.Swo:
                        standAgeBudgetAvailableAtNextTimeStep = 500 - stand.AgeInYears - 5;
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        standAgeBudgetAvailableAtNextTimeStep = 120 - stand.AgeInYears - 5;
                        break;
                    case Variant.Rap:
                        standAgeBudgetAvailableAtNextTimeStep = 30 - stand.AgeInYears - 1;
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
                        standAgeBudgetAvailableAtNextTimeStep = 500 - (simulationStep + 1) * 5;
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        standAgeBudgetAvailableAtNextTimeStep = 120 - (simulationStep + 1) * 5;
                        break;
                    case Variant.Rap:
                        standAgeBudgetAvailableAtNextTimeStep = 30 - (simulationStep + 1) * 1;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (standAgeBudgetAvailableAtNextTimeStep < 0)
            {
                stand.Warnings.TreesOld = true;
            }

            float B1 = -0.04484724F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = stand.Species[treeIndex];
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

                float dbhInInches = stand.Dbh[treeIndex];
                float potentialHeight = 4.5F + B0 * dbhInInches / (1.0F - B1 * dbhInInches);
                float heightInFeet = stand.Height[treeIndex];
                if (heightInFeet > potentialHeight)
                {
                    stand.TreeHeightWarning[treeIndex] = true;
                }
            }
        }

        private static int GetSpeciesGroup(Variant variant, FiaCode species)
        {
            switch (variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    return Constant.NwoSmcSpecies.IndexOf(species);
                case Variant.Rap:
                    return Constant.RapSpecies.IndexOf(species);
                case Variant.Swo:
                    int speciesGroup = Constant.SwoSpecies.IndexOf(species);
                    if (speciesGroup > 1)
                    {
                        --speciesGroup;
                    }
                    return speciesGroup;
                default:
                    throw VariantExtensions.CreateUnhandledVariantException(variant);
            }
        }

        private static bool IsSpeciesSupported(Variant variant, FiaCode speciesCode)
        {
            int speciesGroup = StandGrowth.GetSpeciesGroup(variant, speciesCode);
            return speciesGroup >= 0;
        }

        private static void CKAGE(OrganonConfiguration configuration, Stand stand, out float OLD)
        {
            OLD = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float treeHeightInFeet = stand.Height[treeIndex];
                if (treeHeightInFeet < 4.5F)
                {
                    continue;
                }

                int speciesGroup = stand.SpeciesGroup[treeIndex];
                float growthEffectiveAge = 0.0F; // BUGBUG not intitialized on all Fortran paths
                float IDXAGE;
                float SITE;
                switch (configuration.Variant)
                {
                    case Variant.Swo:
                        // GROWTH EFFECTIVE AGE FROM HANN AND SCRIVANI'S (1987) DOMINANT HEIGHT GROWTH EQUATION
                        bool treatAsDouglasFir = false;
                        if (speciesGroup == 3)
                        {
                            SITE = stand.MortalitySiteIndex - 4.5F;
                        }
                        else
                        {
                            SITE = stand.PrimarySiteIndex - 4.5F;
                            if (speciesGroup == 5)
                            {
                                SITE = stand.PrimarySiteIndex * 0.66F - 4.5F;
                            }
                            treatAsDouglasFir = true;
                        }
                        HeightGrowth.HS_HG(treatAsDouglasFir, SITE, treeHeightInFeet, out growthEffectiveAge, out _);
                        IDXAGE = 500.0F;
                        break;
                    case Variant.Nwo:
                        float GP = 5.0F;
                        if (speciesGroup == 3)
                        {
                            // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQATION
                            SITE = stand.MortalitySiteIndex;
                            HeightGrowth.F_HG(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        else
                        {
                            // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOR DOUGLAS-FIR AND GRAND FIR
                            SITE = stand.PrimarySiteIndex;
                            HeightGrowth.BrucePsmeAbgrGrowthEffectiveAge(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        IDXAGE = 120.0F;
                        break;
                    case Variant.Smc:
                        GP = 5.0F;
                        if (speciesGroup == 3)
                        {
                            // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQUATION
                            SITE = stand.MortalitySiteIndex;
                            HeightGrowth.F_HG(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        else
                        {
                            // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOP DOUGLAS-FIR AND GRAND FIR
                            SITE = stand.PrimarySiteIndex;
                            HeightGrowth.BrucePsmeAbgrGrowthEffectiveAge(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        IDXAGE = 120.0F;
                        break;
                    case Variant.Rap:
                        if (speciesGroup == 1)
                        {
                            // GROWTH EFFECTIVE AGE FROM WEISKITTEL ET AL.'S (2009) RED ALDER DOMINANT HEIGHT GROWTH EQUATION
                            SITE = stand.PrimarySiteIndex;
                            RedAlder.WHHLB_SI_UC(SITE, configuration.PDEN, out float SI_UC);
                            RedAlder.WHHLB_GEA(treeHeightInFeet, SI_UC, out growthEffectiveAge);
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
            CCH[40] = stand.Height[0];
            for (int treeIndex = 1; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float heightInFeet = stand.Height[treeIndex];
                if (heightInFeet > CCH[40])
                {
                    CCH[40] = heightInFeet;
                }
            }

            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                int speciesGroup = stand.SpeciesGroup[treeIndex];
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet  = stand.Height[treeIndex];
                float crownRatio = stand.CrownRatio[treeIndex];

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
            else if ((variant == Variant.Nwo) || (variant == Variant.Smc))
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
            for (int treeIndex = stand.TreeRecordCount - NINGRO; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float heightInFeet = stand.Height[treeIndex];
                if (heightInFeet != 0.0F)
                {
                    continue;
                }

                int speciesGroup = stand.SpeciesGroup[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
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
                stand.Height[treeIndex] = 4.5F + ACALIB[speciesGroup, 0] * (RHT - 4.5F);
            }

            Mortality.OldGro(stand, 0.0F, out float OG);
            Stats.SSTATS(variant, stand, out float SBA, out float _, out float _, out TreeCompetition treeCompetition);
            for (int treeIndex = stand.TreeRecordCount - NINGRO; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float crownRatio = stand.CrownRatio[treeIndex];
                if (crownRatio != 0.0F)
                {
                    continue;
                }

                // CALCULATE HCB
                int speciesGroup = stand.SpeciesGroup[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float SCCFL = treeCompetition.GET_CCFL(dbhInInches);
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
                stand.CrownRatio[treeIndex] = (1.0F - (HCB / heightInFeet)) * ACALIB[speciesGroup, 1];
            }
        }
    }
}
