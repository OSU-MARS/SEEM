using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicParameters
    {
        public float PerturbBy { get; set; }
        public float ProportionalPercentage { get; set; }

        public virtual string GetCsvHeader()
        {
            return "perturbation,proportional";
        }

        public virtual string GetCsvValues()
        {
            return this.PerturbBy.ToString(CultureInfo.InvariantCulture) + "," + 
                   this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
