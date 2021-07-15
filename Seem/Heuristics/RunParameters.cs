using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Tree;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RunParameters
    {
        public FinancialScenarios Financial { get; set; }
        public int LastThinPeriod { get; set; }
        public int MaximizeForPlanningPeriod { get; init; }
        public OrganonConfiguration OrganonConfiguration { get; private init; }
        public IList<int> RotationLengths { get; init; }
        public TimberObjective TimberObjective { get; init; }
        public OrganonTreatments Treatments { get; init; }
        public TreeVolume TreeVolume { get; init; }

        public RunParameters(IList<int> rotationLengths, OrganonConfiguration organonConfiguration)
        {
            this.Financial = FinancialScenarios.Default;
            this.LastThinPeriod = Constant.NoThinPeriod;
            this.MaximizeForPlanningPeriod = Constant.HeuristicDefault.RotationIndex;
            this.OrganonConfiguration = organonConfiguration;
            this.RotationLengths = rotationLengths;
            this.TimberObjective = TimberObjective.LandExpectationValue;
            this.Treatments = new();
            this.TreeVolume = TreeVolume.Default;
        }
    }
}
