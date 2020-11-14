using Osu.Cof.Ferm.Organon;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class SingleTreeHeuristic : Heuristic
    {
        public SingleTreeMoveLog MoveLog { get; private init; }

        public SingleTreeHeuristic(OrganonStand stand, OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
            : base(stand, organonConfiguration, objective, parameters)
        {
            this.MoveLog = new SingleTreeMoveLog();
        }

        protected void EvaluateInitialSelection(int moveCapacity)
        {
            this.AcceptedObjectiveFunctionByMove.Capacity = moveCapacity;
            this.CandidateObjectiveFunctionByMove.Capacity = moveCapacity;
            this.MoveLog.TreeIDByMove.Capacity = moveCapacity;

            this.CurrentTrajectory.Simulate();
            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
            this.BestObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
            this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
            this.CandidateObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
            this.MoveLog.TreeIDByMove.Add(-1);
        }

        public override IHeuristicMoveLog GetMoveLog()
        {
            return this.MoveLog;
        }
    }
}
