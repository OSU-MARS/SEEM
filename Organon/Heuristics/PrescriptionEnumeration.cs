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
            ThinByPrescription prescription = (ThinByPrescription)this.BestTrajectory.Configuration.Treatments.Harvests.First();
            this.Parameters.SetBestPrescription(prescription);
            return this.Parameters;
        }

        public override TimeSpan Run()
        {
            if ((this.Parameters.IntensityStep < 0.0F) || (this.Parameters.IntensityStep > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.IntensityStep));
            }
            if ((this.Parameters.MaximumIntensity < 0.0F) || (this.Parameters.MaximumIntensity > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.MaximumIntensity));
            }
            if ((this.Parameters.MinimumIntensity < 0.0F) || (this.Parameters.MinimumIntensity > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.MinimumIntensity));
            }

            if (this.Parameters.MaximumIntensity < this.Parameters.MinimumIntensity)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int intensityStepsPerThinMethod = (int)(100.0F / (this.Parameters.MaximumIntensity - this.Parameters.MinimumIntensity)) + 1;
            this.AcceptedObjectiveFunctionByMove.Capacity = intensityStepsPerThinMethod * intensityStepsPerThinMethod * intensityStepsPerThinMethod;
            this.CandidateObjectiveFunctionByMove.Capacity = this.AcceptedObjectiveFunctionByMove.Capacity;

            ThinByPrescription prescription = (ThinByPrescription)this.CurrentTrajectory.Configuration.Treatments.Harvests.First();
            for (float fromAbovePercentage = 0.0F; fromAbovePercentage < 100.0F; fromAbovePercentage += this.Parameters.IntensityStep)
            {
                for (float proportionalPercentage = 0.0F; proportionalPercentage < 100.0F; proportionalPercentage += this.Parameters.IntensityStep)
                {
                    for (float fromBelowPercentage = 0.0F; fromBelowPercentage < 100.0F; fromBelowPercentage += this.Parameters.IntensityStep)
                    {
                        float totalIntensity = fromAbovePercentage + proportionalPercentage + fromBelowPercentage;
                        if (totalIntensity < this.Parameters.MinimumIntensity)
                        {
                            continue;
                        }
                        if (totalIntensity > this.Parameters.MaximumIntensity)
                        {
                            break;
                        }

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
