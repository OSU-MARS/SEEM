namespace Mars.Seem.Organon
{
    public class SpeciesCalibration
    {
        public float CrownRatio { get; set; } // only used by NWO and SMC in certain  cases
        public float Diameter { get; set; } // applied uniformly across all variants
        public float Height { get; set; } // only applied to non-big six species (see OrganonVariant.IsBigSixSpecies())

        public SpeciesCalibration()
        {
            this.CrownRatio = 1.0F;
            this.Diameter = 1.0F;
            this.Height = 1.0F;
        }
    }
}
