using Mars.Seem.Heuristics;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Hero")]
    public class OptimizeHero : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public int? MaxIterations { get; set; }

        [Parameter]
        public SwitchParameter Stochastic { get; set; }

        protected override Heuristic<HeuristicParameters> CreateHeuristic(HeuristicParameters heuristicParameters, RunParameters runParameters)
        {
            Hero hero = new(this.Stand!, heuristicParameters, runParameters)
            {
                IsStochastic = this.Stochastic
            };
            if (this.MaxIterations.HasValue)
            {
                hero.MaximumIterations = this.MaxIterations.Value;
            }
            return hero;
        }

        protected override string GetName()
        {
            return "Optimize-Hero";
        }

        protected override List<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}
