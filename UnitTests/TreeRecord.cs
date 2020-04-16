using Osu.Cof.Ferm.Organon;
using System;

namespace Osu.Cof.Ferm.Test
{
    internal class TreeRecord
    {
        public float CrownRatio { get; private set; }
        public float DbhInInches { get; private set; }
        public float ExpansionFactor { get; private set; }
        public float HeightInFeet { get; private set; }
        public FiaCode Species { get; private set; }

        public TreeRecord(FiaCode species, float dbhInInches, float expansionFactor, float crownRatio)
        {
            this.CrownRatio = crownRatio;
            this.DbhInInches = dbhInInches;
            this.ExpansionFactor = expansionFactor;
            this.HeightInFeet = TreeRecord.EstimateHeightInFeet(species, dbhInInches);
            this.Species = species;
        }

        public static float EstimateHeightInFeet(FiaCode species, float dbhInInches)
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
            float dbhInCm = Constant.CmPerInch * dbhInInches;
            float heightInM;
            switch (species)
            {
                case FiaCode.AbiesAmabalis:
                    heightInM = 45.8F * (1.0F - MathF.Exp(-0.008F * MathF.Pow(dbhInCm, 1.36F)));
                    break;
                case FiaCode.AbiesProcera:
                    heightInM = dbhInCm / (0.6035F + 0.0095F * dbhInCm);
                    break;
                case FiaCode.PseudotsugaMenziesii:
                    heightInM = 60.1F * (1.0F - MathF.Exp(-0.007F * MathF.Pow(dbhInCm, 1.25F)));
                    break;
                case FiaCode.TaxusBrevifolia:
                    heightInM = 50.0F * (1.0F - MathF.Exp(-0.025F * MathF.Pow(dbhInCm, 0.71F)));
                    break;
                case FiaCode.ThujaPlicata:
                    heightInM = 68.5F * (1.0F - MathF.Exp(-0.009F * MathF.Pow(dbhInCm, 1.04F)));
                    break;
                case FiaCode.TsugaHeterophylla:
                    heightInM = 56.9F * (1.0F - MathF.Exp(-0.007F * MathF.Pow(dbhInCm, 1.29F)));
                    break;

                // simple defaults for other conifers and willows
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                case FiaCode.CalocedrusDecurrens:
                case FiaCode.NotholithocarpusDensiflorus:
                case FiaCode.PinusLambertiana:
                case FiaCode.PinusPonderosa:
                case FiaCode.Salix:
                    heightInM = dbhInCm + 1.37F;
                    break;

                // simple defaults for most hardwoods
                case FiaCode.AcerMacrophyllum:
                case FiaCode.AlnusRubra:
                case FiaCode.ArbutusMenziesii:
                case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                case FiaCode.CornusNuttallii:
                case FiaCode.QuercusChrysolepis:
                case FiaCode.QuercusGarryana:
                case FiaCode.QuercusKelloggii:
                    heightInM = 0.8F * dbhInCm + 1.37F;
                    break;

                default:
                    throw OrganonVariant.CreateUnhandledSpeciesException(species);
            };
            if (heightInM < 1.372F)
            {
                // regression equations above may be inaccurate on small trees, resulting in heights below Organon 2.2.4's 4.5 foot minimum
                // For now, and for simplicity, force these trees to satisfy legacy Organon height requirements.
                heightInM = 1.372F;
            }
            return TestConstant.FeetPerMeter * heightInM;
        }

        public static float EstimateHeightInMeters(string usdaSpeciesCode, float dbhInInches)
        {
            var species = usdaSpeciesCode switch
            {
                "ABAM" => FiaCode.AbiesAmabalis,
                "ABCO" => FiaCode.AbiesConcolor,
                "ABGR" => FiaCode.AbiesGrandis,
                "ABPR" => FiaCode.AbiesProcera,
                "ACGL" => FiaCode.AcerGlabrum,
                "ACMA3" => FiaCode.AcerMacrophyllum,
                "ALRU" => FiaCode.AlnusRubra,
                "ARME" => FiaCode.ArbutusMenziesii,
                "CADE" => FiaCode.CalocedrusDecurrens,
                "CHCH" => FiaCode.ChrysolepisChrysophyllaVarChrysophylla,
                "CONU" => FiaCode.CornusNuttallii,
                "PISI" => FiaCode.PiceaSitchensis,
                "PILA" => FiaCode.PinusLambertiana,
                "PIPO" => FiaCode.PinusPonderosa,
                "PSME" => FiaCode.PseudotsugaMenziesii,
                "QUCH" => FiaCode.QuercusChrysolepis,
                "QUGA" => FiaCode.QuercusGarryana,
                "QUKE" => FiaCode.QuercusKelloggii,
                "TABR" => FiaCode.TaxusBrevifolia,
                "THPL" => FiaCode.ThujaPlicata,
                "TSHE" => FiaCode.TsugaHeterophylla,
                _ => throw new NotSupportedException(String.Format("Unhandled species code '{0}'.", usdaSpeciesCode)),
            };
            return TreeRecord.EstimateHeightInFeet(species, dbhInInches);
        }
    }
}
