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
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> FixedDeviation { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> FixedIncrease { get; set; }

        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> IncreaseAfter { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> Iterations { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> RelativeDeviation { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> RelativeIncrease { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeRecordTravel()
        {
            this.FixedDeviation = null;
            this.FixedIncrease = null;
            this.IncreaseAfter = null;
            this.Iterations = null;
            this.RelativeDeviation = null;
            this.RelativeIncrease = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, HeuristicParameters _)
        {
            RecordTravel recordTravel = new RecordTravel(this.Stand, organonConfiguration, planningPeriods, objective);
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
            return this.ProportionalPercentagesAsHeuristicParameters();
        }
    }
}
