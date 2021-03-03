using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionEnumeration : Heuristic
    {
        public PrescriptionParameters Parameters { get; private init; }
        public PrescriptionMoveLog MoveLog { get; private init; }

        public PrescriptionEnumeration(OrganonStand stand, OrganonConfiguration configuration, Objective objective, PrescriptionParameters parameters)
            : base(stand, configuration, objective, parameters)
        {

            this.Parameters = parameters;
            this.MoveLog = new PrescriptionMoveLog();
        }

        private void EnumerateThinningIntensities(ThinByPrescription thinPrescription, float stepSize, Action<float> evaluatePrescriptions)
        {
            float maximumPercentage = this.Parameters.Maximum;
            float minimumPercentage = this.Parameters.Minimum;
            switch (this.Parameters.Units)
            {
                case PrescriptionUnits.BasalAreaPerAcreRetained:
                    // obtain stand's basal area prior to thinning if it's not already available
                    if (this.CurrentTrajectory.DensityByPeriod[thinPrescription.Period - 1] == null)
                    {
                        // TODO: check for no conflicting other prescriptions
                        this.CurrentTrajectory.Simulate();
                    }
                    float basalAreaPerAcreBeforeThin = this.CurrentTrajectory.DensityByPeriod[thinPrescription.Period - 1].BasalAreaPerAcre;
                    if (maximumPercentage >= basalAreaPerAcreBeforeThin)
                    {
                        throw new NotSupportedException(nameof(this.Parameters.Maximum));
                    }
                    // convert retained basal area to removed percentage
                    maximumPercentage = 100.0F * (1.0F - maximumPercentage / basalAreaPerAcreBeforeThin);
                    minimumPercentage = 100.0F * (1.0F - minimumPercentage / basalAreaPerAcreBeforeThin);
                    break;
                case PrescriptionUnits.TreePercentageRemoved:
                    // no changes needed
                    break;
                default:
                    throw new NotSupportedException(String.Format("Unhandled units {0}.", this.Parameters.Units));
            }

            float maximumAllowedPercentage = this.Parameters.FromAbovePercentageUpperLimit + this.Parameters.ProportionalPercentageUpperLimit + this.Parameters.FromBelowPercentageUpperLimit;
            if (maximumAllowedPercentage < minimumPercentage)
            {
                throw new NotSupportedException(nameof(this.Parameters));
            }

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
            for (float fromAbovePercentage = 0.0F; fromAbovePercentage <= this.Parameters.FromAbovePercentageUpperLimit; fromAbovePercentage += stepSize)
            {
                float availableProportionalAndBelowPercentage = maximumPercentage - fromAbovePercentage;
                float maximumProportionalPercentage = MathF.Min(availableProportionalAndBelowPercentage, this.Parameters.ProportionalPercentageUpperLimit);
                float requiredProportionalPercentage = MathF.Max(minimumPercentage - fromAbovePercentage - this.Parameters.FromBelowPercentageUpperLimit, 0.0F);
                for (float proportionalPercentage = requiredProportionalPercentage; proportionalPercentage <= maximumProportionalPercentage; proportionalPercentage += stepSize)
                {
                    float availableBelowPercentage = availableProportionalAndBelowPercentage - proportionalPercentage;
                    float maximumFromBelowPercentage = MathF.Min(availableBelowPercentage, this.Parameters.FromBelowPercentageUpperLimit);
                    float requiredBelowPercentage = MathF.Max(minimumPercentage - proportionalPercentage - fromAbovePercentage, 0.0F);
                    for (float fromBelowPercentage = requiredBelowPercentage; fromBelowPercentage <= maximumFromBelowPercentage; fromBelowPercentage += stepSize)
                    {
                        Debug.Assert(fromBelowPercentage >= 0.0F);
                        float totalIntensity = fromAbovePercentage + proportionalPercentage + fromBelowPercentage;
                        Debug.Assert(totalIntensity >= minimumPercentage);
                        Debug.Assert(totalIntensity <= maximumPercentage);

                        thinPrescription.FromAbovePercentage = fromAbovePercentage;
                        thinPrescription.FromBelowPercentage = fromBelowPercentage;
                        thinPrescription.ProportionalPercentage = proportionalPercentage;

                        evaluatePrescriptions.Invoke(totalIntensity);
                    }
                }
            }
        }

        private void EvaluateCurrentPrescriptions(ThinByPrescription? firstThinPrescription, ThinByPrescription? secondThinPrescription)
        {
            this.CurrentTrajectory.DeselectAllTrees();
            this.CurrentTrajectory.Simulate();

            float candidateObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
            if (candidateObjectiveFunction > this.BestObjectiveFunction)
            {
                // accept change of prescription if it improves upon the best solution
                this.BestObjectiveFunction = candidateObjectiveFunction;
                this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
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
            return this.Parameters;
        }

        public override TimeSpan Run()
        {
            if ((this.Parameters.FromAbovePercentageUpperLimit < 0.0F) || (this.Parameters.FromAbovePercentageUpperLimit > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.FromAbovePercentageUpperLimit));
            }
            if ((this.Parameters.FromBelowPercentageUpperLimit < 0.0F) || (this.Parameters.FromBelowPercentageUpperLimit > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.FromBelowPercentageUpperLimit));
            }
            if ((this.Parameters.ProportionalPercentageUpperLimit < 0.0F) || (this.Parameters.ProportionalPercentageUpperLimit > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.ProportionalPercentageUpperLimit));
            }

            float intensityUpperBound = this.Parameters.Units switch
            {
                PrescriptionUnits.BasalAreaPerAcreRetained => 1000.0F,
                PrescriptionUnits.TreePercentageRemoved => 100.0F,
                _ => throw new NotSupportedException(String.Format("Unhandled units {0}.", this.Parameters.Units))
            };
            if ((this.Parameters.StepSize < 0.0F) || (this.Parameters.StepSize > intensityUpperBound))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.StepSize));
            }
            if ((this.Parameters.Maximum < 0.0F) || (this.Parameters.Maximum > intensityUpperBound))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.Maximum));
            }
            if ((this.Parameters.Minimum < 0.0F) || (this.Parameters.Minimum > intensityUpperBound))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.Minimum));
            }
            if (this.Parameters.Maximum < this.Parameters.Minimum)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (this.CurrentTrajectory.Configuration.Treatments.Harvests.Count > 2)
            {
                throw new NotSupportedException("Enumeration of more than two thinnings is not currently supported.");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            IList<IHarvest> harvests = this.CurrentTrajectory.Configuration.Treatments.Harvests;
            if (harvests.Count > 0)
            {
                ThinByPrescription firstThinPrescription = (ThinByPrescription)this.CurrentTrajectory.Configuration.Treatments.Harvests[0];

                if (harvests.Count > 1)
                {
                    // two thins
                    ThinByPrescription secondThinPrescription = (ThinByPrescription)this.CurrentTrajectory.Configuration.Treatments.Harvests[1];
                    this.EnumerateThinningIntensities(firstThinPrescription, this.Parameters.StepSize, (float firstIntensity) =>
                    {
                        float secondStepSize = this.Parameters.StepSize / (1.0F - 0.01F * firstIntensity);
                        this.EnumerateThinningIntensities(secondThinPrescription!, secondStepSize, (float secondIntensity) =>
                        {
                            this.EvaluateCurrentPrescriptions(firstThinPrescription, secondThinPrescription);
                        });
                    });
                }
                else
                {
                    // one thin
                    this.EnumerateThinningIntensities(firstThinPrescription, this.Parameters.StepSize, (float firstIntensity) =>
                    {
                        this.EvaluateCurrentPrescriptions(firstThinPrescription, null);
                    });
                }
            }
            else
            {
                // no thins: no intensities to enumerate so only a single growth model call to obtain a no action trajectory
                this.EvaluateCurrentPrescriptions(null, null);
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
