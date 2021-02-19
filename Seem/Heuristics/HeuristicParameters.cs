using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicParameters
    {
        public float PerturbBy { get; set; }
        public float ProportionalPercentage { get; set; }
        public TimberValue TimberValue { get; set; }
        public bool UseFiaVolume { get; set; }

        public HeuristicParameters()
        {
            this.PerturbBy = Constant.MetaheuristicDefault.PerturbBy;
            this.ProportionalPercentage = Constant.HeuristicDefault.ProportionalPercentage;
            this.TimberValue = TimberValue.Default;
            this.UseFiaVolume = false;
        }

        public virtual string GetCsvHeader()
        {
            return "perturbation,proportional,scaled";
        }

        public virtual string GetCsvValues()
        {
            return this.PerturbBy.ToString(CultureInfo.InvariantCulture) + "," + 
                   this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   (this.UseFiaVolume ? "0" : "1");
        }
    }
}
