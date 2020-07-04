using System.Collections.Generic;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionMoveLog : IHeuristicMoveLog
    {
        public List<float> FromAbovePercentageByMove { get; set; }
        public List<float> FromBelowPercentageByMove { get; set; }
        public List<float> ProportionalPercentageByMove { get; set; }

        public PrescriptionMoveLog()
        {
            this.FromAbovePercentageByMove = new List<float>();
            this.FromBelowPercentageByMove = new List<float>();
            this.ProportionalPercentageByMove = new List<float>();
        }

        public string GetCsvHeader(string prefix)
        {
            return prefix + "above," + prefix + "proportional," + prefix + "below";
        }

        public string GetCsvValues(int move)
        {
            return this.FromAbovePercentageByMove[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageByMove[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageByMove[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
