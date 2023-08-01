using Mars.Seem.Tree;
using System;

namespace Mars.Seem.Extensions
{
    internal static class FiaCodeExtensions
    {
        public static FiaCode Parse(string twoOrFourLetterCode)
        {
            if (String.IsNullOrWhiteSpace(twoOrFourLetterCode))
            {
                throw new ArgumentOutOfRangeException(nameof(twoOrFourLetterCode));
            }

            if (twoOrFourLetterCode.Length == 2)
            {
                return twoOrFourLetterCode switch
                {
                    // common name abbreviations
                    //"BC" => FiaCode.PopulusTrichocarpa,
                    "BM" => FiaCode.AcerMacrophyllum,
                    //"CA" => FiaCode.FrangulaPurshiana,
                    //"CH" => FiaCode.Prunus,
                    "DF" => FiaCode.PseudotsugaMenziesii,
                    "GC" => FiaCode.ChrysolepisChrysophyllaVarChrysophylla,
                    "GF" => FiaCode.AbiesGrandis,
                    "IC" => FiaCode.CalocedrusDecurrens,
                    //"LP" => FiaCode.PinusContorta,
                    //"OA" => FiaCode.FraxinusLatifolia,
                    //"OM" => FiaCode.UmbelulariaCalifornica,
                    //"PC" => FiaCode.ChamaecyparisLawsoniana,
                    "PD" => FiaCode.CornusNuttallii,
                    "PM" => FiaCode.ArbutusMenziesii,
                    "PY" => FiaCode.TaxusBrevifolia,
                    "RA" => FiaCode.AlnusRubra,
                    "RC" => FiaCode.ThujaPlicata,
                    "SS" => FiaCode.PiceaSitchensis,
                    "TO" => FiaCode.NotholithocarpusDensiflorus,
                    "WF" => FiaCode.AbiesConcolor,
                    "WH" => FiaCode.TsugaHeterophylla,
                    "WI" => FiaCode.Salix,
                    "WO" => FiaCode.QuercusGarryana,
                    // reversed common name abbreviations (used in British Columbia)
                    "CW" => FiaCode.ThujaPlicata,
                    "DR" => FiaCode.AlnusRubra,
                    "FD" => FiaCode.PseudotsugaMenziesii,
                    "MB" => FiaCode.AcerMacrophyllum,
                    "HW" => FiaCode.TsugaHeterophylla,
                    _ => throw new NotSupportedException(String.Format("Unhandled species '{0}'.", twoOrFourLetterCode))
                };
            }
            else if (twoOrFourLetterCode.Length == 4)
            {
                return twoOrFourLetterCode switch
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
                    _ => throw new NotSupportedException(String.Format("Unhandled species '{0}'.", twoOrFourLetterCode))
                };
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(twoOrFourLetterCode));
            }
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
