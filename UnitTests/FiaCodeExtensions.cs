using System;

namespace Osu.Cof.Organon.Test
{
    internal static class FiaCodeExtensions
    {
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
                FiaCode.LithocarpusDensiflorus => "LIDE",
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
