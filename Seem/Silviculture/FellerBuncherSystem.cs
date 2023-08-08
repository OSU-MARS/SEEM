using System;

namespace Mars.Seem.Silviculture
{
    public class FellerBuncherSystem : FallingSystem
    {
        public float SystemCostPerHa { get; set; } // US$/ha

        public FellerBuncherSystem()
        {
            this.SystemCostPerHa = Single.NaN;
        }

        public new void Clear()
        {
            base.Clear();

            this.SystemCostPerHa = Single.NaN;
        }
    }
}
