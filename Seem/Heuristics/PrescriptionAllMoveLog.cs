using Osu.Cof.Ferm.Organon;
using System.Collections.Generic;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionAllMoveLog : HeuristicMoveLog
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

        public PrescriptionAllMoveLog(int moveCapacity)
            : base(moveCapacity)
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

        public override string GetCsvHeader(string prefix)
        {
            return prefix + "Thin1above," + prefix + "Thin1proportional," + prefix + "Thin1below," +
                   prefix + "Thin2above," + prefix + "Thin2proportional," + prefix + "Thin2below," +
                   prefix + "Thin3above," + prefix + "Thin3proportional," + prefix + "Thin3below";
        }

        public override string GetCsvValues(HeuristicResultPosition position, int moveNumber)
        {
            return this.FromAbovePercentageByMove1[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageByMove1[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageByMove1[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromAbovePercentageByMove2[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageByMove2[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageByMove2[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromAbovePercentageByMove3[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageByMove3[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageByMove3[moveNumber].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }

        public bool TryAddMove(ThinByPrescription? firstThinPrescription, ThinByPrescription? secondThinPrescription, ThinByPrescription? thirdThinPrescription)
        {
            if (this.FromAbovePercentageByMove1.Count > this.MoveCapacity)
            {
                return false;
            }

            float fromAbovePercentageFirst = 0.0F;
            float proportionalPercentageFirst = 0.0F;
            float fromBelowPercentageFirst = 0.0F;
            if (firstThinPrescription != null)
            {
                fromAbovePercentageFirst = firstThinPrescription.FromAbovePercentage;
                proportionalPercentageFirst = firstThinPrescription.ProportionalPercentage;
                fromBelowPercentageFirst = firstThinPrescription.FromBelowPercentage;
            }
            this.FromAbovePercentageByMove1.Add(fromAbovePercentageFirst);
            this.ProportionalPercentageByMove1.Add(proportionalPercentageFirst);
            this.FromBelowPercentageByMove1.Add(fromBelowPercentageFirst);

            float fromAbovePercentageSecond = 0.0F;
            float proportionalPercentageSecond = 0.0F;
            float fromBelowPercentageSecond = 0.0F;
            if (secondThinPrescription != null)
            {
                fromAbovePercentageSecond = secondThinPrescription.FromAbovePercentage;
                proportionalPercentageSecond = secondThinPrescription.ProportionalPercentage;
                fromBelowPercentageSecond = secondThinPrescription.FromBelowPercentage;
            }
            this.FromAbovePercentageByMove2.Add(fromAbovePercentageSecond);
            this.ProportionalPercentageByMove2.Add(proportionalPercentageSecond);
            this.FromBelowPercentageByMove2.Add(fromBelowPercentageSecond);

            float fromAbovePercentageThird = 0.0F;
            float proportionalPercentageThird = 0.0F;
            float fromBelowPercentageThird = 0.0F;
            if (thirdThinPrescription != null)
            {
                fromAbovePercentageThird = thirdThinPrescription.FromAbovePercentage;
                proportionalPercentageThird = thirdThinPrescription.ProportionalPercentage;
                fromBelowPercentageThird = thirdThinPrescription.FromBelowPercentage;
            }
            this.FromAbovePercentageByMove3.Add(fromAbovePercentageThird);
            this.ProportionalPercentageByMove3.Add(proportionalPercentageThird);
            this.FromBelowPercentageByMove3.Add(fromBelowPercentageThird);

            return false;
        }
    }
}
