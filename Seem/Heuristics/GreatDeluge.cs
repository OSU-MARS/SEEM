﻿using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GreatDeluge : SingleTreeHeuristic
    {
        public float ChangeToExchangeAfter { get; set; }
        public float FinalMultiplier { get; set; }
        public float IntitialMultiplier { get; set; }
        public int Iterations { get; set; }
        public int LowerWaterAfter { get; set; }
        public float LowerWaterBy { get; set; }
        public MoveType MoveType { get; set; }
        public float? RainRate { get; set; }
        public int StopAfter { get; set; }

        public GreatDeluge(OrganonStand stand, OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
            : base(stand, organonConfiguration, objective, parameters)
        {
            int treeRecords = stand.GetTreeRecordCount();
            this.ChangeToExchangeAfter = Int32.MaxValue;
            this.FinalMultiplier = Constant.MonteCarloDefault.DelugeFinalMultiplier;
            this.IntitialMultiplier = Constant.MonteCarloDefault.DelugeInitialMultiplier;
            this.Iterations = Constant.MonteCarloDefault.IterationMultiplier * treeRecords;
            this.LowerWaterAfter = (int)(Constant.MonteCarloDefault.ReheatAfter * treeRecords + 0.5F);
            this.LowerWaterBy = Constant.MonteCarloDefault.DelugeLowerWaterBy;
            this.MoveType = MoveType.OneOpt;
            this.RainRate = null;
            this.StopAfter = Constant.MonteCarloDefault.StopAfter * treeRecords;
        }

        public override string GetName()
        {
            return "Deluge";
        }

        public override void CopySelectionsFrom(StandTrajectory trajectory)
        {
            base.CopySelectionsFrom(trajectory);
            this.RainRate = (this.FinalMultiplier - this.IntitialMultiplier) * this.BestObjectiveFunction / this.Iterations;
        }

        public override TimeSpan Run()
        {
            if (this.ChangeToExchangeAfter < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ChangeToExchangeAfter));
            }
            if (this.FinalMultiplier < this.IntitialMultiplier)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FinalMultiplier));
            }
            if (this.LowerWaterAfter < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.LowerWaterAfter));
            }
            if (this.StopAfter < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.StopAfter));
            }

            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            if (initialTreeRecordCount < 2)
            {
                throw new NotSupportedException();
            }
            IList<int> thinningPeriods = this.CurrentTrajectory.Configuration.Treatments.GetValidThinningPeriods();
            if ((thinningPeriods.Count < 2) || (thinningPeriods.Count > 3))
            {
                throw new NotSupportedException("Currently, only one or two thins are supported.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();

            this.EvaluateInitialSelection(this.Iterations);
            if (this.RainRate.HasValue == false)
            {
                this.RainRate = (this.FinalMultiplier - this.IntitialMultiplier) * this.BestObjectiveFunction / this.Iterations;
            }
            if ((this.RainRate.HasValue == false) || (this.RainRate.Value <= 0.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.RainRate));
            }

            float acceptedObjectiveFunction = this.BestObjectiveFunction;
            //float harvestPeriodScalingFactor = (this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / byte.MaxValue;
            float treeIndexScalingFactor = (initialTreeRecordCount - Constant.RoundTowardsZeroTolerance) / UInt16.MaxValue;

            // initial selection is considered iteration 0, so loop starts with iteration 1
            OrganonStandTrajectory candidateTrajectory = new(this.CurrentTrajectory);
            float hillClimbingThreshold = this.BestObjectiveFunction;
            int iterationsSinceBestObjectiveImproved = 0;
            int iterationsSinceObjectiveImprovedOrMoveTypeChanged = 0;
            int iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;
            float waterLevel = this.IntitialMultiplier * this.BestObjectiveFunction;
            for (int iteration = 1; iteration < this.Iterations; ++iteration, waterLevel += this.RainRate.Value)
            {
                int firstTreeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int firstCurrentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(firstTreeIndex);
                int firstCandidateHarvestPeriod;
                int secondTreeIndex = -1;
                switch (this.MoveType)
                {
                    case MoveType.OneOpt:
                        firstCandidateHarvestPeriod = this.GetOneOptCandidateRandom(firstCurrentHarvestPeriod, thinningPeriods);
                        candidateTrajectory.SetTreeSelection(firstTreeIndex, firstCandidateHarvestPeriod);
                        break;
                    case MoveType.TwoOptExchange:
                        secondTreeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                        firstCandidateHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(secondTreeIndex);
                        while (firstCandidateHarvestPeriod == firstCurrentHarvestPeriod)
                        {
                            // retry until a modifying exchange is found
                            // This also excludes the case where a tree is exchanged with itself.
                            // BUGBUG: infinite loop if all trees have the same selection
                            secondTreeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                            firstCandidateHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(secondTreeIndex);
                        }
                        candidateTrajectory.SetTreeSelection(firstTreeIndex, firstCandidateHarvestPeriod);
                        candidateTrajectory.SetTreeSelection(secondTreeIndex, firstCurrentHarvestPeriod);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                //int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                //while (candidateHarvestPeriod == currentHarvestPeriod)
                //{
                //    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                //}
                Debug.Assert(firstCandidateHarvestPeriod >= 0);

                candidateTrajectory.SetTreeSelection(firstTreeIndex, firstCandidateHarvestPeriod);
                candidateTrajectory.Simulate();
                ++iterationsSinceBestObjectiveImproved;
                ++iterationsSinceObjectiveImprovedOrMoveTypeChanged;
                ++iterationsSinceObjectiveImprovedOrWaterLevelLowered;

                float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                if ((candidateObjectiveFunction > waterLevel) || (candidateObjectiveFunction > hillClimbingThreshold))
                {
                    // accept move
                    acceptedObjectiveFunction = candidateObjectiveFunction;
                    this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                    hillClimbingThreshold = candidateObjectiveFunction;
                    iterationsSinceObjectiveImprovedOrMoveTypeChanged = 0;
                    iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;

                    if (candidateObjectiveFunction > this.BestObjectiveFunction)
                    {
                        this.BestObjectiveFunction = candidateObjectiveFunction;
                        this.BestTrajectory.CopyFrom(this.CurrentTrajectory);

                        iterationsSinceBestObjectiveImproved = 0;
                    }
                }
                else
                {
                    // undo move
                    switch (this.MoveType)
                    {
                        case MoveType.OneOpt:
                            candidateTrajectory.SetTreeSelection(firstTreeIndex, firstCurrentHarvestPeriod);
                            break;
                        case MoveType.TwoOptExchange:
                            candidateTrajectory.SetTreeSelection(firstTreeIndex, firstCurrentHarvestPeriod);
                            candidateTrajectory.SetTreeSelection(secondTreeIndex, firstCandidateHarvestPeriod);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }

                this.AcceptedObjectiveFunctionByMove.Add(acceptedObjectiveFunction);
                this.CandidateObjectiveFunctionByMove.Add(candidateObjectiveFunction);
                this.MoveLog.TreeIDByMove.Add(firstTreeIndex);

                if (iterationsSinceBestObjectiveImproved > this.StopAfter)
                {
                    break;
                }
                else if (iterationsSinceObjectiveImprovedOrMoveTypeChanged > this.ChangeToExchangeAfter)
                {
                    // will fire repeatedly but no importa since this is idempotent
                    this.MoveType = MoveType.TwoOptExchange;
                    iterationsSinceObjectiveImprovedOrMoveTypeChanged = 0;
                }
                else if (iterationsSinceObjectiveImprovedOrWaterLevelLowered > this.LowerWaterAfter)
                {
                    // could also adjust rain rate but there but does not seem to be a clear need to do so
                    waterLevel = (1.0F - this.LowerWaterBy) * this.BestObjectiveFunction;
                    hillClimbingThreshold = waterLevel;
                    iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
