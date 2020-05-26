using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "RecordTravel")]
    public class OptimizeRecordTravel : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> FixedDeviation { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> RelativeDeviation { get; set; }
        
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeRecordTravel()
        {
            this.FixedDeviation = null;
            this.RelativeDeviation = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, float defaultSelectionProbability)
        {
            RecordTravel recordTravel = new RecordTravel(this.Stand, organonConfiguration, planningPeriods, objective);
            if (this.FixedDeviation.HasValue)
            {
                recordTravel.FixedDeviation = this.FixedDeviation.Value;
            }
            if (this.RelativeDeviation.HasValue)
            {
                recordTravel.RelativeDeviation = this.RelativeDeviation.Value;
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
    }
}
