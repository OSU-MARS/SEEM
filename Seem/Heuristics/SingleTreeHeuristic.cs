using Osu.Cof.Ferm.Organon;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class SingleTreeHeuristic<TParameters> : Heuristic<TParameters> where TParameters : HeuristicParameters
    {
        public SingleTreeMoveLog MoveLog { get; private init; }

        public SingleTreeHeuristic(OrganonStand stand, TParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters, false)
        {
            this.MoveLog = new SingleTreeMoveLog(runParameters.MoveCapacity);
        }

        protected override float EvaluateInitialSelection(HeuristicResultPosition position, int moveCapacity, HeuristicPerformanceCounters perfCounters)
        {
            float financialValue = base.EvaluateInitialSelection(position, moveCapacity, perfCounters);
            
            this.MoveLog.TreeIDByMove.Capacity = moveCapacity;
            this.MoveLog.TryAddMove(-1);
            return financialValue;
        }

        public override HeuristicMoveLog? GetMoveLog()
        {
            return this.MoveLog;
        }
    }
}
