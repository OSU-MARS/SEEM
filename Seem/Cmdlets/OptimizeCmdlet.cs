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
        [ValidateNotNull]
        [ValidateRange(0.0F, 100.0F)]
        public List<float> DiscountRates { get; set; }

        [Parameter]
        [ValidateNotNull]
        [ValidateRange(Constant.NoThinPeriod, 100)]
        public List<int> FirstThinPeriod { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 1.0F)]
        public float PerturbBy { get; set; }
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
            this.PerturbBy = Constant.MetaheuristicDefault.PerturbBy;
            this.ProportionalPercentage = new List<float>() { Constant.HeuristicDefault.ProportionalPercentage };
            this.ScaledVolume = false;
            this.SecondThinPeriod = new List<int>() { Constant.NoThinPeriod };
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
                    PerturbBy = this.PerturbBy,
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
            if ((this.PerturbBy < 0.0F) || (this.PerturbBy > 1.0F))
            {
                throw new ParameterOutOfRangeException(nameof(this.PerturbBy));
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

            int treeCount = this.Stand!.GetTreeRecordCount();
            List<TParameters> parameterCombinations = new();
            List<HeuristicSolutionDistribution> distributions = new(this.FirstThinPeriod.Count * this.PlanningPeriods.Count);
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

                            for (int parameterIndex = 0; parameterIndex < parameterCombinationsForDiscountRate.Count; ++parameterIndex)
                            {
                                distributions.Add(new HeuristicSolutionDistribution(1, treeCount)
                                {
                                    FirstThinPeriodIndex = firstThinIndex,
                                    ParameterIndex = parameterCombinations.Count + parameterIndex,
                                    PlanningPeriodIndex = planningPeriodIndex,
                                    SecondThinPeriodIndex = secondThinIndex
                                });
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
            int totalRuns = this.BestOf * distributions.Count;
            int runsCompleted = 0;
            Task runs = Task.Run(() =>
            {
                Parallel.For(0, totalRuns, parallelOptions, (int iteration, ParallelLoopState loopState) =>
                {
                    if (loopState.ShouldExitCurrentIteration)
                    {
                        return;
                    }

                    int distributionIndex = iteration / this.BestOf;
                    HeuristicSolutionDistribution distribution = distributions[distributionIndex];
                    OrganonConfiguration organonConfiguration = new(OrganonVariant.Create(this.TreeModel));
                    if (this.TryCreateFirstThin(distribution.FirstThinPeriodIndex, out IHarvest? firstThin))
                    {
                        organonConfiguration.Treatments.Harvests.Add(firstThin);
                        if (this.TryCreateSecondThin(distribution.SecondThinPeriodIndex, out IHarvest? secondThin))
                        {
                            organonConfiguration.Treatments.Harvests.Add(secondThin);
                        }
                    }

                    Objective objective = new()
                    {
                        PlanningPeriods = this.PlanningPeriods[distribution.PlanningPeriodIndex],
                        TimberObjective = this.TimberObjective
                    };
                    TParameters runParameters = parameterCombinations[distribution.ParameterIndex];
                    Heuristic currentHeuristic = this.CreateHeuristic(organonConfiguration, objective, runParameters);
                    if (runParameters.PerturbBy > 0.0F)
                    {
                        if ((runParameters.PerturbBy == 1.0F) || (distribution.EliteSolutions.NewIndividuals == 0))
                        {
                            // minor optimization point: save a few time steps by by re-using pre-thin results
                            // minor optimization point: save one loop over stand by skipping this for genetic algorithms
                            currentHeuristic.RandomizeTreeSelection(runParameters.ProportionalPercentage);
                        }
                        else
                        {
                            // TODO: support initialization from unperturbed elite solutions
                            // TODO: intialize genetic algorithm population from elite solutions?
                            // TODO: how to define generation statistics?
                            // TODO: more granular locking?
                            lock (distributions)
                            {
                                currentHeuristic.RandomizeTreeSelectionFrom(runParameters.PerturbBy, distribution.EliteSolutions);
                            }
                        }
                    }
                    TimeSpan runTime = currentHeuristic.Run();

                    lock (distributions)
                    {
                        distribution.AddRun(currentHeuristic, runTime, runParameters);
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

            foreach (HeuristicSolutionDistribution distribution in distributions)
            {
                distribution.OnRunsComplete();
            }
            stopwatch.Stop();

            this.WriteObject(distributions);
            if (distributions.Count == 1)
            {
                this.WriteSingleDistributionSummary(distributions[0], stopwatch.Elapsed);
            }
            else if (distributions.Count > 1)
            {
                this.WriteMultipleDistributionSummary(distributions, stopwatch.Elapsed);
            }
            else
            {
                this.WriteWarning("No valid parameter combininations found. No runs performed.");
            }
        }

        private void WriteMultipleDistributionSummary(List<HeuristicSolutionDistribution> distributions, TimeSpan elapsedTime)
        {
            Heuristic? firstHeuristic = distributions[0].HighestSolution;
            if (firstHeuristic == null)
            {
                throw new ArgumentOutOfRangeException(nameof(distributions));
            }

            base.WriteVerbose(String.Empty); // Visual Studio code workaround
            this.WriteVerbose("{0}: {1} configurations ({2} runs) in {3:0.00} minutes.", firstHeuristic.GetName(), distributions.Count, this.BestOf * distributions.Count, elapsedTime.TotalMinutes);
        }

        private void WriteSingleDistributionSummary(HeuristicSolutionDistribution distribution, TimeSpan elapsedTime)
        {
            Heuristic? bestHeuristic = distribution.HighestSolution;
            if (bestHeuristic == null)
            {
                throw new ArgumentOutOfRangeException(nameof(distribution));
            }

            int movesAccepted = 1;
            int movesRejected = 0;
            float previousObjectiveFunction = bestHeuristic.AcceptedObjectiveFunctionByMove[0];
            for (int index = 1; index < bestHeuristic.AcceptedObjectiveFunctionByMove.Count; ++index)
            {
                float currentObjectiveFunction = bestHeuristic.AcceptedObjectiveFunctionByMove[index];
                if (currentObjectiveFunction != previousObjectiveFunction)
                {
                    ++movesAccepted;
                }
                else
                {
                    ++movesRejected;
                }
                previousObjectiveFunction = currentObjectiveFunction;
            }

            float maximumHarvest = Single.MinValue;
            float minimumHarvest = Single.MaxValue;
            float harvestSum = 0.0F;
            float harvestSumOfSquares = 0.0F;
            for (int periodIndex = 1; periodIndex < bestHeuristic.BestTrajectory.PlanningPeriods; ++periodIndex)
            {
                float harvestVolumeScribner = bestHeuristic.BestTrajectory.ThinningVolume.ScribnerTotal[periodIndex];
                maximumHarvest = Math.Max(harvestVolumeScribner, maximumHarvest);
                harvestSum += harvestVolumeScribner;
                harvestSumOfSquares += harvestVolumeScribner * harvestVolumeScribner;
                minimumHarvest = Math.Min(harvestVolumeScribner, minimumHarvest);
            }
            float periods = (float)(bestHeuristic.BestTrajectory.PlanningPeriods - 1);
            float meanHarvest = harvestSum / periods;
            float variance = harvestSumOfSquares / periods - meanHarvest * meanHarvest;
            float standardDeviation = MathF.Sqrt(variance);
            float flowEvenness = Math.Max(maximumHarvest - meanHarvest, meanHarvest - minimumHarvest) / meanHarvest;

            base.WriteVerbose(String.Empty); // Visual Studio code workaround
            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0}: {1} moves, {2} changing ({3:0%}), {4} unchanging ({5:0%})", bestHeuristic.GetName(), totalMoves, movesAccepted, (float)movesAccepted / (float)totalMoves, movesRejected, (float)movesRejected / (float)totalMoves);
            this.WriteVerbose("objective: best {0:0.00#}, mean {1:0.00#} ending {2:0.00#}.", bestHeuristic.BestObjectiveFunction, distribution.BestObjectiveFunctionBySolution.Average(), bestHeuristic.AcceptedObjectiveFunctionByMove.Last());
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
