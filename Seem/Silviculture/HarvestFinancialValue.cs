namespace Osu.Cof.Ferm.Silviculture
{
    public class HarvestFinancialValue
    {
        public float CubicVolumePerHa { get; set; } // merchantable m³/ha
        public float NetPresentValuePerHa { get; set; } // US$/ha
        public float PondValue2SawPerHa { get; set; } // US$/ha
        public float PondValue3SawPerHa { get; set; } // US$/ha
        public float PondValue4SawPerHa { get; set; } // US$/ha
        public float TaskCostPerHa { get; set; } // US$/ha

        protected HarvestFinancialValue()
        {
            this.CubicVolumePerHa = 0.0F;
            this.NetPresentValuePerHa = 0.0F;
            this.PondValue2SawPerHa = 0.0F;
            this.PondValue3SawPerHa = 0.0F;
            this.PondValue4SawPerHa = 0.0F;
            this.TaskCostPerHa = 0.0F;
        }
    }
}
