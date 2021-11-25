using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Tree;

namespace Osu.Cof.Ferm.Silviculture
{
    public class StandTrajectoryAndObjectives
    {
        public float FinancialValue { get; set; }
        public Heuristic? Heuristic { get; set; }
        public StandTrajectory? Trajectory { get; set; }

        public StandTrajectoryAndObjectives(float defaultFinancialValue)
        {
            this.FinancialValue = defaultFinancialValue;
            this.Heuristic = null;
            this.Trajectory = null;
        }

    }
}
