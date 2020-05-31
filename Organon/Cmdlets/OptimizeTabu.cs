using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Tabu")]
    public class OptimizeTabu : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Iterations { get; set; }

        //[Parameter]
        //[ValidateRange(1, 100)]
        //public Nullable<int> Jump { get; set; }

        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Tenure { get; set; }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, HeuristicParameters _)
        {
            TabuSearch tabu = new TabuSearch(this.Stand, organonConfiguration, planningPeriods, objective);
            if (this.Iterations.HasValue)
            {
                tabu.Iterations = this.Iterations.Value;
            }
            //if (this.Jump.HasValue)
            //{
            //    tabu.Jump = this.Jump.Value;
            //}
            if (this.Tenure.HasValue)
            {
                tabu.Tenure = this.Tenure.Value;
            }
            return tabu;
        }

        protected override string GetName()
        {
            return "Optimize-Tabu";
        }

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            return this.ProportionalPercentagesAsHeuristicParameters();
        }
    }
}
