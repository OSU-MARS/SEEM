using System.Collections.ObjectModel;

namespace Osu.Cof.Organon.Test
{
    internal static class TestConstant
    {
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

        public static readonly OrganonVariant[] Variants = new OrganonVariant[] 
        { 
            new OrganonVariant(Variant.Nwo),
            new OrganonVariant(Variant.Swo),
            new OrganonVariant(Variant.Smc),
            new OrganonVariant(Variant.Rap)
        };

        public static class Default
        {
            public const float AgeToReachBreastHeightInYears = 5.0F;
            public const float BABT = 0.0F; // (DOUG?)
            public const float CrownRatio = 0.5F;
            public const float MaximumReinekeStandDensityIndex = 600.0F;
            public const float PDEN = 1.0F; // (DOUG?)
            public const float RAAGE = 0.0F; // (DOUG?)
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
            public const int StandAgeInYears = 500;
            public const float StandCrownCompetitionFactor = 10000.0F;
        }

        public static class OpenXml
        {
            public const string Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            public static class Attribute
            {
                public const string CellReference = "r";
                public const string CellType = "t";
                public const string Reference = "ref";
            }

            public static class CellType
            {
                public const string SharedString = "s";
            }

            public static class Element
            {
                public const string Cell = "c";
                public const string CellValue = "v";
                public const string Dimension = "dimension";
                public const string Row = "row";
                public const string SharedString = "si";
                public const string SharedStringText = "t";
                public const string SheetData = "sheetData";
            }
        }
    }
}
