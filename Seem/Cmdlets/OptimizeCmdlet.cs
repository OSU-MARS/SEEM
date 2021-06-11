using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osu.Cof.Ferm.Cmdlets
{
    public abstract class OptimizeCmdlet<TParameters> : Cmdlet where TParameters : HeuristicParameters
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int BestOf { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(Constant.Grasp.FullyRandomConstructionForMaximization, Constant.Grasp.FullyGreedyConstructionForMaximization)]
        public List<float> ConstructionGreediness { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0.0F, 100.0F)]
        public List<float> DiscountRates { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(Constant.NoThinPeriod, 100)]
        public List<int> FirstThinPeriod { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0.0F, 1.0F)]
        public List<float> InitialThinningProbability { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0, 100)]
        public List<int> PlanningPeriods { get; set; }
        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(Constant.NoThinPeriod, 100)]
        public List<int> SecondThinPeriod { get; set; }

        [Parameter]
        [ValidateRange(1, 100)]
        public int SolutionPoolSize { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand? Stand { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(Constant.NoThinPeriod, 100)]
        public List<int> ThirdThinPeriod { get; set; }

        [Parameter]
        public int Threads { get; set; }

        [Parameter]
        public TimberObjective TimberObjective { get; set; }
        [Parameter]
        [ValidateNotNull]
        public TimberValue TimberValue { get; set; }
        [Parameter]
        public TreeModel TreeModel { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.Threads = Environment.ProcessorCount / 2; // assume all cores are hyperthreaded

            this.DiscountRates = new List<float>() { Constant.DefaultAnnualDiscountRate };
            this.FirstThinPeriod = new List<int>() { Constant.DefaultThinningPeriod };
            this.PlanningPeriods = new List<int>() { Constant.DefaultPlanningPeriods };
            this.ConstructionGreediness = new List<float>() { Constant.Grasp.FullyRandomConstructionForMaximization };
            this.InitialThinningProbability = new List<float>() { Constant.HeuristicDefault.InitialThinningProbability };
            this.SecondThinPeriod = new List<int>() { Constant.NoThinPeriod };
            this.SolutionPoolSize = Constant.DefaultSolutionPoolSize;
            this.ThirdThinPeriod = new List<int>() { Constant.NoThinPeriod };
            this.TimberObjective = TimberObjective.LandExpectationValue;
            this.TimberValue = TimberValue.Default;
            this.TreeModel = TreeModel.OrganonNwo;
        }

        protected virtual bool HeuristicEvaluatesDiscountRates
        {
            get { return false; }
        }

        protected abstract Heuristic<TParameters> CreateHeuristic(TParameters heuristicParameters, RunParameters runParameters);

        protected virtual IHarvest CreateThin(int thinPeriodIndex)
        {
            return new ThinByIndividualTreeSelection(thinPeriodIndex);
        }

        protected IList<HeuristicParameters> GetDefaultParameterCombinations()
        {
            List<HeuristicParameters> parameterCombinations = new();
            foreach (float constructionGreediness in this.ConstructionGreediness)
            {
                foreach (float thinningProbability in this.InitialThinningProbability)
                {
                    parameterCombinations.Add(new HeuristicParameters()
                    {
                        ConstructionGreediness = constructionGreediness,
                        InitialThinningProbability = thinningProbability
                    });
                }
            }
            return parameterCombinations;
        }

        // for now, assume runtime complexity is linear with the number of periods requiring thinning optimization
        // This is simplified, but it's more accurate than assuming all runs have equal cost regardless of the number of timesteps required and is
        // a reasonable approximation for hero, Monte Carlo heuristics, prescription enumeration, and genetic algorithms.
        protected static int EstimateRuntimeCost(HeuristicResultPosition position, HeuristicResults<TParameters> results)
        {
            int periodWeight = 0; // no thins so only a single trajectory need be simulated; assume cost is negligible as no optimization occurs
            int firstThinPeriod = results.FirstThinPeriods[position.FirstThinPeriodIndex];
            if (firstThinPeriod != Constant.NoThinPeriod)
            {
                int planningPeriods = results.PlanningPeriods[position.PlanningPeriodIndex];
                periodWeight = planningPeriods - firstThinPeriod;

                int secondThinPeriod = results.SecondThinPeriods[position.SecondThinPeriodIndex];
                if (secondThinPeriod != Constant.NoThinPeriod)
                {
                    periodWeight += planningPeriods - secondThinPeriod;

                    int thirdThinPeriod = results.ThirdThinPeriods[position.ThirdThinPeriodIndex];
                    if (thirdThinPeriod == Constant.NoThinPeriod)
                    {
                        return planningPeriods - thirdThinPeriod;
                    }
                }
            }

            return periodWeight;
        }

        protected abstract string GetName();

        // ideally this would be virtual but, as of C# 9.0, the nature of C# generics requires GetDefaultParameterCombinations() to be separate
        protected abstract IList<TParameters> GetParameterCombinations();

        protected override void ProcessRecord()
        {
            if ((this.TimberObjective == TimberObjective.ScribnerVolume) && (this.DiscountRates.Count > 1) && (this.HeuristicEvaluatesDiscountRates == false))
            {
                // low priority to improve this but at least warn about limited support
                this.WriteWarning("Timber optimization objective is " + this.TimberObjective + " but multiple discount rates are specified. Optimization will be unnecessarily repeated for each discount rate.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters totalPerfCounters = new();
            int totalRuntimeCost = 0;

            int treeCount = this.Stand!.GetTreeRecordCount();
            List<HeuristicResultPosition> combinationsToEvaluate = new();
            IList<TParameters> parameterCombinationsForHeuristic = this.GetParameterCombinations();
            HeuristicResults<TParameters> results = new(parameterCombinationsForHeuristic, this.DiscountRates, this.FirstThinPeriod, this.SecondThinPeriod, this.ThirdThinPeriod, this.PlanningPeriods, this.SolutionPoolSize);
            for (int parameterIndex = 0; parameterIndex < parameterCombinationsForHeuristic.Count; ++parameterIndex)
            {
                for (int firstThinIndex = 0; firstThinIndex < this.FirstThinPeriod.Count; ++firstThinIndex)
                {
                    int firstThinPeriod = this.FirstThinPeriod[firstThinIndex];
                    if (firstThinPeriod == 0)
                    {
                        throw new ParameterOutOfRangeException(nameof(this.FirstThinPeriod), "First thinning period cannot be zero.");
                    }

                    for (int secondThinIndex = 0; secondThinIndex < this.SecondThinPeriod.Count; ++secondThinIndex)
                    {
                        int secondThinPeriod = this.SecondThinPeriod[secondThinIndex];
                        if (secondThinPeriod == 0)
                        {
                            throw new ParameterOutOfRangeException(nameof(this.SecondThinPeriod), "Second thinning period cannot be zero.");
                        }
                        if (secondThinPeriod != Constant.NoThinPeriod)
                        {
                            if ((firstThinPeriod == Constant.NoThinPeriod) || (firstThinPeriod >= secondThinPeriod))
                            {
                                // can't perform a second thin if
                                // - there was no first thin
                                // - the second thin would occur before or in the same period as the first thin
                                // - the second thin would occur before or in the same period as final harvest
                                continue;
                            }
                        }

                        int lastOfFirstOrSecondThinPeriod = Math.Max(firstThinPeriod, secondThinPeriod);
                        for (int thirdThinIndex = 0; thirdThinIndex < this.ThirdThinPeriod.Count; ++thirdThinIndex)
                        {
                            int thirdThinPeriod = this.ThirdThinPeriod[thirdThinIndex];
                            if (thirdThinPeriod == 0)
                            {
                                throw new ParameterOutOfRangeException(nameof(this.ThirdThinPeriod), "Third thinning period cannot be zero.");
                            }
                            if (thirdThinPeriod != Constant.NoThinPeriod)
                            {
                                if ((secondThinPeriod == Constant.NoThinPeriod) || (secondThinPeriod >= thirdThinPeriod))
                                {
                                    // can't perform a third thin if
                                    // - there was no second thin
                                    // - the third thin would occur before or in the same period as the second thin
                                    // - the third thin would occur before or in the same period as final harvest
                                    continue;
                                }
                            }

                            int lastOfFirstSecondOrThirdThinPeriod = Math.Max(lastOfFirstOrSecondThinPeriod, thirdThinPeriod);
                            for (int planningPeriodIndex = 0; planningPeriodIndex < this.PlanningPeriods.Count; ++planningPeriodIndex)
                            {
                                int planningPeriods = this.PlanningPeriods[planningPeriodIndex];
                                if (lastOfFirstSecondOrThirdThinPeriod >= planningPeriods)
                                {
                                    // last thin would occur before or in the same period as the final harvest
                                    continue;
                                }

                                if (this.HeuristicEvaluatesDiscountRates)
                                {
                                    HeuristicResultPosition position = new()
                                    {
                                        DiscountRateIndex = Constant.AllDiscountRatePosition,
                                        FirstThinPeriodIndex = firstThinIndex,
                                        ParameterIndex = parameterIndex,
                                        PlanningPeriodIndex = planningPeriodIndex,
                                        SecondThinPeriodIndex = secondThinIndex,
                                        ThirdThinPeriodIndex = thirdThinIndex
                                    };
                                    combinationsToEvaluate.Add(position);
                                    totalRuntimeCost += OptimizeCmdlet<TParameters>.EstimateRuntimeCost(position, results);
                                }
                                else
                                {
                                    // heuristic optimizes for one discount rate at a time
                                    for (int discountRateIndex = 0; discountRateIndex < this.DiscountRates.Count; ++discountRateIndex)
                                    {
                                        // distributions are unique per combination of run tuple (discount rate, thins 1, 2, 3, and rotation)
                                        // and heuristic parameters but solution pools are shared across heuristic parameters
                                        HeuristicResultPosition position = new()
                                        {
                                            DiscountRateIndex = discountRateIndex,
                                            FirstThinPeriodIndex = firstThinIndex,
                                            ParameterIndex = parameterIndex,
                                            PlanningPeriodIndex = planningPeriodIndex,
                                            SecondThinPeriodIndex = secondThinIndex,
                                            ThirdThinPeriodIndex = thirdThinIndex
                                        };
                                        combinationsToEvaluate.Add(position);
                                        totalRuntimeCost += OptimizeCmdlet<TParameters>.EstimateRuntimeCost(position, results);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            OrganonConfiguration organonConfiguration = new(OrganonVariant.Create(this.TreeModel));
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = this.Threads
            };
            int runsCompleted = 0;
            int runtimeCostCompleted = 0;
            int totalRuns = this.BestOf * combinationsToEvaluate.Count;
            totalRuntimeCost *= this.BestOf;
            Task runs = Task.Run(() =>
            {
                Parallel.For(0, totalRuns, parallelOptions, (int iteration, ParallelLoopState loopState) =>
                {
                    if (loopState.ShouldExitCurrentIteration)
                    {
                        return;
                    }
                    int resultPositionIndex = iteration / this.BestOf;
                    HeuristicResultPosition position = combinationsToEvaluate[resultPositionIndex];

                    try
                    {
                        RunParameters runParameters = new(this.DiscountRates, organonConfiguration)
                        {
                            LastPlanningPeriod = this.PlanningPeriods[position.PlanningPeriodIndex],
                            MaximizeForPlanningPeriod = this.PlanningPeriods[position.PlanningPeriodIndex],
                            TimberObjective = this.TimberObjective,
                            TimberValue = this.TimberValue
                        };
                        if (this.TryCreateFirstThin(position.FirstThinPeriodIndex, out IHarvest? firstThin))
                        {
                            runParameters.Treatments.Harvests.Add(firstThin);
                            if (this.TryCreateSecondThin(position.SecondThinPeriodIndex, out IHarvest? secondThin))
                            {
                                runParameters.Treatments.Harvests.Add(secondThin);
                                if (this.TryCreateThirdThin(position.ThirdThinPeriodIndex, out IHarvest? thirdThin))
                                {
                                    runParameters.Treatments.Harvests.Add(thirdThin);
                                }
                            }
                        }
                        TParameters heuristicParameters = parameterCombinationsForHeuristic[position.ParameterIndex];
                        Heuristic<TParameters> currentHeuristic = this.CreateHeuristic(heuristicParameters, runParameters);
                        HeuristicPerformanceCounters perfCounters = currentHeuristic.Run(position, results);

                        // accumulate run into results and tracking counters
                        int estimatedRuntimeCost = OptimizeCmdlet<TParameters>.EstimateRuntimeCost(position, results);
                        lock (results)
                        {
                            if (this.HeuristicEvaluatesDiscountRates)
                            {
                                // scatter heuristic's results to discount rates
                                // For now, only log perf counters once as counts are merged across discount rates. If needed, move acceptance
                                // and rejection can be tracked by discount rate.
                                for (int discountRateIndex = 0; discountRateIndex < results.DiscountRates.Count; ++discountRateIndex)
                                {
                                    HeuristicResultPosition discountRatePosition = new(position)
                                    {
                                        DiscountRateIndex = discountRateIndex
                                    };
                                    HeuristicPerformanceCounters perfCountersToAssmilate = perfCounters;
                                    if (discountRateIndex > 0)
                                    {
                                        perfCountersToAssmilate = HeuristicPerformanceCounters.Zero;
                                    }
                                    results.AssimilateHeuristicRunIntoPosition(currentHeuristic, perfCountersToAssmilate, discountRatePosition);
                                    results.CombinationsEvaluated.Add(discountRatePosition);
                                }
                            }
                            else
                            {
                                // heuristic is specific to a single discount rate
                                results.AssimilateHeuristicRunIntoPosition(currentHeuristic, perfCounters, position);
                                results.CombinationsEvaluated.Add(position);
                            }

                            totalPerfCounters += perfCounters;
                            ++runsCompleted;
                            runtimeCostCompleted += estimatedRuntimeCost;
                        }
                    }
                    catch (Exception exception)
                    {
                        string discountRate = "vectorized";
                        if (this.HeuristicEvaluatesDiscountRates == false)
                        {
                            discountRate = results.DiscountRates[position.DiscountRateIndex].ToString();
                        }
                        throw new AggregateException("Exception encountered during optimization. Discount rate " + discountRate +
                                                     ", first thin period " + results.FirstThinPeriods[position.FirstThinPeriodIndex] +
                                                     ", second thin period " + results.SecondThinPeriods[position.SecondThinPeriodIndex] +
                                                     ", third thin period " + results.ThirdThinPeriods[position.ThirdThinPeriodIndex] +
                                                     ", planning length " + results.PlanningPeriods[position.PlanningPeriodIndex] +
                                                     ".", 
                                                     exception);
                    }
                    if (this.Stopping)
                    {
                        loopState.Stop();
                    }
                });
            });

            string cmdletName = this.GetName();
            StringBuilder mostRecentEvaluationPoint = new();
            int sleepsSinceLastStatusUpdate = 0;
            while (runs.IsCompleted == false)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1.0));
                ++sleepsSinceLastStatusUpdate;

                if (runs.IsFaulted)
                {
                    Debug.Assert(runs.Exception != null && runs.Exception.InnerException != null);
                    // per https://stackoverflow.com/questions/20170527/how-to-correctly-rethrow-an-exception-of-task-already-in-faulted-state
                    ExceptionDispatchInfo.Capture(runs.Exception.InnerException).Throw();
                }
                if (sleepsSinceLastStatusUpdate > 30)
                {
                    int currentRun = Math.Min(runsCompleted + 1, totalRuns);
                    int currentPositionIndex = Math.Min(currentRun / this.BestOf, combinationsToEvaluate.Count - 1);
                    HeuristicResultPosition mostRecentCombination = combinationsToEvaluate[currentPositionIndex];

                    mostRecentEvaluationPoint.Clear();
                    mostRecentEvaluationPoint.Append("run " + currentRun + "/" + totalRuns);
                    if (this.PlanningPeriods.Count > 1)
                    {
                        mostRecentEvaluationPoint.Append(", rotation " + mostRecentCombination.PlanningPeriodIndex + "/" + this.PlanningPeriods.Count);
                    }
                    if (this.FirstThinPeriod.Count > 1)
                    {
                        mostRecentEvaluationPoint.Append(", thin 1 " + mostRecentCombination.FirstThinPeriodIndex + "/" + this.FirstThinPeriod.Count);
                    }
                    if (this.SecondThinPeriod.Count > 1)
                    {
                        mostRecentEvaluationPoint.Append(", 2 " + mostRecentCombination.SecondThinPeriodIndex + "/" + this.SecondThinPeriod.Count);
                    }
                    if (this.ThirdThinPeriod.Count > 1)
                    {
                        mostRecentEvaluationPoint.Append(", 3 " + mostRecentCombination.ThirdThinPeriodIndex + "/" + this.ThirdThinPeriod.Count);
                    }
                    if (parameterCombinationsForHeuristic.Count > 1)
                    {
                        mostRecentEvaluationPoint.Append(", parameters " + mostRecentCombination.ParameterIndex + "/" + parameterCombinationsForHeuristic.Count);
                    }
                    if ((this.HeuristicEvaluatesDiscountRates == false) && (this.DiscountRates.Count > 1))
                    {
                        mostRecentEvaluationPoint.Append(", rate " + mostRecentCombination.DiscountRateIndex + "/" + this.DiscountRates.Count);
                    }
                    mostRecentEvaluationPoint.Append(" (" + this.Threads + " threads)");

                    double fractionComplete = (double)runtimeCostCompleted / (double)totalRuntimeCost;
                    double secondsElapsed = stopwatch.Elapsed.TotalSeconds;
                    double secondsRemaining = secondsElapsed * (1.0 / fractionComplete - 1.0);
                    this.WriteProgress(new ProgressRecord(0, cmdletName,  mostRecentEvaluationPoint.ToString())
                    {
                        PercentComplete = (int)(100.0 * fractionComplete),
                        SecondsRemaining = (int)Math.Round(secondsRemaining)
                    });
                    sleepsSinceLastStatusUpdate = 0;
                }
            }
            runs.GetAwaiter().GetResult(); // propagate any exceptions since last IsFaulted check
            stopwatch.Stop();

            this.WriteObject(results);
            if (results.CombinationsEvaluated.Count == 1)
            {
                HeuristicResultPosition firstPosition = results.CombinationsEvaluated[0];
                this.WriteSingleDistributionSummary(results[firstPosition], totalPerfCounters, stopwatch.Elapsed);
            }
            else if (results.CombinationsEvaluated.Count > 1)
            {
                this.WriteMultipleDistributionSummary(results, totalPerfCounters, stopwatch.Elapsed);
            }
            else
            {
                this.WriteWarning("No valid parameter combininations found. No runs performed.");
            }
        }

        private void WriteMultipleDistributionSummary(HeuristicResults<TParameters> results, HeuristicPerformanceCounters totalPerfCounters, TimeSpan elapsedTime)
        {
            results.GetPoolPerformanceCounters(out int solutionsCached, out int solutionsAccepted, out int solutionsRejected);

            base.WriteVerbose(String.Empty); // Visual Studio Code display mangling workaround
            this.WriteVerbose("{0}: {1} configurations with {2} runs in {3:0.00} minutes ({4:0.00}M timesteps in {5:0.00} core-minutes, {6:0.00}% move acceptance, {7} solutions pooled, {8:0.00}% pool acceptance).",
                              this.GetName(),
                              results.CombinationsEvaluated.Count,
                              this.BestOf * results.CombinationsEvaluated.Count,
                              elapsedTime.TotalMinutes,
                              1E-6F * totalPerfCounters.GrowthModelTimesteps,
                              totalPerfCounters.Duration.TotalMinutes,
                              100.0F * totalPerfCounters.MovesAccepted / (totalPerfCounters.MovesAccepted + totalPerfCounters.MovesRejected),
                              solutionsCached,
                              100.0F * solutionsAccepted / (solutionsAccepted + solutionsRejected));
        }

        private void WriteSingleDistributionSummary(HeuristicResult result, HeuristicPerformanceCounters totalPerfCounters, TimeSpan elapsedTime)
        {
            Heuristic? heuristic = result.Pool.High;
            if (heuristic == null)
            {
                throw new ArgumentNullException(nameof(result), "Result's solution pool does not have a high heuristic.");
            }

            float maximumHarvest = Single.MinValue;
            float minimumHarvest = Single.MaxValue;
            float harvestSum = 0.0F;
            float harvestSumOfSquares = 0.0F;
            for (int periodIndex = 1; periodIndex < heuristic.BestTrajectory.PlanningPeriods; ++periodIndex)
            {
                float harvestVolumeScribner = heuristic.BestTrajectory.ThinningVolume.GetScribnerTotal(periodIndex);
                maximumHarvest = Math.Max(harvestVolumeScribner, maximumHarvest);
                harvestSum += harvestVolumeScribner;
                harvestSumOfSquares += harvestVolumeScribner * harvestVolumeScribner;
                minimumHarvest = Math.Min(harvestVolumeScribner, minimumHarvest);
            }
            float periods = (float)(heuristic.BestTrajectory.PlanningPeriods - 1);
            float meanHarvest = harvestSum / periods;
            float variance = harvestSumOfSquares / periods - meanHarvest * meanHarvest;
            float standardDeviation = MathF.Sqrt(variance);
            float flowEvenness = Math.Max(maximumHarvest - meanHarvest, meanHarvest - minimumHarvest) / meanHarvest;

            base.WriteVerbose(String.Empty); // Visual Studio code workaround
            int totalMoves = totalPerfCounters.MovesAccepted + totalPerfCounters.MovesRejected;
            this.WriteVerbose("{0}: {1} moves, {2} changing ({3:0%}), {4} unchanging ({5:0%})", heuristic.GetName(), totalMoves, totalPerfCounters.MovesAccepted, (float)totalPerfCounters.MovesAccepted / (float)totalMoves, totalPerfCounters.MovesRejected, (float)totalPerfCounters.MovesRejected / (float)totalMoves);
            this.WriteVerbose("objective: best {0:0.00#}, mean {1:0.00#} ending {2:0.00#}.", heuristic.FinancialValue.GetHighestValueForDefaultDiscountRate(), result.Distribution.HighestFinancialValueBySolution.Average(), heuristic.FinancialValue.GetAcceptedValueForDiscountRateOrDefault(Constant.HeuristicDefault.DiscountRateIndex).Last());
            this.WriteVerbose("flow: {0:0.0#} mean, {1:0.000} σ, {2:0.000}% even, {3:0.0#}-{4:0.0#} = range {5:0.0}.", meanHarvest, standardDeviation, 1E2 * flowEvenness, minimumHarvest, maximumHarvest, maximumHarvest - minimumHarvest);

            double totalSeconds = totalPerfCounters.Duration.TotalSeconds;
            double iterationsPerSecond = totalMoves / totalSeconds;
            double iterationsPerSecondMultiplier = iterationsPerSecond > 1E3 ? 1E-3 : 1.0;
            string iterationsPerSecondScale = iterationsPerSecond > 1E3 ? "k" : String.Empty;
            this.WriteVerbose("{0} iterations in {1:0.000} core-s and {2:0.000}s clock time ({3:0.00} {4}iterations/core-s).", totalMoves, totalSeconds, elapsedTime.TotalSeconds, iterationsPerSecondMultiplier * iterationsPerSecond, iterationsPerSecondScale);
        }

        private bool TryCreateFirstThin(int firstThinPeriodIndex, [NotNullWhen(true)] out IHarvest? firstThin)
        {
            return this.TryCreateThin(this.FirstThinPeriod[firstThinPeriodIndex], out firstThin);
        }

        private bool TryCreateSecondThin(int secondThinPeriodIndex, [NotNullWhen(true)] out IHarvest? secondThin)
        {
            return this.TryCreateThin(this.SecondThinPeriod[secondThinPeriodIndex], out secondThin);
        }

        private bool TryCreateThirdThin(int thirdThinPeriodIndex, [NotNullWhen(true)] out IHarvest? thirdThin)
        {
            return this.TryCreateThin(this.ThirdThinPeriod[thirdThinPeriodIndex], out thirdThin);
        }

        private bool TryCreateThin(int thinPeriodIndex, [NotNullWhen(true)] out IHarvest? thin)
        {
            if (thinPeriodIndex == Constant.NoThinPeriod)
            {
                thin = null;
                return false;
            }

            thin = this.CreateThin(thinPeriodIndex);
            return true;
        }

        protected void WriteVerbose(string format, params object[] args)
        {
            base.WriteVerbose(String.Format(format, args));
        }
    }
}
