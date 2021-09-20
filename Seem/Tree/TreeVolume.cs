using System;

namespace Osu.Cof.Ferm.Tree
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
                MaximumDiameterInCentimeters = Constant.Bucking.MaximumThinningDiameterInCentimeters,
                MaximumHeightInMeters = Constant.Bucking.DefaultThinningMaximumHeightInMeters,
                PreferredLogLengthInMeters = Constant.Bucking.DefaultThinningLogLength,
            };
            TreeVolume.Default = new TreeVolume(finalHarvestRange, thinningRange);
        }

        public TreeVolume(TreeSpeciesVolumeTableRange finalHarvestTableRange, TreeSpeciesVolumeTableRange thinningTableRange)
        {
            TreeSpeciesVolumeTableParameters psmeFinalHarvestParameters = new(finalHarvestTableRange, PoudelRegressions.GetDouglasFirDiameterInsideBark, DouglasFir.GetNeiloidHeight);
            this.RegenerationHarvest = new ScaledVolume(psmeFinalHarvestParameters);

            TreeSpeciesVolumeTableParameters psmeThinningParameters = new(thinningTableRange, PoudelRegressions.GetDouglasFirDiameterInsideBark, DouglasFir.GetNeiloidHeight);
            this.Thinning = new ScaledVolume(psmeThinningParameters);
        }
    }
}