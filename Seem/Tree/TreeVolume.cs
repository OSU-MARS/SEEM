namespace Osu.Cof.Ferm.Tree
{
    public class TreeVolume
    {
        public static TreeVolume Default { get; private set; }

        public ScaledVolume RegenerationHarvest { get; private init; }
        public ScaledVolume Thinning { get; private init; }

        static TreeVolume()
        {
            TreeVolume.Default = new TreeVolume();
        }

        public TreeVolume()
            : this(Constant.Bucking.DefaultMaximumDiameterInCentimeters, Constant.Bucking.DefaultMaximumHeightInMeters, scribnerFromLumberRecovery: false)
        {
        }

        public TreeVolume(float maximumDiameterInCentimeters, float maximumHeightInMeters, bool scribnerFromLumberRecovery)
            : this(Constant.Bucking.LogLengthThinning, Constant.Bucking.LogLengthRegenerationHarvest, maximumDiameterInCentimeters, maximumHeightInMeters, scribnerFromLumberRecovery)
        {
        }

        public TreeVolume(float thinningLogLengthInM, float regenerationHarvestLogLengthInM, float maximumDiameterInCentimeters, float maximumHeightInMeters, bool scribnerFromLumberRecovery)
        {
            this.RegenerationHarvest = new ScaledVolume(maximumDiameterInCentimeters, maximumHeightInMeters, regenerationHarvestLogLengthInM, scribnerFromLumberRecovery);
            this.Thinning = new ScaledVolume(maximumDiameterInCentimeters, maximumHeightInMeters, thinningLogLengthInM, scribnerFromLumberRecovery);
        }
    }
}