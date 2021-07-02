namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicResultPosition
    {
        public int FinancialIndex { get; set; }
        public int FirstThinPeriodIndex { get; init; }
        public int ParameterIndex { get; init; }
        public int RotationIndex { get; set; }
        public int SecondThinPeriodIndex { get; init; }
        public int ThirdThinPeriodIndex { get; init; }

        public HeuristicResultPosition()
        {
            this.FinancialIndex = -1;
            this.FirstThinPeriodIndex = Constant.NoThinPeriod;
            this.SecondThinPeriodIndex = Constant.NoThinPeriod;
            this.ThirdThinPeriodIndex = Constant.NoThinPeriod;
            this.ParameterIndex = -1;
            this.RotationIndex = -1;
        }

        public HeuristicResultPosition(HeuristicResultPosition other)
        {
            this.FinancialIndex = other.FinancialIndex;
            this.FirstThinPeriodIndex = other.FirstThinPeriodIndex;
            this.SecondThinPeriodIndex = other.SecondThinPeriodIndex;
            this.ThirdThinPeriodIndex = other.ThirdThinPeriodIndex;
            this.ParameterIndex = other.ParameterIndex;
            this.RotationIndex = other.RotationIndex;
        }
    }
}
