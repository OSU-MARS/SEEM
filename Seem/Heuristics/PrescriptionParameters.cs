using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionParameters : HeuristicParameters
    {
        public float FromAbovePercentageUpperLimit { get; set; }
        public float FromBelowPercentageUpperLimit { get; set; }
        public float ProportionalPercentageUpperLimit { get; set; }

        public float Maximum { get; set; }
        public float Minimum { get; set; }
        public float StepSize { get; set; }
        public PrescriptionUnits Units { get; set; }

        public PrescriptionParameters()
        {
            this.FromAbovePercentageUpperLimit = 100.0F;
            this.PerturbBy = 0.0F;
            this.ProportionalPercentage = Constant.HeuristicDefault.ProportionalPercentage;
            this.ProportionalPercentageUpperLimit = 100.0F;
            this.FromBelowPercentageUpperLimit = 100.0F;

            this.Maximum = Constant.PrescriptionEnumerationDefault.MaximumIntensity;
            this.Minimum = Constant.PrescriptionEnumerationDefault.MinimumIntensity;
            this.StepSize = Constant.PrescriptionEnumerationDefault.IntensityStep;
            this.Units = Constant.PrescriptionEnumerationDefault.Units;
        }

        public override string GetCsvHeader()
        {
            return base.GetCsvHeader() + ",units,min intensity,max intensity,step,max above,max proportional,max below";
        }

        public override string GetCsvValues()
        {
            return base.GetCsvValues() + "," +
                   this.Units.ToString() + "," +
                   this.Minimum.ToString(CultureInfo.InvariantCulture) + "," +
                   this.Maximum.ToString(CultureInfo.InvariantCulture) + "," +
                   this.StepSize.ToString(CultureInfo.InvariantCulture) + "," +
                   this.FromAbovePercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
