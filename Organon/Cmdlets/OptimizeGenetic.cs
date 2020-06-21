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
        public List<float> CrossoverProbabilityEnd { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public List<float> ExchangeProbabilityEnd { get; set; }

        [Parameter]
        [ValidateRange(-20.0, 0.0)]
        public List<float> ExponentK { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public List<float> FlipProbabilityEnd { get; set; }

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
            this.CrossoverProbabilityEnd = new List<float>() { Constant.GeneticDefault.EndCrossoverProbability };
            this.ExchangeProbabilityEnd = new List<float>() { Constant.GeneticDefault.ExchangeProbabilityEnd };
            this.ExponentK = new List<float>() { Constant.GeneticDefault.ExponentK };
            this.FlipProbabilityEnd = new List<float>() { Constant.GeneticDefault.FlipProbabilityEnd };
            this.GenerationCoefficient = new List<float>() { Constant.GeneticDefault.MaximumGenerationCoefficient };
            this.MinCoefficientOfVariation = new List<float>() { Constant.GeneticDefault.MinimumCoefficientOfVariation };
            this.PopulationSize = new List<int>() { Constant.GeneticDefault.PopulationSize };
            this.ProportionalPercentageWidth = new List<float>() { Constant.GeneticDefault.ProportionalPercentageWidth };
            this.ReservedProportion = new List<float>() { Constant.GeneticDefault.ReservedPopulationProportion };
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective, GeneticParameters parameters)
        {
            GeneticAlgorithm genetic = new GeneticAlgorithm(this.Stand, organonConfiguration, planningPeriods, objective)
            {
                ChainFrom = parameters.ChainFrom,
                CrossoverProbabilityEnd = parameters.CrossoverProbabilityEnd,
                ExchangeProbabilityEnd = parameters.ExchangeProbabilityEnd,
                ExchangeProbabilityStart = parameters.ExchangeProbabilityStart,
                ExponentK = parameters.ExponentK,
                FlipProbabilityEnd = parameters.FlipProbabilityEnd,
                FlipProbabilityStart = parameters.FlipProbabilityStart,
                MaximumGenerations = parameters.MaximumGenerations,
                MinimumCoefficientOfVariation = parameters.MinimumCoefficientOfVariation,
                PopulationSize = parameters.PopulationSize,
                ProportionalPercentageCenter = parameters.ProportionalPercentage,
                ProportionalPercentageWidth = parameters.ProportionalPercentageWidth,
                ReservedPopulationProportion = parameters.ReservedProportion,
            };
            return genetic;
        }

        protected override string GetName()
        {
            return "Optimize-Genetic";
        }

        protected override IList<GeneticParameters> GetParameterCombinations()
        {
            if (this.ChainFrom < Constant.HeuristicDefault.ChainFrom)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ChainFrom));
            }
            if (this.ExchangeProbabilityEnd.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ExchangeProbabilityEnd));
            }
            if (this.ExponentK.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ExponentK));
            }
            if (this.FlipProbabilityEnd.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FlipProbabilityEnd));
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
            foreach (float crossoverProbabilityEnd in this.CrossoverProbabilityEnd)
            {
                foreach (float exponent in this.ExponentK)
                {
                    foreach (float exchangeProbabilityEnd in this.ExchangeProbabilityEnd)
                    {
                        foreach (float flipProbabilityEnd in this.FlipProbabilityEnd)
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
                                                        ChainFrom = this.ChainFrom ?? Constant.HeuristicDefault.ChainFrom,
                                                        CrossoverProbabilityEnd = crossoverProbabilityEnd,
                                                        ExchangeProbabilityEnd = exchangeProbabilityEnd,
                                                        ExchangeProbabilityStart = Constant.GeneticDefault.ExchangeProbabilityStart,
                                                        ExponentK = exponent,
                                                        FlipProbabilityEnd = flipProbabilityEnd,
                                                        FlipProbabilityStart = Constant.GeneticDefault.FlipProbabilityStart,
                                                        MaximumGenerations = (int)(generationCoefficient * treeRecordCount + 0.5F),
                                                        MinimumCoefficientOfVariation = minCoefficientOfVariation,
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
                }
            }
            return parameters;
        }
    }
}
