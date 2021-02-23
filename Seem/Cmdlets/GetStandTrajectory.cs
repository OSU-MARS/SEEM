using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandTrajectory")]
    public class GetStandTrajectory : Cmdlet
    {
        [Parameter]
        public List<float> DiscountRates { get; set; }

        [Parameter]
        public SwitchParameter FiaVolume { get; set; }

        // BUGBUG: currently unused
        [Parameter]
        [ValidateRange(1, 100)]
        public int HarvestPeriods { get; set; }

        [Parameter]
        public string? Name { get; set; }

        [Parameter]
        [ValidateRange(1, 100)]
        public int PlanningPeriods { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float ProportionalThinPercentage { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand? Stand { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float ThinFromAbovePercentage { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float ThinFromBelowPercentage { get; set; }

        [Parameter]
        [ValidateNotNull]
        public TimberValue TimberValue { get; set; }

        [Parameter]
        [ValidateRange(1, 100)]
        public int ThinPeriod { get; set; }

        public GetStandTrajectory()
        {
            this.DiscountRates = new List<float>() { Constant.DefaultAnnualDiscountRate };
            this.FiaVolume = false;
            this.HarvestPeriods = 9;
            this.Name = null;
            this.PlanningPeriods = 20; // 100 years of simulation with Organon's 5 year timestep
            this.ProportionalThinPercentage = 0.0F; // %
            this.ThinFromAbovePercentage = 0.0F; // %
            this.ThinFromBelowPercentage = 0.0F; // %
            this.ThinPeriod = -1; // no stand entry
            this.TimberValue = TimberValue.Default;
        }

        protected override void ProcessRecord()
        {
            if (this.ThinPeriod > this.HarvestPeriods)
            {
                throw new ParameterOutOfRangeException(nameof(this.ThinPeriod));
            }

            OrganonConfiguration configuration = new OrganonConfiguration(new OrganonVariantNwo());
            if (this.ThinPeriod > 0)
            {
                configuration.Treatments.Harvests.Add(new ThinByPrescription(this.ThinPeriod)
                {
                    FromAbovePercentage = this.ThinFromAbovePercentage, 
                    ProportionalPercentage = this.ProportionalThinPercentage, 
                    FromBelowPercentage = this.ThinFromBelowPercentage
                });
            }

            foreach (float discountRate in this.DiscountRates)
            {
                TimberValue timberValue = this.TimberValue;
                if ((discountRate != Constant.DefaultAnnualDiscountRate) && (discountRate != timberValue.DiscountRate))
                {
                    if (timberValue.DiscountRate != Constant.DefaultAnnualDiscountRate)
                    {
                        throw new NotSupportedException("The single, non-default discount rate " + discountRate.ToString("0.00") + " was specified but the discount rate set on TimberValue is the non-default " + this.TimberValue.DiscountRate.ToString("0.00") + ".  Resolve this conflict by not specifying a discount rate, using the same discount rate in both locations, or specifying a default discount rate in the location which should be overridden.");
                    }

                    timberValue = new TimberValue(this.TimberValue)
                    {
                        DiscountRate = discountRate
                    };
                }

                OrganonStandTrajectory trajectory = new OrganonStandTrajectory(this.Stand!, configuration, timberValue, this.PlanningPeriods, this.FiaVolume);
                if (this.Name != null)
                {
                    trajectory.Name = this.Name;
                }

                // performance: if needed, remove unnecessary repetition of stand simulation for each discount rate
                // Only one growth simulation is necessary but StandTrajectory lacks an API to recompute its net present value arrays
                // for a new discount rate.
                trajectory.Simulate();
                this.WriteObject(trajectory);
            }
        }
    }
}
