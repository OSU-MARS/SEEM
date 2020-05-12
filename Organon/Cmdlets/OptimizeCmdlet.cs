using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Osu.Cof.Ferm.Cmdlets
{
    public abstract class OptimizeCmdlet : Cmdlet
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int BestOf { get; set; }
        [Parameter]
        public int Cores { get; set; }

        [Parameter]
        [ValidateRange(0, 100)]
        public int HarvestPeriods { get; set; }
        [Parameter]
        public SwitchParameter NetPresentValue { get; set; }
        [Parameter]
        [ValidateRange(0, 100)]
        public int PlanningPeriods { get; set; }

        [Parameter(Mandatory = true)]
        public OrganonStand Stand { get; set; }
        [Parameter]
        public TreeModel TreeModel { get; set; }
        [Parameter]
        public SwitchParameter UniformHarvestProbability { get; set; }

        [Parameter]
        public VolumeUnits VolumeUnits { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.Cores = 4;
            this.HarvestPeriods = 1;
            this.NetPresentValue = false;
            this.PlanningPeriods = 9;
            this.TreeModel = TreeModel.OrganonNwo;
            this.UniformHarvestProbability = true;
            this.VolumeUnits = VolumeUnits.CubicMetersPerHectare;
        }

        protected abstract Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective);

        protected override void ProcessRecord()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Objective objective = new Objective()
            {
                IsNetPresentValue = this.NetPresentValue,
                VolumeUnits = this.VolumeUnits
            };

            HeuristicSolutionDistribution solutions = new HeuristicSolutionDistribution();
            ParallelOptions parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = this.Cores
            };
            Parallel.For(0, this.BestOf, parallelOptions, (int iteration, ParallelLoopState loopState) => 
            {
                if (loopState.ShouldExitCurrentIteration)
                {
                    return;
                }

                OrganonConfiguration organonConfiguration = new OrganonConfiguration(OrganonVariant.Create(this.TreeModel));
                organonConfiguration.Treatments.Harvests.Add(new ThinByHeuristicIndividualTreeSelection(this.HarvestPeriods));
                Heuristic currentHeuristic = this.CreateHeuristic(organonConfiguration, objective);
                if (this.UniformHarvestProbability)
                {
                    currentHeuristic.RandomizeSchedule();
                }
                TimeSpan runTime = currentHeuristic.Run();

                lock (solutions)
                {
                    solutions.AddRun(currentHeuristic, runTime);
                }

                if (this.Stopping)
                {
                    loopState.Stop();
                }
            });
            solutions.OnRunsComplete();
            stopwatch.Stop();

            this.WriteObject(solutions);
            this.WriteRunSummary(solutions, stopwatch.Elapsed);
        }

        private void WriteRunSummary(HeuristicSolutionDistribution solutions, TimeSpan elapsedTime)
        {
            Heuristic bestHeuristic = solutions.BestSolution;
            int movesAccepted = 0;
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

            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0}: {1} moves, {2} changing ({3:0%}), {4} unchanging ({5:0%})", bestHeuristic.GetName(), totalMoves, movesAccepted, (float)movesAccepted / (float)totalMoves, movesRejected, (float)movesRejected / (float)totalMoves);
            this.WriteVerbose("objective: best {0:0.00#}, mean {1:0.00#} ending {2:0.00#}.", bestHeuristic.BestObjectiveFunction, solutions.BestObjectiveFunctionByRun.Average(), bestHeuristic.ObjectiveFunctionByMove.Last());
            this.WriteVerbose("flow: {0:0.0#} mean, {1:0.000} σ, {2:0.000}% even, {3:0.0#}-{4:0.0#} = range {5:0.0}.", meanHarvest, standardDeviation, 1E2 * flowEvenness, minimumHarvest, maximumHarvest, maximumHarvest - minimumHarvest);

            double iterationsPerSecond = solutions.TotalMoves / solutions.TotalCoreSeconds.TotalSeconds;
            double iterationsPerSecondMultiplier = iterationsPerSecond > 1E3 ? 1E-3 : 1.0;
            string iterationsPerSecondScale = iterationsPerSecond > 1E3 ? "k" : String.Empty;
            this.WriteVerbose("{0} iterations in {1:0.000} core-s and {2:0.000}s clock time ({3:0.00} {4}iterations/core-s).", solutions.TotalMoves, solutions.TotalCoreSeconds.TotalSeconds, elapsedTime.TotalSeconds, iterationsPerSecondMultiplier * iterationsPerSecond, iterationsPerSecondScale);
        }

        protected void WriteVerbose(string format, params object[] args)
        {
            base.WriteVerbose(String.Format(format, args));
        }
    }
}
