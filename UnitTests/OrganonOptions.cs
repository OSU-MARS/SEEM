namespace Osu.Cof.Organon.Test
{
    internal class OrganonOptions
    {
        // INDS[0] switches CALIB[, 0] from 1.0 to ACALIB[, 0]
        public bool CALH { get; set; }
        // INDS[1] switches CALIB[, 1] from 1.0 to ACALIB[, 1]
        public bool CALC { get; set; }
        // INDS[2] switches CALIB[, 2] from 1.0 to ACALIB[, 2]
        public bool CALD { get; set; }
        // INDS[3] hint for error checking age ranges
        public bool IsEvenAge { get; set; }
        // INDS[4] switches on logic in Grow, effect unclear
        public bool Triple { get; set; }
        // INDS[5] switches on use of SCR
        public bool Prune { get; set; }
        // INDS[6] hint for error checking and triggers TCYCLE count
        public bool Thin { get; set; }
        // INDS[7] hint for error checking and triggers FCYCLE count
        public bool Fertilizer { get; set; }
        // INDS[8] switches to if() clause in Mortality.MORTAL()
        public bool AdditionalMortality { get; set; }
        // INDS[9] triggers some data copying in Triple
        public bool WoodQuality { get; set; }
        // INDS[10] unused
        public bool OSTORY { get; set; }
        // INDS[11] unused
        public bool INGRO { get; set; }
        // INDS[12] unused
        public bool B6Thin { get; set; }
        // INDS[13] enables genetic growth modifiers
        public bool Genetics { get; set; }
        // INDS[14] enables Swiss needle cast (Nothophaeocryptopus gaeumanii) growth modifiers, applies only to NWO and SMC variants
        public bool SwissNeedleCast { get; set; }

        // RVARS[0] site index from ground height in feet (internal variable SI_1 is from breast height), used for most species
        public float SITE_1 { get; set; }
        // RVARS[1] site index from ground height in feet (internal variable SI_2 is from breast height), used for ??? in 
        public float SITE_2 { get; set; }
        // RVARS[2] maximum SDI for Douglas-fir?, only used by Submax()
        public float MSDI_1 { get; set; }
        // RVARS[3] maximum SDI for true firs?, only used by Submax() for calculation of TFMOD
        public float MSDI_2 { get; set; }
        // RVARS[4] maximum SDI for OC?, only used by Submax() for calculation of OCMOD
        public float MSDI_3 { get; set; }
        // RVARS[5] genetic diameter growth modifier (requires Genetics = true)
        public float GWDG { get; set; }
        // RVARS[6] genetic height growth modifier (requires Genetics = true)
        public float GWHG { get; set; }
        // RVARS[7] Swiss needle cast coefficient for diameter and height growth modifiers, accepted range is [0.85 - 4.0]
        public float FR { get; set; }
        // RVARS[8] density correction coefficient for red alder height growth (WHHLB_SI_UC) and additional mortality (Mortality = true)
        public float PDEN { get; set; }

        // STOR[0]
        public float NO { get; set; }
        // STOR[1]
        public float RD0 { get; set; }
        // STOR[2]
        public float A1 { get; set; }
        // STOR[3] exponent for slope of SDImax line
        public float A2 { get; set; }
        // STOR[4] SDImax cap for mortality, overrides A1 if lower
        public float A1MAX { get; set; }
        // STOR[5] SDImax cap for mortality, overrides A1MAX if lower
        public float PA1MAX { get; set; }

        public OrganonOptions(Variant variant)
        {
            if (variant == Variant.Rap)
            {
                // only even age red alder plantations are supported
                this.IsEvenAge = true;
            }

            this.SITE_1 = TestConstant.Default.SiteIndex;
            this.SITE_2 = TestConstant.Default.SiteIndex - 10.0F;
            this.MSDI_1 = TestConstant.Default.MaximumReinekeStandDensityIndex;
            this.MSDI_2 = TestConstant.Default.MaximumReinekeStandDensityIndex;
            this.MSDI_3 = TestConstant.Default.MaximumReinekeStandDensityIndex;

            this.A1 = TestConstant.Default.A1;
            this.A2 = TestConstant.Default.A2;
            this.A1MAX = TestConstant.Default.A1MAX;
            this.PA1MAX = TestConstant.Default.PA1MAX;
            this.NO = TestConstant.Default.NO;
            this.RD0 = TestConstant.Default.RD0;
        }

        public int[] ToFeatureFlagsArray()
        {
            int[] featureFlags = new int[15];
            featureFlags[0] = this.CALH ? 1 : 0;
            featureFlags[1] = this.CALC ? 1 : 0;
            featureFlags[2] = this.CALD ? 1 : 0;
            featureFlags[3] = this.IsEvenAge ? 1 : 0;
            featureFlags[4] = this.Triple ? 1 : 0;
            featureFlags[5] = this.Prune ? 1 : 0;
            featureFlags[6] = this.Thin ? 1 : 0;
            featureFlags[7] = this.Fertilizer ? 1 : 0;
            featureFlags[8] = this.AdditionalMortality ? 1 : 0;
            featureFlags[9] = this.WoodQuality ? 1 : 0;
            featureFlags[10] = this.OSTORY ? 1 : 0;
            featureFlags[11] = this.INGRO ? 1 : 0;
            featureFlags[12] = this.B6Thin ? 1 : 0;
            featureFlags[13] = this.Genetics ? 1 : 0;
            featureFlags[14] = this.SwissNeedleCast ? 1 : 0;
            return featureFlags;
        }

        public float[] ToSiteVariablesArray()
        {
            float[] siteVariables = new float[9];
            siteVariables[0] = this.SITE_1;
            siteVariables[1] = this.SITE_2;
            siteVariables[2] = this.MSDI_1;
            siteVariables[3] = this.MSDI_2;
            siteVariables[4] = this.MSDI_3;
            siteVariables[5] = this.GWDG;
            siteVariables[6] = this.GWHG;
            siteVariables[7] = this.FR;
            siteVariables[8] = this.PDEN;
            return siteVariables;
        }

        public float[] ToStandParametersArray()
        {
            float[] standParameters = new float[6];
            standParameters[0] = this.NO;
            standParameters[1] = this.RD0;
            standParameters[2] = this.A1;
            standParameters[3] = this.A2;
            standParameters[4] = this.A1MAX;
            standParameters[5] = this.PA1MAX;
            return standParameters;
        }
    }
}
