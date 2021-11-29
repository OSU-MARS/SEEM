using Mars.Seem.Extensions;
using Mars.Seem.Heuristics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "ThresholdAccepting")]
    public class OptimizeThresholdAccepting : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(1, Int32.MaxValue)]
        public List<int>? IterationsPerThreshold { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0.0F, 1.0F)]
        public List<float>? Thresholds { get; set; }

        public OptimizeThresholdAccepting()
        {
            this.IterationsPerThreshold = null;
            this.Thresholds = null;
        }

        protected override Heuristic<HeuristicParameters> CreateHeuristic(HeuristicParameters heuristicParameters, RunParameters runParameters)
        {
            if ((this.IterationsPerThreshold != null) ^ (this.Thresholds != null))
            {
                throw new ParameterOutOfRangeException(nameof(this.IterationsPerThreshold), nameof(this.IterationsPerThreshold) + " and " + this.Thresholds + " must both be set or both be null.");
            }

            ThresholdAccepting acceptor = new(this.Stand!, heuristicParameters, runParameters);
            if (this.IterationsPerThreshold != null)
            {
                Debug.Assert(this.Thresholds != null);
                if (this.IterationsPerThreshold.Count != this.Thresholds.Count)
                {
                    throw new ParameterOutOfRangeException(nameof(this.IterationsPerThreshold), nameof(this.IterationsPerThreshold) + " and " + this.Thresholds + " must be of the same length.");
                }

                acceptor.IterationsPerThreshold.CopyFrom(this.IterationsPerThreshold);
                acceptor.Thresholds.CopyFrom(this.Thresholds);
            }
            return acceptor;
        }

        protected override string GetName()
        {
            return "Optimize-ThresholdAccepting";
        }

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}
