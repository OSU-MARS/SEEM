using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Osu.Cof.Organon.Test
{
    internal static class TestConstant
    {
        public static readonly ReadOnlyCollection<FiaCode> NwoSmcSpeciesCodes = new ReadOnlyCollection<FiaCode>(new List<FiaCode>() 
        {
            FiaCode.PseudotsugaMenziesii, 
            FiaCode.AbiesGrandis,
            FiaCode.TsugaHeterophylla,
            FiaCode.ThujaPlicata,
            FiaCode.TaxusBrevifolia,
            FiaCode.ArbutusMenziesii,
            FiaCode.AcerMacrophyllum,
            FiaCode.QuercusGarryana,
            FiaCode.AlnusRubra,
            FiaCode.CornusNuttallii,
            FiaCode.Salix
        });
        public static readonly ReadOnlyCollection<FiaCode> RapSpeciesCodes = new ReadOnlyCollection<FiaCode>(new List<FiaCode>() 
        {
            FiaCode.AlnusRubra, 
            FiaCode.PseudotsugaMenziesii,
            FiaCode.TsugaHeterophylla,
            FiaCode.ThujaPlicata,
            FiaCode.AcerMacrophyllum,
            FiaCode.CornusNuttallii,
            FiaCode.Salix
        });
        public static readonly ReadOnlyCollection<FiaCode> SwoSpeciesCodes = new ReadOnlyCollection<FiaCode>(new List<FiaCode>() 
        {
            FiaCode.PseudotsugaMenziesii, 
            FiaCode.AbiesConcolor, 
            FiaCode.AbiesGrandis,
            FiaCode.PinusPonderosa,
            FiaCode.PinusLambertiana,
            FiaCode.CalocedrusDecurrens,
            FiaCode.TsugaHeterophylla,
            FiaCode.ThujaPlicata,
            FiaCode.TaxusBrevifolia,
            FiaCode.ArbutusMenziesii,
            FiaCode.ChrysolepisChrysophyllaVarChrysophylla,
            FiaCode.LithocarpusDensiflorus,
            FiaCode.QuercusChrysolepis,
            FiaCode.AcerMacrophyllum,
            FiaCode.QuercusGarryana,
            FiaCode.QuercusKelloggii,
            FiaCode.AlnusRubra,
            FiaCode.CornusNuttallii,
            FiaCode.Salix
        });

        public static ReadOnlyCollection<FiaCode> TreeSpeciesList = new ReadOnlyCollection<FiaCode>(new FiaCode[] {
                FiaCode.AbiesGrandis,
                FiaCode.CalocedrusDecurrens,
                FiaCode.PinusPonderosa,
                FiaCode.Salix,

                FiaCode.PinusLambertiana,
                FiaCode.TaxusBrevifolia,
                FiaCode.TsugaHeterophylla,
                FiaCode.AcerMacrophyllum,
                FiaCode.AlnusRubra,
                FiaCode.ArbutusMenziesii,
                FiaCode.ChrysolepisChrysophyllaVarChrysophylla,
                FiaCode.CornusNuttallii,
                FiaCode.QuercusGarryana,

                FiaCode.AbiesConcolor,
                FiaCode.LithocarpusDensiflorus,
                FiaCode.PinusLambertiana,
                FiaCode.QuercusChrysolepis,
                FiaCode.QuercusKelloggii
            });

        public static readonly Variant[] Variants = new Variant[] { Variant.Nwo, Variant.Swo, Variant.Smc, Variant.Rap };

        public static class Default
        {
            public const float A1 = 0.62F;
            public const float A1MAX = 0.62F;
            public const float A2 = 5.5F;
            public const float AgeToReachBreastHeightInYears = 5.0F;
            public const float BABT = 0.0F; // (DOUG?)
            public const float MaximumReinekeStandDensityIndex = 600.0F;
            public const float PDEN = 1.0F; // (DOUG?)
            public const float NO = 0.0F; // (DOUG?)
            public const float RD0 = 0.0F; // (DOUG?)
            public const float RAAGE = 0.0F; // (DOUG?)
            public const float RedAlderSiteIndex = 100.0F; // TODO: look at Andy's papers and change to a not made up value
            public const int SimulationCyclesToRun = 20; // 20 5 year time steps
            public const float SiteIndex = 125.0F; // feet at 50 years
        }

        public static class Maximum
        {
            // TODO: make DBH, height, and crown limits species specific
            public const float CrownClosure = 1000.0F;
            public const float DbhInInches = 120.0F;
            public const float ExpansionFactor = 100.0F;
            public const float HeightInFeet = 380.0F;
            public const float LargestCrownWidthInFeet = 300.0F;
            public const float TreeBasalAreaLarger = 1000.0F;
            public const float MaximumCrownWidthInFeet = 300.0F;
            public const float MGExpansionFactor = 1000.0F;
            public const int StandAgeInYears = 500;
            public const float StandCrownCompetitionFactor = 10000.0F;
        }
    }
}
