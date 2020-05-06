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

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective)
        {
            Hero hero = new Hero(this.Stand, organonConfiguration, this.PlanningPeriods, objective);
            if (this.Iterations.HasValue)
            {
                hero.Iterations = this.Iterations.Value;
            }
            return hero;
        }
    }
}
