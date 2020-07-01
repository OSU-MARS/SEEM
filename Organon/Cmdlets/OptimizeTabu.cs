using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Tabu")]
    public class OptimizeTabu : OptimizeCmdlet<TabuParameters>
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public List<int> Iterations { get; set; }

        //[Parameter]
        //[ValidateRange(1, 100)]
        //public Nullable<int> Jump { get; set; }

        [Parameter]
        [ValidateRange(2, Int32.MaxValue)]
        public List<int> MaxTenure { get; set; }

        public OptimizeTabu()
        {
            this.Iterations = null;
            this.MaxTenure = null;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, TabuParameters parameters)
        {
            TabuSearch tabu = new TabuSearch(this.Stand, organonConfiguration, planningPeriods, objective)
            {
                Iterations = parameters.Iterations,
                // Jump = parameters.Jump,
                MaximumTenure = parameters.MaximumTenure
            };
            return tabu;
        }

        protected override string GetName()
        {
            return "Optimize-Tabu";
        }

        protected override IList<TabuParameters> GetParameterCombinations()
        {
            int treeCount = this.Stand.GetTreeRecordCount();
            if (this.Iterations == null)
            {
                this.Iterations = new List<int>() { treeCount };
            }
            if (this.MaxTenure == null)
            {
                this.MaxTenure = new List<int>() { (int)(Constant.TabuDefault.MaximumTenureRatio * treeCount) };
            }

            List<TabuParameters> parameters = new List<TabuParameters>(this.Iterations.Count * this.MaxTenure.Count * this.ProportionalPercentage.Count);
            foreach (int iterations in this.Iterations)
            {
                foreach (int tenure in this.MaxTenure)
                {
                    foreach (float proportionalPercentage in this.ProportionalPercentage)
                    {
                        parameters.Add(new TabuParameters()
                        {
                            Iterations = iterations,
                            PerturbBy = this.PerturbBy,
                            ProportionalPercentage = proportionalPercentage,
                            MaximumTenure = tenure
                        });
                    }
                }
            }
            return parameters;
        }
    }
}
