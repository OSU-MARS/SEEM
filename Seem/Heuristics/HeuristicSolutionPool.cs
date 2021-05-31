namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicSolutionPool
    {
        public Heuristic? Low { get; private set; }
        public Heuristic? High { get; private set; }

        public HeuristicSolutionPool()
        {
            this.Low = null;
            this.High = null;
        }

        public void AddRun(Heuristic heuristic)
        {
            if ((this.High == null) || (heuristic.BestObjectiveFunction > this.High.BestObjectiveFunction))
            {
                this.High = heuristic;
            }
            if ((this.Low == null) || (heuristic.BestObjectiveFunction < this.Low.BestObjectiveFunction))
            {
                this.Low = heuristic;
            }
        }
    }
}
