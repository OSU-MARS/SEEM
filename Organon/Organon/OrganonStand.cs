using Osu.Cof.Ferm.Species;
using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonStand : Stand
    {
        public float A1 { get; private set; }
        // STOR[3] exponent for slope of SDImax line
        public float A2 { get; private set; }

        // time since last stand replacing disturbance
        public int AgeInYears { get; set; }

        // time since oldest cohort of trees in the stand reached breast height (4.5 feet) (DOUG?)
        public int BreastHeightAgeInYears { get; set; }

        public DouglasFir.SiteConstants DouglasFirSiteConstants { get; private set; }

        // also used for ponderosa (SWO) and western redcedar (NWO)
        public float HemlockSiteIndex { get; private set; }

        // number of plots tree data is from
        // If data is for entire stand use one plot.
        public float NumberOfPlots { get; set; }

        // site index from ground height in feet (internal variable SI_1 is from breast height), used for most species
        public float SiteIndex { get; private set; }

        public float RedAlderSiteIndex { get; private set; }
        public float RedAlderGrowthEffectiveAge { get; set; }

        public OrganonWarnings Warnings { get; private set; }

        public SortedDictionary<FiaCode, bool[]> TreeHeightWarningBySpecies { get; private set; }

        public OrganonStand(int ageInYears, float primarySiteIndex)
        {
            this.AgeInYears = ageInYears;
            this.BreastHeightAgeInYears = ageInYears;
            this.DouglasFirSiteConstants = new DouglasFir.SiteConstants(primarySiteIndex);
            this.HemlockSiteIndex = -1.0F;
            this.NumberOfPlots = 1;
            this.SiteIndex = primarySiteIndex;
            this.RedAlderSiteIndex = -1.0F;
            this.RedAlderGrowthEffectiveAge = -1.0F;
            this.TreeHeightWarningBySpecies = new SortedDictionary<FiaCode, bool[]>();
            this.Warnings = new OrganonWarnings();
        }

        public OrganonStand(OrganonStand other)
            : this(other.AgeInYears, other.SiteIndex)
        {
            this.BreastHeightAgeInYears = other.BreastHeightAgeInYears;
            this.HemlockSiteIndex = other.HemlockSiteIndex;
            this.Name = other.Name;
            this.NumberOfPlots = other.NumberOfPlots;
            this.RedAlderSiteIndex = other.RedAlderSiteIndex;
            this.Warnings = new OrganonWarnings(other.Warnings);

            foreach (KeyValuePair<FiaCode, bool[]> species in other.TreeHeightWarningBySpecies)
            {
                bool[] heightWarnings = new bool[species.Value.Length];
                species.Value.CopyTo(heightWarnings, 0);
                this.TreeHeightWarningBySpecies.Add(species.Key, heightWarnings);
            }
            foreach (KeyValuePair<FiaCode, Trees> species in other.TreesBySpecies)
            {
                this.TreesBySpecies.Add(species.Key, new Trees(species.Value));
            }
        }

        public void CopyTreeGrowthFrom(OrganonStand other)
        {
            foreach (Trees otherTreesOfSpecies in other.TreesBySpecies.Values)
            {
                Trees thisTreesOfSpecies = this.TreesBySpecies[otherTreesOfSpecies.Species];
                if (otherTreesOfSpecies.Count > thisTreesOfSpecies.Capacity)
                {
                    throw new ArgumentOutOfRangeException(nameof(other));
                }

                Array.Copy(otherTreesOfSpecies.CrownRatio, 0, thisTreesOfSpecies.CrownRatio, 0, otherTreesOfSpecies.Count);
                Array.Copy(otherTreesOfSpecies.Dbh, 0, thisTreesOfSpecies.Dbh, 0, otherTreesOfSpecies.Count);
                Array.Copy(otherTreesOfSpecies.DbhGrowth, 0, thisTreesOfSpecies.DbhGrowth, 0, otherTreesOfSpecies.Count);
                Array.Copy(otherTreesOfSpecies.DeadExpansionFactor, 0, thisTreesOfSpecies.DeadExpansionFactor, 0, otherTreesOfSpecies.Count);
                Array.Copy(otherTreesOfSpecies.Height, 0, thisTreesOfSpecies.Height, 0, otherTreesOfSpecies.Count);
                Array.Copy(otherTreesOfSpecies.HeightGrowth, 0, thisTreesOfSpecies.HeightGrowth, 0, otherTreesOfSpecies.Count);
                Array.Copy(otherTreesOfSpecies.LiveExpansionFactor, 0, thisTreesOfSpecies.LiveExpansionFactor, 0, otherTreesOfSpecies.Count);
            }
        }

        public int GetTreeRecordCount()
        {
            int treeRecords = 0;
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                treeRecords += treesOfSpecies.Count;
            }
            return treeRecords;
        }

        public void SetDefaultAndMortalitySiteIndices(TreeModel treeModel)
        {
            switch (treeModel)
            {
                case TreeModel.OrganonNwo:
                case TreeModel.OrganonSmc:
                    // Site index equation from Nigh(1995, Forest Science 41:84-98)
                    if ((this.SiteIndex < 0.0F) && (this.HemlockSiteIndex > 0.0F))
                    {
                        this.SiteIndex = 0.480F + (1.110F * this.HemlockSiteIndex);
                    }
                    else if (this.HemlockSiteIndex < 0.0F)
                    {
                        this.HemlockSiteIndex = -0.432F + (0.899F * this.SiteIndex);
                    }
                    break;
                case TreeModel.OrganonRap:
                    if (this.HemlockSiteIndex < 0.0F)
                    {
                        // Fortran code sets SITE_2 from an uninitialized value of SI_1. It's unclear what the Fortran equation was intended
                        // to accomplish as using SITE_1, which is initialized translates to
                        //   this.MortalitySiteIndex = 4.776377F * MathF.Pow(this.PrimarySiteIndex, 0.763530587);
                        // which produces mortality site indices outside of the range supported for RAP.
                        // BUGBUG: clamp range to maximum and minimum once these constants are available from variant capabilities
                        this.HemlockSiteIndex = this.SiteIndex;
                    }
                    break;
                case TreeModel.OrganonSwo:
                    if ((this.SiteIndex < 0.0F) && (this.HemlockSiteIndex > 0.0F))
                    {
                        this.SiteIndex = 1.062934F * this.HemlockSiteIndex;
                    }
                    else if (this.HemlockSiteIndex < 0.0F)
                    {
                        this.HemlockSiteIndex = 0.940792F * this.SiteIndex;
                    }
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledModelException(treeModel);
            }
        }

        public void SetRedAlderSiteIndexAndGrowthEffectiveAge()
        {
            // find red alder site index and growth effective age
            // In CIPSR 2.2.4 these paths are disabled for SMC red alder even though it's a supported species, resulting in zero
            // height growth. In this fork the code's called regardless of variant.
            float heightOfTallestRedAlderInFeet = 0.0F;
            if (this.TreesBySpecies.TryGetValue(FiaCode.AlnusRubra, out Trees redAlders))
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

            this.RedAlderSiteIndex = RedAlder.ConiferToRedAlderSiteIndex(this.SiteIndex);
            this.RedAlderGrowthEffectiveAge = RedAlder.GetGrowthEffectiveAge(heightOfTallestRedAlderInFeet, this.RedAlderSiteIndex);
            if (this.RedAlderGrowthEffectiveAge <= 0.0F)
            {
                this.RedAlderGrowthEffectiveAge = Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears;
                this.RedAlderSiteIndex = RedAlder.GetSiteIndex(heightOfTallestRedAlderInFeet, this.RedAlderGrowthEffectiveAge);
            }
            else if (this.RedAlderGrowthEffectiveAge > Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears)
            {
                this.RedAlderGrowthEffectiveAge = Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears;
            }
        }

        /// <summary>
        /// Finds SDImax line. Sets A1 (constant of SDImax line) and A2 (exponent of SDImax line, dimensionless).
        /// </summary>
        /// <param name="configuration">Organon configuration.</param>
        public void SetSdiMax(OrganonConfiguration configuration)
        {
            // CALCULATE THE MAXIMUM SIZE-DENISTY LINE
            switch (configuration.Variant.TreeModel)
            {
                case TreeModel.OrganonSwo:
                case TreeModel.OrganonNwo:
                case TreeModel.OrganonSmc:
                    // REINEKE (1933): 1.605^-1 = 0.623053
                    this.A2 = 0.62305F;
                    break;
                case TreeModel.OrganonRap:
                    // PUETTMANN ET AL. (1993)
                    this.A2 = 0.64F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel);
            }

            float TEMPA1;
            if (configuration.DefaultMaximumSdi > 0.0F)
            {
                TEMPA1 = Constant.NaturalLogOf10 + this.A2 * MathV.Ln(configuration.DefaultMaximumSdi);
            }
            else
            {
                TEMPA1 = configuration.Variant.TreeModel switch
                {
                    TreeModel.OrganonSwo => 6.21113F,
                    TreeModel.OrganonNwo => 6.19958F,
                    TreeModel.OrganonSmc => 6.19958F,
                    TreeModel.OrganonRap => 5.96F,
                    _ => throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel),
                };
            }

            // BUGBUG need API with maximum species group ID to safely allocate BAGRP
            float douglasFirBasalArea = 0.0F;
            float hemlockBasalArea = 0.0F;
            float ponderosaBasalArea = 0.0F;
            float totalBasalArea = 0.0F;
            float trueFirBasalArea = 0.0F;
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                float speciesBasalArea = 0.0F;
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float basalArea = treesOfSpecies.GetBasalArea(treeIndex);
                    speciesBasalArea += speciesBasalArea;
                }

                switch (treesOfSpecies.Species)
                {
                    case FiaCode.AbiesAmabalis:
                    case FiaCode.AbiesConcolor:
                    case FiaCode.AbiesGrandis:
                    case FiaCode.AbiesProcera:
                        trueFirBasalArea += speciesBasalArea;
                        break;
                    case FiaCode.PinusPonderosa:
                        ponderosaBasalArea += speciesBasalArea;
                        break;
                    case FiaCode.PseudotsugaMenziesii:
                        douglasFirBasalArea += speciesBasalArea;
                        break;
                    case FiaCode.TsugaHeterophylla:
                        hemlockBasalArea += speciesBasalArea;
                        break;
                }
                totalBasalArea += speciesBasalArea;
            }

            float douglasFirProportion = 0.0F;
            float hemlockProportion = 0.0F;
            float ponderosaProportion = 0.0F;
            float trueFirProportion = 0.0F;
            if (totalBasalArea > 0.0F)
            {
                douglasFirProportion /= totalBasalArea;
                hemlockProportion /= totalBasalArea;
                ponderosaProportion /= totalBasalArea;
                trueFirProportion /= totalBasalArea;
            }

            float A1MOD;
            switch (configuration.Variant.TreeModel)
            {
                case TreeModel.OrganonSwo:
                    float trueFirModifier = 1.03481817F;
                    if (configuration.TrueFirMaximumSdi > 0.0F)
                    {
                        trueFirModifier = Constant.NaturalLogOf10 + this.A2 * MathV.Ln(configuration.TrueFirMaximumSdi) / TEMPA1;
                    }
                    float hemlockModifier = 0.9943501F;
                    if (configuration.HemlockMaximumSdi > 0.0F)
                    {
                        hemlockModifier = Constant.NaturalLogOf10 + this.A2 * MathV.Ln(configuration.HemlockMaximumSdi) / TEMPA1;
                    }

                    if (douglasFirProportion >= 0.5F)
                    {
                        A1MOD = 1.0F;
                    }
                    else if (trueFirProportion >= 0.6666667F)
                    {
                        A1MOD = trueFirModifier;
                    }
                    else if (ponderosaProportion >= 0.6666667F)
                    {
                        A1MOD = hemlockModifier;
                    }
                    else
                    {
                        A1MOD = douglasFirProportion + trueFirModifier * trueFirProportion + hemlockModifier * ponderosaProportion;
                    }
                    break;
                case TreeModel.OrganonNwo:
                case TreeModel.OrganonSmc:
                    trueFirModifier = 1.03481817F;
                    if (configuration.TrueFirMaximumSdi > 0.0F)
                    {
                        trueFirModifier = Constant.NaturalLogOf10 + this.A2 * MathV.Ln(configuration.TrueFirMaximumSdi) / TEMPA1;
                    }
                    // Based on Johnson's (2000) analysis of Max. SDI for western hemlock
                    hemlockModifier = 1.014293245F;
                    if (configuration.HemlockMaximumSdi > 0.0F)
                    {
                        hemlockModifier = Constant.NaturalLogOf10 + this.A2 * MathV.Ln(configuration.HemlockMaximumSdi) / TEMPA1;
                    }

                    if (douglasFirProportion >= 0.5F)
                    {
                        A1MOD = 1.0F;
                    }
                    else if (hemlockProportion >= 0.5F)
                    {
                        A1MOD = hemlockModifier;
                    }
                    else if (trueFirProportion >= 0.6666667)
                    {
                        A1MOD = trueFirModifier;
                    }
                    else
                    {
                        A1MOD = douglasFirProportion + hemlockModifier * hemlockProportion + trueFirModifier * trueFirProportion;
                    }
                    break;
                case TreeModel.OrganonRap:
                    A1MOD = 1.0F;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledModelException(configuration.Variant.TreeModel);
            }
            if (A1MOD <= 0.0F)
            {
                // BUGBUG: silently ignores error condition
                //Debug.Assert(A1MOD > 0.0F);
                A1MOD = 1.0F;
            }

            this.A1 = TEMPA1 * A1MOD;
        }
    }
}