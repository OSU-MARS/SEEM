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
        public float Step { get; set; }
        public PrescriptionUnits Units { get; set; }

        public PrescriptionParameters()
        {
            this.FromAbovePercentageUpperLimit = 100.0F;
            this.ProportionalPercentage = -1.0F;
            this.ProportionalPercentageUpperLimit = 100.0F;
            this.FromBelowPercentageUpperLimit = 100.0F;

            this.Maximum = Constant.PrescriptionEnumerationDefault.MaximumIntensity;
            this.Minimum = Constant.PrescriptionEnumerationDefault.MinimumIntensity;
            this.Step = Constant.PrescriptionEnumerationDefault.IntensityStep;
            this.Units = Constant.PrescriptionEnumerationDefault.Units;
        }

        public void CopyFrom(PrescriptionParameters other)
        {
            this.FromAbovePercentageUpperLimit = other.FromAbovePercentageUpperLimit;
            this.FromBelowPercentageUpperLimit = other.FromBelowPercentageUpperLimit;
            this.Maximum = other.Maximum;
            this.Minimum = other.Minimum;
            this.ProportionalPercentageUpperLimit = other.ProportionalPercentageUpperLimit;
            this.Step = other.Step;
            this.Units = other.Units;
        }

        public override string GetCsvHeader()
        {
            return "units,min intensity,max intensity,step,max above,max proportional,max below";
        }

        public override string GetCsvValues()
        {
            return this.Units.ToString() + "," +
                   this.Minimum.ToString(CultureInfo.InvariantCulture) + "," +
                   this.Maximum.ToString(CultureInfo.InvariantCulture) + "," +
                   this.Step.ToString(CultureInfo.InvariantCulture) + "," +
                   this.FromAbovePercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
