using System;

namespace Osu.Cof.Organon
{
    public class OrganonVariant
    {
        public int TimeStepInYears { get; private set; }
        public Variant Variant { get; private set; }

        public OrganonVariant(Variant variant)
        {
            this.TimeStepInYears = variant == Variant.Rap ? 1 : 5;
            this.Variant = variant;
        }

        public static NotSupportedException CreateUnhandledSpeciesException(FiaCode species)
        {
            return new NotSupportedException(String.Format("Unhandled species {0}.", species));
        }

        public static NotSupportedException CreateUnhandledVariantException(Variant variant)
        {
            return new NotSupportedException(String.Format("Unhandled Organon variant {0}.", variant));
        }

        public int GetEndYear(int simulationStep)
        {
            if (this.Variant == Variant.Rap)
            {
                return simulationStep + 1;
            }
            return Constant.DefaultTimeStepInYears * (simulationStep + 1);
        }

        public bool IsBigSixSpecies(FiaCode species)
        {
            switch (this.Variant)
            {
                case Variant.Nwo:
                    return (species == FiaCode.PseudotsugaMenziesii) || (species == FiaCode.AbiesGrandis) ||
                           (species == FiaCode.TsugaHeterophylla);
                case Variant.Smc:
                    return (species == FiaCode.PseudotsugaMenziesii) || (species == FiaCode.AbiesConcolor) ||
                           (species == FiaCode.AbiesGrandis) || (species == FiaCode.TsugaHeterophylla);
                case Variant.Rap:
                    return (species == FiaCode.AlnusRubra) || (species == FiaCode.PseudotsugaMenziesii) ||
                           (species == FiaCode.TsugaHeterophylla);
                case Variant.Swo:
                    return (species == FiaCode.AbiesConcolor) || (species == FiaCode.AbiesGrandis) ||
                           (species == FiaCode.CalocedrusDecurrens) || (species == FiaCode.PinusLambertiana) ||
                           (species == FiaCode.PinusPonderosa) || (species == FiaCode.PseudotsugaMenziesii);
                default:
                    throw Organon.OrganonVariant.CreateUnhandledVariantException(this.Variant);
            }
        }

        public bool IsSpeciesSupported(FiaCode species)
        {
            switch (this.Variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    return Constant.NwoSmcSpecies.Contains(species);
                case Variant.Rap:
                    return Constant.RapSpecies.Contains(species);
                case Variant.Swo:
                    return Constant.SwoSpecies.Contains(species);
                default:
                    throw Organon.OrganonVariant.CreateUnhandledVariantException(this.Variant);
            }
        }
    }
}
