namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicSolutionPosition
    {
        public int DiscountRateIndex { get; init; }
        public int FirstThinPeriodIndex { get; init; }
        public int PlanningPeriodIndex { get; init; }
        public int SecondThinPeriodIndex { get; init; }
        public int ThirdThinPeriodIndex { get; init; }

        public HeuristicSolutionPosition()
        {
            this.DiscountRateIndex = -1;
            this.FirstThinPeriodIndex = Constant.NoThinPeriod;
            this.SecondThinPeriodIndex = Constant.NoThinPeriod;
            this.ThirdThinPeriodIndex = Constant.NoThinPeriod;
            this.PlanningPeriodIndex = -1;
        }
    }
}
