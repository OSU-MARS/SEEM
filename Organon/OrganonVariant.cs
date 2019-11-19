using System;

namespace Osu.Cof.Organon
{
    public class OrganonVariant
    {
        public int SpeciesGroupCount { get; private set; }
        public int TimeStepInYears { get; private set; }
        public Variant Variant { get; private set; }

        public OrganonVariant(Variant variant)
        {
            switch (variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    this.SpeciesGroupCount = Constant.NwoSmcSpecies.Count;
                    this.TimeStepInYears = 5;
                    break;
                case Variant.Rap:
                    this.SpeciesGroupCount = Constant.RapSpecies.Count;
                    this.TimeStepInYears = 1;
                    break;
                case Variant.Swo:
                    this.SpeciesGroupCount = Constant.SwoSpecies.Count - 1;
                    this.TimeStepInYears = 5;
                    break;
                default:
                    throw OrganonVariant.CreateUnhandledVariantException(variant);
            }
            this.Variant = variant;
        }

        public static NotSupportedException CreateUnhandledVariantException(Variant variant)
        {
            return new NotSupportedException(String.Format("Unhandled Organon variant {0}.", variant));
        }

        public int GetSpeciesGroup(FiaCode species)
        {
            switch (this.Variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    return Constant.NwoSmcSpecies.IndexOf(species);
                case Variant.Rap:
                    return Constant.RapSpecies.IndexOf(species);
                case Variant.Swo:
                    int speciesGroup = Constant.SwoSpecies.IndexOf(species);
                    if (speciesGroup > 1)
                    {
                        --speciesGroup;
                    }
                    return speciesGroup;
                default:
                    throw Organon.OrganonVariant.CreateUnhandledVariantException(this.Variant);
            }
        }

        public int GetEndYear(int simulationStep)
        {
            if (this.Variant == Variant.Rap)
            {
                return simulationStep + 1;
            }
            return Constant.DefaultTimeStepInYears * (simulationStep + 1);
        }

        public bool IsSpeciesSupported(FiaCode speciesCode)
        {
            int speciesGroup = this.GetSpeciesGroup(speciesCode);
            return speciesGroup >= 0;
        }
    }
}
