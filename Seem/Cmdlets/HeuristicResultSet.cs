using Osu.Cof.Ferm.Heuristics;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Cmdlets
{
    // provide a non-template base class to allow PowerShell cmdlets to accept results from any heuristic as a parameter
    public class HeuristicResultSet
    {
        public IList<HeuristicObjectiveDistribution> Distributions { get; private init; }
        public HeuristicSolutionIndex SolutionIndex { get; private init; }

        public IList<float> DiscountRates { get; private init; }
        public IList<int> FirstThinPeriod { get; private init; }
        public IList<int> PlanningPeriods { get; private init; }
        public IList<int> SecondThinPeriod { get; private init; }
        public IList<int> ThirdThinPeriod { get; private init; }

        protected HeuristicResultSet(int parameterCombinations, IList<float> discountRates, IList<int> firstThinPeriod, IList<int> secondThinPeriod, IList<int> thirdThinPeriod, IList<int> planningPeriods, int individualSolutionPoolSize)
        {
            this.Distributions = new List<HeuristicObjectiveDistribution>();
            this.SolutionIndex = new(parameterCombinations, discountRates.Count, firstThinPeriod.Count, secondThinPeriod.Count, thirdThinPeriod.Count, planningPeriods.Count, individualSolutionPoolSize);

            this.DiscountRates = discountRates;
            this.FirstThinPeriod = firstThinPeriod;
            this.PlanningPeriods = planningPeriods;
            this.SecondThinPeriod = secondThinPeriod;
            this.ThirdThinPeriod = thirdThinPeriod;
        }
    }

    public class HeuristicResultSet<TParameters> : HeuristicResultSet where TParameters : HeuristicParameters
    {
        public IList<TParameters> ParameterCombinations { get; private init; }

        public HeuristicResultSet(IList<TParameters> parameterCombinations, IList<float> discountRates, IList<int> firstThinPeriod, IList<int> secondThinPeriod, IList<int> thirdThinPeriod, IList<int> planningPeriods, int individualSolutionPoolSize)
            : base(parameterCombinations.Count, discountRates, firstThinPeriod, secondThinPeriod, thirdThinPeriod, planningPeriods, individualSolutionPoolSize)
        {
            this.ParameterCombinations = parameterCombinations;
        }
    }
}
