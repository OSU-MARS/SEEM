namespace Mars.Seem.Silviculture
{
    public class HarvestFinancialValue
    {
        public float CubicVolumePerHa { get; set; } // merchantable m³/ha
        public float MerchantableCubicVolumePerHa { get; protected set; } // m³/ha
        public float NetPresentValuePerHa { get; set; } // US$/ha
        public float PondValue2SawPerHa { get; set; } // US$/ha
        public float PondValue3SawPerHa { get; set; } // US$/ha
        public float PondValue4SawPerHa { get; set; } // US$/ha
        public float HarvestRelatedTaskCostPerHa { get; set; } // US$/ha

        protected HarvestFinancialValue()
        {
            this.Clear();
        }

        protected void Clear()
        {
            this.CubicVolumePerHa = 0.0F;
            this.MerchantableCubicVolumePerHa = 0.0F;
            this.NetPresentValuePerHa = 0.0F;
            this.PondValue2SawPerHa = 0.0F;
            this.PondValue3SawPerHa = 0.0F;
            this.PondValue4SawPerHa = 0.0F;
            this.HarvestRelatedTaskCostPerHa = 0.0F;
        }
    }
}
