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
        [ValidateRange(0.0, 10.0)]
        public List<float> FlipProbabilityEnd { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public List<float> GenerationMultiplier { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public List<int> InitializationClasses { get; set; }

        [Parameter]
        public List<PopulationInitializationMethod> InitializationMethod { get; set; }

        [Parameter]
        [ValidateRange(0.0, Single.MaxValue)]
        public List<float> MinCoefficientOfVariation { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public List<int> PopulationSize { get; set; }

        [Parameter]
        public PopulationReplacementStrategy ReplacementStrategy { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public List<float> ReservedProportion { get; set; }

        public OptimizeGenetic()
        {
            this.CrossoverProbabilityEnd = new List<float>() { Constant.GeneticDefault.CrossoverProbabilityEnd };
            this.ExchangeProbabilityEnd = new List<float>() { Constant.GeneticDefault.ExchangeProbabilityEnd };
            this.ExponentK = new List<float>() { Constant.GeneticDefault.ExponentK };
            this.FlipProbabilityEnd = new List<float>() { Constant.GeneticDefault.FlipProbabilityEnd };
            this.GenerationMultiplier = new List<float>() { Constant.GeneticDefault.GenerationMultiplier };
            this.InitializationClasses = new List<int>() { Constant.GeneticDefault.InitializationClasses };
            this.InitializationMethod = new List<PopulationInitializationMethod> { Constant.GeneticDefault.InitializationMethod };
            this.MinCoefficientOfVariation = new List<float>() { Constant.GeneticDefault.MinimumCoefficientOfVariation };
            this.PopulationSize = new List<int>() { Constant.GeneticDefault.PopulationSize };
            this.ReplacementStrategy = Constant.GeneticDefault.ReplacementStrategy;
            this.ReservedProportion = new List<float>() { Constant.GeneticDefault.ReservedPopulationProportion };
        }

        protected override Heuristic CreateHeuristic(OrganonConfiguration organonConfiguration, Objective objective, GeneticParameters parameters)
        {
            return new GeneticAlgorithm(this.Stand!, organonConfiguration, objective, parameters);
        }

        protected override string GetName()
        {
            return "Optimize-Genetic";
        }

        protected override IList<GeneticParameters> GetParameterCombinations()
        {
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
            if (this.GenerationMultiplier.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.GenerationMultiplier));
            }
            if (this.InitializationClasses.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.InitializationClasses));
            }
            if (this.MinCoefficientOfVariation.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MinCoefficientOfVariation));
            }
            if (this.PopulationSize.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.PopulationSize));
            }
            if (this.ReservedProportion.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ReservedProportion));
            }

            List<GeneticParameters> parameters = new List<GeneticParameters>(this.ProportionalPercentage.Count);
            int treeRecordCount = this.Stand!.GetTreeRecordCount();
            foreach (float crossoverProbabilityEnd in this.CrossoverProbabilityEnd)
            {
                foreach (float exponent in this.ExponentK)
                {
                    foreach (float exchangeProbabilityEnd in this.ExchangeProbabilityEnd)
                    {
                        foreach (float flipProbabilityEnd in this.FlipProbabilityEnd)
                        {
                            foreach (float generationMultiplier in this.GenerationMultiplier)
                            {
                                foreach (PopulationInitializationMethod initializationMethod in this.InitializationMethod)
                                {
                                    foreach (int initializationClassCount in this.InitializationClasses)
                                    {
                                        foreach (float minCoefficientOfVariation in this.MinCoefficientOfVariation)
                                        {
                                            foreach (int populationSize in this.PopulationSize)
                                            {
                                                foreach (float proportionalPercentage in this.ProportionalPercentage)
                                                {
                                                    foreach (float reservedProportion in this.ReservedProportion)
                                                    {
                                                        parameters.Add(new GeneticParameters()
                                                        {
                                                            CrossoverProbabilityEnd = crossoverProbabilityEnd,
                                                            ExchangeProbabilityEnd = exchangeProbabilityEnd,
                                                            ExchangeProbabilityStart = Constant.GeneticDefault.ExchangeProbabilityStart,
                                                            ExponentK = exponent,
                                                            FlipProbabilityEnd = flipProbabilityEnd,
                                                            FlipProbabilityStart = Constant.GeneticDefault.FlipProbabilityStart,
                                                            InitializationClasses = initializationClassCount,
                                                            InitializationMethod = initializationMethod,
                                                            MaximumGenerations = (int)(generationMultiplier * MathF.Pow(treeRecordCount, Constant.GeneticDefault.GenerationPower) + 0.5F),
                                                            MinimumCoefficientOfVariation = minCoefficientOfVariation,
                                                            PerturbBy = this.PerturbBy,
                                                            PopulationSize = populationSize,
                                                            ProportionalPercentage = proportionalPercentage,
                                                            ReplacementStrategy = this.ReplacementStrategy,
                                                            ReservedProportion = reservedProportion,
                                                            TimberValue = this.TimberValue,
                                                            UseFiaVolume = this.ScaledVolume
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
            }
            return parameters;
        }
    }
}
