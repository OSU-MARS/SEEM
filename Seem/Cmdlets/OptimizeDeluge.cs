using Osu.Cof.Ferm.Heuristics;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Deluge")]
    public class OptimizeDeluge : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? ChangeToExchangeAfter { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1000.0F)]
        public float? FinalMultiplier { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1000.0F)]
        public float? InitialMultiplier { get; set; }

        [Parameter]
        [ValidateRange(1, 1000 * 1000)]
        public int? Iterations { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? LowerAfter { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public float? LowerBy { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public float? RainRate { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? StopAfter { get; set; }

        public OptimizeDeluge()
        {
            this.ChangeToExchangeAfter = null;
            this.FinalMultiplier = null;
            this.InitialMultiplier = null;
            this.Iterations = null;
            this.LowerAfter = null;
            this.LowerBy = null;
            this.RainRate = null;
            this.StopAfter = null;
        }

        protected override Heuristic<HeuristicParameters> CreateHeuristic(HeuristicParameters heuristicParameters, RunParameters runParameters)
        {
            GreatDeluge deluge = new(this.Stand!, heuristicParameters, runParameters);
            if (this.ChangeToExchangeAfter.HasValue)
            {
                deluge.ChangeToExchangeAfter = this.ChangeToExchangeAfter.Value;
            }
            if (this.FinalMultiplier.HasValue)
            {
                deluge.FinalMultiplier = this.FinalMultiplier.Value * MathF.Abs(deluge.IntitialMultiplier);
            }
            if (this.InitialMultiplier.HasValue)
            {
                deluge.IntitialMultiplier = this.InitialMultiplier.Value;
            }

            if (this.Iterations.HasValue)
            {
                deluge.Iterations = this.Iterations.Value;
            }
            if (this.LowerAfter.HasValue)
            {
                deluge.LowerWaterAfter = this.LowerAfter.Value;
            }
            if (this.LowerBy.HasValue)
            {
                deluge.LowerWaterBy = this.LowerBy.Value;
            }

            if (this.RainRate.HasValue)
            {
                deluge.RainRate = this.RainRate.Value;
            }
            if (this.StopAfter.HasValue)
            {
                deluge.StopAfter = this.StopAfter.Value;
            }
            return deluge;
        }

        protected override string GetName()
        {
            return "Optimize-GreatDeluge";
        }

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}
