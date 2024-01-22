using Mars.Seem.Extensions;
using Mars.Seem.Optimization;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandTrajectory")]
    public class GetStandTrajectory : GetTrajectoryCmdlet
    {
        [Parameter]
        public SwitchParameter Benchmark { get; set; }
        [Parameter]
        public TimeSpan BenchmarkDuration { get; set; }
        [Parameter]
        public TimeSpan BenchmarkPolling { get; set; }
        [Parameter(HelpMessage = "List of thread counts to use when stepping benchmark load. Overrides -Threads.")]
        [ValidateNotNullOrEmpty]
        [ValidateRange(1, 128)]
        public List<int> BenchmarkThreads { get; set; }
        [Parameter]
        public TimeSpan BenchmarkWarmup { get; set; }

        [Parameter]
        [ValidateNotNull]
        public FinancialScenarios Financial { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FirstThinAbove { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FirstThinBelow { get; set; }
        [Parameter]
        [ValidateRange(1, 100)]
        public int FirstThinPeriod { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FirstThinProportional { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string? Name { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0, 100)]
        public List<int> RotationLengths { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand? Stand { get; set; }

        public GetStandTrajectory()
        {
            this.Benchmark = false;
            this.BenchmarkDuration = TimeSpan.FromMinutes(2.0); // typically long enough to average out tau if on an Intel processor
            this.BenchmarkPolling = TimeSpan.FromSeconds(1.0);
            this.BenchmarkThreads = [ 1, 2, 4, 6, 8, 12, 14, 16 ]; // simple default to 16 core CPU, could build dynamically to this.Threads
            this.BenchmarkWarmup = TimeSpan.FromMinutes(1.0); // roughly two core temperature time constants under an air cooler
            this.Financial = FinancialScenarios.Default;
            this.FirstThinAbove = 0.0F; // %
            this.FirstThinBelow = 0.0F; // %
            this.FirstThinProportional = 0.0F; // %
            this.FirstThinPeriod = Constant.NoHarvestPeriod; // no stand entry
            this.Name = null;
            this.RotationLengths = [ 15 ]; // 75 years of simulation with Organon's 5 year timestep
            this.TreeVolume = TreeScaling.Default;
        }

        [SupportedOSPlatform("windows")]
        private List<GrowthModelBenchmark> BenchmarkStandTimesteps(StandTrajectory unsimulatedTrajectory)
        {
            if ((this.BenchmarkDuration < TimeSpan.FromSeconds(1.0)) || (this.BenchmarkDuration > TimeSpan.FromMinutes(10.0)))
            {
                throw new ParameterOutOfRangeException(nameof(this.BenchmarkDuration)); // can't readily use TimeSpan with [ValidateRange] as it's a struct
            }
            if ((this.BenchmarkPolling < TimeSpan.FromSeconds(1.0)) || (this.BenchmarkPolling > TimeSpan.FromMinutes(1.0)))
            {
                throw new ParameterOutOfRangeException(nameof(this.BenchmarkPolling));
            }
            // this.BenchmarkThreads is [ValidateNotNullOrEmpty] and [ValidateRange]
            if ((this.BenchmarkWarmup < TimeSpan.Zero) || (this.BenchmarkWarmup > TimeSpan.FromMinutes(1.0)))
            {
                throw new ParameterOutOfRangeException(nameof(this.BenchmarkDuration));
            }
            if (SimdInstructionsExtensions.IsSupported(this.Simd) == false)
            {
                throw new ParameterOutOfRangeException(nameof(this.Simd), this.Simd + " instructions are not supported on this processor.");
            }

            int totalLoadStepDurationInSeconds = (int)((this.BenchmarkDuration + this.BenchmarkWarmup).TotalSeconds);
            ParallelOptions parallelOptions = new();
            ProgressRecord progressRecord = new(0)
            {
                Activity = "Get-StandTrajectory"
            };

            List<GrowthModelBenchmark> benchmarks = [];
            using LogicalProcessorFrequencies processorFrequencies = new();                
            for (int threadCountIndex = 1; threadCountIndex <= this.BenchmarkThreads.Count; ++threadCountIndex)
            {
                progressRecord.PercentComplete = (int)(100.0F * benchmarks.Count / this.BenchmarkThreads.Count);
                progressRecord.SecondsRemaining = totalLoadStepDurationInSeconds * (this.BenchmarkThreads.Count - benchmarks.Count);
                progressRecord.StatusDescription = "Starting benchmark with " + threadCountIndex + " thread" + (threadCountIndex > 1 ? "s" : String.Empty) + "...";
                this.WriteProgress(progressRecord);

                int threads = this.BenchmarkThreads[threadCountIndex];
                parallelOptions.MaxDegreeOfParallelism = threads;
                GrowthModelBenchmark benchmarkAtThreadCount = new(threads)
                {
                    PollingInterval = this.BenchmarkPolling,
                    Simd = this.Simd,
                    Start = DateTime.UtcNow
                };

                Task benchmarkRunAtThreadCount = Task.Run(() =>
                {
                    DateTime warmupEnd = benchmarkAtThreadCount.Start + this.BenchmarkWarmup;
                    DateTime benchmarkEnd = warmupEnd + this.BenchmarkDuration;

                    Parallel.For(0, parallelOptions.MaxDegreeOfParallelism, parallelOptions, (int workerThreadIndex) =>
                    {
                        while ((this.Stopping == false) && (DateTime.UtcNow < warmupEnd))
                        {
                            StandTrajectory trajectory = unsimulatedTrajectory.Clone();
                            benchmarkAtThreadCount.WarmupTimesteps[workerThreadIndex] += trajectory.Simulate();
                        }
                        benchmarkAtThreadCount.WarmupEnd[workerThreadIndex] = DateTime.UtcNow;

                        while ((this.Stopping == false) && (DateTime.UtcNow < benchmarkEnd))
                        {
                            StandTrajectory trajectory = unsimulatedTrajectory.Clone();
                            benchmarkAtThreadCount.Timesteps[workerThreadIndex] += trajectory.Simulate();
                        }
                        benchmarkAtThreadCount.End[workerThreadIndex] = DateTime.UtcNow;
                    });
                });

                while (benchmarkRunAtThreadCount.IsCompleted == false)
                {
                    Thread.Sleep(this.BenchmarkPolling);
                    benchmarkAtThreadCount.ProcessorFrequenciesInPercent.Add(processorFrequencies.NextValue());
                }
                benchmarkRunAtThreadCount.GetAwaiter().GetResult(); // propagate any exceptions since last IsFaulted check

                benchmarks.Add(benchmarkAtThreadCount);
            }

            progressRecord.PercentComplete = 100;
            progressRecord.SecondsRemaining = 0;
            progressRecord.StatusDescription = this.Simd + " benchmarking complete.";
            this.WriteProgress(progressRecord);
            return benchmarks;
        }

        [SupportedOSPlatform("windows")]
        protected override void ProcessRecord()
        {
            // argument checking and longest rotation length
            if (this.Stand == null)
            {
                throw new ParameterOutOfRangeException(nameof(this.Stand));
            }

            int lastPlanningPeriod = Int32.MinValue;
            for (int rotationIndex = 0; rotationIndex < this.RotationLengths.Count; ++rotationIndex)
            {
                int rotationLength = this.RotationLengths[rotationIndex];
                if (this.FirstThinPeriod >= rotationIndex)
                {
                    throw new ParameterOutOfRangeException(nameof(this.FirstThinPeriod));
                }

                if (rotationLength > lastPlanningPeriod)
                {
                    lastPlanningPeriod = rotationLength;
                }
            }

            if (this.FirstThinPeriod > Constant.NoHarvestPeriod)
            {
                float thinningIntensity = this.FirstThinAbove + this.FirstThinBelow + this.FirstThinProportional;
                if (thinningIntensity <= 0.0F)
                {
                    throw new ParameterOutOfRangeException(nameof(this.FirstThinPeriod), "-ThinPeriod is specified but -ThinAbove, -ThinBelow, and -ThinProportional add to zero so no thin will be performed.");
                }
            }

            // create stand trajectory
            OrganonConfiguration configuration = new(new OrganonVariantNwo()
            {
                Simd = this.Simd
            });
            OrganonStandTrajectory trajectory = new(this.Stand, configuration, this.TreeVolume, lastPlanningPeriod);
            if (this.Name != null)
            {
                trajectory.Name = this.Name;
            }
            if (this.FirstThinPeriod != Constant.NoHarvestPeriod)
            {
                trajectory.Treatments.Harvests.Add(new ThinByPrescription(this.FirstThinPeriod)
                {
                    FromAbovePercentage = this.FirstThinAbove,
                    ProportionalPercentage = this.FirstThinProportional,
                    FromBelowPercentage = this.FirstThinBelow
                });
            }

            if (this.Benchmark)
            {
                // benchmarking case: return results from 1, 2, 4, ..., nLogicalProcessors
                List<GrowthModelBenchmark> benchmarks = this.BenchmarkStandTimesteps(trajectory);
                this.WriteObject(benchmarks);
            }
            else
            {
                // mainline path: return simulated stand trajectory
                PrescriptionPerformanceCounters perfCounters = new();
                Stopwatch stopwatch = new();
                stopwatch.Start();
                perfCounters.GrowthModelTimesteps += trajectory.Simulate();
                stopwatch.Stop();
                perfCounters.Duration += stopwatch.Elapsed;

                // assign stand trajectory to coordinates
                SilviculturalSpace trajectories = new(new List<int>() { this.FirstThinPeriod }, this.RotationLengths, this.Financial);
                for (int rotationIndex = 0; rotationIndex < this.RotationLengths.Count; ++rotationIndex)
                {
                    int endOfRotationPeriod = this.RotationLengths[rotationIndex];

                    for (int financialIndex = 0; financialIndex < this.Financial.Count; ++financialIndex)
                    {
                        // must be new each time to uniquely populate results.CombinationsEvaluated through AddEvaluatedPosition()
                        SilviculturalCoordinate currentCoordinate = new()
                        {
                            FinancialIndex = financialIndex,
                            RotationIndex = rotationIndex
                        };

                        float landExpectationValue = this.Financial.GetLandExpectationValue(trajectory, financialIndex, endOfRotationPeriod);
                        trajectories.AssimilateIntoCoordinate(trajectory, landExpectationValue, currentCoordinate, perfCounters);
                        trajectories.AddEvaluatedPosition(currentCoordinate);
                    }
                }

                this.WriteObject(trajectories);
            }
        }
    }
}
