using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class HeuristicDistribution : HeuristicSolutionPosition
    {
        public List<float> BestObjectiveFunctionBySolution { get; private init; }
        // TODO: remove since this is now available on HeuristicResultSet
        public HeuristicParameters? HeuristicParameters { get; private set; }

        public List<int> CountByMove { get; private init; }
        public List<float> TwoPointFivePercentileByMove { get; private init; }
        public List<float> FifthPercentileByMove { get; private init; }
        public List<float> LowerQuartileByMove { get; private init; }
        public List<float> MaximumObjectiveFunctionByMove { get; private init; }
        public List<float> MeanObjectiveFunctionByMove { get; private init; }
        public List<float> MedianObjectiveFunctionByMove { get; private init; }
        public List<float> MinimumObjectiveFunctionByMove { get; private init; }
        public List<float> NinetyFifthPercentileByMove { get; private init; }
        public List<float> NinetySevenPointFivePercentileByMove { get; private init; }
        public List<List<float>> ObjectiveFunctionValuesByMove { get; private init; }
        public List<float> UpperQuartileByMove { get; private init; }
        public List<float> VarianceByMove { get; private init; }

        public List<HeuristicPerformanceCounters> PerfCountersBySolution { get; private init; }

        public TimeSpan TotalCoreSeconds { get; private set; }
        public int TotalMoves { get; private set; }
        public int TotalRuns { get; private set; }

        public HeuristicDistribution(int treeCount)
        {
            int defaultMoveCapacity = treeCount;

            this.BestObjectiveFunctionBySolution = new(100);
            this.CountByMove = new List<int>(defaultMoveCapacity);
            this.FifthPercentileByMove = new(defaultMoveCapacity);
            this.HeuristicParameters = null;
            this.LowerQuartileByMove = new(defaultMoveCapacity);
            this.MaximumObjectiveFunctionByMove = new(defaultMoveCapacity);
            this.MeanObjectiveFunctionByMove = new(defaultMoveCapacity);
            this.MedianObjectiveFunctionByMove = new(defaultMoveCapacity);
            this.MinimumObjectiveFunctionByMove = new(defaultMoveCapacity);
            this.NinetyFifthPercentileByMove = new(defaultMoveCapacity);
            this.NinetySevenPointFivePercentileByMove = new(defaultMoveCapacity);
            this.ObjectiveFunctionValuesByMove = new(defaultMoveCapacity);
            this.PerfCountersBySolution = new(defaultMoveCapacity);
            this.TotalCoreSeconds = TimeSpan.Zero;
            this.TotalMoves = 0;
            this.TotalRuns = 0;
            this.TwoPointFivePercentileByMove = new(defaultMoveCapacity);
            this.UpperQuartileByMove = new(defaultMoveCapacity);
            this.VarianceByMove = new(defaultMoveCapacity);
        }

        public void AddRun(Heuristic heuristic, HeuristicPerformanceCounters perfCounters, HeuristicParameters runParameters)
        {
            this.BestObjectiveFunctionBySolution.Add(heuristic.BestObjectiveFunction);
            this.PerfCountersBySolution.Add(perfCounters);

            if (this.HeuristicParameters == null)
            {
                this.HeuristicParameters = heuristic.GetParameters(); // if heuristic reports parameters, prefer them
                if (this.HeuristicParameters == null)
                {
                    this.HeuristicParameters = runParameters; // otherwise, fall back to run parameters
                }
            }

            for (int moveIndex = 0; moveIndex < heuristic.AcceptedObjectiveFunctionByMove.Count; ++moveIndex)
            {
                float objectiveFunction = heuristic.AcceptedObjectiveFunctionByMove[moveIndex];
                if (moveIndex >= this.CountByMove.Count)
                {
                    // all quantiles are found in OnRunsComplete()
                    this.CountByMove.Add(1);
                    this.MaximumObjectiveFunctionByMove.Add(objectiveFunction);
                    this.MeanObjectiveFunctionByMove.Add(objectiveFunction);
                    this.MinimumObjectiveFunctionByMove.Add(objectiveFunction);
                    this.ObjectiveFunctionValuesByMove.Add(new List<float>() { objectiveFunction });
                    this.VarianceByMove.Add(objectiveFunction * objectiveFunction);
                }
                else
                {
                    ++this.CountByMove[moveIndex];

                    float maxObjectiveFunction = this.MaximumObjectiveFunctionByMove[moveIndex];
                    if (objectiveFunction > maxObjectiveFunction)
                    {
                        this.MaximumObjectiveFunctionByMove[moveIndex] = objectiveFunction;
                    }

                    // division and convergence to variance are done in OnRunsComplete()
                    this.MeanObjectiveFunctionByMove[moveIndex] += objectiveFunction;
                    this.VarianceByMove[moveIndex] += objectiveFunction * objectiveFunction;

                    float minObjectiveFunction = this.MinimumObjectiveFunctionByMove[moveIndex];
                    if (objectiveFunction < minObjectiveFunction)
                    {
                        this.MinimumObjectiveFunctionByMove[moveIndex] = objectiveFunction;
                    }

                    this.ObjectiveFunctionValuesByMove[moveIndex].Add(objectiveFunction);
                }
            }

            this.TotalCoreSeconds += perfCounters.Duration;
            this.TotalMoves += heuristic.AcceptedObjectiveFunctionByMove.Count;
            ++this.TotalRuns;
        }

        public void OnRunsComplete()
        {
            Debug.Assert(this.MeanObjectiveFunctionByMove.Count == this.VarianceByMove.Count);

            // find objective function statistics
            // Quantile calculations assume number of objective functions observed is constant or decreases monotonically with the number of moves the 
            // heuristic made.
            for (int moveIndex = 0; moveIndex < this.MeanObjectiveFunctionByMove.Count; ++moveIndex)
            {
                float runsAsFloat = this.CountByMove[moveIndex];
                this.MeanObjectiveFunctionByMove[moveIndex] /= runsAsFloat;
                this.VarianceByMove[moveIndex] = this.VarianceByMove[moveIndex] / runsAsFloat - this.MeanObjectiveFunctionByMove[moveIndex] * this.MeanObjectiveFunctionByMove[moveIndex];

                List<float> objectiveFunctions = this.ObjectiveFunctionValuesByMove[moveIndex];
                objectiveFunctions.Sort();

                float median;
                bool exactMedian = (objectiveFunctions.Count % 2) == 1;
                if (exactMedian)
                {
                    median = objectiveFunctions[objectiveFunctions.Count / 2]; // x.5 truncates to x, matching middle element due to zero based indexing
                }
                else
                {
                    int halfIndex = objectiveFunctions.Count / 2;
                    median = 0.5F * objectiveFunctions[halfIndex - 1] + 0.5F * objectiveFunctions[halfIndex];

                    Debug.Assert(median >= objectiveFunctions[0]);
                    Debug.Assert(median <= objectiveFunctions[^1]);
                }
                this.MedianObjectiveFunctionByMove.Add(median);

                if (objectiveFunctions.Count > 4)
                {
                    bool exactQuartiles = (objectiveFunctions.Count % 4) == 0;
                    if (exactQuartiles)
                    {
                        this.LowerQuartileByMove.Add(objectiveFunctions[objectiveFunctions.Count / 4]);
                        this.UpperQuartileByMove.Add(objectiveFunctions[3 * objectiveFunctions.Count / 4]);
                    }
                    else
                    {
                        float lowerQuartilePosition = 0.25F * objectiveFunctions.Count;
                        float ceilingIndex = MathF.Ceiling(lowerQuartilePosition);
                        float floorIndex = MathF.Floor(lowerQuartilePosition);
                        float ceilingWeight = 1.0F + lowerQuartilePosition - ceilingIndex;
                        float floorWeight = 1.0F - lowerQuartilePosition + floorIndex;
                        float lowerQuartile = floorWeight * objectiveFunctions[(int)floorIndex] + ceilingWeight * objectiveFunctions[(int)ceilingIndex];
                        this.LowerQuartileByMove.Add(lowerQuartile);

                        float upperQuartilePosition = 0.75F * objectiveFunctions.Count;
                        ceilingIndex = MathF.Ceiling(upperQuartilePosition);
                        floorIndex = MathF.Floor(upperQuartilePosition);
                        ceilingWeight = 1.0F + upperQuartilePosition - ceilingIndex;
                        floorWeight = 1.0F - upperQuartilePosition + floorIndex;
                        float upperQuartile = floorWeight * objectiveFunctions[(int)floorIndex] + ceilingWeight * objectiveFunctions[(int)ceilingIndex];
                        this.UpperQuartileByMove.Add(upperQuartile);

                        Debug.Assert(lowerQuartile >= objectiveFunctions[0]);
                        Debug.Assert(lowerQuartile <= median);
                        Debug.Assert(upperQuartile >= median);
                        Debug.Assert(upperQuartile <= objectiveFunctions[^1]);
                    }

                    if (objectiveFunctions.Count > 19)
                    {
                        bool exactPercentiles = (objectiveFunctions.Count % 20) == 0;
                        if (exactPercentiles)
                        {
                            this.FifthPercentileByMove.Add(objectiveFunctions[objectiveFunctions.Count / 20]);
                            this.NinetyFifthPercentileByMove.Add(objectiveFunctions[19 * objectiveFunctions.Count / 20]);
                        }
                        else
                        {
                            float fifthPercentilePosition = 0.05F * objectiveFunctions.Count;
                            float ceilingIndex = MathF.Ceiling(fifthPercentilePosition);
                            float floorIndex = MathF.Floor(fifthPercentilePosition);
                            float ceilingWeight = 1.0F + fifthPercentilePosition - ceilingIndex;
                            float floorWeight = 1.0F - fifthPercentilePosition + floorIndex;
                            float fifthPercentile = floorWeight * objectiveFunctions[(int)floorIndex] + ceilingWeight * objectiveFunctions[(int)ceilingIndex];
                            this.FifthPercentileByMove.Add(fifthPercentile);

                            float ninetyFifthPercentilePosition = 0.95F * objectiveFunctions.Count;
                            ceilingIndex = MathF.Ceiling(ninetyFifthPercentilePosition);
                            floorIndex = MathF.Floor(ninetyFifthPercentilePosition);
                            ceilingWeight = 1.0F + ninetyFifthPercentilePosition - ceilingIndex;
                            floorWeight = 1.0F - ninetyFifthPercentilePosition + floorIndex;
                            float ninetyFifthPercentile = floorWeight * objectiveFunctions[(int)floorIndex] + ceilingWeight * objectiveFunctions[(int)ceilingIndex];
                            this.NinetyFifthPercentileByMove.Add(ninetyFifthPercentile);

                            Debug.Assert(fifthPercentile >= objectiveFunctions[0]);
                            Debug.Assert(fifthPercentile <= median);
                            Debug.Assert(ninetyFifthPercentile >= median);
                            Debug.Assert(ninetyFifthPercentile <= objectiveFunctions[^1]);
                        }

                        if (objectiveFunctions.Count > 39)
                        {
                            exactPercentiles = (objectiveFunctions.Count % 40) == 0;
                            if (exactPercentiles)
                            {
                                this.TwoPointFivePercentileByMove.Add(objectiveFunctions[objectiveFunctions.Count / 40]);
                                this.NinetySevenPointFivePercentileByMove.Add(objectiveFunctions[39 * objectiveFunctions.Count / 40]);
                            }
                            else
                            {
                                float twoPointFivePercentilePosition = 0.025F * objectiveFunctions.Count;
                                float ceilingIndex = MathF.Ceiling(twoPointFivePercentilePosition);
                                float floorIndex = MathF.Floor(twoPointFivePercentilePosition);
                                float ceilingWeight = 1.0F + twoPointFivePercentilePosition - ceilingIndex;
                                float floorWeight = 1.0F - twoPointFivePercentilePosition + floorIndex;
                                float twoPointFivePercentile = floorWeight * objectiveFunctions[(int)floorIndex] + ceilingWeight * objectiveFunctions[(int)ceilingIndex];
                                this.TwoPointFivePercentileByMove.Add(twoPointFivePercentile);

                                float ninetySevenPointFivePercentilePosition = 0.975F * objectiveFunctions.Count;
                                ceilingIndex = MathF.Ceiling(ninetySevenPointFivePercentilePosition);
                                floorIndex = MathF.Floor(ninetySevenPointFivePercentilePosition);
                                ceilingWeight = 1.0F + ninetySevenPointFivePercentilePosition - ceilingIndex;
                                floorWeight = 1.0F - ninetySevenPointFivePercentilePosition + floorIndex;
                                float ninetySevenPointFivePercentile = floorWeight * objectiveFunctions[(int)floorIndex] + ceilingWeight * objectiveFunctions[(int)ceilingIndex];
                                this.NinetySevenPointFivePercentileByMove.Add(ninetySevenPointFivePercentile);

                                Debug.Assert(twoPointFivePercentile >= objectiveFunctions[0]);
                                Debug.Assert(twoPointFivePercentile <= median);
                                Debug.Assert(ninetySevenPointFivePercentile >= median);
                                Debug.Assert(ninetySevenPointFivePercentile <= objectiveFunctions[^1]);
                            }
                        }
                    }
                }
            }
        }
    }
}
