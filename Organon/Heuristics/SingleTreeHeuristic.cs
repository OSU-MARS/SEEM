using Osu.Cof.Ferm.Organon;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class SingleTreeHeuristic : Heuristic
    {
        public List<int> TreeIDByMove { get; protected set; }

        public SingleTreeHeuristic(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            this.TreeIDByMove = new List<int>();
        }

        protected void EvaluateInitialSelection(int moveCapacity)
        {
            this.AcceptedObjectiveFunctionByMove.Capacity = moveCapacity;
            this.CandidateObjectiveFunctionByMove.Capacity = moveCapacity;
            this.TreeIDByMove.Capacity = moveCapacity;

            this.CurrentTrajectory.Simulate();
            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
            this.BestObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
            this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
            this.CandidateObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
            this.TreeIDByMove.Add(-1);
        }
    }
}
