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
        [ValidateRange(0, 1000 * 1000)]
        public Nullable<int> ChainFrom { get; set; }

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

        [Parameter]
        public VolumeUnits VolumeUnits { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.ChainFrom = null;
            this.Cores = 4;
            this.DiscountRate = 4; // percent per year
            this.HarvestPeriods = new List<int>() { 3 };
            this.LandExpectationValue = false;
            this.PlanningPeriods = new List<int>() { 9 };
            this.TreeModel = TreeModel.OrganonNwo;
            this.ProportionalPercentage = new List<float>() { 50.0F };
            this.VolumeUnits = VolumeUnits.CubicMetersPerHectare;
        }

        protected virtual IHarvest CreateHarvest(int harvestPeriodIndex)
        {
            return new ThinByIndividualTreeSelection(this.HarvestPeriods[harvestPeriodIndex]);
        }

        protected abstract Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, TParameters parameters);
        protected abstract string GetName();
        protected abstract IList<TParameters> GetParameterCombinations();

        protected override void ProcessRecord()
        {
            if (this.HarvestPeriods.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.HarvestPeriods));
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
                VolumeUnits = this.VolumeUnits
            };

            IList<TParameters> parameterCombinations = this.GetParameterCombinations();
            List<HeuristicSolutionDistribution> distributions = new List<HeuristicSolutionDistribution>(parameterCombinations.Count * this.HarvestPeriods.Count * this.PlanningPeriods.Count);
            for (int planningPeriodIndex = 0; planningPeriodIndex < this.PlanningPeriods.Count; ++planningPeriodIndex)
            {
                for (int harvestPeriodIndex = 0; harvestPeriodIndex < this.HarvestPeriods.Count; ++harvestPeriodIndex)
                {
                    for (int parameterIndex = 0; parameterIndex < parameterCombinations.Count; ++parameterIndex)
                    {
                        distributions.Add(new HeuristicSolutionDistribution());
                    }
                }
            }

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = this.Cores
            };
            Pseudorandom pseudorandom = new Pseudorandom();
            int totalRuns = this.BestOf * parameterCombinations.Count * this.HarvestPeriods.Count * this.PlanningPeriods.Count;
            int runsCompleted = 0;
            List<StandTrajectory> trajectoriesForChaining = new List<StandTrajectory>();
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
                    if (this.ChainFrom.HasValue && (trajectoriesForChaining.Count > 0))
                    {
                        int previousSolutionIndex = pseudorandom.Next(trajectoriesForChaining.Count);
                        currentHeuristic.CopySelectionsFrom(trajectoriesForChaining[previousSolutionIndex]);
                    }
                    else if (runParameters.ProportionalPercentage > 0.0F)
                    {
                        // minor optimization point: save one stand simulation by skipping this for genetic algorithms
                        currentHeuristic.RandomizeSelections(runParameters.ProportionalPercentage);
                    }
                    TimeSpan runTime = currentHeuristic.Run();

                    lock (distributions)
                    {
                        HeuristicSolutionDistribution distribution = distributions[distributionIndex];
                        distribution.AddRun(currentHeuristic, runTime, runParameters);
                        ++runsCompleted;

                        StandTrajectory trajectoryForChaining = currentHeuristic.BestTrajectoryByMove.Values.FirstOrDefault();
                        if (trajectoryForChaining != null)
                        {
                            trajectoriesForChaining.Add(trajectoryForChaining);
                        }
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

        protected IList<HeuristicParameters> ProportionalPercentagesAsHeuristicParameters()
        {
            List<HeuristicParameters> parameters = new List<HeuristicParameters>(this.ProportionalPercentage.Count);
            foreach (float proportionalPercentage in this.ProportionalPercentage)
            {
                parameters.Add(new HeuristicParameters()
                {
                    ProportionalPercentage = proportionalPercentage
                });
            }
            return parameters;
        }

        private void WriteMultipleDistributionSummary(List<HeuristicSolutionDistribution> distributions, TimeSpan elapsedTime)
        {
            Heuristic firstHeuristic = distributions[0].BestSolution;
            base.WriteVerbose(String.Empty); // Visual Studio code workaround
            this.WriteVerbose("{0}: {1} configurations ({2} runs) in {3:0.00} minutes.", firstHeuristic.GetName(), distributions.Count, this.BestOf * distributions.Count, elapsedTime.TotalMinutes);
        }

        private void WriteSingleDistributionSummary(HeuristicSolutionDistribution distribution, TimeSpan elapsedTime)
        {
            Heuristic bestHeuristic = distribution.BestSolution;
            int movesAccepted = 1;
            int movesRejected = 0;
            float previousObjectiveFunction = bestHeuristic.ObjectiveFunctionByMove[0];
            for (int index = 1; index < bestHeuristic.ObjectiveFunctionByMove.Count; ++index)
            {
                float currentObjectiveFunction = bestHeuristic.ObjectiveFunctionByMove[index];
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
            for (int periodIndex = 1; periodIndex < bestHeuristic.BestTrajectory.HarvestVolumesByPeriod.Length; ++periodIndex)
            {
                float harvest = bestHeuristic.BestTrajectory.HarvestVolumesByPeriod[periodIndex];
                maximumHarvest = Math.Max(harvest, maximumHarvest);
                harvestSum += harvest;
                harvestSumOfSquares += harvest * harvest;
                minimumHarvest = Math.Min(harvest, minimumHarvest);
            }
            float periods = (float)(bestHeuristic.BestTrajectory.HarvestVolumesByPeriod.Length - 1);
            float meanHarvest = harvestSum / periods;
            float variance = harvestSumOfSquares / periods - meanHarvest * meanHarvest;
            float standardDeviation = MathF.Sqrt(variance);
            float flowEvenness = Math.Max(maximumHarvest - meanHarvest, meanHarvest - minimumHarvest) / meanHarvest;
            if (bestHeuristic.BestTrajectory.VolumeUnits == VolumeUnits.ScribnerBoardFeetPerAcre)
            {
                // convert from BF to MBF
                meanHarvest *= 0.001F;
                // variance *= 0.001 * 0.001;
                standardDeviation *= 0.001F;
            }

            base.WriteVerbose(String.Empty); // Visual Studio code workaround
            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0}: {1} moves, {2} changing ({3:0%}), {4} unchanging ({5:0%})", bestHeuristic.GetName(), totalMoves, movesAccepted, (float)movesAccepted / (float)totalMoves, movesRejected, (float)movesRejected / (float)totalMoves);
            this.WriteVerbose("objective: best {0:0.00#}, mean {1:0.00#} ending {2:0.00#}.", bestHeuristic.BestObjectiveFunction, distribution.BestObjectiveFunctionBySolution.Average(), bestHeuristic.ObjectiveFunctionByMove.Last());
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
