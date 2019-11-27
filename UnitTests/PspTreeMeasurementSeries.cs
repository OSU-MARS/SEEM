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
            float initialDiameterInInches = this.GetInitialDiameter() / TestConstant.CmPerInch;
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

        public float EstimateInitialHeightInMeters()
        {
            // height-diameter equations from
            // Ishii H, Reynolds JH, Ford ED, Shaw DC. 2000. Height growth and vertical development of an old-growth
            //   Pseudotsuga-Tsuga forest in southwestern Washington State, U.S.A. Canadian Journal of Forest Resources
            //   30:17-24. http://faculty.washington.edu/joel/Papers/IshiiReynoldsetalCJFR2000.pdf
            //   Figure 2: ABAM, PSME, TABR, THPL, and TSHE at Wind River / TT Munger Experimental Forest
            // Fujimori T, Kawanabe S, Hideki S, et al. 1976. Biomass and primary production in forests of three major 
            //   vegetation zones of the northwestern United States. Journal of the Japanese Forestry Society 58(10):360-373.
            //   http://andrewsforest.oregonstate.edu/publications/800
            //   Figure 6: ABPR Goat Marsh Research Natural Area
            //
            // also
            // Curtis RO. 2015. Development of Top Heights and Corresponding Diameters in High Elevation Noble Fir Plantations. 
            //   Research Paper PNW-RP-603 Pacific Northwest Research Station USFS. https://www.fs.fed.us/pnw/pubs/pnw_rp603.pdf
            //   ABPR 50 year site index: 24 m @ n = 1
            // Franklin JF. ND. Abies procera. https://andrewsforest.oregonstate.edu/sites/default/files/lter/pubs/pdf/pub1168.pdf
            //   ABPR class II 100 year site index: 36 m => 48 cm DBH under Ishii 2000
            float initialDiameterInCm = this.GetInitialDiameter();
            return this.Species switch
            {
                "ABAM" => 45.8F * (1.0F - (float)Math.Exp(-0.008 * Math.Pow(initialDiameterInCm, 1.36))),
                "ABPR" => initialDiameterInCm / (0.6035F + 0.0095F * initialDiameterInCm),
                "PSME" => 60.1F * (1.0F - (float)Math.Exp(-0.007 * Math.Pow(initialDiameterInCm, 1.25))),
                "TABR" => 50.0F * (1.0F - (float)Math.Exp(-0.025 * Math.Pow(initialDiameterInCm, 0.71))),
                "THPL" => 68.5F * (1.0F - (float)Math.Exp(-0.009 * Math.Pow(initialDiameterInCm, 1.04))),
                "TSHE" => 56.9F * (1.0F - (float)Math.Exp(-0.007 * Math.Pow(initialDiameterInCm, 1.29))),
                // BUGBUG: add other species
                _ => 2.0F * 0.274F * initialDiameterInCm,
            };
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
