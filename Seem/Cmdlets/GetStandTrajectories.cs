using Mars.Seem.Data;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandTrajectories")]
    public class GetStandTrajectories : GetTrajectoryCmdlet
    {
        [Parameter(HelpMessage = "Time range, in years, to predict stands over.")]
        [ValidateRange(1, 500)]
        public int PredictionInterval { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "List of stands to predict.")]
        [ValidateNotNull]
        public CruisedStands? Stands { get; set; }

        public GetStandTrajectories()
        {
            this.PredictionInterval = 100;
            this.Stands = null;
        }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Stands != null);
            if (this.Stands is not CruisedStands cruisedStands)
            {
                throw new NotSupportedException("Unable to determine growth model to use in predicting stands from type " + this.Stands.GetType().Name + ".");
            }

            OrganonConfiguration organonConfiguration = new(cruisedStands.OrganonVariant);
            int organonTimesteps = this.PredictionInterval / cruisedStands.OrganonVariant.TimeStepInYears;

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = this.Threads
            };

            List<StandTrajectory> standTrajectories = new(this.Stands.Count);
            Parallel.For(0, cruisedStands.Stands.Count, parallelOptions, (int standIndex) =>
            {
                OrganonStand stand = (OrganonStand)cruisedStands.Stands[standIndex];
                OrganonStandTrajectory trajectory = new(stand, organonConfiguration, this.TreeVolume, organonTimesteps);
                trajectory.Simulate();

                lock (standTrajectories)
                {
                    standTrajectories.Add(trajectory);
                }
            });

            this.WriteObject(standTrajectories);
        }
    }
}
