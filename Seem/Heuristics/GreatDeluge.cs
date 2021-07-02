using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GreatDeluge : SingleTreeHeuristic<HeuristicParameters>
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

        public GreatDeluge(OrganonStand stand, HeuristicParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters)
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

        public override HeuristicPerformanceCounters Run(HeuristicResultPosition position, HeuristicResults results)
        {
            if (this.ChangeToExchangeAfter < 0)
            {
                throw new InvalidOperationException(nameof(this.ChangeToExchangeAfter));
            }
            if (this.FinalMultiplier < this.IntitialMultiplier)
            {
                throw new InvalidOperationException(nameof(this.FinalMultiplier));
            }
            if (this.LowerWaterAfter < 0)
            {
                throw new InvalidOperationException(nameof(this.LowerWaterAfter));
            }
            if (this.StopAfter < 1)
            {
                throw new InvalidOperationException(nameof(this.StopAfter));
            }

            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            if (initialTreeRecordCount < 2)
            {
                throw new NotSupportedException();
            }
            IList<int> thinningPeriods = this.CurrentTrajectory.Treatments.GetHarvestPeriods();
            if ((thinningPeriods.Count < 2) || (thinningPeriods.Count > 3))
            {
                throw new NotSupportedException("Currently, only one or two thins are supported.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(position, results);
            float acceptedFinancialValue = this.EvaluateInitialSelection(this.Iterations, perfCounters);
            if (this.RainRate.HasValue == false)
            {
                this.RainRate = (this.FinalMultiplier - this.IntitialMultiplier) * acceptedFinancialValue / this.Iterations;
            }
            if ((this.RainRate.HasValue == false) || (this.RainRate.Value <= 0.0))
            {
                throw new InvalidOperationException(nameof(this.RainRate));
            }

            //float harvestPeriodScalingFactor = (this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / byte.MaxValue;
            float treeIndexScalingFactor = (initialTreeRecordCount - Constant.RoundTowardsZeroTolerance) / UInt16.MaxValue;

            // initial selection is considered iteration 0, so loop starts with iteration 1
            OrganonStandTrajectory candidateTrajectory = new(this.CurrentTrajectory);
            float highestFinancialValue = acceptedFinancialValue;
            float hillClimbingThreshold = acceptedFinancialValue;
            int iterationsSinceFinancialValueIncrease = 0;
            int iterationsSinceFinancialValueIncreaseOrMoveTypeChanged = 0;
            int iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;
            float waterLevel = this.IntitialMultiplier * acceptedFinancialValue;
            for (int iteration = 1; iteration < this.Iterations; ++iteration, waterLevel += this.RainRate.Value)
            {
                int firstTreeIndex = (int)(treeIndexScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
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
                        secondTreeIndex = (int)(treeIndexScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                        firstCandidateHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(secondTreeIndex);
                        while (firstCandidateHarvestPeriod == firstCurrentHarvestPeriod)
                        {
                            // retry until a modifying exchange is found
                            // This also excludes the case where a tree is exchanged with itself.
                            // BUGBUG: infinite loop if all trees have the same selection
                            secondTreeIndex = (int)(treeIndexScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
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
                perfCounters.GrowthModelTimesteps += candidateTrajectory.Simulate();
                ++iterationsSinceFinancialValueIncrease;
                ++iterationsSinceFinancialValueIncreaseOrMoveTypeChanged;
                ++iterationsSinceObjectiveImprovedOrWaterLevelLowered;

                float candidateFinancialValue = this.GetFinancialValue(candidateTrajectory, position.FinancialIndex);
                if ((candidateFinancialValue > waterLevel) || (candidateFinancialValue > hillClimbingThreshold))
                {
                    // accept move
                    acceptedFinancialValue = candidateFinancialValue;
                    this.CurrentTrajectory.CopyTreeGrowthFrom(candidateTrajectory);
                    hillClimbingThreshold = candidateFinancialValue;
                    iterationsSinceFinancialValueIncreaseOrMoveTypeChanged = 0;
                    iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;
                    ++perfCounters.MovesAccepted;

                    if (candidateFinancialValue > highestFinancialValue)
                    {
                        highestFinancialValue = candidateFinancialValue;
                        this.CopyTreeGrowthToBestTrajectory(this.CurrentTrajectory);

                        iterationsSinceFinancialValueIncrease = 0;
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
                    ++perfCounters.MovesRejected;
                }

                this.FinancialValue.AddMove(acceptedFinancialValue, candidateFinancialValue);
                this.MoveLog.TreeIDByMove.Add(firstTreeIndex);

                if (iterationsSinceFinancialValueIncrease > this.StopAfter)
                {
                    break;
                }
                else if (iterationsSinceFinancialValueIncreaseOrMoveTypeChanged > this.ChangeToExchangeAfter)
                {
                    // will fire repeatedly but no importa since this is idempotent
                    this.MoveType = MoveType.TwoOptExchange;
                    iterationsSinceFinancialValueIncreaseOrMoveTypeChanged = 0;
                }
                else if (iterationsSinceObjectiveImprovedOrWaterLevelLowered > this.LowerWaterAfter)
                {
                    // could also adjust rain rate but there but does not seem to be a clear need to do so
                    waterLevel = (1.0F - this.LowerWaterBy) * acceptedFinancialValue;
                    hillClimbingThreshold = waterLevel;
                    iterationsSinceObjectiveImprovedOrWaterLevelLowered = 0;
                }
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}
