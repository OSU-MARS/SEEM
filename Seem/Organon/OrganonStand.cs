using Mars.Seem.Tree;
using System;
using System.Collections.Generic;

namespace Mars.Seem.Organon
{
    public class OrganonStand : Stand
    {
        // time since last stand replacing disturbance
        public int AgeInYears { get; set; }

        // time since oldest cohort of trees in the stand reached breast height (4.5 feet) (DOUG?)
        public int BreastHeightAgeInYears { get; set; }

        public DouglasFir.SiteConstants DouglasFirSiteConstants { get; private set; }

        // also used for ponderosa (SWO) and western redcedar (NWO)
        public float HemlockSiteIndexInFeet { get; init; }

        // number of plots tree data is from
        // If data is for entire stand use one plot.
        public float NumberOfPlots { get; set; }

        // legacy SDImax components present in Organon Fortran but superseded by estimation of SDI from site index per FRL Research Contribution 40
        // natural logarithm of quadratic mean diameter associated with a given SDImax
        //public float SdiMaxLnQmd { get; private set; }
        // reciprocal of exponent for slope of SDImax line, e.g. 0.62305 for Reineke => 1/1.605
        //public float SdiMaxReciprocalExponent { get; private set; }

        // site index from ground height in feet (internal variable SI_1 is from breast height), used for most species
        // Also controls SDI max. See FRL Research Contribution 40 (Hann et al. 2003).
        public float SiteIndexInFeet { get; private set; }

        public float RedAlderSiteIndexInfeet { get; private set; }
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
            this.BreastHeightAgeInYears = ageInYears;
            this.DouglasFirSiteConstants = new DouglasFir.SiteConstants(primarySiteIndexInFeet);
            this.DouglasFirSiteConstants = new(primarySiteIndexInFeet);
            this.HemlockSiteIndexInFeet = variant.ToHemlockSiteIndex(primarySiteIndexInFeet);
            this.NumberOfPlots = 1;
            this.PlantingDensityInTreesPerHectare = 0.0F;
            this.RedAlderSiteIndexInfeet = -1.0F;
            this.RedAlderGrowthEffectiveAge = -1.0F;
            this.SiteIndexInFeet = primarySiteIndexInFeet;
            this.TreeHeightWarningBySpecies = new();
            this.Warnings = new OrganonWarnings();
        }

        public OrganonStand(OrganonStand other)
            : base(other)
        {
            this.AgeInYears = other.AgeInYears;
            this.BreastHeightAgeInYears = other.BreastHeightAgeInYears;
            this.DouglasFirSiteConstants = other.DouglasFirSiteConstants; // currently immutable, so shallow copy for now
            this.HemlockSiteIndexInFeet = other.HemlockSiteIndexInFeet;
            this.Name = other.Name;
            this.NumberOfPlots = other.NumberOfPlots;
            this.RedAlderSiteIndexInfeet = other.RedAlderSiteIndexInfeet;
            this.RedAlderGrowthEffectiveAge = other.RedAlderGrowthEffectiveAge;
            //this.SdiMaxLnQmd = other.SdiMaxLnQmd;
            //this.SdiMaxReciprocalExponent = other.SdiMaxReciprocalExponent;
            this.SiteIndexInFeet = other.SiteIndexInFeet;
            this.TreeHeightWarningBySpecies = new();
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

            this.RedAlderSiteIndexInfeet = RedAlder.ConiferToRedAlderSiteIndex(this.SiteIndexInFeet);
            this.RedAlderGrowthEffectiveAge = RedAlder.GetGrowthEffectiveAge(heightOfTallestRedAlderInFeet, this.RedAlderSiteIndexInfeet);
            if (this.RedAlderGrowthEffectiveAge <= 0.0F)
            {
                this.RedAlderGrowthEffectiveAge = Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears;
                this.RedAlderSiteIndexInfeet = RedAlder.GetSiteIndex(heightOfTallestRedAlderInFeet, this.RedAlderGrowthEffectiveAge);
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