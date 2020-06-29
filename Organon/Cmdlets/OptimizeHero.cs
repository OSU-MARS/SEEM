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
        public Nullable<int> MaxIterations { get; set; }

        [Parameter]
        public SwitchParameter Stochastic { get; set; }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, HeuristicParameters parameters)
        {
            Hero hero = new Hero(this.Stand, organonConfiguration, planningPeriods, objective)
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

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}
