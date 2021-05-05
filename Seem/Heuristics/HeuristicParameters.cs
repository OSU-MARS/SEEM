using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicParameters
    {
        /// <summary>
        /// GRASP quality-enforcing parameter α. Purely greedy construction at α = 0, purely random at α = 1.
        /// </summary>
        /// <remarks>
        /// See section 5.2 (Semi-greedy multistart) of Resende MGC, Riberio CC. 2016. Optimization by GRASP. Springer Science+Business Media, 
        /// New York, New York, USA. https://doi.org/10.1007/978-1-4939-6530-4
        /// </remarks>
        public float ConstructionRandomness { get; set; }

        public float ProportionalPercentage { get; set; }
        public TimberValue TimberValue { get; set; }
        public bool UseFiaVolume { get; set; }

        public HeuristicParameters()
        {
            this.ConstructionRandomness = Constant.GraspDefault.FullyRandomConstruction;
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
            return this.ConstructionRandomness.ToString(CultureInfo.InvariantCulture) + "," + 
                   this.ProportionalPercentage.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                   (this.UseFiaVolume ? "0" : "1");
        }
    }
}
