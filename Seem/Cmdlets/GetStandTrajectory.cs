using Mars.Seem.Optimization;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandTrajectory")]
    public class GetStandTrajectory : Cmdlet
    {
        [Parameter]
        [ValidateNotNull]
        public FinancialScenarios Financial { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FirstThinAbove { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FirstThinBelow { get; set; }
        [Parameter]
        [ValidateRange(1, 100)]
        public int FirstThinPeriod { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 100.0F)]
        public float FirstThinProportional { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string? Name { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0, 100)]
        public List<int> RotationLengths { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public OrganonStand? Stand { get; set; }

        [Parameter]
        [ValidateNotNull]
        public TreeVolume TreeVolume { get; set; }

        public GetStandTrajectory()
        {
            this.Financial = FinancialScenarios.Default;
            this.Name = null;
            this.RotationLengths = new() { 15 }; // 75 years of simulation with Organon's 5 year timestep
            this.FirstThinAbove = 0.0F; // %
            this.FirstThinBelow = 0.0F; // %
            this.FirstThinProportional = 0.0F; // %
            this.FirstThinPeriod = Constant.NoThinPeriod; // no stand entry
            this.TreeVolume = TreeVolume.Default;
        }

        protected override void ProcessRecord()
        {
            // argument checking and longest rotation length
            if (this.Stand == null)
            {
                throw new ParameterOutOfRangeException(nameof(this.Stand));
            }

            int lastPlanningPeriod = Int32.MinValue;
            for (int rotationIndex = 0; rotationIndex < this.RotationLengths.Count; ++rotationIndex)
            {
                int rotationLength = this.RotationLengths[rotationIndex];
                if (this.FirstThinPeriod >= rotationIndex)
                {
                    throw new ParameterOutOfRangeException(nameof(this.FirstThinPeriod));
                }

                if (rotationLength > lastPlanningPeriod)
                {
                    lastPlanningPeriod = rotationLength;
                }
            }

            if (this.FirstThinPeriod > Constant.NoThinPeriod)
            {
                float thinningIntensity = this.FirstThinAbove + this.FirstThinBelow + this.FirstThinProportional;
                if (thinningIntensity <= 0.0F)
                {
                    throw new ParameterOutOfRangeException(nameof(this.FirstThinPeriod), "-ThinPeriod is specified but -ThinAbove, -ThinBelow, and -ThinProportional add to zero so no thin will be performed.");
                }
            }

            // simulate stand trajectory
            OrganonConfiguration configuration = new(new OrganonVariantNwo());
            OrganonStandTrajectory trajectory = new(this.Stand, configuration, this.TreeVolume, lastPlanningPeriod);
            if (this.Name != null)
            {
                trajectory.Name = this.Name;
            }
            if (this.FirstThinPeriod != Constant.NoThinPeriod)
            {
                trajectory.Treatments.Harvests.Add(new ThinByPrescription(this.FirstThinPeriod)
                {
                    FromAbovePercentage = this.FirstThinAbove,
                    ProportionalPercentage = this.FirstThinProportional,
                    FromBelowPercentage = this.FirstThinBelow
                });
            }

            PrescriptionPerformanceCounters perfCounters = new();
            Stopwatch stopwatch = new();
            stopwatch.Start();
            perfCounters.GrowthModelTimesteps += trajectory.Simulate();
            stopwatch.Stop();
            perfCounters.Duration += stopwatch.Elapsed;

            // assign stand trajectory to coordinates
            StandTrajectories trajectories = new(new List<int>() { this.FirstThinPeriod }, this.RotationLengths, this.Financial);
            for (int rotationIndex = 0; rotationIndex < this.RotationLengths.Count; ++rotationIndex)
            {
                int endOfRotationPeriod = this.RotationLengths[rotationIndex];

                for (int financialIndex = 0; financialIndex < this.Financial.Count; ++financialIndex)
                {
                    // must be new each time to uniquely populate results.CombinationsEvaluated through AddEvaluatedPosition()
                    StandTrajectoryCoordinate currentCoordinate = new()
                    {
                        FinancialIndex = financialIndex,
                        RotationIndex = rotationIndex
                    };

                    float landExpectationValue = this.Financial.GetLandExpectationValue(trajectory, financialIndex, endOfRotationPeriod);
                    trajectories.AssimilateIntoCoordinate(trajectory, landExpectationValue, currentCoordinate, perfCounters);
                    trajectories.AddEvaluatedPosition(currentCoordinate);
                }
            }

            this.WriteObject(trajectories);
        }
    }
}
