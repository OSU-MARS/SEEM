namespace Osu.Cof.Ferm.Heuristics
{
    public class Objective
    {
        public int PlanningPeriods { get; set; }
        public TimberObjective TimberObjective { get; set; }

        public Objective()
        {
            this.PlanningPeriods = 1;
            this.TimberObjective = TimberObjective.LandExpectationValue;
        }
    }
}
