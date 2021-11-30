namespace Mars.Seem.Tree
{
    public class TreeVolume
    {
        public static TreeVolume Default { get; private set; }

        public ScaledVolume RegenerationHarvest { get; private init; }
        public ScaledVolume Thinning { get; private init; }

        static TreeVolume()
        {
            TreeSpeciesVolumeTableRange finalHarvestRange = new();
            TreeSpeciesVolumeTableRange thinningRange = new()
            {
                MaximumDiameterInCentimeters = Constant.Bucking.DefaultMaximumThinningDiameterInCentimeters,
                MaximumHeightInMeters = Constant.Bucking.DefaultMaximumThinningHeightInMeters,
                PreferredLogLengthInMeters = Constant.Bucking.DefaultShortLogLengthInM,
            };
            TreeVolume.Default = new TreeVolume(finalHarvestRange, thinningRange);
        }

        public TreeVolume(TreeSpeciesVolumeTableRange finalHarvestTableRange, TreeSpeciesVolumeTableRange thinningTableRange)
        {
            TreeSpeciesVolumeTableParameters psmeFinalHarvestParameters = new(finalHarvestTableRange, PoudelRegressions.GetDouglasFirDiameterInsideBark, DouglasFir.GetNeiloidHeight);
            TreeSpeciesVolumeTableParameters tsheFinalHarvestParameters = new(finalHarvestTableRange, PoudelRegressions.GetWesternHemlockDiameterInsideBark, WesternHemlock.GetNeiloidHeight);
            this.RegenerationHarvest = new ScaledVolume(psmeFinalHarvestParameters, tsheFinalHarvestParameters);

            TreeSpeciesVolumeTableParameters psmeThinningParameters = new(thinningTableRange, PoudelRegressions.GetDouglasFirDiameterInsideBark, DouglasFir.GetNeiloidHeight);
            TreeSpeciesVolumeTableParameters thseThinningParameters = new(thinningTableRange, PoudelRegressions.GetWesternHemlockDiameterInsideBark, WesternHemlock.GetNeiloidHeight);
            this.Thinning = new ScaledVolume(psmeThinningParameters, thseThinningParameters);
        }
    }
}