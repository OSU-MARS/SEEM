using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public class ThresholdAccepting : SingleTreeHeuristic
    {
        public List<int> IterationsPerThreshold { get; private init; }
        public List<float> Thresholds { get; private init; }

        public ThresholdAccepting(OrganonStand stand, OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
            : base(stand, organonConfiguration, objective, parameters)
        {
            int treeRecords = stand.GetTreeRecordCount();
            this.IterationsPerThreshold = new List<int>() { (int)(11.5F * treeRecords), 25, (int)(7.5F * treeRecords) };
            this.Thresholds = new List<float>() { 1.0F, 0.999F, 1.0F };
        }

        public override string GetName()
        {
            return "ThresholdAccepting";
        }

        // similar to SimulatedAnnealing.Run(), differences are in move acceptance
        public override TimeSpan Run()
        {
            if (this.IterationsPerThreshold.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.IterationsPerThreshold));
            }
            if (this.Thresholds.Count != this.IterationsPerThreshold.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Thresholds));
            }
            foreach (float threshold in this.Thresholds)
            {
                if ((threshold < 0.0F) || (threshold > 1.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Thresholds));
                }
            }

            IList<int> thinningPeriods = this.CurrentTrajectory.Configuration.Treatments.GetValidThinningPeriods();
            if ((thinningPeriods.Count < 2) || (thinningPeriods.Count > 3))
            {
                throw new NotSupportedException("Currently, only one or two thins are supported.");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            this.EvaluateInitialSelection(this.IterationsPerThreshold.Sum());

            float acceptedObjectiveFunction = this.BestObjectiveFunction;
            float treeIndexScalingFactor = (this.CurrentTrajectory.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            for (int thresholdIndex = 0; thresholdIndex < this.Thresholds.Count; ++thresholdIndex)
            {
                float iterations = this.IterationsPerThreshold[thresholdIndex];
                float threshold = this.Thresholds[thresholdIndex];
                for (int iterationInThreshold = 0; iterationInThreshold < iterations; ++iterationInThreshold)
                {
                    // if needed, support two opt moves
                    int treeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                    int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex);
                    int candidateHarvestPeriod = this.GetOneOptCandidateRandom(currentHarvestPeriod, thinningPeriods);
                    Debug.Assert(candidateHarvestPeriod >= 0);

                    candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                    candidateTrajectory.Simulate();

                    float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                    bool acceptMove = candidateObjectiveFunction > threshold * acceptedObjectiveFunction;
                    if (acceptMove)
                    {
                        acceptedObjectiveFunction = candidateObjectiveFunction; 
                        this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                        if (acceptedObjectiveFunction > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = acceptedObjectiveFunction;
                            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
                        }
                    }
                    else
                    {
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }

                    this.AcceptedObjectiveFunctionByMove.Add(acceptedObjectiveFunction);
                    this.CandidateObjectiveFunctionByMove.Add(candidateObjectiveFunction);
                    this.MoveLog.TreeIDByMove.Add(treeIndex);
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
