using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Runtime.ExceptionServices;
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
        [ValidateRange(0.0F, 1.0F)]
        public float ConstructionRandomness { get; set; }

        [Parameter]
        [ValidateNotNull]
        [ValidateRange(0.0F, 100.0F)]
        public List<float> DiscountRates { get; set; }

        [Parameter]
        [ValidateNotNull]
        [ValidateRange(Constant.NoThinPeriod, 100)]
        public List<int> FirstThinPeriod { get; set; }

        [Parameter]
        [ValidateNotNull]
        [ValidateRange(1, 100)]
        public List<int> PlanningPeriods { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public List<float> ProportionalPercentage { get; set; }

        [Parameter]
        [ValidateNotNull]
        [ValidateRange(Constant.NoThinPeriod, 100)]
        public List<int> SecondThinPeriod { get; set; }

        [Parameter]
        public SwitchParameter ScaledVolume { get; set; }
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand? Stand { get; set; }

        [Parameter]
        [ValidateNotNull]
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
            this.FirstThinPeriod = new List<int>() { 3 };
            this.PlanningPeriods = new List<int>() { 9 };
            this.ConstructionRandomness = Constant.GraspDefault.FullyRandomConstruction;
            this.ProportionalPercentage = new List<float>() { Constant.HeuristicDefault.ProportionalPercentage };
            this.ScaledVolume = false;
            this.SecondThinPeriod = new List<int>() { Constant.NoThinPeriod };
            this.ThirdThinPeriod = new List<int>() { Constant.NoThinPeriod };
            this.TimberObjective = TimberObjective.LandExpectationValue;
            this.TimberValue = TimberValue.Default;
            this.TreeModel = TreeModel.OrganonNwo;
        }

        protected abstract Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective, TParameters parameters);

        protected virtual IHarvest CreateThin(int thinPeriodIndex)
        {
            return new ThinByIndividualTreeSelection(thinPeriodIndex);
        }

        protected IList<HeuristicParameters> GetDefaultParameterCombinations(TimberValue timberValue)
        {
            List<HeuristicParameters> parameterCombinations = new();
            foreach (float proportionalPercentage in this.ProportionalPercentage)
            {
                parameterCombinations.Add(new HeuristicParameters()
                {
                    ConstructionRandomness = this.ConstructionRandomness,
                    ProportionalPercentage = proportionalPercentage,
                    TimberValue = timberValue,
                    UseFiaVolume = this.ScaledVolume
                });
            }
            return parameterCombinations;
        }

        protected abstract string GetName();
        protected abstract IList<TParameters> GetParameterCombinations(TimberValue timberValue);

        protected override void ProcessRecord()
        {
            if (this.FirstThinPeriod.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.FirstThinPeriod));
            }
            if ((this.ConstructionRandomness < 0.0F) || (this.ConstructionRandomness > 1.0F))
            {
                throw new ParameterOutOfRangeException(nameof(this.ConstructionRandomness));
            }
            if (this.PlanningPeriods.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.PlanningPeriods));
            }
            if (this.ProportionalPercentage.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.ProportionalPercentage));
            }
            if (this.SecondThinPeriod.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.SecondThinPeriod));
            }
            if (this.ThirdThinPeriod.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.SecondThinPeriod));
            }

            // for now, if multiple discount rates are specified assume they should override the discount rate on TimberValue
            if (this.DiscountRates.Count == 1)
            {
                if (this.TimberValue.DiscountRate != Constant.DefaultAnnualDiscountRate)
                {
                    // conflicting single discount rates
                    if ((this.DiscountRates[0] != Constant.DefaultAnnualDiscountRate) &&
                        (this.DiscountRates[0] != this.TimberValue.DiscountRate))
                    {
                        throw new NotSupportedException("The single, non-default discount rate " + this.DiscountRates[0].ToString("0.00") + " was specified but the discount rate set on TimberValue is the non-default " + this.TimberValue.DiscountRate.ToString("0.00") + ".  Resolve this conflict by not specifying a discount rate, using the same discount rate in both locations, or specifying a default discount rate in the location which should be overridden.");
                    }

                    // override default discount rate with non-default value 
                    this.DiscountRates[0] = this.TimberValue.DiscountRate;
                }
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters totalPerfCounters = new();

            int treeCount = this.Stand!.GetTreeRecordCount();
            List<TParameters> parameterCombinations = new();
            HeuristicResultSet results = new(this.DiscountRates, this.FirstThinPeriod, this.SecondThinPeriod, this.ThirdThinPeriod, this.PlanningPeriods);
            for (int discountRateIndex = 0; discountRateIndex < this.DiscountRates.Count; ++discountRateIndex)
            {
                TimberValue timberValue = new(this.TimberValue)
                {
                    DiscountRate = this.DiscountRates[discountRateIndex]
                };
                IList<TParameters> parameterCombinationsForDiscountRate = this.GetParameterCombinations(timberValue);
                for (int planningPeriodIndex = 0; planningPeriodIndex < this.PlanningPeriods.Count; ++planningPeriodIndex)
                {
                    int planningPeriods = this.PlanningPeriods[planningPeriodIndex];
                    for (int firstThinIndex = 0; firstThinIndex < this.FirstThinPeriod.Count; ++firstThinIndex)
                    {
                        int firstThinPeriod = this.FirstThinPeriod[firstThinIndex];
                        if (firstThinPeriod == 0)
                        {
                            throw new ParameterOutOfRangeException(nameof(this.FirstThinPeriod), "First thinning period cannot be zero.");
                        }
                        if (firstThinPeriod >= planningPeriods) // minimum 10 years between thinning and final harvest (if five year time step)
                        {
                            continue;
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
                                if ((firstThinPeriod == Constant.NoThinPeriod) || (firstThinPeriod >= secondThinPeriod) || (secondThinPeriod >= planningPeriods))
                                {
                                    // can't perform a second thin if
                                    // - there was no first thin
                                    // - the second thin would occur before or in the same period as the first thin
                                    // - the second thin would occur before or in the same period as final harvest
                                    continue;
                                }
                            }

                            for (int thirdThinIndex = 0; thirdThinIndex < this.ThirdThinPeriod.Count; ++thirdThinIndex)
                            {
                                int thirdThinPeriod = this.ThirdThinPeriod[thirdThinIndex];
                                if (thirdThinPeriod == 0)
                                {
                                    throw new ParameterOutOfRangeException(nameof(this.ThirdThinPeriod), "Third thinning period cannot be zero.");
                                }
                                if (thirdThinPeriod != Constant.NoThinPeriod)
                                {
                                    if ((secondThinPeriod == Constant.NoThinPeriod) || (secondThinPeriod >= thirdThinPeriod) || (thirdThinPeriod >= planningPeriods))
                                    {
                                        // can't perform a third thin if
                                        // - there was no second thin
                                        // - the third thin would occur before or in the same period as the second thin
                                        // - the third thin would occur before or in the same period as final harvest
                                        continue;
                                    }
                                }

                                for (int parameterIndex = 0; parameterIndex < parameterCombinationsForDiscountRate.Count; ++parameterIndex)
                                {
                                    results.Add(new HeuristicDistribution(treeCount)
                                    {
                                        DiscountRateIndex = discountRateIndex,
                                        FirstThinPeriodIndex = firstThinIndex,
                                        ParameterIndex = parameterCombinations.Count + parameterIndex,
                                        PlanningPeriodIndex = planningPeriodIndex,
                                        SecondThinPeriodIndex = secondThinIndex,
                                        ThirdThinPeriodIndex = thirdThinIndex
                                    });
                                }
                            }
                        }
                    }
                }

                parameterCombinations.AddRange(parameterCombinationsForDiscountRate);
            }
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = this.Threads
            };
            int totalRuns = this.BestOf * results.Distributions.Count;
            int runsCompleted = 0;
            Task runs = Task.Run(() =>
            {
                Parallel.For(0, totalRuns, parallelOptions, (int iteration, ParallelLoopState loopState) =>
                {
                    if (loopState.ShouldExitCurrentIteration)
                    {
                        return;
                    }

                    int distributionAndSolutionIndex = iteration / this.BestOf;
                    HeuristicDistribution distribution = results.Distributions[distributionAndSolutionIndex];
                    OrganonConfiguration organonConfiguration = new(OrganonVariant.Create(this.TreeModel));
                    if (this.TryCreateFirstThin(distribution.FirstThinPeriodIndex, out IHarvest? firstThin))
                    {
                        organonConfiguration.Treatments.Harvests.Add(firstThin);
                        if (this.TryCreateSecondThin(distribution.SecondThinPeriodIndex, out IHarvest? secondThin))
                        {
                            organonConfiguration.Treatments.Harvests.Add(secondThin);
                            if (this.TryCreateThirdThin(distribution.ThirdThinPeriodIndex, out IHarvest? thirdThin))
                            {
                                organonConfiguration.Treatments.Harvests.Add(thirdThin);
                            }
                        }
                    }

                    Objective objective = new()
                    {
                        PlanningPeriods = this.PlanningPeriods[distribution.PlanningPeriodIndex],
                        TimberObjective = this.TimberObjective
                    };
                    TParameters runParameters = parameterCombinations[distribution.ParameterIndex];
                    // TODO: save a few time steps by re-using pre-thin results
                    Heuristic currentHeuristic = this.CreateHeuristic(organonConfiguration, objective, runParameters);
                    // TODO: override ConstructTreeSelection() for population initialization in GeneticAlgorithm
                    currentHeuristic.ConstructTreeSelection(runParameters, distribution, results.SolutionIndex);
                    HeuristicPerformanceCounters perfCounters = currentHeuristic.Run();

                    HeuristicSolutionPool solutionPool = results.Solutions[distributionAndSolutionIndex];
                    lock (results)
                    {
                        distribution.AddRun(currentHeuristic, perfCounters, runParameters);
                        solutionPool.AddRun(currentHeuristic);
                        totalPerfCounters += perfCounters;
                        ++runsCompleted;
                    }

                    if (this.Stopping)
                    {
                        loopState.Stop();
                    }
                });
            });

            string name = this.GetName();
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
                    double fractionComplete = (double)runsCompleted / (double)totalRuns;
                    double secondsElapsed = stopwatch.Elapsed.TotalSeconds;
                    double secondsRemaining = secondsElapsed * (1.0 / fractionComplete - 1.0);
                    this.WriteProgress(new ProgressRecord(0, name, String.Format(runsCompleted + " of " + totalRuns + " runs completed by " + this.Threads + " threads."))
                    {
                        PercentComplete = (int)(100.0 * fractionComplete),
                        SecondsRemaining = (int)Math.Round(secondsRemaining)
                    });
                    sleepsSinceLastStatusUpdate = 0;
                }
            }
            runs.GetAwaiter().GetResult(); // propagate any exceptions since last IsFaulted check

            foreach (HeuristicDistribution distribution in results.Distributions)
            {
                distribution.OnRunsComplete();
            }
            stopwatch.Stop();


            this.WriteObject(results);
            if (results.Distributions.Count == 1)
            {
                this.WriteSingleDistributionSummary(results.Solutions[0].Highest, results.Distributions[0], totalPerfCounters, stopwatch.Elapsed);
            }
            else if (results.Distributions.Count > 1)
            {
                this.WriteMultipleDistributionSummary(results, totalPerfCounters, stopwatch.Elapsed);
            }
            else
            {
                this.WriteWarning("No valid parameter combininations found. No runs performed.");
            }
        }

        private void WriteMultipleDistributionSummary(HeuristicResultSet results, HeuristicPerformanceCounters totalPerfCounters, TimeSpan elapsedTime)
        {
            Heuristic? firstHeuristic = results.Solutions[0].Highest;
            if (firstHeuristic == null)
            {
                throw new ArgumentOutOfRangeException(nameof(results));
            }

            base.WriteVerbose(String.Empty); // Visual Studio code workaround
            this.WriteVerbose("{0}: {1} configurations with {2} runs in {3:0.00} minutes ({4:0.00}M timesteps in {5:0.00} core-minutes, {6:0.00}% move acceptance).", 
                              firstHeuristic.GetName(), 
                              results.Distributions.Count, 
                              this.BestOf * results.Distributions.Count, 
                              elapsedTime.TotalMinutes,
                              1E-6F * totalPerfCounters.GrowthModelTimesteps,
                              totalPerfCounters.Duration.TotalMinutes,
                              100.0F * totalPerfCounters.MovesAccepted / (totalPerfCounters.MovesAccepted + totalPerfCounters.MovesRejected));
        }

        private void WriteSingleDistributionSummary(Heuristic? heuristic, HeuristicDistribution distribution, HeuristicPerformanceCounters totalPerfCounters, TimeSpan elapsedTime)
        {
            if (heuristic == null)
            {
                throw new ArgumentNullException(nameof(heuristic));
            }

            float maximumHarvest = Single.MinValue;
            float minimumHarvest = Single.MaxValue;
            float harvestSum = 0.0F;
            float harvestSumOfSquares = 0.0F;
            for (int periodIndex = 1; periodIndex < heuristic.BestTrajectory.PlanningPeriods; ++periodIndex)
            {
                float harvestVolumeScribner = heuristic.BestTrajectory.ThinningVolume.ScribnerTotal[periodIndex];
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
            this.WriteVerbose("objective: best {0:0.00#}, mean {1:0.00#} ending {2:0.00#}.", heuristic.BestObjectiveFunction, distribution.BestObjectiveFunctionBySolution.Average(), heuristic.AcceptedObjectiveFunctionByMove.Last());
            this.WriteVerbose("flow: {0:0.0#} mean, {1:0.000} σ, {2:0.000}% even, {3:0.0#}-{4:0.0#} = range {5:0.0}.", meanHarvest, standardDeviation, 1E2 * flowEvenness, minimumHarvest, maximumHarvest, maximumHarvest - minimumHarvest);

            double iterationsPerSecond = distribution.TotalMoves / distribution.TotalCoreSeconds.TotalSeconds;
            double iterationsPerSecondMultiplier = iterationsPerSecond > 1E3 ? 1E-3 : 1.0;
            string iterationsPerSecondScale = iterationsPerSecond > 1E3 ? "k" : String.Empty;
            this.WriteVerbose("{0} iterations in {1:0.000} core-s and {2:0.000}s clock time ({3:0.00} {4}iterations/core-s).", distribution.TotalMoves, distribution.TotalCoreSeconds.TotalSeconds, elapsedTime.TotalSeconds, iterationsPerSecondMultiplier * iterationsPerSecond, iterationsPerSecondScale);
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
