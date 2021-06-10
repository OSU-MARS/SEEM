namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicResultPosition
    {
        public int DiscountRateIndex { get; init; }
        public int FirstThinPeriodIndex { get; init; }
        public int ParameterIndex { get; init; }
        public int PlanningPeriodIndex { get; init; }
        public int SecondThinPeriodIndex { get; init; }
        public int ThirdThinPeriodIndex { get; init; }

        public HeuristicResultPosition()
        {
            this.DiscountRateIndex = -1;
            this.FirstThinPeriodIndex = Constant.NoThinPeriod;
            this.SecondThinPeriodIndex = Constant.NoThinPeriod;
            this.ThirdThinPeriodIndex = Constant.NoThinPeriod;
            this.ParameterIndex = -1;
            this.PlanningPeriodIndex = -1;
        }

        public HeuristicResultPosition(HeuristicResultPosition other)
        {
            this.DiscountRateIndex = other.DiscountRateIndex;
            this.FirstThinPeriodIndex = other.FirstThinPeriodIndex;
            this.SecondThinPeriodIndex = other.SecondThinPeriodIndex;
            this.ThirdThinPeriodIndex = other.ThirdThinPeriodIndex;
            this.ParameterIndex = other.ParameterIndex;
            this.PlanningPeriodIndex = other.PlanningPeriodIndex;
        }
    }
}
