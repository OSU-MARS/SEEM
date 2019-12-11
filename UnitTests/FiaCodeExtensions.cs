using System;

namespace Osu.Cof.Organon.Test
{
    internal static class FiaCodeExtensions
    {
        public static FiaCode Parse(string fourLetterCode)
        {
            return fourLetterCode switch
            {
                //"ABAM" => FiaCode.AbiesAmabalis,
                //"ABPR" => FiaCode.AbiesProcera,
                "ABAM" => FiaCode.AbiesGrandis,
                "ABCO" => FiaCode.AbiesConcolor,
                "ABGR" => FiaCode.AbiesGrandis,
                "ABPR" => FiaCode.AbiesConcolor,
                "ACGL" => FiaCode.AcerGlabrum,
                "ACMA" => FiaCode.AcerMacrophyllum,
                "ALRU" => FiaCode.AlnusRubra,
                "ALSI" => FiaCode.Alnus, // no code for Alnus viridis ssp. sinuata
                "ARME" => FiaCode.ArbutusMenziesii,
                "CADE" => FiaCode.CalocedrusDecurrens,
                "CHCH" => FiaCode.ChrysolepisChrysophyllaVarChrysophylla,
                "CONU" => FiaCode.CornusNuttallii,
                "LIDE" => FiaCode.NotholithocarpusDensiflorus,
                "PILA" => FiaCode.PinusLambertiana,
                "PIPO" => FiaCode.PinusPonderosa,
                "PISI" => FiaCode.PiceaSitchensis,
                "PSME" => FiaCode.PseudotsugaMenziesii,
                "QUCH" => FiaCode.QuercusChrysolepis,
                "QUGA" => FiaCode.QuercusGarryana,
                "QUKE" => FiaCode.QuercusKelloggii,
                "Salix" => FiaCode.Salix,
                "TABR" => FiaCode.TaxusBrevifolia,
                "THPL" => FiaCode.ThujaPlicata,
                "TSHE" => FiaCode.TsugaHeterophylla,
                _ => throw new NotSupportedException(String.Format("Unhandled species '{0}'.", fourLetterCode))
            };
        }

        public static string ToFourLetterCode(this FiaCode species)
        {
            return species switch
            {
                FiaCode.AbiesAmabalis => "ABAM",
                FiaCode.AbiesConcolor => "ABCO",
                FiaCode.AbiesGrandis => "ABGR",
                FiaCode.AbiesProcera => "ABPR",
                FiaCode.AcerGlabrum => "ACGL",
                FiaCode.AcerMacrophyllum => "ACMA",
                FiaCode.AlnusRubra => "ALRU",
                FiaCode.ArbutusMenziesii => "ARME",
                FiaCode.CalocedrusDecurrens => "CADE",
                FiaCode.ChrysolepisChrysophyllaVarChrysophylla => "CHCH",
                FiaCode.CornusNuttallii => "CONU",
                FiaCode.NotholithocarpusDensiflorus => "LIDE",
                FiaCode.PiceaSitchensis => "PISI",
                FiaCode.PinusLambertiana => "PILA",
                FiaCode.PinusPonderosa => "PIPO",
                FiaCode.PseudotsugaMenziesii => "PSME",
                FiaCode.QuercusChrysolepis => "QUCH",
                FiaCode.QuercusGarryana => "QUGA",
                FiaCode.QuercusKelloggii => "QUKE",
                FiaCode.Salix => "Salix",
                FiaCode.TaxusBrevifolia => "TABR",
                FiaCode.ThujaPlicata => "THPL",
                FiaCode.TsugaHeterophylla => "TSHE",
                _ => throw new NotSupportedException(String.Format("Unhandled species {0}.", species)),
            };
        }
    }
}
