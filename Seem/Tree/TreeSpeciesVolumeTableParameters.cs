using System;

namespace Mars.Seem.Tree
{
    public class TreeSpeciesVolumeTableParameters : TreeSpeciesVolumeTableRange
    {
        public Func<float, float, float, float> GetDiameterInsideBark { get; private init; }
        public Func<float, float, float> GetNeiloidHeight { get; private init; }

        // peelers, 1S, and special mill not currently supported
        public float MinimumLogLength2SawInM { get; private init; }
        public float MinimumLogLength3SawInM { get; private init; }
        public float MinimumLogLength4SawInM { get; private init; }
        public float MinimumScalingDiameter2Saw { get; private init; } // cm
        public float MinimumScalingDiameter3Saw { get; private init; } // cm
        public float MinimumScalingDiameter4Saw { get; private init; } // cm
        public float MinimumLogScribner2Saw { get; private init; } // Scribner board feet
        public float MinimumLogScribner3Saw { get; private init; } // Scribner board feet
        public float MinimumLogScribner4Saw { get; private init; } // Scribner board feet

        public FiaCode Species { get; private init; }

        public TreeSpeciesVolumeTableParameters(FiaCode species, TreeSpeciesVolumeTableRange range, Func<float, float, float, float> getDiameterInsideBark, Func<float, float, float> getNeiloidHeight)
            : base(range)
        {
            this.GetDiameterInsideBark = getDiameterInsideBark;
            this.GetNeiloidHeight = getNeiloidHeight;
            this.Species = species;

            // Douglas-fir, western hemlock
            // Oester PT, Bowers S. 2009. Measuring Timber Products Harvested from Your Woodland. The Woodland Workbook,
            //   Oregon State Extension. https://catalog.extension.oregonstate.edu/ec1127
            switch (species)
            {
                case FiaCode.AlnusRubra:
                    this.MinimumLogLength2SawInM = Constant.MetersPerFoot * 8.0F; // m
                    this.MinimumLogLength3SawInM = Constant.MetersPerFoot * 8.0F; // m
                    this.MinimumLogScribner2Saw = 60.0F; // board feet
                    this.MinimumScalingDiameter2Saw = Constant.CentimetersPerInch * 12.0F; // cm
                    this.MinimumScalingDiameter3Saw = Constant.CentimetersPerInch * 10.0F; // cm
                    this.MinimumScalingDiameter4Saw = Constant.CentimetersPerInch * 5.0F; // cm
                    break;
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.TsugaHeterophylla:
                    this.MinimumLogLength2SawInM = Constant.MetersPerFoot * 12.0F; // m
                    this.MinimumLogLength3SawInM = Constant.MetersPerFoot * 12.0F; // m
                    this.MinimumLogScribner2Saw = 60.0F; // board feet
                    this.MinimumScalingDiameter2Saw = Constant.CentimetersPerInch * 12.0F; // cm
                    this.MinimumScalingDiameter3Saw = Constant.CentimetersPerInch * 6.0F; // cm
                    this.MinimumScalingDiameter4Saw = Constant.CentimetersPerInch * 5.0F; // cm
                    break;
                case FiaCode.ThujaPlicata:
                    // Northwest Log Rules Advisory Group. 1982. Official Rules for the Following Log Scaling and Grading Bureaus:
                    //   Columbia River, Grays Harbor, Northern California, Puget Sound, Southern Oregon, Yamhill.
                    // no special mill
                    this.MinimumLogLength2SawInM = Constant.MetersPerFoot * 12.0F; // m
                    this.MinimumLogLength3SawInM = Constant.MetersPerFoot * 12.0F; // m
                    this.MinimumLogScribner2Saw = 210.0F; // board feet
                    this.MinimumScalingDiameter2Saw = Constant.CentimetersPerInch * 20.0F; // cm
                    this.MinimumScalingDiameter3Saw = Constant.CentimetersPerInch * 6.0F; // cm
                    this.MinimumScalingDiameter4Saw = Constant.CentimetersPerInch * 5.0F; // cm
                    break;
                default:
                    throw new NotSupportedException("Unhandled species " + species + ".");
            }

            this.MinimumLogLength4SawInM = Constant.MetersPerFoot * 8.0F; // m, sometimes not indicated, other times indicated as 12 foot
            this.MinimumLogScribner3Saw = 50.0F; // board feet
            this.MinimumLogScribner4Saw = 10.0F; // board feet
        }
    }
}
