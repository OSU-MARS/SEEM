using System;
using System.Collections.Generic;

namespace Osu.Cof.Organon.Test
{
    public class PspTreeMeasurementSeries
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
            float initialDiameterInInches = this.GetInitialDiameter() / Constant.CmPerInch;
            float crownCompetition = density.GetCrownCompetitionFactorLarger(initialDiameterInInches);
            float crownCompetitionMidpoint = this.Species switch
            {
                "PSME" => 125.0F,
                "THPL" => 250.0F,
                "TSHE" => 300.0F,
                _ => 200.0F
            };
            return TestConstant.Default.CrownRatio / (float)(1.0 + Math.Exp(0.015 * (crownCompetition - crownCompetitionMidpoint)));
        }

        public int GetFirstMeasurementYear()
        {
            return this.DbhInCentimetersByYear.Keys[0];
        }

        public float GetInitialDiameter()
        {
            return this.DbhInCentimetersByYear.Values[0];
        }
    }
}
