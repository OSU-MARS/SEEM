﻿using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Test
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

        public float EstimateInitialCrownRatio(OrganonStandDensity density)
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
