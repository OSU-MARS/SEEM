using Osu.Cof.Ferm.Organon;
using System.Collections.Generic;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    // for now, stores only one set of thinning intensities per discount rate
    // If neeeded, can be extended to retain n prescriptions per discount rate or similar.
    public class PrescriptionSparseMoveLog : IHeuristicMoveLog
    {
        // first thin
        public List<float> FromAbovePercentageByDiscountRate1 { get; set; }
        public List<float> FromBelowPercentageByDiscountRate1 { get; set; }
        public List<float> ProportionalPercentageByDiscountRate1 { get; set; }

        // second thin
        public List<float> FromAbovePercentageByDiscountRate2 { get; set; }
        public List<float> FromBelowPercentageByDiscountRate2 { get; set; }
        public List<float> ProportionalPercentageByDiscountRate2 { get; set; }

        // third thin
        public List<float> FromAbovePercentageByDiscountRate3 { get; set; }
        public List<float> FromBelowPercentageByDiscountRate3 { get; set; }
        public List<float> ProportionalPercentageByDiscountRate3 { get; set; }

        public int LengthInMoves { get; set; }
        public List<int> MoveIndexByDiscountRate { get; set; }

        public PrescriptionSparseMoveLog(int discountRates)
        {
            this.FromAbovePercentageByDiscountRate1 = new(discountRates);
            this.FromBelowPercentageByDiscountRate1 = new(discountRates);
            this.ProportionalPercentageByDiscountRate1 = new(discountRates);

            this.FromAbovePercentageByDiscountRate2 = new(discountRates);
            this.FromBelowPercentageByDiscountRate2 = new(discountRates);
            this.ProportionalPercentageByDiscountRate2 = new(discountRates);

            this.FromAbovePercentageByDiscountRate3 = new(discountRates);
            this.FromBelowPercentageByDiscountRate3 = new(discountRates);
            this.ProportionalPercentageByDiscountRate3 = new(discountRates);

            this.LengthInMoves = 0;
            this.MoveIndexByDiscountRate = new(discountRates);

            for (int discountRateIndex = 0; discountRateIndex < discountRates; ++discountRateIndex)
            {
                this.FromAbovePercentageByDiscountRate1.Add(-1.0F);
                this.FromBelowPercentageByDiscountRate1.Add(-1.0F);
                this.ProportionalPercentageByDiscountRate1.Add(-1.0F);

                this.FromAbovePercentageByDiscountRate2.Add(-1.0F);
                this.FromBelowPercentageByDiscountRate2.Add(-1.0F);
                this.ProportionalPercentageByDiscountRate2.Add(-1.0F);

                this.FromAbovePercentageByDiscountRate3.Add(-1.0F);
                this.FromBelowPercentageByDiscountRate3.Add(-1.0F);
                this.ProportionalPercentageByDiscountRate3.Add(-1.0F);

                this.MoveIndexByDiscountRate.Add(-1);
            }
        }

        public string GetCsvHeader(string prefix)
        {
            return prefix + "Thin1above," + prefix + "Thin1proportional," + prefix + "Thin1below," +
                   prefix + "Thin2above," + prefix + "Thin2proportional," + prefix + "Thin2below," +
                   prefix + "Thin3above," + prefix + "Thin3proportional," + prefix + "Thin3below";
        }

        // TODO: change interface signature to pass in discount rate
        public string GetCsvValues(int move)
        {
            for (int discountRateIndex = 0; discountRateIndex < this.MoveIndexByDiscountRate.Count; ++discountRateIndex)
            {
                if (move == this.MoveIndexByDiscountRate[discountRateIndex])
                {
                    return this.FromAbovePercentageByDiscountRate1[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           this.ProportionalPercentageByDiscountRate1[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           this.FromBelowPercentageByDiscountRate1[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           this.FromAbovePercentageByDiscountRate2[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           this.ProportionalPercentageByDiscountRate2[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           this.FromBelowPercentageByDiscountRate2[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           this.FromAbovePercentageByDiscountRate3[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           this.ProportionalPercentageByDiscountRate3[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           this.FromBelowPercentageByDiscountRate3[discountRateIndex].ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
                }
            }

            return ",,,,,,,,";
        }

        public void SetPrescription(int discountRateIndex, int moveIndex, ThinByPrescription? firstThinPrescription, ThinByPrescription? secondThinPrescription, ThinByPrescription? thirdThinPrescription)
        {
            // update specified discount rate
            this.MoveIndexByDiscountRate[discountRateIndex] = moveIndex;

            if (firstThinPrescription != null)
            {
                this.FromAbovePercentageByDiscountRate1[discountRateIndex] = firstThinPrescription.FromAbovePercentage;
                this.ProportionalPercentageByDiscountRate1[discountRateIndex] = firstThinPrescription.ProportionalPercentage;
                this.FromBelowPercentageByDiscountRate1[discountRateIndex] = firstThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.FromAbovePercentageByDiscountRate1[discountRateIndex] = 0.0F;
                this.ProportionalPercentageByDiscountRate1[discountRateIndex] = 0.0F;
                this.FromBelowPercentageByDiscountRate1[discountRateIndex] = 0.0F;
            }

            if (secondThinPrescription != null)
            {
                this.FromAbovePercentageByDiscountRate2[discountRateIndex] = secondThinPrescription.FromAbovePercentage;
                this.ProportionalPercentageByDiscountRate2[discountRateIndex] = secondThinPrescription.ProportionalPercentage;
                this.FromBelowPercentageByDiscountRate2[discountRateIndex] = secondThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.FromAbovePercentageByDiscountRate2[discountRateIndex] = 0.0F;
                this.ProportionalPercentageByDiscountRate2[discountRateIndex] = 0.0F;
                this.FromBelowPercentageByDiscountRate2[discountRateIndex] = 0.0F;
            }

            if (thirdThinPrescription != null)
            {
                this.FromAbovePercentageByDiscountRate3[discountRateIndex] = thirdThinPrescription.FromAbovePercentage;
                this.ProportionalPercentageByDiscountRate3[discountRateIndex] = thirdThinPrescription.ProportionalPercentage;
                this.FromBelowPercentageByDiscountRate3[discountRateIndex] = thirdThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.FromAbovePercentageByDiscountRate3[discountRateIndex] = 0.0F;
                this.ProportionalPercentageByDiscountRate3[discountRateIndex] = 0.0F;
                this.FromBelowPercentageByDiscountRate3[discountRateIndex] = 0.0F;
            }
        }
    }
}
