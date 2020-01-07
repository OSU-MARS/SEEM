using System;
using System.Collections.Generic;
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
        /// <param name="ACALIB">Array of calibration coefficients. Values must be between 0.5 and 2.0.</param>
        /// <param name="PN">Pounds of nitrogen per acre?</param>
        /// <param name="YSF">Years fertilization performed?</param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YST">Years thinning performed?</param>
        public static void EXECUTE(int simulationStep, OrganonConfiguration configuration, Stand stand, Dictionary<FiaCode, float[]> ACALIB, float[] PN, float[] YSF, 
                                   float BABT, float[] BART, float[] YST)
        {
            // BUGBUG: simulationStep largely duplicates stand age
            ValidateArguments(simulationStep, configuration, stand, ACALIB, PN, YSF, BABT, BART, YST, out int BIG6, out int BNXT);

            // BUGBUG: 5 * simulationStep is incorrect for RAP
            int simulationYear = Constant.DefaultTimeStepInYears * simulationStep;
            int FCYCLE = 0;
            int TCYCLE = 0;
            if (configuration.Fertilizer && (YSF[0] == (float)simulationYear))
            {
                FCYCLE = 1;
            }

            Dictionary<FiaCode, float[]> CALIB = new Dictionary<FiaCode, float[]>(ACALIB.Count);
            foreach (KeyValuePair<FiaCode, float[]> species in ACALIB)
            {
                float[] speciesCalibration = new float[6];
                CALIB.Add(species.Key, speciesCalibration);

                if (configuration.CalibrateHeight)
                {
                    speciesCalibration[0] = (1.0F + ACALIB[species.Key][0]) / 2.0F + (float)Math.Pow(0.5, 0.5 * simulationStep) * ((ACALIB[species.Key][0] - 1.0F) / 2.0F);
                }
                else 
                {
                    speciesCalibration[0] = 1.0F;
                }
                if (configuration.CalibrateCrownRatio)
                {
                    speciesCalibration[1] = (1.0F + ACALIB[species.Key][1]) / 2.0F + (float)Math.Pow(0.5F, 0.5F * simulationStep) * ((ACALIB[species.Key][1] - 1.0F) / 2.0F);
                }
                else 
                {
                    speciesCalibration[1] = 1.0F;
                }
                if (configuration.CalibrateDiameter)
                {
                    speciesCalibration[2] = (1.0F + ACALIB[species.Key][2]) / 2.0F + (float)Math.Pow(0.5F, 0.5F * simulationStep) * ((ACALIB[species.Key][2] - 1.0F) / 2.0F);
                }
                else 
                {
                    speciesCalibration[2] = 1.0F;
                }
            }

            float[] YF = new float[5];
            float[] YT = new float[5];
            for (int I = 0; I < 5; ++I)
            {
                YF[I] = YSF[I];
                YT[I] = YST[I];
            }

            // find red alder site index and growth effective age
            float redAlderAge = stand.SetRedAlderSiteIndex();

            // density at start of growth
            StandDensity densityBeforeGrowth = new StandDensity(stand, configuration.Variant);

            // CCH and crown closure at start of growth
            float[] CCH = new float[41];
            CrownGrowth.CRNCLO(configuration.Variant, stand, CCH, out float _);
            float OLD = 0.0F;
            TreeGrowth.GROW(ref simulationStep, configuration, stand, ref TCYCLE, ref FCYCLE, densityBeforeGrowth, CALIB, PN, YF, BABT, BART,
                            YT, CCH, ref OLD, redAlderAge, out StandDensity _);

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
            if (configuration.Variant.Variant == Variant.Swo)
            {
                if (configuration.IsEvenAge && (stand.BreastHeightAgeInYears > 500.0F))
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else if ((configuration.Variant.Variant == Variant.Nwo) || (configuration.Variant.Variant == Variant.Smc))
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

                FiaCode species = stand.Species[treeIndex];
                float growthEffectiveAge = 0.0F; // BUGBUG not intitialized on all Fortran paths
                float IDXAGE;
                float SITE;
                switch (configuration.Variant.Variant)
                {
                    case Variant.Swo:
                        // GROWTH EFFECTIVE AGE FROM HANN AND SCRIVANI'S (1987) DOMINANT HEIGHT GROWTH EQUATION
                        bool treatAsDouglasFir = false;
                        if (species == FiaCode.TsugaHeterophylla)
                        {
                            SITE = stand.HemlockSiteIndex - 4.5F;
                        }
                        else
                        {
                            SITE = stand.SiteIndex - 4.5F;
                            if (species == FiaCode.PinusLambertiana)
                            {
                                SITE = stand.SiteIndex * 0.66F - 4.5F;
                            }
                            treatAsDouglasFir = true;
                        }
                        HeightGrowth.HS_HG(treatAsDouglasFir, SITE, treeHeightInFeet, out growthEffectiveAge, out _);
                        IDXAGE = 500.0F;
                        break;
                    case Variant.Nwo:
                        float GP = 5.0F;
                        if (species == FiaCode.TsugaHeterophylla)
                        {
                            // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQATION
                            SITE = stand.HemlockSiteIndex;
                            HeightGrowth.F_HG(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        else
                        {
                            // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOR DOUGLAS-FIR AND GRAND FIR
                            SITE = stand.SiteIndex;
                            HeightGrowth.BrucePsmeAbgrGrowthEffectiveAge(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        IDXAGE = 120.0F;
                        break;
                    case Variant.Smc:
                        GP = 5.0F;
                        if (species == FiaCode.TsugaHeterophylla)
                        {
                            // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQUATION
                            SITE = stand.HemlockSiteIndex;
                            HeightGrowth.F_HG(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        else
                        {
                            // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOP DOUGLAS-FIR AND GRAND FIR
                            SITE = stand.SiteIndex;
                            HeightGrowth.BrucePsmeAbgrGrowthEffectiveAge(SITE, treeHeightInFeet, GP, out growthEffectiveAge, out _);
                        }
                        IDXAGE = 120.0F;
                        break;
                    case Variant.Rap:
                        if (species == FiaCode.AlnusRubra)
                        {
                            // GROWTH EFFECTIVE AGE FROM WEISKITTEL ET AL.'S (2009) RED ALDER DOMINANT HEIGHT GROWTH EQUATION
                            SITE = stand.SiteIndex;
                            RedAlder.WHHLB_SI_UC(SITE, configuration.PDEN, out float SI_UC);
                            RedAlder.WHHLB_GEA(treeHeightInFeet, SI_UC, out growthEffectiveAge);
                        }
                        IDXAGE = 30.0F;
                        break;
                    default:
                        throw OrganonVariant.CreateUnhandledVariantException(configuration.Variant.Variant);
                }

                // BUGBUG inconsistent use of < IB rather than <= IB
                if (configuration.Variant.IsBigSixSpecies(species) && (growthEffectiveAge > IDXAGE))
                {
                    OLD += 1.0F;
                }
            }
        }

        public static void CROWN_CLOSURE(OrganonVariant variant, Stand stand, out float CC)
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
                FiaCode species = stand.Species[treeIndex];
                float expansionFactor = stand.LiveExpansionFactor[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet  = stand.Height[treeIndex];
                float crownRatio = stand.CrownRatio[treeIndex];

                float CL = crownRatio * heightInFeet;
                float HCB = heightInFeet - CL;
                float EXPFAC = expansionFactor / (float)stand.NumberOfPlots;
                float MCW = variant.GetMaximumCrownWidth(species, dbhInInches, heightInFeet);
                float LCW = variant.GetLargestCrownWidth(species, MCW, crownRatio, dbhInInches, heightInFeet);
                float HLCW = variant.GetHeightToLargestCrownWidth(species, heightInFeet, crownRatio);
                CrownGrowth.CALC_CC(variant, species, HLCW, LCW, heightInFeet, dbhInInches, HCB, EXPFAC, CCH);
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
        public static void INGRO_FILL(OrganonVariant variant, Stand stand, int NINGRO, Dictionary<FiaCode, float[]> ACALIB)
        {
            // ROUTINE TO CALCULATE MISSING CROWN RATIOS
            // BUGBUG: does this duplicate site index code elsewhere?
            // NINGRO = NUMBER OF TREES ADDED
            float SITE_1 = stand.SiteIndex;
            float SITE_2 = stand.HemlockSiteIndex;
            float SI_1;
            if (variant.Variant == Variant.Swo)
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
            else if ((variant.Variant == Variant.Nwo) || (variant.Variant == Variant.Smc))
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

                FiaCode species = stand.Species[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float RHT = variant.GetPredictedHeight(species, dbhInInches);
                stand.Height[treeIndex] = 4.5F + ACALIB[species][0] * (RHT - 4.5F);
            }

            Mortality.OldGro(variant, stand, 0.0F, out float OG);
            StandDensity standDensity = new StandDensity(stand, variant);
            for (int treeIndex = stand.TreeRecordCount - NINGRO; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                float crownRatio = stand.CrownRatio[treeIndex];
                if (crownRatio != 0.0F)
                {
                    continue;
                }

                // CALCULATE HCB
                FiaCode species = stand.Species[treeIndex];
                float dbhInInches = stand.Dbh[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                float SCCFL = standDensity.GetCrownCompetitionFactorLarger(dbhInInches);
                float HCB = variant.GetHeightToCrownBase(species, heightInFeet, dbhInInches, SCCFL, standDensity.BasalAreaPerAcre, SI_1, SI_2, OG);
                if (HCB < 0.0F)
                {
                    HCB = 0.0F;
                }
                if (HCB > 0.95F * heightInFeet)
                {
                    HCB = 0.95F * heightInFeet;
                }
                stand.CrownRatio[treeIndex] = (1.0F - (HCB / heightInFeet)) * ACALIB[species][1];
            }
        }

        /// <summary>
        /// Does argument checking and raises error flags if problems are found.
        /// </summary>
        /// <param name="simulationStep"></param>
        /// <param name="variant"></param>
        /// <param name="configuration">Organon configuration settings.</param>
        /// <param name="stand"></param>
        /// <param name="ACALIB"></param>
        /// <param name="PN"></param>
        /// <param name="YSF"></param>
        /// <param name="BABT"></param>
        /// <param name="BART"></param>
        /// <param name="YST"></param>
        /// <param name="BIG6"></param>
        /// <param name="BNXT"></param>
        private static void ValidateArguments(int simulationStep, OrganonConfiguration configuration, Stand stand, Dictionary<FiaCode, float[]> ACALIB, float[] PN, float[] YSF, float BABT, float[] BART, float[] YST,
                                              out int BIG6, out int BNXT)
        {
            if (stand.TreeRecordCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stand.TreeRecordCount));
            }
            if (Enum.IsDefined(typeof(Variant), configuration.Variant.Variant) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.Variant));
            }
            if (stand.NumberOfPlots < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stand.NumberOfPlots));
            }
            if ((stand.SiteIndex <= 0.0F) || (stand.SiteIndex > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(stand.SiteIndex));
            }
            if ((stand.HemlockSiteIndex <= 0.0F) || (stand.HemlockSiteIndex > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(stand.HemlockSiteIndex));
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

            if (configuration.DefaultMaximumSdi > Constant.Maximum.Sdi)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.DefaultMaximumSdi));
            }
            if (configuration.TrueFirMaximumSdi > Constant.Maximum.Sdi)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.TrueFirMaximumSdi));
            }
            if (configuration.HemlockMaximumSdi > Constant.Maximum.Sdi)
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.HemlockMaximumSdi));
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
                if ((configuration.Variant.Variant == Variant.Swo) || (configuration.Variant.Variant == Variant.Rap))
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

            if ((configuration.Variant.Variant >= Variant.Rap) && (stand.SiteIndex < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(stand.SiteIndex));
            }
            if ((configuration.Variant.Variant >= Variant.Rap) && (configuration.PDEN < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.PDEN));
            }
            if (!configuration.IsEvenAge && (configuration.Variant.Variant >= Variant.Rap))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.IsEvenAge));
            }

            // TODO: is it desirable to clear existing stand warnings?
            stand.Warnings.BigSixHeightAbovePotential = false;
            stand.Warnings.LessThan50TreeRecords = false;
            stand.Warnings.HemlockSiteIndexOutOfRange = false;
            stand.Warnings.OtherSpeciesBasalAreaTooHigh = false;
            stand.Warnings.SiteIndexOutOfRange = false;
            stand.Warnings.TreesOld = false;
            stand.Warnings.TreesYoung = false;

            foreach (float[] speciesCalibration in ACALIB.Values)
            {
                if ((speciesCalibration.Length != 6) ||
                    (speciesCalibration[0] < 0.5F) || (speciesCalibration[0] > 2.0F) ||
                    (speciesCalibration[1] < 0.5F) || (speciesCalibration[1] > 2.0F) ||
                    (speciesCalibration[2] < 0.5F) || (speciesCalibration[2] > 2.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(ACALIB));
                }
            }

            // check tree records for errors
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode species = stand.Species[treeIndex];
                if (configuration.Variant.IsSpeciesSupported(species) == false)
                {
                    throw new NotSupportedException(String.Format("{0} does not support {1} (tree {2}).", configuration.Variant.Variant, species, treeIndex));
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
            float maxGrandFirHeight = 0.0F;
            float maxDouglasFirHeight = 0.0F;
            float maxWesternHemlockHeight = 0.0F;
            float maxPonderosaHeight = 0.0F;
            float maxIncenseCedarHeight = 0.0F;
            float maxRedAlderHeight = 0.0F;
            for (int treeIndex = 0; treeIndex < stand.TreeRecordCount; ++treeIndex)
            {
                FiaCode speciesCode = stand.Species[treeIndex];
                float heightInFeet = stand.Height[treeIndex];
                switch (configuration.Variant.Variant)
                {
                    // SWO BIG SIX
                    case Variant.Swo:
                        if ((speciesCode == FiaCode.PinusPonderosa) && (heightInFeet > maxPonderosaHeight))
                        {
                            maxPonderosaHeight = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.CalocedrusDecurrens) && (heightInFeet > maxIncenseCedarHeight))
                        {
                            maxIncenseCedarHeight = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.PseudotsugaMenziesii) && (heightInFeet > maxDouglasFirHeight))
                        {
                            maxDouglasFirHeight = heightInFeet;
                        }
                        // BUGBUG: why are true firs and sugar pine being assigned to Douglas-fir max height?
                        else if ((speciesCode == FiaCode.AbiesConcolor) && (heightInFeet > maxDouglasFirHeight))
                        {
                            maxDouglasFirHeight = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.AbiesGrandis) && (heightInFeet > maxDouglasFirHeight))
                        {
                            maxDouglasFirHeight = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.PinusLambertiana) && (heightInFeet > maxDouglasFirHeight))
                        {
                            maxDouglasFirHeight = heightInFeet;
                        }
                        break;
                    case Variant.Nwo:
                    case Variant.Smc:
                        if ((speciesCode == FiaCode.AbiesGrandis) && (heightInFeet > maxGrandFirHeight))
                        {
                            maxGrandFirHeight = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.PseudotsugaMenziesii) && (heightInFeet > maxDouglasFirHeight))
                        {
                            maxDouglasFirHeight = heightInFeet;
                        }
                        else if ((speciesCode == FiaCode.TsugaHeterophylla) && (heightInFeet > maxWesternHemlockHeight))
                        {
                            maxWesternHemlockHeight = heightInFeet;
                        }
                        break;
                    case Variant.Rap:
                        if ((speciesCode == FiaCode.AlnusRubra) && (heightInFeet > maxRedAlderHeight))
                        {
                            maxRedAlderHeight = heightInFeet;
                        }
                        break;
                }

                if (configuration.Variant.IsBigSixSpecies(stand.Species[treeIndex]))
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

                FiaCode species = stand.Species[treeIndex];
                if (configuration.Variant.IsBigSixSpecies(species))
                {
                    standBigSixBasalArea += basalArea;
                }
                if (configuration.Variant.Variant == Variant.Swo)
                {
                    if ((species == FiaCode.ArbutusMenziesii) || (species == FiaCode.ChrysolepisChrysophyllaVarChrysophylla) || (species == FiaCode.QuercusKelloggii))
                    {
                        standHardwoodBasalArea += basalArea;
                    }
                }
            }

            standBasalArea *= Constant.ForestersEnglish / stand.NumberOfPlots;
            standBigSixBasalArea *= Constant.ForestersEnglish / stand.NumberOfPlots;
            if (standBigSixBasalArea < 0.0F)
            {
                throw new NotSupportedException("Total basal area big six species is negative.");
            }

            if (configuration.Variant.Variant >= Variant.Rap)
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
            switch (configuration.Variant.Variant)
            {
                case Variant.Swo:
                    if ((stand.SiteIndex > 0.0F) && ((stand.SiteIndex < 40.0F) || (stand.SiteIndex > 150.0F)))
                    {
                        stand.Warnings.SiteIndexOutOfRange = true;
                    }
                    if ((stand.HemlockSiteIndex > 0.0F) && ((stand.HemlockSiteIndex < 50.0F) || (stand.HemlockSiteIndex > 140.0F)))
                    {
                        stand.Warnings.HemlockSiteIndexOutOfRange = true;
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if ((stand.SiteIndex > 0.0F) && ((stand.SiteIndex < 90.0F) || (stand.SiteIndex > 142.0F)))
                    {
                        stand.Warnings.SiteIndexOutOfRange = true;
                    }
                    if ((stand.HemlockSiteIndex > 0.0F) && ((stand.HemlockSiteIndex < 90.0F) || (stand.HemlockSiteIndex > 142.0F)))
                    {
                        stand.Warnings.HemlockSiteIndexOutOfRange = true;
                    }
                    break;
                case Variant.Rap:
                    if ((stand.SiteIndex < 20.0F) || (stand.SiteIndex > 125.0F))
                    {
                        stand.Warnings.SiteIndexOutOfRange = true;
                    }
                    if ((stand.HemlockSiteIndex > 0.0F) && (stand.HemlockSiteIndex < 90.0F || stand.HemlockSiteIndex > 142.0F))
                    {
                        stand.Warnings.HemlockSiteIndexOutOfRange = true;
                    }
                    break;
            }

            // check tallest trees in stand against maximum height for big six species
            // BUGBUG: need an API for maximum heights rather than inline code here
            switch (configuration.Variant.Variant)
            {
                case Variant.Swo:
                    if (maxPonderosaHeight > 0.0F)
                    {
                        float MAXHT = (stand.HemlockSiteIndex - 4.5F) * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.164985 * (stand.HemlockSiteIndex - 4.5), 0.288169)))) + 4.5F;
                        if (maxPonderosaHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (maxIncenseCedarHeight > 0.0F)
                    {
                        float ICSI = (0.66F * stand.SiteIndex) - 4.5F;
                        float MAXHT = ICSI * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.174929 * ICSI, 0.281176)))) + 4.5F;
                        if (maxIncenseCedarHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (maxDouglasFirHeight > 0.0F)
                    {
                        float MAXHT = (stand.SiteIndex - 4.5F) * (1.0F / (1.0F - (float)Math.Exp(Math.Pow(-0.174929 * (stand.SiteIndex - 4.5), 0.281176)))) + 4.5F;
                        if (maxDouglasFirHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if (maxDouglasFirHeight > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.SiteIndex - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (maxDouglasFirHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (maxGrandFirHeight > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.SiteIndex - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (-0.000733819F + 0.000197693F * Z50);
                        if (maxGrandFirHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    if (maxWesternHemlockHeight > 0.0F)
                    {
                        float Z50 = 2500.0F / (stand.HemlockSiteIndex - 4.5F);
                        float MAXHT = 4.5F + 1.0F / (0.00192F + 0.00007F * Z50);
                        if (maxWesternHemlockHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
                case Variant.Rap:
                    if (maxRedAlderHeight > 0.0F)
                    {
                        RedAlder.WHHLB_H40(stand.SiteIndex, 20.0F, 150.0F, out float MAXHT);
                        if (maxRedAlderHeight > MAXHT)
                        {
                            stand.Warnings.BigSixHeightAbovePotential = true;
                        }
                    }
                    break;
            }

            if (configuration.IsEvenAge && (configuration.Variant.Variant != Variant.Smc))
            {
                stand.Warnings.TreesYoung = stand.BreastHeightAgeInYears < 10;
            }

            float requiredWellKnownSpeciesBasalAreaFraction;
            switch (configuration.Variant.Variant)
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
                    throw OrganonVariant.CreateUnhandledVariantException(configuration.Variant.Variant);
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
            if (configuration.Variant.Variant == Variant.Swo)
            {
                if (configuration.IsEvenAge && stand.BreastHeightAgeInYears > 500)
                {
                    stand.Warnings.TreesOld = true;
                }
            }
            else if (configuration.Variant.Variant == Variant.Nwo || configuration.Variant.Variant == Variant.Smc)
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
                switch (configuration.Variant.Variant)
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
                        throw OrganonVariant.CreateUnhandledVariantException(configuration.Variant.Variant);
                }
            }
            else
            {
                switch (configuration.Variant.Variant)
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
                        throw OrganonVariant.CreateUnhandledVariantException(configuration.Variant.Variant);
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
                        if ((configuration.Variant.Variant == Variant.Nwo) || (configuration.Variant.Variant == Variant.Smc))
                        {
                            B0 = 19.04942539F;
                        }
                        else if (configuration.Variant.Variant == Variant.Rap)
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
    }
}
