using Osu.Cof.Ferm.Organon;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RunParameters
    {
        public IList<float> DiscountRates { get; init; }
        public int LastThinPeriod { get; set; }
        public int MaximizeForPlanningPeriod { get; init; }
        public OrganonConfiguration OrganonConfiguration { get; private init; }
        public IList<int> RotationLengths { get; init; }
        public TimberObjective TimberObjective { get; init; }
        public TimberValue TimberValue { get; set; }
        public OrganonTreatments Treatments { get; init; }

        public RunParameters(IList<int> rotationLengths, IList<float> discountRates, OrganonConfiguration organonConfiguration)
        {
            this.DiscountRates = discountRates;
            this.LastThinPeriod = Constant.NoThinPeriod;
            this.MaximizeForPlanningPeriod = Constant.HeuristicDefault.RotationIndex;
            this.OrganonConfiguration = organonConfiguration;
            this.RotationLengths = rotationLengths;
            this.TimberObjective = TimberObjective.LandExpectationValue;
            this.TimberValue = TimberValue.Default;
            this.Treatments = new();
        }
    }
}
