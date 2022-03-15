using System;

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
            TreeSpeciesVolumeTableParameters[] finalHarvestParameters = new TreeSpeciesVolumeTableParameters[]
            {
                new(FiaCode.PseudotsugaMenziesii, finalHarvestTableRange, PoudelRegressions.GetDouglasFirDiameterInsideBark, DouglasFir.GetNeiloidHeight),
                new(FiaCode.ThujaPlicata, finalHarvestTableRange, WesternRedcedar.GetDiameterInsideBark, WesternRedcedar.GetNeiloidHeight),
                new(FiaCode.TsugaHeterophylla, finalHarvestTableRange, PoudelRegressions.GetWesternHemlockDiameterInsideBark, WesternHemlock.GetNeiloidHeight)
            };
            this.RegenerationHarvest = new ScaledVolume(finalHarvestParameters);

            TreeSpeciesVolumeTableParameters[] thinningParameters = new TreeSpeciesVolumeTableParameters[]
            {
                new(FiaCode.PseudotsugaMenziesii, thinningTableRange, PoudelRegressions.GetDouglasFirDiameterInsideBark, DouglasFir.GetNeiloidHeight),
                new(FiaCode.ThujaPlicata, thinningTableRange, WesternRedcedar.GetDiameterInsideBark, WesternRedcedar.GetNeiloidHeight),
                new(FiaCode.TsugaHeterophylla, thinningTableRange, PoudelRegressions.GetWesternHemlockDiameterInsideBark, WesternHemlock.GetNeiloidHeight)
            };
            this.Thinning = new ScaledVolume(thinningParameters);
        }
    }
}