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
        public float InitialThinningProbability { get; set; }
        public TimberValue TimberValue { get; set; }

        public HeuristicParameters()
        {
            this.ConstructionRandomness = Constant.GraspDefault.FullyRandomConstruction;
            this.InitialThinningProbability = Constant.HeuristicDefault.InitialThinningProbability;
            this.TimberValue = TimberValue.Default;
        }

        public virtual string GetCsvHeader()
        {
            return "constructionRandomness,thinProbability";
        }

        public virtual string GetCsvValues()
        {
            return this.ConstructionRandomness.ToString(CultureInfo.InvariantCulture) + "," + 
                   this.InitialThinningProbability.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
