using System;

namespace Mars.Seem.Tree
{
    public class TreeSpeciesVolumeTableParameters : TreeSpeciesVolumeTableRange
    {
        public Func<float, float, float, float> GetDiameterInsideBark { get; init; }
        public Func<float, float> GetNeiloidHeight { get; init; }

        public TreeSpeciesVolumeTableParameters(TreeSpeciesVolumeTableRange range, Func<float, float, float, float> getDiameterInsideBark, Func<float, float> getNeiloidHeight)
            : base(range)
        {
            this.GetDiameterInsideBark = getDiameterInsideBark;
            this.GetNeiloidHeight = getNeiloidHeight;
        }
    }
}
