using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicParameters
    {
        /// <summary>
        /// GRASP quality-enforcing parameter α. For maximization, purely greedy construction at α = 1, purely random at α = 0.
        /// </summary>
        /// <remarks>
        /// See section 5.2 (Semi-greedy multistart) of Resende MGC, Riberio CC. 2016. Optimization by GRASP. Springer Science+Business Media, 
        /// New York, New York, USA. https://doi.org/10.1007/978-1-4939-6530-4
        /// For minimization problems, α is reversed with purely greedy construction at α = 0 and purely random at α = 1.
        /// </remarks>
        public float ConstructionGreediness { get; set; }
        public float InitialThinningProbability { get; set; }

        public HeuristicParameters()
        {
            this.ConstructionGreediness = Constant.Grasp.DefaultConstructionForMaximization;
            this.InitialThinningProbability = Constant.HeuristicDefault.InitialThinningProbability;
        }

        public virtual string GetCsvHeader()
        {
            return "constructionGreediness,thinProbability";
        }

        public virtual string GetCsvValues()
        {
            return this.ConstructionGreediness.ToString(CultureInfo.InvariantCulture) + "," + 
                   this.InitialThinningProbability.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
        }
    }
}
