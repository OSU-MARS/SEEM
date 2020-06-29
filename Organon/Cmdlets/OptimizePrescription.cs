using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Prescription")]
    public class OptimizePrescription : OptimizeCmdlet<HeuristicParameters>
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
            this.ProportionalPercentage[0] = 0.0F;
        }

        protected override IHarvest CreateHarvest(int harvestPeriodIndex)
        {
            return new ThinByPrescription(this.HarvestPeriods[harvestPeriodIndex]);
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, HeuristicParameters _)
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

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            // if needed, remove decoherence between reporting prescription search space and the thinning parameters of the best prescription
            // The logging framework doesn't distinguish between parameters used to configure runs and parameters associated with the best
            // solution found, which is accommodated by OptimizeCmdlet asking the heuristic for its parameters and using the parameters from
            // GetParameterCombinations() if the heuristic reports no parameters.
            HeuristicParameters placeholder = new HeuristicParameters();
            return new List<HeuristicParameters>() { placeholder };
        }
    }
}
