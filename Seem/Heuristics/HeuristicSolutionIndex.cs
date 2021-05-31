using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicSolutionIndex
    {
        private readonly int discountRates;
        //private readonly int planningPeriods;

        // indices are: discount rate, first thin, second thin, third thin, planning periods
        private readonly HeuristicSolutionPool[][][][][] solutionsByIndices;

        public HeuristicSolutionIndex(List<float> discountRates, List<int> firstThins, List<int> secondThins, List<int> thirdThins, List<int> planningPeriods)
        {
            this.discountRates = discountRates.Count;
            //this.planningPeriods = planningPeriods.Count;
            this.solutionsByIndices = new HeuristicSolutionPool[discountRates.Count][][][][];

            // create main set of solution pools and allocate unthinned solution pool arrays
            for (int discountRateIndex = 0; discountRateIndex < discountRates.Count; ++discountRateIndex)
            {
                HeuristicSolutionPool[][][][] discountRatePools = new HeuristicSolutionPool[firstThins.Count][][][];
                this.solutionsByIndices[discountRateIndex] = discountRatePools;

                for (int firstThinIndex = 0; firstThinIndex < firstThins.Count; ++firstThinIndex)
                {
                    HeuristicSolutionPool[][][] firstThinPools = new HeuristicSolutionPool[secondThins.Count][][];
                    discountRatePools[firstThinIndex] = firstThinPools;

                    for (int secondThinIndex = 0; secondThinIndex < secondThins.Count; ++secondThinIndex)
                    {
                        HeuristicSolutionPool[][] secondThinPools = new HeuristicSolutionPool[thirdThins.Count][];
                        firstThinPools[secondThinIndex] = secondThinPools;
                        
                        for (int thirdThinIndex = 0; thirdThinIndex < thirdThins.Count; ++thirdThinIndex)
                        {
                            HeuristicSolutionPool[] thirdThinPools = new HeuristicSolutionPool[planningPeriods.Count];
                            secondThinPools[thirdThinIndex] = thirdThinPools;

                            for (int planningPeriodIndex = 0; planningPeriodIndex < planningPeriods.Count; ++planningPeriodIndex)
                            {
                                // since an array of reference types with ostensibly non-null elements is initialized with nulls, ensure pools are present
                                // at all positions to avoid downstream failures with nullability checking
                                this[discountRateIndex, firstThinIndex, secondThinIndex, thirdThinIndex, planningPeriodIndex] = new HeuristicSolutionPool();
                            }
                        }
                    }
                }
            }
        }

        public HeuristicSolutionPool this[HeuristicSolutionPosition position]
        {
            get { return this[position.DiscountRateIndex, position.FirstThinPeriodIndex, position.SecondThinPeriodIndex, position.ThirdThinPeriodIndex, position.PlanningPeriodIndex]; }
        }

        private HeuristicSolutionPool this[int discountRateIndex, int firstThinPeriodIndex, int secondThinPeriodIndex, int thirdThinPeriodIndex, int planningPeriodIndex]
        {
            get { return this.solutionsByIndices[discountRateIndex][firstThinPeriodIndex][secondThinPeriodIndex][thirdThinPeriodIndex][planningPeriodIndex]; }
            set { this.solutionsByIndices[discountRateIndex][firstThinPeriodIndex][secondThinPeriodIndex][thirdThinPeriodIndex][planningPeriodIndex] = value; }
        }

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
                    solutions = this[discountRateIndex, position.FirstThinPeriodIndex, position.SecondThinPeriodIndex, position.ThirdThinPeriodIndex, planningPeriodIndex];
                    if (solutions.High != null)
                    {
                        return true;
                    }

                    if (discountRateOffset > 0)
                    {
                        discountRateIndex = position.DiscountRateIndex - discountRateOffset;
                        if (discountRateIndex >= 0)
                        {
                            solutions = this[discountRateIndex, position.FirstThinPeriodIndex, position.SecondThinPeriodIndex, position.ThirdThinPeriodIndex, planningPeriodIndex];
                            if (solutions.High != null)
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

        public bool TryFindWithinDiscountRate(HeuristicSolutionPosition position, [NotNullWhen(true)] out HeuristicSolutionPool? solutions)
        {
            return this.TryFindMatchingStandEntry(position, position.PlanningPeriodIndex, out solutions);
        }

        //public bool TryFindMatchingThinnings(HeuristicSolutionPosition position, [NotNullWhen(true)] out HeuristicSolutionPool? solutions)
        //{
        //    int maxPlanningPeriodOffset = Math.Max(position.PlanningPeriodIndex, this.discountRates - position.PlanningPeriodIndex);
        //    for (int planningPeriodOffset = 0; planningPeriodOffset <= maxPlanningPeriodOffset; ++planningPeriodOffset)
        //    {
        //        int planningPeriodIndex = position.PlanningPeriodIndex + planningPeriodOffset;
        //        if ((planningPeriodIndex < this.planningPeriods) && this.TryFindMatchingStandEntry(position, planningPeriodIndex, out solutions))
        //        {
        //            return true;
        //        }
        //        planningPeriodIndex = position.PlanningPeriodIndex - planningPeriodOffset;
        //        if ((planningPeriodIndex >= 0) && this.TryFindMatchingStandEntry(position, planningPeriodIndex, out solutions))
        //        {
        //            return true;
        //        }
        //    }

        //    solutions = null;
        //    return false;
        //}
    }
}
