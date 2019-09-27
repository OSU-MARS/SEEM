using System;

namespace Osu.Cof.Organon.Test
{
    internal class TreeRecord
    {
        public float CrownRatio { get; private set; }
        public float DbhInInches { get; private set; }
        public float ExpansionFactor { get; private set; }
        public float HeightInFeet { get; private set; }
        public FiaCode Species { get; private set; }

        // TODO: move stand and breast height ages from options
        public TreeRecord(FiaCode species, float dbhInInches, float expansionFactor, float crownRatio)
        {
            this.CrownRatio = crownRatio;
            this.DbhInInches = dbhInInches;
            this.ExpansionFactor = expansionFactor;
            this.HeightInFeet = this.GetHeightInFeet(species, dbhInInches);
            this.Species = species;
        }

        private float GetHeightInFeet(FiaCode species, float dbhInInches)
        {
            // Organon requires trees be at least 4.5 feet tall
            switch (species)
            {
                // most conifers
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                case FiaCode.CalocedrusDecurrens:
                case FiaCode.LithocarpusDensiflorus:
                case FiaCode.PinusLambertiana:
                case FiaCode.PinusPonderosa:
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.ThujaPlicata:
                case FiaCode.TsugaHeterophylla:
                    return 3.0F * dbhInInches + 4.5F;

                // most hardwoods
                case FiaCode.AcerMacrophyllum:
                case FiaCode.AlnusRubra:
                case FiaCode.ArbutusMenziesii:
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                case FiaCode.CornusNuttallii:
                case FiaCode.QuercusChrysolepis:
                case FiaCode.QuercusGarryana:
                case FiaCode.QuercusKelloggii:
                    return 2.0F * dbhInInches + 4.5F;

                // special cases
                case FiaCode.Salix:
                case FiaCode.TaxusBrevifolia:
                    return 1.5F * dbhInInches + 4.5F;

                default:
                    throw new NotSupportedException(String.Format("Unhandled species {0}.", species));
            }
        }
    }
}
