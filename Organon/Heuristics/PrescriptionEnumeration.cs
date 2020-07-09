using Osu.Cof.Ferm.Organon;
using System;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionEnumeration : Heuristic
    {
        public PrescriptionParameters Parameters { get; private set; }
        public PrescriptionMoveLog MoveLog { get; private set; }

        public PrescriptionEnumeration(OrganonStand stand, OrganonConfiguration configuration, int planningPeriods, Objective objective)
            : base(stand, configuration, planningPeriods, objective)
        {
            this.Parameters = new PrescriptionParameters();
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
            if ((this.Parameters.Step < 0.0F) || (this.Parameters.Step > intensityUpperBound))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.Step));
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
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ThinByPrescription prescription = (ThinByPrescription)this.CurrentTrajectory.Configuration.Treatments.Harvests.First();
            float maximumPercentage = this.Parameters.Maximum;
            float minimumPercentage = this.Parameters.Minimum;
            switch (this.Parameters.Units)
            {
                case PrescriptionUnits.BasalAreaPerAcreRetained:
                    // obtain stand's basal area prior to thinning if it's not already available
                    if (this.CurrentTrajectory.DensityByPeriod[prescription.Period - 1] == null)
                    {
                        // TODO: check for no conflicting other prescriptions
                        this.CurrentTrajectory.Simulate();
                    }    
                    float basalAreaPerAcreBeforeThin = this.CurrentTrajectory.DensityByPeriod[prescription.Period - 1].BasalAreaPerAcre;
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
            for (float fromAbovePercentage = 0.0F; fromAbovePercentage <= this.Parameters.FromAbovePercentageUpperLimit; fromAbovePercentage += this.Parameters.Step)
            {
                float availableProportionalAndBelowPercentage = maximumPercentage - fromAbovePercentage;
                float maximumProportionalPercentage = MathF.Min(availableProportionalAndBelowPercentage, this.Parameters.ProportionalPercentageUpperLimit);
                float requiredProportionalPercentage = MathF.Max(minimumPercentage - fromAbovePercentage - this.Parameters.FromBelowPercentageUpperLimit, 0.0F);
                for (float proportionalPercentage = requiredProportionalPercentage; proportionalPercentage <= maximumProportionalPercentage; proportionalPercentage += this.Parameters.Step)
                {
                    float availableBelowPercentage = availableProportionalAndBelowPercentage - proportionalPercentage;
                    float maximumFromBelowPercentage = MathF.Min(availableBelowPercentage, this.Parameters.FromBelowPercentageUpperLimit);
                    float requiredBelowPercentage = MathF.Max(minimumPercentage - proportionalPercentage - fromAbovePercentage, 0.0F);
                    for (float fromBelowPercentage = requiredBelowPercentage; fromBelowPercentage <= maximumFromBelowPercentage; fromBelowPercentage += this.Parameters.Step)
                    {
                        Debug.Assert(fromBelowPercentage >= 0.0F);
                        float totalIntensity = fromAbovePercentage + proportionalPercentage + fromBelowPercentage;
                        Debug.Assert(totalIntensity >= minimumPercentage);
                        Debug.Assert(totalIntensity <= maximumPercentage);

                        prescription.FromAbovePercentage = fromAbovePercentage;
                        prescription.FromBelowPercentage = fromBelowPercentage;
                        prescription.ProportionalPercentage = proportionalPercentage;
                        this.CurrentTrajectory.DeselectAllTrees();
                        this.CurrentTrajectory.Simulate();
                        Debug.Assert((totalIntensity == 0.0F && this.CurrentTrajectory.HarvestVolumesByPeriod.Sum() == 0.0F) || (totalIntensity > 0.0F && this.CurrentTrajectory.HarvestVolumesByPeriod.Sum() > 0.0F));

                        float candidateObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
                        if (candidateObjectiveFunction > this.BestObjectiveFunction)
                        {
                            // accept change of prescription if it improves upon the best solution
                            this.BestObjectiveFunction = candidateObjectiveFunction;
                            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
                        }

                        this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
                        this.CandidateObjectiveFunctionByMove.Add(candidateObjectiveFunction);

                        this.MoveLog.FromAbovePercentageByMove.Add(fromAbovePercentage);
                        this.MoveLog.ProportionalPercentageByMove.Add(proportionalPercentage);
                        this.MoveLog.FromBelowPercentageByMove.Add(fromBelowPercentage);
                    }
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
