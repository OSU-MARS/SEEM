using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PopulationParameters : HeuristicParameters
    {
        public int InitializationClasses { get; set; }
        public PopulationInitializationMethod InitializationMethod { get; set; }
        public int PopulationSize { get; set; }

        public PopulationParameters()
        {
            this.InitializationClasses = Constant.GeneticDefault.InitializationClasses;
            this.InitializationMethod = Constant.GeneticDefault.InitializationMethod;
            this.PopulationSize = Constant.GeneticDefault.PopulationSize;
        }

        public override string GetCsvHeader()
        {
            return base.GetCsvHeader() + ",population size,init method,init classes";
        }

        public override string GetCsvValues()
        {
            return base.GetCsvValues() + "," +
                   this.InitializationClasses.ToString(CultureInfo.InvariantCulture) + "," +
                   this.InitializationMethod.ToString() + "," +
                   this.PopulationSize.ToString(CultureInfo.InvariantCulture);
        }
    }
}
