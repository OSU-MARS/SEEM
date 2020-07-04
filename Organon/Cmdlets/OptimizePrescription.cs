using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Prescription")]
    public class OptimizePrescription : OptimizeCmdlet<PrescriptionParameters>
    {
        [Parameter(HelpMessage = "Maximum thinning intensity to evaluate. Paired with minimum intensities rather than used combinatorially.")]
        [ValidateRange(0.0F, 100.0F)]
        public List<float> MaximumIntensity { get; set; }

        [Parameter(HelpMessage = "Minimum thinning intensity to evaluate. Paired with maximum intensities rather than used combinatorially.")]
        [ValidateRange(0.0F, 100.0F)]
        public List<float> MinimumIntensity { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float Step { get; set; }

        public OptimizePrescription()
        {
            this.MaximumIntensity = new List<float>() { Constant.PrescriptionEnumerationDefault.MaximumIntensity };
            this.MinimumIntensity = new List<float>() { Constant.PrescriptionEnumerationDefault.MinimumIntensity };
            this.PerturbBy = 0.0F;
            this.ProportionalPercentage[0] = 0.0F;
            this.Step = Constant.PrescriptionEnumerationDefault.IntensityStep;
        }

        protected override IHarvest CreateHarvest(int harvestPeriodIndex)
        {
            return new ThinByPrescription(this.HarvestPeriods[harvestPeriodIndex]);
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, PrescriptionParameters parameters)
        {
            PrescriptionEnumeration enumerator = new PrescriptionEnumeration(this.Stand, organonConfiguration, planningPeriods, objective);
            enumerator.Parameters.IntensityStep = parameters.IntensityStep;
            enumerator.Parameters.MaximumIntensity = parameters.MaximumIntensity;
            enumerator.Parameters.MinimumIntensity = parameters.MinimumIntensity;
            return enumerator;
        }

        protected override string GetName()
        {
            return "Optimize-Prescription";
        }

        protected override IList<PrescriptionParameters> GetParameterCombinations()
        {
            if (this.MinimumIntensity.Count != this.MaximumIntensity.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (this.PerturbBy != 0.0F)
            {
                throw new NotSupportedException();
            }
            if ((this.ProportionalPercentage.Count != 1) || (this.ProportionalPercentage[0] != 0.0F))
            {
                throw new NotSupportedException();
            }
            if (this.Step < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Step));
            }

            List<PrescriptionParameters> parameters = new List<PrescriptionParameters>(this.MinimumIntensity.Count);
            for (int intensityIndex = 0; intensityIndex < this.MinimumIntensity.Count; ++intensityIndex)
            {
                float minimumIntensity = this.MinimumIntensity[intensityIndex];
                float maximumIntensity = this.MaximumIntensity[intensityIndex];
                if (maximumIntensity < minimumIntensity)
                {
                    throw new ArgumentOutOfRangeException();
                }

                parameters.Add(new PrescriptionParameters()
                {
                    MinimumIntensity = minimumIntensity,
                    MaximumIntensity = maximumIntensity,
                    IntensityStep = this.Step
                });
            }
            return parameters;
        }
    }
}
