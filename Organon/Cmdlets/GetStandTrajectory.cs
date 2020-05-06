using Osu.Cof.Ferm.Organon;
using System;
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
        public string Name { get; set; }

        [Parameter]
        [ValidateRange(1, 100)]
        public int PlanningPeriods { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float ProportionalThinIntensity { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand Stand { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float ThinFromBelowIntensity { get; set; }

        [Parameter]
        [ValidateRange(1, 100)]
        public int ThinPeriod { get; set; }

        [Parameter]
        public VolumeUnits Units { get; set; }

        public GetStandTrajectory()
        {
            this.HarvestPeriods = 9;
            this.Name = null;
            this.PlanningPeriods = 9;
            this.ProportionalThinIntensity = 5.0F; // %
            this.ThinFromBelowIntensity = 25.0F; // %
            this.ThinPeriod = -1; // no stand entry
            this.Units = VolumeUnits.CubicMetersPerHectare;
        }

        protected override void ProcessRecord()
        {
            if (this.ThinPeriod > this.HarvestPeriods)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ThinPeriod));
            }

            OrganonConfiguration configuration = new OrganonConfiguration(new OrganonVariantNwo());
            if (this.ThinPeriod > 0)
            {
                configuration.Treatments.Harvests.Add(new ThinByPrescription(this.ThinPeriod, this.ThinFromBelowIntensity, this.ProportionalThinIntensity));
            }

            OrganonStandTrajectory trajectory = new OrganonStandTrajectory(this.Stand, configuration, this.PlanningPeriods, this.Units);
            if (this.Name != null)
            {
                trajectory.Name = this.Name;
            }

            trajectory.Simulate();
            this.WriteObject(trajectory);
        }
    }
}
