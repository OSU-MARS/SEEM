namespace Osu.Cof.Ferm.Tree
{
    public struct TreeSpeciesProperties
    {
        public float BarkDensity { get; init; } // kg/m³, green
        public float BarkFraction { get; init; }
        public float BarkFractionRemainingAfterProcessing { get; init; }
        public float WoodDensity { get; init; } // kg/m³, green

        public float GetStemDensity()
        {
            return (1.0F - this.BarkFraction) * this.WoodDensity + this.BarkFraction * this.BarkDensity;
        }

        public float GetStemDensityAfterProcessing()
        {
            return (1.0F - this.BarkFractionRemainingAfterProcessing) * this.WoodDensity + this.BarkFractionRemainingAfterProcessing * this.BarkDensity;
        }
    }
}
