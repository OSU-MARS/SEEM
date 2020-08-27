using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int Cores { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 30.0F)]
        public float DiscountRate { get; set; }

        [Parameter]
        [ValidateNotNull]
        [ValidateRange(1, 100)]
        public List<int> HarvestPeriods { get; set; }

        [Parameter]
        public SwitchParameter LandExpectationValue { get; set; }
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

        [Parameter(Mandatory = true)]
        public OrganonStand Stand { get; set; }
        [Parameter]
        public TreeModel TreeModel { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.Cores = 4;
            this.DiscountRate = 4; // percent per year
            this.HarvestPeriods = new List<int>() { 3 };
            this.LandExpectationValue = false;
            this.PlanningPeriods = new List<int>() { 9 };
            this.PerturbBy = Constant.MetaheuristicDefault.PerturbBy;
            this.TreeModel = TreeModel.OrganonNwo;
            this.ProportionalPercentage = new List<float>() { 50.0F };
        }

        protected virtual IHarvest CreateHarvest(int harvestPeriodIndex)
        {
            return new ThinByIndividualTreeSelection(this.HarvestPeriods[harvestPeriodIndex]);
        }

        protected abstract Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, TParameters parameters);

        protected IList<HeuristicParameters> GetDefaultParameterCombinations()
        {
            List<HeuristicParameters> parameters = new List<HeuristicParameters>(this.ProportionalPercentage.Count);
            foreach (float proportionalPercentage in this.ProportionalPercentage)
            {
                parameters.Add(new HeuristicParameters()
                {
                    PerturbBy = this.PerturbBy,
                    ProportionalPercentage = proportionalPercentage
                });
            }
            return parameters;
        }

        protected abstract string GetName();
        protected abstract IList<TParameters> GetParameterCombinations();

        protected override void ProcessRecord()
        {
            if (this.HarvestPeriods.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.HarvestPeriods));
            }
            if ((this.PerturbBy < 0.0F) || (this.PerturbBy > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.PerturbBy));
            }
            if (this.PlanningPeriods.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.PlanningPeriods));
            }
            if (this.ProportionalPercentage.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ProportionalPercentage));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Objective objective = new Objective()
            {
                DiscountRate = 0.01F * this.DiscountRate,
                IsLandExpectationValue = this.LandExpectationValue,
            };

            IList<TParameters> parameterCombinations = this.GetParameterCombinations();
            int treeCount = this.Stand.GetTreeRecordCount();
            List<HeuristicSolutionDistribution> distributions = new List<HeuristicSolutionDistribution>(parameterCombinations.Count * this.HarvestPeriods.Count * this.PlanningPeriods.Count);
            for (int planningPeriodIndex = 0; planningPeriodIndex < this.PlanningPeriods.Count; ++planningPeriodIndex)
            {
                for (int harvestPeriodIndex = 0; harvestPeriodIndex < this.HarvestPeriods.Count; ++harvestPeriodIndex)
                {
                    int harvestPeriods = this.HarvestPeriods[harvestPeriodIndex];
                    for (int parameterIndex = 0; parameterIndex < parameterCombinations.Count; ++parameterIndex)
                    {
                        distributions.Add(new HeuristicSolutionDistribution(1, harvestPeriods, treeCount));
                    }
                }
            }

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = this.Cores
            };
            int totalRuns = this.BestOf * parameterCombinations.Count * this.HarvestPeriods.Count * this.PlanningPeriods.Count;
            int runsCompleted = 0;
            Task runs = Task.Run(() =>
            {
                Parallel.For(0, totalRuns, parallelOptions, (int iteration, ParallelLoopState loopState) =>
                {
                    if (loopState.ShouldExitCurrentIteration)
                    {
                        return;
                    }

                    int parameterIndex = (iteration / this.BestOf) % parameterCombinations.Count; // innermost "loop": tree selection probability
                    int harvestPeriodIndex = (iteration / (this.BestOf * parameterCombinations.Count)) % this.HarvestPeriods.Count; // middle "loop": harvest timings
                    int planningPeriodIndex = (iteration / (this.BestOf * this.HarvestPeriods.Count * parameterCombinations.Count)) % this.PlanningPeriods.Count; // outer "loop": rotation length
                    int distributionIndex = parameterIndex + harvestPeriodIndex * parameterCombinations.Count + planningPeriodIndex * this.HarvestPeriods.Count * parameterCombinations.Count;

                    OrganonConfiguration organonConfiguration = new OrganonConfiguration(OrganonVariant.Create(this.TreeModel));
                    organonConfiguration.Treatments.Harvests.Add(this.CreateHarvest(harvestPeriodIndex));

                    TParameters runParameters = parameterCombinations[parameterIndex];
                    Heuristic currentHeuristic = this.CreateHeuristic(organonConfiguration, this.PlanningPeriods[planningPeriodIndex], objective, runParameters);
                    HeuristicSolutionDistribution distribution = distributions[distributionIndex];
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

            if (totalRuns == 1) 
            {
                // only one selection probability, harvest period, and rotation length, so maximize responsiveness
                // Also a partial workaround for a Visual Studio Code bug where the first line of cmdlet verbose output is overwritten if WriteProgress()
                // is used.
                runs.Wait();
            }
            else
            {
                string name = this.GetName();
                int sleepsSinceLastStatusUpdate = 0;
                while (runs.IsCompleted == false)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1.0));
                    ++sleepsSinceLastStatusUpdate;

                    if (runs.IsFaulted)
                    {
                        // per https://stackoverflow.com/questions/20170527/how-to-correctly-rethrow-an-exception-of-task-already-in-faulted-state
                        ExceptionDispatchInfo.Capture(runs.Exception.InnerException).Throw();
                    }
                    if (sleepsSinceLastStatusUpdate > 30)
                    {
                        double fractionComplete = (double)runsCompleted / (double)totalRuns;
                        double secondsElapsed = stopwatch.Elapsed.TotalSeconds;
                        double secondsRemaining = secondsElapsed * (1.0 / fractionComplete - 1.0);
                        this.WriteProgress(new ProgressRecord(0, name, String.Format(runsCompleted + " of " + totalRuns + " runs completed."))
                        {
                            PercentComplete = (int)(100.0 * fractionComplete),
                            SecondsRemaining = (int)Math.Round(secondsRemaining)
                        });
                        sleepsSinceLastStatusUpdate = 0;
                    }
                }
                runs.GetAwaiter().GetResult(); // propagate any exceptions since last IsFaulted check
            }
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
            else
            {
                this.WriteMultipleDistributionSummary(distributions, stopwatch.Elapsed);
            }
        }

        private void WriteMultipleDistributionSummary(List<HeuristicSolutionDistribution> distributions, TimeSpan elapsedTime)
        {
            Heuristic firstHeuristic = distributions[0].HighestSolution;
            base.WriteVerbose(String.Empty); // Visual Studio code workaround
            this.WriteVerbose("{0}: {1} configurations ({2} runs) in {3:0.00} minutes.", firstHeuristic.GetName(), distributions.Count, this.BestOf * distributions.Count, elapsedTime.TotalMinutes);
        }

        private void WriteSingleDistributionSummary(HeuristicSolutionDistribution distribution, TimeSpan elapsedTime)
        {
            Heuristic bestHeuristic = distribution.HighestSolution;
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
            for (int periodIndex = 1; periodIndex < bestHeuristic.BestTrajectory.HarvestVolume.Scribner.Length; ++periodIndex)
            {
                float harvestVolumeScribner = bestHeuristic.BestTrajectory.HarvestVolume.Scribner[periodIndex];
                maximumHarvest = Math.Max(harvestVolumeScribner, maximumHarvest);
                harvestSum += harvestVolumeScribner;
                harvestSumOfSquares += harvestVolumeScribner * harvestVolumeScribner;
                minimumHarvest = Math.Min(harvestVolumeScribner, minimumHarvest);
            }
            float periods = (float)(bestHeuristic.BestTrajectory.HarvestVolume.Scribner.Length - 1);
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

        protected void WriteVerbose(string format, params object[] args)
        {
            base.WriteVerbose(String.Format(format, args));
        }
    }
}
