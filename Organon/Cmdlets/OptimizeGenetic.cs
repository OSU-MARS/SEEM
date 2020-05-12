using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Genetic")]
    public class OptimizeGenetic : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public Nullable<float> EndStandardDeviation { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<float> ExchangeProbability { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<float> FlipProbability { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> MaximumGenerations { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> PopulationSize { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<float> ReservedPopulationProportion { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<float> SelectionProbabilityWidth { get; set; }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, float defaultSelectionProbability)
        {
            GeneticAlgorithm genetic = new GeneticAlgorithm(this.Stand, organonConfiguration, planningPeriods, objective)
            {
                CentralSelectionProbability = defaultSelectionProbability
            };

            if (this.EndStandardDeviation.HasValue)
            {
                genetic.EndStandardDeviation = this.EndStandardDeviation.Value;
            }
            if (this.ExchangeProbability.HasValue)
            {
                genetic.ExchangeProbability = this.ExchangeProbability.Value;
            }
            if (this.FlipProbability.HasValue)
            {
                genetic.FlipProbability = this.FlipProbability.Value;
            }
            if (this.SelectionProbabilityWidth.HasValue)
            {
                genetic.SelectionProbabilityWidth = this.SelectionProbabilityWidth.Value;
            }
            if (this.MaximumGenerations.HasValue)
            {
                genetic.MaximumGenerations = this.MaximumGenerations.Value;
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

        protected override string GetName()
        {
            return "Optimize-Genetic";
        }
    }
}
