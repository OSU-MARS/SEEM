using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "SimulatedAnnealing")]
    public class OptimizeSimulatedAnnealing : OptimizeCmdlet<HeuristicParameters>
    {
        [Parameter]
        [ValidateRange(0.0F, 1.0F)]
        public float? Alpha { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? ChangeToExchangeAfter { get; set; }

        [Parameter]
        [ValidateRange(0.0F, Single.MaxValue)]
        public float? FinalProbability { get; set; }
        
        [Parameter]
        [ValidateRange(0.0F, Single.MaxValue)]
        public float? InitialProbability { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? Iterations { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? IterationsPerTemperature { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? ProbabilityWindowLength { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? ReheatAfter { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 1.0F)]
        public float? ReheatBy { get; set; }

        public OptimizeSimulatedAnnealing()
        {
            this.Alpha = null;
            this.ChangeToExchangeAfter = null;
            this.FinalProbability = null;
            this.InitialProbability = null;
            this.Iterations = null;
            this.IterationsPerTemperature = null;
            this.ProbabilityWindowLength = null;
            this.ReheatAfter = null;
            this.ReheatBy = null;
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
        {
            SimulatedAnnealing annealer = new SimulatedAnnealing(this.Stand!, organonConfiguration, objective, parameters);
            if (this.Alpha.HasValue)
            {
                annealer.Alpha = this.Alpha.Value;
            }
            if (this.ChangeToExchangeAfter.HasValue)
            {
                annealer.ChangeToExchangeAfter = this.ChangeToExchangeAfter.Value;
            }
            if (this.FinalProbability.HasValue)
            {
                annealer.FinalProbability = this.FinalProbability.Value;
            }
            if (this.InitialProbability.HasValue)
            {
                annealer.InitialProbability = this.InitialProbability.Value;
            }
            if (this.Iterations.HasValue)
            {
                annealer.Iterations = this.Iterations.Value;
            }
            if (this.IterationsPerTemperature.HasValue)
            {
                annealer.IterationsPerTemperature = this.IterationsPerTemperature.Value;
            }
            if (this.ProbabilityWindowLength.HasValue)
            {
                annealer.ProbabilityWindowLength = this.ProbabilityWindowLength.Value;
            }
            if (this.ReheatAfter.HasValue)
            {
                annealer.ReheatAfter = this.ReheatAfter.Value;
            }
            if (this.ReheatBy.HasValue)
            {
                annealer.ReheatBy = this.ReheatBy.Value;
            }
            return annealer;
        }

        protected override string GetName()
        {
            return "Optimize-SimulatedAnnealing";
        }

        protected override IList<HeuristicParameters> GetParameterCombinations()
        {
            return this.GetDefaultParameterCombinations();
        }
    }
}
