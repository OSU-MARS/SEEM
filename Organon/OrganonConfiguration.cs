namespace Osu.Cof.Organon
{
    public class OrganonConfiguration
    {
        public Variant Variant { get; private set; }

        // INDS[0] switches CALIB[, 0] from 1.0 to ACALIB[, 0]
        public bool CALH { get; set; }
        // INDS[1] switches CALIB[, 1] from 1.0 to ACALIB[, 1]
        public bool CALC { get; set; }
        // INDS[2] switches CALIB[, 2] from 1.0 to ACALIB[, 2]
        public bool CALD { get; set; }
        // INDS[3] hint for error checking age ranges
        public bool IsEvenAge { get; set; }
        // INDS[5] switches on use of SCR
        public bool Prune { get; set; }
        // INDS[6] hint for error checking and triggers TCYCLE count
        public bool Thin { get; set; }
        // INDS[7] hint for error checking and triggers FCYCLE count
        public bool Fertilizer { get; set; }
        // INDS[8] switches to if() clause in Mortality.MORTAL()
        public bool AdditionalMortality { get; set; }
        // INDS[13] enables genetic growth modifiers
        public bool Genetics { get; set; }
        // INDS[14] enables Swiss needle cast (Nothophaeocryptopus gaeumanii) growth modifiers, applies only to NWO and SMC variants
        public bool SwissNeedleCast { get; set; }

        // RVARS[0] site index from ground height in feet (internal variable SI_1 is from breast height), used for most species
        public float SITE_1 { get; set; }
        // RVARS[1] site index from ground height in feet (internal variable SI_2 is from breast height), used for ??? in 
        public float SITE_2 { get; set; }
        // RVARS[2] maximum SDI for Douglas-fir?, only used by Submax()
        // If less than or equal to zero. (DOUG? also, what species?)</param>
        public float MSDI_1 { get; set; }
        // RVARS[3] maximum SDI for true firs?, only used by Submax() for calculation of TFMOD
        // Maximum? stand density index for calculating TFMOD, ignored if less than or equal to zero. (DOUG?)
        public float MSDI_2 { get; set; }
        // RVARS[4] maximum SDI for OC?, only used by Submax() for calculation of OCMOD
        // Maximum? stand density index for calculating OCMOD, ignored if less than or equal to zero. (DOUG?)
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

        public OrganonConfiguration(Variant variant)
        {
            this.Variant = variant;
            if (this.Variant == Variant.Rap)
            {
                // only even age red alder plantations are supported
                this.IsEvenAge = true;
            }
        }
    }
}
