namespace Osu.Cof.Ferm.Tree
{
    public struct TreeSpeciesProperties
    {
        public float BarkDensity { get; init; } // kg/m³, green
        public float BarkFraction { get; init; }
        public float ProcessingBarkLoss { get; init; }
        public float WoodDensity { get; init; } // kg/m³, green

        public float GetStemDensity()
        {
            return (1.0F - this.BarkFraction) * this.WoodDensity + this.BarkFraction * this.BarkDensity;
        }

        public float GetStemDensityAfterProcessing()
        {
            float barkFractionAfterProcessing = (1.0F - this.ProcessingBarkLoss) * this.BarkFraction;
            return (1.0F - barkFractionAfterProcessing) * this.WoodDensity + barkFractionAfterProcessing * this.BarkDensity;
        }
    }
}
