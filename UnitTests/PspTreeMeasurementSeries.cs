using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            // height-diameter equations from
            // Ishii H, Reynolds JH, Ford ED, Shaw DC. 2000. Height growth and vertical development of an old-growth
            //   Pseudotsuga-Tsuga forest in southwestern Washington State, U.S.A. Canadian Journal of Forest Resources
            //   30:17-24. http://faculty.washington.edu/joel/Papers/IshiiReynoldsetalCJFR2000.pdf
            //   Figure 2: ABAM, PSME, TABR, THPL, and TSHE at Wind River / TT Munger Experimental Forest
            // Fujimori T, Kawanabe S, Hideki S, et al.. 1976. Biomass and primary production in forests of three major 
            //   vegetation zones of the northwestern United States. Journal of the Japanese Forestry Society 58(10):360-373.
            //   http://andrewsforest.oregonstate.edu/publications/800
            //   Figure 6: ABPR Goat Marsh Research Natural Area
            float initialDiameterInCm = this.GetInitialDiameter();
            return this.Species switch
            {
                "ABAM" => 45.8F * (1.0F - (float)Math.Exp(-0.008 * Math.Pow(initialDiameterInCm, 1.36))),
                "ABPR" => initialDiameterInCm / (0.6035F + 0.0095F * initialDiameterInCm),
                "PSME" => 60.1F * (1.0F - (float)Math.Exp(-0.007 * Math.Pow(initialDiameterInCm, 1.25))),
                "TABR" => 50.0F * (1.0F - (float)Math.Exp(-0.025 * Math.Pow(initialDiameterInCm, 0.71))),
                "THPL" => 68.5F * (1.0F - (float)Math.Exp(-0.009 * Math.Pow(initialDiameterInCm, 1.04))),
                "TSHE" => 56.9F * (1.0F - (float)Math.Exp(-0.007 * Math.Pow(initialDiameterInCm, 1.29))),
                _ => 2.0F * 0.274F * initialDiameterInCm,
            };
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
