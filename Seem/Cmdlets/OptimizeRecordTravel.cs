using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "RecordTravel")]
    public class OptimizeRecordTravel : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(0.0F, 1.0F)]
        public float? Alpha { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? ChangeToExchangeAfter { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public float? FixedDeviation { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public float? FixedIncrease { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? IncreaseAfter { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? Iterations { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public float? RelativeDeviation { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public float? RelativeIncrease { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? StopAfter { get; set; }

        public OptimizeRecordTravel()
        {
            this.ChangeToExchangeAfter = null;
            this.FixedDeviation = null;
            this.FixedIncrease = null;
            this.IncreaseAfter = null;
            this.Iterations = null;
            this.RelativeDeviation = null;
            this.RelativeIncrease = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
        {
            RecordTravel recordTravel = new RecordTravel(this.Stand!, organonConfiguration, objective, parameters);
            if (this.Alpha.HasValue)
            {
                recordTravel.Alpha = this.Alpha.Value;
            }
            if (this.ChangeToExchangeAfter.HasValue)
            {
                recordTravel.ChangeToExchangeAfter = this.ChangeToExchangeAfter.Value;
            }
            if (this.FixedDeviation.HasValue)
            {
                recordTravel.FixedDeviation = this.FixedDeviation.Value;
            }
            if (this.FixedIncrease.HasValue)
            {
                recordTravel.FixedIncrease = this.FixedIncrease.Value;
            }
            if (this.IncreaseAfter.HasValue)
            {
                recordTravel.IncreaseAfter = this.IncreaseAfter.Value;
            }
            if (this.Iterations.HasValue)
            {
                recordTravel.Iterations = this.Iterations.Value;
            }
            if (this.RelativeDeviation.HasValue)
            {
                recordTravel.RelativeDeviation = this.RelativeDeviation.Value;
            }
            if (this.RelativeIncrease.HasValue)
            {
                recordTravel.RelativeIncrease = this.RelativeIncrease.Value;
            }
            if (this.StopAfter.HasValue)
            {
                recordTravel.StopAfter = this.StopAfter.Value;
            }
            return recordTravel;
        }

        protected override string GetName()
        {
            return "Optimize-RecordTravel";
        }

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}
