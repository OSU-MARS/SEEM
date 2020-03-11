using Osu.Cof.Organon.Heuristics;
using System;
using System.Management.Automation;

namespace Osu.Cof.Organon.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "RecordTravel")]
    public class OptimizeRecordTravel : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, float.MaxValue)]
        public Nullable<float> Deviation { get; set; }
        
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeRecordTravel()
        {
            this.Deviation = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic()
        {
            OrganonConfiguration organonConfiguration = new OrganonConfiguration(OrganonVariant.Create(this.TreeModel));
            RecordToRecordTravel recordTravel = new RecordToRecordTravel(this.Stand, organonConfiguration, this.HarvestPeriods, this.PlanningPeriods, this.VolumeUnits);
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
    }
}
