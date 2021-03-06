﻿using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class Hero : SingleTreeHeuristic
    {
        public bool IsStochastic { get; set; }
        public int MaximumIterations { get; set; }

        public Hero(OrganonStand stand, OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
            : base(stand, organonConfiguration, objective, parameters)
        {
            this.IsStochastic = false;
            this.MaximumIterations = Constant.HeuristicDefault.HeroMaximumIterations;
        }

        public override string GetName()
        {
            if (this.IsStochastic)
            {
                return "HeroStochastic";
            }
            return "Hero";
        }

        private int[] GetPeriodIndices(int allSpeciesTreeCount, IList<int> thinningPeriods)
        {
            // this function is inefficient as it reverses RandomizeTreeSelection()'s internal logic
            // Not currently enough of a performance advantage for refactoring to be worthwhile.
            int[] periodIndices = new int[allSpeciesTreeCount];
            for (int allSpeciesUncompactedTreeIndex = 0; allSpeciesUncompactedTreeIndex < periodIndices.Length; ++allSpeciesUncompactedTreeIndex)
            {
                int currentTreeHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(allSpeciesUncompactedTreeIndex);
                for (int periodIndex = 0; periodIndex < thinningPeriods.Count; ++periodIndex)
                {
                    if (thinningPeriods[periodIndex] == currentTreeHarvestPeriod)
                    {
                        periodIndices[allSpeciesUncompactedTreeIndex] = periodIndex;
                        break;
                    }
                }
            }
            return periodIndices;
        }

        public override TimeSpan Run()
        {
            if (this.MaximumIterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaximumIterations));
            }

            IList<int> thinningPeriods = this.CurrentTrajectory.Configuration.Treatments.GetValidThinningPeriods();

            Stopwatch stopwatch = new();
            stopwatch.Start();

            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            this.EvaluateInitialSelection(this.MaximumIterations * initialTreeRecordCount);

            float acceptedObjectiveFunction = this.BestObjectiveFunction;
            float previousBestObjectiveFunction = this.BestObjectiveFunction;
            OrganonStandTrajectory candidateTrajectory = new(this.CurrentTrajectory);
            bool decrementPeriodIndex = false;
            int[] uncompactedPeriodIndices = this.GetPeriodIndices(initialTreeRecordCount, thinningPeriods);
            int[] uncompactedTreeIndices = Heuristic.CreateSequentialArray(initialTreeRecordCount);
            for (int iteration = 0; iteration < this.MaximumIterations; ++iteration)
            {
                // randomize on every iteration since a single randomization against the order of the data has little effect
                if (this.IsStochastic)
                {
                    this.Pseudorandom.Shuffle(uncompactedTreeIndices);
                }

                for (int sourceTreeIndex = 0; sourceTreeIndex < initialTreeRecordCount; ++sourceTreeIndex)
                {
                    // evaluate other cut option
                    int treeIndex = uncompactedTreeIndices[sourceTreeIndex];
                    int currentPeriodIndex = uncompactedPeriodIndices[treeIndex];
                    int candidatePeriodIndex = decrementPeriodIndex ? currentPeriodIndex - 1 : currentPeriodIndex + 1;
                    if (this.IsStochastic)
                    {
                        decrementPeriodIndex = this.GetPseudorandomByteAsProbability() < 0.5F;
                    }

                    for (int periodEvaluationForTree = 0; periodEvaluationForTree < thinningPeriods.Count - 1; ++periodEvaluationForTree)
                    {
                        if (candidatePeriodIndex >= thinningPeriods.Count)
                        {
                            candidatePeriodIndex = 0;
                        }
                        else if (candidatePeriodIndex < 0)
                        {
                            candidatePeriodIndex = thinningPeriods.Count - 1;
                        }
                        int candidateHarvestPeriod = thinningPeriods[candidatePeriodIndex];
                        int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex); // capture for revert
                        Debug.Assert(currentHarvestPeriod != candidateHarvestPeriod);
                        candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                        candidateTrajectory.Simulate();

                        float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                        if (candidateObjectiveFunction > acceptedObjectiveFunction)
                        {
                            // accept change of no cut-cut decision if it improves upon the best solution
                            acceptedObjectiveFunction = candidateObjectiveFunction;
                            uncompactedPeriodIndices[treeIndex] = candidatePeriodIndex;
                            this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                        }
                        else
                        {
                            // otherwise, revert changes candidate trajectory for considering next tree's move
                            candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                        }

                        this.AcceptedObjectiveFunctionByMove.Add(acceptedObjectiveFunction);
                        this.CandidateObjectiveFunctionByMove.Add(candidateObjectiveFunction);
                        this.MoveLog.TreeIDByMove.Add(treeIndex);

                        if (decrementPeriodIndex)
                        {
                            --candidatePeriodIndex;
                        }
                        else
                        {
                            ++candidatePeriodIndex;
                        }
                    }
                }

                if (acceptedObjectiveFunction <= previousBestObjectiveFunction)
                {
                    // convergence: stop if no improvement
                    break;
                }
                previousBestObjectiveFunction = acceptedObjectiveFunction;
            }

            this.BestObjectiveFunction = acceptedObjectiveFunction;
            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
