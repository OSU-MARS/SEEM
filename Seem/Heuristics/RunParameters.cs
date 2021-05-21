namespace Osu.Cof.Ferm.Heuristics
{
    public class RunParameters
    {
        public float DiscountRate { get; init; }
        public int PlanningPeriods { get; init; }
        public TimberObjective TimberObjective { get; init; }

        public RunParameters()
        {
            this.DiscountRate = Constant.DefaultAnnualDiscountRate;
            this.PlanningPeriods = 1;
            this.TimberObjective = TimberObjective.LandExpectationValue;
        }
    }
}
