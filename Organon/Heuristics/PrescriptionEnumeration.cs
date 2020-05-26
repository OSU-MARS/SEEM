using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionEnumeration : Heuristic
    {
        public float IntensityStep { get; set; }
        public float MaximumIntensity { get; set; }
        public float MinimumIntensity { get; set; }

        public PrescriptionEnumeration(OrganonStand stand, OrganonConfiguration configuration, int planningPeriods, Objective objective)
            : base(stand, configuration, planningPeriods, objective)
        {
            this.IntensityStep = 5.0F;
            this.MaximumIntensity = 90.0F;
            this.MinimumIntensity = 30.0F;

            this.ObjectiveFunctionByMove = new List<float>(1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetName()
        {
            return "Prescription";
        }

        public override string GetParameterHeaderForCsv()
        {
            return "above,proportional,below";
        }

        public override string GetParametersForCsv()
        {
            ThinByPrescription prescription = (ThinByPrescription)this.BestTrajectory.Configuration.Treatments.Harvests.First();
            return prescription.FromAbovePercentage.ToString("0.0", CultureInfo.InvariantCulture) + "," + 
                   prescription.ProportionalPercentage.ToString("0.0", CultureInfo.InvariantCulture) + "," + 
                   prescription.FromBelowPercentage.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public override TimeSpan Run()
        {
            if (this.ChainFrom >= 0)
            {
                throw new NotSupportedException(nameof(this.ChainFrom));
            }
            if ((this.IntensityStep < 0.0F) || (this.IntensityStep > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.IntensityStep));
            }
            if ((this.MaximumIntensity < 0.0F) || (this.MaximumIntensity > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaximumIntensity));
            }
            if ((this.MinimumIntensity < 0.0F) || (this.MinimumIntensity > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(this.MinimumIntensity));
            }

            if (this.MaximumIntensity < this.MinimumIntensity)
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
            for (float fromAbovePercentage = 0.0F; fromAbovePercentage < 100.0F; fromAbovePercentage += this.IntensityStep)
            {
                for (float proportionalPercentage = 0.0F; proportionalPercentage < 100.0F; proportionalPercentage += this.IntensityStep)
                {
                    for (float fromBelowPercentage = 0.0F; fromBelowPercentage < 100.0F; fromBelowPercentage += this.IntensityStep)
                    {
                        float totalIntensity = fromAbovePercentage + proportionalPercentage + fromBelowPercentage;
                        if (totalIntensity < this.MinimumIntensity)
                        {
                            continue;
                        }
                        if (totalIntensity > this.MaximumIntensity)
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

                        this.ObjectiveFunctionByMove.Add(candidateObjectiveFunction);
                    }
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
