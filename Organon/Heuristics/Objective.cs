namespace Osu.Cof.Ferm.Heuristics
{
    public class Objective
    {
        public HarvestPeriodSelection HarvestPeriodSelection { get; set; }
        public bool IsLandExpectationValue { get; set; }
        public int PlanningPeriods { get; set; }

        public Objective()
        {
            this.HarvestPeriodSelection = HarvestPeriodSelection.NoneOrLast;
            this.IsLandExpectationValue = false;
            this.PlanningPeriods = 1;
        }
    }
}
