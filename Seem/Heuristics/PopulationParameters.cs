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
    }
}
