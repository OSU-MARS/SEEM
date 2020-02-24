using Osu.Cof.Organon.Heuristics;
using System;
using System.Management.Automation;

namespace Osu.Cof.Organon.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Genetic")]
    public class OptimizeGenetic : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, float.MaxValue)]
        public Nullable<float> EndStandardDeviation { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> MaximumGenerations { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<float> MutationProbability { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> PopulationSize { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<float> ReservedPopulationProportion { get; set; }

        protected override Heuristic CreateHeuristic()
        {
            OrganonConfiguration organonConfiguration = new OrganonConfiguration(OrganonVariant.Create(this.TreeModel));
            GeneticAlgorithm genetic = new GeneticAlgorithm(this.Stand, organonConfiguration, this.HarvestPeriods, this.PlanningPeriods);
            if (this.EndStandardDeviation.HasValue)
            {
                genetic.EndStandardDeviation = this.EndStandardDeviation.Value;
            }
            if (this.MaximumGenerations.HasValue)
            {
                genetic.MaximumGenerations = this.MaximumGenerations.Value;
            }
            if (this.MutationProbability.HasValue)
            {
                genetic.MutationProbability = this.MutationProbability.Value;
            }
            if (this.PopulationSize.HasValue)
            {
                genetic.PopulationSize = this.PopulationSize.Value;
            }
            if (this.ReservedPopulationProportion.HasValue)
            {
                genetic.ReservedPopulationProportion = this.ReservedPopulationProportion.Value;
            }
            return genetic;
        }
    }
}
