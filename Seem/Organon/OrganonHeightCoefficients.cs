namespace Mars.Seem.Organon
{
    public class OrganonHeightCoefficients
    {
        // height growth coefficients when treated as a minor species
        public float B0 { get; init; }
        public float B1 { get; init; }
        public float B2 { get; init; }

        // height growth coefficients when treated as a big six species (PSME, ABGR, TSHE, PIPO, PILA, and ALRU2 in RAP)
        public float P0 { get; init; }
        public float P1 { get; init; }
        public float P2 { get; init; }
        public float P3 { get; init; }
        public float P4 { get; init; }
        public float P5 { get; init; }
        public float P6 { get; init; }
        public float P7 { get; init; }
        public float P8 { get; init; }

        public OrganonHeightCoefficients()
        {
            this.B0 = 0.0F;
            this.B1 = 0.0F;
            this.B2 = 0.0F;

            this.P0 = 0.0F;
            this.P1 = 0.0F;
            this.P2 = 0.0F;
            this.P3 = 0.0F;
            this.P4 = 0.0F;
            this.P5 = 0.0F;
            this.P6 = 0.0F;
            this.P7 = 0.0F;
            this.P8 = 0.0F;
        }
    }
}
