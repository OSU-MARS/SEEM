using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Prescription")]
    public class OptimizePrescription : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public Nullable<float> IntensityStep { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public Nullable<float> MaximumIntensity { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public Nullable<float> MinimumIntensity { get; set; }

        public OptimizePrescription()
        {
            this.SelectionProbabilities[0] = 0.0F;
        }

        protected override IHarvest CreateHarvest(int harvestPeriodIndex)
        {
            return new ThinByPrescription(this.HarvestPeriods[harvestPeriodIndex]);
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, float defaultSelectionProbability)
        {
            PrescriptionEnumeration enumerator = new PrescriptionEnumeration(this.Stand, organonConfiguration, planningPeriods, objective);
            if (this.IntensityStep.HasValue)
            {
                enumerator.IntensityStep = this.IntensityStep.Value;
            }
            if (this.MaximumIntensity.HasValue)
            {
                enumerator.MaximumIntensity = this.MaximumIntensity.Value;
            }
            if (this.MinimumIntensity.HasValue)
            {
                enumerator.MinimumIntensity = this.MinimumIntensity.Value;
            }
            return enumerator;
        }

        protected override string GetName()
        {
            return "Optimize-Prescription";
        }
    }
}
