﻿using Mars.Seem.Heuristics;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Walk")]
    public class OptimizeWalk : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public int? Iterations { get; set; }

        protected override Heuristic<HeuristicParameters> CreateHeuristic(HeuristicParameters heuristicParameters, RunParameters runParameters)
        {
            AutocorrelatedWalk random = new(this.Stand!, heuristicParameters, runParameters);
            if (this.Iterations.HasValue)
            {
                random.Iterations = this.Iterations.Value;
            }
            return random;
        }

        protected override string GetName()
        {
            return "Optimize-Random";
        }

        protected override List<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}