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
        [Parameter(HelpMessage = "Step size, in percent, of above, proportional, and below percentages of first thinning prescription. If present, a second or third thinning's step size is scaled to account for trees removed in the first thinning.")]
        [ValidateRange(0.0F, 100.0F)]
        public float DefaultStep { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FromAbovePercentageUpperLimit { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FromBelowPercentageUpperLimit { get; set; }

        [Parameter(HelpMessage = "Maximum thinning intensity to evaluate. Paired with minimum intensities rather than used combinatorially.")]
        [ValidateRange(0.0F, 1000.0F)]
        public List<float> MaximumIntensity { get; set; }

        [Parameter(HelpMessage = "Maximum step size, in percent, of above, proportional, and below percentages.")]
        [ValidateRange(0.0F, 100.0F)]
        public float MaximumStep { get; set; }

        [Parameter(HelpMessage = "Minimum thinning intensity to evaluate. Paired with maximum intensities rather than used combinatorially.")]
        [ValidateRange(0.0F, 1000.0F)]
        public List<float> MinimumIntensity { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float ProportionalPercentageUpperLimit { get; set; }

        [Parameter]
        public PrescriptionUnits Units { get; set; }

        public OptimizePrescription()
        {
            this.DefaultStep = Constant.PrescriptionEnumerationDefault.DefaultIntensityStepSize;
            this.FromAbovePercentageUpperLimit = 100.0F;
            this.FromBelowPercentageUpperLimit = 100.0F;
            this.MaximumIntensity = new List<float>() { Constant.PrescriptionEnumerationDefault.MaximumIntensity };
            this.MinimumIntensity = new List<float>() { Constant.PrescriptionEnumerationDefault.MinimumIntensity };
            this.ConstructionRandomness = 0.0F;
            this.InitialThinningProbability[0] = 0.0F;
            this.ProportionalPercentageUpperLimit = 100.0F;
            this.MaximumStep = Constant.PrescriptionEnumerationDefault.MaximumIntensityStepSize;
            this.Units = Constant.PrescriptionEnumerationDefault.Units;
        }

        protected override Heuristic<PrescriptionParameters> CreateHeuristic(OrganonConfiguration organonConfiguration, PrescriptionParameters heuristicParameters, RunParameters runParameters)
        {
            if (this.BestOf != 1)
            {
                throw new NotSupportedException(nameof(this.BestOf)); // enumeration is deterministic, so no value in repeated runs
            }
            return new PrescriptionEnumeration(this.Stand!, organonConfiguration, runParameters, heuristicParameters);
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
            if (this.MinimumIntensity.Count != this.MaximumIntensity.Count)
            {
                throw new ParameterOutOfRangeException(nameof(this.MinimumIntensity));
            }
            if (this.ConstructionRandomness != 0.0F)
            {
                throw new NotSupportedException(nameof(this.ConstructionRandomness));
            }
            if ((this.InitialThinningProbability.Count != 1) || (this.InitialThinningProbability[0] != 0.0F))
            {
                throw new NotSupportedException(nameof(this.InitialThinningProbability));
            }
            if (this.DefaultStep < 0.0F)
            {
                throw new ParameterOutOfRangeException(nameof(this.DefaultStep));
            }

            List<PrescriptionParameters> parameterCombinations = new(this.MinimumIntensity.Count);
            for (int intensityIndex = 0; intensityIndex < this.MinimumIntensity.Count; ++intensityIndex)
            {
                float minimumIntensity = this.MinimumIntensity[intensityIndex];
                float maximumIntensity = this.MaximumIntensity[intensityIndex];
                if (maximumIntensity < minimumIntensity)
                {
                    throw new ParameterOutOfRangeException(nameof(this.MinimumIntensity));
                }

                parameterCombinations.Add(new PrescriptionParameters()
                {
                    DefaultIntensityStepSize = this.DefaultStep,
                    FromAbovePercentageUpperLimit = this.FromAbovePercentageUpperLimit,
                    FromBelowPercentageUpperLimit = this.FromBelowPercentageUpperLimit,
                    MinimumIntensity = minimumIntensity,
                    MaximumIntensity = maximumIntensity,
                    MaximumIntensityStepSize = this.MaximumStep,
                    ProportionalPercentageUpperLimit = this.ProportionalPercentageUpperLimit,
                    TimberValue = timberValue,
                    Units = this.Units,
                });
            }
            return parameterCombinations;
        }
    }
}
