using Osu.Cof.Ferm.Organon;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionSingleMoveLog : IHeuristicMoveLog
    {
        public int Count { get; set; }
        public int MoveIndex { get; set; }

        public float FromAbovePercentageByMove1 { get; set; }
        public float FromBelowPercentageByMove1 { get; set; }
        public float ProportionalPercentageByMove1 { get; set; }

        public float FromAbovePercentageByMove2 { get; set; }
        public float FromBelowPercentageByMove2 { get; set; }
        public float ProportionalPercentageByMove2 { get; set; }

        public float FromAbovePercentageByMove3 { get; set; }
        public float FromBelowPercentageByMove3 { get; set; }
        public float ProportionalPercentageByMove3 { get; set; }

        public PrescriptionSingleMoveLog()
        {
            this.Count = 0;
            this.MoveIndex = -1;

            this.FromAbovePercentageByMove1 = -1.0F;
            this.FromBelowPercentageByMove1 = -1.0F;
            this.ProportionalPercentageByMove1 = -1.0F;

            this.FromAbovePercentageByMove2 = -1.0F;
            this.FromBelowPercentageByMove2 = -1.0F;
            this.ProportionalPercentageByMove2 = -1.0F;

            this.FromAbovePercentageByMove3 = -1.0F;
            this.FromBelowPercentageByMove3 = -1.0F;
            this.ProportionalPercentageByMove3 = -1.0F;
        }

        public void Add(int moveIndex, ThinByPrescription? firstThinPrescription, ThinByPrescription? secondThinPrescription, ThinByPrescription? thirdThinPrescription)
        {
            this.MoveIndex = moveIndex;

            if (firstThinPrescription != null)
            {
                this.FromAbovePercentageByMove1 = firstThinPrescription.FromAbovePercentage;
                this.ProportionalPercentageByMove1 = firstThinPrescription.ProportionalPercentage;
                this.FromBelowPercentageByMove1 = firstThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.FromAbovePercentageByMove1 = 0.0F;
                this.ProportionalPercentageByMove1 = 0.0F;
                this.FromBelowPercentageByMove1 = 0.0F;
            }

            if (secondThinPrescription != null)
            {
                this.FromAbovePercentageByMove2 = secondThinPrescription.FromAbovePercentage;
                this.ProportionalPercentageByMove2 = secondThinPrescription.ProportionalPercentage;
                this.FromBelowPercentageByMove2 = secondThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.FromAbovePercentageByMove2 = 0.0F;
                this.ProportionalPercentageByMove2 = 0.0F;
                this.FromBelowPercentageByMove2 = 0.0F;
            }

            if (thirdThinPrescription != null)
            {
                this.FromAbovePercentageByMove3 = thirdThinPrescription.FromAbovePercentage;
                this.ProportionalPercentageByMove3 = thirdThinPrescription.ProportionalPercentage;
                this.FromBelowPercentageByMove3 = thirdThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.FromAbovePercentageByMove3 = 0.0F;
                this.ProportionalPercentageByMove3 = 0.0F;
                this.FromBelowPercentageByMove3 = 0.0F;
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
            if (move == this.MoveIndex)
            {
                return this.FromAbovePercentageByMove1.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                       this.ProportionalPercentageByMove1.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                       this.FromBelowPercentageByMove1.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                       this.FromAbovePercentageByMove2.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                       this.ProportionalPercentageByMove2.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                       this.FromBelowPercentageByMove2.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                       this.FromAbovePercentageByMove3.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                       this.ProportionalPercentageByMove3.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                       this.FromBelowPercentageByMove3.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
            }

            return ",,,,,,,,";
        }
    }
}
