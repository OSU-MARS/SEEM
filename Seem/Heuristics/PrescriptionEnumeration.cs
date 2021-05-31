using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionEnumeration : Heuristic<PrescriptionParameters>
    {
        public PrescriptionMoveLog MoveLog { get; private init; }

        public PrescriptionEnumeration(OrganonStand stand, OrganonConfiguration configuration, RunParameters runParameters, PrescriptionParameters parameters)
            : base(stand, configuration, parameters, runParameters)
        {
            this.MoveLog = new PrescriptionMoveLog();
        }

        private void EnumerateThinningIntensities(ThinByPrescription thinPrescription, float percentIntensityOfPreviousThins, Action<float> evaluatePrescriptions, HeuristicPerformanceCounters perfCounters)
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
                    float basalAreaPerAcreBeforeThin = this.CurrentTrajectory.DensityByPeriod[thinPrescription.Period - 1].BasalAreaPerAcre;
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
            Debug.Assert((stepSize > 0.0F) && (stepSize <= 100.0F));

            //int intensityStepsPerThinMethod = 1;
            //if (maximumPercentage > minimumPercentage)
            //{
            //    intensityStepsPerThinMethod += (int)(100.0F / (maximumPercentage - minimumPercentage));
            //}
            //this.AcceptedObjectiveFunctionByMove.Capacity = intensityStepsPerThinMethod * intensityStepsPerThinMethod * intensityStepsPerThinMethod;
            //this.CandidateObjectiveFunctionByMove.Capacity = this.AcceptedObjectiveFunctionByMove.Capacity;

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
                        Debug.Assert(totalRelativeIntensityOfThisThin >= minimumPercentage);
                        Debug.Assert(totalRelativeIntensityOfThisThin <= maximumPercentage);

                        thinPrescription.FromAbovePercentage = fromAbovePercentage;
                        thinPrescription.FromBelowPercentage = fromBelowPercentage;
                        thinPrescription.ProportionalPercentage = proportionalPercentage;

                        evaluatePrescriptions.Invoke(totalRelativeIntensityOfThisThin);
                    }
                }
            }
        }

        private void EvaluateCurrentPrescriptions(ThinByPrescription? firstThinPrescription, ThinByPrescription? secondThinPrescription, ThinByPrescription? thirdThinPrescription, HeuristicPerformanceCounters perfCounters)
        {
            // for now, assume execution with fixed thinning times and rotation lengths, meaning tree selections do not need to be moved between periods
            // this.CurrentTrajectory.DeselectAllTrees();
            perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();

            float candidateObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
            if (candidateObjectiveFunction > this.BestObjectiveFunction)
            {
                // accept change of prescription if it improves upon the best solution
                this.BestObjectiveFunction = candidateObjectiveFunction;
                this.BestTrajectory.CopyTreeGrowthAndTreatmentsFrom(this.CurrentTrajectory);
                ++perfCounters.MovesAccepted;
            }
            else
            {
                ++perfCounters.MovesRejected;
            }

            this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
            this.CandidateObjectiveFunctionByMove.Add(candidateObjectiveFunction);

            float fromAbovePercentageFirst = 0.0F;
            float proportionalPercentageFirst = 0.0F;
            float fromBelowPercentageFirst = 0.0F;
            if (firstThinPrescription != null)
            {
                fromAbovePercentageFirst = firstThinPrescription.FromAbovePercentage;
                proportionalPercentageFirst = firstThinPrescription.ProportionalPercentage;
                fromBelowPercentageFirst = firstThinPrescription.FromBelowPercentage;
            }
            this.MoveLog.FromAbovePercentageByMove1.Add(fromAbovePercentageFirst);
            this.MoveLog.ProportionalPercentageByMove1.Add(proportionalPercentageFirst);
            this.MoveLog.FromBelowPercentageByMove1.Add(fromBelowPercentageFirst);

            float fromAbovePercentageSecond = 0.0F;
            float proportionalPercentageSecond = 0.0F;
            float fromBelowPercentageSecond = 0.0F;
            if (secondThinPrescription != null)
            {
                fromAbovePercentageSecond = secondThinPrescription.FromAbovePercentage;
                proportionalPercentageSecond = secondThinPrescription.ProportionalPercentage;
                fromBelowPercentageSecond = secondThinPrescription.FromBelowPercentage;
            }
            this.MoveLog.FromAbovePercentageByMove2.Add(fromAbovePercentageSecond);
            this.MoveLog.ProportionalPercentageByMove2.Add(proportionalPercentageSecond);
            this.MoveLog.FromBelowPercentageByMove2.Add(fromBelowPercentageSecond);

            float fromAbovePercentageThird = 0.0F;
            float proportionalPercentageThird = 0.0F;
            float fromBelowPercentageThird = 0.0F;
            if (thirdThinPrescription != null)
            {
                fromAbovePercentageThird = thirdThinPrescription.FromAbovePercentage;
                proportionalPercentageThird = thirdThinPrescription.ProportionalPercentage;
                fromBelowPercentageThird = thirdThinPrescription.FromBelowPercentage;
            }
            this.MoveLog.FromAbovePercentageByMove3.Add(fromAbovePercentageThird);
            this.MoveLog.ProportionalPercentageByMove3.Add(proportionalPercentageThird);
            this.MoveLog.FromBelowPercentageByMove3.Add(fromBelowPercentageThird);
        }

        public override string GetName()
        {
            return "Prescription";
        }

        public override IHeuristicMoveLog GetMoveLog()
        {
            return this.MoveLog;
        }

        public override HeuristicParameters GetParameters()
        {
            return this.HeuristicParameters;
        }

        public override HeuristicPerformanceCounters Run(HeuristicSolutionPosition position, HeuristicSolutionIndex solutionIndex)
        {
            if ((this.HeuristicParameters.FromAbovePercentageUpperLimit < 0.0F) || (this.HeuristicParameters.FromAbovePercentageUpperLimit > 100.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.FromAbovePercentageUpperLimit));
            }
            if ((this.HeuristicParameters.FromBelowPercentageUpperLimit < 0.0F) || (this.HeuristicParameters.FromBelowPercentageUpperLimit > 100.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.FromBelowPercentageUpperLimit));
            }
            if ((this.HeuristicParameters.ProportionalPercentageUpperLimit < 0.0F) || (this.HeuristicParameters.ProportionalPercentageUpperLimit > 100.0F))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.ProportionalPercentageUpperLimit));
            }

            float intensityUpperBound = this.HeuristicParameters.Units switch
            {
                PrescriptionUnits.BasalAreaPerAcreRetained => 1000.0F,
                PrescriptionUnits.StemPercentageRemoved => 100.0F,
                _ => throw new NotSupportedException(String.Format("Unhandled units {0}.", this.HeuristicParameters.Units))
            };
            if ((this.HeuristicParameters.DefaultIntensityStepSize < 0.0F) || (this.HeuristicParameters.DefaultIntensityStepSize > intensityUpperBound))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.DefaultIntensityStepSize));
            }
            if ((this.HeuristicParameters.MaximumIntensity < 0.0F) || (this.HeuristicParameters.MaximumIntensity > intensityUpperBound))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.MaximumIntensity));
            }
            if ((this.HeuristicParameters.MinimumIntensity < 0.0F) || (this.HeuristicParameters.MinimumIntensity > intensityUpperBound))
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.MinimumIntensity));
            }
            if (this.HeuristicParameters.MaximumIntensity < this.HeuristicParameters.MinimumIntensity)
            {
                throw new InvalidOperationException(nameof(this.HeuristicParameters.MinimumIntensity));
            }

            if (this.CurrentTrajectory.Configuration.Treatments.Harvests.Count > 3)
            {
                throw new NotSupportedException("Enumeration of more than three thinnings is not currently supported.");
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            HeuristicPerformanceCounters perfCounters = new();

            // can't currently copy from existing solutions as doing so results in a call to OrganonConfiguration.CopyFrom(), which overwrites treatments
            // no need to call ConstructTreeSelection() or EvaluateInitialSelection() as tree selection is done by the thinning prescriptions during
            // stand trajectory simulation
            IList<IHarvest> harvests = this.CurrentTrajectory.Configuration.Treatments.Harvests;
            if (harvests.Count == 0)
            {
                // no thins: no intensities to enumerate so only a single growth model call to obtain a no action trajectory
                this.EvaluateCurrentPrescriptions(null, null, null, perfCounters);
            }
            else
            {
                ThinByPrescription firstThinPrescription = (ThinByPrescription)this.CurrentTrajectory.Configuration.Treatments.Harvests[0];

                if (harvests.Count == 1)
                {
                    // one thin
                    this.EnumerateThinningIntensities(firstThinPrescription, 0.0F, (float firstIntensity) =>
                    {
                        this.EvaluateCurrentPrescriptions(firstThinPrescription, null, null, perfCounters);
                    }, perfCounters);
                }
                else
                {
                    ThinByPrescription secondThinPrescription = (ThinByPrescription)this.CurrentTrajectory.Configuration.Treatments.Harvests[1];

                    if (harvests.Count == 2)
                    {
                        // two thins
                        this.EnumerateThinningIntensities(firstThinPrescription, 0.0F, (float firstIntensity) =>
                        {
                            this.EnumerateThinningIntensities(secondThinPrescription!, firstIntensity, (float secondIntensity) =>
                            {
                                this.EvaluateCurrentPrescriptions(firstThinPrescription, secondThinPrescription, null, perfCounters);
                            }, perfCounters);
                        }, perfCounters);
                    }
                    else
                    {
                        // three thins
                        ThinByPrescription thirdThinPrescription = (ThinByPrescription)this.CurrentTrajectory.Configuration.Treatments.Harvests[2];
                        this.EnumerateThinningIntensities(firstThinPrescription, 0.0F, (float firstIntensity) =>
                        {
                            this.EnumerateThinningIntensities(secondThinPrescription!, firstIntensity, (float secondIntensity) =>
                            {
                                float previousIntensity = firstIntensity + (100.0F - firstIntensity) * 0.01F * secondIntensity;
                                this.EnumerateThinningIntensities(thirdThinPrescription, previousIntensity, (float thirdIntensity) =>
                                {
                                    this.EvaluateCurrentPrescriptions(firstThinPrescription, secondThinPrescription, thirdThinPrescription, perfCounters);
                                }, perfCounters);
                            }, perfCounters);
                        }, perfCounters);
                    }
                }
            }

            stopwatch.Stop();
            perfCounters.Duration = stopwatch.Elapsed;
            return perfCounters;
        }
    }
}
