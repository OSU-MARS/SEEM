using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Organon
{
    public class OrganonStand : Stand
    {
        // nominal age of dominiant cohort established after last replacing disturbance
        // In mixed age stands, prevailing approximate age of dominant and co-dominatn trees. In even age stands, time from germination,
        // from planting, or possible since harvest (often these are within 1-2 years of each other).
        public int AgeInYears { get; set; }

        public DouglasFir.SiteConstants DouglasFirSiteConstants { get; private set; }

        // also used for ponderosa (SWO) and western redcedar (NWO) - really secondary or tertiary species site idex
        public float HemlockSiteIndexInFeet { get; init; }

        // legacy SDImax components present in Organon Fortran but superseded by estimation of SDI from site index per FRL Research Contribution 40
        // natural logarithm of quadratic mean diameter associated with a given SDImax
        //public float SdiMaxLnQmd { get; private set; }
        // reciprocal of exponent for slope of SDImax line, e.g. 0.62305 for Reineke => 1/1.605
        //public float SdiMaxReciprocalExponent { get; private set; }

        // site index from ground height in feet (internal variable SI_1 is from breast height), used for most species
        // Also controls SDI max. See FRL Research Contribution 40 (Hann et al. 2003).
        public float SiteIndexInFeet { get; private set; }

        public float RedAlderSiteIndexInFeet { get; private set; }
        public float RedAlderGrowthEffectiveAge { get; set; }

        public OrganonWarnings Warnings { get; private init; }

        public SortedList<FiaCode, bool[]> TreeHeightWarningBySpecies { get; private init; }

        public OrganonStand(OrganonVariant variant, int ageInYears, float primarySiteIndexInFeet)
        {
            if ((ageInYears < 0) || (ageInYears > Constant.Maximum.AgeInYears))
            {
                throw new ArgumentOutOfRangeException(nameof(ageInYears));
            }
            if ((primarySiteIndexInFeet < Constant.Minimum.SiteIndexInFeet) || (primarySiteIndexInFeet > Constant.Maximum.SiteIndexInFeet))
            {
                throw new ArgumentOutOfRangeException(nameof(primarySiteIndexInFeet));
            }

            this.AgeInYears = ageInYears;
            this.DouglasFirSiteConstants = new(primarySiteIndexInFeet);
            this.HemlockSiteIndexInFeet = variant.ToHemlockSiteIndex(primarySiteIndexInFeet);
            this.PlantingDensityInTreesPerHectare = 0.0F;
            this.RedAlderSiteIndexInFeet = -1.0F;
            this.RedAlderGrowthEffectiveAge = -1.0F;
            this.SiteIndexInFeet = primarySiteIndexInFeet;
            this.TreeHeightWarningBySpecies = [];
            this.Warnings = new OrganonWarnings();
        }

        public OrganonStand(OrganonStand other)
            : base(other)
        {
            this.AgeInYears = other.AgeInYears;
            this.DouglasFirSiteConstants = other.DouglasFirSiteConstants; // currently immutable, so shallow copy for now
            this.HemlockSiteIndexInFeet = other.HemlockSiteIndexInFeet;
            this.Name = other.Name;
            this.RedAlderSiteIndexInFeet = other.RedAlderSiteIndexInFeet;
            this.RedAlderGrowthEffectiveAge = other.RedAlderGrowthEffectiveAge;
            //this.SdiMaxLnQmd = other.SdiMaxLnQmd;
            //this.SdiMaxReciprocalExponent = other.SdiMaxReciprocalExponent;
            this.SiteIndexInFeet = other.SiteIndexInFeet;
            this.TreeHeightWarningBySpecies = [];
            this.Warnings = new OrganonWarnings(other.Warnings);

            foreach (KeyValuePair<FiaCode, bool[]> species in other.TreeHeightWarningBySpecies)
            {
                bool[] heightWarnings = new bool[species.Value.Length];
                species.Value.CopyTo(heightWarnings, 0);
                this.TreeHeightWarningBySpecies.Add(species.Key, heightWarnings);
            }
        }

        public override OrganonStand Clone()
        {
            return new OrganonStand(this);
        }

        public void CopyTreeGrowthFrom(OrganonStand other)
        {
            foreach (Trees otherTreesOfSpecies in other.TreesBySpecies.Values)
            {
                Trees thisTreesOfSpecies = this.TreesBySpecies[otherTreesOfSpecies.Species];
                thisTreesOfSpecies.CopyFrom(otherTreesOfSpecies);
            }
        }

        public void SetHeightToCrownBase(OrganonVariant organonVariant)
        {
            OrganonStandDensity density = new(organonVariant, this);
            float oldGrowthIndicator = OrganonMortality.GetOldGrowthIndicator(organonVariant, this);
            for (int speciesIndex = 0; speciesIndex < this.TreesBySpecies.Count; ++speciesIndex)
            {
                Trees organonTreesOfSpecies = this.TreesBySpecies.Values[speciesIndex];
                Debug.Assert(organonTreesOfSpecies.Units == Units.English);

                // initialize crown ratio from Organon variant
                Debug.Assert(organonTreesOfSpecies.Units == Units.English);
                for (int treeIndex = 0; treeIndex < organonTreesOfSpecies.Count; ++treeIndex)
                {
                    float dbhInInches = organonTreesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = organonTreesOfSpecies.Height[treeIndex];
                    float crownCompetitionFactorLarger = density.GetCrownCompetitionFactorLarger(dbhInInches);
                    float heightToCrownBase = organonVariant.GetHeightToCrownBase(this, organonTreesOfSpecies.Species, heightInFeet, dbhInInches, crownCompetitionFactorLarger, density, oldGrowthIndicator);
                    float crownRatio = (heightInFeet - heightToCrownBase) / heightInFeet;
                    Debug.Assert((crownRatio >= 0.0F) && (crownRatio <= 1.0F));

                    organonTreesOfSpecies.CrownRatio[treeIndex] = crownRatio;
                }

                // alternatively, initialize crown ratio from FVS-PN dubbing
                // https://www.fs.fed.us/fmsc/ftp/fvs/docs/overviews/FVSpn_Overview.pdf, section 4.3.1
                // https://sourceforge.net/p/open-fvs/code/HEAD/tree/trunk/pn/crown.f#l67
                // for live > 1.0 inch DBH
                //   estimated crown ratio = d0 + d1 * 100.0 * SDI / SDImax
                //   PSME d0 = 5.666442, d1 = -0.025199
                //if ((this.TreesBySpecies.Count != 1) || (organonTreesOfSpecies.Species != FiaCode.PseudotsugaMenziesii))
                //{
                //    throw new NotImplementedException();
                //}

                // FVS-PN crown ratio dubbing for Douglas-fir
                // Resulted in 0.28% less volume than Organon NWO on Malcolm Knapp Nelder 1 at stand age 70.
                // float qmd = stand.GetQuadraticMeanDiameter();
                // float reinekeSdi = density.TreesPerAcre * MathF.Pow(0.1F * qmd, 1.605F);
                // float reinekeSdiMax = MathF.Exp((stand.A1 - Constant.NaturalLogOf10) / stand.A2);
                // float meanCrownRatioFvs = 5.666442F - 0.025199F * 100.0F * reinekeSdi / reinekeSdiMax;
                // Debug.Assert(meanCrownRatioFvs >= 0.0F);
                // Debug.Assert(meanCrownRatioFvs <= 10.0F); // FVS uses a 0 to 10 range, so 10 = 100% crown ratio
                // float weibullA = 0.0F;
                // float weibullB = -0.012061F + 1.119712F * meanCrownRatioFvs;
                // float weibullC = 3.2126F;
                // int[] dbhOrder = treesOfSpecies.GetDbhSortOrder();

                // for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                // {
                //     float dbhFraction = (float)dbhOrder[treeIndex] / (float)treesOfSpecies.Count;
                //     float fvsCrownRatio = weibullA + weibullB * MathV.Pow(-1.0F * MathV.Ln(1.0F - dbhFraction), 1.0F / weibullC);
                //     Debug.Assert(fvsCrownRatio >= 0.0F);
                //     Debug.Assert(fvsCrownRatio <= 10.0F);

                //     treesOfSpecies.CrownRatio[treeIndex] = 0.1F * fvsCrownRatio;
                // }
            }
        }

        public void SetRedAlderSiteIndexAndGrowthEffectiveAge()
        {
            // find red alder site index and growth effective age
            // In CIPSR 2.2.4 these paths are disabled for SMC red alder even though it's a supported species, resulting in zero
            // height growth. In this fork the code's called regardless of variant.
            float heightOfTallestRedAlderInFeet = 0.0F;
            if (this.TreesBySpecies.TryGetValue(FiaCode.AlnusRubra, out Trees? redAlders))
            {
                for (int alderIndex = 0; alderIndex < redAlders.Count; ++alderIndex)
                {
                    float heightInFeet = redAlders.Height[alderIndex];
                    if (heightInFeet > heightOfTallestRedAlderInFeet)
                    {
                        heightOfTallestRedAlderInFeet = heightInFeet;
                    }
                }
            }

            this.RedAlderSiteIndexInFeet = RedAlder.ConiferToRedAlderSiteIndex(this.SiteIndexInFeet);
            this.RedAlderGrowthEffectiveAge = RedAlder.GetGrowthEffectiveAge(heightOfTallestRedAlderInFeet, this.RedAlderSiteIndexInFeet);
            if (this.RedAlderGrowthEffectiveAge <= 0.0F)
            {
                this.RedAlderGrowthEffectiveAge = Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears;
                this.RedAlderSiteIndexInFeet = RedAlder.GetSiteIndex(heightOfTallestRedAlderInFeet, this.RedAlderGrowthEffectiveAge);
            }
            else if (this.RedAlderGrowthEffectiveAge > Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears)
            {
                this.RedAlderGrowthEffectiveAge = Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears;
            }
        }

        /// <summary>
        /// Finds SDImax line. Sets A1 (constant of SDImax line) and exponent of SDImax line (dimensionless).
        /// </summary>
        /// <param name="configuration">Organon configuration.</param>
        //public void SetSdiMax(OrganonConfiguration configuration)
        //{
        //    // find maximum Reineke SDI size-density line in natural log space
        //    // In Organon's English units,
        //    //   SDI = TPA (QMD/10)^1.605 => ln(SDI) = ln(TPA) + 1.605 * (ln(QMD) - ln(10))
        //    // Organon's Fortran works with TEMPA1 and A1 = ln(10) + 1/1.605 * ln(SDI) = ln(QMD). A1 is renamed lnQmd in this version of the
        //    // code for clarity.
        //    this.SdiMaxReciprocalExponent = configuration.Variant.TreeModel switch
        //    {
        //        TreeModel.OrganonSwo or 
        //        TreeModel.OrganonNwo or 
        //        TreeModel.OrganonSmc => 0.62305F,// Reineke 1933: 1/1.605 = 0.623053
        //        TreeModel.OrganonRap => 0.64F,// Puettmann et al. 1993: 1/1.5625 = 0.64
        //        _ => throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel),
        //    };
        //    float temporaryLnQmd;
        //    if (configuration.DefaultMaximumSdi > 0.0F)
        //    {
        //        temporaryLnQmd = Constant.NaturalLogOf10 + this.SdiMaxReciprocalExponent * MathV.Ln(configuration.DefaultMaximumSdi);
        //    }
        //    else
        //    {
        //        temporaryLnQmd = configuration.Variant.TreeModel switch
        //        {
        //            TreeModel.OrganonSwo => 6.21113F, // SDImax = exp(6.21113) = 498.3, 530.2 in orignal SWO variant
        //            TreeModel.OrganonNwo => 6.19958F, // SDImax = 492.5, 520.5 in preceeding western Washington variant
        //            TreeModel.OrganonSmc => 6.19958F,
        //            TreeModel.OrganonRap => 5.96F,
        //            _ => throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel),
        //        };
        //    }

        //    // BUGBUG need API with maximum species group ID to safely allocate BAGRP
        //    float douglasFirBasalArea = 0.0F;
        //    float hemlockBasalArea = 0.0F;
        //    float ponderosaBasalArea = 0.0F;
        //    float totalBasalArea = 0.0F;
        //    float trueFirBasalArea = 0.0F;
        //    foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
        //    {
        //        float speciesBasalArea = 0.0F;
        //        for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
        //        {
        //            float basalArea = treesOfSpecies.GetBasalArea(treeIndex);
        //            speciesBasalArea += basalArea;
        //        }

        //        switch (treesOfSpecies.Species)
        //        {
        //            case FiaCode.AbiesAmabalis:
        //            case FiaCode.AbiesConcolor:
        //            case FiaCode.AbiesGrandis:
        //            case FiaCode.AbiesProcera:
        //                trueFirBasalArea += speciesBasalArea;
        //                break;
        //            case FiaCode.PinusPonderosa:
        //                ponderosaBasalArea += speciesBasalArea;
        //                break;
        //            case FiaCode.PseudotsugaMenziesii:
        //                douglasFirBasalArea += speciesBasalArea;
        //                break;
        //            case FiaCode.TsugaHeterophylla:
        //                hemlockBasalArea += speciesBasalArea;
        //                break;
        //        }
        //        totalBasalArea += speciesBasalArea;
        //    }

        //    float douglasFirProportion = 0.0F;
        //    float hemlockProportion = 0.0F;
        //    float ponderosaProportion = 0.0F;
        //    float trueFirProportion = 0.0F;
        //    if (totalBasalArea > 0.0F)
        //    {
        //        douglasFirProportion = douglasFirBasalArea / totalBasalArea;
        //        hemlockProportion = hemlockBasalArea / totalBasalArea;
        //        ponderosaProportion = ponderosaBasalArea / totalBasalArea;
        //        trueFirProportion = trueFirBasalArea / totalBasalArea;
        //    }

        //    float a1multiplier;
        //    switch (configuration.Variant.TreeModel)
        //    {
        //        case TreeModel.OrganonSwo:
        //            float trueFirModifier = 1.03481817F;
        //            if (configuration.TrueFirMaximumSdi > 0.0F)
        //            {
        //                trueFirModifier = Constant.NaturalLogOf10 + this.SdiMaxReciprocalExponent * MathV.Ln(configuration.TrueFirMaximumSdi) / temporaryLnQmd;
        //            }
        //            float hemlockModifier = 0.9943501F;
        //            if (configuration.HemlockMaximumSdi > 0.0F)
        //            {
        //                hemlockModifier = Constant.NaturalLogOf10 + this.SdiMaxReciprocalExponent * MathV.Ln(configuration.HemlockMaximumSdi) / temporaryLnQmd;
        //            }

        //            if (douglasFirProportion >= 0.5F)
        //            {
        //                a1multiplier = 1.0F;
        //            }
        //            else if (trueFirProportion >= 0.6666667F)
        //            {
        //                a1multiplier = trueFirModifier;
        //            }
        //            else if (ponderosaProportion >= 0.6666667F)
        //            {
        //                a1multiplier = hemlockModifier;
        //            }
        //            else
        //            {
        //                a1multiplier = douglasFirProportion + trueFirModifier * trueFirProportion + hemlockModifier * ponderosaProportion;
        //            }
        //            break;
        //        case TreeModel.OrganonNwo:
        //        case TreeModel.OrganonSmc:
        //            trueFirModifier = 1.03481817F;
        //            if (configuration.TrueFirMaximumSdi > 0.0F)
        //            {
        //                trueFirModifier = Constant.NaturalLogOf10 + this.SdiMaxReciprocalExponent * MathV.Ln(configuration.TrueFirMaximumSdi) / temporaryLnQmd;
        //            }
        //            // Based on Johnson's (2000) analysis of Max. SDI for western hemlock
        //            hemlockModifier = 1.014293245F;
        //            if (configuration.HemlockMaximumSdi > 0.0F)
        //            {
        //                hemlockModifier = Constant.NaturalLogOf10 + this.SdiMaxReciprocalExponent * MathV.Ln(configuration.HemlockMaximumSdi) / temporaryLnQmd;
        //            }

        //            if (douglasFirProportion >= 0.5F)
        //            {
        //                a1multiplier = 1.0F;
        //            }
        //            else if (hemlockProportion >= 0.5F)
        //            {
        //                a1multiplier = hemlockModifier;
        //            }
        //            else if (trueFirProportion >= 0.6666667)
        //            {
        //                a1multiplier = trueFirModifier;
        //            }
        //            else
        //            {
        //                a1multiplier = douglasFirProportion + hemlockModifier * hemlockProportion + trueFirModifier * trueFirProportion;
        //            }
        //            break;
        //        case TreeModel.OrganonRap:
        //            a1multiplier = 1.0F;
        //            break;
        //        default:
        //            throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel);
        //    }
        //    if (a1multiplier <= 0.0F)
        //    {
        //        // BUGBUG: silently ignores error condition
        //        //Debug.Assert(A1MOD > 0.0F);
        //        a1multiplier = 1.0F;
        //    }

        //    this.SdiMaxLnQmd = a1multiplier * temporaryLnQmd;
        //}
    }
}