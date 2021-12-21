using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Heuristics
{
    public class SimulatedAnnealing : SingleTreeHeuristic<HeuristicParameters>
    {
        public float Alpha { get; set; }
        public float ChangeToExchangeAfter { get; set; }
        public float FinalProbability { get; set; }
        public float InitialProbability { get; set; }
        public int Iterations { get; set; }
        public int IterationsPerTemperature { get; set; }
        public MoveType MoveType { get; set; }
        public int ProbabilityWindowLength { get; set; }
        public int ReheatAfter { get; set; }
        public float ReheatBy { get; set; }

        public SimulatedAnnealing(OrganonStand stand, HeuristicParameters heuristicParameters, RunParameters runParameters)
            :  base(stand, heuristicParameters, runParameters)
        {
            int treeRecords = stand.GetTreeRecordCount();
            this.Alpha = Constant.MonteCarloDefault.AnnealingAlpha;
            this.ChangeToExchangeAfter = Int32.MaxValue;
            this.FinalProbability = 0.0F;
            this.InitialProbability = 0.0F;
            this.Iterations = Constant.MonteCarloDefault.IterationMultiplier * treeRecords;
            this.IterationsPerTemperature = Constant.MonteCarloDefault.AnnealingIterationsPerTemperature;
            this.MoveType = MoveType.OneOpt;
            this.ProbabilityWindowLength = Constant.MonteCarloDefault.AnnealingAveragingWindowLength;
            this.ReheatAfter = (int)(Constant.MonteCarloDefault.ReheatAfter * treeRecords + 0.5F);
            this.ReheatBy = Constant.MonteCarloDefault.AnnealingReheadBy;

            // float temperatureSteps = (float)(defaultIterations / this.IterationsPerTemperature);
            // this.Alpha = 1.0F / MathF.Pow(this.InitialAcceptProbability / this.FinalAcceptProbability, 1.0F / temperatureSteps);
        }

        public override string GetName()
        {
            return "SimulatedAnnealing";
        }

        public override PrescriptionPerformanceCounters Run(StandTrajectoryCoordinate coordinate, HeuristicStandTrajectories trajectories)
        {
            if ((this.Alpha <= 0.0) || (this.Alpha >= 1.0))
            {
                throw new InvalidOperationException(nameof(this.Alpha));
            }
            if (this.ChangeToExchangeAfter < 0)
            {
                throw new InvalidOperationException(nameof(this.ChangeToExchangeAfter));
            }
            if (this.FinalProbability < 0.0)
            {
                throw new InvalidOperationException(nameof(this.FinalProbability));
            }
            if (this.InitialProbability < this.FinalProbability)
            {
                throw new InvalidOperationException(nameof(this.InitialProbability));
            }
            if (this.Iterations < 1)
            {
                throw new InvalidOperationException(nameof(this.Iterations));
            }
            if (this.IterationsPerTemperature < 1)
            {
                throw new InvalidOperationException(nameof(this.IterationsPerTemperature));
            }
            if (this.ProbabilityWindowLength < 1)
            {
                throw new InvalidOperationException(nameof(this.ProbabilityWindowLength));
            }
            if (this.ReheatAfter < 0)
            {
                throw new InvalidOperationException(nameof(this.ReheatAfter));
            }
            if (this.ReheatBy < 0.0F)
            {
                throw new InvalidOperationException(nameof(this.ReheatBy));
            }
            if (this.RunParameters.LogOnlyImprovingMoves)
            {
                throw new NotSupportedException("Logging of only improving moves isn't currently supported.");
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

            int iterationsSinceMoveTypeOrObjectiveChange = 0;
            int iterationsSinceReheatOrFinancialValueIncreased = 0;
            float meanAcceptanceProbability = this.InitialProbability;
            float movingAverageOfObjectiveChange = -1.0F;
            float movingAverageMemory = 1.0F - 1.0F / this.ProbabilityWindowLength;
            float treeIndexScalingFactor = (this.CurrentTrajectory.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new(this.CurrentTrajectory);
            for (int iteration = 1; (iteration < this.Iterations) && (meanAcceptanceProbability >= this.FinalProbability); meanAcceptanceProbability *= this.Alpha)
            {
                float logMeanAcceptanceProbability = Single.NegativeInfinity;
                if (meanAcceptanceProbability > 0.0F)
                {
                    logMeanAcceptanceProbability = MathV.Ln(meanAcceptanceProbability);
                }

                for (int iterationAtTemperature = 0; iterationAtTemperature < this.IterationsPerTemperature; ++iteration, ++iterationAtTemperature)
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

                    perfCounters.GrowthModelTimesteps += candidateTrajectory.Simulate();
                    ++iterationsSinceMoveTypeOrObjectiveChange;
                    ++iterationsSinceReheatOrFinancialValueIncreased;

                    float candidateFinancialValue = this.GetFinancialValue(candidateTrajectory, coordinate.FinancialIndex);

                    bool acceptMove = candidateFinancialValue > acceptedFinancialValue;
                    // require at least one improving move be accepted to set moving average before accepting disimproving moves
                    if ((acceptMove == false) && (logMeanAcceptanceProbability > Single.NegativeInfinity) && (movingAverageOfObjectiveChange > 0.0F))
                    {
                        // objective function increase is negative and log of acceptance probability is negative or zero, so exponent is positive or zero
                        float objectiveFunctionIncrease = candidateFinancialValue - acceptedFinancialValue;
                        float exponent = logMeanAcceptanceProbability * objectiveFunctionIncrease / movingAverageOfObjectiveChange;
                        Debug.Assert(exponent >= 0.0F);
                        if (exponent < 10.0F)
                        {
                            // exponent is small enough not to round acceptance probabilities down to zero
                            // 1/e^10 accepts 1 in 22,026 moves.
                            float acceptanceProbability = 1.0F / MathV.Exp(exponent);
                            float moveProbability = this.Pseudorandom.GetPseudorandomByteAsProbability();
                            if (moveProbability < acceptanceProbability)
                            {
                                acceptMove = true;
                            }
                        }
                    }

                    if (acceptMove)
                    {
                        float objectiveFunctionChange = MathF.Abs(acceptedFinancialValue - candidateFinancialValue);
                        if (movingAverageOfObjectiveChange < 0.0F)
                        {
                            // acceptance of first move
                            movingAverageOfObjectiveChange = objectiveFunctionChange;
                        }
                        else
                        {
                            // all subsequent acceptances
                            movingAverageOfObjectiveChange = movingAverageMemory * movingAverageOfObjectiveChange + (1.0F - movingAverageMemory) * objectiveFunctionChange;
                        }

                        acceptedFinancialValue = candidateFinancialValue;
                        this.CurrentTrajectory.CopyTreeGrowthFrom(candidateTrajectory);
                        ++perfCounters.MovesAccepted;

                        if (acceptedFinancialValue > this.FinancialValue.GetHighestValue())
                        {
                            this.CopyTreeGrowthToBestTrajectory(coordinate, this.CurrentTrajectory);
                            iterationsSinceReheatOrFinancialValueIncreased = 0;
                        }

                        iterationsSinceMoveTypeOrObjectiveChange = 0;
                        Debug.Assert(movingAverageOfObjectiveChange > 0.0F);
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

                    if (iterationsSinceMoveTypeOrObjectiveChange > this.ChangeToExchangeAfter)
                    {
                        this.MoveType = MoveType.TwoOptExchange;
                        iterationsSinceMoveTypeOrObjectiveChange = 0;
                    }
                    if (iterationsSinceReheatOrFinancialValueIncreased > this.ReheatAfter)
                    {
                        // while it's unlikely alpha would be close enough to 1 and reheat intervals short enough to drive the acceptance probability
                        // above one, it is possible
                        meanAcceptanceProbability = MathF.Min(meanAcceptanceProbability + this.ReheatBy, 1.0F);
                        logMeanAcceptanceProbability = MathV.Ln(meanAcceptanceProbability);
                        iterationsSinceReheatOrFinancialValueIncreased = 0;
                    }
                }
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}
