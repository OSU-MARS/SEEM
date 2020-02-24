using System;

namespace Osu.Cof.Organon
{
    public class Stand : Trees
    {
        // STOR[2]
        public float A1 { get; private set; }
        // STOR[4] SDImax cap for mortality, overrides A1 if lower
        public float A1MAX { get; set; }
        // STOR[3] exponent for slope of SDImax line
        public float A2 { get; private set; }

        // time since last stand replacing disturbance
        public int AgeInYears { get; set; }

        // time since oldest cohort of trees in the stand reached breast height (4.5 feet) (DOUG?)
        public int BreastHeightAgeInYears { get; set; }

        // also used for ponderosa (SWO) and western redcedar (NWO)
        public float HemlockSiteIndex { get; private set; }

        // STOR[0]
        public float NO { get; set; }

        // number of plots tree data is from
        // If data is for entire stand use one plot.
        public float NumberOfPlots { get; set; }

        // RVARS[0] site index from ground height in feet (internal variable SI_1 is from breast height), used for most species
        public float SiteIndex { get; private set; }

        public float RedAlderSiteIndex { get; private set; }

        // STOR[1]
        public float RD0 { get; set; }

        public OrganonWarnings Warnings { get; private set; }

        public bool[] TreeHeightWarning { get; private set; }

        public Stand(int ageInYears, int treeRecordCount, float primarySiteIndex)
            : base(treeRecordCount)
        {
            this.AgeInYears = ageInYears;
            this.BreastHeightAgeInYears = ageInYears;
            this.HemlockSiteIndex = -1.0F;
            this.NumberOfPlots = 1;
            this.SiteIndex = primarySiteIndex;
            this.RedAlderSiteIndex = -1.0F;
            this.TreeHeightWarning = new bool[treeRecordCount];
            this.Warnings = new OrganonWarnings();
        }

        public Stand(Stand other)
            : this(other.AgeInYears, other.TreeRecordCount, other.SiteIndex)
        {
            this.BreastHeightAgeInYears = other.BreastHeightAgeInYears;

            other.CrownRatio.CopyTo(this.CrownRatio, 0);
            other.Dbh.CopyTo(this.Dbh, 0);
            other.DbhGrowth.CopyTo(this.DbhGrowth, 0);
            other.DeadExpansionFactor.CopyTo(this.DeadExpansionFactor, 0);
            other.LiveExpansionFactor.CopyTo(this.LiveExpansionFactor, 0);
            other.Height.CopyTo(this.Height, 0);
            other.HeightGrowth.CopyTo(this.HeightGrowth, 0);
            this.HemlockSiteIndex = other.HemlockSiteIndex;
            this.NumberOfPlots = other.NumberOfPlots;
            this.RedAlderSiteIndex = other.RedAlderSiteIndex;
            other.Species.CopyTo(this.Species, 0);
            other.TreeHeightWarning.CopyTo(this.TreeHeightWarning, 0);
            this.Warnings = new OrganonWarnings(other.Warnings);
        }

        public Stand Clone()
        {
            return new Stand(this);
        }

        public void CopyTreeGrowth(Trees trees)
        {
            if (trees.TreeRecordCount != this.TreeRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(trees));
            }

            Array.Copy(trees.CrownRatio, 0, this.CrownRatio, 0, this.CrownRatio.Length);
            Array.Copy(trees.Dbh, 0, this.Dbh, 0, this.Dbh.Length);
            Array.Copy(trees.DbhGrowth, 0, this.DbhGrowth, 0, this.DbhGrowth.Length);
            Array.Copy(trees.DeadExpansionFactor, 0, this.DeadExpansionFactor, 0, this.DeadExpansionFactor.Length);
            Array.Copy(trees.Height, 0, this.Height, 0, this.Height.Length);
            Array.Copy(trees.HeightGrowth, 0, this.HeightGrowth, 0, this.HeightGrowth.Length);
            Array.Copy(trees.LiveExpansionFactor, 0, this.LiveExpansionFactor, 0, this.LiveExpansionFactor.Length);
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
                        //   this.MortalitySiteIndex = 4.776377F * (float)Math.Pow(this.PrimarySiteIndex, 0.763530587);
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

        public float SetRedAlderSiteIndex()
        {
            // find red alder site index and growth effective age
            // In CIPSR 2.2.4 these paths are disabled for SMC red alder even though it's a supported species, resulting in zero
            // height growth. In this fork the code's called regardless of variant.
            float heightOfTallestRedAlderInFeet = 0.0F;
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                if (this.Species[treeIndex] == FiaCode.AlnusRubra)
                {
                    float alderHeightInFeet = this.Height[treeIndex];
                    if (alderHeightInFeet > heightOfTallestRedAlderInFeet)
                    {
                        heightOfTallestRedAlderInFeet = alderHeightInFeet;
                    }
                }
            }

            this.RedAlderSiteIndex = RedAlder.ConiferToRedAlderSiteIndex(this.SiteIndex);
            float redAlderAge = RedAlder.GetGrowthEffectiveAge(heightOfTallestRedAlderInFeet, this.RedAlderSiteIndex);
            if (redAlderAge <= 0.0F)
            {
                redAlderAge = 55.0F;
                this.RedAlderSiteIndex = RedAlder.GetSiteIndex(heightOfTallestRedAlderInFeet, redAlderAge);
            }
            else if (redAlderAge > 55.0F)
            {
                redAlderAge = 55.0F;
            }

            return redAlderAge;
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
                TEMPA1 = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.DefaultMaximumSdi));
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
            for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
            {
                float basalArea = this.GetBasalArea(treeIndex);
                totalBasalArea += basalArea;

                switch (this.Species[treeIndex])
                {
                    case FiaCode.AbiesAmabalis:
                    case FiaCode.AbiesConcolor:
                    case FiaCode.AbiesGrandis:
                    case FiaCode.AbiesProcera:
                        trueFirBasalArea += basalArea;
                        break;
                    case FiaCode.PinusPonderosa:
                        ponderosaBasalArea += basalArea;
                        break;
                    case FiaCode.PseudotsugaMenziesii:
                        douglasFirBasalArea += basalArea;
                        break;
                    case FiaCode.TsugaHeterophylla:
                        hemlockBasalArea += basalArea;
                        break;
                }
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
                        trueFirModifier = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.TrueFirMaximumSdi)) / TEMPA1;
                    }
                    float hemlockModifier = 0.9943501F;
                    if (configuration.HemlockMaximumSdi > 0.0F)
                    {
                        hemlockModifier = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.HemlockMaximumSdi)) / TEMPA1;
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
                        trueFirModifier = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.TrueFirMaximumSdi)) / TEMPA1;
                    }
                    // Based on Johnson's (2000) analysis of Max. SDI for western hemlock
                    hemlockModifier = 1.014293245F;
                    if (configuration.HemlockMaximumSdi > 0.0F)
                    {
                        hemlockModifier = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.HemlockMaximumSdi)) / TEMPA1;
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