using DocumentFormat.OpenXml.Drawing.Diagrams;
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
        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FromAbovePercentageUpperLimit { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FromBelowPercentageUpperLimit { get; set; }

        [Parameter(HelpMessage = "Maximum thinning intensity to evaluate. Paired with minimum intensities rather than used combinatorially.")]
        [ValidateRange(0.0F, 1000.0F)]
        public List<float> Maximum { get; set; }

        [Parameter(HelpMessage = "Minimum thinning intensity to evaluate. Paired with maximum intensities rather than used combinatorially.")]
        [ValidateRange(0.0F, 1000.0F)]
        public List<float> Minimum { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float ProportionalPercentageUpperLimit { get; set; }

        [Parameter(HelpMessage = "Step size, in percent, of above, proportional, and below percentages of first thinning prescription. If present, a second thinning's step size is scaled to account for trees removed in the first thinning.")]
        [ValidateRange(0.0F, 100.0F)]
        public float Step { get; set; }

        [Parameter]
        public PrescriptionUnits Units { get; set; }

        public OptimizePrescription()
        {
            this.SupportsSecondThin = true;

            this.FromAbovePercentageUpperLimit = 100.0F;
            this.FromBelowPercentageUpperLimit = 100.0F;
            this.Maximum = new List<float>() { Constant.PrescriptionEnumerationDefault.MaximumIntensity };
            this.Minimum = new List<float>() { Constant.PrescriptionEnumerationDefault.MinimumIntensity };
            this.PerturbBy = 0.0F;
            this.ProportionalPercentage[0] = 0.0F;
            this.ProportionalPercentageUpperLimit = 100.0F;
            this.Step = Constant.PrescriptionEnumerationDefault.IntensityStep;
            this.Units = Constant.PrescriptionEnumerationDefault.Units;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective, PrescriptionParameters parameters)
        {
            if (this.BestOf != 1)
            {
                throw new NotSupportedException(nameof(this.BestOf)); // enumeration is deterministic, so no value in repeated runs
            }
            return new PrescriptionEnumeration(this.Stand!, organonConfiguration, objective, parameters);
        }

        protected override IHarvest CreateThin(int thinPeriodIndex)
        {
            return new ThinByPrescription(thinPeriodIndex);
        }

        protected override string GetName()
        {
            return "Optimize-Prescription";
        }

        protected override IList<PrescriptionParameters> GetParameterCombinations(TimberValue timberValue)
        {
            if (this.Minimum.Count != this.Maximum.Count)
            {
                throw new ParameterOutOfRangeException(nameof(this.Minimum));
            }
            if (this.PerturbBy != 0.0F)
            {
                throw new NotSupportedException(nameof(this.PerturbBy));
            }
            if ((this.ProportionalPercentage.Count != 1) || (this.ProportionalPercentage[0] != 0.0F))
            {
                throw new NotSupportedException(nameof(this.ProportionalPercentage));
            }
            if (this.Step < 0.0F)
            {
                throw new ParameterOutOfRangeException(nameof(this.Step));
            }

            List<PrescriptionParameters> parameterCombinations = new List<PrescriptionParameters>(this.Minimum.Count);
            for (int intensityIndex = 0; intensityIndex < this.Minimum.Count; ++intensityIndex)
            {
                float minimumIntensity = this.Minimum[intensityIndex];
                float maximumIntensity = this.Maximum[intensityIndex];
                if (maximumIntensity < minimumIntensity)
                {
                    throw new ParameterOutOfRangeException(nameof(this.Minimum));
                }

                parameterCombinations.Add(new PrescriptionParameters()
                {
                    FromAbovePercentageUpperLimit = this.FromAbovePercentageUpperLimit,
                    FromBelowPercentageUpperLimit = this.FromBelowPercentageUpperLimit,
                    Minimum = minimumIntensity,
                    Maximum = maximumIntensity,
                    ProportionalPercentageUpperLimit = this.ProportionalPercentageUpperLimit,
                    StepSize = this.Step,
                    TimberValue = timberValue,
                    Units = this.Units,
                    UseFiaVolume = this.ScaledVolume
                });
            }
            return parameterCombinations;
        }
    }
}
