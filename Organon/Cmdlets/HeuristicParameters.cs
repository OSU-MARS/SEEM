using System.Globalization;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class HeuristicParameters
    {
        public float ProportionalPercentage { get; set; }

        public virtual string GetCsvHeader()
        {
            return "proportional";
        }

        public virtual string GetCsvValues()
        {
            return this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
