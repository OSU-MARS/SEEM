using Osu.Cof.Ferm.Organon;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RunParameters
    {
        public float DiscountRate { get; init; }
        public int PlanningPeriods { get; init; }
        public OrganonConfiguration OrganonConfiguration { get; private init; }
        public TimberObjective TimberObjective { get; init; }
        public TimberValue TimberValue { get; set; }
        public OrganonTreatments Treatments { get; init; }

        public RunParameters(OrganonConfiguration organonConfiguration)
        {
            this.DiscountRate = Constant.DefaultAnnualDiscountRate;
            this.OrganonConfiguration = organonConfiguration;
            this.PlanningPeriods = 1;
            this.TimberObjective = TimberObjective.LandExpectationValue;
            this.TimberValue = TimberValue.Default;
            this.Treatments = new();
        }
    }
}
