using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class RecordTravel : Heuristic
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

        public RecordTravel(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            : base(stand, organonConfiguration, planningPeriods, objective)
        {
            int treeRecordCount = stand.GetTreeRecordCount();
            this.Alpha = 0.75F;
            this.ChangeToExchangeAfter = Int32.MaxValue;
            this.FixedDeviation = 0.0F;
            this.FixedIncrease = 0.0F;
            this.IncreaseAfter = (int)(1.7F * treeRecordCount);
            this.Iterations = 10 * treeRecordCount;
            this.MoveType = MoveType.OneOpt;
            this.RelativeDeviation = 0.0F;
            this.RelativeIncrease = 0.01F;
            this.StopAfter = 1000;

            this.ObjectiveFunctionByMove = new List<float>(this.Iterations)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetName()
        {
            return "RecordTravel";
        }

        public override TimeSpan Run()
        {
            if ((this.Alpha < 0.0F) || (this.Alpha >  1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Alpha));
            }
            if (this.ChangeToExchangeAfter < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ChangeToExchangeAfter));
            }
            if (this.FixedDeviation < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FixedDeviation));
            }
            if (this.FixedIncrease < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FixedIncrease));
            }
            if (this.IncreaseAfter < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.IncreaseAfter));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }
            if ((this.RelativeDeviation < 0.0) || (this.RelativeDeviation > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.RelativeDeviation));
            }
            if ((this.RelativeIncrease < 0.0) || (this.RelativeIncrease > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.RelativeIncrease));
            }
            if (this.StopAfter < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.StopAfter));
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float currentObjectiveFunction = this.BestObjectiveFunction;
            //float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            int iterationsSinceBestObjectiveImproved = 0;
            int iterationsSinceObjectiveImprovedOrReheat = 0;
            float previousObjectiveFunction = Single.MinValue;
            float treeIndexScalingFactor = ((float)this.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / (float)UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            float deviation = this.RelativeDeviation * MathF.Abs(this.BestObjectiveFunction) + this.FixedDeviation;
            for (int iteration = 1; (iteration < this.Iterations) && (iterationsSinceBestObjectiveImproved < this.StopAfter); deviation *= this.Alpha, ++iteration)
            {
                int firstTreeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int firstCurrentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(firstTreeIndex);
                int firstCandidateHarvestPeriod;
                int secondTreeIndex = -1;
                switch (this.MoveType)
                {
                    case MoveType.OneOpt:
                        firstCandidateHarvestPeriod = firstCurrentHarvestPeriod == 0 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
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
                ++iterationsSinceObjectiveImprovedOrReheat;

                float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                float minimumAcceptableObjectiveFunction = this.BestObjectiveFunction - deviation;
                if ((candidateObjectiveFunction > minimumAcceptableObjectiveFunction) || (candidateObjectiveFunction > previousObjectiveFunction))
                {
                    // accept move
                    currentObjectiveFunction = candidateObjectiveFunction;
                    this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                    iterationsSinceObjectiveImprovedOrReheat = 0;

                    if (currentObjectiveFunction > this.BestObjectiveFunction)
                    {
                        this.BestObjectiveFunction = currentObjectiveFunction;
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

                this.ObjectiveFunctionByMove.Add(currentObjectiveFunction);
                if (iterationsSinceBestObjectiveImproved > this.ChangeToExchangeAfter)
                {
                    this.MoveType = MoveType.TwoOptExchange;
                }
                if (iterationsSinceObjectiveImprovedOrReheat == this.IncreaseAfter)
                {
                    deviation += this.RelativeIncrease * MathF.Abs(this.BestObjectiveFunction) + this.FixedIncrease;
                    iterationsSinceObjectiveImprovedOrReheat = 0;
                }

                if (this.ObjectiveFunctionByMove.Count == this.ChainFrom)
                {
                    this.BestTrajectoryByMove.Add(this.ChainFrom, new StandTrajectory(this.BestTrajectory));
                }
                previousObjectiveFunction = currentObjectiveFunction;
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
