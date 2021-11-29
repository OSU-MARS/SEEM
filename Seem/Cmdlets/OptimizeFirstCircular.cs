using Mars.Seem.Heuristics;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "FirstCircular")]
    public class OptimizeFirstCircular : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public int? MaxIterations { get; set; }

        [Parameter]
        public SwitchParameter Stochastic { get; set; }

        protected override Heuristic<HeuristicParameters> CreateHeuristic(HeuristicParameters heuristicParameters, RunParameters runParameters)
        {
            FirstImprovingCircularSearch firstCircular = new(this.Stand!, heuristicParameters, runParameters)
            {
                IsStochastic = this.Stochastic
            };
            if (this.MaxIterations.HasValue)
            {
                firstCircular.MaximumIterations = this.MaxIterations.Value;
            }
            return firstCircular;
        }

        protected override string GetName()
        {
            return "Optimize-FirstCircular";
        }

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}
