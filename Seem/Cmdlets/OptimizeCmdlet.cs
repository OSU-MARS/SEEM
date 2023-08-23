using Mars.Seem.Heuristics;
using Mars.Seem.Optimization;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mars.Seem.Cmdlets
{
    public abstract class OptimizeCmdlet<TParameters> : Cmdlet where TParameters : HeuristicParameters
    {
        [Parameter]
        [ValidateRange(1, 10000)]
        public int BestOf { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(Constant.Grasp.FullyRandomConstructionForMaximization, Constant.Grasp.FullyGreedyConstructionForMaximization)]
        public List<float> ConstructionGreediness { get; set; }

        [Parameter]
        [ValidateNotNull]
        public FinancialScenarios Financial { get; set; }
        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(Constant.NoHarvestPeriod, 100)]
        public List<int> FirstThinAge { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0.0F, 1.0F)]
        public List<float> InitialThinningProbability { get; set; }
        [Parameter]
        public SwitchParameter LogImprovingOnly;

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int MoveCapacity { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0, 200)]
        public List<int> RotationAge { get; set; }
        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(Constant.NoHarvestPeriod, 100)]
        public List<int> SecondThinAge { get; set; }

        [Parameter]
        [ValidateRange(1, 100)]
        public int SolutionPoolSize { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand? Stand { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(Constant.NoHarvestPeriod, 100)]
        public List<int> ThirdThinAge { get; set; }

        [Parameter]
        [ValidateRange(1, 100)]
        public int Threads { get; set; }

        [Parameter]
        public TimberObjective TimberObjective { get; set; }
        [Parameter]
        public TreeModel TreeModel { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.Threads = Environment.ProcessorCount / 2; // assume all cores are hyperthreaded

            this.Financial = FinancialScenarios.Default;
            this.FirstThinAge = new() { Constant.Default.ThinningPeriod };
            this.ConstructionGreediness = new() { Constant.Grasp.DefaultMinimumConstructionGreedinessForMaximization };
            this.LogImprovingOnly = false;
            this.InitialThinningProbability = new() { Constant.HeuristicDefault.InitialThinningProbability };
            this.MoveCapacity = Constant.HeuristicDefault.MoveCapacity;
            this.RotationAge = new() { Constant.Default.RotationLengths };
            this.SecondThinAge = new() { Constant.NoHarvestPeriod };
            this.SolutionPoolSize = Constant.Default.SolutionPoolSize;
            this.ThirdThinAge = new() { Constant.NoHarvestPeriod };
            this.TimberObjective = TimberObjective.LandExpectationValue;
            this.TreeModel = TreeModel.OrganonNwo;
        }

        protected virtual bool HeuristicEvaluatesAcrossRotationsAndScenarios
        {
            get { return false; }
        }

        private IList<int> ConvertAgesToSimulationPeriods(IList<int> agesInYears, float timestepInYears, bool thinningAges)
        {
            Debug.Assert(this.Stand != null);

            float initialStandAge = this.Stand.AgeInYears;
            int minimumValidPeriod = thinningAges ? 1 : 0;  // can't thin at period 0 as no previous stand information is available for yield
            IList<int> periods = new List<int>(agesInYears.Count);

            for (int thinIndex = 0; thinIndex < agesInYears.Count; ++thinIndex)
            {
                int ageInYears = agesInYears[thinIndex];
                if (ageInYears == Constant.NoHarvestPeriod)
                {
                    periods.Add(Constant.NoHarvestPeriod);
                    continue;
                }

                int simulationPeriod = (int)MathF.Round((ageInYears - initialStandAge) / timestepInYears);
                if (simulationPeriod >= minimumValidPeriod)
                {
                    periods.Add(simulationPeriod);
                }
            }

            return periods;
        }

        protected abstract Heuristic<TParameters> CreateHeuristic(TParameters heuristicParameters, RunParameters runParameters);

        private RunParameters CreateRunParameters(SilviculturalSpace silviculturalSpace, SilviculturalCoordinate coordinate, OrganonConfiguration organonConfiguration)
        {
            RunParameters runParameters = new(silviculturalSpace.RotationLengths, organonConfiguration)
            {
                Financial = this.Financial,
                LogOnlyImprovingMoves = this.LogImprovingOnly,
                MaximizeForPlanningPeriod = this.HeuristicEvaluatesAcrossRotationsAndScenarios ? Constant.MaximizeForAllPlanningPeriods : silviculturalSpace.RotationLengths[coordinate.RotationIndex],
                MoveCapacity = this.MoveCapacity,
                TimberObjective = this.TimberObjective
            };

            int lastThinPeriod = Constant.NoHarvestPeriod;
            if (this.TryCreateThin(silviculturalSpace.FirstThinPeriods[coordinate.FirstThinPeriodIndex], out Harvest? firstThin))
            {
                lastThinPeriod = Math.Max(lastThinPeriod, silviculturalSpace.FirstThinPeriods[coordinate.FirstThinPeriodIndex]);
                runParameters.Treatments.Harvests.Add(firstThin);

                if (this.TryCreateThin(silviculturalSpace.SecondThinPeriods[coordinate.SecondThinPeriodIndex], out Harvest? secondThin))
                {
                    lastThinPeriod = Math.Max(lastThinPeriod, silviculturalSpace.SecondThinPeriods[coordinate.SecondThinPeriodIndex]);
                    runParameters.Treatments.Harvests.Add(secondThin);

                    if (this.TryCreateThin(silviculturalSpace.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex], out Harvest? thirdThin))
                    {
                        lastThinPeriod = Math.Max(lastThinPeriod, silviculturalSpace.SecondThinPeriods[coordinate.ThirdThinPeriodIndex]);
                        runParameters.Treatments.Harvests.Add(thirdThin);
                    }
                }
            }

            runParameters.LastThinPeriod = lastThinPeriod;
            return runParameters;
        }

        protected virtual Harvest CreateThin(int thinPeriodIndex)
        {
            return new ThinByIndividualTreeSelection(thinPeriodIndex);
        }

        protected IList<HeuristicParameters> GetDefaultParameterCombinations()
        {
            List<HeuristicParameters> parameterCombinations = new();
            foreach (float constructionGreediness in this.ConstructionGreediness)
            {
                if ((constructionGreediness < Constant.Grasp.FullyRandomConstructionForMaximization) || (constructionGreediness > Constant.Grasp.FullyGreedyConstructionForMaximization))
                {
                    throw new ParameterOutOfRangeException(nameof(this.ConstructionGreediness));
                }

                foreach (float thinningProbability in this.InitialThinningProbability)
                {
                    if ((thinningProbability < 0.0F) || (thinningProbability > 1.0F))
                    {
                        throw new ParameterOutOfRangeException(nameof(this.InitialThinningProbability));
                    }

                    parameterCombinations.Add(new HeuristicParameters()
                    {
                        MinimumConstructionGreediness = constructionGreediness,
                        InitialThinningProbability = thinningProbability
                    });
                }
            }
            return parameterCombinations;
        }

        // for now, assume runtime complexity is linear with the number of periods requiring thinning optimization
        // This is simplified, but it's more accurate than assuming all runs have equal cost regardless of the number of timesteps required and is
        // a reasonable approximation for hero, Monte Carlo heuristics, prescription enumeration, and genetic algorithms.
        protected static int EstimateRuntimeCost(SilviculturalCoordinate coordinate, HeuristicStandTrajectories<TParameters> results)
        {
            int periodWeight = 0; // no thins so only a single trajectory need be simulated; assume cost is negligible as no optimization occurs
            int firstThinPeriod = results.FirstThinPeriods[coordinate.FirstThinPeriodIndex];
            if (firstThinPeriod != Constant.NoHarvestPeriod)
            {
                int endOfRotationPeriod;
                if (coordinate.RotationIndex == Constant.AllRotationPosition)
                {
                    endOfRotationPeriod = results.RotationLengths.Max();
                }
                else
                {
                    endOfRotationPeriod = results.RotationLengths[coordinate.RotationIndex];
                }
                periodWeight = endOfRotationPeriod - firstThinPeriod;

                int secondThinPeriod = results.SecondThinPeriods[coordinate.SecondThinPeriodIndex];
                if (secondThinPeriod != Constant.NoHarvestPeriod)
                {
                    periodWeight += endOfRotationPeriod - secondThinPeriod;

                    int thirdThinPeriod = results.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex];
                    if (thirdThinPeriod == Constant.NoHarvestPeriod)
                    {
                        return endOfRotationPeriod - thirdThinPeriod;
                    }
                }
            }

            return periodWeight;
        }

        protected abstract string GetName();

        // ideally this would be virtual but, as of C# 9.0, the nature of C# generics requires GetDefaultParameterCombinations() to be separate
        protected abstract IList<TParameters> GetParameterCombinations();

        private string GetStatusDescription(SilviculturalSpace silviculturalSpace, SilviculturalCoordinate currentPosition, int workerThreadsUsed)
        {
            string mostRecentEvaluationDescription = String.Empty;
            if (silviculturalSpace.ParameterCombinations > 1)
            {
                mostRecentEvaluationDescription += ", parameters " + currentPosition.ParameterIndex + "/" + silviculturalSpace.ParameterCombinations;
            }
            if (silviculturalSpace.FirstThinPeriods.Count > 1)
            {
                mostRecentEvaluationDescription += ", thin 1 " + currentPosition.FirstThinPeriodIndex + "/" + silviculturalSpace.FirstThinPeriods.Count;
            }
            if (silviculturalSpace.SecondThinPeriods.Count > 1)
            {
                mostRecentEvaluationDescription += ", 2 " + currentPosition.SecondThinPeriodIndex + "/" + silviculturalSpace.SecondThinPeriods.Count;
            }
            if (silviculturalSpace.ThirdThinPeriods.Count > 1)
            {
                mostRecentEvaluationDescription += ", 3 " + currentPosition.ThirdThinPeriodIndex + "/" + silviculturalSpace.ThirdThinPeriods.Count;
            }
            if (this.HeuristicEvaluatesAcrossRotationsAndScenarios == false)
            {
                if (silviculturalSpace.RotationLengths.Count > 1)
                {
                    mostRecentEvaluationDescription += ", rotation " + currentPosition.RotationIndex + "/" + silviculturalSpace.RotationLengths.Count;
                }
                if (this.Financial.Count > 1)
                {
                    mostRecentEvaluationDescription += ", scenario " + currentPosition.FinancialIndex + "/" + this.Financial.Count;
                }
            }
            mostRecentEvaluationDescription += " (" + workerThreadsUsed + " threads)";
            return mostRecentEvaluationDescription;
        }

        protected override void ProcessRecord()
        {
            if (this.ConstructionGreediness.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.ConstructionGreediness));
            }
            if (this.Financial.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Financial));
            }
            if (this.FirstThinAge.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.FirstThinAge));
            }
            if (this.InitialThinningProbability.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.InitialThinningProbability));
            }
            if (this.MoveCapacity < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.MoveCapacity));
            }
            if (this.RotationAge.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.RotationAge));
            }
            if (this.SecondThinAge.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.SecondThinAge));
            }
            if (this.ThirdThinAge.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.ThirdThinAge));
            }
            if ((this.TimberObjective == TimberObjective.ScribnerVolume) && (this.Financial.Count > 1) && (this.HeuristicEvaluatesAcrossRotationsAndScenarios == false))
            {
                // low priority to improve this by checking for varying discount rates but at least warn about limited support
                this.WriteWarning("Timber optimization objective is " + this.TimberObjective + " but multiple discount rates may be specified in different financial scenarios. If so, optimization will be unnecessarily repeated for each discount rate.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();

            OrganonConfiguration organonConfiguration = new(OrganonVariant.Create(this.TreeModel));
            PrescriptionPerformanceCounters totalPerfCounters = new();
            int totalRuntimeCost = 0;

            float timestepInYears = (float)organonConfiguration.Variant.TimeStepInYears;
            IList<int> firstThinPeriods = this.ConvertAgesToSimulationPeriods(this.FirstThinAge, timestepInYears, thinningAges: true);
            IList<int> secondThinPeriods = this.ConvertAgesToSimulationPeriods(this.SecondThinAge, timestepInYears, thinningAges: true);
            IList<int> thirdThinPeriods = this.ConvertAgesToSimulationPeriods(this.ThirdThinAge, timestepInYears, thinningAges: true);
            IList<int> rotationLengthsInPeriods = this.ConvertAgesToSimulationPeriods(this.RotationAge, timestepInYears, thinningAges: false);

            int treeCount = this.Stand!.GetTreeRecordCount();
            List<SilviculturalCoordinate> combinationsToEvaluate = new();
            IList<TParameters> parameterCombinationsForHeuristic = this.GetParameterCombinations();
            HeuristicStandTrajectories<TParameters> results = new(parameterCombinationsForHeuristic, firstThinPeriods, secondThinPeriods, thirdThinPeriods, rotationLengthsInPeriods, this.Financial, this.SolutionPoolSize);
            
            for (int parameterIndex = 0; parameterIndex < parameterCombinationsForHeuristic.Count; ++parameterIndex)
            {
                for (int firstThinIndex = 0; firstThinIndex < firstThinPeriods.Count; ++firstThinIndex)
                {
                    int firstThinPeriod = firstThinPeriods[firstThinIndex];
                    Debug.Assert(firstThinPeriod != 0, "First thinning period cannot be zero.");

                    for (int secondThinIndex = 0; secondThinIndex < secondThinPeriods.Count; ++secondThinIndex)
                    {
                        int secondThinPeriod = secondThinPeriods[secondThinIndex];
                        Debug.Assert(secondThinPeriod != 0, "First thinning period cannot be zero.");

                        if (secondThinPeriod != Constant.NoHarvestPeriod)
                        {
                            if ((firstThinPeriod == Constant.NoHarvestPeriod) || (firstThinPeriod >= secondThinPeriod))
                            {
                                // can't perform a second thin if
                                // - there was no first thin
                                // - the second thin would occur before or in the same period as the first thin
                                // - the second thin would occur before or in the same period as final harvest
                                continue;
                            }
                        }

                        int lastOfFirstOrSecondThinPeriod = Math.Max(firstThinPeriod, secondThinPeriod);
                        for (int thirdThinIndex = 0; thirdThinIndex < thirdThinPeriods.Count; ++thirdThinIndex)
                        {
                            int thirdThinPeriod = thirdThinPeriods[thirdThinIndex];
                            Debug.Assert(thirdThinPeriod != 0, "First thinning period cannot be zero.");

                            if (thirdThinPeriod != Constant.NoHarvestPeriod)
                            {
                                if ((secondThinPeriod == Constant.NoHarvestPeriod) || (secondThinPeriod >= thirdThinPeriod))
                                {
                                    // can't perform a third thin if
                                    // - there was no second thin
                                    // - the third thin would occur before or in the same period as the second thin
                                    // - the third thin would occur before or in the same period as final harvest
                                    continue;
                                }
                            }

                            if (this.HeuristicEvaluatesAcrossRotationsAndScenarios)
                            {
                                SilviculturalCoordinate coordinate = new()
                                {
                                    FinancialIndex = Constant.AllFinancialScenariosPosition,
                                    FirstThinPeriodIndex = firstThinIndex,
                                    ParameterIndex = parameterIndex,
                                    RotationIndex = Constant.AllRotationPosition,
                                    SecondThinPeriodIndex = secondThinIndex,
                                    ThirdThinPeriodIndex = thirdThinIndex
                                };
                                combinationsToEvaluate.Add(coordinate);
                                totalRuntimeCost += OptimizeCmdlet<TParameters>.EstimateRuntimeCost(coordinate, results);
                            }
                            else
                            {
                                int lastOfFirstSecondOrThirdThinPeriod = Math.Max(lastOfFirstOrSecondThinPeriod, thirdThinPeriod);
                                for (int rotationIndex = 0; rotationIndex < rotationLengthsInPeriods.Count; ++rotationIndex)
                                {
                                    int planningPeriods = rotationLengthsInPeriods[rotationIndex];
                                    if (lastOfFirstSecondOrThirdThinPeriod >= planningPeriods)
                                    {
                                        // last thin would occur before or in the same period as the final harvest
                                        continue;
                                    }

                                    // heuristic optimizes for one financial scenario at a time
                                    for (int financialIndex = 0; financialIndex < this.Financial.Count; ++financialIndex)
                                    {
                                        // distributions are unique per combination of run tuple (discount rate, thins 1, 2, 3, and rotation)
                                        // and heuristic parameters but solution pools are shared across heuristic parameters
                                        SilviculturalCoordinate coordinate = new()
                                        {
                                            FinancialIndex = financialIndex,
                                            FirstThinPeriodIndex = firstThinIndex,
                                            ParameterIndex = parameterIndex,
                                            RotationIndex = rotationIndex,
                                            SecondThinPeriodIndex = secondThinIndex,
                                            ThirdThinPeriodIndex = thirdThinIndex
                                        };
                                        combinationsToEvaluate.Add(coordinate);
                                        totalRuntimeCost += OptimizeCmdlet<TParameters>.EstimateRuntimeCost(coordinate, results);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            int runsCompleted = 0;
            int runsStarted = -1; // step through combinationsToEvaluate sequentially
            int runtimeCostCompleted = 0;
            int totalRuns = this.BestOf * combinationsToEvaluate.Count;
            int usableWorkerThreads = Int32.Min(totalRuns, this.Threads);
            totalRuntimeCost *= this.BestOf;
            Task runs = Task.Run(() =>
            {
                Task[] workers = new Task[usableWorkerThreads];
                for (int workerThread = 0; workerThread < workers.Length; ++workerThread)
                {
                    workers[workerThread] = Task.Run(() =>
                    {
                        for (int runNumber = Interlocked.Increment(ref runsStarted); runNumber < totalRuns; runNumber = Interlocked.Increment(ref runsStarted))
                        {
                            if (this.Stopping)
                            {
                                return;
                            }

                            int combinationIndex = runNumber / this.BestOf;
                            SilviculturalCoordinate coordinate = combinationsToEvaluate[combinationIndex];
                            try
                            {
                                TParameters heuristicParameters = parameterCombinationsForHeuristic[coordinate.ParameterIndex];
                                RunParameters runParameters = this.CreateRunParameters(results, coordinate, organonConfiguration);
                                Heuristic<TParameters> currentHeuristic = this.CreateHeuristic(heuristicParameters, runParameters);
                                PrescriptionPerformanceCounters perfCounters = currentHeuristic.Run(coordinate, results);

                                // accumulate run into results and tracking counters
                                int estimatedRuntimeCost = OptimizeCmdlet<TParameters>.EstimateRuntimeCost(coordinate, results);
                                int runWithinCombination = runNumber - this.BestOf * combinationIndex;
                                lock (results)
                                {
                                    if (this.HeuristicEvaluatesAcrossRotationsAndScenarios)
                                    {
                                        // scatter heuristic's results to rotation lengths and discount rates
                                        PrescriptionPerformanceCounters perfCountersToAssmilate = perfCounters;
                                        bool perfCountersLogged = false;
                                        for (int rotationIndex = 0; rotationIndex < rotationLengthsInPeriods.Count; ++rotationIndex)
                                        {
                                            int endOfRotationPeriod = rotationLengthsInPeriods[rotationIndex];
                                            if (endOfRotationPeriod <= runParameters.LastThinPeriod)
                                            {
                                                continue; // not a valid position because end of rotation would occur before or in the same period as the last thin
                                            }

                                            for (int financialIndex = 0; financialIndex < this.Financial.Count; ++financialIndex)
                                            {
                                                // must be new each time to uniquely populate results.CombinationsEvaluated through AddEvaluatedPosition()
                                                SilviculturalCoordinate evaluatedCoordinate = new(coordinate)
                                                {
                                                    FinancialIndex = financialIndex,
                                                    RotationIndex = rotationIndex
                                                };

                                                results.AssimilateIntoCoordinate(currentHeuristic, evaluatedCoordinate, perfCountersToAssmilate);
                                                if (runWithinCombination == this.BestOf - 1)
                                                {
                                                    // last run in combination adds the evaluated positions to CombinationsEvaluated
                                                    // The evaluated position is added because it is specific to a particular rotation and
                                                    // financial scenario. In this case the heuristic position spans rotations and scenarios.
                                                    results.AddEvaluatedPosition(evaluatedCoordinate);
                                                }

                                                if (perfCountersLogged == false)
                                                {
                                                    perfCountersLogged = true;
                                                    perfCountersToAssmilate = PrescriptionPerformanceCounters.Zero;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // heuristic is specific to a single discount rate
                                        results.AssimilateIntoCoordinate(currentHeuristic, coordinate, perfCounters);
                                        if (runWithinCombination == this.BestOf - 1)
                                        {
                                            // last run in combination adds the position to CombinationsEvaluated
                                            results.AddEvaluatedPosition(coordinate);
                                        }
                                    }

                                    totalPerfCounters += perfCounters;
                                    ++runsCompleted;
                                    runtimeCostCompleted += estimatedRuntimeCost;
                                }
                            }
                            catch (Exception exception)
                            {
                                string rotationLength = "vectorized";
                                string financialScenario = "vectorized";
                                if (this.HeuristicEvaluatesAcrossRotationsAndScenarios == false)
                                {
                                    financialScenario = results.FinancialScenarios.DiscountRate[coordinate.FinancialIndex].ToString();
                                    rotationLength = results.RotationLengths[coordinate.RotationIndex].ToString();
                                }
                                throw new AggregateException("Exception encountered during optimization. Parameters " + coordinate.ParameterIndex +
                                                             ", first thin period " + results.FirstThinPeriods[coordinate.FirstThinPeriodIndex] +
                                                             ", second thin period " + results.SecondThinPeriods[coordinate.SecondThinPeriodIndex] +
                                                             ", third thin period " + results.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex] +
                                                             ", rotation length " + rotationLength +
                                                             ", financial scenario " + financialScenario +
                                                             ".",
                                                             exception);
                            }
                        }
                    });
                }

                Task.WaitAll(workers);
            });

            string cmdletName = this.GetName();
            bool progressWritten = false;
            int sleepsSinceLastStatusUpdate = 0;
            // check work status frequently at first in case tasks run very quickly
            // Quick checking makes single threaded PowerShell loops requesting many short optimizations (e.g. coarse thinning prescription enumerations)
            // run much more quickly.
            TimeSpan sleepTime = TimeSpan.FromSeconds(0.1);
            while (runs.IsCompleted == false)
            {
                Thread.Sleep(sleepTime);
                if (runs.IsFaulted)
                {
                    Debug.Assert(runs.Exception != null && runs.Exception.InnerException != null);
                    // per https://stackoverflow.com/questions/20170527/how-to-correctly-rethrow-an-exception-of-task-already-in-faulted-state
                    ExceptionDispatchInfo.Capture(runs.Exception.InnerException).Throw();
                }

                ++sleepsSinceLastStatusUpdate;
                if (sleepsSinceLastStatusUpdate > 30)
                {
                    int currentRun = Math.Min(runsCompleted + 1, totalRuns);
                    int currentPositionIndex = Math.Min(currentRun / this.BestOf, combinationsToEvaluate.Count - 1);
                    SilviculturalCoordinate mostRecentCombination = combinationsToEvaluate[currentPositionIndex];

                    string mostRecentEvaluationDescription = "run " + currentRun + "/" + totalRuns + this.GetStatusDescription(results, mostRecentCombination, usableWorkerThreads);
                    double fractionComplete = (double)runtimeCostCompleted / (double)totalRuntimeCost;
                    double secondsElapsed = stopwatch.Elapsed.TotalSeconds;
                    double secondsRemaining = secondsElapsed * (1.0 / fractionComplete - 1.0);
                    this.WriteProgress(new ProgressRecord(0, cmdletName, mostRecentEvaluationDescription.ToString())
                    {
                        PercentComplete = (int)(100.0 * fractionComplete),
                        SecondsRemaining = (int)Math.Round(secondsRemaining)
                    });

                    progressWritten = true;
                    sleepsSinceLastStatusUpdate = 0;
                    // assume longer running tasks and, after first status, move to less less frequent checking and updates
                    sleepTime = TimeSpan.FromSeconds(1.0);
                }
            }
            runs.GetAwaiter().GetResult(); // propagate any exceptions since last IsFaulted check
            stopwatch.Stop();
            this.WriteObject(results);
            
            if (progressWritten)
            {
                // write progress complete
                string mostRecentEvaluationDescription = totalRuns + " runs " + this.GetStatusDescription(results, combinationsToEvaluate[^1], usableWorkerThreads);
                this.WriteProgress(new ProgressRecord(0, cmdletName, mostRecentEvaluationDescription)
                {
                    PercentComplete = 100,
                    SecondsRemaining = 0
                });
            }

            if (results.CoordinatesEvaluated.Count == 1)
            {
                SilviculturalCoordinate firstPosition = results.CoordinatesEvaluated[0];
                this.WriteSingleDistributionSummary(results[firstPosition], totalPerfCounters, stopwatch.Elapsed);
            }
            else if (results.CoordinatesEvaluated.Count > 1)
            {
                this.WriteMultipleDistributionSummary(results, totalPerfCounters, stopwatch.Elapsed);
            }
            else
            {
                this.WriteWarning("No valid parameter combininations found. No runs performed.");
            }
        }

        private void WriteMultipleDistributionSummary(HeuristicStandTrajectories<TParameters> results, PrescriptionPerformanceCounters totalPerfCounters, TimeSpan elapsedTime)
        {
            results.GetPoolPerformanceCounters(out int solutionsCached, out int solutionsAccepted, out int solutionsRejected);

            this.WriteVerbose("{0}: {1} configurations with {2} runs in {3:0.00} minutes ({4:0.00}M timesteps in {5:0.00} core-minutes, {6:0.00}% move acceptance, {7} solutions pooled, {8:0.00}% pool acceptance).",
                              this.GetName(),
                              results.CoordinatesEvaluated.Count,
                              this.BestOf * results.CoordinatesEvaluated.Count,
                              elapsedTime.TotalMinutes,
                              1E-6F * totalPerfCounters.GrowthModelTimesteps,
                              totalPerfCounters.Duration.TotalMinutes,
                              100.0F * totalPerfCounters.MovesAccepted / (totalPerfCounters.MovesAccepted + totalPerfCounters.MovesRejected),
                              solutionsCached,
                              100.0F * solutionsAccepted / (solutionsAccepted + solutionsRejected));
        }

        private void WriteSingleDistributionSummary(SilviculturalCoordinateExploration element, PrescriptionPerformanceCounters totalPerfCounters, TimeSpan elapsedTime)
        {
            Heuristic? highHeuristic = element.Pool.High.Heuristic;
            if (highHeuristic == null)
            {
                throw new ArgumentOutOfRangeException(nameof(element), "Element's prescription pool does not have a high heuristic.");
            }

            base.WriteVerbose(String.Empty); // Visual Studio code workaround
            int totalMoves = totalPerfCounters.MovesAccepted + totalPerfCounters.MovesRejected;
            this.WriteVerbose("{0}: {1} moves, {2} changing ({3:0%}), {4} unchanging ({5:0%})", highHeuristic.GetName(), totalMoves, totalPerfCounters.MovesAccepted, (float)totalPerfCounters.MovesAccepted / (float)totalMoves, totalPerfCounters.MovesRejected, (float)totalPerfCounters.MovesRejected / (float)totalMoves);
            this.WriteVerbose("objective: best {0:0.00#}, mean {1:0.00#} ending {2:0.00#}.", highHeuristic.FinancialValue.GetHighestValue(), element.Distribution.HighestFinancialValueBySolution.Average(), highHeuristic.FinancialValue.GetAcceptedValuesWithDefaulting(Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex).Last());

            double totalSeconds = totalPerfCounters.Duration.TotalSeconds;
            double movesPerSecond = totalMoves / totalSeconds;
            double movesPerSecondMultiplier = movesPerSecond > 1E3 ? 1E-3 : 1.0;
            string movesPerSecondScale = movesPerSecond > 1E3 ? "k" : String.Empty;
            this.WriteVerbose("{0} moves in {1:0.000} core-s and {2:0.000}s clock time ({3:0.00} {4} moves/core-s).", totalMoves, totalSeconds, elapsedTime.TotalSeconds, movesPerSecondMultiplier * movesPerSecond, movesPerSecondScale);
        }

        private bool TryCreateThin(int thinPeriodIndex, [NotNullWhen(true)] out Harvest? thin)
        {
            if (thinPeriodIndex == Constant.NoHarvestPeriod)
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
