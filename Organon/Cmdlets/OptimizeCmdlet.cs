using Osu.Cof.Organon.Heuristics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Osu.Cof.Organon.Cmdlets
{
    public abstract class OptimizeCmdlet : Cmdlet
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int BestOf { get; set; }
        [Parameter]
        [ValidateRange(0, 100)]
        public int HarvestPeriods { get; set; }
        [Parameter]
        [ValidateRange(0, 100)]
        public int PlanningPeriods { get; set; }

        [Parameter(Mandatory = true)]
        public Stand Stand { get; set; }
        [Parameter]
        public TreeModel TreeModel { get; set; }
        [Parameter]
        public SwitchParameter UniformHarvestProbability { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.HarvestPeriods = 1;
            this.PlanningPeriods = 9;
            this.TreeModel = TreeModel.OrganonNwo;
            this.UniformHarvestProbability = false;
        }

        protected abstract Heuristic CreateHeuristic();

        protected override void ProcessRecord()
        {
            Heuristic bestHeuristic = null;
            int totalIterations = 0;
            TimeSpan totalRunTime = TimeSpan.Zero;
            List<double> objectiveFunctionValues = new List<double>();
            for (int iteration = 0; iteration < this.BestOf; ++iteration)
            {
                Heuristic currentHeuristic = this.CreateHeuristic();
                if (this.UniformHarvestProbability)
                {
                    currentHeuristic.RandomizeSchedule();
                }
                totalRunTime += currentHeuristic.Run();
                totalIterations += currentHeuristic.ObjectiveFunctionByIteration.Count;
                objectiveFunctionValues.Add(currentHeuristic.BestObjectiveFunction);

                if ((bestHeuristic == null) || (currentHeuristic.BestObjectiveFunction < bestHeuristic.BestObjectiveFunction))
                {
                    bestHeuristic = currentHeuristic;
                }

                if (this.Stopping)
                {
                    break;
                }
            }

            this.WriteObject(bestHeuristic);
            if (this.BestOf > 1)
            {
                this.WriteObject(objectiveFunctionValues);
            }

            this.WriteHeuristicRun(bestHeuristic, objectiveFunctionValues, totalIterations, totalRunTime);
        }

        private void WriteHeuristicRun(Heuristic heuristic, List<double> objectiveFunctionValues, int totalIterations, TimeSpan runTime)
        {
            int movesAccepted = 0;
            int movesRejected = 0;
            double previousObjectiveFunction = heuristic.ObjectiveFunctionByIteration[0];
            for (int index = 1; index < heuristic.ObjectiveFunctionByIteration.Count; ++index)
            {
                double currentObjectiveFunction = heuristic.ObjectiveFunctionByIteration[index];
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

            double maximumHarvest = Double.MinValue;
            double minimumHarvest = Double.MaxValue;
            double sum = 0.0;
            double sumOfSquares = 0.0;
            for (int periodIndex = 1; periodIndex < heuristic.BestTrajectory.HarvestVolumesByPeriod.Length; ++periodIndex)
            {
                double harvest = heuristic.BestTrajectory.HarvestVolumesByPeriod[periodIndex];
                maximumHarvest = Math.Max(harvest, maximumHarvest);
                sum += harvest;
                sumOfSquares += harvest * harvest;
                minimumHarvest = Math.Min(harvest, minimumHarvest);
            }
            double periods = (double)(heuristic.BestTrajectory.HarvestVolumesByPeriod.Length - 1);
            double meanHarvest = sum / periods;
            double variance = sumOfSquares / periods - meanHarvest * meanHarvest;
            double standardDeviation = Math.Sqrt(variance);
            double flowEvenness = Math.Max(maximumHarvest - meanHarvest, meanHarvest - minimumHarvest) / meanHarvest;

            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0}: {1} moves, {2} changing ({3:0%}), {4} unchanging ({5:0%})", heuristic.GetType().Name, totalMoves, movesAccepted, (double)movesAccepted / (double)totalMoves, movesRejected, (double)movesRejected / (double)totalMoves);
            this.WriteVerbose("objective: best {0:0.00#}, mean {1:0.00#} ending {2:0.00#}.", heuristic.BestObjectiveFunction, objectiveFunctionValues.Average(), heuristic.ObjectiveFunctionByIteration.Last());
            this.WriteVerbose("flow: {0:0.0#} mean, {1:0.000} σ, {2:0.000}% even, {3:0.0#}-{4:0.0#} = range {5:0.0}.", meanHarvest, standardDeviation, 1E2 * flowEvenness, minimumHarvest, maximumHarvest, maximumHarvest - minimumHarvest);

            double iterationsPerSecond = (double)totalIterations / runTime.TotalSeconds;
            double iterationsPerSecondMultiplier = iterationsPerSecond > 1E3 ? 1E-3 : 1.0;
            string iterationsPerSecondScale = iterationsPerSecond > 1E3 ? "k" : String.Empty;
            this.WriteVerbose("{0} iterations in {1:0.000}s ({2:0.00} {3}iterations/s).", totalIterations, runTime.TotalSeconds, iterationsPerSecondMultiplier * iterationsPerSecond, iterationsPerSecondScale);
        }

        protected void WriteVerbose(string format, params object[] args)
        {
            base.WriteVerbose(String.Format(format, args));
        }
    }
}
