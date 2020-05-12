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
        public Nullable<float> Deviation { get; set; }
        
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeRecordTravel()
        {
            this.Deviation = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, float defaultSelectionProbability)
        {
            RecordToRecordTravel recordTravel = new RecordToRecordTravel(this.Stand, organonConfiguration, planningPeriods, objective);
            if (this.Deviation.HasValue)
            {
                recordTravel.Deviation = this.Deviation.Value;
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
