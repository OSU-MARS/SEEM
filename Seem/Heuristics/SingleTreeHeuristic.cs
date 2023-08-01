using Mars.Seem.Organon;
using Mars.Seem.Silviculture;

namespace Mars.Seem.Heuristics
{
    public abstract class SingleTreeHeuristic<TParameters> : Heuristic<TParameters> where TParameters : HeuristicParameters
    {
        public SingleTreeMoveLog MoveLog { get; private init; }

        public SingleTreeHeuristic(OrganonStand stand, TParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters, evaluatesAcrossRotationsAndFinancialScenarios: false)
        {
            this.MoveLog = new SingleTreeMoveLog(runParameters.MoveCapacity);
        }

        protected override float EvaluateInitialSelection(SilviculturalCoordinate coordinate, int moveCapacity, PrescriptionPerformanceCounters perfCounters)
        {
            float initialFinancialValue = base.EvaluateInitialSelection(coordinate, moveCapacity, perfCounters);
            
            this.MoveLog.TreeIDByMove.Capacity = moveCapacity;
            this.MoveLog.TryAddMove(-1);
            return initialFinancialValue;
        }

        public override HeuristicMoveLog? GetMoveLog()
        {
            return this.MoveLog;
        }
    }
}
