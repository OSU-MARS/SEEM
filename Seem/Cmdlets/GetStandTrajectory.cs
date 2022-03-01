using Mars.Seem.Optimization;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandTrajectory")]
    public class GetStandTrajectory : Cmdlet
    {
        [Parameter]
        public SwitchParameter Benchmark { get; set; }
        [Parameter]
        public TimeSpan BenchmarkDuration { get; set; }
        [Parameter]
        [ValidateRange(1, 128)]
        public int BenchmarkMaxThreads { get; set; }
        [Parameter]
        public TimeSpan BenchmarkPolling { get; set; }
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

        [Parameter]
        public Simd Simd { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand? Stand { get; set; }

        [Parameter]
        [ValidateNotNull]
        public TreeVolume TreeVolume { get; set; }

        public GetStandTrajectory()
        {
            this.Benchmark = false;
            this.BenchmarkDuration = TimeSpan.FromMinutes(1.0); // long enough to average out processor turbo
            this.BenchmarkMaxThreads = Environment.ProcessorCount;
            this.BenchmarkPolling = TimeSpan.FromSeconds(1.0);
            this.BenchmarkWarmup = TimeSpan.FromSeconds(10.0);
            this.Financial = FinancialScenarios.Default;
            this.Name = null;
            this.RotationLengths = new() { 15 }; // 75 years of simulation with Organon's 5 year timestep
            this.FirstThinAbove = 0.0F; // %
            this.FirstThinBelow = 0.0F; // %
            this.FirstThinProportional = 0.0F; // %
            this.FirstThinPeriod = Constant.NoThinPeriod; // no stand entry
            this.TreeVolume = TreeVolume.Default;
        }

        private List<GrowthModelBenchmark> BenchmarkStandTimesteps(StandTrajectory unsimulatedTrajectory)
        {
            if ((this.BenchmarkDuration < TimeSpan.FromSeconds(1.0)) || (this.BenchmarkDuration > TimeSpan.FromMinutes(10.0)))
            {
                throw new ParameterOutOfRangeException(nameof(this.BenchmarkDuration));
            }
            if ((this.BenchmarkPolling < TimeSpan.FromSeconds(1.0)) || (this.BenchmarkPolling > TimeSpan.FromMinutes(1.0)))
            {
                throw new ParameterOutOfRangeException(nameof(this.BenchmarkPolling));
            }
            if ((this.BenchmarkWarmup < TimeSpan.Zero) || (this.BenchmarkWarmup > TimeSpan.FromMinutes(1.0)))
            {
                throw new ParameterOutOfRangeException(nameof(this.BenchmarkDuration));
            }
            if ((this.Simd == Simd.Width256) && (Avx2.IsSupported == false))
            {
                throw new ParameterOutOfRangeException(nameof(this.Simd), "256 bit wide SIMD is not supported on processors without AVX2 instructions.");
            }

            int benchmarkCount = (int)MathF.Log2(this.BenchmarkMaxThreads) + 1;
            int benchmarkDurationInSeconds = (int)(this.BenchmarkDuration + this.BenchmarkWarmup).TotalSeconds;
            ParallelOptions parallelOptions = new();

            List<GrowthModelBenchmark> benchmarks = new();
            using LogicalProcessorFrequencies processorFrequencies = new();                
            for (int threads = 1; threads <= this.BenchmarkMaxThreads; threads *= 2)
            {
                this.WriteProgress(new ProgressRecord(0, "Get-StandTrajectory", "Starting benchmark with " + threads + " thread" + (threads > 1 ? "s" : String.Empty) + "...")
                {
                    PercentComplete = (int)(100.0F * benchmarks.Count / benchmarkCount),
                    SecondsRemaining = benchmarkDurationInSeconds * (benchmarkCount - benchmarks.Count)
                });

                parallelOptions.MaxDegreeOfParallelism = threads;
                GrowthModelBenchmark benchmark = new(threads)
                {
                    PollingInterval = this.BenchmarkPolling,
                    Simd = this.Simd
                };
                benchmark.Start = DateTime.UtcNow;

                Task runs = Task.Run(() =>
                {
                    DateTime warmupEnd = benchmark.Start + this.BenchmarkWarmup;
                    DateTime benchmarkEnd = warmupEnd + this.BenchmarkDuration;

                    Parallel.For(0, threads, parallelOptions, (int threadNumber) =>
                    {
                        while ((this.Stopping == false) && (DateTime.UtcNow < warmupEnd))
                        {
                            StandTrajectory trajectory = unsimulatedTrajectory.Clone();
                            benchmark.WarmupTimesteps[threadNumber] += trajectory.Simulate();
                        }
                        benchmark.WarmupEnd[threadNumber] = DateTime.UtcNow;

                        while ((this.Stopping == false) && (DateTime.UtcNow < benchmarkEnd))
                        {
                            StandTrajectory trajectory = unsimulatedTrajectory.Clone();
                            benchmark.Timesteps[threadNumber] += trajectory.Simulate();
                        }
                        benchmark.End[threadNumber] = DateTime.UtcNow;
                    });
                });

                while (runs.IsCompleted == false)
                {
                    Thread.Sleep(this.BenchmarkPolling);
                    benchmark.ProcessorFrequenciesInPercent.Add(processorFrequencies.NextValue());
                }
                runs.GetAwaiter().GetResult(); // propagate any exceptions since last IsFaulted check

                benchmarks.Add(benchmark);
            }

            this.WriteProgress(new ProgressRecord(0, "Get-StandTrajectory", ((int)this.Simd).ToString() + " bit SIMD benchmarking complete.")
            {
                PercentComplete = 100,
                SecondsRemaining = 0
            });
            return benchmarks;
        }

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

            if (this.FirstThinPeriod > Constant.NoThinPeriod)
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
            if (this.FirstThinPeriod != Constant.NoThinPeriod)
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
                StandTrajectories trajectories = new(new List<int>() { this.FirstThinPeriod }, this.RotationLengths, this.Financial);
                for (int rotationIndex = 0; rotationIndex < this.RotationLengths.Count; ++rotationIndex)
                {
                    int endOfRotationPeriod = this.RotationLengths[rotationIndex];

                    for (int financialIndex = 0; financialIndex < this.Financial.Count; ++financialIndex)
                    {
                        // must be new each time to uniquely populate results.CombinationsEvaluated through AddEvaluatedPosition()
                        StandTrajectoryCoordinate currentCoordinate = new()
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
