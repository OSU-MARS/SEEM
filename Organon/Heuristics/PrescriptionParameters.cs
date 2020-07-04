using Osu.Cof.Ferm.Organon;
using System.Collections.Generic;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionParameters : HeuristicParameters
    {
        public float FromAbovePercentage { get; set; }
        public float FromBelowPercentage { get; set; }

        public float IntensityStep { get; set; }
        public float MaximumIntensity { get; set; }
        public float MinimumIntensity { get; set; }

        public PrescriptionParameters()
        {
            this.FromAbovePercentage = -1.0F;
            this.ProportionalPercentage = -1.0F;
            this.FromBelowPercentage = -1.0F;

            this.IntensityStep = Constant.PrescriptionEnumerationDefault.IntensityStep;
            this.MaximumIntensity = Constant.PrescriptionEnumerationDefault.MaximumIntensity;
            this.MinimumIntensity = Constant.PrescriptionEnumerationDefault.MinimumIntensity;
        }

        public override string GetCsvHeader()
        {
            return "min intensity,max intensity,step,above,proportional,below";
        }

        public override string GetCsvValues()
        {
            return this.MinimumIntensity.ToString(CultureInfo.InvariantCulture) + "," +
                   this.MaximumIntensity.ToString(CultureInfo.InvariantCulture) + "," +
                   this.IntensityStep.ToString(CultureInfo.InvariantCulture) + "," +
                   this.FromAbovePercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }

        public void SetBestPrescription(ThinByPrescription prescription)
        {
            this.FromAbovePercentage = prescription.FromAbovePercentage;
            this.FromBelowPercentage = prescription.FromBelowPercentage;
            this.ProportionalPercentage = prescription.ProportionalPercentage;
        }
    }
}
