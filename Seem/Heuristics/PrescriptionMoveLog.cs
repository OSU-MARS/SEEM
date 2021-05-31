using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionMoveLog : IHeuristicMoveLog
    {
        public List<float> FromAbovePercentageByMove1 { get; private init; }
        public List<float> FromBelowPercentageByMove1 { get; private init; }
        public List<float> ProportionalPercentageByMove1 { get; private init; }

        public List<float> FromAbovePercentageByMove2 { get; private init; }
        public List<float> FromBelowPercentageByMove2 { get; private init; }
        public List<float> ProportionalPercentageByMove2 { get; private init; }

        public List<float> FromAbovePercentageByMove3 { get; private init; }
        public List<float> FromBelowPercentageByMove3 { get; private init; }
        public List<float> ProportionalPercentageByMove3 { get; private init; }

        public PrescriptionMoveLog()
        {
            this.FromAbovePercentageByMove1 = new List<float>();
            this.FromBelowPercentageByMove1 = new List<float>();
            this.ProportionalPercentageByMove1 = new List<float>();

            this.FromAbovePercentageByMove2 = new List<float>();
            this.FromBelowPercentageByMove2 = new List<float>();
            this.ProportionalPercentageByMove2 = new List<float>();

            this.FromAbovePercentageByMove3 = new List<float>();
            this.FromBelowPercentageByMove3 = new List<float>();
            this.ProportionalPercentageByMove3 = new List<float>();
        }

        public int Count
        {
            get 
            {
                Debug.Assert(this.FromAbovePercentageByMove1.Count == this.FromBelowPercentageByMove1.Count);
                Debug.Assert(this.FromBelowPercentageByMove1.Count == this.ProportionalPercentageByMove1.Count);
                Debug.Assert(this.ProportionalPercentageByMove1.Count == this.FromAbovePercentageByMove2.Count);
                Debug.Assert(this.FromAbovePercentageByMove2.Count == this.FromBelowPercentageByMove2.Count);
                Debug.Assert(this.FromBelowPercentageByMove2.Count == this.ProportionalPercentageByMove2.Count);
                Debug.Assert(this.ProportionalPercentageByMove2.Count == this.FromAbovePercentageByMove3.Count);
                Debug.Assert(this.FromAbovePercentageByMove3.Count == this.FromBelowPercentageByMove3.Count);
                Debug.Assert(this.FromBelowPercentageByMove3.Count == this.ProportionalPercentageByMove3.Count);
                return this.ProportionalPercentageByMove1.Count; 
            }
        }

        public string GetCsvHeader(string prefix)
        {
            return prefix + "Thin1above," + prefix + "Thin1proportional," + prefix + "Thin1below," +
                   prefix + "Thin2above," + prefix + "Thin2proportional," + prefix + "Thin2below," +
                   prefix + "Thin3above," + prefix + "Thin3proportional," + prefix + "Thin3below";
        }

        public string GetCsvValues(int move)
        {
            return this.FromAbovePercentageByMove1[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageByMove1[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageByMove1[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromAbovePercentageByMove2[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageByMove2[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageByMove2[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromAbovePercentageByMove3[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageByMove3[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageByMove3[move].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
