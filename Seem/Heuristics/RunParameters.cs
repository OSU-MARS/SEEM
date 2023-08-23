using Mars.Seem.Optimization;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System.Collections.Generic;

namespace Mars.Seem.Heuristics
{
    public class RunParameters
    {
        public FinancialScenarios Financial { get; set; }
        public int LastThinPeriod { get; set; }
        public int MaximizeForPlanningPeriod { get; init; }
        public int MoveCapacity { get; init; }
        public bool LogOnlyImprovingMoves { get; init; }
        public OrganonConfiguration OrganonConfiguration { get; private init; }
        public IList<int> RotationLengths { get; init; }
        public TimberObjective TimberObjective { get; init; }
        public OrganonTreatments Treatments { get; init; }
        public TreeScaling TreeVolume { get; init; }

        public RunParameters(IList<int> rotationLengthInPeriods, OrganonConfiguration organonConfiguration)
        {
            this.Financial = FinancialScenarios.Default;
            this.LastThinPeriod = Constant.NoHarvestPeriod;
            this.MaximizeForPlanningPeriod = Constant.HeuristicDefault.CoordinateIndex;
            this.MoveCapacity = Constant.HeuristicDefault.MoveCapacity;
            this.LogOnlyImprovingMoves = Constant.HeuristicDefault.LogOnlyImprovingMoves;
            this.OrganonConfiguration = organonConfiguration;
            this.RotationLengths = rotationLengthInPeriods;
            this.TimberObjective = TimberObjective.LandExpectationValue;
            this.Treatments = new();
            this.TreeVolume = TreeScaling.Default;
        }
    }
}
