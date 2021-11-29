using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Heuristics
{
    public class RecordTravel : SingleTreeHeuristic<HeuristicParameters>
    {
        public float Alpha { get; set; }
        public float ChangeToExchangeAfter { get; set; }
        public float FixedDeviation { get; set; }
        public float FixedIncrease { get; set; }
        public int IncreaseAfter { get; set; }
        public int Iterations { get; set; }
        public MoveType MoveType { get; set; }
        public float RelativeDeviation { get; set; }
        public float RelativeIncrease { get; set; }
        public int StopAfter { get; set; }

        public RecordTravel(OrganonStand stand, HeuristicParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters)
        {
            int treeRecordCount = stand.GetTreeRecordCount();
            this.Alpha = Constant.MonteCarloDefault.RecordTravelAlpha;
            this.ChangeToExchangeAfter = Int32.MaxValue;
            this.FixedDeviation = 0.0F;
            this.FixedIncrease = 0.0F;
            this.IncreaseAfter = (int)(Constant.MonteCarloDefault.ReheatAfter * treeRecordCount + 0.5F);
            this.Iterations = Constant.MonteCarloDefault.IterationMultiplier * treeRecordCount;
            this.MoveType = MoveType.OneOpt;
            this.RelativeDeviation = 0.0F;
            this.RelativeIncrease = Constant.MonteCarloDefault.RecordTravelRelativeIncrease;
            this.StopAfter = Constant.MonteCarloDefault.StopAfter * treeRecordCount;
        }

        public override string GetName()
        {
            return "RecordTravel";
        }

        public override PrescriptionPerformanceCounters Run(StandTrajectoryCoordinate coordinate, HeuristicStandTrajectories trajectories)
        {
            if ((this.Alpha < 0.0F) || (this.Alpha >  1.0F))
            {
                throw new InvalidOperationException(nameof(this.Alpha));
            }
            if (this.ChangeToExchangeAfter < 0)
            {
                throw new InvalidOperationException(nameof(this.ChangeToExchangeAfter));
            }
            if (this.FixedDeviation < 0.0F)
            {
                throw new InvalidOperationException(nameof(this.FixedDeviation));
            }
            if (this.FixedIncrease < 0.0F)
            {
                throw new InvalidOperationException(nameof(this.FixedIncrease));
            }
            if (this.IncreaseAfter < 0)
            {
                throw new InvalidOperationException(nameof(this.IncreaseAfter));
            }
            if ((this.RelativeDeviation < 0.0) || (this.RelativeDeviation > 1.0))
            {
                throw new InvalidOperationException(nameof(this.RelativeDeviation));
            }
            if ((this.RelativeIncrease < 0.0) || (this.RelativeIncrease > 1.0))
            {
                throw new InvalidOperationException(nameof(this.RelativeIncrease));
            }
            if (this.RunParameters.LogOnlyImprovingMoves)
            {
                throw new NotSupportedException("Logging of only improving moves isn't currently supported.");
            }
            if (this.StopAfter < 1)
            {
                throw new InvalidOperationException(nameof(this.StopAfter));
            }

            IList<int> harvestPeriods = this.CurrentTrajectory.Treatments.GetHarvestPeriods();
            if ((harvestPeriods.Count < 2) || (harvestPeriods.Count > 3))
            {
                throw new NotSupportedException("Currently, only one or two thins are supported.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            PrescriptionPerformanceCounters perfCounters = new();

            perfCounters.TreesRandomizedInConstruction += this.ConstructTreeSelection(coordinate, trajectories);
            float acceptedFinancialValue = this.EvaluateInitialSelection(coordinate, this.Iterations, perfCounters);

            //float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            int iterationsSinceBestObjectiveImproved = 0;
            int iterationsSinceFinancialValueIncreaseOrReheat = 0;
            float previousObjectiveFunction = Single.MinValue;
            float treeIndexScalingFactor = (this.CurrentTrajectory.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new(this.CurrentTrajectory);
            float deviation = this.RelativeDeviation * MathF.Abs(acceptedFinancialValue) + this.FixedDeviation;
            for (int iteration = 1; (iteration < this.Iterations) && (iterationsSinceBestObjectiveImproved < this.StopAfter); deviation *= this.Alpha, ++iteration)
            {
                int firstTreeIndex = (int)(treeIndexScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                int firstCurrentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(firstTreeIndex);
                int firstCandidateHarvestPeriod;
                int secondTreeIndex = -1;
                switch (this.MoveType)
                {
                    case MoveType.OneOpt:
                        firstCandidateHarvestPeriod = this.GetOneOptCandidateRandom(firstCurrentHarvestPeriod, harvestPeriods);
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
                ++iterationsSinceBestObjectiveImproved;
                ++iterationsSinceFinancialValueIncreaseOrReheat;

                float candidateFinancialValue = this.GetFinancialValue(candidateTrajectory, coordinate.FinancialIndex);
                float highestFinancialValue = this.FinancialValue.GetHighestValue();
                float minimumAcceptableFinancialValue = highestFinancialValue - deviation;
                if ((candidateFinancialValue > minimumAcceptableFinancialValue) || (candidateFinancialValue > previousObjectiveFunction))
                {
                    // accept move
                    acceptedFinancialValue = candidateFinancialValue;
                    this.CurrentTrajectory.CopyTreeGrowthFrom(candidateTrajectory);
                    iterationsSinceFinancialValueIncreaseOrReheat = 0;
                    ++perfCounters.MovesAccepted;

                    if (acceptedFinancialValue > highestFinancialValue)
                    {
                        this.CopyTreeGrowthToBestTrajectory(coordinate, this.CurrentTrajectory);
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
                    ++perfCounters.MovesRejected;
                }

                this.FinancialValue.TryAddMove(coordinate, acceptedFinancialValue, candidateFinancialValue);
                this.MoveLog.TryAddMove(firstTreeIndex);

                if (iterationsSinceBestObjectiveImproved > this.ChangeToExchangeAfter)
                {
                    this.MoveType = MoveType.TwoOptExchange;
                }
                if (iterationsSinceFinancialValueIncreaseOrReheat == this.IncreaseAfter)
                {
                    deviation += this.RelativeIncrease * MathF.Abs(highestFinancialValue) + this.FixedIncrease;
                    iterationsSinceFinancialValueIncreaseOrReheat = 0;
                }
                previousObjectiveFunction = acceptedFinancialValue;
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}
