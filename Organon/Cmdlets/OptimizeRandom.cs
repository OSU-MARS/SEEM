using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Random")]
    public class OptimizeRandom : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Iterations { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<float> SelectionProbabilityWidth { get; set; }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, float defaultSelectionProbability)
        {
            RandomGuessing random = new RandomGuessing(this.Stand, organonConfiguration, planningPeriods, objective, defaultSelectionProbability);
            if (this.Iterations.HasValue)
            {
                random.Iterations = this.Iterations.Value;
            }
            if (this.SelectionProbabilityWidth.HasValue)
            {
                random.SelectionProbabilityWidth = this.SelectionProbabilityWidth.Value;
            }
            return random;
        }

        protected override string GetName()
        {
            return "Optimize-Random";
        }
    }
}
