using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Osu.Cof.Ferm.Heuristics
{
    // provide a non-template base class to allow PowerShell cmdlets to accept results from any heuristic as a parameter
    public class HeuristicResults
    {
        private readonly int parameterCombinationCount;
        // indices are: parameter combination, first thin, second thin, third thin, planning periods, discount rate
        // Nullability in multidimensional arrays does not appear supported as of Visual Studio 16.10.1. See remarks in constructor.
        private readonly HeuristicResult[][][][][][] results;

        public IList<HeuristicResultPosition> CombinationsEvaluated { get; private init; }

        public IList<float> DiscountRates { get; private init; }
        public IList<int> FirstThinPeriods { get; private init; }
        public IList<int> RotationLengths { get; private init; }
        public IList<int> SecondThinPeriods { get; private init; }
        public IList<int> ThirdThinPeriods { get; private init; }

        protected HeuristicResults(int parameterCombinationCount, IList<int> firstThinPeriods, IList<int> secondThinPeriods, IList<int> thirdThinPeriods, IList<int> rotationLengths, IList<float> discountRates, int individualPoolSize)
        {
            this.parameterCombinationCount = parameterCombinationCount;
            this.results = new HeuristicResult[parameterCombinationCount][][][][][];

            this.CombinationsEvaluated = new List<HeuristicResultPosition>();
            this.DiscountRates = discountRates;
            this.FirstThinPeriods = firstThinPeriods;
            this.RotationLengths = rotationLengths;
            this.SecondThinPeriods = secondThinPeriods;
            this.ThirdThinPeriods = thirdThinPeriods;

            // fill result arrays where applicable
            //   parameter array elements: never null
            //   first thin array elements: never null
            //   second and third thin and planning period thin array elements: maybe null
            //   results in discount rate array elements: never null
            // Jagged arrays which aren't needed are left null. Partly for the minor memory savings, mostly to increase the chances of
            // detecting storage of results to nonsensical locations or other indexing issues.
            for (int parameterIndex = 0; parameterIndex < parameterCombinationCount; ++parameterIndex)
            {
                HeuristicResult[][][][][] parameterCombinationResults = new HeuristicResult[this.FirstThinPeriods.Count][][][][];
                this.results[parameterIndex] = parameterCombinationResults;

                for (int firstThinIndex = 0; firstThinIndex < this.FirstThinPeriods.Count; ++firstThinIndex)
                {
                    HeuristicResult[][][][] firstThinResults = new HeuristicResult[this.SecondThinPeriods.Count][][][];
                    parameterCombinationResults[firstThinIndex] = firstThinResults;

                    int firstThinPeriod = this.FirstThinPeriods[firstThinIndex];
                    for (int secondThinIndex = 0; secondThinIndex < this.SecondThinPeriods.Count; ++secondThinIndex)
                    {
                        int secondThinPeriod = this.SecondThinPeriods[secondThinIndex];
                        if ((secondThinPeriod == Constant.NoThinPeriod) || (secondThinPeriod > firstThinPeriod))
                        {
                            HeuristicResult[][][] secondThinResults = new HeuristicResult[this.ThirdThinPeriods.Count][][];
                            firstThinResults[secondThinIndex] = secondThinResults;

                            int lastOfFirstOrSecondThinPeriod = Math.Max(firstThinPeriod, secondThinPeriod);
                            for (int thirdThinIndex = 0; thirdThinIndex < this.ThirdThinPeriods.Count; ++thirdThinIndex)
                            {
                                int thirdThinPeriod = this.ThirdThinPeriods[thirdThinIndex];
                                if ((thirdThinPeriod == Constant.NoThinPeriod) || (thirdThinPeriod > secondThinPeriod))
                                {
                                    HeuristicResult[][] thirdThinResults = new HeuristicResult[this.RotationLengths.Count][];
                                    secondThinResults[thirdThinIndex] = thirdThinResults;

                                    int lastOfFirstSecondOrThirdThinPeriod = Math.Max(lastOfFirstOrSecondThinPeriod, thirdThinPeriod);
                                    for (int rotationIndex = 0; rotationIndex < this.RotationLengths.Count; ++rotationIndex)
                                    {
                                        int endOfRotationPeriodPeriod = this.RotationLengths[rotationIndex];
                                        if (endOfRotationPeriodPeriod > lastOfFirstSecondOrThirdThinPeriod)
                                        {
                                            HeuristicResult[] rotationLengthResults = new HeuristicResult[this.DiscountRates.Count];
                                            thirdThinResults[rotationIndex] = rotationLengthResults;

                                            for (int discountRateIndex = 0; discountRateIndex < this.DiscountRates.Count; ++discountRateIndex)
                                            {
                                                rotationLengthResults[discountRateIndex] = new HeuristicResult(individualPoolSize);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public HeuristicResult this[HeuristicResultPosition position]
        {
            get { return this[position.ParameterIndex, position.FirstThinPeriodIndex, position.SecondThinPeriodIndex, position.ThirdThinPeriodIndex, position.RotationIndex, position.DiscountRateIndex]; }
        }

        private HeuristicResult this[int parameterIndex, int firstThinPeriodIndex, int secondThinPeriodIndex, int thirdThinPeriodIndex, int rotationIndex, int discountRateIndex]
        {
            get 
            {
                HeuristicResult[][][][][] parameterResults = this.results[parameterIndex];
                Debug.Assert(parameterResults != null, "Invalid parameter index " + parameterIndex + ".");
                HeuristicResult[][][][] firstThinResults = parameterResults[firstThinPeriodIndex];
                Debug.Assert(firstThinResults != null, "Invalid first thin index " + firstThinPeriodIndex + ".");
                HeuristicResult[][][] secondThinResults = firstThinResults[secondThinPeriodIndex];
                Debug.Assert(secondThinResults != null, "Invalid second thin index " + secondThinPeriodIndex + ".");
                HeuristicResult[][] thirdThinResults = secondThinResults[thirdThinPeriodIndex];
                Debug.Assert(thirdThinResults != null, "Invalid third thin index " + thirdThinPeriodIndex + ".");
                HeuristicResult[] rotationLengthResults = thirdThinResults[rotationIndex];
                Debug.Assert(rotationLengthResults != null, "Invalid rotation index " + rotationIndex + ".");
                return rotationLengthResults[discountRateIndex]; 
            }
            set
            {
                HeuristicResult[][][][][] parameterResults = this.results[parameterIndex];
                Debug.Assert(parameterResults != null, "Invalid parameter index " + parameterIndex + ".");
                HeuristicResult[][][][] firstThinResults = parameterResults[firstThinPeriodIndex];
                Debug.Assert(firstThinResults != null, "Invalid first thin index " + firstThinPeriodIndex + ".");
                HeuristicResult[][][] secondThinResults = firstThinResults[secondThinPeriodIndex];
                Debug.Assert(secondThinResults != null, "Invalid second thin index " + secondThinPeriodIndex + ".");
                HeuristicResult[][] thirdThinResults = secondThinResults[thirdThinPeriodIndex];
                Debug.Assert(thirdThinResults != null, "Invalid third thin index " + thirdThinPeriodIndex + ".");
                HeuristicResult[] rotationLengthResults = thirdThinResults[rotationIndex];
                Debug.Assert(rotationLengthResults != null, "Invalid rotation index " + rotationIndex + ".");
                rotationLengthResults[discountRateIndex] = value; 
            }
        }

        public void AssimilateHeuristicRunIntoPosition(Heuristic heuristic, HeuristicPerformanceCounters perfCounters, HeuristicResultPosition position)
        {
            HeuristicResult result = this[position];
            Debug.Assert(result != null);
            result.Distribution.AddSolution(heuristic, position, perfCounters);
            result.Pool.TryAddOrReplace(heuristic, position);
        }

        public void GetPoolPerformanceCounters(out int solutionsCached, out int solutionsAccepted, out int solutionsRejected)
        {
            solutionsAccepted = 0;
            solutionsCached = 0;
            solutionsRejected = 0;

            for (int parameterIndex = 0; parameterIndex < this.parameterCombinationCount; ++parameterIndex)
            {
                HeuristicResult[][][][][] parameterCombinationResults = this.results[parameterIndex];
                for (int firstThinIndex = 0; firstThinIndex < parameterCombinationResults.Length; ++firstThinIndex)
                {
                    HeuristicResult[][][][] firstThinResults = parameterCombinationResults[firstThinIndex];
                    for (int secondThinIndex = 0; secondThinIndex < firstThinResults.Length; ++secondThinIndex)
                    {
                        HeuristicResult[][][] secondThinResults = firstThinResults[secondThinIndex];
                        if (secondThinResults != null)
                        {
                            for (int thirdThinIndex = 0; thirdThinIndex < secondThinResults.Length; ++thirdThinIndex)
                            {
                                HeuristicResult[][] thirdThinResults = secondThinResults[thirdThinIndex];
                                if (thirdThinResults != null)
                                {
                                    for (int rotationIndex = 0; rotationIndex < thirdThinResults.Length; ++rotationIndex)
                                    {
                                        HeuristicResult[] rotationLengthResults = thirdThinResults[rotationIndex];
                                        if (rotationLengthResults != null)
                                        {
                                            for (int discountRateIndex = 0; discountRateIndex < rotationLengthResults.Length; ++discountRateIndex)
                                            {
                                                HeuristicResult result = rotationLengthResults[discountRateIndex];
                                                solutionsAccepted += result.Pool.SolutionsAccepted;
                                                solutionsCached += result.Pool.SolutionsInPool;
                                                solutionsRejected += result.Pool.SolutionsRejected;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // searches among discount rates for solutions assigned to the same set of heuristic parameters, thinnings, and rotation length
        private bool TryFindSolutionsMatchingStandEntries(HeuristicResultPosition position, int rotationIndex, [NotNullWhen(true)] out HeuristicSolutionPool? pool)
        {
            pool = null;

            // see if a discount rate array exists for this combination of thins and rotation length
            HeuristicResult[][][][][] parameterResults = this.results[position.ParameterIndex];
            Debug.Assert(parameterResults != null);
            HeuristicResult[][][][] firstThinResults = parameterResults[position.FirstThinPeriodIndex];
            Debug.Assert(firstThinResults != null);
            HeuristicResult[][][] secondThinResults = firstThinResults[position.SecondThinPeriodIndex];
            if (secondThinResults == null)
            {
                return false;
            }
            HeuristicResult[][] thirdThinResults = secondThinResults[position.ThirdThinPeriodIndex];
            if (thirdThinResults == null)
            {
                return false;
            }
            HeuristicResult[] rotationLengthResults = thirdThinResults[rotationIndex];
            if (rotationLengthResults == null)
            {
                return false;
            }

            // search among discount rates
            int maxDiscountRateOffset = Math.Max(position.DiscountRateIndex, rotationLengthResults.Length - position.DiscountRateIndex);
            for (int discountRateOffset = 0; discountRateOffset <= maxDiscountRateOffset; ++discountRateOffset)
            {
                // for now, arbitrarily define the next following discount rate as closer than the previous discount rate
                // If needed, this can be made intelligent enough to look at the actual discount rates.
                int discountRateIndex = position.DiscountRateIndex + discountRateOffset;
                if (discountRateIndex < rotationLengthResults.Length)
                {
                    pool = rotationLengthResults[discountRateIndex].Pool;
                    if (pool.SolutionsInPool > 0)
                    {
                        return true;
                    }

                    if (discountRateOffset > 0)
                    {
                        discountRateIndex = position.DiscountRateIndex - discountRateOffset;
                        if (discountRateIndex >= 0)
                        {
                            pool = rotationLengthResults[discountRateIndex].Pool;
                            if (pool.SolutionsInPool > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            pool = null;
            return false;
        }

        // searches among discount rates and rotation lengths for solutions assigned to the same set of heuristic parameters and thinnings
        // Looking across parameters would confound heuristic parameter tuning runs by allowing later parameter sets to access solutions obtained
        // by earlier runs. It's assumed parameter set evaluations should always be independent of each other.
        public bool TryFindSolutionsMatchingThinnings(HeuristicResultPosition position, [NotNullWhen(true)] out HeuristicSolutionPool? solutions)
        {
            int maxRotationOffset = Math.Max(position.RotationIndex, this.RotationLengths.Count - position.RotationIndex);
            for (int rotationOffset = 0; rotationOffset <= maxRotationOffset; ++rotationOffset)
            {
                int rotationIndex = position.RotationIndex + rotationOffset;
                if ((rotationIndex < this.RotationLengths.Count) && this.TryFindSolutionsMatchingStandEntries(position, rotationIndex, out solutions))
                {
                    return true;
                }
                rotationIndex = position.RotationIndex - rotationOffset;
                if ((rotationIndex >= 0) && this.TryFindSolutionsMatchingStandEntries(position, rotationIndex, out solutions))
                {
                    return true;
                }
            }

            solutions = null;
            return false;
        }
    }

    public class HeuristicResults<TParameters> : HeuristicResults where TParameters : HeuristicParameters
    {
        public IList<TParameters> ParameterCombinations { get; private init; }

        public HeuristicResults(IList<TParameters> parameterCombinations, IList<float> discountRates, IList<int> firstThinPeriods, IList<int> secondThinPeriods, IList<int> thirdThinPeriods, IList<int> rotationLengths, int individualSolutionPoolSize)
            : base(parameterCombinations.Count, firstThinPeriods, secondThinPeriods, thirdThinPeriods, rotationLengths, discountRates, individualSolutionPoolSize)
        {
            this.ParameterCombinations = parameterCombinations;
        }
    }
}
