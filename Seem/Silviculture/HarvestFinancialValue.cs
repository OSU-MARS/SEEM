namespace Osu.Cof.Ferm.Silviculture
{
    public class HarvestFinancialValue
    {
        public float CubicVolume { get; set; }
        public float NetPresentValue { get; set; }
        public float NetPresentValue2Saw { get; set; }
        public float NetPresentValue3Saw { get; set; }
        public float NetPresentValue4Saw { get; set; }

        public HarvestFinancialValue()
        {
            this.CubicVolume = 0.0F;
            this.NetPresentValue = 0.0F;
            this.NetPresentValue2Saw = 0.0F;
            this.NetPresentValue3Saw = 0.0F;
            this.NetPresentValue4Saw = 0.0F;
        }
    }
}
