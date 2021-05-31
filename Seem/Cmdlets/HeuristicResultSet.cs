using Osu.Cof.Ferm.Heuristics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class HeuristicResultSet
    {
        public List<HeuristicDistribution> Distributions { get; private init; }
        public HeuristicSolutionIndex SolutionIndex { get; private init; }
        public List<HeuristicSolutionPool> Solutions { get; private init; }

        public List<float> DiscountRates { get; private init; }
        public List<int> FirstThinPeriod { get; private init; }
        public List<int> PlanningPeriods { get; private init; }
        public List<int> SecondThinPeriod { get; private init; }
        public List<int> ThirdThinPeriod { get; private init; }

        public HeuristicResultSet(List<float> discountRates, List<int> firstThinPeriod, List<int> secondThinPeriod, List<int> thirdThinPeriod, List<int> planningPeriods)
        {
            this.Distributions = new();
            this.SolutionIndex = new(discountRates, firstThinPeriod, secondThinPeriod, thirdThinPeriod, planningPeriods);
            this.Solutions = new();

            this.DiscountRates = discountRates;
            this.FirstThinPeriod = firstThinPeriod;
            this.PlanningPeriods = planningPeriods;
            this.SecondThinPeriod = secondThinPeriod;
            this.ThirdThinPeriod = thirdThinPeriod;
        }

        public int Count
        {
            get 
            {
                Debug.Assert(this.Distributions.Count == this.Solutions.Count);
                return this.Distributions.Count; 
            }
        }

        public void Add(HeuristicDistribution distribution)
        {
            this.Distributions.Add(distribution);

            HeuristicSolutionPool solution = this.SolutionIndex[distribution];
            this.Solutions.Add(solution);
        }
    }
}
