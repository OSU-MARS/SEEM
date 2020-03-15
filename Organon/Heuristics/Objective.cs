namespace Osu.Cof.Organon.Heuristics
{
    public class Objective
    {
        public float DiscountRate { get; set; }
        public float DouglasFirPricePerMbf { get; set; }
        public float FixedRegenerationHarvestCostPerAcre { get; set; }
        public float FixedThinningCostPerAcre { get; set; }
        public HarvestPeriodSelection HarvestPeriodSelection { get; set; }
        public bool IsNetPresentValue { get; set; }
        public float TimberAppreciationRate { get; set; }
        public VolumeUnits VolumeUnits { get; set; }

        public Objective()
        {
            this.DiscountRate = 0.04F;
            // from recent Oregon Department of Forestry bids on West Oregon District
            this.DouglasFirPricePerMbf = 450.0F;
            // from FOR 469 timber appraisal
            this.FixedRegenerationHarvestCostPerAcre = 570.0F;
            // rough approximation
            this.FixedThinningCostPerAcre = 100.0F;
            this.HarvestPeriodSelection = HarvestPeriodSelection.NoneOrLast;
            this.IsNetPresentValue = false;
            this.TimberAppreciationRate = 0.01F;
            this.VolumeUnits = VolumeUnits.CubicMetersPerHectare;
        }
    }
}
