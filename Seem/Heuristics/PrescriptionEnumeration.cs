using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;
using System.Linq;

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
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.ThinPeriodOrRetain)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float maximumPercentage = 0.0F;
            float minimumPercentage = 0.0F;
            ThinByPrescription? firstThinPrescription = (ThinByPrescription?)this.CurrentTrajectory.Configuration.Treatments.Harvests.FirstOrDefault();
            ThinByPrescription? secondThinPrescription = null;
            if (firstThinPrescription != null)
            {
                maximumPercentage = this.Parameters.Maximum;
                minimumPercentage = this.Parameters.Minimum;
                switch (this.Parameters.Units)
                {
                    case PrescriptionUnits.BasalAreaPerAcreRetained:
                        // obtain stand's basal area prior to thinning if it's not already available
                        if (this.CurrentTrajectory.DensityByPeriod[firstThinPrescription.Period - 1] == null)
                        {
                            // TODO: check for no conflicting other prescriptions
                            this.CurrentTrajectory.Simulate();
                        }
                        float basalAreaPerAcreBeforeThin = this.CurrentTrajectory.DensityByPeriod[firstThinPrescription.Period - 1].BasalAreaPerAcre;
                        if (maximumPercentage >= basalAreaPerAcreBeforeThin)
                        {
                            throw new ArgumentOutOfRangeException(nameof(this.Parameters.Minimum));
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

                if (this.CurrentTrajectory.Configuration.Treatments.Harvests.Count > 1)
                {
                    secondThinPrescription = (ThinByPrescription?)this.CurrentTrajectory.Configuration.Treatments.Harvests[1];
                }
            }

            float maximumAllowedPercentage = this.Parameters.FromAbovePercentageUpperLimit + this.Parameters.ProportionalPercentageUpperLimit + this.Parameters.FromBelowPercentageUpperLimit;
            if (maximumAllowedPercentage < minimumPercentage)
            {
                throw new ArgumentOutOfRangeException();
            }

            int intensityStepsPerThinMethod = 1;
            if (maximumPercentage > minimumPercentage)
            {
                intensityStepsPerThinMethod += (int)(100.0F / (maximumPercentage - minimumPercentage));
            }
            this.AcceptedObjectiveFunctionByMove.Capacity = intensityStepsPerThinMethod * intensityStepsPerThinMethod * intensityStepsPerThinMethod;
            this.CandidateObjectiveFunctionByMove.Capacity = this.AcceptedObjectiveFunctionByMove.Capacity;

            // This set of loops attempts to reactively set the
            // - proportional percentage based on the from above percentage
            // - from below percentage based on the proportional and from above percentages
            // in such a way that valid combinations will be found within the maximum and minimum intensities and percentage limits and
            // granularity specified by the step size. This is nontrivial and valid parameter combinations may exist which the current
            // code fails to locate.
            for (float fromAbovePercentage1 = 0.0F; fromAbovePercentage1 <= this.Parameters.FromAbovePercentageUpperLimit; fromAbovePercentage1 += this.Parameters.StepSize)
            {
                float availableProportionalAndBelowPercentage1 = maximumPercentage - fromAbovePercentage1;
                float maximumProportionalPercentage1 = MathF.Min(availableProportionalAndBelowPercentage1, this.Parameters.ProportionalPercentageUpperLimit);
                float requiredProportionalPercentage1 = MathF.Max(minimumPercentage - fromAbovePercentage1 - this.Parameters.FromBelowPercentageUpperLimit, 0.0F);
                for (float proportionalPercentage1 = requiredProportionalPercentage1; proportionalPercentage1 <= maximumProportionalPercentage1; proportionalPercentage1 += this.Parameters.StepSize)
                {
                    float availableBelowPercentage1 = availableProportionalAndBelowPercentage1 - proportionalPercentage1;
                    float maximumFromBelowPercentage1 = MathF.Min(availableBelowPercentage1, this.Parameters.FromBelowPercentageUpperLimit);
                    float requiredBelowPercentage1 = MathF.Max(minimumPercentage - proportionalPercentage1 - fromAbovePercentage1, 0.0F);
                    for (float fromBelowPercentage1 = requiredBelowPercentage1; fromBelowPercentage1 <= maximumFromBelowPercentage1; fromBelowPercentage1 += this.Parameters.StepSize)
                    {
                        Debug.Assert(fromBelowPercentage1 >= 0.0F);
                        float totalIntensity1 = fromAbovePercentage1 + proportionalPercentage1 + fromBelowPercentage1;
                        Debug.Assert(totalIntensity1 >= minimumPercentage);
                        Debug.Assert(totalIntensity1 <= maximumPercentage);

                        if (firstThinPrescription != null)
                        {
                            firstThinPrescription.FromAbovePercentage = fromAbovePercentage1;
                            firstThinPrescription.FromBelowPercentage = fromBelowPercentage1;
                            firstThinPrescription.ProportionalPercentage = proportionalPercentage1;
                        }

                        float secondStepSize = this.Parameters.StepSize / (1.0F - 0.01F * totalIntensity1);
                        for (float fromAbovePercentage2 = 0.0F; fromAbovePercentage2 <= this.Parameters.FromAbovePercentageUpperLimit; fromAbovePercentage2 += secondStepSize)
                        {
                            float availableProportionalAndBelowPercentage2 = maximumPercentage - fromAbovePercentage2;
                            float maximumProportionalPercentage2 = MathF.Min(availableProportionalAndBelowPercentage2, this.Parameters.ProportionalPercentageUpperLimit);
                            float requiredProportionalPercentage2 = MathF.Max(minimumPercentage - fromAbovePercentage2 - this.Parameters.FromBelowPercentageUpperLimit, 0.0F);
                            for (float proportionalPercentage2 = requiredProportionalPercentage2; proportionalPercentage2 <= maximumProportionalPercentage2; proportionalPercentage2 += secondStepSize)
                            {
                                float availableBelowPercentage2 = availableProportionalAndBelowPercentage2 - proportionalPercentage2;
                                float maximumFromBelowPercentage2 = MathF.Min(availableBelowPercentage2, this.Parameters.FromBelowPercentageUpperLimit);
                                float requiredBelowPercentage2 = MathF.Max(minimumPercentage - proportionalPercentage2 - fromAbovePercentage2, 0.0F);
                                for (float fromBelowPercentage2 = requiredBelowPercentage2; fromBelowPercentage2 <= maximumFromBelowPercentage2; fromBelowPercentage2 += secondStepSize)
                                {
                                    if (secondThinPrescription != null)
                                    {
                                        secondThinPrescription.FromAbovePercentage = fromAbovePercentage2;
                                        secondThinPrescription.FromBelowPercentage = fromBelowPercentage2;
                                        secondThinPrescription.ProportionalPercentage = proportionalPercentage2;
                                    }

                                    this.CurrentTrajectory.DeselectAllTrees();
                                    this.CurrentTrajectory.Simulate();

                                    if (secondThinPrescription == null)
                                    {
                                        Debug.Assert((totalIntensity1 == 0.0F && this.CurrentTrajectory.ThinningVolume.ScribnerTotal.Sum() == 0.0F) || (totalIntensity1 > 0.0F && this.CurrentTrajectory.ThinningVolume.ScribnerTotal.Sum() > 0.0F));
                                    }

                                    float candidateObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
                                    if (candidateObjectiveFunction > this.BestObjectiveFunction)
                                    {
                                        // accept change of prescription if it improves upon the best solution
                                        this.BestObjectiveFunction = candidateObjectiveFunction;
                                        this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
                                    }

                                    this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                                    this.CandidateObjectiveFunctionByMove.Add(candidateObjectiveFunction);

                                    this.MoveLog.FromAbovePercentageByMove1.Add(fromAbovePercentage1);
                                    this.MoveLog.ProportionalPercentageByMove1.Add(proportionalPercentage1);
                                    this.MoveLog.FromBelowPercentageByMove1.Add(fromBelowPercentage1);

                                    this.MoveLog.FromAbovePercentageByMove2.Add(fromAbovePercentage2);
                                    this.MoveLog.ProportionalPercentageByMove2.Add(proportionalPercentage2);
                                    this.MoveLog.FromBelowPercentageByMove2.Add(fromBelowPercentage2);
                                }
                            }
                        }
                    }
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
