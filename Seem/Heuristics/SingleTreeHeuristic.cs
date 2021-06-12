using Osu.Cof.Ferm.Organon;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class SingleTreeHeuristic<TParameters> : Heuristic<TParameters> where TParameters : HeuristicParameters
    {
        public SingleTreeMoveLog MoveLog { get; private init; }

        public SingleTreeHeuristic(OrganonStand stand, TParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters, false)
        {
            this.MoveLog = new SingleTreeMoveLog();
        }

        protected override float EvaluateInitialSelection(int moveCapacity, HeuristicPerformanceCounters perfCounters)
        {
            float financialValue = base.EvaluateInitialSelection(moveCapacity, perfCounters);
            
            this.MoveLog.TreeIDByMove.Capacity = moveCapacity;
            this.MoveLog.TreeIDByMove.Add(-1);
            return financialValue;
        }

        public override IHeuristicMoveLog? GetMoveLog()
        {
            return this.MoveLog;
        }
    }
}
