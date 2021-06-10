using Osu.Cof.Ferm.Organon;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RunParameters
    {
        public List<float> DiscountRates { get; init; }
        public int LastPlanningPeriod { get; init; }
        public int MaximizeForPlanningPeriod { get; init; }
        public OrganonConfiguration OrganonConfiguration { get; private init; }
        public TimberObjective TimberObjective { get; init; }
        public TimberValue TimberValue { get; set; }
        public OrganonTreatments Treatments { get; init; }

        public RunParameters(List<float> discountRates, OrganonConfiguration organonConfiguration)
        {
            this.DiscountRates = discountRates;
            this.LastPlanningPeriod = 1;
            this.MaximizeForPlanningPeriod = 1;
            this.OrganonConfiguration = organonConfiguration;
            this.TimberObjective = TimberObjective.LandExpectationValue;
            this.TimberValue = TimberValue.Default;
            this.Treatments = new();
        }
    }
}
