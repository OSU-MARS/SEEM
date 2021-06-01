using Osu.Cof.Ferm.Organon;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class SingleTreeHeuristic<TParameters> : Heuristic<TParameters> where TParameters : HeuristicParameters
    {
        public SingleTreeMoveLog MoveLog { get; private init; }

        public SingleTreeHeuristic(OrganonStand stand, TParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters)
        {
            this.MoveLog = new SingleTreeMoveLog();
        }

        protected override void EvaluateInitialSelection(int moveCapacity, HeuristicPerformanceCounters perfCounters)
        {
            base.EvaluateInitialSelection(moveCapacity, perfCounters);
            
            this.MoveLog.TreeIDByMove.Capacity = moveCapacity;
            this.MoveLog.TreeIDByMove.Add(-1);
        }

        public override IHeuristicMoveLog GetMoveLog()
        {
            return this.MoveLog;
        }
    }
}
