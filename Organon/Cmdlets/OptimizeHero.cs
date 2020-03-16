using Osu.Cof.Organon.Heuristics;
using System;
using System.Management.Automation;

namespace Osu.Cof.Organon.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Hero")]
    public class OptimizeHero : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Iterations { get; set; }

        protected override Heuristic CreateHeuristic(Objective objective)
        {
            OrganonConfiguration organonConfiguration = new OrganonConfiguration(OrganonVariant.Create(this.TreeModel));
            Hero hero = new Hero(this.Stand, organonConfiguration, this.HarvestPeriods, this.PlanningPeriods, objective);
            if (this.Iterations.HasValue)
            {
                hero.Iterations = this.Iterations.Value;
            }
            return hero;
        }
    }
}
