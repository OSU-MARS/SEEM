using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class TabuParameters : HeuristicParameters
    {
        public int Iterations { get; set; }
        public int MaximumTenure { get; set; }

        public override string GetCsvHeader()
        {
            // don't need to log iterations as it is logged separately as count
            return "perturbation,max tenure,proportional";
        }

        public override string GetCsvValues()
        {
            // don't need to log iterations as it is logged separately as count
            return this.PerturbBy.ToString(CultureInfo.InvariantCulture) + "," +
                   this.MaximumTenure.ToString(CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
