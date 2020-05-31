using Osu.Cof.Ferm.Cmdlets;
using Osu.Cof.Ferm.Organon;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionParameters : HeuristicParameters
    {
        public float FromAbovePercentage { get; set; }
        public float FromBelowPercentage { get; set; }

        public PrescriptionParameters(ThinByPrescription prescription)
        {
            this.FromAbovePercentage = prescription.FromAbovePercentage;
            this.FromBelowPercentage = prescription.FromBelowPercentage;
            this.ProportionalPercentage = prescription.ProportionalPercentage;
        }

        public override string GetCsvHeader()
        {
            return "above,proportional,below";
        }

        public override string GetCsvValues()
        {
            return this.FromAbovePercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
