using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Hero")]
    public class OptimizeHero : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Iterations { get; set; }

        [Parameter]
        public SwitchParameter Stochastic { get; set; }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, float defaultSelectionProbability)
        {
            Hero hero = new Hero(this.Stand, organonConfiguration, planningPeriods, objective, this.Stochastic);
            if (this.Iterations.HasValue)
            {
                hero.Iterations = this.Iterations.Value;
            }
            return hero;
        }

        protected override string GetName()
        {
            return "Optimize-Hero";
        }
    }
}
