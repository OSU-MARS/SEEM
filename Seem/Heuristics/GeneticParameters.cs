using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GeneticParameters : HeuristicParameters
    {
        public float CrossoverProbabilityEnd { get; set; }
        public float CrossoverProbabilityStart { get; set; }
        public float ExchangeProbabilityEnd { get; set; }
        public float ExchangeProbabilityStart { get; set; }
        public float ExponentK { get; set; }
        public float FlipProbabilityStart { get; set; }
        public float FlipProbabilityEnd { get; set; }
        public int MaximumGenerations { get; set; }
        public float MinimumCoefficientOfVariation { get; set; }
        public int PopulationSize { get; set; }
        public float ProportionalPercentageWidth { get; set; }
        public PopulationReplacementStrategy ReplacementStrategy { get; set; }
        public float ReservedProportion { get; set; }

        public GeneticParameters()
        {
            this.CrossoverProbabilityEnd = Constant.GeneticDefault.CrossoverProbabilityEnd;
            this.ExchangeProbabilityEnd = Constant.GeneticDefault.ExchangeProbabilityEnd;
            this.ExchangeProbabilityStart = Constant.GeneticDefault.ExchangeProbabilityStart;
            this.ExponentK = Constant.GeneticDefault.ExponentK;
            this.FlipProbabilityEnd = Constant.GeneticDefault.FlipProbabilityEnd;
            this.FlipProbabilityStart = Constant.GeneticDefault.FlipProbabilityStart;
            this.MaximumGenerations = 50;
            this.MinimumCoefficientOfVariation = Constant.GeneticDefault.MinimumCoefficientOfVariation;
            this.PopulationSize = Constant.GeneticDefault.PopulationSize;
            this.ProportionalPercentageWidth = Constant.GeneticDefault.ProportionalPercentageWidth;
            this.ReplacementStrategy = Constant.GeneticDefault.ReplacementStrategy;
            this.ReservedProportion = Constant.GeneticDefault.ReservedPopulationProportion;
        }

        public GeneticParameters(int treeCount)
            : this()
        {
            this.MaximumGenerations = (int)(Constant.GeneticDefault.GenerationMultiplier * treeCount + 0.5F);
        }

        public override string GetCsvHeader()
        {
            return "perturbation,generations,size,strategy,crossover end,exchange start,exchange end,flip start,flip end,exponent,proportional,width,reserved,min CV";
        }

        public override string GetCsvValues()
        {
            return this.PerturbBy.ToString(CultureInfo.InvariantCulture) + "," +
                   this.MaximumGenerations.ToString(CultureInfo.InvariantCulture) + "," +
                   this.PopulationSize.ToString(CultureInfo.InvariantCulture) + "," +
                   this.ReplacementStrategy.ToString() + "," +
                   this.CrossoverProbabilityEnd.ToString(CultureInfo.InvariantCulture) + "," +
                   this.ExchangeProbabilityStart.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture) + "," +
                   this.ExchangeProbabilityEnd.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture) + "," +
                   this.FlipProbabilityStart.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture) + "," +
                   this.FlipProbabilityEnd.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture) + "," +
                   this.ExponentK.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageWidth.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ReservedProportion.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture) + "," +
                   this.MinimumCoefficientOfVariation.ToString(CultureInfo.InvariantCulture);
        }
    }
}
