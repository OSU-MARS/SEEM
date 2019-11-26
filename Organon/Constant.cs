using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Osu.Cof.Organon
{
    internal static class Constant
    {
        public const int DefaultTimeStepInYears = 5;

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
            FiaCode.LithocarpusDensiflorus,
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
            public const float MSDI = 1000.0F;
            public const float SiteIndexInFeet = 300.0F;
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
            }
        }

        public static class TreeIndex
        {
            public static class Growth
            {
                public const int Height = 0;
                public const int Diameter = 1;
                public const int AccumulatedHeight = 2;
                public const int AccumulatedDiameter = 3;
            }
        }
    }
}
