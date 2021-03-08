using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Hero")]
    public class OptimizeHero : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public int? MaxIterations { get; set; }

        [Parameter]
        public SwitchParameter Stochastic { get; set; }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
        {
            Hero hero = new(this.Stand!, organonConfiguration, objective, parameters)
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

        protected override IList<HeuristicParameters> GetParameterCombinations(TimberValue timberValue)
        {
            return this.GetDefaultParameterCombinations(timberValue);
        }
    }
}
