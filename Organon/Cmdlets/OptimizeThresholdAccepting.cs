using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "ThresholdAccepting")]
    public class OptimizeThresholdAccepting : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> IterationsPerThreshold { get; set; }

        [Parameter]
        public List<float> Thresholds { get; set; }

        public OptimizeThresholdAccepting()
        {
            this.IterationsPerThreshold = null;
            this.Thresholds = null;
        }

        protected override Heuristic CreateHeuristic(Objective objective)
        {
            OrganonConfiguration organonConfiguration = new OrganonConfiguration(OrganonVariant.Create(this.TreeModel));
            ThresholdAccepting acceptor = new ThresholdAccepting(this.Stand, organonConfiguration, this.HarvestPeriods, this.PlanningPeriods, objective);
            if (this.IterationsPerThreshold.HasValue)
            {
                acceptor.IterationsPerThreshold = this.IterationsPerThreshold.Value;
            }
            if (this.Thresholds != null)
            {
                acceptor.Thresholds.Clear();
                acceptor.Thresholds.AddRange(this.Thresholds);
            }
            return acceptor;
        }
    }
}
