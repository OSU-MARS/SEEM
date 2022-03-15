using System;

namespace Mars.Seem.Tree
{
    public class TreeSpeciesVolumeTableParameters : TreeSpeciesVolumeTableRange
    {
        public Func<float, float, float, float> GetDiameterInsideBark { get; private init; }
        public Func<float, float, float> GetNeiloidHeight { get; private init; }
        public FiaCode Species { get; private init; }

        public TreeSpeciesVolumeTableParameters(FiaCode species, TreeSpeciesVolumeTableRange range, Func<float, float, float, float> getDiameterInsideBark, Func<float, float, float> getNeiloidHeight)
            : base(range)
        {
            this.GetDiameterInsideBark = getDiameterInsideBark;
            this.GetNeiloidHeight = getNeiloidHeight;
            this.Species = species;
        }
    }
}
