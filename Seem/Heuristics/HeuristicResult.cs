namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicResult
    {
        public HeuristicObjectiveDistribution Distribution { get; set; }
        public HeuristicSolutionPool Pool { get; set; }

        public HeuristicResult(int poolCapacity)
        {
            this.Distribution = new HeuristicObjectiveDistribution();
            this.Pool = new HeuristicSolutionPool(poolCapacity);
        }
    }
}
