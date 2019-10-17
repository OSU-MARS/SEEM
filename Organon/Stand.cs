using System;

namespace Osu.Cof.Organon
{
    public class Stand
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

        // accumulated expansion factor of dead trees from mortality chipping? (DOUG?)
        public float[] DeadExpansionFactor { get; private set; }

        // TDATAR[, 5] is variously assigned in rundll and wood quality to DBH, CR, and EF but is never consumed
        //   this suggests it's an output trace
        //     0    1       2            3                 4                     5  6              7
        //     DBH, height, crown ratio, expansion factor, crown ratio? (DOUG?), ?, crown ratio 2, expansion factor 2
        //                                                 expansion factor?
        // { { DBH, HT,     CR,          EF,               unused?,              ?, CR t + 1?,     CR t + 1? } }
        // TODO: can this be reduced to width 4 rather than 8?
        public float[,] Float { get; private set; }

        //     0,             1                2,                         3
        //     height growth, diameter growth, accumulated height growth, accumulated diameter growth 
        // { { HGRO,          DGRO,            GROWTH + GROWTH,           GROWTH + GROWTH } }
        public float[,] Growth { get; private set; }

        // valid range for species group is [0, 17]
        //     0        1              2
        //     species, species group, user data passthrough, not used by Organon
        // { { ISP,     ISPGRP,        USER } }
        public int[,] Integer { get; private set; }

        // IB, sometimes also named IIB
        public int MaxBigSixSpeciesGroupIndex { get; private set; }

        // must be greater than zero when thinning is enabled? (DOUG?)
        public float[] MGExpansionFactor { get; private set; }

        // STOR[0]
        public float NO { get; set; }

        // number of plots? (DOUG?)
        public int NPTS { get; private set; }

        // RVARS[0] site index from ground height in feet (internal variable SI_1 is from breast height), used for most species
        public float PrimarySiteIndex { get; private set; }

        // RVARS[1] site index from ground height in feet (internal variable SI_2 is from breast height), used for ??? in 
        public float MortalitySiteIndex { get; private set; }

        // STOR[1]
        public float RD0 { get; set; }

        public StandWarnings Warnings { get; private set; }

        public bool[] TreeHeightWarning { get; private set; }

        public int TreeRecordsInUse { get; set; }

        protected Stand(Stand other)
            : this(other.AgeInYears, other.MaximumTreeRecords, other.PrimarySiteIndex, other.MaxBigSixSpeciesGroupIndex)
        {
            this.BreastHeightAgeInYears = other.BreastHeightAgeInYears;

            other.DeadExpansionFactor.CopyTo(this.DeadExpansionFactor, 0);
            Buffer.BlockCopy(other.Float, 0, this.Float, 0, sizeof(float) * other.Float.Length);
            Buffer.BlockCopy(other.Growth, 0, this.Growth, 0, sizeof(float) * other.Growth.Length);
            Buffer.BlockCopy(other.Integer, 0, this.Integer, 0, sizeof(int) * other.Integer.Length);
            other.MGExpansionFactor.CopyTo(this.MGExpansionFactor, 0);
            this.MortalitySiteIndex = other.MortalitySiteIndex;
            this.NPTS = other.NPTS;
            this.TreeRecordsInUse = other.TreeRecordsInUse;
            other.TreeHeightWarning.CopyTo(this.TreeHeightWarning, 0);
            this.Warnings = new StandWarnings(other.Warnings);
        }

        protected Stand(int ageInYears, int treeCount, float primarySiteIndex, int maxBigSixSpeciesGroupIndex)
        {
            this.AgeInYears = ageInYears;
            this.BreastHeightAgeInYears = ageInYears;
            this.DeadExpansionFactor = new float[treeCount];
            this.Float = new float[treeCount, 8];
            this.Growth = new float[treeCount, 4];
            this.Integer = new int[treeCount, 3];
            this.MaxBigSixSpeciesGroupIndex = maxBigSixSpeciesGroupIndex;
            this.MGExpansionFactor = new float[treeCount];
            this.NPTS = 1;
            this.PrimarySiteIndex = primarySiteIndex;
            this.MortalitySiteIndex = -1.0F;
            this.TreeHeightWarning = new bool[treeCount];
            this.TreeRecordsInUse = 0;
            this.Warnings = new StandWarnings();
        }

        public int MaximumTreeRecords
        {
            get { return this.MGExpansionFactor.Length; }
        }

        public bool IsBigSixSpecies(int treeIndex)
        {
            return this.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup] <= this.MaxBigSixSpeciesGroupIndex;
        }

        /// <summary>
        /// Finds power of SDImax line. Sets A1 (constant of SDImax line) and A2 (exponent of SDImax line, dimensionless).
        /// </summary>
        /// <param name="configuration">Organon configuration.</param>
        /// <param name="stand">Stand data.</param>
        public void SUBMAX(OrganonConfiguration configuration)
        {
            // CALCULATE THE MAXIMUM SIZE-DENISTY LINE
            switch (configuration.Variant)
            {
                case Variant.Swo:
                case Variant.Nwo:
                case Variant.Smc:
                    // REINEKE (1933): 1.605^-1 = 0.623053
                    this.A2 = 0.62305F;
                    break;
                case Variant.Rap:
                    // PUETTMANN ET AL. (1993)
                    this.A2 = 0.64F;
                    break;
                default:
                    throw new NotSupportedException();
            }

            float KB = 0.005454154F;
            float TEMPA1;
            if (configuration.MSDI_1 > 0.0F)
            {
                TEMPA1 = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.MSDI_1));
            }
            else
            {
                switch (configuration.Variant)
                {
                    case Variant.Swo:
                        // ORIGINAL SWO-ORGANON - Max.SDI = 530.2
                        TEMPA1 = 6.21113F;
                        break;
                    case Variant.Nwo:
                        // ORIGINAL WWV-ORGANON - Max.SDI = 520.5
                        TEMPA1 = 6.19958F;
                        break;
                    case Variant.Smc:
                        // ORIGINAL WWV-ORGANON
                        TEMPA1 = 6.19958F;
                        break;
                    case Variant.Rap:
                        // PUETTMANN ET AL. (1993)
                        TEMPA1 = 5.96F;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            // BUGBUG need API with maximum species group ID to safely allocate BAGRP
            float[] BAGRP = new float[18];
            for (int treeIndex = 0; treeIndex < this.TreeRecordsInUse; ++treeIndex)
            {
                int ISPGRP = this.Integer[treeIndex, 1];
                float DBH = this.Float[treeIndex, 0];
                float EX1 = this.Float[treeIndex, 3];
                BAGRP[ISPGRP] = BAGRP[ISPGRP] + KB * DBH * DBH * EX1;
            }

            float TOTBA = 0.0F;
            for (int I = 0; I < 3; ++I)
            {
                TOTBA += BAGRP[I];
            }

            float PDF;
            float PTF = 0.0F; // BUGBUG not intialized in Fortran code
            if (TOTBA > 0.0F)
            {
                if (configuration.Variant <= Variant.Smc)
                {
                    PDF = BAGRP[0] / TOTBA;
                    PTF = BAGRP[1] / TOTBA;
                }
                else
                {
                    // (DOUG? typo for PTF?)
                    // PRA = BAGRP[0] / TOTBA;
                    PDF = BAGRP[1] / TOTBA;
                }
            }
            else
            {
                if (configuration.Variant <= Variant.Smc)
                {
                    PDF = 0.0F;
                    PTF = 0.0F;
                }
                else
                {
                    // (DOUG? typo for PTF?)
                    // PRA = 0.0F;
                    PDF = 0.0F;
                }
            }

            float A1MOD;
            float OCMOD;
            float PPP;
            float TFMOD;
            switch (configuration.Variant)
            {
                case Variant.Swo:
                    if (configuration.MSDI_2 > 0.0F)
                    {
                        TFMOD = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.MSDI_2)) / TEMPA1;
                    }
                    else
                    {
                        TFMOD = 1.03481817F;
                    }
                    if (configuration.MSDI_3 > 0.0F)
                    {
                        OCMOD = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.MSDI_3)) / TEMPA1;
                    }
                    else
                    {
                        OCMOD = 0.9943501F;
                    }
                    if (TOTBA > 0.0F)
                    {
                        PPP = BAGRP[2] / TOTBA;
                    }
                    else
                    {
                        PPP = 0.0F;
                    }

                    if (PDF >= 0.5F)
                    {
                        A1MOD = 1.0F;
                    }
                    else if (PTF >= 0.6666667F)
                    {
                        A1MOD = TFMOD;
                    }
                    else if (PPP >= 0.6666667F)
                    {
                        A1MOD = OCMOD;
                    }
                    else
                    {
                        A1MOD = PDF + TFMOD * PTF + OCMOD * PPP;
                    }
                    break;
                case Variant.Nwo:
                case Variant.Smc:
                    if (configuration.MSDI_2 > 0.0F)
                    {
                        TFMOD = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.MSDI_2)) / TEMPA1;
                    }
                    else
                    {
                        TFMOD = 1.03481817F;
                    }
                    if (configuration.MSDI_3 > 0.0F)
                    {
                        OCMOD = (float)(Math.Log(10.0) + this.A2 * Math.Log(configuration.MSDI_3)) / TEMPA1;
                    }
                    else
                    {
                        // Based on Johnson's (2000) analysis of Max. SDI for western hemlock
                        OCMOD = 1.014293245F;
                    }

                    float PWH;
                    if (TOTBA > 0.0F)
                    {
                        PWH = BAGRP[2] / TOTBA;
                    }
                    else
                    {
                        PWH = 0.0F;
                    }
                    if (PDF >= 0.5F)
                    {
                        A1MOD = 1.0F;
                    }
                    else if (PWH >= 0.5F)
                    {
                        A1MOD = OCMOD;
                    }
                    else if (PTF >= 0.6666667)
                    {
                        A1MOD = TFMOD;
                    }
                    else
                    {
                        A1MOD = PDF + OCMOD * PWH + TFMOD * PTF;
                    }
                    break;
                case Variant.Rap:
                    A1MOD = 1.0F;
                    break;
                default:
                    throw new NotSupportedException();
            }
            if (A1MOD <= 0.0F)
            {
                A1MOD = 1.0F;
            }

            this.A1 = TEMPA1 * A1MOD;
        }

        public void SetDefaultSiteIndices(Variant variant)
        {
            switch (variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    // Site index equation from Nigh(1995, Forest Science 41:84-98)
                    if ((this.PrimarySiteIndex < 0.0F) && (this.MortalitySiteIndex > 0.0F))
                    {
                        this.PrimarySiteIndex = 0.480F + (1.110F * this.MortalitySiteIndex);
                    }
                    else if (this.MortalitySiteIndex < 0.0F)
                    {
                        this.MortalitySiteIndex = -0.432F + (0.899F * this.PrimarySiteIndex);
                    }
                    break;
                case Variant.Rap:
                    if (this.MortalitySiteIndex < 0.0F)
                    {
                        // Fortran code sets SITE_2 from an uninitialized value of SI_1. It's unclear what the Fortran equation was intended
                        // to accomplish as using SITE_1, which is initialized translates to
                        //   this.MortalitySiteIndex = 4.776377F * (float)Math.Pow(this.PrimarySiteIndex, 0.763530587);
                        // which produces mortality site indices outside of the range supported for RAP.
                        // BUGBUG: clamp range to maximum and minimum once these constants are available from variant capabilities
                        this.MortalitySiteIndex = this.PrimarySiteIndex;
                    }
                    break;
                case Variant.Swo:
                    if ((this.PrimarySiteIndex < 0.0F) && (this.MortalitySiteIndex > 0.0F))
                    {
                        this.PrimarySiteIndex = 1.062934F * this.MortalitySiteIndex;
                    }
                    else if (this.MortalitySiteIndex < 0.0F)
                    {
                        this.MortalitySiteIndex = 0.940792F * this.PrimarySiteIndex;
                    }
                    break;
                default:
                    throw new NotSupportedException(String.Format("Unhandled Organon variant {0}.", variant));
            }
        }
    }
}