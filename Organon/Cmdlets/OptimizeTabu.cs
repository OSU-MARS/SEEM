using Osu.Cof.Organon.Heuristics;
using System;
using System.Management.Automation;

namespace Osu.Cof.Organon.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Tabu")]
    public class OptimizeTabu : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Iterations { get; set; }

        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Tenure { get; set; }

        protected override Heuristic CreateHeuristic()
        {
            OrganonConfiguration organonConfiguration = new OrganonConfiguration(OrganonVariant.Create(this.TreeModel));
            TabuSearch tabu = new TabuSearch(this.Stand, organonConfiguration, this.HarvestPeriods, this.PlanningPeriods);
            if (this.Iterations.HasValue)
            {
                tabu.Iterations = this.Iterations.Value;
            }
            if (this.Tenure.HasValue)
            {
                tabu.Tenure = this.Tenure.Value;
            }
            return tabu;
        }
    }
}
