namespace Osu.Cof.Organon
{
    public class OrganonConfiguration
    {
        public OrganonVariant Variant { get; private set; }

        // switches to if() clause in Mortality.MORTAL()
        public bool AdditionalMortality { get; set; }
        // enable per species crown ratio growth multiplier used only for NWO
        public bool CalibrateCrownRatio { get; set; }
        // enable per species diameter growth multiplier for minor species
        public bool CalibrateDiameter { get; set; }
        // enable per species height multiplier
        public bool CalibrateHeight { get; set; }
        // hint for error checking and triggers FCYCLE count
        public bool Fertilizer { get; set; }
        // enables genetic growth modifiers
        public bool Genetics { get; set; }
        // hint for error checking age ranges
        public bool IsEvenAge { get; set; }
        // enables Swiss needle cast (Nothophaeocryptopus gaeumanii) growth modifiers, applies only to NWO and SMC variants
        public bool SwissNeedleCast { get; set; }
        // hint for error checking and triggers TCYCLE count
        public bool Thin { get; set; }

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

        public OrganonConfiguration(OrganonVariant variant)
        {
            this.Variant = variant;
            if (this.Variant.Variant == Organon.Variant.Rap)
            {
                // only even age red alder plantations more than 10 years old are supported
                this.IsEvenAge = true;
            }
        }
    }
}
