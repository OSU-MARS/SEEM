namespace Osu.Cof.Organon.Test
{
    public class VariantCapabilities
    {
        public int SpeciesGroupCount { get; private set; }
        public int TimeStepInYears { get; private set; }
        public Variant Variant { get; private set; }

        public VariantCapabilities(Variant variant)
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
                    throw VariantExtensions.CreateUnhandledVariantException(variant);
            }
            this.Variant = variant;
        }
    }
}
