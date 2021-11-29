using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;

namespace Mars.Seem.Test
{
    public class PspTreeMeasurementSeries
    {
        public SortedList<int, float> DbhInCentimetersByYear { get; private set; }
        public int Plot { get; private set; }
        public FiaCode Species { get; private set; }
        public int Tag { get; private set; }

        public PspTreeMeasurementSeries(int plot, int tag, FiaCode species)
        {
            this.DbhInCentimetersByYear = new SortedList<int, float>(Constant.Psp.DefaultNumberOfStandMeasurements);
            this.Species = species;
            this.Plot = plot;
            this.Tag = tag;
        }

        public float EstimateInitialCrownRatio(OrganonStandDensity density)
        {
            float initialDiameterInInches = this.GetInitialDiameter() / Constant.CentimetersPerInch;
            float crownCompetition = density.GetCrownCompetitionFactorLarger(initialDiameterInInches);
            float crownCompetitionMidpoint = this.Species switch
            {
                FiaCode.PseudotsugaMenziesii => 125.0F,
                FiaCode.ThujaPlicata => 250.0F,
                FiaCode.TsugaHeterophylla => 300.0F,
                _ => 200.0F
            };
            return TestConstant.Default.CrownRatio / (1.0F + MathF.Exp(0.015F * (crownCompetition - crownCompetitionMidpoint)));
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
