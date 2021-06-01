using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicSolutionIndex
    {
        private readonly int discountRates;
        private readonly int planningPeriods;

        // indices are: parameter combination, discount rate, first thin, second thin, third thin, planning periods
        private readonly HeuristicSolutionPool[][][][][][] solutionsByIndices;

        public HeuristicSolutionIndex(int parameterCombinations, int discountRates, int firstThins, int secondThins, int thirdThins, int planningPeriods, int individualPoolSize)
        {
            this.discountRates = discountRates;
            this.planningPeriods = planningPeriods;
            this.solutionsByIndices = new HeuristicSolutionPool[parameterCombinations][][][][][];

            // create main set of solution pools and allocate unthinned solution pool arrays
            for (int parameterIndex = 0; parameterIndex < parameterCombinations; ++parameterIndex)
            {
                HeuristicSolutionPool[][][][][] parameterCombinationPools = new HeuristicSolutionPool[discountRates][][][][];
                this.solutionsByIndices[parameterIndex] = parameterCombinationPools;

                for (int discountRateIndex = 0; discountRateIndex < discountRates; ++discountRateIndex)
                {
                    HeuristicSolutionPool[][][][] discountRatePools = new HeuristicSolutionPool[firstThins][][][];
                    parameterCombinationPools[discountRateIndex] = discountRatePools;

                    for (int firstThinIndex = 0; firstThinIndex < firstThins; ++firstThinIndex)
                    {
                        HeuristicSolutionPool[][][] firstThinPools = new HeuristicSolutionPool[secondThins][][];
                        discountRatePools[firstThinIndex] = firstThinPools;

                        for (int secondThinIndex = 0; secondThinIndex < secondThins; ++secondThinIndex)
                        {
                            HeuristicSolutionPool[][] secondThinPools = new HeuristicSolutionPool[thirdThins][];
                            firstThinPools[secondThinIndex] = secondThinPools;

                            for (int thirdThinIndex = 0; thirdThinIndex < thirdThins; ++thirdThinIndex)
                            {
                                HeuristicSolutionPool[] thirdThinPools = new HeuristicSolutionPool[planningPeriods];
                                secondThinPools[thirdThinIndex] = thirdThinPools;

                                for (int planningPeriodIndex = 0; planningPeriodIndex < planningPeriods; ++planningPeriodIndex)
                                {
                                    // since an array of reference types with ostensibly non-null elements is initialized with nulls, ensure pools are present
                                    // at all positions to avoid downstream failures with nullability checking
                                    this[parameterIndex, discountRateIndex, firstThinIndex, secondThinIndex, thirdThinIndex, planningPeriodIndex] = new HeuristicSolutionPool(individualPoolSize);
                                }
                            }
                        }
                    }
                }
            }
        }

        public HeuristicSolutionPool this[HeuristicSolutionPosition position]
        {
            get { return this[position.ParameterIndex, position.DiscountRateIndex, position.FirstThinPeriodIndex, position.SecondThinPeriodIndex, position.ThirdThinPeriodIndex, position.PlanningPeriodIndex]; }
        }

        private HeuristicSolutionPool this[int parameterIndex, int discountRateIndex, int firstThinPeriodIndex, int secondThinPeriodIndex, int thirdThinPeriodIndex, int planningPeriodIndex]
        {
            get { return this.solutionsByIndices[parameterIndex][discountRateIndex][firstThinPeriodIndex][secondThinPeriodIndex][thirdThinPeriodIndex][planningPeriodIndex]; }
            set { this.solutionsByIndices[parameterIndex][discountRateIndex][firstThinPeriodIndex][secondThinPeriodIndex][thirdThinPeriodIndex][planningPeriodIndex] = value; }
        }

        // searches among discount rates for solutions assigned to the same set of heuristic parameters, thinnings, and rotation length
        private bool TryFindMatchingStandEntry(HeuristicSolutionPosition position, int planningPeriodIndex, [NotNullWhen(true)] out HeuristicSolutionPool? solutions)
        {
            int maxDiscountRateOffset = Math.Max(position.DiscountRateIndex, this.discountRates - position.DiscountRateIndex);
            for (int discountRateOffset = 0; discountRateOffset <= maxDiscountRateOffset; ++discountRateOffset)
            {
                // for now, arbitrarily define the next following discount rate as closer than the previous discount rate
                // If needed, this can be made intelligent enough to look at the actual discount rates.
                int discountRateIndex = position.DiscountRateIndex + discountRateOffset;
                if (discountRateIndex < this.discountRates)
                {
                    solutions = this[position.ParameterIndex, discountRateIndex, position.FirstThinPeriodIndex, position.SecondThinPeriodIndex, position.ThirdThinPeriodIndex, planningPeriodIndex];
                    if (solutions.SolutionsInPool > 0)
                    {
                        return true;
                    }

                    if (discountRateOffset > 0)
                    {
                        discountRateIndex = position.DiscountRateIndex - discountRateOffset;
                        if (discountRateIndex >= 0)
                        {
                            solutions = this[position.ParameterIndex, discountRateIndex, position.FirstThinPeriodIndex, position.SecondThinPeriodIndex, position.ThirdThinPeriodIndex, planningPeriodIndex];
                            if (solutions.SolutionsInPool > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            solutions = null;
            return false;
        }

        // searches among discount rates and rotation lengths for solutions assigned to the same set of heuristic parameters and thinnings
        // Looking across parameters would confound heuristic parameter tuning runs by allowing later parameter sets to access solutions obtained
        // by earlier runs. It's assumed parameter set evaluations should always be independent of each other.
        public bool TryFindMatchingThinnings(HeuristicSolutionPosition position, [NotNullWhen(true)] out HeuristicSolutionPool? solutions)
        {
            int maxPlanningPeriodOffset = Math.Max(position.PlanningPeriodIndex, this.discountRates - position.PlanningPeriodIndex);
            for (int planningPeriodOffset = 0; planningPeriodOffset <= maxPlanningPeriodOffset; ++planningPeriodOffset)
            {
                int planningPeriodIndex = position.PlanningPeriodIndex + planningPeriodOffset;
                if ((planningPeriodIndex < this.planningPeriods) && this.TryFindMatchingStandEntry(position, planningPeriodIndex, out solutions))
                {
                    return true;
                }
                planningPeriodIndex = position.PlanningPeriodIndex - planningPeriodOffset;
                if ((planningPeriodIndex >= 0) && this.TryFindMatchingStandEntry(position, planningPeriodIndex, out solutions))
                {
                    return true;
                }
            }

            solutions = null;
            return false;
        }
    }
}
