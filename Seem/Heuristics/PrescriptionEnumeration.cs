using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Silviculture;
using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionEnumeration : PrescriptionHeuristic
    {
        private int prescriptionsEnumerated;

        public PrescriptionEnumeration(OrganonStand stand, PrescriptionParameters heuristicParameters, RunParameters runParameters)
            : base(stand, heuristicParameters, runParameters, evaluatesAcrossRotationsAndFinancialScenarios: true)
        {
            // this.prescriptionsEnumerated is (re)set in EvaluateThinningPrescriptions()
        }

        protected override void EvaluateThinningPrescriptions(StandTrajectoryCoordinate coordinate, StandTrajectories trajectories, PrescriptionPerformanceCounters perfCounters)
        {
            this.prescriptionsEnumerated = 0;

            IList<Harvest> harvests = this.CurrentTrajectory.Treatments.Harvests;
            if (harvests.Count == 0)
            {
                // no thins: no intensities to enumerate so only a single growth model call to obtain a no action trajectory
                this.EvaluateThinningPrescriptions(coordinate, null, null, null, perfCounters);
                return;
            }

            ThinByPrescription firstThinPrescription = (ThinByPrescription)harvests[0];
            if (harvests.Count == 1)
            {
                // one thin
                this.EnumerateThinningIntensities(firstThinPrescription, 0.0F, (float firstIntensity) =>
                {
                    this.EvaluateThinningPrescriptions(coordinate, firstThinPrescription, null, null, perfCounters);
                }, perfCounters);
            }
            else
            {
                ThinByPrescription secondThinPrescription = (ThinByPrescription)harvests[1];

                if (harvests.Count == 2)
                {
                    // two thins
                    this.EnumerateThinningIntensities(firstThinPrescription, 0.0F, (float firstIntensity) =>
                    {
                        this.EnumerateThinningIntensities(secondThinPrescription!, firstIntensity, (float secondIntensity) =>
                        {
                            this.EvaluateThinningPrescriptions(coordinate, firstThinPrescription, secondThinPrescription, null, perfCounters);
                        }, perfCounters);
                    }, perfCounters);
                }
                else
                {
                    // three thins
                    ThinByPrescription thirdThinPrescription = (ThinByPrescription)harvests[2];
                    this.EnumerateThinningIntensities(firstThinPrescription, 0.0F, (float firstIntensity) =>
                    {
                        this.EnumerateThinningIntensities(secondThinPrescription!, firstIntensity, (float secondIntensity) =>
                        {
                            float previousIntensity = firstIntensity + (100.0F - firstIntensity) * 0.01F * secondIntensity;
                            this.EnumerateThinningIntensities(thirdThinPrescription, previousIntensity, (float thirdIntensity) =>
                            {
                                this.EvaluateThinningPrescriptions(coordinate, firstThinPrescription, secondThinPrescription, thirdThinPrescription, perfCounters);
                            }, perfCounters);
                        }, perfCounters);
                    }, perfCounters);
                }
            }
        }

        protected void EvaluateThinningPrescriptions(StandTrajectoryCoordinate coordinate, ThinByPrescription? firstThinPrescription, ThinByPrescription? secondThinPrescription, ThinByPrescription? thirdThinPrescription, PrescriptionPerformanceCounters perfCounters)
        {
            // for now, assume execution with fixed thinning times and rotation lengths, meaning tree selections do not need to be moved between periods
            // this.CurrentTrajectory.DeselectAllTrees();
            perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();

            OrganonStandTrajectory? acceptedTrajectory = null; // lazily instantiated clone of current trajectory
            StandTrajectoryCoordinate currentCoordinate = new(coordinate);
            for (int rotationIndex = 0; rotationIndex < this.RunParameters.RotationLengths.Count; ++rotationIndex)
            {
                int endOfRotationPeriod = this.RunParameters.RotationLengths[rotationIndex];
                if (endOfRotationPeriod <= this.RunParameters.LastThinPeriod) // if needed, use < instead of <=, code should match Heuristic..ctor()
                {
                    continue; // not a valid rotation length because it doesn't include the last thinning scheduled
                }
                currentCoordinate.RotationIndex = rotationIndex;

                for (int financialIndex = 0; financialIndex < this.RunParameters.Financial.Count; ++financialIndex)
                {
                    float acceptedFinancialValue = this.FinancialValue.GetHighestValue(rotationIndex, financialIndex);
                    float candidateFinancialValue = this.GetFinancialValue(this.CurrentTrajectory, financialIndex, endOfRotationPeriod);
                    currentCoordinate.FinancialIndex = financialIndex;
                    if (candidateFinancialValue > acceptedFinancialValue)
                    {
                        // accept change of prescription if it improves upon the best solution
                        // Clone the current trajectory only once to minimize memory consumption and, in some cases, reduce data file size. For
                        // example, if this is the first tree selection generated it will be accepted across all combinations of rotation length
                        // and financial scenario.
                        if (acceptedTrajectory == null)
                        {
                            acceptedTrajectory = this.CurrentTrajectory.Clone();
                        }
                        this.BestTrajectoryByRotationAndScenario[rotationIndex, financialIndex] = acceptedTrajectory;

                        if (this.lastNImprovingMovesLog != null)
                        {
                            this.lastNImprovingMovesLog.TryAddMove(currentCoordinate, this.prescriptionsEnumerated, firstThinPrescription, secondThinPrescription, thirdThinPrescription);
                        }

                        this.FinancialValue.TryAddMove(currentCoordinate, candidateFinancialValue, candidateFinancialValue); // candidate value is accepted
                        ++perfCounters.MovesAccepted;
                    }
                    else
                    {
                        if (this.RunParameters.LogOnlyImprovingMoves == false)
                        {
                            this.FinancialValue.TryAddMove(currentCoordinate, acceptedFinancialValue, candidateFinancialValue);
                        }
                        ++perfCounters.MovesRejected;
                    }
                }
            }

            if (this.allMoveLog != null)
            {
                this.allMoveLog.TryAddMove(firstThinPrescription, secondThinPrescription, thirdThinPrescription);
            }
            ++this.prescriptionsEnumerated;
        }

        private void EnumerateThinningIntensities(ThinByPrescription thinPrescription, float percentIntensityOfPreviousThins, Action<float> evaluatePrescriptions, PrescriptionPerformanceCounters perfCounters)
        {
            if ((percentIntensityOfPreviousThins < 0.0F) || (percentIntensityOfPreviousThins > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(percentIntensityOfPreviousThins));
            }

            float maximumPercentage = this.HeuristicParameters.MaximumIntensity;
            float minimumPercentage = this.HeuristicParameters.MinimumIntensity;
            switch (this.HeuristicParameters.Units)
            {
                case PrescriptionUnits.BasalAreaPerAcreRetained:
                    // obtain stand's basal area prior to thinning if it's not already available
                    if (this.CurrentTrajectory.DensityByPeriod[thinPrescription.Period - 1] == null)
                    {
                        // TODO: check for no conflicting other prescriptions
                        perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();
                    }
                    OrganonStandDensity? standDensityAtEndOfPreviousPeriod = this.CurrentTrajectory.DensityByPeriod[thinPrescription.Period - 1];
                    Debug.Assert(standDensityAtEndOfPreviousPeriod != null);
                    float basalAreaPerAcreBeforeThin = standDensityAtEndOfPreviousPeriod.BasalAreaPerAcre;
                    if (maximumPercentage >= basalAreaPerAcreBeforeThin)
                    {
                        throw new InvalidOperationException(nameof(this.HeuristicParameters.MaximumIntensity));
                    }
                    // convert retained basal area to removed percentage
                    maximumPercentage = 100.0F * (1.0F - maximumPercentage / basalAreaPerAcreBeforeThin);
                    minimumPercentage = 100.0F * (1.0F - minimumPercentage / basalAreaPerAcreBeforeThin);
                    break;
                case PrescriptionUnits.StemPercentageRemoved:
                    // no changes needed
                    break;
                default:
                    throw new NotSupportedException(String.Format("Unhandled units {0}.", this.HeuristicParameters.Units));
            }

            float maximumAllowedPercentage = this.HeuristicParameters.FromAbovePercentageUpperLimit + this.HeuristicParameters.ProportionalPercentageUpperLimit + this.HeuristicParameters.FromBelowPercentageUpperLimit;
            if (maximumAllowedPercentage < minimumPercentage)
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters));
            }

            // adjust step size and minimum intensity for trees or basal area removed by earlier things
            // This is an approximate correction which can be made more detailed if needed.
            //   1) Both mortality and ingrowth may change tree counts between thins.
            //   2) Mortality and growth change basal area between thins.
            float previousIntensityMultiplier = 1.0F / (1.0F - 0.01F * percentIntensityOfPreviousThins);
            minimumPercentage = MathF.Min(previousIntensityMultiplier * minimumPercentage, maximumPercentage);
            float stepSize = MathF.Min(previousIntensityMultiplier * this.HeuristicParameters.DefaultIntensityStepSize, this.HeuristicParameters.MaximumIntensityStepSize);
            Debug.Assert(previousIntensityMultiplier >= 1.0F);
            Debug.Assert((stepSize >= this.HeuristicParameters.MinimumIntensityStepSize) && (stepSize <= 100.0F));

            // This set of loops attempts to reactively set the
            // - proportional percentage based on the from above percentage
            // - from below percentage based on the proportional and from above percentages
            // in such a way that valid combinations will be found within the maximum and minimum intensities and percentage limits and
            // granularity specified by the step size. This is nontrivial and valid parameter combinations may exist which the current
            // code fails to locate.
            for (float fromAbovePercentage = 0.0F; fromAbovePercentage <= this.HeuristicParameters.FromAbovePercentageUpperLimit; fromAbovePercentage += stepSize)
            {
                float availableProportionalAndBelowPercentage = maximumPercentage - fromAbovePercentage;
                float maximumProportionalPercentage = MathF.Min(availableProportionalAndBelowPercentage, this.HeuristicParameters.ProportionalPercentageUpperLimit);
                float requiredProportionalPercentage = MathF.Max(minimumPercentage - fromAbovePercentage - this.HeuristicParameters.FromBelowPercentageUpperLimit, 0.0F);
                for (float proportionalPercentage = requiredProportionalPercentage; proportionalPercentage <= maximumProportionalPercentage; proportionalPercentage += stepSize)
                {
                    float availableBelowPercentage = availableProportionalAndBelowPercentage - proportionalPercentage;
                    float maximumFromBelowPercentage = MathF.Min(availableBelowPercentage, this.HeuristicParameters.FromBelowPercentageUpperLimit);
                    float requiredBelowPercentage = MathF.Max(minimumPercentage - proportionalPercentage - fromAbovePercentage, 0.0F);
                    for (float fromBelowPercentage = requiredBelowPercentage; fromBelowPercentage <= maximumFromBelowPercentage; fromBelowPercentage += stepSize)
                    {
                        Debug.Assert(fromBelowPercentage >= 0.0F);
                        float totalRelativeIntensityOfThisThin = fromAbovePercentage + proportionalPercentage + fromBelowPercentage;
                        Debug.Assert(totalRelativeIntensityOfThisThin - minimumPercentage > -0.000002F);
                        Debug.Assert(totalRelativeIntensityOfThisThin - maximumPercentage < 0.000002F);

                        thinPrescription.FromAbovePercentage = fromAbovePercentage;
                        thinPrescription.FromBelowPercentage = fromBelowPercentage;
                        thinPrescription.ProportionalPercentage = proportionalPercentage;

                        evaluatePrescriptions.Invoke(totalRelativeIntensityOfThisThin);
                    }
                }
            }
        }

        public override string GetName()
        {
            return "PrescriptionEnumeration";
        }
    }
}
