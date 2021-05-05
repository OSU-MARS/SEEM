namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicSolutionIndex
    {
        // indices are: discount rate, first thin, second thin, third thin, planning periods
        private readonly HeuristicSolutionPool[][][][][] solutionsByIndices;

        public HeuristicSolutionIndex(int discountRates, int firstThins, int secondThins, int thirdThins, int planningPeriods)
        {
            this.solutionsByIndices = new HeuristicSolutionPool[discountRates][][][][];
            for (int discountRateIndex = 0; discountRateIndex < discountRates; ++discountRateIndex)
            {
                HeuristicSolutionPool[][][][] discountRatePool = new HeuristicSolutionPool[firstThins][][][];
                this.solutionsByIndices[discountRateIndex] = discountRatePool;
                for (int firstThinIndex = 0; firstThinIndex < firstThins; ++firstThinIndex)
                {
                    HeuristicSolutionPool[][][] firstThinPool = new HeuristicSolutionPool[secondThins][][];
                    discountRatePool[firstThinIndex] = firstThinPool;
                    for (int secondThinIndex = 0; secondThinIndex < secondThins; ++secondThinIndex)
                    {
                        HeuristicSolutionPool[][] secondThinPool = new HeuristicSolutionPool[thirdThins][];
                        firstThinPool[secondThinIndex] = secondThinPool;
                        for (int thirdThinIndex = 0; thirdThinIndex < thirdThins; ++thirdThinIndex)
                        {
                            HeuristicSolutionPool[] thirdThinPool = new HeuristicSolutionPool[planningPeriods];
                            secondThinPool[thirdThinIndex] = thirdThinPool;
                        }
                    }
                }
            }
        }

        public HeuristicSolutionPool this[HeuristicSolutionPosition position]
        {
            get { return this.solutionsByIndices[position.DiscountRateIndex][position.FirstThinPeriodIndex][position.SecondThinPeriodIndex][position.ThirdThinPeriodIndex][position.PlanningPeriodIndex]; }
        }

        public void Add(HeuristicSolutionPosition position, HeuristicSolutionPool value)
        {
            this.solutionsByIndices[position.DiscountRateIndex][position.FirstThinPeriodIndex][position.SecondThinPeriodIndex][position.ThirdThinPeriodIndex][position.PlanningPeriodIndex] = value;
        }
    }
}
