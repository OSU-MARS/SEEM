namespace Osu.Cof.Ferm
{
    public class TreeVolume
    {
        public static TreeVolume Default { get; private set; }

        public ScaledVolume RegenerationHarvest { get; private init; }
        public ScaledVolume Thinning { get; private init; }

        static TreeVolume()
        {
            TreeVolume.Default = new TreeVolume(Constant.Bucking.DefaultMaximumDiameterInCentimeters, Constant.Bucking.DefaultMaximumHeightInMeters, false);
        }

        public TreeVolume(float maximumDiameterInCentimeters, float maximumHeightInMeters, bool scribnerFromLumberRecovery)
        {
            this.RegenerationHarvest = new ScaledVolume(maximumDiameterInCentimeters, maximumHeightInMeters, Constant.Bucking.LogLengthRegenerationHarvest, scribnerFromLumberRecovery);
            this.Thinning = new ScaledVolume(maximumDiameterInCentimeters, maximumHeightInMeters, Constant.Bucking.LogLengthThinning, scribnerFromLumberRecovery);
        }
    }
}
