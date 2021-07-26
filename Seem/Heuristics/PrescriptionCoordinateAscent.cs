using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionCoordinateAscent: PrescriptionHeuristic
    {
        public bool Gradient { get; set; }
        public bool IsStochastic { get; set; }
        public bool RestartOnLocalMaximum { get; set; }

        public PrescriptionCoordinateAscent(OrganonStand stand, PrescriptionParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters, evaluatesAcrossRotationsAndDiscountRates: false)
        {
            this.Gradient = false;
            this.IsStochastic = false;
            this.RestartOnLocalMaximum = false;
        }

        private float EvaluateCandidatePosition(MoveState moveState, HeuristicPerformanceCounters perfCounters)
        {
            // skip moves which return to recently visited positions
            if (moveState.CandidatePositionIsRecentlyVisited())
            {
                return Single.MinValue;
            }

            // evaluate objective function at position since it has not been recently visited
            IList<IHarvest> harvests = this.CurrentTrajectory.Treatments.Harvests;
            Debug.Assert((harvests.Count < 4) && (moveState.CandidateIntensities.Length == 3 * harvests.Count));

            ThinByPrescription? firstThinPrescription = null;
            if (harvests.Count > 0)
            {
                firstThinPrescription = (ThinByPrescription)harvests[0];
                firstThinPrescription.FromAbovePercentage = moveState.CandidateIntensities[0];
                firstThinPrescription.ProportionalPercentage = moveState.CandidateIntensities[1];
                firstThinPrescription.FromBelowPercentage = moveState.CandidateIntensities[2];
            }
            ThinByPrescription? secondThinPrescription = null;
            if (harvests.Count > 1)
            {
                secondThinPrescription = (ThinByPrescription)harvests[1];
                secondThinPrescription.FromAbovePercentage = moveState.CandidateIntensities[3];
                secondThinPrescription.ProportionalPercentage = moveState.CandidateIntensities[4];
                secondThinPrescription.FromBelowPercentage = moveState.CandidateIntensities[5];
            }
            ThinByPrescription? thirdThinPrescription = null;
            if (harvests.Count > 2)
            {
                thirdThinPrescription = (ThinByPrescription)harvests[2];
                thirdThinPrescription.FromAbovePercentage = moveState.CandidateIntensities[6];
                thirdThinPrescription.ProportionalPercentage = moveState.CandidateIntensities[7];
                thirdThinPrescription.FromBelowPercentage = moveState.CandidateIntensities[8];
            }

            perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();

            float candidateFinancialValue = this.GetFinancialValue(this.CurrentTrajectory, moveState.FinancialIndex);
            float acceptedFinancialValue = this.FinancialValue.GetHighestValue();
            float financialValueChange = candidateFinancialValue - acceptedFinancialValue;
            if (financialValueChange > 0.0F)
            {
                // accept change of prescription if it improves upon the best solution
                acceptedFinancialValue = candidateFinancialValue;
                this.CopyTreeGrowthToBestTrajectory(this.CurrentTrajectory);

                if (this.lastNImprovingMovesLog != null)
                {
                    this.lastNImprovingMovesLog.TryAddMove(perfCounters.MovesAccepted + perfCounters.MovesRejected, firstThinPrescription, secondThinPrescription, thirdThinPrescription);
                }

                this.FinancialValue.TryAddMove(acceptedFinancialValue, candidateFinancialValue);
                ++perfCounters.MovesAccepted;
            }
            else
            {
                if (this.RunParameters.LogOnlyImprovingMoves == false)
                {
                    this.FinancialValue.TryAddMove(acceptedFinancialValue, candidateFinancialValue);
                }
                ++perfCounters.MovesRejected;
            }

            if (this.allMoveLog != null)
            {
                this.allMoveLog.TryAddMove(firstThinPrescription, secondThinPrescription, thirdThinPrescription);
            }

            if (harvests.Count > 0)
            {
                moveState.UpsertRecentlyVisitedPosition();
            }
            return financialValueChange;
        }

        private bool EvaluateGradientMove(MoveState moveState, HeuristicPerformanceCounters perfCounters)
        {
            // synthesize vector in direction of most recent improving gradient with length step size
            int dimensions = moveState.MostRecentIncrementGradient.Length;
            Span<float> candidateIntensities = stackalloc float[dimensions]; // separate from move state to simplify rollback if move isn't feasible
            float euclideanFeasibleGradientLength = 0.0F;
            for (int dimension = 0; dimension < dimensions; ++dimension)
            {
                // at least for now, suppress gradients which obviously go beyond constaints
                // Since this zeros out infeasible motions it results in a larger gradient motion in feasible dimensions.
                float netGradientInDimension = moveState.MostRecentIncrementGradient[dimension] - moveState.MostRecentDecrementGradient[dimension];
                float currentPositionInDimension = moveState.ThinningIntensities[dimension];
                if ((netGradientInDimension < 0.0F) && (currentPositionInDimension == 0.0F))
                {
                    continue;
                }
                if ((netGradientInDimension > 0.0F) && (currentPositionInDimension >= moveState.MaximumIntensities[dimension]))
                {
                    continue;
                }

                candidateIntensities[dimension] = netGradientInDimension;
                euclideanFeasibleGradientLength += netGradientInDimension * netGradientInDimension;
            }
            if (euclideanFeasibleGradientLength <= 0.0F)
            {
                Debug.Assert(euclideanFeasibleGradientLength == 0.0F);
                return false; // gradient points into constrained area
            }
            euclideanFeasibleGradientLength = MathF.Sqrt(euclideanFeasibleGradientLength);

            float euclideanStepSize = 0.0F;
            float previousThinIntensityAdjustment = 1.0F;
            for (int thinIndex = 0; thinIndex < moveState.Thinnings; ++thinIndex)
            {
                float lengthMultiplier = moveState.StepSizeByThinning[thinIndex] / (euclideanFeasibleGradientLength * previousThinIntensityAdjustment);
                
                int baseIndex = 3 * thinIndex;
                float currentIntensityOfThin = 0.0F;
                for (int dimensionIndex = 0; dimensionIndex < 3; ++dimensionIndex)
                {
                    int dimension = baseIndex + dimensionIndex;
                    float currentDimensionIntensity = moveState.ThinningIntensities[dimension];
                    float stepInDimension = lengthMultiplier * candidateIntensities[dimension];
                    float candidateIntensityInDimension = currentDimensionIntensity + stepInDimension;
                    if (candidateIntensityInDimension < 0.0F)
                    {
                        // since lengthMultiplier is not calculated iteratively, clamping shrinks the gradient step
                        candidateIntensityInDimension = 0.0F;
                        stepInDimension = moveState.ThinningIntensities[dimension];
                    }
                    else if (candidateIntensityInDimension > moveState.MaximumIntensities[dimension])
                    {
                        // also shrinks gradient step size
                        candidateIntensityInDimension = moveState.MaximumIntensities[dimension];
                        stepInDimension = moveState.MaximumIntensities[dimension] - moveState.ThinningIntensities[dimension];
                    }

                    candidateIntensities[dimension] = candidateIntensityInDimension;
                    euclideanStepSize += stepInDimension * stepInDimension;
                    currentIntensityOfThin += currentDimensionIntensity;
                }

                previousThinIntensityAdjustment *= MathF.Max(1.0F - 0.01F * currentIntensityOfThin, 100.0F);
            }

            // check if move is feasible within constraints
            for (int thinIndex = 0; thinIndex < dimensions; thinIndex += 3)
            {
                float candidateTotalIntensity = candidateIntensities[thinIndex] + candidateIntensities[thinIndex + 1] + candidateIntensities[thinIndex + 2];
                if ((candidateTotalIntensity > this.HeuristicParameters.MaximumIntensity) ||
                    (candidateTotalIntensity < this.HeuristicParameters.MinimumIntensity))
                {
                    // TODO: truncate or extend gradient within this thinning to statisfy constraints?
                    return false;
                }
            }
            euclideanStepSize = MathF.Sqrt(euclideanStepSize);
            if (euclideanStepSize < this.HeuristicParameters.MinimumIntensityStepSize)
            {
                // TODO: adjust minimum step size using thinning weights?
                return false;
            }

            // evaluate move
            candidateIntensities.CopyTo(moveState.CandidateIntensities);
            float objectiveFunctionChange = this.EvaluateCandidatePosition(moveState, perfCounters);

            // update or roll back intensities depending on whether move was accepted or rejected
            bool moveAccepted = objectiveFunctionChange > 0.0F;
            if (moveAccepted)
            {
                Array.Copy(moveState.CandidateIntensities, 0, moveState.ThinningIntensities, 0, dimensions);
            }
            else
            {
                Array.Copy(moveState.ThinningIntensities, 0, moveState.CandidateIntensities, 0, dimensions);
            }

            moveState.UpsertRecentlyVisitedPosition();
            return moveAccepted;
        }

        private bool EvaluateOneDimensionalMove(MoveDirection direction, int thinIndex, int dimensionIndex, MoveState moveState, HeuristicPerformanceCounters perfCounters)
        {
            // generate move
            float candidateStep = PrescriptionCoordinateAscent.GetCandidateStep(direction, thinIndex, dimensionIndex, moveState, out float candidateStepSize);

            // check if move is feasible within constraints
            if (candidateStepSize < this.HeuristicParameters.MinimumIntensityStepSize)
            {
                return false;
            }
            int baseIndex = 3 * thinIndex;
            float totalIntensity = moveState.CandidateIntensities[baseIndex] + moveState.CandidateIntensities[baseIndex + 1] + moveState.CandidateIntensities[baseIndex + 2];
            float candidateTotalIntensity = totalIntensity + candidateStep;
            if ((candidateTotalIntensity > this.HeuristicParameters.MaximumIntensity) ||
                (candidateTotalIntensity < this.HeuristicParameters.MinimumIntensity))
            {
                return false;
            }

            // evaluate move
            int dimension = baseIndex + dimensionIndex;
            moveState.CandidateIntensities[dimension] += candidateStep;
            float objectiveFunctionChange = this.EvaluateCandidatePosition(moveState, perfCounters);

            // update or roll back intensities depending on whether move was accepted or rejected
            bool moveAccepted = objectiveFunctionChange > 0.0F;
            if (moveAccepted)
            {
                // basic stochastic testing @ n = 100
                // ~4% reduction in timesteps from remembering a dimension's previous movement direction
                // maybe ~1% reduction in timesteps and possibly some disadvantage to repeating steps in the direction of an accepted move
                // no advantage to combining these two forms of short term memory
                moveState.MostRecentAcceptedDirection[dimension] = direction;
                // --dimensionIndex; // immediately making another move in the same direction unlikely to provide overall benefit
                    
                // update most recent accepted dimension to allow checking for and avoiding moving back to previous positions
                moveState.ThinningIntensities[dimension] = moveState.CandidateIntensities[dimension];
            }
            else
            {
                moveState.CandidateIntensities[dimension] = moveState.ThinningIntensities[dimension];
            }

            // if enabled, update gradient information based on result
            if (this.Gradient)
            {
                float gradient = objectiveFunctionChange / candidateStepSize;
                if (direction == MoveDirection.Increase)
                {
                    moveState.MostRecentIncrementGradient[dimension] = gradient;
                }
                else
                {
                    Debug.Assert(direction == MoveDirection.Decrease);
                    moveState.MostRecentDecrementGradient[dimension] = gradient;
                }
            }

            return moveAccepted;
        }

        //private bool EvaluateTwoDimensionalMove(MoveDirection direction0, int harvestIndex0, int dimensionIndex0, MoveDirection direction1, int harvestIndex1, int dimensionIndex1, MoveState moveState, HeuristicPerformanceCounters perfCounters)
        //{
        //    // generate move
        //    float candidateStep1 = PrescriptionCoordinateAscent.GetCandidateStep(direction0, harvestIndex0, dimensionIndex0, moveState, out float candidateStepSize1);
        //    float candidateStep2 = PrescriptionCoordinateAscent.GetCandidateStep(direction1, harvestIndex1, dimensionIndex1, moveState, out float candidateStepSize2);

        //    // check if move is feasible within constraints
        //    if ((candidateStep1 == 0.0F) || (candidateStep2 == 0.0F))
        //    {
        //        // move has collapsed to unidrectional
        //        return false;
        //    }
        //    float candidateStepSize = MathF.Sqrt(candidateStep1 * candidateStep1 + candidateStep2 * candidateStep2);
        //    if (candidateStepSize < this.HeuristicParameters.MinimumIntensityStepSize)
        //    {
        //        // move is too short
        //        return false;
        //    }

        //    int baseIndex0 = 3 * harvestIndex0;
        //    int baseIndex1 = 3 * harvestIndex1;
        //    if (baseIndex0 == baseIndex1)
        //    {
        //        float totalIntensity = moveState.CandidateIntensities[baseIndex0] + moveState.CandidateIntensities[baseIndex0 + 1] + moveState.CandidateIntensities[baseIndex0 + 2];
        //        float candidateTotalIntensity = totalIntensity + candidateStep1 + candidateStep2;
        //        if ((candidateTotalIntensity > this.HeuristicParameters.MaximumIntensity) ||
        //            (candidateTotalIntensity < this.HeuristicParameters.MinimumIntensity))
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        float totalIntensity1 = moveState.CandidateIntensities[baseIndex0] + moveState.CandidateIntensities[baseIndex0 + 1] + moveState.CandidateIntensities[baseIndex0 + 2];
        //        float candidateTotalIntensity1 = totalIntensity1 + candidateStep1;
        //        if ((candidateTotalIntensity1 > this.HeuristicParameters.MaximumIntensity) ||
        //            (candidateTotalIntensity1 < this.HeuristicParameters.MinimumIntensity))
        //        {
        //            return false;
        //        }

        //        float totalIntensity2 = moveState.CandidateIntensities[baseIndex1] + moveState.CandidateIntensities[baseIndex1 + 1] + moveState.CandidateIntensities[baseIndex1 + 2];
        //        float candidateTotalIntensity2 = totalIntensity2 + candidateStep2;
        //        if ((candidateTotalIntensity2 > this.HeuristicParameters.MaximumIntensity) ||
        //            (candidateTotalIntensity2 < this.HeuristicParameters.MinimumIntensity))
        //        {
        //            return false;
        //        }
        //    }

        //    // evaluate move
        //    int dimension0 = baseIndex0 + dimensionIndex0;
        //    int dimension1 = baseIndex1 + dimensionIndex1;
        //    moveState.CandidateIntensities[dimension0] += candidateStep1;
        //    moveState.CandidateIntensities[dimension1] += candidateStep2;
        //    float objectiveFunctionChange = this.EvaluateCandidatePosition(moveState, perfCounters);

        //    // update or roll back intensities depending on whether move was accepted or rejected
        //    bool moveAccepted = objectiveFunctionChange > 0.0F;
        //    if (moveAccepted)
        //    {
        //        moveState.MostRecentAcceptedDirection[dimension0] = direction0;
        //        moveState.MostRecentAcceptedDirection[dimension1] = direction1;
        //        moveState.ThinningIntensities[dimension0] = moveState.CandidateIntensities[dimension0];
        //        moveState.ThinningIntensities[dimension1] = moveState.CandidateIntensities[dimension1];
        //    }
        //    else
        //    {
        //        moveState.CandidateIntensities[dimension0] = moveState.ThinningIntensities[dimension0];
        //        moveState.CandidateIntensities[dimension1] = moveState.ThinningIntensities[dimension1];
        //    }

        //    // if enabled, update gradient information based on result
        //    if (this.Gradient)
        //    {
        //        float gradient1 = objectiveFunctionChange / candidateStepSize1;
        //        if (direction0 == MoveDirection.Increase)
        //        {
        //            moveState.MostRecentIncrementGradient[dimension0] = gradient1;
        //        }
        //        else
        //        {
        //            Debug.Assert(direction0 == MoveDirection.Decrease);
        //            moveState.MostRecentDecrementGradient[dimension0] = gradient1;
        //        }

        //        float gradient2 = objectiveFunctionChange / candidateStepSize2;
        //        if (direction1 == MoveDirection.Increase)
        //        {
        //            moveState.MostRecentIncrementGradient[dimension1] = gradient2;
        //        }
        //        else
        //        {
        //            Debug.Assert(direction1 == MoveDirection.Decrease);
        //            moveState.MostRecentDecrementGradient[dimension1] = gradient2;
        //        }
        //    }

        //    return moveAccepted;
        //}

        protected override void EvaluateThinningPrescriptions(HeuristicResultPosition position, HeuristicResults results, HeuristicPerformanceCounters perfCounters)
        {
            // initialize search position from whatever values are already set on the current trajectory
            // This allows prescriptions to flow between positions in sweeps using GRASP, potentially reducing search effort.
            // TODO: move this code into an override of ConstructTreeSelection(float)
            IList<IHarvest> harvests = this.CurrentTrajectory.Treatments.Harvests;
            MoveState moveState = new(harvests.Count, 3)
            {
                FinancialIndex = position.FinancialIndex,
            };

            // if no thinning is specified assume, for now, that no elite solution is flowing and the search should begin from a set of
            // balanced proportional thins
            bool startFromDefaultPosition = true;
            if (harvests.Count > 0)
            {
                // set thinning intensities
                float defaultProportionalIntensity = this.GetDefaultProportionalIntensity();
                for (int thinIndex = 0; thinIndex < harvests.Count; ++thinIndex)
                {
                    //ThinByPrescription thinPrescription = (ThinByPrescription)harvests[thinIndex];
                    //float totalIntensity = thinPrescription.FromAbovePercentage + thinPrescription.ProportionalPercentage + thinPrescription.FromBelowPercentage;
                    int baseIndex = 3 * thinIndex;
                    //if (totalIntensity > 0.0F)
                    //{
                    //    if ((totalIntensity < this.HeuristicParameters.MinimumIntensity) || (totalIntensity > this.HeuristicParameters.MaximumIntensity))
                    //    {
                    //        throw new NotSupportedException("Total intensity of thinning prescription " + thinIndex + "  is " + totalIntensity + ". This does not fall within the required range of " + this.HeuristicParameters.MinimumIntensity + "-" + this.HeuristicParameters.MaximumIntensity + ".");
                    //    }

                    //    // within numerical precision, moving intensity from above and below to proportional can't violate constraints since
                    //    //   1) above and below percentages can go to zero and proportional percentage can go to maximum intensity
                    //    //   2) rearranging intensity between thinning modes is neutral with respect to minimum and maximum intensities
                    //    //TODO: construction greediness
                    //    //float fromAboveReallocation = (1.0F - this.ConstructionGreediness) * thinPrescription.FromAbovePercentage;
                    //    //float fromBelowReallocation = (1.0F - this.ConstructionGreediness) * thinPrescription.FromBelowPercentage;
                    //    //moveState.CandidateIntensities[baseIndex] = thinPrescription.FromAbovePercentage - fromAboveReallocation;
                    //    //moveState.CandidateIntensities[baseIndex + 1] = thinPrescription.ProportionalPercentage + fromAboveReallocation + fromBelowReallocation;
                    //    //moveState.CandidateIntensities[baseIndex + 2] = thinPrescription.FromBelowPercentage - fromBelowReallocation;
                    //    moveState.CandidateIntensities[baseIndex] = thinPrescription.FromAbovePercentage;
                    //    moveState.CandidateIntensities[baseIndex + 1] = thinPrescription.ProportionalPercentage;
                    //    moveState.CandidateIntensities[baseIndex + 2] = thinPrescription.FromBelowPercentage;
                    //    startFromDefaultPosition = false;
                    //}
                    //else
                    //{
                        moveState.CandidateIntensities[baseIndex] = 0.0F;
                        moveState.CandidateIntensities[baseIndex + 1] = defaultProportionalIntensity;
                        moveState.CandidateIntensities[baseIndex + 2] = 0.0F;
                    //}
                }

                // swapping intensities between thins found to be mildly detrimental in basic testing
                //if (harvests.Count > 1)
                //{
                //    if ((this.ConstructionGreediness == Constant.Grasp.FullyRandomConstructionForMaximization) || (this.Pseudorandom.GetPseudorandomByteAsProbability() > this.ConstructionGreediness))
                //    {
                //        float fromAboveBuffer = moveState.CandidateIntensities[0];
                //        moveState.CandidateIntensities[0] = moveState.CandidateIntensities[3];
                //        moveState.CandidateIntensities[3] = fromAboveBuffer;

                //        float proportionalBuffer = moveState.CandidateIntensities[1];
                //        moveState.CandidateIntensities[1] = moveState.CandidateIntensities[4];
                //        moveState.CandidateIntensities[4] = proportionalBuffer;

                //        float fromBelowBuffer = moveState.CandidateIntensities[2];
                //        moveState.CandidateIntensities[2] = moveState.CandidateIntensities[5];
                //        moveState.CandidateIntensities[5] = fromBelowBuffer;
                //    }

                //    if (harvests.Count > 2)
                //    {
                //        if ((this.ConstructionGreediness == Constant.Grasp.FullyRandomConstructionForMaximization) || (this.Pseudorandom.GetPseudorandomByteAsProbability() > this.ConstructionGreediness))
                //        {
                //            float fromAboveBuffer = moveState.CandidateIntensities[3];
                //            moveState.CandidateIntensities[3] = moveState.CandidateIntensities[6];
                //            moveState.CandidateIntensities[6] = fromAboveBuffer;

                //            float proportionalBuffer = moveState.CandidateIntensities[4];
                //            moveState.CandidateIntensities[4] = moveState.CandidateIntensities[7];
                //            moveState.CandidateIntensities[7] = proportionalBuffer;

                //            float fromBelowBuffer = moveState.CandidateIntensities[5];
                //            moveState.CandidateIntensities[5] = moveState.CandidateIntensities[8];
                //            moveState.CandidateIntensities[8] = fromBelowBuffer;
                //        }
                //    }
                //}

                // complete initialization
                for (int harvest = 0; harvest < harvests.Count; ++harvest)
                {
                    int baseIndex = 3 * harvest;
                    moveState.ThinningIntensities[baseIndex] = moveState.CandidateIntensities[baseIndex];
                    moveState.ThinningIntensities[baseIndex + 1] = moveState.CandidateIntensities[baseIndex + 1];
                    moveState.ThinningIntensities[baseIndex + 2] = moveState.CandidateIntensities[baseIndex + 2];

                    moveState.MaximumIntensities[baseIndex] = this.HeuristicParameters.FromAbovePercentageUpperLimit;
                    moveState.MaximumIntensities[baseIndex + 1] = this.HeuristicParameters.ProportionalPercentageUpperLimit;
                    moveState.MaximumIntensities[baseIndex + 2] = this.HeuristicParameters.FromBelowPercentageUpperLimit;
                }
            }

            this.EvaluateCandidatePosition(moveState, perfCounters); // evaluate initial "move"
            if (harvests.Count == 0)
            {
                // no thinning prescriptions to search
                return;
            }

            this.SearchFromPosition(moveState, perfCounters); // look for improving moves in neighborhood

            // restart search from default position if requested
            Debug.Assert(perfCounters.MovesRejected > 0);
            if (this.RestartOnLocalMaximum && (perfCounters.MovesAccepted == 1) && (startFromDefaultPosition == false))
            {
                // move to default position
                float defaultProportionalIntensity = this.GetDefaultProportionalIntensity();
                for (int harvest = 0; harvest < harvests.Count; ++harvest)
                {
                    int baseIndex = 3 * harvest;
                    moveState.CandidateIntensities[baseIndex] = 0.0F;
                    moveState.CandidateIntensities[baseIndex + 1] = defaultProportionalIntensity;
                    moveState.CandidateIntensities[baseIndex + 2] = 0.0F;

                    moveState.ThinningIntensities[baseIndex] = 0.0F;
                    moveState.ThinningIntensities[baseIndex + 1] = defaultProportionalIntensity;
                    moveState.ThinningIntensities[baseIndex + 2] = 0.0F;
                }

                // restart search
                this.SearchFromPosition(moveState, perfCounters);
            }
        }

        private static float GetCandidateStep(MoveDirection direction, int thinIndex, int dimensionIndex, MoveState moveState, out float candidateStepSize)
        {
            float adjustmentForPreviousThinIntensity = 1.0F;
            if (thinIndex == 1)
            {
                //int periodBeforeFirstThin = this.CurrentTrajectory.Treatments.Harvests[0].Period - 1;
                //OrganonStandDensity? firstThinDensity = this.CurrentTrajectory.DensityByPeriod[periodBeforeFirstThin];
                //int periodBeforeSecondThin = this.CurrentTrajectory.Treatments.Harvests[1].Period - 1;
                //OrganonStandDensity? secondThinDensity = this.CurrentTrajectory.DensityByPeriod[periodBeforeSecondThin];
                //if ((firstThinDensity != null) && (secondThinDensity != null))
                //{
                //    adjustmentForPreviousThinIntensity = secondThinDensity.BasalAreaPerAcre / (firstThinDensity.BasalAreaPerAcre - this.CurrentTrajectory.BasalAreaRemoved[periodBeforeFirstThin + 1]);
                //}
                //else
                //{
                    float thin0intensity = moveState.ThinningIntensities[0] + moveState.ThinningIntensities[1] + moveState.ThinningIntensities[2];
                    Debug.Assert(thin0intensity < 100.0F);
                    adjustmentForPreviousThinIntensity = MathF.Min(1.0F / (1.0F - 0.01F * thin0intensity), 100.0F);
                //}
            }
            candidateStepSize = adjustmentForPreviousThinIntensity * moveState.StepSizeByThinning[thinIndex];

            float candidateStep;
            int dimension = 3 * thinIndex + dimensionIndex;
            if (direction == MoveDirection.Increase)
            {
                float candidateIntensity = moveState.CandidateIntensities[dimension] + candidateStepSize;
                if (candidateIntensity > moveState.MaximumIntensities[dimension])
                {
                    candidateStepSize = moveState.MaximumIntensities[dimension] - moveState.CandidateIntensities[dimension];
                }
                candidateStep = candidateStepSize;
            }
            else
            {
                Debug.Assert(direction == MoveDirection.Decrease);
                float candidateIntensity = moveState.CandidateIntensities[dimension] - candidateStepSize;
                if (candidateIntensity < 0.0F)
                {
                    candidateStepSize = moveState.CandidateIntensities[dimension];
                }
                candidateStep = -candidateStepSize;
            }

            return candidateStep;
        }

        private float GetDefaultProportionalIntensity()
        {
            float defaultProportionalIntensity = 0.5F * (this.HeuristicParameters.MaximumIntensity + this.HeuristicParameters.MinimumIntensity);
            if (defaultProportionalIntensity > this.HeuristicParameters.ProportionalPercentageUpperLimit)
            {
                defaultProportionalIntensity = this.HeuristicParameters.ProportionalPercentageUpperLimit;
            }
            if (defaultProportionalIntensity < this.HeuristicParameters.MinimumIntensity)
            {
                throw new NotSupportedException();
            }
            return defaultProportionalIntensity;
        }

        public override string GetName()
        {
            if (this.IsStochastic)
            {
                return "PrescriptionStochasticAscent";
            }
            return "PrescriptionCoordinateAscent";
        }

        private void SearchFromPosition(MoveState moveState, HeuristicPerformanceCounters perfCounters)
        {
            Array.Fill(moveState.StepSizeByThinning, this.HeuristicParameters.DefaultIntensityStepSize);
            while (moveState.StepSizeByThinning[0] >= this.HeuristicParameters.MinimumIntensityStepSize)
            {
                if (this.IsStochastic)
                {
                    this.Pseudorandom.Shuffle(moveState.Dimensions);
                }

                // unidimensional moves
                bool atLeastOneMoveAccepted = false;
                for (int thinIndex = 0; thinIndex < moveState.Thinnings; ++thinIndex)
                {
                    for (int dimensionIndex = 0; dimensionIndex < 3; ++dimensionIndex)
                    {
                        int dimension = moveState.Dimensions[3 * thinIndex + dimensionIndex];
                        MoveDirection previousDirection = moveState.MostRecentAcceptedDirection[dimension];
                        MoveDirection oppositeDirection = previousDirection == MoveDirection.Decrease ? MoveDirection.Increase : MoveDirection.Decrease;
                        if (this.EvaluateOneDimensionalMove(previousDirection, thinIndex, dimensionIndex, moveState, perfCounters))
                        {
                            atLeastOneMoveAccepted = true;
                        }
                        else if (this.EvaluateOneDimensionalMove(oppositeDirection, thinIndex, dimensionIndex, moveState, perfCounters))
                        {
                            atLeastOneMoveAccepted = true;
                        }
                    }
                }

                #region two dimensional moves
                // TODO: stochastic ordering
                //for (int thinIndex = 0; thinIndex < moveState.Thinnings; ++thinIndex)
                //{
                //    // increase both proportional and above
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, thinIndex, 0, MoveDirection.Increase, thinIndex, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from proportional to above
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, thinIndex, 0, MoveDirection.Decrease, thinIndex, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from above to proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, thinIndex, 0, MoveDirection.Increase, thinIndex, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // decrease both proportional and above
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, thinIndex, 0, MoveDirection.Decrease, thinIndex, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }

                //    // increase both proportional and below
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, thinIndex, 1, MoveDirection.Increase, thinIndex, 2, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from proportional to below
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, thinIndex, 1, MoveDirection.Increase, thinIndex, 2, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from below to proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, thinIndex, 1, MoveDirection.Decrease, thinIndex, 2, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // decrease both proportional and below
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, thinIndex, 1, MoveDirection.Decrease, thinIndex, 2, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //}
                //if (moveState.Dimensions.Length > 5)
                //{
                //    // increase both first and second above
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, 0, 0, MoveDirection.Increase, 1, 0, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from first above to second above
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, 0, 0, MoveDirection.Increase, 1, 0, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from second above to first above
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, 0, 0, MoveDirection.Decrease, 1, 0, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // decrease both first and second above
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, 0, 0, MoveDirection.Decrease, 1, 0, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }

                //    // increase both first above and second proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, 0, 0, MoveDirection.Increase, 1, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from first above to second proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, 0, 0, MoveDirection.Increase, 1, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from first above to second proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, 0, 0, MoveDirection.Decrease, 1, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // decrease both first above and second proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, 0, 0, MoveDirection.Decrease, 1, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }

                //    // increase both first and second proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, 0, 1, MoveDirection.Increase, 1, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from first proportional to second proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, 0, 1, MoveDirection.Increase, 1, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from second proportional to first proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, 0, 1, MoveDirection.Decrease, 1, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // decrease both first and second proportional
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, 0, 1, MoveDirection.Decrease, 1, 1, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }

                //    // increase both first and second below
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, 0, 2, MoveDirection.Increase, 1, 2, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from first below to second below
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, 0, 2, MoveDirection.Increase, 1, 2, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // flow intensity from second below to first below
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Increase, 0, 2, MoveDirection.Decrease, 1, 2, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //    // decrease both first and second below
                //    if (this.EvaluateTwoDimensionalMove(MoveDirection.Decrease, 0, 2, MoveDirection.Decrease, 1, 2, moveState, perfCounters))
                //    {
                //        atLeastOneMoveAccepted = true;
                //    }
                //}
                #endregion

                // basic stochastic testing @ n = 100
                // ~2% reduction in timesteps from inclusion of a single gradient move per coordinate sweep, ~0.1% increase in financial value, possibly detrimental
                // no clear advantage to repeating gradient moves so long as they're improving (±2% timesteps, ±0.1% financial value, one outlier of 25% reduction in timesteps with 2% reduction in mean financial value observed)
                // no ability to update direction of gradient when repeating gradient moves due to collinearity
                if (this.Gradient && this.EvaluateGradientMove(moveState, perfCounters))
                {
                    atLeastOneMoveAccepted = true;
                }

                if (atLeastOneMoveAccepted == false)
                {
                    // the closer the multiplier is to 1.0 the lower the convergence and, at least as a general principle, the more likely
                    // it is higher quality solutions will be found due to increased search intensity compared to smaller multipliers
                    // Variability in the number of timesteps to 1/ε convergence is substantial but a multiplier of 0.5 appears to be a
                    // reasonable compromise and is seems a common default.
                    for (int thinIndex = 0; thinIndex < moveState.Thinnings; ++thinIndex)
                    {
                        moveState.StepSizeByThinning[thinIndex] *= this.HeuristicParameters.StepSizeMultiplier;
                    }
                }
            }
        }

        private class MoveState
        {
            private readonly float[][] recentlyVisitedPositions;
            private int recentlyVisitedPositionUpdateIndex;

            public float[] CandidateIntensities { get; private init; }
            public int[] Dimensions { get; private init; }
            public int FinancialIndex { get; init; }
            public float[] MaximumIntensities { get; private init; }
            public MoveDirection[] MostRecentAcceptedDirection { get; private init; }
            public float[] MostRecentDecrementGradient { get; private init; }
            public float[] MostRecentIncrementGradient { get; private init; }
            public float[] StepSizeByThinning { get; set; }
            public float[] ThinningIntensities { get; set; }
            public int Thinnings { get; init; }

            public MoveState(int thinnings, int dimensionsPerHarvest)
            {
                int dimensions = dimensionsPerHarvest * thinnings;

                // each iteration generates 2*dimensions moves, unless prevented by a constraint, plus possibly also a gradient move
                // Minimum memory requirement to prevent reevaluation of an immediately previous move is therefore 2*dimensions. Similarly,
                // guaranteed avoidance of repeating any of n previous moves requires 2*n*dimensions. Actual requirements are likely to be lower
                // if more than one move is accepted per iteration.
                this.recentlyVisitedPositions = new float[4 * dimensions + 2][]; // default to n = 2 with allowance for dimensionally aligned gradient moves
                this.recentlyVisitedPositionUpdateIndex = 0;

                this.CandidateIntensities = new float[dimensions];
                this.Dimensions = ArrayExtensions.CreateSequentialIndices(dimensions);
                this.FinancialIndex = Constant.HeuristicDefault.FinancialIndex;
                this.MaximumIntensities = new float[dimensions];
                this.MostRecentAcceptedDirection = new MoveDirection[dimensions]; // defaults to increment
                this.MostRecentDecrementGradient = new float[dimensions]; // unused if gradient disabled
                this.MostRecentIncrementGradient = new float[dimensions];
                this.StepSizeByThinning = new float[thinnings];
                this.ThinningIntensities = new float[dimensions];
                this.Thinnings = thinnings;
            }

            public bool CandidatePositionIsRecentlyVisited()
            {
                int positionIndex = this.recentlyVisitedPositionUpdateIndex;
                for (int recentPositionsChecked = 0; recentPositionsChecked < this.recentlyVisitedPositions.Length; ++recentPositionsChecked)
                {
                    float[]? recentlyVisitedPosition = this.recentlyVisitedPositions[positionIndex];
                    if (recentlyVisitedPosition == null)
                    {
                        break;
                    }

                    bool positionMatchesCandidate = true;
                    for (int dimension = 0; dimension < this.CandidateIntensities.Length; ++dimension)
                    {
                        if (this.CandidateIntensities[dimension] != recentlyVisitedPosition[dimension])
                        {
                            positionMatchesCandidate = false;
                            break;
                        }
                    }
                    if (positionMatchesCandidate)
                    {
                        return true;
                    }

                    --positionIndex;
                    if (positionIndex < 0)
                    {
                        positionIndex = this.recentlyVisitedPositions.Length - 1;
                    }
                }

                return false;
            }

            public void UpsertRecentlyVisitedPosition()
            {
                float[]? recentlyVisitedPosition = this.recentlyVisitedPositions[this.recentlyVisitedPositionUpdateIndex];
                if (recentlyVisitedPosition == null)
                {
                    recentlyVisitedPosition = new float[this.CandidateIntensities.Length];
                    this.recentlyVisitedPositions[this.recentlyVisitedPositionUpdateIndex] = recentlyVisitedPosition;
                }
                Array.Copy(this.CandidateIntensities, 0, recentlyVisitedPosition, 0, this.CandidateIntensities.Length);

                ++this.recentlyVisitedPositionUpdateIndex;
                if (this.recentlyVisitedPositionUpdateIndex >= this.recentlyVisitedPositions.Length)
                {
                    this.recentlyVisitedPositionUpdateIndex = 0;
                }
            }
        }
    }
}
