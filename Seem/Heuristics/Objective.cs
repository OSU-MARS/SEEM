namespace Osu.Cof.Ferm.Heuristics
{
    public class Objective
    {
        public HarvestPeriodSelection HarvestPeriodSelection { get; set; }
        public int PlanningPeriods { get; set; }
        public TimberObjective TimberObjective { get; set; }

        public Objective()
        {
            this.HarvestPeriodSelection = HarvestPeriodSelection.ThinPeriodOrRetain;
            this.PlanningPeriods = 1;
            this.TimberObjective = TimberObjective.LandExpectationValue;
        }
    }
}
