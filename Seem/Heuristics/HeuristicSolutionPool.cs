namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicSolutionPool
    {
        public Heuristic? Lowest { get; private set; }
        public Heuristic? Highest { get; private set; }

        public HeuristicSolutionPool()
        {
            this.Lowest = null;
            this.Highest = null;
        }

        public void AddRun(Heuristic heuristic)
        {
            if ((this.Highest == null) || (heuristic.BestObjectiveFunction > this.Highest.BestObjectiveFunction))
            {
                this.Highest = heuristic;
            }
            if ((this.Lowest == null) || (heuristic.BestObjectiveFunction < this.Lowest.BestObjectiveFunction))
            {
                this.Lowest = heuristic;
            }
        }
    }
}
