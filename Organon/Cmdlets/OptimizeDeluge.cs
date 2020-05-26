using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Deluge")]
    public class OptimizeDeluge : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, 1000.0F)]
        public Nullable<float> FinalMultiplier { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1000.0F)]
        public Nullable<float> InitialMultiplier { get; set; }

        [Parameter]
        [ValidateRange(1, 1000 * 1000)]
        public Nullable<int> Iterations { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> LowerAfter { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<float> LowerBy { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> RainRate { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeDeluge()
        {
            this.FinalMultiplier = null;
            this.InitialMultiplier = null;
            this.Iterations = null;
            this.LowerAfter = null;
            this.LowerBy = null;
            this.RainRate = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, float defaultSelectionProbability)
        {
            GreatDeluge deluge = new GreatDeluge(this.Stand, organonConfiguration, planningPeriods, objective);
            if (this.ChainFrom.HasValue)
            {
                deluge.ChainFrom = this.ChainFrom.Value;
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
            else if (this.InitialMultiplier.HasValue || this.FinalMultiplier.HasValue)
            {
                deluge.RainRate = (deluge.FinalMultiplier - deluge.IntitialMultiplier) * deluge.BestObjectiveFunction / deluge.Iterations;
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
    }
}
