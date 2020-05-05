using Osu.Cof.Ferm.Organon;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandTrajectory")]
    public class GetStandTrajectory : Cmdlet
    {
        [Parameter]
        [ValidateRange(1, 100)]
        public int HarvestPeriods { get; set; }

        [Parameter]
        [ValidateRange(1, 100)]
        public int PlanningPeriods { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand Stand { get; set; }

        [Parameter]
        public int ThinPeriod { get; set; }

        [Parameter]
        public VolumeUnits Units { get; set; }

        public GetStandTrajectory()
        {
            this.HarvestPeriods = 9;
            this.PlanningPeriods = 9;
            this.ThinPeriod = -1; // no stand endtry
            this.Units = VolumeUnits.CubicMetersPerHectare;
        }

        protected override void ProcessRecord()
        {
            OrganonConfiguration configuration = new OrganonConfiguration(new OrganonVariantNwo());
            OrganonStandTrajectory trajectory = new OrganonStandTrajectory(this.Stand, configuration, this.HarvestPeriods, this.PlanningPeriods, this.Units);
            if (this.ThinPeriod > 0)
            {
                configuration.Treatments.AddThin(this.ThinPeriod * trajectory.PeriodLengthInYears);
            }
            trajectory.Simulate();
            this.WriteObject(trajectory);
        }
    }
}
