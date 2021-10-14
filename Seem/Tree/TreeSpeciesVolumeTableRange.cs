namespace Osu.Cof.Ferm.Tree
{
    public class TreeSpeciesVolumeTableRange
    {
        public float MaximumDiameterInCentimeters { get; init; }
        public float MaximumHeightInMeters { get; init; }
        public float PreferredLogLengthInMeters { get; init; }
        public bool ScribnerFromLumberRecovery { get; init; }

        public TreeSpeciesVolumeTableRange()
        {
            this.MaximumDiameterInCentimeters = Constant.Bucking.DefaultMaximumFinalHarvestDiameterInCentimeters;
            this.MaximumHeightInMeters = Constant.Bucking.DefaultMaximumFinalHarvestHeightInMeters;
            this.PreferredLogLengthInMeters = Constant.Bucking.DefaultLongLogLength;
            this.ScribnerFromLumberRecovery = false;
        }

        public TreeSpeciesVolumeTableRange(TreeSpeciesVolumeTableRange other)
        {
            this.MaximumDiameterInCentimeters = other.MaximumDiameterInCentimeters;
            this.MaximumHeightInMeters = other.MaximumHeightInMeters;
            this.PreferredLogLengthInMeters = other.PreferredLogLengthInMeters;
            this.ScribnerFromLumberRecovery = other.ScribnerFromLumberRecovery;
        }
    }
}
