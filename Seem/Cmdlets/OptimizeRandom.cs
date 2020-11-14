using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Random")]
    public class OptimizeRandom : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public int? Iterations { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public float? SelectionProbabilityWidth { get; set; }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
        {
            RandomGuessing random = new RandomGuessing(this.Stand!, organonConfiguration, objective, parameters);
            if (this.Iterations.HasValue)
            {
                random.Iterations = this.Iterations.Value;
            }
            if (this.SelectionProbabilityWidth.HasValue)
            {
                random.SelectionPercentageWidth = this.SelectionProbabilityWidth.Value;
            }
            return random;
        }

        protected override string GetName()
        {
            return "Optimize-Random";
        }

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}