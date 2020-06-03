using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Genetic")]
    public class OptimizeGenetic : OptimizeCmdlet<GeneticParameters>
    {
        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public List<float> ExchangeProbability { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public List<float> FlipProbability { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public List<float> GenerationCoefficient { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public List<float> MinCoefficientOfVariation { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public List<int> PopulationSize { get; set; }

        [Parameter]
        [ValidateRange(0.0, 100.0)]
        public List<float> ProportionalPercentageWidth { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public List<float> ReservedProportion { get; set; }

        public OptimizeGenetic()
        {
            this.ExchangeProbability = new List<float>() { Constant.GeneticDefault.ExchangeProbability };
            this.FlipProbability = new List<float>() { Constant.GeneticDefault.ExchangeProbability };
            this.GenerationCoefficient = new List<float>() { Constant.GeneticDefault.MaximumGenerationCoefficient };
            this.MinCoefficientOfVariation = new List<float>() { Constant.GeneticDefault.MinCoefficientOfVariation };
            this.PopulationSize = new List<int>() { Constant.GeneticDefault.PopulationSize };
            this.ProportionalPercentageWidth = new List<float>() { Constant.GeneticDefault.ProportionalPercentageWidth };
            this.ReservedProportion = new List<float>() { Constant.GeneticDefault.ReservedPopulationProportion };
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, GeneticParameters parameters)
        {
            GeneticAlgorithm genetic = new GeneticAlgorithm(this.Stand, organonConfiguration, planningPeriods, objective)
            {
                ExchangeProbability = parameters.ExchangeProbability,
                FlipProbability = parameters.FlipProbability,
                MaximumGenerations = parameters.MaximumGenerations,
                MinCoefficientOfVariation = parameters.MinCoefficientOfVariation,
                PopulationSize = parameters.PopulationSize,
                ProportionalPercentageCenter = parameters.ProportionalPercentage,
                ProportionalPercentageWidth = parameters.ProportionalPercentageWidth,
                ReservedPopulationProportion = parameters.ReservedProportion
            };
            return genetic;
        }

        protected override string GetName()
        {
            return "Optimize-Genetic";
        }

        protected override IList<GeneticParameters> GetParameterCombinations()
        {
            if (this.ExchangeProbability.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ExchangeProbability));
            }
            if (this.FlipProbability.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FlipProbability));
            }
            if (this.GenerationCoefficient.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.GenerationCoefficient));
            }
            if (this.MinCoefficientOfVariation.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MinCoefficientOfVariation));
            }
            if (this.PopulationSize.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.PopulationSize));
            }
            if (this.ProportionalPercentageWidth.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ProportionalPercentageWidth));
            }
            if (this.ReservedProportion.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ReservedProportion));
            }

            List<GeneticParameters> parameters = new List<GeneticParameters>(this.ProportionalPercentage.Count);
            int treeRecordCount = this.Stand.GetTreeRecordCount();
            foreach (float exchangeProbability in this.ExchangeProbability)
            {
                foreach (float flipProbability in this.FlipProbability)
                {
                    foreach (float generationCoefficient in this.GenerationCoefficient)
                    {
                        foreach (float minCoefficientOfVariation in this.MinCoefficientOfVariation)
                        {
                            foreach (int populationSize in this.PopulationSize)
                            {
                                foreach (float proportionalPercentage in this.ProportionalPercentage)
                                {
                                    foreach (float proportionalPercentageWidth in this.ProportionalPercentageWidth)
                                    {
                                        foreach (float reservedProportion in this.ReservedProportion)
                                        {
                                            parameters.Add(new GeneticParameters()
                                            {
                                                ExchangeProbability = exchangeProbability,
                                                FlipProbability = flipProbability,
                                                MaximumGenerations = (int)(generationCoefficient * treeRecordCount + 0.5F),
                                                MinCoefficientOfVariation = minCoefficientOfVariation,
                                                PopulationSize = populationSize,
                                                ProportionalPercentage = proportionalPercentage,
                                                ProportionalPercentageWidth = proportionalPercentageWidth,
                                                ReservedProportion = reservedProportion,
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return parameters;
        }
    }
}
