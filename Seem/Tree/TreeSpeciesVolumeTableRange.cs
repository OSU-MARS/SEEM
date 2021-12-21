namespace Mars.Seem.Tree
{
    public class TreeSpeciesVolumeTableRange : PreferredLogLength
    {
        public float MaximumDiameterInCentimeters { get; init; }
        public float MaximumHeightInMeters { get; init; }
        public bool ScribnerFromLumberRecovery { get; init; }

        public TreeSpeciesVolumeTableRange()
        {
            this.MaximumDiameterInCentimeters = Constant.Bucking.DefaultMaximumFinalHarvestDiameterInCentimeters;
            this.MaximumHeightInMeters = Constant.Bucking.DefaultMaximumFinalHarvestHeightInMeters;
            this.ScribnerFromLumberRecovery = false;
        }

        public TreeSpeciesVolumeTableRange(TreeSpeciesVolumeTableRange other)
            : base(other)
        {
            this.MaximumDiameterInCentimeters = other.MaximumDiameterInCentimeters;
            this.MaximumHeightInMeters = other.MaximumHeightInMeters;
            this.ScribnerFromLumberRecovery = other.ScribnerFromLumberRecovery;
        }
    }
}
