using System;

namespace Osu.Cof.Ferm.Test
{
    public class TreeRecord
    {
        public float CrownRatio { get; private set; }
        public float DbhInInches { get; private set; }
        public float HeightInFeet { get; private set; }
        public float LiveExpansionFactor { get; private set; }
        public int Plot { get; private set; }
        public FiaCode Species { get; private set; }
        public int Tag { get; private set; }

        public TreeRecord(int plot, int tag, FiaCode species, float dbhInInches, float crownRatio, float expansionFactor)
        {
            this.CrownRatio = crownRatio;
            this.DbhInInches = dbhInInches;
            this.HeightInFeet = TreeRecord.EstimateHeightInFeet(species, dbhInInches);
            this.LiveExpansionFactor = expansionFactor;
            this.Plot = plot;
            this.Species = species;
            this.Tag = tag;
        }

        public static float EstimateHeightInFeet(FiaCode species, float dbhInInches)
        {
            return TestConstant.FeetPerMeter * TreeRecord.EstimateHeightInMeters(species, dbhInInches);
        }

        public static float EstimateHeightInMeters(FiaCode species, float dbhInInches)
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
            float dbhInCm = Constant.CentimetersPerInch * dbhInInches;
            var heightInM = species switch
            {
                FiaCode.AbiesAmabalis => 45.8F * (1.0F - MathF.Exp(-0.008F * MathF.Pow(dbhInCm, 1.36F))),
                FiaCode.AbiesProcera => dbhInCm / (0.6035F + 0.0095F * dbhInCm),
                FiaCode.PseudotsugaMenziesii => 60.1F * (1.0F - MathF.Exp(-0.007F * MathF.Pow(dbhInCm, 1.25F))),
                FiaCode.TaxusBrevifolia => 50.0F * (1.0F - MathF.Exp(-0.025F * MathF.Pow(dbhInCm, 0.71F))),
                FiaCode.ThujaPlicata => 68.5F * (1.0F - MathF.Exp(-0.009F * MathF.Pow(dbhInCm, 1.04F))),
                FiaCode.TsugaHeterophylla => 56.9F * (1.0F - MathF.Exp(-0.007F * MathF.Pow(dbhInCm, 1.29F))),
                // simple defaults for other conifers and willows
                FiaCode.AbiesConcolor or 
                FiaCode.AbiesGrandis or 
                FiaCode.CalocedrusDecurrens or 
                FiaCode.NotholithocarpusDensiflorus or 
                FiaCode.PinusLambertiana or 
                FiaCode.PinusPonderosa or 
                FiaCode.Salix => dbhInCm + 1.37F,
                // simple defaults for most hardwoods
                FiaCode.AcerMacrophyllum or 
                FiaCode.AlnusRubra or 
                FiaCode.ArbutusMenziesii or 
                FiaCode.ChrysolepisChrysophyllaVarChrysophylla or 
                FiaCode.CornusNuttallii or 
                FiaCode.QuercusChrysolepis or 
                FiaCode.QuercusGarryana or 
                FiaCode.QuercusKelloggii => 0.8F * dbhInCm + 1.37F,
                _ => throw Trees.CreateUnhandledSpeciesException(species),
            };
            ;
            if (heightInM < 1.372F)
            {
                // regression equations above may be inaccurate on small trees, resulting in heights below Organon 2.2.4's 4.5 foot minimum
                // For now, and for simplicity, force these trees to satisfy legacy Organon height requirements.
                heightInM = 1.372F;
            }
            return heightInM;
        }
    }
}
