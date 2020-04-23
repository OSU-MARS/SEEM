using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Osu.Cof.Ferm
{
    internal static class Constant
    {
        public const float AcresPerHectare = 2.47105F;
        public const float CmPerInch = 2.54000F;
        public const float CubicFeetPerCubicMeter = 35.3147F;
        public const float CubicMetersPerCubicFoot = 0.0283168F;
        public const int DefaultTimeStepInYears = 5;
        public const float FeetPerMeter = 3.28084F;
        public const float ForestersEnglish = 0.005454154F;
        public const float HectaresPerAcre = 0.404685F;
        public const float HeightStrataAsFloat = 40.0F;
        public const float InchesPerCm = 0.393701F;
        public const float MetersPerFoot = 0.3048F;
        public const float NaturalLogOf10 = 2.3025850930F;
        // 0.00003 and smaller result in expected ArgumentOutOfRangeExceptions due to single precision
        // However, 0.0001 still results in rare exceptions. The underlying cause is unclear.
        public const float RoundToZeroTolerance = 0.001F;
        public const int SimdWidthInSingles = 4;

        public static readonly ReadOnlyCollection<FiaCode> NwoSmcSpecies = new ReadOnlyCollection<FiaCode>(new List<FiaCode>()
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
        public static readonly ReadOnlyCollection<FiaCode> RapSpecies = new ReadOnlyCollection<FiaCode>(new List<FiaCode>()
        {
            FiaCode.AlnusRubra,
            FiaCode.PseudotsugaMenziesii,
            FiaCode.TsugaHeterophylla,
            FiaCode.ThujaPlicata,
            FiaCode.AcerMacrophyllum,
            FiaCode.CornusNuttallii,
            FiaCode.Salix
        });
        public static readonly ReadOnlyCollection<FiaCode> SwoSpecies = new ReadOnlyCollection<FiaCode>(new List<FiaCode>()
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
            FiaCode.NotholithocarpusDensiflorus,
            FiaCode.QuercusChrysolepis,
            FiaCode.AcerMacrophyllum,
            FiaCode.QuercusGarryana,
            FiaCode.QuercusKelloggii,
            FiaCode.AlnusRubra,
            FiaCode.CornusNuttallii,
            FiaCode.Salix
        });

        public static class Maximum
        {
            public const float DiameterIncrementInInches = 4.0F;
            public const float HeightIncrementInFeet = 20.0F;
            public const float Sdi = 1000.0F;
            public const float SiteIndexInFeet = 300.0F;
        }

        public static class Minimum
        {
            // volume thresholds are debatable as smallest trees used in forming Poudel 2018's regressions were 15 cm DBH
            // However, ignoring biomass in trees less than 15 cm is probably a larger error than extending the regression beyond its fitting
            // range.
            public const float DiameterForVolumeInInches = 4.0F;
        }

        public static class Nelder
        {
            public static class ColumnIndex
            {
                public static int DbhInMillimeters = 3;
                public static int HeightInDecimeters = 4;
                public static int Species = 0;
                public static int Tree = 2;
            }
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

        public static class Psp
        {
            public const int DefaultNumberOfStandMeasurements = 8;

            public static class ColumnIndex
            {
                public const int Dbh = 11;
                public const int Plot = 5;
                public const int Species = 7;
                public const int Status = 10;
                public const int Tag = 8;
                public const int Year = 9;
            }

            public static class TreeStatus
            {
                public static readonly int Dead = 6;
                public static readonly int Fused = 3;
                public static readonly int Ingrowth = 2;
                public static readonly int Live = 1;
                public static readonly int NotFound = 9;
            }
        }
    }
}
