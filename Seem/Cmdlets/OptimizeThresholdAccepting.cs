using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "ThresholdAccepting")]
    public class OptimizeThresholdAccepting : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public List<int>? IterationsPerThreshold { get; set; }

        [Parameter]
        public List<float>? Thresholds { get; set; }

        public OptimizeThresholdAccepting()
        {
            this.IterationsPerThreshold = null;
            this.Thresholds = null;
        }

        protected override Heuristic<HeuristicParameters> CreateHeuristic(OrganonConfiguration organonConfiguration, HeuristicParameters heuristicParameters, RunParameters runParameters)
        {
            ThresholdAccepting acceptor = new(this.Stand!, organonConfiguration, heuristicParameters, runParameters);
            if (this.IterationsPerThreshold != null)
            {
                acceptor.IterationsPerThreshold.Clear();
                acceptor.IterationsPerThreshold.AddRange(this.IterationsPerThreshold);
            }
            if (this.Thresholds != null)
            {
                acceptor.Thresholds.Clear();
                acceptor.Thresholds.AddRange(this.Thresholds);
            }
            return acceptor;
        }

        protected override string GetName()
        {
            return "Optimize-ThresholdAccepting";
        }

        protected override IList<HeuristicParameters> GetParameterCombinations(TimberValue timberValue)
        {
            return this.GetDefaultParameterCombinations(timberValue);
        }
    }
}
