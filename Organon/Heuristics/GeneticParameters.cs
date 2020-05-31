using Osu.Cof.Ferm.Cmdlets;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class GeneticParameters : HeuristicParameters
    {
        public float ExchangeProbability { get; set; }
        public float FlipProbability { get; set; }
        public int MaximumGenerations { get; set; }
        public float MinCoefficientOfVariation { get; set; }
        public int PopulationSize { get; set; }
        public float ProportionalPercentageWidth { get; set; }
        public float ReservedProportion { get; set; }

        public override string GetCsvHeader()
        {
            return "generations,size,min CV,proportional,width,exchange,flip,reserved";
        }

        public override string GetCsvValues()
        {
            return this.MaximumGenerations.ToString(CultureInfo.InvariantCulture) + "," +
                   this.PopulationSize.ToString(CultureInfo.InvariantCulture) + "," +
                   this.MinCoefficientOfVariation.ToString(CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageWidth.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ExchangeProbability.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture) + "," +
                   this.FlipProbability.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture) + "," +
                   this.ReservedProportion.ToString(Constant.DefaultProbabilityFormat, CultureInfo.InvariantCulture);
        }
    }
}
