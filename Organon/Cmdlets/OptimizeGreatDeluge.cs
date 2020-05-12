using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "GreatDeluge")]
    public class OptimizeGreatDeluge : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> FinalWaterLevel { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> InitialWaterLevel { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> RainRate { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeGreatDeluge()
        {
            this.FinalWaterLevel = null;
            this.RainRate = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, float defaultSelectionProbability)
        {
            GreatDeluge deluge = new GreatDeluge(this.Stand, organonConfiguration, planningPeriods, objective);
            if (this.FinalWaterLevel.HasValue)
            {
                deluge.FinalWaterLevel = this.FinalWaterLevel.Value;
            }
            if (this.InitialWaterLevel.HasValue)
            {
                deluge.InitialWaterLevel = this.InitialWaterLevel.Value;
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
    }
}
