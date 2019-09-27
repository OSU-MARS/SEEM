using System;

namespace Osu.Cof.Organon.Test
{
    public class OrganonCapabilities
    {
        public int SpeciesGroupCount { get; private set; }
        public int TimeStepInYears { get; private set; }

        public OrganonCapabilities(Variant variant)
        {
            switch (variant)
            {
                case Variant.Nwo:
                case Variant.Smc:
                    this.SpeciesGroupCount = TestConstant.NwoSmcSpeciesCodes.Count;
                    this.TimeStepInYears = 5;
                    break;
                case Variant.Rap:
                    this.SpeciesGroupCount = TestConstant.RapSpeciesCodes.Count;
                    this.TimeStepInYears = 1;
                    break;
                case Variant.Swo:
                    this.SpeciesGroupCount = TestConstant.SwoSpeciesCodes.Count - 1;
                    this.TimeStepInYears = 5;
                    break;
                default:
                    throw new NotSupportedException(String.Format("Unhandled variant {0}.", variant));
            }
        }
    }
}
