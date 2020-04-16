namespace Osu.Cof.Ferm.Organon
{
    public class OrganonTreatments
    {
        // basal area before most recent thinning (Hann RC 40, equation 10)
        // TODO: move to simulation state
        public float BasalAreaBeforeThin { get; private set; }
        // basal area removed by thinning in ft²/ac in the years specified
        public float[] BasalAreaRemovedByThin { get; private set; }

        public int FertilizationCycle { get; set; }

        // simulation periods where fertilization is performed, as indicated by year
        public float[] FertilizationYears { get; private set; }
        // N applied in lb/ac at 
        public float[] PoundsOfNitrogenPerAcre { get; private set; }

        public int ThinningCycle { get; set; }
        // simulation periods in which thinning is performed, as indicated by year
        public float[] ThinningYears { get; private set; }

        public OrganonTreatments()
        {
            this.BasalAreaBeforeThin = 0.0F;
            this.BasalAreaRemovedByThin = new float[5];
            this.FertilizationCycle = 0;
            this.FertilizationYears = new float[5];
            this.PoundsOfNitrogenPerAcre = new float[5];
            this.ThinningCycle = 0;
            this.ThinningYears = new float[5];
        }

        public bool HasFertilization
        {
            get { return false; }
        }

        public bool HasThinning
        {
            get { return false; }
        }
    }
}
