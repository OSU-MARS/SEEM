using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon.Test
{
    internal class PspTreeMeasurementSeries
    {
        public SortedList<int, float> DbhInCentimetersByYear { get; private set; }
        public string Species { get; private set; }
        public int Tag { get; private set; }

        public PspTreeMeasurementSeries(int tag, string species)
        {
            this.DbhInCentimetersByYear = new SortedList<int, float>(Constant.Psp.DefaultNumberOfStandMeasurements);
            this.Species = species;
            this.Tag = tag;
        }
        
        public float EstimateInitialCrownRatio(StandDensity density)
        {
            float initialDiameterInInches = 0.393701F * this.GetInitialDiameter();
            float basalAreaLargerRatio = density.GetBasalAreaLarger(initialDiameterInInches) / density.BasalAreaPerAcre;
            Debug.Assert(basalAreaLargerRatio >= 0.0F);
            Debug.Assert(basalAreaLargerRatio <= 1.0F);

            if (basalAreaLargerRatio < 0.25F)
            {
                // emergent or dominant
                return TestConstant.Default.CrownRatio;
            }

            return TestConstant.Default.CrownRatio * (1.3F - 0.9F / 0.75F * basalAreaLargerRatio);
        }

        public float EstimateInitialHeightInMeters()
        {
            float initialDiameterInCm = this.GetInitialDiameter();
            return 2.0F * 0.274F * initialDiameterInCm;
        }

        public FiaCode GetFiaCode()
        {
            return this.Species switch
            {
                //"ABAM" => FiaCode.AbiesAmabalis,
                //"ABPR" => FiaCode.AbiesProcera,
                "ABAM" => FiaCode.AbiesGrandis,
                "ABPR" => FiaCode.AbiesConcolor,
                "PSME" => FiaCode.PseudotsugaMenziesii,
                "TSHE" => FiaCode.TsugaHeterophylla,
                _ => throw new NotSupportedException(String.Format("Unhandled species {0}.", this.Species))
            };
        }

        public float GetInitialDiameter()
        {
            return this.DbhInCentimetersByYear.Values[0];
        }
    }
}
