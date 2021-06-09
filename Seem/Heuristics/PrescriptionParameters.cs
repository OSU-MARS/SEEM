using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PrescriptionParameters : HeuristicParameters
    {
        public float FromAbovePercentageUpperLimit { get; set; }
        public float FromBelowPercentageUpperLimit { get; set; }
        public float ProportionalPercentageUpperLimit { get; set; }

        // for now, intensity step is relative to the density of the stand before thinning
        // For example if the step size is 5% and the first thin removes 50%, then the second thin will be evaluated in 10% steps. This is
        // potentially a computationally sensitive parameter as prescription enumeration cost can increase at high powers. The step size individually
        // affects the cost of enumerating from above, from below, and proportionally, resulting in a quadratic increase for each thin if all three
        // modes are enabled. If three thins are also configured, enumeration cost therefore increases as the ninth power of the step size. In this
        // case, halving the step size increases compute time by a factor of 512. If the full step size takes one hour to run the half step will take
        // 21 days.
        public float DefaultIntensityStepSize { get; set; }

        public bool LogAllMoves { get; set; }

        public float MaximumIntensity { get; set; }
        public float MaximumIntensityStepSize { get; set; }
        public float MinimumIntensity { get; set; }
        public PrescriptionUnits Units { get; set; }

        public PrescriptionParameters()
        {
            this.ConstructionGreediness = Constant.Grasp.FullyGreedyConstructionForMaximization;
            this.InitialThinningProbability = 0.0F;
            this.LogAllMoves = false;

            this.FromAbovePercentageUpperLimit = 100.0F;
            this.ProportionalPercentageUpperLimit = 100.0F;
            this.FromBelowPercentageUpperLimit = 100.0F;

            this.DefaultIntensityStepSize = Constant.PrescriptionEnumerationDefault.DefaultIntensityStepSize;
            this.MaximumIntensity = Constant.PrescriptionEnumerationDefault.MaximumIntensity;
            this.MaximumIntensityStepSize = Constant.PrescriptionEnumerationDefault.MaximumIntensityStepSize;
            this.MinimumIntensity = Constant.PrescriptionEnumerationDefault.MinimumIntensity;
            this.Units = Constant.PrescriptionEnumerationDefault.Units;
        }

        public override string GetCsvHeader()
        {
            return base.GetCsvHeader() + ",units,minIntensity,maxIntensity,step,maxAbove,maxProportional,maxBelow";
        }

        public override string GetCsvValues()
        {
            return base.GetCsvValues() + "," +
                   this.Units.ToString() + "," +
                   this.MinimumIntensity.ToString(CultureInfo.InvariantCulture) + "," +
                   this.MaximumIntensity.ToString(CultureInfo.InvariantCulture) + "," +
                   this.DefaultIntensityStepSize.ToString(CultureInfo.InvariantCulture) + "," +
                   this.FromAbovePercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.ProportionalPercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   this.FromBelowPercentageUpperLimit.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
